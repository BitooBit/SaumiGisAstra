import { LoginForm, useBootstrapSession } from "./pages/login/LoginForm";
import { SaumiPanel } from "./pages/shell/SaumiPanel";
import "./styles/tokens.css";
import "./styles/login.css";
import "./styles/shell.css";

export default function App() {
  const { session, setSession, ready, logout } = useBootstrapSession();

  if (!ready) {
    return <div className="login-page">Загрузка…</div>;
  }

  if (!session) {
    return <LoginForm onSuccess={setSession} />;
  }

  return <SaumiPanel session={session} onLogout={logout} />;
}
