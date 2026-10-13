#!/usr/bin/env bash
# Стоп SasCoreService для текущего пользователя.
set -euo pipefail
W="${SASCORE_PREFIX:-${HOME}/SasCoreWine}"
LOG_DIR="${W}/logs"
LOG="${LOG_DIR}/sascore.log"
mkdir -p "$LOG_DIR"
ts() { date '+%F %T'; }
log() { echo "[$(ts)] $*" | tee -a "$LOG"; }

SASCORE_PGREP='[S]asCoreWindowsService\.exe'
PIDS="$(pgrep -u "$(id -u)" -f "$SASCORE_PGREP" || true)"
if [[ -z "$PIDS" ]]; then
  log "не запущена"
  exit 0
fi
log "стоп pid=$PIDS"
kill $PIDS 2>/dev/null || true
sleep 2
PIDS2="$(pgrep -u "$(id -u)" -f "$SASCORE_PGREP" || true)"
if [[ -n "$PIDS2" ]]; then
  log "kill -9 $PIDS2"
  kill -9 $PIDS2 2>/dev/null || true
fi
WS="${W}/runtime/wine-11.0/bin/wineserver"
[[ -x "$WS" ]] && "$WS" -k 2>/dev/null || true
log "стоп OK"
