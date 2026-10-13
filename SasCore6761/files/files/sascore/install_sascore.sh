#!/usr/bin/env bash
# Установка SasCoreService (SAUMI Application Server / ГИС ГМП).
# Отдельный Wine-prefix ~/SasCoreWine — не смешивается с клиентом SAUMI.
#
#   bash install_sascore.sh
#   Порты firewall открывает внешний installSasCore.sh (уже root).
#
set -euo pipefail
P="$(cd "$(dirname "$0")" && pwd)"
SKIP_START=0
for a in "$@"; do
  case "$a" in
    --open-ports) ;; # устарело, порты открываются всегда
    --skip-start) SKIP_START=1 ;;
    --install-net4|--skip-net4) ;; # устарело: .NET 4 только из tar
    --help|-h)
      echo "install_sascore.sh [--skip-start]"
      exit 0
      ;;
    *)
      echo "Неизвестный аргумент: $a" >&2
      exit 1
      ;;
  esac
done

W="${SASCORE_PREFIX:-${HOME}/SasCoreWine}"
APP="${W}/drive_c/SaumiAppServer"
CORE="${APP}/Core"
LOG_DIR="${W}/logs"
LOG="${LOG_DIR}/sascore_install.log"
BIN="${W}/bin"

SRC=""
for cand in \
  "${P}/payload" \
  "${P}/../payload" \
  "${W}/drive_c/SaumiAppServer" \
  "${APP}"
do
  [[ -f "${cand}/Core/SasCoreWindowsService.exe" ]] && SRC="$cand" && break
done

mkdir -p "$LOG_DIR" "$BIN"
ts() { date '+%F %T'; }
log() { echo "[$(ts)] $*" | tee -a "$LOG"; }
fail() { log "FAIL: $*"; exit 1; }

log "=== install SasCoreService (user=$(id -un) host=$(hostname)) ==="
log "PREFIX=$W SRC=${SRC:-none}"
[[ -d "${W}/drive_c" ]] || fail "нет Wine-prefix $W — сначала запустите installSasCore.sh"
[[ -n "$SRC" ]] || fail "нет payload (Core/SasCoreWindowsService.exe)"

log "копируем SaumiAppServer → $APP"
mkdir -p "$APP"
if [[ "$SRC" != "$APP" ]]; then
  (cd "$SRC" && tar cf - .) | (cd "$APP" && tar xf -)
fi
[[ -f "${CORE}/SasCoreWindowsService.exe" ]] || fail "нет ${CORE}/SasCoreWindowsService.exe"
log "OK Core/SasCoreWindowsService.exe"

if [[ "$P" != "$BIN" ]]; then
  for s in install_sascore.sh start_sascore.sh stop_sascore.sh probe_sascore.sh oracle_settings.sh oracle_settings.cs; do
    [[ -f "$P/$s" ]] && cp -f "$P/$s" "$BIN/" && chmod 755 "$BIN/$s" 2>/dev/null || true
  done
