@echo off
cd /d "C:\Users\musta\Desktop\pro\accountant"
dotnet ef migrations add AddTripIdToBankTransfer
dotnet ef database update
pause
