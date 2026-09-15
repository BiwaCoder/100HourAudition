from typing import Optional

from pydantic import BaseModel, Field


class ChatRequest(BaseModel):
    message: str = Field(..., description="ユーザーからのメッセージ")
    system_message: Optional[str] = Field(None, description="システムプロンプト(省略時はデフォルトを使用)")


class ChatResponse(BaseModel):
    success: bool
    response: Optional[str] = None
    error: Optional[str] = None
