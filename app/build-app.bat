@echo off
cd /d "%~dp0\.."
powershell -NoProfile -Command "Compress-Archive -Path 'assetreuploader.exe','AssetReuploader2021.rbxm','AssetReuploader2021.rbxmx','config.ini','cookie.txt','roblox_cookie.txt','README_2021.txt','README.md','assets' -DestinationPath 'app\payload.zip' -Force"
set "NETDIR=C:\Windows\Microsoft.NET\Framework64\v4.0.30319"
set "WPFDIR=%NETDIR%\WPF"
"%NETDIR%\csc.exe" /nologo /target:winexe /out:"Octane Asset Reuploader Setup.exe" /win32icon:"app\app.ico" /resource:"app\payload.zip",OctaneReup.Payload.zip /r:"%WPFDIR%\PresentationFramework.dll" /r:"%WPFDIR%\PresentationCore.dll" /r:"%WPFDIR%\WindowsBase.dll" /r:"%NETDIR%\System.Xaml.dll" /r:"%NETDIR%\System.IO.Compression.dll" "app\LauncherApp.cs"
del /q "app\payload.zip" 2>nul
echo Built self-contained "Octane Asset Reuploader Setup.exe" successfully.
