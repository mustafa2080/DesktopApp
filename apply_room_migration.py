#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
تطبيق Migration لإضافة حقول الغرفة للمعتمرين
"""

import sqlite3
import os
from pathlib import Path

def apply_migration():
    # مسار قاعدة البيانات
    db_path = Path(__file__).parent / "accountant.db"
    
    if not db_path.exists():
        print(f"ERROR: Database not found: {db_path}")
        return False
    
    try:
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()
        
        print("Applying Migration...")
        
        # التحقق من وجود الأعمدة
        cursor.execute("PRAGMA table_info(UmrahPilgrims)")
        columns = [row[1] for row in cursor.fetchall()]
        
        # إضافة عمود RoomType إذا لم يكن موجوداً
        if "RoomType" not in columns:
            print("  + Adding RoomType column...")
            cursor.execute("""
                ALTER TABLE UmrahPilgrims 
                ADD COLUMN RoomType INTEGER NULL
            """)
            print("  OK: RoomType column added")
        else:
            print("  INFO: RoomType column already exists")
        
        # إضافة عمود SharedRoomNumber إذا لم يكن موجوداً
        if "SharedRoomNumber" not in columns:
            print("  + Adding SharedRoomNumber column...")
            cursor.execute("""
                ALTER TABLE UmrahPilgrims 
                ADD COLUMN SharedRoomNumber TEXT NULL
            """)
            print("  OK: SharedRoomNumber column added")
        else:
            print("  INFO: SharedRoomNumber column already exists")
        
        # إنشاء الفهرس
        print("  + Creating index on SharedRoomNumber...")
        try:
            cursor.execute("""
                CREATE INDEX IF NOT EXISTS IX_UmrahPilgrims_SharedRoomNumber 
                ON UmrahPilgrims(SharedRoomNumber)
            """)
            print("  OK: Index created")
        except Exception as e:
            print(f"  INFO: Index already exists or error: {e}")
        
        conn.commit()
        
        # التحقق من النتيجة
        cursor.execute("PRAGMA table_info(UmrahPilgrims)")
        all_columns = cursor.fetchall()
        
        print("\nSUCCESS: Migration applied successfully!")
        print(f"\nUmrahPilgrims table columns ({len(all_columns)} columns):")
        for col in all_columns:
            col_id, name, col_type, not_null, default, pk = col
            nullable = "NULL" if not not_null else "NOT NULL"
            print(f"   - {name}: {col_type} {nullable}")
        
        conn.close()
        return True
        
    except Exception as e:
        print(f"ERROR: {e}")
        import traceback
        traceback.print_exc()
        return False

if __name__ == "__main__":
    print("=" * 60)
    print("Applying Migration: Add Room Fields to Pilgrims")
    print("=" * 60)
    print()
    
    success = apply_migration()
    
    print()
    print("=" * 60)
    if success:
        print("SUCCESS: Migration completed!")
        print("\nNext steps:")
        print("  - Rebuild the project (dotnet build)")
        print("  - Run the application and test the new feature!")
    else:
        print("FAILED: Migration failed!")
    print("=" * 60)
