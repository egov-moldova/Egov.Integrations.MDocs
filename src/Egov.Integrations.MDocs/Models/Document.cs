namespace Egov.Integrations.MDocs.Models;

/// <summary>
/// An object that represent a principal permissions to access a blob.
/// </summary>
public class Document
{
    /// <summary>
    /// Owning principal.
    /// </summary>
    public required Uri Principal { get; set; }

    /// <summary>
    /// Document name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional document number.
    /// </summary>
    public string? Number { get; set; }

    /// <summary>
    /// Optional document destination folder id.
    /// </summary>
    public Guid? FolderId { get; set; }

    /// <summary>
    /// A principal that shows who created the document.
    /// </summary>
    public required Uri CreatedBy { get; set; }

    /// <summary>
    /// The date and time when the document was created. Optional, defaults to current date and time.
    /// </summary>
    public DateTime? CreatedOn { get; set; }

    /// <summary>
    /// The date and time when the document shall expire. Optional. Note that principals can change this value at their discretion.
    /// </summary>
    public DateTime? ExpiresOn { get; set; }

    /// <summary>
    /// Set to true to suppress notifications regarding the newly published document. Defaults to false.
    /// </summary>
    public bool? SuppressNotifications { get; set; }

    /// <summary>
    /// A list of shares to create along with the document.
    /// </summary>
    public IList<ShareRequest> Shares { get; set; } = new List<ShareRequest>();
}

/// <summary>
/// An object that represents the response after document creation.
/// </summary>
public class PublishedDocument
{
    /// <summary>
    /// Published document id.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Published document name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Published document owner.
    /// </summary>
    public required Uri Principal { get; set; }

    /// <summary>
    /// Published document expiration date, if applicable.
    /// </summary>
    public DateTime? ExpiresOn { get; set; }
}