@echo off
echo ============================================
echo   FORCE COMPLETE REBUILD
echo ============================================
echo.

cd /d "C:\Users\musta\Desktop\pro\accountant"

echo [1/5] Killing any running processes...
taskkill /F /IM accountant.exe 2>nul
taskkill /F /IM GraceWay.AccountingSystem.exe 2>nul
taskkill /F /IM dotnet.exe 2>nul
timeout /t 2 /nobreak >nul

echo [2/5] Deleting bin and obj folders...
if exist "bin" rd /s /q "bin"
if exist "obj" rd /s /q "obj"
if exist "Presentation\bin" rd /s /q "Presentation\bin"
if exist "Presentation\obj" rd /s /q "Presentation\obj"
if exist "Domain\bin" rd /s /q "Domain\bin"
if exist "Domain\obj" rd /s /q "Domain\obj"
if exist "Infrastructure\bin" rd /s /q "Infrastructure\bin"
if exist "Infrastructure\obj" rd /s /q "Infrastructure\obj"

echo [3/5] Cleaning solution...
dotnet clean

echo [4/5] Restoring packages...
dotnet restore

echo [5/5] Building project (FULL REBUILD)...
dotnet build --no-incremental

echo.
echo ============================================
if %ERRORLEVEL% EQU 0 (
    echo   BUILD SUCCESSFUL!
    echo ============================================
    echo.
    echo Press any key to run the application...
    pause
    dotnet run
) else (
    echo   BUILD FAILED!
    echo ============================================
    echo.
    echo Check the errors above.
    pause
)
