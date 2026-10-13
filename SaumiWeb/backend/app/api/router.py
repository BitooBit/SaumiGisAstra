from fastapi import APIRouter

from app.api.routes import auth, menu

api_router = APIRouter()
api_router.include_router(auth.router)
api_router.include_router(menu.router)


@api_router.get("/health", tags=["health"])
def health():
    return {"status": "ok"}
