using System.Security.Cryptography.X509Certificates;

namespace Egov.Integrations.MDocs;

/// <summary>
/// Options for MDocs Client.
/// </summary>
public class MDocsClientOptions
{
    /// <summary>
    /// The base address of MDocs service.
    /// </summary>
    public required Uri BaseAddress { get; set; }

    /// <summary>
    /// Explicit service certificate to use.
    /// </summary>
    public X509Certificate2? SystemCertificate { get; set; }

    /// <summary>
    /// Explicit intermediate certificates to use.
    /// </summary>
    public X509Certificate2Collection? SystemCertificateIntermediaries { get; set; }
}