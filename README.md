# Dotnet.DeepSigma.PythonService

A reusable template for the pattern **"Python service runs arbitrary functions behind FastAPI endpoints; a .NET library calls them."** ML inference is one example endpoint — the framework itself has no domain opinions, so the same template fits embeddings, document processing, agent calls, or any other Python-side function.

The project ships:

- A reusable **Python framework package** (`deepsigma_pyservice`) that builds the FastAPI app.
- A reusable **.NET client library** (`DeepSigma.PythonService.Client`) that gives consumers a typed base class for talking to the service.
- A **.NET Aspire AppHost** that launches the FastAPI server and the demo together with one F5.
- Two **swappable example implementations**: an echo endpoint (non-ML) and a scikit-learn iris classifier (ML).

## Repository layout

```
Dotnet.DeepSigma.PythonService/
├── DeepSigma.PythonService/             # Reusable .NET client library (NuGet target)
│   ├── PythonServiceClient.cs           # Abstract base for derived typed clients
│   ├── PythonServiceOptions.cs          # BaseUrl + timeout
│   ├── HealthClient.cs                  # Built-in /health client
│   ├── Models/HealthResponse.cs
│   ├── ServiceCollectionExtensions.cs   # AddPythonServiceClient<TClient>(...)
│   └── DeepSigma.PythonService.slnx     # Visual Studio solution
│
├── DeepSigma.PythonService.Demo/        # Example consumer
│   ├── DemoEchoClient.cs                # Derived client, calls /echo
│   ├── DemoIrisClient.cs                # Derived client, calls /ml/iris/predict
│   ├── Models/{Echo,Iris}.cs
│   └── Program.cs
│
├── DeepSigma.PythonService.AppHost/     # Aspire orchestration
│   └── AppHost.cs                       # AddUvicornApp + demo WithReference
│
├── DeepSigma.PythonService.Test/        # xUnit tests for the .NET client
│
└── python/                              # All Python code
    ├── Dotnet.DeepSigma.PythonService.pyproj  # VS Python project (Solution Explorer)
    ├── pyproject.toml
    ├── requirements.txt
    ├── server.py                        # Entrypoint: wires routers -> create_app
    ├── src/deepsigma_pyservice/         # REUSABLE FRAMEWORK PACKAGE
    │   ├── app_factory.py               # create_app(routers, settings)
    │   ├── settings.py                  # AppSettings
    │   └── routers/health.py            # Built-in /health
    └── implementations/                 # SWAPPABLE — one APIRouter per file
        ├── echo.py
        └── ml_iris.py
```

## Design rules

The framework layers (`deepsigma_pyservice/` on Python, `DeepSigma.PythonService/` on .NET) impose **zero domain shape**. Anything ML-, predict-, or inference-specific lives in `implementations/` or in the Demo project. If a name in either framework references a specific function ("Predict", "Inference"), it doesn't belong there.

- **Python framework** deals only in `APIRouter`s — no protocols, no `predict()` contracts. An implementation is just a module that exposes an `APIRouter` with whatever endpoints and Pydantic models it wants.
- **.NET client library** ships an abstract base class plus DI plumbing. It does **not** wrap or re-expose HTTP verbs — the underlying `DeepSigma.DataAccess.Http.HttpApi` (v1.3.0+) already provides `PostJsonAsync`, `GetDataFromUrlAsync`, `PutJsonAsync`, `PatchJsonAsync`, `DeleteAsync`, plus a `SendAsync` escape hatch. Derived clients call those directly.
- **Aspire** lives only in the AppHost project. The reusable client library has zero Aspire references, so consumers can host the Python server however they want.

## Getting started

### Prerequisites

- .NET 10 SDK
- Python 3.11+
- Visual Studio with the "Python development" workload (only required for editing `.pyproj` in Solution Explorer)
- Aspire dashboard runs automatically when you launch the AppHost; no separate install needed

### One-time Python setup

```powershell
cd python
python -m venv .venv
.venv\Scripts\python.exe -m pip install -r requirements.txt
```

The `.venv/` folder is gitignored. Aspire auto-detects this venv when starting the FastAPI process.

### F5 from Visual Studio (recommended)

1. Open `DeepSigma.PythonService\DeepSigma.PythonService.slnx`.
2. Set **DeepSigma.PythonService.AppHost** as the startup project.
3. Press F5.

The Aspire dashboard opens with two resources: `python-api` (uvicorn + FastAPI) and `demo` (the .NET console app). The demo logs `[health]`, `[echo]`, and `[iris]` results to its console pane.

### Running pieces individually

