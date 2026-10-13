#!/usr/bin/env bash
# Сборка Wine-префикса SAUMI 4.8.5 для Astra Linux SE 1.8.6 (Воронеж)
#
# Запуск на Astra (не на Mac). Нужны файлы в ~/saumi_build_src:
#   SAUMI485.zip, oracle_client_from1710.tar.gz, tnsnames.ora,
#   dotnet_from1710.tar.gz, dotnet_assembly_from1710.tar.gz,
#   dotnet_registry_from1710.reg (или system.reg.old1710 + merge_dotnet_registry.py)
#
# Результат: ~/SAUMI1860wine.tar.gz (базовый prefix, БЕЗ wine-11.0 и БЕЗ патчей).
# После сборки на Mac:
#   bake_into_tar.sh → Excel/EW/hide
#   bake_licence_into_tar.sh → Guardant
# Wine-11.0 в текущий tar добавлялся отдельным шагом (runtime/wine-11.0).
#
set -euo pipefail

BUILD_ID="$(date +%Y%m%d_%H%M%S)"
SRC_DIR="${HOME}/saumi_build_src"
LOG_DIR="${HOME}/saumi_build_logs/${BUILD_ID}"
PREFIX="${HOME}/SAUMIwine"
WINEARCH=win32
export WINEPREFIX="${PREFIX}"
export WINEARCH

mkdir -p "${LOG_DIR}" "${SRC_DIR}" "${PREFIX}/logs"
exec > >(tee -a "${LOG_DIR}/build.log") 2>&1

log() { echo "[$(date '+%F %T')] $*"; }
run() { log "CMD: $*"; "$@"; }

log "=== SAUMI 4.8.5 prefix build start ==="
log "BUILD_ID=${BUILD_ID}"
log "PREFIX=${PREFIX}"
uname -a
wine --version || true
sudo -n /usr/sbin/astra-ptrace-lock status || true
df -h "${HOME}"

if [[ -d "${HOME}/SAUMIwine18" && ! -d "${PREFIX}/drive_c" ]]; then
  log "Renaming ${HOME}/SAUMIwine18 -> ${PREFIX}"
  mv "${HOME}/SAUMIwine18" "${PREFIX}"
fi

if [[ ! -d "${PREFIX}/drive_c" ]]; then
  run env WINEPREFIX="${PREFIX}" WINEARCH="${WINEARCH}" wineboot -u
fi

export WINEPREFIX="${PREFIX}"
touch "${PREFIX}/winetricks.log"

log "=== winetricks: UI/search components ==="
if ! grep -qx 'comctl32' "${PREFIX}/winetricks.log" 2>/dev/null; then
  run xvfb-run -a winetricks -q comctl32 msxml3 msxml6 riched20 corefonts
fi

install_dotnet_from1710() {
  log "=== .NET 3.5 SP1 from old prefix (offline) ==="
  tar --warning=no-unknown-keyword -xzf "${SRC_DIR}/dotnet_from1710.tar.gz" -C "${PREFIX}/drive_c/" 2>/dev/null \
    || tar -xzf "${SRC_DIR}/dotnet_from1710.tar.gz" -C "${PREFIX}/drive_c/"
  if [[ -f "${SRC_DIR}/dotnet_assembly_from1710.tar.gz" ]]; then
    tar --warning=no-unknown-keyword -xzf "${SRC_DIR}/dotnet_assembly_from1710.tar.gz" -C "${PREFIX}/drive_c/windows/" 2>/dev/null \
      || tar -xzf "${SRC_DIR}/dotnet_assembly_from1710.tar.gz" -C "${PREFIX}/drive_c/windows/"
  fi
  if [[ -f "${SRC_DIR}/dotnet_registry_from1710.reg" ]]; then
    cp -f "${PREFIX}/system.reg" "${LOG_DIR}/system.reg.before_dotnet"
    run env WINEDEBUG="-all" wine regedit /S "${SRC_DIR}/dotnet_registry_from1710.reg" || \
      log "WARN: regedit import returned non-zero (may still work)"
  elif [[ -f "${SRC_DIR}/system.reg.old1710" && -f "${SRC_DIR}/merge_dotnet_registry.py" ]]; then
    run python3 "${SRC_DIR}/merge_dotnet_registry.py" \
      "${SRC_DIR}/system.reg.old1710" \
      "${PREFIX}/system.reg" \
      "${LOG_DIR}/system.reg.before_dotnet" || \
      log "WARN: merge_dotnet_registry failed"
  fi
  grep -qx 'remove_mono' "${PREFIX}/winetricks.log" || echo 'remove_mono' >> "${PREFIX}/winetricks.log"
  grep -qx 'dotnet35sp1' "${PREFIX}/winetricks.log" || echo 'dotnet35sp1' >> "${PREFIX}/winetricks.log"
  run env WINEPREFIX="${PREFIX}" WINEARCH="${WINEARCH}" wineboot -u
}

