# -*- coding: utf-8 -*-
import os
import re
import sys

if sys.platform == 'win32':
    import codecs
    sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')

base_path = r'C:\Users\musta\Desktop\pro\accountant\Presentation\Forms'

# الملفات الموجودة فعلياً بناءً على البحث
files_to_process = [
    'ReservationsListForm.cs',
    'FlightBookingsListForm.cs',
    'InvoicesListForm.cs',
    'CashBoxForm.cs',
    'BankAccountsForm.cs'
]

modified_files = []
total_changes = 0

print("="*120)
print("تحويل ShowDialog إلى Show في جميع الأقسام")
print("="*120)
print()

def process_file(file_path):
    """
    يبحث ويستبدل كل استخدامات ShowDialog بـ Show
    """
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
    except Exception as e:
        return 0, f"خطأ في القراءة: {e}"
    
    original_content = content
    changes = 0
    
    # Pattern 1: form.ShowDialog() بدون شرط
    pattern1 = r'(\w+)\.ShowDialog\(\);'
    matches1 = re.findall(pattern1, content)
    if matches1:
        content = re.sub(pattern1, r'\1.Show(); // ✅ نافذة مستقلة', content)
        changes += len(matches1)
    
    # Pattern 2: if (xxx.ShowDialog() == DialogResult.OK)
    pattern2 = r'if\s*\(\s*(\w+)\.ShowDialog\(\)\s*==\s*DialogResult\.OK\s*\)'
    matches2 = list(re.finditer(pattern2, content))
    
    for match in reversed(matches2):  # نبدأ من الآخر عشان ما نخرب الـ indices
        form_var = match.group(1)
        
        # نبحث عن الـ block الكامل
        start = match.start()
        
        # نبحث عن بداية السطر
        line_start = content.rfind('\n', 0, start) + 1
        
        # نبحث عن الـ closing brace
        brace_count = 0
        i = match.end()
        while i < len(content):
            if content[i] == '{':
                brace_count += 1
            elif content[i] == '}':
                if brace_count == 0:
                    break
                brace_count -= 1
            i += 1
        
        if i < len(content):
            # استخرجنا الـ block
            old_block = content[line_start:i+1]
            
            # نستخرج الكود جوا الـ if
            if_body_start = content.find('{', match.end()) + 1
            if_body = content[if_body_start:i].strip()
            
            # نبحث عن تعريف الـ form قبل الـ if
            form_def_pattern = rf'(var\s+{form_var}\s*=\s*new\s+\w+\([^)]*\);)'
            form_def_match = re.search(form_def_pattern, content[max(0, line_start-500):line_start])
            
            if form_def_match:
                form_def = form_def_match.group(1)
                indent = ' ' * 8  # افتراض indent 8 spaces
                
                new_code = f"{indent}{form_var}.FormClosed += (s, args) =>\n{indent}{{\n"
                for line in if_body.split('\n'):
                    new_code += f"{indent}    {line.strip()}\n"
                new_code += f"{indent}}}; // ✅ تحديث عند الإغلاق\n"
                new_code += f"{indent}{form_var}.Show(); // ✅ نافذة مستقلة"
                
                content = content[:line_start] + new_code + content[i+1:]
                changes += 1
    
    # Pattern 3: new XxxForm(...).ShowDialog()
    pattern3 = r'new\s+(\w+Form)\s*\(([^)]*)\)\.ShowDialog\(\)'
    matches3 = re.findall(pattern3, content)
    if matches3:
        for form_class, args in matches3:
            old = f'new {form_class}({args}).ShowDialog()'
            new = f'''var form = new {form_class}({args});
        form.Show(); // ✅ نافذة مستقلة'''
            content = content.replace(old, new)
            changes += 1
    
    if content != original_content:
        try:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            return changes, "تم التعديل بنجاح"
        except Exception as e:
            return 0, f"خطأ في الكتابة: {e}"
    
    return 0, "لا يوجد تغييرات"

# معالجة كل ملف
for filename in files_to_process:
    file_path = os.path.join(base_path, filename)
    
    if not os.path.exists(file_path):
        print(f"⚠️  {filename} - الملف غير موجود")
        continue
    
    print(f"📄 معالجة: {filename}")
    
    changes, message = process_file(file_path)
    
    if changes > 0:
        modified_files.append((filename, changes))
        total_changes += changes
        print(f"   ✅ {message} - {changes} تعديلات")
    else:
        print(f"   ℹ️  {message}")
    
    print()

print()
print("="*120)
print("الملخص النهائي")
print("="*120)
print(f"الملفات المعدلة: {len(modified_files)}")
print(f"إجمالي التعديلات: {total_changes}")
print()

if modified_files:
    print("الملفات المعدلة:")
    for filename, changes in modified_files:
        print(f"  ✅ {filename} ({changes} تعديلات)")
    
    print()
    print("🎉 تم بنجاح!")
    print()
    print("الآن يمكنك فتح عدة نوافذ في نفس الوقت في:")
    print("  ✅ الحجوزات")
    print("  ✅ الطيران")
    print("  ✅ الفواتير")
    print("  ✅ الخزنة")
    print("  ✅ البنوك")
else:
    print("⚠️ لم يتم تعديل أي ملفات")

print()
print("تم الانتهاء!")
