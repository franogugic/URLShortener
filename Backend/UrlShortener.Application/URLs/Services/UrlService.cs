using AutoMapper;
using Microsoft.Extensions.Logging;
using UrlShortener.Application.Interfaces;
using UrlShortener.Application.URLs.DTO_s;
using UrlShortener.Application.URLs.Exceptions;
using UrlShortener.Application.URLs.Interfaces;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.URLs.Services;

public class UrlService : IUrlService
{
    
    private readonly ILogger _logger;
    private readonly IUrlRepository _urlRepository;
    private readonly IMapper _mapper;
    private readonly IUrlCache _urlCache;

    public UrlService(ILogger<UrlService> logger, IUserService userService, IUrlRepository urlRepository, IMapper mapper, IUrlCache urlCache)
    {
        _logger = logger;
        _urlRepository = urlRepository;
        _mapper = mapper;
        _urlCache = urlCache;
    }
    
    
    public async Task<CreateUrlResponseDTO> CreateAsync(CreateUrlRequestDTO request,User user, CancellationToken cancellationToken)
    {
        var shortUrlCode = request.ShortUrlCode;
        var longUrl = request.LongUrl;
        var description = request.Description;
        
        var url = Url.Create(shortUrlCode, longUrl, description, user);
        
        var isShortCodeValid =  await _urlRepository.GetUrlByShortCode(shortUrlCode, cancellationToken);
        if (isShortCodeValid != null)
            throw new ShortUrlCodeAlreadyExistsException(shortUrlCode);
        
        await _urlRepository.CreateAsync(url, cancellationToken);
        
        _logger.LogInformation($"Created url: {url}");
        return _mapper.Map<CreateUrlResponseDTO>(url);
    }

    public async Task<List<CreateUrlResponseDTO>> GetAllUrlsByUser(Guid userId, CancellationToken cancellationToken)
    {
        var urls = await _urlRepository.GetAllUrlsByUser(userId ,cancellationToken);
        _logger.LogInformation($"Retrieved urls: {urls}");
        return _mapper.Map<List<CreateUrlResponseDTO>>(urls);
    }

    public async Task<CreateUrlResponseDTO?> GetUrlById(Guid id, Guid currentUserId, CancellationToken cancellationToken)
    {
        var url = await _urlRepository.GetUrlById(id, cancellationToken);
        
        if (url == null)
            throw new UrlNotFoundException();
        
        if(url?.UserId != currentUserId)
            throw new UnauthorizedAccessException("You are not authorized to get this url");
        
        _logger.LogInformation("Retrieved url {UrlId}", url.Id);
        return _mapper.Map<CreateUrlResponseDTO>(url);
    }

    public async Task DeleteAsync(Guid id, Guid currentUserId ,CancellationToken cancellationToken)
    {
        var url = await _urlRepository.GetUrlById(id, cancellationToken);
        if (url == null)
            throw new UrlNotFoundException();
        
        if(url.UserId  != currentUserId)
            throw new UnauthorizedAccessException("You are not authorized to delete this url");
        
        await _urlRepository.DeleteAsync(url, cancellationToken);
        
        await _urlCache.RemoveAsync(url.ShortUrlCode, cancellationToken);
        
        _logger.LogInformation($"Deleted url: {url}");
        
    }

    public async Task<string?> GetLongUrlByCode(string shortUrlCode, CancellationToken cancellationToken)
    {
        var cachedUrl = await _urlCache.GetLongUrlAsync(shortUrlCode, cancellationToken);
        if (cachedUrl != null)
        {
            _logger.LogInformation($"Cache hit for {shortUrlCode}");
            return cachedUrl;
        }

        var url = await _urlRepository.GetUrlByShortCode(shortUrlCode, cancellationToken);
        if (url == null)
        {
            _logger.LogWarning($"URL not found for {shortUrlCode}");
            return null; 
        }

        await _urlCache.SetLongUrlAsync(shortUrlCode, url.LongUrl, cancellationToken, TimeSpan.FromHours(3));
        _logger.LogInformation($"Cache set for {shortUrlCode}");

        return url.LongUrl;
    }

}