using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sendbyte.Emails.Models;

/// <summary>
/// Represents a delivery lifecycle event for a SendByte email.
/// </summary>
public sealed class EmailEvent
{
    /// <summary>
    /// Event type, for example email.sent or email.delivered.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Event-specific payload.
    /// </summary>
    [JsonPropertyName("payload")]
    public Dictionary<string, JsonElement>? Payload { get; set; }

    /// <summary>
    /// Time the event was recorded.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}
