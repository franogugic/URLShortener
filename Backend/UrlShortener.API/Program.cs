using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UrlShortener.API.Data;
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

// --- OpenAPI / Swagger ---
builder.Services.AddOpenApi();

// --- Database ---
// --- Database ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 33)),
        mysqlOptions => mysqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)
    )
);

// --- Authentication (cookie) ---
builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.Cookie.Name = "UrlShortenerAuthCookie";
        options.LoginPath = "/api/user/login";
        options.LogoutPath = "/api/user/logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(3);
        options.SlidingExpiration = true;

        // SPAs: return 401 instead of redirect
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });

// --- DI for services ---
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

builder.Services.AddScoped<IUrlService, UrlService>();
builder.Services.AddScoped<IUrlRepository, UrlRepository>();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var options = ConfigurationOptions.Parse("redis:6379,abortConnect=false");
    return ConnectionMultiplexer.Connect(options);
});

builder.Services.AddSingleton<IUrlCache, RedisUrlCache>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(UserProfile).Assembly, typeof(UrlProfile).Assembly);

// --- CORS ---
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins("http://localhost:5173") // frontend dev
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("Retry-After");
    });
});

// --- Rate Limiting ---
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth-limit", httpContext =>
    {
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
            PermitLimit = 15,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext => { 
        // Svi zahtjevi na cijeloj aplikaciji dijele ovaj limit
        return RateLimitPartition.GetFixedWindowLimiter("global-safety",
            _ => new FixedWindowRateLimiterOptions { 
                PermitLimit = 5000, 
                    //Maksimalno 5000 zahtjeva u minuti za CIJELI server
                Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }); });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests",
            message = "Rate limit exceeded",
            retryAfter = "30s"
        }, cancellationToken: token);
    };
});

var app = builder.Build();

// --- Middleware order ---
app.UseRouting();

app.UseCors(MyAllowSpecificOrigins);


app.UseMiddleware<ExceptionMiddleware>();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        context.Database.Migrate();
        await AppSeeder.SeedAsync(context, passwordHasher);
        Console.WriteLine("Database migration successful!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
    }
}

app.Run();

public partial class Program { }
