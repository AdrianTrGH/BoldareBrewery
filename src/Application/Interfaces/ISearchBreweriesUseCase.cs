using BoldareBrewery.Application.Common;
using BoldareBrewery.Application.UseCases.SearchBreweries;

namespace BoldareBrewery.Application.Interfaces
{
    public interface ISearchBreweriesUseCase
    {
        Task<Result<SearchBreweriesResponse>> SearchAsync(SearchBreweriesRequest request);
    }
}
