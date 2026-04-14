namespace Egov.Integrations.MDocs.Models;

/// <summary>
/// A class that contains principal quota information.
/// </summary>
public class PrincipalQuota
{
    /// <summary>
    /// Maximum available storage.
    /// </summary>
    public long StorageMaximum { get; set; }

    /// <summary>
    /// Used storage.
    /// </summary>
    public long StorageUsage { get; set; }
}
