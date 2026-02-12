# -*- coding: utf-8 -*-
import psycopg2
import sys
from decimal import Decimal

if sys.platform == 'win32':
    import codecs
    sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')

# سعر الصرف الحالي للدولار
USD_EXCHANGE_RATE = Decimal('51.0')  # يمكن تعديله حسب السعر الحالي

print("="*120)
print("FIX EXCHANGE RATES FOR USD TRIPS")
print("="*120)

conn = psycopg2.connect(
    host='localhost',
    port=5432,
    dbname='graceway_accounting',
    user='postgres',
    password='123456'
)

cur = conn.cursor()

# جلب الرحلات بالدولار مع سعر صرف خاطئ
cur.execute("""
    SELECT 
        tripid, 
        tripnumber, 
        tripname, 
        sellingpriceperperson, 
        currencyid, 
        exchangerate
    FROM trips 
    WHERE currencyid = 2 AND exchangerate = 1.0
    ORDER BY tripid
""")

trips_to_fix = cur.fetchall()

print(f"\nFound {len(trips_to_fix)} trips with incorrect USD exchange rate (1.0)\n")

if len(trips_to_fix) == 0:
    print("No trips to fix!")
    conn.close()
    sys.exit(0)

print("Trips to be updated:")
print("-"*120)
for trip in trips_to_fix:
    trip_id, trip_num, trip_name, price, curr_id, exrate = trip
    new_price_egp = price * USD_EXCHANGE_RATE
    print(f"ID: {trip_id} | {trip_num} | {trip_name}")
    print(f"  Current: ${float(price):.2f} USD x {float(exrate)} = {float(price * exrate):.2f} EGP (WRONG!)")
    print(f"  Will be: ${float(price):.2f} USD x {float(USD_EXCHANGE_RATE)} = {float(new_price_egp):.2f} EGP (CORRECT)")
    print("-"*120)

print(f"\nAUTO-CONFIRMING UPDATE for {len(trips_to_fix)} trips...")
print(f"New exchange rate will be: {float(USD_EXCHANGE_RATE)} EGP per USD")

# تحديث سعر الصرف تلقائياً
cur.execute("""
    UPDATE trips 
    SET exchangerate = %s
    WHERE currencyid = 2 AND exchangerate = 1.0
""", (USD_EXCHANGE_RATE,))

conn.commit()

print(f"\nSUCCESS! Updated {cur.rowcount} trips.")
print(f"New exchange rate: {float(USD_EXCHANGE_RATE)} EGP per USD")

# التحقق من التحديث
cur.execute("""
    SELECT 
        tripid, 
        tripnumber, 
        sellingpriceperperson, 
        exchangerate,
        sellingpriceperperson * exchangerate as price_in_egp
    FROM trips 
    WHERE currencyid = 2
    ORDER BY tripid
""")

updated_trips = cur.fetchall()

print("\nUpdated trips verification:")
print("-"*120)
for trip in updated_trips:
    trip_id, trip_num, price, exrate, price_egp = trip
    print(f"{trip_num}: ${float(price):.2f} x {float(exrate)} = {float(price_egp):.2f} EGP")

conn.close()
print("\nDone!")
