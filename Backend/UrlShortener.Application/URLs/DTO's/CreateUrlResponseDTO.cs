using System.ComponentModel.DataAnnotations;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.URLs.DTO_s;

public class CreateUrlResponseDTO
{
    public Guid Id { get; set; }
    public string ShortUrlCode { get; set; }
    public string LongUrl { get; set; }
    public string Description { get; set; }
    public int Clicks { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guid UserId { get; set; }
}
