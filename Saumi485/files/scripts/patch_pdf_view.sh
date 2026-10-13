#!/usr/bin/env bash
# «Показать содержимое»: не кеш/вкладки Sumatra (прошлый документ + листы копятся).
#   cd /var/install/saumi485/pathes/18
#   SAUMI_PREFIX=$HOME/Saumi bash apply.sh
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PREFIX="${SAUMI_PREFIX:-${HOME}/Saumi}"
[[ -f "${PREFIX}/startSAUMI.sh" ]] || { echo "FAIL: нет prefix"; exit 1; }

DEST="${PREFIX}/drive_c/SAUMI/components/SumatraPDF"
[[ -f "${DEST}/SumatraPDF.exe" ]] || { echo "FAIL: нет ${DEST}/SumatraPDF.exe — сначала pathes/6"; exit 1; }

OPEN=""
for f in "${SCRIPT_DIR}/pdfopen.exe" \
         "${SCRIPT_DIR}/../src/document_handlers/pdfopen.exe"; do
  [[ -f "$f" ]] && OPEN="$f" && break
done
[[ -n "$OPEN" ]] || { echo "FAIL: нет pdfopen.exe"; exit 1; }

SETF=""
for f in "${SCRIPT_DIR}/SumatraPDF-settings.txt" \
         "${SCRIPT_DIR}/../src/document_handlers/SumatraPDF-settings.txt"; do
  [[ -f "$f" ]] && SETF="$f" && break
done

WINESERVER="${PREFIX}/runtime/wine-11.0/bin/wineserver"
if [[ -x "$WINESERVER" ]]; then
  env WINEPREFIX="$PREFIX" WINEARCH=win32 "$WINESERVER" -k >/dev/null 2>&1 || true
  sleep 1
fi

cp -f "$OPEN" "${DEST}/pdfopen.exe"
chmod 755 "${DEST}/pdfopen.exe"
echo "OK: pdfopen.exe → ${DEST}/pdfopen.exe ($(wc -c < "$OPEN") bytes)"

if [[ -n "$SETF" ]]; then
  cp -f "$SETF" "${DEST}/SumatraPDF-settings.txt"
  WUSER="$(id -un)"
  for d in \
    "${PREFIX}/drive_c/users/${WUSER}/AppData/Roaming/SumatraPDF" \
    "${PREFIX}/drive_c/users/${WUSER}/Application Data/SumatraPDF"; do
    mkdir -p "$d"
    cp -f "$SETF" "${d}/SumatraPDF-settings.txt"
  done
  echo "OK: SumatraPDF-settings.txt (без вкладок, без сессии)"
fi

python3 - "$PREFIX/system.reg" <<'PY'
from pathlib import Path
import sys
p = Path(sys.argv[1])
t = p.read_text(encoding="utf-8", errors="replace")
old = 'SumatraPDF.exe\\" \\"%1\\"'
new = 'pdfopen.exe\\" \\"%1\\"'
n = t.count(old)
if n == 0:
    old2 = 'SumatraPDF.exe" "%1"'
    new2 = 'pdfopen.exe" "%1"'
    n = t.count(old2)
    if n:
        t = t.replace(old2, new2)
        p.write_text(t, encoding="utf-8")
        print(f"OK: system.reg open → pdfopen.exe ({n})")
    else:
        print("WARN: в system.reg нет команды open Sumatra — ассоциация уже другая")
else:
    t = t.replace(old, new)
    p.write_text(t, encoding="utf-8")
    print(f"OK: system.reg open → pdfopen.exe ({n})")
PY

echo "OK: pathes/18 pdf view"
echo "  Показать содержимое → только текущий файл, без прошлого документа."
