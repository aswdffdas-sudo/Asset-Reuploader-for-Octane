================================================================================
   ASSET REUPLOADER - OCTANE REVIVAL (ANIMATIONS, AUDIOS, GAMEPASSES)
================================================================================

This edition is fully integrated with Octane (https://octane.wtf):

1. ANIMATION UPLOADS (develop?View=24):
   - Downloads original animations from Roblox.
   - Uploads to Octane as AssetType 24 (Animation).
   - Automatically replaces AnimationId across your game!

2. AUDIO / SOUND UPLOADS (develop?View=3):
   - Downloads original MP3/OGG sound files from Roblox.
   - Uploads directly to Octane (https://octane.wtf/develop?View=3) as AssetType 3 (Audio).
   - Automatically replaces SoundId across your game!
   * Note: Octane charges 15 Robux per audio upload.

3. GAMEPASSES AS T-SHIRTS (develop?View=2):
   - Scans game scripts (GamepassManager, Shop, etc.) for Gamepass IDs.
   - Automatically downloads the official gamepass badges/icons from Roblox.
   - Publishes them to Octane as T-Shirts (AssetType 2).
   - Automatically replaces the old Gamepass IDs with the new Octane T-Shirt IDs!
   - Features a 1-click "Convert APIs" button to turn UserOwnsGamePassAsync into
     PlayerOwnsAsset so gamepass checks work natively in 2021 Studio!

--------------------------------------------------------------------------------
SETUP:
--------------------------------------------------------------------------------
LINUX (FEDORA): Run ./setup.sh once. It builds the backend, creates the cookie
files and installs the plugin into your Wine prefix (./setup.sh <prefix> to pick
one). See README.md for details.

1. cookie.txt -> Your OCTANE session cookie (from octane.wtf).
2. roblox_cookie.txt -> Your OFFICIAL ROBLOX .ROBLOSECURITY (to download private assets).
3. config.ini ->
   port=38073
   domain=octane.wtf
   user_id=<your own Octane user ID>
   cookie_file=cookie.txt
   roblox_cookie_file=roblox_cookie.txt

4. Run ./start.sh
5. In Studio, open the Asset Reuploader plugin, pick your tab, and click Reupload!
================================================================================
