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
        Assert.Contains("\"Name\":\"widget\"", capturedBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BaseUrlIsAppliedToHttpClient()
    {
        var handler = new StubHandler((_, _) => Json("{}"));
        ServiceProvider sp = BuildProvider(handler, "http://configured.example.com:9999", b => b.AddClient<WidgetClient>());

        HttpApi httpApi = sp.GetRequiredService<HttpApi>();
        HttpClient http = HttpClientOf(httpApi);

        Assert.Equal(new Uri("http://configured.example.com:9999"), http.BaseAddress);
    }

    [Fact]
    public void MultipleClients_ShareSameHttpApiInstanceAndBaseAddress()
    {
        var handler = new StubHandler((_, _) => Json("{}"));
        ServiceProvider sp = BuildProvider(handler, "http://shared.local",
            b => b.AddClient<WidgetClient>().AddClient<HealthClient>());

        var w = sp.GetRequiredService<WidgetClient>();
        var h = sp.GetRequiredService<HealthClient>();

        HttpClient wHttp = HttpClientOf(HttpApiOf(w));
        HttpClient hHttp = HttpClientOf(HttpApiOf(h));

        Assert.Equal(new Uri("http://shared.local"), wHttp.BaseAddress);
        Assert.Equal(new Uri("http://shared.local"), hHttp.BaseAddress);
    }

    private static TClient BuildClient<TClient>(StubHandler handler, string baseUrl)
        where TClient : PythonServiceClient
        => BuildProvider(handler, baseUrl, b => b.AddClient<TClient>()).GetRequiredService<TClient>();

    private static ServiceProvider BuildProvider(StubHandler handler, string baseUrl, Action<IPythonServiceBuilder> configure)
    {
        var services = new ServiceCollection();
        IPythonServiceBuilder builder = services.AddPythonService(opts => opts.BaseUrl = baseUrl);
        builder.HttpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => handler);
        configure(builder);
        return services.BuildServiceProvider();
    }

    private static HttpApi HttpApiOf(PythonServiceClient client)
        => (HttpApi)typeof(PythonServiceClient)
            .GetProperty("Http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(client)!;

    private static HttpClient HttpClientOf(HttpApi api)
        => (HttpClient)typeof(HttpApi)
            .GetField("_http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(api)!;

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
            => PostAsync<WidgetRequest, WidgetResponse>("/widgets", request, ct);
    }
}
