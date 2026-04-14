namespace Egov.Integrations.MDocs.Models;

/// <summary>
/// Complete sharing information.
/// </summary>
public class ShareDetails
{
    /// <summary>
    /// Id of the document share.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Id of the shared document.
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Is the id of the folder where this document is located, null if root.
    /// </summary>
    public Guid? FolderId { get; set; }

    /// <summary>
    /// Name of the Document.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Document number assigned by issuer.
    /// </summary>
    public required string Number { get; set; }

    /// <summary>
    /// Assigned share permission.
    /// </summary>
    public Permission Permission { get; set; }

    /// <summary>
    /// This is the code associated with the document type.
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Size of the blob.
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// The date the document was created.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Identity that created initial document, if specified.
    /// </summary>
    public Uri? CreatedBy { get; set; }

    /// <summary>
    /// The name of the identity that created the document, if specified.
    /// </summary>
    public string? CreatedByName { get; set; }

    /// <summary>
    /// The date the document was modified, if specified.
    /// </summary>
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Identity that created modified document, if specified.
    /// </summary>
    public Uri? ModifiedBy { get; set; }

    /// <summary>
    /// The name of the identity that modified the document, if specified.
    /// </summary>
    public string? ModifiedByName { get; set; }

    /// <summary>
    /// The date the document was shared, if specified.
    /// </summary>
    public DateTime? SharedOn { get; set; }

    /// <summary>
    /// Identity that shared the document, if specified.
    /// </summary>
    public Uri? SharedBy { get; set; }

    /// <summary>
    /// The name of the identity that shared the document, if specified.
    /// </summary>
    public string? SharedByName { get; set; }
    
    /// <summary>
    /// Specify DateTime from witch share is started
    /// </summary>
    public DateTime? From { get; set; }
    /// <summary>
    /// Specify DateTime when share is ended
    /// </summary>
    public DateTime? To { get; set; }
}
