# Convert all to RTL (Right to Left)
$projectPath = "C:\Users\musta\Desktop\pro\accountant\Presentation\Forms"
$files = Get-ChildItem -Path $projectPath -Filter "*.cs" -Recurse

Write-Host "Converting to RTL (Right to Left)..." -ForegroundColor Green

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
    
    # Convert to RTL
    $content = $content -replace 'this\.RightToLeft\s*=\s*RightToLeft\.No', 'this.RightToLeft = RightToLeft.Yes'
    $content = $content -replace 'this\.RightToLeft\s*=\s*System\.Windows\.Forms\.RightToLeft\.No', 'this.RightToLeft = System.Windows.Forms.RightToLeft.Yes'
    $content = $content -replace 'RightToLeft\s*=\s*RightToLeft\.No', 'RightToLeft = RightToLeft.Yes'
    $content = $content -replace 'RightToLeft\s*=\s*System\.Windows\.Forms\.RightToLeft\.No', 'RightToLeft = System.Windows.Forms.RightToLeft.Yes'
    
    # Convert RightToLeftLayout
    $content = $content -replace 'this\.RightToLeftLayout\s*=\s*false', 'this.RightToLeftLayout = true'
    
    Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
    Write-Host "Converted: $($file.Name)"
}

Write-Host "Forms converted to RTL!" -ForegroundColor Green