```powershell
# Python only
cd python
.venv\Scripts\python.exe -m uvicorn server:app --port 8000

# .NET demo only (against a server you already started)
$env:PythonService__BaseUrl = "http://127.0.0.1:8000"
dotnet run --project DeepSigma.PythonService.Demo

# Aspire (CLI)
dotnet run --project DeepSigma.PythonService.AppHost

# Tests
dotnet test DeepSigma.PythonService.Test
```

## Adding a new endpoint

The whole point of the template — adding a new Python function (ML or otherwise) **does not touch the framework layers**.

### 1. Add the Python implementation

`python/implementations/documents.py`:

```python
from fastapi import APIRouter
from pydantic import BaseModel

router = APIRouter(prefix="/documents", tags=["documents"])


class SummarizeRequest(BaseModel):
    text: str


class SummarizeResponse(BaseModel):
    summary: str


@router.post("/summarize", response_model=SummarizeResponse)
def summarize(req: SummarizeRequest) -> SummarizeResponse:
    return SummarizeResponse(summary=req.text[:100])
```

### 2. Register the router

`python/server.py`:

```python
from implementations.documents import router as documents_router

app = create_app(routers=[echo_router, iris_router, documents_router])
```

### 3. Add the .NET typed client

`DeepSigma.PythonService.Demo/DemoDocumentsClient.cs`:

```csharp
public sealed class DemoDocumentsClient(HttpApi http, IOptions<PythonServiceOptions> opts)
    : PythonServiceClient(http, opts)
{
    public Task<SummarizeResponse?> SummarizeAsync(SummarizeRequest req, CancellationToken ct = default)
        => Http.PostJsonAsync<SummarizeRequest, SummarizeResponse>(
            "/documents/summarize", req, cancellationToken: ct);
}
```

DTOs need `[JsonPropertyName]` for snake_case matching:

```csharp
public sealed record SummarizeRequest([property: JsonPropertyName("text")] string Text);
public sealed record SummarizeResponse([property: JsonPropertyName("summary")] string Summary);
```

### 4. Register in DI

```csharp
builder.Services.AddPythonServiceClient<DemoDocumentsClient>(opts => opts.BaseUrl = baseUrl);
```

No edits to `deepsigma_pyservice/` or `DeepSigma.PythonService.Client`.

## Using the client library in your own app

Reference the NuGet package, derive your typed clients, register them:

```csharp
using DeepSigma.DataAccess.Http;
using DeepSigma.PythonService;
using Microsoft.Extensions.Options;

public sealed class MyClient(HttpApi http, IOptions<PythonServiceOptions> options)
    : PythonServiceClient(http, options)
{
    public Task<MyResponse?> DoThingAsync(MyRequest req, CancellationToken ct = default)
        => Http.PostJsonAsync<MyRequest, MyResponse>("/my/endpoint", req, cancellationToken: ct);
}

// Program.cs
services.AddPythonServiceClient<MyClient>(opts =>
{
    opts.BaseUrl = "http://your-python-host:8000";
    opts.RequestTimeoutSeconds = 30;
});
```

Configuring the underlying transport (retry, throttle, custom handlers) uses the `IHttpClientBuilder` returned by `AddPythonServiceClient`:

```csharp
services.AddPythonServiceClient<MyClient>(opts => opts.BaseUrl = "...")
        .AddRetryAfterPolicy()
        .AddMinIntervalThrottle(TimeSpan.FromMilliseconds(100));
```

## JSON conventions

Pydantic uses `snake_case` field names by default; `System.Text.Json` uses `PascalCase`. Bridge with `[JsonPropertyName]` on each DTO record property — see the examples above. This is verbose but explicit; the alternative (configuring a `JsonNamingPolicy.SnakeCaseLower` on the underlying serializer) lives in the `DeepSigma.DataAccess.Http` package.

## Dependencies

| Layer | Package | Version |
| --- | --- | --- |
| .NET client | `DeepSigma.DataAccess.Http` | 1.3.0 |
| .NET client | `Microsoft.Extensions.Options.ConfigurationExtensions` | 10.0.0 |
| Demo | `Microsoft.Extensions.Hosting` | 10.0.0 |
| AppHost | `Aspire.AppHost.Sdk` / `Aspire.Hosting.AppHost` / `Aspire.Hosting.Python` | 13.4.6 |
| Python framework | `fastapi`, `uvicorn[standard]`, `pydantic`, `pydantic-settings` | see `pyproject.toml` |
| Python ML example | `scikit-learn`, `numpy` | optional `[examples]` extra |

## License

MIT. See [LICENSE](LICENSE).
