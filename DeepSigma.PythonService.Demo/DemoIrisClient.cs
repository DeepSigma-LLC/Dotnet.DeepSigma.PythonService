using DeepSigma.DataAccess.Http;
using DeepSigma.PythonService;
using DeepSigma.PythonService.Demo.Models;
using Microsoft.Extensions.Options;

namespace DeepSigma.PythonService.Demo;

public sealed class DemoIrisClient(HttpApi http, IOptions<PythonServiceOptions> options)
    : PythonServiceClient(http, options)
{
    public Task<IrisResult?> PredictAsync(IrisFeatures features, CancellationToken cancellationToken = default)
        => Http.PostJsonAsync<IrisFeatures, IrisResult>(
            "/ml/iris/predict",
            features,
            timeoutInSeconds: Options.RequestTimeoutSeconds,
            cancellationToken: cancellationToken);
}
