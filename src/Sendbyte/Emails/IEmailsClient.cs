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
    /// <param name="request">The email send request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The accepted email response.</returns>
    Task<SendEmailResponse> SendAsync(
        SendEmailRequest request,
        CancellationToken cancellationToken = default);
}
