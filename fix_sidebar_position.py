import re

file_path = r"C:\Users\musta\Desktop\pro\accountant\Presentation\Forms\MainForm.cs"

with open(file_path, 'r', encoding='utf-8-sig') as f:
    content = f.read()

# Fix Column Styles Order (Sidebar first, Content second)
content = re.sub(
    r'// Column styles: Content \| Sidebar \(عكس الترتيب لدعم RTL\)\s*\n\s*mainLayout\.ColumnStyles\.Add\(new ColumnStyle\(SizeType\.Percent, 100\)\);\s*\n\s*mainLayout\.ColumnStyles\.Add\(new ColumnStyle\(SizeType\.Absolute, 350\)\);',
    '// Column styles: Sidebar | Content (RTL)\n        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350));\n        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));',
    content
)

# Fix Controls Position (Sidebar column 0, Header/Content column 1)
content = re.sub(
    r'// Sidebar في العمود 1 \(اليمين\)\s*\n\s*mainLayout\.Controls\.Add\(_sidebar, 1, 0\);',
    '// Sidebar في العمود 0 (اليمين في RTL)\n        mainLayout.Controls.Add(_sidebar, 0, 0);',
    content
)

content = re.sub(
    r'mainLayout\.Controls\.Add\(_header, 0, 0\);',
    'mainLayout.Controls.Add(_header, 1, 0);',
    content
)

content = re.sub(
    r'mainLayout\.Controls\.Add\(_contentPanel, 0, 1\);',
    'mainLayout.Controls.Add(_contentPanel, 1, 1);',
    content
)

with open(file_path, 'w', encoding='utf-8-sig', newline='') as f:
    f.write(content)

print("Fixed MainForm.cs - Sidebar now on the right!")
