namespace UrlShortener.Application.URLs.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) :  base(message){}
}