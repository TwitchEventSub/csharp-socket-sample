using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using EventSub.Models;

namespace EventSub.Services;

class EventSubSocket {
    public Func<BaseEventSubMessage<EventSubConnectionMessage>, Task>? OnWelcome { get; set; }

    public Func<string, Task>? OnNotification { get; set; }

    public Func<string, Task>? OnRevocation { get; set; }

    private Uri _uri = new("wss://eventsub.wss.twitch.tv/ws");

    private ClientWebSocket? _client = null;

    private IdPersistance? _idPersistance = null;

    private CancellationToken _token = CancellationToken.None;

    private Timer? _timer = null;

    private TimeSpan _waitTime = TimeSpan.FromSeconds(30);

    public async Task ConnectAsync() {
        _idPersistance = new IdPersistance(TimeSpan.FromSeconds(60));

        await InnerConnectAsync();
    }

    public async Task CloseAsync() {
        _timer?.Dispose();
        _timer = null;
        _idPersistance = null;

        if (_client == null) {
            return;
        }

        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", default);
    }

    private async Task InnerConnectAsync() {
        if (_client != null) {
            await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Reconnecting", default);
        }
        
        _client = new ClientWebSocket();
        _token = new CancellationToken();
        await _client.ConnectAsync(_uri, _token);
        await RecieveDataAsync();
    }

    private async Task RecieveDataAsync() {
        if (_client == null) {
            return;
        }

        var message = "";
        var bytes = new byte[1024];
        do {
            var result = await _client.ReceiveAsync(bytes, default);
            message += Encoding.UTF8.GetString(bytes, 0, result.Count);

            if (result.EndOfMessage && message.Length > 0) {
                await HandleMessageAsync(message + "");
                message = "";
            }
        } while (_client.State == WebSocketState.Open);
    }

        private async Task HandleMessageAsync(string message) {
        if (IsPingMessage(message)) {
            await SendPong();
            return;
        }

        var messageData = JsonSerializer.Deserialize<BaseEventSubMessage<dynamic>>(message)
        ?? throw new Exception("Failed to deserialize message");

        if (_idPersistance == null) {
            throw new Exception("Id persistance is null");
        }

        if (_idPersistance.Contains(messageData.Metadata.MessageId)) {
            return;
        }

        _idPersistance.Add(messageData.Metadata.MessageId);

        switch (messageData.Metadata.MessageType) {
            case "session_welcome":
                SetTimer();
                var sessionWelcome = JsonSerializer
                .Deserialize<BaseEventSubMessage<EventSubConnectionMessage>>(message)
                ?? throw new Exception("Failed to deserialize session welcome message");
                
                if (sessionWelcome.Payload.Session.KeepaliveTimeoutSeconds == null) {
                    throw new Exception("Session is null in session welcome message");
                }

                _waitTime = TimeSpan.FromSeconds(sessionWelcome.Payload.Session.KeepaliveTimeoutSeconds.Value + 1);
                
                if (OnWelcome is not null) {
                    await OnWelcome.Invoke(sessionWelcome);
                }
                break;
            case "session_keepalive":
                SetTimer();
                break;
            case "notification":
                SetTimer();
                if (OnNotification is not null) {
                    await OnNotification.Invoke(message);
                }
                break;
            case "revocation":
                if (OnRevocation is not null) {
                    await OnRevocation.Invoke(message);
                }
                break;
            case "session_reconnect":
                await ReconnectAsync(message);
                break;
            default:
                return;
        }
    }

    private async Task ReconnectAsync(string message) {
        var sessionReconnect = JsonSerializer
        .Deserialize<BaseEventSubMessage<EventSubConnectionMessage>>(message)
        ?? throw new Exception("Failed to deserialize session reconnect message");
        
        if (sessionReconnect.Payload.Session.ReconnectUrl == null) {
            throw new Exception("Reconnect url is null in reconnect message");
        }

        _uri = new Uri(sessionReconnect.Payload.Session.ReconnectUrl);

        await InnerConnectAsync();
    }

    private static bool IsPingMessage(string message) {
        try {
            var isPing = JsonSerializer.Deserialize<Dictionary<string, object>>(message)
            ?.TryGetValue("type", out var type) == true 
            && type.ToString() == "PING";

            return isPing;
        } catch {
            return false;
        }
    }

    private async Task SendPong() {
        if (_client == null) {
            return;
        }

        var pong = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new PingMessage()));

        await _client.SendAsync(pong, WebSocketMessageType.Text, true, default);
    }

    private void SetTimer() {
        _timer?.Dispose();
        _timer = new Timer(
            async (s) => {
                await InnerConnectAsync();
            },
            null,
            _waitTime,
            Timeout.InfiniteTimeSpan
        );
    }
}
