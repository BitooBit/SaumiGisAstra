#!/usr/bin/env bash
# Вшить в распакованный prefix то, что сработало на АРМ (замечания SAUMI 186).
# Не трогает mmp32.exe. Вызывается из bake_into_tar.sh; архив сам не собирает.
#
# Успешные решения pathes/6…19:
#   иконки 16x16 4bpp, Excel Xlsx.23, Wine Hilight (не Oracle),
#   gphoto2/sane.off + hp_sane.ds + демон scanimage,
#   EZTwain proxy (PDF вложение, без BMP/*.2D),
#   Sumatra + pdfopen (без вкладок/пустой страницы).
#
#   PREFIX=/path/to/SAUMIwine bash bake_remarks_into_prefix.sh
#
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
PREFIX="${1:-${PREFIX:-}}"
[[ -n "$PREFIX" && -d "${PREFIX}/drive_c/SAUMI" ]] || { echo "FAIL: PREFIX=.../SAUMIwine"; exit 1; }

SRC_ICO="${ROOT}/files/src/document_handlers"
SRC_EX="${ROOT}/files/src/excel_proxy"
SRC_TW="${ROOT}/files/src/twain_ds"
SRC_EZ="${ROOT}/files/src/eztwain_proxy"
SCR="${ROOT}/files/scripts"
MERGE="${SCR}/merge_excel_proxy_registry.py"

mkdir -p \
  "${PREFIX}/drive_c/SAUMI/filetypes" \
  "${PREFIX}/drive_c/SAUMI/temp/twain" \
  "${PREFIX}/drive_c/SAUMI/temp/SAUMI_SCANS" \
  "${PREFIX}/drive_c/SAUMI/twain" \
  "${PREFIX}/drive_c/SAUMI/components/SumatraPDF" \
  "${PREFIX}/drive_c/windows/twain_32" \
  "${PREFIX}/bin" \
  "${PREFIX}/logs" \
  "${PREFIX}/config"

# 1) Иконки: teambox MIT → 16x16 4bpp ICO (Wine SYSICONINDEX). PNG-only ICO пустые.
[[ -f "${SRC_ICO}/pdf.ico" && -f "${SRC_ICO}/doc.ico" && -f "${SRC_ICO}/xls.ico" ]] \
  || { echo "FAIL: нет pdf/doc/xls.ico"; exit 1; }
cp -f "${SRC_ICO}/pdf.ico" "${SRC_ICO}/doc.ico" "${SRC_ICO}/xls.ico" \
  "${PREFIX}/drive_c/SAUMI/filetypes/"
[[ -f "${SRC_ICO}/dummy.pdf" ]] && cp -f "${SRC_ICO}/dummy.pdf" \
  "${PREFIX}/drive_c/SAUMI/filetypes/"
echo "OK filetypes ico"

# 2) Sumatra + pdfopen (pathes/6 иконка/просмотр, 18–19 без вкладок и без -new-instance)
SUM_DIR="${PREFIX}/drive_c/SAUMI/components/SumatraPDF"
if [[ -f "${SRC_ICO}/SumatraPDF.exe" ]]; then
  if [[ -f "${SRC_ICO}/SumatraPDF.exe.sha256" ]]; then
    want="$(tr -d ' \n' < "${SRC_ICO}/SumatraPDF.exe.sha256" | awk '{print $1}')"
    got="$(shasum -a 256 "${SRC_ICO}/SumatraPDF.exe" 2>/dev/null | awk '{print $1}')"
    [[ -n "$got" ]] || got="$(sha256sum "${SRC_ICO}/SumatraPDF.exe" | awk '{print $1}')"
    [[ "$got" == "$want" ]] || { echo "FAIL: sha256 SumatraPDF.exe $got != $want"; exit 1; }
  fi
  cp -f "${SRC_ICO}/SumatraPDF.exe" "${SUM_DIR}/SumatraPDF.exe"
  chmod 755 "${SUM_DIR}/SumatraPDF.exe"
  echo "OK SumatraPDF.exe"
fi
[[ -f "${SUM_DIR}/SumatraPDF.exe" ]] || { echo "FAIL: нет SumatraPDF.exe в prefix"; exit 1; }
if [[ -f "${SRC_ICO}/pdfopen.exe" ]]; then
  cp -f "${SRC_ICO}/pdfopen.exe" "${SUM_DIR}/pdfopen.exe"
  chmod 755 "${SUM_DIR}/pdfopen.exe"
  echo "OK pdfopen.exe"
fi
if [[ -f "${SRC_ICO}/SumatraPDF-settings.txt" ]]; then
  cp -f "${SRC_ICO}/SumatraPDF-settings.txt" "${SUM_DIR}/SumatraPDF-settings.txt"
fi
cp -f "${SRC_ICO}/pdf.ico" "${SRC_ICO}/doc.ico" "${SUM_DIR}/" 2>/dev/null || true

