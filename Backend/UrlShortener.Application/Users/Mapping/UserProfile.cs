using AutoMapper;
using UrlShortener.Application.DTO_s;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Users.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, LoginUserResponseDTO>();
        CreateMap<User, RegisterUserResponseDTO>();
    }
}