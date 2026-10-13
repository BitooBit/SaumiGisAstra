#!/usr/bin/env bash
# Вшить runtime/wine-11.0 (+ wine11-debs) в SAUMI1860wine.tar.gz из files/files/runtime/.
# Запуск на Mac после build_saumi1860_prefix на Astra (базовый tar без wine).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
INSTALL="${ROOT}/install"
TAR_IN="${1:-${INSTALL}/SAUMI1860wine.tar.gz}"
TAR_OUT="${ROOT}/files/_build_tmp/SAUMI1860wine.tar.gz.new"
WORKDIR="${ROOT}/files/_build_tmp/inject_wine_$$"
WINE_SRC="${ROOT}/files/files/runtime/wine-11.0"
DEBS_SRC="${ROOT}/files/files/runtime/wine11-debs"

echo "=== inject wine-11.0 into tar ==="
[[ -f "$TAR_IN" ]] || { echo "FAIL: нет $TAR_IN"; exit 1; }
[[ -d "$WINE_SRC/bin" ]] || { echo "FAIL: нет $WINE_SRC (вытяните из install/SAUMI1860wine.tar.gz)"; exit 1; }

mkdir -p "${ROOT}/files/_build_tmp"
rm -rf "$WORKDIR"
mkdir -p "$WORKDIR/prefix"
tar -xzf "$TAR_IN" -C "$WORKDIR/prefix"
P="$WORKDIR/prefix/SAUMIwine"
[[ -d "$P/drive_c" ]] || { echo "FAIL: структура tar"; exit 1; }

mkdir -p "$P/runtime"
rm -rf "$P/runtime/wine-11.0" "$P/runtime/wine11-debs"
cp -a "$WINE_SRC" "$P/runtime/wine-11.0"
[[ -d "$DEBS_SRC" ]] && cp -a "$DEBS_SRC" "$P/runtime/wine11-debs"

find "$WORKDIR/prefix" \( -name '._*' -o -name '.DS_Store' \) -delete 2>/dev/null || true
export COPYFILE_DISABLE=1
(cd "$WORKDIR/prefix" && tar --format=ustar --uid=0 --gid=0 -czf "$TAR_OUT" SAUMIwine)
mv -f "$TAR_OUT" "$TAR_IN"
rm -rf "$WORKDIR"
echo "=== OK: $TAR_IN (wine-11.0 вшит) ==="
