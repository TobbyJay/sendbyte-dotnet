using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sendbyte.Emails.Models;

/// <summary>
/// Summary representation of a sent SendByte email.
/// </summary>
public sealed class EmailSummary
{
    /// <summary>
    /// Unique email identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Sender address.
    /// </summary>
    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// Recipient addresses.
    /// </summary>
    [JsonPropertyName("to")]
    public IReadOnlyList<string> To { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Email subject.
    /// </summary>
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Current delivery status.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Whether the email was sent using sandbox mode.
    /// </summary>
    [JsonPropertyName("sandbox")]
    public bool Sandbox { get; set; }

    /// <summary>
    /// Time SendByte accepted the email.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}
