namespace Egov.Integrations.MDocs.Models;

/// <summary>
/// Represents information about a recycled document.
/// </summary>
public class RecycledDocumentDetails : DocumentDetails
{
    /// <summary>
    /// The date when the document was deleted.
    /// </summary>
    public DateTime DeletedOn { get; set; }

    /// <summary>
    /// The principal that deleted the document.
    /// </summary>
    public required Uri DeletedBy { get; set; }

    /// <summary>
    /// The name of the principal that deleted the document.
    /// </summary>
    public string? DeletedByName { get; set; }

    /// <summary>
    /// The original location of the document.
    /// </summary>
    public required string DeletedFrom { get; set; }
}
