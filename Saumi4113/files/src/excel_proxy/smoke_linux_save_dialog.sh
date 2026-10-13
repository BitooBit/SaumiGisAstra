#!/usr/bin/env bash
# Автоsmoke Linux Save-диалога на Astra (без SAUMI UI).
#   bash smoke_linux_save_dialog.sh              # fake (без GUI) — обязательный PASS
#   bash smoke_linux_save_dialog.sh --gui        # zenity+xdotool под xvfb
set -euo pipefail
GUI=0
[[ "${1:-}" == "--gui" ]] && GUI=1

PFX="${WINEPREFIX:-$HOME/SAUMIwine}"
WINE="${PFX}/runtime/wine-11.0/bin/wine"
[[ -x "$WINE" ]] || WINE="$(command -v wine)"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
CHOOSER="${SCRIPT_DIR}/saumi_excel_save_dialog.sh"
SMOKE_EXE="${SCRIPT_DIR}/ExcelLinuxSaveSmoke.exe"
LOGDIR="${PFX}/logs"
mkdir -p "${PFX}/drive_c/SAUMI/export" "$LOGDIR" "${PFX}/bin"

cp -f "$CHOOSER" /tmp/saumi_excel_save_dialog.sh
cp -f "$CHOOSER" "${PFX}/bin/saumi_excel_save_dialog.sh"
chmod +x /tmp/saumi_excel_save_dialog.sh "${PFX}/bin/saumi_excel_save_dialog.sh"

echo "=== FAKE path IPC (no GUI) ==="
export WINEPREFIX="$PFX" WINEARCH=win32 SAUMI_SAVE_SMOKE_FAKE=1
# compile on host if exe missing and vbc available
if [[ ! -f "$SMOKE_EXE" ]]; then
  VBC="${PFX}/drive_c/windows/Microsoft.NET/Framework/v2.0.50727/vbc.exe"
  if [[ -x "$WINE" && -f "$VBC" ]]; then
    echo "compile smoke with vbc..."
    (cd "$SCRIPT_DIR" && env WINEPREFIX="$PFX" WINEARCH=win32 "$WINE" "$VBC" \
      /nologo /target:exe /out:ExcelLinuxSaveSmoke.exe ExcelLinuxSaveSmoke.vb) || true
  fi
fi
[[ -f "$SMOKE_EXE" ]] || { echo "FAIL: нет ExcelLinuxSaveSmoke.exe — соберите на Mac сначала"; exit 1; }

set +e
timeout 60 env WINEPREFIX="$PFX" WINEARCH=win32 SAUMI_SAVE_SMOKE_FAKE=1 \
  "$WINE" "$SMOKE_EXE" | tee "${LOGDIR}/excel_linux_save_smoke_fake.log"
ec_fake=${PIPESTATUS[0]}
set -e
echo "fake_exit=$ec_fake"
[[ "$ec_fake" -eq 0 ]] || { echo "FAIL fake smoke"; exit 1; }
echo "PASS fake"

if [[ "$GUI" -eq 1 ]]; then
  echo "=== GUI zenity under xvfb + xdotool Return ==="
  command -v xvfb-run >/dev/null || { echo "FAIL: no xvfb-run"; exit 1; }
  command -v xdotool >/dev/null || { echo "FAIL: no xdotool"; exit 1; }
  OUTF="${LOGDIR}/excel_linux_save_gui_out.txt"
  SUG="${PFX}/drive_c/SAUMI/export/gui_smoke.xlsx"
  rm -f "$OUTF"
  xvfb-run -a bash -c '
    set -e
    OUTF="'"$OUTF"'"
    SUG="'"$SUG"'"
    /tmp/saumi_excel_save_dialog.sh "$SUG" "$OUTF" &
    pid=$!
    for i in $(seq 1 30); do
      if xdotool search --name "Сохранить Excel" >/dev/null 2>&1; then
        wid=$(xdotool search --name "Сохранить Excel" | head -1)
        xdotool windowactivate "$wid"
        sleep 0.3
        xdotool key Return
        break
      fi
      sleep 0.2
    done
    wait $pid || true
    echo "gui_out=$(cat "$OUTF" 2>/dev/null || echo EMPTY)"
  ' | tee "${LOGDIR}/excel_linux_save_smoke_gui.log"
  body=$(cat "$OUTF" 2>/dev/null || true)
  echo "gui_result=$body"
  if [[ "$body" == *"gui_smoke.xlsx"* || "$body" == CANCEL || "$body" == /*.xlsx || "$body" == *.xlsx ]]; then
    echo "PASS gui (got dialog result)"
  else
    echo "FAIL gui unexpected: $body"
    exit 1
  fi
fi

echo "=== ALL SMOKE PASS ==="
