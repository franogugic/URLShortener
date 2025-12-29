using System.ComponentModel.DataAnnotations;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.URLs.DTO_s;

public class CreateUrlRequestDTO
{
    [Required]
    public string ShortUrlCode { get; set; }
    [Required]
    public string LongUrl { get; set; }
    [Required]
    public string Description { get; set; }
}