using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Sendbyte;
using Sendbyte.Emails.Models;
using Sendbyte.Exceptions;
using Xunit;

namespace Sendbyte.Tests;

public class EmailRetrievalTests
{
    [Fact]
    public async Task GetAsync_SendsGetRequestToEmailByIdEndpoint()
    {
        HttpRequestMessage? capturedRequest = null;

        var client = CreateClient(request =>
        {
            capturedRequest = request;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent("""
                {
                  "id": "em_123",
                  "from": "PayLink <hello@example.com>",
                  "to": ["customer@example.com"],
                  "subject": "Receipt",
                  "status": "delivered",
                  "sandbox": false,
                  "html": "<p>Payment received.</p>",
                  "text": null,
                  "created_at": "2026-06-13T09:14:07.614Z",
                  "events": []
                }
                """)
            };
        });

        await client.Emails.GetAsync("em_123");

        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Get, capturedRequest!.Method);
        Assert.Equal("https://api.sendbyte.africa/v1/emails/em_123", capturedRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task GetAsync_ReturnsEmailDetails()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent("""
            {
              "id": "em_123",
              "from": "PayLink <hello@example.com>",
              "to": ["customer@example.com"],
              "subject": "Receipt",
              "status": "delivered",
              "sandbox": false,
              "html": "<p>Payment received.</p>",
              "text": null,
              "created_at": "2026-06-13T09:14:07.614Z",
              "events": [
                {
                  "type": "email.delivered",
                  "payload": { "smtp_response": "250 2.0.0 OK", "delivered_in_ms": 1180 },
                  "created_at": "2026-06-13T09:14:09.181Z"
                }
              ]
            }
            """)
        });

        var email = await client.Emails.GetAsync("em_123");

        Assert.Equal("em_123", email.Id);
        Assert.Equal("delivered", email.Status);
        Assert.Equal("<p>Payment received.</p>", email.Html);
        Assert.Single(email.Events);
        Assert.Equal("email.delivered", email.Events[0].Type);
        Assert.True(email.Events[0].Payload!.ContainsKey("smtp_response"));
    }

    [Fact]
    public async Task GetAsync_ThrowsArgumentException_WhenIdIsMissing()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Emails.GetAsync(""));
    }

    [Fact]
    public async Task GetAsync_ThrowsSendbyteException_OnNotFound()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = JsonContent("""
            {
              "error": {
                "code": "not_found",
                "message": "No email with the provided ID exists."
              }
            }
            """)
        });

        var exception = await Assert.ThrowsAsync<SendbyteException>(() =>
            client.Emails.GetAsync("em_missing"));

        Assert.Equal(404, exception.StatusCode);
        Assert.Equal("not_found", exception.Code);
    }

    [Fact]
    public async Task ListAsync_SendsGetRequestToEmailsEndpoint()
    {
        HttpRequestMessage? capturedRequest = null;

        var client = CreateClient(request =>
        {
            capturedRequest = request;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent("""
                {
                  "data": [],
                  "has_more": false
                }
                """)
            };
        });

        await client.Emails.ListAsync();

        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Get, capturedRequest!.Method);
        Assert.Equal("https://api.sendbyte.africa/v1/emails", capturedRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task ListAsync_AddsQueryParameters()
    {
        HttpRequestMessage? capturedRequest = null;

        var client = CreateClient(request =>
        {
            capturedRequest = request;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent("""
                {
                  "data": [],
                  "has_more": false
                }
                """)
            };
        });

        await client.Emails.ListAsync(new ListEmailsRequest
        {
            Limit = 20,
            After = "em_123",
            Status = "delivered"
        });

        Assert.NotNull(capturedRequest);
        Assert.Equal(
            "https://api.sendbyte.africa/v1/emails?limit=20&after=em_123&status=delivered",
            capturedRequest!.RequestUri!.ToString());
    }

    [Fact]
    public async Task ListAsync_ReturnsPaginatedEmailSummaries()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent("""
            {
              "data": [
                {
                  "id": "em_123",
                  "from": "PayLink <hello@example.com>",
                  "to": ["customer@example.com"],
                  "subject": "Receipt",
                  "status": "delivered",
                  "sandbox": false,
                  "created_at": "2026-06-13T09:14:07.614Z"
                }
              ],
              "has_more": true
            }
            """)
        });

        var response = await client.Emails.ListAsync(new ListEmailsRequest
        {
            Limit = 20,
            Status = "delivered"
        });

        Assert.True(response.HasMore);
        Assert.Single(response.Data);
        Assert.Equal("em_123", response.Data[0].Id);
        Assert.Equal("delivered", response.Data[0].Status);
    }

    [Fact]
    public async Task ListAsync_ThrowsArgumentOutOfRangeException_WhenLimitIsLessThanOne()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            client.Emails.ListAsync(new ListEmailsRequest { Limit = 0 }));
    }

    [Fact]
    public async Task ListAsync_ThrowsArgumentOutOfRangeException_WhenLimitIsGreaterThanOneHundred()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            client.Emails.ListAsync(new ListEmailsRequest { Limit = 101 }));
    }

    private static SendbyteClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler(handler))
        {
            BaseAddress = new Uri("https://api.sendbyte.africa/v1/")
        };

        return new SendbyteClient(httpClient);
    }

    private static StringContent JsonContent(string json)
    {
        return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }
}
