from __future__ import annotations

from typing import Optional

from pydantic import BaseModel, Field


class LoginRequest(BaseModel):
    """Как в exe DOA: Username / Password / Database (TNS)."""

    username: str = Field(..., min_length=1, max_length=128)
    password: str = Field(..., min_length=1, max_length=256)
    database: str = Field(
        ...,
        min_length=1,
        max_length=512,
        description="TNS alias или host:port/service",
    )


class TokenPair(BaseModel):
    access_token: str
    refresh_token: str
    token_type: str = "bearer"
    expires_in: int


class LoginResponse(BaseModel):
    ok: bool = True
    tokens: TokenPair
    user: str
    current_schema: Optional[str] = None
    database: str
    mock: bool = True


class RefreshRequest(BaseModel):
    refresh_token: str = Field(..., min_length=10)


class MeResponse(BaseModel):
    user: str
    database: Optional[str] = None
    current_schema: Optional[str] = None
