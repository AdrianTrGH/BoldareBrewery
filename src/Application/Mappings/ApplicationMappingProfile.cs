using AutoMapper;
using BoldareBrewery.Application.Models.External;
using BoldareBrewery.Application.UseCases.SearchBreweries;

namespace BoldareBrewery.Application.Mappings
{
    public class ApplicationMappingProfile : Profile
    {
        public ApplicationMappingProfile()
        {
            CreateMap<ExternalBrewery, BreweryInfo>()
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.Phone) ? "N/A" : src.Phone))
                .ForMember(dest => dest.Distance, opt => opt.Ignore());
        }
    }
}