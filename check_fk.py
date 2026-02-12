import psycopg2
import sys

sys.stdout.reconfigure(encoding='utf-8')

try:
    conn = psycopg2.connect(
        host="localhost",
        port=5432,
        database="graceway_accounting",
        user="postgres",
        password="123456"
    )
    
    cursor = conn.cursor()
    
    # فحص Foreign Key Constraint
    print("=" * 60)
    print("Foreign Key Constraints على جدول trips:")
    print("=" * 60)
    cursor.execute("""
        SELECT
            tc.constraint_name, 
            tc.table_name, 
            kcu.column_name, 
            ccu.table_name AS foreign_table_name,
            ccu.column_name AS foreign_column_name 
        FROM 
            information_schema.table_constraints AS tc 
            JOIN information_schema.key_column_usage AS kcu
              ON tc.constraint_name = kcu.constraint_name
              AND tc.table_schema = kcu.table_schema
            JOIN information_schema.constraint_column_usage AS ccu
              ON ccu.constraint_name = tc.constraint_name
              AND ccu.table_schema = tc.table_schema
        WHERE tc.constraint_type = 'FOREIGN KEY' 
        AND tc.table_name='trips'
        AND kcu.column_name = 'currencyid';
    """)
    
    fk = cursor.fetchall()
    if fk:
        for f in fk:
            print(f"\nConstraint: {f[0]}")
            print(f"  Table: {f[1]}.{f[2]}")
            print(f"  References: {f[3]}.{f[4]}")
    else:
        print("لا يوجد Foreign Key Constraint على currencyid!")
        
    # فحص القيم الحالية
    print("\n" + "=" * 60)
    print("القيم الحالية في trips:")
    print("=" * 60)
    cursor.execute("""
        SELECT COUNT(*) as total,
               COUNT(DISTINCT currencyid) as unique_currencies,
               MIN(currencyid) as min_id,
               MAX(currencyid) as max_id
        FROM trips;
    """)
    stats = cursor.fetchone()
    print(f"إجمالي الرحلات: {stats[0]}")
    print(f"عدد العملات المستخدمة: {stats[1]}")
    print(f"أصغر CurrencyId: {stats[2]}")
    print(f"أكبر CurrencyId: {stats[3]}")
    
    # فحص العملات الموجودة
    print("\n" + "=" * 60)
    print("العملات المتاحة:")
    print("=" * 60)
    cursor.execute("SELECT currencyid, currencycode, currencyname FROM currencies ORDER BY currencyid;")
    currencies = cursor.fetchall()
    for curr in currencies:
        print(f"  ID: {curr[0]} | Code: {curr[1]} | Name: {curr[2]}")
    
    cursor.close()
    conn.close()
    
except Exception as e:
    print(f"Error: {e}")
    import traceback
    traceback.print_exc()
