using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sendbyte.Domains.Models;

/// <summary>
/// Response returned after triggering domain verification.
/// </summary>
public sealed class VerifyDomainResponse
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
    /// Domain status after verification.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Per-record DNS verification checks.
    /// </summary>
    [JsonPropertyName("checks")]
    public IReadOnlyList<DomainVerificationCheck> Checks { get; set; } = Array.Empty<DomainVerificationCheck>();
}
