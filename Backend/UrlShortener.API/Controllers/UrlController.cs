using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using UrlShortener.Application.Interfaces;
using UrlShortener.Application.URLs.DTO_s;
using UrlShortener.Application.URLs.Interfaces;
using UrlShortener.Domain.Entities;


namespace UrlShortener.API.Controllers;

[ApiController]
[Route("[controller]")]
public class UrlController : ControllerBase
{
    private readonly IUrlService _urlService;
    private readonly IUserService _userService;
    private readonly IConnectionMultiplexer _redis;

    public UrlController(IUrlService urlService, IUserService userService, IConnectionMultiplexer redis)
    {
        _urlService = urlService;
        _userService = userService;
        _redis = redis;
    }
    
    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUrlRequestDTO request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();
        
        var user = await _userService.GetUserById(userId, cancellationToken);
        if (user == null)
            return Unauthorized();
        
        var response = await _urlService.CreateAsync(request, user, cancellationToken);
        
        return Ok(response);
        
    }

    [Authorize]
    [HttpGet("getAllUrlsByUserId")]
    public async Task<IActionResult> GetAllUrlsByUser(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();
        
        var urls = await _urlService.GetAllUrlsByUser(userId, cancellationToken);
        return Ok(urls);
    }

    [Authorize]
    [HttpGet("getUrlById/{id}")]
    public async Task<IActionResult> GetUrlById(Guid id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();
        
        var url = await _urlService.GetUrlById(id, userId ,cancellationToken);
        return Ok(url);
    }

    [Authorize]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteUrl(Guid id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();
        await _urlService.DeleteAsync(id, userId, cancellationToken);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("{shortUrlCode}")]
    public async Task<IActionResult> RedirectToLongUrl(String shortUrlCode, CancellationToken cancellationToken)
    {
        var urlLong = await _urlService.GetLongUrlByCode(shortUrlCode, cancellationToken);
        
        if (urlLong == null)
            return NotFound();
        
        return Redirect(urlLong);
    }
}