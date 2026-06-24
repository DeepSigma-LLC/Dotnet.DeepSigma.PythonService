using System.Text.Json.Serialization;

namespace DeepSigma.PythonService.Models;

public sealed record HealthResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("service")] string Service);
