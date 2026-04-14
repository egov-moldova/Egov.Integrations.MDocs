using System.Text.Json.Serialization;

namespace Egov.Integrations.MDocs.Models.Internal;

internal class EntityOperation
{
    [JsonPropertyName("path")]
    public required string Path { get; set; }

    [JsonPropertyName("op")]
    public required string Operation { get; set; }

    [JsonPropertyName("value")]
    public object? Value { get; set; }
}
