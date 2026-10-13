from __future__ import annotations

from fastapi import Depends, HTTPException, status
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer

from app.core.config import Settings, get_settings
from app.repositories.factory import get_auth_repository, get_menu_repository
from app.services.auth_service import AuthService, AuthServiceError
from app.services.menu_service import MenuService

_bearer = HTTPBearer(auto_error=False)


def get_auth_service(
    settings: Settings = Depends(get_settings),
) -> AuthService:
    return AuthService(settings=settings, repo=get_auth_repository(settings))


def get_menu_service(
    settings: Settings = Depends(get_settings),
) -> MenuService:
    return MenuService(repo=get_menu_repository(settings))


def require_access_token(
    creds: HTTPAuthorizationCredentials | None = Depends(_bearer),
    service: AuthService = Depends(get_auth_service),
):
    if creds is None or creds.scheme.lower() != "bearer":
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Требуется Bearer access token",
            headers={"WWW-Authenticate": "Bearer"},
        )
    try:
        return service.me(creds.credentials)
    except AuthServiceError as e:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail=str(e),
            headers={"WWW-Authenticate": "Bearer"},
        ) from e
