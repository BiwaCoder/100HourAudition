import os
from dataclasses import dataclass


@dataclass(frozen=True)
class Settings:
    openai_api_key: str = os.environ.get("OPENAI_API_KEY", "")
    host: str = os.environ.get("PYTHONAPI_HOST", "127.0.0.1")
    port: int = int(os.environ.get("PYTHONAPI_PORT", "8001"))
    default_model: str = os.environ.get("PYTHONAPI_MODEL", "gpt-4o-mini")
    default_system_message: str = os.environ.get(
        "PYTHONAPI_SYSTEM_MESSAGE", "あなたは親切なアシスタントです。"
    )
    # 未設定(ローカル開発など)なら署名検証は無効。公開デプロイ時のみ設定する。
    api_hmac_secret: str = os.environ.get("API_HMAC_SECRET", "")


settings = Settings()
