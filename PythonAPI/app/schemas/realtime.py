from typing import Optional, Literal

from pydantic import BaseModel


class RealtimeTokenResponse(BaseModel):
    success: bool
    client_secret: Optional[str] = None
    model: Optional[str] = None
    interviewer_name: Optional[str] = None
    error: Optional[str] = None


class BuildCharacterRequest(BaseModel):
    player_gender: Optional[Literal["male", "female"]] = None
    profile_mode: Literal["legacy", "interview"] = "legacy"
    voice_answer: str
    photo_impression: str = ""
    language: Literal["ja", "en"] = "ja"


class BuildCharacterResponse(BaseModel):
    success: bool
    character: Optional[dict] = None
    error: Optional[str] = None
