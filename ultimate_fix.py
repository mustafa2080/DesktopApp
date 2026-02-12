# -*- coding: utf-8 -*-
# هذا السكريبت يصلح المشكلة بالكامل - يرجع ShowDialog لو موجودة أخطاء ثم يعيد التطبيق بشكل صحيح

import re
import sys

if sys.platform == 'win32':
    import codecs
    sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')

files = [
    r'C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\BankAccountsForm.cs',
    r'C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\InvoicesListForm.cs',
    r'C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\CashBoxForm.cs',
    r'C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\ReservationsListForm.cs',
    r'C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\FlightBookingsListForm.cs'
]

def completely_fix_file(content):
    """
    يصلح الملف بالكامل - يرجع أي تعديلات خاطئة ثم يطبق التعديل الصحيح
    """
    
    # Step 1: نرجع أي تعديلات خاطئة
    # نرجع الـ broken FormClosed patterns
    content = re.sub(
        r'(\w+)\.FormClosed \+= \(s, args\) => ([^\n;]+)\s*\}\s*\};?\s*//[^\n]*\n\s*\1\.Show\(\);',
        r'\1.ShowDialog();',
        content,
        flags=re.MULTILINE
    )
    
    # Step 2: نطبق التعديل الصحيح
    # Pattern: var form = new XxxForm(...);
    #          if (form.ShowDialog() == DialogResult.OK)
    #          {
    #              LoadXxx();
    #          }
    pattern = r'(var\s+(\w+)\s*=\s*new\s+\w+Form[^;]+;)\s*if\s*\(\s*\2\.ShowDialog\(\)\s*==\s*DialogResult\.OK\s*\)\s*\{\s*([^}]+)\}'
    
    def replace_func(match):
        form_declaration = match.group(1)
        form_var = match.group(2)
        reload_code = match.group(3).strip()
        
        return f'''{form_declaration}
            {form_var}.FormClosed += (s, args) => {{ {reload_code} }};
            {form_var}.Show();'''
    
    content = re.sub(pattern, replace_func, content, flags=re.MULTILINE | re.DOTALL)
    
    # Pattern 2: Direct ShowDialog without if
    # form.ShowDialog(); -> form.Show();
    content = re.sub(
        r'([a-zA-Z_][a-zA-Z0-9_]*)\.ShowDialog\(\);',
        r'\1.Show();',
        content
    )
    
    return content

print("="*100)
print("إصلاح شامل ونهائي")
print("="*100)
print()

for file_path in files:
    try:
        filename = file_path.split('\\')[-1]
        print(f"📄 {filename}")
        
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()
        except FileNotFoundError:
            print(f"   ⚠️  الملف غير موجود")
            continue
        
        original = content
        content = completely_fix_file(content)
        
        if content != original:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            print("   ✅ تم الإصلاح والتعديل بنجاح")
        else:
            print("   ℹ️  لا يوجد تغييرات")
        
    except Exception as e:
        print(f"   ❌ خطأ: {e}")

print()
print("="*100)
print("تم الانتهاء! جرّب dotnet build الآن")
print("="*100)
