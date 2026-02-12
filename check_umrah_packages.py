# -*- coding: utf-8 -*-
import sqlite3
import sys
import io

if sys.platform == 'win32':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

db_path = r"C:\Users\musta\Desktop\pro\accountant\accountant.db"

print("Checking Umrah Packages table...")
print("=" * 60)

conn = sqlite3.connect(db_path)
cursor = conn.cursor()

# Check if table exists
cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='umrahpackages'")
table_exists = cursor.fetchone() is not None

if not table_exists:
    print("ERROR: umrahpackages table does not exist!")
    conn.close()
    exit(1)

print("OK: Table exists")
print("\nTable Structure:")
print("-" * 60)

cursor.execute("PRAGMA table_info(umrahpackages)")
columns = cursor.fetchall()

for col in columns:
    pk = " (PK)" if col[5] else ""
    notnull = " NOT NULL" if col[3] else ""
    default = f" DEFAULT {col[4]}" if col[4] else ""
    print(f"  {col[1]:25} {col[2]:10}{notnull}{default}{pk}")

# Check if CreatedBy column exists
has_created_by = any(col[1].lower() == 'createdby' for col in columns)
print(f"\nCreatedBy column exists: {has_created_by}")

# Check data
print("\n" + "=" * 60)
print("Umrah Packages Data:")
print("=" * 60)

cursor.execute("""
    SELECT 
        UmrahPackageId,
        PackageNumber,
        TripName,
        CreatedBy,
        IsActive,
        Status
    FROM umrahpackages
    ORDER BY CreatedAt DESC
    LIMIT 10
""")

packages = cursor.fetchall()
print(f"\nTotal packages: {len(packages)}\n")

if packages:
    for pkg in packages:
        pkg_id, pkg_num, trip_name, user_id, is_active, status = pkg
        print(f"ID: {pkg_id} | No: {pkg_num} | Trip: {trip_name[:20]:20} | UserID: {user_id} | Active: {is_active} | Status: {status}")
else:
    print("No packages found")

# Check users
print("\n" + "=" * 60)
print("Users:")
print("=" * 60)

cursor.execute("SELECT UserId, Username, FullName FROM users")
users = cursor.fetchall()
print(f"Total users: {len(users)}\n")
for user in users:
    print(f"  ID: {user[0]} | Username: {user[1]} | Name: {user[2]}")

# Check join
print("\n" + "=" * 60)
print("Packages with User Info:")
print("=" * 60)

cursor.execute("""
    SELECT 
        p.UmrahPackageId,
        p.PackageNumber,
        p.TripName,
        p.CreatedBy,
        u.Username,
        u.FullName
    FROM umrahpackages p
    LEFT JOIN users u ON p.CreatedBy = u.UserId
    ORDER BY p.CreatedAt DESC
    LIMIT 5
""")

packages_with_users = cursor.fetchall()
if packages_with_users:
    for pkg in packages_with_users:
        pkg_id, pkg_num, trip_name, user_id, username, fullname = pkg
        print(f"ID: {pkg_id} | No: {pkg_num} | Trip: {trip_name[:15]:15} | UserID: {user_id} | User: {username or 'NULL'} ({fullname or 'NULL'})")
else:
    print("No packages with user data found")

conn.close()
print("\n" + "=" * 60)
print("Check completed!")
