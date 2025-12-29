using UrlShortener.Application.Users.Exceptions;

namespace UrlShortener.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
    public DateTime DateCreated { get; private set; }
    
    public ICollection<Url>  Urls { get; private set; } = new List<Url>();
    
    
    private User() {}

    public static User Create(string username, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new InvalidUserCreateException("Username cannot be null or empty");
        
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new InvalidUserCreateException("Password hash cannot be null or empty");

        return new User()
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = passwordHash,
            DateCreated = DateTime.UtcNow
        };
    }
}