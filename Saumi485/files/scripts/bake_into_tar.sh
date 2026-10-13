#!/usr/bin/env bash
# Обновить в golden tar: hide + minimize-guard + taskbar_fix + замечания 186 (иконки/Excel 23/TWAIN/EZTwain/Sumatra).
# mmp32.exe не копируем и не touch — только sha/size/mtime как в SAUMI485.zip.
# Режимы окон — только ключи instalSaumi.sh (--wm-mode=1|2).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
INSTALL="${ROOT}/install"
TAR_IN="${1:-${INSTALL}/SAUMI1860wine.tar.gz}"
TAR_OUT="${ROOT}/files/_build_tmp/SAUMI1860wine.tar.gz.new"
WORKDIR="${ROOT}/files/_build_tmp/bake_excel_$$"
MMP32_SHA="0a39282c3b3cc2b73b3b422d07748ea943ad935b4c2fe5f3b9f8bf27be5a9a4f"
MMP32_SIZE=35627696

HIDE="${ROOT}/files/src/saumi_hide_windows.sh"
MINGUARD="${ROOT}/files/src/saumi_minimize_guard.sh"
TASKBAR="${ROOT}/files/src/saumi_taskbar_fix.sh"
REG="${ROOT}/files/src/excel_proxy/SaumiExcelProxy.reg"
FK="${ROOT}/files/src/bin/force_kill_saumi.sh"

echo "=== bake Saumi485: helpers + verify Excel/EW already in tar ==="
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
[[ -f "$TAR_IN" ]] || { echo "FAIL: нет $TAR_IN"; exit 1; }
for f in "$HIDE" "$MINGUARD" "$TASKBAR" "$REG"; do
  [[ -f "$f" ]] || { echo "FAIL: нет $f"; exit 1; }
done

MMP32_MTIME_IN=$(python3 -c 'import tarfile,sys; m=tarfile.open(sys.argv[1],"r:gz").getmember("SAUMIwine/drive_c/SAUMI/mmp32.exe"); print(int(m.mtime))' "$TAR_IN")

mkdir -p "${ROOT}/files/_build_tmp"
rm -rf "$WORKDIR"
mkdir -p "$WORKDIR/prefix"
tar -xzf "$TAR_IN" -C "$WORKDIR/prefix"
P="$WORKDIR/prefix/SAUMIwine"
[[ -d "$P/drive_c/SAUMI" ]] || { echo "FAIL: структура tar"; exit 1; }

MMP="$(sha256sum "$P/drive_c/SAUMI/mmp32.exe" | awk '{print $1}')"
MMP_SZ=$(stat -c%s "$P/drive_c/SAUMI/mmp32.exe" 2>/dev/null || stat -f%z "$P/drive_c/SAUMI/mmp32.exe")
[[ "$MMP" == "$MMP32_SHA" && "$MMP_SZ" == "$MMP32_SIZE" ]] || { echo "FAIL: mmp32 sha/size до bake"; exit 1; }
echo "mmp32 до bake: sha=${MMP:0:12}… size=${MMP_SZ} mtime_tar=${MMP32_MTIME_IN}"

DLL_IN_TAR="$P/drive_c/SAUMI/SaumiExcelProxy.dll"
EW_IN_TAR="$P/drive_c/SAUMI/ExplorerWindow.exe"
[[ -f "$DLL_IN_TAR" ]] || { echo "FAIL: в tar нет SaumiExcelProxy.dll"; exit 1; }
[[ -f "$EW_IN_TAR" ]] || { echo "FAIL: в tar нет ExplorerWindow.exe"; exit 1; }
# Xlsx.23 (даты без 00:00:00) — канон в files/src; mmp32 не трогаем.
if [[ -f "${ROOT}/files/src/excel_proxy/SaumiExcelProxy.dll" ]]; then
  cp -f "${ROOT}/files/src/excel_proxy/SaumiExcelProxy.dll" "$DLL_IN_TAR"
fi
bash "${SCRIPT_DIR}/verify_excel_proxy.sh" "$DLL_IN_TAR" || { echo "FAIL: verify_excel_proxy"; exit 1; }
DLL_SHA_TAR="$(sha256sum "$DLL_IN_TAR" | awk '{print $1}')"
echo "Excel dll in tar=${DLL_SHA_TAR:0:12}… (оставляем как есть)"

