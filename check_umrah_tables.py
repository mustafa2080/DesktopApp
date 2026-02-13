import sqlite3

conn = sqlite3.connect('accountant.db')
cursor = conn.cursor()

# Get all table names
cursor.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name")
tables = cursor.fetchall()

print("=" * 60)
print("Umrah-related tables:")
print("=" * 60)

# Search for umrah tables
for table in tables:
    table_name = table[0]
    if 'umrah' in table_name.lower():
        print(f"\nTable: {table_name}")
        
        # Get columns
        cursor.execute(f"PRAGMA table_info({table_name})")
        columns = cursor.fetchall()
        
        print("Columns:")
        for col in columns:
            col_id, name, col_type, not_null, default, pk = col
            nullable = "NULL" if not not_null else "NOT NULL"
            print(f"  - {name}: {col_type} {nullable}")

# Check specifically for umrahpilgrims and RoomType column
print("\n" + "=" * 60)
print("Checking umrahpilgrims for RoomType column:")
print("=" * 60)

cursor.execute("PRAGMA table_info(umrahpilgrims)")
columns = cursor.fetchall()
column_names = [col[1] for col in columns]

if 'RoomType' in column_names:
    print("OK: RoomType column EXISTS")
elif 'roomtype' in column_names:
    print("WARNING: Found 'roomtype' (lowercase)")
else:
    print("ERROR: RoomType column NOT FOUND")
    print(f"Available columns: {column_names}")

conn.close()
