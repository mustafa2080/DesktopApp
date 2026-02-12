import sqlite3
import hashlib
import sys

# Fix encoding for Windows
sys.stdout.reconfigure(encoding='utf-8')

def hash_password(password):
    """Hash password using SHA256"""
    return hashlib.sha256(password.encode()).hexdigest()

# Connect to database
conn = sqlite3.connect(r'C:\Users\musta\Desktop\pro\accountant\accountant.db')
cursor = conn.cursor()

print("Creating Users & Permissions System...")
print("="*50)

# 1. Create Roles Table
print("\n1. Creating Roles table...")
cursor.execute("""
CREATE TABLE IF NOT EXISTS Roles (
    RoleId INTEGER PRIMARY KEY AUTOINCREMENT,
    RoleName TEXT NOT NULL UNIQUE,
    Description TEXT
)
""")

# 2. Create Permissions Table
print("2. Creating Permissions table...")
cursor.execute("""
CREATE TABLE IF NOT EXISTS Permissions (
    PermissionId INTEGER PRIMARY KEY AUTOINCREMENT,
    PermissionType INTEGER NOT NULL UNIQUE,
    PermissionName TEXT NOT NULL,
    Description TEXT,
    Category TEXT NOT NULL,
    Module TEXT NOT NULL,
    IsSystemPermission INTEGER DEFAULT 0
)
""")

# 3. Create RolePermissions Table
print("3. Creating RolePermissions table...")
cursor.execute("""
CREATE TABLE IF NOT EXISTS RolePermissions (
    RolePermissionId INTEGER PRIMARY KEY AUTOINCREMENT,
    RoleId INTEGER NOT NULL,
    PermissionId INTEGER NOT NULL,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId),
    FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId),
    UNIQUE(RoleId, PermissionId)
)
""")

# 4. Update Users Table if needed
print("4. Checking Users table...")
cursor.execute("""
CREATE TABLE IF NOT EXISTS Users (
    UserId INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    FullName TEXT NOT NULL,
    Email TEXT,
    Phone TEXT,
    RoleId INTEGER,
    IsActive INTEGER DEFAULT 1,
    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
)
""")

conn.commit()
print("\n✅ Tables created successfully!")

# 5. Insert Basic Roles
print("\n5. Inserting basic roles...")
roles_data = [
    (1, 'Admin', 'مدير النظام - صلاحيات كاملة'),
    (2, 'Aviation and Umrah', 'موظف الطيران والعمرة'),
    (3, 'Operations', 'موظف العمليات والرحلات')
]

for role in roles_data:
    try:
        cursor.execute("INSERT OR IGNORE INTO Roles (RoleId, RoleName, Description) VALUES (?, ?, ?)", role)
        print(f"   ✓ {role[1]}")
    except Exception as e:
        print(f"   ✗ {role[1]}: {e}")

conn.commit()

# 6. Insert Permissions
print("\n6. Inserting permissions...")
permissions_data = [
    # Trips Module
    (1, 'ViewTrips', 'عرض الرحلات', 'Trips', 'Trips', 0),
    (2, 'CreateTrip', 'إضافة رحلة', 'Trips', 'Trips', 0),
    (3, 'EditTrip', 'تعديل رحلة', 'Trips', 'Trips', 0),
    (6, 'ManageTripBookings', 'إدارة حجوزات الرحلات', 'Trips', 'Trips', 0),
    
    # Aviation Module
    (10, 'ViewFlightBookings', 'عرض حجوزات الطيران', 'Aviation', 'Aviation', 0),
    (11, 'CreateFlightBooking', 'إضافة حجز طيران', 'Aviation', 'Aviation', 0),
    (12, 'EditFlightBooking', 'تعديل حجز طيران', 'Aviation', 'Aviation', 0),
    
    # Umrah Module
    (20, 'ViewUmrahPackages', 'عرض باقات العمرة', 'Umrah', 'Umrah', 0),
    (21, 'CreateUmrahPackage', 'إضافة باقة عمرة', 'Umrah', 'Umrah', 0),
    (22, 'EditUmrahPackage', 'تعديل باقة عمرة', 'Umrah', 'Umrah', 0),
    (24, 'ViewUmrahTrips', 'عرض رحلات العمرة', 'Umrah', 'Umrah', 0),
    
    # Calculator
    (30, 'UseCalculator', 'استخدام الآلة الحاسبة', 'Calculator', 'Calculator', 0),
    
    # System Module
    (140, 'ManageUsers', 'إدارة المستخدمين', 'System', 'System', 1),
    (141, 'ManageRoles', 'إدارة الأدوار', 'System', 'System', 1),
    (142, 'ManagePermissions', 'إدارة الصلاحيات', 'System', 'System', 1)
]

