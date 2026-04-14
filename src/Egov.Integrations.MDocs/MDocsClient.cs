using System.Buffers;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Egov.Integrations.MDocs.Helpers;
using Egov.Integrations.MDocs.Models;
using Egov.Integrations.MDocs.Models.Internal;
using Egov.Integrations.MDocs.Streaming;
using Microsoft.AspNetCore.Http.Extensions;

namespace Egov.Integrations.MDocs;

internal sealed class MDocsClient : IMDocsClient
{
    private const int BlobPartSize = 5 * 1024 * 1024;

    private readonly HttpClient _httpClient;

    public MDocsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Guid> UploadBlobAsync(string filePath, string contentType, string? documentTypeCode = null, CancellationToken cancellationToken = default)
    {
        await using var fileStream = File.OpenRead(filePath);
        return await UploadBlobAsync(fileStream, contentType, documentTypeCode, cancellationToken);
    }

    public async Task<Guid> UploadBlobAsync(Stream blobStream, string contentType, string? documentTypeCode = null,
        CancellationToken cancellationToken = default)
    {
        if (!blobStream.CanSeek || blobStream.Length <= BlobPartSize)
        {
            return await UploadBlob(new StreamContent(blobStream), contentType, documentTypeCode, cancellationToken);
        }

        var blobStreamLength = blobStream.Length;

        var buffer = ArrayPool<byte>.Shared.Rent(BlobPartSize);
        try
        {
            var bufferOffset = 0;
            while (bufferOffset < BlobPartSize)
            {
                var bytesRead = await blobStream.ReadAsync(buffer.AsMemory(bufferOffset, BlobPartSize - bufferOffset), cancellationToken);
                if (bytesRead == 0) break;
                bufferOffset += bytesRead;
            }

            if (bufferOffset != BlobPartSize)
            {
                throw new MDocsException("Blob stream reading failed!");
            }

            var blobId = await UploadBlob(new ReadOnlyMemoryContent(buffer.AsMemory(0, BlobPartSize))
            {
                Headers =
                {
                    ContentRange = new ContentRangeHeaderValue(0, BlobPartSize - 1, blobStreamLength)
                }
            }, contentType, documentTypeCode, cancellationToken);

            long currentOffset = BlobPartSize;
            while (currentOffset < blobStreamLength)
            {
                var partSize = (int)Math.Min(BlobPartSize, blobStreamLength - currentOffset);
                bufferOffset = 0;
                while (bufferOffset < partSize)
                {
                    var bytesRead = await blobStream.ReadAsync(buffer.AsMemory(bufferOffset, partSize - bufferOffset), cancellationToken);
                    if (bytesRead == 0) break;
                    bufferOffset += bytesRead;
                }

                await UploadBlobPart(blobId, buffer.AsMemory(0, partSize), currentOffset, blobStreamLength, cancellationToken);
                currentOffset += partSize;
            }

            return blobId;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public async Task<Guid> UploadBlobAsync(ReadOnlyMemory<byte> blobBytes, string contentType, string? documentTypeCode = null, 
        CancellationToken cancellationToken = default)
    {
        if (blobBytes.Length <= BlobPartSize)
        {
            return await UploadBlob(new ReadOnlyMemoryContent(blobBytes), contentType, documentTypeCode, cancellationToken);
        }

        var firstPart = blobBytes[..BlobPartSize];
        var blobId = await UploadBlob(new ReadOnlyMemoryContent(firstPart)
        {
            Headers =
            {
                ContentRange = new ContentRangeHeaderValue(0, BlobPartSize - 1, blobBytes.Length)
            }
        }, contentType, documentTypeCode, cancellationToken);

        var currentOffset = BlobPartSize;
        while (currentOffset < blobBytes.Length)
        {
            var partSize = Math.Min(BlobPartSize, blobBytes.Length - currentOffset);
            var currentPart = blobBytes.Slice(currentOffset, partSize);
            await UploadBlobPart(blobId, currentPart, currentOffset, blobBytes.Length, cancellationToken);
            currentOffset += partSize;
        }

        return blobId;
    }

    private async Task<Guid> UploadBlob(HttpContent blobContent, string contentType, string? documentTypeCode = null,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse;
        using (blobContent)
        {
            var requestUri = "blobs";
            var query = new QueryBuilder();
            if (!string.IsNullOrEmpty(documentTypeCode))
            {
                query.Add("documentTypeCode", documentTypeCode);
            }

            if (string.IsNullOrEmpty(contentType))
            {
                throw new MDocsException("'ContentType' must me specified!");
            }
            blobContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            httpResponse = await _httpClient.PostAsync(requestUri + query, blobContent, cancellationToken);
        }

        using (httpResponse)
        {
            var result = await httpResponse.HandleMDocsResponse<EntityIdentifier>(cancellationToken);
            return result.Id;
        }
    }

    private async Task UploadBlobPart(Guid blobId, ReadOnlyMemory<byte> blobPart, long blobOffset, long blobTotalSize,
        CancellationToken cancellationToken = default)
    {
        var requestUri = $"blobs/{blobId}";
        var content = new ReadOnlyMemoryContent(blobPart)
        {
            Headers =
            {
                ContentRange = new ContentRangeHeaderValue(blobOffset, blobOffset + blobPart.Length - 1, blobTotalSize)
            }
        };

        using var response = await _httpClient.PutAsync(requestUri, content, cancellationToken);
        response.CheckMDocsResponse();
    }

    public async Task DeleteBlobAsync(Guid blobId, CancellationToken cancellationToken = default)
    {
        var requestUri = $"blobs/{blobId}";

        using var response = await _httpClient.DeleteAsync(requestUri, cancellationToken);
        response.CheckMDocsResponse();
    }

    public async Task<Stream> TransformDocumentAsync(Stream documentStream, string contentType, string documentTypeCode, TemplateContentType format,
        string? language = null, CancellationToken cancellationToken = default)
        => await TransformDocument(new StreamContent(documentStream), contentType, documentTypeCode, format, language, cancellationToken);

    public async Task<Stream> TransformDocumentAsync(ReadOnlyMemory<byte> documentBytes, string contentType, string documentTypeCode, TemplateContentType format,
        string? language = null, CancellationToken cancellationToken = default)
        => await TransformDocument(new ReadOnlyMemoryContent(documentBytes), contentType, documentTypeCode, format, language, cancellationToken);

    private async Task<Stream> TransformDocument(HttpContent documentContent, string contentType, string documentTypeCode, TemplateContentType format,
        string? language = null, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse;
        using (documentContent)
        {
            var requestUri = "transform";
            var query = new QueryBuilder
            {
                { "format", format.ToString() },
                { "documentTypeCode", documentTypeCode }
            };

            if (string.IsNullOrEmpty(contentType))
            {
                throw new MDocsException("'ContentType' must me specified!");
            }
            if (!string.IsNullOrEmpty(language))
            {
                query.Add("language", language);
            }

            documentContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            httpResponse = await _httpClient.PostAsync(requestUri + query, documentContent, cancellationToken);
        }

        // TODO: Is HttpResponseMessage disposed when returned responseContent is disposed?
        httpResponse.CheckMDocsResponse();

        return await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
    }

    public async Task<IList<PublishedDocument>> PublishDocumentsAsync(Guid? blobId, IReadOnlyList<Document> documents, CancellationToken cancellationToken)
    {
        var requestUri = "documents";
        if (documents.Count == 0)
        {
            throw new MDocsException("No documents provided in the entry parameter 'Documents'");
        }

        var content = JsonContent.Create(new PublishDocumentsModel(blobId, documents));
        using var response = await _httpClient.PostAsync(requestUri, content, cancellationToken);

        return await response.HandleMDocsResponse<IList<PublishedDocument>>(cancellationToken);
    }

    public async Task<IPagedItems<DocumentDetails>> GetDocumentsAsync(Uri? principal = null, string? type = null, Guid? folderId = null, int? page = null, int? itemsPerPage = null, string? orderField = null, string? searchBy = null, CancellationToken cancellationToken = default)
        => await GetDocumentsQuery<DocumentDetails>("documents", principal, type, folderId, false, page, itemsPerPage, orderField, searchBy, cancellationToken);

    public async Task<IPagedItems<RecycledDocumentDetails>> GetRecycledDocumentsAsync(Uri? principal = null, string? type = null, int? page = null, int? itemsPerPage = null, string? orderField = null, string? searchBy = null, CancellationToken cancellationToken = default)
        => await GetDocumentsQuery<RecycledDocumentDetails>("recycled-documents", principal, type, null, true, page, itemsPerPage, orderField, searchBy, cancellationToken);

    private async Task<IPagedItems<T>> GetDocumentsQuery<T>(string requestUri, Uri? principal = null, string? type = null, Guid? folderId = null, bool recycled = false, int? page = null, int? itemsPerPage = null, string? orderField = null, string? searchBy = null,
        CancellationToken cancellationToken = default)
    {
        var query = new QueryBuilder();
        if (principal != null)
        {
            query.Add("principal", principal.ToString());
        }
        if (!string.IsNullOrEmpty(type))
        {
            query.Add("type", type);
        }
        if (recycled)
        {
            query.Add("recycled", recycled.ToString());
        }
        if (folderId != null)
        {
            query.Add("folderId", folderId.Value.ToString());
        }
        if (page != null)
        {
            query.Add("page", page.Value.ToString());
        }
        if (itemsPerPage != null)
        {
            query.Add("itemsPerPage", itemsPerPage.Value.ToString());
        }
        if (!string.IsNullOrEmpty(orderField))
        {
            query.Add("orderField", orderField);
        }
        if (!string.IsNullOrEmpty(searchBy))
        {
            query.Add("searchBy", searchBy);
        }

        using var response = await _httpClient.GetAsync(requestUri + query, cancellationToken);
        return await response.HandleMDocsPagedResponse<T>(cancellationToken);
    }

    public async Task<DocumentDetails> GetDocumentAsync(Guid documentId, Uri? principal = null, CancellationToken cancellationToken = default)
    {
        var requestUri = $"documents/{documentId}";
        var query = new QueryBuilder();
        if (principal != null)
        {
            query.Add("principal", principal.ToString());
        }

        using var response = await _httpClient.GetAsync(requestUri + query, cancellationToken);
        return await response.HandleMDocsResponse<DocumentDetails>(cancellationToken);
    }

    public async Task<long> GetDocumentSizeAsync(Guid documentId, Uri? principal = null, CancellationToken cancellationToken = default)
    {
        var requestUri = $"documents/{documentId}/size";
        var query = new QueryBuilder();
        if (principal != null)
        {
            query.Add("principal", principal.ToString());
        }

        using var response = await _httpClient.GetAsync(requestUri + query, cancellationToken);
        var result = await response.HandleMDocsResponse<EntitySize>(cancellationToken);
        return result.Size;
    }

    public async Task PatchDocumentAsync(Guid documentId, DocumentPatch patch, Uri? principal = null, CancellationToken cancellationToken = default)
    {
        var requestUri = $"documents/{documentId}";
        var query = new QueryBuilder();
        if (principal != null)
        {
            query.Add("principal", principal.ToString());
        }
        var content = JsonContent.Create(patch.Operations);
        using var response = await _httpClient.PatchAsync(requestUri + query, content, cancellationToken);
        response.CheckMDocsResponse();
    }

    public async Task<Stream> DownloadDocumentAsync(Guid documentId, TemplateContentType? format = null, string? language = null,
        Uri? principal = null, CancellationToken cancellationToken = default)
    {
        var requestUri = $"documents/{documentId}/blob";
        var query = new QueryBuilder();
        if (format != null)
        {
            query.Add("format", format.Value.ToString());
        }
        if (principal != null)
        {
            query.Add("principal", principal.ToString());
        }
        if (!string.IsNullOrEmpty(language))
        {
            query.Add("language", language);
        }

        var blobUri = new Uri(requestUri + query, UriKind.Relative);
        var httpResponse = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, blobUri)
        {
            Headers =
            {
                Range = new RangeHeaderValue(0, null)
            }
        }, cancellationToken);
        if (!httpResponse.IsSuccessStatusCode)
        {
            using (httpResponse) throw httpResponse.CreateExceptionFromBadResponse();
        }

        return new DownloadingBlobStream(_httpClient, blobUri, httpResponse);
    }

