using AutoMapper;
using Microsoft.Extensions.Logging;
using UrlShortener.Application.Interfaces;
using UrlShortener.Application.URLs.DTO_s;
using UrlShortener.Application.URLs.Interfaces;
using UrlShortener.Application.Users.Exceptions;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.URLs.Services;

public class UrlService : IUrlService
{
    
    private readonly ILogger _logger;
    private readonly IUrlRepository _urlRepository;
    private readonly IMapper _mapper;

    public UrlService(ILogger<UrlService> logger, IUserService userService, IUrlRepository urlRepository, IMapper mapper)
    {
        _logger = logger;
        _urlRepository = urlRepository;
        _mapper = mapper;
    }
    
    
    public async Task<CreateUrlResponseDTO> CreateAsync(CreateUrlRequestDTO request,User user, CancellationToken cancellationToken)
    {
        var shortUrlCode = request.ShortUrlCode;
        var longUrl = request.LongUrl;
        var description = request.Description;
        
        var url = Url.Create(shortUrlCode, longUrl, description, user);
        
        await _urlRepository.CreateAsync(url, cancellationToken);
        
        _logger.LogInformation($"Created url: {url}");
        return _mapper.Map<CreateUrlResponseDTO>(url);

    }
}