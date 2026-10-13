#!/usr/bin/env bash
# Загрузка исходников SasCore / dotnet4 на тестовую Astra с Mac.
#   bash deploy_build_src_to_astra.sh
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
SAUMI_ROOT="${ROOT}/../Saumi485"
SSH_KEY="${SAUMI_ROOT}/../../.ssh/saumi_astra_ed25519"
[[ -f "${SSH_KEY}" ]] || SSH_KEY="${SAUMI_ROOT}/../.ssh/saumi_astra_ed25519"
[[ -f "${SSH_KEY}" ]] || SSH_KEY="${HOME}/.ssh/saumi_astra_ed25519"
HOST="${SAUMI_TEST_HOST:-denis@192.168.222.239}"
REMOTE="~/saumi_deploy/incoming"
FILES="${ROOT}/files/files"

SSH_OPTS=(-o BatchMode=yes -o StrictHostKeyChecking=no -i "${SSH_KEY}")
log() { echo "[$(date '+%F %T')] $*"; }

upload() {
  local src="$1" name
  name="$(basename "$src")"
  log "  → $name"
  ssh "${SSH_OPTS[@]}" "$HOST" "mkdir -p $REMOTE"
  cat "$src" | ssh "${SSH_OPTS[@]}" "$HOST" "cat > $REMOTE/$name"
}

log "=== deploy SasCore build-src → $HOST ==="
ssh "${SSH_OPTS[@]}" "$HOST" "mkdir -p $REMOTE"

for f in dotnet4_framework.tar.gz dotnet4_assembly.tar.gz dotnet4_registry.reg; do
  upload "${FILES}/${f}"
done
upload "${FILES}/dotnet4_installer/dotNetFx40_Full_x86_x64.exe"

# Saumi build bundles (prefix rebuild)
for f in SAUMI485.zip tnsnames.ora oracle_client_from1710.tar.gz \
         dotnet_from1710.tar.gz dotnet_assembly_from1710.tar.gz \
         dotnet_registry_from1710.reg system.reg.old1710; do
  upload "${SAUMI_ROOT}/files/files/${f}"
done
for f in build_saumi1860_prefix.sh rebuild_saumi_on_astra.sh merge_dotnet_registry.py inject_wine_into_tar.sh; do
  upload "${SAUMI_ROOT}/files/scripts/${f}"
done

log "=== done ==="
