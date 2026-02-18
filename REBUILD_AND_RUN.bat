@echo off
echo Closing any running instances...
taskkill /F /IM accountant.exe 2>nul
taskkill /F /IM GraceWay.AccountingSystem.exe 2>nul

echo Cleaning project...
cd /d "C:\Users\musta\Desktop\pro\accountant"
dotnet clean

echo Building project (force rebuild)...
dotnet build --no-incremental

echo.
echo Done! Press any key to run the application...
pause

dotnet run
