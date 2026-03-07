
path = r'C:\Users\musta\Desktop\pro\accountant\Presentation\Controls\DashboardControl.cs'

content = open(path, encoding='utf-8-sig').read()
print(f'Current lines: {content.count(chr(10))}')
