import psycopg2

try:
    conn = psycopg2.connect(
        host="localhost",
        database="graceway_accounting",
        user="postgres",
        password="123456"
    )
    
    c = conn.cursor()
    
    # Get column names for cashtransactions table
    c.execute("""
        SELECT column_name, data_type 
        FROM information_schema.columns 
        WHERE table_name = 'cashtransactions'
        ORDER BY ordinal_position
    """)
    
    columns = c.fetchall()
    print("=== CashTransactions Columns ===")
    for col in columns:
        print(f"  {col[0]} ({col[1]})")
    
    conn.close()
    
except Exception as e:
    print(f"Error: {e}")
    import traceback
    traceback.print_exc()
