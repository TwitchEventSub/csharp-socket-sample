using System.Text.Json.Serialization;

namespace EventSub.Models.Validate;

public class ValidationModel {
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = "";

    [JsonPropertyName("scopes")]
    public List<string> ScopeList { get; set; } = new List<string>();

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = "";
}
