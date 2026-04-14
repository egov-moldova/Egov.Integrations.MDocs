using System.Text.Json.Serialization;

namespace Egov.Integrations.MDocs.Models;

/// <summary>
/// Enum definition for permission.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Permission : short
{
    /// <summary>
    /// No permissions.
    /// </summary>
    None = 0,

    /// <summary>
    /// Permission to read.
    /// </summary>
    Read = 1,

    /// <summary>
    /// Permission to write.
    /// </summary>
    Write = 2
}
