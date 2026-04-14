using System.Net;
using System.Text;
using System.Text.Json;

namespace Egov.Integrations.MDocs.Tests;

public abstract class MDocsClientTestsBase
{
    protected static readonly JsonSerializerOptions WebJsonSerializerOptions = new(JsonSerializerDefaults.Web);

    internal (MDocsClient Client, MockHttpMessageHandler HttpHandler) CreateClient(string baseAddress = "https://api.mdocs.test/")
    {
        var httpHandler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(httpHandler)
        {
            BaseAddress = new Uri(baseAddress)
        };
        var client = new MDocsClient(httpClient);
        return (client, httpHandler);
    }

    protected internal class MockHttpMessageHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = new();
        public Func<HttpRequestMessage, HttpResponseMessage>? ResponseFactory { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            if (ResponseFactory != null)
            {
                return Task.FromResult(ResponseFactory(request));
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }

        public void SetupResponse(HttpStatusCode statusCode, object? content = null, IDictionary<string, string>? headers = null)
        {
            ResponseFactory = _ =>
            {
                var response = new HttpResponseMessage(statusCode);
                if (content != null)
                {
                    var json = JsonSerializer.Serialize(content, WebJsonSerializerOptions);
                    response.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        if (!response.Headers.TryAddWithoutValidation(header.Key, header.Value))
                        {
                            response.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                }

                return response;
            };
        }
    }
}
