import psycopg2

conn = psycopg2.connect(host='localhost', port=5432, database='graceway_accounting', user='postgres', password='123456')
cur = conn.cursor()

# محاكاة GetAllCashBoxesAsync مع Include Creator/Updater
cur.execute("""
    SELECT c.cashboxid, c.cashboxname, c.currentbalance, c.createdby, u.userid
    FROM cashboxes c
    LEFT JOIN users u ON u.userid = c.createdby
    WHERE c.isdeleted = false
    ORDER BY c.cashboxname
""")
rows = cur.fetchall()
print(f"Cashboxes with LEFT JOIN: {len(rows)} rows")
for r in rows:
    print(f"  cashboxid={r[0]}, name={r[1]}, balance={r[2]}, createdby={r[3]}, userid_joined={r[4]}")

# فحص كولم CreatorUserId
cur.execute("SELECT cashboxid, createdby, \"CreatorUserId\" FROM cashboxes WHERE isdeleted=false")
rows2 = cur.fetchall()
print("\nCreator fields:")
for r in rows2:
    print(f"  cashboxid={r[0]}, createdby={r[1]}, CreatorUserId={r[2]}")

conn.close()
