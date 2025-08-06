using AutoMapper;
using BoldareBrewery.Application.Models.External;
using BoldareBrewery.Domain.Data.Entities;

namespace BoldareBrewery.Infrastructure.Mappings
{
    public class InfrastructureMappingProfile : Profile
    {
        public InfrastructureMappingProfile()
        {
            CreateMap<ExternalBrewery, BreweryEntity>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<BreweryEntity, ExternalBrewery>();
        }
    }
}