import psycopg2
conn = psycopg2.connect(host='localhost', port=5432, dbname='graceway_accounting', user='postgres', password='123456')
cur = conn.cursor()
cur.execute("SELECT column_name FROM information_schema.columns WHERE table_name='cashboxes' ORDER BY ordinal_position")
print('cashboxes cols:', [c[0] for c in cur.fetchall()])
cur.execute("SELECT column_name FROM information_schema.columns WHERE table_name='cashtransactions' ORDER BY ordinal_position")
print('cashtransactions cols:', [c[0] for c in cur.fetchall()])
conn.close()
