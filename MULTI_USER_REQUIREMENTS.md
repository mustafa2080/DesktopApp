# تحليل متطلبات الاستخدام المتعدد (Multi-User)
## Multi-User Requirements Analysis

---

## 📋 الملخص التنفيذي

المشروع الحالي **لا يدعم الاستخدام المتعدد بشكل كامل** ويحتاج إلى تطويرات أساسية في:
1. ✅ **نظام الصلاحيات** (موجود ولكن غير مكتمل)
2. ❌ **إدارة الجلسات والأمان**
3. ❌ **معالجة التزامن والـ Concurrency**
4. ❌ **Audit Trail وتتبع العمليات**
5. ❌ **Locking Mechanism** (القفل على السجلات)
6. ⚠️ **Connection Pooling** (موجود في Connection String ولكن يحتاج تحسين)

---

## 1️⃣ نظام الصلاحيات (Permissions System)

### ✅ ما هو موجود حالياً:

```csharp
// الجداول موجودة:
- Users
- Roles  
- Permissions
- RolePermissions
```

### ❌ ما هو ناقص:

#### 1.1 تعريف الصلاحيات (Permissions)
```csharp
// يجب إضافة Enum للصلاحيات
public enum PermissionType
{
    // القراءة
    ViewCustomers,
    ViewSuppliers,
    ViewInvoices,
    ViewReports,
    ViewJournalEntries,
    ViewTrips,
    ViewReservations,
    ViewCashBox,
    ViewBankAccounts,
    
    // الإضافة
    CreateCustomer,
    CreateSupplier,
    CreateInvoice,
    CreateReservation,
    CreateTrip,
    CreateJournalEntry,
    CreateCashTransaction,
    CreateBankTransaction,
    
    // التعديل
    EditCustomer,
    EditSupplier,
    EditInvoice,
    EditReservation,
    EditTrip,
    EditJournalEntry,
    EditCashTransaction,
    EditBankTransaction,
    
    // الحذف
    DeleteCustomer,
    DeleteSupplier,
    DeleteInvoice,
    DeleteReservation,
    DeleteTrip,
    DeleteJournalEntry,
    DeleteCashTransaction,
    DeleteBankTransaction,
    
    // الإجراءات الحساسة
    ApproveInvoice,
    CloseTrip,
    CloseFiscalYear,
    EditClosedPeriod,
    ViewSensitiveReports,
    ManageUsers,
    ManageRoles,
    BackupDatabase,
    RestoreDatabase,
    EditCompanySettings,
    
    // الصلاحيات المالية
    ViewProfitMargins,
    EditPricing,
    ApproveDiscounts,
    ManageCreditLimits,
    
    // التصدير والطباعة
    ExportData,
    PrintReports
}
```

#### 1.2 Permission Attribute للتحكم في الفورم
```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiresPermissionAttribute : Attribute
{
    public PermissionType[] Permissions { get; }
    
    public RequiresPermissionAttribute(params PermissionType[] permissions)
    {
        Permissions = permissions;
    }
}

// مثال استخدام:
[RequiresPermission(PermissionType.ViewCustomers)]
public class CustomersListForm : Form
{
    // ...
}
```

#### 1.3 Permission Service
```csharp
public interface IPermissionService
{
    Task<bool> HasPermissionAsync(int userId, PermissionType permission);
    Task<bool> HasAnyPermissionAsync(int userId, params PermissionType[] permissions);
    Task<bool> HasAllPermissionsAsync(int userId, params PermissionType[] permissions);
    Task<List<PermissionType>> GetUserPermissionsAsync(int userId);
}
```

