using System.Threading;
using System.Threading.Tasks;
using Sendbyte.Emails.Models;

namespace Sendbyte.Emails;

/// <summary>
/// Provides access to SendByte email APIs.
/// </summary>
public interface IEmailsClient
{
    /// <summary>
    /// Sends a transactional email.
    /// </summary>
    Task<SendEmailResponse> SendAsync(
        SendEmailRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a sent email by ID.
    /// </summary>
    Task<EmailDetails> GetAsync(
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists sent emails.
    /// </summary>
    Task<ListEmailsResponse> ListAsync(
        ListEmailsRequest? request = null,
        CancellationToken cancellationToken = default);
}
