namespace UrlShortener.Application.URLs.Interfaces;

public interface IUrlCache
{
    Task<string?> GetLongUrlAsync(string shortUrlCode, CancellationToken cancellationToken);
    Task SetLongUrlAsync(string shortUrlCode, string longUrl, CancellationToken cancellationToken, TimeSpan? ttl);
    Task RemoveAsync(string shortUrlCode, CancellationToken cancellationToken);
}
