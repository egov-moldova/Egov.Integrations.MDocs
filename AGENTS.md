# AGENTS.md

This project is built and maintained with the help of AI agents. This file serves as a guide for agents to understand the repository structure and development practices.

## Agent Guidelines

1.  **Code Consistency**: Always follow the existing code style. Use `dotnet format` or IDE-equivalent tools to maintain consistency.
2.  **Documentation**: Every public class and method must have XML documentation. Ensure `GenerateDocumentationFile` is set to `true` in the project file.
3.  **Tests**: New features must be accompanied by tests in `Egov.Integrations.MDocs.Tests`.
4.  **NuGet Metadata**: Maintain up-to-date NuGet metadata in `Egov.Integrations.MDocs.csproj`.
5.  **Clean Code**: Avoid unnecessary dependencies and keep the implementation minimal and focused.

## Repository Layout

*   `src/Egov.Integrations.MDocs`: Main library project.
*   `src/Egov.Integrations.MDocs.Tests`: Unit and integration tests.
*   `src/Test`: Sample console application for manual testing.
*   `.github/workflows`: CI/CD pipelines.
*   `files/`: Assets like icons and sample documents.
