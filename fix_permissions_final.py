#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Fix Permissions System - Final Version
This script resets and properly configures permissions for all users:
- operations: Trips + Calculator
- aviation: Aviation + Umrah + Calculator  
- admin: All permissions
"""

import psycopg2
from psycopg2.extras import RealDictCursor

# Database connection
conn = psycopg2.connect(
    host="localhost",
    port=5432,
    database="graceway_accounting",
    user="postgres",
    password="Onlyme@121"
)

cursor = conn.cursor(cursor_factory=RealDictCursor)

print("=" * 80)
print("FIXING PERMISSIONS SYSTEM - FINAL VERSION")
print("=" * 80)

# Step 1: Clear existing role_permissions
print("\n1. Clearing existing role_permissions...")
cursor.execute("DELETE FROM role_permissions")
conn.commit()
print("   ✓ Cleared all role_permissions")

# Step 2: Get all roles
print("\n2. Getting roles...")
cursor.execute("SELECT * FROM roles ORDER BY role_id")
roles = cursor.fetchall()
for role in roles:
    print(f"   - {role['role_name']} (ID: {role['role_id']})")

# Get role IDs
operations_role = next((r for r in roles if r['role_name'] == 'Operations Department'), None)
aviation_role = next((r for r in roles if r['role_name'] == 'Aviation and Umrah'), None)
admin_role = next((r for r in roles if r['role_name'] == 'Administrator'), None)

if not all([operations_role, aviation_role, admin_role]):
    print("\n❌ ERROR: Missing required roles!")
    exit(1)

# Step 3: Get all permissions
print("\n3. Getting permissions...")
cursor.execute("SELECT * FROM permissions ORDER BY permission_id")
permissions = cursor.fetchall()
print(f"   Total permissions: {len(permissions)}")

# Group permissions by module
by_module = {}
for perm in permissions:
    module = perm['module']
    if module not in by_module:
        by_module[module] = []
    by_module[module].append(perm)

print("\n   Permissions by module:")
for module, perms in by_module.items():
    print(f"   - {module}: {len(perms)} permissions")

# Step 4: Assign permissions to Operations Department
print(f"\n4. Assigning permissions to Operations Department (ID: {operations_role['role_id']})...")

operations_permissions = []

# Add Trips module permissions
trips_perms = [p for p in permissions if p['module'] == 'Trips']
operations_permissions.extend(trips_perms)
print(f"   + Trips permissions: {len(trips_perms)}")

# Add Calculator permission
calc_perm = next((p for p in permissions if p['permission_name'] == 'استخدام الآلة الحاسبة'), None)
if calc_perm:
    operations_permissions.append(calc_perm)
    print(f"   + Calculator permission")

# Add Reports permissions for trips
report_perms = [p for p in permissions if p['module'] == 'Reports' and 
                ('Trip' in p['permission_name'] or 'تقارير' in p['permission_name'] or 
                 'تصدير' in p['permission_name'] or 'طباعة' in p['permission_name'])]
operations_permissions.extend(report_perms)
print(f"   + Reports permissions: {len(report_perms)}")

# Insert operations permissions
for perm in operations_permissions:
    cursor.execute(
        "INSERT INTO role_permissions (role_id, permission_id) VALUES (%s, %s)",
        (operations_role['role_id'], perm['permission_id'])
    )
conn.commit()
print(f"   ✓ Total Operations permissions: {len(operations_permissions)}")

# Step 5: Assign permissions to Aviation and Umrah
print(f"\n5. Assigning permissions to Aviation and Umrah (ID: {aviation_role['role_id']})...")

aviation_permissions = []

# Add Aviation module permissions
aviation_perms = [p for p in permissions if p['module'] == 'Aviation']
aviation_permissions.extend(aviation_perms)
print(f"   + Aviation permissions: {len(aviation_perms)}")

# Add Umrah module permissions
umrah_perms = [p for p in permissions if p['module'] == 'Umrah']
aviation_permissions.extend(umrah_perms)
print(f"   + Umrah permissions: {len(umrah_perms)}")

# Add Calculator permission
if calc_perm:
    aviation_permissions.append(calc_perm)
    print(f"   + Calculator permission")

# Add Reports permissions for aviation and umrah
aviation_report_perms = [p for p in permissions if p['module'] == 'Reports' and 
                         ('Flight' in p['permission_name'] or 'Umrah' in p['permission_name'] or
                          'الطيران' in p['permission_name'] or 'العمرة' in p['permission_name'] or
                          'تصدير' in p['permission_name'] or 'طباعة' in p['permission_name'])]
aviation_permissions.extend(aviation_report_perms)
print(f"   + Reports permissions: {len(aviation_report_perms)}")

# Insert aviation permissions
for perm in aviation_permissions:
    cursor.execute(
        "INSERT INTO role_permissions (role_id, permission_id) VALUES (%s, %s)",
        (aviation_role['role_id'], perm['permission_id'])
    )
conn.commit()
print(f"   ✓ Total Aviation and Umrah permissions: {len(aviation_permissions)}")

# Step 6: Assign ALL permissions to Administrator
print(f"\n6. Assigning ALL permissions to Administrator (ID: {admin_role['role_id']})...")

for perm in permissions:
    cursor.execute(
        "INSERT INTO role_permissions (role_id, permission_id) VALUES (%s, %s)",
        (admin_role['role_id'], perm['permission_id'])
    )
conn.commit()
print(f"   ✓ Total Admin permissions: {len(permissions)}")

# Step 7: Verify permissions
print("\n7. Verifying permissions...")
cursor.execute("""
    SELECT r.role_name, COUNT(rp.role_permission_id) as perm_count
    FROM roles r
    LEFT JOIN role_permissions rp ON r.role_id = rp.role_id
    GROUP BY r.role_id, r.role_name
    ORDER BY r.role_id
""")
verification = cursor.fetchall()
for row in verification:
    print(f"   - {row['role_name']}: {row['perm_count']} permissions")

# Step 8: Show detailed permissions for each role
print("\n8. Detailed permissions by role:")

for role in [operations_role, aviation_role, admin_role]:
    print(f"\n   {role['role_name']}:")
    cursor.execute("""
        SELECT p.module, COUNT(*) as count
        FROM role_permissions rp
        JOIN permissions p ON rp.permission_id = p.permission_id
        WHERE rp.role_id = %s
        GROUP BY p.module
        ORDER BY p.module
    """, (role['role_id'],))
    modules = cursor.fetchall()
    for mod in modules:
        print(f"      - {mod['module']}: {mod['count']} permissions")

print("\n" + "=" * 80)
print("✓ PERMISSIONS FIXED SUCCESSFULLY!")
print("=" * 80)

print("\nYou can now:")
print("1. Login as 'operations' (password: operations123) - Should see Trips + Calculator")
print("2. Login as 'aviation' (password: aviation123) - Should see Aviation + Umrah + Calculator")
print("3. Login as 'admin' (password: admin123) - Should see ALL sections")

cursor.close()
conn.close()
