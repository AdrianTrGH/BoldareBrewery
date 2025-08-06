using BoldareBrewery.Application.Models.External;
using BoldareBrewery.Domain.Data.Entities;

namespace BoldareBrewery.Application.Interfaces
{
    public interface IBreweryRepository
    {
        Task<IEnumerable<ExternalBrewery>> GetAllAsync();
        Task SaveBreweriesAsync(IEnumerable<ExternalBrewery> breweries);
        Task<DateTime?> GetLastSyncTimeAsync();
        Task UpdateSyncTimeAsync();
        Task<IEnumerable<ExternalBrewery>> SearchAsync(ISpecification<BreweryEntity> specification);
        Task<int> CountAsync(ISpecification<BreweryEntity> specification);
    }
}
