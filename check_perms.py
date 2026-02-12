import sqlite3

conn = sqlite3.connect('accountant.db')
cursor = conn.cursor()

# Check users and their permission counts
cursor.execute('''
SELECT u.UserId, u.Username, r.RoleName, COUNT(rp.PermissionId) as PermCount
FROM Users u
LEFT JOIN Roles r ON u.RoleId = r.RoleId
LEFT JOIN RolePermissions rp ON r.RoleId = rp.RoleId
GROUP BY u.UserId
''')

print('\n=== Users and Permission Counts ===')
for row in cursor.fetchall():
    print(f'User ID: {row[0]:2d} | Username: {row[1]:15s} | Role: {row[2]:20s} | Permissions: {row[3]:3d}')

# Check Permission table columns first
cursor.execute("PRAGMA table_info(Permissions)")
print('\n=== Permission Table Structure ===')
for row in cursor.fetchall():
    print(f'Column: {row[1]} | Type: {row[2]}')

# Check specific user permissions
print('\n=== Detailed Permissions for User ID 2 (Aviation) ===')
cursor.execute('''
SELECT p.Module, p.PermissionType
FROM Users u
JOIN Roles r ON u.RoleId = r.RoleId
JOIN RolePermissions rp ON r.RoleId = rp.RoleId
JOIN Permissions p ON rp.PermissionId = p.PermissionId
WHERE u.UserId = 2
ORDER BY p.Module, p.PermissionType
''')

current_module = None
for row in cursor.fetchall():
    module, perm_type = row
    if module != current_module:
        print(f'\n{module}:')
        current_module = module
    print(f'  - {perm_type}')

print('\n=== Detailed Permissions for User ID 3 (Operations) ===')
cursor.execute('''
SELECT p.Module, p.PermissionType
FROM Users u
JOIN Roles r ON u.RoleId = r.RoleId
JOIN RolePermissions rp ON r.RoleId = rp.RoleId
JOIN Permissions p ON rp.PermissionId = p.PermissionId
WHERE u.UserId = 3
ORDER BY p.Module, p.PermissionType
''')

current_module = None
for row in cursor.fetchall():
    module, perm_type = row
    if module != current_module:
        print(f'\n{module}:')
        current_module = module
    print(f'  - {perm_type}')

conn.close()
