from __future__ import annotations

from typing import Any

from app.repositories.menu_repository import MenuRaw, MenuRepository
from app.schemas.menu import MainPanelMenu, MenuButton, MenuItem


def _role_ok(role: str | None, roles: set[str]) -> bool:
    """Клиентский фильтр вместо TCheckRole, пока нет дизасма: членство в session_roles."""
    if role is None:
        return True
    name = str(role).strip()
    if not name:
        return True
    return name.upper() in {r.upper() for r in roles}


def _sep(i: int) -> MenuItem:
    return MenuItem(id=f"sep-{i}", caption="-", kind="separator")


def _cmd(
    dfm_id: str,
    caption: str,
    onclick: str | None,
    *,
    visible: bool = True,
    children: list[MenuItem] | None = None,
    kind: str = "command",
) -> MenuItem:
    return MenuItem(
        id=dfm_id,
        caption=caption,
        kind=kind,  # type: ignore[arg-type]
        visible=visible,
        onclick=onclick,
        children=children or [],
    )


def static_saumi_popup(forms: list[MenuItem]) -> list[MenuItem]:
    """BarPopupMenu1 из DFM TSAUMIMAINPANEL. Дети MI_ReportFormsRoot — из SQL."""
    return [
        _cmd("MI_Welcome", "Добро пожаловать в SAUMI", "MI_WelcomeClick"),
        MenuItem(
            id="MI_ReportFormsRoot",
            caption="Отчётные формы",
            kind="submenu",
            children=forms,
        ),
        _sep(1),
        _cmd("N4", "Мои действия за день", "N4Click"),
        _cmd("N2", "Документы на исполнение", "N2Click"),
        _cmd("MI_CompareAns", "Сравнительный анализ", "MI_CompareAnsClick", visible=False),
        _sep(2),
        _cmd("MI_ExportPayments", "Выгрузка начислений и платежей", "MI_ExportPaymentsClick"),
        _sep(3),
        MenuItem(
            id="N14",
            caption="Настройки..",
            kind="submenu",
            children=[
                _cmd(
                    "MI_SaveCurrentOwnerID",
                    "Запомнить текущего базового собственника",
                    "MI_SaveCurrentOwnerIDClick",
                ),
                _sep(4),
                _cmd("MI_ShowCase", "Использовать буфер", "MI_ShowCaseClick"),
                _cmd("MI_SaveCase", "Сохранять содержимое буфера", "MI_SaveCaseClick"),
                _cmd("MI_FreeSAUMIpanel", "Плавающая панель SAUMI", "MI_FreeSAUMIpanelClick"),
                _sep(5),
                _cmd("MI_ChangePassword", "Сменить пароль..", "MI_ChangePasswordClick"),
            ],
        ),
        _sep(6),
        _cmd("MI_DEBUGMODE", "Режим отладки", "MI_DEBUGMODEClick"),
        _cmd(
            "MI_SPDEBUG",
            "Пошаговое выполнение SAUMI-Pascal",
            "MI_SPDEBUGClick",
            visible=False,
        ),
        _sep(7),
        _cmd("N9", "О программе..", "N9Click"),
        _cmd("N10", "Выход", "N10Click"),
    ]


def _report_nodes(
    rows: list[dict[str, Any]],
    by_parent: dict[Any, list[dict[str, Any]]],
    *,
    prefix: str,
) -> list[MenuItem]:
    items: list[MenuItem] = []
    for row in rows:
        rid = row.get("id")
        kids_raw = by_parent.get(rid, [])
        is_group = bool(row.get("groupmark")) or bool(kids_raw)
        kids = _report_nodes(kids_raw, by_parent, prefix=prefix) if kids_raw else []
        items.append(
            MenuItem(
                id=f"{prefix}:{rid}",
                caption=str(row.get("name") or ""),
                kind="report_group" if is_group else "report",
                onclick=None if is_group else "ReportClick",
                children=kids,
                extra={
                    "groupmark": row.get("groupmark"),
                    "bigreport": row.get("bigreport"),
                    "analitics": row.get("analitics"),
                },
            )
        )
    return items


