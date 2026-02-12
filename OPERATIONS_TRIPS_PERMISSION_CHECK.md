# تحقق من صلاحيات Operations User

## المشكلة
- المستخدم operations المفروض يشوف قسم الرحلات في القائمة الجانبية
- الصلاحيات موجودة في قاعدة البيانات

## التحقق من الصلاحيات

### صلاحيات Operations Role في قاعدة البيانات:

**Module: Calculator**
- ✅ UseCalculator (ID: 12)

**Module: Trips**
- ✅ ViewTrips (ID: 1) - عرض الرحلات
- ✅ CreateTrip (ID: 2) - إنشاء رحلة
- ✅ EditTrip (ID: 3) - تعديل رحلة
- ✅ ManageTripBookings (ID: 4) - إدارة حجوزات الرحلات

---

## التشخيص

### 1. الصلاحيات موجودة ✅
```sql
SELECT p.PermissionName, p.Module
FROM RolePermissions rp
JOIN Permissions p ON rp.PermissionId = p.PermissionId
WHERE rp.RoleId = 3
```
النتيجة: operations user عنده 4 صلاحيات في Module "Trips"

### 2. الكود الصحيح في SidebarControl.cs ✅
```csharp
bool hasTrips = permissionsByModule.ContainsKey("Trips");

if (hasTrips)
{
    Console.WriteLine("🚌 User has Trips permission - showing Trips");
    SetMenuItemVisibility("trips", true);
}
```

---

## الحل المحتمل

### السبب 1: Cache
الـ PermissionService يستخدم cache. إذا كان الـ cache قديم، ممكن الصلاحيات ما تظهرش.

**الحل:**
1. إعادة تشغيل البرنامج (يمسح الـ cache)
2. أو تسجيل خروج ثم دخول مرة أخرى

### السبب 2: الـ Service Provider
ممكن الـ PermissionService مش بيتحدث صح.

**للتحقق:**
1. افتح البرنامج
2. سجل دخول بـ operations user
3. شوف الـ Console Output
4. لازم يظهر:
   ```
   🔍 Getting permissions for user ID: 3
   📊 Found X modules:
      ✓ Module: Trips (4 permissions)
      ✓ Module: Calculator (1 permissions)
   🚌 User has Trips permission - showing Trips
   ```

---

## خطوات الاختبار

### 1. تسجيل الدخول
```
Username: operations
Password: operations123
```

### 2. التحقق من القائمة الجانبية
يجب أن يظهر:
- ✅ 🏠 الرئيسية (Dashboard)
- ✅ 🚌 الرحلات (Trips)
- ✅ 🧮 الآلة الحاسبة (Calculator)

يجب ألا يظهر:
- ❌ إدارة المستخدمين
- ❌ الحسابات
- ❌ العملاء
- ❌ الموردين
- ❌ الحجوزات (Reservations)
- ❌ الطيران
- ❌ العمرة
- ❌ الفواتير
- ❌ الخزنة
- ❌ البنوك
- ❌ القيود اليومية
- ❌ التقارير
- ❌ التقارير المحاسبية
- ❌ الإعدادات

---

## إذا لم يظهر قسم الرحلات

### خطوة 1: تحقق من Console Output
عند تسجيل الدخول، شوف الـ console في Visual Studio. المفروض يظهر:
```
🔍 Getting permissions for user ID: 3
📊 Found 2 modules:
   ✓ Module: Trips (4 permissions)
   ✓ Module: Calculator (1 permissions)
🔧 ApplyPermissionsSync started
   Modules received: Trips, Calculator
   hasTrips: True
🚌 User has Trips permission - showing Trips
   trips: ✓ VISIBLE
```

### خطوة 2: إذا ظهر hasTrips: False
معناها مشكلة في الـ PermissionService. الحل:
1. امسح ملف `accountant.db` وأعد إنشائه
2. أو أعد تشغيل migration

### خطوة 3: إذا ظهر "trips: ✗ HIDDEN"
معناها مشكلة في الكود. راجع SidebarControl.cs

---

## الملخص

✅ **الصلاحيات صحيحة في قاعدة البيانات**
✅ **الكود صحيح في SidebarControl.cs**
✅ **المفروض قسم الرحلات يظهر تلقائياً**

إذا لم يظهر، المشكلة في:
1. **Cache** - الحل: إعادة تشغيل
2. **PermissionService** - الحل: تحقق من Console Output
3. **Database** - الحل: راجع RolePermissions table
