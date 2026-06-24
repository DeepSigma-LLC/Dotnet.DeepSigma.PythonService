namespace DeepSigma.PythonService;

/// <summary>
/// Aspire resource-name and service-discovery config-key for the Python API.
/// Shared by AppHost (resource registration) and consuming hosts (URL lookup).
/// Override <see cref="Name"/> by wrapping with your own constants if you use a different resource name.
/// </summary>
public static class PythonApiResource
{
    public const string Name = "python-api";

    public const string ConfigKey = "services:" + Name + ":http:0";
}
