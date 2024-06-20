using System.Net.Http.Json;
using EventSub.Models.User;

namespace EventSub.Services;

public class UserFetch {
    public static async Task<UserResponseModel?> FetchUserAsync(string clientId, string accessToken, string userId) {
        return await InternalFetchUserAsync(clientId, accessToken, new List<string> { userId });
    }

    public static async Task<UserResponseModel?> FetchUserAsync(string clientId, string accessToken, IEnumerable<string> userIdEnumerable) {
        if (!userIdEnumerable.Any()) {
            return null;
        }
        
        if (userIdEnumerable.Count() <= 100) {
            return await InternalFetchUserAsync(clientId, accessToken, userIdEnumerable);
        }

        var aggregator = new List<List<string>>() {
            new(),
        };

        foreach (var userId in userIdEnumerable) {
            if (aggregator.Last().Count >= 100) {
                aggregator.Add(new());
            }
            
            aggregator.Last().Add(userId);
        }

        var users = (await Task.WhenAll(aggregator.Select(userIdList => InternalFetchUserAsync(clientId, accessToken, userIdList))))
        .SelectMany(response => response?.Data ?? new List<UserDataModel>())
        .ToList();

        if (!users.Any()) {
            return null;
        }

        var result = new UserResponseModel() {
            Data = users,
        };

        return result;
    }

    private static async Task<UserResponseModel?> InternalFetchUserAsync(string clientId, string accessToken, IEnumerable<string> userIdEnumerable) {
        if (!userIdEnumerable.Any() || userIdEnumerable.Count() > 100) {
            return null;
        }

        var userIds = string.Join("&id=", userIdEnumerable);
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Client-ID", clientId);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        using var response = await client.GetAsync($"https://api.twitch.tv/helix/users?id={userIds}");
        if (!response.IsSuccessStatusCode) {
            return null;
        }

        var responseObj = await response.Content.ReadFromJsonAsync<UserResponseModel>();
        return responseObj;
    }
}
