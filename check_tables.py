import sqlite3

conn = sqlite3.connect('accountant.db')
cursor = conn.cursor()

# Get all table names
cursor.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name")
tables = cursor.fetchall()

print("=" * 60)
print("All Tables in Database:")
print("=" * 60)
for table in tables:
    print(f"  - {table[0]}")

print("\n" + "=" * 60)
print("Searching for RoomType-related tables:")
print("=" * 60)

# Search for tables containing "room" or "umrah"
for table in tables:
    table_name = table[0]
    if 'room' in table_name.lower() or 'umrah' in table_name.lower():
        print(f"\n📋 Table: {table_name}")
        
        # Get columns
        cursor.execute(f"PRAGMA table_info({table_name})")
        columns = cursor.fetchall()
        
        print("   Columns:")
        for col in columns:
            col_id, name, col_type, not_null, default, pk = col
            print(f"     - {name} ({col_type})")

conn.close()
