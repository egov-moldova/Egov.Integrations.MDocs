namespace Egov.Integrations.MDocs.Models.Internal;

/// <summary>
/// Class used to capture validation error messages
/// </summary>
internal class ValidationErrorResponse
{
    /// <summary>
    /// Short title describing the error
    /// </summary>
    public required string Title { get; set; }
    /// <summary>
    /// Details regarding fields and error messages
    /// </summary>
    public required Dictionary<string, string[]> Errors { get; set; }
    public int Status { get; set; }
    public required string Type { get; set; }
}