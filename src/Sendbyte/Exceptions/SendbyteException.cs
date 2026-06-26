using System;

namespace Sendbyte.Exceptions;

/// <summary>
/// Represents an error returned by the SendByte API.
/// </summary>
public class SendbyteException : Exception
{
    /// <summary>
    /// Creates a new Sendbyte exception.
    /// </summary>
    public SendbyteException()
    {
    }

    /// <summary>
    /// Creates a new Sendbyte exception with a message.
    /// </summary>
    public SendbyteException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates a new Sendbyte exception with a message and inner exception.
    /// </summary>
    public SendbyteException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Creates a new Sendbyte API exception.
    /// </summary>
    public SendbyteException(
        string message,
        int? statusCode,
        string? code = null,
        string? docsUrl = null,
        string? requestId = null)
        : base(message)
    {
        StatusCode = statusCode;
        Code = code;
        DocsUrl = docsUrl;
        RequestId = requestId;
    }

    /// <summary>
    /// HTTP status code returned by the API, if available.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// SendByte error code, if available.
    /// </summary>
    public string? Code { get; }

    /// <summary>
    /// Documentation URL for the error, if available.
    /// </summary>
    public string? DocsUrl { get; }

    /// <summary>
    /// Request identifier returned by the API, if available.
    /// </summary>
    public string? RequestId { get; }
}
