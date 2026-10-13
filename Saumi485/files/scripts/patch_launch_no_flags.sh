#!/usr/bin/env bash
# Микропатч ПОСЛЕ полной установки: ярлык → mmp32.exe БЕЗ /oldupdate (и без других флагов).
# Установщик не меняет. Гипотеза: обычный запуск без флагов обновления.
#
# На клиенте (после sudo bash instalSaumi.sh):
#   bash patch_launch_no_flags.sh
# или для чужого prefix:
#   SAUMI_PREFIX=/home/user/Saumi bash patch_launch_no_flags.sh
#
set -euo pipefail

PREFIX="${SAUMI_PREFIX:-${HOME}/Saumi}"
START="${PREFIX}/startSAUMI.sh"

[[ -f "$START" ]] || { echo "FAIL: нет $START — сначала полная установка"; exit 1; }

if ! grep -qE 'mmp32\.exe( /oldupdate| /updated)?' "$START"; then
  echo "FAIL: в $START нет строки запуска mmp32.exe"
  exit 1
fi

cp -f "$START" "${START}.bak_before_noflags_$(date +%Y%m%d_%H%M%S)"

# Только флаги запуска: /oldupdate или /updated → без флагов. Остальное не трогаем.
# portable sed (Astra GNU / macBSD)
tmp_edit="$(mktemp)"
sed -e 's|mmp32\.exe /oldupdate|mmp32.exe|g' -e 's|mmp32\.exe /updated|mmp32.exe|g' "$START" >"$tmp_edit"
mv -f "$tmp_edit" "$START"
chmod 755 "$START"

if grep -qE 'mmp32\.exe /(oldupdate|updated)' "$START"; then
  echo "FAIL: флаги всё ещё есть в $START"
  exit 1
fi

grep -n 'mmp32\.exe' "$START" | head -5
echo "OK: ярлык/startSAUMI → ${WINE:-wine} mmp32.exe (без флагов)"
echo "    prefix=$PREFIX"
echo "    бэкап рядом с startSAUMI.sh (*.bak_before_noflags_*)"
echo "Перезапусти SAUMI с ярлыка."
