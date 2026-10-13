from __future__ import annotations

from dataclasses import dataclass, field
from typing import Any


class MenuRepositoryError(Exception):
    pass


@dataclass
class MenuRaw:
    """Сырьё из БД / мока. Сборка дерева — в service (как PrepareMenu*)."""

    mock: bool
    oracle_session: bool
    session_roles: list[str] = field(default_factory=list)
    display_user: str = ""
    get_user_name: str | None = None
    plugins: list[dict[str, Any]] = field(default_factory=list)
    reports_root: list[dict[str, Any]] = field(default_factory=list)
    reports_by_parent: dict[Any, list[dict[str, Any]]] = field(default_factory=dict)
    user_reports_root: list[dict[str, Any]] = field(default_factory=list)
    user_reports_by_parent: dict[Any, list[dict[str, Any]]] = field(default_factory=dict)
    autonomous_forms: list[dict[str, Any]] = field(default_factory=list)
    create_items: list[dict[str, Any]] = field(default_factory=list)
    search_templates: list[dict[str, Any]] = field(default_factory=list)
    history: list[dict[str, Any]] = field(default_factory=list)
    workspaces: list[dict[str, Any]] = field(default_factory=list)
    pending_docs: int | None = None
    owner_agents: list[dict[str, Any]] = field(default_factory=list)
    gaps: list[str] = field(default_factory=list)


class MenuRepository:
    def load(self, username: str) -> MenuRaw:
        raise NotImplementedError
