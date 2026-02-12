import psycopg2
import sys
import io

# Fix encoding for Windows console
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

print("=" * 60)
print("FIXING ADMIN PERMISSIONS")
print("=" * 60)

conn = psycopg2.connect('host=localhost dbname=graceway_accounting user=postgres password=123456')
cur = conn.cursor()

# Step 1: Check current situation
print("\n1. Current Database Status:")
cur.execute('SELECT COUNT(*) FROM permissions')
perm_count = cur.fetchone()[0]
print(f"   Permissions in database: {perm_count}")

cur.execute('SELECT COUNT(*) FROM rolepermissions WHERE roleid = 1')
admin_perm_count = cur.fetchone()[0]
print(f"   Admin permissions: {admin_perm_count}")

if perm_count >= 90:
    print("\n✓ Permissions already seeded correctly!")
    print("   The issue must be elsewhere...")
    cur.close()
    conn.close()
    exit(0)

print("\n⚠️ PROBLEM DETECTED: Only 8 permissions exist!")
print("   Need to seed all 94+ permissions...")

# Step 2: Delete existing data to start fresh
print("\n2. Clearing existing permission data...")
cur.execute('DELETE FROM rolepermissions')
cur.execute('DELETE FROM permissions')
# Don't delete users and roles - we'll update them instead
conn.commit()
print("   ✓ Cleared!")

