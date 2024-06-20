using System.Text.Json.Serialization;

namespace EventSub.Models;
public class PingMessage {
    [JsonPropertyName("type")]
    public string Type { get; set; } = "PONG";
}
