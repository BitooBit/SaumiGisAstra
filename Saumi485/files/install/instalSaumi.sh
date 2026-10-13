#!/usr/bin/env bash
# SAUMI 4.8.5 — установщик (prod-4: Guardant + ГИС ГМП / SasCore)
# Комплект поставки: instalSaumi.sh + SAUMI1860wine.tar.gz (всё внутри архива)
#   sudo bash instalSaumi.sh                 — клиент всем (режим окон 2)
#   sudo bash instalSaumi.sh --users         — только где ещё нет ~/Saumi
#   sudo bash instalSaumi.sh --licence-server — Guardant на сервере лицензий
#   sudo bash instalSaumi.sh --refresh-start — только startSAUMI/helpers (mmp32 не трогаем)
# Offline host libs: только из SAUMI1860wine.tar.gz → SAUMIwine/runtime/saumi-host-debs/
#   sudo bash instalSaumi.sh --wm-mode=2|1   — зафиксировать режим (без переустановки)
#   SAUMI_EXCLUDE="u1 u2"                    — пропуск пользователей
#   SAUMI_FOR_USER='user@DOMAIN'             — только этот пользователь (--for-user=)
#
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ARCHIVE="${SAUMI_ARCHIVE:-${SCRIPT_DIR}/SAUMI1860wine.tar.gz}"
INSTALL_LOG_FILE="${SCRIPT_DIR}/saumi_install.log"

SAUMI_ADMIN_USER="${SAUMI_ADMIN_USER:-vrsaum}"
if [[ -n "${SAUMI_EXCLUDE:-}" ]]; then
  read -ra SAUMI_EXCLUDE_USERS <<< "${SAUMI_EXCLUDE}"
else
  SAUMI_EXCLUDE_USERS=(fiksik)
fi

INSTALLER_REV="2026-10-13-runuser-domain"
MMP32_SIZE=35627696
MMP32_SHA="0a39282c3b3cc2b73b3b422d07748ea943ad935b4c2fe5f3b9f8bf27be5a9a4f"
# mmp32: содержимое/size/mtime из SAUMI485.zip — 2021-06-28 13:25:40.
# После tar выставляем Изменён(mtime) и Создан(birth/crtime) = mtime из архива (в tar birth нет).
# Вердикт komp-05 2026-09-04: эталон sha/size; даты восстанавливаем после распаковки.
MODE="full"
WITH_LICENCE=0
WITH_GMP=0
FOR_USER="${SAUMI_FOR_USER:-}"
FORCE_FLAG=0
# Режим окон при установке (по умолчанию 2 — рабочий на komp-05). --wm-mode=1|2 переключают.
SAUMI_WM_MODE="${SAUMI_WM_MODE:-2}"
for _arg in "$@"; do
  case "$_arg" in
    --users|--clients|-u) MODE="users" ;;
    --user) MODE="users" ;; # совместимость; для одного юзера — --for-user=
    --force-users) MODE="force-users"; FORCE_FLAG=1 ;;
    --for-user=*)
      MODE="for-user"
      FOR_USER="${_arg#--for-user=}"
      ;;
    --fix-owner) MODE="fix-owner" ;;
    --diag|--collect-diag) MODE="diag" ;;
    --licence-server|--license-server) MODE="licence-server" ;;
    --with-licence-server|--with-license-server) WITH_LICENCE=1 ;;
    --gmp-server|--sas-core|--gis-gmp) MODE="gmp-server" ;;
    --with-gmp-server|--with-sas-core|--with-gis-gmp) WITH_GMP=1 ;;
    --wm-mode=1|--wm-mode=2)
      SAUMI_WM_MODE="${_arg##*=}"
      MODE="wm-mode"
      ;;
    --refresh-start|--update-start|--patch-start)
      MODE="refresh-start"
      ;;
    --open-ports) ;; # устарело: порты открываются автоматически
    --help|-h)
      echo "Использование:"
      echo "  sudo bash instalSaumi.sh                 клиент всем (установка / переустановка)"
      echo "  sudo bash instalSaumi.sh --users         только кому ещё нет ~/Saumi"
      echo "  sudo bash instalSaumi.sh --for-user=NAME  только один (домен: user@DOMAIN)"
      echo "  sudo bash instalSaumi.sh --for-user=NAME --force-users  переустановить одному"
      echo "  sudo bash instalSaumi.sh --fix-owner     chown ~/Saumi всем найденным"
      echo "  sudo bash instalSaumi.sh --diag          только сбор диагностики (без установки)"
      echo "  sudo bash instalSaumi.sh --licence-server  Guardant (USB-ключ, порты 3182–3185)"
      echo "  sudo bash instalSaumi.sh --refresh-start  только скрипты запуска, mmp32 не трогаем"
      echo "  sudo bash instalSaumi.sh --wm-mode=2     зафиксировать режим 2 (без переустановки)"
      echo "  sudo bash instalSaumi.sh --wm-mode=1     зафиксировать режим 1 (без переустановки)"
      exit 0
      ;;
    *)
      echo "Неизвестный аргумент: $_arg (см. --help)"; exit 1
      ;;
  esac
done
# --for-user= имеет приоритет над --users/--force-users (FORCE_FLAG сохраняется)
if [[ -n "${FOR_USER}" ]]; then
  MODE="for-user"
fi
SAUMI_WM_MODE="$(echo "${SAUMI_WM_MODE}" | tr -cd '12' | head -c 1)"
SAUMI_WM_MODE="${SAUMI_WM_MODE:-2}"

STAGING_LOG="$(mktemp /tmp/saumi_install_XXXXXX.log)"
CURRENT_LOG="${STAGING_LOG}"

if [[ "$(id -u)" -eq 0 && -n "${SUDO_USER:-}" && "${SUDO_USER}" != "root" ]]; then
  INVOKER="${SUDO_USER}"
else
  INVOKER="$(id -un)"
fi
INVOKER_HOME="$(getent passwd "${INVOKER}" | cut -d: -f6 || true)"
export HOME="${INVOKER_HOME:-${HOME:-/root}}"
INSTALL_USER="${INVOKER}"
INSTALL_GROUP="$(id -gn "${INSTALL_USER}" 2>/dev/null || echo "${INSTALL_USER}")"

SAUMI_WINE_EXPECT_PREFIX="wine-11.0"
SAUMI_WINE_RUNTIME_REL="runtime/wine-11.0"

SUDO_OK=0
have_passwordless_sudo() {
  [[ "$(id -u)" -eq 0 ]] && return 0
  command -v sudo >/dev/null 2>&1 || return 1
  sudo -n true 2>/dev/null
}

ask_sudo_if_needed() {
  local why="${1:-установка SAUMI}"
  if [[ "$(id -u)" -eq 0 ]]; then
    SUDO_OK=1
    return 0
  fi
  command -v sudo >/dev/null 2>&1 || fail "Нужен sudo (${why})"
  if have_passwordless_sudo; then
    SUDO_OK=1
    log "sudo без пароля — OK (${why})"
    return 0
  fi
  echo ""
  echo "============================================"
  echo " Нужны права администратора"
  echo " Причина: ${why}"
  echo " Введите пароль sudo ниже:"
  echo "============================================"
  sudo -v || fail "Не удалось получить права admin (${why})"
  SUDO_OK=1
}

run_as_root() {
  if [[ "$(id -u)" -eq 0 ]]; then
    "$@"
    return $?
  fi
  [[ "${SUDO_OK}" -eq 1 ]] || ask_sudo_if_needed "$*"
  sudo "$@"
}

stop_saumi_for_user() {
  local u="$1" prefix="${2:-}"
  if [[ -n "$prefix" && -x "${prefix}/runtime/wine-11.0/bin/wineserver" ]]; then
    sudo -u "$u" "${prefix}/runtime/wine-11.0/bin/wineserver" -k 2>/dev/null || true
  fi
  sudo pkill -9 -u "$u" -x wineserver 2>/dev/null || true
  sudo pkill -9 -u "$u" -f '[m]mp32\.exe' 2>/dev/null || true
  sudo pkill -9 -u "$u" -f '[E]xplorerWindow' 2>/dev/null || true
  sleep 1
}

log() {
  local line="[$(date '+%F %T')] $*"
  echo "$line"
  if [[ -n "${CURRENT_LOG:-}" ]]; then
    printf '%s\n' "$line" >> "${CURRENT_LOG}" 2>/dev/null || true
  fi
}
fail() {
  local backup="${SCRIPT_DIR}/saumi_install_last.log"
  echo ""
  echo "ОШИБКА: $*"
  if [[ -n "${CURRENT_LOG:-}" && -f "${CURRENT_LOG}" ]]; then
    sync "${CURRENT_LOG}" 2>/dev/null || true
    cp -f "${CURRENT_LOG}" "${backup}" 2>/dev/null || true
    [[ -s "${backup}" ]] && echo "Копия лога: ${backup}"
  fi
  # sudo → логи root:600; отдать вызывающему (vrsaum), чтобы можно было скачать/открыть
  if [[ -n "${INVOKER:-}" && "${INVOKER}" != "root" ]]; then
    local ug
    ug="$(id -gn "${INVOKER}" 2>/dev/null || echo "${INVOKER}")"
    chown "${INVOKER}:${ug}" "${CURRENT_LOG:-}" "${backup}" "${INSTALL_LOG_FILE:-}" 2>/dev/null || true
    chmod 644 "${CURRENT_LOG:-}" "${backup}" "${INSTALL_LOG_FILE:-}" 2>/dev/null || true
  fi
  echo "Лог: ${CURRENT_LOG:-/tmp}"
  exit 1
}

echo ""
echo "============================================"
echo "  Установка SAUMI 4.8.5 — один клик"
echo "============================================"
echo ""

[[ -f /etc/os-release ]] || fail "Не Astra/Linux? Нет /etc/os-release"
# shellcheck source=/dev/null
source /etc/os-release
log "ОС: ${PRETTY_NAME:-unknown}"
log "installer=${INSTALLER_REV}"
log "host=$(hostname) SCRIPT_DIR=${SCRIPT_DIR}  archive=${ARCHIVE}"

require_admin_users() {
  if [[ "$(id -u)" -eq 0 ]]; then
    SUDO_OK=1
    log "режим --users: root — OK"
    return 0
  fi
  command -v sudo >/dev/null 2>&1 || fail "Нужен sudo."
  echo ""
  echo "============================================"
  echo " Доустановка SAUMI (--users)"
  echo " Архив → ~/Saumi только тем, у кого ещё нет"
  echo "============================================"
  sudo -v || fail "Нет прав sudo у $(id -un)."
  SUDO_OK=1
  log "режим --users: sudo OK"
}

require_admin() {
  if [[ "${INVOKER}" != "${SAUMI_ADMIN_USER}" && "$(id -u)" -ne 0 ]]; then
    fail "Запустите от ${SAUMI_ADMIN_USER} (сейчас ${INVOKER})"
  fi
  if [[ "$(id -u)" -eq 0 ]]; then
    SUDO_OK=1
    return 0
  fi
  command -v sudo >/dev/null 2>&1 || fail "Нужен sudo."
  echo ""
  echo "============================================"
  echo " Установка SAUMI 4.8.5"
  echo " Архив → ~/Saumi"
  echo "============================================"
  sudo -v || fail "Нет прав sudo у $(id -un)."
  SUDO_OK=1
  log "sudo OK"
}

is_user_excluded() {
  local u="$1" x
  for x in "${SAUMI_EXCLUDE_USERS[@]}"; do
    [[ "$u" == "$x" ]] && return 0
  done
  return 1
}

