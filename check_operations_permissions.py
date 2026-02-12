import sqlite3
import sys

# Fix encoding for Windows console
if sys.platform == 'win32':
    import codecs
    sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')

conn = sqlite3.connect('C:/Users/musta/Desktop/pro/accountant/accountant.db')
cursor = conn.cursor()

# Check Permissions table structure
cursor.execute("PRAGMA table_info(Permissions)")
perm_columns = cursor.fetchall()
print('Permissions table columns:')
for col in perm_columns:
    print(f'  - {col[1]} ({col[2]})')

# Get all permissions with their modules
cursor.execute('SELECT PermissionId, PermissionName, Module, Description FROM Permissions ORDER BY Module, PermissionName')
all_perms = cursor.fetchall()
print(f'\nAll Permissions (Total: {len(all_perms)}):')
current_module = None
for p in all_perms:
    if p[2] != current_module:
        current_module = p[2]
        print(f'\n[Module: {current_module}]')
    print(f'  - {p[1]} (ID: {p[0]})')

# Get permissions for operations role
cursor.execute('''
    SELECT p.PermissionId, p.PermissionName, p.Module
    FROM RolePermissions rp
    JOIN Permissions p ON rp.PermissionId = p.PermissionId
    WHERE rp.RoleId = 3
    ORDER BY p.Module, p.PermissionName
''')
ops_perms = cursor.fetchall()
print(f'\n\nOperations Role Permissions by Module:')
current_module = None
for p in ops_perms:
    if p[2] != current_module:
        current_module = p[2]
        print(f'\n[Module: {current_module}]')
    print(f'  - {p[1]} (ID: {p[0]})')

conn.close()
