from __future__ import annotations

import logging

from app.core.config import Settings
from app.repositories.auth_repository import (
    AuthRepository,
    AuthRepositoryError,
    SessionProbe,
)

logger = logging.getLogger(__name__)


class OracleAuthRepository(AuthRepository):
    """Реальный connect как DOA / OracleProvider.StartConnection.

    Включается USE_ORACLE=1. Пока БД недоступна — используйте MockAuthRepository.
    """

    def __init__(self, settings: Settings) -> None:
        self._settings = settings

    def login(self, username: str, password: str, database: str) -> SessionProbe:
        try:
            import oracledb
        except ImportError as e:
            raise AuthRepositoryError("oracledb не установлен") from e

        dsn = database.strip() or self._settings.oracle_dsn
        user = username.strip()

        # --- реальный вызов (как exe) ---
        # if self._settings.oracle_thick_mode:
        #     oracledb.init_oracle_client()
        try:
            conn = oracledb.connect(user=user, password=password, dsn=dsn)
        except Exception as e:  # noqa: BLE001
            logger.warning("oracle login failed user=%s", user)
            raise AuthRepositoryError(f"Ошибка соединения: {e}") from e

        try:
            with conn.cursor() as cur:
                # Post-logon probe из docs/login_flow.md
                cur.execute(
                    "SELECT USER, sys_context('userenv', 'CURRENT_SCHEMA') FROM dual"
                )
                db_user, schema = cur.fetchone()

                # TODO (когда добьём MMP_SessionAfterLogOn):
                # cur.execute("alter session set …")
                # права I/U/S/D по all_tab_privs — см. login_flow.md

            return SessionProbe(
                user=str(db_user),
                current_schema=str(schema) if schema is not None else None,
                database=dsn,
                mock=False,
            )
        except Exception as e:  # noqa: BLE001
            raise AuthRepositoryError(f"Post-logon probe failed: {e}") from e
        finally:
            conn.close()