# Локальные + доменные: getent часто НЕ перечисляет AD/FreeIPA (enumerate=false).
# Дополнительно сканируем /home/* и резолвим через getent passwd <имя|владелец>.
list_human_users() {
  local user home d owner ent base
  declare -A _saumi_seen_homes=()

  _saumi_emit_user() {
    local u="$1" h="$2"
    [[ -n "$u" && -n "$h" && -d "$h" ]] || return 0
    is_user_excluded "$u" && return 0
    [[ -n "${_saumi_seen_homes[$h]:-}" ]] && return 0
    _saumi_seen_homes[$h]=1
    printf '%s\t%s\n' "$u" "$h"
  }

  # 1) полный getent (локальные + enumerate=true). UID!=65534 (nobody).
  while IFS=$'\t' read -r user home; do
    _saumi_emit_user "$user" "$home"
  done < <(getent passwd | awk -F: '
    $3 >= 1000 && $3 != 65534 && $6 ~ /^\/home\// {
      if ($7 ~ /(nologin|false)$/) next
      print $1 "\t" $6
    }')

  # 2) каталоги /home/* — доменные user@REALM после первого логина
  shopt -s nullglob
  for d in /home/*; do
    [[ -d "$d" ]] || continue
    base="$(basename -- "$d")"
    [[ "$base" == "lost+found" ]] && continue

    ent="$(getent passwd | awk -F: -v h="$d" '$6 == h { print $1 "\t" $6; exit }')"
    if [[ -n "$ent" ]]; then
      user="${ent%%	*}"
      home="${ent#*	}"
      _saumi_emit_user "$user" "$home"
      continue
    fi

    if getent passwd "$base" >/dev/null 2>&1; then
      home="$(getent passwd "$base" | cut -d: -f6)"
      _saumi_emit_user "$base" "${home:-$d}"
      continue
    fi

    owner="$(stat -c '%U' -- "$d" 2>/dev/null || true)"
    if [[ -n "$owner" && "$owner" != "root" ]] && getent passwd "$owner" >/dev/null 2>&1; then
      home="$(getent passwd "$owner" | cut -d: -f6)"
      _saumi_emit_user "$owner" "${home:-$d}"
    fi
  done
  shopt -u nullglob
}

has_saumi() {
  local home="$1"
  path_is_file "${home}/Saumi/drive_c/SAUMI/mmp32.exe" \
    && path_is_exec "${home}/Saumi/startSAUMI.sh"
}

has_saumi_ok() {
  local home="$1"
  has_saumi "$home" && mmp32_sha_ok "${home}/Saumi/drive_c/SAUMI/mmp32.exe"
}

write_desktop_file() {
  local prefix="$1" out="$2" icon="${1}/saumi.ico"
  [[ -f "$icon" ]] || icon=""
  cat >"$out" <<EOF
[Desktop Entry]
Version=1.0
Type=Application
Name=SAUMI
Comment=SAUMI 4.8.5
Exec=env FORCE_KILL=1 ${prefix}/startSAUMI.sh
Path=${prefix}/drive_c/SAUMI
Icon=${icon}
Terminal=false
Categories=Office;
EOF
  chmod 755 "$out"
}

install_saumi_kill_icon() {
  local prefix="$1"
  local kill="${prefix}/saumi_kill.ico"
  local base="${prefix}/saumi.ico"
  [[ -f "$kill" ]] && return 0
  if command -v wrestool >/dev/null 2>&1; then
    local exe ico tmp
    for exe in "${prefix}/drive_c/SAUMI/saumi.exe" "${prefix}/drive_c/SAUMI/ExplorerWindow.exe"; do
      [[ -f "$exe" ]] || continue
      tmp="$(mktemp -d /tmp/saumi_ico_XXXXXX)"
      wrestool -x --type=14 --name=MAINICON -o "$tmp" "$exe" 2>/dev/null || true
      ico="$(ls "$tmp"/*MAINICON*.ico 2>/dev/null | head -1 || true)"
      if [[ -n "$ico" && -f "$ico" ]]; then
        run_as_root cp -f "$ico" "$kill" 2>/dev/null || cp -f "$ico" "$kill"
        rm -rf "$tmp"
        return 0
      fi
      rm -rf "$tmp"
    done
  fi
  [[ -f "$base" ]] && run_as_root cp -f "$base" "$kill" 2>/dev/null || cp -f "$base" "$kill" 2>/dev/null || true
}

write_force_kill_script() {
  local prefix="$1"
  local f="${prefix}/bin/force_kill_saumi.sh"
  run_as_root mkdir -p "${prefix}/bin" 2>/dev/null || mkdir -p "${prefix}/bin"
  local tmp
  tmp="$(mktemp /tmp/force_kill_XXXXXX.sh)"
  cat >"$tmp" <<'KILLEOF'
#!/usr/bin/env bash
# Принудительное закрытие SAUMI (если зависло)
set -uo pipefail
W="${HOME}/Saumi"
export WINEPREFIX="${WINEPREFIX:-${W}}"
export WINEARCH=win32
WS="${W}/runtime/wine-11.0/bin/wineserver"
[[ -x "$WS" ]] || WS="$(command -v wineserver 2>/dev/null || true)"
pkill -u "$(id -u)" -f 'saumi_hide_windows\.sh' 2>/dev/null || true
pkill -u "$(id -u)" -f 'saumi_minimize_guard\.sh' 2>/dev/null || true
pkill -u "$(id -u)" -f 'saumi_taskbar_fix\.sh' 2>/dev/null || true
pkill -9 -u "$(id -u)" -f '[m]mp32\.exe' 2>/dev/null || true
pkill -9 -u "$(id -u)" -f '[E]xplorerWindow' 2>/dev/null || true
pkill -9 -u "$(id -u)" -f '[s]aumi\.exe' 2>/dev/null || true
if [[ -n "${WS:-}" && -x "$WS" ]]; then "$WS" -k 2>/dev/null || true; fi
pkill -9 -u "$(id -u)" -x wineserver 2>/dev/null || true
sleep 1
if command -v zenity >/dev/null 2>&1; then
  zenity --info --title="SAUMI" --text="SAUMI принудительно закрыт." --timeout=4 2>/dev/null || true
fi
exit 0
KILLEOF
  run_as_root cp -f "$tmp" "$f" 2>/dev/null || cp -f "$tmp" "$f"
  run_as_root chmod 755 "$f" 2>/dev/null || chmod 755 "$f"
  rm -f "$tmp"
}

write_force_kill_desktop_file() {
  local prefix="$1" out="$2"
  local icon="${prefix}/saumi_kill.ico"
  [[ -f "$icon" ]] || icon="${prefix}/saumi.ico"
  [[ -f "$icon" ]] || icon=""
  cat >"$out" <<EOF
[Desktop Entry]
Version=1.0
Type=Application
Name=Принудительное закрытие Сауми
Comment=Закрыть зависший SAUMI
Exec=${prefix}/bin/force_kill_saumi.sh
Path=${prefix}
Icon=${icon}
Terminal=false
Categories=Utility;
EOF
  chmod 755 "$out"
}

# Ярлык на папку Excel-экспорта (SaumiExcelProxy → drive_c/SAUMI/export).
write_export_folder_desktop_file() {
  local prefix="$1" out="$2"
  local export_dir="${prefix}/drive_c/SAUMI/export"
  local url
  url="$(python3 -c 'import pathlib,sys; print(pathlib.Path(sys.argv[1]).resolve().as_uri())' "$export_dir" 2>/dev/null \
    || echo "file://${export_dir}")"
  cat >"$out" <<EOF
[Desktop Entry]
Version=1.0
Type=Link
Name=Экспорт Saumi
Comment=Папка Excel-экспорта SAUMI
URL=${url}
Icon=folder
EOF
  chmod 755 "$out"
}

write_start_saumi() {
  local prefix="$1"
  local user="${2:-}"
  local f="${prefix}/startSAUMI.sh"
  local tmp ug
  tmp="$(mktemp /tmp/startSAUMI_XXXXXX.sh)"
  cat >"$tmp" <<'STARTSAUMIEOF'
#!/usr/bin/env bash
# Запуск SAUMI 4.8.5 — префикс = каталог этого скрипта (~/Saumi)
set -uo pipefail

_SAUMI_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
export WINEPREFIX="${WINEPREFIX:-${_SAUMI_ROOT}}"
export WINEARCH=win32
export WINEDLLOVERRIDES="mscoree,mscorwks,fusion=native;msvcr80,msvcp80,msvcm80=native;msxml3,msxml4,msxml6=builtin"
export ORACLE_HOME="C:\\oracle"
export TNS_ADMIN="C:\\oracle\\NETWORK\\ADMIN"
export TMP="C:\\SAUMI\\temp"
export TEMP="C:\\SAUMI\\temp"
export NLS_LANG="AMERICAN_AMERICA.CL8MSWIN1251"
export LANG="${LANG:-ru_RU.UTF-8}"
export LC_ALL="${LC_ALL:-ru_RU.UTF-8}"
export LC_CTYPE="${LC_CTYPE:-ru_RU.UTF-8}"
export PATH="${WINEPREFIX}/drive_c/oracle/bin:${WINEPREFIX}/drive_c/SAUMI:${PATH}"

_SAUMI_WINE11="${WINEPREFIX}/runtime/wine-11.0/bin/wine"
if [[ -x "${_SAUMI_WINE11}" ]]; then
  export WINE="${_SAUMI_WINE11}"
  export WINESERVER="${WINEPREFIX}/runtime/wine-11.0/bin/wineserver"
else
  export WINE="${WINE:-wine}"
fi

export DISPLAY="${DISPLAY:-:0}"

# Astra/Fly: cookie в /var/run/xauth; ~/.Xauthority часто не тот → Wine lock_display / EXIT 40
_saumi_pick_xauth() {
  local _xa _try
  if [[ -n "${XAUTHORITY:-}" && -f "${XAUTHORITY}" ]]; then
    DISPLAY="${DISPLAY}" XAUTHORITY="${XAUTHORITY}" xrandr --query >/dev/null 2>&1 && return 0
  fi
  for _try in /var/run/xauth/A:0-* "${HOME}/.Xauthority"; do
    for _xa in ${_try}; do
      [[ -f "${_xa}" ]] || continue
      if DISPLAY="${DISPLAY}" XAUTHORITY="${_xa}" xrandr --query >/dev/null 2>&1; then
        export XAUTHORITY="${_xa}"
        return 0
      fi
    done
  done
  [[ -f "${HOME}/.Xauthority" ]] && export XAUTHORITY="${HOME}/.Xauthority"
  _xa="$(ls /var/run/xauth/A:0-* 2>/dev/null | head -1 || true)"
  [[ -n "${_xa}" ]] && export XAUTHORITY="${_xa}"
  return 1
}
_saumi_pick_xauth || true

VERBOSE=0
for arg in "$@"; do
  [[ "$arg" == "--verbose" || "$arg" == "-v" ]] && VERBOSE=1
done

LOG_DIR="${WINEPREFIX}/logs"
mkdir -p "${LOG_DIR}" 2>/dev/null || LOG_DIR="/tmp/saumi_${USER}_logs"
mkdir -p "${LOG_DIR}"
STAMP="$(date +%Y%m%d_%H%M%S)"
if [[ "${VERBOSE}" -eq 1 ]]; then
  RUN_LOG="${LOG_DIR}/saumi_${STAMP}_verbose.log"
  LATEST_LINK="${LOG_DIR}/saumi_latest_verbose.log"
  export WINEDEBUG="+timestamp,+tid,+seh,+loaddll,err+all,warn+all"
else
  RUN_LOG="${LOG_DIR}/saumi_${STAMP}.log"
  LATEST_LINK="${LOG_DIR}/saumi_latest.log"
  export WINEDEBUG="+timestamp,+tid,+seh,err+all,warn+seh,warn+module,warn+ole"
fi
ROLLING_LOG="${LOG_DIR}/saumi_run.log"
KEEP_LOGS="${KEEP_LOGS:-5}"
: > "${RUN_LOG}"
: > "${ROLLING_LOG}"
ln -sfn "$(basename "${RUN_LOG}")" "${LATEST_LINK}"
(
  cd "${LOG_DIR}" || exit 0
  ls -1t saumi_20*.log 2>/dev/null | grep -v '_verbose\.log$' | tail -n "+$((KEEP_LOGS + 1))" | xargs -r rm -f --
  ls -1t saumi_20*_verbose.log 2>/dev/null | tail -n +3 | xargs -r rm -f --
) || true

finalize_run() {
  local code=$?
  echo "========== EXIT CODE: ${code} $(date '+%F %T') ==========" | tee -a "${RUN_LOG}" "${ROLLING_LOG}" >/dev/null
  ln -sfn "$(basename "${RUN_LOG}")" "${LATEST_LINK}" 2>/dev/null || true
  echo "Лог запуска: ${LATEST_LINK}"
  exit "${code}"
}
trap finalize_run EXIT

log_line() { echo "$*" | tee -a "${RUN_LOG}" "${ROLLING_LOG}"; }

log_line "========== SAUMI RUN ${STAMP} $(date '+%F %T') =========="
log_line "user=$(whoami) host=$(hostname) DISPLAY=${DISPLAY} XAUTHORITY=${XAUTHORITY:-unset}"
log_line "wine=$(${WINE} --version 2>&1 || echo unknown)"
log_line "WINEPREFIX=${WINEPREFIX}"
log_line "RUN_LOG=${RUN_LOG}"
if command -v xrandr >/dev/null 2>&1; then
  xrandr --query 2>&1 | head -3 | while read -r _l; do log_line "xrandr: ${_l}"; done || \
    log_line "WARN: xrandr не видит монитор — EXIT 40 вероятен"
fi

if pgrep -u "$(id -u)" -f "mmp32\.exe" >/dev/null 2>&1; then
  if [[ "${FORCE_KILL:-0}" == "1" ]]; then
    log_line "WARN: killing previous mmp32.exe"
    pkill -9 -u "$(id -u)" -f '[m]mp32' || true
    [[ -x "${WINESERVER:-}" ]] && "${WINESERVER}" -k 2>/dev/null || true
    sleep 1
  else
    log_line "WARN: already running; FORCE_KILL=1 to restart"
    exit 0
  fi
fi

{
  ls -la "${WINEPREFIX}/drive_c/SAUMI/mmp32.exe" 2>&1 || true
  head -n 10 "${WINEPREFIX}/drive_c/oracle/NETWORK/ADMIN/tnsnames.ora" 2>&1 || true
} >>"${RUN_LOG}" 2>&1

echo "Лог: ${LATEST_LINK}"

# Режим окон: 2=Managed=Y (по умолчанию, рабочий); 1=Managed=N (запасной, 1710)
WM_MODE_FILE="${WINEPREFIX}/config/wm_mode"
WM_MODE="${SAUMI_WM_MODE:-}"
if [[ -z "${WM_MODE}" && -f "${WM_MODE_FILE}" ]]; then
  WM_MODE="$(tr -cd '12' <"${WM_MODE_FILE}" | head -c 1)"
fi
WM_MODE="${WM_MODE:-2}"
log_line "WM_MODE=${WM_MODE} (2=Managed=Y default, 1=Managed=N)"
_saumi_x11_reg() {
  "${WINE}" reg add 'HKCU\Software\Wine\X11 Driver' /v "$1" /t REG_SZ /d "$2" /f >/dev/null 2>&1 || true
}
pkill -u "$(id -u)" -f 'saumi_minimize_guard\.sh' 2>/dev/null || true
pkill -u "$(id -u)" -f 'saumi_taskbar_fix\.sh' 2>/dev/null || true
pkill -u "$(id -u)" -f 'saumi_wm_boot_switch\.sh' 2>/dev/null || true
pkill -u "$(id -u)" -f 'saumi_hide_windows\.sh' 2>/dev/null || true
if [[ "${WM_MODE}" == "2" ]]; then
  # Железо: Managed=Y ДО wine. Mid-session reg+switch не работает (окна уже созданы).
  _saumi_x11_reg Managed Y
  _saumi_x11_reg Decorated Y
  _saumi_x11_reg UseTakeFocus Y
  if [[ -x "${WINEPREFIX}/bin/saumi_taskbar_fix.sh" ]]; then
    "${WINEPREFIX}/bin/saumi_taskbar_fix.sh" >/dev/null 2>&1 &
  fi
  log_line "WM_MODE=2: Managed=Y before wine + taskbar_fix (no hybrid)"
else
  _saumi_x11_reg Managed N
  _saumi_x11_reg Decorated Y
  _saumi_x11_reg UseTakeFocus N
  if [[ -x "${WINEPREFIX}/bin/saumi_hide_windows.sh" ]]; then
    "${WINEPREFIX}/bin/saumi_hide_windows.sh" >/dev/null 2>&1 &
  fi
  if [[ -x "${WINEPREFIX}/bin/saumi_minimize_guard.sh" ]]; then
    "${WINEPREFIX}/bin/saumi_minimize_guard.sh" >/dev/null 2>&1 &
  fi
  log_line "WM_MODE=1: Managed=N, hide+minimize-guard on"
fi

cd "${WINEPREFIX}/drive_c/SAUMI" || exit 1
mkdir -p "${WINEPREFIX}/drive_c/SAUMI/temp/twain" \
  "${WINEPREFIX}/drive_c/SAUMI/temp/SAUMI_SCANS" \
  "${WINEPREFIX}/drive_c/users/$(id -un)/Desktop" \
  "${WINEPREFIX}/drive_c/users/$(id -un)/Documents" \
  "${WINEPREFIX}/drive_c/users/$(id -un)/AppData/Roaming" \
  "${WINEPREFIX}/drive_c/users/Public/Desktop" 2>/dev/null || true
if [[ -x "${WINEPREFIX}/bin/saumi_twain_runtime.sh" ]]; then
  SAUMI_PREFIX="${WINEPREFIX}" bash "${WINEPREFIX}/bin/saumi_twain_runtime.sh" || true
fi
log_line "cmd: ${WINE} mmp32.exe /oldupdate"
${WINE} mmp32.exe /oldupdate >>"${RUN_LOG}" 2>&1
EXIT=$?
log_line "========== EXIT CODE: ${EXIT} $(date '+%F %T') =========="
exit "${EXIT}"
STARTSAUMIEOF
  run_as_root mkdir -p "${prefix}"
  run_as_root cp -f "$tmp" "$f"
  run_as_root chmod 755 "$f"
  if [[ -n "$user" ]]; then
    ug="$(id -gn "$user" 2>/dev/null || echo "$user")"
    run_as_root chown "${user}:${ug}" "$f"
  fi
  rm -f "$tmp"
  log "  startSAUMI.sh → ${f}"
}

# Найти исходник helper рядом с установщиком (Mac tree) или уже в prefix (из tar).
_find_saumi_src() {
  local name="$1" cand
  for cand in \
    "${SCRIPT_DIR}/../files/src/${name}" \
    "${SCRIPT_DIR}/../../files/src/${name}" \
    "${SCRIPT_DIR}/files/src/${name}"
  do
    [[ -f "$cand" ]] && { echo "$cand"; return 0; }
  done
  return 1
}

# Пишет saumi_wm_boot_switch.sh в prefix (для сервера без files/src).
write_embedded_boot_switch() {
  local prefix="$1"
  local tmp
  tmp="$(mktemp /tmp/saumi_wm_boot_XXXXXX.sh)"
  cat >"$tmp" <<'BOOTSWITCHEOF'
#!/usr/bin/env bash
# Устарело: гибрид mode1→mode2 mid-session не работает (Wine читает Managed при создании окна).
# Режим 2 задаётся в startSAUMI.sh ДО запуска wine. Этот скрипт — заглушка.
set -uo pipefail
PREFIX="${WINEPREFIX:-${HOME}/Saumi}"
LOG="${PREFIX}/logs/wm_boot_switch.log"
mkdir -p "$(dirname "$LOG")"
echo "========== wm_boot_switch $(date '+%F %T') STUB pid=$$ (hybrid disabled) ==========" >>"$LOG"
echo "noop: Managed must be set before wine; see startSAUMI.sh WM_MODE=2" >>"$LOG"
exit 0
BOOTSWITCHEOF
  run_as_root cp -f "$tmp" "${prefix}/bin/saumi_wm_boot_switch.sh"
  run_as_root chmod 755 "${prefix}/bin/saumi_wm_boot_switch.sh"
  rm -f "$tmp"
}

write_embedded_taskbar_fix() {
  local prefix="$1"
  local tmp
  tmp="$(mktemp /tmp/saumi_taskbar_XXXXXX.sh)"
  cat >"$tmp" <<'TASKBAREOF'
#!/usr/bin/env bash
# Режим 2: окна SAUMI → NORMAL, убрать SKIP_TASKBAR/ABOVE (панель Fly + свернуть).
# Класс окон Wine: saumi.exe (не mmp32.exe). Managed не трогаем — задаётся до старта wine.
set -uo pipefail

PREFIX="${WINEPREFIX:-${HOME}/Saumi}"
LOG="${PREFIX}/logs/taskbar_fix.log"
POLL_SEC="${SAUMI_TASKBAR_POLL:-1}"

mkdir -p "$(dirname "$LOG")"
echo "========== taskbar_fix $(date '+%F %T') pid=$$ ==========" >>"$LOG"

command -v wmctrl >/dev/null 2>&1 || { echo "WARN: нет wmctrl" >>"$LOG"; exit 0; }
command -v xprop >/dev/null 2>&1 || { echo "WARN: нет xprop" >>"$LOG"; exit 0; }

saumi_running() {
  pgrep -u "$(id -u)" -f '[m]mp32\.exe' >/dev/null 2>&1 \
    || pgrep -u "$(id -u)" -f '[E]xplorerWindow' >/dev/null 2>&1 \
    || pgrep -u "$(id -u)" -f '[s]aumi\.exe' >/dev/null 2>&1
}

fix_one() {
  local id="$1"
  [[ -n "$id" ]] || return 0
  xprop -id "$id" -f _NET_WM_WINDOW_TYPE 32a -set _NET_WM_WINDOW_TYPE _NET_WM_WINDOW_TYPE_NORMAL >/dev/null 2>&1 || true
  xprop -id "$id" -remove _NET_WM_STATE >/dev/null 2>&1 || true
  wmctrl -i -r "$id" -b remove,skip_taskbar,above >/dev/null 2>&1 || true
}

list_ids() {
  wmctrl -lx 2>/dev/null | awk 'BEGIN{IGNORECASE=1}
    /saumi\.exe|mmp32\.exe|explorerwindow/ {print $1}'
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
  while read -r id; do
    [[ -n "$id" ]] || continue
    fix_one "$id"
  done < <(list_ids)
  sleep "$POLL_SEC"
done

echo "exit $(date '+%F %T')" >>"$LOG"
TASKBAREOF
  run_as_root cp -f "$tmp" "${prefix}/bin/saumi_taskbar_fix.sh"
  run_as_root chmod 755 "${prefix}/bin/saumi_taskbar_fix.sh"
  rm -f "$tmp"
}


# config/wm_mode + helpers (внутренние). Переключение режима — только ключи instalSaumi.sh.
write_wm_mode_helpers() {
  local prefix="$1"
  local user="${2:-}"
  local ug mode src f
  mode="${SAUMI_WM_MODE:-2}"
  mode="$(echo "$mode" | tr -cd '12' | head -c 1)"
  mode="${mode:-2}"

  run_as_root mkdir -p "${prefix}/bin" "${prefix}/config"
  echo "$mode" | run_as_root tee "${prefix}/config/wm_mode" >/dev/null
  run_as_root rm -f "${prefix}/bin/saumi_wm_mode.sh" "${prefix}/СПРАВКА.txt" 2>/dev/null || true
  for f in saumi_minimize_guard.sh saumi_hide_windows.sh saumi_taskbar_fix.sh saumi_wm_boot_switch.sh force_kill_saumi.sh; do
    src="$(_find_saumi_src "$f" 2>/dev/null || true)"
    [[ -z "$src" && -f "${SCRIPT_DIR}/../files/src/bin/${f}" ]] && src="${SCRIPT_DIR}/../files/src/bin/${f}"
    [[ -z "$src" && -f "${SCRIPT_DIR}/../../files/src/bin/${f}" ]] && src="${SCRIPT_DIR}/../../files/src/bin/${f}"
    if [[ -n "$src" && -f "$src" ]]; then
      run_as_root cp -f "$src" "${prefix}/bin/${f}"
      run_as_root chmod 755 "${prefix}/bin/${f}"
    fi
  done
  # На сервере files/src нет — вшиваем скрипты в установщик
  write_embedded_boot_switch "$prefix"
  write_embedded_taskbar_fix "$prefix"
  if [[ -n "$user" ]]; then
    ug="$(id -gn "$user" 2>/dev/null || echo "$user")"
    run_as_root chown "${user}:${ug}" "${prefix}/config/wm_mode" 2>/dev/null || true
    for f in saumi_minimize_guard.sh saumi_hide_windows.sh saumi_taskbar_fix.sh saumi_wm_boot_switch.sh force_kill_saumi.sh; do
      [[ -f "${prefix}/bin/${f}" ]] && \
        run_as_root chown "${user}:${ug}" "${prefix}/bin/${f}" 2>/dev/null || true
    done
  fi
  log "  wm_mode=${mode} → ${prefix}/config/wm_mode (+ boot_switch)"
}

# Команда от имени владельца ~/Saumi.
# Доменные: sudo -u user@DOMAIN часто PAM «Доступ запрещен» / password required
# (hous-a13208 / i.nelepova@ 2026-10-13). runuser — как Excel.reg, работает.
run_cmd_as_user() {
  local u="$1"; shift
  local home
  if [[ -z "$u" || "$u" == "root" ]]; then
    "$@"
    return
  fi
  home="$(getent passwd "$u" | cut -d: -f6 2>/dev/null || true)"
  [[ -n "$home" ]] || home="$(dirname "$(echo ~"$u" 2>/dev/null || echo "/home/$u")")"
  [[ -d "$home" ]] || home="/home/${u}"
  if command -v runuser >/dev/null 2>&1; then
    if command -v timeout >/dev/null 2>&1; then
      timeout 90 runuser -u "$u" -- env HOME="$home" USER="$u" LOGNAME="$u" "$@"
    else
      runuser -u "$u" -- env HOME="$home" USER="$u" LOGNAME="$u" "$@"
    fi
  else
    if command -v timeout >/dev/null 2>&1; then
      timeout 90 sudo -u "$u" -H env HOME="$home" USER="$u" LOGNAME="$u" "$@"
    else
      sudo -u "$u" -H env HOME="$home" USER="$u" LOGNAME="$u" "$@"
    fi
  fi
}

# Применить режим окон к уже установленному prefix (реестр + config).
apply_wm_mode_to_prefix() {
  local prefix="$1" user="$2" mode="${3:-${SAUMI_WM_MODE:-2}}"
  local wine ws ug
  mode="$(echo "$mode" | tr -cd '12' | head -c 1)"
  mode="${mode:-2}"
  [[ -d "${prefix}/drive_c/SAUMI" ]] || { log "[skip] ${user} — нет ${prefix}"; return 1; }
  wine="${prefix}/runtime/wine-11.0/bin/wine"
  ws="${prefix}/runtime/wine-11.0/bin/wineserver"
  [[ -x "$wine" ]] || { log "[skip] ${user} — нет wine"; return 1; }
  run_as_root mkdir -p "${prefix}/config"
  echo "$mode" | run_as_root tee "${prefix}/config/wm_mode" >/dev/null
  run_as_root rm -f "${prefix}/bin/saumi_wm_mode.sh" "${prefix}/СПРАВКА.txt" 2>/dev/null || true
  ug="$(id -gn "$user" 2>/dev/null || echo "$user")"
  run_as_root chown "${user}:${ug}" "${prefix}/config/wm_mode" 2>/dev/null || true
  stop_saumi_for_user "$user" "$prefix" 2>/dev/null || true
  if [[ "$mode" == "2" ]]; then
    run_cmd_as_user "$user" WINEPREFIX="$prefix" WINEARCH=win32 WINESERVER="$ws" \
      "$wine" reg add 'HKCU\Software\Wine\X11 Driver' /v Managed /t REG_SZ /d Y /f >/dev/null 2>&1 || true
    run_cmd_as_user "$user" WINEPREFIX="$prefix" WINEARCH=win32 WINESERVER="$ws" \
      "$wine" reg add 'HKCU\Software\Wine\X11 Driver' /v UseTakeFocus /t REG_SZ /d Y /f >/dev/null 2>&1 || true
  else
    run_cmd_as_user "$user" WINEPREFIX="$prefix" WINEARCH=win32 WINESERVER="$ws" \
      "$wine" reg add 'HKCU\Software\Wine\X11 Driver' /v Managed /t REG_SZ /d N /f >/dev/null 2>&1 || true
    run_cmd_as_user "$user" WINEPREFIX="$prefix" WINEARCH=win32 WINESERVER="$ws" \
      "$wine" reg add 'HKCU\Software\Wine\X11 Driver' /v UseTakeFocus /t REG_SZ /d N /f >/dev/null 2>&1 || true
  fi
  run_cmd_as_user "$user" WINEPREFIX="$prefix" WINEARCH=win32 WINESERVER="$ws" \
    "$wine" reg add 'HKCU\Software\Wine\X11 Driver' /v Decorated /t REG_SZ /d Y /f >/dev/null 2>&1 || true
  write_start_saumi "$prefix" "$user"
  write_wm_mode_helpers "$prefix" "$user"
  fix_prefix_owner "$prefix" "$user"
  log "  OK ${user} wm_mode=${mode}"
  return 0
}

apply_wm_mode_all_users() {
  local mode="${1:-${SAUMI_WM_MODE:-2}}" user home n=0
  mode="$(echo "$mode" | tr -cd '12' | head -c 1)"
  mode="${mode:-2}"
  ask_sudo_if_needed "переключить режим окон SAUMI (${mode})"
  log "=== --wm-mode=${mode} для всех ~/Saumi ==="
  while IFS=$'\t' read -r user home; do
    [[ -n "$user" && -n "$home" ]] || continue
    [[ -d "${home}/Saumi/drive_c/SAUMI" ]] || continue
    if apply_wm_mode_to_prefix "${home}/Saumi" "$user" "$mode"; then
      n=$((n + 1))
    fi
  done < <(list_human_users)
  log "Готово: wm_mode=${mode}, обновлено=${n}"
  [[ "$n" -gt 0 ]] || fail "Нет установленных ~/Saumi — сначала sudo bash instalSaumi.sh"
}

# Ставим под sudo, работаем под обычным пользователем:
# после любых mkdir/cp/wine/reg под root весь prefix должен снова быть у user.
fix_prefix_owner() {
  local prefix="$1"
  local user="${2:-}"
  local ug
  [[ -d "$prefix" ]] || return 0
  if [[ -z "$user" || "$user" == root ]]; then
    user="$(stat -c%U "$prefix" 2>/dev/null || true)"
  fi
  if [[ -z "$user" || "$user" == root ]]; then
    if [[ "$prefix" =~ ^/home/([^/]+)/ ]]; then
      user="${BASH_REMATCH[1]}"
    fi
  fi
  [[ -n "$user" && "$user" != root ]] || {
    log "  WARN: не удалось определить владельца для ${prefix}"
    return 0
  }
  ug="$(id -gn "$user" 2>/dev/null || echo "$user")"
  run_as_root chown -R "${user}:${ug}" -- "$prefix" 2>/dev/null || true
  if [[ -d "${prefix}/logs" ]]; then
    run_as_root chmod -R u+rwX "${prefix}/logs" 2>/dev/null || true
  fi
  log "  owner ${user}:${ug} → ${prefix} (включая logs)"
}

can_provision_user_home() {
  local user="$1" home="$2" tdir
  [[ -d "$home" ]] || { log "  WARN: нет дома ${home} — пропуск"; return 1; }
  tdir="${home}/.saumi_wtest_$$"
  if [[ "$(id -u)" -eq 0 ]]; then
    if mkdir "$tdir" 2>/dev/null && rmdir "$tdir" 2>/dev/null; then
      return 0
    fi
  fi
  if sudo mkdir "$tdir" 2>/dev/null && sudo rmdir "$tdir" 2>/dev/null; then
    return 0
  fi
  log "  WARN: нет прав писать в ${home} (даже через sudo) — пропуск ${user}"
  return 1
}

install_user_shortcuts() {
  local user="$1"
  local home="$2"
  local prefix="${home}/Saumi"
  local ug dests=() d desk
  local export_dir="${prefix}/drive_c/SAUMI/export"
  local export_desk_name="Экспорт Saumi.desktop"
  ug="$(id -gn "$user" 2>/dev/null || echo "$user")"
  write_force_kill_script "$prefix"
  install_saumi_kill_icon "$prefix"
  run_as_root mkdir -p "$export_dir" 2>/dev/null || mkdir -p "$export_dir" 2>/dev/null || true
  run_as_root chown "${user}:${ug}" "$export_dir" 2>/dev/null || true
  desk="$(mktemp /tmp/saumi_desk_XXXXXX.desktop)"
  write_desktop_file "$prefix" "$desk"
  run_as_root cp -f "$desk" "${prefix}/SAUMI.desktop" 2>/dev/null || cp -f "$desk" "${prefix}/SAUMI.desktop" 2>/dev/null || true
  run_as_root chown "${user}:${ug}" "${prefix}/SAUMI.desktop" 2>/dev/null || true
  rm -f "$desk"
  local killdesk
  killdesk="$(mktemp /tmp/saumi_kill_desk_XXXXXX.desktop)"
  write_force_kill_desktop_file "$prefix" "$killdesk"
  run_as_root cp -f "$killdesk" "${prefix}/SAUMI-force-kill.desktop" 2>/dev/null || cp -f "$killdesk" "${prefix}/SAUMI-force-kill.desktop" 2>/dev/null || true
  run_as_root chown "${user}:${ug}" "${prefix}/SAUMI-force-kill.desktop" 2>/dev/null || true
  rm -f "$killdesk"
  local exportdesk
  exportdesk="$(mktemp /tmp/saumi_export_desk_XXXXXX.desktop)"
  write_export_folder_desktop_file "$prefix" "$exportdesk"
  run_as_root cp -f "$exportdesk" "${prefix}/${export_desk_name}" 2>/dev/null \
    || cp -f "$exportdesk" "${prefix}/${export_desk_name}" 2>/dev/null || true
  run_as_root chown "${user}:${ug}" "${prefix}/${export_desk_name}" 2>/dev/null || true
  rm -f "$exportdesk"

  [[ -d "${home}/Desktop" ]] && dests+=("${home}/Desktop")
  [[ -d "${home}/Рабочий стол" ]] && dests+=("${home}/Рабочий стол")
  [[ -d "${home}/Desktop1" ]] && dests+=("${home}/Desktop1")
  if [[ -d "${home}/Desktops" ]]; then
    for d in "${home}/Desktops"/Desktop*; do [[ -d "$d" ]] && dests+=("$d"); done
  fi
  if [[ ${#dests[@]} -eq 0 ]]; then
    run_as_root mkdir -p "${home}/Desktops/Desktop1" 2>/dev/null || \
      run_as_root mkdir -p "${home}/Desktop" 2>/dev/null || true
    [[ -d "${home}/Desktops/Desktop1" ]] && dests+=("${home}/Desktops/Desktop1")
    [[ -d "${home}/Desktop" ]] && dests+=("${home}/Desktop")
  fi
  for d in "${dests[@]}"; do
    run_as_root mkdir -p "$d" 2>/dev/null || true
    if run_as_root cp -f "${prefix}/SAUMI.desktop" "${d}/SAUMI.desktop" 2>/dev/null; then
      run_as_root chown "${user}:${ug}" "${d}/SAUMI.desktop" 2>/dev/null || true
      run_as_root chmod 755 "${d}/SAUMI.desktop" 2>/dev/null || true
      sudo -u "$user" gio set "${d}/SAUMI.desktop" metadata::trusted true 2>/dev/null || true
      log "  ярлык → ${d}/SAUMI.desktop"
    else
      log "  WARN: не удалось ярлык в ${d}"
    fi
    if run_as_root cp -f "${prefix}/SAUMI-force-kill.desktop" "${d}/SAUMI-force-kill.desktop" 2>/dev/null; then
      run_as_root chown "${user}:${ug}" "${d}/SAUMI-force-kill.desktop" 2>/dev/null || true
      run_as_root chmod 755 "${d}/SAUMI-force-kill.desktop" 2>/dev/null || true
      sudo -u "$user" gio set "${d}/SAUMI-force-kill.desktop" metadata::trusted true 2>/dev/null || true
      log "  ярлык → ${d}/SAUMI-force-kill.desktop (Принудительное закрытие)"
    fi
    if run_as_root cp -f "${prefix}/${export_desk_name}" "${d}/${export_desk_name}" 2>/dev/null; then
      run_as_root chown "${user}:${ug}" "${d}/${export_desk_name}" 2>/dev/null || true
      run_as_root chmod 755 "${d}/${export_desk_name}" 2>/dev/null || true
      sudo -u "$user" gio set "${d}/${export_desk_name}" metadata::trusted true 2>/dev/null || true
      log "  ярлык → ${d}/${export_desk_name} (папка Excel-экспорта)"
    else
      log "  WARN: не удалось ярлык экспорта в ${d}"
    fi
  done
}


remove_saumi_dir() {
  local path="$1"
  [[ -e "$path" ]] || return 0
  log "  удаляем старый ${path}"
  if command -v chattr >/dev/null 2>&1; then
    run_as_root chattr -R -i -- "$path" 2>/dev/null || true
  fi
  run_as_root rm -rf -- "$path" || fail "Не удалось удалить ${path}"
}

unpack_archive_to() {
  local home="$1"
  local user="$2"
  local dest="${home}/Saumi"
  local ug TAR_ERR TAR_FLAGS _b _m
  TAR_ERR="/tmp/saumi_tar_${user}_$$.err"
  : > "${TAR_ERR}" 2>/dev/null || TAR_ERR="$(mktemp /tmp/saumi_tar_XXXXXX.err)"
  chmod 666 "${TAR_ERR}" 2>/dev/null || true
  run_as_root rm -rf -- "${home}/._Saumi" "${home}/._SAUMIwine" "${home}/.DS_Store" 2>/dev/null || true
  log "  tar -xzf → ${dest} (free=$(df -h "$home" 2>/dev/null | awk 'NR==2{print $4}' || echo '?'))"
  run_as_root mkdir -p "$home"
  TAR_FLAGS=(-xzf "${ARCHIVE}" -C "$home"
    --delay-directory-restore
    --transform='s|^SAUMIwine/|Saumi/|'
    --transform='s|^SAUMIwine$|Saumi|'
    --transform='s|^Saumi/|Saumi/|'
    --transform='s|^Saumi$|Saumi|')
  if tar --version 2>/dev/null | head -1 | grep -qi 'GNU tar'; then
    TAR_FLAGS+=(--no-same-owner --no-same-permissions --warning=no-unknown-keyword)
  fi
  if ! run_as_root tar "${TAR_FLAGS[@]}" 2>"${TAR_ERR}"; then
    log "  tar stderr:"
    if [[ -s "${TAR_ERR}" ]]; then
      grep -v 'LIBARCHIVE\|com.apple\|Игнорируется неизвестное' "${TAR_ERR}" 2>/dev/null \
        | tail -n 40 | while IFS= read -r _l; do log "    ${_l}"; done || true
    fi
    fail "Не удалось распаковать архив в ${dest}"
  fi
  if [[ ! -d "${dest}/drive_c" && -d "${home}/SAUMIwine/drive_c" ]]; then
    log "  WARN: архив лёг как SAUMIwine — переименовываем в Saumi"
    run_as_root rm -rf -- "$dest" 2>/dev/null || true
    run_as_root mv "${home}/SAUMIwine" "$dest" || fail "Не удалось mv ${home}/SAUMIwine → ${dest}"
  fi
  if [[ -d "$dest" ]]; then
    run_as_root find "$dest" \( -name '._*' -o -name '.DS_Store' \) -delete 2>/dev/null || true
  fi
  run_as_root rm -rf -- "${home}/._Saumi" "${home}/._SAUMIwine" 2>/dev/null || true
  if [[ ! -d "${dest}/drive_c" ]]; then
    log "  содержимое ${home}:"
    run_as_root ls -la "$home" 2>/dev/null | while IFS= read -r _l; do log "    ${_l}"; done || true
    if [[ -d "$dest" ]]; then
      log "  содержимое ${dest} (нет drive_c):"
      run_as_root ls -la "$dest" 2>/dev/null | head -40 | while IFS= read -r _l; do log "    ${_l}"; done || true
      run_as_root du -sh "$dest" 2>/dev/null | while IFS= read -r _l; do log "    size: ${_l}"; done || true
    fi
    if [[ -s "${TAR_ERR}" ]]; then
      log "  tar stderr:"
      grep -v 'LIBARCHIVE\|com.apple\|Игнорируется неизвестное' "${TAR_ERR}" 2>/dev/null \
        | tail -n 30 | while IFS= read -r _l; do log "    ${_l}"; done || true
    fi
    rm -f "${TAR_ERR}"
    fail "После распаковки нет ${dest}/drive_c (квота/права/место? см. лог)"
  fi
  rm -f "${TAR_ERR}"
  verify_unpack_mmp32 "${dest}/drive_c/SAUMI/mmp32.exe"
  ug="$(id -gn "$user" 2>/dev/null || echo "$user")"
  run_as_root mkdir -p "${dest}/logs" "${dest}/bin" "${dest}/drive_c/SAUMI/export"
  run_as_root chown -R "${user}:${ug}" -- "$dest"
  run_as_root chmod +x "${dest}"/*.sh "${dest}/bin"/*.sh 2>/dev/null || true
  # После chown: вернуть Изменён+Создан mmp32 к дате из архива (size/sha уже эталон).
  # Без верного birth SAUMI лезет в автообновление → «Saumi.exe занят».
  restore_mmp32_archive_times "${dest}/drive_c/SAUMI/mmp32.exe" "$user" \
    || fail "mmp32 Создан(birth) не выставлен — прервано (см. лог). Нужны debugfs + ext4."
  fix_prefix_owner "$dest" "$user"
  # chown не должен сбивать birth; перепроверим на всякий случай
  _b="$(run_as_root stat -c '%W' "${dest}/drive_c/SAUMI/mmp32.exe" 2>/dev/null || echo 0)"
  _m="$(archive_mmp32_mtime "${ARCHIVE}")"
  if [[ -n "${_m}" && "${_b}" != "${_m}" ]]; then
    fail "mmp32 birth сбился после fix_prefix_owner (birth=${_b} цель=${_m})"
  fi
}

path_is_file() {
  local p="$1"
  [[ -f "$p" ]] && return 0
  run_as_root test -f "$p" 2>/dev/null
}

path_is_exec() {
  local p="$1"
  [[ -x "$p" ]] && return 0
  run_as_root test -x "$p" 2>/dev/null
}

file_size() {
  local p="$1" s
  s=$(stat -c%s "$p" 2>/dev/null || true)
  if [[ -z "$s" ]]; then
    s=$(run_as_root stat -c%s "$p" 2>/dev/null || true)
  fi
  echo "${s:-0}"
}

wine11_version_ok() {
  local bin="$1" ver
  path_is_exec "$bin" || return 1
  if [[ -x "$bin" ]]; then
    ver="$("$bin" --version 2>/dev/null || true)"
  else
    ver="$(run_as_root "$bin" --version 2>/dev/null || true)"
  fi
  [[ "$ver" == *"${SAUMI_WINE_EXPECT_PREFIX}"* ]]
}


configure_wine_prefix() {
  # $1=prefix $2=user — wine/reg ОБЯЗАТЕЛЬНО от владельца prefix.
  # Иначе после chown: «wine: … is not owned by you» → overrides/X11 не пишутся.
  # Для доменных — runuser, не sudo -u (PAM).
  local prefix="$1" user="${2:-}" WINE SYS32 SQLNET _wver user_home
  local _own_before=0 _own_after=0 _pam_before=0 _pam_after=0
  export WINEPREFIX="${prefix}" WINEARCH=win32
  WINE="${prefix}/${SAUMI_WINE_RUNTIME_REL}/bin/wine"
  export WINESERVER="${prefix}/${SAUMI_WINE_RUNTIME_REL}/bin/wineserver"
  if [[ ! -x "$WINE" ]]; then
    fail "В архиве нет ${SAUMI_WINE_RUNTIME_REL}/bin/wine"
  fi
  if [[ -z "$user" || "$user" == "root" ]]; then
    if [[ "$prefix" =~ ^/home/([^/]+)/ ]]; then
      user="${BASH_REMATCH[1]}"
    fi
  fi
  user_home=""
  if [[ -n "$user" && "$user" != "root" ]]; then
    user_home="$(getent passwd "$user" | cut -d: -f6 || true)"
    [[ -n "$user_home" ]] || user_home="$(dirname "$prefix")"
  fi
  _wver="$("$WINE" --version 2>&1 || true)"
  if [[ -n "$user" && "$user" != "root" ]] && command -v runuser >/dev/null 2>&1; then
    log "  Wine: ${_wver} (configure as user=${user} via runuser)"
  else
    log "  Wine: ${_wver} (configure as user=${user:-root})"
  fi
  if [[ -n "$user" && "$user" != "root" ]]; then
    run_cmd_as_user "$user" \
      WINEPREFIX="${prefix}" WINEARCH=win32 WINESERVER="${WINESERVER}" \
      "${WINESERVER}" -k 2>/dev/null || true
  else
    "${WINESERVER}" -k 2>/dev/null || true
  fi

  run_wine_as_owner() {
    if [[ -n "$user" && "$user" != "root" ]]; then
      run_cmd_as_user "$user" \
        WINEPREFIX="${prefix}" WINEARCH=win32 WINESERVER="${WINESERVER}" \
        DISPLAY="${DISPLAY:-:0}" XAUTHORITY="${XAUTHORITY:-${user_home}/.Xauthority}" \
        "$WINE" "$@"
    else
      if command -v timeout >/dev/null 2>&1; then
        timeout 90 env WINEPREFIX="${prefix}" WINEARCH=win32 WINESERVER="${WINESERVER}" \
          "$WINE" "$@"
      else
        env WINEPREFIX="${prefix}" WINEARCH=win32 WINESERVER="${WINESERVER}" \
          "$WINE" "$@"
      fi
    fi
  }

  wine_reg() {
    if ! run_wine_as_owner reg add "$@" >/dev/null 2>>"${CURRENT_LOG}"; then
      fail "wine reg failed user=${user:-root}: $* — overrides не записаны (см. PAM/password/runuser в логе)"
    fi
  }

  _own_before="$(grep -c 'is not owned by you' "${CURRENT_LOG}" 2>/dev/null || echo 0)"
  _pam_before="$(grep -cE 'PAM account management|a password is required' "${CURRENT_LOG}" 2>/dev/null || echo 0)"
  log "  настройка Wine для ${prefix} (owner=${user:-root})..."
  wine_reg 'HKCU\Software\Wine\DllOverrides' /v '*mscoree' /t REG_SZ /d native /f
wine_reg 'HKCU\Software\Wine\DllOverrides' /v '*mscorwks' /t REG_SZ /d native /f
wine_reg 'HKCU\Software\Wine\DllOverrides' /v 'mscoree' /t REG_SZ /d native /f
wine_reg 'HKCU\Software\Wine\DllOverrides' /v '*fusion' /t REG_SZ /d native /f
wine_reg 'HKCU\Software\Wine\DllOverrides' /v 'fusion' /t REG_SZ /d native /f
wine_reg 'HKCU\Software\Wine\DllOverrides' /v '*msxml3' /t REG_SZ /d builtin /f
wine_reg 'HKCU\Software\Wine\DllOverrides' /v '*msxml4' /t REG_SZ /d builtin /f
wine_reg 'HKCU\Software\Wine\DllOverrides' /v '*msxml6' /t REG_SZ /d builtin /f
wine_reg 'HKCU\Software\Wine\DllOverrides' /v '*comctl32' /t REG_SZ /d native,builtin /f
wine_reg 'HKCU\Software\Wine\DllOverrides' /v '*riched20' /t REG_SZ /d native,builtin /f
wine_reg 'HKCU\Software\Wine\DllOverrides' /v 'psapi' /t REG_SZ /d builtin /f
wine_reg 'HKCU\Software\Wine\DllOverrides' /v '*psapi' /t REG_SZ /d builtin /f
log "  DLL overrides OK (msxml builtin, psapi builtin)"
# Режим окон: по умолчанию 2 (Managed=Y, рабочий). SAUMI_WM_MODE=1 → Managed=N.
_install_wm_mode="${SAUMI_WM_MODE:-2}"
_install_wm_mode="$(echo "${_install_wm_mode}" | tr -cd '12' | head -c 1)"
_install_wm_mode="${_install_wm_mode:-2}"
run_as_root mkdir -p "${prefix}/config"
echo "${_install_wm_mode}" | run_as_root tee "${prefix}/config/wm_mode" >/dev/null
if [[ "${_install_wm_mode}" == "1" ]]; then
  wine_reg 'HKCU\Software\Wine\X11 Driver' /v Managed /t REG_SZ /d N /f
  log "  X11 Managed=N (WM_MODE=1 запасной)"
else
  wine_reg 'HKCU\Software\Wine\X11 Driver' /v Managed /t REG_SZ /d Y /f
  wine_reg 'HKCU\Software\Wine\X11 Driver' /v UseTakeFocus /t REG_SZ /d Y /f
  log "  X11 Managed=Y UseTakeFocus=Y (WM_MODE=2 по умолчанию)"
fi
wine_reg 'HKCU\Software\Wine\X11 Driver' /v Decorated /t REG_SZ /d Y /f
# UseTakeFocus для режима 1 — N; для режима 2 уже выставлен Y выше
if [[ "${_install_wm_mode}" == "1" ]]; then
  wine_reg 'HKCU\Software\Wine\X11 Driver' /v UseTakeFocus /t REG_SZ /d N /f
fi
WPF_KEY='HKLM\Software\Microsoft\NET Framework Setup\NDP\v3.0\Setup\Windows Presentation Foundation'
wine_reg "$WPF_KEY" /ve /t REG_SZ /d 'Windows Presentation Foundation' /f
wine_reg "$WPF_KEY" /v InstallRoot /t REG_SZ /d 'C:\windows\Microsoft.NET\Framework\v3.0\WPF\' /f
wine_reg "$WPF_KEY" /v InstallPath /t REG_SZ /d 'C:\windows\Microsoft.NET\Framework\v3.0\WPF\' /f
wine_reg "$WPF_KEY" /v InstallSuccess /t REG_DWORD /d 1 /f
log "  WPF InstallRoot OK (2.8)"
wine_reg 'HKLM\System\CurrentControlSet\Control\Nls\CodePage' /v ACP /t REG_SZ /d 1251 /f
wine_reg 'HKLM\System\CurrentControlSet\Control\Nls\CodePage' /v OEMCP /t REG_SZ /d 866 /f
log "  ACP=1251 OEMCP=866 OK"
wine_reg 'HKCU\Software\Wine\Environment' /v ORACLE_HOME /t REG_SZ /d 'C:\oracle' /f
wine_reg 'HKCU\Software\Wine\Environment' /v TNS_ADMIN /t REG_SZ /d 'C:\oracle\NETWORK\ADMIN' /f
wine_reg 'HKCU\Software\Wine\Environment' /v NLS_LANG /t REG_SZ /d 'AMERICAN_AMERICA.CL8MSWIN1251' /f
wine_reg 'HKCU\Software\Wine\Environment' /v PATH /t REG_SZ \
  /d 'C:\windows\Microsoft.NET\Framework\v3.0\WPF;C:\oracle\bin;C:\SAUMI;C:\windows\system32;C:\windows' /f
log "  Oracle/WPF PATH OK"

SYS32="${prefix}/drive_c/windows/system32"
for src in \
  "${prefix}/${SAUMI_WINE_RUNTIME_REL}/lib/wine/i386-windows" \
  "${prefix}/${SAUMI_WINE_RUNTIME_REL}/lib32/wine/i386-windows"
do
  [[ -d "$src" ]] || continue
  for d in msxml3.dll msxml4.dll msxml6.dll; do
    [[ -f "$src/$d" ]] && cp -f "$src/$d" "$SYS32/$d" 2>/dev/null \
      || run_as_root cp -f "$src/$d" "$SYS32/$d"
    log "  msxml: $d ← $src"
  done
  break
done
log "  msxml dll скопированы, регистрация COM..."

reg_msxml() {
  local dll="$1"
  export DISPLAY="${DISPLAY:-:0}"
  if run_wine_as_owner regsvr32 /S "$dll" >>"${CURRENT_LOG}" 2>&1; then
    log "  regsvr32 OK $dll"
  else
    log "  WARN: regsvr32 $dll (можно продолжить)"
  fi
  if [[ -n "$user" && "$user" != "root" ]]; then
    run_cmd_as_user "$user" \
      WINEPREFIX="${prefix}" WINEARCH=win32 WINESERVER="${WINESERVER}" \
      "${WINESERVER}" -k 2>/dev/null || true
  else
    [[ -n "${WINESERVER:-}" ]] && "${WINESERVER}" -k 2>/dev/null || true
  fi
}
reg_msxml 'C:\windows\system32\msxml3.dll'
reg_msxml 'C:\windows\system32\msxml4.dll'
reg_msxml 'C:\windows\system32\msxml6.dll'
log "  msxml регистрация завершена"

[[ -f "${prefix}/drive_c/SAUMI/inflexe_gw.dll" ]] \
  && log "  inflexe_gw.dll OK (из архива)" \
  || log "WARN: нет inflexe_gw.dll в архиве"

wine_reg 'HKLM\Software\XS\SAUMI 4.0' /v SetupPath /t REG_SZ /d 'C:\SAUMI' /f
wine_reg 'HKLM\Software\XS\SAUMI 4.0' /v SetupType /t REG_SZ /d 'ADMIN' /f
wine_reg 'HKCU\Software\XS\SAUMI 4.0' /v SetupPath /t REG_SZ /d 'C:\SAUMI' /f
wine_reg 'HKCU\Software\XS\SAUMI 4.0' /v SetupType /t REG_SZ /d 'ADMIN' /f
log "  реестр XS\\SAUMI 4.0 SetupPath/SetupType OK"

if path_is_file "${prefix}/drive_c/SAUMI/SaumiExcelProxy.dll"; then
  if path_is_file "${prefix}/drive_c/SAUMI/SaumiExcelProxy.reg"; then
    # Offline merge в system.reg — wine regedit под root на чужой WINEPREFIX падает
    # («is not owned by you»), а verify раньше смотрел только на файлы.
    _excel_merged=0
    if command -v python3 >/dev/null 2>&1; then
      _merge_py=""
      for _cand in \
        "${SCRIPT_DIR}/merge_excel_proxy_registry.py" \
        "${SCRIPT_DIR}/../files/scripts/merge_excel_proxy_registry.py"
      do
        [[ -f "$_cand" ]] && _merge_py="$_cand" && break
      done
      if [[ -n "$_merge_py" ]]; then
        _bak="${prefix}/logs/system.reg.bak_excel_$(date +%Y%m%d_%H%M%S)"
        run_as_root mkdir -p "${prefix}/logs" 2>/dev/null || mkdir -p "${prefix}/logs" || true
        if python3 "$_merge_py" \
             "${prefix}/drive_c/SAUMI/SaumiExcelProxy.reg" \
             "${prefix}/system.reg" \
             "$_bak" >>"${CURRENT_LOG}" 2>&1 \
           || run_as_root python3 "$_merge_py" \
             "${prefix}/drive_c/SAUMI/SaumiExcelProxy.reg" \
             "${prefix}/system.reg" \
             "$_bak" >>"${CURRENT_LOG}" 2>&1; then
          if grep -q 'Excel.Application' "${prefix}/system.reg" 2>/dev/null \
             || run_as_root grep -q 'Excel.Application' "${prefix}/system.reg" 2>/dev/null; then
            log "  Excel.Application → system.reg OK (offline merge)"
            _excel_merged=1
          fi
        fi
      fi
    fi
    if [[ "$_excel_merged" -eq 0 ]]; then
      # fallback: wine от имени владельца prefix
      _owner="$(stat -c%U "${prefix}" 2>/dev/null || echo "")"
      if [[ -n "$_owner" && "$_owner" != "root" ]] && command -v runuser >/dev/null 2>&1; then
        timeout 60 runuser -u "$_owner" -- env WINEPREFIX="${prefix}" WINEARCH=win32 \
          WINESERVER="${WINESERVER:-}" "$WINE" regedit /S 'C:\SAUMI\SaumiExcelProxy.reg' \
          >>"${CURRENT_LOG}" 2>&1 \
          && log "  Excel.Application → SaumiExcelProxy.reg OK (runuser ${_owner})" \
          || log "  WARN: regedit Excel (runuser) не удался"
      else
        log "  WARN: offline merge Excel недоступен — скопируйте merge_excel_proxy_registry.py рядом с установщиком"
      fi
    fi
  else
    log "  WARN: есть SaumiExcelProxy.dll, нет .reg"
  fi
  mkdir -p "${prefix}/drive_c/SAUMI/export" 2>/dev/null \
    || run_as_root mkdir -p "${prefix}/drive_c/SAUMI/export" || true
else
  log "  WARN: нет SaumiExcelProxy.dll в архиве — Excel-экспорт не будет работать"
fi

SQLNET="${prefix}/drive_c/oracle/NETWORK/ADMIN/sqlnet.ora"
if [[ -f "$SQLNET" ]]; then
  if ! cat >"$SQLNET" <<'SQL'
SQLNET.AUTHENTICATION_SERVICES = (NONE)
NAMES.DIRECTORY_PATH = (TNSNAMES, EZCONNECT)
SQL
  then
    run_as_root tee "$SQLNET" >/dev/null <<'SQL'
SQLNET.AUTHENTICATION_SERVICES = (NONE)
NAMES.DIRECTORY_PATH = (TNSNAMES, EZCONNECT)
SQL
  fi
fi

  head -1 "${prefix}/system.reg" | grep -q 'WINE REGISTRY' \
    || fail "Повреждён system.reg в ${prefix}"

  _own_after="$(grep -c 'is not owned by you' "${CURRENT_LOG}" 2>/dev/null || echo 0)"
  _own_before="${_own_before:-0}"
  if [[ "${_own_after}" -gt "${_own_before}" ]]; then
    fail "wine: «is not owned by you» при configure ${prefix} (user=${user:-?}) — overrides не применились. Нужен запуск wine от владельца."
  fi
  _pam_after="$(grep -cE 'PAM account management|a password is required' "${CURRENT_LOG}" 2>/dev/null || echo 0)"
  _pam_before="${_pam_before:-0}"
  if [[ "${_pam_after}" -gt "${_pam_before}" ]]; then
    fail "PAM/sudo при configure ${prefix} (user=${user:-?}) — wine от владельца не выполнился (нужен runuser)."
  fi
  log "  configure_wine_prefix OK (no ownership/PAM reject)"

  # wine/reg/merge могли создать файлы от root
  fix_prefix_owner "$prefix" "$user"
}

mmp32_sha_ok() {
  local mmp="$1"
  local mmp_sz mmp_sha
  path_is_file "$mmp" || return 1
  mmp_sz=$(file_size "$mmp")
  mmp_sha=$(run_as_root sha256sum "$mmp" 2>/dev/null | awk '{print $1}' || true)
  [[ "$mmp_sz" == "$MMP32_SIZE" && "$mmp_sha" == "$MMP32_SHA" ]]
}

archive_mmp32_mtime() {
  python3 -c 'import tarfile,sys; m=tarfile.open(sys.argv[1],"r:gz").getmember("SAUMIwine/drive_c/SAUMI/mmp32.exe"); print(int(m.mtime))' "$1" 2>/dev/null || true
}

# После tar: size/sha эталон; Изменён+Создан = mtime из архива.
# birth: sibling + debugfs(crtime @epoch) + mv. Формат @epoch — как в e2fsprogs (не YYYYMMDD).
# Возврат 0 только если birth==mtime_архива.
_mmp32_block_device() {
  local path="$1" src
  src="$(run_as_root findmnt -n -o SOURCE -T "$path" 2>/dev/null | head -1 | tr -d ' ' || true)"
  [[ -z "$src" ]] && src="$(run_as_root df --output=source "$path" 2>/dev/null | tail -1 | tr -d ' ' || true)"
  [[ -n "$src" ]] && src="$(run_as_root readlink -f "$src" 2>/dev/null || echo "$src")"
  printf '%s' "$src"
}

_mmp32_debugfs_stat_crtime() {
  local inode="$1" device="$2"
  # строка вида: crtime: 0x... -- Mon Jun 28 ...
  run_as_root debugfs -R "stat <${inode}>" "$device" 2>/dev/null | grep -E '^[[:space:]]*crtime:' || true
}

_mmp32_debugfs_set_crtime() {
  local inode="$1" epoch="$2" device="$3" cmdf out rc=0
  cmdf="$(mktemp /tmp/saumi_dbgcmd_XXXXXX.txt)"
  out="$(mktemp /tmp/saumi_debugfs_XXXXXX.log)"
  # -f здесь = файл команд (правильно). Значение crtime: @unix_epoch
  printf 'set_inode_field <%s> crtime @%s\nset_inode_field <%s> crtime_extra 0\n' \
    "$inode" "$epoch" "$inode" >"$cmdf"
  log "  debugfs: set crtime @${epoch} inode=${inode} device=${device}"
  if run_as_root debugfs -w -f "$cmdf" "$device" >"$out" 2>&1; then
    :
  else
    rc=$?
  fi
  if [[ -s "$out" ]]; then
    while IFS= read -r _l; do log "    ${_l}"; done <"$out" || true
  fi
  rm -f "$cmdf" "$out"
  run_as_root sync
  run_as_root blockdev --flushbufs "$device" 2>/dev/null || true
  run_as_root bash -c 'echo 3 >/proc/sys/vm/drop_caches' 2>/dev/null || true
  sleep 0.4
  log "  debugfs after: $(_mmp32_debugfs_stat_crtime "$inode" "$device" | tr -s ' ')"
  return "$rc"
}

restore_mmp32_archive_times() {
  local mmp="$1"
  local user="${2:-}"
  local mt_tar inode device mt_after birth_after tmp ug fstype has_dbg
  path_is_file "$mmp" || return 1
  mt_tar="$(archive_mmp32_mtime "${ARCHIVE}")"
  [[ -n "$mt_tar" ]] || { log "  FAIL mmp32: нет mtime в tar"; return 1; }
  ug="$(id -gn "${user:-root}" 2>/dev/null || echo "${user:-root}")"
  fstype="$(run_as_root df -T "$mmp" 2>/dev/null | awk 'NR==2{print $2}')"
  device="$(_mmp32_block_device "$mmp")"
  has_dbg=0
  if command -v debugfs >/dev/null 2>&1 || run_as_root bash -c 'command -v debugfs' >/dev/null 2>&1; then
    has_dbg=1
  fi
  log "  birth restore: цель=@${mt_tar} fstype=${fstype:-?} device=${device:-?} debugfs=${has_dbg}"

  if [[ -n "$user" ]]; then
    local _pfx
    _pfx="$(cd "$(dirname "$mmp")/../.." && pwd)"
    run_as_root pkill -9 -u "$user" -f '[m]mp32\.exe' 2>/dev/null || true
    run_as_root pkill -9 -u "$user" -f '[E]xplorerWindow' 2>/dev/null || true
    run_as_root pkill -9 -u "$user" -f '[s]aumi\.exe' 2>/dev/null || true
    if [[ -x "${_pfx}/runtime/wine-11.0/bin/wineserver" ]]; then
      run_as_root sudo -u "$user" env WINEPREFIX="${_pfx}" \
        "${_pfx}/runtime/wine-11.0/bin/wineserver" -k 2>/dev/null || true
    fi
    command -v fuser >/dev/null 2>&1 && run_as_root fuser -k "$mmp" 2>/dev/null || true
    sleep 0.5
  fi
  run_as_root chattr -i -- "$mmp" 2>/dev/null || true

  if [[ "$has_dbg" -ne 1 || "$device" != /dev/* ]]; then
    log "  FAIL mmp32 birth: нужен debugfs и блочное устройство (device=${device:-?})"
    return 1
  fi
  if [[ "${fstype}" != ext4 && "${fstype}" != ext3 ]]; then
    log "  FAIL mmp32 birth: fstype=${fstype:-?} (нужен ext4/ext3)"
    return 1
  fi

  tmp="${mmp}.__birthfix"
  run_as_root rm -f -- "$tmp" 2>/dev/null || true
  run_as_root cp -f -- "$mmp" "$tmp"
  run_as_root sync
  inode="$(run_as_root stat -c '%i' "$tmp" 2>/dev/null || true)"
  [[ -n "$inode" ]] || { log "  FAIL mmp32 birth: нет inode у tmp"; run_as_root rm -f -- "$tmp"; return 1; }

  log "  debugfs before: $(_mmp32_debugfs_stat_crtime "$inode" "$device" | tr -s ' ')"
  _mmp32_debugfs_set_crtime "$inode" "$mt_tar" "$device" || true
  birth_after="$(run_as_root stat -c '%W' "$tmp" 2>/dev/null || echo 0)"
  if [[ "${birth_after}" != "$mt_tar" ]]; then
    log "  FAIL mmp32 birth на tmp: stat=%W→${birth_after} цель=${mt_tar} (inode=${inode})"
    run_as_root rm -f -- "$tmp"
    return 1
  fi

  run_as_root mv -f -- "$tmp" "$mmp"
  [[ -n "$user" ]] && run_as_root chown "${user}:${ug}" -- "$mmp" 2>/dev/null || true
  run_as_root touch -d "@${mt_tar}" -- "$mmp" 2>/dev/null || true
  run_as_root sync
  run_as_root bash -c 'echo 3 >/proc/sys/vm/drop_caches' 2>/dev/null || true

  mt_after="$(run_as_root stat -c '%Y' "$mmp" 2>/dev/null || true)"
  birth_after="$(run_as_root stat -c '%W' "$mmp" 2>/dev/null || echo 0)"
  if [[ "$mt_after" != "$mt_tar" ]]; then
    log "  FAIL mmp32 Изменён disk=${mt_after} ≠ архив=${mt_tar}"
    return 1
  fi
  log "  mmp32 Изменён(mtime)=${mt_after} (=архив)"
  if [[ "${birth_after}" != "$mt_tar" ]]; then
    log "  birth после mv сбился (${birth_after}) — повтор debugfs"
    inode="$(run_as_root stat -c '%i' "$mmp" 2>/dev/null || true)"
    _mmp32_debugfs_set_crtime "$inode" "$mt_tar" "$device" || true
    run_as_root touch -d "@${mt_tar}" -- "$mmp" 2>/dev/null || true
    birth_after="$(run_as_root stat -c '%W' "$mmp" 2>/dev/null || echo 0)"
  fi
  if [[ "${birth_after}" == "$mt_tar" ]]; then
    log "  mmp32 Создан(birth)=${birth_after} (=архив) fstype=${fstype}"
    return 0
  fi
  log "  FAIL mmp32 Создан(birth)=${birth_after} цель=${mt_tar} fstype=${fstype} device=${device}"
  return 1
}

verify_unpack_mmp32() {
  local mmp="$1" sz sha mt_tar mt_disk birth_disk
  path_is_file "$mmp" || fail "нет mmp32 после распаковки tar"
  sz=$(file_size "$mmp")
  sha=$(run_as_root sha256sum "$mmp" 2>/dev/null | awk '{print $1}' || true)
  if ! mmp32_sha_ok "$mmp"; then
    fail "mmp32 повреждён при распаковке (size=${sz}, ожид. ${MMP32_SIZE}; sha=${sha:0:16}…)"
  fi
  mt_tar="$(archive_mmp32_mtime "${ARCHIVE}")"
  mt_disk="$(run_as_root stat -c '%Y' "$mmp" 2>/dev/null || true)"
  birth_disk="$(run_as_root stat -c '%W' "$mmp" 2>/dev/null || true)"
  log "  OK mmp32 после tar: size=${MMP32_SIZE} sha=${MMP32_SHA:0:12}… (mtime в tar ${mt_tar:-?})"
  log "  (даты до restore: Изменён=${mt_disk:-?} Создан=${birth_disk:-?})"
}

verify_archive_mmp32() {
  local arch="$1" sha sz
  [[ -f "$arch" ]] || fail "Нет архива ${arch}"
  sz=$(stat -c%s "$arch" 2>/dev/null || stat -f%z "$arch")
  log "Архив: $(basename "$arch") size=${sz} bytes"
  if [[ "$sz" -lt 1700000000 ]]; then
    fail "Архив слишком мал (${sz}) — нужен пересобранный tar ~1.8G, не старый 1.4G"
  fi
  sha=$(tar -xOf "$arch" SAUMIwine/drive_c/SAUMI/mmp32.exe 2>/dev/null | sha256sum | awk '{print $1}')
  if [[ "$sha" == "$MMP32_SHA" ]]; then
    log "OK: mmp32 в tar sha=${sha:0:12}…"
  else
    fail "mmp32 в tar битый (sha=${sha:0:12}…). Залейте новый SAUMI1860wine.tar.gz (~1.8G)"
  fi
}

verify_survivors() {
  local prefix="$1"
  local gac sys32 gsz ssz wpf err=0 mmp mmp_sha mmp_sz
  log "=== verify survivors ==="
  wine11_version_ok "${prefix}/${SAUMI_WINE_RUNTIME_REL}/bin/wine" \
    && log "OK 3.6 wine-11.0" || { log "FAIL 3.6 wine-11"; err=1; }
  path_is_file "${prefix}/drive_c/SAUMI/inflexe_gw.dll" \
    && log "OK 1.3 inflexe_gw.dll" || { log "FAIL 1.3 inflexe_gw"; err=1; }
  if path_is_file "${prefix}/drive_c/SAUMI/SaumiExcelProxy.dll" \
     && path_is_file "${prefix}/drive_c/SAUMI/SaumiExcelProxy.reg"; then
    if python3 - "${prefix}/drive_c/SAUMI/SaumiExcelProxy.dll" <<'PY' 2>/dev/null
import sys
from pathlib import Path
b = Path(sys.argv[1]).read_bytes()
ok = any(
    m.encode("utf-16le") in b
    for m in (
        "SaumiExcelProxy.Xlsx.23",
        "SaumiExcelProxy.Xlsx.22",
        "SaumiExcelProxy.Xlsx.21",
        "SaumiExcelProxy.Xlsx.20",
        "SaumiExcelProxy.Xlsx.19",
        "SaumiExcelProxy.Xlsx.18",
        "SaumiExcelProxy.Xlsx.17",
        "SaumiExcelProxy.Xlsx.16",
        "SaumiExcelProxy.Xlsx.15",
    )
)
sys.exit(0 if ok else 1)
PY
    then
      if grep -q 'Excel.Application' "${prefix}/system.reg" 2>/dev/null \
         || run_as_root grep -q 'Excel.Application' "${prefix}/system.reg" 2>/dev/null; then
        log "OK Excel SaumiExcelProxy Xlsx.15+ FolderSave + .reg + system.reg"
      else
        log "FAIL Excel — dll/reg есть, но Excel.Application нет в system.reg"
        err=1
      fi
    elif strings "${prefix}/drive_c/SAUMI/SaumiExcelProxy.dll" 2>/dev/null | grep -Eq 'SaumiExcelProxy\.Xlsx\.1[567]'; then
      if grep -q 'Excel.Application' "${prefix}/system.reg" 2>/dev/null \
         || run_as_root grep -q 'Excel.Application' "${prefix}/system.reg" 2>/dev/null; then
        log "OK Excel SaumiExcelProxy Xlsx.15+ FolderSave + .reg + system.reg"
      else
        log "FAIL Excel — dll/reg есть, но Excel.Application нет в system.reg"
        err=1
      fi
    else
      log "FAIL Excel SaumiExcelProxy — нужен Xlsx.15+ (FolderSave) в tar"
      err=1
    fi
  else
    log "FAIL Excel SaumiExcelProxy (dll/reg) — нужен актуальный SAUMI1860wine.tar.gz"
    err=1
  fi
  if path_is_file "${prefix}/bin/saumi_hide_windows.sh" \
     && grep -aqF 'hide 3.22' "${prefix}/bin/saumi_hide_windows.sh" 2>/dev/null; then
    log "OK hide 3.22"
  else
    log "FAIL hide 3.22 (bin/saumi_hide_windows.sh)"
    err=1
  fi
  if path_is_file "${prefix}/bin/saumi_taskbar_fix.sh" \
     && grep -aqF 'taskbar_fix' "${prefix}/bin/saumi_taskbar_fix.sh" 2>/dev/null; then
    log "OK taskbar_fix"
  else
    log "FAIL taskbar_fix (bin/saumi_taskbar_fix.sh)"
    err=1
  fi
  if path_is_file "${prefix}/config/wm_mode"; then
    _wm="$(run_as_root cat "${prefix}/config/wm_mode" 2>/dev/null | tr -cd '12' | head -c 1 || true)"
    log "OK wm_mode=${_wm:-?}"
  else
    log "FAIL config/wm_mode"
    err=1
  fi
  path_is_file "${prefix}/drive_c/SAUMI/ExplorerWindow.exe" \
    && log "OK ExplorerWindow.exe" || { log "FAIL ExplorerWindow.exe"; err=1; }
  path_is_file "${prefix}/drive_c/SAUMI/mmp32.exe" \
    && log "OK mmp32.exe" || { log "FAIL mmp32"; err=1; }
  mmp="${prefix}/drive_c/SAUMI/mmp32.exe"
  if mmp32_sha_ok "$mmp"; then
    log "OK mmp32 sha/size = SAUMI485.zip"
  elif path_is_file "$mmp"; then
    mmp_sz=$(file_size "$mmp")
    mmp_sha=$(run_as_root sha256sum "$mmp" 2>/dev/null | awk '{print $1}' || true)
    log "FAIL mmp32 sha/size (size=${mmp_sz} sha=${mmp_sha:0:12}…) — пересоберите tar из SAUMI485.zip"
    err=1
  fi
  wpf="${prefix}/drive_c/windows/Microsoft.NET/Framework/v3.0/WPF/wpfgfx_v0300.dll"
  path_is_file "$wpf" && log "OK 2.8/2.9 wpfgfx" || { log "FAIL 2.8 wpfgfx"; err=1; }
  gac="$(run_as_root find "${prefix}/drive_c/windows/assembly/GAC_MSIL/System" -name psapi.dll 2>/dev/null | head -1 || true)"
  sys32="${prefix}/drive_c/windows/system32/psapi.dll"
  if [[ -n "$gac" ]] && path_is_file "$gac" && path_is_file "$sys32"; then
    gsz=$(file_size "$gac")
    ssz=$(file_size "$sys32")
    if [[ "$gsz" != "$ssz" && "$gsz" -gt 80000 && "$ssz" -gt 60000 ]]; then
      log "OK 3.2 psapi split gac=${gsz} sys32=${ssz}"
    else
      log "FAIL 3.2 psapi sizes gac=${gsz} sys32=${ssz} (нужны разные)"
      err=1
    fi
  else
    log "FAIL 3.2 psapi files missing"; err=1
  fi
  [[ "$err" -eq 0 ]] || fail "Проверка установки провалена — см. ${CURRENT_LOG}"
  log "=== verify OK ==="
}


find_valid_saumi_source() { return 1; }

remove_all_saumi_dirs() {
  local user home dest
  log "=== удаляем все ~/Saumi перед установкой из tar ==="
  while IFS=$'\t' read -r user home; do
    [[ -n "$user" && -n "$home" ]] || continue
    dest="${home}/Saumi"
    stop_saumi_for_user "$user" "$dest" 2>/dev/null || true
    remove_saumi_dir "$dest" 2>/dev/null || true
  done < <(list_human_users)
}

install_saumi_for_user() {
  local user="$1"
  local home="$2"
  local force="${3:-0}"
  local dest="${home}/Saumi"
  [[ -d "$home" ]] || { log "[skip] ${user} — нет ${home}"; return 1; }
  if ! can_provision_user_home "$user" "$home"; then
    return 1
  fi
  if [[ "$force" != "1" ]] && has_saumi_ok "$home"; then
    log "[skip] ${user} — ~/Saumi OK (mmp32 из SAUMI485)"
    write_start_saumi "$dest" "$user"
    write_wm_mode_helpers "$dest" "$user"
    install_user_shortcuts "$user" "$home" || true
    return 0
  fi
  if has_saumi "$home"; then
    log "[reinstall] ${user} — ~/Saumi битый/старый → tar"
  else
    log "[install] ${user} → ${dest} из tar"
  fi
  stop_saumi_for_user "$user" "$dest"
  remove_saumi_dir "$dest"
  unpack_archive_to "$home" "$user"
  configure_wine_prefix "$dest" "$user"
  verify_survivors "$dest"
  write_start_saumi "$dest" "$user"
  write_wm_mode_helpers "$dest" "$user"
  install_user_shortcuts "$user" "$home" || true
  run_as_root mkdir -p "${dest}/logs" 2>/dev/null || true
  run_as_root cp -f "${CURRENT_LOG}" "${dest}/logs/install.log" 2>/dev/null || true
  # финально: всё под user (логи/export/reg после sudo)
  fix_prefix_owner "$dest" "$user"
  log "  OK ${user} (tar)"
  return 0
}

# Диагностика домена / прав / .NET / ExplorerWindow → рядом с установщиком.
# Вызывается автоматически после установки; отдельно: sudo bash instalSaumi.sh --diag
collect_install_diag() {
  local stamp out_dir out_tgz d tag dest f home_user u
  stamp="$(date +%Y%m%d_%H%M%S)"
  out_dir="/tmp/saumi_diag_${stamp}"
  out_tgz="${SCRIPT_DIR}/saumi_diag_${stamp}.tar.gz"
  mkdir -p "${out_dir}"
  log "=== collect diag → ${out_tgz} ==="
  {
    echo "SAUMI diag ${stamp} installer=${INSTALLER_REV}"
    echo "host=$(hostname -f 2>/dev/null || hostname)"
    echo "date=$(date -Iseconds 2>/dev/null || date)"
    echo "id=$(id)"
    uname -a
    cat /etc/os-release 2>/dev/null || true
  } >"${out_dir}/00_host.txt"
  {
    echo "=== sssd / realm ==="
    realm list 2>/dev/null || true
    grep -E 'enumerate|use_fully_qualified|default_domain|^domains' /etc/sssd/sssd.conf 2>/dev/null || true
    echo "=== getent home-users ==="
    getent passwd | awk -F: '$3>=1000 && $3!=65534 && $6 ~ /^\/home\// {print $1 "\t" $3 "\t" $6}'
    echo "=== /home ==="
    ls -la /home 2>/dev/null || true
    for u in "${FOR_USER:-}" "${SAUMI_DIAG_USER:-}" 'ochernaja@Cityhall.krd'; do
      [[ -n "$u" ]] || continue
      echo "--- getent/id $u ---"
      getent passwd "$u" || echo "(no getent)"
      id "$u" 2>&1 || true
    done
  } >"${out_dir}/01_users.txt"
  {
    echo "=== ~/Saumi ownership ==="
    for d in /home/*/Saumi /home/*@*/Saumi; do
      [[ -d "$d" ]] || continue
      echo "---- $d ----"
      ls -lad "$d" "$d/logs" "$d/startSAUMI.sh" "$d/drive_c/SAUMI/mmp32.exe" 2>&1 || true
      stat -c 'owner=%U:%G' "$d" 2>/dev/null || true
      home_user="$(stat -c '%U' "$(dirname "$d")" 2>/dev/null || true)"
      find "$d" -maxdepth 2 \( -user root -o ! -user "$home_user" \) 2>/dev/null \
        | head -40 | sed 's/^/FOREIGN /' || true
      echo
    done
  } >"${out_dir}/02_ownership.txt"
  for d in /home/*/Saumi /home/*@*/Saumi; do
    [[ -d "$d/drive_c/SAUMI" ]] || continue
    tag="$(basename "$(dirname "$d")" | tr '@/ ' '___')"
    dest="${out_dir}/pfx_${tag}"
    mkdir -p "$dest"
    {
      echo "prefix=$d"
      for f in \
        drive_c/windows/system32/mscoree.dll \
        drive_c/windows/Microsoft.NET/Framework/v2.0.50727/mscorwks.dll \
        drive_c/windows/Microsoft.NET/Framework/v2.0.50727/mscorlib.dll \
        drive_c/SAUMI/ExplorerWindow.exe \
        drive_c/SAUMI/mmp32.exe
      do
        if [[ -f "$d/$f" ]]; then sha256sum "$d/$f"; else echo "MISSING $f"; fi
      done
      grep -E -i 'mscoree|mscorwks|fusion' "$d/user.reg" "$d/system.reg" 2>/dev/null | head -40 || true
      grep -nE 'mmp32|WINEDLLOVERRIDES|oldupdate' "$d/startSAUMI.sh" 2>/dev/null | head -20 || true
    } >"${dest}/dotnet.txt"
    if [[ -f "$d/logs/saumi_latest.log" ]]; then
      grep -E -i 'ExplorerWindow|TypeInitialization|mscoree|GetFunctionShim|80070002|e0434f4d|Отказано|Access denied|Application Error' \
        "$d/logs/saumi_latest.log" 2>/dev/null | tail -100 >"${dest}/latest_hits.txt" || true
      tail -120 "$d/logs/saumi_latest.log" >"${dest}/latest_tail.txt" 2>/dev/null || true
    fi
    cp -f "$d/logs/install.log" "${dest}/install.log" 2>/dev/null || true
    cp -f "$d/logs/saumi_run.log" "${dest}/saumi_run.log" 2>/dev/null || true
  done
  cp -f "${INSTALL_LOG_FILE}" "${out_dir}/saumi_install.log" 2>/dev/null || true
  cp -f "${SCRIPT_DIR}/saumi_install_users.txt" "${out_dir}/saumi_install_users.txt" 2>/dev/null || true
  {
    echo "DISPLAY=${DISPLAY:-} XAUTHORITY=${XAUTHORITY:-}"
    (command -v wine; wine --version) 2>&1 || true
    ss -lntp 2>/dev/null | grep -E '318[2-5]' || true
  } >"${out_dir}/03_runtime.txt"
  if tar -czf "${out_tgz}" -C /tmp "$(basename "${out_dir}")" 2>/dev/null; then
    rm -rf "${out_dir}"
    log "diag OK: ${out_tgz}"
    echo "  Диагностика: ${out_tgz}"
  else
    log "WARN: не удалось собрать ${out_tgz}"
    rm -rf "${out_dir}"
  fi
}

