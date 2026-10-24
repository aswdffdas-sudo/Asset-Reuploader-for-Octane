#!/usr/bin/env bash
# Builds the reuploader backend as ./assetreuploader
set -euo pipefail
cd "$(dirname "$(readlink -f "$0")")"

if ! command -v go >/dev/null 2>&1; then
    echo "Go is not installed. On Fedora run:  sudo dnf install golang" >&2
    exit 1
fi

VERSION=$(tr -d '\r\n' < plugin/Version.txt)
CGO_ENABLED=0 go build \
    -ldflags="-s -w -X main.CompatiblePluginVersion=${VERSION}" \
    -o assetreuploader \
    ./cmd/assetreuploader

echo "Built ./assetreuploader"
