using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByUsername(string username, CancellationToken cancellationToken);
    Task CreateAsync(User user, CancellationToken cancellationToken);
    Task<User?> GetUserById(Guid userId, CancellationToken cancellationToken);
}