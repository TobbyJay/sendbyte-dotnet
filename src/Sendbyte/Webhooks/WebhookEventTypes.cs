namespace Sendbyte.Webhooks;

/// <summary>
/// Event type constants supported by SendByte webhooks.
/// </summary>
public static class WebhookEventTypes
{
    /// <summary>
    /// Email was accepted for sending.
    /// </summary>
    public const string EmailSent = "email.sent";

    /// <summary>
    /// Email was delivered.
    /// </summary>
    public const string EmailDelivered = "email.delivered";

    /// <summary>
    /// Email was opened.
    /// </summary>
    public const string EmailOpened = "email.opened";

    /// <summary>
    /// Email link was clicked.
    /// </summary>
    public const string EmailClicked = "email.clicked";

    /// <summary>
    /// Email bounced.
    /// </summary>
    public const string EmailBounced = "email.bounced";

    /// <summary>
    /// Email was marked as spam or complained.
    /// </summary>
    public const string EmailComplained = "email.complained";

    /// <summary>
    /// Recipient unsubscribed.
    /// </summary>
    public const string EmailUnsubscribed = "email.unsubscribed";

    /// <summary>
    /// Domain was verified.
    /// </summary>
    public const string DomainVerified = "domain.verified";

    /// <summary>
    /// Domain health degraded.
    /// </summary>
    public const string DomainDegraded = "domain.degraded";
}
