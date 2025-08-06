using BoldareBrewery.Shared.Helpers;

namespace BoldareBrewery.Domain.Data.ValueObjects
{
    public record Coordinates(double Latitude, double Longitude)
    {     
        public static readonly Coordinates Empty = new(-999, -999);
    
        public static Coordinates FromNullable(double? latitude, double? longitude)
        {
            // Return Empty if either coordinate is null
            if (!latitude.HasValue || !longitude.HasValue)
                return Empty;

            // CRITICAL: Return Empty if coordinates are exactly (0,0) - likely invalid data from API
            if (latitude.Value == 0 && longitude.Value == 0)
                return Empty;

            // Validate coordinate ranges
            if (!IsValidLatitude(latitude.Value) || !IsValidLongitude(longitude.Value))
                return Empty;

            return new Coordinates(latitude.Value, longitude.Value);
        }

        private static bool IsValidLatitude(double latitude)
            => latitude is >= -90 and <= 90;

        private static bool IsValidLongitude(double longitude)
            => longitude is >= -180 and <= 180;

        public bool IsValidLocation()
        {
            return !IsEmpty() &&
                   IsValidLatitude(Latitude) &&
                   IsValidLongitude(Longitude);
        }

        public bool IsEmpty()
            => this == Empty;

        public double DistanceTo(Coordinates other)
        {
            if (IsEmpty() || other.IsEmpty())
                return double.MaxValue;

            if (!IsValidLocation() || !other.IsValidLocation())
                return double.MaxValue;

            return DistanceCalculator.CalculateDistance(
                Latitude, Longitude,
                other.Latitude, other.Longitude);
        }
  
    }
}