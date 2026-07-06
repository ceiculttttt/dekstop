@echo off
setlocal EnableDelayedExpansion
title Ansem Bull Overlay - One-Click Builder
color 0A

echo.
echo  ================================================================
echo    ANSEM BULL OVERLAY  //  One-Click Builder
echo  ================================================================
echo.
echo   This will:
echo     1. Check for .NET 8 SDK (install via winget if missing)
echo     2. Build a single portable AnsemBullOverlay.exe
echo     3. Copy it to your Desktop
echo.
echo   You only need to run this ONCE. After that just double-click
echo   the .exe on your Desktop to start the bull.
echo.
pause

REM ---- 1. Check for dotnet ----
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo  [!] .NET SDK not found. Attempting install via winget...
    where winget >nul 2>nul
    if !ERRORLEVEL! NEQ 0 (
        echo.
        echo  [X] winget is not available on this PC.
        echo      Please install the .NET 8 SDK manually from:
        echo      https://dotnet.microsoft.com/download/dotnet/8.0
        echo.
        pause
        exit /b 1
    )
    winget install --id Microsoft.DotNet.SDK.8 -e --accept-package-agreements --accept-source-agreements
    if !ERRORLEVEL! NEQ 0 (
        echo.
        echo  [X] winget install failed. Install manually from:
        echo      https://dotnet.microsoft.com/download/dotnet/8.0
        pause
        exit /b 1
    )
    echo.
    echo  [+] .NET SDK installed. You may need to open a NEW terminal.
    echo      Re-run this script after closing this window.
    pause
    exit /b 0
)

echo.
echo  [+] .NET SDK detected:
dotnet --version

REM ---- 2. Build self-contained single-file exe ----
echo.
echo  [~] Building portable executable... (takes ~1-2 minutes first time)
echo.

pushd "%~dp0"
dotnet publish AnsemBullOverlay -c Release -r win-x64 --self-contained true ^
    /p:PublishSingleFile=true ^
    /p:IncludeNativeLibrariesForSelfExtract=true ^
    /p:EnableCompressionInSingleFile=true ^
    -o "%~dp0\_build"
set BUILD_RESULT=%ERRORLEVEL%
popd

if %BUILD_RESULT% NEQ 0 (
    echo.
    echo  [X] Build failed. Scroll up for details.
    pause
    exit /b 1
)

REM ---- 3. Copy to Desktop ----
set "EXE_SRC=%~dp0_build\AnsemBullOverlay.exe"
set "DESKTOP=%USERPROFILE%\Desktop"
set "EXE_DEST=%DESKTOP%\AnsemBullOverlay.exe"

if not exist "%EXE_SRC%" (
    echo  [X] Build finished but exe not found at:
    echo      %EXE_SRC%
    pause
    exit /b 1
)

copy /Y "%EXE_SRC%" "%EXE_DEST%" >nul

echo.
echo  ================================================================
echo    SUCCESS
echo  ================================================================
echo.
echo   Portable exe created here:
echo     %EXE_DEST%
echo.
echo   Double-click it any time to start the bull.
echo.
echo   Hotkeys:
echo     Ctrl+Shift+B  =  Bull Charge
echo     Ctrl+Shift+R  =  Rocket Boost
echo     Ctrl+Shift+H  =  Toggle Overlay
echo.
echo   Right-click the tray icon for the menu + Configure panel.
echo.

choice /M "  Launch it now"
if %ERRORLEVEL% EQU 1 start "" "%EXE_DEST%"

exit /b 0
