$formsPath = "C:\Users\musta\Desktop\pro\accountant\Presentation\Forms"
$controlsPath = "C:\Users\musta\Desktop\pro\accountant\Presentation\Controls"

Write-Host "=== RTL Verification Report ===" -ForegroundColor Cyan
Write-Host ""

$allFiles = @()
$allFiles += Get-ChildItem -Path $formsPath -Filter "*.cs" -Recurse | Where-Object { $_.Name -notlike "*.Designer.cs" }
$allFiles += Get-ChildItem -Path $controlsPath -Filter "*.cs" -Recurse

$totalFiles = $allFiles.Count
$rtlFiles = 0
$ltrFiles = 0
$mixedFiles = 0

foreach ($file in $allFiles) {
    $content = Get-Content -Path $file.FullName -Raw
    
    $hasRTL = $content -match 'RightToLeft\s*=\s*RightToLeft\.Yes'
    $hasLTR = $content -match 'RightToLeft\s*=\s*RightToLeft\.No'
    
    if ($hasRTL -and -not $hasLTR) {
        $rtlFiles++
    }
    elseif ($hasLTR -and -not $hasRTL) {
        $ltrFiles++
        Write-Host "LTR: $($file.Name)" -ForegroundColor Red
    }
    elseif ($hasRTL -and $hasLTR) {
        $mixedFiles++
        Write-Host "MIXED: $($file.Name)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "Total Files: $totalFiles"
Write-Host "RTL (Correct): $rtlFiles" -ForegroundColor Green
Write-Host "LTR (Wrong): $ltrFiles" -ForegroundColor Red
Write-Host "Mixed: $mixedFiles" -ForegroundColor Yellow
Write-Host ""

if ($ltrFiles -eq 0 -and $mixedFiles -eq 0) {
    Write-Host "SUCCESS: All files are RTL!" -ForegroundColor Green
} else {
    Write-Host "WARNING: Some files still have LTR!" -ForegroundColor Red
}
