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

Write-Host "Fixing locked files..."

foreach ($file in $lockedFiles) {
    try {
        $content = Get-Content -Path $file -Raw -Encoding UTF8
        
        $content = $content -replace 'this\.RightToLeft\s*=\s*RightToLeft\.No', 'this.RightToLeft = RightToLeft.Yes'
        $content = $content -replace 'this\.RightToLeft\s*=\s*System\.Windows\.Forms\.RightToLeft\.No', 'this.RightToLeft = System.Windows.Forms.RightToLeft.Yes'
        $content = $content -replace 'RightToLeft\s*=\s*RightToLeft\.No', 'RightToLeft = RightToLeft.Yes'
        $content = $content -replace 'this\.RightToLeftLayout\s*=\s*false', 'this.RightToLeftLayout = true'
        
        Set-Content -Path $file -Value $content -Encoding UTF8 -NoNewline
        Write-Host "Fixed: $file"
    }
    catch {
        Write-Host "Error: $file"
    }
}

Write-Host "Done!"
