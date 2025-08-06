namespace BoldareBrewery.Configuration.Settings
{
    public class OpenBreweryDbSettings
    {
        public string BaseUrl { get; set; } = "https://api.openbrewerydb.org/v1/breweries";
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxRetries { get; set; } = 3;
    }
}
