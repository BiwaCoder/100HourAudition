"""Lightweight, append-only JSONL event log for PDCA / balance tuning.

Writes to a file OUTSIDE the rsynced project directory (matching the
~/openai.env convention) so it survives redeploys. Pure local file I/O —
no database, no network call — negligible overhead per request.

Never lets a logging failure break the actual request.
"""
import json
import os
import time
from pathlib import Path
from typing import Any, Optional

_LOG_PATH = Path(os.environ.get(
    "EVENT_LOG_PATH", os.path.expanduser("~/hundredhour-api-data/events.jsonl")
))
_MAX_FIELD_CHARS = 2000


def _clip(value: Any) -> Any:
    if isinstance(value, str) and len(value) > _MAX_FIELD_CHARS:
        return value[:_MAX_FIELD_CHARS] + "…(省略)"
    return value


def log_event(kind: str, request: Optional[object] = None, **fields: Any) -> None:
    try:
        record = {"ts": round(time.time(), 3), "kind": kind}
        if request is not None:
            client = getattr(request, "client", None)
            record["ip"] = client.host if client else None
            headers = getattr(request, "headers", None)
            if headers is not None:
                record["client_id"] = headers.get("x-client-id")
        record.update({k: _clip(v) for k, v in fields.items()})
        _LOG_PATH.parent.mkdir(parents=True, exist_ok=True)
        with _LOG_PATH.open("a", encoding="utf-8") as f:
            f.write(json.dumps(record, ensure_ascii=False) + "\n")
    except Exception:
        pass
