@echo off
SETLOCAL
SET "bingo_dir=%~dp0"

cd "C:\Program Files (x86)\Steam\steamapps\common\Rain World\RainWorld_Data\StreamingAssets\mods\bingomode" || (
    echo Failed to change directory.
    exit /b
)

REM rm /S /Q "plugins"
REM Backup existing plugins folder if it exists
IF EXIST plugins (
    echo Backing up existing plugins folder...
    rename plugins plugins_bak
)

REM Create symbolic link to development build
echo Creating symbolic link...
mklink /D plugins "%bingo_dir%\BingoMode\bin\Release" || (
    echo Failed to create symbolic link. Are you running as administrator?
    pause
    exit /b
)

echo Done.
pause