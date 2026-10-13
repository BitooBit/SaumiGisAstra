from __future__ import annotations

from typing import Any, Literal, Optional

from pydantic import BaseModel, Field


class MenuItem(BaseModel):
    """Узел меню/панели. id — имя DFM или ключ из SQL (plugin:12, report:5, …)."""

    id: str
    caption: str
    kind: Literal[
        "button",
        "label",
        "separator",
        "command",
        "submenu",
        "plugin",
        "plugin_group",
        "report",
        "report_group",
        "report_form",
        "report_form_group",
        "create",
        "template",
        "history",
        "workspace",
    ]
    visible: bool = True
    enabled: bool = True
    onclick: Optional[str] = None
    shortcut: Optional[str] = None
    tag: Optional[int] = None
    children: list["MenuItem"] = Field(default_factory=list)
    extra: dict[str, Any] = Field(default_factory=dict)


class MenuButton(BaseModel):
    id: str
    caption: str
    onclick: Optional[str] = None
    visible: bool = True
    popup: str
    """Имя popup из DFM: ReportList | PluginsMenu | … | BarPopupMenu1 | none."""


class MainPanelMenu(BaseModel):
    mock: bool
    oracle_session: bool
    form_caption: str
    user_label: str
    display_user: str
    pending_docs: Optional[int] = None
    buttons: list[MenuButton]
    popups: dict[str, list[MenuItem]]
    session_roles: list[str]
    gaps: list[str]


MenuItem.model_rebuild()
