#!/usr/bin/env bash
# Проверка / запись Settings.xml через DPAPI MachineKey (как ConfigurationUtils).
# Только внутри Wine-prefix SasCoreWine.
set -euo pipefail

W="${SASCORE_PREFIX:-${HOME}/SasCoreWine}"
APP="${W}/drive_c/SaumiAppServer"
CORE="${APP}/Core"

WINE="${W}/runtime/wine-11.0/bin/wine"
[[ -x "$WINE" ]] || WINE="$(command -v wine)"
CSC="${W}/drive_c/windows/Microsoft.NET/Framework/v4.0.30319/csc.exe"
EXE="${CORE}/oracle_settings.exe"

export WINEPREFIX="$W" WINEARCH=win32
export WINEDEBUG="${WINEDEBUG:--all}"
export WINEDLLOVERRIDES="${WINEDLLOVERRIDES:-mscoree,mscorwks,fusion=native}"
export DISPLAY="${DISPLAY:-:0}"
[[ -z "${XAUTHORITY:-}" && -f "${HOME}/.Xauthority" ]] && export XAUTHORITY="${HOME}/.Xauthority"

compile_oracle_settings() {
  local src="$1"
  [[ -f "$src" ]] || { echo "FAIL: нет $src"; return 1; }
  [[ -f "$CSC" ]] || { echo "FAIL: нет csc.exe (нет полного .NET 4 в prefix)"; return 1; }
  [[ -f "${CORE}/CommonUtils.dll" ]] || { echo "FAIL: нет CommonUtils.dll"; return 1; }
  cp -f "$src" "${CORE}/oracle_settings.cs"
  "$WINE" "C:\\windows\\Microsoft.NET\\Framework\\v4.0.30319\\csc.exe" /nologo /t:exe /platform:x86 \
    /out:"C:\\SaumiAppServer\\Core\\oracle_settings.exe" \
    /r:"C:\\SaumiAppServer\\Core\\CommonUtils.dll" \
    "C:\\SaumiAppServer\\Core\\oracle_settings.cs"
  [[ -f "$EXE" ]] || { echo "FAIL: csc не собрал oracle_settings.exe"; return 1; }
}

win_path() {
  local p="$1"
  p="${p#${W}/drive_c/}"
  p="${p//\//\\}"
  printf 'C:\\%s' "$p"
}

check_oracle_settings() {
  local xml="${1:-${CORE}/Settings.xml}"
  "$WINE" "C:\\SaumiAppServer\\Core\\oracle_settings.exe" check "$(win_path "$xml")"
}

write_oracle_settings() {
  local tns="$1" user="$2" pass="$3"
  [[ -n "$tns" && -n "$user" && -n "$pass" ]] || { echo "FAIL: пустые TNS/user/password"; return 1; }
  export SASCORE_PASSWORD="$pass"
  "$WINE" "C:\\SaumiAppServer\\Core\\oracle_settings.exe" write "$tns" "$user" \
    "C:\\SaumiAppServer\\Core\\Settings.xml" \
    "C:\\SaumiAppServer\\Settings.xml"
  unset SASCORE_PASSWORD
  chmod 600 "${CORE}/Settings.xml" "${APP}/Settings.xml" 2>/dev/null || true
}

# --- CLI, если вызван как скрипт ---
if [[ "${BASH_SOURCE[0]}" == "$0" ]]; then
  src="${ORACLE_SETTINGS_CS:-$(cd "$(dirname "$0")" && pwd)/oracle_settings.cs}"
  action="${1:-}"
  case "$action" in
    compile)
      compile_oracle_settings "${2:-$src}"
      ;;
    check)
      compile_oracle_settings "$src"
      check_oracle_settings "${2:-}"
      ;;
    write)
      compile_oracle_settings "$src"
      write_oracle_settings "${2:-}" "${3:-}" "${4:-}"
      ;;
    *)
      echo "usage: oracle_settings.sh compile [oracle_settings.cs]"
      echo "       oracle_settings.sh check [Settings.xml]"
      echo "       oracle_settings.sh write TNS USER PASSWORD"
      exit 2
      ;;
  esac
fi
