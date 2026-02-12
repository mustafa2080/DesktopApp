import psycopg2

conn = psycopg2.connect('host=localhost dbname=graceway_accounting user=postgres password=123456')
cur = conn.cursor()

# Check admin user
print("=" * 60)
print("1. Admin User Details")
print("=" * 60)
cur.execute('SELECT userid, username, fullname, roleid FROM users WHERE username = \'admin\'')
user = cur.fetchone()
if user:
    print(f"User ID: {user[0]}")
    print(f"Username: {user[1]}")
    print(f"Full Name: {user[2]}")
    print(f"Role ID: {user[3]}")
    
    # Get role name
    cur.execute('SELECT rolename FROM roles WHERE roleid = %s', (user[3],))
    role = cur.fetchone()
    if role:
        print(f"Role Name: {role[0]}")
else:
    print("Admin user not found!")

# Check admin permissions count
print("\n" + "=" * 60)
print("2. Admin Permissions Count")
print("=" * 60)
cur.execute('SELECT COUNT(*) FROM rolepermissions WHERE roleid = 1')
count = cur.fetchone()[0]
print(f"Total Permissions Assigned: {count}")

# Check all permissions in system
cur.execute('SELECT COUNT(*) FROM permissions')
total_perms = cur.fetchone()[0]
print(f"Total Permissions in System: {total_perms}")

if count < total_perms:
    print(f"\n⚠️ WARNING: Admin only has {count} out of {total_perms} permissions!")
    print("Admin should have ALL permissions!")

# Check modules
print("\n" + "=" * 60)
print("3. Modules with Permissions (Admin Role)")
print("=" * 60)
cur.execute('SELECT DISTINCT p.module FROM permissions p INNER JOIN rolepermissions rp ON p.permissionid = rp.permissionid WHERE rp.roleid = 1 ORDER BY p.module')
modules = cur.fetchall()
if modules:
    for module in modules:
        print(f"  ✓ {module[0]}")
else:
    print("  No modules found!")

# Check ALL modules in system
print("\n" + "=" * 60)
print("4. ALL Modules in System")
print("=" * 60)
cur.execute('SELECT DISTINCT module FROM permissions ORDER BY module')
all_modules = cur.fetchall()
if all_modules:
    admin_modules = [m[0] for m in modules] if modules else []
    for module in all_modules:
        if module[0] in admin_modules:
            print(f"  ✓ {module[0]} (assigned)")
        else:
            print(f"  ✗ {module[0]} (NOT assigned)")
else:
    print("  No modules found!")

# Count permissions per module
print("\n" + "=" * 60)
print("5. Permissions per Module (Admin Role)")
print("=" * 60)
cur.execute('SELECT p.module, COUNT(*) as count FROM permissions p INNER JOIN rolepermissions rp ON p.permissionid = rp.permissionid WHERE rp.roleid = 1 GROUP BY p.module ORDER BY p.module')
module_counts = cur.fetchall()
if module_counts:
    for module, count in module_counts:
        print(f"  {module}: {count} permissions")
else:
    print("  No permissions found!")

# Check specific permissions assigned to admin
print("\n" + "=" * 60)
print("6. Specific Permissions Assigned to Admin")
print("=" * 60)
cur.execute('SELECT p.permissionname, p.module FROM permissions p INNER JOIN rolepermissions rp ON p.permissionid = rp.permissionid WHERE rp.roleid = 1 ORDER BY p.module, p.permissionname')
perms = cur.fetchall()
if perms:
    current_module = None
    for perm_name, module in perms:
        if current_module != module:
            current_module = module
            print(f"\n{module}:")
        print(f"  - {perm_name}")
else:
    print("  No permissions found!")

cur.close()
conn.close()
