import psycopg2
import datetime

conn = psycopg2.connect(host='localhost', port=5432, database='graceway_accounting', user='postgres', password='123456')
cur = conn.cursor()

# اسماء الاعمدة
cur.execute("SELECT column_name FROM information_schema.columns WHERE table_name='CashTransactions' ORDER BY ordinal_position")
cols = [r[0] for r in cur.fetchall()]
print("Columns:", cols)

# عدد المعاملات
cur.execute('SELECT COUNT(*) FROM "CashTransactions"')
print("Total transactions:", cur.fetchone()[0])

# عينة
cur.execute('SELECT "Id", "Type", "Amount", "Month", "Year", "TransactionCurrency", "IsDeleted" FROM "CashTransactions" LIMIT 5')
rows = cur.fetchall()
print("\nSample data:")
for r in rows:
    print(f"  Id={r[0]}, Type={r[1]}, Amount={r[2]}, Month={r[3]}, Year={r[4]}, Currency={r[5]}, Deleted={r[6]}")

# قيم Type الموجودة
cur.execute('SELECT DISTINCT "Type" FROM "CashTransactions"')
print("\nDistinct Type values:", [r[0] for r in cur.fetchall()])

# اجمالي حسب الشهر الحالي
now = datetime.datetime.now()
cur.execute(f'SELECT "Type", SUM("Amount"), COUNT(*) FROM "CashTransactions" WHERE "Month"={now.month} AND "Year"={now.year} AND "IsDeleted"=false GROUP BY "Type"')
print(f"\nMonth {now.month}/{now.year}:")
for r in cur.fetchall():
    print(f"  Type={r[0]}, Total={r[1]}, Count={r[2]}")

# فحص التواريخ - هل TransactionDate بـ UTC؟
cur.execute('SELECT "Id", "TransactionDate", "Month", "Year" FROM "CashTransactions" WHERE "IsDeleted"=false LIMIT 5')
print("\nDates sample:")
for r in cur.fetchall():
    print(f"  Id={r[0]}, Date={r[1]}, Month={r[2]}, Year={r[3]}")

# فحص كل الشهور الموجودة
cur.execute('SELECT "Month", "Year", COUNT(*) FROM "CashTransactions" WHERE "IsDeleted"=false GROUP BY "Month", "Year" ORDER BY "Year", "Month"')
print("\nAll months with data:")
for r in cur.fetchall():
    print(f"  {r[1]}/{r[0]}: {r[2]} transactions")

conn.close()
print("\nDone!")