    public async Task CopyDocumentAsync(Guid documentId, Guid? folderId, Uri? principal = null, CancellationToken cancellationToken = default)
    {
        var requestUri = $"documents/{documentId}/copy";
        var query = new QueryBuilder();
        if (folderId != null)
        {
            query.Add("folderId", folderId.Value.ToString());
        }
        if (principal != null)
        {
            query.Add("principal", principal.ToString());
        }
        using var response = await _httpClient.PostAsync(requestUri + query, null, cancellationToken);
        response.CheckMDocsResponse();
    }

    public async Task DeleteDocumentAsync(Guid documentId, Uri? principal = null, bool permanent = false, bool force = false, CancellationToken cancellationToken = default)
    {
        var requestUri = $"documents/{documentId}";
        var query = new QueryBuilder();
        if (principal != null)
        {
            query.Add("principal", principal.ToString());
        }
        if (permanent)
        {
            query.Add("permanent", permanent.ToString());
        }
        if (force)
        {
            query.Add("force", force.ToString());
        }
        using var response = await _httpClient.DeleteAsync(requestUri + query, cancellationToken);
        response.CheckMDocsResponse();
    }

    public async Task RestoreRecycledDocumentAsync(Guid documentId, Uri? principal = null, Guid? folderId = null, CancellationToken cancellationToken = default)
    {
        var requestUri = $"recycled-documents/{documentId}/restore";
        var query = new QueryBuilder();
        if (principal != null)
        {
            query.Add("principal", principal.ToString());
        }
        if (folderId != null)
        {
            query.Add("folderId", folderId.Value.ToString());
        }
        using var response = await _httpClient.PostAsync(requestUri + query, null, cancellationToken);
        response.CheckMDocsResponse();
    }

