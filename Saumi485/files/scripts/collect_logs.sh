#!/usr/bin/env bash
# Сбор всех логов SAUMI в один архив для отправки в поддержку
set -euo pipefail

PREFIX="${WINEPREFIX:-/var/install/Saumi}"
STAMP="$(date +%Y%m%d_%H%M%S)"
OUT="/tmp/saumi_support_${STAMP}.tar.gz"
TMP="/tmp/.saumi_collect_${STAMP}"
mkdir -p "${TMP}"

{
  echo "=== SAUMI support bundle $(date) ==="
  echo "user=$(whoami) host=$(hostname)"
  uname -a
  wine --version 2>&1 || true
  echo ""
  echo "=== Canonical log paths ==="
  echo "install:      ${PREFIX}/logs/install.log"
  echo "install_last: ${PREFIX}/logs/install_latest.log"
  echo "run_last:     ${PREFIX}/logs/saumi_latest.log"
  echo "run_verbose:  ${PREFIX}/logs/saumi_latest_verbose.log"
  echo "rolling:      ${PREFIX}/logs/saumi_run.log"
  echo ""
  ls -la "${PREFIX}/logs/" 2>/dev/null || true
} > "${TMP}/00_collect_info.txt"

copy_if() {
  [[ -f "$1" ]] || return 0
  cp -f "$1" "${TMP}/"
  echo "  + $1"
}

echo "Collecting logs..."
copy_if "${PREFIX}/logs/install.log"
copy_if "${PREFIX}/logs/install_latest.log"
copy_if "${PREFIX}/logs/saumi_latest.log"
copy_if "${PREFIX}/logs/saumi_latest_verbose.log"
copy_if "${PREFIX}/logs/saumi_run.log"
copy_if "${PREFIX}/winetricks.log"
copy_if "${PREFIX}/BUILD_ID.txt"

# All timestamped run/install logs
shopt -s nullglob
for f in "${PREFIX}/logs/saumi_"*.log "${PREFIX}/logs/install_"*.log; do
  cp -f "$f" "${TMP}/" && echo "  + $f"
done
shopt -u nullglob

# Build logs optional (may be large) — only summary
if [[ -d "${HOME}/saumi_build_logs" ]]; then
  ls -laR "${HOME}/saumi_build_logs" > "${TMP}/build_logs_index.txt" 2>/dev/null || true
  echo "  + build_logs_index.txt (listing only)"
fi

tar -czf "${OUT}" -C "${TMP}" .
rm -rf "${TMP}"

echo ""
echo "Отправьте этот файл: ${OUT}"
ls -lah "${OUT}"
