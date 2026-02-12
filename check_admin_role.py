# -*- coding: utf-8 -*-
import sqlite3
import sys
import io

if sys.platform == 'win32':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

db_path = r"C:\Users\musta\Desktop\pro\accountant\accountant.db"

print("Checking Admin User and Roles...")
print("=" * 60)

conn = sqlite3.connect(db_path)
cursor = conn.cursor()

# Check Roles table
print("\n1. Roles in database:")
print("-" * 60)
cursor.execute("SELECT RoleId, RoleName FROM roles")
roles = cursor.fetchall()
for role in roles:
    print(f"  RoleId: {role[0]} | RoleName: {role[1]}")

# Check Users with their roles
print("\n2. Users with their roles:")
print("-" * 60)
cursor.execute("""
    SELECT u.UserId, u.Username, u.FullName, u.RoleId, r.RoleName
    FROM users u
    LEFT JOIN roles r ON u.RoleId = r.RoleId
""")
users = cursor.fetchall()
for user in users:
    print(f"  UserId: {user[0]} | Username: {user[1]} | FullName: {user[2]} | RoleId: {user[3]} | RoleName: {user[4]}")

# Check if admin user has correct role
print("\n3. Admin user check:")
print("-" * 60)
cursor.execute("""
    SELECT u.UserId, u.Username, r.RoleName
    FROM users u
    LEFT JOIN roles r ON u.RoleId = r.RoleId
    WHERE u.Username = 'admin'
""")
admin = cursor.fetchone()
if admin:
    print(f"  Admin found: UserId={admin[0]}, Username={admin[1]}, Role={admin[2]}")
    is_admin_role = admin[2] and (admin[2].lower() == 'admin' or admin[2].lower() == 'مدير')
    print(f"  Is Admin Role: {is_admin_role}")
else:
    print("  ⚠️ Admin user not found!")

# Check Umrah packages
print("\n4. Umrah Packages:")
print("-" * 60)
cursor.execute("""
    SELECT 
        p.UmrahPackageId,
        p.PackageNumber,
        p.CreatedBy,
        u.Username,
        u.FullName
    FROM umrahpackages p
    LEFT JOIN users u ON p.CreatedBy = u.UserId
""")
packages = cursor.fetchall()
print(f"  Total packages: {len(packages)}")
for pkg in packages:
    print(f"  ID: {pkg[0]} | No: {pkg[1]} | CreatedBy: {pkg[2]} | User: {pkg[3]} ({pkg[4]})")

conn.close()
print("\n" + "=" * 60)
