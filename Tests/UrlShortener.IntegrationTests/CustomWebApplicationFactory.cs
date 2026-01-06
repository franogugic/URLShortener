using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using UrlShortener.Infrastructure.Db;
using System.Security.Claims;
using NSubstitute;
using UrlShortener.Application.URLs.Interfaces;
using System.Linq;
using System.Text.Encodings.Web;

namespace UrlShortener.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 1. UKLANJANJE SVIH POSTOJEĆIH REGISTRACIJA ZA DB CONTEXT
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // 2. KREIRANJE POTPUNO NOVOG I IZOLIRANOG SERVICE PROVIDERA ZA EF CORE
            // Ovo sprječava miješanje s MySQL servisima iz Program.cs
            var internalServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // 3. REGISTRACIJA IN-MEMORY BAZE S IZOLIRANIM PROVIDEROM
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
                options.UseInternalServiceProvider(internalServiceProvider);
            });

            // 4. UKLANJANJE REDISA I CACHE-A
            var redisDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IConnectionMultiplexer));
            if (redisDescriptor != null) services.Remove(redisDescriptor);
            services.AddSingleton<IConnectionMultiplexer>(Substitute.For<IConnectionMultiplexer>());

            var cacheDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUrlCache));
            if (cacheDescriptor != null) services.Remove(cacheDescriptor);
            services.AddSingleton<IUrlCache, InMemoryUrlCache>();

            // 5. AUTHENTICATION SHEMA
            services.AddAuthentication("TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
                options.DefaultScheme = "TestScheme";
            });

            // 6. INICIJALIZACIJA BAZE (EnsureCreated)
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(AuthenticateResult.NoResult());

        var authHeader = Request.Headers["Authorization"].ToString();
        var userId = authHeader.Split(' ').LastOrDefault();

        if (string.IsNullOrEmpty(userId) || userId == "TestScheme")
            userId = "00000000-0000-0000-0000-000000000001";

        var claims = new[] { 
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.NameIdentifier, userId) 
        };

        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}