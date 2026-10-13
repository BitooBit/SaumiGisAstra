from __future__ import annotations

from fastapi import APIRouter, Depends, HTTPException, Request, status
from slowapi import Limiter
from slowapi.util import get_remote_address

from app.api.deps import get_auth_service, require_access_token
from app.core.config import get_settings
from app.schemas.auth import (
    LoginRequest,
    LoginResponse,
    MeResponse,
    RefreshRequest,
    TokenPair,
)
from app.services.auth_service import AuthService, AuthServiceError

router = APIRouter(prefix="/auth", tags=["auth"])
limiter = Limiter(key_func=get_remote_address)


_login_limit = f"{get_settings().login_rate_limit_per_minute}/minute"


@router.post("/login", response_model=LoginResponse)
@limiter.limit(_login_limit)
def login(
    request: Request,
    body: LoginRequest,
    service: AuthService = Depends(get_auth_service),
) -> LoginResponse:
    """Публичный: зеркало формы DOA (user / password / database)."""
    try:
        return service.login(body.username, body.password, body.database)
    except AuthServiceError as e:
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, detail=str(e)) from e


@router.post("/refresh", response_model=TokenPair)
def refresh(
    body: RefreshRequest,
    service: AuthService = Depends(get_auth_service),
) -> TokenPair:
    """Публичный: обмен refresh → новая пара (ротация jti)."""
    try:
        return service.refresh(body.refresh_token)
    except AuthServiceError as e:
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, detail=str(e)) from e


@router.get("/me", response_model=MeResponse)
def me(current: MeResponse = Depends(require_access_token)) -> MeResponse:
    """Защищённый: текущий пользователь из access token."""
    return current
