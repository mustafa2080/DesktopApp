using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace GraceWay.AccountingSystem.Infrastructure.Data;

/// <summary>
/// إنشاء البيانات الأساسية للصلاحيات والأدوار والمستخدمين
/// </summary>
public static class PermissionSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // التحقق من وجود بيانات قديمة ومسحها إذا كانت غير كاملة
        var rolesCount = await context.Roles.CountAsync();
        var permissionsCount = await context.Permissions.CountAsync();
        var usersCount = await context.Users.CountAsync();
        
        // إذا كانت البيانات موجودة ولكن غير كاملة، امسح كل شيء وأبدأ من جديد
        if (rolesCount > 0 || permissionsCount > 0 || usersCount > 0)
        {
            // التحقق من أن البيانات كاملة
            var expectedRoles = 3; // Operations, Aviation, Admin
            var expectedPermissions = 88; // جميع الصلاحيات
            var expectedUsers = 3; // operations, aviation, admin
            
            if (rolesCount < expectedRoles || permissionsCount < expectedPermissions || usersCount < expectedUsers)
            {
                Console.WriteLine("⚠️  بيانات غير مكتملة! سيتم مسح البيانات القديمة وإعادة الإنشاء...");
                
                try
                {
                    // مسح البيانات بالترتيب الصحيح (من الأطفال إلى الآباء)
                    // استخدم ExecuteSqlRaw بدلاً من ExecuteSqlRawAsync في حال كانت بعض الجداول غير موجودة
                    try { await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE role_permissions CASCADE"); } catch { }
                    try { await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE users CASCADE"); } catch { }
                    try { await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE roles CASCADE"); } catch { }
                    try { await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE permissions CASCADE"); } catch { }
                    
                    Console.WriteLine("✓ تم مسح البيانات القديمة");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️  تحذير أثناء مسح البيانات: {ex.Message}");
                    Console.WriteLine("سنستمر في المحاولة...");
                }
            }
            else
            {
                Console.WriteLine("✓ البيانات موجودة ومكتملة، تخطي الإنشاء...");
                return;
            }
        }
        
        // 1. إنشاء الصلاحيات
        await SeedPermissionsAsync(context);
        
        // 2. إنشاء الأدوار
        await SeedRolesAsync(context);
        
        // 3. ربط الصلاحيات بالأدوار
        await SeedRolePermissionsAsync(context);
        
        // 4. إنشاء المستخدمين
        await SeedUsersAsync(context);
    }
    
    private static async Task SeedPermissionsAsync(AppDbContext context)
    {
        if (await context.Permissions.AnyAsync())
            return;
        
        var permissions = new List<Permission>
        {
            // ============================================
            // قسم الرحلات (Trips)
            // ============================================
            new() { PermissionType = PermissionType.ViewTrips, PermissionName = "عرض الرحلات", Category = "Trips", Module = "Trips" },
            new() { PermissionType = PermissionType.CreateTrip, PermissionName = "إضافة رحلة", Category = "Trips", Module = "Trips" },
            new() { PermissionType = PermissionType.EditTrip, PermissionName = "تعديل رحلة", Category = "Trips", Module = "Trips" },
            new() { PermissionType = PermissionType.DeleteTrip, PermissionName = "حذف رحلة", Category = "Trips", Module = "Trips" },
            new() { PermissionType = PermissionType.CloseTrip, PermissionName = "إغلاق رحلة", Category = "Trips", Module = "Trips" },
            new() { PermissionType = PermissionType.ManageTripBookings, PermissionName = "إدارة حجوزات الرحلات", Category = "Trips", Module = "Trips" },
            
            // ============================================
            // قسم الطيران (Aviation)
            // ============================================
            new() { PermissionType = PermissionType.ViewFlightBookings, PermissionName = "عرض حجوزات الطيران", Category = "Aviation", Module = "Aviation" },
            new() { PermissionType = PermissionType.CreateFlightBooking, PermissionName = "إضافة حجز طيران", Category = "Aviation", Module = "Aviation" },
            new() { PermissionType = PermissionType.EditFlightBooking, PermissionName = "تعديل حجز طيران", Category = "Aviation", Module = "Aviation" },
            new() { PermissionType = PermissionType.DeleteFlightBooking, PermissionName = "حذف حجز طيران", Category = "Aviation", Module = "Aviation" },
            new() { PermissionType = PermissionType.ManageFlightPayments, PermissionName = "إدارة مدفوعات الطيران", Category = "Aviation", Module = "Aviation" },
            
            // ============================================
            // قسم العمرة (Umrah)
            // ============================================
            new() { PermissionType = PermissionType.ViewUmrahPackages, PermissionName = "عرض باقات العمرة", Category = "Umrah", Module = "Umrah" },
            new() { PermissionType = PermissionType.CreateUmrahPackage, PermissionName = "إضافة باقة عمرة", Category = "Umrah", Module = "Umrah" },
            new() { PermissionType = PermissionType.EditUmrahPackage, PermissionName = "تعديل باقة عمرة", Category = "Umrah", Module = "Umrah" },
            new() { PermissionType = PermissionType.DeleteUmrahPackage, PermissionName = "حذف باقة عمرة", Category = "Umrah", Module = "Umrah" },
            new() { PermissionType = PermissionType.ViewUmrahTrips, PermissionName = "عرض رحلات العمرة", Category = "Umrah", Module = "Umrah" },
            new() { PermissionType = PermissionType.CreateUmrahTrip, PermissionName = "إضافة رحلة عمرة", Category = "Umrah", Module = "Umrah" },
            new() { PermissionType = PermissionType.EditUmrahTrip, PermissionName = "تعديل رحلة عمرة", Category = "Umrah", Module = "Umrah" },
            new() { PermissionType = PermissionType.DeleteUmrahTrip, PermissionName = "حذف رحلة عمرة", Category = "Umrah", Module = "Umrah" },
            new() { PermissionType = PermissionType.ManageUmrahPilgrims, PermissionName = "إدارة معتمرين", Category = "Umrah", Module = "Umrah" },
            new() { PermissionType = PermissionType.ManageUmrahPayments, PermissionName = "إدارة مدفوعات العمرة", Category = "Umrah", Module = "Umrah" },
            
            // ============================================
            // الآلة الحاسبة
            // ============================================
            new() { PermissionType = PermissionType.UseCalculator, PermissionName = "استخدام الآلة الحاسبة", Category = "Tools", Module = "Calculator" },
            
            // ============================================
            // قسم العملاء
            // ============================================
            new() { PermissionType = PermissionType.ViewCustomers, PermissionName = "عرض العملاء", Category = "Customers", Module = "Accounting" },
            new() { PermissionType = PermissionType.CreateCustomer, PermissionName = "إضافة عميل", Category = "Customers", Module = "Accounting" },
            new() { PermissionType = PermissionType.EditCustomer, PermissionName = "تعديل عميل", Category = "Customers", Module = "Accounting" },
            new() { PermissionType = PermissionType.DeleteCustomer, PermissionName = "حذف عميل", Category = "Customers", Module = "Accounting" },
            new() { PermissionType = PermissionType.ViewCustomerStatement, PermissionName = "عرض كشف حساب عميل", Category = "Customers", Module = "Accounting" },
            
            // ============================================
            // قسم الموردين
            // ============================================
            new() { PermissionType = PermissionType.ViewSuppliers, PermissionName = "عرض الموردين", Category = "Suppliers", Module = "Accounting" },
            new() { PermissionType = PermissionType.CreateSupplier, PermissionName = "إضافة مورد", Category = "Suppliers", Module = "Accounting" },
            new() { PermissionType = PermissionType.EditSupplier, PermissionName = "تعديل مورد", Category = "Suppliers", Module = "Accounting" },
            new() { PermissionType = PermissionType.DeleteSupplier, PermissionName = "حذف مورد", Category = "Suppliers", Module = "Accounting" },
            new() { PermissionType = PermissionType.ViewSupplierStatement, PermissionName = "عرض كشف حساب مورد", Category = "Suppliers", Module = "Accounting" },
            
            // ============================================
            // قسم الفواتير
            // ============================================
            new() { PermissionType = PermissionType.ViewInvoices, PermissionName = "عرض الفواتير", Category = "Invoices", Module = "Accounting" },
            new() { PermissionType = PermissionType.CreateSalesInvoice, PermissionName = "إضافة فاتورة بيع", Category = "Invoices", Module = "Accounting" },
            new() { PermissionType = PermissionType.EditSalesInvoice, PermissionName = "تعديل فاتورة بيع", Category = "Invoices", Module = "Accounting" },
            new() { PermissionType = PermissionType.DeleteSalesInvoice, PermissionName = "حذف فاتورة بيع", Category = "Invoices", Module = "Accounting" },
            new() { PermissionType = PermissionType.CreatePurchaseInvoice, PermissionName = "إضافة فاتورة شراء", Category = "Invoices", Module = "Accounting" },
            new() { PermissionType = PermissionType.EditPurchaseInvoice, PermissionName = "تعديل فاتورة شراء", Category = "Invoices", Module = "Accounting" },
            new() { PermissionType = PermissionType.DeletePurchaseInvoice, PermissionName = "حذف فاتورة شراء", Category = "Invoices", Module = "Accounting" },
            new() { PermissionType = PermissionType.ApproveInvoice, PermissionName = "اعتماد فاتورة", Category = "Invoices", Module = "Accounting" },
            
            // ============================================
            // قسم الحجوزات
            // ============================================
            new() { PermissionType = PermissionType.ViewReservations, PermissionName = "عرض الحجوزات", Category = "Reservations", Module = "Operations" },
            new() { PermissionType = PermissionType.CreateReservation, PermissionName = "إضافة حجز", Category = "Reservations", Module = "Operations" },
            new() { PermissionType = PermissionType.EditReservation, PermissionName = "تعديل حجز", Category = "Reservations", Module = "Operations" },
            new() { PermissionType = PermissionType.DeleteReservation, PermissionName = "حذف حجز", Category = "Reservations", Module = "Operations" },
            
            // ============================================
            // قسم الخزنة والبنوك
            // ============================================
            new() { PermissionType = PermissionType.ViewCashBox, PermissionName = "عرض الخزنة", Category = "Cash", Module = "Accounting" },
            new() { PermissionType = PermissionType.CreateCashTransaction, PermissionName = "إضافة حركة نقدية", Category = "Cash", Module = "Accounting" },
            new() { PermissionType = PermissionType.EditCashTransaction, PermissionName = "تعديل حركة نقدية", Category = "Cash", Module = "Accounting" },
            new() { PermissionType = PermissionType.DeleteCashTransaction, PermissionName = "حذف حركة نقدية", Category = "Cash", Module = "Accounting" },
            new() { PermissionType = PermissionType.ViewBankAccounts, PermissionName = "عرض الحسابات البنكية", Category = "Bank", Module = "Accounting" },
            new() { PermissionType = PermissionType.CreateBankTransaction, PermissionName = "إضافة حركة بنكية", Category = "Bank", Module = "Accounting" },
            new() { PermissionType = PermissionType.EditBankTransaction, PermissionName = "تعديل حركة بنكية", Category = "Bank", Module = "Accounting" },
            new() { PermissionType = PermissionType.DeleteBankTransaction, PermissionName = "حذف حركة بنكية", Category = "Bank", Module = "Accounting" },
            new() { PermissionType = PermissionType.ManageBankTransfers, PermissionName = "إدارة التحويلات البنكية", Category = "Bank", Module = "Accounting" },
            
            // ============================================
            // قسم القيود اليومية
            // ============================================
            new() { PermissionType = PermissionType.ViewJournalEntries, PermissionName = "عرض القيود اليومية", Category = "Journal", Module = "Accounting" },
            new() { PermissionType = PermissionType.CreateJournalEntry, PermissionName = "إضافة قيد يومي", Category = "Journal", Module = "Accounting" },
            new() { PermissionType = PermissionType.EditJournalEntry, PermissionName = "تعديل قيد يومي", Category = "Journal", Module = "Accounting" },
            new() { PermissionType = PermissionType.DeleteJournalEntry, PermissionName = "حذف قيد يومي", Category = "Journal", Module = "Accounting" },
            new() { PermissionType = PermissionType.EditClosedPeriod, PermissionName = "تعديل فترة مغلقة", Category = "Journal", Module = "Accounting", IsSystemPermission = true },
            
            // ============================================
            // قسم الحسابات
            // ============================================
            new() { PermissionType = PermissionType.ViewChartOfAccounts, PermissionName = "عرض شجرة الحسابات", Category = "Accounts", Module = "Accounting" },
            new() { PermissionType = PermissionType.CreateAccount, PermissionName = "إضافة حساب", Category = "Accounts", Module = "Accounting" },
            new() { PermissionType = PermissionType.EditAccount, PermissionName = "تعديل حساب", Category = "Accounts", Module = "Accounting" },
            new() { PermissionType = PermissionType.DeleteAccount, PermissionName = "حذف حساب", Category = "Accounts", Module = "Accounting" },
            
            // ============================================
            // قسم التقارير
            // ============================================
            new() { PermissionType = PermissionType.ViewReports, PermissionName = "عرض التقارير", Category = "Reports", Module = "Reports" },
            new() { PermissionType = PermissionType.ViewFinancialReports, PermissionName = "عرض التقارير المالية", Category = "Reports", Module = "Reports" },
            new() { PermissionType = PermissionType.ViewTrialBalance, PermissionName = "عرض ميزان المراجعة", Category = "Reports", Module = "Reports" },
            new() { PermissionType = PermissionType.ViewIncomeStatement, PermissionName = "عرض قائمة الدخل", Category = "Reports", Module = "Reports" },
            new() { PermissionType = PermissionType.ViewBalanceSheet, PermissionName = "عرض الميزانية العمومية", Category = "Reports", Module = "Reports" },
            new() { PermissionType = PermissionType.ViewCashFlowStatement, PermissionName = "عرض قائمة التدفقات النقدية", Category = "Reports", Module = "Reports" },
            new() { PermissionType = PermissionType.ViewTripReports, PermissionName = "عرض تقارير الرحلات", Category = "Reports", Module = "Reports" },
            new() { PermissionType = PermissionType.ViewFlightReports, PermissionName = "عرض تقارير الطيران", Category = "Reports", Module = "Reports" },
            new() { PermissionType = PermissionType.ViewUmrahReports, PermissionName = "عرض تقارير العمرة", Category = "Reports", Module = "Reports" },
            new() { PermissionType = PermissionType.ViewProfitMargins, PermissionName = "عرض هوامش الربح", Category = "Reports", Module = "Reports", IsSystemPermission = true },
            new() { PermissionType = PermissionType.ExportReports, PermissionName = "تصدير التقارير", Category = "Reports", Module = "Reports" },
            new() { PermissionType = PermissionType.PrintReports, PermissionName = "طباعة التقارير", Category = "Reports", Module = "Reports" },
            
            // ============================================
            // قسم الإعدادات
            // ============================================
            new() { PermissionType = PermissionType.ViewSettings, PermissionName = "عرض الإعدادات", Category = "Settings", Module = "System" },
            new() { PermissionType = PermissionType.EditCompanySettings, PermissionName = "تعديل إعدادات الشركة", Category = "Settings", Module = "System", IsSystemPermission = true },
            new() { PermissionType = PermissionType.EditInvoiceSettings, PermissionName = "تعديل إعدادات الفواتير", Category = "Settings", Module = "System" },
            new() { PermissionType = PermissionType.EditFiscalYearSettings, PermissionName = "تعديل إعدادات السنة المالية", Category = "Settings", Module = "System", IsSystemPermission = true },
            new() { PermissionType = PermissionType.ManageCurrencies, PermissionName = "إدارة العملات", Category = "Settings", Module = "System" },
            new() { PermissionType = PermissionType.ManageServiceTypes, PermissionName = "إدارة أنواع الخدمات", Category = "Settings", Module = "System" },
            
            // ============================================
            // قسم إدارة النظام
            // ============================================
            new() { PermissionType = PermissionType.ManageUsers, PermissionName = "إدارة المستخدمين", Category = "Administration", Module = "System", IsSystemPermission = true },
            new() { PermissionType = PermissionType.ManageRoles, PermissionName = "إدارة الأدوار", Category = "Administration", Module = "System", IsSystemPermission = true },
            new() { PermissionType = PermissionType.ManagePermissions, PermissionName = "إدارة الصلاحيات", Category = "Administration", Module = "System", IsSystemPermission = true },
            new() { PermissionType = PermissionType.ViewAuditLogs, PermissionName = "عرض سجل التدقيق", Category = "Administration", Module = "System", IsSystemPermission = true },
            new() { PermissionType = PermissionType.ViewSystemLogs, PermissionName = "عرض سجل النظام", Category = "Administration", Module = "System", IsSystemPermission = true },
            new() { PermissionType = PermissionType.BackupDatabase, PermissionName = "نسخ احتياطي لقاعدة البيانات", Category = "Administration", Module = "System", IsSystemPermission = true },
            new() { PermissionType = PermissionType.RestoreDatabase, PermissionName = "استعادة قاعدة البيانات", Category = "Administration", Module = "System", IsSystemPermission = true },
            new() { PermissionType = PermissionType.ManageSessions, PermissionName = "إدارة الجلسات", Category = "Administration", Module = "System", IsSystemPermission = true }
        };
        
        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();
    }
    
    private static async Task SeedRolesAsync(AppDbContext context)
    {
        if (await context.Roles.AnyAsync())
            return;
        
        var roles = new List<Role>
        {
            new() 
            { 
                RoleName = "Operations Department", 
                Description = "قسم العمليات - الوصول إلى الرحلات والآلة الحاسبة فقط" 
            },
            new() 
            { 
                RoleName = "Aviation and Umrah", 
                Description = "قسم الطيران والعمرة - الوصول إلى الطيران والعمرة فقط" 
            },
            new() 
            { 
                RoleName = "Administrator", 
                Description = "المدير - الوصول الكامل لجميع أقسام النظام" 
            }
        };
        
        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
    }
    
    private static async Task SeedRolePermissionsAsync(AppDbContext context)
    {
        if (await context.RolePermissions.AnyAsync())
            return;
        
        var roles = await context.Roles.ToListAsync();
        var permissions = await context.Permissions.ToListAsync();
        
        if (!roles.Any() || !permissions.Any())
        {
            throw new InvalidOperationException("يجب إنشاء الأدوار والصلاحيات قبل ربطها");
        }
        
        var operationsRole = roles.FirstOrDefault(r => r.RoleName == "Operations Department");
        var aviationUmrahRole = roles.FirstOrDefault(r => r.RoleName == "Aviation and Umrah");
        var adminRole = roles.FirstOrDefault(r => r.RoleName == "Administrator");
        
        if (operationsRole == null || aviationUmrahRole == null || adminRole == null)
        {
            throw new InvalidOperationException("لم يتم العثور على جميع الأدوار المطلوبة");
        }
        
        var rolePermissions = new List<RolePermission>();
        
        // ============================================
        // Operations Department Permissions (Trips + Calculator)
        // ============================================
        var operationsPermissions = new[]
        {
            // Trips - CRITICAL: These must be in Trips module
            PermissionType.ViewTrips,
            PermissionType.CreateTrip,
            PermissionType.EditTrip,
            PermissionType.DeleteTrip,
            PermissionType.CloseTrip,
            PermissionType.ManageTripBookings,
            
            // Calculator
            PermissionType.UseCalculator,
            
            // Reports - These enable trip reports
            PermissionType.ViewReports,
            PermissionType.ViewTripReports,
            PermissionType.ExportReports,
            PermissionType.PrintReports
        };
        
        foreach (var permType in operationsPermissions)
        {
            var perm = permissions.First(p => p.PermissionType == permType);
            rolePermissions.Add(new RolePermission
            {
                RoleId = operationsRole.RoleId,
                PermissionId = perm.PermissionId
            });
        }
        
        // ============================================
        // Aviation and Umrah Permissions (Aviation + Umrah + Calculator)
        // ============================================
        var aviationUmrahPermissions = new[]
        {
            // Flight Bookings - CRITICAL: These must be in Aviation module
            PermissionType.ViewFlightBookings,
            PermissionType.CreateFlightBooking,
            PermissionType.EditFlightBooking,
            PermissionType.DeleteFlightBooking,
            PermissionType.ManageFlightPayments,
            
            // Umrah - CRITICAL: These must be in Umrah module
            PermissionType.ViewUmrahPackages,
            PermissionType.CreateUmrahPackage,
            PermissionType.EditUmrahPackage,
            PermissionType.DeleteUmrahPackage,
            PermissionType.ViewUmrahTrips,
            PermissionType.CreateUmrahTrip,
            PermissionType.EditUmrahTrip,
            PermissionType.DeleteUmrahTrip,
            PermissionType.ManageUmrahPilgrims,
            PermissionType.ManageUmrahPayments,
            
            // Calculator
            PermissionType.UseCalculator,
            
            // Reports - These enable flight and umrah reports
            PermissionType.ViewReports,
            PermissionType.ViewFlightReports,
            PermissionType.ViewUmrahReports,
            PermissionType.ExportReports,
            PermissionType.PrintReports
        };
        
        foreach (var permType in aviationUmrahPermissions)
        {
            var perm = permissions.First(p => p.PermissionType == permType);
            rolePermissions.Add(new RolePermission
            {
                RoleId = aviationUmrahRole.RoleId,
                PermissionId = perm.PermissionId
            });
        }
        
        // ============================================
        // Administrator - ALL Permissions
        // ============================================
        foreach (var perm in permissions)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.RoleId,
                PermissionId = perm.PermissionId
            });
        }
        
        await context.RolePermissions.AddRangeAsync(rolePermissions);
        await context.SaveChangesAsync();
    }
    
    private static async Task SeedUsersAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync())
            return;
        
        var roles = await context.Roles.ToListAsync();
        
        if (!roles.Any())
        {
            throw new InvalidOperationException("يجب إنشاء الأدوار قبل إنشاء المستخدمين");
        }
        
        var operationsRole = roles.FirstOrDefault(r => r.RoleName == "Operations Department");
        var aviationUmrahRole = roles.FirstOrDefault(r => r.RoleName == "Aviation and Umrah");
        var adminRole = roles.FirstOrDefault(r => r.RoleName == "Administrator");
        
        if (operationsRole == null || aviationUmrahRole == null || adminRole == null)
        {
            throw new InvalidOperationException("لم يتم العثور على جميع الأدوار المطلوبة");
        }
        
        var users = new List<User>
        {
            new()
            {
                Username = "operations",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("operations123"),
                FullName = "قسم العمليات",
                Email = "operations@graceway.com",
                RoleId = operationsRole.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Username = "aviation",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("aviation123"),
                FullName = "قسم الطيران والعمرة",
                Email = "aviation@graceway.com",
                RoleId = aviationUmrahRole.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                FullName = "المدير العام",
                Email = "admin@graceway.com",
                RoleId = adminRole.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
        
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }
}
