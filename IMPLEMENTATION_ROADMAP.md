# خطة تنفيذ الاستخدام المتعدد - مرحلية
## Multi-User Implementation Roadmap

---

## 📅 المرحلة 1: الأساسيات (Week 1-2)

### Day 1-3: Concurrency Control

#### 1. إضافة RowVersion للـ Entities

```csharp
// في Domain/Entities/BaseEntity.cs
public abstract class AuditableEntity
{
    public int CreatedBy { get; set; }
    
    [Column("CreatedAt", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? UpdatedBy { get; set; }
    
    [Column("UpdatedAt", TypeName = "timestamp with time zone")]
    public DateTime? UpdatedAt { get; set; }
    
    [Timestamp]
    public byte[]? RowVersion { get; set; }
}
```

#### 2. تعديل الـ Entities الرئيسية

```csharp
// مثال: Customer.cs
public class Customer : AuditableEntity
{
    [Key]
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    // ... باقي الخصائص
}
```

#### 3. Migration للتغييرات

```bash
dotnet ef migrations add AddConcurrencySupport
dotnet ef database update
```

#### 4. معالجة Concurrency Exceptions

```csharp
// في Application/Services/BaseService.cs
public abstract class BaseService
{
    protected async Task<Result<T>> HandleConcurrencyAsync<T>(
        Func<Task<T>> operation, 
        string entityName = "السجل")
    {
        try
        {
            var result = await operation();
            return Result<T>.Success(result);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry = ex.Entries.FirstOrDefault();
            if (entry != null)
            {
                var databaseValues = await entry.GetDatabaseValuesAsync();
                
                if (databaseValues == null)
                {
                    return Result<T>.Failure($"{entityName} تم حذفه من قبل مستخدم آخر");
                }
                
                return Result<T>.Failure(
                    $"{entityName} تم تعديله من قبل مستخدم آخر. يرجى تحديث البيانات والمحاولة مرة أخرى");
            }
            
            return Result<T>.Failure("حدث خطأ في التزامن");
        }
    }
}
```

**Deliverables:**
- ✅ جميع الـ Entities ترث من AuditableEntity
- ✅ Migration مطبقة على قاعدة البيانات
- ✅ معالجة Concurrency Exceptions في BaseService

---

### Day 4-5: Session Management

#### 1. إنشاء UserSession Entity

```csharp
// في Domain/Entities/UserSession.cs
public class UserSession
{
    [Key]
    public int SessionId { get; set; }
    
    public int UserId { get; set; }
    
    [MaxLength(255)]
    public string SessionToken { get; set; } = Guid.NewGuid().ToString();
    
    [MaxLength(255)]
    public string MachineName { get; set; } = Environment.MachineName;
    
    [MaxLength(50)]
    public string? IpAddress { get; set; }
    
    [Column("LoginTime", TypeName = "timestamp with time zone")]
    public DateTime LoginTime { get; set; } = DateTime.UtcNow;
    
    [Column("LastActivityTime", TypeName = "timestamp with time zone")]
    public DateTime? LastActivityTime { get; set; }
    
    [Column("LogoutTime", TypeName = "timestamp with time zone")]
    public DateTime? LogoutTime { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    [MaxLength(100)]
    public string? LogoutReason { get; set; }
    
    // Navigation
    public User User { get; set; } = null!;
}
```

#### 2. إنشاء SessionService

