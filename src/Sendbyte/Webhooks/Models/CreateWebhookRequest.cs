using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sendbyte.Webhooks.Models;

/// <summary>
/// Request payload for registering a SendByte webhook endpoint.
/// </summary>
public sealed class CreateWebhookRequest
{
    /// <summary>
    /// Absolute HTTPS endpoint URL that will receive webhook events.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Optional event types to subscribe to. If omitted, SendByte uses the API default.
    /// </summary>
    [JsonPropertyName("events")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Events { get; set; }
}