    public async Task EmptyRecycledDocumentsAsync(Uri? principal = null, CancellationToken cancellationToken = default)
    {
        var requestUri = "recycled-documents";
        var query = new QueryBuilder();
        if (principal != null)
        {
            query.Add("principal", principal.ToString());
        }
        using var response = await _httpClient.DeleteAsync(requestUri + query, cancellationToken);
        response.CheckMDocsResponse();
    }

    public async Task<IList<DocumentVersion>> GetDocumentVersionsAsync(Guid documentId, Uri? principal = null, CancellationToken cancellationToken = default)
    {
        var requestUri = $"documents/{documentId}/versions";
        var query = new QueryBuilder();
        if (principal != null)
        {
            query.Add("principal", principal.ToString());
        }
        using var response = await _httpClient.GetAsync(requestUri + query, cancellationToken);
        return await response.HandleMDocsResponse<List<DocumentVersion>>(cancellationToken);
    }

    public async Task<Stream> DownloadDocumentVersionAsync(Guid documentId, short version, Uri? principal = null, CancellationToken cancellationToken = default)
    {
        var requestUri = $"documents/{documentId}/versions/{version}/blob";
        var query = new QueryBuilder();
        if (principal != null)
        {
            query.Add("principal", principal.ToString());
        }

        var blobUri = new Uri(requestUri + query, UriKind.Relative);
        var httpResponse = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, blobUri)
        {
            Headers =
            {
                Range = new RangeHeaderValue(0, null)
            }
        }, cancellationToken);
        if (!httpResponse.IsSuccessStatusCode)
        {
            using (httpResponse) throw httpResponse.CreateExceptionFromBadResponse();
        }

        return new DownloadingBlobStream(_httpClient, blobUri, httpResponse);
    }

    public async Task DeleteDocumentVersionAsync(Guid documentId, short version, bool includingPrevious = false,
        Uri? principal = null, CancellationToken cancellationToken = default)
    {
        var requestUri = $"documents/{documentId}/versions/{version}";
        var query = new QueryBuilder();
        if (includingPrevious)
        {
            query.Add("includingPrevious", "true");
        }
        if (principal != null)
        {
            query.Add("principal", principal.ToString());
        }
        using var response = await _httpClient.DeleteAsync(requestUri + query, cancellationToken);
        response.CheckMDocsResponse();
    }

    public async Task<IList<Guid>> ShareDocumentAsync(Guid documentId, IReadOnlyList<ShareRequest> shares, Uri? principal = null, CancellationToken cancellationToken = default)
    {
        var requestUri = $"documents/{documentId}/shares";
        var query = new QueryBuilder();
        if (principal != null)
        {
            query.Add("principal", principal.ToString());
        }
        var content = JsonContent.Create(shares);
        using var response = await _httpClient.PostAsync(requestUri + query, content, cancellationToken);
        var result = await response.HandleMDocsResponse<List<EntityIdentifier>>(cancellationToken);

        return result.ConvertAll(eid => eid.Id);
    }

    public async Task<IPagedItems<ShareInfo>> GetDocumentSharesAsync(Guid documentId, Uri? principal = null, int? page = null, int? itemsPerPage = null, string? orderField = null, string? searchBy = null, CancellationToken cancellationToken = default)
    {
        var requestUri = $"documents/{documentId}/shares";
        var query = new QueryBuilder();
        if (principal != null)
        {
            query.Add("principal", principal.ToString());
        }
        if (page != null)
        {
            query.Add("page", page.Value.ToString());
        }
        if (itemsPerPage != null)
        {
            query.Add("itemsPerPage", itemsPerPage.Value.ToString());
        }
        if (!string.IsNullOrEmpty(orderField))
        {
            query.Add("orderField", orderField);
        }
        if (!string.IsNullOrEmpty(searchBy))
        {
            query.Add("searchBy", searchBy);
        }
        using var response = await _httpClient.GetAsync(requestUri + query, cancellationToken);
        return await response.HandleMDocsPagedResponse<ShareInfo>(cancellationToken);
    }

    public async Task UpdateDocumentShareAsync(Guid documentId, Guid shareId, ShareBase share, Uri? principal = null, CancellationToken cancellationToken = default)
    {
        var requestUri = $"documents/{documentId}/shares/{shareId}";
        var query = new QueryBuilder();
        if (principal != null)
        {
            query.Add("principal", principal.ToString());
        }
        var content = JsonContent.Create(share);
        using var response = await _httpClient.PutAsync(requestUri + query, content, cancellationToken);
        response.CheckMDocsResponse();
    }

    public async Task DeleteDocumentShareAsync(Guid documentId, Guid shareId, Uri? principal = null, CancellationToken cancellationToken = default)
    {
        var requestUri = $"documents/{documentId}/shares/{shareId}";
        var query = new QueryBuilder();
        if (principal != null)
        {
            query.Add("principal", principal.ToString());
        }
        using var response = await _httpClient.DeleteAsync(requestUri + query, cancellationToken);
        response.CheckMDocsResponse();
    }

    public async Task<IPagedItems<ShareDetails>> GetSharesForMeAsync(Uri? principal = null, int? page = null, int? itemsPerPage = null, string? orderField = null, string? searchBy = null, CancellationToken cancellationToken = default)
        => await GetSharesQuery<ShareDetails>("shares/for-me", principal, page, itemsPerPage, orderField, searchBy, cancellationToken);

    public async Task<IPagedItems<SharedDocument>> GetSharesByMeAsync(Uri? principal = null, int? page = null, int? itemsPerPage = null, string? orderField = null, string? searchBy = null, CancellationToken cancellationToken = default)
        => await GetSharesQuery<SharedDocument>("shares/by-me", principal, page, itemsPerPage, orderField, searchBy, cancellationToken);

    private async Task<IPagedItems<T>> GetSharesQuery<T>(string requestUri, Uri? principal = null, int? page = null, int? itemsPerPage = null, string? orderField = null, string? searchBy = null,
        CancellationToken cancellationToken = default)
    {
        var query = new QueryBuilder();
        if (principal != null)
        {
            query.Add("principal", principal.ToString());
        }
        if (page != null)
        {
            query.Add("page", page.Value.ToString());
        }
        if (itemsPerPage != null)
        {
            query.Add("itemsPerPage", itemsPerPage.Value.ToString());
        }
        if (!string.IsNullOrEmpty(orderField))
        {
            query.Add("orderField", orderField);
        }
        if (!string.IsNullOrEmpty(searchBy))
        {
            query.Add("searchBy", searchBy);
        }

        using var response = await _httpClient.GetAsync(requestUri + query, cancellationToken);
        return await response.HandleMDocsPagedResponse<T>(cancellationToken);
    }

    public async Task<ShareReservation> ReserveShareAsync(bool generateAccessCode = true, CancellationToken cancellationToken = default)
    {
        var requestUri = $"shares/reservations";
        var query = new QueryBuilder();
        if (!generateAccessCode)
        {
            query.Add("generateAccessCode", generateAccessCode.ToString());
        }

        using var response = await _httpClient.PostAsync(requestUri + query, null, cancellationToken);
        return await response.HandleMDocsResponse<ShareReservation>(cancellationToken);
    }

    public async Task<DocumentTypeDetails> GetDocumentTypeAsync(string documentTypeCode, CancellationToken cancellationToken = default)
    {
        var requestUri = $"document-types/{documentTypeCode}";

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        return await response.HandleMDocsResponse<DocumentTypeDetails>(cancellationToken);
    }

    public async Task<PrincipalQuota> GetPrincipalQuotaAsync(Uri? principal = null, CancellationToken cancellationToken = default)
    {
        var requestUri = "quota";
        var query = new QueryBuilder();
        if (principal != null)
        {
            query.Add("principal", principal.ToString());
        }

        using var response = await _httpClient.GetAsync(requestUri + query, cancellationToken);
        return await response.HandleMDocsResponse<PrincipalQuota>(cancellationToken);
    }
}