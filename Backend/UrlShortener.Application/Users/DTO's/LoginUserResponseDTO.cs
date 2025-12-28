namespace UrlShortener.Application.DTO_s;

public class LoginUserResponseDTO
{
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public DateTime DateCreated { get; set; }
}