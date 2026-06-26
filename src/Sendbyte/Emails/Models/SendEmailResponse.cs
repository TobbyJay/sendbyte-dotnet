using System;
using System.Text.Json.Serialization;

namespace Sendbyte.Emails.Models;

/// <summary>
/// Response returned after SendByte accepts an email.
/// </summary>
public sealed class SendEmailResponse
{
    /// <summary>
    /// Unique email identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Current email status.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Whether the email was sent using sandbox mode.
    /// </summary>
    [JsonPropertyName("sandbox")]
    public bool Sandbox { get; set; }

    /// <summary>
    /// Scheduled delivery time, if any.
    /// </summary>
    [JsonPropertyName("scheduled_at")]
    public DateTimeOffset? ScheduledAt { get; set; }

    /// <summary>
    /// Time SendByte accepted the email.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}
