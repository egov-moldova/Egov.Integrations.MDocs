# Contributing to Egov.Integrations.MDocs

Thank you for your interest in contributing! We welcome all contributions that help improve this library.

## How to Contribute

1.  **Report Bugs**: Use the GitHub issues tracker to report bugs.
2.  **Suggest Features**: Use the GitHub issues tracker to suggest new features.
3.  **Submit Pull Requests**:
    *   Fork the repository.
    *   Create a new branch for your changes.
    *   Ensure your code follows the existing style.
    *   Add tests for any new functionality.
    *   Ensure all tests pass.
    *   Submit a pull request.

## Development Workflow

### Prerequisites

*   .NET 10.0 SDK

### Build

```bash
dotnet build src/Egov.Integrations.MDocs.sln
```

### Run Tests

```bash
dotnet test src/Egov.Integrations.MDocs.sln
```

## Code Style

*   Use PascalCase for public members.
*   Use camelCase for private fields (with `_` prefix).
*   Ensure all public APIs have XML documentation comments.
*   Keep methods focused and concise.
