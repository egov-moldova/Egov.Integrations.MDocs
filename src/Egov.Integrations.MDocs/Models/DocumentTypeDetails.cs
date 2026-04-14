using System.Text.Json.Serialization;

namespace Egov.Integrations.MDocs.Models;

/// <summary>
/// Complete document type metadata.
/// </summary>
public class DocumentTypeDetails
{
    /// <summary>
    /// Romanian title for document type.
    /// </summary>
    [JsonPropertyName("title_Romanian")]
    public required string TitleRomanian { get; set; }

    /// <summary>
    /// English title for document type.
    /// </summary>
    [JsonPropertyName("title_English")]
    public string? TitleEnglish { get; set; }

    /// <summary>
    /// Russian title for document type.
    /// </summary>
    [JsonPropertyName("title_Russian")]
    public string? TitleRussian { get; set; }

    /// <summary>
    /// Icon for document type.
    /// </summary>
    public byte[]? Icon { get; set; }
}
