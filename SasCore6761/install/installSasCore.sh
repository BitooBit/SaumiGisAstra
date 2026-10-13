#!/usr/bin/env bash
# SasCoreService — установщик «один клик».
# Комплект на сервер: installSasCore.sh + SasCoreWine.tar.gz
#
#   sudo bash installSasCore.sh
#
# Целый prefix (exe, .NET 4, GAC System.dll, NDP v4, wine) → tar не трогаем.
# Битый или пустая папка → снос, tar, восстановление Settings.xml.
# --reinstall — принудительный сброс.
#
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ARCHIVE="${SASCORE_ARCHIVE:-${SCRIPT_DIR}/SasCoreWine.tar.gz}"
INSTALL_LOG="${SCRIPT_DIR}/sascore_install.log"
INSTALLER_REV="2026-09-11-sascore-v12"
PREFIX_NAME="SasCoreWine"
SASCORE_EXE_REL="drive_c/SaumiAppServer/Core/SasCoreWindowsService.exe"
SASCORE_EXE_SIZE=8704
SASCORE_EXE_SHA="b845bae66e94252593bd2fe0d06abf0c3c63b838f46da6d37cb4a2bfc546b1b4"
SASCORE_PORTS_TCP="${SASCORE_PORTS_TCP:-8080 8732 8733 9000 80 443}"
SETTINGS_REL="drive_c/SaumiAppServer/Core/Settings.xml"
REINSTALL=0

for _arg in "$@"; do
  case "$_arg" in
    --open-ports) ;; # устарело: порты открываются всегда, без вопроса
    --reinstall|--force) REINSTALL=1 ;;
    --help|-h)
      echo "Использование:"
      echo "  sudo bash installSasCore.sh             установка / донастройка (авто)"
      echo "  sudo bash installSasCore.sh --reinstall принудительно снести ~/SasCoreWine и tar"
      exit 0
      ;;
    *)
      echo "Неизвестный аргумент: $_arg (см. --help)"; exit 1
      ;;
  esac
done

if [[ "$(id -u)" -eq 0 && -n "${SUDO_USER:-}" && "${SUDO_USER}" != "root" ]]; then
  INVOKER="${SUDO_USER}"
else
  INVOKER="$(id -un)"
fi
INVOKER_HOME="$(getent passwd "${INVOKER}" 2>/dev/null | cut -d: -f6 || echo "${HOME}")"
export HOME="${INVOKER_HOME}"

DEST="${HOME}/${PREFIX_NAME}"
STAGING_LOG="$(mktemp /tmp/sascore_install_XXXXXX.log)"
CURRENT_LOG="${STAGING_LOG}"

log() {
  local line="[$(date '+%F %T')] $*"
  echo "$line"
  printf '%s\n' "$line" >> "${CURRENT_LOG}" 2>/dev/null || true
}
fail() {
  echo ""
  echo "ОШИБКА: $*"
  [[ -f "${CURRENT_LOG}" ]] && cp -f "${CURRENT_LOG}" "${SCRIPT_DIR}/sascore_install_last.log" 2>/dev/null || true
  exit 1
}

finalize_log() {
  [[ -f "${CURRENT_LOG}" ]] && cp -f "${CURRENT_LOG}" "${INSTALL_LOG}" 2>/dev/null || true
}

path_is_file() {
  [[ -f "$1" ]]
}

path_is_exec() {
  [[ -x "$1" ]]
}

file_size() {
  stat -c%s "$1" 2>/dev/null || stat -f%z "$1" 2>/dev/null || echo 0
}

file_sha256() {
  if command -v sha256sum >/dev/null 2>&1; then
    sha256sum "$1" 2>/dev/null | awk '{print $1}'
  else
    shasum -a 256 "$1" 2>/dev/null | awk '{print $1}'
  fi
}

settings_is_windows_stock() {
  [[ -f "$1" ]] && grep -q '8QzkIvdEXU2aUVXLX1JNIg' "$1" 2>/dev/null
}

settings_is_usable() {
  [[ -f "$1" ]] || return 1
  settings_is_windows_stock "$1" && return 1
  return 0
}

sascore_exe_ok() {
  local f="$1" sz sha
  path_is_file "$f" || return 1
  sz=$(file_size "$f")
  sha=$(file_sha256 "$f")
  [[ "$sz" == "$SASCORE_EXE_SIZE" && "$sha" == "$SASCORE_EXE_SHA" ]]
}

