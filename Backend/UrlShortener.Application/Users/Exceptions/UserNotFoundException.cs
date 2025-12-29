namespace UrlShortener.Application.Users.Exceptions;

public class UserNotFoundException : Exception
{
    public UserNotFoundException() : base("User with this Id not found.") { }
}