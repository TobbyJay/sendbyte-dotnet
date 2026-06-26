using System;
using System.Net.Http;
using Sendbyte.Emails;

namespace Sendbyte;

/// <summary>
/// Default Sendbyte API client.
/// </summary>
public sealed class SendbyteClient : ISendbyteClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Creates a new Sendbyte client.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for SendByte API calls.</param>
    public SendbyteClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Emails = new EmailsClient(_httpClient);
    }

    /// <inheritdoc />
    public IEmailsClient Emails { get; }
}
