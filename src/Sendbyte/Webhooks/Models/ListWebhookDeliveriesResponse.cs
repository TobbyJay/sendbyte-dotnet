using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sendbyte.Webhooks.Models;

/// <summary>
/// Response returned when listing SendByte webhook delivery attempts.
/// </summary>
public sealed class ListWebhookDeliveriesResponse
{
    /// <summary>
    /// Webhook delivery attempts.
    /// </summary>
    [JsonPropertyName("data")]
    public IReadOnlyList<WebhookDelivery> Data { get; set; } = Array.Empty<WebhookDelivery>();
}
