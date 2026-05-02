namespace Tiki.Documents;

/// <summary>
/// Represents parsed metadata for email messages (EML, MSG).
/// </summary>
public class TikiMessage : TikiFile
{
    /// <summary>
    /// The sender's email address.
    /// </summary>
    public string? FromAddress { get; init; }

    /// <summary>
    /// The sender's display name.
    /// </summary>
    public string? FromName { get; init; }

    /// <summary>
    /// The To recipients' email addresses.
    /// </summary>
    public string[]? ToAddresses { get; init; }

    /// <summary>
    /// The To recipients' display names.
    /// </summary>
    public string[]? ToNames { get; init; }

    /// <summary>
    /// The CC recipients' email addresses.
    /// </summary>
    public string[]? CcAddresses { get; init; }

    /// <summary>
    /// The BCC recipients' email addresses.
    /// </summary>
    public string[]? BccAddresses { get; init; }

    /// <summary>
    /// The email subject line.
    /// </summary>
    public string? Subject { get; init; }

    /// <summary>
    /// The date and time the message was sent.
    /// </summary>
    public DateTime? DateSent { get; init; }

    /// <summary>
    /// The date and time the message was received.
    /// </summary>
    public DateTime? DateReceived { get; init; }

    /// <summary>
    /// The names of attached files.
    /// </summary>
    public string[]? AttachmentNames { get; init; }

    /// <summary>
    /// The conversation/thread identifier.
    /// </summary>
    public string? ConversationId { get; init; }
}
