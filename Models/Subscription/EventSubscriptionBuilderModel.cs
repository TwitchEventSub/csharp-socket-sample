namespace EventSub.Models.Subscription;

public class EventSubscriptionBuilderModel {
    public Dictionary<string, string?> UserToUserIdMap { get; set; } = new Dictionary<string, string?>();

    public SubscriptionBody Subscription { get; set; } = new SubscriptionBody();

    public bool RequiresBarerUserId { get; set; } = false;

    public Func<SubscriptionBody, Dictionary<string, string?>, bool>? Build { get; set; }

    public bool Keep { get; set; } = true;
}
