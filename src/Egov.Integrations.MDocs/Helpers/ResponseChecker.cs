using System.Net.Http.Json;
using System.Text.Json;
using Egov.Integrations.MDocs.Models;
using Egov.Integrations.MDocs.Models.Internal;

namespace Egov.Integrations.MDocs.Helpers;

internal static class ResponseChecker
{
    private static readonly JsonSerializerOptions WebJsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public static MDocsException CreateExceptionFromBadResponse(this HttpResponseMessage response)
    {
        var httpErrorObject = response.Content.ReadAsStringAsync().Result;
        var deserializedErrorObject = JsonSerializer.Deserialize<ValidationErrorResponse>(httpErrorObject, WebJsonSerializerOptions);
        
        switch (response.StatusCode)
        {
            case System.Net.HttpStatusCode.BadRequest:

                return new MDocsException(deserializedErrorObject?.Title, deserializedErrorObject?.Errors);
            case System.Net.HttpStatusCode.Unauthorized:
                return new MDocsException("You are not authorized!", response.StatusCode);
            case System.Net.HttpStatusCode.Forbidden:
                return new MDocsException($"You don't have access! Reason: {deserializedErrorObject?.Errors}", response.StatusCode);
            case System.Net.HttpStatusCode.NotFound:
                return new MDocsException("Resource not found!", response.StatusCode);
            case System.Net.HttpStatusCode.InternalServerError:
                return new MDocsException("Server error!", response.StatusCode);
            default:
                return new MDocsException($"Failed to evaluate: {response.StatusCode}", response.StatusCode);
        }
    }

    public static void CheckMDocsResponse(this HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw response.CreateExceptionFromBadResponse();
        }
    }

    public static async Task<T> HandleMDocsResponse<T>(this HttpResponseMessage response, CancellationToken cancellationToken)
    {
        response.CheckMDocsResponse();

        var result = await response.Content.ReadFromJsonAsync<T>(WebJsonSerializerOptions, cancellationToken);
        return result ?? throw new MDocsException("MDocs returned unexpected response");
    }

    public static async Task<PagedItems<T>> HandleMDocsPagedResponse<T>(this HttpResponseMessage response, CancellationToken cancellationToken)
    {
        response.CheckMDocsResponse();

        var paginationString = response.Headers.GetValues("X-Pagination").FirstOrDefault();
        if (string.IsNullOrWhiteSpace(paginationString)) throw new MDocsException("MDocs returned no X-Pagination header");
        var pagination = JsonSerializer.Deserialize<Pagination>(paginationString, WebJsonSerializerOptions);
        if (pagination == null) throw new MDocsException("MDocs returned empty pagination");

        var items = await response.Content.ReadFromJsonAsync<IReadOnlyList<T>>(WebJsonSerializerOptions, cancellationToken);
        if (items == null) throw new MDocsException("MDocs returned unexpected response");

        return new PagedItems<T>
        {
            Items = items,
            TotalCount = pagination.TotalCount,
            CurrentPage = pagination.CurrentPage,
            PageSize = pagination.PageSize
        };
    }
}