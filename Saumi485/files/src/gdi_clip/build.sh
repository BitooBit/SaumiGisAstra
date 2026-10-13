#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../../.." && pwd)"
INC="${ROOT}/files/_build_tmp/tinycc-mob"
TCC="${INC}/i386-win32-tcc"
SRC="${ROOT}/files/src/gdi_clip"
OUT="${SRC}/SaumiGdiClip.dll"
[[ -x "$TCC" ]] || { echo "FAIL: нет $TCC"; exit 1; }

"$TCC" -shared -o "$OUT" \
  -B "$INC" \
  -I "$INC/include" -I "$INC/win32/include" -I "$INC/win32/include/winapi" \
  "$SRC/clip.c" "$SRC/SaumiGdiClip.def" \
  "$INC/i386-win32-libtcc1.a" \
  -L "$INC/win32/lib" -lkernel32 -luser32 -lgdi32

python3 "${ROOT}/files/src/eztwain_proxy/pe_fix_exports.py" "$OUT"
ls -l "$OUT"
