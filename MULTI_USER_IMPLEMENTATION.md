# Multi-User Support Implementation

## نظرة عامة
تم تطوير البرنامج لدعم استخدام متعدد المستخدمين في وقت واحد على أجهزة متعددة عبر شبكة داخلية.

## التحسينات المطبقة

### 1. Optimistic Concurrency Control
**الملفات المعنية:**
- `Infrastructure/Data/AppDbContext.cs`
- `Infrastructure/Migrations/20260213120000_AddConcurrencySupport.cs`

**الوصف:**
- استخدام PostgreSQL xmin system column كـ concurrency token
- منع تعارض البيانات عند تعديل نفس السجل من مستخدمين مختلفين
- الجداول المحمية: CashBoxes, CashTransactions, Invoices, Trips, UmrahPackages, FlightBookings, Customers, Suppliers, Accounts, JournalEntries

**المثال:**
```csharp
modelBuilder.Entity<CashBox>()
    .Property<uint>("xmin")
    .HasColumnType("xid")
    .ValueGeneratedOnAddOrUpdate()
    .IsConcurrencyToken();
```

### 2. Concurrency Exception Handler
**الملف:** `Infrastructure/Data/ConcurrencyExceptionHandler.cs`

**المميزات:**
- Auto-retry logic مع exponential backoff
- رسائل خطأ واضحة للمستخدم بالعربي
- حد أقصى 3 محاولات للـ retry
- معالجة automatic للـ conflicts

**الاستخدام:**
```csharp
await ConcurrencyExceptionHandler.ExecuteWithRetryAsync(async () =>
{
    // Your database operation here
    await _context.SaveChangesAsync();
});
```

### 3. Session Management
**الملف:** `Infrastructure/Data/SessionManager.cs`

**المميزات:**
- تتبع جميع الجلسات النشطة
- معلومات عن كل جلسة: User, Machine, Login Time, Last Activity
- تنظيف automatic للجلسات الخاملة
- Thread-safe باستخدام ConcurrentDictionary

**الواجهة:**
- Form جديدة: `Presentation/Forms/ActiveSessionsForm.cs`
- عرض real-time للجلسات النشطة
- تحديث automatic كل 5 ثوان
- إمكانية تنظيف الجلسات القديمة

### 4. Transaction Scope Helper
**الملف:** `Infrastructure/Data/TransactionScopeHelper.cs`

**المستويات المدعومة:**
- **Read Committed**: للعمليات العادية (منع dirty reads)
- **Serializable**: للعمليات المالية الحساسة (منع phantom reads)
- **Snapshot**: للتقارير طويلة المدة

**الاستخدام:**
```csharp
// For financial operations
await TransactionScopeHelper.ExecuteInSerializableTransactionAsync(_context, async () =>
{
    // Critical financial operation
    cashBox.CurrentBalance += amount;
    await _context.SaveChangesAsync();
});
```

### 5. Enhanced Connection Pooling
**الملف:** `appsettings.json`

**التحسينات:**
- Min Pool Size: 10 → 100 connections
- Max Pool Size: 50 → 100 connections
- Connection Lifetime: 300 seconds
- Automatic connection pruning
- Load Table Composites enabled

### 6. Enhanced DbContext Configuration
**الملف:** `Infrastructure/Data/AppDbContext.cs`

**التحسينات:**
- NoTracking query behavior بشكل افتراضي
- Session tracking في كل DbContext
- Automatic session cleanup عند dispose
- Retry logic مدمج في SaveChangesAsync

### 7. Service Layer Improvements
**الملف:** `Application/Services/CashBoxService.cs`

**التحسينات:**
- استخدام ConcurrencyExceptionHandler في Update/Delete
- استخدام TransactionScopeHelper للعمليات المالية
- Proper isolation levels لكل نوع عملية

## كيفية الاستخدام

### للمطورين:

#### 1. إضافة Concurrency Control لجدول جديد:
```csharp
modelBuilder.Entity<YourEntity>()
    .Property<uint>("xmin")
    .HasColumnType("xid")
    .ValueGeneratedOnAddOrUpdate()
    .IsConcurrencyToken();
```