sascore_net4_ok() {
  path_is_file "${DEST}/drive_c/windows/Microsoft.NET/Framework/v4.0.30319/mscorlib.dll" \
    && path_is_file "${DEST}/drive_c/windows/Microsoft.NET/Framework/v4.0.30319/clr.dll" \
    && { path_is_file "${DEST}/drive_c/windows/Microsoft.NET/Framework/v4.0.30319/MSVCR100_CLR0400.dll" \
         || path_is_file "${DEST}/drive_c/windows/system32/MSVCR100_CLR0400.dll"; }
}

sascore_gac_ok() {
  path_is_file "${DEST}/drive_c/windows/Microsoft.NET/assembly/GAC_MSIL/System/v4.0_4.0.0.0__b77a5c561934e089/System.dll"
}

sascore_ndp_ok() {
  grep -q 'NET Framework Setup\\\\NDP\\\\v4\\\\Full' "${DEST}/system.reg" 2>/dev/null
}

has_sascore() {
  path_is_file "${DEST}/${SASCORE_EXE_REL}" \
    && path_is_exec "${DEST}/bin/start_sascore.sh" \
    && [[ -d "${DEST}/drive_c" ]]
}

has_sascore_ok() {
  has_sascore || return 1
  sascore_exe_ok "${DEST}/${SASCORE_EXE_REL}" || return 1
  sascore_net4_ok || return 1
  sascore_gac_ok || return 1
  sascore_ndp_ok || return 1
  path_is_exec "${DEST}/runtime/wine-11.0/bin/wine" || return 1
  path_is_file "${DEST}/bin/install_sascore.sh" || return 1
  return 0
}

run_as_invoker() {
  if [[ "$(id -u)" -eq 0 && "${INVOKER}" != "root" ]]; then
    runuser -u "${INVOKER}" -- env \
      HOME="${HOME}" \
      SASCORE_PREFIX="${DEST}" \
      SASCORE_TNS="${SASCORE_TNS:-}" \
      SASCORE_USER="${SASCORE_USER:-}" \
      SASCORE_PASSWORD="${SASCORE_PASSWORD:-}" \
      SASCORE_PORTS_TCP="${SASCORE_PORTS_TCP}" \
      DISPLAY="${DISPLAY:-:0}" \
      XAUTHORITY="${HOME}/.Xauthority" \
      "$@"
  else
    env \
      HOME="${HOME}" \
      SASCORE_PREFIX="${DEST}" \
      SASCORE_TNS="${SASCORE_TNS:-}" \
      SASCORE_USER="${SASCORE_USER:-}" \
      SASCORE_PASSWORD="${SASCORE_PASSWORD:-}" \
      SASCORE_PORTS_TCP="${SASCORE_PORTS_TCP}" \
      DISPLAY="${DISPLAY:-:0}" \
      XAUTHORITY="${HOME}/.Xauthority" \
      "$@"
  fi
}

stop_sascore_prefix() {
  [[ -d "${DEST}" ]] || return 0
  log "остановка SasCoreWine..."
  if [[ -x "${DEST}/bin/stop_sascore.sh" ]]; then
    run_as_invoker bash "${DEST}/bin/stop_sascore.sh" 2>/dev/null || true
  fi
  local ws="${DEST}/runtime/wine-11.0/bin/wineserver"
  if [[ -x "$ws" ]]; then
    run_as_invoker env WINEPREFIX="${DEST}" "$ws" -k 2>/dev/null || true
  fi
  pkill -u "$(id -u "${INVOKER}")" -f '[S]asCoreWindowsService\.exe' 2>/dev/null || true
  sleep 1
}

remove_sascore_prefix() {
  [[ -d "${DEST}" ]] || return 0
  stop_sascore_prefix
  log "удаляем ${DEST}"
  rm -rf "${DEST}" "${HOME}/._SasCoreWine" 2>/dev/null || true
}

backup_settings() {
  SETTINGS_BAK=""
  local src="${DEST}/${SETTINGS_REL}"
  settings_is_usable "$src" || return 0
  SETTINGS_BAK="$(mktemp /tmp/sascore_settings_XXXXXX.xml)"
  cp -f "$src" "$SETTINGS_BAK"
  log "сохранили Settings.xml (Oracle) — вернём после tar"
}

restore_settings() {
  [[ -n "${SETTINGS_BAK:-}" && -f "${SETTINGS_BAK}" ]] || return 0
  mkdir -p "${DEST}/drive_c/SaumiAppServer/Core"
  cp -f "${SETTINGS_BAK}" "${DEST}/${SETTINGS_REL}"
  chmod 600 "${DEST}/${SETTINGS_REL}" 2>/dev/null || true
  log "вернули Settings.xml"
  rm -f "${SETTINGS_BAK}"
  SETTINGS_BAK=""
}

