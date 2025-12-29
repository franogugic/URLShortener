using System.Text.Json;
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occured");
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
                
                default:
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            var response = new
            {
                message = ex.Message,
            };
            
            //pretvara u json
            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}