#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "${SCRIPT_DIR}"

OUT_NAME="MacWindowPlugin.bundle"

clang -bundle \
  -o "${OUT_NAME}" \
  MacWindowPlugin.m \
  -framework Cocoa \
  -framework ApplicationServices

echo "Built: ${SCRIPT_DIR}/${OUT_NAME}"

