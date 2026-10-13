#!/usr/bin/env bash
# Микропатч ПОСЛЕ полной установки: PDF/образы + иконка документа в Wine.
# Установщик и golden tar не трогает. mmp32.exe не патчит.
#
# pathes/6: иконки P/W/X 16x16 4bpp в C:\SAUMI\filetypes\, TMP в user.reg.
#   cd /var/install/saumi485/pathes/6
#   SAUMI_PREFIX=$HOME/Saumi bash patch_document_handlers.sh
#
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
SRC=""
for d in "${SCRIPT_DIR}" "${SCRIPT_DIR}/../src/document_handlers"; do
  if [[ -f "${d}/document_handlers.reg" ]]; then
    SRC="$(cd "$d" && pwd)"
    break
  fi
done
[[ -n "$SRC" ]] || { echo "FAIL: нет document_handlers.reg"; exit 1; }
REG="${SRC}/document_handlers.reg"
MERGE=""
for f in "${SCRIPT_DIR}/merge_excel_proxy_registry.py" \
         "${SCRIPT_DIR}/../scripts/merge_excel_proxy_registry.py"; do
  [[ -f "$f" ]] && MERGE="$f" && break
done
PREFIX="${SAUMI_PREFIX:-${HOME}/Saumi}"

[[ -f "${PREFIX}/startSAUMI.sh" ]] || { echo "FAIL: нет ${PREFIX}/startSAUMI.sh — сначала полная установка"; exit 1; }
[[ -f "$REG" ]] || { echo "FAIL: нет $REG"; exit 1; }
[[ -f "$MERGE" ]] || { echo "FAIL: нет $MERGE"; exit 1; }
[[ -f "${PREFIX}/system.reg" ]] || { echo "FAIL: нет ${PREFIX}/system.reg"; exit 1; }

DEST_DIR="${PREFIX}/drive_c/SAUMI/components/SumatraPDF"
FT_DIR="${PREFIX}/drive_c/SAUMI/filetypes"
mkdir -p "$DEST_DIR" "$FT_DIR" "${PREFIX}/logs" "${PREFIX}/drive_c/SAUMI/temp"

EXE_SRC=""
for f in "${SRC}/SumatraPDF.exe" "${DEST_DIR}/SumatraPDF.exe"; do
  [[ -f "$f" ]] && EXE_SRC="$f" && break
done
[[ -n "$EXE_SRC" ]] || { echo "FAIL: нет SumatraPDF.exe (ни в патче, ни в prefix)"; exit 1; }

if [[ -f "${SRC}/SumatraPDF.exe.sha256" && "$EXE_SRC" == "${SRC}/SumatraPDF.exe" ]]; then
  want="$(tr -d ' \n' < "${SRC}/SumatraPDF.exe.sha256" | awk '{print $1}')"
  got="$(shasum -a 256 "$EXE_SRC" 2>/dev/null | awk '{print $1}')"
  if [[ -z "$got" ]]; then
    got="$(sha256sum "$EXE_SRC" | awk '{print $1}')"
  fi
  [[ "$got" == "$want" ]] || { echo "FAIL: sha256 SumatraPDF.exe $got != $want"; exit 1; }
fi

# Пересобрать ICO если есть python+PIL и PNG. Иначе оставляем ico из патча.
MAKE=""
for f in "${SRC}/make_file_icons.py" "${SCRIPT_DIR}/make_file_icons.py"; do
  [[ -f "$f" ]] && MAKE="$f" && break
done
if [[ -n "$MAKE" ]]; then
  python3 "$MAKE" || echo "WARN: make_file_icons не собрал (нет PIL?) — беру ico из патча"
fi

stamp="$(date +%Y%m%d_%H%M%S)"
WINESERVER="${PREFIX}/runtime/wine-11.0/bin/wineserver"
if [[ -x "$WINESERVER" ]]; then
  env WINEPREFIX="$PREFIX" WINEARCH=win32 "$WINESERVER" -k >/dev/null 2>&1 || true
  sleep 1
fi

# GNU cp файла в себя даёт exit 1 — патч обрывался до иконок и реестра.
if [[ "$EXE_SRC" != "${DEST_DIR}/SumatraPDF.exe" ]]; then
  cp -f "$EXE_SRC" "${DEST_DIR}/SumatraPDF.exe"
fi
chmod 755 "${DEST_DIR}/SumatraPDF.exe"
[[ -f "${SRC}/pdf.ico" ]] || { echo "FAIL: нет pdf.ico после make_file_icons"; exit 1; }
[[ -f "${SRC}/doc.ico" ]] || { echo "FAIL: нет doc.ico после make_file_icons"; exit 1; }
[[ -f "${SRC}/xls.ico" ]] || { echo "FAIL: нет xls.ico после make_file_icons"; exit 1; }
cp -f "${SRC}/pdf.ico" "${DEST_DIR}/pdf.ico"
cp -f "${SRC}/doc.ico" "${DEST_DIR}/doc.ico"
cp -f "${SRC}/pdf.ico" "${FT_DIR}/pdf.ico"
cp -f "${SRC}/doc.ico" "${FT_DIR}/doc.ico"
cp -f "${SRC}/xls.ico" "${FT_DIR}/xls.ico"
[[ -f "${SRC}/dummy.pdf" ]] && cp -f "${SRC}/dummy.pdf" "${FT_DIR}/dummy.pdf"
if [[ -f "${SRC}/pdfopen.exe" ]]; then
  cp -f "${SRC}/pdfopen.exe" "${DEST_DIR}/pdfopen.exe"
  chmod 755 "${DEST_DIR}/pdfopen.exe"
  echo "OK: pdfopen.exe"
