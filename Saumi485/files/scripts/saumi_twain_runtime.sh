#!/usr/bin/env bash
# Вызывается из startSAUMI.sh каждый запуск: gphoto2/sane.ds не должны вернуться
# после wineboot. Демон scanimage должен быть жив.
set -euo pipefail
PREFIX="${SAUMI_PREFIX:-${HOME}/Saumi}"
T32="${PREFIX}/drive_c/windows/twain_32"
S32="${PREFIX}/drive_c/windows/system32"
mkdir -p "$T32" "${PREFIX}/drive_c/SAUMI/temp/twain" "${PREFIX}/logs" "${PREFIX}/bin"
for dir in "$T32" "$S32"; do
  mv -f "${dir}/gphoto2.ds" "${dir}/gphoto2.ds.off" 2>/dev/null || true
  mv -f "${dir}/sane.ds" "${dir}/sane.ds.off" 2>/dev/null || true
done
if [[ -f "${PREFIX}/drive_c/SAUMI/twain/hp_sane.ds" ]]; then
  cp -f "${PREFIX}/drive_c/SAUMI/twain/hp_sane.ds" "${T32}/hp_sane.ds"
fi
pidf="${PREFIX}/logs/scan_daemon.pid"
alive=0
if [[ -f "$pidf" ]]; then
  pid="$(cat "$pidf" 2>/dev/null || true)"
  if [[ -n "${pid:-}" ]] && kill -0 "$pid" 2>/dev/null; then
    alive=1
  fi
fi
if [[ "$alive" -eq 0 && -x "${PREFIX}/bin/saumi_scan_daemon.sh" ]]; then
  SAUMI_PREFIX="$PREFIX" nohup bash "${PREFIX}/bin/saumi_scan_daemon.sh" >/dev/null 2>&1 &
  echo $! >"$pidf"
fi
