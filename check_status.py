import psycopg2

conn = psycopg2.connect('host=localhost dbname=graceway_accounting user=postgres password=123456')
cur = conn.cursor()

print("=" * 60)
print("Database Status Check")
print("=" * 60)

# Check permissions
cur.execute('SELECT COUNT(*) FROM permissions')
perm_count = cur.fetchone()[0]
print(f"\nPermissions in database: {perm_count}")

# Check roles
cur.execute('SELECT COUNT(*) FROM roles')
role_count = cur.fetchone()[0]
print(f"Roles in database: {role_count}")

# Check rolepermissions
cur.execute('SELECT COUNT(*) FROM rolepermissions')
rp_count = cur.fetchone()[0]
print(f"RolePermissions in database: {rp_count}")

# Check users
cur.execute('SELECT COUNT(*) FROM users')
user_count = cur.fetchone()[0]
print(f"Users in database: {user_count}")

# Show roles
if role_count > 0:
    print("\n" + "=" * 60)
    print("Roles:")
    print("=" * 60)
    cur.execute('SELECT roleid, rolename, description FROM roles')
    roles = cur.fetchall()
    for role in roles:
        print(f"  ID: {role[0]}, Name: {role[1]}, Desc: {role[2]}")

# Check Admin role specifically
print("\n" + "=" * 60)
print("Admin Role Check:")
print("=" * 60)
cur.execute('SELECT roleid FROM roles WHERE rolename = \'Administrator\' OR rolename = \'Admin\'')
admin_role = cur.fetchone()
if admin_role:
    print(f"Admin role found with ID: {admin_role[0]}")
    
    # Count permissions for admin
    cur.execute('SELECT COUNT(*) FROM rolepermissions WHERE roleid = %s', (admin_role[0],))
    admin_perms = cur.fetchone()[0]
    print(f"Admin has {admin_perms} permissions")
else:
    print("⚠️ No Administrator or Admin role found!")

cur.close()
conn.close()