#### 1.4 Form Access Control
```csharp
public abstract class SecureBaseForm : Form
{
    protected IPermissionService PermissionService;
    protected int CurrentUserId;
    
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        
        // Check permissions
        var attr = GetType().GetCustomAttribute<RequiresPermissionAttribute>();
        if (attr != null)
        {
            foreach (var permission in attr.Permissions)
            {
                if (!PermissionService.HasPermissionAsync(CurrentUserId, permission).Result)
                {
                    MessageBox.Show("ليس لديك صلاحية للوصول إلى هذه الصفحة", "خطأ في الصلاحيات", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }
            }
        }
    }
    
    protected void DisableControlsBasedOnPermissions()
    {
        // Example: Disable edit button if no edit permission
        if (!PermissionService.HasPermissionAsync(CurrentUserId, PermissionType.EditCustomer).Result)
        {
            btnEdit.Enabled = false;
        }
    }
}
```

---

## 2️⃣ إدارة الجلسات (Session Management)

### ❌ ما هو ناقص:

#### 2.1 User Session Entity
```csharp
public class UserSession
{
    public int SessionId { get; set; }
    public int UserId { get; set; }
    public string SessionToken { get; set; } = Guid.NewGuid().ToString();
    public string MachineName { get; set; } = Environment.MachineName;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; } = DateTime.UtcNow;
    public DateTime? LastActivityTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public bool IsActive { get; set; } = true;
    public string? LogoutReason { get; set; } // "Manual", "Timeout", "ForcedByAdmin", "ApplicationClosed"
    
    // Navigation
    public User User { get; set; } = null!;
}
```

#### 2.2 Session Service
```csharp
public interface ISessionService
{
    Task<UserSession> CreateSessionAsync(int userId, string ipAddress);
    Task UpdateLastActivityAsync(string sessionToken);
    Task<bool> IsSessionValidAsync(string sessionToken);
    Task EndSessionAsync(string sessionToken, string reason);
    Task<List<UserSession>> GetActiveSessionsAsync();
    Task<List<UserSession>> GetUserActiveSessionsAsync(int userId);
    Task ForceLogoutUserAsync(int userId, string reason);
    Task CleanupExpiredSessionsAsync(); // للجلسات التي انقضى وقتها
}
```

#### 2.3 Session Timeout Configuration
```json
// في appsettings.json
{
  "SessionSettings": {
    "SessionTimeoutMinutes": 60,
    "MaxConcurrentSessions": 3,
    "AllowMultipleSessionsPerUser": true,
    "InactivityTimeoutMinutes": 30
  }
}
```

#### 2.4 Activity Tracking
```csharp
public class UserActivity
{
    public long ActivityId { get; set; }
    public int UserId { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty; // "View", "Create", "Edit", "Delete", "Export", "Print"
    public string EntityType { get; set; } = string.Empty; // "Customer", "Invoice", "Trip", etc.
    public string? EntityId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? OldValue { get; set; } // JSON
    public string? NewValue { get; set; } // JSON
    public string MachineName { get; set; } = Environment.MachineName;
    public string? IpAddress { get; set; }
    
    // Navigation
    public User User { get; set; } = null!;
}
```

---

## 3️⃣ Concurrency Control (التحكم في التزامن)

### ❌ المشكلة الحالية:

عند تعديل نفس السجل من قبل أكثر من مستخدم في نفس الوقت، سيحدث:
- **Lost Update Problem**: آخر تحديث يمسح التحديث السابق
- **Dirty Read**: قراءة بيانات غير محفوظة بعد
- **Non-Repeatable Read**: القراءة مرتين تعطي نتائج مختلفة

### ✅ الحل المطلوب:

#### 3.1 Optimistic Concurrency (التزامن المتفائل)

```csharp
// إضافة RowVersion/Timestamp لكل Entity
public abstract class AuditableEntity
{
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    [Timestamp] // مهم جداً للـ Concurrency
    public byte[]? RowVersion { get; set; }
}

// تطبيق على الـ Entities
public class Customer : AuditableEntity
{
    public int CustomerId { get; set; }
    // ... باقي الخصائص
}

public class Invoice : AuditableEntity
{
    public int InvoiceId { get; set; }
    // ... باقي الخصائص
}
```

