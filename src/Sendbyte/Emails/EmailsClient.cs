using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Sendbyte.Emails.Models;
using Sendbyte.Exceptions;

namespace Sendbyte.Emails;

/// <summary>
/// Default implementation of the SendByte email client.
/// </summary>
public sealed class EmailsClient : IEmailsClient
{
    private readonly HttpClient _httpClient;

    internal EmailsClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public async Task<SendEmailResponse> SendAsync(
        SendEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateSendEmailRequest(request);

        var json = JsonSerializer.Serialize(request);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient
            .PostAsync("emails", content, cancellationToken)
            .ConfigureAwait(false);

        return await HandleResponseAsync<SendEmailResponse>(response).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<EmailDetails> GetAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Email ID is required.", nameof(id));
        }

        using var response = await _httpClient
            .GetAsync("emails/" + Uri.EscapeDataString(id), cancellationToken)
            .ConfigureAwait(false);

        return await HandleResponseAsync<EmailDetails>(response).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ListEmailsResponse> ListAsync(
        ListEmailsRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        ValidateListEmailsRequest(request);

        var path = BuildListEmailsPath(request);

        using var response = await _httpClient
            .GetAsync(path, cancellationToken)
            .ConfigureAwait(false);

        return await HandleResponseAsync<ListEmailsResponse>(response).ConfigureAwait(false);
    }

    private static void ValidateSendEmailRequest(SendEmailRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.From))
        {
            throw new ArgumentException("From is required.", nameof(request));
        }

        if (request.To is null || request.To.Count == 0 || request.To.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("At least one recipient is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Subject))
        {
            throw new ArgumentException("Subject is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Html)
            && string.IsNullOrWhiteSpace(request.Text)
            && string.IsNullOrWhiteSpace(request.TemplateId))
        {
            throw new ArgumentException("At least one of Html, Text, or TemplateId is required.", nameof(request));
        }
    }

    private static void ValidateListEmailsRequest(ListEmailsRequest? request)
    {
        if (request?.Limit is null)
        {
            return;
        }

        if (request.Limit < 1 || request.Limit > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "Limit must be between 1 and 100.");
        }
    }

    private static string BuildListEmailsPath(ListEmailsRequest? request)
    {
        if (request is null)
        {
            return "emails";
        }

        var queryParts = new List<string>();

        if (request.Limit.HasValue)
        {
            queryParts.Add("limit=" + request.Limit.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.After))
        {
            queryParts.Add("after=" + Uri.EscapeDataString(request.After));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            queryParts.Add("status=" + Uri.EscapeDataString(request.Status));
        }

        return queryParts.Count == 0
            ? "emails"
            : "emails?" + string.Join("&", queryParts);
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
            message,
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
