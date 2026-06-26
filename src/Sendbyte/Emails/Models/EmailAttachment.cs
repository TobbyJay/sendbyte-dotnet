using System.Text.Json.Serialization;

namespace Sendbyte.Emails.Models;

/// <summary>
/// Represents an attachment for a SendByte email.
/// </summary>
public sealed class EmailAttachment
{
    /// <summary>
    /// File name shown to the recipient.
    /// </summary>
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    /// <summary>
    /// Base64-encoded file content.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// MIME content type, for example application/pdf.
    /// </summary>
    [JsonPropertyName("content_type")]
    public string ContentType { get; set; } = string.Empty;
}
