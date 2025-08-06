using BoldareBrewery.Application.Models.Internal;

namespace BoldareBrewery.Application.Interfaces
{
    public interface ISearchStrategyFactory
    {
        IBrewerySearchStrategy CreateStrategy(SearchContext context);
    }
}