#### 3.2 معالجة DbUpdateConcurrencyException

```csharp
public async Task<Result> UpdateCustomerAsync(Customer customer)
{
    try
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
        return Result.Success();
    }
    catch (DbUpdateConcurrencyException ex)
    {
        var entry = ex.Entries.Single();
        var databaseValues = await entry.GetDatabaseValuesAsync();
        
        if (databaseValues == null)
        {
            return Result.Failure("السجل تم حذفه من قبل مستخدم آخر");
        }
        
        var databaseCustomer = (Customer)databaseValues.ToObject();
        
        // عرض Conflict Resolution Dialog
        var resolver = new ConflictResolutionDialog(
            currentValues: customer,
            databaseValues: databaseCustomer,
            userValues: (Customer)entry.Entity
        );
        
        if (resolver.ShowDialog() == DialogResult.OK)
        {
            // المستخدم اختار الحل
            entry.OriginalValues.SetValues(databaseValues);
            return await UpdateCustomerAsync(customer); // إعادة المحاولة
        }
        
        return Result.Failure("تم إلغاء العملية");
    }
}
```

#### 3.3 Pessimistic Locking (القفل المتشائم)

```csharp
public class RecordLock
{
    public int LockId { get; set; }
    public string EntityType { get; set; } = string.Empty; // "Customer", "Invoice", etc.
    public string EntityId { get; set; } = string.Empty;
    public int LockedBy { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public DateTime LockedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; } // قفل مؤقت
    public string? Reason { get; set; } // "Editing", "Viewing"
    
    // Navigation
    public User LockedByUser { get; set; } = null!;
}
```

```csharp
public interface ILockingService
{
    Task<RecordLock?> AcquireLockAsync(string entityType, string entityId, int userId, TimeSpan? duration = null);
    Task<bool> IsLockedAsync(string entityType, string entityId);
    Task<RecordLock?> GetLockAsync(string entityType, string entityId);
    Task ReleaseLockAsync(string entityType, string entityId, int userId);
    Task ReleaseAllUserLocksAsync(int userId);
    Task CleanupExpiredLocksAsync();
}
```

```csharp
// مثال استخدام في الفورم
public class EditCustomerForm : Form
{
    private RecordLock? _lock;
    
    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        
        // محاولة أخذ قفل على السجل
        _lock = await _lockingService.AcquireLockAsync("Customer", _customerId.ToString(), _currentUserId);
        
        if (_lock == null)
        {
            var existingLock = await _lockingService.GetLockAsync("Customer", _customerId.ToString());
            
            MessageBox.Show(
                $"هذا السجل قيد التعديل حالياً بواسطة: {existingLock.LockedByUser.FullName}\n" +
                $"منذ: {existingLock.LockedAt:yyyy-MM-dd HH:mm}",
                "السجل مقفل",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            
            this.Close();
            return;
        }
        
        // تحميل البيانات
        LoadCustomerData();
    }
    
    protected override async void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        
        // تحرير القفل
        if (_lock != null)
        {
            await _lockingService.ReleaseLockAsync("Customer", _customerId.ToString(), _currentUserId);
        }
    }
}
```

---

## 4️⃣ Audit Trail (تتبع التغييرات)

### ❌ ما هو ناقص:

```csharp
public class AuditLog
{
    public long AuditId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // "INSERT", "UPDATE", "DELETE"
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? ChangedFields { get; set; } // CSV: "Name,Email,Phone"
    public string MachineName { get; set; } = Environment.MachineName;
    public string? IpAddress { get; set; }
    public string? SessionToken { get; set; }
    
    // Navigation
    public User User { get; set; } = null!;
}
```

