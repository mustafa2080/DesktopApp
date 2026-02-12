# -*- coding: utf-8 -*-
import os
import re
import sys

if sys.platform == 'win32':
    import codecs
    sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')

base_path = r'C:\Users\musta\Desktop\pro\accountant\Presentation\Forms'

# الأقسام المطلوب تعديلها
forms_to_fix = {
    # الحجوزات
    'ReservationsListForm.cs': [
        'ReservationDetailsForm',
        'AddEditReservationForm',
        'AddReservationForm',
        'EditReservationForm'
    ],
    
    # الطيران
    'FlightBookingsListForm.cs': [
        'FlightBookingDetailsForm',
        'AddEditFlightBookingForm',
        'AddFlightBookingForm',
        'EditFlightBookingForm'
    ],
    
    # الفواتير
    'InvoicesListForm.cs': [
        'InvoiceDetailsForm',
        'AddSalesInvoiceForm',
        'AddPurchaseInvoiceForm',
        'EditInvoiceForm',
        'AddEditInvoiceForm'
    ],
    
    # الخزنة
    'CashBoxForm.cs': [
        'CashTransactionDetailsForm',
        'AddCashTransactionForm',
        'EditCashTransactionForm'
    ],
    
    # البنوك
    'BankAccountsForm.cs': [
        'BankAccountDetailsForm',
        'AddEditBankAccountForm',
        'BankTransactionDetailsForm',
        'AddBankTransactionForm'
    ],
    'BanksListForm.cs': [
        'BankAccountDetailsForm',
        'AddEditBankAccountForm',
        'BankTransactionDetailsForm',
        'AddBankTransactionForm'
    ],
    
    # التقارير المحاسبية
    'AccountingReportsForm.cs': [
        'ReportViewerForm',
        'ReportDetailsForm'
    ],
    'FinancialReportsForm.cs': [
        'ReportViewerForm',
        'ReportDetailsForm'
    ]
}

modified_files = []
total_changes = 0

print("="*120)
print("تحويل النوافذ المقيدة إلى نوافذ مستقلة")
print("="*120)
print()

def convert_file(file_path, form_classes):
    """
    يحول ShowDialog إلى Show في الملف
    """
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
    except FileNotFoundError:
        return 0, f"الملف غير موجود"
    except Exception as e:
        return 0, f"خطأ في القراءة: {e}"
    
    original_content = content
    changes = 0
    
    for form_class in form_classes:
        # Pattern 1: Direct ShowDialog call
        # form.ShowDialog() -> form.Show()
        pattern1 = rf'(\b{form_class}\s+form[^;]*;)\s*form\.ShowDialog\(\);'
        if re.search(pattern1, content, re.MULTILINE):
            content = re.sub(
                pattern1,
                r'\1\n        form.Show(); // ✅ نافذة مستقلة',
                content,
                flags=re.MULTILINE
            )
            changes += 1
        
        # Pattern 2: new Form().ShowDialog()
        pattern2 = rf'(new\s+{form_class}\s*\([^)]*\))\.ShowDialog\(\)'
        if re.search(pattern2, content):
            content = re.sub(
                pattern2,
                r'var form = \1;\n        form.Show(); // ✅ نافذة مستقلة',
                content
            )
            changes += 1
        
        # Pattern 3: if (form.ShowDialog() == DialogResult.OK)
        pattern3 = rf'if\s*\(\s*form\.ShowDialog\(\)\s*==\s*DialogResult\.OK\s*\)'
        if re.search(pattern3, content):
            # نبحث عن الكود الكامل
            pattern3_full = rf'(var\s+form\s*=\s*new\s+{form_class}\s*\([^)]*\);)\s*if\s*\(\s*form\.ShowDialog\(\)\s*==\s*DialogResult\.OK\s*\)\s*\{{\s*([^}}]+)\s*\}}'
            matches = re.findall(pattern3_full, content, re.MULTILINE | re.DOTALL)
            
            for match in matches:
                form_creation = match[0]
                reload_code = match[1].strip()
                
                old_block = f"{form_creation}\n        if (form.ShowDialog() == DialogResult.OK)\n        {{\n            {reload_code}\n        }}"
                
                new_block = f"{form_creation}\n        form.FormClosed += (s, args) => {{ {reload_code.strip()} }}; // ✅ تحديث عند الإغلاق\n        form.Show(); // ✅ نافذة مستقلة"
                
                if old_block in content:
                    content = content.replace(old_block, new_block)
                    changes += 1
        
        # Pattern 4: using (var form = ...) form.ShowDialog()
        pattern4 = rf'using\s*\(\s*var\s+form\s*=\s*new\s+{form_class}\s*\([^)]*\)\s*\)\s*{{\s*form\.ShowDialog\(\);?\s*}}'
        if re.search(pattern4, content, re.MULTILINE | re.DOTALL):
            content = re.sub(
                pattern4,
                lambda m: m.group(0).replace('using (', '// using (').replace('form.ShowDialog()', 'form.Show(); // ✅ نافذة مستقلة'),
                content,
                flags=re.MULTILINE | re.DOTALL
            )
            changes += 1
    
    if changes > 0:
        try:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            return changes, "تم التعديل بنجاح"
        except Exception as e:
            return 0, f"خطأ في الكتابة: {e}"
    
    return 0, "لا يوجد تغييرات"

# معالجة كل ملف
for filename, form_classes in forms_to_fix.items():
    file_path = os.path.join(base_path, filename)
    
    print(f"📄 معالجة: {filename}")
    
    changes, message = convert_file(file_path, form_classes)
    
    if changes > 0:
        modified_files.append((filename, changes))
        total_changes += changes
        print(f"   ✅ {message} - {changes} تعديلات")
        for form_class in form_classes:
            print(f"      - {form_class}")
    else:
        print(f"   ℹ️  {message}")
    
    print()

print()
print("="*120)
print("الملخص")
print("="*120)
print(f"الملفات المعدلة: {len(modified_files)}")
print(f"إجمالي التعديلات: {total_changes}")
print()

if modified_files:
    print("الملفات المعدلة:")
    for filename, changes in modified_files:
        print(f"  ✅ {filename} ({changes} تعديلات)")
    
    print()
    print("🎉 تم بنجاح! الآن يمكنك فتح عدة نوافذ في نفس الوقت في جميع الأقسام!")
else:
    print("⚠️ لم يتم تعديل أي ملفات")

print()
print("تم الانتهاء!")
