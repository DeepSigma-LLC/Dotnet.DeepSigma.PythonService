using DeepSigma.DataAccess.Http;
using DeepSigma.PythonService;
using DeepSigma.PythonService.Demo.Models;
using Microsoft.Extensions.Options;

namespace DeepSigma.PythonService.Demo;

public sealed class DemoEchoClient(HttpApi http, IOptions<PythonServiceOptions> options)
    : PythonServiceClient(http, options)
{
    public Task<EchoResponse?> EchoAsync(EchoRequest request, CancellationToken cancellationToken = default)
        => Http.PostJsonAsync<EchoRequest, EchoResponse>(
            "/echo",
            request,
            timeoutInSeconds: Options.RequestTimeoutSeconds,
            cancellationToken: cancellationToken);
}