log_users_discovery() {
  local user home n_getent=0 n_home=0 n_total=0 line
  log "=== обнаружение пользователей (getent + /home) ==="
  n_getent="$(getent passwd | awk -F: '$3>=1000 && $3!=65534 && $6 ~ /^\/home\// {c++} END{print c+0}')"
  n_home="$(find /home -mindepth 1 -maxdepth 1 -type d ! -name lost+found 2>/dev/null | wc -l | tr -d ' ')"
  log "  getent home-users=${n_getent}  каталогов_/home=${n_home}"
  log "  список к установке:"
  while IFS=$'\t' read -r user home; do
    [[ -n "$user" && -n "$home" ]] || continue
    n_total=$((n_total + 1))
    line="  - ${user} → ${home}"
    if [[ -d "${home}/Saumi/drive_c/SAUMI" ]]; then
      line+=" [есть_Saumi owner=$(stat -c '%U:%G' "${home}/Saumi" 2>/dev/null || echo '?')]"
    else
      line+=" [нет_Saumi]"
    fi
    if [[ "$user" == *@* ]]; then
      line+=" [domain]"
    fi
    log "$line"
  done < <(list_human_users)
  log "  итого к установке: ${n_total}"
  {
    echo "installer=${INSTALLER_REV} host=$(hostname -f 2>/dev/null || hostname) date=$(date -Iseconds)"
    echo "getent_home_users=${n_getent} home_dirs=${n_home} install_targets=${n_total}"
    list_human_users
  } >"${SCRIPT_DIR}/saumi_install_users.txt" 2>/dev/null || true
}

