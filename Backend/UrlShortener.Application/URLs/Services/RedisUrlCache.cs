using StackExchange.Redis;
using UrlShortener.Application.URLs.Interfaces;

namespace UrlShortener.Application.URLs.Services;

public class RedisUrlCache : IUrlCache
{
    private readonly IDatabase _db;

    public RedisUrlCache(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<string?> GetLongUrlAsync(string shortUrlCode, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var value = await _db.StringGetAsync($"url:{shortUrlCode}");
        if (value.HasValue)
            return value;
        return null;
    }

    public async Task SetLongUrlAsync(string shortUrlCode, string longUrl, CancellationToken cancellationToken , TimeSpan? ttl = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _db.StringSetAsync($"url:{shortUrlCode}", longUrl, ttl ?? TimeSpan.FromHours(3));
    }
    
    public async Task RemoveAsync(string shortUrlCode, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _db.KeyDeleteAsync($"url:{shortUrlCode}");
    }
}