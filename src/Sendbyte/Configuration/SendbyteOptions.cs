using System;

namespace Sendbyte.Configuration;

/// <summary>
/// Configuration options for the Sendbyte SDK.
/// </summary>
public sealed class SendbyteOptions
{
    /// <summary>
    /// Default SendByte API base URL.
    /// </summary>
    public const string DefaultBaseUrl = "https://api.sendbyte.africa/v1/";

    /// <summary>
    /// SendByte API key.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// SendByte API base URL.
    /// </summary>
    public string BaseUrl { get; set; } = DefaultBaseUrl;

    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            throw new InvalidOperationException("Sendbyte API key is required. Configure it using options.ApiKey.");
        }

        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            throw new InvalidOperationException("Sendbyte BaseUrl is required.");
        }

        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("Sendbyte BaseUrl must be a valid absolute URL.");
        }
    }
}
