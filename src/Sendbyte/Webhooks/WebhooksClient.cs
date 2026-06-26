using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Sendbyte.Exceptions;
using Sendbyte.Webhooks.Models;

namespace Sendbyte.Webhooks;

/// <summary>
/// Default implementation of the SendByte webhooks client.
/// </summary>
public sealed class WebhooksClient : IWebhooksClient
{
    private readonly HttpClient _httpClient;

    internal WebhooksClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public async Task<WebhookEndpoint> CreateAsync(
        CreateWebhookRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateWebhookRequest(request);

        var json = JsonSerializer.Serialize(request);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient
            .PostAsync("webhooks", content, cancellationToken)
            .ConfigureAwait(false);

        return await HandleResponseAsync<WebhookEndpoint>(response).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ListWebhookEndpointsResponse> ListAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient
            .GetAsync("webhooks", cancellationToken)
            .ConfigureAwait(false);

        return await HandleResponseAsync<ListWebhookEndpointsResponse>(response).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DisableAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Webhook endpoint ID is required.", nameof(id));
        }

        using var response = await _httpClient
            .DeleteAsync("webhooks/" + Uri.EscapeDataString(id), cancellationToken)
            .ConfigureAwait(false);

        await HandleResponseAsync(response).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ListWebhookDeliveriesResponse> ListDeliveriesAsync(
        string webhookEndpointId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(webhookEndpointId))
        {
            throw new ArgumentException("Webhook endpoint ID is required.", nameof(webhookEndpointId));
        }

        using var response = await _httpClient
            .GetAsync("webhooks/" + Uri.EscapeDataString(webhookEndpointId) + "/deliveries", cancellationToken)
            .ConfigureAwait(false);

        return await HandleResponseAsync<ListWebhookDeliveriesResponse>(response).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<WebhookDelivery> ReplayDeliveryAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Webhook delivery ID is required.", nameof(id));
        }

        using var response = await _httpClient
            .PostAsync("webhooks/deliveries/" + Uri.EscapeDataString(id) + "/replay", content: null, cancellationToken)
            .ConfigureAwait(false);

        return await HandleResponseAsync<WebhookDelivery>(response).ConfigureAwait(false);
    }

    private static void ValidateCreateWebhookRequest(CreateWebhookRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Url))
        {
            throw new ArgumentException("Webhook URL is required.", nameof(request));
        }

        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) ||
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Webhook URL must be an absolute HTTPS URL.", nameof(request));
        }

        if (request.Events is not null && request.Events.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Webhook events cannot be empty.", nameof(request));
        }
    }

    private static async Task HandleResponseAsync(HttpResponseMessage response)
    {
        var responseBody = response.Content is null
            ? string.Empty
            : await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        throw CreateSendbyteException(response, responseBody);
    }

    private static async Task<T> HandleResponseAsync<T>(HttpResponseMessage response)
    {
        var responseBody = response.Content is null
            ? string.Empty
            : await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            return DeserializeResponse<T>(responseBody);
        }

        throw CreateSendbyteException(response, responseBody);
    }

    private static T DeserializeResponse<T>(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            throw new SendbyteException("Sendbyte API returned an empty response body.");
        }

        try
        {
            var response = JsonSerializer.Deserialize<T>(responseBody);

            if (response is null)
            {
                throw new SendbyteException("Sendbyte API response could not be deserialized.");
            }

            return response;
        }
        catch (JsonException exception)
        {
            throw new SendbyteException(
                "Sendbyte API response could not be deserialized.",
                exception);
        }
    }

    private static SendbyteException CreateSendbyteException(
        HttpResponseMessage response,
        string responseBody)
    {
        var statusCode = (int)response.StatusCode;
        var requestId = TryGetHeader(response, "x-request-id") ?? TryGetHeader(response, "request-id");

        var parsedError = TryParseError(responseBody);

        var message = parsedError?.Message;

        if (string.IsNullOrWhiteSpace(message))
        {
            message = $"Sendbyte API request failed with status code {statusCode} ({response.ReasonPhrase}).";
        }

        return new SendbyteException(
            message!,
            statusCode,
            parsedError?.Code,
            parsedError?.DocsUrl,
            requestId);
    }

    private static ParsedSendbyteError? TryParseError(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            var wrappedError = JsonSerializer.Deserialize<WrappedErrorResponse>(responseBody);

            if (wrappedError?.Error is not null)
            {
                return new ParsedSendbyteError(
                    wrappedError.Error.Message,
                    wrappedError.Error.Code,
                    wrappedError.Error.DocsUrl);
            }

            var topLevelError = JsonSerializer.Deserialize<TopLevelErrorResponse>(responseBody);

            if (topLevelError is not null)
            {
                return new ParsedSendbyteError(
                    topLevelError.Message,
                    topLevelError.Code,
                    topLevelError.DocsUrl);
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    private static string? TryGetHeader(HttpResponseMessage response, string headerName)
    {
        if (response.Headers.TryGetValues(headerName, out var values))
        {
            return values.FirstOrDefault();
        }

        return null;
    }

    private sealed class WrappedErrorResponse
    {
        [JsonPropertyName("error")]
        public ErrorBody? Error { get; set; }
    }

    private sealed class TopLevelErrorResponse
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("docs_url")]
        public string? DocsUrl { get; set; }
    }

    private sealed class ErrorBody
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("docs_url")]
        public string? DocsUrl { get; set; }
    }

    private sealed class ParsedSendbyteError
    {
        public ParsedSendbyteError(string? message, string? code, string? docsUrl)
        {
            Message = message;
            Code = code;
            DocsUrl = docsUrl;
        }

        public string? Message { get; }

        public string? Code { get; }

        public string? DocsUrl { get; }
    }
}
