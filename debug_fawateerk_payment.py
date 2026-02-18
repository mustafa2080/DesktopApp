import psycopg2
import json

conn = psycopg2.connect(
    host='localhost',
    port=5432,
    database='graceway_accounting',
    user='postgres',
    password='123456'
)
cursor = conn.cursor()

# Get the exact payment that's causing the error (ID from screenshot is visible)
query = """
    SELECT id, amount, transferdate, referencenumber, notes, "TripId",
           sourcebankaccountid, destinationbankaccountid, 
           sourcecashboxid, destinationcashboxid, transfertype
    FROM banktransfers 
    WHERE transfertype = 'FawateerkPayment'
    ORDER BY transferdate DESC
    LIMIT 5
"""

cursor.execute(query)
rows = cursor.fetchall()

print("=" * 100)
print("آخر 5 دفعات فواتيرك في قاعدة البيانات:")
print("=" * 100)

for i, row in enumerate(rows, 1):
    print(f"\nدفعة #{i}:")
    print(f"   ID: {row[0]}")
    print(f"   Amount: {row[1]} (Type: {type(row[1])})")
    print(f"   Date: {row[2]}")
    print(f"   Ref: {row[3]}")
    print(f"   TripId: {row[5]}")
    print(f"   Source Bank: {row[6]}")
    print(f"   Dest Bank: {row[7]}")
    print(f"   Source CashBox: {row[8]}")
    print(f"   Dest CashBox: {row[9]}")
    print(f"   TransferType: {row[10]}")
    
    # Check if Amount is None, NULL, or 0
    if row[1] is None:
        print(f"   WARNING: Amount is NULL!")
    elif row[1] == 0:
        print(f"   WARNING: Amount is ZERO!")
    elif row[1] < 0:
        print(f"   WARNING: Amount is NEGATIVE!")

conn.close()
