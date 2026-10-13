#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
# DLL уже в golden tar; для проверки передайте путь (bake передаёт из распакованного prefix).
DLL="${1:-}"
VB="${ROOT}/files/src/excel_proxy/SaumiExcelProxy.vb"
if [[ -z "$DLL" ]]; then
  echo "usage: verify_excel_proxy.sh /path/to/SaumiExcelProxy.dll"
  echo "(канон DLL — внутри install/SAUMI1860wine.tar.gz)"
  exit 1
fi
[[ -f "$DLL" ]] || { echo "FAIL: нет $DLL"; exit 1; }
[[ -f "$VB" ]] || { echo "FAIL: нет $VB"; exit 1; }

python3 - "$DLL" "$VB" <<'PY'
import sys
from pathlib import Path
dll = Path(sys.argv[1]).read_bytes()
vb = Path(sys.argv[2]).read_text(encoding="utf-8")

def u16(s):
    return s.encode("utf-16le")

checks = {
    "Xlsx.23": u16("SaumiExcelProxy.Xlsx.23") in dll,
    "no Xlsx.24": u16("SaumiExcelProxy.Xlsx.24") not in dll,
    "no ShowViaHelper": "ShowViaHelper" not in vb,
    "no SaumiExcelSave": "SaumiExcelSave.exe" not in vb,
    "ComboBox name": "_nameBox As ComboBox" in vb,
    "DropDown": "ComboBoxStyle.DropDown" in vb,
    "no LinuxNativeSave": "LinuxNativeSave" not in vb,
    "OnFormKeyPress": "OnFormKeyPress" in vb,
    "InsertIntoName": "InsertIntoName" in vb,
    "FolderSave": "FolderSaveDialog" in vb,
}
for name, ok in checks.items():
    print(f"{'OK' if ok else 'FAIL'}: {name}")
    if not ok:
        sys.exit(1)
print(f"ALL PASS ({len(dll)} bytes)")
PY
