@echo off
echo ============================================
echo   NUCLEAR OPTION - COMPLETE CLEAN
echo ============================================
echo.

cd /d "C:\Users\musta\Desktop\pro\accountant"

echo [1/8] Killing ALL .NET processes...
taskkill /F /IM dotnet.exe 2>nul
taskkill /F /IM MSBuild.exe 2>nul
taskkill /F /IM VBCSCompiler.exe 2>nul
timeout /t 3 /nobreak >nul

echo [2/8] Deleting ALL bin/obj folders recursively...
for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s /q "%%d"

echo [3/8] Deleting NuGet cache...
dotnet nuget locals all --clear

echo [4/8] Cleaning solution...
dotnet clean

echo [5/8] Restoring packages...
dotnet restore --force

echo [6/8] Building in Debug mode...
dotnet build --configuration Debug --no-incremental --force

echo [7/8] Checking if build succeeded...
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ============================================
    echo   BUILD FAILED! Check errors above.
    echo ============================================
    pause
    exit /b 1
)

echo [8/8] SUCCESS! Ready to run.
echo.
echo ============================================
echo   ALL DONE - PRESS ANY KEY TO RUN
echo ============================================
pause

dotnet run --no-build
