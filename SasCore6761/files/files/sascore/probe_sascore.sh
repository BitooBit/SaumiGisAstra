#!/usr/bin/env bash
# Проверка SasCoreService (SAUMI Application Server / ГИС ГМП).
set -euo pipefail
W="${SASCORE_PREFIX:-${HOME}/SasCoreWine}"
APP="${W}/drive_c/SaumiAppServer"
CORE="${APP}/Core"
LOG_DIR="${W}/logs"
LOG="${LOG_DIR}/sascore_probe.log"
mkdir -p "$LOG_DIR"
ts() { date '+%F %T'; }
log() { echo "[$(ts)] $*" | tee -a "$LOG"; }

SASCORE_PGREP='[S]asCoreWindowsService\.exe'
OK=1
log "=== probe SasCoreService PREFIX=$W ==="

NET4="${W}/drive_c/windows/Microsoft.NET/Framework/v4.0.30319"
if [[ -f "${NET4}/mscorlib.dll" ]]; then
  log "OK .NET 4: $NET4"
else
  log "FAIL нет .NET 4 (mscorlib.dll)"
  OK=0
fi

[[ -f "${CORE}/SasCoreWindowsService.exe" ]] && log "OK файл службы" || { log "FAIL нет exe"; OK=0; }
[[ -f "${CORE}/AppServerCore.dll" ]] && log "OK AppServerCore.dll" || { log "FAIL нет AppServerCore.dll"; OK=0; }
[[ -f "${CORE}/Settings.xml" ]] && log "OK Settings.xml" || { log "WARN нет Settings.xml"; }
if [[ -f "${CORE}/Settings.xml" ]] && grep -q '8QzkIvdEXU2aUVXLX1JNIg' "${CORE}/Settings.xml"; then
  log "FAIL Settings.xml ещё с Windows DPAPI — Wine не расшифрует"
  OK=0
fi
[[ -f "${CORE}/CoreConfig.xml" ]] && log "OK CoreConfig.xml" || { log "WARN нет CoreConfig.xml"; }

PID=""
if pgrep -u "$(id -u)" -f "$SASCORE_PGREP" >/dev/null 2>&1; then
  PID="$(pgrep -u "$(id -u)" -f "$SASCORE_PGREP" | head -1)"
  log "OK процесс pid=$PID"
else
  log "FAIL процесс не запущен"
  OK=0
fi

LISTEN_N=0
if [[ -n "$PID" ]]; then
  while read -r l; do
    [[ -n "$l" ]] || continue
    LISTEN_N=$((LISTEN_N + 1))
    log "OK LISTEN: $l"
  done < <(ss -lntp 2>/dev/null | grep -E "pid=$PID[, ]" || true)
  [[ "$LISTEN_N" -gt 0 ]] || log "WARN нет LISTEN"
fi

if [[ -n "$PID" ]]; then
  PORTS="$(ss -lntp 2>/dev/null | grep -E "pid=$PID[, ]" | sed -n 's/.*:\([0-9][0-9]*\) .*/\1/p' | sort -u || true)"
  for port in $PORTS; do
    if python3 -c "import socket;s=socket.socket();s.settimeout(2);s.connect(('127.0.0.1',int('$port')));s.close()" 2>/dev/null; then
      log "OK probe TCP :$port connect"
    else
      log "FAIL probe TCP :$port"
      OK=0
    fi
  done
fi

SRV_LOG="${LOG_DIR}/sascore.log"
CORE_LOG=""
for f in "${APP}/Logs/AppServerCoreLogs.log" "${CORE}/AppServerCoreLogs.log"; do
  [[ -f "$f" ]] && CORE_LOG="$f" && break
done
STARTED=0
for f in "$CORE_LOG" "$SRV_LOG"; do
  [[ -n "$f" && -f "$f" ]] || continue
  if tail -n 120 "$f" | grep -q 'Core is STARTED'; then
    log "OK в хвосте лога: Core is STARTED ($f)"
    STARTED=1
    break
  fi
done
[[ "$STARTED" -eq 1 ]] || log "WARN в хвосте лога нет 'Core is STARTED' (см. ${CORE_LOG:-$SRV_LOG})"
if [[ -n "$CORE_LOG" && -f "$CORE_LOG" ]]; then
  if tail -n 120 "$CORE_LOG" | grep -q 'CryptUnprotectData\|DPAPI was unable to decrypt'; then
    log "FAIL DPAPI: не удалось расшифровать Settings.xml (хвост $CORE_LOG)"
    OK=0
  fi
  if tail -n 120 "$CORE_LOG" | grep -q "Can't create component.*IDataAccessComponent"; then
    log "FAIL IDataAccessComponent (хвост $CORE_LOG)"
    OK=0
  fi
fi
if [[ -f "$SRV_LOG" ]]; then
  if grep -qiE 'Exception|Can.t start core|Settings\.xml|v4\.0\.30319' "$SRV_LOG" 2>/dev/null; then
    log "WARN хвост лога службы:"
    tail -n 20 "$SRV_LOG" | while read -r l; do log "  $l"; done
  fi
fi

if [[ -n "$CORE_LOG" ]]; then
  log "есть AppServerCoreLogs.log ($CORE_LOG, $(wc -l <"$CORE_LOG" | tr -d ' ') строк)"
  tail -n 5 "$CORE_LOG" | while read -r l; do log "  corelog: $l"; done || true
fi

if [[ "$OK" -eq 1 && "$LISTEN_N" -gt 0 ]]; then
  log "=== PROBE PASS ==="
  exit 0
fi
if [[ "$OK" -eq 1 ]]; then
  log "=== PROBE PARTIAL (процесс есть, LISTEN нет) ==="
  exit 2
fi
log "=== PROBE FAIL ==="
exit 1
