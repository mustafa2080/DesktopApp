import psycopg2

conn = psycopg2.connect('host=localhost dbname=graceway_accounting user=postgres password=123456')
cur = conn.cursor()

# Get all tables
print("=" * 60)
print("Tables in database")
print("=" * 60)
cur.execute("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' ORDER BY table_name")
tables = cur.fetchall()
for table in tables:
    print(f"  - {table[0]}")

cur.close()
conn.close()
