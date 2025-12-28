namespace UrlShortener.Application.Users.Exceptions;

public sealed class UsernameAlreadyExistsException : Exception
{
    public UsernameAlreadyExistsException(string username) : base($"Username '{username}' is already exists")
    { }
}