using BoldareBrewery.Application.UseCases.SearchBreweries;

namespace BoldareBrewery.Application.Interfaces
{
    public interface IBrewerySearchStrategy
    {
        Task<SearchBreweriesResponse> SearchAsync(SearchBreweriesRequest request);
    }
}