if [[ -f "${SRC_ICO}/document_handlers.reg" && -f "$MERGE" && -f "${PREFIX}/system.reg" ]]; then
  python3 "$MERGE" "${SRC_ICO}/document_handlers.reg" "${PREFIX}/system.reg" \
    "${PREFIX}/logs/system.reg.bak_before_doc_handlers_bake"
  grep -q 'SumatraPDF.pdf' "${PREFIX}/system.reg" \
    || { echo "FAIL: SumatraPDF.pdf не в system.reg"; exit 1; }
  echo "OK document_handlers.reg → system.reg"
fi

# HKCU FileExts + короткий TEMP (иначе smDocs под C:\users\… и Sumatra «не найден»)
if [[ -f "${PREFIX}/user.reg" ]]; then
  python3 - "${PREFIX}/user.reg" <<'PY'
import re, sys, time
from pathlib import Path
p = Path(sys.argv[1])
ts = int(time.time())
text = p.read_text(encoding="utf-8", errors="replace")
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
print("OK user.reg FileExts + TMP")
PY
fi

# 3) Excel: готовая net35 DLL (не wine vbc на АРМ). Даты dd.mm.yyyy.
if [[ -f "${SRC_EX}/SaumiExcelProxy.dll" ]]; then
  cp -f "${SRC_EX}/SaumiExcelProxy.dll" "${PREFIX}/drive_c/SAUMI/SaumiExcelProxy.dll"
  echo "OK SaumiExcelProxy.dll Xlsx.23"
fi

# 4) Сканер: свой TWAIN DS, gphoto2/sane.ds выключить (40с камера + MSG_GET fail).
[[ -f "${SRC_TW}/hp_sane.ds" ]] || { echo "FAIL: нет hp_sane.ds"; exit 1; }
cp -f "${SRC_TW}/hp_sane.ds" "${PREFIX}/drive_c/SAUMI/twain/hp_sane.ds"
cp -f "${SRC_TW}/hp_sane.ds" "${PREFIX}/drive_c/windows/twain_32/hp_sane.ds"
for f in gphoto2.ds sane.ds; do
  if [[ -f "${PREFIX}/drive_c/windows/twain_32/${f}" ]]; then
    mv -f "${PREFIX}/drive_c/windows/twain_32/${f}" \
      "${PREFIX}/drive_c/windows/twain_32/${f}.off"
  fi
  if [[ -f "${PREFIX}/drive_c/windows/system32/${f}" ]]; then
    mv -f "${PREFIX}/drive_c/windows/system32/${f}" \
      "${PREFIX}/drive_c/windows/system32/${f}.off"
  fi
done
cp -f "${SCR}/saumi_scan_daemon.sh" "${PREFIX}/bin/"
cp -f "${SCR}/saumi_twain_runtime.sh" "${PREFIX}/bin/"
cp -f "${SRC_TW}/pnm_to_dib.py" "${PREFIX}/bin/"
cp -f "${SCR}/saumi_scan.sh" "${PREFIX}/bin/"
chmod 755 "${PREFIX}/bin/saumi_scan_daemon.sh" "${PREFIX}/bin/saumi_twain_runtime.sh" \
  "${PREFIX}/bin/saumi_scan.sh"
echo "OK twain hp_sane.ds + daemon"

# 5) EZTwain proxy (pathes/10…19). Оригинал >100К → Eztwain3_orig.dll (лицензия).
EZ="${PREFIX}/drive_c/SAUMI/Eztwain3.dll"
ORIG="${PREFIX}/drive_c/SAUMI/Eztwain3_orig.dll"
PROXY="${SRC_EZ}/Eztwain3.dll"
[[ -f "$PROXY" ]] || { echo "FAIL: нет $PROXY"; exit 1; }
[[ "$(wc -c < "$PROXY")" -gt 10000 ]] || { echo "FAIL: proxy слишком мал"; exit 1; }
if [[ ! -f "$ORIG" ]]; then
  [[ -f "$EZ" ]] || { echo "FAIL: нет оригинального Eztwain3.dll в prefix"; exit 1; }
  [[ "$(wc -c < "$EZ")" -gt 100000 ]] || { echo "FAIL: Eztwain3.dll в tar не оригинал ($(wc -c < "$EZ") bytes)"; exit 1; }
  cp -f "$EZ" "$ORIG"
  echo "OK оригинал Eztwain3.dll → Eztwain3_orig.dll ($(wc -c < "$ORIG") bytes)"
fi
cp -f "$PROXY" "$EZ"
chmod 644 "$EZ" "$ORIG"
echo "OK EZTwain proxy → Eztwain3.dll ($(wc -c < "$EZ") bytes)"

# 6) Цвета выделения только Wine (не Oracle MAP_COLOR).
if [[ -f "${PREFIX}/user.reg" && -f "${PREFIX}/startSAUMI.sh" ]]; then
  SAUMI_PREFIX="$PREFIX" bash "${SCR}/patch_wine_syscolors.sh" || true
fi

echo "bake_remarks_into_prefix OK $PREFIX"
