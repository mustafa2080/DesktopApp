import psycopg2

try:
    # الاتصال بقاعدة البيانات
    conn = psycopg2.connect(
        host='localhost',
        port=5432,
        database='graceway_accounting',
        user='postgres',
        password='123456'
    )
    cursor = conn.cursor()
    
    print("\n=== Checking 'aviation' user ===")
    
    # البحث عن المستخدم aviation
    cursor.execute("""
        SELECT userid, username, fullname, roleid, isactive
        FROM users 
        WHERE username = 'aviation'
    """)
    
    user = cursor.fetchone()
    if user:
        print(f"\nUser Found:")
        print(f"  UserId: {user[0]}")
        print(f"  Username: {user[1]}")
        print(f"  FullName: {user[2]}")
        print(f"  RoleId: {user[3]}")
        print(f"  IsActive: {user[4]}")
        
        # الحصول على الدور
        cursor.execute("""
            SELECT roleid, rolename
            FROM roles
            WHERE roleid = %s
        """, (user[3],))
        
        role = cursor.fetchone()
        if role:
            print(f"\nRole:")
            print(f"  RoleId: {role[0]}")
            print(f"  RoleName: {role[1]}")
            
            # الحصول على الصلاحيات
            cursor.execute("""
                SELECT p.permissionid, p.permissionname, p."Module"
                FROM rolepermissions rp
                JOIN permissions p ON rp.permissionid = p.permissionid
                WHERE rp.roleid = %s
                ORDER BY p."Module", p.permissionname
            """, (role[0],))
            
            permissions = cursor.fetchall()
            if permissions:
                print(f"\nPermissions ({len(permissions)} total):")
                current_module = None
                for perm in permissions:
                    if perm[2] != current_module:
                        current_module = perm[2]
                        print(f"\n  Module: {current_module}")
                    print(f"    - {perm[1]}")
            else:
                print("\n  No permissions found!")
    else:
        print("User 'aviation' not found!")
    
    cursor.close()
    conn.close()
    
except Exception as e:
    print(f"Error: {e}")
    import traceback
    traceback.print_exc()
