using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Db;

namespace UrlShortener.API.Data;

public static class AppSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher, CancellationToken cancellationToken = default)
    {
        var demoUser = await EnsureUserAsync(context, passwordHasher, "demo", "demo123", cancellationToken);
        var adminUser = await EnsureUserAsync(context, passwordHasher, "admin", "admin123", cancellationToken);

        await EnsureUrlAsync(context, demoUser, "google", "https://www.google.com", "Search engine", cancellationToken);
        await EnsureUrlAsync(context, demoUser, "github", "https://github.com", "Code hosting", cancellationToken);
        await EnsureUrlAsync(context, demoUser, "docs", "https://learn.microsoft.com", "Microsoft docs", cancellationToken);

        await EnsureUrlAsync(context, adminUser, "news", "https://news.ycombinator.com", "Tech news", cancellationToken);
        await EnsureUrlAsync(context, adminUser, "youtube", "https://www.youtube.com", "Video platform", cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task<User> EnsureUserAsync(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        string username,
        string password,
        CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        if (user != null)
        {
            user.UpdatePasswordHash(passwordHasher.Hash(password));
            return user;
        }

        user = User.Create(username, passwordHasher.Hash(password));
        await context.Users.AddAsync(user, cancellationToken);
        return user;
    }

    private static async Task EnsureUrlAsync(
        AppDbContext context,
        User user,
        string shortCode,
        string longUrl,
        string description,
        CancellationToken cancellationToken)
    {
        var exists = await context.Urls.AnyAsync(u => u.ShortUrlCode == shortCode, cancellationToken);
        if (exists)
            return;

        var url = Url.Create(shortCode, longUrl, description, user);
        await context.Urls.AddAsync(url, cancellationToken);
    }
}
