#!/usr/bin/env bash
# Старт Guardant Net (NNKSRV32) для текущего пользователя.
# Логи: ~/Saumi/logs/licence_server.log (или $SAUMI_PREFIX/logs)
set -euo pipefail

W="${SAUMI_PREFIX:-${HOME}/Saumi}"
[[ -d "${W}/drive_c/SAUMI" ]] || W="${HOME}/SAUMIwine"
SAUMI_DIR="${W}/drive_c/SAUMI"
LOG_DIR="${W}/logs"
LOG="${LOG_DIR}/licence_server.log"
WINE="${W}/runtime/wine-11.0/bin/wine"
[[ -x "$WINE" ]] || WINE="$(command -v wine)"

mkdir -p "$LOG_DIR"
ts() { date '+%F %T'; }

log() { echo "[$(ts)] $*" | tee -a "$LOG"; }

[[ -f "${SAUMI_DIR}/NNKSRV32.EXE" ]] || { echo "FAIL: нет ${SAUMI_DIR}/NNKSRV32.EXE"; exit 1; }
[[ -f "${SAUMI_DIR}/NOVEX32.DLL" ]] || { echo "FAIL: нет ${SAUMI_DIR}/NOVEX32.DLL"; exit 1; }
[[ -x "$WINE" ]] || { echo "FAIL: нет wine"; exit 1; }

if command -v lsusb >/dev/null 2>&1; then
  if ! lsusb 2>/dev/null | grep -qiE 'ID 0a89:|Aktiv.*Guardant|Guardant'; then
    echo ""
    echo "============================================"
    echo "  КЛЮЧ GUARDANT НЕ НАЙДЕН"
    echo "============================================"
    echo "  Вставьте USB-ключ Guardant и снова запустите старт."
    echo "============================================"
    echo ""
    log "FAIL: USB-ключ Guardant не найден — вставьте ключ и продолжите"
    exit 1
  fi
  log "OK ключ: $(lsusb 2>/dev/null | grep -iE 'ID 0a89:|Guardant' | head -1)"
fi

if pgrep -u "$(id -u)" -f 'NNKSRV32' >/dev/null 2>&1; then
  log "уже запущена (pid=$(pgrep -u "$(id -u)" -f 'NNKSRV32' | tr '\n' ' '))"
  exit 0
fi

export WINEPREFIX="$W" WINEARCH=win32 WINEDEBUG=-all
export DISPLAY="${DISPLAY:-:0}"
[[ -z "${XAUTHORITY:-}" && -f "${HOME}/.Xauthority" ]] && export XAUTHORITY="${HOME}/.Xauthority"

log "старт NNKSRV32 PREFIX=$W DISPLAY=$DISPLAY"
cd "$SAUMI_DIR"
# прямой запуск EXE (надёжнее, чем wine start /unix)
setsid "$WINE" "C:\\SAUMI\\NNKSRV32.EXE" >>"$LOG" 2>&1 < /dev/null &
sleep 5

if ! pgrep -u "$(id -u)" -f 'NNKSRV32' >/dev/null 2>&1; then
  log "FAIL: процесс не появился — см. $LOG"
  tail -n 30 "$LOG" || true
  exit 1
fi

PID="$(pgrep -u "$(id -u)" -f 'NNKSRV32' | head -1)"
log "OK процесс pid=$PID"

# порты
for i in 1 2 3 4 5 6; do
  if ss -lntp 2>/dev/null | grep -q ':3182'; then
    log "OK порт TCP 3182 слушает"
    break
  fi
  sleep 1
done
ss -luntp 2>/dev/null | grep -E '3182|3183|3184|3185' | while read -r l; do log "  $l"; done || true

CODE="$(curl -sS -m 3 -o /tmp/nnksrv_http_$$.html -w '%{http_code}' http://127.0.0.1:3185/ 2>/dev/null || echo 000)"
if [[ "$CODE" == "200" ]]; then
  log "OK HTTP монитор http://127.0.0.1:3185/ → $CODE"
  if command -v iconv >/dev/null; then
    iconv -f windows-1251 -t utf-8 "/tmp/nnksrv_http_$$.html" 2>/dev/null \
      | tr '\n' ' ' | sed 's/<[^>]*>/ /g;s/  */ /g' | fold -s -w 120 | head -5 \
      | while read -r l; do log "  status: $l"; done || true
  fi
else
  log "WARN HTTP монитор код=$CODE (служба могла ещё подниматься)"
fi
rm -f "/tmp/nnksrv_http_$$.html"
log "старт завершён"
