using System.ComponentModel.DataAnnotations;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.URLs.DTO_s;

public class CreateUrlRequestDTO
{
    [Required]
    public required string ShortUrlCode { get; set; }  = string.Empty;
    [Required]
    public required string LongUrl { get; set; } = string.Empty;
    [Required]
    public required string Description { get; set; } = string.Empty;
}