using DeepSigma.DataAccess.Http;
using DeepSigma.PythonService.Models;
using Microsoft.Extensions.Options;

namespace DeepSigma.PythonService;

public sealed class HealthClient(HttpApi http, IOptions<PythonServiceOptions> options)
    : PythonServiceClient(http, options)
{
    public Task<HealthResponse?> GetHealthAsync(CancellationToken cancellationToken = default)
        => Http.GetDataFromUrlAsync<HealthResponse>(
            "/health",
            timeoutInSeconds: Options.RequestTimeoutSeconds,
            cancellationToken: cancellationToken);
}
