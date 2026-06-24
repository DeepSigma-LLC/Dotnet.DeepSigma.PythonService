def test_iris_predicts_setosa_for_classic_features(client):
    response = client.post("/ml/iris/predict", json={"features": [5.1, 3.5, 1.4, 0.2]})

    assert response.status_code == 200
    body = response.json()
    assert body["class_name"] == "setosa"
    assert body["class_index"] == 0
    assert set(body["probabilities"]) == {"setosa", "versicolor", "virginica"}
    assert abs(sum(body["probabilities"].values()) - 1.0) < 1e-6


def test_iris_rejects_wrong_feature_count(client):
    response = client.post("/ml/iris/predict", json={"features": [5.1, 3.5, 1.4]})

    assert response.status_code == 422
