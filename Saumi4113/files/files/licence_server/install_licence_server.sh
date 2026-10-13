#!/usr/bin/env bash
# Установка службы лицензий Guardant Net для ТЕКУЩЕГО пользователя.
# Не трогает других пользователей. После установки — сразу старт.
#
# Использование:
#   SAUMI_PREFIX=~/Saumi bash install_licence_server.sh
#   Порты 3182–3185 открываются всегда (sudo по запросу).
#
set -euo pipefail
P="$(cd "$(dirname "$0")" && pwd)"
for a in "$@"; do
  case "$a" in
    --open-ports) ;; # устарело, порты открываются всегда
    --help|-h)
      echo "install_licence_server.sh"
      exit 0
      ;;
    *)
      echo "Неизвестный аргумент: $a" >&2
      exit 1
      ;;
  esac
done

W="${SAUMI_PREFIX:-${HOME}/Saumi}"
[[ -d "${W}/drive_c/SAUMI" ]] || W="${HOME}/SAUMIwine"
SAUMI_DIR="${W}/drive_c/SAUMI"
LOG_DIR="${W}/logs"
LOG="${LOG_DIR}/licence_install.log"
BIN="${W}/bin"

# файлы: рядом со скриптом ИЛИ уже в ~/Saumi (из tar)
SRC_DIR="$P"
[[ -f "${SRC_DIR}/NNKSRV32.EXE" ]] || SRC_DIR="$SAUMI_DIR"
[[ -f "${BIN}/start_licence_server.sh" ]] || BIN="$P"

mkdir -p "$LOG_DIR" "$BIN"
ts() { date '+%F %T'; }
log() { echo "[$(ts)] $*" | tee -a "$LOG"; }
fail() { log "FAIL: $*"; exit 1; }

log "=== install licence server (user=$(id -un) host=$(hostname)) ==="
log "PREFIX=$W SRC=$SRC_DIR"
[[ -d "$SAUMI_DIR" ]] || fail "нет $SAUMI_DIR — сначала поставьте SAUMI"

# --- USB-ключ Guardant ---
guardant_key_present() {
  command -v lsusb >/dev/null 2>&1 || return 1
  # Aktiv Guardant: vendor 0a89 (Stealth 2 = 0003 и др.)
  lsusb 2>/dev/null | grep -qiE 'ID 0a89:|Aktiv.*Guardant|Guardant'
}
if guardant_key_present; then
  KEY_LINE="$(lsusb 2>/dev/null | grep -iE 'ID 0a89:|Aktiv.*Guardant|Guardant' | head -1)"
  log "OK USB-ключ Guardant найден: ${KEY_LINE}"
else
  echo ""
  echo "============================================"
  echo "  КЛЮЧ GUARDANT НЕ НАЙДЕН"
  echo "============================================"
  echo "  Вставьте USB-ключ Guardant (лицензия SAUMI)"
  echo "  и повторите установку службы лицензий."
  echo "  После вставки ключа продолжите:"
  echo "    bash install_licence_server.sh"
  echo "============================================"
  echo ""
  log "FAIL: USB-ключ Guardant не найден (lsusb без 0a89/Aktiv Guardant)"
  if command -v lsusb >/dev/null; then
    log "  lsusb (кратко):"
    lsusb 2>/dev/null | while IFS= read -r _l; do log "    ${_l}"; done || true
  else
    log "  команда lsusb недоступна — установите usbutils"
  fi
  log "  Установите/вставьте ключ Guardant и продолжите установку."
  exit 1
fi

need=(NNKSRV32.EXE NNKSRV32.INI NOVEX32.DLL)
for f in "${need[@]}"; do
  [[ -f "$SRC_DIR/$f" ]] || [[ -f "$SAUMI_DIR/$f" ]] || fail "нет файла: $f"
done
# скрипты start/stop: из комплекта или уже в bin
for s in start_licence_server.sh stop_licence_server.sh; do
  if [[ -f "$P/$s" ]]; then
    :
  elif [[ -f "$BIN/$s" ]]; then
    :
  else
    fail "нет скрипта: $s"
  fi
