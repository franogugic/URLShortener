using UrlShortener.Application.URLs.DTO_s;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.URLs.Interfaces;

public interface IUrlRepository
{
    Task CreateAsync(Url url, CancellationToken cancellationToken);
}