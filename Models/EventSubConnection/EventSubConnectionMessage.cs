using System.Text.Json.Serialization;
using EventSub.Models.EventSubConnection;

namespace EventSub.Models;

public class EventSubConnectionMessage {
    [JsonPropertyName("session")]
    public EventSubConnectionSession Session { get; set; } = new EventSubConnectionSession();
}
