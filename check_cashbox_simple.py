import psycopg2
import sys

# Fix encoding
if sys.platform == 'win32':
    import codecs
    sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')

# Connect to PostgreSQL
conn = psycopg2.connect(
    host="localhost",
    port=5432,
    database="graceway_accounting",
    user="postgres",
    password="123456"
)

cursor = conn.cursor()

print("="*60)
print("CASHBOX DATA CHECK - PostgreSQL")
print("="*60)

# Check February 2026 transactions
print("\nFEBRUARY 2026 TRANSACTIONS:")
print("-"*60)

cursor.execute("""
    SELECT 
        "Id",
        "TransactionDate",
        "Type",
        "Amount",
        "Description",
        "Month",
        "Year",
        "CashBoxId",
        "TransactionCurrency"
    FROM "CashTransactions" 
    WHERE "IsDeleted" = false 
        AND "Month" = 2
        AND "Year" = 2026
    ORDER BY "TransactionDate" DESC
    LIMIT 20
""")

feb_trans = cursor.fetchall()

if feb_trans:
    print(f"\nID       Date         Type      Amount       Currency  CashBoxId")
    print("-"*75)
    for t in feb_trans:
        trans_type = "Income" if t[2] == 0 else "Expense"
        currency = t[8] if t[8] else "EGP"
        print(f"{t[0]:<8} {str(t[1]):<12} {trans_type:<9} {t[3]:<12.2f} {currency:<9} {t[7]:<8}")
    print(f"\nTOTAL: {len(feb_trans)} transactions in February 2026")
else:
    print("\nNO TRANSACTIONS FOUND in February 2026!")

# Check all transactions count by month
print("\n" + "="*60)
print("TRANSACTIONS COUNT BY MONTH:")
print("-"*60)

cursor.execute("""
    SELECT 
        TO_CHAR("TransactionDate", 'YYYY-MM') as month,
        COUNT(*) as count,
        SUM(CASE WHEN "Type" = 0 THEN "Amount" ELSE 0 END) as income,
        SUM(CASE WHEN "Type" = 1 THEN "Amount" ELSE 0 END) as expense
    FROM "CashTransactions" 
    WHERE "IsDeleted" = false 
    GROUP BY month 
    ORDER BY month DESC 
    LIMIT 12
""")

results = cursor.fetchall()

if results:
    print(f"\nMonth        Count      Income        Expense")
    print("-"*60)
    for row in results:
        print(f"{row[0]:<12} {row[1]:<10} {row[2] or 0:<13.2f} {row[3] or 0:<13.2f}")
else:
    print("\nNO TRANSACTIONS FOUND!")

# Check CashBoxes
print("\n" + "="*60)
print("CASHBOXES:")
print("-"*60)

cursor.execute("""
    SELECT "Id", "Name", "CurrentBalance"
    FROM "CashBoxes"
    WHERE "IsDeleted" = false
    ORDER BY "Id"
""")

cashboxes = cursor.fetchall()

if cashboxes:
    print(f"\nID       Name                           Balance")
    print("-"*60)
    for cb in cashboxes:
        print(f"{cb[0]:<8} {cb[1]:<30} {cb[2] or 0:<13.2f}")
else:
    print("\nNO CASHBOXES FOUND!")

# Check latest 10 transactions
print("\n" + "="*60)
print("LATEST 10 TRANSACTIONS (any month):")
print("-"*60)

cursor.execute("""
    SELECT 
        "Id",
        "TransactionDate",
        "Type",
        "Amount",
        "Month",
        "Year",
        "CashBoxId"
    FROM "CashTransactions" 
    WHERE "IsDeleted" = false 
    ORDER BY "TransactionDate" DESC, "Id" DESC
    LIMIT 10
""")

latest = cursor.fetchall()

if latest:
    print(f"\nID       Date         Type      Amount       Month  Year   CashBoxId")
    print("-"*75)
    for t in latest:
        trans_type = "Income" if t[2] == 0 else "Expense"
        print(f"{t[0]:<8} {str(t[1]):<12} {trans_type:<9} {t[3]:<12.2f} {t[4]:<6} {t[5]:<6} {t[6]:<8}")
else:
    print("\nNO TRANSACTIONS FOUND!")

conn.close()

print("\n" + "="*60)
print("DONE!")
print("="*60)
