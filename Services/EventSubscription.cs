using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using EventSub.Models.Subscription.Management;
using EventSub.Models.Subscription.Response;

namespace EventSub.Services;

class EventSubscription {
    public static async Task<bool> SubscribeAsync(EventSubscriptionModel subscriptionModel) {
        using var httpClient = new HttpClient();

        var request = CreateRequest(subscriptionModel);
        using HttpResponseMessage response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) {
            // add your logging here (response usually includes the error message in json)
            return false;
        }

        var data = await response.Content.ReadFromJsonAsync<SubscriptionResponseMessage>()
        ?? throw new Exception("Failed to parse event subscription response");

        subscriptionModel.Subscription.Id = data?.Data.FirstOrDefault()?.Id ?? "";
        
        return true;
    }

    public static async Task<bool> DeleteSubscriptionAsync(EventSubscriptionModel subscriptionModel) {
        using var httpClient = new HttpClient();

        var request = CreateDeleteRequest(subscriptionModel);
        using HttpResponseMessage response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode) {
            // add your logging here (response usually includes the error message in json)
            return false;
        }

        return true;
    }

    public static async Task<bool> UnsubscribeAsync(string id, string barer) {
        var httpClient = new HttpClient();
        var request = new HttpRequestMessage() {
            RequestUri = new Uri("https://api.twitch.tv/helix/eventsub/subscriptions?id=" + id),
            Method = HttpMethod.Delete,
        };
        
        request.Headers.Add("Authorization", "Bearer " + barer);
        request.Headers.Add("Client-Id", "your-client-id");

        using HttpResponseMessage response = await httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    private static HttpRequestMessage CreateRequest(EventSubscriptionModel subscriptionModel) {
        var body = JsonSerializer.Serialize(subscriptionModel.Subscription);
        var request = new HttpRequestMessage() {
            RequestUri = new Uri("https://api.twitch.tv/helix/eventsub/subscriptions"),
            Method = HttpMethod.Post,
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };


        request.Headers.Add("Client-ID", subscriptionModel.ClientId);
        request.Headers.Add("Authorization", "Bearer " + subscriptionModel.Barer);

        return request;
    }

    private static HttpRequestMessage CreateDeleteRequest(EventSubscriptionModel subscriptionModel) {
        var request = new HttpRequestMessage() {
            RequestUri = new Uri($"https://api.twitch.tv/helix/eventsub/subscriptions?id={subscriptionModel.Subscription.Id}"),
            Method = HttpMethod.Delete,
        };

        request.Headers.Add("Client-ID", subscriptionModel.ClientId);
        request.Headers.Add("Authorization", "Bearer " + subscriptionModel.Barer);

        return request;
    }
}
