import { apiRequest } from "./http";

export type LoginPayload = {
  username: string;
  password: string;
  database: string;
};

export type TokenPair = {
  access_token: string;
  refresh_token: string;
  token_type: string;
  expires_in: number;
};

export type LoginResult = {
  ok: boolean;
  tokens: TokenPair;
  user: string;
  current_schema: string | null;
  database: string;
  mock: boolean;
};

export type MeResult = {
  user: string;
  database: string | null;
  current_schema: string | null;
};

/** Слой API: только HTTP-контракты бэка. */
export const authApi = {
  login(payload: LoginPayload) {
    return apiRequest<LoginResult>("/auth/login", { body: payload });
  },
  refresh(refreshToken: string) {
    return apiRequest<TokenPair>("/auth/refresh", {
      body: { refresh_token: refreshToken },
    });
  },
  me(accessToken: string) {
    return apiRequest<MeResult>("/auth/me", { token: accessToken });
  },
};
