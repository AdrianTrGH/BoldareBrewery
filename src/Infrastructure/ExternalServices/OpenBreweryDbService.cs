using BoldareBrewery.Application.Interfaces;
using BoldareBrewery.Application.Models.External;
using BoldareBrewery.Configuration.Settings;
using BoldareBrewery.Infrastructure.ExternalServices.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BoldareBrewery.Infrastructure.ExternalServices
{
    public class OpenBreweryDbService : IOpenBreweryDbService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenBreweryDbSettings _settings;
        private readonly ILogger<OpenBreweryDbService> _logger;

        public OpenBreweryDbService(HttpClient httpClient,
            IOptions<OpenBreweryDbSettings> settings,
            ILogger<OpenBreweryDbService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _settings = settings.Value;
            _logger = logger;

            // Configure HTTP client defaults
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BoldareBrewery/1.0");
        }

        public async Task<IEnumerable<ExternalBrewery>> GetBreweriesAsync()
        {
            var retryCount = 0;

            while (retryCount < _settings.MaxRetries)
            {
                try
                {
                    // Fetch data from API
                    _logger.LogInformation("Fetching breweries from external API. Attempt: {Attempt}/{MaxAttempts}",
                        retryCount + 1, _settings.MaxRetries);
                    var response = await _httpClient.GetAsync(_settings.BaseUrl);
                    response.EnsureSuccessStatusCode();

                    var jsonContent = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(jsonContent))
                    {
                        _logger.LogWarning("External API returned empty response");
                        return [];
                    }

                    // Deserialize JSON content
                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                    };

                    var apiBreweries = JsonSerializer.Deserialize<List<OpenBreweryApiModel>>(jsonContent, jsonOptions);

                    if (apiBreweries == null || !apiBreweries.Any())
                    {
                        _logger.LogWarning("External API returned no brewery data");
                        return Enumerable.Empty<ExternalBrewery>();
                    }

                    var validBreweries = apiBreweries
                        .Where(api => api.IsValidForDisplay()) // Pre-filter invalid data
                        .Select(api => ExternalBrewery.CreateSafe(
                            api.Id,
                            api.Name,
                            api.BreweryType,
                            api.GetBestAddress(),     // Handles Address1/Street redundancy
                            api.City,
                            api.GetBestState(),       // Handles StateProvince/State redundancy
                            api.PostalCode,
                            api.Country,
                            api.Phone,
                            api.WebsiteUrl,
                            api.Latitude,             // Safe null handling in CreateSafe
                            api.Longitude))           // Safe null handling in CreateSafe
                        .Where(brewery => brewery.IsValidForDisplay()) // Double-check validity
                        .ToList();


                    var withCoordinates = validBreweries.Count(b => b.HasValidCoordinates());
                    var withoutCoordinates = validBreweries.Count - withCoordinates;

                    _logger.LogInformation("External API call successful. Total: {Total}, Valid: {Valid}, WithCoordinates: {WithCoords}, WithoutCoordinates: {WithoutCoords}",
                        apiBreweries.Count, validBreweries.Count, withCoordinates, withoutCoordinates);

                    return validBreweries;
                }
                catch (HttpRequestException httpEx)
                {
                    retryCount++;
                    _logger.LogWarning(httpEx, "HTTP error on external API call. Attempt: {Attempt}, Will retry: {WillRetry}",
                        retryCount, retryCount < _settings.MaxRetries);

                    if (retryCount >= _settings.MaxRetries)
                    {
                        _logger.LogError("Max retries exceeded for external API call");
                        break;
                    }

                    // Exponential backoff: 2^retry seconds
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "JSON parsing error from external API response");
                    break;
                }
                catch (TaskCanceledException timeoutEx)
                {
                    retryCount++;
                    _logger.LogWarning(timeoutEx, "Timeout error on external API call. Attempt: {Attempt}, Will retry: {WillRetry}",
                        retryCount, retryCount < _settings.MaxRetries);

                    if (retryCount >= _settings.MaxRetries)
                    {
                        _logger.LogError("Max retries exceeded due to timeouts");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error during external API call");
                    break; // Don't retry unexpected errors
                }
            }
            _logger.LogError("External API call failed after all retry attempts");
            return [];
        }       
    }
}