from __future__ import annotations

import logging

from app.repositories.auth_repository import (
    AuthRepository,
    AuthRepositoryError,
    SessionProbe,
)

logger = logging.getLogger(__name__)


class MockAuthRepository(AuthRepository):
    """Пока нет Oracle: принимаем любые непустые credentials (как стенд UI)."""

    def login(self, username: str, password: str, database: str) -> SessionProbe:
        user = username.strip()
        db = database.strip()
        if not user or not password or not db:
            raise AuthRepositoryError("username, password и database обязательны")

        # Имитация post-logon probe exe:
        # SELECT USER, sys_context('userenv','CURRENT_SCHEMA') FROM dual
        schema = user.upper()
        logger.info("mock login ok user=%s database=%s", user, db)
        return SessionProbe(
            user=schema,
            current_schema=schema,
            database=db,
            mock=True,
        )
