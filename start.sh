#!/usr/bin/env bash
# Starts the reuploader backend from this folder (builds it first if needed)
set -euo pipefail
cd "$(dirname "$(readlink -f "$0")")"

if [ ! -x ./assetreuploader ]; then
    ./build.sh
fi

exec ./assetreuploader "$@"