log_prefix_health() {
  local prefix="$1" user="$2"
  local ow mscoree mscorwks expl
  [[ -d "$prefix" ]] || return 0
  ow="$(stat -c '%U:%G' "$prefix" 2>/dev/null || echo '?')"
  mscoree="no"; [[ -f "${prefix}/drive_c/windows/system32/mscoree.dll" ]] && mscoree="yes"
  mscorwks="no"; [[ -f "${prefix}/drive_c/windows/Microsoft.NET/Framework/v2.0.50727/mscorwks.dll" ]] && mscorwks="yes"
  expl="no"; [[ -f "${prefix}/drive_c/SAUMI/ExplorerWindow.exe" ]] && expl="yes"
  log "  health ${user}: owner=${ow} mscoree=${mscoree} mscorwks=${mscorwks} ExplorerWindow=${expl}"
  if [[ -d "${prefix}/logs" ]]; then
    find "${prefix}/logs" -maxdepth 1 -type f ! -user "$user" 2>/dev/null \
      | head -5 | while read -r f; do log "  WARN foreign_log_owner: $f ($(stat -c '%U:%G' "$f"))"; done || true
  fi
}

install_all_users() {
  local force="${1:-0}" user home n=0 skip=0
  ask_sudo_if_needed "распаковать SAUMI в ~/Saumi"
  verify_archive_mmp32 "${ARCHIVE}"
  log "Установка: tar → ~/Saumi, force=${force}"
  log_users_discovery
  if [[ "$force" == "1" ]]; then
    remove_all_saumi_dirs
  fi
  while IFS=$'\t' read -r user home; do
    [[ -n "$user" && -n "$home" ]] || continue
    if install_saumi_for_user "$user" "$home" "$force"; then
      n=$((n + 1))
      log_prefix_health "${home}/Saumi" "$user"
    else
      skip=$((skip + 1))
    fi
  done < <(list_human_users)
  run_as_root mkdir -p /usr/local/share/applications
  run_as_root tee /usr/local/share/applications/SAUMI.desktop >/dev/null <<'EOF'
[Desktop Entry]
Version=1.0
Type=Application
Name=SAUMI
Comment=Ярлык на рабочем столе или ~/Saumi/startSAUMI.sh
Exec=sh -c 'env FORCE_KILL=1 "$HOME/Saumi/startSAUMI.sh"'
Terminal=false
Categories=Office;
EOF
  run_as_root chmod 644 /usr/local/share/applications/SAUMI.desktop
  log "Готово: установлено=${n}, пропусков=${skip}"
}

