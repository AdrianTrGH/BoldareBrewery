namespace BoldareBrewery.Configuration.Settings
{
    public class CacheSettings
    {
        public int ExpirationMinutes { get; set; } = 10;
        public int DatabaseSyncHours { get; set; } = 24;
    }
}
