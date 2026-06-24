var builder = DistributedApplication.CreateBuilder(args);

var pythonApi = builder.AddUvicornApp(
        name: "python-api",
        appDirectory: "../python",
        app: "server:app")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.DeepSigma_PythonService_Demo>("demo")
    .WithReference(pythonApi)
    .WaitFor(pythonApi);

builder.Build().Run();