fi
if [[ -f "${SRC}/SumatraPDF-settings.txt" ]]; then
  cp -f "${SRC}/SumatraPDF-settings.txt" "${DEST_DIR}/SumatraPDF-settings.txt"
fi

# Профиль Wine под Linux-логин: иначе SHGetDesktopFolder → 0x80070003, pidl is null.
WUSER="$(id -un)"
WPROF="${PREFIX}/drive_c/users/${WUSER}"
mkdir -p \
  "${WPROF}/Desktop" \
  "${WPROF}/Documents" \
  "${WPROF}/Downloads" \
  "${WPROF}/Pictures" \
  "${WPROF}/Music" \
  "${WPROF}/Videos" \
  "${WPROF}/Favorites" \
  "${WPROF}/Templates" \
  "${WPROF}/AppData/Roaming" \
  "${WPROF}/AppData/Local/Temp" \
  "${WPROF}/AppData/Local/Microsoft/Windows/INetCache" \
  "${WPROF}/AppData/Local/Microsoft/Windows/History" \
  "${WPROF}/AppData/Local/Microsoft/Windows/INetCookies" \
  "${WPROF}/Application Data" \
  "${WPROF}/Local Settings/Temp" \
  "${WPROF}/Local Settings/Application Data" \
  "${PREFIX}/drive_c/users/Public/Desktop"
echo "OK: Wine profile ${WPROF}/Desktop"

BAK="${PREFIX}/logs/system.reg.bak_before_doc_handlers_${stamp}"
python3 "$MERGE" "$REG" "${PREFIX}/system.reg" "$BAK"

# HKCU FileExts — иначе Explorer/SHGetFileInfo может игнорировать HKCR
python3 - "$PREFIX/user.reg" <<'PY'
import re, sys, time
from pathlib import Path
p = Path(sys.argv[1])
if not p.is_file():
    sys.exit(0)
ts = int(time.time())
text = p.read_text(encoding="utf-8", errors="replace")
bak = p.with_name(p.name + ".bak_before_doc_handlers")
if not bak.exists():
    bak.write_text(text, encoding="utf-8")
keys = [
    r"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.pdf\\UserChoice",
    r"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.doc\\UserChoice",
    r"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.xls\\UserChoice",
    r"Environment",
]
parts = re.split(r'(?=^\[)', text, flags=re.M)
keep = []
for part in parts:
    m = re.match(r'^\[(.+?)\]', part)
    if m and m.group(1) in keys:
        continue
    keep.append(part)
body = "".join(keep).rstrip() + "\n\n"
body += f"[{keys[0]}] {ts}\n#time={ts:x}\n\"ProgId\"=\"SumatraPDF.pdf\"\n\n"
body += f"[{keys[1]}] {ts}\n#time={ts:x}\n\"ProgId\"=\"SAUMI.doc\"\n\n"
body += f"[{keys[2]}] {ts}\n#time={ts:x}\n\"ProgId\"=\"SAUMI.xls\"\n\n"
body += f"[{keys[3]}] {ts}\n#time={ts:x}\n\"TMP\"=\"C:\\\\SAUMI\\\\temp\"\n\"TEMP\"=\"C:\\\\SAUMI\\\\temp\"\n"
p.write_text(body, encoding="utf-8")
print("user.reg FileExts + Environment TMP=C:\\\\SAUMI\\\\temp")
PY

grep -q 'SumatraPDF.pdf' "${PREFIX}/system.reg" || { echo "FAIL: SumatraPDF.pdf не попал в system.reg"; exit 1; }
[[ -f "${FT_DIR}/pdf.ico" ]] || { echo "FAIL: pdf.ico не скопирован в filetypes"; exit 1; }

# Короткий TEMP: SAUMI пишет smDocs под C:\users\denis\... → Sumatra «\\C:\... не найден».
START="${PREFIX}/startSAUMI.sh"
MARKER="saumi-pathes6-tmp"
if ! grep -q "$MARKER" "$START"; then
  cp -f "$START" "${START}.bak_before_tmp_${stamp}"
  tmp="$(mktemp)"
  awk -v m="$MARKER" '
    { print }
    /export TNS_ADMIN=/ && !done {
      print "# " m
      print "export TMP=\"C:\\\\SAUMI\\\\temp\""
      print "export TEMP=\"C:\\\\SAUMI\\\\temp\""
      done=1
    }
  ' "$START" >"$tmp"
  mv -f "$tmp" "$START"
  chmod 755 "$START"
  echo "OK: TMP=C:\\SAUMI\\temp в startSAUMI.sh"
else
  echo "OK: TMP уже в startSAUMI.sh"
fi

echo "OK: иконки → ${FT_DIR} (pdf/doc/xls 16x16 4bpp)"
echo "    SumatraPDF → ${DEST_DIR}/SumatraPDF.exe"
echo "    Wine Desktop=${WUSER}  prefix=$PREFIX"
echo "    бэкап: $BAK"
echo "Дальше: ярлык SAUMI → свойства (иконка PDF/Word/Excel у номера) и Просмотр."
