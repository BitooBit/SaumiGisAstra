#!/usr/bin/env bash
# Принудительное закрытие SAUMI (если зависло)
set -uo pipefail

W="${HOME}/Saumi"
export WINEPREFIX="${WINEPREFIX:-${W}}"
export WINEARCH=win32

WS="${W}/runtime/wine-11.0/bin/wineserver"
[[ -x "$WS" ]] || WS="$(command -v wineserver 2>/dev/null || true)"

pkill -u "$(id -u)" -f 'saumi_hide_windows\.sh' 2>/dev/null || true
pkill -u "$(id -u)" -f 'saumi_minimize_guard\.sh' 2>/dev/null || true
pkill -u "$(id -u)" -f 'saumi_taskbar_fix\.sh' 2>/dev/null || true
pkill -9 -u "$(id -u)" -f '[m]mp32\.exe' 2>/dev/null || true
pkill -9 -u "$(id -u)" -f '[E]xplorerWindow' 2>/dev/null || true
pkill -9 -u "$(id -u)" -f '[s]aumi\.exe' 2>/dev/null || true
if [[ -n "${WS:-}" && -x "$WS" ]]; then
  "$WS" -k 2>/dev/null || true
fi
pkill -9 -u "$(id -u)" -x wineserver 2>/dev/null || true
sleep 1

if command -v zenity >/dev/null 2>&1; then
  zenity --info --title="SAUMI" --text="SAUMI принудительно закрыт." --timeout=4 2>/dev/null || true
fi

exit 0
