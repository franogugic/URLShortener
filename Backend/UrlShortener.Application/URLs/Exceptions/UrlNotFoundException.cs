namespace UrlShortener.Application.URLs.Exceptions;

public class UrlNotFoundException : Exception
{
    public UrlNotFoundException() : base("Url not found") {}
}