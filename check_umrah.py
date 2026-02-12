import psycopg2

try:
    conn = psycopg2.connect(
        host="localhost",
        port=5432,
        database="graceway_accounting",
        user="postgres",
        password="123456"
    )
    cursor = conn.cursor()
    
    # Check umrahpackages table
    cursor.execute("""
        SELECT column_name, data_type 
        FROM information_schema.columns 
        WHERE table_name = 'umrahpackages'
        ORDER BY ordinal_position
    """)
    umrah_cols = cursor.fetchall()
    print("umrahpackages columns:")
    for col in umrah_cols:
        print(f"  {col[0]} - {col[1]}")
    
    print("\n" + "="*50 + "\n")
    
    # Check foreign keys for umrahpackages
    cursor.execute("""
        SELECT
            kcu.column_name,
            ccu.table_name AS foreign_table_name,
            ccu.column_name AS foreign_column_name
        FROM information_schema.table_constraints AS tc
        JOIN information_schema.key_column_usage AS kcu
          ON tc.constraint_name = kcu.constraint_name
        JOIN information_schema.constraint_column_usage AS ccu
          ON ccu.constraint_name = tc.constraint_name
        WHERE tc.constraint_type = 'FOREIGN KEY' 
          AND tc.table_name = 'umrahpackages'
    """)
    umrah_fks = cursor.fetchall()
    print("umrahpackages foreign keys:")
    for fk in umrah_fks:
        print(f"  {fk[0]} -> {fk[1]}.{fk[2]}")
    
    cursor.close()
    conn.close()
    print("\nDone!")
    
except Exception as e:
    print(f"Error: {e}")
