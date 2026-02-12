# تحقق من صلاحيات Aviation User

## معلومات المستخدم

**Username:** aviation  
**Full Name:** موظف الطيران والعمرة  
**User ID:** 2  
**Role ID:** 2  
**Role Name:** Aviation and Umrah  

---

## الصلاحيات الحالية

### ✅ Module: Aviation (3 permissions)
- **CreateFlightBooking** (ID: 6) - إنشاء حجز طيران
- **EditFlightBooking** (ID: 7) - تعديل حجز طيران
- **ViewFlightBookings** (ID: 5) - عرض حجوزات الطيران

### ✅ Module: Umrah (4 permissions)
- **CreateUmrahPackage** (ID: 9) - إنشاء باقة عمرة
- **EditUmrahPackage** (ID: 10) - تعديل باقة عمرة
- **ViewUmrahPackages** (ID: 8) - عرض باقات العمرة
- **ViewUmrahTrips** (ID: 11) - عرض رحلات العمرة

### ✅ Module: Calculator (1 permission)
- **UseCalculator** (ID: 12) - استخدام الآلة الحاسبة

**إجمالي الصلاحيات:** 8 صلاحيات ✅

---

## الأقسام المتوقعة في القائمة الجانبية

عند تسجيل الدخول بـ aviation user، يجب أن تظهر الأقسام التالية:

### ✅ يجب أن يظهر:
- 🏠 **الرئيسية** (Dashboard) - دائماً ظاهر
- ✈️ **الطيران** (Flights) - لأن عنده صلاحيات Aviation
- 🕌 **العمرة** (Umrah) - لأن عنده صلاحيات Umrah
- 🧮 **الآلة الحاسبة** (Calculator) - لأن عنده صلاحية UseCalculator

### ❌ يجب ألا يظهر:
- إدارة المستخدمين (System permissions)
- الحسابات (Accounting)
- العملاء (Customers)
- الموردين (Suppliers)
- الحجوزات (Reservations)
- الرحلات (Trips)
- الفواتير (Invoices)
- الخزنة (CashBox)
- البنوك (Banks)
- القيود اليومية (Journals)
- التقارير (Reports)
- التقارير المحاسبية (Accounting Reports)
- الإعدادات (Settings)

---

## التحقق من الكود

### SidebarControl.cs - ApplyPermissionsSync Method

```csharp
bool hasAviation = permissionsByModule.ContainsKey("Aviation");
bool hasUmrah = permissionsByModule.ContainsKey("Umrah");

// إظهار الطيران إذا كان للمستخدم صلاحية Aviation
if (hasAviation)
{
    Console.WriteLine("✈️ User has Aviation permission - showing Flights");
    SetMenuItemVisibility("flights", true);
}

// إظهار العمرة إذا كان للمستخدم صلاحية Umrah
if (hasUmrah)
{
    Console.WriteLine("🕌 User has Umrah permission - showing Umrah");
    SetMenuItemVisibility("umrah", true);
}
```

✅ **الكود صحيح ويدعم Aviation و Umrah**

---

## Console Output المتوقع

عند تسجيل الدخول بـ aviation user، يجب أن يظهر في Console:

```
🔍 Getting permissions for user ID: 2
📊 Found 3 modules:
   ✓ Module: Aviation (3 permissions)
   ✓ Module: Umrah (4 permissions)
   ✓ Module: Calculator (1 permissions)

🔧 ApplyPermissionsSync started
   Modules received: Aviation, Umrah, Calculator
   hasSystem: False
   hasAviation: True
   hasUmrah: True
   hasTrips: False
   hasReports: False

👤 Regular user - applying permission filters
   Hiding all sections first...
   
✈️ User has Aviation permission - showing Flights
   flights: ✓ VISIBLE
   
🕌 User has Umrah permission - showing Umrah
   umrah: ✓ VISIBLE
   
   calculator: ✓ VISIBLE
   dashboard: ✓ VISIBLE
   
🔧 ApplyPermissionsSync completed
```

---

## خطوات الاختبار

### 1. تسجيل الدخول
```
Username: aviation
Password: aviation123
```

