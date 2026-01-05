namespace UrlShortener.Application.Users.Exceptions;

public sealed class UsernameAlreadyExistsException : Exception
{
    public string Username { get; }
    
    public UsernameAlreadyExistsException(string username) : base($"Username '{username}' is already exists")
    {
        Username = username;
    }
}