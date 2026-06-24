from deepsigma_pyservice import create_app
from implementations.echo import router as echo_router
from implementations.ml_iris import router as iris_router

app = create_app(routers=[echo_router, iris_router])


if __name__ == "__main__":
    import uvicorn

    uvicorn.run("server:app", host="127.0.0.1", port=8000, reload=True)