#### 2. استخدام Retry Logic:
```csharp
public async Task UpdateEntityAsync(Entity entity)
{
    using var context = _contextFactory.CreateDbContext();
    
    await ConcurrencyExceptionHandler.ExecuteWithRetryAsync(async () =>
    {
        var existing = await context.Entities.FindAsync(entity.Id);
        // Update properties
        await context.SaveChangesAsync();
    });
}
```

#### 3. استخدام Transactions للعمليات المعقدة:
```csharp
public async Task ComplexFinancialOperation()
{
    using var context = _contextFactory.CreateDbContext();
    
    await TransactionScopeHelper.ExecuteInSerializableTransactionAsync(context, async () =>
    {
        // Multiple related operations
        await context.SaveChangesAsync();
    });
}
```

### للمستخدمين:

#### مراقبة الجلسات النشطة:
1. افتح البرنامج كـ Admin
2. من القائمة الجانبية، اختر "🔗 الجلسات النشطة"
3. ستشاهد قائمة بجميع المستخدمين المتصلين حالياً
4. يمكنك تنظيف الجلسات الخاملة (أكثر من 60 دقيقة)

#### رسائل التعارض:
- إذا حاول مستخدمان تعديل نفس السجل:
  - سيظهر رسالة: "تم تعديل [السجل] بواسطة مستخدم آخر"
  - يتم إعادة المحاولة automatically حتى 3 مرات
  - إذا فشل، يُطلب من المستخدم تحديث البيانات

## متطلبات النظام

### قاعدة البيانات:
- PostgreSQL 12 أو أحدث
- Connection pooling enabled
- Proper indexes على الجداول الرئيسية

### الشبكة:
- شبكة داخلية مستقرة (LAN)
- اتصال سريع بـ PostgreSQL server
- Firewall rules مناسبة

### الأجهزة:
- RAM: 8GB minimum (16GB recommended)
- معالج: Dual-core minimum (Quad-core recommended)

## الأداء والتحسينات

### Connection Pooling:
- يحافظ على 10 connections جاهزة دائماً
- يصل إلى 100 connection عند الحاجة
- Auto-cleanup للـ idle connections

### Query Performance:
- NoTracking mode يقلل memory overhead
- Proper indexes على foreign keys
- Batch operations حيثما أمكن

### Concurrency:
- Optimistic locking يقلل lock contention
- Retry logic يعالج temporary conflicts
- Transaction isolation يمنع dirty reads

## الأمان

### Session Security:
- كل session له ID فريد
- تتبع Machine Name لكل جلسة
- Automatic timeout للجلسات الخاملة

### Data Integrity:
- Serializable transactions للعمليات المالية
- Concurrency tokens تمنع lost updates
- Audit logging لجميع التغييرات

## استكشاف الأخطاء

### مشكلة: "تم تعديل السجل بواسطة مستخدم آخر"
**الحل:**
1. تحديث البيانات من القاعدة
2. إعادة المحاولة
3. إذا تكرر، التواصل مع المستخدمين الآخرين

### مشكلة: بطء في الأداء
**الحل:**
1. التحقق من عدد الجلسات النشطة
2. تنظيف الجلسات القديمة
3. إعادة تشغيل PostgreSQL إذا لزم الأمر

### مشكلة: Connection pool exhausted
**الحل:**
1. زيادة Max Pool Size في appsettings.json
2. التحقق من وجود connection leaks
3. استخدام proper using statements

## الخطوات التالية (اختياري)

### تحسينات مستقبلية:
- [ ] إضافة Real-time notifications للمستخدمين
- [ ] Lock indication في الواجهة
- [ ] Conflict resolution UI
- [ ] Performance monitoring dashboard
- [ ] Database connection monitoring
- [ ] Advanced session analytics

## الملخص

تم تطبيق نظام متكامل لدعم المستخدمين المتعددين مع:
✅ Optimistic Concurrency Control
✅ Automatic Retry Logic
✅ Session Management
✅ Transaction Isolation
✅ Enhanced Connection Pooling
✅ Real-time Session Monitoring

البرنامج الآن جاهز للاستخدام على شبكة داخلية مع عدة مستخدمين في نفس الوقت.
