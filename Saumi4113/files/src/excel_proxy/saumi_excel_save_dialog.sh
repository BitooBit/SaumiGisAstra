#!/bin/bash
# Linux Save-диалог для SAUMI Excel (обход сломанного Wine BrowseObject).
# Аргументы:
#   $1 = полный путь-подсказка (unix), напр. .../export/saumi_export_....xlsx
#   $2 = файл куда записать результат (unix)
# Результат в $2:
#   CANCEL — пользователь отменил
#   FAIL:<code> — ошибка
#   /абсолютный/путь.xlsx — выбранный файл
set -euo pipefail
SUGGESTED="${1:?}"
OUTFILE="${2:?}"
mkdir -p "$(dirname "$SUGGESTED")" "$(dirname "$OUTFILE")" 2>/dev/null || true

if ! command -v zenity >/dev/null 2>&1 && [[ ! -x /usr/bin/zenity ]]; then
  if command -v kdialog >/dev/null 2>&1; then
    RES=$(kdialog --getsavefilename "$SUGGESTED" "*.xlsx" 2>/dev/null) || {
      ec=$?
      if [[ $ec -eq 1 ]]; then echo CANCEL >"$OUTFILE"; exit 0; fi
      echo "FAIL:$ec" >"$OUTFILE"; exit 0
    }
    [[ "$RES" == *.xlsx ]] || RES="${RES}.xlsx"
    echo "$RES" >"$OUTFILE"
    exit 0
  fi
  echo "FAIL:no-zenity" >"$OUTFILE"
  exit 0
fi

ZENITY="$(command -v zenity 2>/dev/null || echo /usr/bin/zenity)"
set +e
RES=$("$ZENITY" --file-selection --save --confirm-overwrite \
  --title="Сохранить Excel" \
  --filename="$SUGGESTED" 2>/dev/null)
ec=$?
set -e
if [[ $ec -eq 1 ]]; then
  echo CANCEL >"$OUTFILE"
  exit 0
fi
if [[ $ec -ne 0 ]]; then
  echo "FAIL:$ec" >"$OUTFILE"
  exit 0
fi
if [[ -z "${RES:-}" ]]; then
  echo CANCEL >"$OUTFILE"
  exit 0
fi
[[ "$RES" == *.xlsx ]] || [[ "$RES" == *.XLSX ]] || RES="${RES}.xlsx"
echo "$RES" >"$OUTFILE"
exit 0
