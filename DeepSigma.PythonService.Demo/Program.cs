using DeepSigma.PythonService;
using DeepSigma.PythonService.Demo;
using DeepSigma.PythonService.Demo.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

string baseUrl = builder.Configuration[PythonApiResource.ConfigKey]
                 ?? builder.Configuration["PythonService:BaseUrl"]
                 ?? "http://127.0.0.1:8000";

builder.Services
    .AddPythonService(opts => opts.BaseUrl = baseUrl)
    .AddClient<HealthClient>()
    .AddClient<DemoEchoClient>()
    .AddClient<DemoIrisClient>();

using IHost host = builder.Build();

HealthClient health = host.Services.GetRequiredService<HealthClient>();
DemoEchoClient echo = host.Services.GetRequiredService<DemoEchoClient>();
DemoIrisClient iris = host.Services.GetRequiredService<DemoIrisClient>();

Console.WriteLine($"Python service at: {baseUrl}");

var healthResult = await health.GetHealthAsync();
Console.WriteLine($"[health] status={healthResult?.Status} service={healthResult?.Service}");

var echoResult = await echo.EchoAsync(new EchoRequest("hello from .NET"));
Console.WriteLine($"[echo]   echoed=\"{echoResult?.Echoed}\" length={echoResult?.Length}");

var irisResult = await iris.PredictAsync(new IrisFeatures([5.1, 3.5, 1.4, 0.2]));
Console.WriteLine($"[iris]   class={irisResult?.ClassName} (index {irisResult?.ClassIndex})");
