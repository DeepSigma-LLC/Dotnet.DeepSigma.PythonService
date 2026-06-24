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
}
