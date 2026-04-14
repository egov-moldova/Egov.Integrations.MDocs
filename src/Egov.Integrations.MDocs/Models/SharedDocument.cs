namespace Egov.Integrations.MDocs.Models;

/// <summary>
/// Shared document metadata.
/// </summary>
public class SharedDocument
{
    /// <summary>
    /// Is the id of the document
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Is the id of the folder where this document is located, null if root
    /// </summary>
    public Guid? FolderId { get; set; }

    /// <summary>
    /// Name of the Document
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Document number assigned by issuer
    /// </summary>
    public string? Number { get; set; }

    /// <summary>
    /// This is the code associated with the document type
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Size of the document blob.
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// The date the document was created.
    /// </summary>
    public required DateTime CreatedOn { get; set; }

    /// <summary>
    /// Identity that created initial document if specified
    /// </summary>
    public Uri? CreatedBy { get; set; }

    /// <summary>
    /// The name of the identity that created the document if specified
    /// </summary>
    public string? CreatedByName { get; set; }

    /// <summary>
    /// The date the document was modified if specified
    /// </summary>
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Identity that created modified document if specified
    /// </summary>
    public Uri? ModifiedBy { get; set; }

    /// <summary>
    /// The name of the identity that modified the document if specified
    /// </summary>
    public string? ModifiedByName { get; set; }

    /// <summary>
    /// Shares of the document.
    /// </summary>
    public IList<SharedDocumentShare> Shares { get; set; } = new List<SharedDocumentShare>();
}

