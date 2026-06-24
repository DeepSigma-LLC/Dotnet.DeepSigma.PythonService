namespace DeepSigma.PythonService.Demo.Models;

public sealed record IrisFeatures(double[] Features);

public sealed record IrisResult(
    int ClassIndex,
    string ClassName,
    Dictionary<string, double> Probabilities);
