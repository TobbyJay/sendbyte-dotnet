using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Sendbyte.Webhooks;

/// <summary>
/// Verifies SendByte webhook signatures.
/// </summary>
public static class SendbyteWebhookVerifier
{
    /// <summary>
    /// SendByte webhook signature header name.
    /// </summary>
    public const string SignatureHeader = "sendbyte-signature";

    private const int DefaultToleranceSeconds = 300;

    /// <summary>
    /// Verifies a SendByte webhook signature using the default 300-second tolerance.
    /// </summary>
    public static bool VerifySignature(
        string secret,
        string? signatureHeader,
        string rawBody)
    {
        if (rawBody is null)
        {
            return false;
        }

        return VerifySignature(
            secret,
            signatureHeader,
            Encoding.UTF8.GetBytes(rawBody),
            TimeSpan.FromSeconds(DefaultToleranceSeconds));
    }

    /// <summary>
    /// Verifies a SendByte webhook signature using the default 300-second tolerance.
    /// </summary>
    public static bool VerifySignature(
        string secret,
        string? signatureHeader,
        byte[] rawBody)
    {
        return VerifySignature(
            secret,
            signatureHeader,
            rawBody,
            TimeSpan.FromSeconds(DefaultToleranceSeconds));
    }

    /// <summary>
    /// Verifies a SendByte webhook signature using a custom tolerance.
    /// </summary>
    public static bool VerifySignature(
        string secret,
        string? signatureHeader,
        string rawBody,
        TimeSpan tolerance)
    {
        if (rawBody is null)
        {
            return false;
        }

        return VerifySignature(
            secret,
            signatureHeader,
            Encoding.UTF8.GetBytes(rawBody),
            tolerance);
    }

    /// <summary>
    /// Verifies a SendByte webhook signature using a custom tolerance.
    /// </summary>
    public static bool VerifySignature(
        string secret,
        string? signatureHeader,
        byte[] rawBody,
        TimeSpan tolerance)
    {
        if (string.IsNullOrWhiteSpace(secret)
            || string.IsNullOrWhiteSpace(signatureHeader)
            || rawBody is null)
        {
            return false;
        }

        if (!TryParseSignatureHeader(signatureHeader, out var timestamp, out var receivedSignature))
        {
            return false;
        }

        if (IsTimestampOutsideTolerance(timestamp, tolerance))
        {
            return false;
        }

        var signedPayload = BuildSignedPayload(timestamp, rawBody);
        var expectedSignature = ComputeHmacSha256Hex(secret, signedPayload);

        return FixedTimeEqualsHex(expectedSignature, receivedSignature);
    }

    private static bool TryParseSignatureHeader(
        string signatureHeader,
        out long timestamp,
        out string signature)
    {
        timestamp = 0;
        signature = string.Empty;

        var parts = signatureHeader
            .Split(',')
            .Select(part => part.Trim())
            .ToArray();

        string? timestampPart = null;
        string? signaturePart = null;

        foreach (var part in parts)
        {
            if (part.StartsWith("t=", StringComparison.Ordinal))
            {
                timestampPart = part.Substring(2);
            }

            if (part.StartsWith("v1=", StringComparison.Ordinal))
            {
                signaturePart = part.Substring(3);
            }
        }

        if (string.IsNullOrWhiteSpace(timestampPart)
            || string.IsNullOrWhiteSpace(signaturePart))
        {
            return false;
        }

        if (!long.TryParse(
            timestampPart,
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out timestamp))
        {
            return false;
        }

        if (!IsHex(signaturePart))
        {
            return false;
        }

        signature = signaturePart;
        return true;
    }

    private static bool IsTimestampOutsideTolerance(long timestamp, TimeSpan tolerance)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var age = Math.Abs(now - timestamp);

        return age > tolerance.TotalSeconds;
    }

    private static byte[] BuildSignedPayload(long timestamp, byte[] rawBody)
    {
        var prefix = Encoding.UTF8.GetBytes(
            timestamp.ToString(CultureInfo.InvariantCulture) + ".");

        var signedPayload = new byte[prefix.Length + rawBody.Length];

        Buffer.BlockCopy(prefix, 0, signedPayload, 0, prefix.Length);
        Buffer.BlockCopy(rawBody, 0, signedPayload, prefix.Length, rawBody.Length);

        return signedPayload;
    }

    private static string ComputeHmacSha256Hex(string secret, byte[] payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(payload);

        var builder = new StringBuilder(hash.Length * 2);

        foreach (var b in hash)
        {
            builder.Append(b.ToString("x2", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }

    private static bool FixedTimeEqualsHex(string expectedHex, string receivedHex)
    {
        if (expectedHex.Length != receivedHex.Length)
        {
            return false;
        }

        var expectedBytes = HexToBytes(expectedHex);
        var receivedBytes = HexToBytes(receivedHex);

#if NETSTANDARD2_0
        var difference = 0;

        for (var i = 0; i < expectedBytes.Length; i++)
        {
            difference |= expectedBytes[i] ^ receivedBytes[i];
        }

        return difference == 0;
#else
        return CryptographicOperations.FixedTimeEquals(expectedBytes, receivedBytes);
#endif
    }

    private static bool IsHex(string value)
    {
        if (value.Length == 0 || value.Length % 2 != 0)
        {
            return false;
        }

        foreach (var character in value)
        {
            var isHex =
                character >= '0' && character <= '9'
                || character >= 'a' && character <= 'f'
                || character >= 'A' && character <= 'F';

            if (!isHex)
            {
                return false;
            }
        }

        return true;
    }

    private static byte[] HexToBytes(string hex)
    {
        var bytes = new byte[hex.Length / 2];

        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = byte.Parse(
                hex.Substring(i * 2, 2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture);
        }

        return bytes;
    }
}
