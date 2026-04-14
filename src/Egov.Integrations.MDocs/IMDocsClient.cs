using Egov.Integrations.MDocs.Models;

namespace Egov.Integrations.MDocs;

/// <summary>
/// An interface that represents an MDocs Client.
/// </summary>
public interface IMDocsClient
{
    /// <summary>
    /// Uploads a blob to MDocs.
    /// </summary>
    /// <param name="filePath">File path of the blob to upload.</param>
    /// <param name="contentType">The type of the content that is uploaded. For known content types, please use <see ref="MDocsContentType" />.</param>
    /// <param name="documentTypeCode">The code of the document type.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>A task that includes the ID of the resulting blob.</returns>
    Task<Guid> UploadBlobAsync(string filePath, string contentType, string? documentTypeCode = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a blob to MDocs.
    /// </summary>
    /// <param name="blobStream">The content of the blob to upload.</param>
    /// <param name="contentType">The type of the content that is uploaded. For known content types, please use <see ref="MDocsContentType" />.</param>
    /// <param name="documentTypeCode">The code of the document type.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>A task that includes the ID of the resulting blob.</returns>
    Task<Guid> UploadBlobAsync(Stream blobStream, string contentType, string? documentTypeCode = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a blob to MDocs.
    /// </summary>
    /// <param name="blobBytes">The content of the blob to upload.</param>
    /// <param name="contentType">The type of the content that is uploaded. For known content types, please use <see ref="MDocsContentType" />.</param>
    /// <param name="documentTypeCode">The code of the document type.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>A task that includes the ID of the resulting blob.</returns>
    Task<Guid> UploadBlobAsync(ReadOnlyMemory<byte> blobBytes, string contentType, string? documentTypeCode = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a blob from MDocs.
    /// </summary>
    /// <param name="blobId">The id of the blob to be deleted</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>A task to be awaited.</returns>
    Task DeleteBlobAsync(Guid blobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Transforms the uploaded file into the specified format.
    /// </summary>
    /// <param name="documentStream">Input stream of file to be transformed.</param>
    /// <param name="documentTypeCode">The code associated with a Document Type.</param>
    /// <param name="format">Format of document.</param>
    /// <param name="contentType">Content Type of input stream.</param>
    /// <param name="language">The language of the resulting document.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>Stream of the transformed document.</returns>
    Task<Stream> TransformDocumentAsync(Stream documentStream, string contentType, string documentTypeCode, TemplateContentType format,
        string? language = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Transforms the uploaded file into the specified format.
    /// </summary>
    /// <param name="documentBytes">Input stream of file to be transformed.</param>
    /// <param name="documentTypeCode">The code associated with a Document Type.</param>
    /// <param name="format">Format of document.</param>
    /// <param name="contentType">Content Type of input stream.</param>
    /// <param name="language">The language of the resulting document.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>Stream of the transformed document.</returns>
    Task<Stream> TransformDocumentAsync(ReadOnlyMemory<byte> documentBytes, string documentTypeCode, string contentType, TemplateContentType format,
        string? language = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates folders or documents with permissions for blob.
    /// </summary>
    /// <param name="blobId">Id of uploaded blob. When <value>null</value> a folder is created instead.</param>
    /// <param name="documents">Documents to be created, referencing principals for the indicated blob.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>Details about published documents.</returns>
    Task<IList<PublishedDocument>> PublishDocumentsAsync(Guid? blobId, IReadOnlyList<Document> documents, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all documents for principal.
    /// </summary>
    /// <param name="principal">Principal for whom the action is made.</param>
    /// <param name="type">The code associated with a Document Type.</param>
    /// <param name="folderId">The folder in which to list the documents.</param>
    /// <param name="page">Page of list.</param>
    /// <param name="itemsPerPage">Items for page to display.</param>
    /// <param name="orderField">Field used to sort results.</param>
    /// <param name="searchBy">Text used to filter results (multi-keyword filter separated using single space character)</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>The paginated list of requested documents.</returns>
    Task<IPagedItems<DocumentDetails>> GetDocumentsAsync(Uri? principal = null, string? type = null, Guid? folderId = null, int? page = null, int? itemsPerPage = null, string? orderField = null, string? searchBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all recycled documents for principal.
    /// </summary>
    /// <param name="principal">Principal for whom the action is made.</param>
    /// <param name="type">The code associated with a Document Type.</param>
    /// <param name="page">Page of list.</param>
    /// <param name="itemsPerPage">Items for page to display.</param>
    /// <param name="orderField">Field used to sort results.</param>
    /// <param name="searchBy">Text used to filter results (multi-keyword filter separated using single space character)</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>The paginated list of requested recycled documents.</returns>
    Task<IPagedItems<RecycledDocumentDetails>> GetRecycledDocumentsAsync(Uri? principal = null, string? type = null, int? page = null, int? itemsPerPage = null, string? orderField = null, string? searchBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets particular document details.
    /// </summary>
    /// <param name="documentId">Id of document.</param>
    /// <param name="principal">Principal for whom the action is made.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>Details for the requested document</returns>
    Task<DocumentDetails> GetDocumentAsync(Guid documentId, Uri? principal = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets document size
    /// </summary>
    /// <param name="documentId">Id of document.</param>
    /// <param name="principal">Principal for whom the action is made.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>The size of the document.</returns>
    Task<long> GetDocumentSizeAsync(Guid documentId, Uri? principal = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Patches a document.
    /// </summary>
    /// <param name="documentId">Id of document.</param>
    /// <param name="patch">An object that represents patch operations to be performed.</param>
    /// <param name="principal">Principal for whom the action is made.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>A task to be awaited.</returns>
    Task PatchDocumentAsync(Guid documentId, DocumentPatch patch, Uri? principal = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a document in the specified format.
    /// </summary>
    /// <param name="documentId">Id of document.</param>
    /// <param name="format">Format of document.</param>
    /// <param name="language">The language of the resulting document.</param>
    /// <param name="principal">Principal for whom the action is made.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>Stream used to fetch data.</returns>
    Task<Stream> DownloadDocumentAsync(Guid documentId, TemplateContentType? format = null, string? language = null,
        Uri? principal = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copy a document to a folder.
    /// </summary>
    /// <param name="documentId">Id of document.</param>
    /// <param name="folderId">Destination folder, use null for root.</param>
    /// <param name="principal">Principal for whom the action is made.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>A task to be awaited.</returns>
    Task CopyDocumentAsync(Guid documentId, Guid? folderId, Uri? principal = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a document.
    /// </summary>
    /// <param name="documentId">Id of document.</param>
    /// <param name="principal">Principal for whom the action is made.</param>
    /// <param name="permanent">If true, delete document permanently, without recycling it.</param>
    /// <param name="force">If true, delete the folder event if it is not empty.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>A task to be awaited.</returns>
    Task DeleteDocumentAsync(Guid documentId, Uri? principal = null, bool permanent = false, bool force = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restore a document from recycle bin.
    /// </summary>
    /// <param name="documentId">Id of the document.</param>
    /// <param name="principal">Principal for whom the action is made.</param>
    /// <param name="folderId">The folder in which to restore the document.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>Restore a document from recycle bin.</returns>
    Task RestoreRecycledDocumentAsync(Guid documentId, Uri? principal = null, Guid? folderId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Empties the recycle bin.
    /// </summary>
    /// <param name="principal">Principal for whom the action is made.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>A task to be awaited.</returns>
    Task EmptyRecycledDocumentsAsync(Uri? principal = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets previous versions for a document.
    /// </summary>
    /// <param name="documentId">Id of document</param>
    /// <param name="principal">Principal for whom the action is made.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>Details about previous document versions.</returns>
    Task<IList<DocumentVersion>> GetDocumentVersionsAsync(Guid documentId, Uri? principal = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a previous version of the document.
    /// </summary>
    /// <param name="documentId">Id of document.</param>
    /// <param name="version">Version of document.</param>
    /// <param name="principal">Principal for whom the action is made.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>Stream used to fetch data.</returns>
    Task<Stream> DownloadDocumentVersionAsync(Guid documentId, short version, Uri? principal = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a previous version of the document. If <paramref name="includingPrevious"/> is true, all versions previous to the indicated one are also deleted.
    /// </summary>
    /// <param name="documentId">Id of document.</param>
    /// <param name="version">Version of document.</param>
    /// <param name="includingPrevious">If true, also deletes all versions previous to the indicated one.</param>
    /// <param name="principal">Principal for whom the action is made.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>A task to be awaited.</returns>
    Task DeleteDocumentVersionAsync(Guid documentId, short version, bool includingPrevious = false, Uri? principal = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Shares a document with one or more principals.
    /// </summary>
    /// <param name="documentId">Id of document.</param>
    /// <param name="shares">A list of document sharing request details.</param>
    /// <param name="principal">Principal for whom the action is made.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>A task that includes the list of ID of the created document shares.</returns>
    Task<IList<Guid>> ShareDocumentAsync(Guid documentId, IReadOnlyList<ShareRequest> shares, Uri? principal = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the list shares for a document.
    /// </summary>
    /// <param name="documentId">Id of document.</param>
    /// <param name="page">Page of list.</param>
    /// <param name="itemsPerPage">Items for page to display.</param>
    /// <param name="orderField">Field used to sort results.</param>
    /// <param name="searchBy">Text used to filter results (multi-keyword filter separated using single space character).</param>
    /// <param name="principal">Principal for whom the action is made.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>s
    /// <returns>The paginated list of document shares.</returns>
    Task<IPagedItems<ShareInfo>> GetDocumentSharesAsync(Guid documentId, Uri? principal = null, int? page = null, int? itemsPerPage = null, string? orderField = null, string? searchBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing document share.
    /// </summary>
    /// <param name="documentId">Id of document.</param>
    /// <param name="shareId">Id of share.</param>
    /// <param name="share">Updated document share details.</param>
    /// <param name="principal">Principal for whom the action is made.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>s
    /// <returns>A task to be awaited.</returns>
    Task UpdateDocumentShareAsync(Guid documentId, Guid shareId, ShareBase share, Uri? principal = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a document share.
    /// </summary>
    /// <param name="documentId">Id of document.</param>
    /// <param name="shareId">Id of share.</param>
    /// <param name="principal">Principal for whom the action is made.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>s
    /// <returns>A task to be awaited.</returns>
    Task DeleteDocumentShareAsync(Guid documentId, Guid shareId, Uri? principal = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all shares made for a principal.
    /// </summary>
    /// <param name="principal">Principal for whom the action is made.</param>
    /// <param name="page">Page of list.</param>
    /// <param name="itemsPerPage">Items for page to display.</param>
    /// <param name="orderField">Field used to sort results.</param>
    /// <param name="searchBy">Text used to filter results (multi-keyword filter separated using single space character).</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>The paginated list of requested shares.</returns>
    Task<IPagedItems<ShareDetails>> GetSharesForMeAsync(Uri? principal = null, int? page = null, int? itemsPerPage = null, string? orderField = null, string? searchBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all documents shared by a principal.
    /// </summary>
    /// <param name="principal">Principal for whom the action is made.</param>
    /// <param name="page">Page of list.</param>
    /// <param name="itemsPerPage">Items for page to display.</param>
    /// <param name="orderField">Field used to sort results.</param>
    /// <param name="searchBy">Text used to filter results (multi-keyword filter separated using single space character).</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>The paginated list of shared documents.</returns>
    Task<IPagedItems<SharedDocument>> GetSharesByMeAsync(Uri? principal = null, int? page = null, int? itemsPerPage = null, string? orderField = null, string? searchBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reserves a share.
    /// </summary>
    /// <param name="generateAccessCode"></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>Details for reserved share</returns>
    Task<ShareReservation> ReserveShareAsync(bool generateAccessCode = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get document type details.
    /// </summary>
    /// <param name="documentTypeCode">The code of the document type.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>Details for document type.</returns>
    Task<DocumentTypeDetails> GetDocumentTypeAsync(string documentTypeCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get principal quota.
    /// </summary>
    /// <param name="principal">Principal for whom the action is made.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor operation cancellation.</param>
    /// <returns>Maximum available storage and used storage by principal.</returns>
    Task<PrincipalQuota> GetPrincipalQuotaAsync(Uri? principal = null, CancellationToken cancellationToken = default);
}