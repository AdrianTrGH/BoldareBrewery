namespace BoldareBrewery.Api.Models.DTOs.Responses
{
    public class SearchBreweriesResponseDto
    {
        public IEnumerable<BreweryInfoDto> Breweries { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
       
    }
    public class BreweryInfoDto
    {
        //public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public double? Distance { get; set; }
    }
}
