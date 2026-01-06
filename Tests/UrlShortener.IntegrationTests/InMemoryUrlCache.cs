using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UrlShortener.Application.URLs.Interfaces;

namespace UrlShortener.IntegrationTests
{
    public class InMemoryUrlCache : IUrlCache
    {
        private readonly Dictionary<string, string> _cache = new();

        public Task<string?> GetLongUrlAsync(string shortUrlCode, CancellationToken cancellationToken)
        {
            _cache.TryGetValue(shortUrlCode, out var longUrl);
            return Task.FromResult(longUrl);
        }

        public Task SetLongUrlAsync(string shortUrlCode, string longUrl, CancellationToken cancellationToken, TimeSpan? expiration)
        {
            _cache[shortUrlCode] = longUrl;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string shortUrlCode, CancellationToken cancellationToken)
        {
            _cache.Remove(shortUrlCode);
            return Task.CompletedTask;
        }
    }
}