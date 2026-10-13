#!/usr/bin/env bash
# Вшить offline host .deb (i386/ia32 + wm tools) в SAUMI1860wine.tar.gz.
# Источник: files/files/runtime/saumi-host-debs/*.deb
# mmp32.exe не трогаем (только добавляем runtime/saumi-host-debs/).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
INSTALL="${ROOT}/install"
TAR_IN="${1:-${INSTALL}/SAUMI1860wine.tar.gz}"
TAR_OUT="${ROOT}/files/_build_tmp/SAUMI1860wine.tar.gz.new"
WORKDIR="${ROOT}/files/_build_tmp/inject_hostdebs_$$"
DEBS_SRC="${ROOT}/files/files/runtime/saumi-host-debs"
MMP32_SHA="0a39282c3b3cc2b73b3b422d07748ea943ad935b4c2fe5f3b9f8bf27be5a9a4f"
MMP32_SIZE=35627696

echo "=== inject saumi-host-debs into tar ==="
[[ -f "$TAR_IN" ]] || { echo "FAIL: нет $TAR_IN"; exit 1; }
shopt -s nullglob
debs=("$DEBS_SRC"/*.deb)
[[ ${#debs[@]} -gt 0 ]] || { echo "FAIL: нет $DEBS_SRC/*.deb"; exit 1; }

MMP32_MTIME_IN=$(python3 -c 'import tarfile,sys; m=tarfile.open(sys.argv[1],"r:gz").getmember("SAUMIwine/drive_c/SAUMI/mmp32.exe"); print(int(m.mtime))' "$TAR_IN")

mkdir -p "${ROOT}/files/_build_tmp"
rm -rf "$WORKDIR"
mkdir -p "$WORKDIR/prefix"
tar -xzf "$TAR_IN" -C "$WORKDIR/prefix"
P="$WORKDIR/prefix/SAUMIwine"
[[ -d "$P/drive_c/SAUMI" ]] || { echo "FAIL: структура tar"; exit 1; }

MMP="$(sha256sum "$P/drive_c/SAUMI/mmp32.exe" | awk '{print $1}')"
MMP_SZ=$(stat -c%s "$P/drive_c/SAUMI/mmp32.exe" 2>/dev/null || stat -f%z "$P/drive_c/SAUMI/mmp32.exe")
[[ "$MMP" == "$MMP32_SHA" && "$MMP_SZ" == "$MMP32_SIZE" ]] || { echo "FAIL: mmp32 sha/size до inject"; exit 1; }

mkdir -p "$P/runtime/saumi-host-debs"
rm -f "$P/runtime/saumi-host-debs"/*.deb
cp -a "${debs[@]}" "$P/runtime/saumi-host-debs/"
echo "debs: $(ls -1 "$P/runtime/saumi-host-debs"/*.deb | wc -l | tr -d ' ')"

MMP="$(sha256sum "$P/drive_c/SAUMI/mmp32.exe" | awk '{print $1}')"
[[ "$MMP" == "$MMP32_SHA" ]] || { echo "FAIL: mmp32 sha после cp debs"; exit 1; }

find "$WORKDIR/prefix" \( -name '._*' -o -name '.DS_Store' \) -delete 2>/dev/null || true
export COPYFILE_DISABLE=1
(cd "$WORKDIR/prefix" && tar --format=ustar --uid=0 --gid=0 -czf "$TAR_OUT" SAUMIwine)
mv -f "$TAR_OUT" "$TAR_IN"
rm -rf "$WORKDIR"

sha=$(tar -xOf "$TAR_IN" SAUMIwine/drive_c/SAUMI/mmp32.exe | sha256sum | awk '{print $1}')
[[ "$sha" == "$MMP32_SHA" ]] || { echo "FAIL: mmp32 sha в новом tar"; exit 1; }
python3 - "$TAR_IN" "$MMP32_MTIME_IN" "$MMP32_SIZE" <<'PY'
import sys, tarfile
t = tarfile.open(sys.argv[1], 'r:gz')
m = t.getmember('SAUMIwine/drive_c/SAUMI/mmp32.exe')
assert m.size == int(sys.argv[3])
assert int(m.mtime) == int(sys.argv[2])
n = sum(1 for x in t.getnames() if x.startswith('SAUMIwine/runtime/saumi-host-debs/') and x.endswith('.deb'))
print(f"OK mmp32 size/mtime; host-debs in tar={n}")
assert n >= 3
PY
# companion copy убран: поставка только instalSaumi.sh + SAUMI1860wine.tar.gz
echo "=== OK: $TAR_IN (host-debs внутри архива) ==="
