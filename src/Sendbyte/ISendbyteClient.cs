using Sendbyte.Domains;
using Sendbyte.Emails;
using Sendbyte.Webhooks;

namespace Sendbyte;

/// <summary>
/// Root client for accessing SendByte APIs.
/// </summary>
public interface ISendbyteClient
{
    /// <summary>
    /// Provides access to SendByte email APIs.
    /// </summary>
    IEmailsClient Emails { get; }

    /// <summary>
    /// Provides access to SendByte domain APIs.
    /// </summary>
    IDomainsClient Domains { get; }

    /// <summary>
    /// Provides access to SendByte webhook management APIs.
    /// </summary>
    IWebhooksClient Webhooks { get; }
}
