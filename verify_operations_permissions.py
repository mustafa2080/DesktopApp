#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
التحقق من صلاحيات Operations Department
"""

import sqlite3
import sys

# Fix encoding for Windows console
if sys.platform == 'win32':
    sys.stdout.reconfigure(encoding='utf-8')

def verify_operations_permissions():
    print("=" * 70)
    print("🔍 التحقق من صلاحيات Operations Department")
    print("=" * 70)
    
    conn = sqlite3.connect('accountant.db')
    cursor = conn.cursor()
    
    # 1. الحصول على معلومات Operations Role
    print("\n1️⃣ معلومات دور Operations Department:")
    cursor.execute("""
        SELECT role_id, role_name, description 
        FROM roles 
        WHERE role_name = 'Operations Department'
    """)
    role = cursor.fetchone()
    if role:
        role_id, role_name, desc = role
        print(f"   ✓ ID: {role_id}")
        print(f"   ✓ الاسم: {role_name}")
        print(f"   ✓ الوصف: {desc}")
    else:
        print("   ❌ الدور غير موجود!")
        return
    
    # 2. الحصول على الصلاحيات المرتبطة بهذا الدور
    print(f"\n2️⃣ الصلاحيات المرتبطة بـ Operations Department (Role ID: {role_id}):")
    cursor.execute("""
        SELECT p.permission_id, p.permission_name, p.category, p.module
        FROM permissions p
        INNER JOIN role_permissions rp ON p.permission_id = rp.permission_id
        WHERE rp.role_id = ?
        ORDER BY p.module, p.category
    """, (role_id,))
    
    permissions = cursor.fetchall()
    if permissions:
        print(f"   إجمالي الصلاحيات: {len(permissions)}")
        
        # تجميع حسب Module
        modules = {}
        for perm_id, perm_name, category, module in permissions:
            if module not in modules:
                modules[module] = []
            modules[module].append((perm_name, category))
        
        print("\n   📊 توزيع الصلاحيات حسب Module:")
        for module, perms in modules.items():
            print(f"\n   📦 Module: {module} ({len(perms)} صلاحيات)")
            for perm_name, category in perms:
                print(f"      - {perm_name} ({category})")
    else:
        print("   ❌ لا توجد صلاحيات!")
    
    # 3. التحقق من مستخدم operations
    print(f"\n3️⃣ التحقق من مستخدم operations:")
    cursor.execute("""
        SELECT u.user_id, u.username, u.full_name, r.role_name
        FROM users u
        INNER JOIN roles r ON u.role_id = r.role_id
        WHERE u.username = 'operations'
    """)
    user = cursor.fetchone()
    if user:
        user_id, username, full_name, role_name = user
        print(f"   ✓ ID: {user_id}")
        print(f"   ✓ اسم المستخدم: {username}")
        print(f"   ✓ الاسم الكامل: {full_name}")
        print(f"   ✓ الدور: {role_name}")
    else:
        print("   ❌ المستخدم غير موجود!")
    
    # 4. الخلاصة
    print("\n" + "=" * 70)
    print("📋 الخلاصة:")
    print("=" * 70)
    
    if 'Trips' in modules:
        print("✅ Operations Department لديه صلاحيات في Module: Trips")
        print("✅ الكود يجب أن يتحقق من وجود 'Trips' module")
        print("✅ عند تسجيل دخول operations، يجب أن يرى:")
        print("   - لوحة التحكم (مفعلة)")
        print("   - الرحلات (مفعلة)")
        print("   - الآلة الحاسبة (مفعلة)")
        print("   - باقي الأقسام (معروضة لكن معطلة)")
    else:
        print("❌ Operations Department ليس لديه صلاحيات Trips!")
        print("⚠️  يجب مراجعة PermissionSeeder!")
    
    conn.close()

if __name__ == "__main__":
    verify_operations_permissions()
