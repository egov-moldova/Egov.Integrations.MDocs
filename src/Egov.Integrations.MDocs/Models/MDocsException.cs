using System.Net;

namespace Egov.Integrations.MDocs.Models;

/// <summary>
/// Represents an exception thrown by the MDocs API.
/// </summary>
public class MDocsException : HttpRequestException
{
    /// <summary>
    /// Creates a new instance of the <see cref="MDocsException"/> class.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public MDocsException(string message) 
        : base(message)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MDocsException"/> class.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="inner">Inner exception.</param>
    /// <param name="statusCode">HTTP status code.</param>
    public MDocsException(string? message, Exception? inner, HttpStatusCode? statusCode) 
        : base(message, inner, statusCode)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MDocsException"/> class.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="statusCode">HTTP status code.</param>
    public MDocsException(string? message, HttpStatusCode? statusCode) 
        : base(message, null, statusCode)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MDocsException"/> class.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="validationErrors">Validation errors returned by the API.</param>
    public MDocsException(string? message, Dictionary<string, string[]>? validationErrors) 
        : base($"{message}. Validation Errors: {FormatValidationErrors(validationErrors)}", null, HttpStatusCode.BadRequest)
    {
        ValidationErrors = validationErrors;
    }

    private static string FormatValidationErrors(Dictionary<string, string[]>? validationErrors)
    {
        if (validationErrors == null || validationErrors.Count == 0)
        {
            return string.Empty;
        }

        return string.Join("; ", validationErrors.Select(e => $"{e.Key}: {string.Join(", ", e.Value)}"));
    }

    /// <summary>
    /// Any validation errors returned by the API.
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}