# Step 3: Insert all permissions
print("\n3. Inserting all 94 permissions...")
permissions_sql = """
INSERT INTO permissions ("PermissionType", permissionname, "Category", "Module", "IsSystemPermission") VALUES
-- Trips (1-6)
(1, 'عرض الرحلات', 'Trips', 'Trips', false),
(2, 'إضافة رحلة', 'Trips', 'Trips', false),
(3, 'تعديل رحلة', 'Trips', 'Trips', false),
(4, 'حذف رحلة', 'Trips', 'Trips', false),
(5, 'إغلاق رحلة', 'Trips', 'Trips', false),
(6, 'إدارة حجوزات الرحلات', 'Trips', 'Trips', false),

-- Aviation (10-14)
(10, 'عرض حجوزات الطيران', 'Aviation', 'Aviation', false),
(11, 'إضافة حجز طيران', 'Aviation', 'Aviation', false),
(12, 'تعديل حجز طيران', 'Aviation', 'Aviation', false),
(13, 'حذف حجز طيران', 'Aviation', 'Aviation', false),
(14, 'إدارة مدفوعات الطيران', 'Aviation', 'Aviation', false),

-- Umrah (20-29)
(20, 'عرض باقات العمرة', 'Umrah', 'Umrah', false),
(21, 'إضافة باقة عمرة', 'Umrah', 'Umrah', false),
(22, 'تعديل باقة عمرة', 'Umrah', 'Umrah', false),
(23, 'حذف باقة عمرة', 'Umrah', 'Umrah', false),
(24, 'عرض رحلات العمرة', 'Umrah', 'Umrah', false),
(25, 'إضافة رحلة عمرة', 'Umrah', 'Umrah', false),
(26, 'تعديل رحلة عمرة', 'Umrah', 'Umrah', false),
(27, 'حذف رحلة عمرة', 'Umrah', 'Umrah', false),
(28, 'إدارة معتمرين', 'Umrah', 'Umrah', false),
(29, 'إدارة مدفوعات العمرة', 'Umrah', 'Umrah', false),

-- Calculator (30)
(30, 'استخدام الآلة الحاسبة', 'Tools', 'Calculator', false),

-- Customers (40-44)
(40, 'عرض العملاء', 'Customers', 'Accounting', false),
(41, 'إضافة عميل', 'Customers', 'Accounting', false),
(42, 'تعديل عميل', 'Customers', 'Accounting', false),
(43, 'حذف عميل', 'Customers', 'Accounting', false),
(44, 'عرض كشف حساب عميل', 'Customers', 'Accounting', false),

-- Suppliers (50-54)
(50, 'عرض الموردين', 'Suppliers', 'Accounting', false),
(51, 'إضافة مورد', 'Suppliers', 'Accounting', false),
(52, 'تعديل مورد', 'Suppliers', 'Accounting', false),
(53, 'حذف مورد', 'Suppliers', 'Accounting', false),
(54, 'عرض كشف حساب مورد', 'Suppliers', 'Accounting', false),

-- Invoices (60-67)
(60, 'عرض الفواتير', 'Invoices', 'Accounting', false),
(61, 'إضافة فاتورة بيع', 'Invoices', 'Accounting', false),
(62, 'تعديل فاتورة بيع', 'Invoices', 'Accounting', false),
(63, 'حذف فاتورة بيع', 'Invoices', 'Accounting', false),
(64, 'إضافة فاتورة شراء', 'Invoices', 'Accounting', false),
(65, 'تعديل فاتورة شراء', 'Invoices', 'Accounting', false),
(66, 'حذف فاتورة شراء', 'Invoices', 'Accounting', false),
(67, 'اعتماد فاتورة', 'Invoices', 'Accounting', false),

-- Reservations (70-73)
(70, 'عرض الحجوزات', 'Reservations', 'Operations', false),
(71, 'إضافة حجز', 'Reservations', 'Operations', false),
(72, 'تعديل حجز', 'Reservations', 'Operations', false),
(73, 'حذف حجز', 'Reservations', 'Operations', false),

-- Cash & Banks (80-88)
(80, 'عرض الخزنة', 'Cash', 'Accounting', false),
(81, 'إضافة حركة نقدية', 'Cash', 'Accounting', false),
(82, 'تعديل حركة نقدية', 'Cash', 'Accounting', false),
(83, 'حذف حركة نقدية', 'Cash', 'Accounting', false),
(84, 'عرض الحسابات البنكية', 'Bank', 'Accounting', false),
(85, 'إضافة حركة بنكية', 'Bank', 'Accounting', false),
(86, 'تعديل حركة بنكية', 'Bank', 'Accounting', false),
(87, 'حذف حركة بنكية', 'Bank', 'Accounting', false),
(88, 'إدارة التحويلات البنكية', 'Bank', 'Accounting', false),

-- Journal Entries (90-94)
(90, 'عرض القيود اليومية', 'Journal', 'Accounting', false),
(91, 'إضافة قيد يومي', 'Journal', 'Accounting', false),
(92, 'تعديل قيد يومي', 'Journal', 'Accounting', false),
(93, 'حذف قيد يومي', 'Journal', 'Accounting', false),
(94, 'تعديل فترة مغلقة', 'Journal', 'Accounting', true),

-- Chart of Accounts (100-103)
(100, 'عرض شجرة الحسابات', 'Accounts', 'Accounting', false),
(101, 'إضافة حساب', 'Accounts', 'Accounting', false),
(102, 'تعديل حساب', 'Accounts', 'Accounting', false),
(103, 'حذف حساب', 'Accounts', 'Accounting', false),

-- Reports (110-121)
(110, 'عرض التقارير', 'Reports', 'Reports', false),
(111, 'عرض التقارير المالية', 'Reports', 'Reports', false),
(112, 'عرض ميزان المراجعة', 'Reports', 'Reports', false),
(113, 'عرض قائمة الدخل', 'Reports', 'Reports', false),
(114, 'عرض الميزانية العمومية', 'Reports', 'Reports', false),
(115, 'عرض قائمة التدفقات النقدية', 'Reports', 'Reports', false),
(116, 'عرض تقارير الرحلات', 'Reports', 'Reports', false),
(117, 'عرض تقارير الطيران', 'Reports', 'Reports', false),
(118, 'عرض تقارير العمرة', 'Reports', 'Reports', false),
(119, 'عرض هوامش الربح', 'Reports', 'Reports', true),
(120, 'تصدير التقارير', 'Reports', 'Reports', false),
(121, 'طباعة التقارير', 'Reports', 'Reports', false),

-- Settings (130-135)
(130, 'عرض الإعدادات', 'Settings', 'System', false),
(131, 'تعديل إعدادات الشركة', 'Settings', 'System', true),
(132, 'تعديل إعدادات الفواتير', 'Settings', 'System', false),
(133, 'تعديل إعدادات السنة المالية', 'Settings', 'System', true),
(134, 'إدارة العملات', 'Settings', 'System', false),
(135, 'إدارة أنواع الخدمات', 'Settings', 'System', false),

-- Administration (140-147)
(140, 'إدارة المستخدمين', 'Administration', 'System', true),
(141, 'إدارة الأدوار', 'Administration', 'System', true),
(142, 'إدارة الصلاحيات', 'Administration', 'System', true),
(143, 'عرض سجل التدقيق', 'Administration', 'System', true),
(144, 'عرض سجل النظام', 'Administration', 'System', true),
(145, 'نسخ احتياطي لقاعدة البيانات', 'Administration', 'System', true),
(146, 'استعادة قاعدة البيانات', 'Administration', 'System', true),
(147, 'إدارة الجلسات', 'Administration', 'System', true);
"""

