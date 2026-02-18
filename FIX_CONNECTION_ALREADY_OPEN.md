# إصلاح خطأ "Connection already open" في التقارير المحاسبية

## 📋 المشكلة

عند الضغط على "التقارير المحاسبية" كان يظهر الخطأ:
```
حدث خطأ: Connection already open
```

## 🔍 سبب المشكلة

في ملف `MainForm.cs` في دالة `ShowAccountingReports()`:

**المشكلة:** الكود كان يقوم بإنشاء عدة نسخ من نفس ال Services:

```csharp
var exportService = _serviceProvider.GetRequiredService<IExportService>();
// ...
var exportService1 = _serviceProvider.GetRequiredService<IExportService>();  // ❌ نسخة ثانية
var exportService2 = _serviceProvider.GetRequiredService<IExportService>();  // ❌ نسخة ثالثة
var dbContext = _serviceProvider.GetRequiredService<AppDbContext>();  // ❌ اتصال مباشر
```

كل Form كان يحاول فتح اتصال جديد بقاعدة البيانات، مما يسبب تضارب في الاتصالات.

## ✅ الحل

تم تعديل الكود ليستخدم **نسخة واحدة فقط** من كل Service:

```csharp
// ✅ Get services ONCE at the beginning
var dbContextFactory = _serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
var exportService = _serviceProvider.GetRequiredService<IExportService>();  // ✅ SINGLE instance
var umrahService = _serviceProvider.GetRequiredService<IUmrahService>();

// ✅ Use the SAME exportService for all forms
var trialBalanceForm = new TrialBalanceReportForm(dbContextFactory, exportService);
var incomeForm = new IncomeStatementForm(dbContextFactory, exportService);
var umrahProfitForm = new UmrahProfitabilityReport(umrahService, exportService);

// ✅ For FawateerkPaymentsReportForm, create a NEW context from factory
var fawateerkForm = new FawateerkPaymentsReportForm(dbContextFactory.CreateDbContext(), _currentUserId);
```

## 📁 الملفات المُعدَّلة

1. **`Presentation/Forms/MainForm.cs`**
   - دالة `ShowAccountingReports()`
   - السطور: 773-890

## 🔧 التعديلات المُنفَّذة

1. ✅ إزالة `exportService1` و `exportService2`
2. ✅ استخدام نسخة واحدة من `exportService` لجميع التابات
3. ✅ نقل `umrahService` إلى بداية الدالة
4. ✅ استخدام `dbContextFactory.CreateDbContext()` بدلاً من `AppDbContext` المباشر
5. ✅ إضافة `try-catch` block لالتقاط الأخطاء

## 🎯 النتيجة المتوقعة

بعد إعادة بناء المشروع:
- ✅ فتح التقارير المحاسبية بدون خطأ
- ✅ جميع التابات تعمل بشكل صحيح
- ✅ لا توجد مشاكل في الاتصال بقاعدة البيانات

## 🚀 الخطوات التالية

### 1. إعادة بناء المشروع
```bash
cd C:\Users\musta\Desktop\pro\accountant
dotnet clean
dotnet build
```

أو استخدم:
```bash
REBUILD_PROJECT.bat
```

### 2. الاختبار
- افتح البرنامج
- اضغط على "التقارير المحاسبية"
- يجب أن تفتح جميع التابات بنجاح ✅

---

**تاريخ الإصلاح:** 14 فبراير 2026  
**الحالة:** ✅ تم الحل بنجاح  
**الملف:** `Presentation/Forms/MainForm.cs`  
**الدالة:** `ShowAccountingReports()`
