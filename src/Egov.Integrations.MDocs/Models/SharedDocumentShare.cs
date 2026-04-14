namespace Egov.Integrations.MDocs.Models;

/// <summary>
/// Document share metadata.
/// </summary>
public class SharedDocumentShare
{
    /// <summary>
    /// Id of the document share.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Assigned share permission.
    /// </summary>
    public required Permission Permission { get; set; }

    /// <summary>
    /// From what date the share is accessible.
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// To what date the share is accessible.
    /// </summary>
    public DateTime? To { get; set; }

    /// <summary>
    /// The date the document was shared.
    /// </summary>
    public DateTime? SharedOn { get; set; }

    /// <summary>
    /// Identity that got document share.
    /// </summary>
    public required Uri SharedFor { get; set; }

    /// <summary>
    /// The name of the identity that got document share, if specified.
    /// </summary>
    public string? SharedForName { get; set; }
}
