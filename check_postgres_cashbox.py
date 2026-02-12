import psycopg2
import sys

sys.stdout.reconfigure(encoding='utf-8')

try:
    # Connect to PostgreSQL
    conn = psycopg2.connect(
        host="localhost",
        port=5432,
        database="graceway_accounting",
        user="postgres",
        password="123456"
    )
    cursor = conn.cursor()
    
    print("=== الجداول الموجودة في PostgreSQL ===\n")
    
    cursor.execute("""
        SELECT table_name 
        FROM information_schema.tables 
        WHERE table_schema = 'public'
        ORDER BY table_name
    """)
    
    tables = cursor.fetchall()
    print(f"عدد الجداول: {len(tables)}\n")
    for table in tables:
        print(f"  - {table[0]}")
    
    print("\n=== فحص جدول CashTransactions ===\n")
    
    # Try lowercase first
    cursor.execute("""
        SELECT table_name 
        FROM information_schema.tables 
        WHERE table_schema = 'public'
        AND LOWER(table_name) LIKE '%cash%'
    """)
    
    cash_tables = cursor.fetchall()
    if cash_tables:
        print("جداول الخزنة الموجودة:")
        for table in cash_tables:
            print(f"  - {table[0]}")
            
            # Get structure
            cursor.execute(f"""
                SELECT column_name, data_type
                FROM information_schema.columns
                WHERE table_name = '{table[0]}'
                ORDER BY ordinal_position
            """)
            columns = cursor.fetchall()
            print(f"    الأعمدة ({len(columns)}):")
            for col in columns[:10]:  # First 10 columns
                print(f"      - {col[0]} ({col[1]})")
            if len(columns) > 10:
                print(f"      ... و {len(columns)-10} عمود آخر")
            
            # Count rows
            cursor.execute(f'SELECT COUNT(*) FROM "{table[0]}"')
            count = cursor.fetchone()[0]
            print(f"    عدد الصفوف: {count}\n")
    else:
        print("❌ لا توجد جداول تحتوي على 'cash' في اسمها")
    
    cursor.close()
    conn.close()
    print("\n✅ انتهى!")
    
except Exception as e:
    print(f"❌ خطأ: {e}")
    import traceback
    traceback.print_exc()
