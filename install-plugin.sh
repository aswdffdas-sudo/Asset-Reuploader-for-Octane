#!/usr/bin/env bash
# Copies the Studio plugin into the Wine prefix that runs Octane Studio 2021.
#
# Usage: ./install-plugin.sh [wine-prefix]
# The prefix defaults to $WINEPREFIX, then ~/.wine.
set -euo pipefail
cd "$(dirname "$(readlink -f "$0")")"

PREFIX="${1:-${WINEPREFIX:-$HOME/.wine}}"
PLUGIN="AssetReuploader2021.rbxmx"

if [ ! -d "$PREFIX/drive_c" ]; then
    echo "No Wine prefix found at: $PREFIX" >&2
    echo "Pass the prefix Studio runs in, e.g.:" >&2
    echo "  ./install-plugin.sh ~/.local/share/bottles/bottles/Octane" >&2
    echo "  ./install-plugin.sh ~/Games/octane" >&2
    exit 1
fi

# Wine names the Windows user after the Linux user, but some launchers
# (Proton, Lutris) use "steamuser"; prefer one that already has Roblox data.
USERS_DIR="$PREFIX/drive_c/users"
WIN_USER="$USER"
for candidate in "$USER" steamuser; do
    if [ -d "$USERS_DIR/$candidate/AppData/Local/Roblox" ]; then
        WIN_USER="$candidate"
        break
    fi
done

DEST="$USERS_DIR/$WIN_USER/AppData/Local/Roblox/Plugins"
mkdir -p "$DEST"
cp -f "$PLUGIN" "$DEST/$PLUGIN"
echo "Installed plugin to: $DEST/$PLUGIN"
