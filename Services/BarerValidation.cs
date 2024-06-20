using System.Net.Http.Json;
using EventSub.Models.Validate;

namespace EventSub.Services;

public class BarerValidation {
    public static async Task<ValidationModel?> ValidateAuthAsync(string barer) {
        var httpClient = new HttpClient();
        var request = new HttpRequestMessage() {
            RequestUri = new Uri("https://id.twitch.tv/oauth2/validate"),
            Method = HttpMethod.Get,
        };
        
        request.Headers.Add("Authorization", "Bearer " + barer);

        using HttpResponseMessage response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) {
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<ValidationModel>();
        return result;
    }
}