finalize_install_log() {
  {
    cat "${STAGING_LOG}" 2>/dev/null || true
    [[ -f "${CURRENT_LOG}" && "${CURRENT_LOG}" != "${STAGING_LOG}" ]] && cat "${CURRENT_LOG}" 2>/dev/null || true
  } > "${INSTALL_LOG_FILE}" 2>/dev/null || true
  CURRENT_LOG="${INSTALL_LOG_FILE}"
  cp -f "${INSTALL_LOG_FILE}" "${SCRIPT_DIR}/saumi_install_last.log" 2>/dev/null || true
  if [[ -n "${INVOKER:-}" && "${INVOKER}" != "root" ]]; then
    local ug
    ug="$(id -gn "${INVOKER}" 2>/dev/null || echo "${INVOKER}")"
    chown "${INVOKER}:${ug}" "${INSTALL_LOG_FILE}" "${SCRIPT_DIR}/saumi_install_last.log" "${STAGING_LOG:-}" 2>/dev/null || true
    chmod 644 "${INSTALL_LOG_FILE}" "${SCRIPT_DIR}/saumi_install_last.log" "${STAGING_LOG:-}" 2>/dev/null || true
  fi
}

# Служба лицензий Guardant — только текущий пользователь (INVOKER)
run_licence_server_install() {
  local home prefix script
  home="$(getent passwd "${INVOKER}" | cut -d: -f6 || true)"
  [[ -n "$home" ]] || home="${HOME}"
  prefix="${home}/Saumi"
  [[ -d "${prefix}/drive_c/SAUMI" ]] || fail "Нет ${prefix} — сначала установите SAUMI"
  script="${prefix}/bin/install_licence_server.sh"
  [[ -f "$script" ]] || fail "В архиве нет install_licence_server.sh — нужен комплект prod-4"
  # актуальные скрипты Guardant из tar (на старых prefix могли отсутствовать)
  if [[ -f "${ARCHIVE}" ]]; then
    tmpdir="$(mktemp -d /tmp/saumi_lic_scripts_XXXXXX)"
    for s in install_licence_server.sh start_licence_server.sh stop_licence_server.sh probe_licence_server.sh; do
      tar -xzf "${ARCHIVE}" -C "$tmpdir" "SAUMIwine/bin/${s}" 2>/dev/null || true
      if [[ -f "${tmpdir}/SAUMIwine/bin/${s}" ]]; then
        cp -f "${tmpdir}/SAUMIwine/bin/${s}" "${prefix}/bin/${s}"
        chmod 755 "${prefix}/bin/${s}"
      fi
    done
    rm -rf "$tmpdir"
  fi
  log "=== служба лицензий Guardant (user=${INVOKER}) ==="
  ask_sudo_if_needed "открыть порты лицензии 3182-3185"
  sudo -u "${INVOKER}" env \
    HOME="$home" \
    SAUMI_PREFIX="$prefix" \
    DISPLAY="${DISPLAY:-:0}" \
    XAUTHORITY="${home}/.Xauthority" \
    bash "$script" \
    || fail "Установка службы лицензий не удалась (ключ Guardant? см. ${prefix}/logs/licence_install.log)"
  log "служба лицензий: OK (логи ${prefix}/logs/licence_*.log)"
}

