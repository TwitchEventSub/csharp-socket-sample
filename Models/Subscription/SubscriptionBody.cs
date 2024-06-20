using System.Text.Json.Serialization;

namespace EventSub.Models.Subscription;

public class SubscriptionBody {
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "";

    [JsonPropertyName("condition")]
    public Dictionary<string, string> Condition { get; set; } = new Dictionary<string, string>();

    [JsonPropertyName("transport")]
    public SubscriptionTransport Transport { get; set; } = new SubscriptionTransport();

    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonIgnore]
    public bool IsSubscribed => !string.IsNullOrEmpty(Id);
}
