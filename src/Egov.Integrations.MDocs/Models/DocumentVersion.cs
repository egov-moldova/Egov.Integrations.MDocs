namespace Egov.Integrations.MDocs.Models;

/// <summary>
/// Document version details.
/// </summary>
public class DocumentVersion
{
    /// <summary>
    /// Version number. Higher is newer.
    /// </summary>
    public required short Version { get; set; }

    /// <summary>
    /// Size of the blob.
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// The date the document version was modified, if specified.
    /// </summary>
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Identity that modified document version, if specified.
    /// </summary>
    public Uri? ModifiedBy { get; set; }

    /// <summary>
    /// The name of the identity that modified the document version, if specified.
    /// </summary>
    public string? ModifiedByName { get; set; }
}