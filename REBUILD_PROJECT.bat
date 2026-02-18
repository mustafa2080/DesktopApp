@echo off
echo ========================================
echo   اعادة بناء مشروع المحاسبة
echo   Rebuilding Accountant Project
echo ========================================
echo.

cd /d "%~dp0"

echo [1/4] تنظيف المشروع (Cleaning)...
dotnet clean

echo.
echo [2/4] حذف ملفات bin و obj القديمة...
if exist "bin" rmdir /s /q "bin"
if exist "obj" rmdir /s /q "obj"

echo.
echo [3/4] اعادة بناء المشروع (Rebuilding)...
dotnet build --configuration Debug

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ========================================
    echo   خطأ في البناء! 
    echo   Build Error!
    echo ========================================
    pause
    exit /b 1
)

echo.
echo ========================================
echo   تم البناء بنجاح!
echo   Build Successful!
echo ========================================
echo.
echo الان يمكنك تشغيل البرنامج
echo Now you can run the application
echo.
pause
