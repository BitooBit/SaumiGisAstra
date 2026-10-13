from __future__ import annotations

from app.core.config import Settings, get_settings
from app.repositories.auth_repository import AuthRepository
from app.repositories.menu_repository import MenuRepository
from app.repositories.mock_auth_repository import MockAuthRepository
from app.repositories.mock_menu_repository import MockMenuRepository
from app.repositories.oracle_auth_repository import OracleAuthRepository
from app.repositories.oracle_menu_repository import OracleMenuRepository


def get_auth_repository(settings: Settings | None = None) -> AuthRepository:
    cfg = settings or get_settings()
    if cfg.use_oracle:
        return OracleAuthRepository(cfg)
    return MockAuthRepository()


def get_menu_repository(settings: Settings | None = None) -> MenuRepository:
    cfg = settings or get_settings()
    if cfg.use_oracle:
        return OracleMenuRepository(cfg)
    return MockMenuRepository()
