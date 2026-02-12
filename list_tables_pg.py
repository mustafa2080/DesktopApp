import psycopg2

# Connect
conn = psycopg2.connect(
    host="localhost",
    port=5432,
    database="graceway_accounting",
    user="postgres",
    password="123456"
)

cursor = conn.cursor()

print("CHECKING TABLES IN DATABASE...")
print("="*60)

# List all tables
cursor.execute("""
    SELECT table_name 
    FROM information_schema.tables 
    WHERE table_schema = 'public'
    ORDER BY table_name
""")

tables = cursor.fetchall()

if tables:
    print("\nTABLES FOUND:")
    for table in tables:
        print(f"  - {table[0]}")
        
        # Count rows in each table
        try:
            cursor.execute(f'SELECT COUNT(*) FROM "{table[0]}"')
            count = cursor.fetchone()[0]
            print(f"    Rows: {count}")
        except Exception as e:
            print(f"    Error: {e}")
else:
    print("\nNO TABLES FOUND!")

conn.close()
