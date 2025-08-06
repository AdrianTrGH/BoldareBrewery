using BoldareBrewery.Application.Models.External;

namespace BoldareBrewery.Application.Interfaces
{
    public interface IOpenBreweryDbService
    {
        Task<IEnumerable<ExternalBrewery>> GetBreweriesAsync();
    }
}
