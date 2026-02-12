# Re-fix the locked files
$lockedFiles = @(
    "C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\AddEditTripForm.cs",
    "C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\CashBoxForm.cs",
    "C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\EditCashBoxForm.cs",
    "C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\FlightBookingsListForm.cs",
    "C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\MainForm.cs",
    "C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\ServiceTypesForm.cs",
    "C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\TripAccountingManagementForm.cs",
    "C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\TripBookingsForm.cs",
    "C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\TripDetailsForm.cs",
    "C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\TripFinancialDetailsForm.cs",
    "C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\TripsListForm.cs",
    "C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\UmrahPackagesListForm.cs",
    "C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\Admin\UserManagementForm.cs",
    "C:\Users\musta\Desktop\pro\accountant\Presentation\Controls\DashboardControl.cs",
    "C:\Users\musta\Desktop\pro\accountant\Presentation\Controls\HeaderControl.cs",
    "C:\Users\musta\Desktop\pro\accountant\Presentation\Controls\SidebarControl.cs"
)

Write-Host "محاولة إصلاح الملفات المقفلة..." -ForegroundColor Yellow
Write-Host "برجاء إغلاق Visual Studio أو أي محرر أولاً!" -ForegroundColor Red
Start-Sleep -Seconds 5

$fixedCount = 0
foreach ($file in $lockedFiles) {
    try {
        $content = Get-Content -Path $file -Raw -Encoding UTF8
        
        # Fix RightToLeft
        $content = $content -replace 'this\.RightToLeft\s*=\s*RightToLeft\.No', 'this.RightToLeft = RightToLeft.Yes'
        $content = $content -replace 'this\.RightToLeft\s*=\s*System\.Windows\.Forms\.RightToLeft\.No', 'this.RightToLeft = System.Windows.Forms.RightToLeft.Yes'
        $content = $content -replace 'RightToLeft\s*=\s*RightToLeft\.No', 'RightToLeft = RightToLeft.Yes'
        $content = $content -replace 'RightToLeft\s*=\s*System\.Windows\.Forms\.RightToLeft\.No', 'RightToLeft = System.Windows.Forms.RightToLeft.Yes'
        
        # Fix RightToLeftLayout
        $content = $content -replace 'this\.RightToLeftLayout\s*=\s*false', 'this.RightToLeftLayout = true'
        
        Set-Content -Path $file -Value $content -Encoding UTF8 -NoNewline
        Write-Host "  ✓ تم إصلاح: $(Split-Path $file -Leaf)" -ForegroundColor Green
        $fixedCount++
    }
    catch {
        Write-Host "  ✗ فشل: $(Split-Path $file -Leaf) - $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`nتم إصلاح $fixedCount من $($lockedFiles.Count) ملف" -ForegroundColor Cyan
