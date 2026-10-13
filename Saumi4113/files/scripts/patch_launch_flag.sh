#!/usr/bin/env bash
# Микропатч ПОСЛЕ полной установки: сменить флаг запуска mmp32 в startSAUMI.sh.
# Установщик и golden tar не трогает. Ярлык вызывает startSAUMI.sh.
#
# AdminGuide §3.1.1:
#   none          — без ключа (новый UpdateService / проверка по БД)
#   oldupdate     — старый cover.exe (рабочий у нас)
#   updated       — подавить локальное обновление (на komp-05 ломал вход)
#   localupdate   — принудительно обновлять даже из копии каталога
#
# Примеры:
#   bash patch_launch_flag.sh localupdate
#   bash patch_launch_flag.sh none
#   bash patch_launch_flag.sh oldupdate
#   SAUMI_PREFIX=/home/testsaum/Saumi bash patch_launch_flag.sh none
#
set -euo pipefail

FLAG_RAW="${1:-}"
if [[ -z "$FLAG_RAW" ]]; then
  echo "Usage: $0 {none|oldupdate|updated|localupdate}"
  echo "  none        → wine mmp32.exe"
  echo "  oldupdate   → wine mmp32.exe /oldupdate"
  echo "  updated     → wine mmp32.exe /updated"
  echo "  localupdate → wine mmp32.exe /localupdate"
  exit 1
fi

case "$FLAG_RAW" in
  none|off|bare) TARGET="" ;;
  oldupdate|/oldupdate) TARGET="/oldupdate" ;;
  updated|/updated) TARGET="/updated" ;;
  localupdate|/localupdate) TARGET="/localupdate" ;;
  *)
    echo "FAIL: неизвестный флаг '$FLAG_RAW' (none|oldupdate|updated|localupdate)"
    exit 1
    ;;
esac

PREFIX="${SAUMI_PREFIX:-${HOME}/Saumi}"
START="${PREFIX}/startSAUMI.sh"

[[ -f "$START" ]] || { echo "FAIL: нет $START — сначала полная установка"; exit 1; }

if ! grep -q 'mmp32\.exe' "$START"; then
  echo "FAIL: в $START нет строки запуска mmp32.exe"
  exit 1
fi

stamp="$(date +%Y%m%d_%H%M%S)"
cp -f "$START" "${START}.bak_before_flag_${stamp}"

replacement="mmp32.exe"
[[ -n "$TARGET" ]] && replacement="mmp32.exe ${TARGET}"

tmp_edit="$(mktemp)"
# Снять любой известный флаг, потом поставить нужный (portable: без look-ahead).
sed \
  -e 's|mmp32\.exe /oldupdate|mmp32.exe|g' \
  -e 's|mmp32\.exe /updated|mmp32.exe|g' \
  -e 's|mmp32\.exe /localupdate|mmp32.exe|g' \
  -e "s|mmp32\\.exe|${replacement}|g" \
  "$START" >"$tmp_edit"
mv -f "$tmp_edit" "$START"
chmod 755 "$START"

if ! grep -qF "$replacement" "$START"; then
  echo "FAIL: после патча нет «${replacement}» в $START"
  exit 1
fi

if [[ -n "$TARGET" ]]; then
  for other in /oldupdate /updated /localupdate; do
    [[ "$other" == "$TARGET" ]] && continue
    if grep -qF "mmp32.exe ${other}" "$START"; then
      echo "FAIL: остался чужой флаг ${other}"
      exit 1
    fi
  done
else
  if grep -qE 'mmp32\.exe /(oldupdate|updated|localupdate)' "$START"; then
    echo "FAIL: флаги всё ещё есть"
    exit 1
  fi
fi

disp="${TARGET:-«без флага»}"
grep -n 'mmp32\.exe' "$START" | head -5
echo "OK: startSAUMI → ${replacement}"
echo "    prefix=$PREFIX  flag=${disp}"
echo "    бэкап: ${START}.bak_before_flag_${stamp}"
echo "Перезапусти SAUMI с ярлыка."
