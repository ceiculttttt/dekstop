@echo off
title Ansem Bull Overlay - Dev Run
color 0A
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo .NET 8 SDK not found. Run Build-Exe.bat first (it will install it).
    pause
    exit /b 1
)
pushd "%~dp0"
dotnet run --project AnsemBullOverlay -c Release
popd
