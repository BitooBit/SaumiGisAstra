#!/usr/bin/env bash
# Режим 2: окна SAUMI → NORMAL, убрать SKIP_TASKBAR/ABOVE (панель Fly + свернуть).
# Класс окон Wine: saumi.exe (не mmp32.exe). Managed не трогаем — задаётся до старта wine.
set -uo pipefail

PREFIX="${WINEPREFIX:-${HOME}/Saumi}"
LOG="${PREFIX}/logs/taskbar_fix.log"
POLL_SEC="${SAUMI_TASKBAR_POLL:-1}"

mkdir -p "$(dirname "$LOG")"
echo "========== taskbar_fix $(date '+%F %T') pid=$$ ==========" >>"$LOG"

command -v wmctrl >/dev/null 2>&1 || { echo "WARN: нет wmctrl" >>"$LOG"; exit 0; }
command -v xprop >/dev/null 2>&1 || { echo "WARN: нет xprop" >>"$LOG"; exit 0; }

saumi_running() {
  pgrep -u "$(id -u)" -f '[m]mp32\.exe' >/dev/null 2>&1 \
    || pgrep -u "$(id -u)" -f '[E]xplorerWindow' >/dev/null 2>&1 \
    || pgrep -u "$(id -u)" -f '[s]aumi\.exe' >/dev/null 2>&1
}

fix_one() {
  local id="$1"
  [[ -n "$id" ]] || return 0
  xprop -id "$id" -f _NET_WM_WINDOW_TYPE 32a -set _NET_WM_WINDOW_TYPE _NET_WM_WINDOW_TYPE_NORMAL >/dev/null 2>&1 || true
  xprop -id "$id" -remove _NET_WM_STATE >/dev/null 2>&1 || true
  wmctrl -i -r "$id" -b remove,skip_taskbar,above >/dev/null 2>&1 || true
}

list_ids() {
  wmctrl -lx 2>/dev/null | awk 'BEGIN{IGNORECASE=1}
    /saumi\.exe|mmp32\.exe|explorerwindow/ {print $1}'
}

gone=0
while true; do
  if ! saumi_running; then
    gone=$((gone + 1))
    [[ "$gone" -ge 45 ]] && break
    sleep 1
    continue
  fi
  gone=0
  while read -r id; do
    [[ -n "$id" ]] || continue
    fix_one "$id"
  done < <(list_ids)
  sleep "$POLL_SEC"
done

echo "exit $(date '+%F %T')" >>"$LOG"
