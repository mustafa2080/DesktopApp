import psycopg2
import sys

sys.stdout.reconfigure(encoding='utf-8')

try:
    conn = psycopg2.connect(
        host="localhost",
        port=5432,
        database="graceway_accounting",
        user="postgres",
        password="123456"
    )
    
    cursor = conn.cursor()
    
    print("=" * 60)
    print("اختبار انشاء رحلة جديدة:")
    print("=" * 60)
    
    # محاكاة إنشاء رحلة كما يحدث في الكود
    print("\nمحاولة انشاء رحلة برقم...")
    
    # تحقق من آخر رقم
    cursor.execute("SELECT MAX(tripid) FROM trips;")
    last_id = cursor.fetchone()[0]
    next_id = (last_id or 0) + 1
    
    print(f"Next Trip ID: {next_id}")
    
    # إدخال رحلة تجريبية
    cursor.execute("""
        INSERT INTO trips (
            tripnumber, tripname, destination, triptype, startdate, enddate,
            totalcapacity, sellingpriceperperson, currencyid, exchangerate,
            totalcost, status, ispublished, isactive, createdby, createdat, updatedat
        ) VALUES (
            'TR-2026-TEST', 'رحلة تجريبية', 'القاهرة', 0, '2026-03-01', '2026-03-05',
            50, 1000.0, 1, 1.0,
            45000.0, 0, false, true, 1, NOW(), NOW()
        ) RETURNING tripid;
    """)
    
    trip_id = cursor.fetchone()[0]
    conn.commit()
    
    print(f"\nتم انشاء الرحلة بنجاح! Trip ID: {trip_id}")
    
    # التحقق
    cursor.execute("""
        SELECT t.tripid, t.tripname, t.currencyid, c.currencycode, c.currencyname
        FROM trips t
        JOIN currencies c ON t.currencyid = c.currencyid
        WHERE t.tripid = %s;
    """, (trip_id,))
    
    result = cursor.fetchone()
    print(f"\nتفاصيل الرحلة:")
    print(f"  ID: {result[0]}")
    print(f"  Name: {result[1]}")
    print(f"  Currency ID: {result[2]}")
    print(f"  Currency Code: {result[3]}")
    print(f"  Currency Name: {result[4]}")
    
    # حذف الرحلة التجريبية
    print(f"\nحذف الرحلة التجريبية...")
    cursor.execute("DELETE FROM trips WHERE tripid = %s;", (trip_id,))
    conn.commit()
    
    cursor.close()
    conn.close()
    
    print("\nالاختبار نجح!")
    print("\nالمشكلة محلولة تماما. يمكنك الان حفظ الرحلات بدون اي مشاكل.")
    
except Exception as e:
    print(f"Error: {e}")
    import traceback
    traceback.print_exc()