```csharp
// Automatic Audit Trail في DbContext
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var entries = ChangeTracker.Entries()
        .Where(e => e.State == EntityState.Added || 
                    e.State == EntityState.Modified || 
                    e.State == EntityState.Deleted)
        .ToList();
    
    foreach (var entry in entries)
    {
        var auditLog = new AuditLog
        {
            UserId = _currentUserId,
            Username = _currentUsername,
            Action = entry.State.ToString().ToUpper(),
            EntityType = entry.Entity.GetType().Name,
            Timestamp = DateTime.UtcNow,
            MachineName = Environment.MachineName,
            SessionToken = _sessionToken
        };
        
        if (entry.State == EntityState.Modified)
        {
            var oldValues = new Dictionary<string, object>();
            var newValues = new Dictionary<string, object>();
            var changedFields = new List<string>();
            
            foreach (var property in entry.Properties)
            {
                if (property.IsModified)
                {
                    oldValues[property.Metadata.Name] = property.OriginalValue;
                    newValues[property.Metadata.Name] = property.CurrentValue;
                    changedFields.Add(property.Metadata.Name);
                }
            }
            
            auditLog.OldValues = JsonSerializer.Serialize(oldValues);
            auditLog.NewValues = JsonSerializer.Serialize(newValues);
            auditLog.ChangedFields = string.Join(",", changedFields);
        }
        else if (entry.State == EntityState.Added)
        {
            var values = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
            auditLog.NewValues = JsonSerializer.Serialize(values);
        }
        else if (entry.State == EntityState.Deleted)
        {
            var values = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
            auditLog.OldValues = JsonSerializer.Serialize(values);
        }
        
        AuditLogs.Add(auditLog);
    }
    
    return await base.SaveChangesAsync(cancellationToken);
}
```

---

## 5️⃣ Connection Pool Management

### ⚠️ الوضع الحالي:

```
MinPoolSize=5
MaxPoolSize=50
```

### ✅ التوصيات:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=graceway_accounting;Username=postgres;Password=123456;Timeout=60;Command Timeout=60;Pooling=true;MinPoolSize=10;MaxPoolSize=100;Connection Idle Lifetime=300;Connection Pruning Interval=10;Keepalive=30"
  }
}
```

**ملاحظات:**
- `MinPoolSize=10`: حد أدنى من الاتصالات دائماً متاحة
- `MaxPoolSize=100`: يسمح بـ 100 مستخدم متزامن
- `Keepalive=30`: فحص الاتصالات كل 30 ثانية
- `Connection Idle Lifetime=300`: إغلاق الاتصالات الخاملة بعد 5 دقائق

---

## 6️⃣ Error Handling & Notification System

### ❌ ما هو ناقص:

#### 6.1 Central Error Handler
```csharp
public static class GlobalErrorHandler
{
    public static void Initialize()
    {
        Application.ThreadException += OnThreadException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }
    
