#!/usr/bin/env bash
# Сканер в SAUMI: не gphoto2 (камера, 40с таймаут), не 32-bit Wine SANE.
# Свой TWAIN DS «HP LaserJet M425dw» + демон scanimage на хосте (64-bit hpaio).
#
#   cd /var/install/saumi485/pathes/17
#   SAUMI_PREFIX=$HOME/Saumi bash patch_scanner.sh
#
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PREFIX="${SAUMI_PREFIX:-${HOME}/Saumi}"
[[ -f "${PREFIX}/startSAUMI.sh" ]] || { echo "FAIL: нет prefix"; exit 1; }

DS=""
for f in "${SCRIPT_DIR}/hp_sane.ds" \
         "${SCRIPT_DIR}/twain/hp_sane.ds" \
         "${SCRIPT_DIR}/../src/twain_ds/hp_sane.ds"; do
  [[ -f "$f" ]] && DS="$f" && break
done
[[ -n "$DS" ]] || { echo "FAIL: нет hp_sane.ds — патч собран без TWAIN DLL"; exit 1; }

DAEMON=""
for f in "${SCRIPT_DIR}/saumi_scan_daemon.sh" \
         "${SCRIPT_DIR}/../scripts/saumi_scan_daemon.sh"; do
  [[ -f "$f" ]] && DAEMON="$f" && break
done
[[ -n "$DAEMON" ]] || { echo "FAIL: нет saumi_scan_daemon.sh"; exit 1; }

PNM=""
for f in "${SCRIPT_DIR}/pnm_to_dib.py" \
         "${SCRIPT_DIR}/../src/twain_ds/pnm_to_dib.py"; do
  [[ -f "$f" ]] && PNM="$f" && break
done
[[ -n "$PNM" ]] || { echo "FAIL: нет pnm_to_dib.py"; exit 1; }

SCAN=""
for f in "${SCRIPT_DIR}/saumi_scan.sh" \
         "${SCRIPT_DIR}/../scripts/saumi_scan.sh"; do
  [[ -f "$f" ]] && SCAN="$f" && break
done

WINESERVER="${PREFIX}/runtime/wine-11.0/bin/wineserver"
if [[ -x "$WINESERVER" ]]; then
  env WINEPREFIX="$PREFIX" WINEARCH=win32 "$WINESERVER" -k >/dev/null 2>&1 || true
  sleep 1
fi

mkdir -p \
  "${PREFIX}/bin" \
  "${PREFIX}/logs" \
  "${PREFIX}/config" \
  "${PREFIX}/drive_c/SAUMI/twain" \
  "${PREFIX}/drive_c/SAUMI/temp/twain" \
  "${PREFIX}/drive_c/windows/twain_32"

stamp="$(date +%Y%m%d_%H%M%S)"
T32="${PREFIX}/drive_c/windows/twain_32"

# Wine TWAIN перечисляет ВСЕ *.ds. gphoto2 — «gphoto2 camera» + 40с autodetection.
# sane.ds MSG_GET падает (32-bit SANE без hpaio) и тормозит меню.
for f in gphoto2.ds sane.ds; do
  if [[ -f "${T32}/${f}" ]]; then
    cp -f "${T32}/${f}" "${PREFIX}/logs/${f}.bak_${stamp}"
    mv -f "${T32}/${f}" "${T32}/${f}.off"
    echo "OK: ${f} → ${f}.off"
  else
    echo "OK: ${f} уже нет в twain_32"
  fi
done

cp -f "$DS" "${PREFIX}/drive_c/SAUMI/twain/hp_sane.ds"
cp -f "$DS" "${T32}/hp_sane.ds"
chmod 644 "${T32}/hp_sane.ds"
echo "OK: TWAIN DS → ${T32}/hp_sane.ds ($(wc -c < "$DS") bytes)"

# SAUMI вызывает TWAIN_AcquireNative в Eztwain3.dll — подменяем как Excel-прокси.
PROXY=""
for f in "${SCRIPT_DIR}/Eztwain3.dll" \
         "${SCRIPT_DIR}/../src/eztwain_proxy/Eztwain3.dll"; do
  [[ -f "$f" ]] && PROXY="$f" && break
