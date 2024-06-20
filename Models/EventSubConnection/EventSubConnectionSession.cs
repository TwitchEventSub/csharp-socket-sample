using System.Text.Json.Serialization;

namespace EventSub.Models.EventSubConnection;

/// <summary>
/// Class has same structure in both welcome and reconnect messages
/// </summary>
public class EventSubConnectionSession {
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("keepalive_timeout_seconds")]
    public int? KeepaliveTimeoutSeconds { get; set; }

    [JsonPropertyName("reconnect_url")]
    public string? ReconnectUrl { get; set; }

    /// <summary>
    /// UTC date and time
    /// </summary>
    [JsonPropertyName("connected_at")]
    public string ConnectedAt { get; set; } = "";
}
