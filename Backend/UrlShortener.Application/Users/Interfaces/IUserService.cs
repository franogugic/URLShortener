using UrlShortener.Application.DTO_s;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Interfaces;

public interface IUserService
{
    public Task<RegisterUserResponseDTO> RegisterUser(RegisterUserRequestDTO request, CancellationToken cancellationToken);
    
    public Task<LoginUserResponseDTO> LoginUser(LoginUserRequestDTO request, CancellationToken cancellationToken);
}