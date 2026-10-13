#!/usr/bin/env bash
# Хост-сканер для SAUMI: список SANE, запомнить default, снять в TIFF.
# 64-bit scanimage (на АРМ airscan/hpaio уже видит устройства).
# Вызов: saumi_scan.sh list|pick|scan [outfile]
set -euo pipefail
PREFIX="${SAUMI_PREFIX:-${HOME}/Saumi}"
CFG="${PREFIX}/config/scanner.conf"
mkdir -p "${PREFIX}/config"

list_devices() {
  timeout 90 scanimage -L 2>/dev/null | sed -n "s/^device \`\([^']*\)'.*/\1/p"
}

pick_default() {
  local devs d
  mapfile -t devs < <(list_devices)
  if [[ ${#devs[@]} -eq 0 ]]; then
    echo "NO_SCANNER"
    return 1
  fi
  # уже запомненный и всё ещё в списке
  if [[ -f "$CFG" ]]; then
    d="$(grep -E '^DEVICE=' "$CFG" | head -1 | cut -d= -f2-)"
    for x in "${devs[@]}"; do
      [[ "$x" == "$d" ]] && { echo "$d"; return 0; }
    done
  fi
  # локальный/HP hpaio предпочтительнее толпы airscan
  for x in "${devs[@]}"; do
    [[ "$x" == hpaio:* ]] && { echo "$x"; return 0; }
  done
  echo "${devs[0]}"
}

cmd="${1:-pick}"
case "$cmd" in
  list)
    list_devices
    ;;
  pick)
    d="$(pick_default)" || exit 1
    printf 'DEVICE=%s\nUPDATED=%s\n' "$d" "$(date -Iseconds)" >"$CFG"
    echo "$d"
    ;;
  scan)
    out="${2:-${PREFIX}/drive_c/SAUMI/filetypes/last_scan.tiff}"
    d="$(pick_default)" || exit 1
    printf 'DEVICE=%s\nUPDATED=%s\n' "$d" "$(date -Iseconds)" >"$CFG"
    mkdir -p "$(dirname "$out")"
    scanimage -d "$d" --format=tiff >"$out"
    echo "$out"
    ;;
  *)
    echo "Usage: $0 list|pick|scan [outfile]" >&2
    exit 1
    ;;
esac
