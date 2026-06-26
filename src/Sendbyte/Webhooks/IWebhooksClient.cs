using System.Threading;
using System.Threading.Tasks;
using Sendbyte.Webhooks.Models;

namespace Sendbyte.Webhooks;

/// <summary>
/// Provides access to SendByte webhook management APIs.
/// </summary>
public interface IWebhooksClient
{
    /// <summary>
    /// Registers a webhook endpoint.
    /// </summary>
    Task<WebhookEndpoint> CreateAsync(
        CreateWebhookRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists webhook endpoints.
    /// </summary>
    Task<ListWebhookEndpointsResponse> ListAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Disables a webhook endpoint.
    /// </summary>
    Task DisableAsync(
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists recent delivery attempts for a webhook endpoint.
    /// </summary>
    Task<ListWebhookDeliveriesResponse> ListDeliveriesAsync(
        string webhookEndpointId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Replays a webhook delivery.
    /// </summary>
    Task<WebhookDelivery> ReplayDeliveryAsync(
        string id,
        CancellationToken cancellationToken = default);
}
