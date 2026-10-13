#!/usr/bin/env bash
# Offline .NET 4 в Wine-prefix при bake (без winetricks).
# Кладём файлы туда же, куда их кладёт настоящий dotnet40:
#   Framework\v4.0.30319
#   system32\mscoree.dll + MSVCR100_CLR0400.dll
#   Microsoft.NET\assembly\GAC_MSIL|GAC_32  (IL, реальный PublicKeyToken)
#   NativeImages_v4 + pubpol4.dat
#   system.reg: CLSID v4 + NDP\v4\Full
#   user.reg: mscoree/mscorwks/fusion=native
#
# Usage: install_dotnet4_offline.sh PREFIX [BUNDLE_DIR]
set -euo pipefail
export COPYFILE_DISABLE=1

P="${1:?usage: install_dotnet4_offline.sh PREFIX [BUNDLE_DIR]}"
BUNDLE="${2:-$(cd "$(dirname "$0")/../files" && pwd)}"
SCRIPTS="$(cd "$(dirname "$0")" && pwd)"
SAS="${BUNDLE}/sascore"

NET4="${P}/drive_c/windows/Microsoft.NET/Framework/v4.0.30319"
SYS32="${P}/drive_c/windows/system32"
GAC_MSIL="${P}/drive_c/windows/Microsoft.NET/assembly/GAC_MSIL"
GAC_32="${P}/drive_c/windows/Microsoft.NET/assembly/GAC_32"
FW="${BUNDLE}/dotnet4_framework.tar.gz"
ASM="${BUNDLE}/dotnet4_assembly.tar.gz"
REG="${BUNDLE}/dotnet4_registry.reg"
CRT="${SAS}/MSVCR100_CLR0400.dll"
NDP_SNIPPET="${SAS}/ndp_v4_full.reg"
GAC_PY="${SCRIPTS}/gac_install_net4.py"

net4_files_ok() { [[ -f "${NET4}/mscorlib.dll" && -f "${NET4}/clr.dll" ]]; }

ensure_crt() {
  [[ -f "$CRT" ]] || { echo "FAIL: нет ${CRT}"; exit 1; }
  mkdir -p "$NET4" "$SYS32"
  cp -f "$CRT" "${NET4}/MSVCR100_CLR0400.dll"
  cp -f "$CRT" "${SYS32}/MSVCR100_CLR0400.dll"
  chmod 644 "${NET4}/MSVCR100_CLR0400.dll" "${SYS32}/MSVCR100_CLR0400.dll"
  echo "  MSVCR100_CLR0400.dll → Framework\\v4.0.30319 и system32"
}

ensure_gac() {
  [[ -f "$GAC_PY" ]] || { echo "FAIL: нет ${GAC_PY}"; exit 1; }
  python3 "$GAC_PY" --framework "$NET4" --gac "$GAC_MSIL" --gac32 "$GAC_32"
  local sysgac="${GAC_MSIL}/System/v4.0_4.0.0.0__b77a5c561934e089/System.dll"
  [[ -f "$sysgac" ]] || { echo "FAIL: нет ${sysgac}"; exit 1; }
  echo "  GAC_MSIL System.dll token=b77a5c561934e089"
}

ensure_ndp_v4() {
  local reg="${P}/system.reg"
  [[ -f "$reg" && -f "$NDP_SNIPPET" ]] || { echo "FAIL: нет system.reg или ndp_v4_full.reg"; exit 1; }
  if grep -q 'NET Framework Setup\\\\NDP\\\\v4\\\\Full' "$reg"; then
    echo "  NDP v4 Full уже в system.reg"
    return 0
  fi
  echo "  NDP v4 Full → system.reg"
  printf '\n' >>"$reg"
  cat "$NDP_SNIPPET" >>"$reg"
}

ensure_native_clr_overrides() {
  local ureg="${P}/user.reg"
  [[ -f "$ureg" ]] || return 0
  if ! grep -q '"\*mscoree"="native"' "$ureg"; then
    echo "  DllOverrides mscoree=native → user.reg"
    cat >>"$ureg" <<'EOF'

[Software\\Wine\\DllOverrides] 0
#time=0
"*mscoree"="native"
"mscoree"="native"
"*mscorwks"="native"
"mscorwks"="native"
"*fusion"="native"
"fusion"="native"
EOF
  fi
}

if ! net4_files_ok; then
  for f in "$FW" "$ASM" "$REG"; do
    [[ -f "$f" ]] || { echo "FAIL: нет offline-бандла $f"; exit 1; }
  done
  echo "  распаковка dotnet4_framework..."
  tar --warning=no-unknown-keyword -xzf "$FW" -C "${P}/drive_c/" 2>/dev/null \
    || tar -xzf "$FW" -C "${P}/drive_c/"
  echo "  распаковка dotnet4_assembly..."
  tar --warning=no-unknown-keyword -xzf "$ASM" -C "${P}/drive_c/windows/" 2>/dev/null \
    || tar -xzf "$ASM" -C "${P}/drive_c/windows/"
  mkdir -p "${P}/logs"
  cp -f "${P}/system.reg" "${P}/logs/system.reg.before_dotnet4"
  python3 "${SCRIPTS}/merge_dotnet4_registry.py" \
    "$REG" "${P}/system.reg" "${P}/logs/system.reg.before_dotnet4"
  grep -qx 'dotnet40' "${P}/winetricks.log" 2>/dev/null || echo 'dotnet40' >>"${P}/winetricks.log"
fi

net4_files_ok || { echo "FAIL: нет ${NET4}/mscorlib.dll"; exit 1; }
# mscoreei.dll только в Framework — копия в system32 ломает native CLR
rm -f "${SYS32}/mscoreei.dll"

ensure_crt
ensure_gac
ensure_ndp_v4
ensure_native_clr_overrides
echo "  OK .NET 4 offline (Framework + GAC + NDP + CRT)"
