#!/usr/bin/env bash
# Копировать установщик из files/install → install/ (tar не трогает).
# ИНСТРУКЦИЯ.docx правится напрямую в install/ — только Word.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
SRC="${ROOT}/files/install"
DST="${ROOT}/install"
[[ -f "${SRC}/instalSaumi.sh" ]] || { echo "FAIL: нет ${SRC}/instalSaumi.sh"; exit 1; }
cp -f "${SRC}/instalSaumi.sh" "${DST}/"
chmod +x "${DST}/instalSaumi.sh"
echo "OK: ${DST}/instalSaumi.sh"
