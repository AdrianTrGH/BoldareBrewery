namespace BoldareBrewery.Shared.Constants
{
    public static class CacheKeys
    {
        public const string AllBreweries = "breweries:all";

        public static string SearchBreweries(
            string? search,
            string? city,
            string? sortBy,
            int page,
            int pageSize,
            double? userLatitude = null,
            double? userLongitude = null)
        {
            var searchKey = string.IsNullOrEmpty(search) ? "null" : search.ToLower();
            var cityKey = string.IsNullOrEmpty(city) ? "null" : city.ToLower();
            var sortKey = string.IsNullOrEmpty(sortBy) ? "name" : sortBy.ToLower();

            // Include coordinates in cache key for distance sorting
            var latKey = userLatitude?.ToString("F6") ?? "null";
            var lngKey = userLongitude?.ToString("F6") ?? "null";

            return $"breweries:search:{searchKey}:{cityKey}:{sortKey}:{page}:{pageSize}:{latKey}:{lngKey}";
        }
    }
}
