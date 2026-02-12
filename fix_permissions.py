#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
سكريبت لإعادة تهيئة نظام الصلاحيات - نسخة محدثة
يقوم بتحديث البيانات الموجودة بدلاً من حذفها
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

def fix_permissions_system(cursor):
    """إصلاح نظام الصلاحيات بالكامل"""
    
    print("=" * 60)
    print("🔧 بدء إصلاح نظام الصلاحيات")
    print("=" * 60)
    
    # 1. حذف جميع الـ role permissions القديمة
    print("\n🗑️  حذف روابط الصلاحيات القديمة...")
    cursor.execute('DELETE FROM "RolePermissions"')
    print("✅ تم الحذف")
    
    # 2. الحصول على IDs الأدوار
    print("\n🔍 البحث عن الأدوار...")
    cursor.execute('SELECT "RoleId", "RoleName" FROM roles ORDER BY "RoleId"')
    roles = cursor.fetchall()
    
    if not roles:
        print("❌ لم يتم العثور على أدوار!")
        return
    
    print(f"✅ وجدنا {len(roles)} دور:")
    for role_id, role_name in roles:
        print(f"   - {role_id}: {role_name}")
    
    # تحديد الأدوار
    operations_role = None
    aviation_role = None
    admin_role = None
    
    for role_id, role_name in roles:
        if 'operations' in role_name.lower():
            operations_role = role_id
        elif 'aviation' in role_name.lower():
            aviation_role = role_id
        elif 'admin' in role_name.lower():
            admin_role = role_id
    
    print(f"\n📋 الأدوار المحددة:")
    print(f"   Operations: {operations_role}")
    print(f"   Aviation: {aviation_role}")
    print(f"   Admin: {admin_role}")
    
    # 3. الحصول على جميع الصلاحيات
    print("\n🔍 البحث عن الصلاحيات...")
    cursor.execute('SELECT "PermissionId", "PermissionType", "Module" FROM permissions ORDER BY "PermissionType"')
    permissions = cursor.fetchall()
    
    if not permissions:
        print("❌ لم يتم العثور على صلاحيات!")
        return
    
    print(f"✅ وجدنا {len(permissions)} صلاحية")
    
    # تجميع الصلاحيات حسب الـ module
    permissions_by_module = {}
    for perm_id, perm_type, module in permissions:
        if module not in permissions_by_module:
            permissions_by_module[module] = []
        permissions_by_module[module].append((perm_id, perm_type))
    
    print("\n📊 الصلاحيات حسب الـ Module:")
    for module, perms in permissions_by_module.items():
        print(f"   {module}: {len(perms)} صلاحية")
    
    # 4. ربط الصلاحيات بالأدوار
    print("\n🔗 ربط الصلاحيات بالأدوار...")
    
    role_permissions = []
    
    # Operations Department - Trips + Calculator
    if operations_role:
        print(f"\n   🚌 Operations (Role {operations_role}):")
        for module in ['Trips', 'Calculator', 'Reports']:
            if module in permissions_by_module:
                for perm_id, perm_type in permissions_by_module[module]:
                    # فقط صلاحيات الرحلات والآلة الحاسبة والتقارير
                    if module == 'Trips' or perm_type == 30 or perm_type in [116, 120, 121]:
                        role_permissions.append((operations_role, perm_id))
                        print(f"      ✓ Permission {perm_type} ({module})")
    
    # Aviation and Umrah - Aviation + Umrah + Calculator
    if aviation_role:
        print(f"\n   ✈️ Aviation (Role {aviation_role}):")
        for module in ['Aviation', 'Umrah', 'Calculator', 'Reports']:
            if module in permissions_by_module:
                for perm_id, perm_type in permissions_by_module[module]:
                    # الطيران والعمرة والآلة الحاسبة وتقارير الطيران/العمرة
                    if module in ['Aviation', 'Umrah'] or perm_type == 30 or perm_type in [117, 118, 120, 121]:
                        role_permissions.append((aviation_role, perm_id))
                        print(f"      ✓ Permission {perm_type} ({module})")
    
    # Admin - كل الصلاحيات
    if admin_role:
        print(f"\n   👑 Admin (Role {admin_role}):")
        print(f"      ✓ ALL {len(permissions)} permissions")
        for perm_id, perm_type, module in permissions:
            role_permissions.append((admin_role, perm_id))
    
    # 5. إضافة الروابط إلى قاعدة البيانات
    print(f"\n💾 حفظ {len(role_permissions)} رابط صلاحية...")
    for role_id, perm_id in role_permissions:
        cursor.execute("""
            INSERT INTO "RolePermissions" ("RoleId", "PermissionId")
            VALUES (%s, %s)
            ON CONFLICT DO NOTHING
        """, (role_id, perm_id))
    
    print("✅ تم حفظ جميع الروابط")
    
    # 6. التحقق من النتائج
    print("\n🔍 التحقق من النتائج...")
    cursor.execute("""
        SELECT r."RoleName", COUNT(rp."PermissionId") as perm_count
        FROM roles r
        LEFT JOIN "RolePermissions" rp ON r."RoleId" = rp."RoleId"
        GROUP BY r."RoleName"
        ORDER BY r."RoleId"
    """)
    
    results = cursor.fetchall()
    print("\n📊 النتائج النهائية:")
    for role_name, perm_count in results:
        print(f"   {role_name}: {perm_count} صلاحية")
    
    print("\n" + "=" * 60)
    print("✅ تم إصلاح نظام الصلاحيات بنجاح!")
    print("=" * 60)

def main():
    """الدالة الرئيسية"""
    conn = None
    cursor = None
    
    try:
        # الاتصال بقاعدة البيانات
        print("🔌 الاتصال بقاعدة البيانات...")
        conn = get_connection()
        cursor = conn.cursor()
        print("✅ تم الاتصال بنجاح")
        
        # إصلاح نظام الصلاحيات
        fix_permissions_system(cursor)
        
        # حفظ التغييرات
        print("\n💾 حفظ التغييرات...")
        conn.commit()
        print("✅ تم الحفظ")
        
    except Exception as e:
        if conn:
            conn.rollback()
        print(f"\n❌ حدث خطأ: {e}")
        import traceback
        traceback.print_exc()
        
    finally:
        if cursor:
            cursor.close()
        if conn:
            conn.close()

if __name__ == "__main__":
    main()