    private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
    {
        LogError(e.Exception);
        ShowErrorDialog(e.Exception);
    }
    
    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogError(ex);
            ShowErrorDialog(ex);
        }
    }
    
    private static void LogError(Exception ex)
    {
        // Log to database
        var errorLog = new ErrorLog
        {
            ErrorMessage = ex.Message,
            StackTrace = ex.StackTrace,
            Source = ex.Source,
            Timestamp = DateTime.UtcNow,
            UserId = CurrentSession.UserId,
            MachineName = Environment.MachineName
        };
        
        // Save to DB asynchronously
    }
}
```

#### 6.2 User Notification System
```csharp
public class UserNotification
{
    public int NotificationId { get; set; }
    public int? UserId { get; set; } // null = broadcast to all
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty; // "Info", "Warning", "Error", "SystemMessage"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public string? ActionUrl { get; set; } // للتنقل إلى صفحة معينة
}
```

```csharp
// مثال: إشعار عند بدء عملية backup
await _notificationService.BroadcastAsync(
    "جاري عمل نسخة احتياطية",
    "سيتم إيقاف النظام لمدة دقيقتين",
    "SystemMessage"
);
```

---

## 7️⃣ Data Validation & Business Rules

### ❌ ما هو ناقص:

#### 7.1 Concurrent Transaction Validation

```csharp
public class TransactionValidator
{
    // مثال: منع إغلاق فترة مالية أثناء إدخال قيود
    public async Task<ValidationResult> ValidateFiscalYearCloseAsync(int fiscalYearId)
    {
        var openTransactions = await _context.JournalEntries
            .Where(j => j.FiscalYearId == fiscalYearId && j.Status == "Draft")
            .CountAsync();
        
        if (openTransactions > 0)
        {
            return ValidationResult.Failure(
                $"يوجد {openTransactions} قيد مفتوح. يجب اعتماد جميع القيود أولاً."
            );
        }
        
        // التحقق من عدم وجود مستخدمين يعملون حالياً
        var activeEdits = await _lockingService.GetLocksForEntityTypeAsync("JournalEntry");
        if (activeEdits.Any())
        {
            return ValidationResult.Failure(
                "يوجد مستخدمون يقومون بتعديل قيود حالياً. يرجى الانتظار."
            );
        }
        
        return ValidationResult.Success();
    }
}
```

---

## 8️⃣ Performance Optimization

### ✅ مطلوب تطبيقه:

#### 8.1 Caching Strategy
```csharp
public interface ICachingService
{
    T? Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan? expiration = null);
    void Remove(string key);
    void Clear();
}

// استخدام في Services
public class AccountService
{
    public async Task<List<Account>> GetChartOfAccountsAsync()
    {
        var cacheKey = "chart_of_accounts";
        var cached = _cachingService.Get<List<Account>>(cacheKey);
        
        if (cached != null)
            return cached;
        
        var accounts = await _context.Accounts.ToListAsync();
        _cachingService.Set(cacheKey, accounts, TimeSpan.FromHours(1));
        
        return accounts;
    }
}
```

#### 8.2 Lazy Loading Configuration
```csharp
// في AppDbContext
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder
        .UseLazyLoadingProxies(false) // تعطيل Lazy Loading
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking); // لتحسين الأداء
}
```

#### 8.3 Bulk Operations
```csharp
// استخدام EFCore.BulkExtensions
public async Task ImportCustomersAsync(List<Customer> customers)
{
    await _context.BulkInsertAsync(customers);
}
```

---

## 9️⃣ Database Maintenance

### ✅ مطلوب إضافته:

#### 9.1 Background Jobs

```csharp
public class MaintenanceService
{
    private Timer _cleanupTimer;
    
    public void Start()
    {
        // تنظيف كل ساعة
        _cleanupTimer = new Timer(async _ => await PerformCleanupAsync(), null, TimeSpan.Zero, TimeSpan.FromHours(1));
    }
    
    private async Task PerformCleanupAsync()
    {
        // 1. حذف الجلسات المنتهية
        await _sessionService.CleanupExpiredSessionsAsync();
        
        // 2. حذف الأقفال المنتهية
        await _lockingService.CleanupExpiredLocksAsync();
        
        // 3. أرشفة السجلات القديمة
        await ArchiveOldAuditLogsAsync();
        
        // 4. تحديث الإحصائيات
        await UpdateStatisticsAsync();
    }
    
    private async Task ArchiveOldAuditLogsAsync()
    {
        var cutoffDate = DateTime.UtcNow.AddMonths(-6);
        var oldLogs = await _context.AuditLogs
            .Where(a => a.Timestamp < cutoffDate)
            .ToListAsync();
        
        // نقل إلى جدول أرشيف أو ملف خارجي
    }
}
```

---

## 🔟 Security Enhancements

### ❌ ما هو ناقص:

#### 10.1 Password Policy
```csharp
public class PasswordPolicy
{
    public int MinLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialChar { get; set; } = true;
    public int PasswordExpiryDays { get; set; } = 90;
    public int PasswordHistoryCount { get; set; } = 5; // منع إعادة استخدام آخر 5 كلمات مرور
}
```

#### 10.2 Failed Login Attempts
```csharp
public class FailedLoginAttempt
{
    public int AttemptId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime AttemptTime { get; set; } = DateTime.UtcNow;
    public string? Reason { get; set; } // "WrongPassword", "UserDisabled", "AccountLocked"
}

