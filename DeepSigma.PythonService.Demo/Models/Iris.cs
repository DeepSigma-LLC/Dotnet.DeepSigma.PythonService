using System.Text.Json.Serialization;

namespace DeepSigma.PythonService.Demo.Models;

public sealed record IrisFeatures(
    [property: JsonPropertyName("features")] double[] Features);

public sealed record IrisResult(
    [property: JsonPropertyName("class_index")] int ClassIndex,
    [property: JsonPropertyName("class_name")] string ClassName,
    [property: JsonPropertyName("probabilities")] Dictionary<string, double> Probabilities);
