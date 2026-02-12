# تطبيق نظام الصلاحيات والمستخدمين
## Permissions & Roles Implementation Guide

---

## 📋 نظرة عامة

تم إنشاء نظام صلاحيات كامل مع **3 أدوار** و **3 مستخدمين**:

### الأدوار (Roles):

1. **Operations Department** - قسم العمليات
   - الوصول: الرحلات + الآلة الحاسبة فقط

2. **Aviation and Umrah** - قسم الطيران والعمرة
   - الوصول: حجوزات الطيران + العمرة فقط

3. **Administrator** - المدير
   - الوصول: كامل النظام

---

## 🚀 خطوات التطبيق

### الخطوة 1: إنشاء Migration

```bash
cd C:\Users\musta\Desktop\pro\accountant
dotnet ef migrations add AddPermissionsSystem
```

### الخطوة 2: تطبيق Migration على قاعدة البيانات

```bash
dotnet ef database update
```

### الخطوة 3: تشغيل Seeder لإضافة البيانات

قم بتشغيل الملف:
```
Infrastructure/Data/SeedPermissions_Program.cs
```

أو استخدم الكود التالي في Program.cs:

```csharp
// في Program.cs في Main method
using var scope = serviceProvider.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await PermissionSeeder.SeedAsync(context);
```

---

## 👤 بيانات تسجيل الدخول

### 1. قسم العمليات (Operations)
```
Username: operations
Password: operations123
Access: 
  ✓ الرحلات (Trips)
  ✓ الآلة الحاسبة (Calculator)
  ✓ تقارير الرحلات
```

### 2. قسم الطيران والعمرة (Aviation & Umrah)
```
Username: aviation
Password: aviation123
Access:
  ✓ حجوزات الطيران (Flight Bookings)
  ✓ باقات العمرة (Umrah Packages)
  ✓ رحلات العمرة (Umrah Trips)
  ✓ إدارة المعتمرين (Pilgrims)
  ✓ تقارير الطيران والعمرة
```

### 3. المدير (Administrator)
```
Username: admin
Password: admin123
Access:
  ✓ جميع أقسام النظام
  ✓ إدارة المستخدمين
  ✓ إدارة الأدوار والصلاحيات
  ✓ النسخ الاحتياطي
```

---

## 📊 الصلاحيات المتاحة (148 صلاحية)

### قسم الرحلات (6 صلاحيات)
- ViewTrips
- CreateTrip
- EditTrip
- DeleteTrip
- CloseTrip
- ManageTripBookings

### قسم الطيران (5 صلاحيات)
- ViewFlightBookings
- CreateFlightBooking
- EditFlightBooking
- DeleteFlightBooking
- ManageFlightPayments

### قسم العمرة (10 صلاحيات)
- ViewUmrahPackages
- CreateUmrahPackage
- EditUmrahPackage
- DeleteUmrahPackage
- ViewUmrahTrips
- CreateUmrahTrip
- EditUmrahTrip
- DeleteUmrahTrip
- ManageUmrahPilgrims
- ManageUmrahPayments

### الآلة الحاسبة (1 صلاحية)
- UseCalculator

### باقي الصلاحيات (126 صلاحية)
- العملاء (5)
- الموردين (5)
- الفواتير (8)
- الحجوزات (4)
- الخزنة والبنوك (9)
- القيود اليومية (5)
- الحسابات (4)
- التقارير (12)
- الإعدادات (6)
- إدارة النظام (8)

---

## 💻 كيفية استخدام نظام الصلاحيات في الكود

### 1. إضافة PermissionService إلى DI Container

في `Program.cs`:

```csharp
services.AddScoped<IPermissionService, PermissionService>();
```

### 2. استخدام الصلاحيات في الفورم

#### مثال 1: إخفاء الفورم بالكامل

```csharp
public class TripsListForm : Form
{
    private readonly IPermissionService _permissionService;
    private readonly int _currentUserId;
    
    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        
        // Check permission
        if (!await _permissionService.HasPermissionAsync(_currentUserId, PermissionType.ViewTrips))
        {
            MessageBox.Show(
                "ليس لديك صلاحية الوصول إلى الرحلات",
                "خطأ في الصلاحيات",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            this.Close();
            return;
        }
        
        // Load data...
    }
}
```

#### مثال 2: تعطيل أزرار معينة

```csharp
private async Task SetupPermissionsAsync()
{
    // Disable buttons based on permissions
    btnAdd.Enabled = await _permissionService.HasPermissionAsync(
        _currentUserId, 
        PermissionType.CreateTrip
    );
    
    btnEdit.Enabled = await _permissionService.HasPermissionAsync(
        _currentUserId, 
        PermissionType.EditTrip
    );
    
    btnDelete.Enabled = await _permissionService.HasPermissionAsync(
        _currentUserId, 
        PermissionType.DeleteTrip
    );
}
```

### 3. إخفاء عناصر من القائمة (Sidebar)

