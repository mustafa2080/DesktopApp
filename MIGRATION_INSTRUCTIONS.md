# تطبيق Migration يدوياً

يمكنك تطبيق الـ migration باستخدام إحدى الطرق التالية:

## الطريقة 1: استخدام Entity Framework (الموصى بها)

قم بتشغيل الأوامر التالية من terminal في مسار المشروع:

```bash
cd C:\Users\musta\Desktop\pro\accountant
dotnet ef migrations add AddAdultChildCount
dotnet ef database update
```

## الطريقة 2: استخدام SQLite Browser

1. افتح ملف `accountant.db` باستخدام DB Browser for SQLite
2. اذهب إلى تبويب "Execute SQL"
3. انسخ والصق الكود التالي:

```sql
-- إضافة عمود AdultCount
ALTER TABLE Trips ADD COLUMN AdultCount INTEGER NOT NULL DEFAULT 0;

-- إضافة عمود ChildCount
ALTER TABLE Trips ADD COLUMN ChildCount INTEGER NOT NULL DEFAULT 0;

-- تحديث البيانات الموجودة بناءً على TotalCapacity (توزيع 70/30)
UPDATE Trips 
SET AdultCount = CAST(TotalCapacity * 0.7 AS INTEGER),
    ChildCount = CAST(TotalCapacity * 0.3 AS INTEGER)
WHERE AdultCount = 0 AND ChildCount = 0;
```

4. اضغط على "Execute" أو F5

## الطريقة 3: البرنامج سيضيف الأعمدة تلقائياً

عند تشغيل البرنامج، Entity Framework سيقوم تلقائياً بإضافة الأعمدة الناقصة إذا كان configured بشكل صحيح.

## التحقق من نجاح العملية

بعد تطبيق Migration، تحقق من وجود الأعمدة الجديدة بتشغيل:

```sql
PRAGMA table_info(Trips);
```

يجب أن ترى `AdultCount` و `ChildCount` في قائمة الأعمدة.