mkdir -p "$P/bin" "$P/config"
cp -f "$HIDE" "$P/bin/saumi_hide_windows.sh"
cp -f "$MINGUARD" "$P/bin/saumi_minimize_guard.sh"
cp -f "$TASKBAR" "$P/bin/saumi_taskbar_fix.sh"
chmod 755 "$P/bin/saumi_hide_windows.sh" "$P/bin/saumi_minimize_guard.sh" \
  "$P/bin/saumi_taskbar_fix.sh"
echo 2 >"$P/config/wm_mode"
rm -f "$P/bin/saumi_wm_mode.sh" "$P/bin/saumi_wm_boot_switch.sh" "$P/СПРАВКА.txt" \
  "$P/drive_c/SAUMI/SaumiExcelSave.exe" 2>/dev/null || true
cp -f "$REG" "$P/drive_c/SAUMI/SaumiExcelProxy.reg"
[[ -f "$FK" ]] && cp -f "$FK" "$P/bin/force_kill_saumi.sh" && chmod 755 "$P/bin/force_kill_saumi.sh"

MMP="$(sha256sum "$P/drive_c/SAUMI/mmp32.exe" | awk '{print $1}')"
[[ "$MMP" == "$MMP32_SHA" ]] || { echo "FAIL: mmp32 sha после bake helpers"; exit 1; }

MERGE="${ROOT}/files/scripts/merge_excel_proxy_registry.py"
python3 "$MERGE" "$REG" "$P/system.reg" "$P/logs/system.reg.bak_before_excel_$$" \
  || { echo "FAIL: merge Excel registry"; exit 1; }
grep -q 'Excel.Application' "$P/system.reg" || { echo "FAIL: Excel.Application не в system.reg"; exit 1; }
bash "${SCRIPT_DIR}/bake_remarks_into_prefix.sh" "$P"
echo "bake-helpers-wm2-nohybrid $(date -u +%Y-%m-%dT%H:%M:%SZ)" >>"$P/BUILD_ID.txt"

find "$WORKDIR/prefix" \( -name '._*' -o -name '.DS_Store' \) -delete 2>/dev/null || true
export COPYFILE_DISABLE=1
(cd "$WORKDIR/prefix" && tar --format=ustar --uid=0 --gid=0 -czf "$TAR_OUT" SAUMIwine)
mv -f "$TAR_OUT" "$TAR_IN"
rm -rf "$WORKDIR"

sha=$(tar -xOf "$TAR_IN" SAUMIwine/drive_c/SAUMI/mmp32.exe | sha256sum | awk '{print $1}')
[[ "$sha" == "$MMP32_SHA" ]] || { echo "FAIL: mmp32 sha в новом tar"; exit 1; }
python3 - "$TAR_IN" "$MMP32_MTIME_IN" "$MMP32_SIZE" <<'PY' || { echo "FAIL: mmp32 в tar"; exit 1; }
import sys, tarfile
t = tarfile.open(sys.argv[1], 'r:gz')
m = t.getmember('SAUMIwine/drive_c/SAUMI/mmp32.exe')
want_mt, want_sz = int(sys.argv[2]), int(sys.argv[3])
assert m.size == want_sz, m.size
assert int(m.mtime) == want_mt, f"mtime {int(m.mtime)} != {want_mt}"
print(f"OK mmp32 tar: size={m.size} sha ok mtime={int(m.mtime)}")
PY
tar -xOf "$TAR_IN" SAUMIwine/drive_c/SAUMI/SaumiExcelProxy.dll | sha256sum | awk -v want="$DLL_SHA_TAR" '{
  if ($1 != want) { print "FAIL Excel sha changed in tar"; exit 1 }
  print "OK Excel dll sha=" substr($1,1,12) "…"
}'
tar -tzf "$TAR_IN" | grep -E 'bin/saumi_(minimize_guard|hide_windows|taskbar_fix)\.sh|config/wm_mode|SaumiExcel' | head
echo "=== OK: $TAR_IN ==="
