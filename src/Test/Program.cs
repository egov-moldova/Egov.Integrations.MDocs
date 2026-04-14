using Egov.Integrations.MDocs;
using Egov.Integrations.MDocs.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSystemCertificate(builder.Configuration.GetSection("Certificate"));
builder.Services.AddMDocsClient(builder.Configuration.GetSection("MDocs"));

var app = builder.Build();
var client = app.Services.GetRequiredService<IMDocsClient>();

// TODO: indicate a file path
// upload a blob
var blobId = await client.UploadBlobAsync(new MemoryStream(Encoding.UTF8.GetBytes("{\r\n  \"Identity\": \"1211112121212\",\r\n\r\n  \"Name\": \"test test\",\r\n\r\n  \"Date\": \"2023-02-28\",\r\n\r\n  \"Depts\": [\r\n    { \"budgetCode\": \"Consolidat\", \"totalDebt\": 31.36 },\r\n\r\n    {\r\n      \"budgetCode\": \"Bugetul asigurărilor sociale de stat\",\r\n      \"totalDebt\": 1436.44\r\n    }\r\n  ]\r\n}\r\n")), MDocsContentType.Json);
Console.WriteLine($"[UploadBlob]::Uploaded blob with Id: {blobId}");

// publish a document
var publishDocumentRequestBody = new[] {
    new Document
    {
        Principal = Principal.ForSystem(Guid.Parse("b06d4b7c-012b-4eb6-8acf-ee5dd6246222")),
        Name = "Doc.json",
        Number = "1",
        CreatedBy = Principal.ForSystem(Guid.Parse("b06d4b7c-012b-4eb6-8acf-ee5dd6246222"))
    }
};
var publishedDocuments = await client.PublishDocumentsAsync(blobId, publishDocumentRequestBody);
Console.WriteLine($"[PublishDocuments]::Published {publishedDocuments.Count} documents");

var documentId = publishedDocuments[0].Id;
Console.WriteLine($"[PublishDocuments]::First Document Id: {documentId}");

// get document metadata
var document = await client.GetDocumentAsync(documentId);
Console.WriteLine($"[GetDocument]::Got document with Id: {document.Id} and flags: {document.Flags}");

// get the list of documents for this client
var getAllDocuments = await client.GetDocumentsAsync(Principal.ForSystem(Guid.Parse("b06d4b7c-012b-4eb6-8acf-ee5dd6246222")));
Console.WriteLine($"[GetDocuments]::Got {getAllDocuments.Items.Count} documents");

// get recycled documents
var getAllRecycledDocuments = await client.GetRecycledDocumentsAsync(Principal.ForSystem(Guid.Parse("b06d4b7c-012b-4eb6-8acf-ee5dd6246222")));
Console.WriteLine($"[GetRecycledDocuments]::Got {getAllRecycledDocuments.Items.Count} recycled documents");


// get document size
var documentSize = await client.GetDocumentSizeAsync(documentId);
Console.WriteLine($"[GetDocumentSize]::Got document size: {documentSize}");

// rename document and change its number
var newName = "Renamed" + publishedDocuments[0].Name;
await client.PatchDocumentAsync(documentId, new DocumentPatch().Rename(newName).ChangeNumber("2"));

// share a document
var documentShares = await client.ShareDocumentAsync(documentId, new[] {
    new ShareRequest
    {
        For = Principal.ForPerson("1211112121212"),
        Permission = Permission.Write
    }
});
Console.WriteLine("[ShareDocument]::Document shared");

var shareId = documentShares[0];

// check share
var shareInfos = await client.GetDocumentSharesAsync(documentId);
Console.WriteLine($"[GetDocumentShares]::Got {shareInfos.Items.Count} shares, matching ID: {shareInfos.Items[0].Id == shareId}");

// retrieve all shares
var sharedDocuments = await client.GetSharesByMeAsync(Principal.ForSystem(Guid.Parse("b06d4b7c-012b-4eb6-8acf-ee5dd6246222")));
Console.WriteLine($"[GetSharesByMe]::Got {sharedDocuments.Items.Count} shared documents");

// delete share
await client.DeleteDocumentShareAsync(documentId, shareId);
Console.WriteLine("[DeleteDocumentShare]::Document unshared");

// delete document
await client.DeleteDocumentAsync(documentId);

var documentType = await client.GetDocumentTypeAsync("Folder");
Console.WriteLine($"[GetDocumentType]::Got document type: {documentType.TitleRomanian}");

// get principal quota
var quota = await client.GetPrincipalQuotaAsync(Principal.ForSystem(Guid.Parse("b06d4b7c-012b-4eb6-8acf-ee5dd6246222")));
Console.WriteLine($"[GetPrincipalQuota]::Maximum storage: {quota.StorageMaximum}; Used storage: {quota.StorageUsage}");

// restore document
await client.RestoreRecycledDocumentAsync(getAllRecycledDocuments.Items[0].Id, Principal.ForSystem(Guid.Parse("b06d4b7c-012b-4eb6-8acf-ee5dd6246222")));
Console.WriteLine($"[RestoreDocument]::Restored document with Id: {getAllRecycledDocuments.Items[0].Id}");