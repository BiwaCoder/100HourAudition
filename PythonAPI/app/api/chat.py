from fastapi import APIRouter, Depends, Request

from app.config import settings
from app.llm.LLMInterface import LLMInterface
from app.schemas.chat import ChatRequest, ChatResponse
from app.security import require_signed_request
from app.telemetry import log_event

router = APIRouter(prefix="/api", tags=["chat"])
llm = LLMInterface(model=settings.default_model)


@router.post("/chat", response_model=ChatResponse, dependencies=[Depends(require_signed_request)])
def chat(body: ChatRequest, http_request: Request = None) -> ChatResponse:
    system_message = body.system_message or settings.default_system_message
    try:
        reply = llm.generate_response(body.message, system_message)
        log_event("chat", http_request, message=body.message, system_message=system_message, response=reply)
        return ChatResponse(success=True, response=reply)
    except Exception as exc:
        log_event("chat_error", http_request, message=body.message, error=str(exc))
        return ChatResponse(success=False, error=str(exc))
