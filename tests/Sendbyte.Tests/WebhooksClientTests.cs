using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Sendbyte;
using Sendbyte.Exceptions;
using Sendbyte.Webhooks;
using Sendbyte.Webhooks.Models;
using Xunit;

namespace Sendbyte.Tests;

public class WebhooksClientTests
{
    [Fact]
    public async Task CreateAsync_SendsPostRequestToWebhooksEndpoint()
    {
        HttpRequestMessage? capturedRequest = null;

        var client = CreateClient(request =>
        {
            capturedRequest = request;

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = JsonContent("""
                {
                  "id": "wh_123",
                  "url": "https://example.com/webhooks/sendbyte",
                  "events": ["email.sent"],
                  "disabled": false,
                  "secret": "whsec_123",
                  "created_at": "2026-06-26T10:00:00Z"
                }
                """)
            };
        });

        await client.Webhooks.CreateAsync(CreateValidRequest());

        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        Assert.Equal("https://api.sendbyte.africa/v1/webhooks", capturedRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task CreateAsync_SerializesUrlAndEvents()
    {
        string? body = null;

        var client = CreateClient(async request =>
        {
            body = await request.Content!.ReadAsStringAsync();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = JsonContent("""
                {
                  "id": "wh_123",
                  "url": "https://example.com/webhooks/sendbyte",
                  "events": ["email.sent", "email.delivered"],
                  "disabled": false,
                  "secret": "whsec_123",
                  "created_at": "2026-06-26T10:00:00Z"
                }
                """)
            };
        });

        await client.Webhooks.CreateAsync(CreateValidRequest());

        using var document = JsonDocument.Parse(body!);
        var root = document.RootElement;

        Assert.Equal("https://example.com/webhooks/sendbyte", root.GetProperty("url").GetString());
        Assert.Equal(WebhookEventTypes.EmailSent, root.GetProperty("events")[0].GetString());
        Assert.Equal(WebhookEventTypes.EmailDelivered, root.GetProperty("events")[1].GetString());
    }

    [Fact]
    public async Task CreateAsync_OmitsEvents_WhenEventsIsNull()
    {
        string? body = null;

        var client = CreateClient(async request =>
        {
            body = await request.Content!.ReadAsStringAsync();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = JsonContent("""
                {
                  "id": "wh_123",
                  "url": "https://example.com/webhooks/sendbyte",
                  "events": [],
                  "disabled": false,
                  "secret": "whsec_123",
                  "created_at": "2026-06-26T10:00:00Z"
                }
                """)
            };
        });

        await client.Webhooks.CreateAsync(new CreateWebhookRequest
        {
            Url = "https://example.com/webhooks/sendbyte"
        });

