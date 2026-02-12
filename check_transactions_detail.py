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
print("DETAILED CHECK OF CASHTRANSACTIONS TABLE")
print("="*80)

# Get column names
cursor.execute("""
    SELECT column_name, data_type 
    FROM information_schema.columns 
    WHERE table_name = 'cashtransactions'
    ORDER BY ordinal_position
""")

columns = cursor.fetchall()

print("\nCOLUMNS:")
for col in columns:
    print(f"  - {col[0]:<30} ({col[1]})")

# Get all rows
cursor.execute('SELECT * FROM cashtransactions')
rows = cursor.fetchall()

print(f"\n\nTOTAL ROWS: {len(rows)}")
print("="*80)

if rows:
    # Get column names for display
    cursor.execute("""
        SELECT column_name 
        FROM information_schema.columns 
        WHERE table_name = 'cashtransactions'
        ORDER BY ordinal_position
    """)
    col_names = [col[0] for col in cursor.fetchall()]
    
    print("\nALL DATA:")
    print("-"*80)
    
    for i, row in enumerate(rows, 1):
        print(f"\n=== ROW {i} ===")
        for col_name, value in zip(col_names, row):
            print(f"  {col_name:<25} = {value}")
else:
    print("\nNO ROWS FOUND!")

conn.close()

print("\n" + "="*80)
print("DONE!")
print("="*80)
