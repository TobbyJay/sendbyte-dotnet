using System;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sendbyte.Configuration;

namespace Sendbyte.DependencyInjection;

/// <summary>
/// Service collection extensions for registering the Sendbyte SDK.
/// </summary>
public static class SendbyteServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Sendbyte SDK using an API key.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiKey">The Sendbyte API key.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddSendbyte(
        this IServiceCollection services,
        string apiKey)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("Sendbyte API key is required.", nameof(apiKey));
        }

        return services.AddSendbyte(options =>
        {
            options.ApiKey = apiKey;
        });
    }

    /// <summary>
    /// Registers the Sendbyte SDK using IHttpClientFactory.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action used to configure Sendbyte options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddSendbyte(
        this IServiceCollection services,
        Action<SendbyteOptions> configureOptions)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configureOptions is null)
        {
            throw new ArgumentNullException(nameof(configureOptions));
        }

        services.Configure(configureOptions);

        services.AddHttpClient<ISendbyteClient, SendbyteClient>((serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<SendbyteOptions>>().Value;
            options.Validate();

            httpClient.BaseAddress = new Uri(EnsureTrailingSlash(options.BaseUrl));
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", options.ApiKey);
        });

        return services;
    }

    private static string EnsureTrailingSlash(string value)
    {
        return value.EndsWith("/", StringComparison.Ordinal)
            ? value
            : value + "/";
    }
}