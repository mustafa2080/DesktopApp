# Fix RightToLeftLayout in all Forms

$formsPath = "C:\Users\musta\Desktop\pro\accountant\Presentation\Forms"

# Get all .cs files recursively
$files = Get-ChildItem -Path $formsPath -Filter "*.cs" -Recurse

$totalFixed = 0
$filesFixed = @()

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
    
    # Check if file contains RightToLeftLayout = true
    if ($content -match "RightToLeftLayout\s*=\s*true") {
        # Replace all occurrences
        $newContent = $content -replace "this\.RightToLeftLayout\s*=\s*true;", "this.RightToLeftLayout = false;"
        $newContent = $newContent -replace "RightToLeftLayout\s*=\s*true", "RightToLeftLayout = false"
        
        # Also fix RightToLeft.Yes if exists on same form
        $newContent = $newContent -replace "this\.RightToLeft\s*=\s*RightToLeft\.Yes;", "this.RightToLeft = RightToLeft.No;"
        $newContent = $newContent -replace "RightToLeft\s*=\s*RightToLeft\.Yes", "RightToLeft = RightToLeft.No"
        $newContent = $newContent -replace "this\.RightToLeft\s*=\s*System\.Windows\.Forms\.RightToLeft\.Yes;", "this.RightToLeft = System.Windows.Forms.RightToLeft.No;"
        
        # Write back to file
        Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8 -NoNewline
        
        $totalFixed++
        $filesFixed += $file.FullName
        Write-Host "Fixed: $($file.FullName)" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Total files fixed: $totalFixed" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($filesFixed.Count -gt 0) {
    Write-Host "Files that were fixed:" -ForegroundColor Cyan
    foreach ($f in $filesFixed) {
        $shortName = Split-Path -Leaf $f
        Write-Host "  - $shortName" -ForegroundColor Gray
    }
}
