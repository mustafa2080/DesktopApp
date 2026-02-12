import sqlite3
import sys

if sys.platform == 'win32':
    import codecs
    sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')

conn = sqlite3.connect('C:/Users/musta/Desktop/pro/accountant/accountant.db')
cursor = conn.cursor()

print("="*80)
print("PERMISSIONS COMPARISON - Operations vs Aviation")
print("="*80)

# Operations User
print("\n[1] OPERATIONS USER (ID: 3, Role: Operations)")
print("-" * 80)
cursor.execute('''
    SELECT p.Module, GROUP_CONCAT(p.PermissionName, ', ') as Permissions
    FROM RolePermissions rp
    JOIN Permissions p ON rp.PermissionId = p.PermissionId
    WHERE rp.RoleId = 3
    GROUP BY p.Module
    ORDER BY p.Module
''')
ops_modules = cursor.fetchall()
for module in ops_modules:
    print(f"  Module: {module[0]}")
    print(f"    -> {module[1]}")

print("\n  Expected Sidebar Items:")
print("    - Dashboard (always visible)")
print("    - Trips (because has Trips module)")
print("    - Calculator (because has Calculator permission)")

# Aviation User
print("\n[2] AVIATION USER (ID: 2, Role: Aviation and Umrah)")
print("-" * 80)
cursor.execute('''
    SELECT p.Module, GROUP_CONCAT(p.PermissionName, ', ') as Permissions
    FROM RolePermissions rp
    JOIN Permissions p ON rp.PermissionId = p.PermissionId
    WHERE rp.RoleId = 2
    GROUP BY p.Module
    ORDER BY p.Module
''')
aviation_modules = cursor.fetchall()
for module in aviation_modules:
    print(f"  Module: {module[0]}")
    print(f"    -> {module[1]}")

print("\n  Expected Sidebar Items:")
print("    - Dashboard (always visible)")
print("    - Flights (because has Aviation module)")
print("    - Umrah (because has Umrah module)")
print("    - Calculator (because has Calculator permission)")

# Check for conflicts
print("\n[3] CONFLICT CHECK")
print("-" * 80)

# Check if Aviation has Trips permissions
cursor.execute('''
    SELECT COUNT(*) 
    FROM RolePermissions rp
    JOIN Permissions p ON rp.PermissionId = p.PermissionId
    WHERE rp.RoleId = 2 AND p.Module = 'Trips'
''')
aviation_has_trips = cursor.fetchone()[0]

# Check if Aviation has Reports permissions
cursor.execute('''
    SELECT COUNT(*) 
    FROM RolePermissions rp
    JOIN Permissions p ON rp.PermissionId = p.PermissionId
    WHERE rp.RoleId = 2 AND p.Module = 'Reports'
''')
aviation_has_reports = cursor.fetchone()[0]

# Check if Operations has Aviation permissions
cursor.execute('''
    SELECT COUNT(*) 
    FROM RolePermissions rp
    JOIN Permissions p ON rp.PermissionId = p.PermissionId
    WHERE rp.RoleId = 3 AND p.Module = 'Aviation'
''')
operations_has_aviation = cursor.fetchone()[0]

# Check if Operations has Umrah permissions
cursor.execute('''
    SELECT COUNT(*) 
    FROM RolePermissions rp
    JOIN Permissions p ON rp.PermissionId = p.PermissionId
    WHERE rp.RoleId = 3 AND p.Module = 'Umrah'
''')
operations_has_umrah = cursor.fetchone()[0]

if aviation_has_trips > 0:
    print("  [WARNING] Aviation user has Trips permissions!")
else:
    print("  [OK] Aviation user does NOT have Trips permissions")

if aviation_has_reports > 0:
    print("  [WARNING] Aviation user has Reports permissions!")
else:
    print("  [OK] Aviation user does NOT have Reports permissions")

if operations_has_aviation > 0:
    print("  [WARNING] Operations user has Aviation permissions!")
else:
    print("  [OK] Operations user does NOT have Aviation permissions")

if operations_has_umrah > 0:
    print("  [WARNING] Operations user has Umrah permissions!")
else:
    print("  [OK] Operations user does NOT have Umrah permissions")

print("\n" + "="*80)
print("SUMMARY")
print("="*80)
print("\nOperations User sees:")
print("  - Dashboard")
print("  - Trips")
print("  - Calculator")
print("\nAviation User sees:")
print("  - Dashboard")
print("  - Flights")
print("  - Umrah")
print("  - Calculator")
print("\nNo conflicts detected!" if (aviation_has_trips == 0 and aviation_has_reports == 0 and operations_has_aviation == 0 and operations_has_umrah == 0) else "\n[WARNING] Conflicts detected!")
print("="*80)

conn.close()
