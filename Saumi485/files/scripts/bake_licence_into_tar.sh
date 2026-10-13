#!/usr/bin/env bash
# Вшить Guardant Net в SAUMI1860wine.tar.gz (Saumi485).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
LIC="${ROOT}/files/files/licence_server"
INSTALL="${ROOT}/install"
TAR_IN="${1:-${INSTALL}/SAUMI1860wine.tar.gz}"
TAR_OUT="${ROOT}/files/_build_tmp/SAUMI1860wine.tar.gz.new"
WORKDIR="${ROOT}/files/_build_tmp/bake_lic_$$"
MMP32_SHA="0a39282c3b3cc2b73b3b422d07748ea943ad935b4c2fe5f3b9f8bf27be5a9a4f"
EW_SHA="6d77ec50640eb927d35e240c4d5fe4390d4ceaebb77e4294cd9a5a2eaa238000"

echo "=== bake Saumi485: Guardant ==="
[[ -f "$TAR_IN" ]] || { echo "FAIL: нет $TAR_IN"; exit 1; }
[[ -f "$LIC/NNKSRV32.EXE" ]] || { echo "FAIL: нет $LIC"; exit 1; }

mkdir -p "${ROOT}/files/_build_tmp"
rm -rf "$WORKDIR"
mkdir -p "$WORKDIR/prefix"
tar -xzf "$TAR_IN" -C "$WORKDIR/prefix"
P="$WORKDIR/prefix/SAUMIwine"
[[ -d "$P/drive_c/SAUMI" ]] || { echo "FAIL: структура tar"; exit 1; }

MMP="$(sha256sum "$P/drive_c/SAUMI/mmp32.exe" | awk '{print $1}')"
EW="$(sha256sum "$P/drive_c/SAUMI/ExplorerWindow.exe" | awk '{print $1}')"
[[ "$MMP" == "$MMP32_SHA" ]] || { echo "FAIL: mmp32 sha"; exit 1; }
[[ "$EW" == "$EW_SHA" ]] || { echo "FAIL: EW sha"; exit 1; }

find "$WORKDIR/prefix" \( -name '._*' -o -name '.DS_Store' \) -delete 2>/dev/null || true
mkdir -p "$P/bin" "$P/logs"
cp -f "$LIC/NNKSRV32.EXE" "$LIC/NNKSRV32.INI" "$LIC/NOVEX32.DLL" "$P/drive_c/SAUMI/"
cp -f "$LIC/GNCLIENT.INI" "$P/drive_c/SAUMI/gnclient.ini.licence_local"
for s in install_licence_server.sh start_licence_server.sh stop_licence_server.sh probe_licence_server.sh; do
  cp -f "$LIC/$s" "$P/bin/" && chmod 755 "$P/bin/$s"
done
echo "bake-licence $(date -u +%Y-%m-%dT%H:%M:%SZ)" >>"$P/BUILD_ID.txt"

export COPYFILE_DISABLE=1
(cd "$WORKDIR/prefix" && tar --format=ustar --uid=0 --gid=0 -czf "$TAR_OUT" SAUMIwine)
mv -f "$TAR_OUT" "$TAR_IN"
rm -rf "$WORKDIR"
bash "${ROOT}/files/scripts/sync_release.sh" 2>/dev/null || true
echo "=== OK: $TAR_IN ==="
