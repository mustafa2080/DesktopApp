# -*- coding: utf-8 -*-
import re
import sys

if sys.platform == 'win32':
    import codecs
    sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')

files = [
    r'C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\BankAccountsForm.cs',
    r'C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\InvoicesListForm.cs',
    r'C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\CashBoxForm.cs'
]

def fix_file(content):
    """
    يصلح الكود المكسور بشكل كامل
    """
    # نبحث عن pattern: FormName.FormClosed += ثم سطر جديد بدون قوس
    # ونصلحها لتكون في سطر واحد
    
    # Pattern: var form = new XxxForm(...);
    #          form.FormClosed += (s, args) => CODE
    # نصلحه ل:
    # var form = new XxxForm(...);
    # form.FormClosed += (s, args) => CODE;
    # form.Show();
    
    lines = content.split('\n')
    fixed_lines = []
    i = 0
    
    while i < len(lines):
        line = lines[i]
        
        # نتحقق إذا السطر يحتوي على .FormClosed
        if '.FormClosed +=' in line and '=>' in line:
            # نشوف إذا في مشكلة
            if not line.rstrip().endswith(';'):
                # المفروض ينتهي بـ ;
                # نجمع الأسطر التالية حتى نلاقي ;
                combined = line.rstrip()
                i += 1
                while i < len(lines) and not combined.endswith(';'):
                    next_line = lines[i].strip()
                    if next_line and not next_line.startswith('//'):
                        if next_line == '}':
                            break
                        combined += ' ' + next_line
                    i += 1
                
                # نتأكد إنه ينتهي بـ ;
                if not combined.endswith(';'):
                    combined += ';'
                
                fixed_lines.append(combined)
                continue
        
        fixed_lines.append(line)
        i += 1
    
    content = '\n'.join(fixed_lines)
    
    # نصلح أي }; زايدة
    content = re.sub(r'\}\s*\};\s*//\s*✅\s*تحديث عند الإغلاق', '};', content)
    
    return content

print("="*100)
print("إصلاح شامل للملفات")
print("="*100)
print()

for file_path in files:
    try:
        print(f"📄 {file_path.split(chr(92))[-1]}")
        
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        content = fix_file(content)
        
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
        
        print("   ✅ تم الإصلاح")
        
    except Exception as e:
        print(f"   ❌ خطأ: {e}")

print()
print("تم الانتهاء!")
