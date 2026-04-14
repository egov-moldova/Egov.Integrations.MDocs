using System.Text.Json.Serialization;

namespace Egov.Integrations.MDocs.Models;

/// <summary>
/// Describes a request for sharing a document.
/// </summary>
public class ShareRequest : ShareBase
{
    /// <summary>
    /// The id of the reserved document share, if any.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? ReservedId { get; set; }

    /// <summary>
    /// With whom to share the document.
    /// </summary>
    public required Uri For { get; set; }

    /// <summary>
    /// Set to true to suppress notifications regarding the share. Defaults to false.
    /// </summary>
    public bool? SuppressNotifications { get; set; }
}
