import sqlite3

conn = sqlite3.connect(r'C:\Users\musta\Desktop\pro\accountant\accountant.db')
cursor = conn.cursor()

# Check UmrahPackages table
cursor.execute("PRAGMA table_info(UmrahPackages)")
umrah_cols = cursor.fetchall()
print("UmrahPackages columns:")
for col in umrah_cols:
    print(f"  {col[1]} - {col[2]}")

print("\n" + "="*50 + "\n")

# Check FlightBookings table  
cursor.execute("PRAGMA table_info(flightbookings)")
flight_cols = cursor.fetchall()
print("FlightBookings columns:")
for col in flight_cols:
    print(f"  {col[1]} - {col[2]}")

print("\n" + "="*50 + "\n")

# Check foreign keys
cursor.execute("PRAGMA foreign_key_list(UmrahPackages)")
umrah_fks = cursor.fetchall()
print("UmrahPackages foreign keys:")
for fk in umrah_fks:
    print(f"  Column: {fk[3]}, References: {fk[2]}.{fk[4]}")

print("\n" + "="*50 + "\n")

cursor.execute("PRAGMA foreign_key_list(flightbookings)")
flight_fks = cursor.fetchall()
print("FlightBookings foreign keys:")
for fk in flight_fks:
    print(f"  Column: {fk[3]}, References: {fk[2]}.{fk[4]}")

conn.close()
print("\nDone!")
