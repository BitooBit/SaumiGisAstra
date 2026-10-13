#!/usr/bin/env bash
# installSasCore.sh → install/. Поставка: installSasCore.sh + SasCoreWine.tar.gz.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
SRC="${ROOT}/files/install"
DST="${ROOT}/install"
[[ -f "${SRC}/installSasCore.sh" ]] || { echo "FAIL: нет ${SRC}/installSasCore.sh"; exit 1; }
cp -f "${SRC}/installSasCore.sh" "${DST}/"
chmod +x "${DST}/installSasCore.sh" "${SRC}/installSasCore.sh"
rm -f "${DST}/install_sascore.sh"
rm -rf "${DST}/sascore-bin" "${SRC}/sascore-bin"
echo "OK: ${DST}/installSasCore.sh"
