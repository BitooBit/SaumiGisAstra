#!/usr/bin/env bash
# Хост-демон: Wine TWAIN DS кладёт request, 64-bit scanimage снимает страницу.
# HP hpaio без --mode даёт P4 (штрих) — конвертер это умеет; сначала пробуем Gray.
set -euo pipefail
PREFIX="${SAUMI_PREFIX:-${HOME}/Saumi}"
TWDIR="${PREFIX}/drive_c/SAUMI/temp/twain"
CFG="${PREFIX}/config/scanner.conf"
LOG="${PREFIX}/logs/scan_daemon.log"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PNM2DIB=""
for f in "${SCRIPT_DIR}/pnm_to_dib.py" \
         "${PREFIX}/bin/pnm_to_dib.py"; do
  [[ -f "$f" ]] && PNM2DIB="$f" && break
done
[[ -n "$PNM2DIB" ]] || { echo "FAIL: нет pnm_to_dib.py" >&2; exit 1; }

mkdir -p "$TWDIR" "${PREFIX}/logs" "${PREFIX}/config"
exec >>"$LOG" 2>&1
echo "=== scan daemon $(date -Iseconds) pid=$$ prefix=$PREFIX ==="

pick_dev() {
  local d="" cached="" lines
  lines="$(timeout 60 scanimage -L 2>/dev/null | sed -n "s/^device \`\([^']*\)'.*/\1/p" || true)"
  if [[ -f "$CFG" ]]; then
    cached="$(awk -F= '/^DEVICE=/{print substr($0,index($0,"=")+1); exit}' "$CFG")"
  fi
  if [[ -n "$cached" ]]; then
    while IFS= read -r d; do
      [[ "$d" == "$cached" ]] && { echo "$d"; return 0; }
    done <<< "$lines"
  fi
  d="$(printf '%s\n' "$lines" | grep -m1 '^hpaio:' || true)"
  if [[ -z "$d" ]]; then
    d="$(printf '%s\n' "$lines" | head -1)"
  fi
  [[ -n "$d" ]] || return 1
  echo "$d"
}

write_source_name() {
  local uri="$1" label
  label="${uri#*/net/}"
  label="${label#*/usb/}"
  label="${label%%\?*}"
  label="${label%%:*}"
  label="${label//_/ }"
  [[ -n "$label" ]] || label="Scanner"
  printf '%s\n' "$label" >"${TWDIR}/source_name.txt"
}

scan_to_dib() {
  local out="$1"
  local extra rc attempt
  rm -f "$out"
  for attempt in 1 2 3 4 5; do
    echo "attempt $attempt DEV=$DEV"
    for extra in "--mode=Gray" "--mode=Color" ""; do
      echo "try scanimage $extra"
      set +e
      # shellcheck disable=SC2086
      timeout 60 scanimage -d "$DEV" --format=pnm --resolution=150 $extra \
        2>"${TWDIR}/scanimage.err" | python3 "$PNM2DIB" "$out"
      rc=$?
      set -e
      if [[ "$rc" -eq 0 && -s "$out" ]]; then
        echo "ok mode='${extra:-default}' size=$(wc -c < "$out")"
        return 0
      fi
      echo "fail rc=$rc extra='$extra'"
      cat "${TWDIR}/scanimage.err" 2>/dev/null || true
      if grep -q "device I/O" "${TWDIR}/scanimage.err" 2>/dev/null; then
        echo "device I/O — не крутим все режимы, пауза"
        break
      fi
    done
    echo "retry sleep 3s (HP I/O / busy)"
    sleep 3
    DEV="$(pick_dev || echo "$DEV")"
  done
  return 1
}

DEV="$(pick_dev || true)"
echo "DEVICE=${DEV:-NONE}"
if [[ -n "${DEV:-}" ]]; then
  printf 'DEVICE=%s\nUPDATED=%s\n' "$DEV" "$(date -Iseconds)" >"$CFG"
  write_source_name "$DEV"
fi

while true; do
  if [[ -f "${TWDIR}/request" ]]; then
    echo "$(date -Iseconds) request"
    rm -f "${TWDIR}/page.dib.done" "${TWDIR}/page.dib"
    DEV="$(pick_dev || echo "${DEV:-}")"
    if [[ -n "${DEV:-}" ]]; then
      write_source_name "$DEV"
    fi
    if [[ -z "${DEV:-}" ]]; then
      echo "NO DEVICE"
    else
      scan_to_dib "${TWDIR}/page.dib" || echo "SCAN FAIL"
    fi
    rm -f "${TWDIR}/request"
    : >"${TWDIR}/page.dib.done"
    if [[ -f "${TWDIR}/ds.log" ]]; then
      cp -f "${TWDIR}/ds.log" "${PREFIX}/logs/twain_ds.log" || true
    fi
  fi
  if [[ -f "${TWDIR}/proxy.log" ]]; then
    cp -f "${TWDIR}/proxy.log" "${PREFIX}/logs/eztwain_proxy.log" || true
  fi
  sleep 0.2
done
