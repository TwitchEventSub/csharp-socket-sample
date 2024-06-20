using System.Text.Json.Serialization;

namespace EventSub.Models;

public class BaseEventSubMessageMetadata {
    /// <summary>
    /// An ID that uniquely identifies the message. Twitch sends messages at least once, but if Twitch is unsure of whether you received a notification, it’ll resend the message. This means you may receive a notification twice. If Twitch resends the message, the message ID will be the same.
    /// </summary>
    [JsonPropertyName("message_id")]
    public string MessageId { get; set; } = "";
    /// <summary>
    /// The type of message
    /// </summary>
    [JsonPropertyName("message_type")]
    public string MessageType { get; set; } = "";
    /// <summary>
    /// The UTC date and time that the message was sent.
    /// </summary>
    [JsonPropertyName("message_timestamp")]
    public string MessageTimestamp { get; set; } = "";
}
