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
│   ├── PythonServiceClient.cs           # Abstract base + PostAsync/GetAsync helpers
│   ├── PythonServiceOptions.cs          # BaseUrl + timeout
│   ├── PythonServiceBuilder.cs          # IPythonServiceBuilder + AddClient<T>
│   ├── HealthClient.cs                  # Built-in /health client
│   ├── PythonApiResource.cs             # Aspire resource-name + config-key constants
│   ├── Models/HealthResponse.cs
│   ├── ServiceCollectionExtensions.cs   # AddPythonService(...).AddClient<T>()
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
    │   ├── settings.py                  # AppSettings (host, port, service_name, ...)
    │   └── routers/health.py            # Built-in /health
    ├── implementations/                 # SWAPPABLE — one APIRouter per file
    │   ├── echo.py
    │   └── ml_iris.py
    └── tests/                           # pytest + FastAPI TestClient
```

## Design rules

The framework layers (`deepsigma_pyservice/` on Python, `DeepSigma.PythonService/` on .NET) impose **zero domain shape**. Anything ML-, predict-, or inference-specific lives in `implementations/` or in the Demo project. If a name in either framework references a specific function ("Predict", "Inference"), it doesn't belong there.

- **Python framework** deals only in `APIRouter`s — no protocols, no `predict()` contracts. An implementation is just a module that exposes an `APIRouter` with whatever endpoints and Pydantic models it wants.
- **.NET client library** ships an abstract base class with thin `PostAsync<TReq,TResp>` / `GetAsync<T>` helpers that bake in the configured timeout. The underlying `DeepSigma.DataAccess.Http.HttpApi` (v1.4.0+) — `PutJsonAsync`, `PatchJsonAsync`, `DeleteAsync`, `SendAsync` (escape hatch) — is exposed as `protected Http { get; }` for cases the two helpers don't cover.
- **Snake_case JSON is automatic.** The library wires `HttpApi.SnakeCaseJsonOptions` so PascalCase C# property names round-trip to/from Python's snake_case fields with no `[JsonPropertyName]` attributes on DTOs.
- **Aspire** lives only in the AppHost project. The reusable client library has zero Aspire references, so consumers can host the Python server however they want.

## Setup — pick a path

Two ways to consume this project, depending on whether you're starting fresh or want the full scaffold.

### Path A — Use this repo as a template

Clone the repo, or click **Use this template** on GitHub. You get the full scaffold: framework, sample implementations (echo + iris), AppHost wiring, tests, Dockerfile. Edit `python/implementations/` and `python/server.py` for your endpoints; the framework in `python/src/deepsigma_pyservice/` is updated in-tree.

Prerequisites: .NET 10 SDK, Python 3.11+, Visual Studio with the "Python development" workload (only required for editing `.pyproj` in Solution Explorer).

One-time Python setup:

```powershell
cd python
python -m venv .venv
.venv\Scripts\python.exe -m pip install -r requirements.txt
```

The `.venv/` folder is gitignored. Aspire auto-detects this venv when starting the FastAPI process.

#### F5 from Visual Studio (recommended)

1. Open `DeepSigma.PythonService\DeepSigma.PythonService.slnx`.
2. Set **DeepSigma.PythonService.AppHost** as the startup project.
3. Pick the **http** launch profile from the dropdown next to the green play button (the `https` profile requires a trusted dev cert; the `http` profile sets `ASPIRE_ALLOW_UNSECURED_TRANSPORT=true`).
4. Press F5.

The Aspire dashboard opens with two resources: `python-api` (uvicorn + FastAPI) and `demo` (the .NET console app). The demo's output (`[health]`, `[echo]`, `[iris]`) appears in the dashboard's **demo** resource log.

#### Running pieces individually

```powershell
# Python only
cd python
.venv\Scripts\python.exe -m uvicorn server:app --port 8000

# .NET demo only (against a server you already started)
$env:PythonService__BaseUrl = "http://127.0.0.1:8000"
dotnet run --project DeepSigma.PythonService.Demo

# Aspire (CLI)
dotnet run --project DeepSigma.PythonService.AppHost --launch-profile http

# Python tests
cd python
.venv\Scripts\python.exe -m pytest

# .NET tests
dotnet test DeepSigma.PythonService.Test
```

### Path B — Depend on the published packages

For a brand-new project not derived from this repo, install both packages and write your own glue:

```powershell
# Python side
pip install deepsigma-pyservice

# .NET side
dotnet add package DeepSigma.PythonService.Client
```

Minimal `server.py`:

```python
from deepsigma_pyservice import create_app
from my_app.endpoints import router

app = create_app(routers=[router])
```

Run with `uvicorn server:app --host 0.0.0.0 --port 8000`. The .NET client (`DeepSigma.PythonService.Client`) connects to it identically — see [Using the client library in your own app](#using-the-client-library-in-your-own-app) below.

Path B skips the in-repo AppHost / Demo / Dockerfile / tests — you bring your own. Path A is the faster start; Path B is the cleaner long-term integration into an existing codebase.

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
        => PostAsync<SummarizeRequest, SummarizeResponse>("/documents/summarize", req, ct);
}

public sealed record SummarizeRequest(string Text);
public sealed record SummarizeResponse(string Summary);
```

PascalCase property names round-trip to Python's snake_case fields automatically — no attributes needed.

