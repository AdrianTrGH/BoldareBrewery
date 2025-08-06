using BoldareBrewery.Configuration.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BoldareBrewery.Configuration.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddBreweryConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<OpenBreweryDbSettings>(
                configuration.GetSection("OpenBreweryDb"));

            services.Configure<CacheSettings>(
                configuration.GetSection("Cache"));

            services.Configure<JwtSettings>(
               configuration.GetSection("Jwt"));

            return services;
        }
    }
}