# ГИС ГМП / SasCore — только текущий пользователь
run_gmp_server_install() {
  local home prefix script
  home="$(getent passwd "${INVOKER}" | cut -d: -f6 || true)"
  [[ -n "$home" ]] || home="${HOME}"
  prefix="${home}/Saumi"
  [[ -d "${prefix}/drive_c" ]] || fail "Нет ${prefix} — сначала установите SAUMI"
  script="${prefix}/bin/install_gmp_appserver.sh"
  [[ -f "$script" ]] || fail "В архиве нет install_gmp_appserver.sh — нужен комплект prod-4 с ГИС ГМП"
  log "=== ГИС ГМП / SasCore (user=${INVOKER}) ==="
  ask_sudo_if_needed "открыть порты ГИС ГМП"
  echo ""
  echo "Нужны: пароль Oracle (SMSYS/MMPSRV) или эталон Settings.xml с Windows."
  echo "Также в prefix должен быть .NET 4 (v4.0.30319) — иначе старт остановится."
  echo ""
  sudo -u "${INVOKER}" env \
    HOME="$home" \
    SAUMI_PREFIX="$prefix" \
    DISPLAY="${DISPLAY:-:0}" \
    XAUTHORITY="${home}/.Xauthority" \
    SAUMI_GMP_TNS="${SAUMI_GMP_TNS:-}" \
    SAUMI_GMP_USER="${SAUMI_GMP_USER:-}" \
    SAUMI_GMP_PASSWORD="${SAUMI_GMP_PASSWORD:-}" \
    bash "$script" \
    || fail "Установка ГИС ГМП не удалась — см. ${prefix}/logs/gmp_install.log"
  log "ГИС ГМП: OK (логи ${prefix}/logs/gmp_*.log)"
}

