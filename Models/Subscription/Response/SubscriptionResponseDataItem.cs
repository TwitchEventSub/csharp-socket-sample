using System.Text.Json.Serialization;

namespace EventSub.Models.Subscription.Response;

public class SubscriptionResponseDataItem : SubscriptionBody {
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("cost")]
    public int Cost { get; set; } = 0;

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = "";
}
