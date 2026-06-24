def test_health_returns_ok(client):
    response = client.get("/health")

    assert response.status_code == 200
    assert response.json() == {"status": "ok", "service": "deepsigma-pyservice"}


def test_health_reflects_service_name_setting():
    from deepsigma_pyservice import AppSettings, create_app
    from fastapi.testclient import TestClient

    app = create_app(settings=AppSettings(service_name="custom-name"))
    with TestClient(app) as c:
        body = c.get("/health").json()

    assert body["service"] == "custom-name"
