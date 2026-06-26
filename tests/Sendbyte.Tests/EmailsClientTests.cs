using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Sendbyte;
using Sendbyte.Emails.Models;
using Sendbyte.Exceptions;
using Xunit;

namespace Sendbyte.Tests;

public class EmailsClientTests
{
    [Fact]
    public async Task SendAsync_SendsPostRequestToEmailsEndpoint()
    {
        HttpRequestMessage? capturedRequest = null;

        var client = CreateClient(request =>
        {
            capturedRequest = request;

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = JsonContent("""
                {
                  "id": "em_123",
                  "status": "queued",
                  "sandbox": true,
                  "created_at": "2026-06-26T10:00:00Z"
                }
                """)
            };
        });

        await client.Emails.SendAsync(CreateValidRequest());

        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        Assert.Equal("https://api.sendbyte.africa/v1/emails", capturedRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task SendAsync_SerializesRequiredFields()
    {
        string? body = null;

        var client = CreateClient(async request =>
        {
            body = await request.Content!.ReadAsStringAsync();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = JsonContent("""
                {
                  "id": "em_123",
                  "status": "queued",
                  "sandbox": false,
                  "created_at": "2026-06-26T10:00:00Z"
                }
                """)
            };
        });

        await client.Emails.SendAsync(CreateValidRequest());

        using var document = JsonDocument.Parse(body!);
        var root = document.RootElement;

        Assert.Equal("PayLink <hello@example.com>", root.GetProperty("from").GetString());
        Assert.Equal("customer@example.com", root.GetProperty("to")[0].GetString());
        Assert.Equal("Receipt for ₦45,000", root.GetProperty("subject").GetString());
        Assert.Equal("<p>Your payment was received.</p>", root.GetProperty("html").GetString());
        Assert.Equal("Your payment was received.", root.GetProperty("text").GetString());
    }

    [Fact]
    public async Task SendAsync_SerializesSnakeCaseFields()
    {
        string? body = null;

        var scheduledAt = new DateTimeOffset(2026, 6, 26, 10, 0, 0, TimeSpan.Zero);

        var client = CreateClient(async request =>
        {
            body = await request.Content!.ReadAsStringAsync();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = JsonContent("""
                {
                  "id": "em_123",
                  "status": "queued",
                  "sandbox": false,
                  "scheduled_at": "2026-06-26T10:00:00Z",
                  "created_at": "2026-06-26T09:00:00Z"
                }
                """)
            };
        });

        await client.Emails.SendAsync(new SendEmailRequest
        {
            From = "PayLink <hello@example.com>",
            To = new[] { "customer@example.com" },
            Subject = "Receipt for ₦45,000",
            TemplateId = "tpl_123",
            ReplyTo = "support@example.com",
            ScheduledAt = scheduledAt,
            IdempotencyKey = "order-123-receipt",
            Attachments = new[]
            {
                new EmailAttachment
                {
                    Filename = "receipt.pdf",
                    Content = "YmFzZTY0",
                    ContentType = "application/pdf"
                }
            }
        });

