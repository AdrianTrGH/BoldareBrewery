namespace BoldareBrewery.Application.Models.External
{
    public class ExternalBrewery
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string BreweryType { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string WebsiteUrl { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public static ExternalBrewery CreateSafe(
            string? id,
            string? name,
            string? breweryType,
            string? street,
            string? city,
            string? state,
            string? postalCode,
            string? country,
            string? phone,
            string? websiteUrl,
            double? latitude,
            double? longitude)
        {
            return new ExternalBrewery
            {
                Id = id ?? Guid.NewGuid().ToString(),
                Name = name ?? "Unknown Brewery",
                BreweryType = breweryType ?? "unknown",
                Street = street ?? string.Empty,
                City = city ?? string.Empty,
                State = state ?? string.Empty,
                PostalCode = postalCode ?? string.Empty,
                Country = country ?? string.Empty,
                Phone = CleanPhoneNumber(phone),
                WebsiteUrl = websiteUrl ?? string.Empty,
                Latitude = IsValidCoordinate(latitude, longitude) ? latitude : null,
                Longitude = IsValidCoordinate(latitude, longitude) ? longitude : null
            };
        }
        private static bool IsValidCoordinate(double? latitude, double? longitude)
        {
            if (!latitude.HasValue || !longitude.HasValue)
                return false;

            if (latitude.Value == 0 && longitude.Value == 0)
                return false;

            return latitude.Value >= -90 && latitude.Value <= 90 &&
                   longitude.Value >= -180 && longitude.Value <= 180;
        }
        private static string CleanPhoneNumber(string? phone)
        {
            if (string.IsNullOrEmpty(phone))
                return string.Empty;

            // Extract only digits
            var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());

            // Return empty if less than 10 digits (invalid US phone number)
            return digitsOnly.Length >= 10 ? digitsOnly : string.Empty;
        }

        public bool HasValidCoordinates()
        {
            return Latitude.HasValue &&
                   Longitude.HasValue &&
                   !(Latitude.Value == 0 && Longitude.Value == 0) &&
                   Latitude.Value >= -90 && Latitude.Value <= 90 &&
                   Longitude.Value >= -180 && Longitude.Value <= 180;
        }

        public bool IsValidForDisplay()
        {
            return !string.IsNullOrEmpty(Id) &&
                   !string.IsNullOrEmpty(Name) &&
                   !string.IsNullOrEmpty(City);
        }
    }
}
