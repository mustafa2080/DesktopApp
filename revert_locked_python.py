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

print("Reverting locked files to LTR...")
fixed = 0
errors = 0

for file_path in locked_files:
    max_retries = 3
    for attempt in range(max_retries):
        try:
            with open(file_path, 'r', encoding='utf-8-sig') as f:
                content = f.read()
            
            original = content
            
            # Revert to LTR (No instead of Yes)
            content = re.sub(r'this\.RightToLeft\s*=\s*RightToLeft\.Yes', 'this.RightToLeft = RightToLeft.No', content)
            content = re.sub(r'this\.RightToLeft\s*=\s*System\.Windows\.Forms\.RightToLeft\.Yes', 'this.RightToLeft = System.Windows.Forms.RightToLeft.No', content)
            content = re.sub(r'RightToLeft\s*=\s*RightToLeft\.Yes', 'RightToLeft = RightToLeft.No', content)
            content = re.sub(r'RightToLeft\s*=\s*System\.Windows\.Forms\.RightToLeft\.Yes', 'RightToLeft = System.Windows.Forms.RightToLeft.No', content)
            
            # Revert RightToLeftLayout (false instead of true)
            content = re.sub(r'this\.RightToLeftLayout\s*=\s*true', 'this.RightToLeftLayout = false', content)
            
            if content != original:
                with open(file_path, 'w', encoding='utf-8-sig', newline='') as f:
                    f.write(content)
                print(f"OK: {os.path.basename(file_path)}")
                fixed += 1
            else:
                print(f"SKIP: {os.path.basename(file_path)} - Already LTR")
                fixed += 1
            break
        except Exception as e:
            if attempt < max_retries - 1:
                time.sleep(0.5)
            else:
                print(f"ERROR: {os.path.basename(file_path)}")
                errors += 1

print(f"\nReverted: {fixed}/{len(locked_files)}, Errors: {errors}")
