# Sendbyte .NET SDK

A .NET SDK for the SendByte API.

> Status: early development. The SDK currently supports ASP.NET Core dependency injection, typed email sending, email retrieval, email listing, domain registration, domain verification, webhook endpoint management, webhook signature verification, basic validation, and API error handling.

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

## Create a Webhook Endpoint

The webhook secret is returned only when the endpoint is created. Store it securely and use it for signature verification.

```csharp
using Sendbyte.Webhooks;
using Sendbyte.Webhooks.Models;

var webhook = await _sendbyte.Webhooks.CreateAsync(new CreateWebhookRequest
{
    Url = "https://example.com/webhooks/sendbyte",
    Events = new[]
    {
        WebhookEventTypes.EmailDelivered,
        WebhookEventTypes.EmailBounced,
        WebhookEventTypes.DomainVerified
    }
});

_logger.LogInformation(
    "Created Sendbyte webhook {WebhookId}. Store the returned secret securely.",
    webhook.Id);
```

## List Webhook Endpoints

List responses do not include webhook secrets.

```csharp
var webhooks = await _sendbyte.Webhooks.ListAsync();

foreach (var webhook in webhooks.Data)
{
    _logger.LogInformation(
        "Webhook {WebhookId} posts to {Url}. Disabled: {Disabled}",
        webhook.Id,
        webhook.Url,
        webhook.Disabled);
}
```

## Disable a Webhook Endpoint

```csharp
await _sendbyte.Webhooks.DisableAsync("wh_123");

_logger.LogInformation("Disabled Sendbyte webhook {WebhookId}", "wh_123");
```

## List Webhook Deliveries

```csharp
var deliveries = await _sendbyte.Webhooks.ListDeliveriesAsync("wh_123");

foreach (var delivery in deliveries.Data)
{
    _logger.LogInformation(
        "Webhook delivery {DeliveryId} for {Event} has status {Status}",
        delivery.Id,
        delivery.Event,
        delivery.Status);
}
```

## Replay a Webhook Delivery

```csharp
var delivery = await _sendbyte.Webhooks.ReplayDeliveryAsync("wd_123");

_logger.LogInformation(
    "Replayed Sendbyte webhook delivery {DeliveryId}. Status: {Status}",
    delivery.Id,
    delivery.Status);
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
- Create webhook endpoints
- List webhook endpoints
- Disable webhook endpoints
- List webhook deliveries
- Replay webhook deliveries
- Webhook signature verification
- Typed request/response models
- Basic request validation
- Basic API error handling
- Idempotency key support

## Coming Soon

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
