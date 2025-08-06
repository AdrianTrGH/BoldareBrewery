using Asp.Versioning;
using BoldareBrewery.Api.Mappings;
using BoldareBrewery.Api.Services;
using BoldareBrewery.Application.Interfaces;
using BoldareBrewery.Application.Mappings;
using BoldareBrewery.Application.UseCases.SearchBreweries;
using BoldareBrewery.Configuration.Extensions;
using BoldareBrewery.Configuration.Settings;
using BoldareBrewery.Infrastructure.Caching;
using BoldareBrewery.Infrastructure.Data;
using BoldareBrewery.Infrastructure.ExternalServices;
using BoldareBrewery.Infrastructure.Factories;
using BoldareBrewery.Infrastructure.Mappings;
using BoldareBrewery.Infrastructure.Repositories;
using BoldareBrewery.Infrastructure.Strategies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

namespace BoldareBrewery.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            // Register Database
            builder.Services.AddDbContext<BreweryDbContext>(options =>
                options.UseSqlite("Data Source=breweries.db"));

            // Register Configuration Settings
            builder.Services.AddBreweryConfiguration(builder.Configuration);

            // Register Memory Cache 
            builder.Services.AddMemoryCache();

            // Configure Serilog
            builder.Host.UseSerilog((context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration));

            // Register HttpClient for external API calls
            builder.Services.AddHttpClient<IOpenBreweryDbService, OpenBreweryDbService>();

            // Register Cache Service (Infrastructure implementation)
            builder.Services.AddScoped<ICacheService, MemoryCacheService>();
            builder.Services.AddScoped<IBreweryRepository, BreweryRepository>();

            // Register Search Strategies
            builder.Services.AddScoped<ISearchStrategyFactory, SearchStrategyFactory>();
            builder.Services.AddScoped<DatabaseSearchStrategy>();
            builder.Services.AddScoped<MemorySearchStrategy>();
          
            // Register Use Cases
            builder.Services.AddScoped<ISearchBreweriesUseCase, SearchBreweriesUseCase>();

            // Register Automapper 
            builder.Services.AddAutoMapper(typeof(ApiMappingProfile),
                              typeof(ApplicationMappingProfile),
                              typeof(InfrastructureMappingProfile));


            // Register API Versioning
            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new QueryStringApiVersionReader("version"),
                    new HeaderApiVersionReader("X-API-Version")
                );
            }).AddMvc().AddApiExplorer(setup =>
            {
                setup.GroupNameFormat = "'v'VVV";
                setup.SubstituteApiVersionInUrl = true;
            });

            // JWT Service
            builder.Services.AddScoped<JwtService>();

            // JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
                ?? throw new InvalidOperationException("JWT configuration is missing in appsettings.json");
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };
                });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Create database if not exists
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BreweryDbContext>();
                context.Database.EnsureCreated();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "BoldareBrewery API v1");
                    options.RoutePrefix = "swagger";
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }   
}
