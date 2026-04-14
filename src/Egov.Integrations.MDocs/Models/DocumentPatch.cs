using Egov.Integrations.MDocs.Models.Internal;

namespace Egov.Integrations.MDocs.Models;

/// <summary>
/// Represents a list of changes to be applied to a document.
/// </summary>
public class DocumentPatch
{
    internal IList<EntityOperation> Operations { get; } = new List<EntityOperation>();

    /// <summary>
    /// Rename the document.
    /// </summary>
    /// <param name="newName">The new name for the document.</param>
    /// <returns>This instance, useful for fluent calls.</returns>
    public DocumentPatch Rename(string newName)
    {
        Operations.Add(new EntityOperation
        {
            Path = "name",
            Operation = "replace",
            Value = newName
        });
        return this;
    }

    /// <summary>
    /// Move the document to a another folder.
    /// </summary>
    /// <param name="folderId">Destination folder or null for root.</param>
    /// <returns>This instance, useful for fluent calls.</returns>
    public DocumentPatch Move(Guid? folderId)
    {
        Operations.Add(new EntityOperation
        {
            Path = "folderid",
            Operation = folderId != null ? "replace" : "remove",
            Value = folderId
        });
        return this;
    }

    /// <summary>
    /// Change or remove the document number.
    /// </summary>
    /// <param name="number">The new value for the document number. If null, number is removed.</param>
    /// <returns>This instance, useful for fluent calls.</returns>
    public DocumentPatch ChangeNumber(string? number)
    {
        Operations.Add(new EntityOperation
        {
            Path = "number",
            Operation = number != null ? "replace" : "remove",
            Value = number
        });
        return this;
    }

    /// <summary>
    /// Change or remove document expiration date.
    /// </summary>
    /// <param name="expiresOn">The new value for the document number. If null, expiration is removed.</param>
    /// <returns>This instance, useful for fluent calls.</returns>
    public DocumentPatch ChangeExpiration(DateTime? expiresOn)
    {
        Operations.Add(new EntityOperation
        {
            Path = "expireson",
            Operation = expiresOn != null ? "replace" : "remove",
            Value = expiresOn
        });
        return this;
    }
}