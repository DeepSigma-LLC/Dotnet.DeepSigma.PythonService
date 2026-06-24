from deepsigma_pyservice import AppSettings, create_app
from implementations.echo import router as echo_router
from implementations.ml_iris import router as iris_router

app = create_app(routers=[echo_router, iris_router])


if __name__ == "__main__":
    import uvicorn

    settings = AppSettings()
    uvicorn.run(
        "server:app",
        host=settings.host,
        port=settings.port,
        log_level=settings.log_level,
        reload=True,
    )
