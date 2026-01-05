namespace UrlShortener.Application.URLs.Exceptions;

public class ShortUrlCodeAlreadyExistsException : Exception
{
    public string ShortUrlCode { get; }
    
    public ShortUrlCodeAlreadyExistsException(string code) : base($"Short url code {code} is already exists")
    {
        ShortUrlCode = code;
    }
}