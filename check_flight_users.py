# -*- coding: utf-8 -*-
import psycopg2

# الاتصال بقاعدة البيانات PostgreSQL
# اقرأ الـ connection string من appsettings.json
import json

with open('C:/Users/musta/Desktop/pro/accountant/appsettings.json', 'r') as f:
    config = json.load(f)
    conn_str = config['ConnectionStrings']['DefaultConnection']

# Parse connection string
parts = {}
for part in conn_str.split(';'):
    if '=' in part:
        key, value = part.split('=', 1)
        parts[key.strip()] = value.strip()

conn = psycopg2.connect(
    host=parts.get('Host', 'localhost'),
    database=parts.get('Database', ''),
    user=parts.get('Username', ''),
    password=parts.get('Password', '')
)
cursor = conn.cursor()

print("=" * 80)
print("Flight Bookings Check")
print("=" * 80)

# جلب بيانات حجوزات الطيران
cursor.execute("""
    SELECT 
        fb.flightbookingid,
        fb.bookingnumber,
        fb.clientname,
        fb.createdbyuserid,
        u.username,
        u.fullname
    FROM flightbookings fb
    LEFT JOIN users u ON fb.createdbyuserid = u.userid
    LIMIT 10
""")

results = cursor.fetchall()

if results:
    print("\nFirst 10 bookings:")
    print("-" * 80)
    for row in results:
        booking_id, booking_num, client_name, user_id, username, fullname = row
        print(f"ID: {booking_id} | Booking: {booking_num} | Client: {client_name}")
        print(f"   UserID: {user_id} | Username: {username} | FullName: {fullname}")
        print()
else:
    print("\nNo bookings found")

# Check stats
cursor.execute("SELECT COUNT(*) FROM flightbookings WHERE createdbyuserid IS NULL")
null_count = cursor.fetchone()[0]

cursor.execute("SELECT COUNT(*) FROM flightbookings")
total_count = cursor.fetchone()[0]

print("\nStatistics:")
print(f"   Total bookings: {total_count}")
print(f"   Bookings without user: {null_count}")
print(f"   Bookings with user: {total_count - null_count}")

conn.close()
