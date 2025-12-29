using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Db;
using UrlShortener.Application.Interfaces;

namespace UrlShortener.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<User?> GetUserByUsername(string username, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }
    
    public async Task<User?> GetUserById(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }
    
    public async Task CreateAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _dbContext.Users.AddAsync(user, cancellationToken);
        
        cancellationToken.ThrowIfCancellationRequested();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    
}