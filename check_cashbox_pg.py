import psycopg2

# الاتصال بقاعدة البيانات PostgreSQL
conn = psycopg2.connect(
    host="localhost",
    port=5432,
    database="graceway_accounting",
    user="postgres",
    password="123456"
)

cursor = conn.cursor()

print("=" * 80)
print("فحص بيانات الخزنة - PostgreSQL")
print("=" * 80)

# 1. عرض عدد المعاملات حسب الشهر
print("\n1️⃣ عدد المعاملات حسب الشهر:")
print("-" * 80)

cursor.execute("""
    SELECT 
        TO_CHAR("TransactionDate", 'YYYY-MM') as month,
        COUNT(*) as count,
        SUM(CASE WHEN "Type" = 0 THEN "Amount" ELSE 0 END) as income,
        SUM(CASE WHEN "Type" = 1 THEN "Amount" ELSE 0 END) as expense
    FROM "CashTransactions" 
    WHERE "IsDeleted" = false 
    GROUP BY month 
    ORDER BY month DESC 
    LIMIT 12
""")

results = cursor.fetchall()

if results:
    print(f"{'الشهر':<15} {'العدد':<10} {'الإيرادات':<15} {'المصروفات':<15}")
    print("-" * 60)
    for row in results:
        print(f"{row[0]:<15} {row[1]:<10} {row[2] or 0:<15.2f} {row[3] or 0:<15.2f}")
else:
    print("❌ لا توجد معاملات في قاعدة البيانات!")

# 2. عرض معاملات شهر فبراير 2026 تحديداً
print("\n" + "=" * 80)
print("2️⃣ معاملات فبراير 2026:")
print("-" * 80)

cursor.execute("""
    SELECT 
        "Id",
        "TransactionDate",
        "Type",
        "Amount",
        "Description",
        "Month",
        "Year",
        "CashBoxId"
    FROM "CashTransactions" 
    WHERE "IsDeleted" = false 
        AND "Month" = 2
        AND "Year" = 2026
    ORDER BY "TransactionDate" DESC, "Id" DESC
    LIMIT 20
""")

feb_transactions = cursor.fetchall()

if feb_transactions:
    print(f"\n{'ID':<8} {'التاريخ':<12} {'النوع':<10} {'المبلغ':<12} {'الوصف':<30} {'الخزنة':<8}")
    print("-" * 90)
    for t in feb_transactions:
        trans_type = "إيراد" if t[2] == 0 else "مصروف"
        print(f"{t[0]:<8} {str(t[1]):<12} {trans_type:<10} {t[3]:<12.2f} {(t[4] or '')[:30]:<30} {t[7]:<8}")
    print(f"\n✅ إجمالي: {len(feb_transactions)} معاملة في فبراير 2026")
else:
    print("\n❌ لا توجد معاملات في فبراير 2026!")

# 3. عرض أحدث 10 معاملات بشكل عام
print("\n" + "=" * 80)
print("3️⃣ أحدث 10 معاملات:")
print("-" * 80)

cursor.execute("""
    SELECT 
        "Id",
        "TransactionDate",
        "Type",
        "Amount",
        "Description",
        "Month",
        "Year",
        "CashBoxId"
    FROM "CashTransactions" 
    WHERE "IsDeleted" = false 
    ORDER BY "TransactionDate" DESC, "Id" DESC
    LIMIT 10
""")

latest_transactions = cursor.fetchall()

if latest_transactions:
    print(f"\n{'ID':<8} {'التاريخ':<12} {'النوع':<10} {'المبلغ':<12} {'الشهر':<6} {'السنة':<6} {'الخزنة':<8}")
    print("-" * 80)
    for t in latest_transactions:
        trans_type = "إيراد" if t[2] == 0 else "مصروف"
        print(f"{t[0]:<8} {str(t[1]):<12} {trans_type:<10} {t[3]:<12.2f} {t[5]:<6} {t[6]:<6} {t[7]:<8}")
else:
    print("\n❌ لا توجد معاملات!")

# 4. عرض الخزنات الموجودة
print("\n" + "=" * 80)
print("4️⃣ الخزنات الموجودة:")
print("-" * 80)

cursor.execute("""
    SELECT "Id", "Name", "CurrentBalance"
    FROM "CashBoxes"
    WHERE "IsDeleted" = false
    ORDER BY "Id"
""")

cashboxes = cursor.fetchall()

if cashboxes:
    print(f"\n{'ID':<8} {'الاسم':<30} {'الرصيد الحالي':<15}")
    print("-" * 60)
    for cb in cashboxes:
        print(f"{cb[0]:<8} {cb[1]:<30} {cb[2] or 0:<15.2f}")
else:
    print("\n❌ لا توجد خزنات!")

# 5. عرض عدد المعاملات لكل خزنة
print("\n" + "=" * 80)
print("5️⃣ عدد المعاملات لكل خزنة:")
print("-" * 80)

cursor.execute("""
    SELECT 
        cb."Id",
        cb."Name",
        COUNT(ct."Id") as transaction_count,
        SUM(CASE WHEN ct."Type" = 0 THEN ct."Amount" ELSE 0 END) as total_income,
        SUM(CASE WHEN ct."Type" = 1 THEN ct."Amount" ELSE 0 END) as total_expense
    FROM "CashBoxes" cb
    LEFT JOIN "CashTransactions" ct ON cb."Id" = ct."CashBoxId" AND ct."IsDeleted" = false
    WHERE cb."IsDeleted" = false
    GROUP BY cb."Id", cb."Name"
    ORDER BY cb."Id"
""")

cashbox_stats = cursor.fetchall()

if cashbox_stats:
    print(f"\n{'ID':<8} {'الاسم':<30} {'عدد المعاملات':<15} {'الإيرادات':<15} {'المصروفات':<15}")
    print("-" * 90)
    for stat in cashbox_stats:
        print(f"{stat[0]:<8} {stat[1]:<30} {stat[2]:<15} {stat[3] or 0:<15.2f} {stat[4] or 0:<15.2f}")
else:
    print("\n❌ لا توجد خزنات!")

conn.close()

print("\n" + "=" * 80)
print("✅ انتهى التقرير")
print("=" * 80)
