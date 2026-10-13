const ACCESS_KEY = "saumiweb.access";
const REFRESH_KEY = "saumiweb.refresh";

/** Хранение токенов (слой логики сессии; не UI). */
export const tokenStore = {
  getAccess(): string | null {
    return sessionStorage.getItem(ACCESS_KEY);
  },
  getRefresh(): string | null {
    return sessionStorage.getItem(REFRESH_KEY);
  },
  set(access: string, refresh: string) {
    sessionStorage.setItem(ACCESS_KEY, access);
    sessionStorage.setItem(REFRESH_KEY, refresh);
  },
  clear() {
    sessionStorage.removeItem(ACCESS_KEY);
    sessionStorage.removeItem(REFRESH_KEY);
  },
};
