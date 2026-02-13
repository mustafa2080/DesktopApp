import psycopg2
from datetime import datetime

# الاتصال بقاعدة البيانات
conn = psycopg2.connect(
    host="localhost",
    port=5432,
    database="accountant_db",
    user="postgres",
    password="123"
)

cursor = conn.cursor()

print("=" * 80)
print("🔍 فحص بيانات العمرة في قاعدة البيانات")
print("=" * 80)

# عرض آخر 5 حزم عمرة
print("\n📦 آخر 5 حزم عمرة:")
cursor.execute("""
    SELECT 
        umrahpackageid,
        packagenumber,
        tripname,
        numberofpersons,
        roomtype,
        makkahhotel,
        madinahhotel,
        sellingprice,
        createdat,
        updatedat,
        createdby,
        updatedby
    FROM umrahpackages
    ORDER BY updatedat DESC
    LIMIT 5
""")

packages = cursor.fetchall()
for pkg in packages:
    print(f"\n{'─' * 80}")
    print(f"ID: {pkg[0]}")
    print(f"رقم الحزمة: {pkg[1]}")
    print(f"اسم الرحلة: {pkg[2]}")
    print(f"عدد الأفراد: {pkg[3]}")
    print(f"نوع الغرفة: {pkg[4]}")
    print(f"فندق مكة: {pkg[5]}")
    print(f"فندق المدينة: {pkg[6]}")
    print(f"سعر البيع: {pkg[7]}")
    print(f"تاريخ الإنشاء: {pkg[8]}")
    print(f"تاريخ التحديث: {pkg[9]}")
    print(f"أنشئ بواسطة: {pkg[10]}")
    print(f"حدث بواسطة: {pkg[11]}")
    
    # عرض المعتمرين
    cursor.execute("""
        SELECT 
            umrahpilgrimid,
            fullname,
            roomtype,
            sharedroomnumber,
            totalamount,
            paidamount
        FROM umrahpilgrims
        WHERE umrahpackageid = %s
        ORDER BY umrahpilgrimid
    """, (pkg[0],))
    
    pilgrims = cursor.fetchall()
    if pilgrims:
        print(f"\n   👥 المعتمرين ({len(pilgrims)}):")
        for i, p in enumerate(pilgrims, 1):
            room_type_ar = {
                1: "فردي",
                2: "ثنائي", 
                3: "ثلاثي",
                4: "رباعي",
                5: "خماسي",
                6: "جناح"
            }.get(p[2], "غير محدد")
            print(f"   {i}. {p[1]} - {room_type_ar} - غرفة: {p[3] or 'لا يوجد'}")
    else:
        print("\n   ⚠️ لا يوجد معتمرين!")

print("\n" + "=" * 80)

cursor.close()
conn.close()

print("\n✅ تم الانتهاء من الفحص")
