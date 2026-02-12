import psycopg2

conn = psycopg2.connect(
    host="localhost",
    port=5432,
    database="graceway_accounting",
    user="postgres",
    password="123456"
)

cursor = conn.cursor()

print("="*80)
print("TESTING TRANSACTIONTYPE VALUES")
print("="*80)

# Check the values in database
cursor.execute("""
    SELECT 
        transactionid,
        transactiontype,
        amount,
        category,
        CASE 
            WHEN transactiontype = 1 THEN 'INCOME (1)'
            WHEN transactiontype = 2 THEN 'EXPENSE (2)'
            ELSE 'UNKNOWN'
        END as type_name
    FROM cashtransactions
    WHERE isdeleted = false
    ORDER BY transactionid
""")

rows = cursor.fetchall()

print(f"\nFound {len(rows)} transactions:\n")
print(f"{'ID':<8} {'Type':<15} {'Amount':<12} {'Category':<20} {'Type Name':<15}")
print("-"*80)

for row in rows:
    print(f"{row[0]:<8} {row[1]:<15} {row[2]:<12} {row[3]:<20} {row[4]:<15}")

# Test filtering
print("\n" + "="*80)
print("TESTING FILTERS")
print("="*80)

cursor.execute("""
    SELECT COUNT(*), SUM(amount)
    FROM cashtransactions
    WHERE transactiontype = 1 AND isdeleted = false
""")

income_result = cursor.fetchone()
print(f"\nIncome (Type=1): Count={income_result[0]}, Total={income_result[1]}")

cursor.execute("""
    SELECT COUNT(*), SUM(amount)
    FROM cashtransactions
    WHERE transactiontype = 2 AND isdeleted = false
""")

expense_result = cursor.fetchone()
print(f"Expense (Type=2): Count={expense_result[0]}, Total={expense_result[1]}")

# Test month/year filter
print("\n" + "="*80)
print("TESTING MONTH/YEAR FILTER (February 2026)")
print("="*80)

cursor.execute("""
    SELECT 
        transactiontype,
        COUNT(*) as count,
        SUM(amount) as total
    FROM cashtransactions
    WHERE month = 2 
        AND year = 2026
        AND isdeleted = false
    GROUP BY transactiontype
""")

month_results = cursor.fetchall()

for row in month_results:
    type_name = "INCOME" if row[0] == 1 else "EXPENSE"
    print(f"{type_name}: Count={row[1]}, Total={row[2]}")

conn.close()

print("\n" + "="*80)
print("DONE!")
print("="*80)
