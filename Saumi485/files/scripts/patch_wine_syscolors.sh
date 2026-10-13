#!/usr/bin/env bash
# Цвета комбо / чёрная строка курсора — ТОЛЬКО Wine prefix.
# SM.MOVETYPE.MAP_COLOR не меняем: Windows и Linux сидят на одной БД.
#
# Зачем: Astra/Wine часто отдаёт COLOR_HIGHLIGHT чёрным → owner-draw
# заливает выбранную строку чёрным, текст тоже чёрный. На Windows
# Hilight синий, HilightText белый — там проблемы нет.
#
#   cd /var/install/saumi485/pathes/6
#   SAUMI_PREFIX=$HOME/Saumi bash patch_wine_syscolors.sh
#
set -euo pipefail
PREFIX="${SAUMI_PREFIX:-${HOME}/Saumi}"
USERREG="${PREFIX}/user.reg"
[[ -f "$USERREG" ]] || { echo "FAIL: нет $USERREG"; exit 1; }
[[ -f "${PREFIX}/startSAUMI.sh" ]] || { echo "FAIL: нет prefix"; exit 1; }
mkdir -p "${PREFIX}/logs"

WINESERVER="${PREFIX}/runtime/wine-11.0/bin/wineserver"
if [[ -x "$WINESERVER" ]]; then
  env WINEPREFIX="$PREFIX" WINEARCH=win32 "$WINESERVER" -k >/dev/null 2>&1 || true
  sleep 1
fi

stamp="$(date +%Y%m%d_%H%M%S)"
cp -f "$USERREG" "${PREFIX}/logs/user.reg.bak_before_syscolors_${stamp}"
mkdir -p "${PREFIX}/logs"

python3 - "$USERREG" <<'PY'
import re, sys, time
from pathlib import Path
p = Path(sys.argv[1])
text = p.read_text(encoding="utf-8", errors="replace")
ts = int(time.time())
key = r"Control Panel\\Colors"
parts = re.split(r'(?=^\[)', text, flags=re.M)
keep = []
for part in parts:
    m = re.match(r'^\[(.+?)\]', part)
    if m and m.group(1) == key:
        continue
    keep.append(part)
body = "".join(keep).rstrip() + "\n\n"
# Светлая схема Windows (только этот prefix). MAP_COLOR в Oracle не трогаем.
body += f"[{key}] {ts}\n#time={ts:x}\n"
body += '"Hilight"="0 56 116"\n'
body += '"HilightText"="255 255 255"\n'
body += '"HotTrackingColor"="0 56 116"\n'
body += '"InactiveTitle"="153 180 209"\n'
body += '"InactiveTitleText"="0 0 0"\n'
body += '"TitleText"="255 255 255"\n'
body += '"Window"="255 255 255"\n'
body += '"WindowText"="0 0 0"\n'
body += '"ButtonFace"="240 240 240"\n'
body += '"ButtonText"="0 0 0"\n'
body += '"Menu"="255 255 255"\n'
body += '"MenuText"="0 0 0"\n'
body += '"GrayText"="128 128 128"\n'
body += '"InfoWindow"="255 255 225"\n'
body += '"InfoText"="0 0 0"\n'
p.write_text(body, encoding="utf-8")
print("user.reg Control Panel\\\\Colors = Windows navy (Hilight 0 56 116, не cyan)")
PY

echo "OK: цвета выделения только в $PREFIX (Windows-клиенты / Oracle не задеты)"
echo "    бэкап ${PREFIX}/logs/user.reg.bak_before_syscolors_${stamp}"
echo "Перезапусти SAUMI с ярлыка, открой комбо «Вид движения» и список с чёрной строкой курсора."
echo "Если невыделенные тёмные пункты (Субаренда и т.п.) всё ещё чёрные — не осветляем БД,"
echo "следующий шаг: отрисовка текста в Wine, не MAP_COLOR."
