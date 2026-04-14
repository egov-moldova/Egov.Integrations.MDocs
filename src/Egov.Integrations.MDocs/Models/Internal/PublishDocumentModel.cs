namespace Egov.Integrations.MDocs.Models.Internal;

internal class PublishDocumentsModel
{
    public Guid? BlobId { get; set; }
    public IReadOnlyList<Document> Documents { get; set; }

    public PublishDocumentsModel(Guid? blobId, IReadOnlyList<Document> documents)
    {
        BlobId = blobId;
        Documents = documents;
    }
}
