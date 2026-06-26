using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sendbyte.Webhooks.Models;

/// <summary>
/// Represents a SendByte webhook endpoint.
/// </summary>
public sealed class WebhookEndpoint
{
    /// <summary>
    /// Unique webhook endpoint identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Endpoint URL that receives webhook events.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Event types subscribed by this endpoint.
    /// </summary>
    [JsonPropertyName("events")]
    public IReadOnlyList<string> Events { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Whether the webhook endpoint is disabled.
    /// </summary>
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// Signing secret. Only returned when creating a webhook endpoint.
    /// </summary>
    [JsonPropertyName("secret")]
    public string? Secret { get; set; }

    /// <summary>
    /// Time the webhook endpoint was created.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}
