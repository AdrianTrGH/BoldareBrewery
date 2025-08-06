using BoldareBrewery.Application.Interfaces;
using BoldareBrewery.Application.Models.Internal;
using BoldareBrewery.Infrastructure.Strategies;
using BoldareBrewery.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace BoldareBrewery.Infrastructure.Factories
{
    public class SearchStrategyFactory : ISearchStrategyFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public SearchStrategyFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IBrewerySearchStrategy CreateStrategy(SearchContext context)
        {
            return context.DataSource switch
            {
                DataSource.Cache => _serviceProvider.GetRequiredService<MemorySearchStrategy>(),
                DataSource.Database => _serviceProvider.GetRequiredService<DatabaseSearchStrategy>(),
                DataSource.ExternalApi => throw new NotImplementedException("External API strategy not yet implemented"),
                _ => _serviceProvider.GetRequiredService<DatabaseSearchStrategy>()
            };
        }
    }
}
