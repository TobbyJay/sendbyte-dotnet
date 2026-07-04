# Sendbyte .NET SDK

A .NET SDK for the SendByte API.

> Status: stable initial release. The SDK supports ASP.NET Core dependency injection, email sending, email retrieval/listing, domain registration/verification, webhook endpoint management, webhook delivery replay, webhook signature verification, validation, and API error handling.

## Installation

```bash
dotnet add package Sendbyte
```

Or install a specific version:

```bash
dotnet add package Sendbyte --version 1.0.0
```

## ASP.NET Core Usage

Register the SDK with dependency injection:

```csharp
using Sendbyte.DependencyInjection;

builder.Services.AddSendbyte(builder.Configuration["Sendbyte:ApiKey"]!);
```

You can also configure options manually:

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
    From = "Tobby Umoh <hello@try.sendbyte.africa>",
    To = new[] { "customer@example.com" },
    Subject = "Hello from Sendbyte",
    Html = "<p>Your email was sent successfully.</p>",
    Text = "Your email was sent successfully.",
    Tags = new[] { "welcome", "test" },
    IdempotencyKey = "welcome-email-123"
});

_logger.LogInformation(
    "Sendbyte accepted email {EmailId} with status {Status}",
    response.Id,
    response.Status);
```

For live sending, the `From` domain must be verified in SendByte. For testing, use SendByte’s test sender where supported, for example `hello@try.sendbyte.africa`.

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
using Sendbyte.Emails.Models;

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
    Domain = "example.com"
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

_logger.LogInformation("Disabled Sendbyte webhook endpoint {WebhookId}", "wh_123");
```

## List Webhook Deliveries

```csharp
var deliveries = await _sendbyte.Webhooks.ListDeliveriesAsync("wh_123");

foreach (var delivery in deliveries.Data)
{
    _logger.LogInformation(
        "Webhook delivery {DeliveryId} for {EventType}. StatusCode: {StatusCode}. Succeeded: {Succeeded}",
        delivery.Id,
        delivery.EventType,
        delivery.StatusCode,
        delivery.Succeeded);
}
```

## Replay a Webhook Delivery

```csharp
var replay = await _sendbyte.Webhooks.ReplayDeliveryAsync("del_123");

_logger.LogInformation(
    "Created replay delivery {DeliveryId} from {OriginalDeliveryId}",
    replay.Id,
    replay.ReplayOf);
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

## Development

Restore, build, and test the solution:

```bash
dotnet restore
dotnet build
dotnet test
```

Create a release package locally:

```bash
dotnet pack src/Sendbyte/Sendbyte.csproj -c Release -o ./artifacts
```

## License

MIT