        Assert.Contains("\"template_id\":\"tpl_123\"", body);
        Assert.Contains("\"reply_to\":\"support@example.com\"", body);
        Assert.Contains("\"scheduled_at\":\"2026-06-26T10:00:00+00:00\"", body);
        Assert.Contains("\"idempotency_key\":\"order-123-receipt\"", body);
        Assert.Contains("\"content_type\":\"application/pdf\"", body);
    }

    [Fact]
    public async Task SendAsync_UsesConfiguredAuthorizationHeader()
    {
        HttpRequestMessage? capturedRequest = null;

        var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            capturedRequest = request;

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = JsonContent("""
                {
                  "id": "em_123",
                  "status": "queued",
                  "sandbox": true,
                  "created_at": "2026-06-26T10:00:00Z"
                }
                """)
            };
        }))
        {
            BaseAddress = new Uri("https://api.sendbyte.africa/v1/")
        };

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "sk_test_123");

        var client = new SendbyteClient(httpClient);

        await client.Emails.SendAsync(CreateValidRequest());

        Assert.NotNull(capturedRequest);
        Assert.Equal("Bearer", capturedRequest!.Headers.Authorization!.Scheme);
        Assert.Equal("sk_test_123", capturedRequest.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task SendAsync_ReturnsResponse_OnCreated()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent("""
            {
              "id": "em_123",
              "status": "queued",
              "sandbox": true,
              "created_at": "2026-06-26T10:00:00Z"
            }
            """)
        });

        var response = await client.Emails.SendAsync(CreateValidRequest());

        Assert.Equal("em_123", response.Id);
        Assert.Equal("queued", response.Status);
        Assert.True(response.Sandbox);
    }

    [Fact]
    public async Task SendAsync_ReturnsResponse_OnOkIdempotentReplay()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent("""
            {
              "id": "em_123",
              "status": "queued",
              "sandbox": true,
              "created_at": "2026-06-26T10:00:00Z"
            }
            """)
        });

        var response = await client.Emails.SendAsync(CreateValidRequest());

        Assert.Equal("em_123", response.Id);
    }

    [Fact]
    public async Task SendAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.Emails.SendAsync(null!));
    }

    [Fact]
    public async Task SendAsync_ThrowsArgumentException_WhenFromIsMissing()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created));
        var request = CreateValidRequest();
        request.From = "";

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Emails.SendAsync(request));
    }

    [Fact]
    public async Task SendAsync_ThrowsArgumentException_WhenToIsEmpty()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created));
        var request = CreateValidRequest();
        request.To = Array.Empty<string>();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Emails.SendAsync(request));
    }

    [Fact]
    public async Task SendAsync_ThrowsArgumentException_WhenSubjectIsMissing()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created));
        var request = CreateValidRequest();
        request.Subject = "";

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Emails.SendAsync(request));
    }

    [Fact]
    public async Task SendAsync_ThrowsArgumentException_WhenContentIsMissing()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created));
        var request = CreateValidRequest();
        request.Html = null;
        request.Text = null;
        request.TemplateId = null;

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Emails.SendAsync(request));
    }

    [Fact]
    public async Task SendAsync_ThrowsSendbyteException_OnApiError()
    {
        var client = CreateClient(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.Forbidden)
            {
                Content = JsonContent("""
                {
                  "error": {
                    "code": "domain_not_verified",
                    "message": "The sending domain is not verified.",
                    "docs_url": "https://docs.sendbyte.africa/errors/domain-not-verified"
                  }
                }
                """)
            };

            response.Headers.Add("x-request-id", "req_123");

            return response;
        });

        var exception = await Assert.ThrowsAsync<SendbyteException>(() =>
            client.Emails.SendAsync(CreateValidRequest()));

        Assert.Equal(403, exception.StatusCode);
        Assert.Equal("domain_not_verified", exception.Code);
        Assert.Equal("The sending domain is not verified.", exception.Message);
        Assert.Equal("https://docs.sendbyte.africa/errors/domain-not-verified", exception.DocsUrl);
        Assert.Equal("req_123", exception.RequestId);
    }

    private static SendbyteClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler(handler))
        {
            BaseAddress = new Uri("https://api.sendbyte.africa/v1/")
        };

        return new SendbyteClient(httpClient);
    }

    private static SendbyteClient CreateClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler(handler))
        {
            BaseAddress = new Uri("https://api.sendbyte.africa/v1/")
        };

        return new SendbyteClient(httpClient);
    }

    private static SendEmailRequest CreateValidRequest()
    {
        return new SendEmailRequest
        {
            From = "PayLink <hello@example.com>",
            To = new[] { "customer@example.com" },
            Subject = "Receipt for ₦45,000",
            Html = "<p>Your payment was received.</p>",
            Text = "Your payment was received.",
            Tags = new[] { "receipt", "payment" },
            IdempotencyKey = "order-123-receipt"
        };
    }

    private static StringContent JsonContent(string json)
    {
        return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
            : this(request => Task.FromResult(handler(request)))
        {
        }

        public StubHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return _handler(request);
        }
    }
}
