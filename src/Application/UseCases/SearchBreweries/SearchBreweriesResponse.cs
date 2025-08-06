namespace BoldareBrewery.Application.UseCases.SearchBreweries
{
    public class SearchBreweriesResponse
    {
        public IEnumerable<BreweryInfo> Breweries { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }   
}
