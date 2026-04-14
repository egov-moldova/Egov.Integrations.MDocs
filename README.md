# Egov.Integrations.MDocs

[![NuGet](https://img.shields.io/nuget/v/Egov.Integrations.MDocs.svg)](https://www.nuget.org/packages/Egov.Integrations.MDocs)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A high-performance .NET library for integrating with the MDocs document management service. It provides a robust client for uploading blobs, publishing documents, managing shares, and performing document transformations. Designed for services on the eGov platform, it leverages `Egov.Extensions.Configuration` for secure certificate-based authentication (mTLS).

---

## Table of Contents

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
  - [Dependency Injection (Recommended)](#dependency-injection-recommended)
  - [Blob Management](#blob-management)
  - [Document Publishing](#document-publishing)
  - [Sharing and Reservations](#sharing-and-reservations)
  - [Document Transformation](#document-transformation)
- [Error Handling](#error-handling)
- [Testing](#testing)
- [Contributing](#contributing)
- [Code of Conduct](#code-of-conduct)
- [AI Assistance](#ai-assistance)
- [License](#license)

---

## Features

- **Blob Management**: Upload large files with automatic chunking (support for Streams, File Paths, and Byte Arrays).
- **Document Publishing**: Create documents with owner-based permissions and metadata.
- **Sharing and Reservations**: Securely share documents with other principals or reserve shares with access codes.
- **Document Transformation**: Transform documents into various formats (e.g., PDF) using server-side templates.
- **Quota Monitoring**: Track storage usage and limits for principals.
- **Certificate-based Auth**: Seamless integration with `Egov.Extensions.Configuration` for mutual TLS (mTLS).
- **Async-first API**: Fully asynchronous methods for all service operations.
- **Built for .NET 10+**: Optimized for the latest .NET features and performance.

---

## Prerequisites

- .NET 10.0 or later
- A valid service certificate for MDocs (PFX or PEM format)
- Access to the MDocs service API
- `Egov.Extensions.Configuration` for certificate management

---

## Installation

Install the package from [NuGet](https://www.nuget.org/packages/Egov.Integrations.MDocs):

```shell
dotnet add package Egov.Integrations.MDocs
```

Or via the Package Manager Console:

```shell
Install-Package Egov.Integrations.MDocs
```

---

## Configuration

Add the following sections to your **appsettings.json**:

```json
{
  "MDocs": {
    "BaseAddress": "https://mdocs.api.example.com"
  },
  "Certificate": {
    "Path": "Files/Certificates/your-certificate.pfx",
    "Password": "your-certificate-password"
  }
}
```

The MDocs client automatically uses the certificate configured via `Egov.Extensions.Configuration`.

---

## Usage

### Dependency Injection (Recommended)

Register the certificate and the MDocs client in **Program.cs**:

```csharp
using Egov.Extensions.Configuration;
using Egov.Integrations.MDocs.Models;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register the system certificate (required for mTLS)
builder.Services.AddSystemCertificate(builder.Configuration.GetSection("Certificate"));

// Register the MDocs client
builder.Services.AddMDocsClient(builder.Configuration.GetSection("MDocs"));

var app = builder.Build();
```

### Blob Management

Upload files to MDocs. Large files are automatically handled using chunked uploads.

```csharp
public async Task<Guid> UploadFileAsync(IMDocsClient mdocsClient, string path)
{
    // Upload a PDF file
    Guid blobId = await mdocsClient.UploadBlobAsync(path, MDocsContentType.Pdf);
    return blobId;
}
```

### Document Publishing

Transform an uploaded blob into a formal document with permissions.

```csharp
public async Task PublishExampleAsync(IMDocsClient client, Guid blobId)
{
    var documents = new List<Document>
    {
        new Document
        {
            Name = "Annual Report 2025",
            Principal = Principal.ForPerson("1234567890123"),
            CreatedBy = Principal.ForSystem(Guid.Parse("b06d4b7c-012b-4eb6-8acf-ee5dd6246222")),
            Number = "AR-2025-001"
        }
    };

    var published = await client.PublishDocumentsAsync(blobId, documents);
    foreach (var doc in published)
    {
        Console.WriteLine($"Published: {doc.Name} with ID: {doc.Id}");
    }
}
```

### Sharing and Reservations

Reserve a share and then use it when publishing a document.

```csharp
public async Task ShareWithReservationAsync(IMDocsClient client, Guid blobId)
{
    // 1. Reserve a share (useful for generating QR codes/links before the file is ready)
    var reservation = await client.ReserveShareAsync(generateAccessCode: true);
    
    // 2. Publish with the reservation
    var document = new Document
    {
        Name = "Private Document",
        Principal = Principal.ForPerson("owner-idnp"),
        CreatedBy = Principal.ForSystem(Guid.Parse("b06d4b7c-012b-4eb6-8acf-ee5dd6246222")),
        Shares = new List<ShareRequest>
        {
            new ShareRequest 
            { 
                For = Principal.ForPerson("recipient-idnp"),
                Permission = Permission.Read,
                ReservedId = reservation.Id // Link to reservation
            }
        }
    };

    await client.PublishDocumentsAsync(blobId, new[] { document });
}
```

### Document Transformation

Convert documents using predefined templates.

```csharp
public async Task<Stream> TransformToPdfAsync(IMDocsClient client, Stream sourceJson)
{
    // Transform JSON data into a PDF using a specific document type template
    return await client.TransformDocumentAsync(
        sourceJson, 
        MDocsContentType.Json, 
        documentTypeCode: "OFFICIAL_LETTER", 
        format: TemplateContentType.Pdf
    );
}
```

---

## Error Handling

The client library throws `MDocsException` for service-level errors, providing details about the failure:

| Scenario | Exception |
|----------|-----------|
| Invalid API response (4xx, 5xx) | `MDocsException` (contains Title, Status, and Errors) |
| Certificate not configured | `InvalidOperationException` (during mTLS handshake) |
| Serialization issues | `JsonException` |
| Cancellation requested | `OperationCanceledException` |

---

## Testing

The solution includes a test project `Egov.Integrations.MDocs.Tests` built with [xUnit](https://xunit.net/).

### Running the tests

```shell
dotnet test src/Egov.Integrations.MDocs.Tests
```

---

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

---

## Code of Conduct

This project adheres to the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md).

---

## AI Assistance

This repository contains an [AGENTS.md](AGENTS.md) file with instructions and context for AI coding agents to assist in development.

---

## License

This project is licensed under the [MIT License](LICENSE).