// Lockout بعد 3 محاولات فاشلة
public async Task<LoginResult> LoginAsync(string username, string password)
{
    var failedAttempts = await _context.FailedLoginAttempts
        .Where(f => f.Username == username && f.AttemptTime > DateTime.UtcNow.AddMinutes(-15))
        .CountAsync();
    
    if (failedAttempts >= 3)
    {
        return LoginResult.AccountLocked("الحساب مقفل لمدة 15 دقيقة بسبب محاولات دخول فاشلة");
    }
    
    // ... باقي منطق تسجيل الدخول
}
```

---

## 📊 أولويات التنفيذ (Priority Order)

### 🔴 High Priority (يجب تنفيذها فوراً):

1. **Optimistic Concurrency** (RowVersion)
2. **User Sessions Management**
3. **Basic Permissions System**
4. **Audit Trail**
5. **Record Locking**

### 🟡 Medium Priority (مهمة ولكن يمكن تأجيلها):

6. **Activity Tracking**
7. **Conflict Resolution UI**
8. **Notification System**
9. **Caching**
10. **Password Policy**

### 🟢 Low Priority (تحسينات إضافية):

11. **Advanced Reporting on User Activity**
12. **Performance Monitoring Dashboard**
13. **Automated Database Maintenance**

---

## 💾 Database Schema Changes Required

### الجداول الجديدة المطلوبة:

```sql
-- 1. User Sessions
CREATE TABLE user_sessions (
    session_id SERIAL PRIMARY KEY,
    user_id INT NOT NULL REFERENCES users(user_id),
    session_token VARCHAR(255) UNIQUE NOT NULL,
    machine_name VARCHAR(255),
    ip_address VARCHAR(50),
    login_time TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    last_activity_time TIMESTAMP WITH TIME ZONE,
    logout_time TIMESTAMP WITH TIME ZONE,
    is_active BOOLEAN DEFAULT TRUE,
    logout_reason VARCHAR(100)
);

-- 2. Record Locks
CREATE TABLE record_locks (
    lock_id SERIAL PRIMARY KEY,
    entity_type VARCHAR(100) NOT NULL,
    entity_id VARCHAR(100) NOT NULL,
    locked_by INT NOT NULL REFERENCES users(user_id),
    session_token VARCHAR(255) NOT NULL,
    locked_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    expires_at TIMESTAMP WITH TIME ZONE,
    reason VARCHAR(100),
    UNIQUE(entity_type, entity_id)
);

-- 3. Audit Logs
CREATE TABLE audit_logs (
    audit_id BIGSERIAL PRIMARY KEY,
    user_id INT NOT NULL REFERENCES users(user_id),
    username VARCHAR(100) NOT NULL,
    action VARCHAR(20) NOT NULL, -- INSERT, UPDATE, DELETE
    entity_type VARCHAR(100) NOT NULL,
    entity_id VARCHAR(100),
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    old_values TEXT, -- JSON
    new_values TEXT, -- JSON
    changed_fields TEXT,
    machine_name VARCHAR(255),
    ip_address VARCHAR(50),
    session_token VARCHAR(255)
);

CREATE INDEX idx_audit_logs_user_id ON audit_logs(user_id);
CREATE INDEX idx_audit_logs_timestamp ON audit_logs(timestamp);
CREATE INDEX idx_audit_logs_entity ON audit_logs(entity_type, entity_id);

