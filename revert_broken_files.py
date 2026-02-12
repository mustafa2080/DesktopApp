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

print("Reverting broken files...")

for file_path in files:
    filename = file_path.split('\\')[-1]
    print(f"\nFile: {filename}")
    
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Remove broken FormClosed patterns  
    content = re.sub(
        r'(\w+)\.FormClosed \+= \(s, args\) => [^;]+;+\s*//[^\n]*\n\s*\1\.Show\(\);[^\n]*',
        r'\1.ShowDialog();',
        content,
        flags=re.MULTILINE
    )
    
    # Revert Show() back to ShowDialog()
    content = re.sub(
        r'([a-zA-Z_]\w*)\.Show\(\); // .+ \w+ \w+',
        r'\1.ShowDialog();',
        content
    )
    
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(content)
    
    print("   OK - File reverted")

print("\nALL FILES REVERTED!")
print("Now run: dotnet build")
