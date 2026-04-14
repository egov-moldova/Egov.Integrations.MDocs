namespace Egov.Integrations.MDocs.Models;

/// <summary>
/// A class that contains document sharing information.
/// </summary>
public class ShareInfo : ShareBase
{
    /// <summary>
    /// Id of the document share.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// With whom the document is shared.
    /// </summary>
    public required Uri Principal { get; set; }

    /// <summary>
    /// The full name of the principal with whom document is shared.
    /// </summary>
    public required string PrincipalFullName { get; set; }
}
