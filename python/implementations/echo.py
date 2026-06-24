from fastapi import APIRouter
from pydantic import BaseModel

router = APIRouter(prefix="/echo", tags=["echo"])


class EchoRequest(BaseModel):
    text: str


class EchoResponse(BaseModel):
    echoed: str
    length: int


@router.post("", response_model=EchoResponse)
def echo(request: EchoRequest) -> EchoResponse:
    return EchoResponse(echoed=request.text, length=len(request.text))
