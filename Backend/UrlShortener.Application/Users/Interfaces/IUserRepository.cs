using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Interfaces;

public interface IUserRepository
{
    public Task<User?> GetUserByUsername(string username, CancellationToken cancellationToken);
    public Task CreateAsync(User user, CancellationToken cancellationToken);
}