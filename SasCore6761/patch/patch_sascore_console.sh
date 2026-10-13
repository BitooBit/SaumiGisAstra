#!/usr/bin/env bash
# Патч на стоящий SasCoreWine: DPAPI Settings.xml + wineconsole для /console.
# Полный SasCoreWine.tar.gz не нужен.
#
# Скопировать папку patch/ на сервер и:
#   bash patch_sascore_console.sh
# от пользователя службы (vrsaum). TNS/логин/пароль Oracle — обязательно,
# пока Settings.xml с Windows (Wine их не расшифрует).
#
#   SASCORE_TNS=... SASCORE_USER=... SASCORE_PASSWORD=... bash patch_sascore_console.sh
#
set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
if [[ "$(id -u)" -eq 0 && -n "${SUDO_USER:-}" && "${SUDO_USER}" != "root" ]]; then
  INVOKER="${SUDO_USER}"
  INVOKER_HOME="$(getent passwd "${INVOKER}" | cut -d: -f6)"
  echo "патч от root → запускаем от ${INVOKER}"
  exec runuser -u "${INVOKER}" -- env HOME="${INVOKER_HOME}" DISPLAY="${DISPLAY:-:0}" \
    XAUTHORITY="${INVOKER_HOME}/.Xauthority" \
    SASCORE_TNS="${SASCORE_TNS:-}" SASCORE_USER="${SASCORE_USER:-}" \
    SASCORE_PASSWORD="${SASCORE_PASSWORD:-}" \
    bash "$0" "$@"
fi

W="${SASCORE_PREFIX:-${HOME}/SasCoreWine}"
BIN="${W}/bin"
CORE="${W}/drive_c/SaumiAppServer/Core"
STAMP="$(date +%Y%m%d_%H%M%S)"

[[ -f "${HERE}/start_sascore.sh" && -f "${HERE}/probe_sascore.sh" ]] \
  || { echo "FAIL: рядом нет start_sascore.sh / probe_sascore.sh"; exit 1; }
[[ -f "${HERE}/oracle_settings.cs" && -f "${HERE}/oracle_settings.sh" ]] \
  || { echo "FAIL: рядом нет oracle_settings.cs / oracle_settings.sh"; exit 1; }
[[ -d "${W}/drive_c" && -f "${CORE}/SasCoreWindowsService.exe" ]] \
  || { echo "FAIL: нет prefix ${W}"; exit 1; }

echo "prefix: $W"
if [[ -x "${BIN}/stop_sascore.sh" ]]; then
  bash "${BIN}/stop_sascore.sh" || true
fi
pkill -u "$(id -u)" -f '[S]asCoreWindowsService\.exe' 2>/dev/null || true
pkill -u "$(id -u)" -f '[w]inedbg' 2>/dev/null || true
sleep 1

mkdir -p "$BIN" "${W}/logs"
for s in start_sascore.sh probe_sascore.sh oracle_settings.sh oracle_settings.cs; do
  [[ -f "${BIN}/${s}" ]] && cp -f "${BIN}/${s}" "${BIN}/${s}.bak_${STAMP}" 2>/dev/null || true
  cp -f "${HERE}/${s}" "${BIN}/${s}"
done
chmod 755 "${BIN}/start_sascore.sh" "${BIN}/probe_sascore.sh" "${BIN}/oracle_settings.sh"

echo "OK скрипты обновлены (bak_${STAMP})"
if ! grep -q 'EnterpriseServices.Wrapper' "${BIN}/start_sascore.sh"; then
  echo "FAIL: в start_sascore.sh нет Wrapper.dll"; exit 1
fi
echo "OK Wrapper.dll в start"

# shellcheck source=/dev/null
source "${BIN}/oracle_settings.sh"
compile_oracle_settings "${BIN}/oracle_settings.cs"
echo "OK oracle_settings.exe"

SETTINGS="${CORE}/Settings.xml"
NEED_ORACLE=1
if check_oracle_settings "$SETTINGS"; then
  echo "OK Settings.xml расшифровывается в этом prefix"
  NEED_ORACLE=0
fi

if [[ "$NEED_ORACLE" -eq 1 ]]; then
  echo ""
  echo "Settings.xml с Windows. Нужны параметры Oracle (шифруются DPAPI уже в Wine):"
  echo "  — имя сервиса (TNS / Data Source)"
  echo "  — имя пользователя"
  echo "  — пароль"
  echo ""
  trim() {
    local s="${1:-}"
    s="${s#"${s%%[![:space:]]*}"}"
    s="${s%"${s##*[![:space:]]}"}"
    printf '%s' "$s"
  }
  TNS="$(trim "${SASCORE_TNS:-}")"
  USER="$(trim "${SASCORE_USER:-}")"
  PASS="${SASCORE_PASSWORD:-}"
  if [[ -z "$TNS" ]]; then
    [[ -t 0 ]] || { echo "FAIL: задайте SASCORE_TNS"; exit 1; }
    read -r -p "Имя сервиса (TNS / Data Source): " TNS
    TNS="$(trim "$TNS")"
  fi
  if [[ -z "$USER" ]]; then
    [[ -t 0 ]] || { echo "FAIL: задайте SASCORE_USER"; exit 1; }
    read -r -p "Имя пользователя: " USER
    USER="$(trim "$USER")"
  fi
  if [[ -z "$PASS" ]]; then
    [[ -t 0 ]] || { echo "FAIL: задайте SASCORE_PASSWORD"; exit 1; }
    read -r -s -p "Пароль пользователя: " PASS
    echo
  fi
  [[ -n "$TNS" && -n "$USER" && -n "$PASS" ]] || { echo "FAIL: пустые параметры Oracle"; exit 1; }
  cp -f "$SETTINGS" "${W}/logs/Settings.xml.bak_${STAMP}" 2>/dev/null || true
  write_oracle_settings "$TNS" "$USER" "$PASS"
  check_oracle_settings "$SETTINGS"
  echo "OK Settings.xml записан и расшифровывается"
fi

bash "${BIN}/start_sascore.sh"
echo "--- probe ---"
bash "${BIN}/probe_sascore.sh" || true
echo "готово. лог: ${W}/logs/sascore.log"
