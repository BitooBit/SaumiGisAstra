from __future__ import annotations

from datetime import datetime, timedelta, timezone
from typing import Any
from uuid import uuid4

from jose import JWTError, jwt

from app.core.config import Settings


class TokenError(Exception):
    pass


def _now() -> datetime:
    return datetime.now(timezone.utc)


def create_access_token(
    settings: Settings,
    *,
    subject: str,
    extra: dict[str, Any] | None = None,
) -> str:
    now = _now()
    payload: dict[str, Any] = {
        "sub": subject,
        "type": "access",
        "iat": int(now.timestamp()),
        "exp": int(
            (now + timedelta(minutes=settings.access_token_expire_minutes)).timestamp()
        ),
        "jti": str(uuid4()),
    }
    if extra:
        payload.update(extra)
    return jwt.encode(
        payload, settings.jwt_secret_key, algorithm=settings.jwt_algorithm
    )


def create_refresh_token(settings: Settings, *, subject: str) -> str:
    now = _now()
    payload = {
        "sub": subject,
        "type": "refresh",
        "iat": int(now.timestamp()),
        "exp": int(
            (now + timedelta(days=settings.refresh_token_expire_days)).timestamp()
        ),
        "jti": str(uuid4()),
    }
    return jwt.encode(
        payload, settings.jwt_secret_key, algorithm=settings.jwt_algorithm
    )


def decode_token(settings: Settings, token: str) -> dict[str, Any]:
    try:
        return jwt.decode(
            token,
            settings.jwt_secret_key,
            algorithms=[settings.jwt_algorithm],
        )
    except JWTError as e:
        raise TokenError("invalid token") from e


def require_token_type(payload: dict[str, Any], expected: str) -> None:
    if payload.get("type") != expected:
        raise TokenError(f"expected {expected} token")
