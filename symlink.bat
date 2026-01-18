@echo off
SETLOCAL
title Rain World BingoMode Dev Setup
REM This batch script will create a bingo mode mod folder in Rain World, symlink it to your development mod folder (right next to this script), grab all deps from Rain World, and nstrip the main assembly for RW (needed for bingo)

REM Check if script is run as administrator (needed for mklink)
net session >nul 2>&1
if %errorlevel% NEQ 0 (
    echo [ERROR] This script must be run as administrator.
    pause
    exit /b
)

SET "bingo_dir=%~dp0"
SET "lib_dir=%bingo_dir%\lib"

REM !!!!!!!!!!UPDATE THIS RIGHT HERE. YOOHOO IF ANYTHING BREAKS MAKE SURE THIS PATH IS RIGHT!!!!!!!!!!
SET "rw_dir=C:\Program Files (x86)\Steam\steamapps\common\Rain World\"

REM Create lib directory if it doesn't exist
if not exist "%lib_dir%" (
    echo Creating lib directory...
    mkdir "%lib_dir%"
)

REM Go to the Rain World mod directory
cd "%rw_dir%RainWorld_Data\StreamingAssets\mods" || (
    echo Failed to find mod directory in Rain World. Your install drive might not be your C drive. Update the rw_dir path in the batch script.
    exit /b
)

REM Remove old plugins folder or symlink
IF EXIST bingomode (
    echo Removing existing plugins folder or symlink...
    rmdir /S /Q bingomode
)

REM Create symbolic link to development build
echo Creating symbolic link to development build...
mklink /D bingomode "%bingo_dir%\mod" || (
    echo [ERROR] Failed to create symbolic link. Skill issue really
    pause
    exit /b
)

echo Copying dependencies to lib...

call :copy_with_check "%rw_dir%BepInEx\plugins\HOOKS-Assembly-CSharp.dll" "%lib_dir%" "HOOKS-Assembly-CSharp.dll"

call :copy_with_check "%rw_dir%RainWorld_Data\Managed\Assembly-CSharp-firstpass.dll" "%lib_dir%" "Assembly-CSharp-firstpass.dll"
call :copy_with_check "%rw_dir%RainWorld_Data\Managed\com.rlabrecque.steamworks.net.dll" "%lib_dir%" "com.rlabrecque.steamworks.net.dll"
call :copy_with_check "%rw_dir%RainWorld_Data\Managed\Rewired_Core.dll" "%lib_dir%" "Rewired_Core.dll"
call :copy_with_check "%rw_dir%RainWorld_Data\Managed\Unity.Mathematics.dll" "%lib_dir%" "Unity.Mathematics.dll"
call :copy_with_check "%rw_dir%RainWorld_Data\Managed\UnityEngine.AudioModule.dll" "%lib_dir%" "UnityEngine.AudioModule.dll"
call :copy_with_check "%rw_dir%RainWorld_Data\Managed\UnityEngine.CoreModule.dll" "%lib_dir%" "UnityEngine.CoreModule.dll"
call :copy_with_check "%rw_dir%RainWorld_Data\Managed\UnityEngine.dll" "%lib_dir%" "UnityEngine.dll"
call :copy_with_check "%rw_dir%RainWorld_Data\Managed\UnityEngine.InputLegacyModule.dll" "%lib_dir%" "UnityEngine.InputLegacyModule.dll"
call :copy_with_check "%rw_dir%RainWorld_Data\Managed\UnityEngine.UnityWebRequestWWWModule.dll" "%lib_dir%" "UnityEngine.UnityWebRequestWWWModule.dll"

call :copy_with_check "%rw_dir%BepInEx\core\BepInEx.dll" "%lib_dir%" "BepInEx.dll"
call :copy_with_check "%rw_dir%BepInEx\core\Mono.Cecil.dll" "%lib_dir%" "Mono.Cecil.dll"
call :copy_with_check "%rw_dir%BepInEx\core\MonoMod.dll" "%lib_dir%" "MonoMod.dll"
call :copy_with_check "%rw_dir%BepInEx\core\MonoMod.RuntimeDetour.dll" "%lib_dir%" "MonoMod.RuntimeDetour.dll"
call :copy_with_check "%rw_dir%BepInEx\core\MonoMod.Utils.dll" "%lib_dir%" "MonoMod.Utils.dll"

echo.

REM Run NStrip to strip Assembly-CSharp.dll
echo Running NStrip on Assembly-CSharp.dll...

set "nstrip_input=%rw_dir%RainWorld_Data\Managed\Assembly-CSharp.dll"
set "nstrip_output=%lib_dir%\Assembly-CSharp-nstrip.dll"

if exist "%nstrip_input%" (
    "%bingo_dir%\NStrip.exe" -p "%nstrip_input%" "%nstrip_output%"
    if errorlevel 1 (
        echo [ERROR] NStrip failed to process Assembly-CSharp.dll
    ) else (
        echo [OK] NStrip output saved to "%nstrip_output%"
    )
) else (
    echo [MISSING] Input file for NStrip not found: "%nstrip_input%"
)

echo.
echo =============================
echo All copy operations complete :)
echo =============================
pause
goto :eof

:copy_with_check
set "source=%~1"
set "target=%~2"
set "desc=%~3"

if exist "%source%" (
    copy /Y "%source%" "%target%" >nul
    if errorlevel 1 (
        echo [ERROR] Failed to copy %desc% from "%source%" to "%target%"
    ) else (
        echo [OK] Copied %desc%
    )
) else (
    echo [MISSING] %desc% not found at "%source%"
)
goto :eof
