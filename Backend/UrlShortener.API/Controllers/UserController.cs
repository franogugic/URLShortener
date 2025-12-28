using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.DTO_s;
using UrlShortener.Application.Interfaces;

namespace UrlShortener.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequestDTO request, CancellationToken cancellationToken)
    {
            var response = await _userService.RegisterUser(request, cancellationToken);
            return CreatedAtAction(nameof(RegisterUser), new { id = response.Id }, response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserRequestDTO request, CancellationToken cancellationToken)
    {
        var user = await _userService.LoginUser(request, cancellationToken);
        if (user == null)
            return StatusCode(StatusCodes.Status401Unauthorized, new { message = "Invalid username or password" });

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };
        
        var identity = new ClaimsIdentity(claims, "MyCookieAuth");
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
        };
        
        await HttpContext.SignInAsync(
            "MyCookieAuth", 
            principal,
            authProperties);
        
        return StatusCode(StatusCodes.Status200OK, new { message = "Logged in successfully" });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var user = new
        {
            Id = User.FindFirst(ClaimTypes.NameIdentifier).Value,
            Username = User.FindFirst(ClaimTypes.Name).Value,
        };
        
        return Ok(user);
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        return Ok("Ovo je zasticena ruta");
    }
}
