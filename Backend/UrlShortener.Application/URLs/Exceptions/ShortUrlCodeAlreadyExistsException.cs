namespace UrlShortener.Application.URLs.Exceptions;

public class ShortUrlCodeAlreadyExistsException : Exception
{
    public ShortUrlCodeAlreadyExistsException(string code) : base($"Short url code {code} is already exists"){}
}