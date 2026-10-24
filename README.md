# Octane Asset Reuploader (2021 Studio Edition)

Asset Reuploader modified and optimized for the **Octane** Roblox revival (`octane.wtf`, formerly Aisaka) and **Roblox Studio 2021 (build 0.477)**.

Based on the original [Asset-Reuploader](https://github.com/kartFr/Asset-Reuploader) by kartFr.

---

## ✨ Features

- **Dual-Cookie Architecture**:
  - `roblox_cookie.txt`: Authenticates with official Roblox to download private/protected assets, animations, and sound files.
  - `cookie.txt`: Authenticates with Octane (`octane.wtf`) to upload and publish reuploaded assets directly to your Octane account.
- **Animations (`develop?View=24`)**:
  - Extracts keyframe sequences and animation data from official Roblox.
  - Publishes them as native Octane Animation assets.
  - Automatically replaces `AnimationId`s across all instances and scripts in your game.
- **Audio / Sounds (`develop?View=3`)**:
  - Downloads original audio files (MP3/OGG) directly from official Roblox CDN.
  - Uploads to Octane as Sound assets (Octane charges 15 Robux per audio upload).
  - Automatically updates `SoundId` references throughout your place.
- **Gamepasses as T-Shirts (`develop?View=2`)**:
  - Scans all scripts (`GamepassManager`, shops, UI controllers) for Gamepass IDs.
  - Downloads official gamepass icons and publishes them to Octane as T-Shirts.
  - Features a 1-click **Convert APIs** button to convert `MarketplaceService:UserOwnsGamePassAsync` calls into `Player:PlayerOwnsAsset` so gamepass functionality works natively in 2021 Studio!
- **2021 Studio / Revival Engine Compatibility**:
  - Patched Luau syntax (compatible with 2021 build 477 without modern type syntax issues).
  - Isolated datamodel traversal preventing `identity 5 lacks permission 6` crashes on internal CoreGui services.
  - Polyfilled `task` scheduling library (`task.spawn`, `task.wait`, `task.delay`).
  - Embedded relative module require resolver.

---

## 🐧 Linux (Fedora) Edition

This is the Linux build of the reuploader. The backend runs natively on Fedora;
Octane Studio 2021 runs under Wine (plain Wine, Bottles, or Lutris), and the
plugin inside Studio talks to the backend over `localhost`, which Wine shares
with Linux.

Differences from the Windows build:
- `setup.sh`, `start.sh`, `install-plugin.sh` and `build.sh` replace
  `Octane Asset Reuploader Setup.exe` and `assetreuploader.exe`.
- The backend finds `config.ini` and the cookie files next to itself, so it works
  when started from a file manager as well as a terminal.
- Cookie and API key files are written with `600` permissions (only you can read them).
- The backend listens on loopback (`127.0.0.1` / `::1`) only. Fedora Workstation's
  firewall leaves high ports open to your network, and this server acts with your
  cookies. Set `bind_address=0.0.0.0` in `config.ini` only if you really need LAN access.

---

## 🚀 Setup & Usage

### 1. Install
```bash
tar xzf Octane-Asset-Reuploader-*-linux-x86_64.tar.gz
cd Octane-Asset-Reuploader-linux
./setup.sh                 # or: ./setup.sh /path/to/your/wine-prefix
```
`setup.sh` builds the backend if needed (offering `sudo dnf install golang`),
creates empty `cookie.txt` / `roblox_cookie.txt` with private permissions, asks
for your Octane user ID, and copies the plugin into your Wine prefix.

### 2. Configuration
1. Open `cookie.txt` and paste your **Octane session cookie** (from `octane.wtf`).
2. Open `roblox_cookie.txt` and paste your **official Roblox `.ROBLOSECURITY`** cookie (required for downloading private/protected sounds and animations).
3. Open `config.ini` and verify the settings:
   ```ini
   port=38073
   domain=octane.wtf
   user_id=YOUR_OCTANE_USER_ID
   cookie_file=cookie.txt
   roblox_cookie_file=roblox_cookie.txt
   ```
   *(`user_id` ships empty. Use your own numeric user ID from your Octane profile URL, e.g. `https://octane.wtf/users/12345/profile`. `setup.sh` and the first `./start.sh` ask for it if it's blank.)*

### 3. Install the Studio Plugin
`setup.sh` does this for you. To do it again or for another prefix:
```bash
./install-plugin.sh                                      # uses $WINEPREFIX or ~/.wine
./install-plugin.sh ~/.local/share/bottles/bottles/Octane # Bottles
./install-plugin.sh ~/Games/octane                        # Lutris
```
It copies `AssetReuploader2021.rbxmx` to
`<prefix>/drive_c/users/<you>/AppData/Local/Roblox/Plugins/`.

### 4. Run the Reuploader Backend
```bash
./start.sh
```
Ensure it displays:
```text
Authenticating cookie...
Official Roblox download cookie loaded!
localhost started on port 38073. Waiting to start reuploading.
```

### 5. Reupload Assets in Studio
1. Open your place in Octane Studio.
2. In the **Plugins** ribbon bar, click **Asset Reuploader**.
3. Select your target tab:
   - **Animation**: Scan and migrate animations.
   - **Sound**: Scan and migrate audio files.
   - **Gamepass**: Scan scripts for gamepass IDs, upload icons as T-Shirts, and convert gamepass check APIs with one click.
   - **Replace**: Search & replace specific IDs manually.
4. Click **Reupload** and wait for the migration to complete!

---

## 🛠️ Building from Source

### Requirements
- [Go](https://go.dev/) 1.23+ (`sudo dnf install golang`)
- [Rojo](https://rojo.space/) 7.x (only to rebuild the plugin)

### Build the Backend
```bash
./build.sh
```

### Build the Studio Plugin
```bash
cd plugin
rojo build default.project.json -o AssetReuploader2021.rbxmx
```

---

## 📜 Credits & License

- Original project by [kartFr](https://github.com/kartFr/Asset-Reuploader).
- Licensed under the [GNU General Public License v3.0](LICENSE).