# Только скрипты запуска. Prefix/mmp32/Excel не трогаем.
refresh_start_all_users() {
  local user home n=0
  ask_sudo_if_needed "обновить скрипты запуска SAUMI (без переустановки)"
  log "=== --refresh-start (скрипты запуска, mmp32 не трогаем) ==="
  while IFS=$'\t' read -r user home; do
    [[ -n "$user" && -n "$home" ]] || continue
    [[ -d "${home}/Saumi/drive_c/SAUMI" ]] || continue
    write_start_saumi "${home}/Saumi" "$user"
    write_wm_mode_helpers "${home}/Saumi" "$user"
    fix_prefix_owner "${home}/Saumi" "$user"
    n=$((n + 1))
    log "  OK ${user}: startSAUMI + boot_switch"
  done < <(list_human_users)
  log "Готово: refresh-start обновлено=${n}"
}

if [[ "${MODE}" == "refresh-start" ]]; then
  echo ""
  echo "============================================"
  echo "  SAUMI — обновить скрипты запуска"
  echo "  mmp32 / Excel / tar НЕ трогаем"
  echo "============================================"
  echo ""
  finalize_install_log
  log "installer=${INSTALLER_REV} режим=refresh-start"
  refresh_start_all_users
  echo ""
  echo "ГОТОВО (--refresh-start)."
  echo "  При запуске: сначала режим 1 (обновление), затем режим 2 (мультиокна)."
  echo "  Запуск: FORCE_KILL=1 ~/Saumi/startSAUMI.sh"
  echo "  Лог taskbar: ~/Saumi/logs/taskbar_fix.log"
  echo "  Лог: ${INSTALL_LOG_FILE}"
  echo ""
  exit 0
fi

if [[ "${MODE}" == "wm-mode" ]]; then
  echo ""
  echo "============================================"
  echo "  SAUMI — режим окон ${SAUMI_WM_MODE}"
  echo "============================================"
  echo ""
  finalize_install_log
  log "installer=${INSTALLER_REV} режим=wm-mode=${SAUMI_WM_MODE}"
  apply_wm_mode_all_users "${SAUMI_WM_MODE}"
  echo ""
  echo "ГОТОВО (--wm-mode=${SAUMI_WM_MODE})."
  echo "  Перезапуск у пользователя: FORCE_KILL=1 ~/Saumi/startSAUMI.sh"
  echo "  Лог: ${INSTALL_LOG_FILE}"
  echo ""
  exit 0
fi

if [[ "${MODE}" == "licence-server" ]]; then
  echo ""
  echo "============================================"
  echo "  SAUMI — служба лицензий Guardant"
  echo "  Только для текущего пользователя: ${INVOKER}"
  echo "============================================"
  echo ""
  finalize_install_log
  log "installer=${INSTALLER_REV} режим=licence-server"
  run_licence_server_install
  echo ""
  echo "ГОТОВО (--licence-server)."
  echo "  Старт/стоп: ярлыки на рабочем столе или ~/Saumi/bin/start|stop_licence_server.sh"
  echo "  Монитор: http://127.0.0.1:3185/"
  echo "  Лог: ~/Saumi/logs/licence_install.log"
  echo ""
  exit 0
fi

if [[ "${MODE}" == "gmp-server" ]]; then
  echo ""
  echo "============================================"
  echo "  SAUMI — ГИС ГМП (SasCoreService)"
  echo "  Только для текущего пользователя: ${INVOKER}"
  echo "============================================"
  echo ""
  finalize_install_log
  log "installer=${INSTALLER_REV} режим=gmp-server"
  run_gmp_server_install
  echo ""
  echo "ГОТОВО (--gmp-server)."
  echo "  Старт/стоп: ярлыки «SAUMI ГИС ГМП» или ~/Saumi/bin/start|stop_gmp_appserver.sh"
  echo "  Probe: ~/Saumi/bin/probe_gmp_appserver.sh"
  echo "  Лог: ~/Saumi/logs/gmp_install.log"
  echo ""
  exit 0
fi

install_named_user() {
  local force="${1:-0}" user="${2:-}" home
  [[ -n "$user" ]] || fail "Нужен --for-user=ИМЯ (пример: --for-user='ochernaja@Cityhall.krd')"
  home="$(getent passwd "$user" | cut -d: -f6 || true)"
  [[ -n "$home" ]] || fail "getent passwd не знает «${user}» — пользователь должен один раз войти (создать home) или имя неверно"
  [[ -d "$home" ]] || fail "Нет каталога home «${home}» у ${user} — пусть войдёт в графическую сессию один раз"
  ask_sudo_if_needed "установить SAUMI пользователю ${user}"
  verify_archive_mmp32 "${ARCHIVE}"
  log "Режим --for-user: user=${user} home=${home} force=${force}"
  log_users_discovery
  if install_saumi_for_user "$user" "$home" "$force"; then
    log_prefix_health "${home}/Saumi" "$user"
    log "OK --for-user ${user}"
  else
    fail "Не удалось установить для ${user} (см. лог)"
  fi
}

