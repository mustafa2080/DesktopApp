using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using System.Drawing.Printing;

namespace GraceWay.AccountingSystem.Presentation.Forms;

/// <summary>
/// 📊 تقارير الفواتير المحترفة - نسخة محسّنة
/// </summary>
public partial class InvoiceReportsForm : Form
{
    private readonly IInvoiceService _invoiceService;
    private readonly IExportService _exportService;
    
    private DateTime _lastRefreshTime = DateTime.MinValue;
    
    private TabControl _tabControl = null!;
    private DateTimePicker _startDatePicker = null!;
    private DateTimePicker _endDatePicker = null!;
    private Button _generateButton = null!;
    private Button _exportExcelButton = null!;
    private Button _exportPdfButton = null!;
    private Button _printButton = null!;
    private Button _refreshButton = null!;
    
    // Filter Controls
    private TextBox _searchBox = null!;
    private ComboBox _statusFilterCombo = null!;
    private ComboBox _customerFilterCombo = null!;
    private ComboBox _supplierFilterCombo = null!;
    private Button _clearFiltersButton = null!;
    
    // Data Grids
    private DataGridView _salesReportGrid = null!;
    private DataGridView _purchaseReportGrid = null!;
    
    // Summary Panels
    private Panel _salesSummaryPanel = null!;
    private Panel _purchaseSummaryPanel = null!;
    private Panel _analyticsSummaryPanel = null!;
    
    // Data cache
    private List<SalesInvoice> _allSalesInvoices = new();
    private List<PurchaseInvoice> _allPurchaseInvoices = new();
    
    public InvoiceReportsForm(IInvoiceService invoiceService, IExportService exportService)
    {
        _invoiceService = invoiceService;
        _exportService = exportService;
        
        InitializeComponent();
        InitializeCustomControls();
        _ = LoadInitialDataAsync();
        
        // ✅ تحديث تلقائي عند تفعيل الفورم
        this.Activated += async (s, e) => await RefreshDataIfNeededAsync();
    }
    
    private void InitializeComponent()
    {
        this.Text = "📊 تقارير الفواتير";
        this.Size = new Size(1650, 980);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = Color.FromArgb(245, 247, 250);
        this.Font = new Font("Cairo", 10F);
        this.WindowState = FormWindowState.Maximized;
    }
    
    private void InitializeCustomControls()
    {
        // ═══════════════════════════════════════════
        // Header Panel - شريط العنوان والأدوات
        // ═══════════════════════════════════════════
        Panel headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 160,
            BackColor = Color.White,
            Padding = new Padding(25, 15, 25, 15)
        };
        
        // Title with icon
        Label titleLabel = new Label
        {
            Text = "📊 تقارير الفواتير",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(25, 15)
        };
        headerPanel.Controls.Add(titleLabel);
        
        Label subtitleLabel = new Label
        {
            Text = "تحليل شامل ومفصل للمبيعات والمشتريات",
            Font = new Font("Cairo", 9.5F),
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(25, 48)
        };
        headerPanel.Controls.Add(subtitleLabel);
        
        // Date Range Section
        int yPos = 85;
        Label startLabel = new Label
        {
            Text = "📅 من:",
            Font = new Font("Cairo", 9.5F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(25, yPos)
        };
        headerPanel.Controls.Add(startLabel);
        
        _startDatePicker = new DateTimePicker
        {
            Font = new Font("Cairo", 9.5F),
            Size = new Size(170, 32),
            Location = new Point(100, yPos - 2),
            Format = DateTimePickerFormat.Short
        };
        _startDatePicker.Value = DateTime.Now.AddMonths(-1);
        headerPanel.Controls.Add(_startDatePicker);
        
        Label endLabel = new Label
        {
            Text = "📅 إلى:",
            Font = new Font("Cairo", 9.5F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(290, yPos)
        };
        headerPanel.Controls.Add(endLabel);
        
        _endDatePicker = new DateTimePicker
        {
            Font = new Font("Cairo", 9.5F),
            Size = new Size(170, 32),
            Location = new Point(355, yPos - 2),
            Format = DateTimePickerFormat.Short
        };
        headerPanel.Controls.Add(_endDatePicker);
        
        // Action Buttons
        _generateButton = CreateStyledButton("📊 إنشاء التقرير", ColorScheme.Primary, new Point(545, yPos - 2));
        _generateButton.Click += GenerateReport_Click;
        headerPanel.Controls.Add(_generateButton);
        
        _refreshButton = CreateStyledButton("🔄 تحديث", Color.FromArgb(76, 175, 80), new Point(725, yPos - 2));
        _refreshButton.Click += async (s, e) => 
        {
            _lastRefreshTime = DateTime.MinValue;
            await LoadInitialDataAsync();
        };
        headerPanel.Controls.Add(_refreshButton);
        
        _exportExcelButton = CreateStyledButton("📥 Excel", Color.FromArgb(33, 150, 83), new Point(905, yPos - 2));
        _exportExcelButton.Click += ExportToExcel_Click;
        headerPanel.Controls.Add(_exportExcelButton);
        
        _exportPdfButton = CreateStyledButton("📄 PDF", Color.FromArgb(211, 47, 47), new Point(1085, yPos - 2));
        _exportPdfButton.Click += ExportToPdf_Click;
        headerPanel.Controls.Add(_exportPdfButton);
        
        _printButton = CreateStyledButton("🖨️ طباعة", ColorScheme.Warning, new Point(1265, yPos - 2));
        _printButton.Click += PrintReport_Click;
        headerPanel.Controls.Add(_printButton);
        
        this.Controls.Add(headerPanel);
        
        // ═══════════════════════════════════════════
        // Filter Panel - شريط الفلاتر
        // ═══════════════════════════════════════════
        Panel filterPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = Color.FromArgb(248, 249, 250),
            Padding = new Padding(25, 12, 25, 12)
        };
        
        Label searchLabel = new Label
        {
            Text = "🔍",
            Font = new Font("Segoe UI Emoji", 12F),
            AutoSize = true,
            Location = new Point(25, 23)
        };
        filterPanel.Controls.Add(searchLabel);
        
        _searchBox = new TextBox
        {
            Font = new Font("Cairo", 9.5F),
            Size = new Size(240, 32),
            Location = new Point(55, 20),
            PlaceholderText = "بحث برقم الفاتورة أو العميل..."
        };
        _searchBox.TextChanged += (s, e) => ApplyFilters();
        filterPanel.Controls.Add(_searchBox);
        
        Label statusLabel = new Label
        {
            Text = "الحالة:",
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(310, 25)
        };
        filterPanel.Controls.Add(statusLabel);
        
        _statusFilterCombo = new ComboBox
        {
            Font = new Font("Cairo", 9F),
            Size = new Size(140, 28),
            Location = new Point(365, 22),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _statusFilterCombo.Items.AddRange(new[] { "الكل", "مدفوعة", "مدفوعة جزئياً", "غير مدفوعة" });
        _statusFilterCombo.SelectedIndex = 0;
        _statusFilterCombo.SelectedIndexChanged += (s, e) => ApplyFilters();
        filterPanel.Controls.Add(_statusFilterCombo);
        
        Label customerLabel = new Label
        {
            Text = "العميل:",
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(520, 25)
        };
        filterPanel.Controls.Add(customerLabel);
        
        _customerFilterCombo = new ComboBox
        {
            Font = new Font("Cairo", 9F),
            Size = new Size(180, 28),
            Location = new Point(575, 22),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _customerFilterCombo.Items.Add("الكل");
        _customerFilterCombo.SelectedIndex = 0;
        _customerFilterCombo.SelectedIndexChanged += (s, e) => ApplyFilters();
        filterPanel.Controls.Add(_customerFilterCombo);
        
        Label supplierLabel = new Label
        {
            Text = "المورد:",
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(770, 25)
        };
        filterPanel.Controls.Add(supplierLabel);
        
        _supplierFilterCombo = new ComboBox
        {
            Font = new Font("Cairo", 9F),
            Size = new Size(180, 28),
            Location = new Point(825, 22),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _supplierFilterCombo.Items.Add("الكل");
        _supplierFilterCombo.SelectedIndex = 0;
        _supplierFilterCombo.SelectedIndexChanged += (s, e) => ApplyFilters();
        filterPanel.Controls.Add(_supplierFilterCombo);
        
        _clearFiltersButton = CreateSmallButton("✖ مسح", Color.FromArgb(158, 158, 158), new Point(1020, 20));
        _clearFiltersButton.Click += (s, e) => ClearFilters();
        filterPanel.Controls.Add(_clearFiltersButton);
        
        this.Controls.Add(filterPanel);
        
        // ═══════════════════════════════════════════
        // Main Tab Control
        // ═══════════════════════════════════════════
        _tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Cairo", 10.5F, FontStyle.Bold),
            Padding = new Point(15, 6)
        };
        
        // Tab 1: Sales Report
        TabPage salesTab = new TabPage("📈 تقرير المبيعات");
        CreateSalesTab(salesTab);
        _tabControl.TabPages.Add(salesTab);
        
        // Tab 2: Purchase Report
        TabPage purchaseTab = new TabPage("📉 تقرير المشتريات");
        CreatePurchaseTab(purchaseTab);
        _tabControl.TabPages.Add(purchaseTab);
        
        // Tab 3: Analytics
        TabPage analyticsTab = new TabPage("📊 التحليلات والإحصائيات");
        CreateAnalyticsTab(analyticsTab);
        _tabControl.TabPages.Add(analyticsTab);
        
        _tabControl.SelectedIndexChanged += (s, e) => OnTabChanged();
        
        this.Controls.Add(_tabControl);
    }    
    // ═══════════════════════════════════════════
    // Tab Creation Methods
    // ═══════════════════════════════════════════
    
    private void CreateSalesTab(TabPage tab)
    {
        Panel container = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(245, 247, 250),
            Padding = new Padding(15)
        };
        
        // Data Grid
        _salesReportGrid = CreateProfessionalGrid();
        _salesReportGrid.Dock = DockStyle.Fill;
        _salesReportGrid.CellDoubleClick += SalesGrid_CellDoubleClick;
        container.Controls.Add(_salesReportGrid);
        
        // Summary Cards Panel
        _salesSummaryPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 130,
            BackColor = Color.FromArgb(245, 247, 250)
        };
        container.Controls.Add(_salesSummaryPanel);
        
        tab.Controls.Add(container);
    }
    