open_ports_root() {
  log "открытие портов firewall: ${SASCORE_PORTS_TCP}"
  local p
  if command -v iptables >/dev/null; then
    for p in $SASCORE_PORTS_TCP; do
      iptables -C INPUT -p tcp --dport "$p" -j ACCEPT 2>/dev/null \
        || iptables -I INPUT -p tcp --dport "$p" -j ACCEPT
      log "  iptables TCP $p ACCEPT"
    done
  fi
  if command -v firewall-cmd >/dev/null && firewall-cmd --state 2>/dev/null | grep -qi running; then
    for p in $SASCORE_PORTS_TCP; do
      firewall-cmd --permanent --add-port="${p}/tcp" 2>>"${CURRENT_LOG}" || true
    done
    firewall-cmd --reload 2>>"${CURRENT_LOG}" || true
    log "  firewalld rules added"
  fi
  if command -v ufw >/dev/null && ufw status 2>/dev/null | grep -qi active; then
    for p in $SASCORE_PORTS_TCP; do
      ufw allow "${p}/tcp" 2>>"${CURRENT_LOG}" || true
    done
    log "  ufw rules added"
  fi
  log "порты: готово"
}

chmod_logs() {
  chmod -R a+rX "${DEST}/logs" 2>/dev/null || true
  chmod a+r "${SCRIPT_DIR}/sascore_install.log" "${SCRIPT_DIR}/sascore_install_last.log" 2>/dev/null || true
  log "права на логи: a+rX ${DEST}/logs"
}

verify_archive_sascore() {
  local arch="$1" sha
  [[ -f "$arch" ]] || fail "Нет архива ${arch}"
  sha=$(tar -xOf "$arch" "SasCoreWine/${SASCORE_EXE_REL}" 2>/dev/null | sha256sum 2>/dev/null | awk '{print $1}' \
    || tar -xOf "$arch" "SasCoreWine/${SASCORE_EXE_REL}" 2>/dev/null | shasum -a 256 2>/dev/null | awk '{print $1}' \
    || true)
  if [[ "$sha" == "$SASCORE_EXE_SHA" ]]; then
    log "OK: SasCoreWindowsService.exe в tar sha=${sha:0:12}…"
  else
    fail "SasCoreWindowsService.exe в tar битый (sha=${sha:0:12}…) — залейте новый SasCoreWine.tar.gz"
  fi
}