-- 4. User Activities
CREATE TABLE user_activities (
    activity_id BIGSERIAL PRIMARY KEY,
    user_id INT NOT NULL REFERENCES users(user_id),
    session_token VARCHAR(255),
    activity_type VARCHAR(50) NOT NULL, -- View, Create, Edit, Delete, Export, Print
    entity_type VARCHAR(100),
    entity_id VARCHAR(100),
    description TEXT,
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    old_value TEXT,
    new_value TEXT,
    machine_name VARCHAR(255),
    ip_address VARCHAR(50)
);

-- 5. User Notifications
CREATE TABLE user_notifications (
    notification_id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id), -- NULL for broadcast
    title VARCHAR(255) NOT NULL,
    message TEXT NOT NULL,
    notification_type VARCHAR(50) DEFAULT 'Info',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    is_read BOOLEAN DEFAULT FALSE,
    read_at TIMESTAMP WITH TIME ZONE,
    action_url VARCHAR(500)
);

-- 6. Failed Login Attempts
CREATE TABLE failed_login_attempts (
    attempt_id SERIAL PRIMARY KEY,
    username VARCHAR(100) NOT NULL,
    ip_address VARCHAR(50),
    attempt_time TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    reason VARCHAR(100)
);

-- 7. Password History (لمنع إعادة استخدام كلمات المرور القديمة)
CREATE TABLE password_history (
    history_id SERIAL PRIMARY KEY,
    user_id INT NOT NULL REFERENCES users(user_id),
    password_hash VARCHAR(255) NOT NULL,
    changed_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- 8. Error Logs
CREATE TABLE error_logs (
    error_id BIGSERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id),
    error_message TEXT NOT NULL,
    stack_trace TEXT,
    source VARCHAR(255),
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    machine_name VARCHAR(255),
    session_token VARCHAR(255)
);
```

### تعديلات على الجداول الموجودة:

```sql
-- إضافة RowVersion لجميع الجداول الرئيسية
ALTER TABLE customers ADD COLUMN row_version BYTEA;
ALTER TABLE suppliers ADD COLUMN row_version BYTEA;
ALTER TABLE sales_invoices ADD COLUMN row_version BYTEA;
ALTER TABLE purchase_invoices ADD COLUMN row_version BYTEA;
ALTER TABLE journal_entries ADD COLUMN row_version BYTEA;
ALTER TABLE cash_transactions ADD COLUMN row_version BYTEA;
ALTER TABLE trips ADD COLUMN row_version BYTEA;
ALTER TABLE reservations ADD COLUMN row_version BYTEA;

-- إضافة Created/Updated By
ALTER TABLE customers ADD COLUMN created_by INT REFERENCES users(user_id);
ALTER TABLE customers ADD COLUMN updated_by INT REFERENCES users(user_id);
-- ... وهكذا لجميع الجداول
```

---

## 📝 الخلاصة والتوصيات

### ✅ الإجراءات الفورية المطلوبة:

1. **إضافة RowVersion** لجميع الجداول الرئيسية
2. **تطبيق Session Management**
3. **إنشاء نظام الصلاحيات الكامل**
4. **تفعيل Audit Trail**
5. **إنشاء Record Locking System**

### 💡 نصائح إضافية:

- استخدام **Transactions** في جميع العمليات المعقدة
- تطبيق **Retry Logic** للتعامل مع Deadlocks
- إنشاء **Health Check Dashboard** لمراقبة أداء النظام
- توثيق جميع العمليات الحساسة
- إجراء **Load Testing** قبل الإطلاق

---

## 📞 للمزيد من المعلومات

إذا كنت بحاجة لتفاصيل التنفيذ لأي من النقاط أعلاه، يمكنني توفير:
- Code samples كاملة
- Migration scripts
- Unit tests
- Integration examples

**ملاحظة مهمة:** يُنصح بشدة بتطبيق هذه التحسينات على مراحل وإجراء اختبارات شاملة في بيئة تطوير قبل النشر على الإنتاج.

---

**تاريخ التحليل:** 2026-02-09  
**الإصدار:** 1.0  
**الحالة:** قيد المراجعة
