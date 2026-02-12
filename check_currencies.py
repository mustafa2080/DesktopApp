import psycopg2
import sys

# تعيين الترميز
sys.stdout.reconfigure(encoding='utf-8')

try:
    # الاتصال بقاعدة البيانات
    conn = psycopg2.connect(
        host="localhost",
        port=5432,
        database="graceway_accounting",
        user="postgres",
        password="123456"
    )
    
    cursor = conn.cursor()
    
    # فحص بنية جدول العملات
    print("=" * 60)
    print("بنية جدول Currencies:")
    print("=" * 60)
    cursor.execute("""
        SELECT column_name, data_type 
        FROM information_schema.columns 
        WHERE table_name = 'currencies'
        ORDER BY ordinal_position;
    """)
    columns = cursor.fetchall()
    for col in columns:
        print(f"  {col[0]} ({col[1]})")
    
    # فحص جدول العملات
    print("\n" + "=" * 60)
    print("محتوى جدول العملات:")
    print("=" * 60)
    cursor.execute("SELECT * FROM currencies ORDER BY currencyid;")
    currencies = cursor.fetchall()
    
    if currencies:
        print(f"\nعدد العملات: {len(currencies)}\n")
        for curr in currencies:
            print(f"Currency: {curr}")
    else:
        print("لا توجد عملات في القاعدة!")
        print("\nسأقوم بإضافة العملات الأساسية...")
        
        # إضافة العملات الأساسية
        cursor.execute("""
            INSERT INTO currencies (currencyid, code, name, symbol, exchangerate, isbasecurrency, isactive, createdat)
            VALUES 
                (1, 'EGP', 'Egyptian Pound', 'جنيه', 1.0, true, true, NOW()),
                (2, 'USD', 'US Dollar', '$', 50.0, false, true, NOW()),
                (3, 'SAR', 'Saudi Riyal', 'ريال', 13.33, false, true, NOW()),
                (4, 'EUR', 'Euro', '€', 55.0, false, true, NOW())
            ON CONFLICT (currencyid) DO NOTHING;
        """)
        conn.commit()
        print("تم إضافة العملات بنجاح!")
    
    print("\n" + "=" * 60)
    print("فحص جدول الرحلات:")
    print("=" * 60)
    cursor.execute("""
        SELECT tripid, tripname, currencyid, exchangerate 
        FROM trips 
        WHERE currencyid IS NULL OR currencyid NOT IN (SELECT currencyid FROM currencies)
        LIMIT 10;
    """)
    invalid_trips = cursor.fetchall()
    
    if invalid_trips:
        print(f"\nرحلات بعملات غير صحيحة: {len(invalid_trips)}\n")
        for trip in invalid_trips:
            print(f"Trip ID: {trip[0]} | Name: {trip[1]} | CurrencyId: {trip[2]}")
            
        print("\nسأقوم بإصلاح الرحلات...")
        cursor.execute("""
            UPDATE trips 
            SET currencyid = 1 
            WHERE currencyid IS NULL OR currencyid NOT IN (SELECT currencyid FROM currencies);
        """)
        affected = cursor.rowcount
        conn.commit()
        print(f"تم تحديث {affected} رحلة")
    else:
        print("جميع الرحلات لديها عملات صحيحة")
    
    cursor.close()
    conn.close()
    
    print("\n✅ تم الفحص والإصلاح بنجاح")
    
except Exception as e:
    print(f"Error: {e}")
    import traceback
    traceback.print_exc()
