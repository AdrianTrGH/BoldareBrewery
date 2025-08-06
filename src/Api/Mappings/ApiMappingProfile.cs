using AutoMapper;
using BoldareBrewery.Api.Models.DTOs.Requests;
using BoldareBrewery.Api.Models.DTOs.Responses;
using BoldareBrewery.Application.UseCases.SearchBreweries;
using BoldareBrewery.Domain.Data.ValueObjects;

namespace BoldareBrewery.Api.Mappings
{
    public class ApiMappingProfile : Profile
    {
        public ApiMappingProfile()
        {
            CreateMap<SearchBreweriesRequestDto, SearchBreweriesRequest>()
                .ForMember(dest => dest.UserLocation, opt => opt.MapFrom(src =>
                    src.HasUserLocation
                        ? Coordinates.FromNullable(src.UserLatitude, src.UserLongitude)
                        : null));

            CreateMap<SearchBreweriesResponse, SearchBreweriesResponseDto>();

            CreateMap<BreweryInfo, BreweryInfoDto>();
        }
    }
}