def _plugin_items(raw: list[dict[str, Any]], roles: set[str]) -> list[MenuItem]:
    visible_rows = [
        r
        for r in raw
        if _role_ok(r.get("execute_role"), roles)
    ]
    grouped: dict[str, list[dict[str, Any]]] = {}
    ungrouped: list[dict[str, Any]] = []
    for r in visible_rows:
        g = r.get("group_name")
        if g:
            grouped.setdefault(str(g), []).append(r)
        else:
            ungrouped.append(r)

    items: list[MenuItem] = []
    for gname, rows in grouped.items():
        items.append(
            MenuItem(
                id=f"plugin-group:{gname}",
                caption=gname,
                kind="plugin_group",
                children=[_plugin_leaf(r) for r in rows],
            )
        )
    items.extend(_plugin_leaf(r) for r in ungrouped)
    return items


def _plugin_leaf(r: dict[str, Any]) -> MenuItem:
    pid = r.get("pl_id")
    return MenuItem(
        id=f"plugin:{pid}",
        caption=str(r.get("menu_caption") or ""),
        kind="plugin",
        onclick="PluginMenuItemClick",
        extra={
            "file_name": r.get("file_name"),
            "file_extention": r.get("file_extention"),
            "execute_role": r.get("execute_role"),
            "is_dot_net": r.get("is_dot_net"),
        },
    )


def _form_items(rows: list[dict[str, Any]]) -> list[MenuItem]:
    by_group: dict[str, list[dict[str, Any]]] = {}
    for r in rows:
        g = r.get("menu_group")
        key = str(g) if g else ""
        by_group.setdefault(key, []).append(r)
    items: list[MenuItem] = []
    for gname, gro in by_group.items():
        leaves = [
            MenuItem(
                id=f"reportform:{r.get('id')}",
                caption=str(r.get("description") or ""),
                kind="report_form",
                onclick="ContextReportFormClick",
                extra={
                    "is_default": r.get("is_default"),
                    "shortcut_code": r.get("shortcut_code"),
                    "item_handling": r.get("item_handling"),
                },
            )
            for r in gro
        ]
        if gname:
            items.append(
                MenuItem(
                    id=f"reportform-group:{gname}",
                    caption=gname,
                    kind="report_form_group",
                    children=leaves,
                )
            )
        else:
            items.extend(leaves)
    return items


def _create_items(rows: list[dict[str, Any]]) -> list[MenuItem]:
    items: list[MenuItem] = []
    for r in rows:
        cap = r.get("creation_name")
        if cap is None or str(cap).strip() == "":
            continue
        items.append(
            MenuItem(
                id=f"create:{r.get('type_id')}",
                caption=str(cap),
                kind="create",
                onclick="NewItemCreateClick",
                extra={"table_name": r.get("table_name"), "type_id": r.get("type_id")},
            )
        )
    return items


def _template_popup(rows: list[dict[str, Any]]) -> list[MenuItem]:
    templates = [
        MenuItem(
            id=f"template:{r.get('templ_id')}",
            caption=str(r.get("templ_name") or ""),
            kind="template",
            onclick="TemplateFindClick",
            extra={
                "user_name": r.get("user_name"),
                "report_id": r.get("report_id"),
                "ord": r.get("ord"),
            },
        )
        for r in rows
    ]
    return [
        _cmd("MI_FullFind", "Полный поиск", "MI_FullFindClick"),
        _sep(10),
        *templates,
        _sep(11),
        _cmd("MI_NewFindTemplate", "Новый шаблон поиска", "MI_NewFindTemplateClick"),
        _cmd("MI_DeleteFindTemplate", "Удалить шаблон поиска", "DeleteFindTemplate"),
    ]


def _history_items(rows: list[dict[str, Any]]) -> list[MenuItem]:
    if not rows:
        return [_cmd("N1", "Пункты меню", "N1Click")]
    items: list[MenuItem] = []
    for r in rows:
        seq = r.get("seq")
        hid = r.get("handler_id")
        did = r.get("data_id")
        items.append(
            MenuItem(
                id=f"history:{seq}",
                caption=f"{hid}:{did}",
                kind="history",
                onclick="HistoryClick",
                extra=dict(r),
            )
        )
    return items


