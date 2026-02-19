using GraceWay.AccountingSystem.Presentation.Forms;
using GraceWay.AccountingSystem.Infrastructure.Data;
using GraceWay.AccountingSystem.Infrastructure.Configuration;
using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GraceWay.AccountingSystem;

static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    public static Icon? AppIcon { get; private set; }

    [STAThread]
    static void Main(string[] args)
    {
        // Global exception handlers
        global::System.Windows.Forms.Application.ThreadException += Application_ThreadException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        global::System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        try
        {
            ApplicationConfiguration.Initialize();

            // تهيئة نظام اللوج أولاً
            AppLogger.Initialize();
            AppLogger.Info("=== APPLICATION STARTING ===");

            // تحميل أيقونة البرنامج
            LoadAppIcon();

            // 🔐 خطوة 1: التحقق من إعدادات الاتصال المشفرة
            if (!EnsureDatabaseConfig())
            {
                AppLogger.Info("User cancelled database setup. Exiting.");
                return;
            }

            // 🔧 خطوة 2: تهيئة الـ Services
            ConfigureServices();
            AppLogger.Info("Services configured successfully");

            // 🗄️ خطوة 3: التحقق من قاعدة البيانات وتطبيق Migrations
            if (!EnsureDatabaseReady())
            {
                AppLogger.Error("Database initialization failed. Exiting.");
                return;
            }

            // 🚀 خطوة 4: تشغيل البرنامج
            AppLogger.Info("Launching LoginForm...");
            var loginForm = ServiceProvider.GetRequiredService<LoginForm>();
            global::System.Windows.Forms.Application.Run(loginForm);

            AppLogger.Info("=== APPLICATION CLOSED NORMALLY ===");
        }
        catch (Exception ex)
        {
            AppLogger.Fatal("Fatal error in Main", ex);
            ShowFatalError(ex);
        }
    }

    /// <summary>
    /// 🔐 التحقق من إعداد الاتصال - يطلب من المستخدم الإعداد لو مش موجود أو فاشل
    /// </summary>
    private static bool EnsureDatabaseConfig()
    {
        // لو في إعداد محفوظ، اختبره أولاً
        if (SecureConfigManager.HasSavedConfig())
        {
            try
            {
                var config = SecureConfigManager.LoadConfig()!;
                var connStr = SecureConfigManager.BuildConnectionString(config);
                using var conn = new Npgsql.NpgsqlConnection(connStr);
                conn.Open();
                conn.Close();
                AppLogger.Info("Secure database config loaded and verified.");
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Warning($"Saved config connection failed: {ex.Message}. Showing setup form.");
            }
        }

        // إظهار شاشة الإعداد
        using var setupForm = new DatabaseSetupForm();
        var result = setupForm.ShowDialog();
        return result == DialogResult.OK;
    }

    private static void ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddDbContextFactory<AppDbContext>((sp, options) =>
        {
            var connectionString = AppConfiguration.Instance.GetConnectionString();
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.CommandTimeout(60);
                npgsqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(10), null);
            });
            options.EnableThreadSafetyChecks(false);
            options.ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<AppDbContext>(sp =>
        {
            var authService = sp.GetRequiredService<IAuthService>();
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = AppConfiguration.Instance.GetConnectionString();

            optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.CommandTimeout(60);
                npgsqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(10), null);
            });
            optionsBuilder.EnableThreadSafetyChecks(false);
            optionsBuilder.ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

            return new AppDbContext(optionsBuilder.Options, authService);
        });

        // Services
        services.AddSingleton<AuthService>();
        services.AddSingleton<IAuthService>(sp => sp.GetRequiredService<AuthService>());

        // ✅ Lazy registration لحل Circular Dependencies بطريقة صحيحة
        services.AddTransient(sp => new Lazy<IJournalService>(sp.GetRequiredService<IJournalService>));
        services.AddTransient(sp => new Lazy<IAuditService>(sp.GetRequiredService<IAuditService>));

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
        services.AddTransient<Application.Services.Backup.IBackupService>(
            sp => sp.GetRequiredService<Application.Services.Backup.BackupService>());

        services.AddTransient<PermissionService>();
        services.AddTransient<IPermissionService>(sp => sp.GetRequiredService<PermissionService>());

        services.AddTransient<AuditService>();
        services.AddTransient<IAuditService>(sp => sp.GetRequiredService<AuditService>());

        services.AddTransient<FileManagerService>();
        services.AddTransient<IFileManagerService>(sp => sp.GetRequiredService<FileManagerService>());

        // Forms
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
        services.AddTransient<FileManagerForm>();

        ServiceProvider = services.BuildServiceProvider();

        // ✅ تم إزالة Circular Dependency workarounds - الآن يستخدم Lazy<T> injection
        SessionManager.Instance.Initialize(ServiceProvider);
        AppLogger.Info("All services registered successfully.");
    }

    /// <summary>
    /// 🗄️ التحقق من قاعدة البيانات مع دعم Migrations
    /// </summary>
    private static bool EnsureDatabaseReady()
    {
        try
        {
            using var scope = ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            AppLogger.Info("Testing database connection...");
            if (!context.Database.CanConnect())
                throw new Exception("تعذر الاتصال بقاعدة البيانات.");

            AppLogger.Info("Applying pending migrations...");
            // تطبيق Migrations بدلاً من EnsureCreated
            context.Database.Migrate();
            AppLogger.Info("Migrations applied successfully.");

            // إنشاء البيانات الافتراضية
            InitializeDefaultData(context).GetAwaiter().GetResult();
            AppLogger.Info("Database ready.");
            return true;
        }
        catch (Exception ex)
        {
            AppLogger.Error("Database initialization failed", ex);

            var choice = MessageBox.Show(
                $"حدث خطأ في قاعدة البيانات:\n\n{ex.Message}\n\n" +
                "هل تريد تغيير إعدادات الاتصال؟",
                "خطأ في قاعدة البيانات",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error);

            if (choice == DialogResult.Yes)
            {
                SecureConfigManager.DeleteConfig();
                AppConfiguration.Instance.RefreshConnectionString();
                return EnsureDatabaseConfig() && EnsureDatabaseReady();
            }

            return false;
        }
    }

    private static async Task InitializeDefaultData(AppDbContext context)
    {
        if (!context.Users.Any() || !context.Roles.Any() || !context.Permissions.Any())
        {
            AppLogger.Info("Running PermissionSeeder...");
            await PermissionSeeder.SeedAsync(context);
            AppLogger.Info("Default data seeded successfully.");
        }
    }

    private static void LoadAppIcon()
    {
        var icoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Icons", "ico.ico");
        if (!File.Exists(icoPath))
            icoPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Icons", "ico.ico");
        if (File.Exists(icoPath))
            AppIcon = new Icon(icoPath);
    }

    private static void ShowFatalError(Exception ex)
    {
        // ❌ لا نعرض Stack Trace للمستخدم
        var userMessage = "حدث خطأ غير متوقع أثناء تشغيل البرنامج.\n\n" +
                          "يرجى التواصل مع الدعم الفني وإرسال ملف السجل (graceway.log).\n\n" +
                          $"رمز الخطأ: {ex.GetType().Name}";

        MessageBox.Show(userMessage, "خطأ فادح", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
    {
        AppLogger.Error("Thread exception", e.Exception);
        MessageBox.Show(
            "حدث خطأ في العملية الحالية.\n\nتم تسجيل الخطأ. يرجى التواصل مع الدعم الفني إذا تكرر.",
            "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        AppLogger.Fatal("Unhandled exception", ex);
    }
}
