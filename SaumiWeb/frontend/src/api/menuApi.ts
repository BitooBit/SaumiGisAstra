import { apiRequest } from "./http";
import { tokenStore } from "../services/tokenStore";

export type MenuKind =
  | "button"
  | "label"
  | "separator"
  | "command"
  | "submenu"
  | "plugin"
  | "plugin_group"
  | "report"
  | "report_group"
  | "report_form"
  | "report_form_group"
  | "create"
  | "template"
  | "history"
  | "workspace";

export type MenuItem = {
  id: string;
  caption: string;
  kind: MenuKind;
  visible: boolean;
  enabled: boolean;
  onclick: string | null;
  shortcut: string | null;
  tag: number | null;
  children: MenuItem[];
  extra: Record<string, unknown>;
};

export type MenuButton = {
  id: string;
  caption: string;
  onclick: string | null;
  visible: boolean;
  popup: string;
};

export type MainPanelMenu = {
  mock: boolean;
  oracle_session: boolean;
  form_caption: string;
  user_label: string;
  display_user: string;
  pending_docs: number | null;
  buttons: MenuButton[];
  popups: Record<string, MenuItem[]>;
  session_roles: string[];
  gaps: string[];
};

export const menuApi = {
  getMainPanel() {
    return apiRequest<MainPanelMenu>("/menu", {
      token: tokenStore.getAccess(),
    });
  },
};
