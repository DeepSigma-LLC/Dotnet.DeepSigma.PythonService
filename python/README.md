# deepsigma-pyservice

Reusable FastAPI framework for exposing Python functions (ML inference, document processing, embeddings, agent calls, anything) behind HTTP endpoints that a .NET client can call. The framework imposes zero domain shape — you bring `APIRouter`s, it handles the wiring, health endpoint, settings, and CORS.

Designed to pair with [`DeepSigma.PythonService.Client`](https://www.nuget.org/packages/DeepSigma.PythonService.Client) on the .NET side, but works standalone with any HTTP client.

## Install

```
pip install deepsigma-pyservice
```

## Quickstart

Create a router for whatever your function is:

```python
# my_app/endpoints.py
from fastapi import APIRouter
from pydantic import BaseModel

router = APIRouter(prefix="/echo", tags=["echo"])


class EchoRequest(BaseModel):
    text: str


class EchoResponse(BaseModel):
    echoed: str
    length: int


@router.post("", response_model=EchoResponse)
def echo(req: EchoRequest) -> EchoResponse:
    return EchoResponse(echoed=req.text, length=len(req.text))
```

Wire it into a FastAPI app:

```python
# server.py
from deepsigma_pyservice import create_app
from my_app.endpoints import router

app = create_app(routers=[router])
```

Run it:

```
uvicorn server:app --host 0.0.0.0 --port 8000
```

`/health` is registered automatically. Hit `/echo` with `curl -X POST localhost:8000/echo -H 'content-type: application/json' -d '{"text":"hi"}'`.

## Configuration

`AppSettings` (read from `PYSERVICE_*` env vars or a `.env` file):

| Setting | Env var | Default |
| --- | --- | --- |
| `host` | `PYSERVICE_HOST` | `127.0.0.1` |
| `port` | `PYSERVICE_PORT` | `8000` |
| `log_level` | `PYSERVICE_LOG_LEVEL` | `info` |
| `cors_origins` | `PYSERVICE_CORS_ORIGINS` | `[]` |
| `title` | `PYSERVICE_TITLE` | `DeepSigma Python Service` |
| `service_name` | `PYSERVICE_SERVICE_NAME` | `deepsigma-pyservice` |

Pass an explicit `AppSettings` to `create_app` if you don't want env-based config:

```python
from deepsigma_pyservice import AppSettings, create_app

app = create_app(routers=[router], settings=AppSettings(service_name="my-service"))
```

## Full template

For a complete scaffold with sample implementations, AppHost wiring for .NET Aspire, tests, and a Dockerfile, see the [reference repository](https://github.com/DeepSigma-LLC/Dotnet.DeepSigma.PythonService). Clone or "Use this template" to start from a working setup.

## License

MIT.