        Assert.DoesNotContain("\"events\"", body);
    }

    [Fact]
    public async Task CreateAsync_ReturnsWebhookEndpointWithSecret_OnCreated()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent("""
            {
              "id": "wh_123",
              "url": "https://example.com/webhooks/sendbyte",
              "events": ["email.sent", "domain.verified"],
              "disabled": false,
              "secret": "whsec_123",
              "created_at": "2026-06-26T10:00:00Z"
            }
            """)
        });

        var response = await client.Webhooks.CreateAsync(CreateValidRequest());

        Assert.Equal("wh_123", response.Id);
        Assert.Equal("https://example.com/webhooks/sendbyte", response.Url);
        Assert.Equal(2, response.Events.Count);
        Assert.False(response.Disabled);
        Assert.Equal("whsec_123", response.Secret);
        Assert.Equal(new DateTimeOffset(2026, 6, 26, 10, 0, 0, TimeSpan.Zero), response.CreatedAt);
    }

    [Fact]
    public async Task CreateAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.Webhooks.CreateAsync(null!));
    }

    [Fact]
    public async Task CreateAsync_ThrowsArgumentException_WhenUrlIsMissing()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Webhooks.CreateAsync(new CreateWebhookRequest { Url = "" }));
    }

    [Theory]
    [InlineData("http://example.com/webhooks/sendbyte")]
    [InlineData("/webhooks/sendbyte")]
    [InlineData("not-a-url")]
    public async Task CreateAsync_ThrowsArgumentException_WhenUrlIsNotAbsoluteHttps(string url)
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Webhooks.CreateAsync(new CreateWebhookRequest { Url = url }));
    }

    [Fact]
    public async Task CreateAsync_ThrowsArgumentException_WhenEventIsEmpty()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Webhooks.CreateAsync(new CreateWebhookRequest
            {
                Url = "https://example.com/webhooks/sendbyte",
                Events = new[] { WebhookEventTypes.EmailSent, "" }
            }));
    }

    [Fact]
    public async Task ListAsync_SendsGetRequestToWebhooksEndpoint()
    {
        HttpRequestMessage? capturedRequest = null;

        var client = CreateClient(request =>
        {
            capturedRequest = request;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent("""
                {
                  "data": []
                }
                """)
            };
        });

        await client.Webhooks.ListAsync();

        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Get, capturedRequest!.Method);
        Assert.Equal("https://api.sendbyte.africa/v1/webhooks", capturedRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task ListAsync_ReturnsWebhookEndpointsWithoutSecret()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent("""
            {
              "data": [
                {
                  "id": "wh_123",
                  "url": "https://example.com/webhooks/sendbyte",
                  "events": ["email.sent"],
                  "disabled": false,
                  "created_at": "2026-06-26T10:00:00Z"
                }
              ]
            }
            """)
        });

        var response = await client.Webhooks.ListAsync();

        Assert.Single(response.Data);
        Assert.Equal("wh_123", response.Data[0].Id);
        Assert.Null(response.Data[0].Secret);
    }

    [Fact]
    public async Task DisableAsync_SendsDeleteRequestToWebhookEndpoint()
    {
        HttpRequestMessage? capturedRequest = null;

        var client = CreateClient(request =>
        {
            capturedRequest = request;

            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });

        await client.Webhooks.DisableAsync("wh_123");

        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Delete, capturedRequest!.Method);
        Assert.Equal("https://api.sendbyte.africa/v1/webhooks/wh_123", capturedRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task DisableAsync_ThrowsArgumentException_WhenIdIsMissing()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.NoContent));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Webhooks.DisableAsync(""));
    }

    [Fact]
    public async Task ListDeliveriesAsync_SendsGetRequestToDeliveriesEndpoint()
    {
        HttpRequestMessage? capturedRequest = null;

        var client = CreateClient(request =>
        {
            capturedRequest = request;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent("""
                {
                  "data": []
                }
                """)
            };
        });

        await client.Webhooks.ListDeliveriesAsync("wh_123");

        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Get, capturedRequest!.Method);
        Assert.Equal("https://api.sendbyte.africa/v1/webhooks/wh_123/deliveries", capturedRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task ListDeliveriesAsync_ReturnsWebhookDeliveries()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent("""
            {
              "data": [
                {
                  "id": "wd_123",
                  "webhook_id": "wh_123",
                  "event": "email.delivered",
                  "status": "succeeded",
                  "status_code": 200,
                  "payload": {
                    "email_id": "em_123"
                  },
                  "created_at": "2026-06-26T10:00:00Z"
                }
              ]
            }
            """)
        });

        var response = await client.Webhooks.ListDeliveriesAsync("wh_123");

        Assert.Single(response.Data);
        Assert.Equal("wd_123", response.Data[0].Id);
        Assert.Equal("wh_123", response.Data[0].WebhookId);
        Assert.Equal(WebhookEventTypes.EmailDelivered, response.Data[0].Event);
        Assert.Equal("succeeded", response.Data[0].Status);
        Assert.Equal(200, response.Data[0].StatusCode);
        Assert.Equal("em_123", response.Data[0].Payload!["email_id"].GetString());
    }

    [Fact]
    public async Task ListDeliveriesAsync_ThrowsArgumentException_WhenWebhookEndpointIdIsMissing()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Webhooks.ListDeliveriesAsync(""));
    }

    [Fact]
    public async Task ReplayDeliveryAsync_SendsPostRequestToReplayEndpoint()
    {
        HttpRequestMessage? capturedRequest = null;

        var client = CreateClient(request =>
        {
            capturedRequest = request;

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = JsonContent("""
                {
                  "id": "wd_123",
                  "webhook_id": "wh_123",
                  "event": "email.delivered",
                  "status": "pending",
                  "created_at": "2026-06-26T10:00:00Z"
                }
                """)
            };
        });

        await client.Webhooks.ReplayDeliveryAsync("wd_123");

        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        Assert.Equal("https://api.sendbyte.africa/v1/webhooks/deliveries/wd_123/replay", capturedRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task ReplayDeliveryAsync_ReturnsWebhookDelivery_OnCreated()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent("""
            {
              "id": "wd_123",
              "webhook_id": "wh_123",
              "event": "email.delivered",
              "status": "pending",
              "status_code": null,
              "created_at": "2026-06-26T10:00:00Z"
            }
            """)
        });

        var response = await client.Webhooks.ReplayDeliveryAsync("wd_123");

        Assert.Equal("wd_123", response.Id);
        Assert.Equal("pending", response.Status);
        Assert.Null(response.StatusCode);
    }

    [Fact]
    public async Task ReplayDeliveryAsync_ThrowsArgumentException_WhenIdIsMissing()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Webhooks.ReplayDeliveryAsync(""));
    }

    [Fact]
    public async Task CreateAsync_ThrowsSendbyteException_OnApiError()
    {
        var client = CreateClient(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = JsonContent("""
                {
                  "error": {
                    "code": "invalid_webhook_url",
                    "message": "Webhook URL must be an absolute HTTPS URL.",
                    "docs_url": "https://docs.sendbyte.africa/errors/invalid-webhook-url"
                  }
                }
                """)
            };

            response.Headers.Add("request-id", "req_123");

            return response;
        });

        var exception = await Assert.ThrowsAsync<SendbyteException>(() =>
            client.Webhooks.CreateAsync(CreateValidRequest()));

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal("invalid_webhook_url", exception.Code);
        Assert.Equal("Webhook URL must be an absolute HTTPS URL.", exception.Message);
        Assert.Equal("https://docs.sendbyte.africa/errors/invalid-webhook-url", exception.DocsUrl);
        Assert.Equal("req_123", exception.RequestId);
    }

    [Fact]
    public async Task DisableAsync_ThrowsSendbyteException_OnApiError()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = JsonContent("""
            {
              "message": "No webhook with the provided ID exists.",
              "code": "not_found"
            }
            """)
        });

        var exception = await Assert.ThrowsAsync<SendbyteException>(() =>
            client.Webhooks.DisableAsync("wh_missing"));

        Assert.Equal(404, exception.StatusCode);
        Assert.Equal("not_found", exception.Code);
    }

    [Fact]
    public void WebhookEventTypes_ExposeDocumentedConstants()
    {
        Assert.Equal("email.sent", WebhookEventTypes.EmailSent);
        Assert.Equal("email.delivered", WebhookEventTypes.EmailDelivered);
        Assert.Equal("email.opened", WebhookEventTypes.EmailOpened);
        Assert.Equal("email.clicked", WebhookEventTypes.EmailClicked);
        Assert.Equal("email.bounced", WebhookEventTypes.EmailBounced);
        Assert.Equal("email.complained", WebhookEventTypes.EmailComplained);
        Assert.Equal("email.unsubscribed", WebhookEventTypes.EmailUnsubscribed);
        Assert.Equal("domain.verified", WebhookEventTypes.DomainVerified);
        Assert.Equal("domain.degraded", WebhookEventTypes.DomainDegraded);
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

    private static CreateWebhookRequest CreateValidRequest()
    {
        return new CreateWebhookRequest
        {
            Url = "https://example.com/webhooks/sendbyte",
            Events = new[]
            {
                WebhookEventTypes.EmailSent,
                WebhookEventTypes.EmailDelivered
            }
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
