using System.Text.Json.Serialization;

namespace Sendbyte.Domains.Models;

/// <summary>
/// DNS record that must be published to verify a SendByte sending domain.
/// </summary>
public sealed class DomainDnsRecord
{
    /// <summary>
    /// DNS record type, for example TXT.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// DNS host/name where the record should be published.
    /// </summary>
    [JsonPropertyName("host")]
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// DNS record value to publish.
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Record purpose, for example spf, dkim, or dmarc.
    /// </summary>
    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = string.Empty;

    /// <summary>
    /// Whether this record is required for verification.
    /// </summary>
    [JsonPropertyName("required")]
    public bool Required { get; set; }
}
