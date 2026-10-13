#!/usr/bin/env bash
# Заменить drive_c/SAUMI в golden tar на клиент из SAUMI4113.zip.
# Wine-prefix / oracle / runtime / helpers не трогаем.
# Сохраняем Linux-оверлеи из 485: SaumiExcelProxy.*, gnclient.ini(.licence_local), export/.
# ControlDisk из zip пропускаем (ломает unzip на macOS + не нужен под Wine).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
INSTALL="${ROOT}/install"
TAR_IN="${1:-${INSTALL}/SAUMI1860wine.tar.gz}"
ZIP="${ROOT}/files/files/SAUMI4113.zip"
WORKDIR="${ROOT}/files/_build_tmp/replace4113_$$"
TAR_OUT="${ROOT}/files/_build_tmp/SAUMI1860wine.tar.gz.new"

MMP32_SHA="50403256117d59968adff29230d3f0b89233a2159eb00425818effbed2dc4789"
MMP32_SIZE=36109873
# mtime из SAUMI4113.zip: 2024-04-23 11:52:12 (локальное = MSK)
MMP32_MTIME=1713862332

echo "=== replace SAUMI client → 4.11.3 in tar ==="
[[ -f "$TAR_IN" ]] || { echo "FAIL: нет $TAR_IN"; exit 1; }
[[ -f "$ZIP" ]] || { echo "FAIL: нет $ZIP"; exit 1; }

mkdir -p "${ROOT}/files/_build_tmp"
rm -rf "$WORKDIR"
mkdir -p "$WORKDIR/prefix" "$WORKDIR/keep"

echo "unpack tar…"
tar -xzf "$TAR_IN" -C "$WORKDIR/prefix"
P="$WORKDIR/prefix/SAUMIwine"
SAUMI="$P/drive_c/SAUMI"
[[ -d "$SAUMI" ]] || { echo "FAIL: нет $SAUMI"; exit 1; }

keep_list=(
  SaumiExcelProxy.dll
  SaumiExcelProxy.reg
  SaumiExcelProxy.vb
  gnclient.ini
  gnclient.ini.licence_local
  inflexe_gw.dll
)
for f in "${keep_list[@]}"; do
  if [[ -e "$SAUMI/$f" ]]; then
    cp -a "$SAUMI/$f" "$WORKDIR/keep/$f"
    echo "  keep: $f"
  fi
done
if [[ -d "$SAUMI/export" ]]; then
  cp -a "$SAUMI/export" "$WORKDIR/keep/export"
  echo "  keep: export/"
fi

echo "extract SAUMI4113.zip (skip ControlDisk)…"
rm -rf "$SAUMI"
python3 - "$ZIP" "$P/drive_c" <<'PY'
import sys, zipfile, os
from pathlib import Path
zip_path, dest_parent = sys.argv[1], Path(sys.argv[2])
dest = dest_parent / "SAUMI"
dest.mkdir(parents=True, exist_ok=True)
skipped = 0
with zipfile.ZipFile(zip_path) as z:
    for info in z.infolist():
        name = info.filename.replace("\\", "/")
        if not name.startswith("SAUMI/"):
            continue
        rel = name[len("SAUMI/"):]
        if not rel or rel.endswith("/"):
            if rel:
                (dest / rel).mkdir(parents=True, exist_ok=True)
            continue
        if rel.startswith("ControlDisk/") or rel == "ControlDisk":
            skipped += 1
            continue
        out = dest / rel
        out.parent.mkdir(parents=True, exist_ok=True)
        with z.open(info) as src, open(out, "wb") as dst:
            while True:
                b = src.read(1 << 20)
                if not b:
                    break
                dst.write(b)
        # zip date_time → mtime
        try:
            import datetime, time
            dt = datetime.datetime(*info.date_time)
            os.utime(out, (dt.timestamp(), dt.timestamp()))
        except Exception:
            pass
