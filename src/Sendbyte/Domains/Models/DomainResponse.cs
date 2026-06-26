using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sendbyte.Domains.Models;

/// <summary>
/// Represents a SendByte sending domain.
/// </summary>
public sealed class DomainResponse
{
    /// <summary>
    /// Unique domain identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Registered domain name.
    /// </summary>
    [JsonPropertyName("domain")]
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Current domain status, for example pending, verified, or degraded.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// DKIM selector used for this domain.
    /// </summary>
    [JsonPropertyName("dkim_selector")]
    public string? DkimSelector { get; set; }

    /// <summary>
    /// DNS records to publish for verification.
    /// </summary>
    [JsonPropertyName("dns_records")]
    public IReadOnlyList<DomainDnsRecord> DnsRecords { get; set; } = Array.Empty<DomainDnsRecord>();

    /// <summary>
    /// Time the domain was verified, if verified.
    /// </summary>
    [JsonPropertyName("verified_at")]
    public DateTimeOffset? VerifiedAt { get; set; }

    /// <summary>
    /// Time the domain was registered.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }
}
