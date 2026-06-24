from collections.abc import Iterable

from fastapi import APIRouter, FastAPI
from fastapi.middleware.cors import CORSMiddleware

from deepsigma_pyservice.routers.health import build_health_router
from deepsigma_pyservice.settings import AppSettings


def create_app(
    routers: Iterable[APIRouter] | None = None,
    settings: AppSettings | None = None,
) -> FastAPI:
    settings = settings or AppSettings()
    app = FastAPI(title=settings.title)

    if settings.cors_origins:
        app.add_middleware(
            CORSMiddleware,
            allow_origins=settings.cors_origins,
            allow_methods=["*"],
            allow_headers=["*"],
        )

    app.include_router(build_health_router(settings))
    for router in routers or ():
        app.include_router(router)

    return app
