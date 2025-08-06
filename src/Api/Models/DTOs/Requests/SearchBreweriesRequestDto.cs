using System.ComponentModel.DataAnnotations;

namespace BoldareBrewery.Api.Models.DTOs.Requests
{
    public class SearchBreweriesRequestDto
    {
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string? City { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double? UserLatitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double? UserLongitude { get; set; }

        public bool HasUserLocation => UserLatitude.HasValue && UserLongitude.HasValue;
    }
}
