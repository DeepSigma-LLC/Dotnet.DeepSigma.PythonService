namespace DeepSigma.PythonService;

public sealed class PythonServiceOptions
{
    public string BaseUrl { get; set; } = "http://127.0.0.1:8000";

    public int RequestTimeoutSeconds { get; set; } = 30;
}