```csharp
// في Application/Services/SessionService.cs
public interface ISessionService
{
    Task<UserSession> CreateSessionAsync(int userId, string ipAddress);
    Task UpdateLastActivityAsync(string sessionToken);
    Task<bool> IsSessionValidAsync(string sessionToken);
    Task EndSessionAsync(string sessionToken, string reason);
    Task<List<UserSession>> GetActiveSessionsAsync();
    Task CleanupExpiredSessionsAsync();
}

public class SessionService : ISessionService
{
    private readonly AppDbContext _context;
    private readonly int _sessionTimeoutMinutes = 60;
    
    public SessionService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<UserSession> CreateSessionAsync(int userId, string ipAddress)
    {
        var session = new UserSession
        {
            UserId = userId,
            SessionToken = Guid.NewGuid().ToString(),
            IpAddress = ipAddress,
            MachineName = Environment.MachineName,
            LoginTime = DateTime.UtcNow,
            LastActivityTime = DateTime.UtcNow,
            IsActive = true
        };
        
        _context.UserSessions.Add(session);
        await _context.SaveChangesAsync();
        
        return session;
    }
    
    public async Task UpdateLastActivityAsync(string sessionToken)
    {
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && s.IsActive);
        
        if (session != null)
        {
            session.LastActivityTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<bool> IsSessionValidAsync(string sessionToken)
    {
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && s.IsActive);
        
        if (session == null)
            return false;
        
        // Check timeout
        if (session.LastActivityTime.HasValue)
        {
            var inactive = DateTime.UtcNow - session.LastActivityTime.Value;
            if (inactive.TotalMinutes > _sessionTimeoutMinutes)
            {
                await EndSessionAsync(sessionToken, "Timeout");
                return false;
            }
        }
        
        return true;
    }
    
    public async Task EndSessionAsync(string sessionToken, string reason)
    {
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && s.IsActive);
        
        if (session != null)
        {
            session.IsActive = false;
            session.LogoutTime = DateTime.UtcNow;
            session.LogoutReason = reason;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<List<UserSession>> GetActiveSessionsAsync()
    {
        return await _context.UserSessions
            .Include(s => s.User)
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.LoginTime)
            .ToListAsync();
    }
    
    public async Task CleanupExpiredSessionsAsync()
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-_sessionTimeoutMinutes);
        
        var expiredSessions = await _context.UserSessions
            .Where(s => s.IsActive && s.LastActivityTime < cutoffTime)
            .ToListAsync();
        
        foreach (var session in expiredSessions)
        {
            session.IsActive = false;
            session.LogoutTime = DateTime.UtcNow;
            session.LogoutReason = "Timeout";
        }
        
        await _context.SaveChangesAsync();
    }
}
```

#### 3. تعديل LoginForm

```csharp
// في Presentation/Forms/LoginForm.cs
public partial class LoginForm : Form
{
    private readonly IAuthService _authService;
    private readonly ISessionService _sessionService;
    
    private async void btnLogin_Click(object sender, EventArgs e)
    {
        // ... التحقق من اسم المستخدم وكلمة المرور
        
        var user = await _authService.AuthenticateAsync(username, password);
        
        if (user != null)
        {
            // إنشاء جلسة
            var session = await _sessionService.CreateSessionAsync(
                user.UserId, 
                GetLocalIPAddress()
            );
            
            // حفظ session token في application state
            ApplicationState.CurrentSession = session;
            ApplicationState.CurrentUser = user;
            
            // فتح الفورم الرئيسية
            var mainForm = new MainForm();
            mainForm.Show();
            this.Hide();
        }
    }
    
    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1";
    }
}
```

#### 4. Activity Tracker

```csharp
// في Application/Services/ActivityTracker.cs
public class ActivityTracker
{
    private readonly ISessionService _sessionService;
    private System.Threading.Timer _timer;
    
    public void Start()
    {
        // Update activity every 2 minutes
        _timer = new System.Threading.Timer(
            async _ => await UpdateActivityAsync(),
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(2)
        );
    }
    
    private async Task UpdateActivityAsync()
    {
        if (ApplicationState.CurrentSession != null)
        {
            await _sessionService.UpdateLastActivityAsync(
                ApplicationState.CurrentSession.SessionToken
            );
        }
    }
    
    public void Stop()
    {
        _timer?.Dispose();
    }
}
```

**Deliverables:**
- ✅ UserSession entity و migration
- ✅ SessionService كامل
- ✅ تعديل LoginForm لإنشاء جلسات
- ✅ Activity Tracker للتحديث التلقائي

---

### Day 6-7: Record Locking

#### 1. RecordLock Entity

