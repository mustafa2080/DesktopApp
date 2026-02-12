import psycopg2

try:
    conn = psycopg2.connect(
        host='localhost',
        port=5432,
        database='graceway_accounting',
        user='postgres',
        password='123456'
    )
    cursor = conn.cursor()
    
    # List all tables
    print("=== All Tables ===")
    cursor.execute('''
        SELECT table_name 
        FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_type = 'BASE TABLE'
        ORDER BY table_name
    ''')
    tables = cursor.fetchall()
    for table in tables:
        print(f"- {table[0]}")
    
    conn.close()
    print("\nDone!")

except Exception as e:
    print(f"Error: {e}")
