using GraceWay.AccountingSystem.Presentation.Forms;
using GraceWay.AccountingSystem.Infrastructure.Data;
using GraceWay.AccountingSystem.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Forms;
using System.Linq;

namespace GraceWay.AccountingSystem;

static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    public static Icon? AppIcon { get; private set; }
    
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        // Database fix mode removed - use separate tool if needed
        
        // Add global exception handler
        global::System.Windows.Forms.Application.ThreadException += Application_ThreadException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        global::System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        
        try
        {
            Console.WriteLine("=== APPLICATION STARTING ===");
            System.Diagnostics.Debug.WriteLine("=== APPLICATION STARTING ===");
            
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            Console.WriteLine("Initializing ApplicationConfiguration...");
            ApplicationConfiguration.Initialize();
            Console.WriteLine("ApplicationConfiguration initialized successfully");
            
            System.Diagnostics.Debug.WriteLine("Configuration initialized");
            
            // Configure services
            Console.WriteLine("Configuring services...");
            ConfigureServices();
            Console.WriteLine("Services configured successfully");
            System.Diagnostics.Debug.WriteLine("Services configured");
            
            // Ensure database is created with all tables
            Console.WriteLine("Checking database...");
            EnsureDatabaseCreated();
            Console.WriteLine("Database ready");
            System.Diagnostics.Debug.WriteLine("Database ready");
            
            // تحميل أيقونة البرنامج
            Console.WriteLine("Loading application icon...");
            var icoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Icons", "ico.ico");
            if (!File.Exists(icoPath))
                icoPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Icons", "ico.ico");
            if (File.Exists(icoPath))
            {
                AppIcon = new Icon(icoPath);
                Console.WriteLine("Icon loaded successfully");
            }
            else
            {
                Console.WriteLine("Icon not found at: " + icoPath);
            }

            // Create and run login form
            Console.WriteLine("Creating LoginForm...");
            System.Diagnostics.Debug.WriteLine("Creating LoginForm");
            var loginForm = ServiceProvider.GetRequiredService<LoginForm>();
            Console.WriteLine("LoginForm created successfully");
            System.Diagnostics.Debug.WriteLine("Running application");
            Console.WriteLine("Running application...");
            
            global::System.Windows.Forms.Application.Run(loginForm);
            
            Console.WriteLine("=== APPLICATION CLOSED NORMALLY ===");
            System.Diagnostics.Debug.WriteLine("=== APPLICATION CLOSED NORMALLY ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FATAL ERROR: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"FATAL ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner: {ex.InnerException.Message}");
                Console.WriteLine($"Inner Stack: {ex.InnerException.StackTrace}");
                System.Diagnostics.Debug.WriteLine($"Inner: {ex.InnerException.Message}");
                System.Diagnostics.Debug.WriteLine($"Inner Stack: {ex.InnerException.StackTrace}");
            }
            
            var errorMsg = $"حدث خطأ غير متوقع أثناء تشغيل البرنامج:\n\n{ex.Message}";
            
            if (ex.InnerException != null)
            {
                errorMsg += $"\n\nالسبب:\n{ex.InnerException.Message}";
            }
            
            errorMsg += $"\n\nتفاصيل تقنية:\n{ex.StackTrace}";
            
            MessageBox.Show(
                errorMsg,
                "خطأ فادح - فشل تشغيل البرنامج",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
    
    private static void ConfigureServices()
    {
        Console.WriteLine("  - Creating service collection...");
        var services = new ServiceCollection();
        
        // Add DbContext Factory for better concurrency handling
        Console.WriteLine("  - Registering DbContext Factory...");
        services.AddDbContextFactory<AppDbContext>((serviceProvider, options) =>
        {
            var connectionString = GraceWay.AccountingSystem.Infrastructure.Configuration.AppConfiguration.Instance.GetConnectionString();
            Console.WriteLine($"  - Connection string: {connectionString}");
            
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // زيادة timeout للعمليات الثقيلة
                npgsqlOptions.CommandTimeout(60); // 60 seconds
                // تفعيل retry on failure
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null
                );
            });
            
            // تعطيل thread safety checking
            options.EnableThreadSafetyChecks(false);
            // تجاهل تحذير pending model changes
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });
        
        // Add AppDbContext as Scoped for backward compatibility - WITH AuthService injection
        Console.WriteLine("  - Registering DbContext as scoped...");
        services.AddScoped<AppDbContext>(sp => 
        {
            var factory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();
            var authService = sp.GetRequiredService<IAuthService>();
            
            // Create DbContext with AuthService
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = GraceWay.AccountingSystem.Infrastructure.Configuration.AppConfiguration.Instance.GetConnectionString();
            
            optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.CommandTimeout(60);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null
                );
            });
            
            optionsBuilder.EnableThreadSafetyChecks(false);
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            
            return new AppDbContext(optionsBuilder.Options, authService);
        });
        
        // Add Services as Transient to get fresh DbContext each time
        Console.WriteLine("  - Registering application services...");
        // AuthService must be Singleton to maintain current user state across the app
        services.AddSingleton<AuthService>();
        services.AddSingleton<IAuthService>(sp => sp.GetRequiredService<AuthService>());
        
        services.AddTransient<CashBoxService>();
        services.AddTransient<ICashBoxService>(sp => sp.GetRequiredService<CashBoxService>());
        
        services.AddTransient<JournalService>();
        services.AddTransient<IJournalService>(sp => sp.GetRequiredService<JournalService>());
        
        services.AddTransient<InvoiceService>();
        services.AddTransient<IInvoiceService>(sp => sp.GetRequiredService<InvoiceService>());
        
        services.AddTransient<CustomerService>();
        services.AddTransient<ICustomerService>(sp => sp.GetRequiredService<CustomerService>());
        
        services.AddTransient<SupplierService>();
        services.AddTransient<ISupplierService>(sp => sp.GetRequiredService<SupplierService>());
        
        services.AddTransient<AccountService>();
        services.AddTransient<IAccountService>(sp => sp.GetRequiredService<AccountService>());
        
        services.AddTransient<ReservationService>();
        services.AddTransient<IReservationService>(sp => sp.GetRequiredService<ReservationService>());
        
        services.AddTransient<TripService>();
        services.AddTransient<ITripService>(sp => sp.GetRequiredService<TripService>());
        
        services.AddTransient<TripBookingService>();
        services.AddTransient<ITripBookingService>(sp => sp.GetRequiredService<TripBookingService>());
        
        services.AddTransient<UmrahService>();
        services.AddTransient<IUmrahService>(sp => sp.GetRequiredService<UmrahService>());
        
        services.AddTransient<FlightBookingService>();
        services.AddTransient<IFlightBookingService>(sp => sp.GetRequiredService<FlightBookingService>());
        
        services.AddTransient<SettingService>();
        services.AddTransient<ISettingService>(sp => sp.GetRequiredService<SettingService>());
        
        services.AddTransient<ExportService>();
        services.AddTransient<IExportService>(sp => sp.GetRequiredService<ExportService>());
        
        services.AddTransient<Application.Services.Backup.BackupService>();
        services.AddTransient<Application.Services.Backup.IBackupService>(sp => sp.GetRequiredService<Application.Services.Backup.BackupService>());
        
        // Permission Service - Transient because it depends on DbContext
        services.AddTransient<PermissionService>();
        services.AddTransient<IPermissionService>(sp => sp.GetRequiredService<PermissionService>());
        
        // Audit Service - Transient for logging all user actions
        services.AddTransient<AuditService>();
        services.AddTransient<IAuditService>(sp => sp.GetRequiredService<AuditService>());
        
        services.AddTransient<ICashBoxService, CashBoxService>();
        services.AddTransient<IInvoiceService, InvoiceService>();
        
        // Add Forms
        Console.WriteLine("  - Registering forms...");
        services.AddTransient<LoginForm>();
        services.AddTransient<RegisterForm>();
        services.AddTransient<MainForm>();
        services.AddTransient<SuppliersListForm>();
        services.AddTransient<AddEditSupplierForm>();
        services.AddTransient<ReservationsListForm>();
        services.AddTransient<AddEditReservationForm>();
        services.AddTransient<ServiceTypesForm>();
        services.AddTransient<AddEditServiceTypeForm>();
        services.AddTransient<UmrahPackagesListForm>();
        services.AddTransient<AddEditUmrahPackageForm>();
        services.AddTransient<BackupManagementForm>();
        services.AddTransient<ActiveSessionsForm>();
        
        Console.WriteLine("  - Building ServiceProvider...");
        ServiceProvider = services.BuildServiceProvider();
        
        // إعداد العلاقة بين Services بعد بناء ServiceProvider لتجنب Circular Dependency
        Console.WriteLine("  - Setting up service relationships...");
        using (var scope = ServiceProvider.CreateScope())
        {
            // Setup CashBoxService and JournalService relationship
            var cashBoxService = scope.ServiceProvider.GetRequiredService<CashBoxService>();
            var journalService = scope.ServiceProvider.GetRequiredService<IJournalService>();
            cashBoxService.SetJournalService(journalService);
            
            // Setup AuthService and AuditService relationship
            var authService = ServiceProvider.GetRequiredService<AuthService>();
            var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();
            authService.SetAuditService(auditService);
        }
        Console.WriteLine("  - All services configured successfully!");
        
        // ✅ Initialize SessionManager with ServiceProvider for database access
        Console.WriteLine("  - Initializing SessionManager...");
        SessionManager.Instance.Initialize(ServiceProvider);
        Console.WriteLine("  - SessionManager initialized successfully!");
    }
    
    private static void EnsureDatabaseCreated()
    {
        try
        {
            Console.WriteLine("  - Checking database connection...");
            System.Diagnostics.Debug.WriteLine("Checking database connection...");
            using var scope = ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            // Test database connection first
            try
            {
                Console.WriteLine("  - Testing database connection...");
                System.Diagnostics.Debug.WriteLine("Testing database connection...");
                var canConnect = context.Database.CanConnect();
                if (canConnect)
                {
                    Console.WriteLine("  - Database connection successful");
                    System.Diagnostics.Debug.WriteLine("Database connection successful");
                }
                else
                {
                    Console.WriteLine("  - WARNING: Database connection returned false");
                }
            }
            catch (Exception connEx)
            {
                Console.WriteLine($"  - Database connection failed: {connEx.Message}");
                System.Diagnostics.Debug.WriteLine($"Database connection failed: {connEx.Message}");
                throw new Exception($"فشل الاتصال بقاعدة البيانات:\n{connEx.Message}\n\nتأكد من:\n1. PostgreSQL يعمل\n2. اسم قاعدة البيانات: graceway_accounting\n3. المستخدم: postgres\n4. كلمة المرور: 123456", connEx);
            }
            
            // Check if tables exist
            bool needsRecreate = false;
            try
            {
                Console.WriteLine("  - Checking if tables exist...");
                System.Diagnostics.Debug.WriteLine("Checking if tables exist...");
                context.Database.ExecuteSqlRaw("SELECT 1 FROM roles LIMIT 1");
                Console.WriteLine("  - Tables exist");
                System.Diagnostics.Debug.WriteLine("Tables exist");
            }
            catch
            {
                Console.WriteLine("  - Tables don't exist, need to create");
                System.Diagnostics.Debug.WriteLine("Tables don't exist, need to create");
                needsRecreate = true;
            }
            
            if (needsRecreate)
            {
                Console.WriteLine("  - Creating database schema...");
                System.Diagnostics.Debug.WriteLine("Creating database schema...");
                context.Database.EnsureCreated();
                Console.WriteLine("  - Database schema created");
                System.Diagnostics.Debug.WriteLine("Database schema created");
            }
            
            // Initialize default data if needed
            Console.WriteLine("  - Initializing default data...");
            System.Diagnostics.Debug.WriteLine("Initializing default data...");
            InitializeDefaultData(context);
            Console.WriteLine("  - Default data initialized");
            System.Diagnostics.Debug.WriteLine("Default data initialized");
            
            Console.WriteLine("  - Database ready!");
            System.Diagnostics.Debug.WriteLine("Database ready!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  - DATABASE ERROR: {ex.Message}");
            Console.WriteLine($"  - Stack: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"DATABASE ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"  - Inner: {ex.InnerException.Message}");
                System.Diagnostics.Debug.WriteLine($"Inner: {ex.InnerException.Message}");
            }
            
            // Show error to user
            var errorMsg = $"خطأ في قاعدة البيانات:\n\n{ex.Message}";
            if (ex.InnerException != null)
            {
                errorMsg += $"\n\nالسبب:\n{ex.InnerException.Message}";
            }
            errorMsg += $"\n\nيرجى التأكد من:\n1. تشغيل PostgreSQL\n2. اسم قاعدة البيانات: graceway_accounting\n3. المستخدم: postgres\n4. كلمة المرور: 123456";
            
            MessageBox.Show(
                errorMsg,
                "خطأ في قاعدة البيانات",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            throw;
        }
    }
    
    private static async void InitializeDefaultData(AppDbContext context)
    {
        Console.WriteLine("    - Checking for default data...");
        
        // استخدام PermissionSeeder لإنشاء جميع البيانات الافتراضية
        if (!context.Users.Any() || !context.Roles.Any() || !context.Permissions.Any())
        {
            Console.WriteLine("    - Running PermissionSeeder...");
            try
            {
                await PermissionSeeder.SeedAsync(context);
                Console.WriteLine("    - PermissionSeeder completed successfully!");
                Console.WriteLine("    - Default users created:");
                Console.WriteLine("      * operations / operations123 (Operations Department)");
                Console.WriteLine("      * aviation / aviation123 (Aviation and Umrah)");
                Console.WriteLine("      * admin / admin123 (Administrator)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    - ERROR in PermissionSeeder: {ex.Message}");
                Console.WriteLine($"    - Stack: {ex.StackTrace}");
                throw;
            }
        }
        else
        {
            Console.WriteLine("    - Default data already exists, skipping seeder.");
        }
    }
    
    private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
    {
        Console.WriteLine($"Thread Exception: {e.Exception.Message}");
        Console.WriteLine($"Stack: {e.Exception.StackTrace}");
        MessageBox.Show(
            $"حدث خطأ في التطبيق:\n\n{e.Exception.Message}\n\nتفاصيل:\n{e.Exception.StackTrace}",
            "خطأ في التطبيق",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
    }
    
    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        Console.WriteLine($"Unhandled Exception: {ex?.Message}");
        Console.WriteLine($"Stack: {ex?.StackTrace}");
        MessageBox.Show(
            $"خطأ غير معالج:\n\n{ex?.Message}\n\nتفاصيل:\n{ex?.StackTrace}",
            "خطأ غير معالج",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
    }
}
