using System.Text.Json.Serialization;

namespace EventSub.Models;

public class BaseEventSubMessage<T> where T : class {
    [JsonPropertyName("metadata")]
    public BaseEventSubMessageMetadata Metadata { get; set; } = new BaseEventSubMessageMetadata();
    [JsonPropertyName("payload")]
    public T Payload { get; set; } = default!;
}
