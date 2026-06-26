using System.Text;
using Sendbyte;
using Sendbyte.DependencyInjection;
using Sendbyte.Emails.Models;
using Sendbyte.Webhooks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSendbyte(options =>
{
    options.ApiKey = builder.Configuration["Sendbyte:ApiKey"] ?? "sk_test_placeholder";
});

var app = builder.Build();

app.MapGet("/", () => "Sendbyte ASP.NET Core sample is running");

app.MapPost("/send-test-email", async (
    ISendbyteClient sendbyte,
    CancellationToken cancellationToken) =>
{
    var response = await sendbyte.Emails.SendAsync(new SendEmailRequest
    {
        From = "Sendbyte <hello@example.com>",
        To = new[] { "customer@example.com" },
        Subject = "Hello from Sendbyte .NET",
        Html = "<p>This is a test email from the Sendbyte .NET SDK.</p>",
        Text = "This is a test email from the Sendbyte .NET SDK.",
        IdempotencyKey = Guid.NewGuid().ToString("N")
    }, cancellationToken);

    return Results.Ok(response);
});

app.MapGet("/emails/{id}", async (
    string id,
    ISendbyteClient sendbyte,
    CancellationToken cancellationToken) =>
{
    var email = await sendbyte.Emails.GetAsync(id, cancellationToken);

    return Results.Ok(email);
});

app.MapGet("/emails", async (
    ISendbyteClient sendbyte,
    int? limit,
    string? after,
    string? status,
    CancellationToken cancellationToken) =>
{
    var emails = await sendbyte.Emails.ListAsync(new ListEmailsRequest
    {
        Limit = limit,
        After = after,
        Status = status
    }, cancellationToken);

    return Results.Ok(emails);
});

app.MapPost("/webhooks/sendbyte", async (
    HttpRequest request,
    IConfiguration configuration,
    CancellationToken cancellationToken) =>
{
    using var reader = new StreamReader(
        request.Body,
        Encoding.UTF8,
        detectEncodingFromByteOrderMarks: false,
        leaveOpen: false);

    var rawBody = await reader.ReadToEndAsync(cancellationToken);
    var signatureHeader = request.Headers[SendbyteWebhookVerifier.SignatureHeader].ToString();
    var webhookSecret = configuration["Sendbyte:WebhookSecret"];

    if (!SendbyteWebhookVerifier.VerifySignature(webhookSecret!, signatureHeader, rawBody))
    {
        return Results.Unauthorized();
    }

    return Results.Ok();
});
app.Run();
