using System.Text.Json.Serialization;

namespace Egov.Integrations.MDocs.Models.Internal;

internal sealed class Pagination
{
    [JsonPropertyName("TotalCount")]
    public int TotalCount { get; set; }
    [JsonPropertyName("PageSize")]
    public int PageSize { get; set; }
    [JsonPropertyName("CurrentPage")]
    public int CurrentPage { get; set; }
}