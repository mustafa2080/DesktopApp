# Fix RTL for all forms in the project
Write-Host "بدء إصلاح مشكلة RTL في جميع الـ Forms..." -ForegroundColor Green

$projectPath = "C:\Users\musta\Desktop\pro\accountant\Presentation\Forms"
$filesFixed = 0
$filesProcessed = 0

# Get all .cs files (not Designer files)
$csFiles = Get-ChildItem -Path $projectPath -Filter "*.cs" -Recurse | Where-Object { 
    $_.Name -notlike "*.Designer.cs" -and 
    $_.Name -notlike "*.resx" -and
    $_.Extension -eq ".cs"
}

foreach ($file in $csFiles) {
    $filesProcessed++
    $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
    $modified = $false
    
    # Fix RightToLeft = No to Yes
    if ($content -match 'this\.RightToLeft\s*=\s*RightToLeft\.No') {
        $content = $content -replace 'this\.RightToLeft\s*=\s*RightToLeft\.No', 'this.RightToLeft = RightToLeft.Yes'
        $modified = $true
        Write-Host "  ✓ تم تصحيح RightToLeft في: $($file.Name)" -ForegroundColor Yellow
    }
    
    # Fix RightToLeftLayout = false to true
    if ($content -match 'this\.RightToLeftLayout\s*=\s*false') {
        $content = $content -replace 'this\.RightToLeftLayout\s*=\s*false', 'this.RightToLeftLayout = true'
        $modified = $true
        Write-Host "  ✓ تم تصحيح RightToLeftLayout في: $($file.Name)" -ForegroundColor Yellow
    }
    
    # Fix any Panel or Control with RightToLeft.No
    if ($content -match 'RightToLeft\s*=\s*RightToLeft\.No') {
        $content = $content -replace 'RightToLeft\s*=\s*RightToLeft\.No', 'RightToLeft = RightToLeft.Yes'
        $modified = $true
        Write-Host "  ✓ تم تصحيح RightToLeft للعناصر في: $($file.Name)" -ForegroundColor Yellow
    }
    
    if ($modified) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        $filesFixed++
    }
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "تم معالجة: $filesProcessed ملف" -ForegroundColor Cyan
Write-Host "تم إصلاح: $filesFixed ملف" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green

# Now fix the Designer files
Write-Host "الآن سيتم إصلاح ملفات Designer..." -ForegroundColor Green

$designerFiles = Get-ChildItem -Path $projectPath -Filter "*.Designer.cs" -Recurse

$designerFilesFixed = 0

foreach ($file in $designerFiles) {
    $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
    $modified = $false
    
    # Fix RightToLeft setting in Designer files
    if ($content -match 'this\.RightToLeft\s*=\s*System\.Windows\.Forms\.RightToLeft\.No') {
        $content = $content -replace 'this\.RightToLeft\s*=\s*System\.Windows\.Forms\.RightToLeft\.No', 'this.RightToLeft = System.Windows.Forms.RightToLeft.Yes'
        $modified = $true
    }
    
    if ($content -match 'this\.RightToLeftLayout\s*=\s*false') {
        $content = $content -replace 'this\.RightToLeftLayout\s*=\s*false', 'this.RightToLeftLayout = true'
        $modified = $true
    }
    
    if ($modified) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        $designerFilesFixed++
        Write-Host "  ✓ تم تصحيح: $($file.Name)" -ForegroundColor Yellow
    }
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "تم إصلاح $designerFilesFixed من ملفات Designer" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green
Write-Host "✓ تم الانتهاء بنجاح!" -ForegroundColor Green
