using System.Net.Mime;

namespace Egov.Integrations.MDocs;

/// <summary>
/// Represents known content types supported by MDocs.
/// </summary>
public static class MDocsContentType
{
    /// <summary>
    /// PDF content type.
    /// </summary>
    public const string Pdf = MediaTypeNames.Application.Pdf;

    /// <summary>
    /// JSON content type.
    /// </summary>
    public const string Json = MediaTypeNames.Application.Json;
}
