import { useCallback, useEffect, useState } from "react";
import type { MainPanelMenu, MenuButton, MenuItem } from "../../api/menuApi";
import {
  formatMenuError,
  loadMainPanel,
  visibleItems,
} from "../../services/menuService";
import type { AuthSession } from "../../services/authService";

type Props = {
  session: AuthSession;
  onLogout: () => void;
};

function PopupList({
  items,
  nested,
  onCommand,
}: {
  items: MenuItem[];
  nested?: boolean;
  onCommand: (item: MenuItem) => void;
}) {
  const [openId, setOpenId] = useState<string | null>(null);
  const shown = visibleItems(items);

  if (shown.length === 0) {
    return (
      <ul className={nested ? "shell-popup shell-popup--nested" : "shell-popup"} role="menu">
        <li className="shell-mi">
          <button type="button" className="shell-mi__btn" disabled>
            Нет пунктов (SQL exe, строки пустые)
          </button>
        </li>
      </ul>
    );
  }

  return (
    <ul className={nested ? "shell-popup shell-popup--nested" : "shell-popup"} role="menu">
      {shown.map((item) => {
        if (item.kind === "separator") {
          return <li key={item.id} className="shell-mi--sep" />;
        }
        const hasKids = item.children.length > 0 || item.kind.endsWith("group") || item.kind === "submenu";
        return (
          <li
            key={item.id}
            className={`shell-mi${openId === item.id ? " is-open" : ""}`}
            onMouseEnter={() => hasKids && setOpenId(item.id)}
            onMouseLeave={() => hasKids && setOpenId(null)}
          >
            <button
              type="button"
              className="shell-mi__btn"
              disabled={!item.enabled}
              onClick={() => {
                if (hasKids && item.children.length > 0) {
                  setOpenId((cur) => (cur === item.id ? null : item.id));
                  return;
                }
                onCommand(item);
              }}
            >
              <span>{item.caption}</span>
              {hasKids ? <span className="shell-mi__arrow">▸</span> : null}
            </button>
            {hasKids && openId === item.id && item.children.length > 0 ? (
              <PopupList items={item.children} nested onCommand={onCommand} />
            ) : null}
          </li>
        );
      })}
    </ul>
  );
}

function PanelButton({
  button,
  items,
  open,
  onToggle,
  onCommand,
}: {
  button: MenuButton;
  items: MenuItem[];
  open: boolean;
  onToggle: () => void;
  onCommand: (item: MenuItem) => void;
}) {
  const isSaumi = button.id === "SAUMI_Button";
  return (
    <div style={{ position: "relative" }}>
      <button
        type="button"
        className={`shell-btn${isSaumi ? " shell-btn--saumi" : ""}${open ? " is-open" : ""}`}
        onClick={onToggle}
      >
        {button.caption}
      </button>
      {open && button.popup !== "none" ? (
        <PopupList items={items} onCommand={onCommand} />
      ) : null}
    </div>
  );
}

export function SaumiPanel({ session, onLogout }: Props) {
  const [menu, setMenu] = useState<MainPanelMenu | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [openPopup, setOpenPopup] = useState<string | null>(null);
  const [toast, setToast] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const data = await loadMainPanel();
        if (!cancelled) setMenu(data);
      } catch (e) {
        if (!cancelled) setError(formatMenuError(e));
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    const close = (e: MouseEvent) => {
      const t = e.target;
      if (t instanceof Element && t.closest(".shell-appbar")) return;
      setOpenPopup(null);
    };
    document.addEventListener("mousedown", close);
    return () => document.removeEventListener("mousedown", close);
  }, []);

  const onCommand = useCallback(
    (item: MenuItem) => {
      setOpenPopup(null);
      if (item.id === "N10" || item.onclick === "N10Click") {
        onLogout();
        return;
      }
      const action = item.onclick ?? item.kind;
      setToast(
        `exe ${action} / ${item.id} — экран ещё не перенесён (меню собрано, обработчик следующий шаг)`,
      );
    },
    [onLogout],
  );

  if (error) {
    return (
      <div className="shell">
        <div className="shell-main">{error}</div>
      </div>
    );
  }

  if (!menu) {
    return (
      <div className="shell">
        <div className="shell-main">Загрузка панели…</div>
      </div>
    );
  }

  const visibleButtons = menu.buttons.filter((b) => b.visible);

  return (
    <div className="shell">
      <header className="shell-appbar">
        <div className="shell-user">
          <span className="shell-user__label">{menu.user_label}</span>
          <span className="shell-user__name">
            {menu.display_user}
            {menu.mock ? " (mock)" : ""}
          </span>
        </div>
        {visibleButtons.map((b) => (
          <PanelButton
            key={b.id}
            button={b}
            items={menu.popups[b.popup] ?? []}
            open={openPopup === b.id}
            onToggle={() => {
              if (b.popup === "none") {
                onCommand({
                  id: b.id,
                  caption: b.caption,
                  kind: "command",
                  visible: true,
                  enabled: true,
                  onclick: b.onclick,
                  shortcut: null,
                  tag: null,
                  children: [],
                  extra: {},
                });
                return;
              }
              setOpenPopup((cur) => (cur === b.id ? null : b.id));
            }}
            onCommand={onCommand}
          />
        ))}
        <div className="shell-spacer" />
        <button type="button" className="login-btn login-btn--ghost shell-logout" onClick={onLogout}>
          Выход
        </button>
      </header>
      <main className="shell-main">
        <h1>{menu.form_caption}</h1>
        <p>
          Панель как `TSaumiMainPanel` в mmp32 (не выдуманный File/Правка). Динамика отчётов и
          дополнений — из SQL exe; в моке строк нет, пока нет пула Oracle-сессии.
        </p>
        {session.database ? <p>База: {session.database}</p> : null}
        {menu.pending_docs != null ? (
          <p>Документы на исполнение (count exe): {menu.pending_docs}</p>
        ) : null}
        {toast ? <div className="shell-toast">{toast}</div> : null}
        {menu.gaps.length > 0 ? (
          <ul className="shell-gaps">
            {menu.gaps.map((g) => (
              <li key={g}>{g}</li>
            ))}
          </ul>
        ) : null}
      </main>
    </div>
  );
}
