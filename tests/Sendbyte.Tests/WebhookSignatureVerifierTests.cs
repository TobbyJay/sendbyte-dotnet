using System;
using System.Security.Cryptography;
using System.Text;
using Sendbyte.Webhooks;
using Xunit;

namespace Sendbyte.Tests;

public class WebhookSignatureVerifierTests
{
    [Fact]
    public void SignatureHeader_IsSendbyteSignature()
    {
        Assert.Equal("sendbyte-signature", SendbyteWebhookVerifier.SignatureHeader);
    }

    [Fact]
    public void VerifySignature_ReturnsTrue_ForValidSignature()
    {
        var secret = "whsec_test";
        var rawBody = "{\"type\":\"email.delivered\",\"data\":{\"email_id\":\"em_123\"}}";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var signature = CreateSignature(secret, timestamp, rawBody);
        var header = $"t={timestamp},v1={signature}";

        var isValid = SendbyteWebhookVerifier.VerifySignature(secret, header, rawBody);

        Assert.True(isValid);
    }

    [Fact]
    public void VerifySignature_ReturnsTrue_ForValidByteArrayPayload()
    {
        var secret = "whsec_test";
        var rawBody = Encoding.UTF8.GetBytes("{\"type\":\"email.delivered\"}");
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var signature = CreateSignature(secret, timestamp, rawBody);
        var header = $"t={timestamp},v1={signature}";

        var isValid = SendbyteWebhookVerifier.VerifySignature(secret, header, rawBody);

        Assert.True(isValid);
    }

    [Fact]
    public void VerifySignature_ReturnsFalse_WhenSecretIsMissing()
    {
        var isValid = SendbyteWebhookVerifier.VerifySignature(
            "",
            "t=123,v1=abcdef",
            "{}");

        Assert.False(isValid);
    }

    [Fact]
    public void VerifySignature_ReturnsFalse_WhenHeaderIsMissing()
    {
        var isValid = SendbyteWebhookVerifier.VerifySignature(
            "whsec_test",
            null,
            "{}");

        Assert.False(isValid);
    }

    [Fact]
    public void VerifySignature_ReturnsFalse_WhenHeaderIsMalformed()
    {
        var isValid = SendbyteWebhookVerifier.VerifySignature(
            "whsec_test",
            "invalid-header",
            "{}");

        Assert.False(isValid);
    }

    [Fact]
    public void VerifySignature_ReturnsFalse_WhenTimestampIsMissing()
    {
        var isValid = SendbyteWebhookVerifier.VerifySignature(
            "whsec_test",
            "v1=abcdef",
            "{}");

        Assert.False(isValid);
    }

    [Fact]
    public void VerifySignature_ReturnsFalse_WhenSignatureIsMissing()
    {
        var isValid = SendbyteWebhookVerifier.VerifySignature(
            "whsec_test",
            "t=123",
            "{}");

        Assert.False(isValid);
    }

    [Fact]
    public void VerifySignature_ReturnsFalse_WhenPayloadIsTampered()
    {
        var secret = "whsec_test";
        var originalBody = "{\"type\":\"email.delivered\"}";
        var tamperedBody = "{\"type\":\"email.bounced\"}";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var signature = CreateSignature(secret, timestamp, originalBody);
        var header = $"t={timestamp},v1={signature}";

        var isValid = SendbyteWebhookVerifier.VerifySignature(secret, header, tamperedBody);

        Assert.False(isValid);
    }

    [Fact]
    public void VerifySignature_ReturnsFalse_WhenSignatureIsStale()
    {
        var secret = "whsec_test";
        var rawBody = "{\"type\":\"email.delivered\"}";
        var timestamp = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeSeconds();
        var signature = CreateSignature(secret, timestamp, rawBody);
        var header = $"t={timestamp},v1={signature}";

        var isValid = SendbyteWebhookVerifier.VerifySignature(
            secret,
            header,
            rawBody,
            TimeSpan.FromMinutes(5));

        Assert.False(isValid);
    }

    [Fact]
    public void VerifySignature_ReturnsFalse_WhenSignatureIsNotHex()
    {
        var isValid = SendbyteWebhookVerifier.VerifySignature(
            "whsec_test",
            "t=123,v1=not-hex",
            "{}");

        Assert.False(isValid);
    }

    private static string CreateSignature(string secret, long timestamp, string rawBody)
    {
        return CreateSignature(secret, timestamp, Encoding.UTF8.GetBytes(rawBody));
    }

    private static string CreateSignature(string secret, long timestamp, byte[] rawBody)
    {
        var prefix = Encoding.UTF8.GetBytes(timestamp + ".");
        var payload = new byte[prefix.Length + rawBody.Length];

        Buffer.BlockCopy(prefix, 0, payload, 0, prefix.Length);
        Buffer.BlockCopy(rawBody, 0, payload, prefix.Length, rawBody.Length);

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(payload);

        var builder = new StringBuilder(hash.Length * 2);

        foreach (var b in hash)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }
}
