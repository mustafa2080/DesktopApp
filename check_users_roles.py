import sqlite3
import sys

sys.stdout.reconfigure(encoding='utf-8')

conn = sqlite3.connect(r'C:\Users\musta\Desktop\pro\accountant\accountant.db')
cursor = conn.cursor()

print("="*60)
print("USERS AND THEIR ROLES")
print("="*60)

cursor.execute("""
    SELECT u.UserId, u.Username, u.FullName, u.RoleId, r.RoleName, u.IsActive
    FROM Users u
    LEFT JOIN Roles r ON u.RoleId = r.RoleId
    ORDER BY u.UserId
""")

users = cursor.fetchall()
for user in users:
    print(f"\nUser ID: {user[0]}")
    print(f"  Username: {user[1]}")
    print(f"  Full Name: {user[2]}")
    print(f"  Role ID: {user[3]}")
    print(f"  Role Name: {user[4] if user[4] else 'NO ROLE!'}")
    print(f"  Active: {'Yes' if user[5] else 'No'}")

print("\n" + "="*60)
print("CHECKING PERMISSIONS FOR EACH USER")
print("="*60)

for user in users:
    user_id = user[0]
    username = user[1]
    role_id = user[3]
    
    print(f"\n{username} (User ID: {user_id}, Role ID: {role_id}):")
    
    if role_id is None:
        print("  ❌ NO ROLE ASSIGNED!")
        continue
    
    cursor.execute("""
        SELECT p.Module, COUNT(*) as perm_count
        FROM RolePermissions rp
        JOIN Permissions p ON rp.PermissionId = p.PermissionId
        WHERE rp.RoleId = ?
        GROUP BY p.Module
        ORDER BY p.Module
    """, (role_id,))
    
    modules = cursor.fetchall()
    if not modules:
        print("  ❌ NO PERMISSIONS!")
    else:
        for module in modules:
            print(f"  ✓ {module[0]}: {module[1]} permissions")

conn.close()
