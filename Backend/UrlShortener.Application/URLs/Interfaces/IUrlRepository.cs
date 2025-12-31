using UrlShortener.Application.URLs.DTO_s;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.URLs.Interfaces;

public interface IUrlRepository
{
    Task CreateAsync(Url url, CancellationToken cancellationToken);
    
    Task<IEnumerable<Url>> GetAllUrlsByUser(Guid userId,CancellationToken cancellationToken);
    
    Task DeleteAsync(Url url, CancellationToken cancellationToken);
    
    Task<Url?> GetUrlById(Guid id, CancellationToken cancellationToken);

    Task<Url?> GetUrlByShortCode(string shortUrl, CancellationToken cancellationToken);


}