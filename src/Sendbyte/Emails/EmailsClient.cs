using System.Net.Http;

namespace Sendbyte.Emails;

/// <summary>
/// Default implementation of the SendByte email client.
/// </summary>
public sealed class EmailsClient : IEmailsClient
{
    private readonly HttpClient _httpClient;

    internal EmailsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}
