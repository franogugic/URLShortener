namespace UrlShortener.Application.Users.Exceptions;

public class InvalidUrlCreateException : Exception
{
    public InvalidUrlCreateException(string message) : base(message){}
}