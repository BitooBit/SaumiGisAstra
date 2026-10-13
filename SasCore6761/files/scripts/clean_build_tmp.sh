#!/usr/bin/env bash
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
rm -rf "${ROOT}/files/_build_tmp"/*
mkdir -p "${ROOT}/files/_build_tmp"
echo "OK: ${ROOT}/files/_build_tmp очищена"
