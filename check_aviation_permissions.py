import sqlite3
import sys

# Fix encoding for Windows console
if sys.platform == 'win32':
    import codecs
    sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')

conn = sqlite3.connect('C:/Users/musta/Desktop/pro/accountant/accountant.db')
cursor = conn.cursor()

# Get aviation user info
cursor.execute('''
    SELECT u.UserId, u.Username, u.FullName, r.RoleId, r.RoleName 
    FROM Users u 
    LEFT JOIN Roles r ON u.RoleId = r.RoleId 
    WHERE u.Username = "aviation"
''')
user_info = cursor.fetchone()

if user_info:
    print(f'Aviation User Info:')
    print(f'  UserId: {user_info[0]}')
    print(f'  Username: {user_info[1]}')
    print(f'  FullName: {user_info[2]}')
    print(f'  RoleId: {user_info[3]}')
    print(f'  RoleName: {user_info[4]}')
    
    role_id = user_info[3]
    
    # Get current permissions for aviation role
    cursor.execute('''
        SELECT p.PermissionId, p.PermissionName, p.Module
        FROM RolePermissions rp
        JOIN Permissions p ON rp.PermissionId = p.PermissionId
        WHERE rp.RoleId = ?
        ORDER BY p.Module, p.PermissionName
    ''', (role_id,))
    
    perms = cursor.fetchall()
    print(f'\n\nCurrent Permissions by Module (Total: {len(perms)}):')
    current_module = None
    for p in perms:
        if p[2] != current_module:
            current_module = p[2]
            print(f'\n[Module: {current_module}]')
        print(f'  - {p[1]} (ID: {p[0]})')
    
    # Check what permissions should be added
    cursor.execute('''
        SELECT PermissionId, PermissionName, Module 
        FROM Permissions 
        WHERE Module IN ('Aviation', 'Umrah')
        ORDER BY Module, PermissionName
    ''')
    
    aviation_umrah_perms = cursor.fetchall()
    print(f'\n\nAvailable Aviation & Umrah Permissions:')
    current_module = None
    for p in aviation_umrah_perms:
        if p[2] != current_module:
            current_module = p[2]
            print(f'\n[Module: {current_module}]')
        print(f'  - {p[1]} (ID: {p[0]})')
        
        # Check if this permission is already assigned
        cursor.execute('''
            SELECT COUNT(*) 
            FROM RolePermissions 
            WHERE RoleId = ? AND PermissionId = ?
        ''', (role_id, p[0]))
        
        is_assigned = cursor.fetchone()[0] > 0
        if not is_assigned:
            print(f'    --> MISSING! Need to add')
else:
    print('Aviation user not found!')

conn.close()
