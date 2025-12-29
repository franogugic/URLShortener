using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.Interfaces;
using UrlShortener.Application.URLs.DTO_s;
using UrlShortener.Application.URLs.Interfaces;
using UrlShortener.Application.URLs.Services;

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
        
        //ovde umj Create async getByIDurl funkciju napravit
        //return CreatedAtAction(nameof(CreateAsync), new { id = response.Id }, response);
    }
}