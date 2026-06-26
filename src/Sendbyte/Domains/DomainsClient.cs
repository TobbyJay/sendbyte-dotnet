using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Sendbyte.Domains.Models;
using Sendbyte.Exceptions;

namespace Sendbyte.Domains;

/// <summary>
/// Default implementation of the SendByte domains client.
/// </summary>
public sealed class DomainsClient : IDomainsClient
{
    private readonly HttpClient _httpClient;

    internal DomainsClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public async Task<DomainResponse> CreateAsync(
        CreateDomainRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateDomainRequest(request);

        var json = JsonSerializer.Serialize(request);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient
            .PostAsync("domains", content, cancellationToken)
            .ConfigureAwait(false);

        return await HandleResponseAsync<DomainResponse>(response).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<VerifyDomainResponse> VerifyAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Domain ID is required.", nameof(id));
        }

        using var response = await _httpClient
            .PostAsync("domains/" + Uri.EscapeDataString(id) + "/verify", content: null, cancellationToken)
            .ConfigureAwait(false);

        return await HandleResponseAsync<VerifyDomainResponse>(response).ConfigureAwait(false);
    }

    private static void ValidateCreateDomainRequest(CreateDomainRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Domain))
        {
            throw new ArgumentException("Domain is required.", nameof(request));
        }

        if (request.Domain.Contains("://") || request.Domain.Contains("/"))
        {
            throw new ArgumentException("Domain must be a bare domain without scheme or path.", nameof(request));
        }
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
