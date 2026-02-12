import psycopg2

try:
    conn = psycopg2.connect(
        host='localhost',
        port=5432,
        database='graceway_accounting',
        user='postgres',
        password='123456'
    )
    cursor = conn.cursor()
    
    print("=== Checking Modules for 'operations' user ===\n")
    
    # الحصول على المستخدم
    cursor.execute("""
        SELECT userid, username, roleid
        FROM users 
        WHERE username = 'operations'
    """)
    
    user = cursor.fetchone()
    if user:
        print(f"User: {user[1]} (ID: {user[0]}, RoleId: {user[2]})")
        
        # الحصول على الصلاحيات مجمعة حسب Module
        cursor.execute("""
            SELECT DISTINCT p."Module"
            FROM rolepermissions rp
            JOIN permissions p ON rp.permissionid = p.permissionid
            WHERE rp.roleid = %s
            ORDER BY p."Module"
        """, (user[2],))
        
        modules = cursor.fetchall()
        print(f"\nModules found ({len(modules)}):")
        for module in modules:
            print(f"  - '{module[0]}'")
            
        print("\n=== Testing Module Check ===")
        module_dict = {m[0]: True for m in modules}
        print(f"Module Dict: {module_dict}")
        print(f"Has 'Operations': {module_dict.get('Operations', False)}")
        print(f"Has 'Trips': {module_dict.get('Trips', False)}")
        
    cursor.close()
    conn.close()
    
except Exception as e:
    print(f"Error: {e}")
    import traceback
    traceback.print_exc()
