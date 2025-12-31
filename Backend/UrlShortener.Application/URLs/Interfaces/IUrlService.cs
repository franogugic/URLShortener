using UrlShortener.Application.URLs.DTO_s;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.URLs.Interfaces;

public interface IUrlService
{
    Task<CreateUrlResponseDTO> CreateAsync(CreateUrlRequestDTO request, User user, CancellationToken cancellationToken);

    Task<List<CreateUrlResponseDTO>> GetAllUrlsByUser(Guid userId,CancellationToken cancellationToken);
    
    Task DeleteAsync(Guid id,Guid currentUserId, CancellationToken cancellationToken);
    
    Task<CreateUrlResponseDTO?> GetUrlById(Guid id, Guid userId, CancellationToken cancellationToken);

    Task<string?> GetLongUrlByCode(string shortCode, CancellationToken cancellationToken);


}