unpack_archive() {
  log "распаковка → ${DEST}"
  rm -rf "${DEST}" "${HOME}/._SasCoreWine" 2>/dev/null || true
  mkdir -p "${HOME}"
  TAR_FLAGS=(-xzf "${ARCHIVE}" -C "${HOME}"
    --delay-directory-restore
    --transform="s|^SasCoreWine/|${PREFIX_NAME}/|"
    --transform="s|^SasCoreWine$|${PREFIX_NAME}|")
  if tar --version 2>/dev/null | head -1 | grep -qi 'GNU tar'; then
    TAR_FLAGS+=(--no-same-owner --warning=no-unknown-keyword)
  fi
  tar "${TAR_FLAGS[@]}" || fail "Не удалось распаковать ${ARCHIVE}"
  if [[ ! -d "${DEST}/drive_c" && -d "${HOME}/SasCoreWine/drive_c" ]]; then
    mv "${HOME}/SasCoreWine" "${DEST}" 2>/dev/null || true
  fi
  [[ -d "${DEST}/drive_c" ]] || fail "После распаковки нет ${DEST}/drive_c"
  find "${DEST}" \( -name '._*' -o -name '.DS_Store' \) -delete 2>/dev/null || true
  mkdir -p "${DEST}/logs" "${DEST}/bin"
  chmod +x "${DEST}/bin"/*.sh 2>/dev/null || true
  sascore_exe_ok "${DEST}/${SASCORE_EXE_REL}" \
    || fail "SasCoreWindowsService.exe повреждён после tar — пересоберите SasCoreWine.tar.gz"
  log "распаковано OK (exe size=${SASCORE_EXE_SIZE} sha=${SASCORE_EXE_SHA:0:12}…)"
}

refresh_bin_from_archive() {
  local s tmp
  mkdir -p "${DEST}/bin"
  for s in install_sascore.sh start_sascore.sh stop_sascore.sh probe_sascore.sh oracle_settings.sh oracle_settings.cs; do
    tmp="$(mktemp)"
    if tar -xOf "${ARCHIVE}" "SasCoreWine/bin/${s}" >"$tmp" 2>/dev/null && [[ -s "$tmp" ]]; then
      mv -f "$tmp" "${DEST}/bin/${s}"
      chmod 755 "${DEST}/bin/${s}"
    else
      rm -f "$tmp"
    fi
  done
  log "скрипты из tar → ${DEST}/bin"
}

echo ""
echo "============================================"
echo "  SasCoreService — установка «один клик»"
echo "  Отдельный prefix: ~/${PREFIX_NAME}"
echo "============================================"
echo ""

[[ -f /etc/os-release ]] || fail "Не Linux? Нет /etc/os-release"
# shellcheck source=/dev/null
source /etc/os-release
log "ОС: ${PRETTY_NAME:-unknown}"
log "installer=${INSTALLER_REV} user=${INVOKER} host=$(hostname)"
log "SCRIPT_DIR=${SCRIPT_DIR} archive=${ARCHIVE}"

[[ -f "${ARCHIVE}" ]] || fail "Нет архива ${ARCHIVE} — положите рядом с installSasCore.sh"
verify_archive_sascore "${ARCHIVE}"

need_free_mb=2500
free_mb="$(df -m "${HOME}" 2>/dev/null | awk 'NR==2{print $4}' || echo 0)"
log "свободно на ${HOME}: ${free_mb}M (нужно ~${need_free_mb}M)"
if [[ "${free_mb}" =~ ^[0-9]+$ ]] && [[ "${free_mb}" -lt "${need_free_mb}" ]]; then
  fail "Мало места на ${HOME}: ${free_mb}M"
fi

SETTINGS_BAK=""
NEED_UNPACK=0
if [[ "${REINSTALL}" -eq 1 ]]; then
  log "режим --reinstall: принудительная переустановка из tar"
  backup_settings
  remove_sascore_prefix
  NEED_UNPACK=1
elif has_sascore_ok; then
  log "prefix OK: ${DEST} — tar не трогаем"
  NEED_UNPACK=0
elif has_sascore || [[ -d "${DEST}" ]]; then
  log "prefix неполный (нет GAC v4 / NDP / CRT) → удаляем и ставим из tar"
  backup_settings
  remove_sascore_prefix
  NEED_UNPACK=1
else
  log "prefix нет → распаковка из tar"
  NEED_UNPACK=1
fi

if [[ "${NEED_UNPACK}" -eq 1 ]]; then
  unpack_archive
  restore_settings
fi

if command -v sha256sum >/dev/null; then
  log "archive sha256=$(sha256sum "${ARCHIVE}" | awk '{print $1}')"
elif command -v shasum >/dev/null; then
  log "archive sha256=$(shasum -a 256 "${ARCHIVE}" | awk '{print $1}')"
fi

if [[ -d "${DEST}" ]]; then
  stop_sascore_prefix
fi

refresh_bin_from_archive

INSTALL_SCRIPT="${DEST}/bin/install_sascore.sh"
[[ -f "$INSTALL_SCRIPT" ]] || fail "нет ${INSTALL_SCRIPT} — архив без скрипта настройки, пересоберите SasCoreWine.tar.gz"

if [[ "$(id -u)" -eq 0 ]]; then
  open_ports_root
  chown -R "${INVOKER}:${INVOKER}" "${DEST}" 2>/dev/null || true
else
  log "WARN: не root — firewall пропускаем (запустите через sudo)"
fi

if settings_is_usable "${DEST}/${SETTINGS_REL}"; then
  echo ""
  echo "Oracle Settings.xml уже есть — не спрашиваем."
  echo ""
else
  echo ""
  echo "Далее: обязательные параметры Oracle"
  echo "  (имя сервиса, имя пользователя, пароль)."
  echo "Без заполненных полей установка не продолжится."
  echo ""
fi

finalize_log
log "запуск install_sascore.sh (от ${INVOKER}, не root)..."

run_as_invoker bash "$INSTALL_SCRIPT" > >(tee -a "${INSTALL_LOG}") 2>&1 || {
  chmod_logs
  fail "install_sascore.sh завершился с ошибкой — см. ${INSTALL_LOG} и ${DEST}/logs/"
}

chmod_logs

echo ""
echo "============================================"
echo "  ГОТОВО — SasCoreService"
echo "============================================"
echo ""
echo "  Prefix: ${DEST}"
echo "  Старт:  ${DEST}/bin/start_sascore.sh"
echo "  Стоп:   ${DEST}/bin/stop_sascore.sh"
echo "  Probe:  ${DEST}/bin/probe_sascore.sh"
echo "  Логи:   ${DEST}/logs/sascore_install.log , sascore.log , sascore_probe.log"
echo "  Ярлыки: «SasCoreService — Старт/Стоп/Проверка»"
echo ""
echo "  Лог установщика: ${INSTALL_LOG}"
echo ""
