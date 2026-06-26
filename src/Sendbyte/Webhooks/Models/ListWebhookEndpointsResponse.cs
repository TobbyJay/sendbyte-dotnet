using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sendbyte.Webhooks.Models;

/// <summary>
/// Response returned when listing SendByte webhook endpoints.
/// </summary>
public sealed class ListWebhookEndpointsResponse
{
    /// <summary>
    /// Webhook endpoints.
    /// </summary>
    [JsonPropertyName("data")]
    public IReadOnlyList<WebhookEndpoint> Data { get; set; } = Array.Empty<WebhookEndpoint>();
}
