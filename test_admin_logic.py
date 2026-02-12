# -*- coding: utf-8 -*-
import sqlite3

db_path = r"C:\Users\musta\Desktop\pro\accountant\accountant.db"

conn = sqlite3.connect(db_path)
cursor = conn.cursor()

# Test the exact condition from C# code
cursor.execute("""
    SELECT 
        u.UserId,
        u.Username,
        r.RoleName,
        LOWER(r.RoleName),
        CASE 
            WHEN LOWER(r.RoleName) = 'admin' OR LOWER(r.RoleName) = 'مدير' 
            THEN 1 
            ELSE 0 
        END as IsAdmin
    FROM users u
    LEFT JOIN roles r ON u.RoleId = r.RoleId
""")

users = cursor.fetchall()
print("Admin Check Results:")
print("=" * 80)
for user in users:
    print(f"UserId: {user[0]}")
    print(f"  Username: {user[1]}")
    print(f"  RoleName: {user[2]}")
    print(f"  RoleName.ToLower(): {user[3]}")
    print(f"  IsAdmin: {bool(user[4])}")
    print()

conn.close()
