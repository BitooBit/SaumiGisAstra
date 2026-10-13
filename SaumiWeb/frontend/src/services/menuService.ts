import { ApiError } from "../api/http";
import { menuApi, type MainPanelMenu, type MenuItem } from "../api/menuApi";
import { tokenStore } from "./tokenStore";
import { loadSessionFromToken } from "./authService";

export async function loadMainPanel(): Promise<MainPanelMenu> {
  try {
    return await menuApi.getMainPanel();
  } catch (e) {
    if (e instanceof ApiError && e.status === 401 && tokenStore.getRefresh()) {
      const session = await loadSessionFromToken();
      if (session) return menuApi.getMainPanel();
    }
    throw e;
  }
}

export function visibleItems(items: MenuItem[]): MenuItem[] {
  return items
    .filter((i) => i.visible)
    .map((i) => ({ ...i, children: visibleItems(i.children) }));
}

export function formatMenuError(err: unknown): string {
  if (err instanceof ApiError) return err.message;
  if (err instanceof Error) return err.message;
  return "Не удалось загрузить меню";
}
