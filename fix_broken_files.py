# -*- coding: utf-8 -*-
import re
import sys

if sys.platform == 'win32':
    import codecs
    sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')

# الملفات المكسورة
files_to_fix = [
    r'C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\BankAccountsForm.cs',
    r'C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\InvoicesListForm.cs',
    r'C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\CashBoxForm.cs'
]

def fix_broken_code(content):
    """
    يصلح الكود المكسور من السكريبت السابق
    """
    # Pattern: form.FormClosed += (s, args) =>
    #         {
    #             CODE
    #             }
    #         }; // comment
    #         form.Show();
    
    # Fix pattern 1: Multi-line broken FormClosed
    pattern1 = r'(\w+)\.FormClosed \+= \(s, args\) =>\s*\{\s*([^\}]+)\s*\}\s*\};\s*//\s*(.+?)\n\s*\1\.Show\(\);'
    
    def replace1(match):
        var_name = match.group(1)
        code = match.group(2).strip()
        comment = match.group(3).strip()
        return f'{var_name}.FormClosed += (s, args) => {code}; // {comment}\n            {var_name}.Show(); // ✅ نافذة مستقلة'
    
    content = re.sub(pattern1, replace1, content, flags=re.MULTILINE | re.DOTALL)
    
    return content

print("="*100)
print("إصلاح الأخطاء في الملفات")
print("="*100)
print()

fixed_count = 0

for file_path in files_to_fix:
    try:
        print(f"📄 معالجة: {file_path.split('\\')[-1]}")
        
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original = content
        content = fix_broken_code(content)
        
        if content != original:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            print(f"   ✅ تم الإصلاح")
            fixed_count += 1
        else:
            print(f"   ℹ️  لا يوجد تغييرات")
        
    except Exception as e:
        print(f"   ❌ خطأ: {e}")

print()
print("="*100)
print(f"تم إصلاح {fixed_count} ملفات")
print("="*100)
