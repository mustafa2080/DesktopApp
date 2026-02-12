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

# Check roles
cursor.execute("SELECT RoleId, RoleName FROM roles")
roles = cursor.fetchall()
print("Roles:")
for role in roles:
    print(f"  ID: {role[0]} | Name: {role[1]}")

print("\n" + "=" * 60)

# Check users with roles
cursor.execute("""
    SELECT u.UserId, u.Username, u.FullName, u.RoleId, r.RoleName
    FROM users u
    LEFT JOIN roles r ON u.RoleId = r.RoleId
""")
users = cursor.fetchall()
print("Users with Roles:")
for user in users:
    print(f"  ID: {user[0]} | Username: {user[1]:15} | Name: {user[2]:25} | RoleId: {user[3]} | RoleName: {user[4] or 'NULL'}")

print("\n" + "=" * 60)

# Check umrah packages
cursor.execute("""
    SELECT 
        p.UmrahPackageId,
        p.PackageNumber,
        p.TripName,
        p.CreatedBy,
        u.Username,
        u.FullName,
        u.RoleId,
        r.RoleName
    FROM umrahpackages p
    LEFT JOIN users u ON p.CreatedBy = u.UserId
    LEFT JOIN roles r ON u.RoleId = r.RoleId
""")
packages = cursor.fetchall()
print("Umrah Packages with Creator Info:")
if packages:
    for pkg in packages:
        print(f"\nPackage ID: {pkg[0]}")
        print(f"  Number: {pkg[1]}")
        print(f"  Trip: {pkg[2]}")
        print(f"  CreatedBy: {pkg[3]}")
        print(f"  Creator Username: {pkg[4] or 'NULL'}")
        print(f"  Creator Name: {pkg[5] or 'NULL'}")
        print(f"  Creator RoleId: {pkg[6] or 'NULL'}")
        print(f"  Creator RoleName: {pkg[7] or 'NULL'}")
else:
    print("No packages found")

conn.close()
print("\n" + "=" * 60)
print("Check completed!")