done
EZ="${PREFIX}/drive_c/SAUMI/Eztwain3.dll"
ORIG="${PREFIX}/drive_c/SAUMI/Eztwain3_orig.dll"
if [[ -n "$PROXY" ]]; then
  if [[ ! -f "$ORIG" ]]; then
    if [[ -f "$EZ" ]] && [[ "$(wc -c < "$EZ")" -gt 100000 ]]; then
      cp -f "$EZ" "$ORIG"
      echo "OK: оригинал Eztwain3.dll → Eztwain3_orig.dll ($(wc -c < "$ORIG") bytes)"
    else
      echo "FAIL: нет оригинального Eztwain3.dll (>100К) для прокси"
      exit 1
    fi
  fi
  cp -f "$PROXY" "$EZ"
  chmod 644 "$EZ"
  echo "OK: EZTwain proxy → $EZ ($(wc -c < "$EZ") bytes)"
else
  echo "WARN: нет Eztwain3.dll прокси в патче"
fi

cp -f "$DAEMON" "${PREFIX}/bin/saumi_scan_daemon.sh"
cp -f "$PNM" "${PREFIX}/bin/pnm_to_dib.py"
chmod 755 "${PREFIX}/bin/saumi_scan_daemon.sh"
[[ -n "$SCAN" ]] && cp -f "$SCAN" "${PREFIX}/bin/saumi_scan.sh" && chmod 755 "${PREFIX}/bin/saumi_scan.sh"

# Старый демон крутит старый pnm_to_dib (P4 → тишина). Всегда перезапуск.
if [[ -f "${PREFIX}/logs/scan_daemon.pid" ]]; then
  oldpid="$(cat "${PREFIX}/logs/scan_daemon.pid" 2>/dev/null || true)"
  if [[ -n "${oldpid:-}" ]]; then
    kill "$oldpid" 2>/dev/null || true
    sleep 0.3
  fi
  rm -f "${PREFIX}/logs/scan_daemon.pid"
  echo "OK: старый scan daemon остановлен"
fi

# Запомнить HP сейчас (не из Wine).
if [[ -n "$SCAN" ]]; then
  if d="$(SAUMI_PREFIX="$PREFIX" bash "$SCAN" pick)"; then
    echo "OK: DEVICE=$d"
    cat "${PREFIX}/config/scanner.conf"
  else
    echo "WARN: scanimage -L пуст сейчас; демон попробует ещё раз при скане"
  fi
fi

# Хук в startSAUMI: каждый запуск прячет gphoto2 и поднимает демон.
START="${PREFIX}/startSAUMI.sh"
MARKER="saumi-pathes6-twain"
RUNTIME_SRC=""
for f in "${SCRIPT_DIR}/saumi_twain_runtime.sh" \
         "${SCRIPT_DIR}/../scripts/saumi_twain_runtime.sh"; do
  [[ -f "$f" ]] && RUNTIME_SRC="$f" && break
done
[[ -n "$RUNTIME_SRC" ]] || { echo "FAIL: нет saumi_twain_runtime.sh"; exit 1; }
cp -f "$RUNTIME_SRC" "${PREFIX}/bin/saumi_twain_runtime.sh"
chmod 755 "${PREFIX}/bin/saumi_twain_runtime.sh"

if ! grep -q "$MARKER" "$START"; then
  cp -f "$START" "${START}.bak_before_twain_${stamp}"
  tmp="$(mktemp)"
  awk -v m="$MARKER" -v pfx="$PREFIX" '
    { print }
    /export TNS_ADMIN=/ && !done {
      print "# " m
      print "SAUMI_PREFIX=\"" pfx "\" bash \"" pfx "/bin/saumi_twain_runtime.sh\" || true"
      done=1
    }
  ' "$START" >"$tmp"
  mv -f "$tmp" "$START"
  chmod 755 "$START"
  echo "OK: хук TWAIN в startSAUMI.sh"
else
  echo "OK: хук TWAIN уже в startSAUMI.sh"
fi

SAUMI_PREFIX="$PREFIX" bash "${PREFIX}/bin/saumi_twain_runtime.sh"
echo "OK: twain runtime применён сейчас"

echo
echo "В SAUMI: новый документ → квадрат у номера → «Выбрать источник сканирования»"
echo "  должен быть один пункт: HP LaserJet M425dw  → OK"
echo "  затем «Сканировать…» (бумага в HP). Word/OLE не трогаем."
