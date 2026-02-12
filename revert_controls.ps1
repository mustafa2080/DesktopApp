# Revert Controls to LTR
$projectPath = "C:\Users\musta\Desktop\pro\accountant\Presentation\Controls"
$files = Get-ChildItem -Path $projectPath -Filter "*.cs" -Recurse

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
    
    # Revert to LTR
    $content = $content -replace 'this\.RightToLeft\s*=\s*RightToLeft\.Yes', 'this.RightToLeft = RightToLeft.No'
    $content = $content -replace 'RightToLeft\s*=\s*RightToLeft\.Yes', 'RightToLeft = RightToLeft.No'
    $content = $content -replace 'this\.RightToLeftLayout\s*=\s*true', 'this.RightToLeftLayout = false'
    
    Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
    Write-Host "Reverted: $($file.Name)"
}

Write-Host "Controls reverted!" -ForegroundColor Green