```csharp
// في Domain/Entities/RecordLock.cs
public class RecordLock
{
    [Key]
    public int LockId { get; set; }
    
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string EntityId { get; set; } = string.Empty;
    
    public int LockedBy { get; set; }
    
    [MaxLength(255)]
    public string SessionToken { get; set; } = string.Empty;
    
    [Column("LockedAt", TypeName = "timestamp with time zone")]
    public DateTime LockedAt { get; set; } = DateTime.UtcNow;
    
    [Column("ExpiresAt", TypeName = "timestamp with time zone")]
    public DateTime? ExpiresAt { get; set; }
    
    [MaxLength(100)]
    public string? Reason { get; set; }
    
    // Navigation
    public User LockedByUser { get; set; } = null!;
}
```

#### 2. LockingService

```csharp
// في Application/Services/LockingService.cs
public interface ILockingService
{
    Task<RecordLock?> AcquireLockAsync(string entityType, string entityId, int userId, TimeSpan? duration = null);
    Task<bool> IsLockedAsync(string entityType, string entityId);
    Task<RecordLock?> GetLockAsync(string entityType, string entityId);
    Task ReleaseLockAsync(string entityType, string entityId, int userId);
    Task ReleaseAllUserLocksAsync(int userId);
    Task CleanupExpiredLocksAsync();
}

public class LockingService : ILockingService
{
    private readonly AppDbContext _context;
    
    public LockingService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<RecordLock?> AcquireLockAsync(
        string entityType, 
        string entityId, 
        int userId, 
        TimeSpan? duration = null)
    {
        // Check if already locked
        var existingLock = await GetLockAsync(entityType, entityId);
        if (existingLock != null)
        {
            // Check if lock expired
            if (existingLock.ExpiresAt.HasValue && existingLock.ExpiresAt.Value < DateTime.UtcNow)
            {
                // Remove expired lock
                _context.RecordLocks.Remove(existingLock);
            }
            else
            {
                return null; // Already locked
            }
        }
        
        var newLock = new RecordLock
        {
            EntityType = entityType,
            EntityId = entityId,
            LockedBy = userId,
            SessionToken = ApplicationState.CurrentSession?.SessionToken ?? "",
            LockedAt = DateTime.UtcNow,
            ExpiresAt = duration.HasValue ? DateTime.UtcNow.Add(duration.Value) : null,
            Reason = "Editing"
        };
        
        _context.RecordLocks.Add(newLock);
        
        try
        {
            await _context.SaveChangesAsync();
            return newLock;
        }
        catch
        {
            // Concurrency issue - someone else acquired lock
            return null;
        }
    }
    
    public async Task<bool> IsLockedAsync(string entityType, string entityId)
    {
        var lock_ = await GetLockAsync(entityType, entityId);
        return lock_ != null;
    }
    
    public async Task<RecordLock?> GetLockAsync(string entityType, string entityId)
    {
        var lock_ = await _context.RecordLocks
            .Include(l => l.LockedByUser)
            .FirstOrDefaultAsync(l => 
                l.EntityType == entityType && 
                l.EntityId == entityId &&
                (!l.ExpiresAt.HasValue || l.ExpiresAt.Value > DateTime.UtcNow)
            );
        
        return lock_;
    }
    
    public async Task ReleaseLockAsync(string entityType, string entityId, int userId)
    {
        var lock_ = await _context.RecordLocks
            .FirstOrDefaultAsync(l => 
                l.EntityType == entityType && 
                l.EntityId == entityId &&
                l.LockedBy == userId
            );
        
        if (lock_ != null)
        {
            _context.RecordLocks.Remove(lock_);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task ReleaseAllUserLocksAsync(int userId)
    {
        var locks = await _context.RecordLocks
            .Where(l => l.LockedBy == userId)
            .ToListAsync();
        
        _context.RecordLocks.RemoveRange(locks);
        await _context.SaveChangesAsync();
    }
    
    public async Task CleanupExpiredLocksAsync()
    {
        var expiredLocks = await _context.RecordLocks
            .Where(l => l.ExpiresAt.HasValue && l.ExpiresAt.Value < DateTime.UtcNow)
            .ToListAsync();
        
        _context.RecordLocks.RemoveRange(expiredLocks);
        await _context.SaveChangesAsync();
    }
}
```

