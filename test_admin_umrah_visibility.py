# -*- coding: utf-8 -*-
import sqlite3
import sys
import io

if sys.platform == 'win32':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

db_path = r"C:\Users\musta\Desktop\pro\accountant\accountant.db"

print("Testing Admin Detection Logic...")
print("=" * 80)

conn = sqlite3.connect(db_path)
cursor = conn.cursor()

# Get admin user
cursor.execute("""
    SELECT u.UserId, u.Username, u.FullName, u.RoleId, r.RoleName
    FROM users u
    LEFT JOIN roles r ON u.RoleId = r.RoleId
    WHERE u.Username = 'admin'
""")

admin = cursor.fetchone()
if admin:
    print(f"Admin User Found:")
    print(f"  UserId: {admin[0]}")
    print(f"  Username: {admin[1]}")
    print(f"  FullName: {admin[2]}")
    print(f"  RoleId: {admin[3]}")
    print(f"  RoleName: {admin[4]}")
    print(f"  RoleName.ToLower(): {admin[4].lower() if admin[4] else 'NULL'}")
    
    # Test the exact C# condition
    is_admin = admin[4] and (admin[4].lower() == 'admin' or admin[4].lower() == 'مدير')
    print(f"\n  IsAdmin Result: {is_admin}")
    
    if not is_admin:
        print(f"\n  ❌ PROBLEM: Admin detection returned FALSE!")
        print(f"     RoleName '{admin[4]}' does not match 'admin' or 'مدير'")
else:
    print("❌ Admin user not found!")

print("\n" + "=" * 80)
print("All Umrah Packages:")
print("=" * 80)

cursor.execute("""
    SELECT 
        p.UmrahPackageId,
        p.PackageNumber,
        p.TripName,
        p.CreatedBy,
        p.IsActive,
        u.Username,
        u.FullName
    FROM umrahpackages p
    LEFT JOIN users u ON p.CreatedBy = u.UserId
""")

packages = cursor.fetchall()
if packages:
    for pkg in packages:
        print(f"\nPackage ID: {pkg[0]}")
        print(f"  Number: {pkg[1]}")
        print(f"  Trip: {pkg[2]}")
        print(f"  CreatedBy: {pkg[3]}")
        print(f"  IsActive: {pkg[4]}")
        print(f"  Creator Username: {pkg[5] or 'NULL'}")
        print(f"  Creator Name: {pkg[6] or 'NULL'}")
else:
    print("No packages found")

print("\n" + "=" * 80)
print("Expected Behavior:")
print("=" * 80)
if admin and packages:
    print(f"✅ Admin (UserId={admin[0]}) should see ALL {len(packages)} package(s)")
    print(f"✅ Non-admin users should see only their own packages")
else:
    print("⚠️ Missing data - cannot test")

conn.close()
