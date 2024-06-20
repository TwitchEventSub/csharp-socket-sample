namespace EventSub.Models.Subscription.Management;

public class EventSubscriptionModel {
    public string ClientId { get; set; } = "";

    public string Barer { get; set; } = "";

    public SubscriptionBody Subscription { get; set; } = new SubscriptionBody();
}
