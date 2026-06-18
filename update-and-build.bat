@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

set REPO_DIR=D:\poerunes\PoeAncientsPriceHelper
set BRANCH=de-translation
set REPO_URL=https://github.com/xtraxion/PoeAncientsPriceHelper.git

:: ── Prerequisites check ──
where git >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Git is not installed or not in PATH.
    echo Download: https://git-scm.com/download/win
    pause
    exit /b 1
)

where dotnet >nul 2>&1
if errorlevel 1 (
    echo [ERROR] .NET SDK is not installed or not in PATH.
    echo Download: https://dotnet.microsoft.com/en-us/download/dotnet/8.0
    pause
    exit /b 1
)

:: ── Decide: fresh install or update ──
if exist "%REPO_DIR%\.git" (
    echo [UPDATE] Repository found — pulling latest changes...
    cd /d "%REPO_DIR%"
    git pull origin %BRANCH%
    if errorlevel 1 (
        echo Git pull failed!
        pause
        exit /b 1
    )
) else (
    echo [FRESH INSTALL] Cloning repository...
    if not exist "D:\poerunes" mkdir "D:\poerunes"
    cd /d "D:\poerunes"
    git clone %REPO_URL%
    if errorlevel 1 (
        echo Git clone failed!
        pause
        exit /b 1
    )
    cd /d "%REPO_DIR%"
    git checkout %BRANCH%
    if errorlevel 1 (
        echo Branch checkout failed!
        pause
        exit /b 1
    )
)

:: ── Build ──
echo Building release...
dotnet publish src\PoeAncientsPriceHelper\ -c Release -r win-x64 --self-contained true
if errorlevel 1 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Done. Executable updated.
echo.
echo Run from:
echo   %REPO_DIR%\src\PoeAncientsPriceHelper\bin\Release\net8.0-windows\win-x64\publish\PoeAncientsPriceHelper.exe
echo.
pause
