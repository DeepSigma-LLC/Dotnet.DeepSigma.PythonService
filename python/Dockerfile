# syntax=docker/dockerfile:1.7

FROM python:3.13-slim AS builder
WORKDIR /app
COPY pyproject.toml requirements.txt ./
COPY src/ src/
RUN pip install --no-cache-dir --prefix=/install -r requirements.txt

FROM python:3.13-slim
WORKDIR /app
COPY --from=builder /install /usr/local
COPY src/ src/
COPY implementations/ implementations/
COPY server.py ./
RUN useradd -m -u 1000 app && chown -R app /app
USER app
EXPOSE 8000
ENV PYTHONUNBUFFERED=1 PYSERVICE_HOST=0.0.0.0 PYSERVICE_PORT=8000
CMD ["uvicorn", "server:app", "--host", "0.0.0.0", "--port", "8000"]
