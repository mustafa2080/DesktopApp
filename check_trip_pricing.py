# -*- coding: utf-8 -*-
import psycopg2
import sys

# تأكد من encoding صحيح
if sys.platform == 'win32':
    import codecs
    sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')

# الاتصال بقاعدة البيانات
conn = psycopg2.connect(
    host='localhost',
    port=5432,
    dbname='graceway_accounting',
    user='postgres',
    password='123456'
)

cur = conn.cursor()

# جلب بيانات الرحلات
cur.execute("""
    SELECT 
        tripid, 
        tripnumber, 
        tripname, 
        sellingpriceperperson, 
        currencyid, 
        exchangerate,
        totalcapacity
    FROM trips 
    ORDER BY tripid DESC
    LIMIT 10
""")

rows = cur.fetchall()

print("="*120)
print("Trip Pricing Check - Currency Analysis")
print("="*120)
print(f"{'ID':<5} | {'Trip#':<15} | {'Name':<30} | {'Price':<12} | {'Curr':<8} | {'ExRate':<12} | {'Cap':<8}")
print("="*120)

issues_found = []

for row in rows:
    trip_id, trip_num, trip_name, selling_price, currency_id, exchange_rate, capacity = row
    
    # حساب السعر بالجنيه
    price_in_egp = selling_price * exchange_rate
    
    currency_name = {1: 'EGP', 2: 'USD', 3: 'SAR', 4: 'EUR'}.get(currency_id, 'Unknown')
    
    print(f"{trip_id:<5} | {trip_num:<15} | {trip_name[:30]:<30} | {selling_price:>10.2f} | {currency_name:<8} | {exchange_rate:>10.2f} | {capacity:<8}")
    print(f"      => Price in EGP: {price_in_egp:.2f} EGP")
    
    # تحذير إذا كان السعر والمعادل متطابقين (مشكلة محتملة)
    if currency_id != 1 and exchange_rate == 1.0:
        issue = f"ISSUE: Trip #{trip_num} - Exchange rate is 1.0 for {currency_name}!"
        print(f"      WARNING: {issue}")
        issues_found.append(issue)
    
    print("-"*120)

print("\n" + "="*120)
print("SUMMARY:")
print("="*120)
print(f"Total trips checked: {len(rows)}")
print(f"Issues found: {len(issues_found)}")

if issues_found:
    print("\nISSUES DETECTED:")
    for i, issue in enumerate(issues_found, 1):
        print(f"{i}. {issue}")
else:
    print("\nNo issues found - all exchange rates look correct!")

conn.close()

print("\nDone!")
