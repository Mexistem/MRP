@echo off
setlocal enabledelayedexpansion

set "SCRIPT_DIR=%~dp0"
cd /d "%SCRIPT_DIR%"
cd ..\..\
set "ROOT_DIR=%cd%"
set "SERVER_PROJECT=%ROOT_DIR%\MRP.Server"
set "TESTS_DIR=%ROOT_DIR%\MRP.Tests\Postman"

echo --------------------------------------
echo Root Directory: %ROOT_DIR%
echo Server Project: %SERVER_PROJECT%
echo Tests Directory: %TESTS_DIR%
echo --------------------------------------

echo Starte MRP.Server ...
start "" /b dotnet run --project "%SERVER_PROJECT%" > "%SCRIPT_DIR%\server_log.txt" 2>&1

echo Warte auf Serverstart ...
set /a retries=0
:wait_for_server
set /a retries+=1
curl -s http://localhost:8080/ >nul 2>&1
if %errorlevel%==0 (
    echo Server antwortet nach %retries% Sekunden.
) else (
    if %retries% lss 30 (
        timeout /t 1 >nul
        goto wait_for_server
    ) else (
        echo Fehler: Server antwortet nicht nach 30 Sekunden.
        echo --- Log-Ausgabe ---
        type "%SCRIPT_DIR%\server_log.txt"
        echo -------------------
        pause
        exit /b 1
    )
)

echo Fuehre Postman Tests aus ...
if not exist "%TESTS_DIR%\MRP API Tests.postman_collection.json" (
    echo Fehler: Postman Collection nicht gefunden.
    echo Erwartet unter: %TESTS_DIR%\MRP API Tests.postman_collection.json
    pause
    exit /b 1
)

newman run "%TESTS_DIR%\MRP API Tests.postman_collection.json" ^
 -e "%TESTS_DIR%\MRP Project.postman_environment.json" ^
 --reporters cli,html ^
 --reporter-html-export "%TESTS_DIR%\report.html"

set "TEST_RESULT=%ERRORLEVEL%"

if "%TEST_RESULT%"=="0" (
    echo Alle Tests erfolgreich.
) else (
    echo Es gab Fehler im Testlauf.
)

echo Beende Server-Prozess ...
for /f "tokens=2" %%a in ('tasklist ^| find "dotnet.exe"') do taskkill /PID %%a /F >nul 2>&1

echo Report gespeichert unter: %TESTS_DIR%\report.html
pause
endlocal