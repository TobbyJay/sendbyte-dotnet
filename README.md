# Sendbyte .NET SDK

A .NET SDK for the SendByte API.

> Status: early development. The SDK currently supports ASP.NET Core dependency injection, typed email sending, email retrieval, email listing, domain registration, domain verification, webhook signature verification, basic validation, and API error handling.

## Installation

```bash
dotnet add package Sendbyte --prerelease
```

## ASP.NET Core Usage

Register the SDK with dependency injection:

```csharp
using Sendbyte.DependencyInjection;

builder.Services.AddSendbyte(options =>
{
    options.ApiKey = builder.Configuration["Sendbyte:ApiKey"]!;
});
```

Inject `ISendbyteClient` into your services:

```csharp
using Sendbyte;

public sealed class NotificationService
{
    private readonly ISendbyteClient _sendbyte;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        ISendbyteClient sendbyte,
        ILogger<NotificationService> logger)
    {
        _sendbyte = sendbyte;
        _logger = logger;
    }
}
```

## Send an Email

```csharp
using Sendbyte.Emails.Models;

var response = await _sendbyte.Emails.SendAsync(new SendEmailRequest
{
    From = "PayLink <hello@example.com>",
    To = new[] { "customer@example.com" },
    Subject = "Receipt for ₦45,000",
    Html = "<p>Your payment was received.</p>",
    Text = "Your payment was received.",
    Tags = new[] { "receipt", "payment" },
    IdempotencyKey = "order-123-receipt"
});

_logger.LogInformation("Sendbyte accepted email {EmailId}", response.Id);
```

## Retrieve an Email

```csharp
var email = await _sendbyte.Emails.GetAsync("em_123");

_logger.LogInformation(
    "Email {EmailId} has status {Status}",
    email.Id,
    email.Status);
```

## List Emails

```csharp
var emails = await _sendbyte.Emails.ListAsync(new ListEmailsRequest
{
    Limit = 20,
    Status = "delivered"
});

foreach (var email in emails.Data)
{
    _logger.LogInformation(
        "Email {EmailId} to {RecipientCount} recipient(s) has status {Status}",
        email.Id,
        email.To.Count,
        email.Status);
}
```

## Register a Domain

```csharp
using Sendbyte.Domains.Models;

var domain = await _sendbyte.Domains.CreateAsync(new CreateDomainRequest
{
    Domain = "paylink.ng"
});

foreach (var record in domain.DnsRecords)
{
    _logger.LogInformation(
        "Publish {Type} record for {Purpose}: {Host} => {Value}",
        record.Type,
        record.Purpose,
        record.Host,
        record.Value);
}
```

## Verify a Domain

```csharp
var verification = await _sendbyte.Domains.VerifyAsync("dom_123");

foreach (var check in verification.Checks)
{
    _logger.LogInformation(
        "Domain check {Purpose} for {Host}. Required: {Required}. Passed: {Passed}",
        check.Purpose,
        check.Host,
        check.Required,
        check.Pass);
}
```

## Webhook Signature Verification

Verify SendByte webhooks against the raw request body before trusting the payload.

```csharp
using Sendbyte.Webhooks;

var isValid = SendbyteWebhookVerifier.VerifySignature(
    webhookSecret,
    signatureHeader,
    rawBody);

if (!isValid)
{
    return Results.Unauthorized();
}
```

The signature header name is available as:

```csharp
SendbyteWebhookVerifier.SignatureHeader
```

## Error Handling

Non-success API responses throw `SendbyteException`.

```csharp
using Sendbyte.Exceptions;

try
{
    await _sendbyte.Emails.SendAsync(request);
}
catch (SendbyteException exception)
{
    _logger.LogError(
        exception,
        "Sendbyte request failed. StatusCode: {StatusCode}, Code: {Code}, RequestId: {RequestId}",
        exception.StatusCode,
        exception.Code,
        exception.RequestId);
}
```

## Supported Features

- ASP.NET Core dependency injection
- Transactional email sending
- Retrieve email by ID
- List sent emails
- Register sending domains
- Verify sending domains
- Webhook signature verification
- Typed request/response models
- Basic request validation
- Basic API error handling
- Idempotency key support

## Coming Soon

- Webhook management APIs
- Template APIs
- NuGet publishing

## Development

Restore, build, and test the solution:

```bash
dotnet restore
dotnet build
dotnet test
```

## License

MIT
