using Microsoft.EntityFrameworkCore;
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
    
    public async Task<IEnumerable<Url>> GetAllUrlsByUser(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _context.Urls.Where(u => u.User.Id == userId).ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(Url url, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _context.Urls.Remove(url);
        
        cancellationToken.ThrowIfCancellationRequested();
        await _context.SaveChangesAsync(cancellationToken); 
    }

    public async Task<Url?> GetUrlById(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _context.Urls.Include(u => u.User).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
    
    public async Task<Url?> GetUrlByShortCode(string shortUrlCode, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _context.Urls.FirstOrDefaultAsync(u => u.ShortUrlCode == shortUrlCode, cancellationToken);
        //return await _context.Urls.Include(u => u.User).AnyAsync(u => u.ShortUrl == shortUrl, cancellationToken);
    }
}