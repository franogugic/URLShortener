using UrlShortener.Application.Users.Exceptions;

namespace UrlShortener.Domain.Entities;

public class Url
{
    public Guid Id { get; private set; }
    public string ShortUrlCode { get; private set; }
    public string ShortUrl { get; private set; }
    public string LongUrl { get; private set; }
    public string Description { get; private set; }
    public int Clicks { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    public Guid UserId { get; private set; }
    public User User { get; private set; }
    
    private Url() {}

    public static Url Create(string shortUrlCode, string longUrl, string description, User user)
    {
        if (string.IsNullOrEmpty(shortUrlCode))
            throw new InvalidUrlCreateException("ShortUrlCode cannot be null or empty");
        
        if (string.IsNullOrEmpty(longUrl))
            throw new InvalidUrlCreateException("LongUrl cannot be null or empty");

        return new Url
        {
            Id = Guid.NewGuid(),
            ShortUrlCode = shortUrlCode,
            LongUrl = longUrl,
            ShortUrl = $"http://localhost:5010/url/{shortUrlCode}",
            Description = description,
            Clicks = 0,
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id,
            User = user
        };
    }
    
    public void IncrementClicks()
    {
        Clicks++;
    }
}
