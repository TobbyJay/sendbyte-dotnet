using Sendbyte;
using Sendbyte.DependencyInjection;
using Sendbyte.Emails.Models;

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

app.Run();
