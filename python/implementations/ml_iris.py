from functools import lru_cache

from fastapi import APIRouter
from pydantic import BaseModel, Field
from sklearn.datasets import load_iris
from sklearn.linear_model import LogisticRegression

router = APIRouter(prefix="/ml/iris", tags=["ml", "iris"])


class IrisFeatures(BaseModel):
    features: list[float] = Field(..., min_length=4, max_length=4, description="[sepal_len, sepal_wid, petal_len, petal_wid]")


class IrisResult(BaseModel):
    class_index: int
    class_name: str
    probabilities: dict[str, float]


@lru_cache(maxsize=1)
def _model() -> tuple[LogisticRegression, list[str]]:
    data = load_iris()
    model = LogisticRegression(max_iter=1000).fit(data.data, data.target)
    return model, list(data.target_names)


@router.post("/predict", response_model=IrisResult)
def predict(payload: IrisFeatures) -> IrisResult:
    model, target_names = _model()
    proba = model.predict_proba([payload.features])[0]
    class_index = int(proba.argmax())
    return IrisResult(
        class_index=class_index,
        class_name=target_names[class_index],
        probabilities={name: float(p) for name, p in zip(target_names, proba)},
    )