done

# --- файлы в prefix ---
log "копируем бинарники → $SAUMI_DIR"
for f in NNKSRV32.EXE NNKSRV32.INI NOVEX32.DLL; do
  src="$SRC_DIR/$f"
  [[ -f "$src" ]] || src="$SAUMI_DIR/$f"
  if [[ "$src" -ef "$SAUMI_DIR/$f" ]]; then
    log "  OK $f (уже на месте)"
  else
    cp -f "$src" "$SAUMI_DIR/"
    log "  OK $f ($(wc -c <"$SAUMI_DIR/$f" | tr -d ' ') bytes)"
  fi
done

# gnclient: локальный сервер
GNC_SRC=""
[[ -f "$P/GNCLIENT.INI" ]] && GNC_SRC="$P/GNCLIENT.INI"
[[ -z "$GNC_SRC" && -f "$SAUMI_DIR/gnclient.ini.licence_local" ]] && GNC_SRC="$SAUMI_DIR/gnclient.ini.licence_local"
[[ -z "$GNC_SRC" && -f "$SRC_DIR/GNCLIENT.INI" ]] && GNC_SRC="$SRC_DIR/GNCLIENT.INI"
[[ -n "$GNC_SRC" ]] || fail "нет GNCLIENT.INI / gnclient.ini.licence_local"

if [[ -f "${SAUMI_DIR}/gnclient.ini" ]] && ! [[ "$GNC_SRC" -ef "${SAUMI_DIR}/gnclient.ini" ]]; then
  cp -f "${SAUMI_DIR}/gnclient.ini" "${LOG_DIR}/gnclient.ini.bak_before_licence_$(date +%Y%m%d_%H%M%S)"
  log "бэкап gnclient.ini → logs/"
fi
if ! [[ "$GNC_SRC" -ef "${SAUMI_DIR}/gnclient.ini" ]]; then
  cp -f "$GNC_SRC" "${SAUMI_DIR}/gnclient.ini"
fi
if ! [[ "$GNC_SRC" -ef "${SAUMI_DIR}/GNCLIENT.INI" ]]; then
  cp -f "$GNC_SRC" "${SAUMI_DIR}/GNCLIENT.INI"
fi
log "gnclient.ini ← $(basename "$GNC_SRC") IP=$(grep -E '^IP_NAME=' "${SAUMI_DIR}/gnclient.ini" | head -1 | cut -d= -f2-)"
log "gnclient.ini NB=$(grep -E '^NB_NAME=' "${SAUMI_DIR}/gnclient.ini" | head -1 | cut -d= -f2-)"

# скрипты start/stop в bin
for s in start_licence_server.sh stop_licence_server.sh probe_licence_server.sh install_licence_server.sh; do
  if [[ -f "$P/$s" ]] && ! [[ "$P/$s" -ef "$BIN/$s" ]]; then
    cp -f "$P/$s" "$BIN/"
    chmod 755 "$BIN/$s"
  elif [[ -f "$BIN/$s" ]]; then
    chmod 755 "$BIN/$s"
  fi
done
log "скрипты → $BIN/"

