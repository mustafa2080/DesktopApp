import sqlite3
import os
import sys

# Force UTF-8 encoding for Windows console
if sys.platform == 'win32':
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

db_path = r"C:\Users\musta\Desktop\pro\accountant\accountant.db"

if not os.path.exists(db_path):
    print(f"ERROR: Database not found at: {db_path}")
    exit(1)

print(f"OK: Database found\n")

conn = sqlite3.connect(db_path)
cursor = conn.cursor()

# Check table structure
print("=" * 60)
print("Flight Bookings Table Structure:")
print("=" * 60)
cursor.execute("PRAGMA table_info(flightbookings)")
columns = cursor.fetchall()
for col in columns:
    print(f"  {col[1]:25} | {col[2]:15}")

# Check if CreatedByUserId column exists
has_user_column = any(col[1].lower() == 'createdbyuserid' for col in columns)
print(f"\nCreatedByUserId column exists: {has_user_column}")

# Check data
print("\n" + "=" * 60)
print("Flight Bookings Data (Last 10):")
print("=" * 60)

cursor.execute("""
    SELECT 
        FlightBookingId,
        BookingNumber,
        ClientName,
        CreatedByUserId,
        CreatedAt
    FROM flightbookings
    ORDER BY CreatedAt DESC
    LIMIT 10
""")

bookings = cursor.fetchall()
print(f"\nTotal bookings found: {len(bookings)}\n")

for booking in bookings:
    booking_id, booking_num, client_name, user_id, created_at = booking
    print(f"ID: {booking_id} | No: {booking_num} | Client: {client_name[:20]:20} | UserID: {user_id}")

# Check if any booking has CreatedByUserId = NULL or 0
print("\n" + "=" * 60)
print("Bookings with missing UserID:")
print("=" * 60)

cursor.execute("""
    SELECT 
        COUNT(*)
    FROM flightbookings
    WHERE CreatedByUserId IS NULL OR CreatedByUserId = 0
""")

missing_count = cursor.fetchone()[0]
print(f"Found {missing_count} bookings with missing UserID")

if missing_count > 0:
    cursor.execute("""
        SELECT 
            FlightBookingId,
            BookingNumber,
            ClientName
        FROM flightbookings
        WHERE CreatedByUserId IS NULL OR CreatedByUserId = 0
        LIMIT 5
    """)
    
    missing = cursor.fetchall()
    print("\nFirst 5 examples:")
    for booking in missing:
        print(f"  ID: {booking[0]} | No: {booking[1]} | Client: {booking[2]}")

# Check users table
print("\n" + "=" * 60)
print("Users Table:")
print("=" * 60)

cursor.execute("SELECT UserId, Username, FullName FROM users")
users = cursor.fetchall()
print(f"Total users: {len(users)}\n")
for user in users:
    print(f"  ID: {user[0]} | Username: {user[1]} | Name: {user[2]}")

conn.close()
print("\nCheck completed!")
