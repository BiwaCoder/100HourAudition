from typing import Optional

from pydantic import BaseModel


class PhotoIllustrationResponse(BaseModel):
    success: bool
    impression: Optional[str] = None
    illustration_base64: Optional[str] = None
    error: Optional[str] = None
