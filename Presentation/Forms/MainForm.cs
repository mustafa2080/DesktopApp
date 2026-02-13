using GraceWay.AccountingSystem.Presentation;
using GraceWay.AccountingSystem.Presentation.Controls;
using GraceWay.AccountingSystem.Presentation.Forms;
using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class MainForm : Form
{
    private readonly string _currentUserName;
    private readonly int _currentUserId;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _sessionId; // ✅ Store session ID
    private SidebarControl? _sidebar;
    private HeaderControl? _header;
    private Panel? _contentPanel;
    private DashboardControl? _dashboard;
    private System.Windows.Forms.Timer? _heartbeatTimer; // ✅ Heartbeat for session activity

    public MainForm(string userName, int userId, IServiceProvider serviceProvider, string sessionId)
    {
        _currentUserName = userName;
        _currentUserId = userId;
        _serviceProvider = serviceProvider;
        _sessionId = sessionId; // ✅ Store session ID
        InitializeComponent();
        InitializeCustomComponents();
        SetupMainLayout();
        
        // ✅ Start heartbeat timer to update session activity every 60 seconds
        _heartbeatTimer = new System.Windows.Forms.Timer();
        _heartbeatTimer.Interval = 60000; // 60 seconds
        _heartbeatTimer.Tick += (s, e) => SessionManager.Instance.UpdateActivity(_sessionId);
        _heartbeatTimer.Start();
    }

    private void InitializeCustomComponents()
    {
        // Setup form properties
        this.Text = "نظام جريس واي المحاسبي";
        this.Size = new Size(1400, 900);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.WindowState = FormWindowState.Maximized;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F, FontStyle.Regular);

        // Enable modern look
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.MinimumSize = new Size(1200, 700);

        try
        {
            if (Program.AppIcon != null)
                this.Icon = Program.AppIcon;
        }
        catch
        {
            // Ignore icon loading errors
        }
    }

    private void SetupMainLayout()
    {
        // Create main container
        TableLayoutPanel mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = ColorScheme.Background,
            Padding = new Padding(0),
            RightToLeft = RightToLeft.Yes // إضافة هذا
        };

        // Column styles: Sidebar | Content (RTL)
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        // Row styles: Header | Content
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // Create Sidebar
        _sidebar = new SidebarControl();
        _sidebar.MenuItemClicked += Sidebar_MenuItemClicked;
        
        // Initialize sidebar services for badges
        var reservationService = _serviceProvider.GetRequiredService<IReservationService>();
        var customerService = _serviceProvider.GetRequiredService<ICustomerService>();
        var supplierService = _serviceProvider.GetRequiredService<ISupplierService>();
        var tripService = _serviceProvider.GetRequiredService<ITripService>();
        var umrahService = _serviceProvider.GetRequiredService<IUmrahService>();
        var invoiceService = _serviceProvider.GetRequiredService<IInvoiceService>();
        var permissionService = _serviceProvider.GetRequiredService<IPermissionService>();
        
        _sidebar.InitializeServices(
            reservationService,
            customerService,
            supplierService,
            tripService,
            umrahService,
            invoiceService,
            permissionService,
            _currentUserId
        );
        
        // Sidebar في العمود 0 (اليمين في RTL)
        mainLayout.Controls.Add(_sidebar, 0, 0);
        mainLayout.SetRowSpan(_sidebar, 2);

        // Create Header
        _header = new HeaderControl(_currentUserName);
        _header.LogoutClicked += Header_LogoutClicked;
        _header.InitializeServices(reservationService, invoiceService);
        mainLayout.Controls.Add(_header, 1, 0);

        // Create Content Panel
        _contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ColorScheme.Background,
            Padding = new Padding(20)
        };
        mainLayout.Controls.Add(_contentPanel, 1, 1);

        // Add main layout to form first
        this.Controls.Add(mainLayout);
        
        // Force handle creation
        this.CreateControl();
        if (_contentPanel != null)
        {
            var handle = _contentPanel.Handle; // Force handle creation
        }
        
        // Show Dashboard after layout is added and handles created
        this.BeginInvoke(new Action(() => ShowDashboard()));
    }

    private void ShowDashboard()
    {
        if (_contentPanel == null || !_contentPanel.IsHandleCreated)
        {
            Console.WriteLine("⚠️ ContentPanel not ready yet, skipping ShowDashboard");
            return;
        }
        
        _contentPanel.Controls.Clear();
        
        try
        {
            var reservationService = _serviceProvider.GetRequiredService<IReservationService>();
            var cashBoxService = _serviceProvider.GetRequiredService<ICashBoxService>();
            var customerService = _serviceProvider.GetRequiredService<ICustomerService>();
            var invoiceService = _serviceProvider.GetRequiredService<IInvoiceService>();
            var tripService = _serviceProvider.GetRequiredService<ITripService>();
            var umrahService = _serviceProvider.GetRequiredService<IUmrahService>();
            var supplierService = _serviceProvider.GetRequiredService<ISupplierService>();
            
            _dashboard = new DashboardControl(
                reservationService, 
                cashBoxService, 
                customerService, 
                invoiceService,
                tripService,
                umrahService,
                supplierService);
            _dashboard.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(_dashboard);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error showing dashboard: {ex.Message}");
        }
    }

    private void ShowCashBox()
    {
        // ✅ عرض قسم الخزنة في نفس المكان القديم (داخل contentPanel)
        _contentPanel?.Controls.Clear();
        
        var cashBoxService = _serviceProvider.GetRequiredService<ICashBoxService>();
        var authService = _serviceProvider.GetRequiredService<IAuthService>();
        
        // ✅ فتح الخزنة كـ embedded form داخل الـ content panel
        CashBoxForm cashBoxForm = new CashBoxForm(cashBoxService, authService, _currentUserId, _serviceProvider)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        
        _contentPanel?.Controls.Add(cashBoxForm);
        cashBoxForm.Show();
    }
    
    private void ShowInvoices()
    {
        _contentPanel?.Controls.Clear();
        var invoiceService = _serviceProvider.GetRequiredService<IInvoiceService>();
        var cashBoxService = _serviceProvider.GetRequiredService<ICashBoxService>();
        var invoicesForm = new InvoicesListForm(invoiceService, cashBoxService, _currentUserId)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        _contentPanel?.Controls.Add(invoicesForm);
        invoicesForm.Show();
    }
    
    private void ShowReservations()
    {
        _contentPanel?.Controls.Clear();
        
        // Get fresh service instances from pool
        var tripService = _serviceProvider.GetRequiredService<ITripService>();
        var reservationsForm = new ReservationsListForm(tripService, _serviceProvider, _currentUserId)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        
        _contentPanel?.Controls.Add(reservationsForm);
        reservationsForm.Show();
    }
    
    private void ShowFlights()
    {
        if (_contentPanel == null) return;
        
        _contentPanel.Controls.Clear();
        
        try
        {
            var flightBookingService = _serviceProvider.GetRequiredService<IFlightBookingService>();
            var flightsForm = new FlightBookingsListForm(flightBookingService, _serviceProvider)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            _contentPanel.Controls.Add(flightsForm);
            flightsForm.Show();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error showing flights: {ex.Message}");
            MessageBox.Show($"حدث خطأ عند فتح قسم الطيران: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }
    
    private void ShowTrips()
    {
        if (_contentPanel == null) return;
        
        _contentPanel.Controls.Clear();
        
        try
        {
            var tripService = _serviceProvider.GetRequiredService<ITripService>();
            var authService = _serviceProvider.GetRequiredService<IAuthService>();
            var tripsForm = new TripsListForm(tripService, authService, _currentUserId)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            _contentPanel.Controls.Add(tripsForm);
            tripsForm.Show();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error showing trips: {ex.Message}");
            MessageBox.Show($"حدث خطأ عند فتح قسم الرحلات: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }
    
    private void ShowUmrah()
    {
        if (_contentPanel == null) return;
        
        _contentPanel.Controls.Clear();
        
        try
        {
            var umrahService = _serviceProvider.GetRequiredService<IUmrahService>();
            var umrahForm = new UmrahPackagesListForm(umrahService, _currentUserId)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            _contentPanel.Controls.Add(umrahForm);
            umrahForm.Show();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error showing umrah: {ex.Message}");
            MessageBox.Show($"حدث خطأ عند فتح قسم العمرة: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }
    
    private void ShowReports()
    {
        _contentPanel?.Controls.Clear();
        
        // Create TabControl for different report types
        var tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            RightToLeftLayout = true
        };
        
        // Tab 1: Invoice Reports
        var invoiceTab = new TabPage("📄 تقارير الفواتير");
        var invoiceService = _serviceProvider.GetRequiredService<IInvoiceService>();
        var exportService = _serviceProvider.GetRequiredService<IExportService>();
        var invoiceReportsForm = new InvoiceReportsForm(invoiceService, exportService)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        invoiceTab.Controls.Add(invoiceReportsForm);
        invoiceReportsForm.Show();
        tabControl.TabPages.Add(invoiceTab);
        
        // Tab 2: Umrah Reports
        var umrahTab = new TabPage("🕌 تقارير العمرة");
        var umrahService = _serviceProvider.GetRequiredService<IUmrahService>();
        var umrahReportsForm = new Reports.UmrahReportsForm(umrahService, exportService)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        umrahTab.Controls.Add(umrahReportsForm);
        umrahReportsForm.Show();
        tabControl.TabPages.Add(umrahTab);
        
        _contentPanel?.Controls.Add(tabControl);
    }
    
    private void ShowCustomers()
    {
        _contentPanel?.Controls.Clear();
        var customerService = _serviceProvider.GetRequiredService<ICustomerService>();
        var customersForm = new CustomersListForm(customerService, _currentUserId)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        _contentPanel?.Controls.Add(customersForm);
        customersForm.Show();
    }
    
    private void ShowSuppliers()
    {
        _contentPanel?.Controls.Clear();
        var supplierService = _serviceProvider.GetRequiredService<ISupplierService>();
        var suppliersForm = new SuppliersListForm(supplierService, _currentUserId)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        _contentPanel?.Controls.Add(suppliersForm);
        suppliersForm.Show();
    }
    
    private void ShowChartOfAccounts()
    {
        _contentPanel?.Controls.Clear();
        var accountService = _serviceProvider.GetRequiredService<IAccountService>();
        var accountsForm = new ChartOfAccountsForm(accountService, _currentUserId)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        _contentPanel?.Controls.Add(accountsForm);
        accountsForm.Show();
    }
    
    private void ShowSettings()
    {
        _contentPanel?.Controls.Clear();
        
        // Create TabControl for Settings
        var tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            RightToLeftLayout = true
        };
        
        // Tab 1: Company Settings
        var companyTab = new TabPage("🏢 معلومات الشركة");
        try
        {
            var settingService = _serviceProvider.GetRequiredService<ISettingService>();
            var companyForm = new CompanySettingsForm(settingService, _currentUserId)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            companyTab.Controls.Add(companyForm);
            companyForm.Show();
            tabControl.TabPages.Add(companyTab);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل إعدادات الشركة: {ex.Message}\n\n{ex.StackTrace}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
        
        // Tab 2: Invoice Settings
        var invoiceTab = new TabPage("📄 إعدادات الفواتير");
        try
        {
            var settingService = _serviceProvider.GetRequiredService<ISettingService>();
            var invoiceForm = new InvoiceSettingsForm(settingService, _currentUserId)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            invoiceTab.Controls.Add(invoiceForm);
            invoiceForm.Show();
            tabControl.TabPages.Add(invoiceTab);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل إعدادات الفواتير: {ex.Message}\n\n{ex.StackTrace}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }

        // Tab 3: Fiscal Year Settings
        var fiscalTab = new TabPage("📅 السنة المالية");
        try
        {
            var dbContext = _serviceProvider.GetRequiredService<Infrastructure.Data.AppDbContext>();
            var settingService = _serviceProvider.GetRequiredService<ISettingService>();
            var fiscalForm = new FiscalYearSettingsForm(dbContext, settingService)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            fiscalTab.Controls.Add(fiscalForm);
            fiscalForm.Show();
            tabControl.TabPages.Add(fiscalTab);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل إعدادات السنة المالية: {ex.Message}\n\n{ex.StackTrace}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }

        // Tab 4: Backup Management
        var backupTab = new TabPage("💾 النسخ الاحتياطية");
        try
        {
            var backupService = _serviceProvider.GetRequiredService<Application.Services.Backup.IBackupService>();
            var backupForm = new BackupManagementForm(backupService)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            backupTab.Controls.Add(backupForm);
            backupForm.Show();
            tabControl.TabPages.Add(backupTab);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل إدارة النسخ الاحتياطية: {ex.Message}\n\n{ex.StackTrace}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
        
        _contentPanel?.Controls.Add(tabControl);
    }

    private void ShowUserManagement()
    {
        _contentPanel?.Controls.Clear();
        
        try
        {
            var dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
            
            var userManagementForm = new Admin.UserManagementForm(dbContext)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            _contentPanel?.Controls.Add(userManagementForm);
            userManagementForm.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل إدارة المستخدمين: {ex.Message}\n\n{ex.StackTrace}", 
                "خطأ",
                MessageBoxButtons.OK, 
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private void Sidebar_MenuItemClicked(object? sender, string menuItem)
    {
        if (_contentPanel == null)
        {
            Console.WriteLine("⚠️ ContentPanel is null, ignoring menu click");
            return;
        }
        
        try
        {
            _contentPanel.Controls.Clear();

            switch (menuItem)
            {
                case "dashboard":
                    ShowDashboard();
                    break;

                case "settings":
                    ShowSettings();
                    break;

                case "users":
                    ShowUserManagement();
                    break;
                
                case "audit":
                    ShowAuditLogs();
                    break;

                case "sessions":
                    ShowActiveSessions();
                    break;

                case "accounts":
                    ShowChartOfAccounts();
                    break;

                case "customers":
                    ShowCustomers();
                    break;

                case "suppliers":
                    ShowSuppliers();
                    break;

                case "reservations":
                    ShowReservations();
                    break;

                case "flights":
                    ShowFlights();
                    break;

                case "trips":
                    ShowTrips();
                    break;

                case "umrah":
                    ShowUmrah();
                    break;

                case "invoices":
                    ShowInvoices();
                    break;

                case "cashbox":
                    ShowCashBox();
                    break;

                case "banks":
                    ShowBanks();
                    break;

                case "journals":
                    ShowJournalEntries();
                    break;

                case "calculator":
                    ShowCalculator();
                    break;

                case "reports":
                    ShowReports();
                    break;

                case "accounting_reports":
                    ShowAccountingReports();
                    break;

                default:
                    ShowDashboard();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in menu click handler: {ex.Message}");
            MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private void ShowPlaceholder(string title, string icon)
    {
        Panel placeholderPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };

        Label iconLabel = new Label
        {
            Text = icon,
            Font = new Font("Segoe UI Emoji", 64F),
            AutoSize = false,
            Size = new Size(200, 100),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(
                (placeholderPanel.Width - 200) / 2,
                (placeholderPanel.Height - 200) / 2
            )
        };

        Label titleLabel = new Label
        {
            Text = title,
            Font = new Font("Cairo", 18F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = false,
            Size = new Size(400, 40),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(
                (placeholderPanel.Width - 400) / 2,
                iconLabel.Bottom + 10
            )
        };

        Label comingSoonLabel = new Label
        {
            Text = "قريباً...",
            Font = new Font("Cairo", 12F),
            ForeColor = Color.Gray,
            AutoSize = false,
            Size = new Size(200, 30),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(
                (placeholderPanel.Width - 200) / 2,
                titleLabel.Bottom + 5
            )
        };

        placeholderPanel.Controls.Add(iconLabel);
        placeholderPanel.Controls.Add(titleLabel);
        placeholderPanel.Controls.Add(comingSoonLabel);

        // Center controls on resize
        placeholderPanel.Resize += (s, e) =>
        {
            iconLabel.Location = new Point((placeholderPanel.Width - 200) / 2, (placeholderPanel.Height - 200) / 2);
            titleLabel.Location = new Point((placeholderPanel.Width - 400) / 2, iconLabel.Bottom + 10);
            comingSoonLabel.Location = new Point((placeholderPanel.Width - 200) / 2, titleLabel.Bottom + 5);
        };

        _contentPanel?.Controls.Add(placeholderPanel);
    }
    
    private void ShowAuditLogs()
    {
        _contentPanel?.Controls.Clear();
        var auditService = _serviceProvider.GetRequiredService<IAuditService>();
        var auditForm = new Admin.AuditLogsForm(auditService)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        _contentPanel?.Controls.Add(auditForm);
        auditForm.Show();
    }

    private void ShowActiveSessions()
    {
        _contentPanel?.Controls.Clear();
        var sessionsForm = new ActiveSessionsForm
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        _contentPanel?.Controls.Add(sessionsForm);
        sessionsForm.Show();
    }

    private void ShowJournalEntries()
    {
        _contentPanel?.Controls.Clear();
        var dbContext = _serviceProvider.GetRequiredService<Infrastructure.Data.AppDbContext>();
        var journalForm = new JournalEntriesListForm(dbContext, _currentUserId)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        _contentPanel?.Controls.Add(journalForm);
        journalForm.Show();
    }

    private void ShowBanks()
    {
        _contentPanel?.Controls.Clear();
        var dbContext = _serviceProvider.GetRequiredService<Infrastructure.Data.AppDbContext>();
        var banksForm = new BankAccountsForm(dbContext, _currentUserId)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        _contentPanel?.Controls.Add(banksForm);
        banksForm.Show();
    }

    private void ShowAccountingReports()
    {
        _contentPanel?.Controls.Clear();
        
        // Create TabControl for Accounting Reports
        var tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            RightToLeftLayout = true
        };
        
        var dbContextFactory = _serviceProvider.GetRequiredService<Microsoft.EntityFrameworkCore.IDbContextFactory<Infrastructure.Data.AppDbContext>>();
        var exportService = _serviceProvider.GetRequiredService<IExportService>();
        
        // Tab 1: Trial Balance
        var trialBalanceTab = new TabPage("⚖️ ميزان المراجعة");
        var exportService1 = _serviceProvider.GetRequiredService<IExportService>();
        var trialBalanceForm = new TrialBalanceReportForm(dbContextFactory, exportService1)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        trialBalanceTab.Controls.Add(trialBalanceForm);
        trialBalanceForm.Show();
        tabControl.TabPages.Add(trialBalanceTab);
        
        // Tab 2: Income Statement
        var incomeTab = new TabPage("📊 قائمة الدخل");
        var exportService2 = _serviceProvider.GetRequiredService<IExportService>();
        var incomeForm = new IncomeStatementForm(dbContextFactory, exportService2)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        incomeTab.Controls.Add(incomeForm);
        incomeForm.Show();
        tabControl.TabPages.Add(incomeTab);
        
        // Tab 3: Balance Sheet
        var balanceSheetTab = new TabPage("💼 الميزانية العمومية");
        var balanceSheetForm = new BalanceSheetForm(dbContextFactory)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        balanceSheetTab.Controls.Add(balanceSheetForm);
        balanceSheetForm.Show();
        tabControl.TabPages.Add(balanceSheetTab);
        
        // Tab 4: CashBox Income Report
        var cashBoxIncomeTab = new TabPage("💰 تقرير الإيرادات");
        var cashBoxIncomeForm = new CashBoxIncomeReportForm(dbContextFactory, _currentUserId)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        cashBoxIncomeTab.Controls.Add(cashBoxIncomeForm);
        cashBoxIncomeForm.Show();
        tabControl.TabPages.Add(cashBoxIncomeTab);
        
        // Tab 5: CashBox Expense Report
        var cashBoxExpenseTab = new TabPage("💸 تقرير المصروفات");
        var cashBoxExpenseForm = new CashBoxExpenseReportForm(dbContextFactory, _currentUserId)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        cashBoxExpenseTab.Controls.Add(cashBoxExpenseForm);
        cashBoxExpenseForm.Show();
        tabControl.TabPages.Add(cashBoxExpenseTab);
        
        // Tab 6: Trip Profitability
        var tripProfitTab = new TabPage("💰 ربحية الرحلات");
        var tripProfitForm = new TripProfitabilityForm(dbContextFactory, exportService)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        tripProfitTab.Controls.Add(tripProfitForm);
        tripProfitForm.Show();
        tabControl.TabPages.Add(tripProfitTab);
        
        // Tab 7: Umrah Profitability
        var umrahProfitTab = new TabPage("🕌 ربحية العمرة");
        var umrahService = _serviceProvider.GetRequiredService<IUmrahService>();
        var umrahProfitForm = new Reports.UmrahProfitabilityReport(umrahService, exportService)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        umrahProfitTab.Controls.Add(umrahProfitForm);
        umrahProfitForm.Show();
        tabControl.TabPages.Add(umrahProfitTab);
        
        _contentPanel?.Controls.Add(tabControl);
    }

    private void ShowCalculator()
    {
        if (_contentPanel == null) return;
        
        _contentPanel.Controls.Clear();
        
        try
        {
            var calculatorForm = new AccountingCalculatorForm
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            
            _contentPanel.Controls.Add(calculatorForm);
            calculatorForm.Show();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error showing calculator: {ex.Message}");
            MessageBox.Show($"حدث خطأ عند فتح الآلة الحاسبة: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private void Header_LogoutClicked(object? sender, EventArgs e)
    {
        DialogResult result = MessageBox.Show(
            "هل أنت متأكد من تسجيل الخروج؟",
            "تأكيد تسجيل الخروج",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2,
            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
        );

        if (result == DialogResult.Yes)
        {
            Console.WriteLine("🚪 User logging out - clearing current user session");
            
            // ✅ End the session
            SessionManager.Instance.EndSession(_sessionId);
            System.Diagnostics.Debug.WriteLine($"✅ Session ended: {_sessionId}");
            
            // ✅ CRITICAL: Clear current user from AuthService (Singleton)
            var authService = _serviceProvider.GetRequiredService<IAuthService>();
            authService.Logout();
            Console.WriteLine("✓ AuthService.Logout() called - CurrentUser cleared");
            
            // ✅ Get fresh LoginForm from ServiceProvider with clean AuthService
            var loginForm = _serviceProvider.GetRequiredService<LoginForm>();
            
            // Close this MainForm and show LoginForm
            this.Hide();
            loginForm.FormClosed += (s, args) => {
                this.Close();
                System.Windows.Forms.Application.Exit(); // Ensure clean exit
            };
            loginForm.Show();
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // ✅ Stop heartbeat timer
        try
        {
            _heartbeatTimer?.Stop();
            _heartbeatTimer?.Dispose();
        }
        catch { }
        
        // ✅ End session when form closes
        try
        {
            SessionManager.Instance.EndSession(_sessionId);
            System.Diagnostics.Debug.WriteLine($"✅ Session ended on form close: {_sessionId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error ending session: {ex.Message}");
        }
        
        base.OnFormClosing(e);
    }
}
