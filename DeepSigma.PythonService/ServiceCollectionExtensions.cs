using DeepSigma.DataAccess.Http;
using DeepSigma.PythonService;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace -- intentional, surfaces wherever DI is in scope.
namespace Microsoft.Extensions.DependencyInjection;

public static class PythonServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="PythonServiceOptions"/>, the underlying <see cref="HttpApi"/>
    /// (with its <c>BaseAddress</c> set to the configured Python service URL), and
    /// <typeparamref name="TClient"/> as a transient client deriving from
    /// <see cref="PythonServiceClient"/>.
    /// </summary>
    public static IHttpClientBuilder AddPythonServiceClient<TClient>(
        this IServiceCollection services,
        Action<PythonServiceOptions> configure)
        where TClient : PythonServiceClient
    {
        services.Configure(configure);

        IHttpClientBuilder builder = services.AddHttpClient<HttpApi>((sp, client) =>
        {
            PythonServiceOptions options = sp.GetRequiredService<IOptions<PythonServiceOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        services.AddTransient<TClient>();
        return builder;
    }
}
