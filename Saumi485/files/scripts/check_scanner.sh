#!/usr/bin/env bash
# Проверка сканера с хоста. URI/IP не зашиты — только scanimage -L и conf.
#
#   SAUMI_PREFIX=$HOME/Saumi bash check_scanner.sh
#
set -u
PREFIX="${SAUMI_PREFIX:-${HOME}/Saumi}"
CONF="${PREFIX}/config/scanner.conf"
LOG="${PREFIX}/logs"
OUT="${LOG}/scan_test.pnm"
mkdir -p "$LOG"

echo "======== 1. живые устройства (scanimage -L, до 45с) ========"
timeout 45 scanimage -L || echo "FAIL: scanimage -L"

echo
echo "======== 2. scanner.conf (это память, не факт что жив) ========"
if [[ -f "$CONF" ]]; then cat "$CONF"; else echo "нет $CONF"; fi

LIVE="$(timeout 45 scanimage -L 2>/dev/null | sed -n "s/^device \`\([^']*\)'.*/\1/p" || true)"
DEV=""
if [[ -f "$CONF" ]]; then
  CACHED="$(awk -F= '/^DEVICE=/{print substr($0,index($0,"=")+1); exit}' "$CONF")"
  while IFS= read -r x; do
    [[ "$x" == "$CACHED" ]] && DEV="$CACHED"
  done <<< "$LIVE"
fi
if [[ -z "${DEV:-}" ]]; then
  DEV="$(printf '%s\n' "$LIVE" | grep -m1 '^hpaio:' || true)"
fi
if [[ -z "${DEV:-}" ]]; then
  DEV="$(printf '%s\n' "$LIVE" | head -1)"
fi
echo "DEV(live)=${DEV:-NONE}"

echo
echo "======== 3. ping только если в URI есть ip= ========"
IP="$(printf '%s' "${DEV:-}" | sed -n 's/.*[?&]ip=\([^&]*\).*/\1/p')"
if [[ -n "${IP:-}" ]]; then
  echo "IP from URI: $IP"
  ping -c 2 -W 2 "$IP" || echo "FAIL: ping $IP — МФУ сейчас не на сети"
else
  echo "в URI нет ip= (usb/airscan) — ping не нужен"
fi

echo
echo "======== 4. пробный кадр Gray 150dpi ========"
if [[ -z "${DEV:-}" ]]; then
  echo "FAIL: scanimage -L пуст — сканера сейчас нет"
  exit 1
fi
rm -f "$OUT"
set +e
timeout 60 scanimage -d "$DEV" --mode=Gray --resolution=150 --format=pnm >"$OUT" 2>"${LOG}/scan_test.err"
rc=$?
set -e
if [[ "$rc" -eq 0 && -s "$OUT" ]]; then
  echo "OK pnm $(wc -c < "$OUT") bytes magic=$(head -c 2 "$OUT")"
else
  echo "FAIL scanimage rc=$rc"
  cat "${LOG}/scan_test.err" 2>/dev/null || true
  exit 1
fi
echo "======== OK хост-сканер жив ========"