### 2. التحقق من القائمة الجانبية
افتح القائمة الجانبية وتأكد من ظهور:
- ✅ 🏠 الرئيسية
- ✅ ✈️ الطيران
- ✅ 🕌 العمرة
- ✅ 🧮 الآلة الحاسبة

### 3. التحقق من الوظائف

#### قسم الطيران:
- عرض قائمة حجوزات الطيران
- إضافة حجز طيران جديد
- تعديل حجز طيران موجود

#### قسم العمرة:
- عرض قائمة باقات العمرة
- إضافة باقة عمرة جديدة
- تعديل باقة عمرة موجودة
- عرض رحلات العمرة

---

## الحلول إذا لم تظهر الأقسام

### السبب 1: Cache قديم ❌

**الأعراض:**
- الصلاحيات موجودة في قاعدة البيانات
- الكود صحيح
- لكن الأقسام لا تظهر

**الحل:**
1. إعادة تشغيل البرنامج (يمسح الـ cache)
2. أو تسجيل خروج ثم دخول مرة أخرى

### السبب 2: خطأ في PermissionService ❌

**الأعراض:**
- Console Output يظهر:
  ```
  📊 Found 0 modules
  ```
  أو
  ```
  hasAviation: False
  hasUmrah: False
  ```

**الحل:**
1. تحقق من جدول RolePermissions في قاعدة البيانات
2. تأكد من وجود السجلات:
   ```sql
   SELECT * FROM RolePermissions WHERE RoleId = 2
   ```

### السبب 3: مشكلة في قاعدة البيانات ❌

**الحل:**
```sql
-- التحقق من الصلاحيات
SELECT p.PermissionName, p.Module
FROM RolePermissions rp
JOIN Permissions p ON rp.PermissionId = p.PermissionId
WHERE rp.RoleId = 2
ORDER BY p.Module;

-- يجب أن يظهر 8 صلاحيات
```

---

## استعلامات SQL للتحقق

### 1. التحقق من معلومات المستخدم
```sql
SELECT u.UserId, u.Username, u.FullName, r.RoleName
FROM Users u
JOIN Roles r ON u.RoleId = r.RoleId
WHERE u.Username = 'aviation';
```

**النتيجة المتوقعة:**
| UserId | Username | FullName | RoleName |
|--------|----------|----------|----------|
| 2 | aviation | موظف الطيران والعمرة | Aviation and Umrah |

### 2. التحقق من الصلاحيات
```sql
SELECT p.Module, p.PermissionName
FROM Users u
JOIN Roles r ON u.RoleId = r.RoleId
JOIN RolePermissions rp ON r.RoleId = rp.RoleId
JOIN Permissions p ON rp.PermissionId = p.PermissionId
WHERE u.Username = 'aviation'
ORDER BY p.Module, p.PermissionName;
```

**النتيجة المتوقعة:** 8 صفوف
- 3 من Aviation
- 4 من Umrah
- 1 من Calculator

---

## الخلاصة

✅ **الصلاحيات صحيحة 100% في قاعدة البيانات**
- Aviation user عنده 3 صلاحيات في Aviation module
- Aviation user عنده 4 صلاحيات في Umrah module
- Aviation user عنده 1 صلاحية في Calculator module

✅ **الكود صحيح 100% في SidebarControl.cs**
- يتحقق من `hasAviation` ويظهر قسم الطيران
- يتحقق من `hasUmrah` ويظهر قسم العمرة

✅ **المفروض يشتغل تلقائياً**
- عند تسجيل الدخول بـ aviation user
- يجب أن تظهر أقسام: الطيران + العمرة + الآلة الحاسبة + الرئيسية

🎯 **إذا لم يظهر:** السبب الأكثر احتمالاً هو Cache قديم
- **الحل:** أعد تشغيل البرنامج أو سجل خروج/دخول

---

## بيانات الدخول

```
Username: aviation
Password: aviation123
```

**الأقسام المتوقعة:**
- 🏠 الرئيسية
- ✈️ الطيران
- 🕌 العمرة
- 🧮 الآلة الحاسبة
