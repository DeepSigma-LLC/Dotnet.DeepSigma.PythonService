namespace DeepSigma.PythonService.Demo.Models;

public sealed record EchoRequest(string Text);

public sealed record EchoResponse(string Echoed, int Length);
