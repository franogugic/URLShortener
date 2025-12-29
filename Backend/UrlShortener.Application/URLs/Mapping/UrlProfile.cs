using AutoMapper;
using UrlShortener.Application.URLs.DTO_s;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.URLs.Mapping;

public class UrlProfile : Profile
{
    public UrlProfile()
    {
        CreateMap<Url, CreateUrlResponseDTO>();
    }
}