```csharp
// في MainForm أو SidebarControl
private async Task SetupMenuPermissionsAsync()
{
    // Get user permissions by module
    var permissions = await _permissionService.GetUserPermissionsByModuleAsync(_currentUserId);
    
    // Hide/Show menu items
    menuTrips.Visible = permissions.ContainsKey("Trips") && permissions["Trips"].Any();
    menuAviation.Visible = permissions.ContainsKey("Aviation") && permissions["Aviation"].Any();
    menuUmrah.Visible = permissions.ContainsKey("Umrah") && permissions["Umrah"].Any();
    menuAccounting.Visible = permissions.ContainsKey("Accounting") && permissions["Accounting"].Any();
    menuReports.Visible = permissions.ContainsKey("Reports") && permissions["Reports"].Any();
    menuSystem.Visible = permissions.ContainsKey("System") && permissions["System"].Any();
    
    // Calculator
    menuCalculator.Visible = await _permissionService.HasPermissionAsync(
        _currentUserId, 
        PermissionType.UseCalculator
    );
}
```

### 4. التحقق من صلاحيات متعددة

```csharp
// التحقق من أي صلاحية (OR)
bool canAccessReports = await _permissionService.HasAnyPermissionAsync(
    _currentUserId,
    PermissionType.ViewReports,
    PermissionType.ViewFinancialReports,
    PermissionType.ViewTripReports
);

// التحقق من جميع الصلاحيات (AND)
bool canManageSystem = await _permissionService.HasAllPermissionsAsync(
    _currentUserId,
    PermissionType.ManageUsers,
    PermissionType.ManageRoles,
    PermissionType.BackupDatabase
);
```

---

## 🔄 تحديث الصلاحيات بعد تغيير الدور

```csharp
// بعد تغيير دور المستخدم
_permissionService.ClearCache(userId);
```

---

## 📝 إضافة صلاحيات جديدة

### الخطوة 1: إضافة في Enum

في `Domain/Entities/Permission.cs`:

```csharp
public enum PermissionType
{
    // ... existing permissions
    
    // New permission
    ManagePayroll = 200,
}
```

### الخطوة 2: إضافة في Seeder

في `Infrastructure/Data/PermissionSeeder.cs`:

```csharp
new() 
{ 
    PermissionType = PermissionType.ManagePayroll, 
    PermissionName = "إدارة الرواتب", 
    Category = "HR", 
    Module = "HumanResources" 
},
```

### الخطوة 3: ربطها بدور معين

في نفس الملف:

```csharp
var adminPermissions = new[]
{
    // ... existing permissions
    PermissionType.ManagePayroll
};
```

---

## 🧪 اختبار النظام

### Test Case 1: تسجيل دخول كـ Operations
```
1. Login با operations / operations123
2. يجب أن ترى: الرحلات + الآلة الحاسبة فقط
3. لا يجب أن ترى: الطيران، العمرة، المحاسبة، إلخ
```

### Test Case 2: تسجيل دخول كـ Aviation
```
1. Login با aviation / aviation123
2. يجب أن ترى: الطيران + العمرة فقط
3. لا يجب أن ترى: الرحلات، المحاسبة، إلخ
```

### Test Case 3: تسجيل دخول كـ Admin
```
1. Login با admin / admin123
2. يجب أن ترى: جميع الأقسام
3. يجب أن تتمكن من: إدارة المستخدمين + الأدوار
```

---

## ⚠️ ملاحظات مهمة

1. **Security**: تأكد من تغيير كلمات المرور الافتراضية في الإنتاج

2. **Caching**: نظام الصلاحيات يستخدم cache لمدة 10 دقائق لتحسين الأداء

3. **Database**: تأكد من عمل backup قبل تطبيق الـ migrations

4. **Testing**: اختبر كل دور بشكل منفصل قبل الإطلاق

5. **Permissions**: يمكن إضافة/تعديل الصلاحيات حسب الحاجة

---

## 🐛 حل المشاكل الشائعة

### المشكلة 1: Migration فشلت
```bash
# حذف آخر migration
dotnet ef migrations remove

# إعادة إنشائها
dotnet ef migrations add AddPermissionsSystem
```

### المشكلة 2: Seeder أضاف بيانات مكررة
```sql
-- حذف البيانات
DELETE FROM rolepermissions;
DELETE FROM permissions;
DELETE FROM users WHERE username IN ('operations', 'aviation', 'admin');
DELETE FROM roles WHERE rolename IN ('Operations Department', 'Aviation and Umrah', 'Administrator');

-- إعادة تشغيل الـ Seeder
```

### المشكلة 3: المستخدم لا يرى أي أقسام
```csharp
// تأكد من:
1. أن المستخدم له RoleId صحيح
2. أن الدور له صلاحيات مرتبطة في RolePermissions
3. أن الـ cache تم مسحه بعد تغيير الصلاحيات
```

---

## 📞 الخطوات التالية

بعد تطبيق نظام الصلاحيات، يجب:

1. ✅ تطبيق الصلاحيات على جميع الفورمز
2. ✅ إخفاء/إظهار عناصر القائمة حسب الصلاحيات
3. ✅ تعطيل الأزرار حسب الصلاحيات
4. ✅ إنشاء فورم إدارة المستخدمين
5. ✅ إنشاء فورم إدارة الأدوار والصلاحيات
6. ✅ إضافة Audit Log لتتبع التغييرات

---

**تاريخ الإنشاء:** 2026-02-09  
**الحالة:** جاهز للتطبيق  
**الإصدار:** 1.0
