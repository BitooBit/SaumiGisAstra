#!/usr/bin/env bash
# Остановка Guardant Net (NNKSRV32) текущего пользователя.
set -euo pipefail
W="${SAUMI_PREFIX:-${HOME}/Saumi}"
[[ -d "${W}/drive_c/SAUMI" ]] || W="${HOME}/SAUMIwine"
LOG="${W}/logs/licence_server.log"
mkdir -p "${W}/logs"
ts() { date '+%F %T'; }
log() { echo "[$(ts)] $*" | tee -a "$LOG"; }

PIDS="$(pgrep -u "$(id -u)" -f 'NNKSRV32' || true)"
if [[ -z "$PIDS" ]]; then
  log "не запущена"
  exit 0
fi
log "остановка pid=$PIDS"
kill $PIDS 2>/dev/null || true
sleep 2
PIDS2="$(pgrep -u "$(id -u)" -f 'NNKSRV32' || true)"
if [[ -n "$PIDS2" ]]; then
  log "force kill $PIDS2"
  kill -9 $PIDS2 2>/dev/null || true
fi
log "остановлена"
