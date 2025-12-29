using UrlShortener.Application.URLs.DTO_s;
using UrlShortener.Application.URLs.Interfaces;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Db;

namespace UrlShortener.Infrastructure.Repositories;

public class UrlRepository : IUrlRepository
{
    private readonly AppDbContext _context;

    public UrlRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task CreateAsync(Url url, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _context.Urls.AddAsync(url, cancellationToken);
        
        cancellationToken.ThrowIfCancellationRequested();
        await _context.SaveChangesAsync(cancellationToken);
    }
}