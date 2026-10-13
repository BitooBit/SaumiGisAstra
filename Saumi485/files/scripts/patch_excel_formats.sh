#!/usr/bin/env bash
# Поставить ГОТОВУЮ SaumiExcelProxy.dll (собрана net35 на Mac).
# На АРМ wine vbc = .NET 2.0 и падает на If()/лямбдах — больше не компилируем здесь.
#
#   cd /var/install/saumi485/pathes/6
#   SAUMI_PREFIX=$HOME/Saumi bash patch_excel_formats.sh
#
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PREFIX="${SAUMI_PREFIX:-${HOME}/Saumi}"
DLL=""
for d in "${SCRIPT_DIR}/excel_proxy" \
         "${SCRIPT_DIR}" \
         "${SCRIPT_DIR}/../src/excel_proxy"; do
  if [[ -f "${d}/SaumiExcelProxy.dll" ]]; then
    DLL="${d}/SaumiExcelProxy.dll"
    break
  fi
done
[[ -n "$DLL" ]] || { echo "FAIL: нет SaumiExcelProxy.dll в патче (должна быть собрана заранее)"; exit 1; }
[[ -f "${PREFIX}/startSAUMI.sh" ]] || { echo "FAIL: нет prefix"; exit 1; }

DEST="${PREFIX}/drive_c/SAUMI/SaumiExcelProxy.dll"
[[ -f "$DEST" ]] || { echo "FAIL: нет $DEST — Excel-прокси не из этого prefix"; exit 1; }

mkdir -p "${PREFIX}/logs"
stamp="$(date +%Y%m%d_%H%M%S)"
WINESERVER="${PREFIX}/runtime/wine-11.0/bin/wineserver"
if [[ -x "$WINESERVER" ]]; then
  env WINEPREFIX="$PREFIX" WINEARCH=win32 "$WINESERVER" -k >/dev/null 2>&1 || true
  sleep 1
fi

cp -f "$DEST" "${PREFIX}/logs/SaumiExcelProxy.dll.bak_${stamp}"
cp -f "$DLL" "$DEST"
chmod 644 "$DEST"
echo "OK: SaumiExcelProxy.dll $(wc -c < "$DEST") bytes (Xlsx.23, даты dd.mm.yyyy, колонки)"
echo "    bak ${PREFIX}/logs/SaumiExcelProxy.dll.bak_${stamp}"
echo "Проверка: выгрузка «Платежи» / акт сверки — даты без 00:00:00."
