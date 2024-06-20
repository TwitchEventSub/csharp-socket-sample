using System.Text.Json.Serialization;

namespace EventSub.Models.Subscription;

public class SubscriptionTransport {
    [JsonPropertyName("method")]
    public string Method { get; private set; } = "websocket";

    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = "";

    public SubscriptionTransport(string sessionId)
    {
        SessionId = sessionId;
    }

    public SubscriptionTransport()
    {
    }
}
