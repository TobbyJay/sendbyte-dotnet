using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sendbyte.Webhooks.Models;

/// <summary>
/// Represents a SendByte webhook delivery attempt.
/// </summary>
public sealed class WebhookDelivery
{
    /// <summary>
    /// Unique delivery identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Webhook endpoint identifier associated with this delivery.
    /// </summary>
    [JsonPropertyName("webhook_id")]
    public string WebhookId { get; set; } = string.Empty;

    /// <summary>
    /// Delivered event type.
    /// </summary>
    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    /// <summary>
    /// Delivery attempt status.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// HTTP status code returned by the webhook endpoint, if available.
    /// </summary>
    [JsonPropertyName("status_code")]
    public int? StatusCode { get; set; }

    /// <summary>
    /// Event payload sent to the webhook endpoint.
    /// </summary>
    [JsonPropertyName("payload")]
    public Dictionary<string, JsonElement>? Payload { get; set; }

    /// <summary>
    /// Time the delivery was attempted.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}
