#!/usr/bin/env bash
# Один запуск. SAUMI закрыть.
#   cd /var/install/saumi485/pathes/6
#   SAUMI_PREFIX=$HOME/Saumi bash apply.sh
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
export SAUMI_PREFIX="${SAUMI_PREFIX:-${HOME}/Saumi}"
[[ -f "${SAUMI_PREFIX}/startSAUMI.sh" ]] || { echo "FAIL: нет ${SAUMI_PREFIX}/startSAUMI.sh"; exit 1; }

echo "======== pathes/6 apply prefix=$SAUMI_PREFIX ========"
echo "1/4 document handlers (иконки P/W/X, TMP)"
bash "${SCRIPT_DIR}/patch_document_handlers.sh"
echo
echo "2/4 wine syscolors (выделение navy, не cyan; Oracle не трогаем)"
bash "${SCRIPT_DIR}/patch_wine_syscolors.sh"
echo
echo "3/4 excel (готовая DLL, без vbc)"
bash "${SCRIPT_DIR}/patch_excel_formats.sh"
echo
echo "4/4 scanner (hp_sane.ds, gphoto2 off)"
bash "${SCRIPT_DIR}/patch_scanner.sh"
echo
echo "======== OK pathes/6 ========"
echo "Дальше: ярлык SAUMI."
echo "  Иконки: красная P / синяя W / зелёная X у типа документа."
echo "  Сканер: Выбрать источник → HP LaserJet M425dw → OK, потом Сканировать."
echo "  Word «не установлен» — не трогаем."
echo "  Excel: выгрузка без 00:00:00."
