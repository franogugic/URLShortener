using UrlShortener.Application.URLs.DTO_s;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.URLs.Interfaces;

public interface IUrlService
{
    Task<CreateUrlResponseDTO> CreateAsync(CreateUrlRequestDTO request, User user, CancellationToken cancellationToken);
}