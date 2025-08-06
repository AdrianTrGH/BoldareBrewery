using BoldareBrewery.Application.Common;
using BoldareBrewery.Application.Interfaces;
using BoldareBrewery.Application.Models.Internal;
using BoldareBrewery.Configuration.Settings;
using BoldareBrewery.Shared.Constants;
using BoldareBrewery.Shared.Enums;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace BoldareBrewery.Application.UseCases.SearchBreweries;

public class SearchBreweriesUseCase : ISearchBreweriesUseCase
{
    private readonly IOpenBreweryDbService _openBreweryDbService;
    private readonly ICacheService _cacheService;
    private readonly IBreweryRepository _breweryRepository;
    private readonly ISearchStrategyFactory _strategyFactory;
    private readonly CacheSettings _cacheSettings;
    private readonly ILogger<SearchBreweriesUseCase> _logger;

    public SearchBreweriesUseCase(
        IOpenBreweryDbService openBreweryDbService,
        ICacheService cacheService,
        IBreweryRepository breweryRepository,
        ISearchStrategyFactory strategyFactory,
        IOptions<CacheSettings> cacheSettings,
        ILogger<SearchBreweriesUseCase> logger)
    {
        _openBreweryDbService = openBreweryDbService;
        _cacheService = cacheService;
        _breweryRepository = breweryRepository;
        _strategyFactory = strategyFactory;
        _cacheSettings = cacheSettings.Value;
        _logger = logger;
    }

    public async Task<Result<SearchBreweriesResponse>> SearchAsync(SearchBreweriesRequest request)
    {
        try
        {
            _logger.LogInformation("Starting brewery search. Search: {Search}, City: {City}, SortBy: {SortBy}, Page: {Page}",
                        request.Search, request.City, request.SortBy, request.Page);

            // Create cache key for this specific search
            var cacheKey = CacheKeys.SearchBreweries(
                request.Search,
                request.City,
                request.SortBy,
                request.Page,
                request.PageSize,
                request.UserLocation?.Latitude,
                request.UserLocation?.Longitude);

            // TIER 1: Try to get from cache first (10-minute requirement)
            var cachedResult = await _cacheService.GetAsync<SearchBreweriesResponse>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
                return cachedResult;
            }

            _logger.LogInformation("Cache miss for key: {CacheKey}", cacheKey);

            // Determine search context
            var searchContext = await DetermineSearchContext();

            // Create appropriate strategy using factory
            var searchStrategy = _strategyFactory.CreateStrategy(searchContext);

            // Execute search using selected strategy
            var result = await searchStrategy.SearchAsync(request);

            // Cache the result for 10 minutes
            var cacheExpiration = TimeSpan.FromMinutes(_cacheSettings.ExpirationMinutes);
            await _cacheService.SetAsync(cacheKey, result, cacheExpiration);

            _logger.LogInformation("Search completed successfully. Results: {Count}, Strategy: {Strategy}",
                        result.TotalCount, searchContext.DataSource);

            return Result.Success(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error in brewery search. Request: {@Request}", request);
            return Result.Failure<SearchBreweriesResponse>(
                Error.ValidationFailure($"Invalid search parameters: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during brewery search. Request: {@Request}", request);
            return Result.Failure<SearchBreweriesResponse>(
                Error.InternalServerError($"Search operation failed: {ex.Message}"));
        }
    }

    private async Task<SearchContext> DetermineSearchContext()
    {
        // Check if we have cached raw data
        var hasCachedData = await _cacheService.ExistsAsync(CacheKeys.AllBreweries);
        if (hasCachedData)
        {
            _logger.LogInformation("Selecting memory search strategy - cached data available");
            return new SearchContext { DataSource = DataSource.Cache };
        }

        // Check database freshness
        var databaseSyncInterval = TimeSpan.FromHours(_cacheSettings.DatabaseSyncHours);
        var lastSync = await _breweryRepository.GetLastSyncTimeAsync();
        var isDataFresh = lastSync.HasValue && DateTime.UtcNow - lastSync.Value < databaseSyncInterval;

        if (isDataFresh)
        {
            _logger.LogInformation("Selecting database search strategy - data is fresh. Last sync: {LastSync}", lastSync);
            
            return new SearchContext
            {
                DataSource = DataSource.Database,
                IsDataFresh = true,
                LastSync = lastSync
            };
        }

        // Need to refresh data first
        _logger.LogInformation("Data is stale, refreshing from external API. Last sync: {LastSync}", lastSync);
        await RefreshDataFromExternalApi();

        return new SearchContext
        {
            DataSource = DataSource.Database,
            IsDataFresh = true,
            LastSync = DateTime.UtcNow
        };
    }
    private async Task RefreshDataFromExternalApi()
    {
        _logger.LogInformation("Starting data refresh from external API");

        // Fetch fresh data from OpenBreweryDb
        var freshBreweries = await _openBreweryDbService.GetBreweriesAsync();
        var breweryList = freshBreweries.ToList();

        if (breweryList.Any())
        {
            await _breweryRepository.SaveBreweriesAsync(breweryList);

            // Also cache raw data for memory strategy
            var cacheExpiration = TimeSpan.FromMinutes(_cacheSettings.ExpirationMinutes);
            await _cacheService.SetAsync(CacheKeys.AllBreweries, breweryList, cacheExpiration);

            _logger.LogInformation("Data refresh completed successfully. Breweries loaded: {Count}", breweryList.Count);
        }
        else
        {
            _logger.LogWarning("Data refresh completed but no breweries were loaded from external API");
        }
    }
}