    private void CreatePurchaseTab(TabPage tab)
    {
        Panel container = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(245, 247, 250),
            Padding = new Padding(15)
        };
        
        // Data Grid
        _purchaseReportGrid = CreateProfessionalGrid();
        _purchaseReportGrid.Dock = DockStyle.Fill;
        _purchaseReportGrid.CellDoubleClick += PurchaseGrid_CellDoubleClick;
        container.Controls.Add(_purchaseReportGrid);
        
        // Summary Cards Panel
        _purchaseSummaryPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 130,
            BackColor = Color.FromArgb(245, 247, 250)
        };
        container.Controls.Add(_purchaseSummaryPanel);
        
        tab.Controls.Add(container);
    }
    
    private void CreateAnalyticsTab(TabPage tab)
    {
        _analyticsSummaryPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(30),
            AutoScroll = true
        };
        
        tab.Controls.Add(_analyticsSummaryPanel);
    }
    
    // ═══════════════════════════════════════════
    // Grid Creation
    // ═══════════════════════════════════════════
    
    private DataGridView CreateProfessionalGrid()
    {
        var grid = new DataGridView
        {
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            RowHeadersVisible = false,
            Font = new Font("Cairo", 10F),
            EnableHeadersVisualStyles = false,
            MultiSelect = false,
            AllowUserToResizeColumns = true,
            GridColor = Color.FromArgb(230, 230, 230)
        };
        
        // Header style
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            Font = new Font("Cairo", 10.5F, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleCenter,
            Padding = new Padding(10),
            WrapMode = DataGridViewTriState.True
        };
        
        // Cell style
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            Font = new Font("Cairo", 10F),
            Padding = new Padding(8, 6, 8, 6),
            SelectionBackColor = Color.FromArgb(230, 240, 255),
            SelectionForeColor = ColorScheme.Primary,
            ForeColor = Color.FromArgb(33, 33, 33),
            WrapMode = DataGridViewTriState.False,
            BackColor = Color.White
        };
        
        grid.ColumnHeadersHeight = 48;
        grid.RowTemplate.Height = 44;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
        
        // Hover effect
        grid.CellMouseEnter += (s, e) =>
        {
            if (e.RowIndex >= 0)
                grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(240, 245, 255);
        };
        
        grid.CellMouseLeave += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.RowIndex < grid.Rows.Count)
            {
                if (e.RowIndex % 2 == 0)
                    grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                else
                    grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
            }
        };
        
        return grid;
    }
    
    // ═══════════════════════════════════════════
    // Button Creation Helpers
    // ═══════════════════════════════════════════
    
    private Button CreateStyledButton(string text, Color bgColor, Point location)
    {
        Button btn = new Button
        {
            Text = text,
            Font = new Font("Cairo", 9.5F, FontStyle.Bold),
            Size = new Size(160, 36),
            Location = location,
            BackColor = bgColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(bgColor, 0.1f);
        btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(bgColor, 0.2f);
        return btn;
    }
    
    private Button CreateSmallButton(string text, Color bgColor, Point location)
    {
        Button btn = new Button
        {
            Text = text,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            Size = new Size(110, 28),
            Location = location,
            BackColor = bgColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }
    
    // ═══════════════════════════════════════════
    // Data Loading
    // ═══════════════════════════════════════════
    
    private async Task LoadInitialDataAsync()
    {
        try
        {
            this.Cursor = Cursors.WaitCursor;
            
            // Load all invoices
            _allSalesInvoices = (await _invoiceService.GetAllSalesInvoicesAsync()).ToList();
            _allPurchaseInvoices = (await _invoiceService.GetAllPurchaseInvoicesAsync()).ToList();
            
            // Populate filter combos
            PopulateFilterCombos();
            
            // Generate initial report
            await GenerateCurrentTabReportAsync();
            
            this.Cursor = Cursors.Default;
        }
        catch (Exception ex)
        {
            this.Cursor = Cursors.Default;
            MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    // ═══════════════════════════════════════════
    // تحديث تلقائي للبيانات
    // ═══════════════════════════════════════════
    
    private async Task RefreshDataIfNeededAsync()
    {
        try
        {
            // تحديث البيانات إذا مر أكثر من 3 ثواني من آخر تحديث
            if ((DateTime.Now - _lastRefreshTime).TotalSeconds > 3)
            {
                _lastRefreshTime = DateTime.Now;
                
                // تحديث البيانات بصمت
                _allSalesInvoices = (await _invoiceService.GetAllSalesInvoicesAsync()).ToList();
                _allPurchaseInvoices = (await _invoiceService.GetAllPurchaseInvoicesAsync()).ToList();
                
                // تحديث الفلاتر والتقرير
                PopulateFilterCombos();
                await GenerateCurrentTabReportAsync();
            }
        }
        catch
        {
            // تجاهل الأخطاء في التحديث التلقائي
        }
    }
    
    private void PopulateFilterCombos()
    {
        // Customers
        _customerFilterCombo.Items.Clear();
        _customerFilterCombo.Items.Add("الكل");
        var customers = _allSalesInvoices
            .Where(i => i.Customer != null)
            .Select(i => i.Customer!.CustomerName)
            .Distinct()
            .OrderBy(n => n);
        foreach (var customer in customers)
            _customerFilterCombo.Items.Add(customer);
        _customerFilterCombo.SelectedIndex = 0;
        
        // Suppliers
        _supplierFilterCombo.Items.Clear();
        _supplierFilterCombo.Items.Add("الكل");
        var suppliers = _allPurchaseInvoices
            .Where(i => i.Supplier != null)
            .Select(i => i.Supplier!.SupplierName)
            .Distinct()
            .OrderBy(n => n);
        foreach (var supplier in suppliers)
            _supplierFilterCombo.Items.Add(supplier);
        _supplierFilterCombo.SelectedIndex = 0;
    }
    
    // ═══════════════════════════════════════════
    // Report Generation
    // ═══════════════════════════════════════════
    
    private async void GenerateReport_Click(object? sender, EventArgs e)
    {
        DateTime startDate = _startDatePicker.Value.Date;
        DateTime endDate = _endDatePicker.Value.Date;
        
        if (startDate > endDate)
        {
            MessageBox.Show("تاريخ البداية يجب أن يكون قبل تاريخ النهاية", "تنبيه",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        await GenerateCurrentTabReportAsync();
    }
    
    private async Task GenerateCurrentTabReportAsync()
    {
        try
        {
            this.Cursor = Cursors.WaitCursor;
            
            if (_tabControl.SelectedIndex == 0)
                await GenerateSalesReportAsync();
            else if (_tabControl.SelectedIndex == 1)
                await GeneratePurchaseReportAsync();
            else
                await GenerateAnalyticsReportAsync();
            
            this.Cursor = Cursors.Default;
        }
        catch (Exception ex)
        {
            this.Cursor = Cursors.Default;
            MessageBox.Show($"خطأ في إنشاء التقرير: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void OnTabChanged()
    {
        _ = GenerateCurrentTabReportAsync();
    }    
    private async Task GenerateSalesReportAsync()
    {
        DateTime startDate = _startDatePicker.Value.Date;
        DateTime endDate = _endDatePicker.Value.Date.AddDays(1).AddSeconds(-1);
        
        var filteredInvoices = GetFilteredSalesInvoices(startDate, endDate);
        
        // Update Summary Cards
        UpdateSalesSummaryCards(filteredInvoices);
        
        // Setup Grid Columns
        SetupSalesGridColumns();
        
        // Populate Grid
        _salesReportGrid.Rows.Clear();
        
        foreach (var invoice in filteredInvoices.OrderByDescending(i => i.InvoiceDate))
        {
            int rowIndex = _salesReportGrid.Rows.Add(
                invoice.SalesInvoiceId, // Hidden ID
                invoice.InvoiceNumber,
                invoice.InvoiceDate.ToString("yyyy-MM-dd"),
                invoice.Customer?.CustomerName ?? "غير محدد",
                invoice.SubTotal.ToString("N2"),
                invoice.TaxAmount.ToString("N2"),
                invoice.TotalAmount.ToString("N2"),
                invoice.PaidAmount.ToString("N2"),
                invoice.RemainingAmount.ToString("N2"),
                GetStatusIcon(invoice.Status) + " " + GetStatusText(invoice.Status),
                "نقدي" // PaymentMethod not available in entity
            );
            
            // Color code by status
            ColorCodeRowByStatus(_salesReportGrid.Rows[rowIndex], invoice.Status);
        }
        
        // Add totals row
        if (filteredInvoices.Any())
        {
            int totalRow = _salesReportGrid.Rows.Add(
                0,
                "",
                "الإجمالي",
                $"{filteredInvoices.Count} فاتورة",
                filteredInvoices.Sum(i => i.SubTotal).ToString("N2"),
                filteredInvoices.Sum(i => i.TaxAmount).ToString("N2"),
                filteredInvoices.Sum(i => i.TotalAmount).ToString("N2"),
                filteredInvoices.Sum(i => i.PaidAmount).ToString("N2"),
                filteredInvoices.Sum(i => i.RemainingAmount).ToString("N2"),
                "",
                ""
            );
            
            _salesReportGrid.Rows[totalRow].DefaultCellStyle.BackColor = ColorScheme.Primary;
            _salesReportGrid.Rows[totalRow].DefaultCellStyle.ForeColor = Color.White;
            _salesReportGrid.Rows[totalRow].DefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        }
        
        await Task.CompletedTask;
    }
    
    private void SetupSalesGridColumns()
    {
        _salesReportGrid.Columns.Clear();
        
        // Hidden ID column
        _salesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "InvoiceId", 
            Visible = false 
        });
        
        _salesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "InvoiceNumber", 
            HeaderText = "رقم الفاتورة", 
            Width = 140,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                ForeColor = ColorScheme.Primary
            }
        });
        
        _salesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "Date", 
            HeaderText = "التاريخ", 
            Width = 120,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        _salesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "Customer", 
            HeaderText = "العميل", 
            Width = 200,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Alignment = DataGridViewContentAlignment.MiddleRight,
                Font = new Font("Cairo", 11F, FontStyle.Bold)
            }
        });
        
        _salesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "SubTotal", 
            HeaderText = "المبلغ قبل الضريبة", 
            Width = 150,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Format = "N2",
                Font = new Font("Cairo", 11F)
            }
        });
        
        _salesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "Tax", 
            HeaderText = "الضريبة", 
            Width = 110,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(255, 152, 0),
                Font = new Font("Cairo", 11F, FontStyle.Bold)
            }
        });
        
        _salesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "Total", 
            HeaderText = "الإجمالي", 
            Width = 140,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Cairo", 12F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary
            }
        });
        
        _salesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "Paid", 
            HeaderText = "المدفوع", 
            Width = 130,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                ForeColor = ColorScheme.Success,
                Font = new Font("Cairo", 11F, FontStyle.Bold)
            }
        });
        
        _salesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "Remaining", 
            HeaderText = "المتبقي", 
            Width = 130,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                ForeColor = ColorScheme.Error,
                Font = new Font("Cairo", 11F, FontStyle.Bold)
            }
        });
        
        _salesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "Status", 
            HeaderText = "الحالة", 
            Width = 130,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Cairo", 9F, FontStyle.Bold)
            }
        });
        
        _salesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "PaymentMethod", 
            HeaderText = "طريقة الدفع", 
            Width = 110,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
    }
    
    private List<SalesInvoice> GetFilteredSalesInvoices(DateTime startDate, DateTime endDate)
    {
        var invoices = _allSalesInvoices.Where(i => 
            i.InvoiceDate >= startDate && 
            i.InvoiceDate <= endDate).ToList();
        
        // Apply search filter
        if (!string.IsNullOrWhiteSpace(_searchBox.Text))
        {
            string search = _searchBox.Text.ToLower();
            invoices = invoices.Where(i =>
                i.InvoiceNumber.ToLower().Contains(search) ||
                (i.Customer?.CustomerName?.ToLower().Contains(search) ?? false)).ToList();
        }
        
        // Apply status filter
        if (_statusFilterCombo.SelectedIndex > 0)
        {
            string statusText = _statusFilterCombo.SelectedItem?.ToString() ?? "";
            string statusEn = statusText switch
            {
                "مدفوعة" => "Paid",
                "مدفوعة جزئياً" => "Partial",
                "غير مدفوعة" => "Unpaid",
                _ => ""
            };
            if (!string.IsNullOrEmpty(statusEn))
                invoices = invoices.Where(i => i.Status == statusEn).ToList();
        }
        
        // Apply customer filter
        if (_customerFilterCombo.SelectedIndex > 0)
        {
            string customer = _customerFilterCombo.SelectedItem?.ToString() ?? "";
            invoices = invoices.Where(i => i.Customer?.CustomerName == customer).ToList();
        }
        
        return invoices;
    }
    
    private void UpdateSalesSummaryCards(List<SalesInvoice> invoices)
    {
        _salesSummaryPanel.Controls.Clear();
        
        int cardWidth = 260;
        int cardSpacing = 15;
        int x = 0;
        
        // Card 1: Total Invoices
        CreateSummaryCard(_salesSummaryPanel, "إجمالي الفواتير", invoices.Count.ToString(), 
            "🧾", ColorScheme.Primary, x, 0, cardWidth);
        x += cardWidth + cardSpacing;
        
        // Card 2: Total Amount
        CreateSummaryCard(_salesSummaryPanel, "إجمالي المبيعات", 
            invoices.Sum(i => i.TotalAmount).ToString("N2") + " ج.م",
            "💵", Color.FromArgb(33, 150, 83), x, 0, cardWidth);
        x += cardWidth + cardSpacing;
        
        // Card 3: Paid Amount
        CreateSummaryCard(_salesSummaryPanel, "المحصل", 
            invoices.Sum(i => i.PaidAmount).ToString("N2") + " ج.م",
            "✅", ColorScheme.Success, x, 0, cardWidth);
        x += cardWidth + cardSpacing;
        
        // Card 4: Remaining Amount
        CreateSummaryCard(_salesSummaryPanel, "المتبقي", 
            invoices.Sum(i => i.RemainingAmount).ToString("N2") + " ج.م",
            "⏳", ColorScheme.Error, x, 0, cardWidth);
        x += cardWidth + cardSpacing;
        
        // Card 5: Average Invoice
        decimal avgInvoice = invoices.Any() ? invoices.Average(i => i.TotalAmount) : 0;
        CreateSummaryCard(_salesSummaryPanel, "متوسط الفاتورة", 
            avgInvoice.ToString("N2") + " ج.م",
            "📊", Color.FromArgb(156, 39, 176), x, 0, cardWidth);
    }
    
    private void CreateSummaryCard(Panel parent, string title, string value, 
        string icon, Color color, int x, int y, int width)
    {
        Panel card = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(width, 110),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        
        // Colored top bar
        Panel topBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 4,
            BackColor = color
        };
        card.Controls.Add(topBar);
        
        // Icon
        Label iconLabel = new Label
        {
            Text = icon,
            Font = new Font("Segoe UI Emoji", 28F),
            ForeColor = color,
            AutoSize = false,
            Size = new Size(60, 60),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(8, 20)
        };
        card.Controls.Add(iconLabel);
        
        // Title
        Label titleLabel = new Label
        {
            Text = title,
            Font = new Font("Cairo", 9F),
            ForeColor = Color.Gray,
            AutoSize = false,
            Size = new Size(width - 75, 22),
            TextAlign = ContentAlignment.MiddleRight,
            Location = new Point(73, 25)
        };
        card.Controls.Add(titleLabel);
        
        // Value
        Label valueLabel = new Label
        {
            Text = value,
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = color,
            AutoSize = false,
            Size = new Size(width - 75, 35),
            TextAlign = ContentAlignment.MiddleRight,
            Location = new Point(73, 52)
        };
        card.Controls.Add(valueLabel);
        
        parent.Controls.Add(card);
    }    
    // ═══════════════════════════════════════════
    // Purchase Report Generation
    // ═══════════════════════════════════════════
    
    private async Task GeneratePurchaseReportAsync()
    {
        DateTime startDate = _startDatePicker.Value.Date;
        DateTime endDate = _endDatePicker.Value.Date.AddDays(1).AddSeconds(-1);
        
        var filteredInvoices = GetFilteredPurchaseInvoices(startDate, endDate);
        
        // Update Summary Cards
        UpdatePurchaseSummaryCards(filteredInvoices);
        
        // Setup Grid Columns (similar to sales)
        SetupPurchaseGridColumns();
        
        // Populate Grid
        _purchaseReportGrid.Rows.Clear();
        
        foreach (var invoice in filteredInvoices.OrderByDescending(i => i.InvoiceDate))
        {
            decimal remaining = invoice.TotalAmount - invoice.PaidAmount;
            
            int rowIndex = _purchaseReportGrid.Rows.Add(
                invoice.PurchaseInvoiceId,
                invoice.InvoiceNumber,
                invoice.InvoiceDate.ToString("yyyy-MM-dd"),
                invoice.Supplier?.SupplierName ?? "غير محدد",
                invoice.SubTotal.ToString("N2"),
                invoice.TaxAmount.ToString("N2"),
                invoice.TotalAmount.ToString("N2"),
                invoice.PaidAmount.ToString("N2"),
                remaining.ToString("N2"),
                GetStatusIcon(invoice.Status) + " " + GetStatusText(invoice.Status),
                "نقدي" // PaymentMethod not available in entity
            );
            
            ColorCodeRowByStatus(_purchaseReportGrid.Rows[rowIndex], invoice.Status);
        }
        
        // Add totals row
        if (filteredInvoices.Any())
        {
            int totalRow = _purchaseReportGrid.Rows.Add(
                0,
                "",
                "الإجمالي",
                $"{filteredInvoices.Count} فاتورة",
                filteredInvoices.Sum(i => i.SubTotal).ToString("N2"),
                filteredInvoices.Sum(i => i.TaxAmount).ToString("N2"),
                filteredInvoices.Sum(i => i.TotalAmount).ToString("N2"),
                filteredInvoices.Sum(i => i.PaidAmount).ToString("N2"),
                filteredInvoices.Sum(i => i.TotalAmount - i.PaidAmount).ToString("N2"),
                "",
                ""
            );
            
            _purchaseReportGrid.Rows[totalRow].DefaultCellStyle.BackColor = ColorScheme.Error;
            _purchaseReportGrid.Rows[totalRow].DefaultCellStyle.ForeColor = Color.White;
            _purchaseReportGrid.Rows[totalRow].DefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        }
        
        await Task.CompletedTask;
    }
    
    private void SetupPurchaseGridColumns()
    {
        _purchaseReportGrid.Columns.Clear();
        
        _purchaseReportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "InvoiceId", Visible = false });
        _purchaseReportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "InvoiceNumber", HeaderText = "رقم الفاتورة", Width = 120 });
        _purchaseReportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "التاريخ", Width = 110 });
        _purchaseReportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Supplier", HeaderText = "المورد", Width = 180 });
        _purchaseReportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "SubTotal", HeaderText = "قبل الضريبة", Width = 130 });
        _purchaseReportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tax", HeaderText = "الضريبة", Width = 100 });
        _purchaseReportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "الإجمالي", Width = 120 });
        _purchaseReportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Paid", HeaderText = "المدفوع", Width = 110 });
        _purchaseReportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Remaining", HeaderText = "المتبقي", Width = 110 });
        _purchaseReportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "الحالة", Width = 130 });
        _purchaseReportGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "PaymentMethod", HeaderText = "طريقة الدفع", Width = 110 });
    }
    
    private List<PurchaseInvoice> GetFilteredPurchaseInvoices(DateTime startDate, DateTime endDate)
    {
        var invoices = _allPurchaseInvoices.Where(i => 
            i.InvoiceDate >= startDate && 
            i.InvoiceDate <= endDate).ToList();
        
        if (!string.IsNullOrWhiteSpace(_searchBox.Text))
        {
            string search = _searchBox.Text.ToLower();
            invoices = invoices.Where(i =>
                i.InvoiceNumber.ToLower().Contains(search) ||
                (i.Supplier?.SupplierName?.ToLower().Contains(search) ?? false)).ToList();
        }
        
        if (_statusFilterCombo.SelectedIndex > 0)
        {
            string statusText = _statusFilterCombo.SelectedItem?.ToString() ?? "";
            string statusEn = statusText switch
            {
                "مدفوعة" => "Paid",
                "مدفوعة جزئياً" => "Partial",
                "غير مدفوعة" => "Unpaid",
                _ => ""
            };
            if (!string.IsNullOrEmpty(statusEn))
                invoices = invoices.Where(i => i.Status == statusEn).ToList();
        }
        
        if (_supplierFilterCombo.SelectedIndex > 0)
        {
            string supplier = _supplierFilterCombo.SelectedItem?.ToString() ?? "";
            invoices = invoices.Where(i => i.Supplier?.SupplierName == supplier).ToList();
        }
        
        return invoices;
    }
    
    private void UpdatePurchaseSummaryCards(List<PurchaseInvoice> invoices)
    {
        _purchaseSummaryPanel.Controls.Clear();
        
        int cardWidth = 280;
        int cardSpacing = 20;
        int x = 0;
        
        CreateSummaryCard(_purchaseSummaryPanel, "إجمالي الفواتير", invoices.Count.ToString(), 
            "🧾", ColorScheme.Primary, x, 0, cardWidth);
        x += cardWidth + cardSpacing;
        
        CreateSummaryCard(_purchaseSummaryPanel, "إجمالي المشتريات", 
            invoices.Sum(i => i.TotalAmount).ToString("N2") + " جنيه",
            "💸", ColorScheme.Error, x, 0, cardWidth);
        x += cardWidth + cardSpacing;
        
        CreateSummaryCard(_purchaseSummaryPanel, "المدفوع", 
            invoices.Sum(i => i.PaidAmount).ToString("N2") + " جنيه",
            "✅", Color.FromArgb(255, 87, 34), x, 0, cardWidth);
        x += cardWidth + cardSpacing;
        
        CreateSummaryCard(_purchaseSummaryPanel, "المتبقي", 
            invoices.Sum(i => i.TotalAmount - i.PaidAmount).ToString("N2") + " جنيه",
            "⏳", ColorScheme.Warning, x, 0, cardWidth);
        x += cardWidth + cardSpacing;
        
        decimal avgInvoice = invoices.Any() ? invoices.Average(i => i.TotalAmount) : 0;
        CreateSummaryCard(_purchaseSummaryPanel, "متوسط الفاتورة", 
            avgInvoice.ToString("N2") + " جنيه",
            "📊", Color.FromArgb(103, 58, 183), x, 0, cardWidth);
    }
    
    // ═══════════════════════════════════════════
    // Analytics Report
    // ═══════════════════════════════════════════
    
    private async Task GenerateAnalyticsReportAsync()
    {
        _analyticsSummaryPanel.Controls.Clear();
        
        DateTime startDate = _startDatePicker.Value.Date;
        DateTime endDate = _endDatePicker.Value.Date.AddDays(1).AddSeconds(-1);
        
        var salesInvoices = _allSalesInvoices.Where(i => 
            i.InvoiceDate >= startDate && i.InvoiceDate <= endDate).ToList();
        var purchaseInvoices = _allPurchaseInvoices.Where(i => 
            i.InvoiceDate >= startDate && i.InvoiceDate <= endDate).ToList();
        
        int yPos = 20;
        
        // ─────────────────────────────────────────
        // Period Header
        // ─────────────────────────────────────────
        Label periodLabel = new Label
        {
            Text = $"📅 الفترة: من {startDate:yyyy/MM/dd} إلى {endDate:yyyy/MM/dd}",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(20, yPos)
        };
        _analyticsSummaryPanel.Controls.Add(periodLabel);
        yPos += 60;
        
        // ─────────────────────────────────────────
        // Financial Overview
        // ─────────────────────────────────────────
        Panel overviewPanel = CreateAnalyticsSection("💰 نظرة عامة مالية", 
            ColorScheme.Primary, new Point(20, yPos), new Size(1500, 200));
        
        decimal totalSales = salesInvoices.Sum(i => i.TotalAmount);
        decimal totalPurchases = purchaseInvoices.Sum(i => i.TotalAmount);
        decimal netProfit = totalSales - totalPurchases;
        decimal profitMargin = totalSales > 0 ? (netProfit / totalSales * 100) : 0;
        
        AddAnalyticsItem(overviewPanel, "إجمالي المبيعات:", totalSales.ToString("N2") + " جنيه", 
            ColorScheme.Success, 20, 60);
        AddAnalyticsItem(overviewPanel, "إجمالي المشتريات:", totalPurchases.ToString("N2") + " جنيه", 
            ColorScheme.Error, 400, 60);
        AddAnalyticsItem(overviewPanel, "صافي الربح:", netProfit.ToString("N2") + " جنيه", 
            netProfit >= 0 ? ColorScheme.Success : ColorScheme.Error, 780, 60);
        AddAnalyticsItem(overviewPanel, "هامش الربح:", profitMargin.ToString("N1") + "%", 
            ColorScheme.Primary, 1160, 60);
        
        AddAnalyticsItem(overviewPanel, "المحصل من المبيعات:", 
            salesInvoices.Sum(i => i.PaidAmount).ToString("N2") + " جنيه", 
            ColorScheme.Success, 20, 120);
        AddAnalyticsItem(overviewPanel, "المدفوع للموردين:", 
            purchaseInvoices.Sum(i => i.PaidAmount).ToString("N2") + " جنيه", 
            ColorScheme.Error, 400, 120);
        AddAnalyticsItem(overviewPanel, "المستحقات (للعملاء):", 
            salesInvoices.Sum(i => i.RemainingAmount).ToString("N2") + " جنيه", 
            ColorScheme.Warning, 780, 120);
        AddAnalyticsItem(overviewPanel, "المستحقات (للموردين):", 
            purchaseInvoices.Sum(i => i.TotalAmount - i.PaidAmount).ToString("N2") + " جنيه", 
            Color.FromArgb(255, 152, 0), 1160, 120);
        
        _analyticsSummaryPanel.Controls.Add(overviewPanel);
        yPos += 220;
        
        // ─────────────────────────────────────────
        // Top Customers
        // ─────────────────────────────────────────
        var topCustomers = salesInvoices
            .Where(i => i.Customer != null)
            .GroupBy(i => i.Customer!.CustomerName)
            .Select(g => new 
            { 
                Name = g.Key, 
                Total = g.Sum(i => i.TotalAmount),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Total)
            .Take(5)
            .ToList();
        
        if (topCustomers.Any())
        {
            Panel topCustomersPanel = CreateAnalyticsSection("🏆 أفضل 5 عملاء", 
                ColorScheme.Success, new Point(20, yPos), new Size(730, 300));
            
            int customerY = 60;
            int rank = 1;
            foreach (var customer in topCustomers)
            {
                AddRankingItem(topCustomersPanel, rank, customer.Name, 
                    customer.Total.ToString("N2") + " جنيه", 
                    $"{customer.Count} فاتورة", customerY);
                customerY += 45;
                rank++;
            }
            
            _analyticsSummaryPanel.Controls.Add(topCustomersPanel);
        }
        
        // ─────────────────────────────────────────
        // Top Suppliers
        // ─────────────────────────────────────────
        var topSuppliers = purchaseInvoices
            .Where(i => i.Supplier != null)
            .GroupBy(i => i.Supplier!.SupplierName)
            .Select(g => new 
            { 
                Name = g.Key, 
                Total = g.Sum(i => i.TotalAmount),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Total)
            .Take(5)
            .ToList();
        
        if (topSuppliers.Any())
        {
            Panel topSuppliersPanel = CreateAnalyticsSection("🏆 أكبر 5 موردين", 
                ColorScheme.Error, new Point(770, yPos), new Size(730, 300));
            
            int supplierY = 60;
            int rank = 1;
            foreach (var supplier in topSuppliers)
            {
                AddRankingItem(topSuppliersPanel, rank, supplier.Name, 
                    supplier.Total.ToString("N2") + " جنيه", 
                    $"{supplier.Count} فاتورة", supplierY);
                supplierY += 45;
                rank++;
            }
            
            _analyticsSummaryPanel.Controls.Add(topSuppliersPanel);
        }
        
        yPos += 320;
        
        // ─────────────────────────────────────────
        // Payment Status Breakdown
        // ─────────────────────────────────────────
        Panel statusPanel = CreateAnalyticsSection("📊 توزيع الفواتير حسب الحالة", 
            ColorScheme.Primary, new Point(20, yPos), new Size(1500, 180));
        
        var salesByStatus = salesInvoices.GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key, Count = g.Count(), Total = g.Sum(i => i.TotalAmount) })
            .ToList();
        
        int statusX = 50;
        AddStatusBreakdown(statusPanel, "✅ مبيعات مدفوعة", 
            salesByStatus.FirstOrDefault(s => s.Status == "Paid")?.Count ?? 0,
            salesByStatus.FirstOrDefault(s => s.Status == "Paid")?.Total ?? 0,
            ColorScheme.Success, statusX, 60);
        statusX += 380;
        
        AddStatusBreakdown(statusPanel, "⏳ مبيعات جزئية", 
            salesByStatus.FirstOrDefault(s => s.Status == "Partial")?.Count ?? 0,
            salesByStatus.FirstOrDefault(s => s.Status == "Partial")?.Total ?? 0,
            ColorScheme.Warning, statusX, 60);
        statusX += 380;
        
        AddStatusBreakdown(statusPanel, "❌ مبيعات غير مدفوعة", 
            salesByStatus.FirstOrDefault(s => s.Status == "Unpaid")?.Count ?? 0,
            salesByStatus.FirstOrDefault(s => s.Status == "Unpaid")?.Total ?? 0,
            ColorScheme.Error, statusX, 60);
        
        _analyticsSummaryPanel.Controls.Add(statusPanel);
        
        await Task.CompletedTask;
    }    
    // ═══════════════════════════════════════════
    // Analytics Helpers
    // ═══════════════════════════════════════════
    
    private Panel CreateAnalyticsSection(string title, Color color, Point location, Size size)
    {
        Panel panel = new Panel
        {
            Location = location,
            Size = size,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        
        Panel topBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 6,
            BackColor = color
        };
        panel.Controls.Add(topBar);
        
        Label titleLabel = new Label
        {
            Text = title,
            Font = new Font("Cairo", 13F, FontStyle.Bold),
            ForeColor = color,
            AutoSize = true,
            Location = new Point(20, 18)
        };
        panel.Controls.Add(titleLabel);
        
        return panel;
    }
    
    private void AddAnalyticsItem(Panel panel, string label, string value, Color color, int x, int y)
    {
        Label lblLabel = new Label
        {
            Text = label,
            Font = new Font("Cairo", 10F),
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(x, y)
        };
        panel.Controls.Add(lblLabel);
        
        Label lblValue = new Label
        {
            Text = value,
            Font = new Font("Cairo", 13F, FontStyle.Bold),
            ForeColor = color,
            AutoSize = true,
            Location = new Point(x, y + 25)
        };
        panel.Controls.Add(lblValue);
    }
    
    private void AddRankingItem(Panel panel, int rank, string name, string amount, string count, int y)
    {
        // Rank badge
        Label rankLabel = new Label
        {
            Text = rank.ToString(),
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = rank == 1 ? Color.FromArgb(255, 193, 7) : 
                       rank == 2 ? Color.FromArgb(158, 158, 158) :
                       rank == 3 ? Color.FromArgb(205, 127, 50) :
                       ColorScheme.Primary,
            Size = new Size(35, 35),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(20, y)
        };
        panel.Controls.Add(rankLabel);
        
        // Name
        Label nameLabel = new Label
        {
            Text = name,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = Color.Black,
            AutoSize = false,
            Size = new Size(300, 25),
            Location = new Point(70, y + 2)
        };
        panel.Controls.Add(nameLabel);
        
        // Amount
        Label amountLabel = new Label
        {
            Text = amount,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = ColorScheme.Success,
            AutoSize = false,
            Size = new Size(180, 20),
            TextAlign = ContentAlignment.MiddleRight,
            Location = new Point(380, y + 5)
        };
        panel.Controls.Add(amountLabel);
        
        // Count
        Label countLabel = new Label
        {
            Text = count,
            Font = new Font("Cairo", 9F),
            ForeColor = Color.Gray,
            AutoSize = false,
            Size = new Size(100, 20),
            TextAlign = ContentAlignment.MiddleRight,
            Location = new Point(570, y + 8)
        };
        panel.Controls.Add(countLabel);
    }
    
    private void AddStatusBreakdown(Panel panel, string status, int count, decimal total, 
        Color color, int x, int y)
    {
        Label statusLabel = new Label
        {
            Text = status,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = color,
            AutoSize = true,
            Location = new Point(x, y)
        };
        panel.Controls.Add(statusLabel);
        
        Label countLabel = new Label
        {
            Text = $"{count} فاتورة",
            Font = new Font("Cairo", 10F),
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(x, y + 30)
        };
        panel.Controls.Add(countLabel);
        
        Label totalLabel = new Label
        {
            Text = total.ToString("N2") + " جنيه",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = color,
            AutoSize = true,
            Location = new Point(x, y + 55)
        };
        panel.Controls.Add(totalLabel);
    }
    
    // ═══════════════════════════════════════════
    // Filtering & Helpers
    // ═══════════════════════════════════════════
    
    private void ApplyFilters()
    {
        _ = GenerateCurrentTabReportAsync();
    }
    
    private void ClearFilters()
    {
        _searchBox.Clear();
        _statusFilterCombo.SelectedIndex = 0;
        _customerFilterCombo.SelectedIndex = 0;
        _supplierFilterCombo.SelectedIndex = 0;
        ApplyFilters();
    }
    
    private string GetStatusText(string status)
    {
        return status switch
        {
            "Paid" => "مدفوعة",
            "Partial" => "مدفوعة جزئياً",
            "Unpaid" => "غير مدفوعة",
            _ => status
        };
    }
    
    private string GetStatusIcon(string status)
    {
        return status switch
        {
            "Paid" => "✅",
            "Partial" => "⏳",
            "Unpaid" => "❌",
            _ => "❓"
        };
    }
    
    private void ColorCodeRowByStatus(DataGridViewRow row, string status)
    {
        Color bgColor = status switch
        {
            "Paid" => Color.FromArgb(232, 245, 233),
            "Partial" => Color.FromArgb(255, 249, 196),
            "Unpaid" => Color.FromArgb(255, 235, 238),
            _ => Color.White
        };
        
        // Set alternating row color
        if (row.Index % 2 == 1)
            row.DefaultCellStyle.BackColor = ControlPaint.Light(bgColor, 0.3f);
        else
            row.DefaultCellStyle.BackColor = bgColor;
    }
    
    // ═══════════════════════════════════════════
    // Invoice Details View
    // ═══════════════════════════════════════════
    
    private void SalesGrid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && e.RowIndex < _salesReportGrid.Rows.Count)
        {
            var row = _salesReportGrid.Rows[e.RowIndex];
            if (row.Cells["InvoiceId"].Value != null)
            {
                int invoiceId = Convert.ToInt32(row.Cells["InvoiceId"].Value);
                ShowInvoiceDetails(invoiceId, "Sales");
            }
        }
    }
    
    private void PurchaseGrid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && e.RowIndex < _purchaseReportGrid.Rows.Count)
        {
            var row = _purchaseReportGrid.Rows[e.RowIndex];
            if (row.Cells["InvoiceId"].Value != null)
            {
                int invoiceId = Convert.ToInt32(row.Cells["InvoiceId"].Value);
                ShowInvoiceDetails(invoiceId, "Purchase");
            }
        }
    }
    
    private async void ShowInvoiceDetails(int invoiceId, string type)
    {
        try
        {
            this.Cursor = Cursors.WaitCursor;
            
            if (type == "Sales")
            {
                var invoice = await _invoiceService.GetSalesInvoiceByIdAsync(invoiceId);
                if (invoice != null)
                    ShowSalesInvoiceDetailsForm(invoice);
            }
            else
            {
                var invoice = await _invoiceService.GetPurchaseInvoiceByIdAsync(invoiceId);
                if (invoice != null)
                    ShowPurchaseInvoiceDetailsForm(invoice);
            }
            
            this.Cursor = Cursors.Default;
        }
        catch (Exception ex)
        {
            this.Cursor = Cursors.Default;
            MessageBox.Show($"خطأ في عرض التفاصيل: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void ShowSalesInvoiceDetailsForm(SalesInvoice invoice)
    {
        Form detailsForm = new Form
        {
            Text = "📋 تفاصيل فاتورة مبيعات",
            Size = new Size(900, 700),
            StartPosition = FormStartPosition.CenterParent,
            RightToLeft = RightToLeft.Yes,
            RightToLeftLayout = false,
            FormBorderStyle = FormBorderStyle.Sizable,
            MaximizeBox = true,
            MinimizeBox = false,
            BackColor = Color.FromArgb(245, 247, 250),
            Font = new Font("Cairo", 10F)
        };
        
        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(20)
        };
        
        int yPos = 20;
        
        // Header Card
        Panel headerCard = CreateCard(20, yPos, 820, 180);
        headerCard.BackColor = ColorScheme.Primary;
        
        Label invoiceTitle = new Label
        {
            Text = "📄 فاتورة مبيعات",
            Font = new Font("Cairo", 18F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 15)
        };
        headerCard.Controls.Add(invoiceTitle);
        
        Label invoiceNumber = new Label
        {
            Text = $"رقم الفاتورة: {invoice.InvoiceNumber}",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 55)
        };
        headerCard.Controls.Add(invoiceNumber);
        
        Label invoiceDate = new Label
        {
            Text = $"📅 التاريخ: {invoice.InvoiceDate:yyyy-MM-dd}",
            Font = new Font("Cairo", 12F),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 95)
        };
        headerCard.Controls.Add(invoiceDate);
        
        // Status Badge
        Panel statusBadge = new Panel
        {
            Size = new Size(150, 50),
            Location = new Point(650, 20),
            BackColor = GetStatusColor(invoice.Status)
        };
        Label statusText = new Label
        {
            Text = GetStatusIcon(invoice.Status) + " " + GetStatusText(invoice.Status),
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };
        statusBadge.Controls.Add(statusText);
        headerCard.Controls.Add(statusBadge);
        
        // Customer Info
        Label customerLabel = new Label
        {
            Text = $"👤 العميل: {invoice.Customer?.CustomerName ?? "غير محدد"}",
            Font = new Font("Cairo", 11F),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 135)
        };
        headerCard.Controls.Add(customerLabel);
        
        mainPanel.Controls.Add(headerCard);
        yPos += 200;
        
        // Financial Summary Card
        Panel financialCard = CreateCard(20, yPos, 820, 200);
        
        Label financialTitle = new Label
        {
            Text = "💰 الملخص المالي",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(15, 15)
        };
        financialCard.Controls.Add(financialTitle);
        
        int finYPos = 60;
        AddFinancialRow(financialCard, "المبلغ قبل الضريبة:", invoice.SubTotal.ToString("N2") + " ج.م", finYPos, Color.FromArgb(66, 66, 66));
        finYPos += 35;
        AddFinancialRow(financialCard, "الضريبة:", invoice.TaxAmount.ToString("N2") + " ج.م", finYPos, Color.FromArgb(255, 152, 0));
        finYPos += 35;
        
        // Separator line
        Panel separator = new Panel
        {
            Location = new Point(15, finYPos),
            Size = new Size(790, 2),
            BackColor = Color.FromArgb(200, 200, 200)
        };
        financialCard.Controls.Add(separator);
        finYPos += 10;
        
        AddFinancialRow(financialCard, "الإجمالي:", invoice.TotalAmount.ToString("N2") + " ج.م", finYPos, ColorScheme.Primary, true);
        finYPos += 35;
        AddFinancialRow(financialCard, "المدفوع:", invoice.PaidAmount.ToString("N2") + " ج.م", finYPos, ColorScheme.Success, true);
        finYPos += 35;
        AddFinancialRow(financialCard, "المتبقي:", invoice.RemainingAmount.ToString("N2") + " ج.م", finYPos, ColorScheme.Error, true);
        
        mainPanel.Controls.Add(financialCard);
        yPos += 220;
        
        // Items Card
        if (invoice.Items?.Any() == true)
        {
            Panel itemsCard = CreateCard(20, yPos, 820, 50 + (invoice.Items.Count * 45) + 20);
            
            Label itemsTitle = new Label
            {
                Text = $"📦 الأصناف ({invoice.Items.Count})",
                Font = new Font("Cairo", 14F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                AutoSize = true,
                Location = new Point(15, 15)
            };
            itemsCard.Controls.Add(itemsTitle);
            
            int itemYPos = 60;
            foreach (var item in invoice.Items)
            {
                Panel itemPanel = new Panel
                {
                    Location = new Point(15, itemYPos),
                    Size = new Size(790, 40),
                    BackColor = Color.FromArgb(248, 249, 250)
                };
                
                Label itemDesc = new Label
                {
                    Text = item.Description,
                    Font = new Font("Cairo", 11F, FontStyle.Bold),
                    Location = new Point(10, 10),
                    AutoSize = true
                };
                itemPanel.Controls.Add(itemDesc);
                
                Label itemQty = new Label
                {
                    Text = $"الكمية: {item.Quantity}",
                    Font = new Font("Cairo", 10F),
                    Location = new Point(400, 12),
                    AutoSize = true
                };
                itemPanel.Controls.Add(itemQty);
                
                Label itemPrice = new Label
                {
                    Text = $"{item.TotalPrice:N2} ج.م",
                    Font = new Font("Cairo", 11F, FontStyle.Bold),
                    ForeColor = ColorScheme.Primary,
                    Location = new Point(650, 10),
                    AutoSize = true
                };
                itemPanel.Controls.Add(itemPrice);
                
                itemsCard.Controls.Add(itemPanel);
                itemYPos += 45;
            }
            
            mainPanel.Controls.Add(itemsCard);
            yPos += itemsCard.Height + 20;
        }
        
        // Notes Card
        if (!string.IsNullOrWhiteSpace(invoice.Notes))
        {
            Panel notesCard = CreateCard(20, yPos, 820, 120);
            
            Label notesTitle = new Label
            {
                Text = "📝 ملاحظات",
                Font = new Font("Cairo", 14F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                AutoSize = true,
                Location = new Point(15, 15)
            };
            notesCard.Controls.Add(notesTitle);
            
            Label notesText = new Label
            {
                Text = invoice.Notes,
                Font = new Font("Cairo", 11F),
                Location = new Point(15, 55),
                Size = new Size(790, 50),
                ForeColor = Color.FromArgb(66, 66, 66)
            };
            notesCard.Controls.Add(notesText);
            
            mainPanel.Controls.Add(notesCard);
        }
        
        detailsForm.Controls.Add(mainPanel);
        detailsForm.ShowDialog();
    }
    
    private void ShowPurchaseInvoiceDetailsForm(PurchaseInvoice invoice)
    {
        Form detailsForm = new Form
        {
            Text = "📋 تفاصيل فاتورة مشتريات",
            Size = new Size(900, 700),
            StartPosition = FormStartPosition.CenterParent,
            RightToLeft = RightToLeft.Yes,
            RightToLeftLayout = false,
            FormBorderStyle = FormBorderStyle.Sizable,
            MaximizeBox = true,
            MinimizeBox = false,
            BackColor = Color.FromArgb(245, 247, 250),
            Font = new Font("Cairo", 10F)
        };
        
        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(20)
        };
        
        int yPos = 20;
        
        // Header Card
        Panel headerCard = CreateCard(20, yPos, 820, 180);
        headerCard.BackColor = Color.FromArgb(211, 47, 47); // Red for purchase
        
        Label invoiceTitle = new Label
        {
            Text = "📄 فاتورة مشتريات",
            Font = new Font("Cairo", 18F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 15)
        };
        headerCard.Controls.Add(invoiceTitle);
        
        Label invoiceNumber = new Label
        {
            Text = $"رقم الفاتورة: {invoice.InvoiceNumber}",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 55)
        };
        headerCard.Controls.Add(invoiceNumber);
        
        Label invoiceDate = new Label
        {
            Text = $"📅 التاريخ: {invoice.InvoiceDate:yyyy-MM-dd}",
            Font = new Font("Cairo", 12F),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 95)
        };
        headerCard.Controls.Add(invoiceDate);
        
        // Status Badge
        Panel statusBadge = new Panel
        {
            Size = new Size(150, 50),
            Location = new Point(650, 20),
            BackColor = GetStatusColor(invoice.Status)
        };
        Label statusText = new Label
        {
            Text = GetStatusIcon(invoice.Status) + " " + GetStatusText(invoice.Status),
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };
        statusBadge.Controls.Add(statusText);
        headerCard.Controls.Add(statusBadge);
        
        // Supplier Info
        Label supplierLabel = new Label
        {
            Text = $"🏢 المورد: {invoice.Supplier?.SupplierName ?? "غير محدد"}",
            Font = new Font("Cairo", 11F),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 135)
        };
        headerCard.Controls.Add(supplierLabel);
        
        mainPanel.Controls.Add(headerCard);
        yPos += 200;
        
        // Financial Summary Card
        Panel financialCard = CreateCard(20, yPos, 820, 200);
        
        Label financialTitle = new Label
        {
            Text = "💰 الملخص المالي",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = Color.FromArgb(211, 47, 47),
            AutoSize = true,
            Location = new Point(15, 15)
        };
        financialCard.Controls.Add(financialTitle);
        
        int finYPos = 60;
        AddFinancialRow(financialCard, "المبلغ قبل الضريبة:", invoice.SubTotal.ToString("N2") + " ج.م", finYPos, Color.FromArgb(66, 66, 66));
        finYPos += 35;
        AddFinancialRow(financialCard, "الضريبة:", invoice.TaxAmount.ToString("N2") + " ج.م", finYPos, Color.FromArgb(255, 152, 0));
        finYPos += 35;
        
        // Separator line
        Panel separator = new Panel
        {
            Location = new Point(15, finYPos),
            Size = new Size(790, 2),
            BackColor = Color.FromArgb(200, 200, 200)
        };
        financialCard.Controls.Add(separator);
        finYPos += 10;
        
        AddFinancialRow(financialCard, "الإجمالي:", invoice.TotalAmount.ToString("N2") + " ج.م", finYPos, Color.FromArgb(211, 47, 47), true);
        finYPos += 35;
        AddFinancialRow(financialCard, "المدفوع:", invoice.PaidAmount.ToString("N2") + " ج.م", finYPos, ColorScheme.Success, true);
        finYPos += 35;
        AddFinancialRow(financialCard, "المتبقي:", invoice.RemainingAmount.ToString("N2") + " ج.م", finYPos, ColorScheme.Error, true);
        
        mainPanel.Controls.Add(financialCard);
        yPos += 220;
        
        // Items Card
        if (invoice.Items?.Any() == true)
        {
            Panel itemsCard = CreateCard(20, yPos, 820, 50 + (invoice.Items.Count * 45) + 20);
            
            Label itemsTitle = new Label
            {
                Text = $"📦 الأصناف ({invoice.Items.Count})",
                Font = new Font("Cairo", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(211, 47, 47),
                AutoSize = true,
                Location = new Point(15, 15)
            };
            itemsCard.Controls.Add(itemsTitle);
            
            int itemYPos = 60;
            foreach (var item in invoice.Items)
            {
                Panel itemPanel = new Panel
                {
                    Location = new Point(15, itemYPos),
                    Size = new Size(790, 40),
                    BackColor = Color.FromArgb(248, 249, 250)
                };
                
                Label itemDesc = new Label
                {
                    Text = item.Description,
                    Font = new Font("Cairo", 11F, FontStyle.Bold),
                    Location = new Point(10, 10),
                    AutoSize = true
                };
                itemPanel.Controls.Add(itemDesc);
                
                Label itemQty = new Label
                {
                    Text = $"الكمية: {item.Quantity}",
                    Font = new Font("Cairo", 10F),
                    Location = new Point(400, 12),
                    AutoSize = true
                };
                itemPanel.Controls.Add(itemQty);
                
                Label itemPrice = new Label
                {
                    Text = $"{item.TotalPrice:N2} ج.م",
                    Font = new Font("Cairo", 11F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(211, 47, 47),
                    Location = new Point(650, 10),
                    AutoSize = true
                };
                itemPanel.Controls.Add(itemPrice);
                
                itemsCard.Controls.Add(itemPanel);
                itemYPos += 45;
            }
            
            mainPanel.Controls.Add(itemsCard);
            yPos += itemsCard.Height + 20;
        }
        
        // Notes Card
        if (!string.IsNullOrWhiteSpace(invoice.Notes))
        {
            Panel notesCard = CreateCard(20, yPos, 820, 120);
            
            Label notesTitle = new Label
            {
                Text = "📝 ملاحظات",
                Font = new Font("Cairo", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(211, 47, 47),
                AutoSize = true,
                Location = new Point(15, 15)
            };
            notesCard.Controls.Add(notesTitle);
            
            Label notesText = new Label
            {
                Text = invoice.Notes,
                Font = new Font("Cairo", 11F),
                Location = new Point(15, 55),
                Size = new Size(790, 50),
                ForeColor = Color.FromArgb(66, 66, 66)
            };
            notesCard.Controls.Add(notesText);
            
            mainPanel.Controls.Add(notesCard);
        }
        
        detailsForm.Controls.Add(mainPanel);
        detailsForm.ShowDialog();
    }
    
    private Panel CreateCard(int x, int y, int width, int height)
    {
        return new Panel
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackColor = Color.White,
            BorderStyle = BorderStyle.None
        };
    }
    
    private void AddFinancialRow(Panel parent, string label, string value, int yPos, Color valueColor, bool isBold = false)
    {
        Label lblLabel = new Label
        {
            Text = label,
            Font = new Font("Cairo", isBold ? 13F : 11F, isBold ? FontStyle.Bold : FontStyle.Regular),
            Location = new Point(15, yPos),
            AutoSize = true,
            ForeColor = Color.FromArgb(66, 66, 66)
        };
        parent.Controls.Add(lblLabel);
        
        Label lblValue = new Label
        {
            Text = value,
            Font = new Font("Cairo", isBold ? 14F : 12F, FontStyle.Bold),
            Location = new Point(650, yPos),
            AutoSize = true,
            ForeColor = valueColor
        };
        parent.Controls.Add(lblValue);
    }
    
    private Color GetStatusColor(string status)
    {
        return status switch
        {
            "Paid" => ColorScheme.Success,
            "Partial" => ColorScheme.Warning,
            _ => ColorScheme.Error
        };
    }
    
    // ═══════════════════════════════════════════
    // Export Functions
    // ═══════════════════════════════════════════
    
    private async void ExportToExcel_Click(object? sender, EventArgs e)
    {
        try
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                FileName = $"تقرير_الفواتير_{DateTime.Now:yyyy-MM-dd_HH-mm}.xlsx"
            };
            
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                this.Cursor = Cursors.WaitCursor;
                
                // Get current tab data
                DataGridView? grid = _tabControl.SelectedIndex == 0 ? _salesReportGrid :
                                    _tabControl.SelectedIndex == 1 ? _purchaseReportGrid : null;
                
                if (grid != null && grid.Rows.Count > 0)
                {
                    await _exportService.ExportToExcelAsync(grid, saveDialog.FileName);
                    
                    this.Cursor = Cursors.Default;
                    
                    if (MessageBox.Show("تم تصدير التقرير بنجاح!\n\nهل تريد فتح الملف؟", "نجح",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
                else
                {
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("لا توجد بيانات للتصدير", "تنبيه",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        catch (Exception ex)
        {
            this.Cursor = Cursors.Default;
            MessageBox.Show($"خطأ في التصدير: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async void ExportToPdf_Click(object? sender, EventArgs e)
    {
        try
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                FileName = $"تقرير_الفواتير_{DateTime.Now:yyyy-MM-dd_HH-mm}.pdf"
            };
            
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                this.Cursor = Cursors.WaitCursor;
                
                DataGridView? grid = _tabControl.SelectedIndex == 0 ? _salesReportGrid :
                                    _tabControl.SelectedIndex == 1 ? _purchaseReportGrid : null;
                
                if (grid != null && grid.Rows.Count > 0)
                {
                    string title = _tabControl.SelectedIndex == 0 ? "تقرير المبيعات" : "تقرير المشتريات";
                    await _exportService.ExportToPdfAsync(grid, saveDialog.FileName, title);
                    
                    this.Cursor = Cursors.Default;
                    
                    if (MessageBox.Show("تم تصدير التقرير بنجاح!\n\nهل تريد فتح الملف؟", "نجح",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
                else
                {
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("لا توجد بيانات للتصدير", "تنبيه",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        catch (Exception ex)
        {
            this.Cursor = Cursors.Default;
            MessageBox.Show($"خطأ في التصدير: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void PrintReport_Click(object? sender, EventArgs e)
    {
        try
        {
            DataGridView? grid = _tabControl.SelectedIndex == 0 ? _salesReportGrid :
                                _tabControl.SelectedIndex == 1 ? _purchaseReportGrid : null;
            string reportTitle = _tabControl.SelectedIndex == 0 ? "تقرير المبيعات" : "تقرير المشتريات";
            
            if (grid == null || grid.Rows.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات للطباعة", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += (s, ev) => PrintDocument_PrintPage(s, ev, grid, reportTitle);
            
            PrintDialog printDialog = new PrintDialog { Document = printDoc };
            
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDoc.Print();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في الطباعة: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e, DataGridView grid, string title)
    {
        // Simplified print implementation - can be enhanced later
        Graphics g = e.Graphics!;
        Font titleFont = new Font("Cairo", 16, FontStyle.Bold);
        
        float y = e.MarginBounds.Top;
        float x = e.MarginBounds.Left;
        
        g.DrawString(title, titleFont, Brushes.Black, x, y);
        y += 50;
        
        g.DrawString($"من {_startDatePicker.Value:yyyy/MM/dd} إلى {_endDatePicker.Value:yyyy/MM/dd}", 
            new Font("Cairo", 10), Brushes.Black, x, y);
        
        // Note: Full implementation similar to previous code can be added
        e.HasMorePages = false;
    }
}