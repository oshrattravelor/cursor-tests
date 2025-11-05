using Adapter.Amadeus.Configuration;
using Adapter.Amadeus.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Adapter.Amadeus.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAmadeusServices(
        this IServiceCollection services,
        Action<AmadeusSettings> configureSettings)
    {
        // Configure settings
        services.Configure(configureSettings);
        
        // Register auth service with its own HttpClient
        services.AddHttpClient<IAmadeusAuthService, AmadeusAuthService>();
        
        // Register flight search service with its own HttpClient
        services.AddHttpClient<IAmadeusFlightSearchService, AmadeusFlightSearchService>();
        
        return services;
    }
}

