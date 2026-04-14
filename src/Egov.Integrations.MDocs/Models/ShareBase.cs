namespace Egov.Integrations.MDocs.Models;

/// <summary>
/// A base class that contains document sharing information.
/// </summary>
public class ShareBase
{
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
}
