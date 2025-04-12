using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OAuthToolkit.Contracts;
using OAuthToolkit.Models;
using OAuthToolkit.Services;

namespace OAuthToolkit;

/// <summary>
/// Provides extension methods for configuring OAuthToolkit services in a host application.
/// </summary>
public static class OAuthToolkitHostConfig {
    /// <summary>
    /// Configures and registers all required OAuthToolkit services in the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The configuration containing OAuthToolkit settings.</param>
    /// <returns>The same service collection for method chaining.</returns>
    /// <remarks>
    /// This method registers the following services:
    /// - <see cref="TokenServiceOptions"/> from configuration
    /// - In-memory cache for token storage
    /// - <see cref="ITokenCache"/> implementation
    /// - <see cref="ITokenHttpClient"/> and its implementation
    /// - <see cref="ITokenValidator"/> implementation
    /// - <see cref="ITokenService"/> implementation
    /// </remarks>
    public static IServiceCollection ConfigureOAuthToolkit(this IServiceCollection services, IConfiguration configuration) {
        _ = services.
            Configure<TokenServiceOptions>(configuration.GetSection("TokenServiceOptions")).
            AddMemoryCache().
            AddSingleton<ITokenCache, MemoryTokenCache>().
            AddSingleton<ITokenHttpClient, TokenHttpClient>().
            AddSingleton<ITokenValidator, TokenValidator>().
            AddSingleton<ITokenService, TokenService>().
            AddHttpClient<ITokenHttpClient, TokenHttpClient>();

        return services;
    }
}