#### 3. LockableForm Base Class

```csharp
// في Presentation/Forms/LockableForm.cs
public abstract class LockableForm : Form
{
    protected ILockingService LockingService;
    protected RecordLock? CurrentLock;
    protected abstract string EntityType { get; }
    protected abstract string EntityId { get; }
    
    protected async Task<bool> TryAcquireLockAsync()
    {
        CurrentLock = await LockingService.AcquireLockAsync(
            EntityType,
            EntityId,
            ApplicationState.CurrentUser.UserId,
            TimeSpan.FromMinutes(30) // Lock expires after 30 minutes
        );
        
        if (CurrentLock == null)
        {
            var existingLock = await LockingService.GetLockAsync(EntityType, EntityId);
            
            if (existingLock != null)
            {
                MessageBox.Show(
                    $"هذا السجل قيد التعديل حالياً بواسطة:\n\n" +
                    $"المستخدم: {existingLock.LockedByUser.FullName}\n" +
                    $"منذ: {existingLock.LockedAt.ToLocalTime():yyyy-MM-dd HH:mm}\n\n" +
                    $"يرجى المحاولة لاحقاً.",
                    "السجل مقفل",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            
            return false;
        }
        
        return true;
    }
    
    protected override async void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        
        if (CurrentLock != null)
        {
            await LockingService.ReleaseLockAsync(
                EntityType,
                EntityId,
                ApplicationState.CurrentUser.UserId
            );
        }
    }
}
```

#### 4. مثال استخدام في EditCustomerForm

```csharp
public class EditCustomerForm : LockableForm
{
    private int _customerId;
    
    protected override string EntityType => "Customer";
    protected override string EntityId => _customerId.ToString();
    
    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        
        // محاولة أخذ قفل
        if (!await TryAcquireLockAsync())
        {
            this.Close();
            return;
        }
        
        // تحميل البيانات
        await LoadCustomerDataAsync();
    }
}
```

**Deliverables:**
- ✅ RecordLock entity و migration
- ✅ LockingService كامل
- ✅ LockableForm base class
- ✅ تطبيق على EditCustomerForm كمثال

---

### Day 8-10: Permissions System

#### 1. PermissionType Enum

```csharp
// في Domain/Enums/PermissionType.cs
public enum PermissionType
{
    // Customers
    ViewCustomers = 1,
    CreateCustomer = 2,
    EditCustomer = 3,
    DeleteCustomer = 4,
    
    // Suppliers
    ViewSuppliers = 10,
    CreateSupplier = 11,
    EditSupplier = 12,
    DeleteSupplier = 13,
    
    // Invoices
    ViewInvoices = 20,
    CreateInvoice = 21,
    EditInvoice = 22,
    DeleteInvoice = 23,
    ApproveInvoice = 24,
    
    // Reports
    ViewReports = 30,
    ViewProfitMargins = 31,
    ViewSensitiveReports = 32,
    ExportData = 33,
    
    // Journal Entries
    ViewJournalEntries = 40,
    CreateJournalEntry = 41,
    EditJournalEntry = 42,
    DeleteJournalEntry = 43,
    EditClosedPeriod = 44,
    
    // Cash Management
    ViewCashBox = 50,
    CreateCashTransaction = 51,
    EditCashTransaction = 52,
    DeleteCashTransaction = 53,
    
    // Bank Management
    ViewBankAccounts = 60,
    CreateBankTransaction = 61,
    EditBankTransaction = 62,
    DeleteBankTransaction = 63,
    
    // Trips
    ViewTrips = 70,
    CreateTrip = 71,
    EditTrip = 72,
    DeleteTrip = 73,
    CloseTrip = 74,
    
    // System
    ManageUsers = 100,
    ManageRoles = 101,
    ManageSettings = 102,
    BackupDatabase = 103,
    RestoreDatabase = 104,
    ViewAuditLogs = 105
}
```

