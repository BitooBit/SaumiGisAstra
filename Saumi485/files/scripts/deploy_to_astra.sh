#!/usr/bin/env bash
# Загрузка комплекта на тестовую Astra с Mac.
#   bash deploy_to_astra.sh saumi       — Saumi485/install (готовый комплект)
#   bash deploy_to_astra.sh build-src   — исходники для пересборки prefix на Astra
#   bash deploy_to_astra.sh sascore     — SasCore6761/install
#   bash deploy_to_astra.sh file1 …      — указанные файлы
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
SSH_KEY="${ROOT}/../../.ssh/saumi_astra_ed25519"
[[ -f "${SSH_KEY}" ]] || SSH_KEY="${ROOT}/../.ssh/saumi_astra_ed25519"
[[ -f "${SSH_KEY}" ]] || SSH_KEY="${HOME}/.ssh/saumi_astra_ed25519"
HOST="${SAUMI_TEST_HOST:-denis@192.168.222.239}"
REMOTE="~/saumi_deploy/incoming"

SSH_OPTS=(-o BatchMode=yes -o StrictHostKeyChecking=accept-new -i "${SSH_KEY}")
log() { echo "[$(date '+%F %T')] $*"; }

upload() {
  local src="$1" name
  name="$(basename "$src")"
  log "  → $name"
  ssh "${SSH_OPTS[@]}" "$HOST" "mkdir -p $REMOTE"
  cat "$src" | ssh "${SSH_OPTS[@]}" "$HOST" "cat > $REMOTE/$name"
}

log "=== deploy_to_astra → $HOST ==="
ssh "${SSH_OPTS[@]}" "$HOST" "mkdir -p $REMOTE"

if [[ $# -eq 0 ]]; then
  set -- saumi
fi

for arg in "$@"; do
  case "$arg" in
    saumi)
      upload "${ROOT}/install/instalSaumi.sh"
      upload "${ROOT}/install/SAUMI1860wine.tar.gz"
      upload "${ROOT}/install/ИНСТРУКЦИЯ.docx"
      ;;
    build-src)
      for f in SAUMI485.zip tnsnames.ora \
               oracle_client_from1710.tar.gz \
               dotnet_from1710.tar.gz dotnet_assembly_from1710.tar.gz \
               dotnet_registry_from1710.reg system.reg.old1710; do
        upload "${ROOT}/files/files/${f}"
      done
      upload "${ROOT}/files/scripts/build_saumi1860_prefix.sh"
      upload "${ROOT}/files/scripts/rebuild_saumi_on_astra.sh"
      upload "${ROOT}/files/scripts/merge_dotnet_registry.py"
      ;;
    sascore)
      upload "${ROOT}/../SasCore6761/install/installSasCore.sh"
      upload "${ROOT}/../SasCore6761/install/SasCoreWine.tar.gz"
      upload "${ROOT}/../SasCore6761/install/ИНСТРУКЦИЯ.docx"
      ;;
    *)
      [[ -f "$arg" ]] && upload "$arg" || log "SKIP: $arg"
      ;;
  esac
done

log "=== done ==="
