#!/usr/bin/env bash
# Старт SasCoreService (SAUMI Application Server / ГИС ГМП).
set -euo pipefail

W="${SASCORE_PREFIX:-${HOME}/SasCoreWine}"
APP="${W}/drive_c/SaumiAppServer"
CORE="${APP}/Core"
LOG_DIR="${W}/logs"
LOG="${LOG_DIR}/sascore.log"
WINE="${W}/runtime/wine-11.0/bin/wine"
[[ -x "$WINE" ]] || WINE="$(command -v wine)"
SYNC_ARG="${SASCORE_SYNC:-/syncfull}"

mkdir -p "$LOG_DIR"
if [[ -f "$LOG" ]]; then
  mv -f "$LOG" "${LOG}.prev" || true
fi
ts() { date '+%F %T'; }
log() { echo "[$(ts)] $*" | tee -a "$LOG"; }

[[ -f "${CORE}/SasCoreWindowsService.exe" ]] || { echo "FAIL: нет ${CORE}/SasCoreWindowsService.exe"; exit 1; }
[[ -x "$WINE" ]] || { echo "FAIL: нет wine"; exit 1; }

NET4="${W}/drive_c/windows/Microsoft.NET/Framework/v4.0.30319"
SYS32="${W}/drive_c/windows/system32"
if [[ ! -f "${NET4}/MSVCR100_CLR0400.dll" && ! -f "${SYS32}/MSVCR100_CLR0400.dll" ]]; then
  echo "FAIL: нет MSVCR100_CLR0400.dll в Framework\\v4.0.30319 / system32 — архив неполный, пересоберите SasCoreWine.tar.gz"
  exit 1
fi

SASCORE_PGREP='[S]asCoreWindowsService\.exe'

if pgrep -u "$(id -u)" -f "$SASCORE_PGREP" >/dev/null 2>&1; then
  log "уже запущена (pid=$(pgrep -u "$(id -u)" -f "$SASCORE_PGREP" | tr '\n' ' '))"
  exit 0
fi

export WINEPREFIX="$W" WINEARCH=win32
export WINEDEBUG="${WINEDEBUG:--all}"
export WINE="${W}/runtime/wine-11.0/bin/wine"
export WINESERVER="${W}/runtime/wine-11.0/bin/wineserver"
[[ -x "$WINE" ]] || WINE="$(command -v wine)"
export PATH="${W}/runtime/wine-11.0/bin:${PATH}"
export WINEDLLOVERRIDES="${WINEDLLOVERRIDES:-mscoree,mscorwks,fusion,System.EnterpriseServices,System.Transactions=native}"
export DISPLAY="${DISPLAY:-:0}"
[[ -z "${XAUTHORITY:-}" && -f "${HOME}/.Xauthority" ]] && export XAUTHORITY="${HOME}/.Xauthority"
export ORACLE_HOME="C:\\oracle"
export TNS_ADMIN="C:\\oracle\\NETWORK\\ADMIN"
export NLS_LANG="AMERICAN_AMERICA.CL8MSWIN1251"

# ODP.NET 4: файл уже в C:\oracle\odp.net\bin\4; fusion ищет его в GAC v4.
ODP_SRC="${W}/drive_c/oracle/odp.net/bin/4/Oracle.DataAccess.dll"
ODP_GAC="${W}/drive_c/windows/Microsoft.NET/assembly/GAC_MSIL/Oracle.DataAccess/v4.0_4.112.3.0__89b483f429c47342/Oracle.DataAccess.dll"
if [[ -f "$ODP_SRC" && ! -f "$ODP_GAC" ]]; then
  mkdir -p "$(dirname "$ODP_GAC")"
  cp -f "$ODP_SRC" "$ODP_GAC"
  log "ODP.NET 4.112.3.0 → GAC_MSIL"
fi

# Wrapper.dll грузится из папки сборки System.EnterpriseServices (GAC), не только из Framework.
ES_WRAP="${NET4}/System.EnterpriseServices.Wrapper.dll"
ES_THUNK="${NET4}/System.EnterpriseServices.Thunk.dll"
if [[ -f "$ES_WRAP" ]]; then
  cp -f "$ES_WRAP" "${SYS32}/System.EnterpriseServices.Wrapper.dll"
  [[ -f "$ES_THUNK" ]] && cp -f "$ES_THUNK" "${SYS32}/System.EnterpriseServices.Thunk.dll"
  shopt -s nullglob
  for d in \
    "${W}/drive_c/windows/Microsoft.NET/assembly/GAC_MSIL/System.EnterpriseServices"/v4.0_* \
    "${W}/drive_c/windows/Microsoft.NET/assembly/GAC_32/System.EnterpriseServices"/v4.0_*
  do
    [[ -d "$d" ]] || continue
    cp -f "$ES_WRAP" "$d/"
    [[ -f "$ES_THUNK" ]] && cp -f "$ES_THUNK" "$d/"
  done
  shopt -u nullglob
  log "OK System.EnterpriseServices.Wrapper.dll → GAC + system32"
else
  log "WARN нет ${ES_WRAP}"
fi

# /console держит процесс через Console.ReadKey — нужен pty (script), не редирект в файл.
"$WINE" reg add 'HKCU\Software\Wine\WineDbg' /v ShowCrashDialog /t REG_DWORD /d 0 /f >/dev/null 2>&1 || true
"$WINE" reg add 'HKCU\Software\Wine\DllOverrides' /v System.EnterpriseServices /t REG_SZ /d native /f >/dev/null 2>&1 || true
"$WINE" reg add 'HKCU\Software\Wine\DllOverrides' /v System.Transactions /t REG_SZ /d native /f >/dev/null 2>&1 || true
log "старт SasCoreService PREFIX=$W /console $SYNC_ARG DISPLAY=$DISPLAY WINEDLLOVERRIDES=$WINEDLLOVERRIDES"
cd "$CORE"
setsid script -q -c "\"$WINE\" 'C:\\SaumiAppServer\\Core\\SasCoreWindowsService.exe' /console $SYNC_ARG" /dev/null &
sleep 8

if ! pgrep -u "$(id -u)" -f "$SASCORE_PGREP" >/dev/null 2>&1; then
  log "FAIL: процесс не появился — см. $LOG"
  tail -n 40 "$LOG" || true
  exit 1
fi

PID="$(pgrep -u "$(id -u)" -f "$SASCORE_PGREP" | head -1)"
log "OK процесс pid=$PID"

for i in 1 2 3 4 5 6 7 8 9 10 11 12; do
  LISTEN="$(ss -lntp 2>/dev/null | grep -E "pid=$PID[, ]" || true)"
  if [[ -n "$LISTEN" ]]; then
    log "OK есть LISTEN:"
    echo "$LISTEN" | while read -r l; do log "  $l"; done
    break
  fi
  sleep 2
done
if ! ss -lntp 2>/dev/null | grep -qE "pid=$PID[, ]"; then
  log "WARN: пока нет LISTEN у pid=$PID"
  grep -iE 'Core is STARTED|Core is STOPPED|Exception|Settings\.xml|connection|\.NET' "$LOG" 2>/dev/null \
    | tail -n 15 | while read -r l; do log "  log: $l"; done || true
fi
log "старт завершён"
