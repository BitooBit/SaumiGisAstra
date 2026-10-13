#!/usr/bin/env bash
# Проверка сканера с хоста (без SAUMI). Пишет PNG в ~/Saumi/logs/scan_test.png
#
#   SAUMI_PREFIX=$HOME/Saumi bash test_scanner.sh
#
set -euo pipefail
PREFIX="${SAUMI_PREFIX:-${HOME}/Saumi}"
CONF="${PREFIX}/config/scanner.conf"
OUT="${PREFIX}/logs/scan_test.png"
DEV=""
if [[ -f "$CONF" ]]; then
  DEV="$(awk -F= '/^DEVICE=/{print substr($0,index($0,"=")+1); exit}' "$CONF")"
fi
if [[ -z "$DEV" ]]; then
  echo "Нет $CONF — сначала patch_scanner.sh. Пробую scanimage -L…"
  scanimage -L || true
  exit 1
fi
echo "DEVICE=$DEV"
echo "Сканирую одну страницу (до 60с) → $OUT"
mkdir -p "${PREFIX}/logs"
set +e
timeout 60 scanimage -d "$DEV" --format=png >"$OUT"
rc=$?
set -e
if [[ "$rc" -eq 0 && -s "$OUT" ]]; then
  ls -la "$OUT"
  echo "OK: хост видит сканер. Дальше в SAUMI:"
  echo "  Новый документ → квадрат у номера → «Сканировать…»"
  echo "  Источник: SANE / Hewlett-Packard HP_LaserJet_400_MFP_M425dw"
  echo "Если в SAUMI список TWAIN пустой — 32-bit Wine не видит hpaio; пиши, делаем прокси EZTwain."
else
  echo "FAIL: scanimage rc=$rc (нет бумаги / занят / сеть). Устройство в conf:"
  cat "$CONF"
  exit 1
fi
