import { useCallback, useEffect, useState, type FormEvent } from "react";
import {
  formatAuthError,
  loadSessionFromToken,
  loginWithCredentials,
  logout as clearSession,
  type AuthSession,
} from "../../services/authService";
import { env } from "../../config/env";

type Props = {
  onSuccess: (session: AuthSession) => void;
};

/**
 * Вёрстка формы входа (поля как в DOA/SAUMI: пользователь, пароль, база данных).
 * Логика — в services; стили — в styles/login.css.
 */
export function LoginForm({ onSuccess }: Props) {
  const [username, setUsername] = useState("SM");
  const [password, setPassword] = useState("");
  const [database, setDatabase] = useState("localhost:1521/FREEPDB1");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const submit = useCallback(
    async (e: FormEvent) => {
      e.preventDefault();
      setError(null);
      setBusy(true);
      try {
        const session = await loginWithCredentials({
          username,
          password,
          database,
        });
        onSuccess(session);
      } catch (err) {
        setError(formatAuthError(err));
      } finally {
        setBusy(false);
      }
    },
    [username, password, database, onSuccess],
  );

  return (
    <div className="login-page">
      <form className="login-dialog" onSubmit={submit} autoComplete="on">
        <div className="login-dialog__titlebar">
          <span className="login-dialog__titlebar-icon" aria-hidden />
          Подключение к {env.appTitle}
        </div>
        <div className="login-dialog__body">
          <p className="login-dialog__hint">
            Учётная запись Oracle и имя базы (TNS), как в клиенте SAUMI.
          </p>

          <div className="login-field">
            <label htmlFor="saumi-user">Пользователь</label>
            <input
              id="saumi-user"
              name="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              autoFocus
              required
              disabled={busy}
            />
          </div>

          <div className="login-field">
            <label htmlFor="saumi-pass">Пароль</label>
            <input
              id="saumi-pass"
              name="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              disabled={busy}
            />
          </div>

          <div className="login-field">
            <label htmlFor="saumi-db">База данных</label>
            <input
              id="saumi-db"
              name="database"
              value={database}
              onChange={(e) => setDatabase(e.target.value)}
              required
              disabled={busy}
            />
          </div>

          {error ? <div className="login-dialog__error">{error}</div> : null}

          <div className="login-dialog__actions">
            <button type="button" className="login-btn login-btn--ghost" disabled={busy}>
              Отмена
            </button>
            <button
              type="submit"
              className="login-btn login-btn--primary"
              disabled={busy}
            >
              {busy ? "Вход…" : "OK"}
            </button>
          </div>
        </div>
      </form>
    </div>
  );
}

export function useBootstrapSession() {
  const [session, setSession] = useState<AuthSession | null>(null);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      const s = await loadSessionFromToken();
      if (!cancelled) {
        setSession(s);
        setReady(true);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  const logout = useCallback(() => {
    clearSession();
    setSession(null);
  }, []);

  return { session, setSession, ready, logout };
}