else
  chmod 755 "$BIN"/*.sh 2>/dev/null || true
fi
log "скрипты → $BIN/"

# shellcheck source=/dev/null
source "${BIN}/oracle_settings.sh"

# --- Settings.xml / CoreConfig.xml ---
SETTINGS="${CORE}/Settings.xml"
CORECFG="${CORE}/CoreConfig.xml"

trim() {
  local s="${1:-}"
  s="${s#"${s%%[![:space:]]*}"}"
  s="${s%"${s##*[![:space:]]}"}"
  printf '%s' "$s"
}

write_settings() {
  local tns="$1" user="$2" pass="$3"
  compile_oracle_settings "${BIN}/oracle_settings.cs" || compile_oracle_settings "${P}/oracle_settings.cs" \
    || fail "не собрался oracle_settings.exe (csc)"
  write_oracle_settings "$tns" "$user" "$pass" || fail "не записали Settings.xml (DPAPI)"
  log "сохранён Settings.xml DataSource=${tns} UserId=${user} (DPAPI MachineKey)"
}

# Обязательный параметр: env или интерактивный ввод. Пустое значение — не продолжаем.
ask_required() {
  local env_var="$1" prompt="$2"
  local val="${!env_var:-}"
  val="$(trim "$val")"
  if [[ -n "$val" ]]; then
    printf '%s' "$val"
    return 0
  fi
  if [[ ! -t 0 ]]; then
    fail "не указан ${env_var} (нужен интерактивный ввод или переменная окружения)"
  fi
  while true; do
    read -r -p "${prompt}: " val || true
    val="$(trim "$val")"
    if [[ -n "$val" ]]; then
      printf '%s' "$val"
      return 0
    fi
    echo "Обязательное поле. Без него установка не продолжается." >&2
  done
}

ask_password_required() {
  local val="${SASCORE_PASSWORD:-}"
  if [[ -n "$val" ]]; then
    printf '%s' "$val"
    return 0
  fi
  if [[ ! -t 0 ]]; then
    fail "не указан пароль (SASCORE_PASSWORD или интерактивный ввод)"
  fi
  while true; do
    read -r -s -p "Пароль пользователя: " val || true
    echo
    if [[ -n "$val" ]]; then
      printf '%s' "$val"
      return 0
    fi
    echo "Пароль обязателен. Без него установка не продолжается." >&2
  done
}

# Settings.xml с Windows (другой master key) под Wine не расшифровывается.
# Рабочий файл — DPAPI, зашифрованный уже в этом prefix.
settings_decrypts_here() {
  compile_oracle_settings "${BIN}/oracle_settings.cs" >/dev/null 2>&1 \
    || compile_oracle_settings "${P}/oracle_settings.cs" >/dev/null 2>&1 || return 1
  check_oracle_settings "$1" >/dev/null 2>&1
}

if settings_decrypts_here "$SETTINGS"; then
  log "Settings.xml уже есть — не перезаписываем"
elif [[ -f "$SETTINGS" ]]; then
  cp -f "$SETTINGS" "${LOG_DIR}/Settings.xml.bak_$(date +%Y%m%d_%H%M%S)" 2>/dev/null || true
  if grep -q '8QzkIvdEXU2aUVXLX1JNIg' "$SETTINGS" 2>/dev/null; then
    log "старый Settings.xml (DPAPI с Windows) — заменим на новые параметры"
  else
    log "старый Settings.xml неполный — запросим параметры"
  fi
  echo ""
  echo "Параметры подключения к Oracle (обязательно):"
  echo "  — имя сервиса (TNS / Data Source)"
  echo "  — имя пользователя"
  echo "  — пароль"
  echo ""
  log "запрос параметров Oracle"
  TNS="$(ask_required SASCORE_TNS 'Имя сервиса (TNS / Data Source)')"
  USER="$(ask_required SASCORE_USER 'Имя пользователя')"
  PASS="$(ask_password_required)"
  [[ -n "$TNS" && -n "$USER" && -n "$PASS" ]] || fail "пустые параметры подключения — установка прервана"
  write_settings "$TNS" "$USER" "$PASS"
else
  echo ""
  echo "Параметры подключения к Oracle (обязательно):"
  echo "  — имя сервиса (TNS / Data Source)"
  echo "  — имя пользователя"
  echo "  — пароль"
  echo ""
  log "запрос параметров Oracle"
  TNS="$(ask_required SASCORE_TNS 'Имя сервиса (TNS / Data Source)')"
  USER="$(ask_required SASCORE_USER 'Имя пользователя')"
  PASS="$(ask_password_required)"
  [[ -n "$TNS" && -n "$USER" && -n "$PASS" ]] || fail "пустые параметры подключения — установка прервана"
  write_settings "$TNS" "$USER" "$PASS"
fi

[[ -f "$CORECFG" ]] && log "OK CoreConfig.xml" || log "WARN: нет CoreConfig.xml (можно скопировать с Windows)"

# --- .NET 4 (должен быть в tar после bake_sascore_tar.sh) ---
NET4="${W}/drive_c/windows/Microsoft.NET/Framework/v4.0.30319"
net4_ok() { [[ -f "${NET4}/mscorlib.dll" && -f "${NET4}/clr.dll" ]]; }

if net4_ok; then
  log "OK .NET Framework 4: $NET4 (mscorlib.dll)"
else
  fail "нет .NET 4 в prefix — пересоберите SasCoreWine.tar.gz (bash bake_sascore_tar.sh на Mac)"
fi

if [[ -d "${W}/drive_c/oracle" ]]; then
  log "OK Oracle client: drive_c/oracle"
else
  log "WARN: нет drive_c/oracle — служба может не подключиться к БД"
fi

# --- порты: открывает installSasCore.sh от root, здесь не спрашиваем ---
log "порты TCP: ${SASCORE_PORTS_TCP:-8080 8732 8733 9000 80 443} (firewall — внешний установщик)"

# --- ярлыки ---
DESK=""
for d in "${HOME}/Рабочий стол" "${HOME}/Desktop" "${HOME}/Desktops/Desktop1"; do
  [[ -d "$d" ]] && DESK="$d" && break
done
write_desktop() {
  local path="$1" name="$2" exec_line="$3"
  cat >"$path" <<EOF
[Desktop Entry]
Version=1.0
Type=Application
Name=${name}
Comment=SasCoreService — сервер приложений SAUMI (ГИС ГМП)
Exec=${exec_line}
Path=${W}
Terminal=false
Categories=Utility;
EOF
  chmod 755 "$path"
  gio set "$path" metadata::trusted true 2>/dev/null || true
  log "ярлык → $path"
}
if [[ -n "$DESK" ]]; then
  write_desktop "${DESK}/SasCore-start.desktop" \
    "SasCoreService — Старт" "${BIN}/start_sascore.sh"
  write_desktop "${DESK}/SasCore-stop.desktop" \
    "SasCoreService — Стоп" "${BIN}/stop_sascore.sh"
  write_desktop "${DESK}/SasCore-probe.desktop" \
    "SasCoreService — Проверка" "${BIN}/probe_sascore.sh"
fi

if [[ "$SKIP_START" -eq 1 ]]; then
  log "=== INSTALL OK (без старта) ==="
  exit 0
fi

log "запуск службы..."
SASCORE_PREFIX="$W" bash "$BIN/start_sascore.sh" || fail "старт не удался — см. ${LOG_DIR}/sascore.log"

log "probe..."
probe_rc=0
SASCORE_PREFIX="$W" bash "$BIN/probe_sascore.sh" || probe_rc=$?
if [[ "$probe_rc" -eq 0 ]]; then
  log "=== INSTALL OK + PROBE PASS ==="
  echo ""
  echo "SasCoreService запущена для $(id -un)."
  echo "Лог установки: $LOG"
  echo "Лог службы: ${LOG_DIR}/sascore.log"
  exit 0
fi
if [[ "$probe_rc" -eq 2 ]]; then
  log "=== INSTALL OK + PROBE PARTIAL (процесс есть, LISTEN нет — часто без живой БД) ==="
  echo ""
  echo "SasCoreService процесс есть. LISTEN может появиться после подключения к Oracle."
  echo "Лог установки: $LOG"
  echo "Лог службы: ${LOG_DIR}/sascore.log"
  exit 0
fi
log "=== INSTALL WARN — см. probe и sascore.log ==="
exit 1
