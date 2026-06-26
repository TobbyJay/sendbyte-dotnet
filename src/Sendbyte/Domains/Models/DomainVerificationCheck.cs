using System.Text.Json.Serialization;

namespace Sendbyte.Domains.Models;

/// <summary>
/// Result of a DNS check for a SendByte sending domain.
/// </summary>
public sealed class DomainVerificationCheck
{
    /// <summary>
    /// Check purpose, for example spf, dkim, or dmarc.
    /// </summary>
    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = string.Empty;

    /// <summary>
    /// DNS host that was checked.
    /// </summary>
    [JsonPropertyName("host")]
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Whether this record is required for verification.
    /// </summary>
    [JsonPropertyName("required")]
    public bool Required { get; set; }

    /// <summary>
    /// Whether the DNS check passed.
    /// </summary>
    [JsonPropertyName("pass")]
    public bool Pass { get; set; }
}
