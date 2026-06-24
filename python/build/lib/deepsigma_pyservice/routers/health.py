from fastapi import APIRouter
from pydantic import BaseModel

from deepsigma_pyservice.settings import AppSettings


class HealthResponse(BaseModel):
    status: str
    service: str


def build_health_router(settings: AppSettings) -> APIRouter:
    router = APIRouter(tags=["health"])

    @router.get("/health", response_model=HealthResponse)
    def get_health() -> HealthResponse:
        return HealthResponse(status="ok", service=settings.service_name)

    return router
