#!/usr/bin/env bash
# Режим 1 (Managed=N): «Свернуть» под Wine часто вешает окно.
# Guard: Iconic → map + увести за экран; активация с панели → вернуть.
set -uo pipefail

PREFIX="${WINEPREFIX:-${HOME}/Saumi}"
LOG="${PREFIX}/logs/minimize_guard.log"
OFF_X=-12000
OFF_Y=-12000
ON_X="${SAUMI_MIN_RESTORE_X:-60}"
ON_Y="${SAUMI_MIN_RESTORE_Y:-40}"
POLL_SEC="${SAUMI_MIN_POLL:-0.35}"

mkdir -p "$(dirname "$LOG")"
echo "========== minimize_guard $(date '+%F %T') pid=$$ ==========" >>"$LOG"

command -v xdotool >/dev/null 2>&1 || { echo "WARN: нет xdotool" >>"$LOG"; exit 0; }
command -v xprop >/dev/null 2>&1 || { echo "WARN: нет xprop" >>"$LOG"; exit 0; }

declare -A FAKE_MIN

saumi_running() {
  pgrep -u "$(id -u)" -f '[m]mp32\.exe' >/dev/null 2>&1 \
    || pgrep -u "$(id -u)" -f '[E]xplorerWindow' >/dev/null 2>&1 \
    || pgrep -u "$(id -u)" -f '[s]aumi\.exe' >/dev/null 2>&1
}

is_saumi_class() {
  case "$1" in
    *mmp32*|*Mmp32*|*saumi*|*Saumi*|*explorerwindow*|*ExplorerWindow*) return 0 ;;
    *) return 1 ;;
  esac
}

is_menu_type() {
  local wtype
  wtype="$(xprop -id "$1" _NET_WM_WINDOW_TYPE 2>/dev/null || true)"
  case "$wtype" in
    *_NET_WM_WINDOW_TYPE_MENU*|*_NET_WM_WINDOW_TYPE_DROPDOWN_MENU*|\
    *_NET_WM_WINDOW_TYPE_POPUP_MENU*|*_NET_WM_WINDOW_TYPE_COMBO*|\
    *_NET_WM_WINDOW_TYPE_TOOLTIP*) return 0 ;;
  esac
  return 1
}

is_iconic() {
  local st
  st="$(xprop -id "$1" WM_STATE 2>/dev/null || true)"
  [[ "$st" == *Iconic* ]]
}

list_saumi_wids() {
  local wid class
  if command -v wmctrl >/dev/null 2>&1; then
    while read -r hex _; do
      [[ -n "$hex" ]] || continue
      printf '%d\n' "$hex" 2>/dev/null || true
    done < <(wmctrl -lx 2>/dev/null | awk 'BEGIN{IGNORECASE=1} /mmp32|saumi|explorerwindow/ {print $1}')
  fi
  for pat in mmp32 saumi.exe Saumi.exe ExplorerWindow; do
    while read -r wid; do
      [[ -n "$wid" ]] || continue
      echo "$wid"
    done < <(xdotool search --classname "$pat" 2>/dev/null || true)
  done
}

fake_minimize() {
  local wid="$1"
  xdotool windowmap "$wid" 2>>"$LOG" || true
  xdotool windowmove "$wid" "$OFF_X" "$OFF_Y" 2>>"$LOG" || true
  FAKE_MIN[$wid]=1
  echo "fake_min wid=$wid $(date '+%H:%M:%S')" >>"$LOG"
}

restore_if_active() {
  local wid="$1" active
  active="$(xdotool getactivewindow 2>/dev/null || true)"
  [[ -n "$active" && "$active" == "$wid" ]] || return 0
  xdotool windowmap "$wid" 2>>"$LOG" || true
  xdotool windowmove "$wid" "$ON_X" "$ON_Y" 2>>"$LOG" || true
  unset "FAKE_MIN[$wid]"
  echo "restore wid=$wid $(date '+%H:%M:%S')" >>"$LOG"
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

  while read -r wid; do
    [[ -n "$wid" ]] || continue
    class="$(xprop -id "$wid" WM_CLASS 2>/dev/null || true)"
    is_saumi_class "$class" || continue
    is_menu_type "$wid" && continue

    if [[ -n "${FAKE_MIN[$wid]+x}" ]]; then
      restore_if_active "$wid"
      continue
    fi
    if is_iconic "$wid"; then
      fake_minimize "$wid"
    fi
  done < <(list_saumi_wids | awk '!a[$0]++')

  sleep "$POLL_SEC"
done

echo "exit $(date '+%F %T')" >>"$LOG"
