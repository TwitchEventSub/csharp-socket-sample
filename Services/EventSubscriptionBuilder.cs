using EventSub.Models.Subscription;
using EventSub.Models.User;

namespace EventSub.Services;

public class EventSubscriptionBuilder
{
    public static readonly string BarerUserKey = "~barer";
    
    private bool Initialized = false;

    private List<EventSubscriptionBuilderModel> _model = new();

    public EventSubscriptionBuilder Add() {
        if (Initialized) {
            throw new Exception("Cannot add after initialization");
        }
        if (_model.Count > 0 && _model.Last().Build == null) {
            throw new Exception("All subscriptions must have a build function");
        }

        _model.Add(new EventSubscriptionBuilderModel());
        return this;
    }

    /// <summary>
    /// Adds BarerUserKey.EventSubscriptionBuilder to the user map
    /// </summary>
    public EventSubscriptionBuilder WithBarerUserId() {
        if (!_model.Any()) {
            Add();
        }

        var item = _model.Last();
        item.RequiresBarerUserId = true;
        return this;
    }

    public EventSubscriptionBuilder Subscription(Func<SubscriptionBody, Dictionary<string, string?>, bool> build) {
        if (!_model.Any()) {
            Add();
        }

        var item = _model.Last();
        item.Build = build;
        return this;
    }

    /// <summary>
    /// Adds a user to user id map to the subscription
    /// </summary>
    /// <param name="userToUserIdMap">A list of users to map to user ids, EventSubscriptionBuilder.BarerUserKey is reserved for barer UserId</param>
    public EventSubscriptionBuilder WithUsers(IEnumerable<string> userToUserIdMap) {
        var distinctUsers = userToUserIdMap.Distinct()
        .Select(x => x.ToLower());

        if (distinctUsers.Contains(BarerUserKey)) {
            throw new Exception("Reserved key used in user map");
        }

        if (!_model.Any()) {
            Add();
        }

        var item = _model.Last();

        item.UserToUserIdMap = distinctUsers.ToDictionary(x => x, x => (string?)null);

        return this;
    }

    public async Task<List<SubscriptionBody>> BuildAsync(string bearer, string clientId, string userId) {
        if (bearer == null) {
            throw new ArgumentNullException(nameof(bearer));
        }
        if (clientId == null) {
            throw new ArgumentNullException(nameof(clientId));
        }
        if (userId == null) {
            throw new ArgumentNullException(nameof(userId));
        }

        if (!Initialized) {
            Initialized = true;
            await FillUsersAsync(bearer, clientId);
            InvokeBuilders(userId);
        }

        var subscriptionList = GetSubscriptionList()!;

        return subscriptionList;
    }

    public void Clear() {
        foreach (var item in _model) {
            item.Keep = false;
            item.Subscription.Id = "";
            item.Subscription.Condition = new();
        }
    }

    public List<SubscriptionBody>? GetSubscriptionList() {
        if (!Initialized) {
            return null;
        }

        var subscriptionList = _model.Where(x => x.Keep)
        .Select(x => x.Subscription)
        .ToList();
        return subscriptionList;
    }

    private void InvokeBuilders(string userId) {
        if (_model.FindAll(x => x.Build == null).Any()) {
            throw new Exception("All subscriptions must have a build function");
        }

        foreach (var item in _model) {
            // cloning the map to avoid modifying the original
            var userToUserIdMap = item.UserToUserIdMap.ToDictionary(x => x.Key, x => x.Value);
            if (item.RequiresBarerUserId) {
                userToUserIdMap[BarerUserKey] = userId;
            }
            item.Keep = item.Build!.Invoke(item.Subscription, userToUserIdMap);
        }
    }

    private async Task FillUsersAsync(string bearer, string clientId) {
        var users = _model.SelectMany(x => x.UserToUserIdMap.Keys)
        .Distinct()
        .ToList();
        
        if (!users.Any()) {
            return;
        }
        
        var userFetch = await UserFetch.FetchUserAsync(clientId, bearer, users)
        ?? throw new Exception("Failed to fetch users");

        var fetchedMap = userFetch.Data.ToDictionary(x => x.Login, x => x.Id);

        foreach (var item in _model) {
            foreach (var user in item.UserToUserIdMap) {
                item.UserToUserIdMap[user.Key] = fetchedMap.TryGetValue(user.Key, out var id)
                ? id
                : null;
            }
        }
    }
}