### 4. Register in DI

```csharp
builder.Services
    .AddPythonService(opts => opts.BaseUrl = baseUrl)
    .AddClient<DemoDocumentsClient>();
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
        => PostAsync<MyRequest, MyResponse>("/my/endpoint", req, ct);
}

// Program.cs
services
    .AddPythonService(opts =>
    {
        opts.BaseUrl = "http://your-python-host:8000";
        opts.RequestTimeoutSeconds = 30;
    })
    .AddClient<MyClient>();
```

Multiple derived clients registered through the same `AddPythonService(...).AddClient<...>()` chain share one configured `HttpApi` — base URL, timeout, retry policies, and JSON options are configured once.

Configuring the underlying transport (retry, throttle, custom handlers) uses the `IHttpClientBuilder` exposed on the builder:

```csharp
var pyBuilder = services.AddPythonService(opts => opts.BaseUrl = "...");
pyBuilder.HttpClientBuilder
    .AddRetryAfterPolicy()
    .AddMinIntervalThrottle(TimeSpan.FromMilliseconds(100));
pyBuilder.AddClient<MyClient>();
```

## JSON conventions

Pydantic uses `snake_case` field names by default; `System.Text.Json` uses `PascalCase`. The library wires `HttpApi.SnakeCaseJsonOptions` (from `DeepSigma.DataAccess.Http` v1.4.0+) so PascalCase C# properties serialize as snake_case on the wire and are read case-insensitively — no `[JsonPropertyName]` attributes required on DTOs.

If you need a different policy for a specific endpoint, drop down to `Http.SendAsync(...)` with your own request, or rebuild `HttpApi` with custom options via `new HttpApi(client, logger, customOptions)`.

## Deployment

A FastAPI service is just an ASGI process — there's no single "right" deployment. The most portable artifact is the included Dockerfile, which works on any container host (Azure Container Apps, AWS ECS / Fargate, Cloud Run, Kubernetes, bare Docker on a VM).

### Docker (recommended)

```powershell
cd python
docker build -t deepsigma-pyservice .
docker run -p 8000:8000 deepsigma-pyservice
```

The image is multi-stage (builder + runtime), runs as a non-root user, and exposes port 8000. The in-repo `server.py` + `implementations/` are baked in — edit those before building, or template a new Dockerfile that copies your own `server.py` if you took Path B.

### Bare uvicorn (VM / systemd)

Install deps in a venv, then run uvicorn behind a reverse proxy (nginx / caddy):

```powershell
.venv\Scripts\python.exe -m uvicorn server:app --host 0.0.0.0 --port 8000 --workers 4
```

For a long-running deployment, wrap that command in a systemd unit (Linux) or a Windows Service.

Configure via env vars (all read by `AppSettings` in [`python/src/deepsigma_pyservice/settings.py`](python/src/deepsigma_pyservice/settings.py)):

| Env var | Default | Meaning |
| --- | --- | --- |
| `PYSERVICE_HOST` | `127.0.0.1` | Bind address |
| `PYSERVICE_PORT` | `8000` | Bind port |
| `PYSERVICE_LOG_LEVEL` | `info` | `debug` / `info` / `warning` / `error` |
| `PYSERVICE_CORS_ORIGINS` | `[]` | JSON list, e.g. `'["https://app.example.com"]'` |
| `PYSERVICE_TITLE` | `DeepSigma Python Service` | FastAPI OpenAPI title |
| `PYSERVICE_SERVICE_NAME` | `deepsigma-pyservice` | Identifier returned by `/health` |

### Aspire (local dev only)

The AppHost orchestrates uvicorn + .NET demo together for local development. **Don't use Aspire for production deployment** — Aspire's resource lifecycle is built around the developer F5 loop. Ship the Python side via Docker (or bare uvicorn) and host the .NET side independently.

## Publishing the framework

When you're ready to ship a new version of `deepsigma-pyservice`:

```powershell
cd python
# bump version in pyproject.toml first
.venv\Scripts\python.exe -m build              # produces dist/*.whl and dist/*.tar.gz
.venv\Scripts\python.exe -m twine upload dist/*   # to the feed configured in ~/.pypirc
```

Configure the target feed via `~/.pypirc` or `twine upload --repository <name>` — works for public PyPI, private PyPI servers (devpi, Gemfury, Azure Artifacts), or GitHub Releases. Both `build` and `twine` ship in the `[dev]` extra.

A CI workflow (GitHub Actions on tag push) is a natural follow-up but isn't wired up yet.

## Dependencies

| Layer | Package | Version |
| --- | --- | --- |
| .NET client | `DeepSigma.DataAccess.Http` | 1.4.0 |
| .NET client | `Microsoft.Extensions.Options.ConfigurationExtensions` | 10.0.0 |
| Demo | `Microsoft.Extensions.Hosting` | 10.0.0 |
| AppHost | `Aspire.AppHost.Sdk` / `Aspire.Hosting.AppHost` / `Aspire.Hosting.Python` | 13.4.6 |
| Python framework | `fastapi`, `uvicorn[standard]`, `pydantic`, `pydantic-settings` | see `pyproject.toml` |
| Python ML example | `scikit-learn`, `numpy` | optional `[examples]` extra |
| Python tests | `pytest`, `httpx2` | optional `[dev]` extra |

## License

MIT. See [LICENSE](LICENSE).
