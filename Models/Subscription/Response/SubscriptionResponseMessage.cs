using System.Text.Json.Serialization;

namespace EventSub.Models.Subscription.Response;

public class SubscriptionResponseMessage {
    [JsonPropertyName("data")]
    public List<SubscriptionResponseDataItem> Data { get; set; } = new List<SubscriptionResponseDataItem>();

    [JsonPropertyName("total")]
    public int Total { get; set; } = 0;
    
    [JsonPropertyName("total_cost")]
    public int TotalCost { get; set; } = 0;

    [JsonPropertyName("max_total_cost")]
    public int MaxTotalCost { get; set; } = 0;
}