print(f"extracted OK, skipped ControlDisk entries={skipped}")
PY
[[ -f "$SAUMI/mmp32.exe" ]] || { echo "FAIL: нет mmp32.exe после extract"; exit 1; }

for f in "${keep_list[@]}"; do
  if [[ -e "$WORKDIR/keep/$f" ]]; then
    cp -a "$WORKDIR/keep/$f" "$SAUMI/$f"
    echo "  restore: $f"
  fi
done
if [[ -d "$WORKDIR/keep/export" ]]; then
  mkdir -p "$SAUMI/export"
  cp -a "$WORKDIR/keep/export/." "$SAUMI/export/"
  echo "  restore: export/"
else
  mkdir -p "$SAUMI/export"
fi

[[ -f "$SAUMI/SaumiExcelProxy.dll" ]] || { echo "FAIL: нет SaumiExcelProxy.dll после restore"; exit 1; }
[[ -f "$SAUMI/SaumiExcelProxy.reg" ]] || { echo "FAIL: нет SaumiExcelProxy.reg после restore"; exit 1; }

python3 - "$SAUMI/mmp32.exe" "$MMP32_MTIME" <<'PY'
import os, sys
path, mt = sys.argv[1], int(sys.argv[2])
os.utime(path, (mt, mt))
print(f"mmp32 utime → {mt}")
PY

MMP="$(shasum -a 256 "$SAUMI/mmp32.exe" | awk '{print $1}')"
MMP_SZ=$(stat -f%z "$SAUMI/mmp32.exe" 2>/dev/null || stat -c%s "$SAUMI/mmp32.exe")
[[ "$MMP" == "$MMP32_SHA" && "$MMP_SZ" == "$MMP32_SIZE" ]] \
  || { echo "FAIL: mmp32 sha/size ($MMP_SZ $MMP)"; exit 1; }
echo "mmp32 OK sha=${MMP:0:12}… size=$MMP_SZ"

{
  echo "client=SAUMI4113.zip"
  echo "mmp32_sha=${MMP32_SHA}"
  echo "replaced_from_485 $(date -u +%Y-%m-%dT%H:%M:%SZ)"
} >>"$P/BUILD_ID.txt"

find "$WORKDIR/prefix" \( -name '._*' -o -name '.DS_Store' \) -delete 2>/dev/null || true
export COPYFILE_DISABLE=1

echo "repack tar…"
(cd "$WORKDIR/prefix" && tar --format=ustar --uid=0 --gid=0 -czf "$TAR_OUT" SAUMIwine)
mv -f "$TAR_OUT" "$TAR_IN"
rm -rf "$WORKDIR"

sha=$(tar -xOf "$TAR_IN" SAUMIwine/drive_c/SAUMI/mmp32.exe | shasum -a 256 | awk '{print $1}')
[[ "$sha" == "$MMP32_SHA" ]] || { echo "FAIL: mmp32 sha в новом tar"; exit 1; }
python3 - "$TAR_IN" "$MMP32_MTIME" "$MMP32_SIZE" <<'PY'
import sys, tarfile
t = tarfile.open(sys.argv[1], 'r:gz')
m = t.getmember('SAUMIwine/drive_c/SAUMI/mmp32.exe')
want_mt, want_sz = int(sys.argv[2]), int(sys.argv[3])
assert m.size == want_sz, m.size
assert int(m.mtime) == want_mt, f"mtime {int(m.mtime)} != {want_mt}"
print(f"OK tar mmp32 size={m.size} mtime={int(m.mtime)}")
names = set(t.getnames())
for n in (
  'SAUMIwine/drive_c/SAUMI/SaumiExcelProxy.dll',
  'SAUMIwine/drive_c/SAUMI/SaumiExcelProxy.reg',
  'SAUMIwine/drive_c/SAUMI/gnclient.ini',
):
  assert n in names, n
print('OK overlays Excel+gnclient in tar')
PY
echo "=== OK: $TAR_IN (SAUMI 4.11.3) ==="
