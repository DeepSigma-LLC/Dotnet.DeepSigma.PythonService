using System.Net;
using System.Text;
using DeepSigma.DataAccess.Http;
using DeepSigma.PythonService;
using DeepSigma.PythonService.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace DeepSigma.PythonService.Test;

public sealed class PythonServiceClientTests
{
    [Fact]
    public async Task HealthClient_DeserializesResponse()
    {
        var handler = new StubHandler((req, _) =>
        {
            Assert.Equal(HttpMethod.Get, req.Method);
            Assert.Equal("http://test.local/health", req.RequestUri!.ToString());
            return Json("""{"status":"ok","service":"deepsigma-pyservice"}""");
        });

        HealthClient client = BuildClient<HealthClient>(handler, "http://test.local");

        HealthResponse? result = await client.GetHealthAsync();

        Assert.NotNull(result);
        Assert.Equal("ok", result!.Status);
        Assert.Equal("deepsigma-pyservice", result.Service);
    }

    [Fact]
    public async Task DerivedClient_PostsJsonAndDeserializesResponse()
    {
        string? capturedBody = null;
        var handler = new StubHandler(async (req, ct) =>
        {
            Assert.Equal(HttpMethod.Post, req.Method);
            Assert.Equal("http://test.local/widgets", req.RequestUri!.ToString());
            capturedBody = await req.Content!.ReadAsStringAsync(ct);
            return Json("""{"id":42,"name":"widget"}""");
        });

        WidgetClient client = BuildClient<WidgetClient>(handler, "http://test.local");

        WidgetResponse? result = await client.CreateAsync(new WidgetRequest("widget"));

        Assert.NotNull(result);
        Assert.Equal(42, result!.Id);
        Assert.Equal("widget", result.Name);
        Assert.Contains("\"name\":\"widget\"", capturedBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BaseUrlIsAppliedToHttpClient()
    {
        var handler = new StubHandler((_, _) => Json("{}"));
        ServiceProvider sp = BuildProvider<WidgetClient>(handler, "http://configured.example.com:9999");

        HttpApi httpApi = sp.GetRequiredService<HttpApi>();
        var http = (HttpClient)typeof(HttpApi).GetField("_http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(httpApi)!;

        Assert.Equal(new Uri("http://configured.example.com:9999"), http.BaseAddress);
    }

    private static TClient BuildClient<TClient>(StubHandler handler, string baseUrl)
        where TClient : PythonServiceClient
        => BuildProvider<TClient>(handler, baseUrl).GetRequiredService<TClient>();

    private static ServiceProvider BuildProvider<TClient>(StubHandler handler, string baseUrl)
        where TClient : PythonServiceClient
    {
        var services = new ServiceCollection();
        services.AddPythonServiceClient<TClient>(opts => opts.BaseUrl = baseUrl)
                .ConfigurePrimaryHttpMessageHandler(() => handler);
        return services.BuildServiceProvider();
    }

    private static HttpResponseMessage Json(string body) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(body, Encoding.UTF8, "application/json"),
    };

    private sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handle) : HttpMessageHandler
    {
        public StubHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> sync)
            : this((req, ct) => Task.FromResult(sync(req, ct))) { }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => handle(request, cancellationToken);
    }

    public sealed record WidgetRequest(string Name);
    public sealed record WidgetResponse(int Id, string Name);

    public sealed class WidgetClient(HttpApi http, IOptions<PythonServiceOptions> options)
        : PythonServiceClient(http, options)
    {
        public Task<WidgetResponse?> CreateAsync(WidgetRequest request, CancellationToken ct = default)
            => Http.PostJsonAsync<WidgetRequest, WidgetResponse>("/widgets", request, cancellationToken: ct);
    }
}
