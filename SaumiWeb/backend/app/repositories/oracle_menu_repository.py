from __future__ import annotations

import logging
from typing import Any

from app.core.config import Settings
from app.repositories import menu_sql
from app.repositories.menu_repository import MenuRaw, MenuRepository, MenuRepositoryError

logger = logging.getLogger(__name__)


class OracleMenuRepository(MenuRepository):
    """Исполняет SQL из mmp32. Нужна живая сессия пользователя (как DOA после logon).

    Сейчас login закрывает connect. Пока нет пула — не подставляем чужие данные и не
    ходим в БД под сервисной УЗ.
    """

    def __init__(self, settings: Settings) -> None:
        self._settings = settings

    def load(self, username: str) -> MenuRaw:
        raise MenuRepositoryError(
            "меню Oracle: нет пула сессии после login (пароль не храним). "
            "SQL готовы в app.repositories.menu_sql; сначала session pool, потом USE_ORACLE=1"
        )

    def _fetch_tree(  # noqa: ARG002 — будет использовано с пулом
        self,
        cur: Any,
        username: str,
    ) -> MenuRaw:
        cur.execute(menu_sql.SESSION_ROLES)
        roles = [str(r[0]) for r in cur.fetchall() if r[0]]

        cur.execute(menu_sql.GET_USER_NAME)
        row = cur.fetchone()
        get_name, db_user = (row[0], row[1]) if row else (None, username)

        cur.execute(menu_sql.PLUGINS_LIST)
        plugins = _dicts(
            cur,
            [
                "pl_ord",
                "pl_id",
                "menu_caption",
                "file_name",
                "file_extention",
                "execute_role",
                "is_dot_net",
                "plg_ord",
                "group_name",
                "ord",
            ],
        )

        cur.execute(menu_sql.REPORTSTREE_ROOT)
        reports_root = _dicts(cur, ["id", "name", "groupmark", "bigreport"])
        reports_by_parent: dict[Any, list[dict[str, Any]]] = {}
        stack = [r["id"] for r in reports_root]
        seen: set[Any] = set(stack)
        while stack:
            parent = stack.pop()
            cur.execute(menu_sql.REPORTSTREE_CHILDREN, {"parent": parent})
            kids = _dicts(cur, ["id", "name", "groupmark", "bigreport"])
            reports_by_parent[parent] = kids
            for k in kids:
                if k["id"] not in seen:
                    seen.add(k["id"])
                    stack.append(k["id"])

        user_root: list[dict[str, Any]] = []
        user_by_parent: dict[Any, list[dict[str, Any]]] = {}
        try:
            cur.execute(menu_sql.USER_REPORTSTREE_ROOT)
            user_root = _dicts(cur, ["id", "name", "groupmark", "analitics"])
            ustack = [r["id"] for r in user_root]
            useen = set(ustack)
            while ustack:
                parent = ustack.pop()
                cur.execute(menu_sql.USER_REPORTSTREE_CHILDREN, {"parent": parent})
                kids = _dicts(cur, ["id", "name", "groupmark", "analitics"])
                user_by_parent[parent] = kids
                for k in kids:
                    if k["id"] not in useen:
                        useen.add(k["id"])
                        ustack.append(k["id"])
        except Exception as e:  # noqa: BLE001
            logger.info("SM_USER_REPORTSTREE skip: %s", e)

        cur.execute(menu_sql.AUTONOMOUS_REPORTFORMS)
        forms = _dicts(
            cur,
            [
                "id",
                "description",
                "is_default",
                "shortcut_code",
                "item_handling",
                "menu_group",
            ],
        )

        cur.execute(menu_sql.CREATE_ITEMS)
        create_items = _dicts(cur, ["type_id", "creation_name", "table_name"])

        cur.execute(menu_sql.SEARCH_TEMPLATES, {"uname": username})
        templates = _dicts(
            cur, ["templ_id", "templ_name", "user_name", "ord", "report_id"]
        )

        history: list[dict[str, Any]] = []
        try:
            cur.execute(menu_sql.HISTORY)
            cols = [d[0].lower() for d in cur.description]
            history = [dict(zip(cols, row, strict=False)) for row in cur.fetchall()]
        except Exception as e:  # noqa: BLE001
            logger.info("SM_USER_HISTORY skip: %s", e)

        workspaces: list[dict[str, Any]] = []
        try:
            cur.execute(menu_sql.MAP_WORKSPACES)
            workspaces = _dicts(cur, ["id", "name", "show_role"])
        except Exception as e:  # noqa: BLE001
            logger.info("sd3_workspaces skip: %s", e)

        pending = None
        try:
            cur.execute(menu_sql.PENDING_DOCS_COUNT)
            pending = int(cur.fetchone()[0])
        except Exception as e:  # noqa: BLE001
            logger.info("pending docs skip: %s", e)

        owners: list[dict[str, Any]] = []
        try:
            cur.execute(menu_sql.OWNER_AGENTS)
            owners = _dicts(cur, ["id", "ownership_name"])
        except Exception as e:  # noqa: BLE001
            logger.info("owner agents skip: %s", e)

        return MenuRaw(
            mock=False,
            oracle_session=True,
            session_roles=roles,
            display_user=str(db_user or username),
            get_user_name=str(get_name) if get_name is not None else None,
            plugins=plugins,
            reports_root=reports_root,
            reports_by_parent=reports_by_parent,
            user_reports_root=user_root,
            user_reports_by_parent=user_by_parent,
            autonomous_forms=forms,
            create_items=create_items,
            search_templates=templates,
            history=history,
            workspaces=workspaces,
            pending_docs=pending,
            owner_agents=owners,
            gaps=[
                "TCheckRole: тело не дизассемблировано; фильтр = ROLE in session_roles",
                "PrepareMenuCreateNewItem: CREATION_ROLE / all_tab_privs без дизасма не применяем",
                "PrepareMenuHistory: caption из HANDLER_ID не снят — сырые SEQ/HANDLER_ID/DATA_ID",
                "SAUMI-Pascal AddMenuItem не эмулируется",
            ],
        )


def _dicts(cur: Any, keys: list[str]) -> list[dict[str, Any]]:
    rows = cur.fetchall()
    return [dict(zip(keys, row, strict=False)) for row in rows]