#### 2. Update Permission Entity

```csharp
public class Permission
{
    [Key]
    public int PermissionId { get; set; }
    
    public PermissionType PermissionType { get; set; }
    
    [MaxLength(100)]
    public string PermissionName { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty; // "Customers", "Invoices", "System", etc.
    
    // Navigation
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
```

#### 3. PermissionService

```csharp
// في Application/Services/PermissionService.cs
public interface IPermissionService
{
    Task<bool> HasPermissionAsync(int userId, PermissionType permission);
    Task<bool> HasAnyPermissionAsync(int userId, params PermissionType[] permissions);
    Task<bool> HasAllPermissionsAsync(int userId, params PermissionType[] permissions);
    Task<List<PermissionType>> GetUserPermissionsAsync(int userId);
    Task<Dictionary<string, List<PermissionType>>> GetUserPermissionsByCategoryAsync(int userId);
}

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;
    private readonly Dictionary<int, List<PermissionType>> _cache = new();
    
    public PermissionService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<bool> HasPermissionAsync(int userId, PermissionType permission)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        return permissions.Contains(permission);
    }
    
    public async Task<bool> HasAnyPermissionAsync(int userId, params PermissionType[] permissions)
    {
        var userPermissions = await GetUserPermissionsAsync(userId);
        return permissions.Any(p => userPermissions.Contains(p));
    }
    
    public async Task<bool> HasAllPermissionsAsync(int userId, params PermissionType[] permissions)
    {
        var userPermissions = await GetUserPermissionsAsync(userId);
        return permissions.All(p => userPermissions.Contains(p));
    }
    
    public async Task<List<PermissionType>> GetUserPermissionsAsync(int userId)
    {
        // Check cache
        if (_cache.TryGetValue(userId, out var cached))
        {
            return cached;
        }
        
        var user = await _context.Users
            .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.UserId == userId);
        
        if (user == null || user.Role == null)
        {
            return new List<PermissionType>();
        }
        
        var permissions = user.Role.RolePermissions
            .Select(rp => rp.Permission.PermissionType)
            .ToList();
        
        // Cache for 5 minutes
        _cache[userId] = permissions;
        Task.Delay(TimeSpan.FromMinutes(5)).ContinueWith(_ => _cache.Remove(userId));
        
        return permissions;
    }
    
    public async Task<Dictionary<string, List<PermissionType>>> GetUserPermissionsByCategoryAsync(int userId)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        
        var permissionDetails = await _context.Permissions
            .Where(p => permissions.Contains(p.PermissionType))
            .ToListAsync();
        
        return permissionDetails
            .GroupBy(p => p.Category)
            .ToDictionary(
                g => g.Key,
                g => g.Select(p => p.PermissionType).ToList()
            );
    }
}
```

#### 4. Seed Permissions

```csharp
// في Infrastructure/Data/PermissionSeeder.cs
public static class PermissionSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Permissions.AnyAsync())
            return;
        
        var permissions = new List<Permission>
        {
            // Customers
            new() { PermissionType = PermissionType.ViewCustomers, PermissionName = "عرض العملاء", Category = "Customers" },
            new() { PermissionType = PermissionType.CreateCustomer, PermissionName = "إضافة عميل", Category = "Customers" },
            new() { PermissionType = PermissionType.EditCustomer, PermissionName = "تعديل عميل", Category = "Customers" },
            new() { PermissionType = PermissionType.DeleteCustomer, PermissionName = "حذف عميل", Category = "Customers" },
            
            // Suppliers
            new() { PermissionType = PermissionType.ViewSuppliers, PermissionName = "عرض الموردين", Category = "Suppliers" },
            new() { PermissionType = PermissionType.CreateSupplier, PermissionName = "إضافة مورد", Category = "Suppliers" },
            // ... باقي الصلاحيات
        };
        
        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();
    }
}
```

#### 5. RequiresPermission Attribute

