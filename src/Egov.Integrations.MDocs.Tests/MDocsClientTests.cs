using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Egov.Integrations.MDocs.Models;

namespace Egov.Integrations.MDocs.Tests;

public class MDocsClientTests : MDocsClientTestsBase
{
    [Fact]
    public async Task UploadBlobAsync_SmallStream_CallsPostAsync()
    {
        // Arrange
        var (client, httpHandler) = CreateClient();
        var blobId = Guid.NewGuid();
        httpHandler.SetupResponse(HttpStatusCode.OK, new { Id = blobId });
        
        using var stream = new MemoryStream("Hello World"u8.ToArray());
        var contentType = "text/plain";

        // Act
        var resultId = await client.UploadBlobAsync(stream, contentType);

        // Assert
        Assert.Equal(blobId, resultId);
        var request = Assert.Single(httpHandler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/blobs", request.RequestUri?.AbsolutePath);
        Assert.Equal(contentType, request.Content?.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task UploadBlobAsync_WithDocumentTypeCode_AddsQueryParameter()
    {
        // Arrange
        var (client, httpHandler) = CreateClient();
        httpHandler.SetupResponse(HttpStatusCode.OK, new { Id = Guid.NewGuid() });
        
        using var stream = new MemoryStream("data"u8.ToArray());
        var docTypeCode = "DOC001";

        // Act
        await client.UploadBlobAsync(stream, "text/plain", docTypeCode);

        // Assert
        var request = Assert.Single(httpHandler.Requests);
        Assert.Contains($"documentTypeCode={docTypeCode}", request.RequestUri?.Query);
    }

    [Fact]
    public async Task DeleteBlobAsync_CallsDeleteAsync()
    {
        // Arrange
        var (client, httpHandler) = CreateClient();
        var blobId = Guid.NewGuid();
        httpHandler.SetupResponse(HttpStatusCode.NoContent);

        // Act
        await client.DeleteBlobAsync(blobId);

        // Assert
        var request = Assert.Single(httpHandler.Requests);
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Equal($"/blobs/{blobId}", request.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task PublishDocumentsAsync_CallsPostAsyncWithJson()
    {
        // Arrange
        var (client, httpHandler) = CreateClient();
        var blobId = Guid.NewGuid();
        var publishedDoc = new PublishedDocument 
        { 
            Id = Guid.NewGuid(), 
            Name = "Test.pdf", 
            Principal = new Uri("principal://test") 
        };
        httpHandler.SetupResponse(HttpStatusCode.OK, new List<PublishedDocument> { publishedDoc });

        var documents = new List<Document>
        {
            new Document 
            { 
                Name = "Test.pdf", 
                Principal = new Uri("principal://test"),
                CreatedBy = new Uri("principal://creator")
            }
        };

        // Act
        var result = await client.PublishDocumentsAsync(blobId, documents, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(publishedDoc.Id, result[0].Id);
        var request = Assert.Single(httpHandler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/documents", request.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task GetDocumentsAsync_SetsPaginationHeaders()
    {
        // Arrange
        var (client, httpHandler) = CreateClient();
        var documents = new List<DocumentDetails>
        {
            new DocumentDetails 
            { 
                Id = Guid.NewGuid(), 
                Name = "Doc1", 
                Type = "T1",
                Flags = new DocumentFlags(),
                CreatedOn = DateTime.UtcNow
            }
        };
        
        var paginationHeader = "{\"TotalCount\":10,\"CurrentPage\":1,\"PageSize\":1}";
        httpHandler.SetupResponse(HttpStatusCode.OK, documents, new Dictionary<string, string> { { "X-Pagination", paginationHeader } });

        // Act
        var result = await client.GetDocumentsAsync(page: 1, itemsPerPage: 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.TotalCount);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(1, result.PageSize);
        Assert.Single(result.Items);
        
        var request = Assert.Single(httpHandler.Requests);
        Assert.Contains("page=1", request.RequestUri?.Query);
        Assert.Contains("itemsPerPage=1", request.RequestUri?.Query);
    }

    [Fact]
    public async Task UploadBlobAsync_LargeBytes_HandlesChunkedUpload()
    {
        // Arrange
        var (client, httpHandler) = CreateClient();
        var blobId = Guid.NewGuid();
        
        // Mocking behavior: First POST returns blobId, subsequent PUTs return success.
        httpHandler.ResponseFactory = request =>
        {
            if (request.Method == HttpMethod.Post)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var json = JsonSerializer.Serialize(new { Id = blobId }, WebJsonSerializerOptions);
                response.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                return response;
            }
            if (request.Method == HttpMethod.Put)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        };

        // 5MB + 1 byte
        var largeData = new byte[5 * 1024 * 1024 + 1];
        new Random().NextBytes(largeData);

        // Act
        var resultId = await client.UploadBlobAsync(largeData, "application/octet-stream");

        // Assert
        Assert.Equal(blobId, resultId);
        // Expecting 2 requests: 1 POST (first 5MB), 1 PUT (remaining 1 byte)
        Assert.Equal(2, httpHandler.Requests.Count);
        
        var postRequest = httpHandler.Requests[0];
        Assert.Equal(HttpMethod.Post, postRequest.Method);
        Assert.NotNull(postRequest.Content?.Headers.ContentRange);
        Assert.Equal(0, postRequest.Content.Headers.ContentRange.From);
        Assert.Equal(5 * 1024 * 1024 - 1, postRequest.Content.Headers.ContentRange.To);

        var putRequest = httpHandler.Requests[1];
        Assert.Equal(HttpMethod.Put, putRequest.Method);
        Assert.Equal($"/blobs/{blobId}", putRequest.RequestUri?.AbsolutePath);
        Assert.NotNull(putRequest.Content?.Headers.ContentRange);
        Assert.Equal(5 * 1024 * 1024, putRequest.Content.Headers.ContentRange.From);
        Assert.Equal(5 * 1024 * 1024, putRequest.Content.Headers.ContentRange.To);
    }

    [Fact]
    public async Task GetDocumentsAsync_ErrorResponse_ThrowsMDocsException()
    {
        // Arrange
        var (client, httpHandler) = CreateClient();
        var errorResponse = new 
        { 
            Title = "Bad Request", 
            Errors = new Dictionary<string, string[]> { { "Principal", new[] { "Invalid" } } },
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };
        httpHandler.SetupResponse(HttpStatusCode.BadRequest, errorResponse);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<MDocsException>(() => client.GetDocumentsAsync());
        Assert.Contains("Bad Request", exception.Message);
        Assert.Contains("Principal", exception.Message);
    }
}
