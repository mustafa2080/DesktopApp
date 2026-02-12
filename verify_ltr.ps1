$formsPath = "C:\Users\musta\Desktop\pro\accountant\Presentation\Forms"
$controlsPath = "C:\Users\musta\Desktop\pro\accountant\Presentation\Controls"

Write-Host "=== Verification Report ===" -ForegroundColor Cyan
Write-Host ""

$allFiles = @()
$allFiles += Get-ChildItem -Path $formsPath -Filter "*.cs" -Recurse | Where-Object { $_.Name -notlike "*.Designer.cs" }
$allFiles += Get-ChildItem -Path $controlsPath -Filter "*.cs" -Recurse

$totalFiles = $allFiles.Count
$ltrFiles = 0
$rtlFiles = 0
$mixedFiles = 0

foreach ($file in $allFiles) {
    $content = Get-Content -Path $file.FullName -Raw
    
    $hasLTR = $content -match 'RightToLeft\s*=\s*RightToLeft\.No'
    $hasRTL = $content -match 'RightToLeft\s*=\s*RightToLeft\.Yes'
    
    if ($hasRTL -and -not $hasLTR) {
        $rtlFiles++
        Write-Host "RTL: $($file.Name)" -ForegroundColor Red
    }
    elseif ($hasLTR -and -not $hasRTL) {
        $ltrFiles++
    }
    elseif ($hasRTL -and $hasLTR) {
        $mixedFiles++
        Write-Host "MIXED: $($file.Name)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "Total Files: $totalFiles"
Write-Host "LTR (Correct): $ltrFiles" -ForegroundColor Green
Write-Host "RTL (Wrong): $rtlFiles" -ForegroundColor Red
Write-Host "Mixed: $mixedFiles" -ForegroundColor Yellow
Write-Host ""

if ($rtlFiles -eq 0 -and $mixedFiles -eq 0) {
    Write-Host "SUCCESS: All files are LTR!" -ForegroundColor Green
} else {
    Write-Host "WARNING: Some files still have RTL!" -ForegroundColor Red
}