fix_owner_all_users() {
  local user home n=0
  ask_sudo_if_needed "исправить владельца ~/Saumi"
  log "=== --fix-owner ==="
  while IFS=$'\t' read -r user home; do
    [[ -n "$user" && -n "$home" ]] || continue
    [[ -d "${home}/Saumi" ]] || continue
    fix_prefix_owner "${home}/Saumi" "$user"
    n=$((n + 1))
  done < <(list_human_users)
  log "Готово: fix-owner обработано=${n}"
  [[ "$n" -gt 0 ]] || fail "Не найдено ни одного ~/Saumi"
}

if [[ "${MODE}" == "diag" ]]; then
  require_admin_users
  finalize_install_log
  log "installer=${INSTALLER_REV} режим=diag"
  log_users_discovery
  collect_install_diag
  echo "ГОТОВО (--diag)."
  exit 0
fi

if [[ "${MODE}" == "fix-owner" ]]; then
  require_admin_users
  finalize_install_log
  log "installer=${INSTALLER_REV} режим=fix-owner"
  fix_owner_all_users
  collect_install_diag
  echo ""
  echo "ГОТОВО (--fix-owner). Лог: ${INSTALL_LOG_FILE}"
  echo "Запуск только под своим пользователем (не root)."
  exit 0
fi

if [[ "${MODE}" == "for-user" ]]; then
  require_admin_users
  [[ -f "${ARCHIVE}" ]] || fail "Нет архива ${ARCHIVE}"
  finalize_install_log
  log "installer=${INSTALLER_REV} режим=for-user user=${FOR_USER} force=${FORCE_FLAG}"
  install_named_user "${FORCE_FLAG}" "${FOR_USER}"
  collect_install_diag
  echo ""
  echo "ГОТОВО (--for-user=${FOR_USER}). Лог: ${INSTALL_LOG_FILE}"
  echo "Войти как этот пользователь → ярлык SAUMI (не от root)."
  exit 0
fi

if [[ "${MODE}" == "users" ]]; then
  require_admin_users
  [[ -f "${ARCHIVE}" ]] || fail "Нет архива ${ARCHIVE}"
  log "Режим --users: архив → ~/Saumi только тем, у кого ещё нет"
  finalize_install_log
  install_all_users 0
  if [[ "${WITH_LICENCE}" -eq 1 ]]; then
    run_licence_server_install
  fi
  if [[ "${WITH_GMP}" -eq 1 ]]; then
    run_gmp_server_install
  fi
  collect_install_diag
  echo ""
  echo "ГОТОВО (--users). Лог: ${INSTALL_LOG_FILE}"
  exit 0
fi

if [[ "${MODE}" == "force-users" ]]; then
  require_admin_users
  [[ -f "${ARCHIVE}" ]] || fail "Нет архива ${ARCHIVE}"
  log "Режим --force-users: полная переустановка ~/Saumi (admin=${SAUMI_ADMIN_USER} из tar)"
  finalize_install_log
  install_all_users 1
  if [[ "${WITH_LICENCE}" -eq 1 ]]; then
    run_licence_server_install
  fi
  if [[ "${WITH_GMP}" -eq 1 ]]; then
    run_gmp_server_install
  fi
  collect_install_diag
  echo ""
  echo "ГОТОВО (--force-users). Лог: ${INSTALL_LOG_FILE}"
  exit 0
fi

require_admin

ensure_system_time() {
  local year now tz
  export TZ="${TZ:-Europe/Moscow}"
  now="$(date '+%F %T %Z' 2>/dev/null || date)"
  year="$(date +%Y 2>/dev/null || echo 0)"
  log "Время: ${now}"
  if command -v timedatectl >/dev/null 2>&1; then
    tz="$(timedatectl show -p Timezone --value 2>/dev/null || true)"
    log "Часовой пояс: ${tz:-unknown}"
    if [[ "${tz}" != "Europe/Moscow" ]]; then
      ask_sudo_if_needed "сменить часовой пояс на Europe/Moscow"
      run_as_root timedatectl set-timezone Europe/Moscow 2>>"${CURRENT_LOG}" || true
    fi
  fi
  if [[ "${year}" =~ ^[0-9]+$ ]] && [[ "${year}" -lt 2024 || "${year}" -gt 2035 ]]; then
    log "WARN: дата выглядит неверно — пробуем NTP"
    ask_sudo_if_needed "исправить системное время"
    run_as_root timedatectl set-ntp true 2>>"${CURRENT_LOG}" || true
    run_as_root hwclock --systohc 2>>"${CURRENT_LOG}" || true
  fi
}
ensure_system_time

have_so() {
  local name="$1"
  [[ -e "/lib32/i386-linux-gnu/${name}" ]] && return 0
  [[ -e "/usr/lib32/i386-linux-gnu/${name}" ]] && return 0
  [[ -e "/usr/lib/i386-linux-gnu/${name}" ]] && return 0
  [[ -e "/lib/i386-linux-gnu/${name}" ]] && return 0
  [[ -e "/lib32/${name}" ]] && return 0
  return 1
}

# Offline host .deb — только из SAUMI1860wine.tar.gz (runtime/saumi-host-debs).
# Без отдельных файлов рядом и без apt/сети.
prepare_local_host_debs() {
  local dest="$1" arch="${ARCHIVE:-}"
  local w n
  mkdir -p "$dest"
  shopt -s nullglob
  [[ -n "$arch" && -f "$arch" ]] || { log "  host-debs FAIL: нет архива arch=${arch:-empty}"; return 1; }
  log "  host-debs: ищем в ${arch} ($(du -h "$arch" 2>/dev/null | awk '{print $1}'))"
  w="$(mktemp -d /tmp/saumi_extract_debs_XXXXXX)"
  # Сначала пробуем сразу извлечь каталог (без полного tar -tz 2ГБ).
  if ! tar -xzf "$arch" -C "$w" SAUMIwine/runtime/saumi-host-debs >>"${CURRENT_LOG}" 2>&1; then
    log "  host-debs FAIL: tar не извлёк SAUMIwine/runtime/saumi-host-debs (см. лог)"
    rm -rf "$w"
    return 1
  fi
  n=0
  if [[ -d "$w/SAUMIwine/runtime/saumi-host-debs" ]]; then
    # find надёжнее glob при именах с %3a / +
    while IFS= read -r -d '' f; do
      cp -a "$f" "$dest/"
      n=$((n + 1))
    done < <(find "$w/SAUMIwine/runtime/saumi-host-debs" -maxdepth 1 -type f -name '*.deb' -print0 2>/dev/null)
  fi
  rm -rf "$w"
  if [[ "$n" -gt 0 ]]; then
    log "  host-debs: из архива ${n} шт."
    return 0
  fi
  log "  host-debs FAIL: каталог в архиве пуст или без .deb"
  return 1
}

install_local_host_debs() {
  local dir="$1"
  local -a ordered=() rest=() f base
  shopt -s nullglob
  # Порядок: base → libc → lib32 → ia32-libs → e2fsprogs/tools
  for f in "$dir"/gcc-12-base_*.deb; do [[ -f "$f" ]] && ordered+=("$f"); done
  for f in "$dir"/libc6-i386_*.deb; do [[ -f "$f" ]] && ordered+=("$f"); done
  for f in "$dir"/lib32gcc*.deb "$dir"/lib32stdc*.deb "$dir"/lib32tinfo*.deb "$dir"/lib32ncurses*.deb; do
    [[ -f "$f" ]] && ordered+=("$f")
  done
  for f in "$dir"/ia32-libs_*.deb; do [[ -f "$f" ]] && ordered+=("$f"); done
  for f in "$dir"/e2fsprogs_*.deb; do [[ -f "$f" ]] && ordered+=("$f"); done
  for f in "$dir"/*.deb; do
    base="$(basename "$f")"
    case "$base" in
      gcc-12-base_*|libc6-i386_*|lib32gcc*|lib32stdc*|lib32tinfo*|lib32ncurses*|ia32-libs_*|e2fsprogs_*) continue ;;
      *) rest+=("$f") ;;
    esac
  done
  ordered+=("${rest[@]}")
  [[ ${#ordered[@]} -gt 0 ]] || return 1
  log "Ставим offline .deb (${#ordered[@]} шт.) через dpkg -i (без apt/сети)..."
  if run_as_root dpkg -i "${ordered[@]}" >>"${CURRENT_LOG}" 2>&1; then
    log "OK: offline host-debs (dpkg -i)"
    return 0
  fi
  log "WARN: dpkg -i с ошибками — dpkg --configure -a и повтор"
  run_as_root dpkg --configure -a >>"${CURRENT_LOG}" 2>&1 || true
  if run_as_root dpkg -i "${ordered[@]}" >>"${CURRENT_LOG}" 2>&1; then
    log "OK: offline host-debs (повтор dpkg -i)"
    return 0
  fi
  # последний шанс: уже стоящие пакеты + --force-depends для битых зависимостей версии
  if run_as_root dpkg -i --force-depends "${ordered[@]}" >>"${CURRENT_LOG}" 2>&1; then
    log "OK: offline host-debs (dpkg -i --force-depends)"
    return 0
  fi
  return 1
}

host_libs_ready() {
  dpkg -s libc6-i386 >/dev/null 2>&1 \
    && dpkg -s lib32gcc-s1 >/dev/null 2>&1 \
    && dpkg -s lib32stdc++6 >/dev/null 2>&1 \
    && have_so libX11.so.6 \
    && have_so libXext.so.6 \
    && have_so libfreetype.so.6
}

ensure_packages() {
  local debdir
  command -v tar >/dev/null 2>&1 || fail "Нет tar (базовый инструмент ОС)"
  command -v dpkg >/dev/null 2>&1 || fail "Нет dpkg (базовый инструмент ОС)"

  if host_libs_ready \
     && { [[ -x /sbin/debugfs || -x /usr/sbin/debugfs ]] || command -v debugfs >/dev/null 2>&1; } \
     && command -v wmctrl >/dev/null 2>&1 \
     && command -v xdotool >/dev/null 2>&1; then
    log "Обязательные i386 libs + debugfs + wm tools уже есть — offline dpkg не нужен"
    return 0
  fi

  ask_sudo_if_needed "установить системные пакеты из локального бандла (без интернета)"

  if ! dpkg --print-foreign-architectures 2>/dev/null | grep -q i386; then
    log "Включаем архитектуру i386..."
    run_as_root dpkg --add-architecture i386 || true
  fi

  debdir="$(mktemp -d /tmp/saumi_host_debs_XXXXXX)"
  if ! prepare_local_host_debs "$debdir"; then
    rm -rf "$debdir"
    fail "В архиве нет runtime/saumi-host-debs/*.deb — нужен актуальный SAUMI1860wine.tar.gz. Интернет/apt не используем."
  fi
  if ! install_local_host_debs "$debdir"; then
    rm -rf "$debdir"
    fail "Не удалось поставить offline host-debs через dpkg -i. См. ${CURRENT_LOG}"
  fi
  rm -rf "$debdir"

  if ! host_libs_ready; then
    fail "После offline dpkg нет обязательных i386 libs (libc6-i386 / libX11). См. ${CURRENT_LOG}"
  fi
  if [[ ! -x /sbin/debugfs && ! -x /usr/sbin/debugfs ]] && ! command -v debugfs >/dev/null 2>&1; then
    log "WARN: debugfs не найден после offline install — birth mmp32 может не встать"
  fi
  command -v wmctrl >/dev/null 2>&1 || log "WARN: wmctrl не установлен (taskbar_fix ослаблен)"
  command -v xdotool >/dev/null 2>&1 || log "WARN: xdotool не установлен"
  log "OK: системные пакеты из локального бандла"
}
ensure_packages
command -v tar >/dev/null 2>&1 || fail "Нет tar"

ensure_ptrace_lock_off() {
  local bin="" st="" st2=""
  if [[ -x /usr/sbin/astra-ptrace-lock ]]; then
    bin=/usr/sbin/astra-ptrace-lock
  elif command -v astra-ptrace-lock >/dev/null 2>&1; then
    bin="$(command -v astra-ptrace-lock)"
  else
    log "astra-ptrace-lock: команда не найдена — пропуск"
    return 0
  fi

  ask_sudo_if_needed "проверить/отключить astra-ptrace-lock"
  st="$(run_as_root "$bin" status 2>&1 || true)"
  log "astra-ptrace-lock status (до): ${st:-empty}"

  log "astra-ptrace-lock: отключаем (нужно для Wine)..."
  if run_as_root "$bin" disable >>"${CURRENT_LOG}" 2>&1; then
    log "astra-ptrace-lock disable: команда OK"
  else
    log "WARN: astra-ptrace-lock disable вернул ошибку — см. лог"
  fi

  st2="$(run_as_root "$bin" status 2>&1 || true)"
  log "astra-ptrace-lock status (после): ${st2:-empty}"

  if echo "${st2}" | grep -qiE 'АКТИВН|ВКЛЮЧ|active|enabled|on[[:space:]]*$'; then
    if ! echo "${st2}" | grep -qiE 'НЕ[[:space:]]*АКТИВ|ВЫКЛ|inactive|disabled|off'; then
      log "WARN: ptrace-lock похоже всё ещё ВКЛЮЧЁН — Wine может зависать. Проверьте вручную: sudo astra-ptrace-lock status"
      return 1
    fi
  fi
  if echo "${st2}" | grep -qi 'нужны права\|permission\|denied'; then
    log "WARN: status после disable всё ещё без прав — проверьте вручную"
    return 1
  fi
  log "astra-ptrace-lock: OK (для установки считаем выключенным)"
  return 0
}
ensure_ptrace_lock_off || true

[[ -f "${ARCHIVE}" ]] || fail "Рядом со скриптом нет файла SAUMI1860wine.tar.gz"
log "Архив: $(du -h "${ARCHIVE}" | awk '{print $1}')  path=${ARCHIVE}"

if [[ -f "${SCRIPT_DIR}/SAUMI1860wine.sha256" ]]; then
  (cd "${SCRIPT_DIR}" && sha256sum -c SAUMI1860wine.sha256) >>"${CURRENT_LOG}" 2>&1 \
    || fail "Контрольная сумма архива не совпала"
  log "Проверка SHA256: OK"
fi

finalize_install_log
log "Старт установки, архив=${ARCHIVE}..."
install_all_users 1

if [[ "${WITH_LICENCE}" -eq 1 ]]; then
  run_licence_server_install
fi
if [[ "${WITH_GMP}" -eq 1 ]]; then
  run_gmp_server_install
fi

collect_install_diag

run_as_root mkdir -p /usr/local/sbin
run_as_root ln -sfn "${SCRIPT_DIR}/instalSaumi.sh" /usr/local/sbin/instalSaumi.sh 2>/dev/null ||   run_as_root cp -f "${SCRIPT_DIR}/instalSaumi.sh" /usr/local/sbin/instalSaumi.sh

echo ""
echo "============================================"
echo "  ГОТОВО"
echo "============================================"
echo ""
echo "  Комплект: instalSaumi.sh + SAUMI1860wine.tar.gz"
echo "  Архив: ${ARCHIVE}"
echo ""
echo "  Запуск: ярлык SAUMI  или  ~/Saumi/startSAUMI.sh"
echo "  Зависание: ярлык «Принудительное закрытие Сауми»"
echo "  Excel-экспорт: ярлык «Экспорт Saumi» → ~/Saumi/drive_c/SAUMI/export"
echo "  Окна: режим 2 (Managed=Y); обновить скрипты: sudo bash instalSaumi.sh --refresh-start"
echo "  Доустановка: sudo bash instalSaumi.sh --users"
echo "  Guardant: sudo bash instalSaumi.sh --licence-server"
echo "  Лог: ${INSTALL_LOG_FILE}"
echo "  Диаг: ${SCRIPT_DIR}/saumi_diag_*.tar.gz (собирается сам)"
echo "============================================"
echo ""
