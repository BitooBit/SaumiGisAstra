#!/usr/bin/env bash
# патч 3.22: continuous hide мелкого chrome + чёрного квадрата;
# не выходить при кратком пропадании процессов SAUMI.
#
# Проблема 3.19: после STARTUP_SEC крупные 300×300 снова в углу
# («large AFTER startup — leave alone»), а hide умирал когда
# mmp32/saumi на миг пропадали после Excel — дальше окно не убиралось.
#
# Схема:
# A) chrome top-left (узкий/низкий) — continuous весь сеанс
# B) чёрный квадрат ~300×300 top-left — continuous (не только старт)
# C) крупные формы (≥500 по стороне) — НИКОГДА (отчёт/основное)
# D) empty+active chrome/квадрат — НЕ protect навсегда
# E) выход hide только после EXIT_GRACE_SEC без процессов SAUMI
#
set -uo pipefail

PREFIX="${WINEPREFIX:-${HOME}/Saumi}"
LOG="${PREFIX}/logs/hide_windows.log"
OFF_X=-12000
OFF_Y=-12000
POLL_SEC=1
GRACE_SEC="${SAUMI_HIDE_GRACE:-0}"
EXIT_GRACE_SEC="${SAUMI_HIDE_EXIT_GRACE:-45}"

MAX_X="${SAUMI_HIDE_MAX_X:-80}"
MAX_Y="${SAUMI_HIDE_MAX_Y:-120}"

# мелкий wine-chrome / titlebar (у SAUMI таких окон нет)
SM_W_MIN="${SAUMI_HIDE_SM_W_MIN:-1}"
SM_W_MAX="${SAUMI_HIDE_SM_W_MAX:-450}"
SM_H_MIN="${SAUMI_HIDE_SM_H_MIN:-1}"
SM_H_MAX="${SAUMI_HIDE_SM_H_MAX:-160}"

# чёрный splash ~300×300
SQ_MIN="${SAUMI_HIDE_SQ_MIN:-180}"
SQ_MAX="${SAUMI_HIDE_SQ_MAX:-420}"

# отчёт / основное — не трогать даже empty top-left
FORM_SIDE="${SAUMI_HIDE_FORM_SIDE:-500}"

mkdir -p "$(dirname "$LOG")"
{
  echo "========== hide 3.22 chrome+square continuous $(date '+%F %T') pid=$$ =========="
  echo "pos X<=${MAX_X} Y<=${MAX_Y} grace=${GRACE_SEC}s exit_grace=${EXIT_GRACE_SEC}s"
  echo "chrome: ${SM_W_MIN}-${SM_W_MAX}x${SM_H_MIN}-${SM_H_MAX}"
  echo "square: ${SQ_MIN}-${SQ_MAX} form_side>=${FORM_SIDE} never"
} >>"$LOG"

if ! command -v xdotool >/dev/null 2>&1; then
  echo "WARN: нет xdotool — выход" >>"$LOG"
  exit 0
fi
command -v xprop >/dev/null 2>&1 || {
  echo "WARN: нет xprop — выход" >>"$LOG"
  exit 0
}

declare -A EMPTY_SINCE
declare -A PROTECTED
declare -A MOVED
declare -A SKIP_LOGGED

saumi_running() {
  pgrep -u "$(id -u)" -f '[m]mp32\.exe' >/dev/null 2>&1 \
    || pgrep -u "$(id -u)" -f '[E]xplorerWindow' >/dev/null 2>&1 \
    || pgrep -u "$(id -u)" -f '[s]aumi\.exe' >/dev/null 2>&1
}

is_empty_name() {
  local name="$1"
  [[ -z "$name" || "$name" == '""' || "$name" == "not found." || "$name" == "(unset)" ]]
}

is_menu_type() {
  local wid="$1" wtype
  wtype="$(xprop -id "$wid" _NET_WM_WINDOW_TYPE 2>/dev/null || true)"
  case "$wtype" in
    *_NET_WM_WINDOW_TYPE_MENU*|*_NET_WM_WINDOW_TYPE_DROPDOWN_MENU*|\
    *_NET_WM_WINDOW_TYPE_POPUP_MENU*|*_NET_WM_WINDOW_TYPE_COMBO*|\
    *_NET_WM_WINDOW_TYPE_TOOLTIP*|*_NET_WM_WINDOW_TYPE_UTILITY*)
      return 0 ;;
  esac
  return 1
}

geom_of() {
  local wid="$1" geo
  gX=; gY=; gW=; gH=
  geo="$(xdotool getwindowgeometry --shell "$wid" 2>/dev/null || true)"
  gX="$(echo "$geo" | sed -n 's/^X=//p')"
  gY="$(echo "$geo" | sed -n 's/^Y=//p')"
  gW="$(echo "$geo" | sed -n 's/^WIDTH=//p')"
  gH="$(echo "$geo" | sed -n 's/^HEIGHT=//p')"
  [[ -n "$gX" && -n "$gY" && -n "$gW" && -n "$gH" ]]
}

is_top_left() {
  local x="$1" y="$2"
  [[ "$x" -ge -20 && "$x" -le "$MAX_X" && "$y" -ge -20 && "$y" -le "$MAX_Y" ]]
}

is_chrome() {
  local w="$1" h="$2"
  [[ "$w" -ge "$SM_W_MIN" && "$w" -le "$SM_W_MAX" \
  && "$h" -ge "$SM_H_MIN" && "$h" -le "$SM_H_MAX" ]]
}

