from __future__ import annotations

from dataclasses import dataclass


class AuthRepositoryError(Exception):
    """Ошибка аутентификации / Oracle."""


@dataclass(frozen=True)
class SessionProbe:
    user: str
    current_schema: str | None
    database: str
    mock: bool


class AuthRepository:
    """Контракт: проверка credentials + post-logon probe (как exe AfterLogon)."""

    def login(self, username: str, password: str, database: str) -> SessionProbe:
        raise NotImplementedError
