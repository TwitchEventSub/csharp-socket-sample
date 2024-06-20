using System.Text.Json.Serialization;

namespace EventSub.Models.User;

public class UserResponseModel {
    [JsonPropertyName("data")]
    public List<UserDataModel> Data { get; set; } = new List<UserDataModel>();
}