log "=== .NET 3.5 SP1 ==="
if ! grep -qx 'dotnet35sp1' "${PREFIX}/winetricks.log" 2>/dev/null; then
  if [[ -f "${SRC_DIR}/dotnet_from1710.tar.gz" ]]; then
    install_dotnet_from1710
  else
    log "WARN: no offline dotnet bundle, trying winetricks download"
    run xvfb-run -a winetricks -q remove_mono dotnet35sp1 || install_dotnet_from1710
  fi
fi

log "=== SAUMI 4.8.5 client ==="
run unzip -o "${SRC_DIR}/SAUMI485.zip" -d "${PREFIX}/drive_c/"
[[ -f "${PREFIX}/drive_c/SAUMI/mmp32.exe" ]] || { log "ERROR: mmp32.exe missing"; exit 1; }
log "mmp32.exe: $(stat -c%s "${PREFIX}/drive_c/SAUMI/mmp32.exe") bytes"

log "=== Oracle client from 1.7.10 build ==="
tar --warning=no-unknown-keyword -xzf "${SRC_DIR}/oracle_client_from1710.tar.gz" -C "${PREFIX}/drive_c/" 2>/dev/null \
  || tar -xzf "${SRC_DIR}/oracle_client_from1710.tar.gz" -C "${PREFIX}/drive_c/"
run mkdir -p "${PREFIX}/drive_c/oracle/NETWORK/ADMIN"
run cp -f "${SRC_DIR}/tnsnames.ora" "${PREFIX}/drive_c/oracle/NETWORK/ADMIN/tnsnames.ora"
[[ -d "${PREFIX}/drive_c/oracle/oo4o/NETWORK/ADMIN" ]] && \
  run cp -f "${SRC_DIR}/tnsnames.ora" "${PREFIX}/drive_c/oracle/oo4o/NETWORK/ADMIN/tnsnames.ora"

log "=== runtime logging scripts ==="
cat > "${PREFIX}/startSAUMI.sh" <<'LAUNCH'
#!/usr/bin/env bash
set -uo pipefail

export WINEPREFIX="${HOME}/SAUMIwine"
export WINEARCH=win32
export WINE="${WINE:-wine}"

VERBOSE=0
for arg in "$@"; do
  case "$arg" in
    --verbose|-v) VERBOSE=1 ;;
  esac
done

LOG_DIR="${WINEPREFIX}/logs"
mkdir -p "${LOG_DIR}"
RUN_LOG="${LOG_DIR}/saumi_$(date +%Y%m%d_%H%M%S).log"

if [[ "${VERBOSE}" -eq 1 ]]; then
  export WINEDEBUG="+timestamp,+tid,+seh,+loaddll,err+all,warn+all"
else
  export WINEDEBUG="+timestamp,+tid,+seh,err+all,warn+all"
fi

{
  echo "========== SAUMI RUN $(date '+%F %T') =========="
  echo "user=$(whoami) host=$(hostname)"
  echo "wine=$(${WINE} --version 2>&1 || echo unknown)"
  echo "WINEPREFIX=${WINEPREFIX}"
  echo "WINEARCH=${WINEARCH}"
  echo "WINEDEBUG=${WINEDEBUG}"
  echo "pwd=$(pwd)"
  cd "${WINEPREFIX}/drive_c/SAUMI" || exit 1
  echo "workdir=$(pwd)"
  ls -la mmp32.exe 2>&1 || true
  echo "--- tnsnames.ora (first 20 lines) ---"
  head -n 20 "${WINEPREFIX}/drive_c/oracle/NETWORK/ADMIN/tnsnames.ora" 2>&1 || true
  echo "--- oracle check ---"
  ls "${WINEPREFIX}/drive_c/oracle/BIN/oci.dll" 2>&1 || \
    ls "${WINEPREFIX}/drive_c/oracle/bin/oci.dll" 2>&1 || \
    echo "oci.dll not found in standard paths"
  echo "--- launch ---"
  echo "cmd: ${WINE} mmp32.exe /oldupdate"
} >>"${RUN_LOG}" 2>&1

