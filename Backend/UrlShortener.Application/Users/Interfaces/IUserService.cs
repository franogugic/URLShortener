using UrlShortener.Application.DTO_s;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Interfaces;

public interface IUserService
{
    Task<RegisterUserResponseDTO> RegisterUser(RegisterUserRequestDTO request, CancellationToken cancellationToken);
    
    Task<LoginUserResponseDTO> LoginUser(LoginUserRequestDTO request, CancellationToken cancellationToken);
    
    Task<User?> GetUserById(Guid id, CancellationToken cancellationToken);
}