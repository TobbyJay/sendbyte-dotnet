using System.Threading;
using System.Threading.Tasks;
using Sendbyte.Domains.Models;

namespace Sendbyte.Domains;

/// <summary>
/// Provides access to SendByte domain APIs.
/// </summary>
public interface IDomainsClient
{
    /// <summary>
    /// Registers a sending domain and returns DNS records to publish.
    /// </summary>
    Task<DomainResponse> CreateAsync(
        CreateDomainRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers a live DNS check for a sending domain.
    /// </summary>
    Task<VerifyDomainResponse> VerifyAsync(
        string id,
        CancellationToken cancellationToken = default);
}
