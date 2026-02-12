# -*- coding: utf-8 -*-
import sqlite3
import sys
import io
from datetime import datetime

if sys.platform == 'win32':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

db_path = r"C:\Users\musta\Desktop\pro\accountant\accountant.db"

print("Creating a test Umrah package...")
print("=" * 60)

conn = sqlite3.connect(db_path)
cursor = conn.cursor()

# إنشاء حزمة تجريبية
cursor.execute("""
    INSERT INTO umrahpackages (
        packagenumber, date, tripname, numberofpersons, roomtype,
        makkahhotel, makkahnights, madinahhotel, madinahnights,
        transportmethod, sellingprice, visapricesar, sarexchangerate,
        accommodationtotal, barcodeprice, flightprice, fasttrainpricesar,
        brokername, supervisorname, commission, supervisorexpenses,
        status, isactive, notes, createdby, createdat, updatedat
    ) VALUES (
        'UMR-2025-0001',
        ?,
        'رحلة العمرة التجريبية',
        10,
        2,
        'فندق دار التوحيد',
        7,
        'فندق الطيبات',
        5,
        'طيران',
        25000.00,
        1500.00,
        13.50,
        150000.00,
        500.00,
        3000.00,
        500.00,
        'محمد أحمد',
        'أحمد محمود',
        2000.00,
        3000.00,
        1,
        1,
        'حزمة تجريبية للاختبار',
        2,
        ?,
        ?
    )
""", (datetime.utcnow().isoformat(), datetime.utcnow().isoformat(), datetime.utcnow().isoformat()))

conn.commit()
package_id = cursor.lastrowid

print(f"✅ Created test package with ID: {package_id}")

# التحقق من البيانات
cursor.execute("""
    SELECT 
        p.UmrahPackageId,
        p.PackageNumber,
        p.TripName,
        p.CreatedBy,
        u.Username,
        u.FullName
    FROM umrahpackages p
    LEFT JOIN users u ON p.CreatedBy = u.UserId
    WHERE p.UmrahPackageId = ?
""", (package_id,))

result = cursor.fetchone()
if result:
    print("\n📦 Package created successfully:")
    print(f"  ID: {result[0]}")
    print(f"  Number: {result[1]}")
    print(f"  Trip: {result[2]}")
    print(f"  CreatedBy: {result[3]}")
    print(f"  Username: {result[4] or 'NULL'}")
    print(f"  FullName: {result[5] or 'NULL'}")

conn.close()
print("\n" + "=" * 60)
print("Test completed!")