```csharp
// في Presentation/Attributes/RequiresPermissionAttribute.cs
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiresPermissionAttribute : Attribute
{
    public PermissionType[] Permissions { get; }
    public bool RequireAll { get; set; } = false; // If true, needs ALL permissions. If false, needs ANY
    
    public RequiresPermissionAttribute(params PermissionType[] permissions)
    {
        Permissions = permissions;
    }
}
```

#### 6. SecureBaseForm

```csharp
// في Presentation/Forms/SecureBaseForm.cs
public abstract class SecureBaseForm : Form
{
    protected IPermissionService PermissionService;
    
    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        
        if (!await CheckPermissionsAsync())
        {
            this.Close();
            return;
        }
        
        await DisableControlsBasedOnPermissionsAsync();
    }
    
    private async Task<bool> CheckPermissionsAsync()
    {
        var attr = GetType().GetCustomAttribute<RequiresPermissionAttribute>();
        if (attr == null)
            return true;
        
        bool hasPermission;
        
        if (attr.RequireAll)
        {
            hasPermission = await PermissionService.HasAllPermissionsAsync(
                ApplicationState.CurrentUser.UserId, 
                attr.Permissions
            );
        }
        else
        {
            hasPermission = await PermissionService.HasAnyPermissionAsync(
                ApplicationState.CurrentUser.UserId, 
                attr.Permissions
            );
        }
        
        if (!hasPermission)
        {
            MessageBox.Show(
                "ليس لديك صلاحية للوصول إلى هذه الصفحة",
                "خطأ في الصلاحيات",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
        
        return hasPermission;
    }
    
    protected abstract Task DisableControlsBasedOnPermissionsAsync();
}
```

#### 7. مثال تطبيق على CustomersListForm

```csharp
[RequiresPermission(PermissionType.ViewCustomers)]
public partial class CustomersListForm : SecureBaseForm
{
    protected override async Task DisableControlsBasedOnPermissionsAsync()
    {
        // Disable buttons based on permissions
        btnAdd.Enabled = await PermissionService.HasPermissionAsync(
            ApplicationState.CurrentUser.UserId,
            PermissionType.CreateCustomer
        );
        
        btnEdit.Enabled = await PermissionService.HasPermissionAsync(
            ApplicationState.CurrentUser.UserId,
            PermissionType.EditCustomer
        );
        
        btnDelete.Enabled = await PermissionService.HasPermissionAsync(
            ApplicationState.CurrentUser.UserId,
            PermissionType.DeleteCustomer
        );
    }
}
```

**Deliverables:**
- ✅ PermissionType enum كامل
- ✅ Permission entity محدثة
- ✅ PermissionService كامل مع caching
- ✅ SecureBaseForm للتحكم بالصلاحيات
- ✅ تطبيق على جميع الفورمز الرئيسية

---

## 📅 المرحلة 2: Audit & Tracking (Week 3)

### Day 11-12: Audit Trail

[... المحتوى كامل من التقرير السابق ...]

### Day 13-14: Activity Tracking

[... المحتوى ...]

---

## 📅 المرحلة 3: Polish & Testing (Week 4)

### Day 15-16: UI Enhancements

### Day 17-18: Integration Testing

### Day 19-20: Load Testing & Optimization

---

## 🎯 Checklist للتأكد من الجاهزية

### ✅ Core Functionality
- [ ] Optimistic Concurrency مطبقة على جميع الجداول
- [ ] Session Management يعمل بكفاءة
- [ ] Record Locking يمنع تعديل متزامن
- [ ] Permissions System يحمي جميع الفورمز
- [ ] Audit Trail يسجل جميع التغييرات

### ✅ Testing
- [ ] Unit Tests للـ Services
- [ ] Integration Tests للعمليات الحرجة
- [ ] Load Testing مع 20+ مستخدم متزامن
- [ ] Concurrency Testing

### ✅ Documentation
- [ ] توثيق الـ API
- [ ] دليل المستخدم
- [ ] دليل الصلاحيات
- [ ] Troubleshooting Guide

---

**ملاحظة:** هذه الخطة قابلة للتعديل حسب الأولويات والموارد المتاحة.