for perm in permissions_data:
    try:
        cursor.execute("""
            INSERT OR IGNORE INTO Permissions 
            (PermissionType, PermissionName, Description, Category, Module, IsSystemPermission) 
            VALUES (?, ?, ?, ?, ?, ?)
        """, perm)
        print(f"   ✓ {perm[1]} ({perm[4]})")
    except Exception as e:
        print(f"   ✗ {perm[1]}: {e}")

conn.commit()

# 7. Assign Permissions to Roles
print("\n7. Assigning permissions to roles...")

# Admin Role - All Permissions
print("\n   Admin Role:")
cursor.execute("SELECT PermissionId FROM Permissions")
all_perms = cursor.fetchall()
for perm in all_perms:
    try:
        cursor.execute("INSERT OR IGNORE INTO RolePermissions (RoleId, PermissionId) VALUES (?, ?)", (1, perm[0]))
    except:
        pass
print(f"   ✓ Assigned all {len(all_perms)} permissions")

# Aviation and Umrah Role
print("\n   Aviation and Umrah Role:")
aviation_umrah_perms = [10, 11, 12, 20, 21, 22, 24, 30]  # Aviation + Umrah + Calculator
for perm_type in aviation_umrah_perms:
    cursor.execute("SELECT PermissionId FROM Permissions WHERE PermissionType = ?", (perm_type,))
    result = cursor.fetchone()
    if result:
        cursor.execute("INSERT OR IGNORE INTO RolePermissions (RoleId, PermissionId) VALUES (?, ?)", (2, result[0]))
        print(f"   ✓ Permission {perm_type}")

# Operations Role
print("\n   Operations Role:")
operations_perms = [1, 2, 3, 6, 30]  # Trips + Calculator
for perm_type in operations_perms:
    cursor.execute("SELECT PermissionId FROM Permissions WHERE PermissionType = ?", (perm_type,))
    result = cursor.fetchone()
    if result:
        cursor.execute("INSERT OR IGNORE INTO RolePermissions (RoleId, PermissionId) VALUES (?, ?)", (3, result[0]))
        print(f"   ✓ Permission {perm_type}")

conn.commit()

# 8. Create Default Users
print("\n8. Creating default users...")
users_data = [
    ('admin', hash_password('admin123'), 'مدير النظام', 1, 1),  # Admin
    ('aviation', hash_password('aviation123'), 'موظف الطيران والعمرة', 2, 1),  # Aviation and Umrah
    ('operations', hash_password('operations123'), 'موظف العمليات', 3, 1)  # Operations
]

for user in users_data:
    try:
        cursor.execute("""
            INSERT OR IGNORE INTO Users 
            (Username, PasswordHash, FullName, RoleId, IsActive) 
            VALUES (?, ?, ?, ?, ?)
        """, user)
        print(f"   ✓ {user[0]} - {user[2]}")
    except Exception as e:
        print(f"   ✗ {user[0]}: {e}")

conn.commit()

# 9. Verify Setup
print("\n" + "="*50)
print("VERIFICATION")
print("="*50)

cursor.execute("SELECT COUNT(*) FROM Roles")
print(f"✓ Roles: {cursor.fetchone()[0]}")

cursor.execute("SELECT COUNT(*) FROM Permissions")
print(f"✓ Permissions: {cursor.fetchone()[0]}")

cursor.execute("SELECT COUNT(*) FROM RolePermissions")
print(f"✓ Role-Permission mappings: {cursor.fetchone()[0]}")

cursor.execute("SELECT COUNT(*) FROM Users")
print(f"✓ Users: {cursor.fetchone()[0]}")

print("\n" + "="*50)
print("DEFAULT LOGIN CREDENTIALS")
print("="*50)
print("1. Admin:")
print("   Username: admin")
print("   Password: admin123")
print("\n2. Aviation and Umrah:")
print("   Username: aviation")
print("   Password: aviation123")
print("\n3. Operations:")
print("   Username: operations")
print("   Password: operations123")

conn.close()
print("\n✅ Setup completed successfully!")
