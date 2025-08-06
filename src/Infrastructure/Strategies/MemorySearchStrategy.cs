using AutoMapper;
using BoldareBrewery.Application.Interfaces;
using BoldareBrewery.Application.Models.External;
using BoldareBrewery.Application.UseCases.SearchBreweries;
using BoldareBrewery.Domain.Data.ValueObjects;
using BoldareBrewery.Shared.Constants;

namespace BoldareBrewery.Infrastructure.Strategies;

public class MemorySearchStrategy : IBrewerySearchStrategy
{
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public MemorySearchStrategy(ICacheService cacheService, IMapper mapper)
    {
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<SearchBreweriesResponse> SearchAsync(SearchBreweriesRequest request)
    {
        var cachedBreweries = await _cacheService.GetAsync<List<ExternalBrewery>>(CacheKeys.AllBreweries) ?? throw new InvalidOperationException("No cached data available for memory search");
        var query = cachedBreweries.AsQueryable();

        // Apply filtering
        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(b =>
                (b.Name != null && b.Name.ToUpper().Contains(request.Search.ToUpper())) ||
                (b.City != null && b.City.ToUpper().Contains(request.Search.ToUpper()))
                );
        }

        if (!string.IsNullOrEmpty(request.City))
        {
            query = query.Where(b => b.City.ToUpper().Contains(request.City.ToUpper()));
        }

        // Convert to list for distance calculations
        var filteredBreweries = query.ToList();

        // Calculate distances if user location provided
        var breweriesWithDistance = filteredBreweries.Select(b => new
        {
            Brewery = b,
            Distance = request.HasUserLocation && b.Latitude.HasValue && b.Longitude.HasValue
                ? new Coordinates(b.Latitude.Value, b.Longitude.Value).DistanceTo(request.UserLocation!)
                : (double?)null
        }).ToList();

        // Apply sorting
        var sortedBreweries = request.SortBy?.ToLower() switch
        {
            "name" => breweriesWithDistance.OrderBy(b => b.Brewery.Name),
            "city" => breweriesWithDistance.OrderBy(b => b.Brewery.City),
            "distance" when request.HasUserLocation => breweriesWithDistance.OrderBy(b => b.Distance ?? double.MaxValue),
            _ => breweriesWithDistance.OrderBy(b => b.Brewery.Name)
        };

        // Apply pagination
        var totalCount = sortedBreweries.Count();
        var pagedResults = sortedBreweries
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Map to response
        var breweryInfos = pagedResults.Select(item =>
        {
            var breweryInfo = _mapper.Map<BreweryInfo>(item.Brewery);
            breweryInfo.Distance = item.Distance;
            return breweryInfo;
        }).ToList();

        return new SearchBreweriesResponse
        {
            Breweries = breweryInfos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}