def _workspace_items(rows: list[dict[str, Any]], roles: set[str]) -> list[MenuItem]:
    items: list[MenuItem] = []
    for r in rows:
        if not _role_ok(r.get("show_role"), roles):
            continue
        items.append(
            MenuItem(
                id=f"ws:{r.get('id')}",
                caption=str(r.get("name") or ""),
                kind="workspace",
                onclick="OnMapMenuViewClick",
                extra={"show_role": r.get("show_role")},
            )
        )
    return items


def assemble_panel(raw: MenuRaw) -> MainPanelMenu:
    roles = set(raw.session_roles)
    plugins = _plugin_items(raw.plugins, roles)
    reports = _report_nodes(raw.reports_root, raw.reports_by_parent, prefix="report")
    # SM_USER_REPORTSTREE: узлы из SQL. Папку-обёртку не выдумываем (подпись склейки в PrepareMenuReports не снята).
    reports.extend(
        _report_nodes(
            raw.user_reports_root,
            raw.user_reports_by_parent,
            prefix="ureport",
        )
    )
    forms = _form_items(raw.autonomous_forms)
    create = _create_items(raw.create_items)
    workspaces = _workspace_items(raw.workspaces, roles)

    buttons = [
        MenuButton(
            id="ReportsBtn",
            caption="Отчеты",
            onclick="ReportsBtnClick",
            visible=True,
            popup="ReportList",
        ),
        MenuButton(
            id="ObligFindBtn",
            caption="Обязательства",
            onclick="ObligFindBtnClick",
            visible=True,
            popup="none",
        ),
        MenuButton(
            id="FindBtn",
            caption="Поиск",
            onclick="FindBtnClick",
            visible=True,
            popup="popFindTempls",
        ),
        MenuButton(
            id="SAUMI_Button",
            caption="SAUMI 4.7i",
            onclick="SAUMI_ButtonClick",
            visible=True,
            popup="BarPopupMenu1",
        ),
        MenuButton(
            id="PluginsBtn",
            caption="Дополнения",
            onclick="PluginsBtnClick",
            visible=bool(plugins),
            popup="PluginsMenu",
        ),
        MenuButton(
            id="HistoryBtn",
            caption="История",
            onclick="HistoryBtnClick",
            visible=True,
            popup="HistoryMenu",
        ),
        MenuButton(
            id="MapBtn",
            caption="Карта",
            onclick="MapBtnClick",
            visible=bool(workspaces),
            popup="popMap",
        ),
        MenuButton(
            id="CreateNewItemBtn",
            caption="Создать",
            onclick="CreateNewItemBtnClick",
            visible=True,
            popup="CreateItemMenu",
        ),
    ]

    extra_gaps: list[str] = []
    if raw.user_reports_root:
        extra_gaps.append(
            "SM_USER_REPORTSTREE добавлен в тот же popup без выдуманной подписи-папки (склейка PrepareMenuReports не снята)"
        )

    return MainPanelMenu(
        mock=raw.mock,
        oracle_session=raw.oracle_session,
        form_caption="SAUMI 4.6i",
        user_label="Имя пользователя ",
        display_user=raw.get_user_name or raw.display_user,
        pending_docs=raw.pending_docs,
        buttons=buttons,
        popups={
            "ReportList": reports,
            "PluginsMenu": plugins,
            "CreateItemMenu": create,
            "HistoryMenu": _history_items(raw.history),
            "popFindTempls": _template_popup(raw.search_templates),
            "popMap": workspaces,
            "BarPopupMenu1": static_saumi_popup(forms),
        },
        session_roles=raw.session_roles,
        gaps=[*raw.gaps, *extra_gaps],
    )


class MenuService:
    def __init__(self, repo: MenuRepository) -> None:
        self._repo = repo

    def get_main_panel(self, username: str) -> MainPanelMenu:
        return assemble_panel(self._repo.load(username))
