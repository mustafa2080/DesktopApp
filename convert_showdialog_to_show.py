# -*- coding: utf-8 -*-
import os
import re
import sys

if sys.platform == 'win32':
    import codecs
    sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')

base_path = r'C:\Users\musta\Desktop\pro\accountant\Presentation'

# النوافذ الرئيسية اللي عايزينها تفتح بـ Show() بدل ShowDialog()
main_forms = [
    'TripDetailsForm',
    'AddEditTripForm',
    'TripListForm',
    'ReservationListForm',
    'CustomerListForm',
    'SupplierListForm',
    'InvoiceListForm',
    'CashBoxForm',
    'JournalEntriesForm',
    'AccountsTreeForm',
    'UmrahPackageListForm'
]

modified_files = []
total_replacements = 0

print("="*120)
print("Converting ShowDialog() to Show() for main forms only")
print("="*120)
print()
print("Forms to convert:")
for form in main_forms:
    print(f"  - {form}")
print()

# البحث في جميع ملفات .cs
for root, dirs, files in os.walk(base_path):
    for file in files:
        if file.endswith('.cs'):
            file_path = os.path.join(root, file)
            
            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    content = f.read()
                
                original_content = content
                file_modified = False
                file_replacements = 0
                
                # البحث عن كل form في القائمة
                for form_name in main_forms:
                    # Pattern: new FormName(...).ShowDialog()
                    pattern = rf'(new\s+{form_name}\s*\([^)]*\))\s*\.ShowDialog\(\)'
                    
                    matches = re.findall(pattern, content)
                    if matches:
                        # الاستبدال
                        content = re.sub(pattern, r'\1.Show()', content)
                        file_replacements += len(matches)
                        file_modified = True
                
                # حفظ الملف إذا تم التعديل
                if file_modified:
                    with open(file_path, 'w', encoding='utf-8') as f:
                        f.write(content)
                    
                    total_replacements += file_replacements
                    modified_files.append((file_path, file_replacements))
                    
                    print(f"Modified: {os.path.relpath(file_path, base_path)}")
                    print(f"  Replacements: {file_replacements}")
                    
            except Exception as e:
                print(f"Error processing {file}: {e}")

print()
print("="*120)
print("SUMMARY")
print("="*120)
print(f"Total files modified: {len(modified_files)}")
print(f"Total replacements: {total_replacements}")
print()

if modified_files:
    print("Modified files:")
    for file_path, count in modified_files:
        print(f"  - {os.path.relpath(file_path, base_path)} ({count} changes)")
    
    print()
    print("SUCCESS! Main forms will now open as non-modal windows.")
    print("You can now open multiple windows at the same time!")
else:
    print("No files were modified - patterns not found.")

print()
print("Done!")
