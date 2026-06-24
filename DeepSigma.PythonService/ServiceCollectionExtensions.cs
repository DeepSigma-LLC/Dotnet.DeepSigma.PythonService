using DeepSigma.DataAccess.Http;
using DeepSigma.PythonService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace -- intentional, surfaces wherever DI is in scope.
namespace Microsoft.Extensions.DependencyInjection;

public static class PythonServiceCollectionExtensions
{
    /// <summary>
    /// Configures <see cref="PythonServiceOptions"/> and registers <see cref="HttpApi"/> with its
    /// <c>BaseAddress</c> bound to <see cref="PythonServiceOptions.BaseUrl"/> and JSON options set to
    /// <see cref="HttpApi.SnakeCaseJsonOptions"/> (Python services use snake_case fields). Call
    /// <see cref="IPythonServiceBuilder.AddClient{TClient}"/> on the returned builder to register
    /// each <see cref="PythonServiceClient"/>-derived client that should share this configuration.
    /// </summary>
    public static IPythonServiceBuilder AddPythonService(
        this IServiceCollection services,
        Action<PythonServiceOptions> configure)
    {
        services.Configure(configure);

        IHttpClientBuilder httpClientBuilder = services.AddHttpClient(nameof(HttpApi), (sp, client) =>
        {
            PythonServiceOptions options = sp.GetRequiredService<IOptions<PythonServiceOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        services.AddTransient(sp =>
        {
            HttpClient http = sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(HttpApi));
            ILogger<HttpApi>? logger = sp.GetService<ILogger<HttpApi>>();
            return new HttpApi(http, logger, HttpApi.SnakeCaseJsonOptions);
        });

        return new PythonServiceBuilder(services, httpClientBuilder);
    }
}
