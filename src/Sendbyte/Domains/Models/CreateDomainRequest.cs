using System.Text.Json.Serialization;

namespace Sendbyte.Domains.Models;

/// <summary>
/// Request payload for registering a SendByte sending domain.
/// </summary>
public sealed class CreateDomainRequest
{
    /// <summary>
    /// Bare domain name to register, for example paylink.ng or mail.paylink.ng.
    /// </summary>
    [JsonPropertyName("domain")]
    public string Domain { get; set; } = string.Empty;
}
