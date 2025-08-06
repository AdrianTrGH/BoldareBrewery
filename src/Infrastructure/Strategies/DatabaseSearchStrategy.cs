using AutoMapper;
using BoldareBrewery.Application.Interfaces;
using BoldareBrewery.Application.Specifications;
using BoldareBrewery.Application.UseCases.SearchBreweries;
using BoldareBrewery.Domain.Data.ValueObjects;

namespace BoldareBrewery.Infrastructure.Strategies;

public class DatabaseSearchStrategy : IBrewerySearchStrategy
{
    private readonly IBreweryRepository _repository;
    private readonly IMapper _mapper;

    public DatabaseSearchStrategy(IBreweryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<SearchBreweriesResponse> SearchAsync(SearchBreweriesRequest request)
    {
        // For distance sorting, we need to handle it differently
        if (request.IsSortingByDistance && request.HasUserLocation)
        {
            return await SearchWithDistanceSorting(request);
        }

        // Regular search with database-level sorting
        var specification = new BrewerySearchSpecification(request);
        var breweries = await _repository.SearchAsync(specification);
        var totalCount = await _repository.CountAsync(specification);

        var breweryInfos = breweries.Select(eb =>
        {
            var breweryInfo = _mapper.Map<BreweryInfo>(eb);
            breweryInfo.Distance = request.HasUserLocation && eb.HasValidCoordinates()
                ? new Coordinates(eb.Latitude!.Value, eb.Longitude!.Value).DistanceTo(request.UserLocation!)
                : null;
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

    private async Task<SearchBreweriesResponse> SearchWithDistanceSorting(SearchBreweriesRequest request)
    {
        // For distance sorting, we need to load all matching records and sort in memory
        // Create specification without pagination for filtering only
        var filterOnlyRequest = new SearchBreweriesRequest
        {
            Search = request.Search,
            City = request.City,
            SortBy = "name", // Temporary sort for database query
            Page = 1,
            PageSize = int.MaxValue // Get all matching records
        };

        var specification = new BrewerySearchSpecification(filterOnlyRequest);
        var allBreweries = await _repository.SearchAsync(specification);

        // Calculate distances and sort in memory
        var breweriesWithDistance = allBreweries
            .Where(b => b.Latitude.HasValue && b.Longitude.HasValue)
            .Select(b => new
            {
                Brewery = b,
                Distance = new Coordinates(b.Latitude!.Value, b.Longitude!.Value).DistanceTo(request.UserLocation!)
            })
            .OrderBy(b => b.Distance)
            .ToList();

        // Apply pagination
        var totalCount = breweriesWithDistance.Count;
        var pagedResults = breweriesWithDistance
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