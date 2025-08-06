namespace BoldareBrewery.Application.UseCases.SearchBreweries
{
    public class BreweryInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public double? Distance { get; set; }
    }
}
