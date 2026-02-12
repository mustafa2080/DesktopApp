import os
import re
import time

locked_files = [
    r"C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\AddEditTripForm.cs",
    r"C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\CashBoxForm.cs",
    r"C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\EditCashBoxForm.cs",
    r"C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\FlightBookingsListForm.cs",
    r"C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\MainForm.cs",
    r"C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\ServiceTypesForm.cs",
    r"C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\TripAccountingManagementForm.cs",
    r"C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\TripBookingsForm.cs",
    r"C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\TripDetailsForm.cs",
    r"C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\TripFinancialDetailsForm.cs",
    r"C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\TripsListForm.cs",
    r"C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\UmrahPackagesListForm.cs",
    r"C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\Admin\UserManagementForm.cs",
    r"C:\Users\musta\Desktop\pro\accountant\Presentation\Controls\DashboardControl.cs",
    r"C:\Users\musta\Desktop\pro\accountant\Presentation\Controls\HeaderControl.cs",
    r"C:\Users\musta\Desktop\pro\accountant\Presentation\Controls\SidebarControl.cs"
]

print("Converting locked files to RTL...")
fixed = 0
errors = 0

for file_path in locked_files:
    max_retries = 3
    for attempt in range(max_retries):
        try:
            with open(file_path, 'r', encoding='utf-8-sig') as f:
                content = f.read()
            
            original = content
            
            # Convert to RTL (Yes instead of No)
            content = re.sub(r'this\.RightToLeft\s*=\s*RightToLeft\.No', 'this.RightToLeft = RightToLeft.Yes', content)
            content = re.sub(r'this\.RightToLeft\s*=\s*System\.Windows\.Forms\.RightToLeft\.No', 'this.RightToLeft = System.Windows.Forms.RightToLeft.Yes', content)
            content = re.sub(r'RightToLeft\s*=\s*RightToLeft\.No', 'RightToLeft = RightToLeft.Yes', content)
            content = re.sub(r'RightToLeft\s*=\s*System\.Windows\.Forms\.RightToLeft\.No', 'RightToLeft = System.Windows.Forms.RightToLeft.Yes', content)
            
            # Convert RightToLeftLayout (true instead of false)
            content = re.sub(r'this\.RightToLeftLayout\s*=\s*false', 'this.RightToLeftLayout = true', content)
            
            if content != original:
                with open(file_path, 'w', encoding='utf-8-sig', newline='') as f:
                    f.write(content)
                print(f"OK: {os.path.basename(file_path)}")
                fixed += 1
            else:
                print(f"SKIP: {os.path.basename(file_path)} - Already RTL")
                fixed += 1
            break
        except Exception as e:
            if attempt < max_retries - 1:
                time.sleep(0.5)
            else:
                print(f"ERROR: {os.path.basename(file_path)}")
                errors += 1

print(f"\nConverted: {fixed}/{len(locked_files)}, Errors: {errors}")
