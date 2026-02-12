#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
سكريبت لإعادة تهيئة نظام الصلاحيات والأدوار
يقوم بحذف البيانات القديمة وإعادة إنشائها من جديد
"""

import psycopg2
import bcrypt
from datetime import datetime
import sys

# إعداد الترميز لدعم UTF-8 في Windows
if sys.platform == 'win32':
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

# معلومات الاتصال بقاعدة البيانات
DB_CONFIG = {
    'host': 'localhost',
    'port': 5432,
    'database': 'graceway_accounting',
    'user': 'postgres',
    'password': '123456'
}

def get_connection():
    """إنشاء اتصال مع قاعدة البيانات"""
    return psycopg2.connect(**DB_CONFIG)

def clear_old_data(cursor):
    """حذف البيانات القديمة"""
    print("🗑️  جاري حذف البيانات القديمة...")
    
    # حذف البيانات بالترتيب الصحيح (من الأطفال إلى الآباء)
    # PostgreSQL يحول الأسماء لـ lowercase
    cursor.execute("DELETE FROM rolepermissions")
    cursor.execute("DELETE FROM roles")
    cursor.execute("DELETE FROM permissions")
    
    # حذف المستخدمين بعناية (تحديث userid=1 للـ trips أولاً)
    try:
        # تحديث جميع الـ trips لتشير إلى userid=1 كمؤقت
        cursor.execute("UPDATE trips SET createdby = NULL WHERE createdby IS NOT NULL")
        cursor.execute("UPDATE trips SET updatedby = NULL WHERE updatedby IS NOT NULL")
        cursor.execute("DELETE FROM users")
    except Exception as e:
        print(f"⚠️ تحذير عند حذف المستخدمين: {e}")
        # نكمل بدون حذف المستخدمين
    
    print("✅ تم حذف البيانات القديمة بنجاح")

def seed_permissions(cursor):
    """إنشاء الصلاحيات"""
    print("📝 جاري إنشاء الصلاحيات...")
    
    permissions = [
        # ============================================
        # قسم الرحلات (Trips)
        # ============================================
        (1, 'عرض الرحلات', 'Trips', 'Trips', False),
        (2, 'إضافة رحلة', 'Trips', 'Trips', False),
        (3, 'تعديل رحلة', 'Trips', 'Trips', False),
        (4, 'حذف رحلة', 'Trips', 'Trips', False),
        (5, 'إغلاق رحلة', 'Trips', 'Trips', False),
        (6, 'إدارة حجوزات الرحلات', 'Trips', 'Trips', False),
        
        # ============================================
        # قسم الطيران (Aviation)
        # ============================================
        (10, 'عرض حجوزات الطيران', 'Aviation', 'Aviation', False),
        (11, 'إضافة حجز طيران', 'Aviation', 'Aviation', False),
        (12, 'تعديل حجز طيران', 'Aviation', 'Aviation', False),
        (13, 'حذف حجز طيران', 'Aviation', 'Aviation', False),
        (14, 'إدارة مدفوعات الطيران', 'Aviation', 'Aviation', False),
        
        # ============================================
        # قسم العمرة (Umrah)
        # ============================================
        (20, 'عرض باقات العمرة', 'Umrah', 'Umrah', False),
        (21, 'إضافة باقة عمرة', 'Umrah', 'Umrah', False),
        (22, 'تعديل باقة عمرة', 'Umrah', 'Umrah', False),
        (23, 'حذف باقة عمرة', 'Umrah', 'Umrah', False),
        (24, 'عرض رحلات العمرة', 'Umrah', 'Umrah', False),
        (25, 'إضافة رحلة عمرة', 'Umrah', 'Umrah', False),
        (26, 'تعديل رحلة عمرة', 'Umrah', 'Umrah', False),
        (27, 'حذف رحلة عمرة', 'Umrah', 'Umrah', False),
        (28, 'إدارة معتمرين', 'Umrah', 'Umrah', False),
        (29, 'إدارة مدفوعات العمرة', 'Umrah', 'Umrah', False),
        
        # ============================================
        # الآلة الحاسبة
        # ============================================
        (30, 'استخدام الآلة الحاسبة', 'Tools', 'Calculator', False),
        
        # ============================================
        # قسم العملاء
        # ============================================
        (40, 'عرض العملاء', 'Customers', 'Accounting', False),
        (41, 'إضافة عميل', 'Customers', 'Accounting', False),
        (42, 'تعديل عميل', 'Customers', 'Accounting', False),
        (43, 'حذف عميل', 'Customers', 'Accounting', False),
        (44, 'عرض كشف حساب عميل', 'Customers', 'Accounting', False),
        
        # ============================================
        # قسم الموردين
        # ============================================
        (50, 'عرض الموردين', 'Suppliers', 'Accounting', False),
        (51, 'إضافة مورد', 'Suppliers', 'Accounting', False),
        (52, 'تعديل مورد', 'Suppliers', 'Accounting', False),
        (53, 'حذف مورد', 'Suppliers', 'Accounting', False),
        (54, 'عرض كشف حساب مورد', 'Suppliers', 'Accounting', False),
        
        # ============================================
        # قسم الفواتير
        # ============================================
        (60, 'عرض الفواتير', 'Invoices', 'Accounting', False),
        (61, 'إضافة فاتورة بيع', 'Invoices', 'Accounting', False),
        (62, 'تعديل فاتورة بيع', 'Invoices', 'Accounting', False),
        (63, 'حذف فاتورة بيع', 'Invoices', 'Accounting', False),
        (64, 'إضافة فاتورة شراء', 'Invoices', 'Accounting', False),
        (65, 'تعديل فاتورة شراء', 'Invoices', 'Accounting', False),
        (66, 'حذف فاتورة شراء', 'Invoices', 'Accounting', False),
        (67, 'اعتماد فاتورة', 'Invoices', 'Accounting', False),
        
        # ============================================
        # قسم الحجوزات
        # ============================================
        (70, 'عرض الحجوزات', 'Reservations', 'Operations', False),
        (71, 'إضافة حجز', 'Reservations', 'Operations', False),
        (72, 'تعديل حجز', 'Reservations', 'Operations', False),
        (73, 'حذف حجز', 'Reservations', 'Operations', False),
        
        # ============================================
        # قسم الخزنة والبنوك
        # ============================================
        (80, 'عرض الخزنة', 'Cash', 'Accounting', False),
        (81, 'إضافة حركة نقدية', 'Cash', 'Accounting', False),
        (82, 'تعديل حركة نقدية', 'Cash', 'Accounting', False),
        (83, 'حذف حركة نقدية', 'Cash', 'Accounting', False),
        (84, 'عرض الحسابات البنكية', 'Bank', 'Accounting', False),
        (85, 'إضافة حركة بنكية', 'Bank', 'Accounting', False),
        (86, 'تعديل حركة بنكية', 'Bank', 'Accounting', False),
        (87, 'حذف حركة بنكية', 'Bank', 'Accounting', False),
        (88, 'إدارة التحويلات البنكية', 'Bank', 'Accounting', False),
        
        # ============================================
        # قسم القيود اليومية
        # ============================================
        (90, 'عرض القيود اليومية', 'Journal', 'Accounting', False),
        (91, 'إضافة قيد يومي', 'Journal', 'Accounting', False),
        (92, 'تعديل قيد يومي', 'Journal', 'Accounting', False),
        (93, 'حذف قيد يومي', 'Journal', 'Accounting', False),
        (94, 'تعديل فترة مغلقة', 'Journal', 'Accounting', True),
        
        # ============================================
        # قسم الحسابات
        # ============================================
        (100, 'عرض شجرة الحسابات', 'Accounts', 'Accounting', False),
        (101, 'إضافة حساب', 'Accounts', 'Accounting', False),
        (102, 'تعديل حساب', 'Accounts', 'Accounting', False),
        (103, 'حذف حساب', 'Accounts', 'Accounting', False),
        
        # ============================================
        # قسم التقارير
        # ============================================
        (110, 'عرض التقارير', 'Reports', 'Reports', False),
        (111, 'عرض التقارير المالية', 'Reports', 'Reports', False),
        (112, 'عرض ميزان المراجعة', 'Reports', 'Reports', False),
        (113, 'عرض قائمة الدخل', 'Reports', 'Reports', False),
        (114, 'عرض الميزانية العمومية', 'Reports', 'Reports', False),
        (115, 'عرض قائمة التدفقات النقدية', 'Reports', 'Reports', False),
        (116, 'عرض تقارير الرحلات', 'Reports', 'Reports', False),
        (117, 'عرض تقارير الطيران', 'Reports', 'Reports', False),
        (118, 'عرض تقارير العمرة', 'Reports', 'Reports', False),
        (119, 'عرض هوامش الربح', 'Reports', 'Reports', True),
        (120, 'تصدير التقارير', 'Reports', 'Reports', False),
        (121, 'طباعة التقارير', 'Reports', 'Reports', False),
        
        # ============================================
        # قسم الإعدادات
        # ============================================
        (130, 'عرض الإعدادات', 'Settings', 'System', False),
        (131, 'تعديل إعدادات الشركة', 'Settings', 'System', True),
        (132, 'تعديل إعدادات الفواتير', 'Settings', 'System', False),
        (133, 'تعديل إعدادات السنة المالية', 'Settings', 'System', True),
        (134, 'إدارة العملات', 'Settings', 'System', False),
        (135, 'إدارة أنواع الخدمات', 'Settings', 'System', False),
        
        # ============================================
        # قسم إدارة النظام
        # ============================================
        (140, 'إدارة المستخدمين', 'Administration', 'System', True),
        (141, 'إدارة الأدوار', 'Administration', 'System', True),
        (142, 'إدارة الصلاحيات', 'Administration', 'System', True),
        (143, 'عرض سجل التدقيق', 'Administration', 'System', True),
        (144, 'عرض سجل النظام', 'Administration', 'System', True),
        (145, 'نسخ احتياطي لقاعدة البيانات', 'Administration', 'System', True),
        (146, 'استعادة قاعدة البيانات', 'Administration', 'System', True),
        (147, 'إدارة الجلسات', 'Administration', 'System', True),
    ]
    
    for perm_type, perm_name, category, module, is_system in permissions:
        cursor.execute("""
            INSERT INTO permissions (permissiontype, permissionname, category, module, issystempermission)
            VALUES (%s, %s, %s, %s, %s)
        """, (perm_type, perm_name, category, module, is_system))
    
    print(f"✅ تم إنشاء {len(permissions)} صلاحية بنجاح")

def seed_roles(cursor):
    """إنشاء الأدوار"""
    print("👥 جاري إنشاء الأدوار...")
    
    roles = [
        ('Operations Department', 'قسم العمليات - الوصول إلى الرحلات والآلة الحاسبة فقط'),
        ('Aviation and Umrah', 'قسم الطيران والعمرة - الوصول إلى الطيران والعمرة والآلة الحاسبة'),
        ('Administrator', 'المدير - الوصول الكامل لجميع أقسام النظام'),
    ]
    
    for role_name, description in roles:
        cursor.execute("""
            INSERT INTO roles (rolename, description)
            VALUES (%s, %s)
        """, (role_name, description))
    
    print(f"✅ تم إنشاء {len(roles)} دور بنجاح")

def seed_role_permissions(cursor):
    """ربط الصلاحيات بالأدوار"""
    print("🔗 جاري ربط الصلاحيات بالأدوار...")
    
    # الحصول على IDs الأدوار
    cursor.execute("SELECT roleid FROM roles WHERE rolename = 'Operations Department'")
    operations_role_id = cursor.fetchone()[0]
    
    cursor.execute("SELECT roleid FROM roles WHERE rolename = 'Aviation and Umrah'")
    aviation_umrah_role_id = cursor.fetchone()[0]
    
    cursor.execute("SELECT roleid FROM roles WHERE rolename = 'Administrator'")
    admin_role_id = cursor.fetchone()[0]
    
    # صلاحيات Operations Department
    operations_permissions = [1, 2, 3, 4, 5, 6, 30, 116, 120, 121]  # Trips + Calculator + Reports
    
    # صلاحيات Aviation and Umrah
    aviation_umrah_permissions = [
        10, 11, 12, 13, 14,  # Aviation
        20, 21, 22, 23, 24, 25, 26, 27, 28, 29,  # Umrah
        30,  # Calculator
        117, 118, 120, 121  # Reports
    ]
    
    # ربط صلاحيات Operations
    for perm_type in operations_permissions:
        cursor.execute("""
            INSERT INTO rolepermissions (roleid, permissionid)
            SELECT %s, permissionid FROM permissions WHERE permissiontype = %s
        """, (operations_role_id, perm_type))
    
    # ربط صلاحيات Aviation and Umrah
    for perm_type in aviation_umrah_permissions:
        cursor.execute("""
            INSERT INTO rolepermissions (roleid, permissionid)
            SELECT %s, permissionid FROM permissions WHERE permissiontype = %s
        """, (aviation_umrah_role_id, perm_type))
    
    # ربط جميع الصلاحيات بـ Administrator
    cursor.execute("""
        INSERT INTO rolepermissions (roleid, permissionid)
        SELECT %s, permissionid FROM permissions
    """, (admin_role_id,))
    
    print("✅ تم ربط الصلاحيات بالأدوار بنجاح")

def seed_users(cursor):
    """إنشاء المستخدمين"""
    print("👤 جاري إنشاء المستخدمين...")
    
    # الحصول على IDs الأدوار
    cursor.execute("SELECT roleid FROM roles WHERE rolename = 'Operations Department'")
    operations_role_id = cursor.fetchone()[0]
    
    cursor.execute("SELECT roleid FROM roles WHERE rolename = 'Aviation and Umrah'")
    aviation_umrah_role_id = cursor.fetchone()[0]
    
    cursor.execute("SELECT roleid FROM roles WHERE rolename = 'Administrator'")
    admin_role_id = cursor.fetchone()[0]
    
    # تشفير كلمات المرور
    operations_password = bcrypt.hashpw('operations123'.encode('utf-8'), bcrypt.gensalt()).decode('utf-8')
    aviation_password = bcrypt.hashpw('aviation123'.encode('utf-8'), bcrypt.gensalt()).decode('utf-8')
    admin_password = bcrypt.hashpw('admin123'.encode('utf-8'), bcrypt.gensalt()).decode('utf-8')
    
    users = [
        ('operations', operations_password, 'قسم العمليات', 'operations@graceway.com', operations_role_id),
        ('aviation', aviation_password, 'قسم الطيران والعمرة', 'aviation@graceway.com', aviation_umrah_role_id),
        ('admin', admin_password, 'المدير العام', 'admin@graceway.com', admin_role_id),
    ]
    
    now = datetime.utcnow()
    for username, password_hash, full_name, email, role_id in users:
        cursor.execute("""
            INSERT INTO users (username, passwordhash, fullname, email, roleid, isactive, createdat, updatedat)
            VALUES (%s, %s, %s, %s, %s, true, %s, %s)
        """, (username, password_hash, full_name, email, role_id, now, now))
    
    print(f"✅ تم إنشاء {len(users)} مستخدم بنجاح")

def main():
    """الدالة الرئيسية"""
    print("=" * 60)
    print("🚀 بدء إعادة تهيئة نظام الصلاحيات والأدوار")
    print("=" * 60)
    
    conn = None
    cursor = None
    
    try:
        # الاتصال بقاعدة البيانات
        conn = get_connection()
        cursor = conn.cursor()
        
        # حذف البيانات القديمة
        clear_old_data(cursor)
        
        # إنشاء البيانات الجديدة
        seed_permissions(cursor)
        seed_roles(cursor)
        seed_role_permissions(cursor)
        seed_users(cursor)
        
        # حفظ التغييرات
        conn.commit()
        
        print("=" * 60)
        print("✅ تمت إعادة التهيئة بنجاح!")
        print("=" * 60)
        print("\n📋 بيانات تسجيل الدخول:")
        print("  🚌 Operations: username=operations, password=operations123")
        print("  ✈️ Aviation: username=aviation, password=aviation123")
        print("  👑 Admin: username=admin, password=admin123")
        print("\n")
        
    except Exception as e:
        if conn:
            conn.rollback()
        print(f"❌ حدث خطأ: {e}")
        import traceback
        traceback.print_exc()
        
    finally:
        if cursor:
            cursor.close()
        if conn:
            conn.close()

if __name__ == "__main__":
    main()
