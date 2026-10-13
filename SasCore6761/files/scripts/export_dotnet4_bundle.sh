#!/usr/bin/env bash
# Экспорт offline-бандла .NET 4 из Wine-prefix с установленным dotnet40.
# Запуск на Astra (или любой Linux) где prefix уже содержит v4.0.30319:
#   bash export_dotnet4_bundle.sh ~/SasCoreWine /tmp/dotnet4_out
#
# Результат (аналог dotnet_from1710 для .NET 3.5):
#   dotnet4_framework.tar.gz   — Framework/v4.0.30319 + system32 CLR
#   dotnet4_assembly.tar.gz    — NativeImages_v4 + pubpol4.dat
#   dotnet4_registry.reg       — ключи реестра v4
#   dotnet4_installer/           — dotNetFx40_Full_x86_x64.exe (если есть в winetricks cache)
set -euo pipefail

PREFIX="${1:?usage: export_dotnet4_bundle.sh WINEPREFIX OUTDIR}"
OUT="${2:?usage: export_dotnet4_bundle.sh WINEPREFIX OUTDIR}"
ROOT="$(cd "$(dirname "$0")" && pwd)"

[[ -d "${PREFIX}/drive_c/windows/Microsoft.NET/Framework/v4.0.30319" ]] \
  || { echo "FAIL: нет v4.0.30319 в ${PREFIX}"; exit 1; }
[[ -f "${PREFIX}/drive_c/windows/Microsoft.NET/Framework/v4.0.30319/mscorlib.dll" ]] \
  || { echo "FAIL: нет mscorlib.dll"; exit 1; }

mkdir -p "${OUT}/dotnet4_installer"
WORKDIR="$(mktemp -d)"
trap 'rm -rf "$WORKDIR"' EXIT

echo "=== export .NET 4 from ${PREFIX} ==="

# --- framework + system32 CLR ---
FW="${WORKDIR}/framework/windows"
mkdir -p "${FW}/Microsoft.NET/Framework" "${FW}/system32"
cp -a "${PREFIX}/drive_c/windows/Microsoft.NET/Framework/v4.0.30319" \
  "${FW}/Microsoft.NET/Framework/"
if [[ -d "${PREFIX}/drive_c/windows/Microsoft.NET/assembly" ]]; then
  mkdir -p "${FW}/Microsoft.NET"
  cp -a "${PREFIX}/drive_c/windows/Microsoft.NET/assembly" "${FW}/Microsoft.NET/"
fi
for dll in mscoree.dll mscorees.dll mscorier.dll mscories.dll MSVCR100_CLR0400.dll; do
  [[ -f "${PREFIX}/drive_c/windows/system32/${dll}" ]] \
    && cp -a "${PREFIX}/drive_c/windows/system32/${dll}" "${FW}/system32/"
done
tar -czf "${OUT}/dotnet4_framework.tar.gz" -C "${WORKDIR}/framework" windows
echo "  dotnet4_framework.tar.gz ($(du -h "${OUT}/dotnet4_framework.tar.gz" | awk '{print $1}'))"

# --- assembly ---
ASM="${WORKDIR}/assembly/windows/assembly"
mkdir -p "${ASM}"
[[ -d "${PREFIX}/drive_c/windows/assembly/NativeImages_v4.0.30319_32" ]] \
  && cp -a "${PREFIX}/drive_c/windows/assembly/NativeImages_v4.0.30319_32" "${ASM}/"
[[ -f "${PREFIX}/drive_c/windows/assembly/pubpol4.dat" ]] \
  && cp -a "${PREFIX}/drive_c/windows/assembly/pubpol4.dat" "${ASM}/"
tar -czf "${OUT}/dotnet4_assembly.tar.gz" -C "${WORKDIR}/assembly" windows
echo "  dotnet4_assembly.tar.gz ($(du -h "${OUT}/dotnet4_assembly.tar.gz" | awk '{print $1}'))"

# --- registry ---
python3 "${ROOT}/extract_dotnet4_registry.py" \
  "${PREFIX}/system.reg" "${OUT}/dotnet4_registry.reg"
echo "  dotnet4_registry.reg ($(du -h "${OUT}/dotnet4_registry.reg" | awk '{print $1}'))"

# --- installer (reference / one-time source) ---
if [[ -f "${HOME}/.cache/winetricks/dotnet40/dotNetFx40_Full_x86_x64.exe" ]]; then
  cp -f "${HOME}/.cache/winetricks/dotnet40/dotNetFx40_Full_x86_x64.exe" \
    "${OUT}/dotnet4_installer/"
  echo "  dotnet4_installer/dotNetFx40_Full_x86_x64.exe"
fi

echo "=== OK: ${OUT} ==="
sha256sum "${OUT}"/dotnet4_*.tar.gz "${OUT}"/dotnet4_registry.reg 2>/dev/null | tee "${OUT}/dotnet4.sha256"
