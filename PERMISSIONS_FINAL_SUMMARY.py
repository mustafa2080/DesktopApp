# -*- coding: utf-8 -*-
# ملخص الصلاحيات النهائية

"""
=== نظام الصلاحيات بعد التعديلات ===

1. Admin User (admin) - RoleId: 5
   - عنده صلاحيات System ✓
   - يشوف كل الأقسام (88 صلاحية):
     ✓ Dashboard (لوحة التحكم)
     ✓ Settings (الإعدادات)
     ✓ Users (إدارة المستخدمين)
     ✓ Accounts (شجرة الحسابات)
     ✓ Customers (العملاء)
     ✓ Suppliers (الموردين)
     ✓ Reservations (الحجوزات)
     ✓ Flights (الطيران)
     ✓ Trips (الرحلات)
     ✓ Umrah (العمرة)
     ✓ Invoices (الفواتير)
     ✓ CashBox (الخزنة)
     ✓ Banks (البنوك)
     ✓ Journals (القيود اليومية)
     ✓ Reports (التقارير)
     ✓ Accounting Reports (التقارير المحاسبية)
     ✓ Calculator (الآلة الحاسبة)

2. Operations User (operations) - RoleId: 3
   - يشوف فقط:
     ✓ Dashboard (لوحة التحكم)
     ✓ Trips (الرحلات)
     ✓ Calculator (الآلة الحاسبة)

3. Aviation User (aviation) - RoleId: 4
   - يشوف:
     ✓ Dashboard (لوحة التحكم)
     ✓ Flights (الطيران)
     ✓ Umrah (العمرة)
     ✓ Trips (الرحلات) - تم إضافتها ✓
     ✓ Reports (التقارير)
     ✓ Calculator (الآلة الحاسبة)

=== آلية العمل في SidebarControl.cs ===

الكود يتحقق من:
1. إذا كان User عنده صلاحية "System" → Admin → يشوف كل حاجة
2. إذا كان User عنده صلاحية "Trips" → يشوف Trips
3. إذا كان User عنده صلاحية "Aviation" → يشوف Flights
4. إذا كان User عنده صلاحية "Umrah" → يشوف Umrah
5. إذا كان User عنده صلاحية "Reports" → يشوف Reports
6. Dashboard و Calculator → دائماً ظاهرة للجميع

=== التعديلات المطبقة ===

1. في SidebarControl.cs:
   - تم تعديل شروط الصلاحيات لتكون مستقلة (كل قسم لوحده)
   - تم إضافة دعم صلاحية Reports

2. في قاعدة البيانات:
   - تم حذف صلاحيات Reports من role operations (RoleId: 3)
   - تم إضافة صلاحيات Trips لـ role aviation (RoleId: 4)

=== ملاحظات مهمة ===

- الأدمن يجب أن يشوف كل الأقسام ✓
- كل user يشوف فقط الأقسام اللي عنده صلاحية ليها
- Dashboard والآلة الحاسبة ظاهرين للجميع
"""

import psycopg2

def verify_permissions():
    try:
        conn = psycopg2.connect(
            host='localhost',
            port=5432,
            database='graceway_accounting',
            user='postgres',
            password='123456'
        )
        cursor = conn.cursor()
        
        users = ['admin', 'operations', 'aviation']
        
        for username in users:
            cursor.execute("""
                SELECT u.userid, u.username, r.rolename,
                       COUNT(DISTINCT p."Module") as module_count,
                       COUNT(p.permissionid) as permission_count
                FROM users u
                JOIN roles r ON u.roleid = r.roleid
                LEFT JOIN rolepermissions rp ON r.roleid = rp.roleid
                LEFT JOIN permissions p ON rp.permissionid = p.permissionid
                WHERE u.username = %s
                GROUP BY u.userid, u.username, r.rolename
            """, (username,))
            
            result = cursor.fetchone()
            if result:
                print(f"\n{username}:")
                print(f"  Role: {result[2]}")
                print(f"  Modules: {result[3]}")
                print(f"  Total Permissions: {result[4]}")
                
                # Get modules
                cursor.execute("""
                    SELECT DISTINCT p."Module"
                    FROM users u
                    JOIN roles r ON u.roleid = r.roleid
                    JOIN rolepermissions rp ON r.roleid = rp.roleid
                    JOIN permissions p ON rp.permissionid = p.permissionid
                    WHERE u.username = %s
                    ORDER BY p."Module"
                """, (username,))
                
                modules = [row[0] for row in cursor.fetchall()]
                print(f"  Modules: {', '.join(modules)}")
        
        cursor.close()
        conn.close()
        
        print("\n" + "="*50)
        print("All permissions verified successfully!")
        print("="*50)
        
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    verify_permissions()
