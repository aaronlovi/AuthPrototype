using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OAuthToolkit.Contracts;
using OAuthToolkit.Models;
using OAuthToolkit.Services;

namespace OAuthToolkit;

public static class OAuthToolkitHostConfig {
    public static IServiceCollection ConfigureOAuthToolkit(this IServiceCollection services, IConfiguration configuration) {
        services.
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
