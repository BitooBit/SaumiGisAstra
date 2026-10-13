from __future__ import annotations

from app.repositories.menu_repository import MenuRaw, MenuRepository


class MockMenuRepository(MenuRepository):
    """Без Oracle: роли как у типичной сессии SM, динамические таблицы пустые.

    Captions отчётов/плагинов не выдумываем — появятся из SQL exe при USE_ORACLE + пул сессии.
    """

    def load(self, username: str) -> MenuRaw:
        user = (username or "SM").strip() or "SM"
        return MenuRaw(
            mock=True,
            oracle_session=False,
            session_roles=["CONNECT", "SM_ALLUSERS"],
            display_user=user,
            get_user_name=user,
            pending_docs=0,
            gaps=[
                "USE_ORACLE=0: динамика (REPORTSTREE, PLUGINS, FM_TABLES, …) пустая",
                "TCheckRole: тело не дизассемблировано; фильтр = ROLE in session_roles",
                "PrepareMenuCreateNewItem: CREATION_ROLE и all_tab_privs без дизасма не применяем",
                "PrepareMenuHistory: caption из HANDLER_ID не снят",
                "SAUMI-Pascal AddMenuItem не эмулируется",
            ],
        )
