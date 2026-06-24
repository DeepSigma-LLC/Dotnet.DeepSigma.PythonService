namespace DeepSigma.PythonService;

/// <summary>
/// Aspire resource-name and service-discovery config-keys for the Python API.
/// Shared by AppHost (resource registration) and consuming hosts (URL lookup).
/// Prefer <see cref="HttpsConfigKey"/> first — Aspire fronts resources with a TLS proxy by default,
/// even when the underlying process is plain HTTP.
/// Override <see cref="Name"/> by wrapping with your own constants if you use a different resource name.
/// </summary>
public static class PythonApiResource
{
    public const string Name = "python-api";

    public const string HttpConfigKey = "services:" + Name + ":http:0";

    public const string HttpsConfigKey = "services:" + Name + ":https:0";
}
