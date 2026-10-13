from __future__ import annotations

from dataclasses import dataclass, field

from app.core.config import Settings
from app.core.security import (
    TokenError,
    create_access_token,
    create_refresh_token,
    decode_token,
    require_token_type,
)
from app.repositories.auth_repository import AuthRepository, AuthRepositoryError
from app.schemas.auth import LoginResponse, MeResponse, TokenPair


@dataclass
class RefreshStore:
    """In-memory allowlist jti refresh (прод → Redis). Ротация при refresh."""

    _active: set[str] = field(default_factory=set)

    def add(self, jti: str) -> None:
        self._active.add(jti)

    def revoke(self, jti: str) -> None:
        self._active.discard(jti)

    def is_active(self, jti: str) -> bool:
        return jti in self._active


_refresh_store = RefreshStore()


class AuthService:
    def __init__(
        self,
        settings: Settings,
        repo: AuthRepository,
        refresh_store: RefreshStore | None = None,
    ) -> None:
        self._settings = settings
        self._repo = repo
        self._refresh = refresh_store or _refresh_store

    def login(self, username: str, password: str, database: str) -> LoginResponse:
        try:
            probe = self._repo.login(username, password, database)
        except AuthRepositoryError as e:
            raise AuthServiceError(str(e)) from e

        extra = {"database": probe.database, "schema": probe.current_schema}
        access = create_access_token(
            self._settings, subject=probe.user, extra=extra
        )
        refresh = create_refresh_token(self._settings, subject=probe.user)
        refresh_payload = decode_token(self._settings, refresh)
        self._refresh.add(str(refresh_payload["jti"]))

        return LoginResponse(
            tokens=TokenPair(
                access_token=access,
                refresh_token=refresh,
                expires_in=self._settings.access_token_expire_minutes * 60,
            ),
            user=probe.user,
            current_schema=probe.current_schema,
            database=probe.database,
            mock=probe.mock,
        )

    def refresh(self, refresh_token: str) -> TokenPair:
        try:
            payload = decode_token(self._settings, refresh_token)
            require_token_type(payload, "refresh")
        except TokenError as e:
            raise AuthServiceError("недействительный refresh token") from e

        jti = str(payload.get("jti", ""))
        if not jti or not self._refresh.is_active(jti):
            raise AuthServiceError("refresh token отозван или неизвестен")

        subject = str(payload["sub"])
        # ротация
        self._refresh.revoke(jti)
        access = create_access_token(self._settings, subject=subject)
        new_refresh = create_refresh_token(self._settings, subject=subject)
        new_payload = decode_token(self._settings, new_refresh)
        self._refresh.add(str(new_payload["jti"]))

        return TokenPair(
            access_token=access,
            refresh_token=new_refresh,
            expires_in=self._settings.access_token_expire_minutes * 60,
        )

    def me(self, access_token: str) -> MeResponse:
        try:
            payload = decode_token(self._settings, access_token)
            require_token_type(payload, "access")
        except TokenError as e:
            raise AuthServiceError("недействительный access token") from e

        return MeResponse(
            user=str(payload["sub"]),
            database=payload.get("database"),
            current_schema=payload.get("schema"),
        )


class AuthServiceError(Exception):
    pass
