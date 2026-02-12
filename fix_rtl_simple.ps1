# Fix RTL for all forms
$projectPath = "C:\Users\musta\Desktop\pro\accountant\Presentation\Forms"
$files = Get-ChildItem -Path $projectPath -Filter "*.cs" -Recurse

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
    
    # Fix RightToLeft No to Yes
    $content = $content -replace 'this\.RightToLeft\s*=\s*RightToLeft\.No', 'this.RightToLeft = RightToLeft.Yes'
    $content = $content -replace 'this\.RightToLeft\s*=\s*System\.Windows\.Forms\.RightToLeft\.No', 'this.RightToLeft = System.Windows.Forms.RightToLeft.Yes'
    
    # Fix RightToLeftLayout false to true
    $content = $content -replace 'this\.RightToLeftLayout\s*=\s*false', 'this.RightToLeftLayout = true'
    
    # Fix any RightToLeft.No in controls
    $content = $content -replace 'RightToLeft\s*=\s*RightToLeft\.No', 'RightToLeft = RightToLeft.Yes'
    $content = $content -replace 'RightToLeft\s*=\s*System\.Windows\.Forms\.RightToLeft\.No', 'RightToLeft = System.Windows.Forms.RightToLeft.Yes'
    
    Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
    Write-Host "Fixed: $($file.Name)"
}

Write-Host "Done!" -ForegroundColor Green
