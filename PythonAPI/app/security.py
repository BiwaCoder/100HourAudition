"""Lightweight shared-secret request signing.

Mirrors the scheme already proven in RunaPhotoServer's EnsurePhotoApiKey
middleware: timestamp + nonce + HMAC-SHA256, with replay protection.
When API_HMAC_SECRET is unset (local development), verification is a no-op —
same policy as the PHP original.
"""
import hashlib
import hmac
import time
from typing import Optional

from fastapi import Header, HTTPException

from app.config import settings

_WINDOW_SECONDS = 300
_seen_nonces: dict = {}


def _prune(now: float) -> None:
    for nonce, seen_at in list(_seen_nonces.items()):
        if now - seen_at > _WINDOW_SECONDS:
            _seen_nonces.pop(nonce, None)


def _check(secret: str, timestamp: str, nonce: str, signature: str) -> Optional[str]:
    """Returns an error message, or None if the signature is valid and fresh."""
    now = time.time()
    _prune(now)
    try:
        ts = float(timestamp)
    except (TypeError, ValueError):
        return "タイムスタンプが不正です。"
    if abs(now - ts) > _WINDOW_SECONDS:
        return "タイムスタンプの有効期限が切れています。"
    if not nonce or nonce in _seen_nonces:
        return "このリクエストはすでに使用されています。"
    expected = hmac.new(secret.encode("utf-8"), f"{timestamp}.{nonce}".encode("utf-8"), hashlib.sha256).hexdigest()
    if not hmac.compare_digest(expected, signature or ""):
        return "署名が一致しません。"
    _seen_nonces[nonce] = now
    return None


def require_signed_request(
    x_auth_timestamp: Optional[str] = Header(default=None),
    x_auth_nonce: Optional[str] = Header(default=None),
    x_auth_signature: Optional[str] = Header(default=None),
) -> None:
    """FastAPI dependency for REST endpoints (X-Auth-* headers)."""
    secret = settings.api_hmac_secret
    if not secret:
        return
    if not (x_auth_timestamp and x_auth_nonce and x_auth_signature):
        raise HTTPException(status_code=401, detail="署名ヘッダーがありません。")
    error = _check(secret, x_auth_timestamp, x_auth_nonce, x_auth_signature)
    if error:
        raise HTTPException(status_code=401, detail=error)


def verify_query_signature(auth_ts: Optional[str], auth_nonce: Optional[str], auth_sig: Optional[str]) -> bool:
    """For the live-interview WebSocket, where custom headers are awkward across platforms."""
    secret = settings.api_hmac_secret
    if not secret:
        return True
    if not (auth_ts and auth_nonce and auth_sig):
        return False
    return _check(secret, auth_ts, auth_nonce, auth_sig) is None
