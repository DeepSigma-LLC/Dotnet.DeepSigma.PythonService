using DeepSigma.DataAccess.Http;
using Microsoft.Extensions.Options;

namespace DeepSigma.PythonService;

public abstract class PythonServiceClient
{
    protected HttpApi Http { get; }

    protected PythonServiceOptions Options { get; }

    protected PythonServiceClient(HttpApi http, IOptions<PythonServiceOptions> options)
    {
        Http = http;
        Options = options.Value;
    }

    protected Task<TResp?> PostAsync<TReq, TResp>(string path, TReq body, CancellationToken cancellationToken = default)
        => Http.PostJsonAsync<TReq, TResp>(path, body, Options.RequestTimeoutSeconds, cancellationToken: cancellationToken);

    protected Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken = default)
        => Http.GetDataFromUrlAsync<T>(path, Options.RequestTimeoutSeconds, cancellationToken: cancellationToken);
}
