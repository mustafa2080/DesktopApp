# Revert RTL back to LTR (Left to Right)
$projectPath = "C:\Users\musta\Desktop\pro\accountant\Presentation\Forms"
$files = Get-ChildItem -Path $projectPath -Filter "*.cs" -Recurse

Write-Host "Reverting back to LTR (Left to Right)..." -ForegroundColor Yellow

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
    
    # Revert RightToLeft Yes to No
    $content = $content -replace 'this\.RightToLeft\s*=\s*RightToLeft\.Yes', 'this.RightToLeft = RightToLeft.No'
    $content = $content -replace 'this\.RightToLeft\s*=\s*System\.Windows\.Forms\.RightToLeft\.Yes', 'this.RightToLeft = System.Windows.Forms.RightToLeft.No'
    $content = $content -replace 'RightToLeft\s*=\s*RightToLeft\.Yes', 'RightToLeft = RightToLeft.No'
    $content = $content -replace 'RightToLeft\s*=\s*System\.Windows\.Forms\.RightToLeft\.Yes', 'RightToLeft = System.Windows.Forms.RightToLeft.No'
    
    # Revert RightToLeftLayout true to false
    $content = $content -replace 'this\.RightToLeftLayout\s*=\s*true', 'this.RightToLeftLayout = false'
    
    Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
    Write-Host "Reverted: $($file.Name)"
}

Write-Host "Forms reverted to LTR!" -ForegroundColor Green
