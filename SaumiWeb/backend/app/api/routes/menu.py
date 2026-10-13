from __future__ import annotations

from fastapi import APIRouter, Depends, HTTPException, status

from app.api.deps import get_menu_service, require_access_token
from app.repositories.menu_repository import MenuRepositoryError
from app.schemas.auth import MeResponse
from app.schemas.menu import MainPanelMenu
from app.services.menu_service import MenuService

router = APIRouter(prefix="/menu", tags=["menu"])


@router.get("", response_model=MainPanelMenu)
def main_panel(
    current: MeResponse = Depends(require_access_token),
    service: MenuService = Depends(get_menu_service),
) -> MainPanelMenu:
    """Главная панель TSaumiMainPanel: DFM + SQL из mmp32 (docs/menu_flow.md)."""
    try:
        return service.get_main_panel(current.user)
    except MenuRepositoryError as e:
        raise HTTPException(status.HTTP_503_SERVICE_UNAVAILABLE, detail=str(e)) from e
