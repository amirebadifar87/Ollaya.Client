using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ollaya.Client.Extensions;

/// <summary>
/// Extension methods for registering OllayaClient with dependency injection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds OllayaClient as a singleton with the given options
    /// </summary>
    public static IServiceCollection AddOllayaClient(
        this IServiceCollection services,
        Action<OllayaClientOptions>? configureOptions = null)
    {
        services.Configure<OllayaClientOptions>(options =>
        {
            configureOptions?.Invoke(options);
        });

        services.AddSingleton<OllayaClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<OllayaClientOptions>>().Value;
            return new OllayaClient(options);
        });

        return services;
    }

    /// <summary>
    /// Adds OllayaClient with options bound from configuration
    /// </summary>
    public static IServiceCollection AddOllayaClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "Ollaya")
    {
        services.Configure<OllayaClientOptions>(configuration.GetSection(sectionName));
        return services.AddOllayaClient();
    }
}