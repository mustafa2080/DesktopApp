import sqlite3

conn = sqlite3.connect(r'C:\Users\musta\Desktop\pro\accountant\accountant.db')
cursor = conn.cursor()

print("="*50)
print("TABLES IN DATABASE")
print("="*50)
cursor.execute("SELECT name FROM sqlite_master WHERE type='table'")
tables = cursor.fetchall()
for table in tables:
    print(f"  - {table[0]}")

print("\n" + "="*50)
print("ROLES TABLE")
print("="*50)
try:
    cursor.execute("SELECT RoleId, RoleName, Description FROM Roles")
    roles = cursor.fetchall()
    for role in roles:
        print(f"Role ID: {role[0]}, Name: {role[1]}, Description: {role[2]}")
except Exception as e:
    print(f"Error: {e}")

print("\n" + "="*50)
print("PERMISSIONS TABLE (first 20)")
print("="*50)
try:
    cursor.execute("SELECT PermissionId, PermissionName, Module FROM Permissions LIMIT 20")
    perms = cursor.fetchall()
    for perm in perms:
        print(f"ID: {perm[0]}, Name: {perm[1]}, Module: {perm[2]}")
except Exception as e:
    print(f"Error: {e}")

print("\n" + "="*50)
print("ROLE PERMISSIONS")
print("="*50)
try:
    cursor.execute("""
        SELECT r.RoleName, p.PermissionName, p.Module 
        FROM RolePermissions rp
        JOIN Roles r ON rp.RoleId = r.RoleId
        JOIN Permissions p ON rp.PermissionId = p.PermissionId
        ORDER BY r.RoleName, p.Module
    """)
    role_perms = cursor.fetchall()
    current_role = None
    for rp in role_perms:
        if current_role != rp[0]:
            current_role = rp[0]
            print(f"\n{current_role}:")
        print(f"  - {rp[1]} ({rp[2]})")
except Exception as e:
    print(f"Error: {e}")

conn.close()
