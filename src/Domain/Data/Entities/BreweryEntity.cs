using BoldareBrewery.Domain.Data.ValueObjects;

namespace BoldareBrewery.Domain.Data.Entities
{
    public class BreweryEntity
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string BreweryType { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Coordinates Location => Coordinates.FromNullable(Latitude, Longitude);
      
    }
}
