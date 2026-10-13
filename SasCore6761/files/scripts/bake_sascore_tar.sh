#!/usr/bin/env bash
# Сборка SasCoreWine.tar.gz (SasCore6761).
# .NET 4 вшивается offline из files/files/dotnet4_*.tar.gz (без winetricks/интернета).
set -euo pipefail
export COPYFILE_DISABLE=1
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
SAS="${ROOT}/files/files/sascore"
BUNDLE="${ROOT}/files/files"
INSTALL="${ROOT}/install"
SCRIPTS="${ROOT}/files/scripts"
TAR_IN="${1:-${ROOT}/../Saumi485/install/SAUMI1860wine.tar.gz}"
TAR_OUT="${INSTALL}/SasCoreWine.tar.gz"
WORKDIR="${ROOT}/files/_build_tmp/bake_sascore_$$"

echo "=== bake SasCore6761 ==="
[[ -f "$TAR_IN" ]] || { echo "FAIL: нет $TAR_IN"; exit 1; }
[[ -f "$SAS/payload/Core/SasCoreWindowsService.exe" ]] || { echo "FAIL: нет payload"; exit 1; }
[[ -f "${BUNDLE}/dotnet4_framework.tar.gz" ]] || {
  echo "FAIL: нет ${BUNDLE}/dotnet4_framework.tar.gz"
  echo "  Сначала: bash export_dotnet4_bundle.sh ~/SasCoreWine /tmp/out (на Astra с dotnet40)"
  exit 1
}

mkdir -p "${ROOT}/files/_build_tmp" "$INSTALL"
rm -rf "$WORKDIR"
mkdir -p "$WORKDIR/prefix"
tar -xzf "$TAR_IN" -C "$WORKDIR/prefix"
SRC="$WORKDIR/prefix/SAUMIwine"
[[ -d "$SRC/drive_c" ]] || { echo "FAIL: структура tar"; exit 1; }

P="$WORKDIR/prefix/SasCoreWine"
mv "$SRC" "$P"
rm -rf "$P/drive_c/SAUMI" "$P/startSAUMI.sh" 2>/dev/null || true
find "$WORKDIR/prefix" \( -name '._*' -o -name '.DS_Store' \) -delete 2>/dev/null || true

mkdir -p "$P/bin" "$P/logs" "$P/drive_c/SaumiAppServer"
(cd "$SAS/payload" && tar cf - .) | (cd "$P/drive_c/SaumiAppServer" && tar xf -)
rm -rf "$P/bin"/*
for s in install_sascore.sh start_sascore.sh stop_sascore.sh probe_sascore.sh oracle_settings.sh oracle_settings.cs; do
  cp -f "$SAS/$s" "$P/bin/" && chmod 755 "$P/bin/$s"
done
echo "bake-sascore $(date -u +%Y-%m-%dT%H:%M:%SZ)" >>"$P/BUILD_ID.txt"

echo "=== offline .NET 4 ==="
bash "${SCRIPTS}/install_dotnet4_offline.sh" "$P" "$BUNDLE"

export COPYFILE_DISABLE=1
find "$WORKDIR/prefix" \( -name '._*' -o -name '.DS_Store' \) -delete 2>/dev/null || true
rm -f "$TAR_OUT"
(cd "$WORKDIR/prefix" && tar --format=ustar --uid=0 --gid=0 -czf "$TAR_OUT" SasCoreWine)
rm -rf "$WORKDIR"
bash "${ROOT}/files/scripts/sync_release.sh" 2>/dev/null || true
echo "=== OK: $TAR_OUT (.NET 4 offline) ==="
