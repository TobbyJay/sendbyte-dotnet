using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sendbyte.Emails.Models;

/// <summary>
/// Paginated response returned when listing SendByte emails.
/// </summary>
public sealed class ListEmailsResponse
{
    /// <summary>
    /// Email summaries.
    /// </summary>
    [JsonPropertyName("data")]
    public IReadOnlyList<EmailSummary> Data { get; set; } = Array.Empty<EmailSummary>();

    /// <summary>
    /// Whether more records exist after the current page.
    /// </summary>
    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }
}
