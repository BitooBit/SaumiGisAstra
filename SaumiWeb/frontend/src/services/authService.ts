import { ApiError } from "../api/http";
import { authApi, type LoginPayload, type LoginResult } from "../api/authApi";
import { tokenStore } from "./tokenStore";

export type AuthSession = {
  user: string;
  database: string;
  currentSchema: string | null;
  mock: boolean;
};

/** Бизнес-логика входа (как exe DOA → средний слой). */
export async function loginWithCredentials(
  payload: LoginPayload,
): Promise<AuthSession> {
  const result: LoginResult = await authApi.login(payload);
  tokenStore.set(result.tokens.access_token, result.tokens.refresh_token);
  return {
    user: result.user,
    database: result.database,
    currentSchema: result.current_schema,
    mock: result.mock,
  };
}

export async function loadSessionFromToken(): Promise<AuthSession | null> {
  const access = tokenStore.getAccess();
  if (!access) return null;
  try {
    const me = await authApi.me(access);
    return {
      user: me.user,
      database: me.database ?? "",
      currentSchema: me.current_schema,
      mock: false,
    };
  } catch (e) {
    if (e instanceof ApiError && e.status === 401) {
      const refreshed = await tryRefresh();
      if (refreshed) return loadSessionFromToken();
    }
    tokenStore.clear();
    return null;
  }
}

async function tryRefresh(): Promise<boolean> {
  const refresh = tokenStore.getRefresh();
  if (!refresh) return false;
  try {
    const pair = await authApi.refresh(refresh);
    tokenStore.set(pair.access_token, pair.refresh_token);
    return true;
  } catch {
    tokenStore.clear();
    return false;
  }
}

export function logout() {
  tokenStore.clear();
}

export function formatAuthError(err: unknown): string {
  if (err instanceof ApiError) return err.message;
  if (err instanceof Error) return err.message;
  return "Неизвестная ошибка входа";
}
