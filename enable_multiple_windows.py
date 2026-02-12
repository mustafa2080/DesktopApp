# -*- coding: utf-8 -*-
import os
import re
import sys

if sys.platform == 'win32':
    import codecs
    sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')

base_path = r'C:\Users\musta\Desktop\pro\accountant\Presentation\Forms'

# القوائم والنماذج المهمة اللي عايزينها تفتح كنوافذ منفصلة
forms_to_fix = {
    'CustomersListForm.cs': [
        ('CustomerDetailsForm', 'عرض تفاصيل عميل'),
        ('AddEditCustomerForm', 'إضافة/تعديل عميل')
    ],
    'SuppliersListForm.cs': [
        ('SupplierDetailsForm', 'عرض تفاصيل مورد'),
        ('AddEditSupplierForm', 'إضافة/تعديل مورد')
    ],
    'ReservationsListForm.cs': [
        ('ReservationDetailsForm', 'عرض تفاصيل حجز'),
        ('AddEditReservationForm', 'إضافة/تعديل حجز')
    ],
    'InvoicesListForm.cs': [
        ('InvoiceDetailsForm', 'عرض تفاصيل فاتورة'),
        ('AddEditInvoiceForm', 'إضافة/تعديل فاتورة'),
        ('AddSalesInvoiceForm', 'إضافة فاتورة مبيعات'),
        ('AddPurchaseInvoiceForm', 'إضافة فاتورة مشتريات')
    ],
    'UmrahPackagesListForm.cs': [
        ('UmrahPackageDetailsForm', 'عرض تفاصيل باقة عمرة'),
        ('AddEditUmrahPackageForm', 'إضافة/تعديل باقة عمرة')
    ]
}

modified_files = []
total_changes = 0

print("="*120)
print("Converting modal dialogs to independent windows")
print("="*120)
print()

def convert_showdialog_to_show(content, form_class_name):
    """
    يحول ShowDialog إلى Show للنماذج المحددة
    """
    changes = 0
    
    # Pattern 1: form.ShowDialog() -> form.Show()
    pattern1 = rf'(new\s+{form_class_name}\s*\([^)]*\));\s*form\.ShowDialog\(\);'
    if re.search(pattern1, content):
        content = re.sub(
            pattern1,
            r'\1;\n        form.Show(); // ✅ فتح كنافذة منفصلة',
            content
        )
        changes += 1
    
    # Pattern 2: if (form.ShowDialog() == DialogResult.OK) -> form.Show() + FormClosed event
    pattern2 = rf'(var\s+form\s*=\s*new\s+{form_class_name}\s*\([^)]*\);)\s*if\s*\(\s*form\.ShowDialog\(\)\s*==\s*DialogResult\.OK\s*\)\s*\{{\s*([^}}]+)\s*\}}'
    matches = re.findall(pattern2, content, re.MULTILINE | re.DOTALL)
    if matches:
        for match in matches:
            form_creation = match[0]
            reload_code = match[1].strip()
            
            replacement = f'''{form_creation}
        form.FormClosed += (s, args) => {reload_code.strip()}; // ✅ تحديث عند الإغلاق
        form.Show(); // ✅ فتح كنافذة منفصلة'''
            
            old_text = f"{form_creation}\n        if (form.ShowDialog() == DialogResult.OK)\n        {{\n            {reload_code}\n        }}"
            
            content = content.replace(old_text, replacement)
            changes += 1
    
    return content, changes

for filename, form_classes in forms_to_fix.items():
    file_path = os.path.join(base_path, filename)
    
    if not os.path.exists(file_path):
        print(f"⚠️  File not found: {filename}")
        continue
    
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        file_changes = 0
        
        for form_class, description in form_classes:
            content, changes = convert_showdialog_to_show(content, form_class)
            file_changes += changes
        
        if file_changes > 0:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            
            modified_files.append((filename, file_changes))
            total_changes += file_changes
            
            print(f"✅ Modified: {filename}")
            print(f"   Changes: {file_changes}")
            for form_class, desc in form_classes:
                print(f"   - {form_class} ({desc})")
            print()
        else:
            print(f"ℹ️  No changes needed: {filename}")
    
    except Exception as e:
        print(f"❌ Error processing {filename}: {e}")

print()
print("="*120)
print("SUMMARY")
print("="*120)
print(f"Files modified: {len(modified_files)}")
print(f"Total changes: {total_changes}")
print()

if modified_files:
    print("Modified files:")
    for filename, changes in modified_files:
        print(f"  ✅ {filename} ({changes} changes)")
    
    print()
    print("SUCCESS! You can now open multiple windows simultaneously!")
    print("All main forms will open as independent windows instead of modal dialogs.")
else:
    print("No files were modified.")

print()
print("Done!")