cd "${WINEPREFIX}/drive_c/SAUMI"
"${WINE}" mmp32.exe /oldupdate >>"${RUN_LOG}" 2>&1
EXIT=$?
echo "========== EXIT CODE: ${EXIT} $(date '+%F %T') ==========" >>"${RUN_LOG}"
exit "${EXIT}"
LAUNCH
chmod +x "${PREFIX}/startSAUMI.sh"

cat > "${PREFIX}/startSAUMI_verbose.sh" <<'LAUNCHV'
#!/usr/bin/env bash
exec "$(dirname "$0")/startSAUMI.sh" --verbose "$@"
LAUNCHV
chmod +x "${PREFIX}/startSAUMI_verbose.sh"

cat > "${PREFIX}/collect_logs.sh" <<'COLLECT'
#!/usr/bin/env bash
set -euo pipefail
STAMP="$(date +%Y%m%d_%H%M%S)"
OUT="${HOME}/saumi_logs_${STAMP}.tar.gz"
TMP="${HOME}/.saumi_collect_${STAMP}"
mkdir -p "${TMP}"

{
  echo "=== collect_logs $(date) ==="
  echo "user=$(whoami) host=$(hostname)"
  uname -a
  wine --version 2>&1 || true
  echo "--- last runtime log ---"
  ls -lt "${HOME}/SAUMIwine/logs/" 2>/dev/null | head -5 || true
} > "${TMP}/collect_info.txt"

cp -f "${HOME}/SAUMIwine/winetricks.log" "${TMP}/" 2>/dev/null || true
cp -f "${HOME}/SAUMIwine/logs/"*.log "${TMP}/" 2>/dev/null || true
cp -rf "${HOME}/saumi_build_logs/" "${TMP}/build_logs/" 2>/dev/null || true

tar -czf "${OUT}" -C "${TMP}" .
rm -rf "${TMP}"
echo "Logs saved: ${OUT}"
ls -lah "${OUT}"
COLLECT
chmod +x "${PREFIX}/collect_logs.sh"

cat > "${PREFIX}/SAUMI.desktop" <<DESKTOP
[Desktop Entry]
Name=SAUMI 4.8.5
Type=Application
Exec=${HOME}/SAUMIwine/startSAUMI.sh
Path=${HOME}/SAUMIwine/drive_c/SAUMI
Terminal=false
StartupNotify=false
DESKTOP

echo "${BUILD_ID}" > "${PREFIX}/BUILD_ID.txt"
cp -f "${LOG_DIR}/build.log" "${PREFIX}/logs/build_${BUILD_ID}.log" 2>/dev/null || true

log "=== smoke test (headless) ==="
set +e
export WINEDEBUG="+timestamp,err+all,warn+all"
xvfb-run -a timeout 30 wine "${PREFIX}/drive_c/SAUMI/mmp32.exe" /? >"${LOG_DIR}/smoke_test.log" 2>&1
SMOKE=$?
set -e
log "smoke test exit: ${SMOKE}"
tail -n 50 "${LOG_DIR}/smoke_test.log" || true

log "=== package ==="
PKG="${HOME}/SAUMI1860wine.tar.gz"
run tar -czf "${PKG}" \
  --exclude='SAUMIwine/drive_c/users/*/Temp/*' \
  --exclude='SAUMIwine/drive_c/users/*/AppData/Local/Temp/*' \
  -C "${HOME}" SAUMIwine
ls -lah "${PKG}"
sha256sum "${PKG}" | tee "${LOG_DIR}/package.sha256"

log "=== build complete ==="
log "Package: ${PKG}"
