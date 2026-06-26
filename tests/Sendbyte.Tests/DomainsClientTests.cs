using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Sendbyte;
using Sendbyte.Domains.Models;
using Sendbyte.Exceptions;
using Xunit;

namespace Sendbyte.Tests;

public class DomainsClientTests
{
    [Fact]
    public async Task CreateAsync_SendsPostRequestToDomainsEndpoint()
    {
        HttpRequestMessage? capturedRequest = null;

        var client = CreateClient(request =>
        {
            capturedRequest = request;

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = JsonContent("""
                {
                  "id": "dom_123",
                  "domain": "paylink.ng",
                  "status": "pending",
                  "dkim_selector": "sb",
                  "dns_records": [],
                  "verified_at": null,
                  "created_at": "2026-06-13T09:00:00.000Z"
                }
                """)
            };
        });

        await client.Domains.CreateAsync(new CreateDomainRequest
        {
            Domain = "paylink.ng"
        });

        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        Assert.Equal("https://api.sendbyte.africa/v1/domains", capturedRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task CreateAsync_SerializesDomain()
    {
        string? body = null;

        var client = CreateClient(async request =>
        {
            body = await request.Content!.ReadAsStringAsync();

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = JsonContent("""
                {
                  "id": "dom_123",
                  "domain": "paylink.ng",
                  "status": "pending",
                  "dkim_selector": "sb",
                  "dns_records": [],
                  "verified_at": null,
                  "created_at": "2026-06-13T09:00:00.000Z"
                }
                """)
            };
        });

        await client.Domains.CreateAsync(new CreateDomainRequest
        {
            Domain = "paylink.ng"
        });

        Assert.Contains("\"domain\":\"paylink.ng\"", body);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDomainResponse_OnCreated()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent("""
            {
              "id": "dom_123",
              "domain": "paylink.ng",
              "status": "pending",
              "dkim_selector": "sb",
              "dns_records": [
                {
                  "type": "TXT",
                  "host": "paylink.ng",
                  "value": "v=spf1 include:spf.sendbyte.africa ~all",
                  "purpose": "spf",
                  "required": true
                }
              ],
              "verified_at": null,
              "created_at": "2026-06-13T09:00:00.000Z"
            }
            """)
        });

        var response = await client.Domains.CreateAsync(new CreateDomainRequest
        {
            Domain = "paylink.ng"
        });

        Assert.Equal("dom_123", response.Id);
        Assert.Equal("paylink.ng", response.Domain);
        Assert.Equal("pending", response.Status);
        Assert.Equal("sb", response.DkimSelector);
        Assert.Single(response.DnsRecords);
        Assert.Equal("spf", response.DnsRecords[0].Purpose);
        Assert.True(response.DnsRecords[0].Required);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDomainResponse_OnOkForExistingDomain()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent("""
            {
              "id": "dom_existing",
              "domain": "paylink.ng",
              "status": "pending",
              "dkim_selector": "sb",
              "dns_records": [],
              "verified_at": null,
              "created_at": "2026-06-13T09:00:00.000Z"
            }
            """)
        });

        var response = await client.Domains.CreateAsync(new CreateDomainRequest
        {
            Domain = "paylink.ng"
        });

        Assert.Equal("dom_existing", response.Id);
    }

    [Fact]
    public async Task CreateAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created));

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.Domains.CreateAsync(null!));
    }

    [Fact]
    public async Task CreateAsync_ThrowsArgumentException_WhenDomainIsMissing()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Domains.CreateAsync(new CreateDomainRequest { Domain = "" }));
    }

    [Fact]
    public async Task CreateAsync_ThrowsArgumentException_WhenDomainContainsScheme()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Domains.CreateAsync(new CreateDomainRequest { Domain = "https://paylink.ng" }));
    }

    [Fact]
    public async Task VerifyAsync_SendsPostRequestToVerifyEndpoint()
    {
        HttpRequestMessage? capturedRequest = null;

        var client = CreateClient(request =>
        {
            capturedRequest = request;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent("""
                {
                  "id": "dom_123",
                  "domain": "paylink.ng",
                  "status": "verified",
                  "checks": []
                }
                """)
            };
        });

        await client.Domains.VerifyAsync("dom_123");

        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        Assert.Equal("https://api.sendbyte.africa/v1/domains/dom_123/verify", capturedRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task VerifyAsync_ReturnsVerifyDomainResponse()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent("""
            {
              "id": "dom_123",
              "domain": "paylink.ng",
              "status": "verified",
              "checks": [
                {
                  "purpose": "spf",
                  "host": "paylink.ng",
                  "required": true,
                  "pass": true
                },
                {
                  "purpose": "dmarc",
                  "host": "_dmarc.paylink.ng",
                  "required": false,
                  "pass": false
                }
              ]
            }
            """)
        });

        var response = await client.Domains.VerifyAsync("dom_123");

        Assert.Equal("dom_123", response.Id);
        Assert.Equal("verified", response.Status);
        Assert.Equal(2, response.Checks.Count);
        Assert.True(response.Checks[0].Pass);
        Assert.False(response.Checks[1].Required);
    }

    [Fact]
    public async Task VerifyAsync_ThrowsArgumentException_WhenIdIsMissing()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Domains.VerifyAsync(""));
    }

    [Fact]
    public async Task VerifyAsync_ThrowsSendbyteException_OnApiError()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = JsonContent("""
            {
              "error": {
                "code": "not_found",
                "message": "No domain with the provided ID exists."
              }
            }
            """)
        });

        var exception = await Assert.ThrowsAsync<SendbyteException>(() =>
            client.Domains.VerifyAsync("dom_missing"));

        Assert.Equal(404, exception.StatusCode);
        Assert.Equal("not_found", exception.Code);
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
