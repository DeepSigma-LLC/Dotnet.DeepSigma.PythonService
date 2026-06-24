using Microsoft.Extensions.DependencyInjection;

namespace DeepSigma.PythonService;

public interface IPythonServiceBuilder
{
    IServiceCollection Services { get; }

    IHttpClientBuilder HttpClientBuilder { get; }

    IPythonServiceBuilder AddClient<TClient>() where TClient : PythonServiceClient;
}

internal sealed class PythonServiceBuilder(IServiceCollection services, IHttpClientBuilder httpClientBuilder)
    : IPythonServiceBuilder
{
    public IServiceCollection Services { get; } = services;

    public IHttpClientBuilder HttpClientBuilder { get; } = httpClientBuilder;

    public IPythonServiceBuilder AddClient<TClient>() where TClient : PythonServiceClient
    {
        Services.AddTransient<TClient>();
        return this;
    }
}
