#!/usr/bin/env bash
# Пересборка SAUMI1860wine.tar.gz на тестовой Astra
# Запуск на Astra: ./rebuild_saumi_on_astra.sh
# Или: ~/bin/rebuild_saumi
set -euo pipefail

INC="${HOME}/saumi_deploy/incoming"
SRC="${HOME}/saumi_build_src"
LOG_ROOT="${HOME}/saumi_build_logs"
BUILD_ID="$(date +%Y%m%d_%H%M%S)"
LOG_DIR="${LOG_ROOT}/${BUILD_ID}"
LOG_FILE="${LOG_DIR}/rebuild.log"

mkdir -p "${LOG_DIR}" "${SRC}"
exec > >(tee -a "${LOG_FILE}") 2>&1

log() { echo "[$(date '+%F %T')] $*"; }
run() { log "CMD: $*"; "$@"; }

log "=== SAUMI rebuild on Astra ==="
log "BUILD_ID=${BUILD_ID}"

# Sync incoming → build_src
log "Sync incoming → saumi_build_src"
for f in SAUMI485.zip SAUMI1860wine.tar.gz oracle_client_from1710.tar.gz \
         dotnet_from1710.tar.gz dotnet_assembly_from1710.tar.gz \
         tnsnames.ora system.reg.old1710 SAUMI1860wine.sha256; do
  if [[ -f "${INC}/${f}" ]]; then
    run cp -f "${INC}/${f}" "${SRC}/${f}"
    log "  synced: ${f}"
  fi
done
if [[ -d "${INC}/build" ]]; then
  run cp -rf "${INC}/build/"* "${SRC}/" 2>/dev/null || true
fi
for f in build_saumi1860_prefix.sh merge_dotnet_registry.py; do
  [[ -f "${INC}/${f}" ]] && cp -f "${INC}/${f}" "${SRC}/"
  [[ -f "${INC}/build/${f}" ]] && cp -f "${INC}/build/${f}" "${SRC}/"
done

# Check required sources
MISSING=0
for req in SAUMI485.zip oracle_client_from1710.tar.gz dotnet_from1710.tar.gz tnsnames.ora; do
  if [[ ! -f "${SRC}/${req}" ]]; then
    log "MISSING: ${SRC}/${req}"
    MISSING=1
  fi
done
if [[ "${MISSING}" -eq 1 ]]; then
  log "ERROR: Upload sources first: deploy_to_astra.sh SAUMI485.zip oracle_client_from1710.tar.gz ..."
  log "  Or copy large files to ~/saumi_deploy/incoming/ manually"
  exit 1
fi

[[ -f "${SRC}/build_saumi1860_prefix.sh" ]] || { log "ERROR: build_saumi1860_prefix.sh not found"; exit 1; }
chmod +x "${SRC}/build_saumi1860_prefix.sh"

log "Starting build..."
run "${SRC}/build_saumi1860_prefix.sh"

PKG="${HOME}/SAUMI1860wine.tar.gz"
if [[ -f "${PKG}" ]]; then
  run cp -f "${PKG}" "${INC}/SAUMI1860wine.tar.gz"
  run sha256sum "${PKG}" | tee "${LOG_DIR}/package.sha256"
  run ls -lah "${PKG}"
  log "Package ready: ${PKG}"
  log "Also copied to: ${INC}/SAUMI1860wine.tar.gz"
else
  log "ERROR: package not created"
  exit 1
fi

log "=== Rebuild complete ==="
log "Log: ${LOG_FILE}"
ln -sf "${LOG_FILE}" "${LOG_ROOT}/rebuild_latest.log" 2>/dev/null || true
