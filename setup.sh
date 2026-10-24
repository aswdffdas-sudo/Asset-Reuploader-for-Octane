#!/usr/bin/env bash
# One-time setup on Fedora: build the backend, create private cookie files,
# and install the Studio plugin into your Wine prefix.
#
# Usage: ./setup.sh [wine-prefix]
set -euo pipefail
cd "$(dirname "$(readlink -f "$0")")"

chmod +x build.sh start.sh install-plugin.sh

if [ ! -x ./assetreuploader ]; then
    if ! command -v go >/dev/null 2>&1; then
        read -rp "Go is needed to build the backend. Install it with 'sudo dnf install golang'? [y/N] " answer
        if [[ "$answer" =~ ^[Yy]$ ]]; then
            sudo dnf install -y golang
        else
            echo "Install Go, then run ./setup.sh again." >&2
            exit 1
        fi
    fi
    ./build.sh
fi

# Cookie files stay readable by you only
for f in cookie.txt roblox_cookie.txt; do
    [ -e "$f" ] || : > "$f"
    chmod 600 "$f"
done
echo "Created cookie.txt and roblox_cookie.txt (paste your cookies into them)."

# Each user needs their own Octane user ID in config.ini
if grep -qE '^user_id=\s*$' config.ini && [ -t 0 ]; then
    read -rp "Your Octane user ID (the number in https://octane.wtf/users/<ID>/profile): " uid
    if [[ "$uid" =~ ^[0-9]+$ ]]; then
        sed -i "s/^user_id=.*/user_id=$uid/" config.ini
        echo "Saved user_id=$uid to config.ini"
    else
        echo "Skipped; you'll be asked again when you run ./start.sh"
    fi
fi

./install-plugin.sh "$@" || echo "Plugin not installed; run ./install-plugin.sh <wine-prefix> later."

echo
echo "Done. Fill in the cookie files, then run ./start.sh"
