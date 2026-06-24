def test_echo_roundtrips_text_and_reports_length(client):
    response = client.post("/echo", json={"text": "hello"})

    assert response.status_code == 200
    assert response.json() == {"echoed": "hello", "length": 5}


def test_echo_rejects_missing_text(client):
    response = client.post("/echo", json={})

    assert response.status_code == 422