is_black_square() {
  local w="$1" h="$2"
  [[ "$w" -ge "$SQ_MIN" && "$w" -le "$SQ_MAX" \
  && "$h" -ge "$SQ_MIN" && "$h" -le "$SQ_MAX" ]]
}

is_real_form() {
  local w="$1" h="$2"
  [[ "$w" -ge "$FORM_SIDE" || "$h" -ge "$FORM_SIDE" ]]
}

already_offscreen() {
  [[ "$1" -lt -1000 ]]
}

move_away() {
  local wid="$1" why="$2" w="$3" h="$4" x="$5" y="$6"
  xdotool windowmove "$wid" "$OFF_X" "$OFF_Y" 2>>"$LOG" || true
  xdotool windowminimize "$wid" 2>>"$LOG" || true
  MOVED[$wid]=1
  unset "EMPTY_SINCE[$wid]"
  echo "move wid=$wid @${x},${y} ${w}x${h} reason=$why $(date '+%H:%M:%S')" >>"$LOG"
}

scan_once() {
  local wid class name netname active now age gX gY gW gH kind
  active="$(xdotool getactivewindow 2>/dev/null || true)"
  now="$(date +%s)"

  while read -r wid; do
    [[ -n "$wid" ]] || continue
    class="$(xprop -id "$wid" WM_CLASS 2>/dev/null || true)"
    case "$class" in
      *saumi.exe*|*Saumi.exe*) ;;
      *) continue ;;
    esac

    is_menu_type "$wid" && continue

    name="$(xprop -id "$wid" WM_NAME 2>/dev/null | sed 's/^[^=]*= //;s/^"//;s/"$//' || true)"
    netname="$(xprop -id "$wid" _NET_WM_NAME 2>/dev/null | sed 's/^[^=]*= //;s/^"//;s/"$//' || true)"

    # titled → настоящее окно SAUMI, навсегда protect
    if ! is_empty_name "$name" || ! is_empty_name "$netname"; then
      PROTECTED[$wid]=1
      unset "EMPTY_SINCE[$wid]"
      continue
    fi
    [[ -n "${PROTECTED[$wid]+x}" ]] && continue

    geom_of "$wid" || continue

    if already_offscreen "$gX"; then
      MOVED[$wid]=1
      unset "EMPTY_SINCE[$wid]"
      continue
    fi

    # крупные формы (отчёт/основное) — даже empty и даже active
    if is_real_form "$gW" "$gH"; then
      if [[ -z "${SKIP_LOGGED[F$wid]+x}" ]]; then
        SKIP_LOGGED[F$wid]=1
        echo "skip wid=$wid @${gX},${gY} ${gW}x${gH} real-form — leave alone $(date '+%H:%M:%S')" >>"$LOG"
      fi
      unset "EMPTY_SINCE[$wid]"
      continue
    fi

    kind=""
    if is_chrome "$gW" "$gH"; then
      kind="chrome"
    elif is_black_square "$gW" "$gH"; then
      kind="square"
    else
      if [[ -z "${SKIP_LOGGED[S$wid]+x}" ]]; then
        SKIP_LOGGED[S$wid]=1
        echo "skip wid=$wid @${gX},${gY} ${gW}x${gH} size mismatch $(date '+%H:%M:%S')" >>"$LOG"
      fi
      unset "EMPTY_SINCE[$wid]"
      continue
    fi

    # active chrome/квадрат — всё равно убираем (не permanent protect)
    if [[ -n "$active" && "$wid" == "$active" ]]; then
      kind="${kind}-active"
    fi

    if ! is_top_left "$gX" "$gY"; then
      if [[ -z "${SKIP_LOGGED[T$wid]+x}" ]]; then
        SKIP_LOGGED[T$wid]=1
        echo "info wid=$wid @${gX},${gY} ${gW}x${gH} ${kind} but NOT top-left $(date '+%H:%M:%S')" >>"$LOG"
      fi
      unset "EMPTY_SINCE[$wid]"
      continue
    fi

    if [[ "$GRACE_SEC" -le 0 ]]; then
      move_away "$wid" "${kind}" "$gW" "$gH" "$gX" "$gY"
      continue
    fi

    if [[ -z "${EMPTY_SINCE[$wid]+x}" ]]; then
      EMPTY_SINCE[$wid]="$now"
      continue
    fi
    age=$((now - EMPTY_SINCE[$wid]))
    if [[ "$age" -lt "$GRACE_SEC" ]]; then
      continue
    fi

    move_away "$wid" "${kind}>${GRACE_SEC}s" "$gW" "$gH" "$gX" "$gY"
  done < <(xdotool search --class saumi.exe 2>/dev/null || true)
}

for _ in $(seq 1 90); do
  saumi_running && break
  sleep 1
done
if ! saumi_running; then
  echo "SAUMI не найден — выход $(date '+%F %T')" >>"$LOG"
  exit 0
fi

echo "loop start $(date '+%F %T')" >>"$LOG"
gone_since=""
while true; do
  if saumi_running; then
    gone_since=""
    scan_once
  else
    if [[ -z "$gone_since" ]]; then
      gone_since="$(date +%s)"
      echo "SAUMI processes gone — wait ${EXIT_GRACE_SEC}s $(date '+%H:%M:%S')" >>"$LOG"
    else
      now="$(date +%s)"
      if [[ $((now - gone_since)) -ge "$EXIT_GRACE_SEC" ]]; then
        echo "SAUMI stopped $(date '+%F %T') moved=${#MOVED[@]}" >>"$LOG"
        exit 0
      fi
    fi
  fi
  sleep "$POLL_SEC"
done
