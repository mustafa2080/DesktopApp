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
    
    print("=== فحص البيانات في cashtransactions ===\n")
    
    # Get all columns
    cursor.execute("""
        SELECT column_name
        FROM information_schema.columns
        WHERE table_name = 'cashtransactions'
        ORDER BY ordinal_position
    """)
    
    columns = [row[0] for row in cursor.fetchall()]
    print(f"الأعمدة ({len(columns)}):")
    for col in columns:
        print(f"  - {col}")
    
    # Get the data
    print("\n=== البيانات ===")
    cursor.execute('SELECT * FROM cashtransactions LIMIT 5')
    
    rows = cursor.fetchall()
    print(f"\nعدد الصفوف: {len(rows)}\n")
    
    if rows:
        for i, row in enumerate(rows, 1):
            print(f"--- صف {i} ---")
            for j, col_name in enumerate(columns):
                value = row[j]
                print(f"  {col_name}: {value}")
            print()
    
    # Summary by month
    print("=== ملخص حسب الشهر والسنة ===")
    cursor.execute("""
        SELECT month, year, COUNT(*) as count,
               SUM(CASE WHEN transactiontype = 1 THEN amount ELSE 0 END) as income,
               SUM(CASE WHEN transactiontype = 2 THEN amount ELSE 0 END) as expense,
               STRING_AGG(DISTINCT "TransactionCurrency", ', ') as currencies
        FROM cashtransactions
        WHERE isdeleted = false
        GROUP BY month, year
        ORDER BY year DESC, month DESC
    """)
    
    print("Month | Year | Count | Income | Expense | Currencies")
    print("-" * 90)
    for row in cursor.fetchall():
        print(f"{row[0]:2d} | {row[1]} | {row[2]:5d} | {row[3]:10.2f} | {row[4]:10.2f} | {row[5]}")
    
    cursor.close()
    conn.close()
    print("\n✅ انتهى!")
    
except Exception as e:
    print(f"❌ خطأ: {e}")
    import traceback
    traceback.print_exc()
