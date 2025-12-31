using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public UrlController(IUrlService urlService, IUserService userService)
    {
        _urlService = urlService;
        _userService = userService;
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
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value); 
        
        var urls = await _urlService.GetAllUrlsByUser(userId, cancellationToken);
        return Ok(urls);
    }

    [Authorize]
    [HttpGet("getUrlById/{id}")]
    public async Task<IActionResult> GetUrlById(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        var url = await _urlService.GetUrlById(id, currentUserId ,cancellationToken);
        return Ok(url);
    }

    [Authorize]
    [HttpGet("delete/{id}")]
    public async Task<IActionResult> DeleteUrl(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        await _urlService.DeleteAsync(id, currentUserId, cancellationToken);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("{shortUrlCode}")]
    public async Task<IActionResult> RedirectToLongUrl(String shortUrlCode, CancellationToken cancellationToken)
    {
        var urlLong = await _urlService.GetLongUrlByCode(shortUrlCode, cancellationToken);
        return Redirect(urlLong);
    }
}