using System.Text.Json;
using UrlShortener.Application.URLs.Exceptions;
using UrlShortener.Application.Users.Exceptions;

namespace UrlShortener.API.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);

            // samo izadi iz middlewarea  ako je 429 i tjt dosl.
            if (httpContext.Response.StatusCode == StatusCodes.Status429TooManyRequests)
            {
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occured");

            // ako je dio pdoataka poslan header ne smimo dirat
            if (httpContext.Response.HasStarted)
            {
                return;
            }

            httpContext.Response.ContentType = "application/json";

            switch (ex)
            {
                case UsernameAlreadyExistsException:
                    httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                    break;
                
                case InvalidUserCreateException:
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;
                
                case InvalidUrlCreateException:
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;
                
                case InvalidCredentialsException:
                    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    break;
                
                case ShortUrlCodeAlreadyExistsException:
                    httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                    break;
                
                case ForbiddenException:
                    httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    break;
                
                case UrlNotFoundException:
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    break;
                
                default:
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            var response = new
            {
                message = ex.Message,
            };
            
            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}