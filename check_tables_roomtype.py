import sqlite3

conn = sqlite3.connect('accountant.db')
cursor = conn.cursor()

# Get all table names
cursor.execute("SELECT name FROM sqlite_master WHERE type='table'")
tables = cursor.fetchall()

print("جداول قاعدة البيانات:")
print("=" * 50)
for table in tables:
    print(f"  - {table[0]}")
    
# الآن ابحث عن الجداول التي تحتوي على RoomType
print("\n" + "=" * 50)
print("الجداول التي تحتوي على عمود room:")
print("=" * 50)

for table in tables:
    table_name = table[0]
    try:
        cursor.execute(f"PRAGMA table_info({table_name})")
        columns = cursor.fetchall()
        room_columns = [col for col in columns if 'room' in col[1].lower()]
        if room_columns:
            print(f"\n[TABLE] {table_name}:")
            for col in room_columns:
                print(f"   - {col[1]} ({col[2]})")
    except Exception as e:
        print(f"   [ERROR] في {table_name}: {e}")

conn.close()
