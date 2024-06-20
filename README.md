# C# Eventsub socket implementation sample

usage:
```c#
    // ...
    var manager = new EventSubManager("your barer key");

    manager.Builder.WithBarerUserId()
    .Subscription((sub, names) => {
        if (!names.TryGetValue(EventSubscriptionBuilder.BarerUserKey, out var userId) || string.IsNullOrEmpty(userId)) {
            return false;
        }

        sub.Type = "channel.follow";
        sub.Version = "2";
        sub.Condition.Add("broadcaster_user_id", userId);
        sub.Condition.Add("moderator_user_id", userId);
        return true;
    })
    .Add()
    .WithUsers(new List<string> { "foo" })
    .Subscription((sub, names) => {
        if (!names.TryGetValue("foo", out var fooUser) || string.IsNullOrEmpty(fooUser)) {
            return false;
        }

        sub.Type = "channel.raid";
        sub.Version = "1";
        sub.Condition.Add("from_broadcaster_user_id", fooUser);
        return true;
    });

    manager.NotificationHandler = (message) => {
        Console.WriteLine(message);
        return Task.CompletedTask;
    };

    Task.Run(async () => await manager.ConnectAsync()).Wait();
```
