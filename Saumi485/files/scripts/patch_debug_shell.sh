#!/usr/bin/env bash
# Включает +shell,+process в лог startSAUMI (один прогон для иконки/просмотра).
# SAUMI закрыть. После теста: отдай ~/Saumi/logs/saumi_latest.log
#
#   cd /var/install/saumi485/pathes/3
#   SAUMI_PREFIX=$HOME/Saumi bash patch_debug_shell.sh
#
set -euo pipefail
PREFIX="${SAUMI_PREFIX:-${HOME}/Saumi}"
START="${PREFIX}/startSAUMI.sh"
[[ -f "$START" ]] || { echo "FAIL: нет $START"; exit 1; }

MARKER="saumi-pathes3-shell-debug"
if grep -q "$MARKER" "$START"; then
  echo "OK: +shell уже в $START"
  echo "Запусти SAUMI с ярлыка, открой свойства документа и «Просмотр документа», закрой."
  echo "Лог: ${PREFIX}/logs/saumi_latest.log"
  exit 0
fi

stamp="$(date +%Y%m%d_%H%M%S)"
cp -f "$START" "${START}.bak_before_shelldebug_${stamp}"
tmp="$(mktemp)"
awk -v m="$MARKER" '
  { print }
  $0 ~ /export WINEDEBUG=/ {
    print "# " m
    print "export WINEDEBUG=\"${WINEDEBUG},+shell,+process\""
  }
' "$START" >"$tmp"
mv -f "$tmp" "$START"
chmod 755 "$START"
grep -n "$MARKER\|WINEDEBUG" "$START" | head -n 15
echo "OK: +shell,+process в startSAUMI (бэкап ${START}.bak_before_shelldebug_${stamp})"
echo "Запусти SAUMI с ярлыка → карточка документа → Просмотр. Закрой. Отдай saumi_latest.log"
