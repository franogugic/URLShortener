using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Application.DTO_s;

public class RegisterUserRequestDTO
{
    [Required(ErrorMessage = "Username is required")]
    public required string Username { get; set; }
   
    [Required(ErrorMessage = "Username is required")]
    public required string Password { get; set; }
}