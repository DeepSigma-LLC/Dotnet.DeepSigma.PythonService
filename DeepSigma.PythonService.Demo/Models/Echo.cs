using System.Text.Json.Serialization;

namespace DeepSigma.PythonService.Demo.Models;

public sealed record EchoRequest(
    [property: JsonPropertyName("text")] string Text);

public sealed record EchoResponse(
    [property: JsonPropertyName("echoed")] string Echoed,
    [property: JsonPropertyName("length")] int Length);
