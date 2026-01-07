using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UrlShortener.API.Middlewares;
using UrlShortener.Application.Interfaces;
using UrlShortener.Application.Services;
using UrlShortener.Application.URLs.Interfaces;
using UrlShortener.Application.URLs.Mapping;
using UrlShortener.Application.URLs.Services;
using UrlShortener.Application.Users.Mapping;
using UrlShortener.Infrastructure.Db;
using UrlShortener.Infrastructure.Repositories;
using UrlShortener.Infrastructure.Security;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 33))
    )
);

//kreiranje session cookiesa, middleware
builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.Cookie.Name = "UrlShortenerAuthCookie";
        options.LoginPath = "/api/user/login";
        options.LogoutPath = "/api/user/logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(3);
        options.SlidingExpiration = true;
    });

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

builder.Services.AddScoped<IUrlService, UrlService>();
builder.Services.AddScoped<IUrlRepository, UrlRepository>();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var options = ConfigurationOptions.Parse("urlshortener-redis:6379,abortConnect=false");
    return ConnectionMultiplexer.Connect(options);
});



builder.Services.AddSingleton<IUrlCache, RedisUrlCache>();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://116.203.122.236") // Dozvoli svom frontendu pristup
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod()
                //koristimo radi Rate Limitera
                .WithExposedHeaders("Retry-After");
        });
});


builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();

builder.Services.AddAutoMapper(typeof(UserProfile).Assembly, typeof(UrlProfile).Assembly);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Koristimo Sliding Window i ip particioniranje
    //npr bez particioniranja bi imali 10 req  a vako imamo 10 po ipu
    options.AddPolicy("auth-limit", httpContext =>
    {
        // Koristimo Sliding Window + IP Particioniranje
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() 
                       ?? httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() 
                       ?? "anonymous";
        
        return RateLimitPartition.GetSlidingWindowLimiter(clientIp, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromSeconds(30),
            SegmentsPerWindow = 3,
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });

    options.AddPolicy("url-limit", httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 15,          // Max 15 linkova po minuti po IP-u
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });
    
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        // Svi zahtjevi na cijeloj aplikaciji dijele ovaj limit
        return RateLimitPartition.GetFixedWindowLimiter("global-safety", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5000, // Maksimalno 5000 zahtjeva u minuti za CIJELI server
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });
    
    //  Custom res body odgovora za 429
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests",
            message = "tmc tbh",
            retryAfter = "30s"
        }, cancellationToken: token);
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(MyAllowSpecificOrigins);

app.UseMiddleware<ExceptionMiddleware>();
app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();
app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate(); // Ovo automatski kreira tablice u MySQL-u
        Console.WriteLine("Database migration successful!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
    }
}

app.Run();

public partial class Program { }



