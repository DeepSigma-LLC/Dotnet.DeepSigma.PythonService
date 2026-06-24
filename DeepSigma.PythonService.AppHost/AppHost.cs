using DeepSigma.PythonService;

var builder = DistributedApplication.CreateBuilder(args);

var pythonApi = builder.AddUvicornApp(
        name: PythonApiResource.Name,
        appDirectory: "../python",
        app: "server:app")
    .WithHttpEndpoint(env: "PORT")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.DeepSigma_PythonService_Demo>("demo")
    .WithReference(pythonApi)
    .WaitFor(pythonApi);

builder.Build().Run();
