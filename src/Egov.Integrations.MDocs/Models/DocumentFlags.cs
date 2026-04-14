using System.Text.Json.Serialization;

namespace Egov.Integrations.MDocs.Models;

/// <summary>
/// Indicative flags for a document.
/// </summary>
[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DocumentFlags
{
    /// <summary>
    /// There are no flags for this document.
    /// </summary>
    None = 0,

    /// <summary>
    /// This document is an official document.
    /// </summary>
    Official = 1,

    /// <summary>
    /// This document is new.
    /// </summary>
    New = 2,

    /// <summary>
    /// This document is expiring.
    /// </summary>
    Expiring = 4
}