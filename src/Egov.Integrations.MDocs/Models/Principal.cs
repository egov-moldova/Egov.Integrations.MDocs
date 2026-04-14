namespace Egov.Integrations.MDocs.Models;

/// <summary>
/// A factory class for valid principals.
/// </summary>
public class Principal : Uri
{
    /// <summary>
    /// Principal type.
    /// </summary>
    public enum Type
    {
        /// <summary>
        /// Principal represents unknown identity.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Principal represents public anonymous access.
        /// </summary>
        Public = 1,

        /// <summary>
        /// Principal represents a natural person.
        /// </summary>
        Person = 2,

        /// <summary>
        /// Principal represents a legal entity.
        /// </summary>
        Organization = 3,

        /// <summary>
        /// Principal represents an informational system.
        /// </summary>
        System = 4
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Principal"/> with the specified uri.
    /// </summary>
    /// <param name="uriString">A string that identifies the principal to be represented by this instance.</param>
    private Principal(string uriString) : base(uriString)
    {
    }

    /// <summary>
    /// Creates a new principal for a public anonymous access.
    /// </summary>
    /// <returns>The new principal as an <see cref="Uri"/>.</returns>
    public static Uri Public()
        => new Principal("urn:public");

    /// <summary>
    /// Creates a principal for a natural person.
    /// </summary>
    /// <param name="personId">Person identifier.</param>
    /// <returns>The new principal as an <see cref="Uri"/>.</returns>
    public static Uri ForPerson(string personId)
        => new Principal("urn:md:idnp:" + personId);

    /// <summary>
    /// Creates a principal for a legal entity.
    /// </summary>
    /// <param name="organizationId">Organization identifier.</param>
    /// <returns>The new principal as an <see cref="Uri"/>.</returns>
    public static Uri ForOrganization(string organizationId)
        => new Principal("urn:md:idno:" + organizationId);

    /// <summary>
    /// Creates a principal for an informational system.
    /// </summary>
    /// <param name="systemId">System identifier.</param>
    /// <returns>The new principal as an <see cref="Uri"/>.</returns>
    public static Uri ForSystem(Guid systemId)
        => new Principal("urn:md:system:" + systemId);

    /// <summary>
    /// Returns the type of the principal.
    /// </summary>
    /// <param name="principal">The principal to analize.</param>
    /// <returns>The type of the principal.</returns>
    /// <exception cref="ArgumentNullException">When principal is null.</exception>
    public static Type GetPrincipalType(Uri principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.Scheme != "urn")
        {
            return Type.Unknown;
        }
        
        var parts = principal.AbsolutePath.Split(':');

        switch (parts.Length)
        {
            case 1:
                return parts[0] switch
                {
                    "public" => Type.Public,
                    _ => Type.Unknown
                };
            case 3:
                if (parts[0] != "md")
                {
                    return Type.Unknown;
                }

                return parts[1] switch
                {
                    "idnp" => Type.Person,
                    "idno" => Type.Organization,
                    "system" => Type.System,
                    _ => Type.Unknown
                };
            default:
                return Type.Unknown;
        }
    }

    /// <summary>
    /// Returns the person identifier from the principal.
    /// </summary>
    /// <param name="principal">The principal to parse.</param>
    /// <returns>The person identifier from the principal.</returns>
    /// <exception cref="ArgumentException">When principal is not a person.</exception>
    public static string GetPersonId(Uri principal)
    {
        if (GetPrincipalType(principal) != Type.Person)
        {
            throw new ArgumentException("Principal is not a person.", nameof(principal));
        }
        return principal.AbsolutePath.Split(':')[2];
    }

    /// <summary>
    /// Returns the organization identifier from the principal.
    /// </summary>
    /// <param name="principal">The principal to parse.</param>
    /// <returns>The organization identifier from the principal.</returns>
    /// <exception cref="ArgumentException">When principal is not a organization.</exception>
    public static string GetOrganizationId(Uri principal)
    {
        if (GetPrincipalType(principal) != Type.Organization)
        {
            throw new ArgumentException("Principal is not an organization.", nameof(principal));
        }
        return principal.AbsolutePath.Split(':')[2];
    }

    /// <summary>
    /// Returns the system identifier from the principal.
    /// </summary>
    /// <param name="principal">The principal to parse.</param>
    /// <returns>The system identifier from the principal.</returns>
    /// <exception cref="ArgumentException">When principal is not a system.</exception>
    public static Guid GetSystemId(Uri principal)
    {
        if (GetPrincipalType(principal) != Type.System)
        {
            throw new ArgumentException("Principal is not a system.", nameof(principal));
        }
        if (!Guid.TryParse(principal.AbsolutePath.Split(':')[2], out var result))
        {
            throw new ArgumentException("Principal is not a system.", nameof(principal));
        }
        return result;
    }
}