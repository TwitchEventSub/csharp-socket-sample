using EventSub.Models;
using EventSub.Models.Subscription;

namespace EventSub.Services;

public class EventSubManager {
    /// <summary>
    /// passes notification event JSON string to the handler
    /// </summary>
    public Func<string, Task>? NotificationHandler { get; set; }

    /// <summary>
    /// passes revocation event JSON string to the handler
    /// </summary>
    public Func<string, Task>? RevocationHandler { get; set; }


    private readonly string _bearer = "";

    private EventSubSocket _twitchWs = new();

    private string _clientId = "";

    private string _bearerUserId = "";

    private EventSubscriptionBuilder _builder = new();

    public EventSubscriptionBuilder Builder => _builder;


    public EventSubManager(string bearer) {
        if (string.IsNullOrEmpty(bearer)) {
            throw new ArgumentException(nameof(bearer));
        }

        _bearer = bearer;
        _twitchWs.OnWelcome = HandleSessionWelcomeAsync;
        _twitchWs.OnNotification = OnNotificationAsync;
        _twitchWs.OnRevocation = OnRevocationAsync;
    }

    /// <summary>
    /// Intializes with handler
    /// </summary>
    /// <param name="handler">passes notification event JSON string to the handler</param>
    public EventSubManager(string bearer, Func<string, Task> notificationNandler) : this(bearer) {
        NotificationHandler = notificationNandler ?? throw new ArgumentNullException(nameof(notificationNandler));
    }

    public async Task ConnectAsync() {
        await ValidateAsync();
        await _twitchWs.ConnectAsync();
    }

    public async Task CloseAsync() {
        await _twitchWs.CloseAsync();
        _builder.Clear();
    }
    private async Task OnNotificationAsync(string message) {
        if (NotificationHandler == null) {
            return;
        }
        await NotificationHandler.Invoke(message);
    }

    private async Task OnRevocationAsync(string message) {
        if (RevocationHandler == null) {
            return;
        }
        await RevocationHandler.Invoke(message);
    }

    private async Task HandleSessionWelcomeAsync(BaseEventSubMessage<EventSubConnectionMessage> message) {
        if (message.Payload.Session.KeepaliveTimeoutSeconds == null) {
            throw new Exception("Session is null in session welcome message");
        }

        await SubscribeAsync(message.Payload.Session.Id);
    }

    private async Task ValidateAsync() {
        var client = await BarerValidation.ValidateAuthAsync(_bearer)
        ?? throw new Exception("Failed to validate bearer");

        _clientId = client.ClientId;
        _bearerUserId = client.UserId;
    }

    private async Task SubscribeAsync(string sessionId) {
        var subscriptions = await _builder.BuildAsync(_bearer, _clientId, _bearerUserId);

        foreach (var subscription in subscriptions) {
            subscription.Transport = new SubscriptionTransport(sessionId);
        }

        await Task.WhenAll(subscriptions.Select(async x => {
            await EventSubscription.SubscribeAsync(new() {
                Barer = _bearer,
                ClientId = _clientId,
                Subscription = x,
            });
        }));

        var failedList = subscriptions.Where(x => string.IsNullOrEmpty(x.Id)).ToList();
    }
}
