# Quick Fix Script for Umrah Changes

## الأخطاء المتبقية:

في الملفات التالية، يجب استبدال:
- `SupervisorExpenses` بـ `SupervisorExpensesSAR` ثم حساب `SupervisorExpensesEGP`
- `ProfitMargin` بـ `ProfitMarginPercent`

### ملفات تحتاج تعديل:

1. **AppDbContext.cs** (سطر 1671 و 1732):
   - استبدل: `entity.Property(e => e.SupervisorExpenses).HasColumnName("supervisorexpenses");`
   - بـ: `entity.Property(e => e.SupervisorExpensesSAR).HasColumnName("supervisorexpensesSAR");`

2. **UmrahService.cs** (سطر 222):
   - استبدل: `existing.SupervisorExpenses = package.SupervisorExpenses;`
   - بـ: `existing.SupervisorExpensesSAR = package.SupervisorExpensesSAR;`

3. **UmrahPackageDetailsForm.cs** (سطر 148):
   - استبدل: `package.SupervisorExpenses`
   - بـ: `package.SupervisorExpensesEGP`

4. **UmrahPackageDetailsForm.cs** (سطر 418):
   - استبدل: `package.ProfitMargin`
   - بـ: `package.ProfitMarginPercent`

5. **UmrahReportsForm.cs** (714, 734, 943):
   - استبدل: `ProfitMargin`
   - بـ: `ProfitMarginPercent`

6. **UmrahProfitabilityReport.cs** (جميع الأماكن):
   - استبدل: `SupervisorExpenses`
   - بـ: `SupervisorExpensesEGP`
   - استبدل: `ProfitMargin`
   - بـ: `ProfitMarginPercent`

## خطوات التطبيق:

1. استبدل في الملفات المذكورة
2. Build المشروع
3. Apply Migration: `dotnet ef database update`
