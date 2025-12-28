namespace UrlShortener.Application.Users.Exceptions;

public class InvalidUserCreateException : Exception
{
    public InvalidUserCreateException(string message) : base(message)
    { }
}