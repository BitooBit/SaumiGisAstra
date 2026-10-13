#!/usr/bin/env bash
# Полоса выделения в «Свойства → Движения»: грузить clip через Eztwain (не AppInit).
#   cd /var/install/saumi485/pathes/21
#   SAUMI_PREFIX=$HOME/Saumi bash apply.sh
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PREFIX="${SAUMI_PREFIX:-${HOME}/Saumi}"
[[ -f "${PREFIX}/startSAUMI.sh" ]] || { echo "FAIL: нет prefix"; exit 1; }

DLL=""
for f in "${SCRIPT_DIR}/SaumiGdiClip.dll" \
         "${SCRIPT_DIR}/../../files/src/gdi_clip/SaumiGdiClip.dll" \
         "${SCRIPT_DIR}/../src/gdi_clip/SaumiGdiClip.dll"; do
  [[ -f "$f" ]] && DLL="$f" && break
done
[[ -n "$DLL" ]] || { echo "FAIL: нет SaumiGdiClip.dll"; exit 1; }

EZ=""
for f in "${SCRIPT_DIR}/Eztwain3.dll" \
         "${SCRIPT_DIR}/../../files/src/eztwain_proxy/Eztwain3.dll"; do
  [[ -f "$f" ]] && EZ="$f" && break
done
[[ -n "$EZ" ]] || { echo "FAIL: нет Eztwain3.dll"; exit 1; }

WINESERVER="${PREFIX}/runtime/wine-11.0/bin/wineserver"
if [[ -x "$WINESERVER" ]]; then
  env WINEPREFIX="$PREFIX" WINEARCH=win32 "$WINESERVER" -k >/dev/null 2>&1 || true
  sleep 1
fi

mkdir -p "${PREFIX}/drive_c/SAUMI" "${PREFIX}/drive_c/windows/system32" "${PREFIX}/logs"
cp -f "$DLL" "${PREFIX}/drive_c/SAUMI/SaumiGdiClip.dll"
cp -f "$DLL" "${PREFIX}/drive_c/windows/system32/SaumiGdiClip.dll"
chmod 644 "${PREFIX}/drive_c/SAUMI/SaumiGdiClip.dll" \
  "${PREFIX}/drive_c/windows/system32/SaumiGdiClip.dll"
echo "OK: SaumiGdiClip.dll ($(wc -c < "$DLL") bytes)"

cp -f "$EZ" "${PREFIX}/drive_c/SAUMI/Eztwain3.dll"
chmod 644 "${PREFIX}/drive_c/SAUMI/Eztwain3.dll"
echo "OK: Eztwain3.dll ($(wc -c < "$EZ") bytes) — LoadLibrary SaumiGdiClip"

# pathes/20 писал AppInit в ключ с одиночными '\' — Wine его не читает. Убрать.
python3 - "${PREFIX}/system.reg" <<'PY'
import re, sys
from pathlib import Path
p = Path(sys.argv[1])
text = p.read_text(encoding="utf-8", errors="replace")
parts = re.split(r'(?=^\[)', text, flags=re.M)
keep = []
dropped = 0
stripped = 0
for part in parts:
    m = re.match(r'^\[(.+?)\]', part)
    if not m:
        keep.append(part)
        continue
    name = m.group(1)
    is_win = ("Windows NT" in name and "CurrentVersion" in name
              and name.endswith("Windows") and "Windows Messaging" not in name)
    if not is_win:
        keep.append(part)
        continue
    bs = name.count("\\")
    if bs <= 5:
        dropped += 1
        continue
    lines = []
    for line in part.splitlines(True):
        if line.startswith('"AppInit_DLLs"') or line.startswith('"LoadAppInit_DLLs"') \
           or line.startswith('"RequireSignedAppInit_DLLs"'):
            stripped += 1
            continue
        lines.append(line)
    keep.append("".join(lines))
p.write_text("".join(keep), encoding="utf-8")
print(f"OK: system.reg AppInit cleanup (drop_bogus={dropped} strip={stripped})")
PY

echo "OK: pathes/21 gdi clip (загрузка через Eztwain, только окно «Свойства»)"
echo "  После ярлыка: объект → Свойства → Движения → выбрать строку."
echo "  Лог: ~/Saumi/drive_c/SAUMI/temp/gdi_clip.log  и  proxy.log: SaumiGdiClip loaded"
