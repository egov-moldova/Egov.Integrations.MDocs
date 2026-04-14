namespace Egov.Integrations.MDocs.Models;

/// <summary>
/// A class that contains share reservation information.
/// </summary>
public class ShareReservation
{
    /// <summary>
    /// Id of the reserved share.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Share access code, if requested.
    /// </summary>
    public string? AccessCode { get; set; }

    /// <summary>
    /// Full link to the reserved share.
    /// </summary>
    public required string FullLink { get; set; }
}
