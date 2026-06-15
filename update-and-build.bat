@echo off
chcp 65001 >nul
cd /d D:\poerunes\PoeAncientsPriceHelper

echo [1/3] Pulling latest changes from origin/de-translation...
git pull origin de-translation
if errorlevel 1 (
    echo Git pull failed!
    pause
    exit /b 1
)

echo [2/3] Building release...
dotnet publish src\PoeAncientsPriceHelper\ -c Release -r win-x64 --self-contained true
if errorlevel 1 (
    echo Build failed!
    pause
    exit /b 1
)

echo [3/3] Done. Executable updated.
echo.
echo Run from:
echo   D:\poerunes\PoeAncientsPriceHelper\src\PoeAncientsPriceHelper\bin\Release\net8.0-windows\win-x64\publish\PoeAncientsPriceHelper.exe
echo.
pause
