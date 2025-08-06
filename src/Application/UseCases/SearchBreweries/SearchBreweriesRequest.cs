using BoldareBrewery.Domain.Data.ValueObjects;

namespace BoldareBrewery.Application.UseCases.SearchBreweries
{

    public class SearchBreweriesRequest
    {
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string? City { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public Coordinates? UserLocation { get; set; }

        public bool HasUserLocation => UserLocation != null && !UserLocation.IsEmpty();
        public bool IsSortingByDistance => "distance".Equals(SortBy, StringComparison.OrdinalIgnoreCase);
    }
}