# --- порты (sudo по запросу) ---
open_ports() {
  log "открытие портов TCP/UDP 3182-3185 (нужен sudo)..."
  if [[ "$(id -u)" -eq 0 ]]; then
    SUDO=()
  else
    command -v sudo >/dev/null || fail "нужен sudo для --open-ports"
    echo "Нужны права администратора для открытия портов лицензии (3182-3185)."
    sudo -v || fail "sudo отклонён"
    SUDO=(sudo)
  fi
  # iptables (если есть)
  if command -v iptables >/dev/null; then
    for p in 3182 3185; do
      "${SUDO[@]}" iptables -C INPUT -p tcp --dport "$p" -j ACCEPT 2>/dev/null \
        || "${SUDO[@]}" iptables -I INPUT -p tcp --dport "$p" -j ACCEPT
      log "  iptables TCP $p ACCEPT"
    done
    for p in 3183 3184; do
      "${SUDO[@]}" iptables -C INPUT -p udp --dport "$p" -j ACCEPT 2>/dev/null \
        || "${SUDO[@]}" iptables -I INPUT -p udp --dport "$p" -j ACCEPT
      log "  iptables UDP $p ACCEPT"
    done
  fi
  # firewalld
  if command -v firewall-cmd >/dev/null && "${SUDO[@]}" firewall-cmd --state 2>/dev/null | grep -qi running; then
    for p in 3182/tcp 3185/tcp 3183/udp 3184/udp; do
      "${SUDO[@]}" firewall-cmd --permanent --add-port="$p" 2>>"$LOG" || true
      log "  firewalld $p"
    done
    "${SUDO[@]}" firewall-cmd --reload 2>>"$LOG" || true
  fi
  # ufw
  if command -v ufw >/dev/null && "${SUDO[@]}" ufw status 2>/dev/null | grep -qi active; then
    "${SUDO[@]}" ufw allow 3182/tcp 2>>"$LOG" || true
    "${SUDO[@]}" ufw allow 3185/tcp 2>>"$LOG" || true
    "${SUDO[@]}" ufw allow 3183:3184/udp 2>>"$LOG" || true
    log "  ufw rules added"
  fi
  log "порты: готово"
}
open_ports

# --- ярлыки на рабочий стол текущего пользователя ---
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
Comment=Guardant Net (SAUMI licence)
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
  write_desktop "${DESK}/SAUMI-licence-start.desktop" \
    "SAUMI Лицензия — Старт" "${BIN}/start_licence_server.sh"
  write_desktop "${DESK}/SAUMI-licence-stop.desktop" \
    "SAUMI Лицензия — Стоп" "${BIN}/stop_licence_server.sh"
else
  log "WARN: рабочий стол не найден — ярлыки только в $BIN"
fi

# --- старт сразу ---
log "запуск службы..."
SAUMI_PREFIX="$W" bash "$BIN/start_licence_server.sh" || fail "старт не удался"

# --- самопроверка ---
log "=== verify ==="
OK=1
pgrep -u "$(id -u)" -f 'NNKSRV32' >/dev/null && log "OK процесс" || { log "FAIL процесс"; OK=0; }
ss -lntp 2>/dev/null | grep -q ':3182' && log "OK TCP 3182" || { log "FAIL TCP 3182"; OK=0; }
CODE="$(curl -sS -m 3 -o /tmp/lic_v.html -w '%{http_code}' http://127.0.0.1:3185/ 2>/dev/null || echo 000)"
[[ "$CODE" == "200" ]] && log "OK HTTP 3185" || { log "FAIL HTTP 3185 code=$CODE"; OK=0; }
if [[ -f /tmp/lic_v.html ]]; then
  iconv -f windows-1251 -t utf-8 /tmp/lic_v.html 2>/dev/null | tr '\n' ' ' \
    | sed 's/<[^>]*>/ /g;s/  */ /g' | head -c 300 >>"$LOG" || true
  echo >>"$LOG"
fi

# probe: TCP connect = сервер отвечает (клиентский протокол — отдельно)
python3 - <<'PY' >>"$LOG" 2>&1 || true
import socket
for port, name in [(3182,"Guardant TCP"), (3185,"HTTP monitor")]:
    s=socket.socket(); s.settimeout(2)
    try:
        s.connect(("127.0.0.1", port))
        print(f"OK probe {name} :{port} connect")
    except Exception as e:
        print(f"FAIL probe {name} :{port} {e}")
    finally:
        s.close()
PY

log "логи службы/установки: $LOG и ${LOG_DIR}/licence_server.log"
if [[ "$OK" -eq 1 ]]; then
  log "=== INSTALL OK (user=$(id -un)) ==="
  echo ""
  echo "Служба лицензий запущена для $(id -un)."
  echo "Монитор: http://127.0.0.1:3185/"
  echo "Лог: $LOG"
  exit 0
fi
log "=== INSTALL WARN — см. лог ==="
exit 1
