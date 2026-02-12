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
    print("اصلاح مشكلة العملة:")
    print("=" * 60)
    
    # الحل 1: حذف العملة الحالية (ID=2)
    print("\n1. حذف العملة الحالية (ID=2)...")
    cursor.execute("DELETE FROM currencies WHERE currencyid = 2;")
    
    print("2. اضافة عملة EGP بـ ID = 1...")
    cursor.execute("""
        INSERT INTO currencies (currencyid, currencycode, currencyname, symbol, exchangerate, isbasecurrency, isactive, "CreatedAt")
        VALUES (1, 'EGP', 'جنيه مصري', 'ج.م', 1.0, true, true, NOW())
        ON CONFLICT (currencyid) DO NOTHING;
    """)
    
    print("3. اضافة عملات اضافية...")
    cursor.execute("""
        INSERT INTO currencies (currencyid, currencycode, currencyname, symbol, exchangerate, isbasecurrency, isactive, "CreatedAt")
        VALUES 
            (2, 'USD', 'دولار امريكي', '$', 50.0, false, true, NOW()),
            (3, 'SAR', 'ريال سعودي', 'ريال', 13.33, false, true, NOW()),
            (4, 'EUR', 'يورو', 'E', 55.0, false, true, NOW())
        ON CONFLICT (currencyid) DO NOTHING;
    """)
    
    conn.commit()
    
    print("\n4. التحقق من النتيجة...")
    cursor.execute("SELECT currencyid, currencycode, currencyname, symbol FROM currencies ORDER BY currencyid;")
    currencies = cursor.fetchall()
    
    print("\nالعملات المتاحة الان:")
    for curr in currencies:
        print(f"  ID: {curr[0]} | Code: {curr[1]} | Name: {curr[2]} | Symbol: {curr[3]}")
    
    cursor.close()
    conn.close()
    
    print("\nتم اصلاح المشكلة بنجاح!")
    print("\nالان يمكنك حفظ الرحلات بدون مشاكل.")
    
except Exception as e:
    print(f"Error: {e}")
    import traceback
    traceback.print_exc()
