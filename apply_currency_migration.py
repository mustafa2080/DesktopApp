import sqlite3
import os
import sys

# Fix encoding for Windows console
sys.stdout.reconfigure(encoding='utf-8')

db_path = r'C:\Users\musta\Desktop\pro\accountant\accountant.db'

if not os.path.exists(db_path):
    print(f"❌ الداتابيز غير موجودة: {db_path}")
    exit(1)

conn = sqlite3.connect(db_path)
cursor = conn.cursor()

print("=== تطبيق Migration: إضافة دعم العملات للخزنة ===\n")

try:
    # Check if columns already exist
    cursor.execute("PRAGMA table_info(CashTransactions)")
    columns = [col[1] for col in cursor.fetchall()]
    
    print(f"الأعمدة الحالية: {', '.join(columns)}\n")
    
    # Add TransactionCurrency column
    if 'transactioncurrency' not in columns:
        print("✅ إضافة عمود transactioncurrency...")
        cursor.execute("""
            ALTER TABLE CashTransactions 
            ADD COLUMN transactioncurrency TEXT NOT NULL DEFAULT 'EGP'
        """)
        print("   تم بنجاح!\n")
    else:
        print("⚠️ عمود transactioncurrency موجود بالفعل\n")
    
    # Add ExchangeRateUsed column
    if 'exchangerateused' not in columns:
        print("✅ إضافة عمود exchangerateused...")
        cursor.execute("""
            ALTER TABLE CashTransactions 
            ADD COLUMN exchangerateused REAL
        """)
        print("   تم بنجاح!\n")
    else:
        print("⚠️ عمود exchangerateused موجود بالفعل\n")
    
    # Add OriginalAmount column  
    if 'originalamount' not in columns:
        print("✅ إضافة عمود originalamount...")
        cursor.execute("""
            ALTER TABLE CashTransactions 
            ADD COLUMN originalamount REAL
        """)
        print("   تم بنجاح!\n")
    else:
        print("⚠️ عمود originalamount موجود بالفعل\n")
    
    # Update existing data
    print("✅ تحديث البيانات الموجودة...")
    cursor.execute("""
        UPDATE CashTransactions 
        SET transactioncurrency = 'EGP' 
        WHERE transactioncurrency IS NULL OR transactioncurrency = ''
    """)
    print(f"   تم تحديث {cursor.rowcount} صف\n")
    
    # Also add UpdatedBy column if missing
    if 'updatedby' not in columns:
        print("✅ إضافة عمود updatedby...")
        cursor.execute("""
            ALTER TABLE CashTransactions 
            ADD COLUMN updatedby INTEGER
        """)
        print("   تم بنجاح!\n")
    
    if 'updatedat' not in columns:
        print("✅ إضافة عمود updatedat...")
        cursor.execute("""
            ALTER TABLE CashTransactions 
            ADD COLUMN updatedat TEXT
        """)
        print("   تم بنجاح!\n")
    
    # Commit changes
    conn.commit()
    print("=" * 60)
    print("✅ تم تطبيق الـ Migration بنجاح!")
    print("=" * 60)
    
    # Verify
    print("\n=== التحقق من البنية الجديدة ===")
    cursor.execute("PRAGMA table_info(CashTransactions)")
    print("\nID | Name | Type | NotNull | Default | PK")
    print("-" * 80)
    for col in cursor.fetchall():
        print(f"{col[0]} | {col[1]} | {col[2]} | {col[3]} | {col[4]} | {col[5]}")
    
except Exception as e:
    print(f"❌ خطأ: {e}")
    conn.rollback()
finally:
    conn.close()

print("\n✅ انتهى!")
