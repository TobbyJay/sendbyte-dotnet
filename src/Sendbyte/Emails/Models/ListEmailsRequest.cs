namespace Sendbyte.Emails.Models;

/// <summary>
/// Query options for listing SendByte emails.
/// </summary>
public sealed class ListEmailsRequest
{
    /// <summary>
    /// Maximum number of emails to return. SendByte accepts 1 to 100.
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// Cursor for pagination. Use the last email ID from the previous page.
    /// </summary>
    public string? After { get; set; }

    /// <summary>
    /// Optional status filter.
    /// </summary>
    public string? Status { get; set; }
}
