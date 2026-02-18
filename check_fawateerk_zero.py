import psycopg2

conn = psycopg2.connect(
    host='localhost',
    port=5432,
    database='graceway_accounting',
    user='postgres',
    password='123456'
)
cursor = conn.cursor()

# Check for zero or negative amounts
query = """
    SELECT id, amount, transferdate, referencenumber 
    FROM banktransfers 
    WHERE transfertype = 'FawateerkPayment' AND amount <= 0
    ORDER BY id
    LIMIT 10
"""

cursor.execute(query)
rows = cursor.fetchall()

print(f"عدد دفعات فواتيرك بمبلغ 0 أو أقل: {len(rows)}")
print("=" * 80)

if rows:
    print("\nتفاصيل الدفعات:")
    for row in rows:
        print(f"ID: {row[0]}, Amount: {row[1]}, Date: {row[2]}, Ref: {row[3]}")
else:
    print("لا توجد دفعات بمبلغ 0 أو أقل")

# Also check total count
cursor.execute("SELECT COUNT(*) FROM banktransfers WHERE transfertype = 'FawateerkPayment'")
total_count = cursor.fetchone()[0]
print(f"\nإجمالي دفعات فواتيرك: {total_count}")

conn.close()
