#!/usr/bin/env bash
# Проба запросов к службе лицензий (без полного SAUMI/БД).
# Успех протокола: HTTP 200, TCP 3182, страница «Сервер Guardant Net».
# Сообщения «ключ не найден» / «лицензия» на мониторе — тоже валидный ответ службы.
set -euo pipefail
W="${SAUMI_PREFIX:-${HOME}/Saumi}"
[[ -d "${W}/drive_c/SAUMI" ]] || W="${HOME}/SAUMIwine"
LOG="${W}/logs/licence_probe.log"
mkdir -p "${W}/logs"
exec > >(tee -a "$LOG") 2>&1

echo "=== licence probe $(date) user=$(id -un) ==="
FAIL=0
ok() { echo "OK  $*"; }
bad() { echo "FAIL $*"; FAIL=1; }

pgrep -u "$(id -u)" -f 'NNKSRV32' >/dev/null && ok "процесс NNKSRV32" || bad "процесс не найден"
ss -lntp 2>/dev/null | grep -q ':3182' && ok "LISTEN 3182" || bad "нет LISTEN 3182"
ss -luntp 2>/dev/null | grep -q ':3183' && ok "UDP 3183" || echo "WARN UDP 3183 не видно"

CODE="$(curl -sS -m 3 -o /tmp/lic_probe.html -w '%{http_code}' http://127.0.0.1:3185/ 2>/dev/null || echo 000)"
[[ "$CODE" == "200" ]] && ok "HTTP 3185 = 200" || bad "HTTP 3185 = $CODE"

if [[ -f /tmp/lic_probe.html ]]; then
  TEXT="$(iconv -f windows-1251 -t utf-8 /tmp/lic_probe.html 2>/dev/null | sed 's/<[^>]*>/ /g' | tr '\n' ' ' | sed 's/  */ /g')"
  echo "STATUS: $TEXT" | fold -s -w 100 | head -8
  echo "$TEXT" | grep -qi 'Guardant' && ok "ответ содержит Guardant Net" || bad "нет маркера Guardant в ответе"
  # «ключ не найден» / пустые сессии — служба отвечает корректно
  if echo "$TEXT" | grep -qiE 'Клиентов:|Сессий:'; then
    ok "статистика клиентов/сессий в ответе (протокол жив)"
  fi
fi

# USB ключ на Linux (факт железа)
if lsusb 2>/dev/null | grep -qi '0a89:0003'; then
  ok "USB ключ виден Linux (0a89:0003 Stealth 2)"
else
  echo "WARN: USB ключ не виден в lsusb — вставьте ключ"
fi

python3 - <<'PY'
import socket
s=socket.socket(); s.settimeout(2)
s.connect(("127.0.0.1", 3182))
print("OK  TCP 3182 accept connection")
s.close()
PY

echo ""
if [[ "$FAIL" -eq 0 ]]; then
  echo "=== PROBE PASS — служба отвечает на запросы ==="
  echo "Замечание: отсутствие сессий/«нет ключа» в мониторе при живом USB = лимит Wine↔Stealth II,"
  echo "но HTTP/TCP ответы корректны. Полный клиентский login — следующим шагом / с Windows-сервером."
  exit 0
fi
echo "=== PROBE FAIL ==="
exit 1