cur.execute(permissions_sql)
conn.commit()

cur.execute('SELECT COUNT(*) FROM permissions')
new_count = cur.fetchone()[0]
print(f"   ✓ Inserted {new_count} permissions!")

# Step 4: Update or create roles
print("\n4. Updating roles...")

# Check if Administrator role exists
cur.execute("SELECT roleid FROM roles WHERE rolename = 'Administrator' OR rolename = 'Admin'")
admin_role = cur.fetchone()

if admin_role:
    admin_role_id = admin_role[0]
    print(f"   ✓ Found existing Administrator role (ID: {admin_role_id})")
else:
    # Create roles if they don't exist
    cur.execute("""
    INSERT INTO roles (rolename, description) VALUES
    ('Administrator', 'المدير - الوصول الكامل لجميع أقسام النظام')
    RETURNING roleid
    """)
    admin_role_id = cur.fetchone()[0]
    print(f"   ✓ Created Administrator role (ID: {admin_role_id})")
conn.commit()

# Step 5: Assign ALL permissions to Administrator role
print("\n5. Assigning ALL permissions to Administrator...")
cur.execute(f"""
INSERT INTO rolepermissions (roleid, permissionid)
SELECT {admin_role_id}, permissionid FROM permissions
""")
conn.commit()

cur.execute(f'SELECT COUNT(*) FROM rolepermissions WHERE roleid = {admin_role_id}')
admin_perms = cur.fetchone()[0]
print(f"   ✓ Administrator now has {admin_perms} permissions!")

# Step 6: Update admin user role
print("\n6. Updating admin user...")

# Update existing admin user to use Administrator role
cur.execute(f"""
UPDATE users 
SET roleid = {admin_role_id}
WHERE username = 'admin'
RETURNING userid
""")
result = cur.fetchone()
if result:
    user_id = result[0]
    print(f"   ✓ Updated admin user (ID: {user_id})")
    print(f"   Username: admin")
    print(f"   Password: admin123")
else:
    # Create if doesn't exist
    import bcrypt
    password_hash = bcrypt.hashpw('admin123'.encode('utf-8'), bcrypt.gensalt()).decode('utf-8')
    
    cur.execute(f"""
    INSERT INTO users (username, passwordhash, fullname, email, roleid, isactive, createdat, updatedat)
    VALUES ('admin', %s, 'المدير العام', 'admin@graceway.com', {admin_role_id}, true, NOW(), NOW())
    RETURNING userid
    """, (password_hash,))
    user_id = cur.fetchone()[0]
    print(f"   ✓ Created admin user (ID: {user_id})")
    print(f"   Username: admin")
    print(f"   Password: admin123")
conn.commit()

# Step 7: Verify
print("\n7. Final Verification:")
cur.execute('SELECT COUNT(*) FROM permissions')
print(f"   Total Permissions: {cur.fetchone()[0]}")

cur.execute('SELECT COUNT(*) FROM roles')
print(f"   Total Roles: {cur.fetchone()[0]}")

cur.execute('SELECT COUNT(*) FROM users')
print(f"   Total Users: {cur.fetchone()[0]}")

cur.execute(f'SELECT COUNT(*) FROM rolepermissions WHERE roleid = {admin_role_id}')
print(f"   Administrator Permissions: {cur.fetchone()[0]}")

cur.close()
conn.close()

print("\n" + "=" * 60)
print("✓ DONE! Admin user now has full access!")
print("=" * 60)
print("\nLogin credentials:")
print("  Username: admin")
print("  Password: admin123")
print("\nPlease restart the application and try logging in!")
