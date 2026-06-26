using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sendbyte.Emails.Models;

/// <summary>
/// Request payload for sending a transactional email.
/// </summary>
public sealed class SendEmailRequest
{
    /// <summary>
    /// Sender address. Use a bare address or display-name format.
    /// </summary>
    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// Recipient email addresses.
    /// </summary>
    [JsonPropertyName("to")]
    public IReadOnlyList<string> To { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Email subject.
    /// </summary>
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// HTML body.
    /// </summary>
    [JsonPropertyName("html")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Html { get; set; }

    /// <summary>
    /// Plain-text body.
    /// </summary>
    [JsonPropertyName("text")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Text { get; set; }

    /// <summary>
    /// SendByte template identifier.
    /// </summary>
    [JsonPropertyName("template_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TemplateId { get; set; }

    /// <summary>
    /// Template variables.
    /// </summary>
    [JsonPropertyName("variables")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object?>? Variables { get; set; }

    /// <summary>
    /// Carbon-copy recipients.
    /// </summary>
    [JsonPropertyName("cc")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Cc { get; set; }

    /// <summary>
    /// Blind-carbon-copy recipients.
    /// </summary>
    [JsonPropertyName("bcc")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Bcc { get; set; }

    /// <summary>
    /// Reply-to address.
    /// </summary>
    [JsonPropertyName("reply_to")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ReplyTo { get; set; }

    /// <summary>
    /// Custom MIME headers.
    /// </summary>
    [JsonPropertyName("headers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Email attachments.
    /// </summary>
    [JsonPropertyName("attachments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<EmailAttachment>? Attachments { get; set; }

    /// <summary>
    /// Tags for filtering and tracking.
    /// </summary>
    [JsonPropertyName("tags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Tags { get; set; }

    /// <summary>
    /// ISO 8601 scheduled delivery time.
    /// </summary>
    [JsonPropertyName("scheduled_at")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? ScheduledAt { get; set; }

    /// <summary>
    /// Unique key used to prevent duplicate sends on retry.
    /// </summary>
    [JsonPropertyName("idempotency_key")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IdempotencyKey { get; set; }
}
