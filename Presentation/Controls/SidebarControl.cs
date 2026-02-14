using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using System.Drawing.Drawing2D;

namespace GraceWay.AccountingSystem.Presentation.Controls;

public class SidebarControl : Panel
{
    public event EventHandler<string>? MenuItemClicked;
    
    private readonly List<SidebarMenuItem> _menuItems;
    private SidebarMenuItem? _activeItem;
    
    private IReservationService? _reservationService;
    private ICustomerService? _customerService;
    private ISupplierService? _supplierService;
    private ITripService? _tripService;
    private IUmrahService? _umrahService;
    private IInvoiceService? _invoiceService;
    private IPermissionService? _permissionService;
    private int _currentUserId;

    public SidebarControl()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = ColorScheme.SidebarBg;
        this.Padding = new Padding(0);
        this.AutoScroll = false; // إلغاء السكرول

        _menuItems = new List<SidebarMenuItem>();

        InitializeMenu();
    }

    public void InitializeServices(
        IReservationService reservationService,
        ICustomerService customerService,
        ISupplierService supplierService,
        ITripService tripService,
        IUmrahService umrahService,
        IInvoiceService invoiceService,
        IPermissionService permissionService,
        int currentUserId)
    {
        Console.WriteLine("=== InitializeServices called ===");
        Console.WriteLine($"User ID: {currentUserId}");
        
        _reservationService = reservationService;
        _customerService = customerService;
        _supplierService = supplierService;
        _tripService = tripService;
        _umrahService = umrahService;
        _invoiceService = invoiceService;
        _permissionService = permissionService;
        _currentUserId = currentUserId;

        Console.WriteLine("Loading badges and permissions...");
        _ = LoadBadgeDataAsync();
        
        // Apply permissions asynchronously
        _ = ApplyPermissionsAsync();
        
        Console.WriteLine("InitializeServices complete");
    }

    private void InitializeMenu()
    {
        int yPosition = 10; // تقليل المسافة العلوية

        // Logo/Title - تقليل الارتفاع
        Panel logoPanel = new Panel
        {
            Location = new Point(0, yPosition),
            Size = new Size(350, 60), // تقليل من 80 إلى 60
            BackColor = ColorScheme.SidebarBg
        };

        Label logoLabel = new Label
        {
            Text = "جراس واي",
            Font = new Font("Cairo", 14F, FontStyle.Bold), // تقليل حجم الخط
            ForeColor = Color.White,
            AutoSize = false,
            Size = new Size(350, 30), // تقليل الارتفاع
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(0, 5)
        };

        Label subtitleLabel = new Label
        {
            Text = "النظام المحاسبي",
            Font = new Font("Cairo", 8F), // تقليل حجم الخط
            ForeColor = Color.FromArgb(180, 180, 180),
            AutoSize = false,
            Size = new Size(350, 20), // تقليل الارتفاع
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(0, 35)
        };

        logoPanel.Controls.Add(logoLabel);
        logoPanel.Controls.Add(subtitleLabel);
        this.Controls.Add(logoPanel);

        yPosition += 65; // تقليل المسافة

        // Menu Items
        AddMenuItem("dashboard", "🏠", "لوحة التحكم", ref yPosition, true);
        AddMenuItem("settings", "⚙️", "الإعدادات", ref yPosition);
        AddMenuItem("users", "👤", "إدارة المستخدمين", ref yPosition);
        AddMenuItem("audit", "📜", "سجل العمليات", ref yPosition);
        AddMenuItem("sessions", "🔗", "الجلسات النشطة", ref yPosition);
        AddMenuItem("accounts", "🌳", "شجرة الحسابات", ref yPosition);
        AddSeparator(ref yPosition);
        AddMenuItem("customers", "👥", "العملاء", ref yPosition);
        AddMenuItem("suppliers", "🏢", "الموردين", ref yPosition);
        AddSeparator(ref yPosition);
        AddMenuItem("reservations", "✈️", "الحجوزات", ref yPosition);
        AddMenuItem("flights", "🛫", "الطيران", ref yPosition);
        AddMenuItem("trips", "🚌", "الرحلات", ref yPosition);
        AddMenuItem("umrah", "🕌", "العمرة", ref yPosition);
        AddMenuItem("invoices", "📄", "الفواتير", ref yPosition);
        AddSeparator(ref yPosition);
        AddMenuItem("cashbox", "💰", "الخزنة", ref yPosition);
        AddMenuItem("banks", "🏦", "البنوك", ref yPosition);
        AddMenuItem("journals", "📋", "القيود اليومية", ref yPosition);
        AddMenuItem("calculator", "🧮", "الآلة الحاسبة", ref yPosition);
        AddSeparator(ref yPosition);
        AddMenuItem("reports", "📈", "التقارير", ref yPosition);
        AddMenuItem("accounting_reports", "📊", "التقارير المحاسبية", ref yPosition);
        AddSeparator(ref yPosition);
        AddMenuItem("filemanager", "📁", "إدارة الملفات", ref yPosition);
    }

    private void AddMenuItem(string id, string icon, string text, ref int yPosition, bool isActive = false)
    {
        SidebarMenuItem menuItem = new SidebarMenuItem(id, icon, text)
        {
            Location = new Point(10, yPosition),
            Size = new Size(330, 38) // تقليل الارتفاع من 50 إلى 38
        };

        if (isActive)
        {
            menuItem.SetActive(true);
            _activeItem = menuItem;
        }

        menuItem.Click += MenuItem_Click;
        _menuItems.Add(menuItem);
        this.Controls.Add(menuItem);

        yPosition += 40; // تقليل المسافة من 55 إلى 40
    }

    private void AddSeparator(ref int yPosition)
    {
        Panel separator = new Panel
        {
            Location = new Point(20, yPosition),
            Size = new Size(310, 1),
            BackColor = Color.FromArgb(60, 60, 60)
        };
        this.Controls.Add(separator);
        yPosition += 10; // تقليل المسافة من 15 إلى 10
    }

    private void MenuItem_Click(object? sender, EventArgs e)
    {
        if (sender is SidebarMenuItem clickedItem)
        {
            // Check if item is enabled
            if (!clickedItem.IsEnabled)
            {
                MessageBox.Show(
                    "ليس لديك صلاحيات للوصول إلى هذا القسم.\nالرجاء التواصل مع المسؤول للحصول على الصلاحيات المطلوبة.",
                    "صلاحيات غير كافية",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                );
                return;
            }

            // Deactivate previous item
            _activeItem?.SetActive(false);

            // Activate clicked item
            clickedItem.SetActive(true);
            _activeItem = clickedItem;

            // Raise event
            MenuItemClicked?.Invoke(this, clickedItem.Id);
        }
    }

    private async Task LoadBadgeDataAsync()
    {
        try
        {
            if (_reservationService != null)
            {
                var reservations = await _reservationService.GetAllReservationsAsync();
                var activeCount = reservations?.Count(r => r.Status == "Confirmed" || r.Status == "Pending") ?? 0;
                UpdateBadge("reservations", activeCount);
            }

            if (_customerService != null)
            {
                var customers = await _customerService.GetAllCustomersAsync();
                UpdateBadge("customers", customers?.Count ?? 0);
            }

            if (_supplierService != null)
            {
                var suppliers = await _supplierService.GetAllSuppliersAsync();
                UpdateBadge("suppliers", suppliers?.Count ?? 0);
            }

            if (_tripService != null)
            {
                var trips = await _tripService.GetAllTripsAsync();
                var activeTrips = trips?.Count(t => t.Status == Domain.Entities.TripStatus.Confirmed || 
                                                    t.Status == Domain.Entities.TripStatus.Unconfirmed) ?? 0;
                UpdateBadge("trips", activeTrips);
            }

            if (_umrahService != null)
            {
                var stats = await _umrahService.GetPackageStatisticsAsync();
                UpdateBadge("umrah", stats.ActivePackages);
            }

            if (_invoiceService != null)
            {
                var salesInvoices = await _invoiceService.GetUnpaidSalesInvoicesAsync();
                var purchaseInvoices = await _invoiceService.GetUnpaidPurchaseInvoicesAsync();
                var unpaidCount = (salesInvoices?.Count ?? 0) + (purchaseInvoices?.Count ?? 0);
                UpdateBadge("invoices", unpaidCount);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading badge data: {ex.Message}");
        }
    }

    private void UpdateBadge(string menuId, int count)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => UpdateBadge(menuId, count)));
            return;
        }

        var menuItem = _menuItems.FirstOrDefault(m => m.Id == menuId);
        menuItem?.SetBadgeCount(count);
    }

    private async Task ApplyPermissionsAsync()
    {
        try
        {
            if (_permissionService == null)
            {
                Console.WriteLine("⚠️ PermissionService is NULL!");
                return;
            }

            Console.WriteLine($"🔍 Getting permissions for user ID: {_currentUserId}");

            // Get user permissions by module
            var permissionsByModule = await _permissionService.GetUserPermissionsByModuleAsync(_currentUserId);

            Console.WriteLine($"📊 Found {permissionsByModule.Count} modules:");
            foreach (var module in permissionsByModule.Keys)
            {
                Console.WriteLine($"   ✓ Module: {module} ({permissionsByModule[module].Count} permissions)");
            }

            // تطبيق الصلاحيات على كل عنصر
            if (InvokeRequired)
            {
                Invoke(new Action(() => ApplyPermissionsSync(permissionsByModule)));
            }
            else
            {
                ApplyPermissionsSync(permissionsByModule);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error applying permissions: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private void ApplyPermissionsSync(Dictionary<string, List<PermissionType>> permissionsByModule)
    {
        Console.WriteLine("\n🔧 ApplyPermissionsSync started");
        Console.WriteLine($"   Modules received: {string.Join(", ", permissionsByModule.Keys)}");
        
        // ═══════════════════════════════════════════════════════
        // ✅ تحديد نوع المستخدم (الأولوية: Admin > Aviation > Operations > Accounting)
        // ═══════════════════════════════════════════════════════
        bool isAdmin = permissionsByModule.ContainsKey("System");
        bool hasAviation = permissionsByModule.ContainsKey("Aviation");
        bool hasUmrah = permissionsByModule.ContainsKey("Umrah");
        bool hasTrips = permissionsByModule.ContainsKey("Trips");
        bool hasAccounting = permissionsByModule.ContainsKey("Accounting");
        
        // تحديد دقيق لنوع المستخدم
        bool isAviationUser = (hasAviation || hasUmrah) && !isAdmin;
        bool isOperationsUser = hasTrips && !hasAviation && !hasUmrah && !hasAccounting && !isAdmin;
        bool isAccountingUser = hasAccounting && !hasAviation && !hasUmrah && !hasTrips && !isAdmin;
        
        Console.WriteLine($"   isAdmin: {isAdmin}");
        Console.WriteLine($"   hasAviation: {hasAviation}");
        Console.WriteLine($"   hasUmrah: {hasUmrah}");
        Console.WriteLine($"   hasTrips: {hasTrips}");
        Console.WriteLine($"   hasAccounting: {hasAccounting}");
        Console.WriteLine($"   → isAviationUser: {isAviationUser}");
        Console.WriteLine($"   → isOperationsUser: {isOperationsUser}");
        Console.WriteLine($"   → isAccountingUser: {isAccountingUser}");
        
        // ═══════════════════════════════════════════════════════
        // ✅ تطبيق الصلاحيات حسب نوع المستخدم (بالترتيب)
        // ═══════════════════════════════════════════════════════
        
        if (isAdmin)
        {
            // ════════════════════════════════════════════════════
            // 👑 Admin: كل الأقسام ENABLED
            // ════════════════════════════════════════════════════
            Console.WriteLine("👑 Admin User - All sections ENABLED");
            SetMenuItemEnabled("dashboard", true, true);
            SetMenuItemEnabled("calculator", true, true);
            SetMenuItemEnabled("settings", true, true);
            SetMenuItemEnabled("users", true, true);
            SetMenuItemEnabled("audit", true, true); // ✅ Audit for admins
            SetMenuItemEnabled("sessions", true, true); // ✅ Sessions for admins ONLY
            SetMenuItemEnabled("flights", true, true);
            SetMenuItemEnabled("umrah", true, true);
            SetMenuItemEnabled("trips", true, true);
            SetMenuItemEnabled("reservations", true, true);
            SetMenuItemEnabled("customers", true, true);
            SetMenuItemEnabled("suppliers", true, true);
            SetMenuItemEnabled("invoices", true, true);
            SetMenuItemEnabled("cashbox", true, true);
            SetMenuItemEnabled("banks", true, true);
            SetMenuItemEnabled("journals", true, true);
            SetMenuItemEnabled("accounts", true, true);
            SetMenuItemEnabled("reports", true, true);
            SetMenuItemEnabled("accounting_reports", true, true);
            SetMenuItemEnabled("filemanager", true, true); // ✅ إدارة الملفات للـ Admin فقط
        }
        else if (isAviationUser)
        {
            // ════════════════════════════════════════════════════
            // ✈️ Aviation & Umrah User
            // ENABLED: لوحة التحكم، الطيران، العمرة، الآلة الحاسبة فقط
            // DISABLED: باقي الأقسام (معروضة لكن معطلة)
            // ════════════════════════════════════════════════════
            Console.WriteLine("✈️ Aviation/Umrah User - Dashboard, Flights, Umrah, Calculator ENABLED, All others DISABLED");
            
            // الأقسام المتاحة فقط
            SetMenuItemEnabled("dashboard", true, true);
            SetMenuItemEnabled("calculator", true, true);
            SetMenuItemEnabled("flights", true, hasAviation);
            SetMenuItemEnabled("umrah", true, hasUmrah);
            
            // باقي الأقسام معروضة لكن معطلة (SHOW ALL but DISABLE)
            SetMenuItemEnabled("settings", true, false);
            SetMenuItemEnabled("users", true, false);
            SetMenuItemEnabled("audit", true, false);
            SetMenuItemEnabled("sessions", false, false); // ❌ HIDE for non-admins
            SetMenuItemEnabled("trips", true, false);
            SetMenuItemEnabled("reservations", true, false);
            SetMenuItemEnabled("customers", true, false);
            SetMenuItemEnabled("suppliers", true, false);
            SetMenuItemEnabled("invoices", true, false);
            SetMenuItemEnabled("cashbox", true, false);
            SetMenuItemEnabled("banks", true, false);
            SetMenuItemEnabled("journals", true, false);
            SetMenuItemEnabled("accounts", true, false);
            SetMenuItemEnabled("reports", true, false);
            SetMenuItemEnabled("accounting_reports", true, false);
            SetMenuItemEnabled("filemanager", false, false); // ❌ إدارة الملفات معطلة ومخفية
        }
        else if (isOperationsUser)
        {
            // ════════════════════════════════════════════════════
            // 🚌 Operations User (has Trips module only)
            // ENABLED: لوحة التحكم، الرحلات، الآلة الحاسبة
            // DISABLED: باقي الأقسام (معروضة لكن معطلة)
            // ════════════════════════════════════════════════════
            Console.WriteLine("🚌 Operations User - Dashboard, Trips, Calculator ENABLED, All others DISABLED");
            
            // الأقسام المتاحة
            SetMenuItemEnabled("dashboard", true, true);
            SetMenuItemEnabled("calculator", true, true);
            SetMenuItemEnabled("trips", true, true);
            
            // باقي الأقسام معروضة لكن معطلة (SHOW ALL but DISABLE)
            SetMenuItemEnabled("settings", true, false);
            SetMenuItemEnabled("users", true, false);
            SetMenuItemEnabled("audit", true, false);
            SetMenuItemEnabled("sessions", false, false); // ❌ HIDE for non-admins
            SetMenuItemEnabled("flights", true, false);
            SetMenuItemEnabled("umrah", true, false);
            SetMenuItemEnabled("reservations", true, false);
            SetMenuItemEnabled("customers", true, false);
            SetMenuItemEnabled("suppliers", true, false);
            SetMenuItemEnabled("invoices", true, false);
            SetMenuItemEnabled("cashbox", true, false);
            SetMenuItemEnabled("banks", true, false);
            SetMenuItemEnabled("journals", true, false);
            SetMenuItemEnabled("accounts", true, false);
            SetMenuItemEnabled("reports", true, false);
            SetMenuItemEnabled("accounting_reports", true, false);
            SetMenuItemEnabled("filemanager", false, false); // ❌ إدارة الملفات معطلة ومخفية
        }
        else if (isAccountingUser)
        {
            // ════════════════════════════════════════════════════
            // 💰 Accounting User: حسب الصلاحيات التفصيلية
            // ════════════════════════════════════════════════════
            Console.WriteLine("💰 Accounting User - Permissions based access");
            
            var accountingPerms = permissionsByModule["Accounting"];
            
            SetMenuItemEnabled("dashboard", true, true);
            SetMenuItemEnabled("calculator", true, true);
            SetMenuItemEnabled("customers", true, accountingPerms.Any(p => p == PermissionType.ViewCustomers));
            SetMenuItemEnabled("suppliers", true, accountingPerms.Any(p => p == PermissionType.ViewSuppliers));
            SetMenuItemEnabled("invoices", true, accountingPerms.Any(p => p == PermissionType.ViewInvoices));
            SetMenuItemEnabled("cashbox", true, accountingPerms.Any(p => p == PermissionType.ViewCashBox));
            SetMenuItemEnabled("banks", true, accountingPerms.Any(p => p == PermissionType.ViewBankAccounts));
            SetMenuItemEnabled("journals", true, accountingPerms.Any(p => p == PermissionType.ViewJournalEntries));
            SetMenuItemEnabled("accounts", true, accountingPerms.Any(p => p == PermissionType.ViewChartOfAccounts));
            SetMenuItemEnabled("accounting_reports", true, accountingPerms.Any(p => p == PermissionType.ViewFinancialReports));
            
            // الأقسام غير المحاسبية معطلة
            SetMenuItemEnabled("settings", true, false);
            SetMenuItemEnabled("users", true, false);
            SetMenuItemEnabled("audit", true, false);
            SetMenuItemEnabled("sessions", false, false); // ❌ HIDE for non-admins
            SetMenuItemEnabled("flights", true, false);
            SetMenuItemEnabled("umrah", true, false);
            SetMenuItemEnabled("trips", true, false);
            SetMenuItemEnabled("reservations", true, false);
            SetMenuItemEnabled("reports", true, false);
            SetMenuItemEnabled("filemanager", false, false); // ❌ إدارة الملفات معطلة ومخفية
        }
        else
        {
            // ════════════════════════════════════════════════════
            // ❓ مستخدم بدون صلاحيات محددة
            // ════════════════════════════════════════════════════
            Console.WriteLine("❓ Unknown user type - minimal access");
            
            SetMenuItemEnabled("dashboard", true, true);
            SetMenuItemEnabled("calculator", true, true);
            
            // باقي الأقسام معطلة
            SetMenuItemEnabled("settings", true, false);
            SetMenuItemEnabled("users", true, false);
            SetMenuItemEnabled("audit", true, false);
            SetMenuItemEnabled("sessions", false, false); // ❌ HIDE for non-admins
            SetMenuItemEnabled("flights", true, false);
            SetMenuItemEnabled("umrah", true, false);
            SetMenuItemEnabled("trips", true, false);
            SetMenuItemEnabled("reservations", true, false);
            SetMenuItemEnabled("customers", true, false);
            SetMenuItemEnabled("suppliers", true, false);
            SetMenuItemEnabled("invoices", true, false);
            SetMenuItemEnabled("cashbox", true, false);
            SetMenuItemEnabled("banks", true, false);
            SetMenuItemEnabled("journals", true, false);
            SetMenuItemEnabled("accounts", true, false);
            SetMenuItemEnabled("reports", true, false);
            SetMenuItemEnabled("accounting_reports", true, false);
            SetMenuItemEnabled("filemanager", false, false); // ❌ إدارة الملفات معطلة ومخفية
        }
        
        Console.WriteLine("🔧 ApplyPermissionsSync completed\n");
    }

    private void SetMenuItemEnabled(string menuId, bool visible, bool enabled)
    {
        var menuItem = _menuItems.FirstOrDefault(m => m.Id == menuId);
        if (menuItem != null)
        {
            menuItem.Visible = visible;
            menuItem.SetEnabled(enabled);
            
            string status = visible ? (enabled ? "✓ ENABLED" : "⊘ DISABLED") : "✗ HIDDEN";
            Console.WriteLine($"   {menuId}: {status}");
        }
        else
        {
            Console.WriteLine($"   ⚠️ Menu item '{menuId}' NOT FOUND!");
        }
    }
}

// SidebarMenuItem Class
public class SidebarMenuItem : Panel
{
    public string Id { get; }
    private readonly Label _iconLabel;
    private readonly Label _textLabel;
    private Label? _badgeLabel;
    private bool _isActive;
    private bool _isEnabled = true;

    public SidebarMenuItem(string id, string icon, string text)
    {
        Id = id;
        this.BackColor = ColorScheme.SidebarBg;
        this.Cursor = Cursors.Hand;
        this.DoubleBuffered = true;

        // Icon Label - تقليل الحجم
        _iconLabel = new Label
        {
            Text = icon,
            Font = new Font("Segoe UI Emoji", 14F), // تقليل من 16 إلى 14
            ForeColor = Color.White,
            AutoSize = false,
            Size = new Size(35, 38), // تقليل الارتفاع
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(5, 0),
            BackColor = Color.Transparent
        };

        // Text Label - تقليل الحجم
        _textLabel = new Label
        {
            Text = text,
            Font = new Font("Cairo", 10F, FontStyle.Regular), // تقليل من 11 إلى 10
            ForeColor = Color.White,
            AutoSize = false,
            Size = new Size(240, 38), // تقليل الارتفاع
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(45, 0),
            BackColor = Color.Transparent
        };

        this.Controls.Add(_iconLabel);
        this.Controls.Add(_textLabel);

        // Hover effects
        this.MouseEnter += (s, e) =>
        {
            if (_isEnabled && !_isActive)
                this.BackColor = ColorScheme.SidebarHover;
        };

        this.MouseLeave += (s, e) =>
        {
            if (!_isActive)
            {
                if (_isEnabled)
                    this.BackColor = ColorScheme.SidebarBg;
                else
                    this.BackColor = Color.FromArgb(40, 40, 40);
            }
        };

        // Click passthrough to child controls
        _iconLabel.Click += (s, e) => this.OnClick(e);
        _textLabel.Click += (s, e) => this.OnClick(e);
    }

    public void SetEnabled(bool enabled)
    {
        _isEnabled = enabled;
        
        if (enabled)
        {
            // Enabled state - normal colors
            _iconLabel.ForeColor = Color.White;
            _textLabel.ForeColor = Color.White;
            this.Cursor = Cursors.Hand;
            
            if (!_isActive)
                this.BackColor = ColorScheme.SidebarBg;
        }
        else
        {
            // Disabled state - grayed out
            _iconLabel.ForeColor = Color.FromArgb(100, 100, 100);
            _textLabel.ForeColor = Color.FromArgb(100, 100, 100);
            this.BackColor = Color.FromArgb(40, 40, 40);
            this.Cursor = Cursors.No;
        }
    }
    
    public bool IsEnabled => _isEnabled;

    public void SetActive(bool isActive)
    {
        _isActive = isActive;
        
        if (isActive && _isEnabled)
        {
            this.BackColor = ColorScheme.Primary;
            _textLabel.Font = new Font("Cairo", 10F, FontStyle.Bold);
        }
        else if (_isEnabled)
        {
            this.BackColor = ColorScheme.SidebarBg;
            _textLabel.Font = new Font("Cairo", 10F, FontStyle.Regular);
        }
        else
        {
            // Keep disabled styling
            this.BackColor = Color.FromArgb(40, 40, 40);
            _textLabel.Font = new Font("Cairo", 10F, FontStyle.Regular);
        }
    }

    public void SetBadgeCount(int count)
    {
        // الشرطة الحمراء ألغيت كاملة
        _badgeLabel?.Dispose();
        _badgeLabel = null;
    }
}
