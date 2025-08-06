namespace BoldareBrewery.Shared.Helpers
{
    public static class DistanceCalculator
    {
        private const double EarthRadiusKm = 6371.0;
        private const double CoordinateEqualityTolerance = 0.0001;

        // Calculates the distance between two geographical coordinates using the Haversine formula.      
        public static double CalculateDistance(
            double startLatitude, double startLongitude,
            double endLatitude, double endLongitude)
        {
            if (Math.Abs(startLatitude - endLatitude) < CoordinateEqualityTolerance &&
                Math.Abs(startLongitude - endLongitude) < CoordinateEqualityTolerance)
            {
                return 0;
            }

            var deltaLatitudeRad = ToRadians(endLatitude - startLatitude);
            var deltaLongitudeRad = ToRadians(endLongitude - startLongitude);

            var haversineComponent =
                Math.Sin(deltaLatitudeRad / 2) * Math.Sin(deltaLatitudeRad / 2) +
                Math.Cos(ToRadians(startLatitude)) * Math.Cos(ToRadians(endLatitude)) *
                Math.Sin(deltaLongitudeRad / 2) * Math.Sin(deltaLongitudeRad / 2);

            var angularDistanceRad = 2 * Math.Atan2(Math.Sqrt(haversineComponent), Math.Sqrt(1 - haversineComponent));

            var distanceInKilometers = EarthRadiusKm * angularDistanceRad;

            return Math.Round(distanceInKilometers, 2);
        }

        private static double ToRadians(double degrees) =>
            degrees * Math.PI / 180.0;
    }
}
