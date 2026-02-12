using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using System.Drawing.Printing;

namespace GraceWay.AccountingSystem.Presentation.Forms.Reports;

/// <summary>
/// 📊 تقارير العمرة الاحترافية - تحليل شامل ومفصل
/// </summary>
public partial class UmrahReportsForm : Form
{
    private readonly IUmrahService _umrahService;
    private readonly IExportService _exportService;
    
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
    private ComboBox _roomTypeFilterCombo = null!;
    private Button _clearFiltersButton = null!;
    
    // Data Grids
    private DataGridView _packagesReportGrid = null!;
    private DataGridView _profitabilityGrid = null!;
    
    // Summary Panels
    private Panel _summaryPanel = null!;
    private Panel _analyticsSummaryPanel = null!;
    
    // Data cache
    private List<UmrahPackage> _allPackages = new();
    
    public UmrahReportsForm(IUmrahService umrahService, IExportService exportService)
    {
        _umrahService = umrahService;
        _exportService = exportService;
        
        InitializeComponent();
        SetupForm();
        InitializeCustomControls();
        
        // Load data when form is shown
        this.Load += async (s, e) => await LoadInitialDataAsync();
    }
    
    private void SetupForm()
    {
        this.Text = "📊 تقارير العمرة الاحترافية";
        this.Size = new Size(1600, 950);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
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
            Height = 180,
            BackColor = Color.White,
            Padding = new Padding(30, 20, 30, 20)
        };
        
        // Title with icon
        Label titleLabel = new Label
        {
            Text = "🕌 تقارير حزم العمرة الاحترافية",
            Font = new Font("Cairo", 18F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(30, 20)
        };
        headerPanel.Controls.Add(titleLabel);
        
        Label subtitleLabel = new Label
        {
            Text = "تحليل شامل للحزم والربحية والأداء المالي",
            Font = new Font("Cairo", 10F),
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(30, 55)
        };
        headerPanel.Controls.Add(subtitleLabel);
        
        // Date Range Section
        int yPos = 95;
        Label startLabel = new Label
        {
            Text = "📅 من تاريخ:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(30, yPos)
        };
        headerPanel.Controls.Add(startLabel);
        
        _startDatePicker = new DateTimePicker
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(180, 35),
            Location = new Point(130, yPos - 3),
            Format = DateTimePickerFormat.Short
        };
        _startDatePicker.Value = DateTime.Now.AddMonths(-1);
        headerPanel.Controls.Add(_startDatePicker);
        
        Label endLabel = new Label
        {
            Text = "📅 إلى تاريخ:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(330, yPos)
        };
        headerPanel.Controls.Add(endLabel);
        
        _endDatePicker = new DateTimePicker
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(180, 35),
            Location = new Point(430, yPos - 3),
            Format = DateTimePickerFormat.Short
        };
        headerPanel.Controls.Add(_endDatePicker);
        
        // CheckBox لإظهار كل الحزم
        CheckBox showAllCheckBox = new CheckBox
        {
            Text = "إظهار كل الحزم (تجاهل التاريخ)",
            Font = new Font("Cairo", 9F),
            AutoSize = true,
            Location = new Point(630, yPos + 40),
            Checked = false
        };
        showAllCheckBox.CheckedChanged += (s, e) =>
        {
            _startDatePicker.Enabled = !showAllCheckBox.Checked;
            _endDatePicker.Enabled = !showAllCheckBox.Checked;
        };
        headerPanel.Controls.Add(showAllCheckBox);
        
        // Action Buttons
        _generateButton = CreateStyledButton("📊 إنشاء التقرير", ColorScheme.Primary, new Point(630, yPos - 3));
        _generateButton.Click += (s, e) => GenerateReport_Click(s, e, showAllCheckBox.Checked);
        headerPanel.Controls.Add(_generateButton);
        
        _refreshButton = CreateStyledButton("🔄 تحديث", Color.FromArgb(76, 175, 80), new Point(820, yPos - 3));
        _refreshButton.Click += async (s, e) => await LoadInitialDataAsync();
        headerPanel.Controls.Add(_refreshButton);
        
        _exportExcelButton = CreateStyledButton("📥 Excel", Color.FromArgb(33, 150, 83), new Point(1010, yPos - 3));
        _exportExcelButton.Click += ExportToExcel_Click;
        headerPanel.Controls.Add(_exportExcelButton);
        
        _exportPdfButton = CreateStyledButton("📄 PDF", Color.FromArgb(211, 47, 47), new Point(1200, yPos - 3));
        _exportPdfButton.Click += ExportToPdf_Click;
        headerPanel.Controls.Add(_exportPdfButton);
        
        _printButton = CreateStyledButton("🖨️ طباعة", ColorScheme.Warning, new Point(1390, yPos - 3));
        _printButton.Click += PrintReport_Click;
        headerPanel.Controls.Add(_printButton);
        
        // headerPanel will be added at the end in correct order
        
        // ═══════════════════════════════════════════
        // Filter Panel - شريط الفلاتر
        // ═══════════════════════════════════════════
        Panel filterPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 80,
            BackColor = Color.FromArgb(245, 247, 250),
            Padding = new Padding(30, 15, 30, 15)
        };
        
        Label searchLabel = new Label
        {
            Text = "🔍",
            Font = new Font("Segoe UI Emoji", 14F),
            AutoSize = true,
            Location = new Point(30, 28)
        };
        filterPanel.Controls.Add(searchLabel);
        
        _searchBox = new TextBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(250, 35),
            Location = new Point(65, 25),
            PlaceholderText = "بحث برقم الحزمة، المعتمر، أو الوسيط..."
        };
        _searchBox.TextChanged += (s, e) => ApplyFilters();
        filterPanel.Controls.Add(_searchBox);
        
        Label statusLabel = new Label
        {
            Text = "الحالة:",
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(335, 30)
        };
        filterPanel.Controls.Add(statusLabel);
        
        _statusFilterCombo = new ComboBox
        {
            Font = new Font("Cairo", 9F),
            Size = new Size(150, 35),
            Location = new Point(395, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _statusFilterCombo.Items.AddRange(new object[] { "الكل", "مسودة", "مؤكد", "قيد التنفيذ", "مكتمل", "ملغي" });
        _statusFilterCombo.SelectedIndex = 0;
        _statusFilterCombo.SelectedIndexChanged += (s, e) => ApplyFilters();
        filterPanel.Controls.Add(_statusFilterCombo);
        
        Label roomLabel = new Label
        {
            Text = "نوع الغرفة:",
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(565, 30)
        };
        filterPanel.Controls.Add(roomLabel);
        
        _roomTypeFilterCombo = new ComboBox
        {
            Font = new Font("Cairo", 9F),
            Size = new Size(150, 35),
            Location = new Point(660, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _roomTypeFilterCombo.Items.AddRange(new object[] { "الكل", "فردي", "ثنائي", "ثلاثي", "رباعي" });
        _roomTypeFilterCombo.SelectedIndex = 0;
        _roomTypeFilterCombo.SelectedIndexChanged += (s, e) => ApplyFilters();
        filterPanel.Controls.Add(_roomTypeFilterCombo);
        
        _clearFiltersButton = CreateStyledButton("🗑️ مسح الفلاتر", Color.Gray, new Point(830, 25));
        _clearFiltersButton.Size = new Size(140, 35);
        _clearFiltersButton.Click += ClearFilters_Click;
        filterPanel.Controls.Add(_clearFiltersButton);
        
        // filterPanel will be added at the end in correct order
        
        // ═══════════════════════════════════════════
        // Main Tab Control
        // ═══════════════════════════════════════════
        _tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Padding = new Point(15, 8)
        };
        
        // Tab 1: تقرير الحزم
        var packagesTab = new TabPage("📦 تقرير الحزم");
        InitializePackagesTab(packagesTab);
        _tabControl.TabPages.Add(packagesTab);
        
        // Tab 2: تحليل الربحية
        var profitabilityTab = new TabPage("💰 تحليل الربحية");
        InitializeProfitabilityTab(profitabilityTab);
        _tabControl.TabPages.Add(profitabilityTab);
        
        // Tab 3: التحليلات المالية
        var analyticsTab = new TabPage("📈 التحليلات المالية");
        InitializeAnalyticsTab(analyticsTab);
        _tabControl.TabPages.Add(analyticsTab);
        
        // IMPORTANT: Add controls in reverse order for proper docking
        // (Fill controls first, then Top controls in reverse order)
        this.Controls.Add(_tabControl);    // Dock.Fill - add first
        this.Controls.Add(filterPanel);    // Dock.Top - add second
        this.Controls.Add(headerPanel);    // Dock.Top - add last (appears at top)
    }
    
    // ═══════════════════════════════════════════
    // Initialize Packages Tab
    // ═══════════════════════════════════════════
    private void InitializePackagesTab(TabPage tab)
    {
        tab.BackColor = ColorScheme.Background;
        tab.Padding = new Padding(20);
        
        // Summary Panel
        _summaryPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 140,
            BackColor = Color.White,
            Padding = new Padding(20)
        };
        
        // Summary Cards Container
        FlowLayoutPanel cardsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = true,
            Padding = new Padding(5)
        };
        
        // Card 1: إجمالي الحزم
        cardsPanel.Controls.Add(CreateSummaryCard("📦", "إجمالي الحزم", "0", ColorScheme.Primary));
        
        // Card 2: إجمالي المعتمرين
        cardsPanel.Controls.Add(CreateSummaryCard("👥", "إجمالي المعتمرين", "0", Color.FromArgb(76, 175, 80)));
        
        // Card 3: إجمالي الإيرادات
        cardsPanel.Controls.Add(CreateSummaryCard("💵", "إجمالي الإيرادات", "0.00 ج.م", Color.FromArgb(33, 150, 243)));
        
        // Card 4: إجمالي التكاليف
        cardsPanel.Controls.Add(CreateSummaryCard("💸", "إجمالي التكاليف", "0.00 ج.م", Color.FromArgb(255, 152, 0)));
        
        // Card 5: صافي الربح
        cardsPanel.Controls.Add(CreateSummaryCard("💰", "صافي الربح", "0.00 ج.م", Color.FromArgb(76, 175, 80)));
        
        _summaryPanel.Controls.Add(cardsPanel);
        
        // DataGridView for packages
        _packagesReportGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            Font = new Font("Cairo", 10F),
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = ColorScheme.Primary,
                ForeColor = Color.White,
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(10)
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                SelectionBackColor = Color.FromArgb(230, 240, 255),
                SelectionForeColor = ColorScheme.Primary,
                Padding = new Padding(5)
            },
            RowTemplate = { Height = 40 }
        };
        
        // Add columns
        _packagesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "PackageNumber", 
            HeaderText = "رقم الحزمة",
            Width = 150
        });
        
        _packagesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "Date", 
            HeaderText = "التاريخ",
            Width = 120
        });
        
        _packagesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "TripName", 
            HeaderText = "اسم الرحلة",
            Width = 200
        });
        
        _packagesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "NumberOfPersons", 
            HeaderText = "عدد الأفراد",
            Width = 100
        });
        
        _packagesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "RoomType", 
            HeaderText = "نوع الغرفة",
            Width = 100
        });
        
        _packagesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "TotalRevenue", 
            HeaderText = "إجمالي الإيرادات",
            Width = 150,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
        });
        
        _packagesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "TotalCosts", 
            HeaderText = "إجمالي التكاليف",
            Width = 150,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
        });
        
        _packagesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "NetProfit", 
            HeaderText = "صافي الربح",
            Width = 150,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
        });
        
        _packagesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "ProfitMargin", 
            HeaderText = "هامش الربح %",
            Width = 120,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
        });
        
        _packagesReportGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "Status", 
            HeaderText = "الحالة",
            Width = 120
        });
        
        // IMPORTANT: Add controls in correct order for docking
        tab.Controls.Add(_packagesReportGrid); // Dock.Fill - add first
        tab.Controls.Add(_summaryPanel);        // Dock.Top - add second
    }
    
    // ═══════════════════════════════════════════
    // Initialize Profitability Tab
    // ═══════════════════════════════════════════
    private void InitializeProfitabilityTab(TabPage tab)
    {
        tab.BackColor = ColorScheme.Background;
        tab.Padding = new Padding(20);
        
        _profitabilityGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            Font = new Font("Cairo", 10F),
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(10)
            },
            RowTemplate = { Height = 40 }
        };
        
        // Add profitability columns
        _profitabilityGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "PackageNumber", HeaderText = "رقم الحزمة" });
        _profitabilityGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TripName", HeaderText = "اسم الرحلة" });
        _profitabilityGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "TotalRevenue", 
            HeaderText = "الإيرادات",
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
        });
        _profitabilityGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "TotalCosts", 
            HeaderText = "التكاليف",
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
        });
        _profitabilityGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "NetProfit", 
            HeaderText = "الربح",
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
        });
        _profitabilityGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "ProfitMargin", 
            HeaderText = "الهامش %",
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
        });
        
        tab.Controls.Add(_profitabilityGrid);
    }
    
    private void InitializeAnalyticsTab(TabPage tab)
    {
        tab.BackColor = ColorScheme.Background;
        tab.Padding = new Padding(20);
        
        _analyticsSummaryPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(30),
            AutoScroll = true
        };
        
        Label titleLabel = new Label
        {
            Text = "📊 التحليلات المالية والإحصائيات",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(30, 30)
        };
        _analyticsSummaryPanel.Controls.Add(titleLabel);
        
        tab.Controls.Add(_analyticsSummaryPanel);
    }
    
    // ═══════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════
    private Button CreateStyledButton(string text, Color color, Point location)
    {
        return new Button
        {
            Text = text,
            Font = new Font("Cairo", 9.5F, FontStyle.Bold),
            Size = new Size(170, 40),
            Location = location,
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
    }
    
    private Panel CreateSummaryCard(string icon, string title, string value, Color accentColor)
    {
        Panel card = new Panel
        {
            Size = new Size(280, 100),
            BackColor = Color.White,
            Margin = new Padding(10),
            BorderStyle = BorderStyle.FixedSingle
        };
        
        Label iconLabel = new Label
        {
            Text = icon,
            Font = new Font("Segoe UI Emoji", 24F),
            Location = new Point(15, 20),
            Size = new Size(50, 50),
            ForeColor = accentColor
        };
        card.Controls.Add(iconLabel);
        
        Label titleLabel = new Label
        {
            Text = title,
            Font = new Font("Cairo", 9F),
            Location = new Point(75, 20),
            Size = new Size(190, 25),
            ForeColor = Color.Gray
        };
        card.Controls.Add(titleLabel);
        
        Label valueLabel = new Label
        {
            Text = value,
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            Location = new Point(75, 45),
            Size = new Size(190, 35),
            ForeColor = accentColor,
            Name = "value_" + title.Replace(" ", "_")
        };
        card.Controls.Add(valueLabel);
        
        return card;
    }
    
    // ═══════════════════════════════════════════
    // Data Loading
    // ═══════════════════════════════════════════
    private async Task LoadInitialDataAsync()
    {
        try
        {
            Console.WriteLine("🔄 LoadInitialDataAsync started...");
            
            _allPackages = await _umrahService.GetAllPackagesAsync();
            
            Console.WriteLine($"✅ Loaded {_allPackages.Count} packages from service");
            
            if (!_allPackages.Any())
            {
                Console.WriteLine("⚠️ No packages found in database!");
                MessageBox.Show(
                    "لا توجد حزم عمرة في النظام!\n\n" +
                    "يرجى إضافة حزم أولاً من قسم \"حزم العمرة\" في القائمة الرئيسية.\n\n" +
                    "📍 القائمة الرئيسية ← حزم العمرة ← إضافة حزمة جديدة",
                    "لا توجد بيانات",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                Console.WriteLine($"📦 Sample packages:");
                foreach (var pkg in _allPackages.Take(3))
                {
                    Console.WriteLine($"  - {pkg.PackageNumber}: {pkg.TripName}, Status={pkg.Status}, IsActive={pkg.IsActive}");
                }
            }
            
            ApplyFilters();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in LoadInitialDataAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}\n\nتفاصيل: {ex.InnerException?.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void ApplyFilters()
    {
        var filtered = _allPackages.AsEnumerable();
        
        // Filter by search
        if (!string.IsNullOrWhiteSpace(_searchBox.Text))
        {
            var searchTerm = _searchBox.Text.ToLower();
            filtered = filtered.Where(p => 
                p.PackageNumber.ToLower().Contains(searchTerm) ||
                p.TripName.ToLower().Contains(searchTerm) ||
                (p.BrokerName?.ToLower().Contains(searchTerm) ?? false));
        }
        
        // Filter by status
        if (_statusFilterCombo.SelectedIndex > 0)
        {
            var statusText = _statusFilterCombo.SelectedItem?.ToString();
            // Convert Arabic status back to enum
            filtered = filtered.Where(p => GetStatusArabic(p.Status) == statusText);
        }
        
        // Filter by room type
        if (_roomTypeFilterCombo.SelectedIndex > 0)
        {
            var roomType = _roomTypeFilterCombo.SelectedItem?.ToString();
            filtered = filtered.Where(p => GetRoomTypeArabic(p.RoomType) == roomType);
        }
        
        var filteredList = filtered.ToList();
        LoadPackagesGrid(filteredList);
        LoadProfitabilityGrid(filteredList);
        UpdateSummary(filteredList);
    }
    
    private void LoadPackagesGrid(List<UmrahPackage> packages)
    {
        _packagesReportGrid.Rows.Clear();
        
        foreach (var package in packages)
        {
            _packagesReportGrid.Rows.Add(
                package.PackageNumber,
                package.Date.ToString("yyyy-MM-dd"),
                package.TripName,
                package.NumberOfPersons,
                GetRoomTypeArabic(package.RoomType),
                package.TotalRevenue,
                package.TotalCosts * package.NumberOfPersons,
                package.NetProfit,
                package.ProfitMargin,
                GetStatusArabic(package.Status)
            );
        }
    }
    
    private void LoadProfitabilityGrid(List<UmrahPackage> packages)
    {
        _profitabilityGrid.Rows.Clear();
        
        var sortedPackages = packages.OrderByDescending(p => p.NetProfit).ToList();
        
        foreach (var package in sortedPackages)
        {
            _profitabilityGrid.Rows.Add(
                package.PackageNumber,
                package.TripName,
                package.TotalRevenue,
                package.TotalCosts * package.NumberOfPersons,
                package.NetProfit,
                package.ProfitMargin
            );
        }
    }
    
    private void UpdateSummary(List<UmrahPackage> packages)
    {
        var totalPackages = packages.Count;
        var totalPersons = packages.Sum(p => p.NumberOfPersons);
        var totalRevenue = packages.Sum(p => p.TotalRevenue);
        var totalCosts = packages.Sum(p => p.TotalCosts * p.NumberOfPersons);
        var netProfit = totalRevenue - totalCosts;
        
        UpdateSummaryCard("إجمالي الحزم", totalPackages.ToString());
        UpdateSummaryCard("إجمالي المعتمرين", totalPersons.ToString());
        UpdateSummaryCard("إجمالي الإيرادات", $"{totalRevenue:N2} ج.م");
        UpdateSummaryCard("إجمالي التكاليف", $"{totalCosts:N2} ج.م");
        UpdateSummaryCard("صافي الربح", $"{netProfit:N2} ج.م");
    }
    
    private void UpdateSummaryCard(string title, string value)
    {
        var cardName = "value_" + title.Replace(" ", "_");
        var control = _summaryPanel.Controls.Find(cardName, true).FirstOrDefault();
        if (control is Label label)
        {
            label.Text = value;
        }
    }
    
    // ═══════════════════════════════════════════
    // Event Handlers
    // ═══════════════════════════════════════════
    private void GenerateReport_Click(object? sender, EventArgs e, bool showAll = false)
    {
        try
        {
            Console.WriteLine($"📊 GenerateReport_Click started. showAll={showAll}");
            Console.WriteLine($"📅 Date range: {_startDatePicker.Value:yyyy-MM-dd} to {_endDatePicker.Value:yyyy-MM-dd}");
            Console.WriteLine($"📦 Total packages in _allPackages: {_allPackages?.Count ?? 0}");
            
            // التأكد من وجود بيانات
            if (_allPackages == null || !_allPackages.Any())
            {
                Console.WriteLine("❌ _allPackages is null or empty!");
                MessageBox.Show("لا توجد حزم عمرة في النظام!\n\nيرجى إضافة حزم أولاً من قسم \"حزم العمرة\" في القائمة الرئيسية.", "تنبيه", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            List<UmrahPackage> filtered;
            
            // إذا كان showAll = true، نعرض كل الحزم بدون فلترة
            if (showAll)
            {
                Console.WriteLine("✅ Show all mode - no date filtering");
                filtered = _allPackages;
            }
            else
            {
                // فلترة حسب التاريخ
                Console.WriteLine($"🔍 Filtering by date...");
                var startDate = DateTime.SpecifyKind(_startDatePicker.Value.Date, DateTimeKind.Utc);
                var endDate = DateTime.SpecifyKind(_endDatePicker.Value.Date, DateTimeKind.Utc);
                
                filtered = _allPackages
                    .Where(p => p.Date >= startDate && p.Date <= endDate)
                    .ToList();
                
                Console.WriteLine($"📊 Filtered count: {filtered.Count}");
            }
            
            // التأكد من وجود نتائج بعد الفلترة
            if (!filtered.Any())
            {
                Console.WriteLine($"⚠️ No packages in filtered result!");
                MessageBox.Show(
                    $"لا توجد حزم عمرة في الفترة المحددة!\n\n" +
                    $"الفترة المحددة: من {_startDatePicker.Value:yyyy-MM-dd} إلى {_endDatePicker.Value:yyyy-MM-dd}\n" +
                    $"إجمالي الحزم في النظام: {_allPackages.Count}\n\n" +
                    $"💡 نصيحة: قم بتوسيع نطاق التاريخ أو فعّل \"إظهار كل الحزم\"",
                    "لا توجد بيانات في الفترة المحددة", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            Console.WriteLine($"✅ Updating UI with {filtered.Count} packages...");
            
            // تحديث الشاشة
            LoadPackagesGrid(filtered);
            LoadProfitabilityGrid(filtered);
            UpdateSummary(filtered);
            
            string periodMessage = showAll ? 
                "كل الفترات" : 
                $"من {_startDatePicker.Value:yyyy-MM-dd} إلى {_endDatePicker.Value:yyyy-MM-dd}";
            
            Console.WriteLine($"✅ Report generated successfully!");
            
            MessageBox.Show($"تم إنشاء التقرير بنجاح!\n\nالفترة: {periodMessage}\nعدد الحزم: {filtered.Count}", 
                "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in GenerateReport_Click: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            MessageBox.Show($"خطأ في إنشاء التقرير: {ex.Message}\n\nStack: {ex.StackTrace}", 
                "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void ClearFilters_Click(object? sender, EventArgs e)
    {
        _searchBox.Clear();
        _statusFilterCombo.SelectedIndex = 0;
        _roomTypeFilterCombo.SelectedIndex = 0;
        ApplyFilters();
    }
    
    private void ExportToExcel_Click(object? sender, EventArgs e)
    {
        try
        {
            // التأكد من وجود بيانات
            if (_allPackages == null || !_allPackages.Any())
            {
                MessageBox.Show("لا توجد حزم عمرة في النظام للتصدير!\n\nيرجى إضافة حزم أولاً.", "تنبيه", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // فلترة حسب التاريخ
            var filtered = _allPackages
                .Where(p => p.Date >= _startDatePicker.Value.Date && 
                           p.Date <= _endDatePicker.Value.Date)
                .ToList();
            
            if (!filtered.Any())
            {
                MessageBox.Show(
                    $"لا توجد حزم للتصدير في الفترة المحددة!\n\n" +
                    $"الفترة: من {_startDatePicker.Value:yyyy-MM-dd} إلى {_endDatePicker.Value:yyyy-MM-dd}\n" +
                    $"إجمالي الحزم في النظام: {_allPackages.Count}\n\n" +
                    $"💡 نصيحة: قم بتوسيع نطاق التاريخ",
                    "لا توجد بيانات", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            // اختيار مكان الحفظ
            using var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "حفظ تقرير العمرة",
                FileName = $"تقرير_العمرة_{DateTime.Now:yyyy-MM-dd_HHmm}.xlsx"
            };
            
            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;
            
            // استخدام ClosedXML للتصدير
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("تقرير العمرة");
            
            // إضافة العنوان
            worksheet.Cell(1, 1).Value = "تقرير حزم العمرة";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Range(1, 1, 1, 10).Merge();
            worksheet.Range(1, 1, 1, 10).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            
            // إضافة تاريخ التقرير
            worksheet.Cell(2, 1).Value = $"الفترة: من {_startDatePicker.Value:yyyy-MM-dd} إلى {_endDatePicker.Value:yyyy-MM-dd}";
            worksheet.Range(2, 1, 2, 10).Merge();
            worksheet.Range(2, 1, 2, 10).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            
            // إضافة الرؤوس
            int headerRow = 4;
            worksheet.Cell(headerRow, 1).Value = "رقم الحزمة";
            worksheet.Cell(headerRow, 2).Value = "التاريخ";
            worksheet.Cell(headerRow, 3).Value = "اسم الرحلة";
            worksheet.Cell(headerRow, 4).Value = "عدد الأفراد";
            worksheet.Cell(headerRow, 5).Value = "نوع الغرفة";
            worksheet.Cell(headerRow, 6).Value = "الإيرادات";
            worksheet.Cell(headerRow, 7).Value = "التكاليف";
            worksheet.Cell(headerRow, 8).Value = "صافي الربح";
            worksheet.Cell(headerRow, 9).Value = "هامش الربح %";
            worksheet.Cell(headerRow, 10).Value = "الحالة";
            
            // تنسيق الرؤوس
            var headerRange = worksheet.Range(headerRow, 1, headerRow, 10);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(52, 152, 219);
            headerRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            
            // إضافة البيانات
            int currentRow = headerRow + 1;
            foreach (var package in filtered)
            {
                worksheet.Cell(currentRow, 1).Value = package.PackageNumber;
                worksheet.Cell(currentRow, 2).Value = package.Date.ToString("yyyy-MM-dd");
                worksheet.Cell(currentRow, 3).Value = package.TripName;
                worksheet.Cell(currentRow, 4).Value = package.NumberOfPersons;
                worksheet.Cell(currentRow, 5).Value = GetRoomTypeArabic(package.RoomType);
                worksheet.Cell(currentRow, 6).Value = package.TotalRevenue;
                worksheet.Cell(currentRow, 7).Value = package.TotalCosts * package.NumberOfPersons;
                worksheet.Cell(currentRow, 8).Value = package.NetProfit;
                worksheet.Cell(currentRow, 9).Value = package.ProfitMargin;
                worksheet.Cell(currentRow, 10).Value = GetStatusArabic(package.Status);
                
                currentRow++;
            }
            
            // إضافة الإجماليات
            int totalRow = currentRow + 1;
            worksheet.Cell(totalRow, 1).Value = "الإجماليات";
            worksheet.Cell(totalRow, 1).Style.Font.Bold = true;
            worksheet.Cell(totalRow, 4).Value = filtered.Sum(p => p.NumberOfPersons);
            worksheet.Cell(totalRow, 6).Value = filtered.Sum(p => p.TotalRevenue);
            worksheet.Cell(totalRow, 7).Value = filtered.Sum(p => p.TotalCosts * p.NumberOfPersons);
            worksheet.Cell(totalRow, 8).Value = filtered.Sum(p => p.NetProfit);
            
            var totalRange = worksheet.Range(totalRow, 1, totalRow, 10);
            totalRange.Style.Font.Bold = true;
            totalRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
            
            // تنسيق الأعمدة المالية
            worksheet.Column(6).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Column(7).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Column(8).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Column(9).Style.NumberFormat.Format = "0.00";
            
            // ضبط عرض الأعمدة
            worksheet.Columns().AdjustToContents();
            
            // حفظ الملف
            workbook.SaveAs(saveDialog.FileName);
            
            MessageBox.Show($"تم التصدير بنجاح!\n\nالملف: {saveDialog.FileName}\nعدد الحزم: {filtered.Count}", 
                "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            // فتح الملف
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = saveDialog.FileName,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في التصدير: {ex.Message}", "خطأ", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void ExportToPdf_Click(object? sender, EventArgs e)
    {
        try
        {
            // التأكد من وجود بيانات
            if (_allPackages == null || !_allPackages.Any())
            {
                MessageBox.Show("لا توجد حزم عمرة في النظام للتصدير!\n\nيرجى إضافة حزم أولاً.", "تنبيه", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // فلترة حسب التاريخ
            var filtered = _allPackages
                .Where(p => p.Date >= _startDatePicker.Value.Date && 
                           p.Date <= _endDatePicker.Value.Date)
                .ToList();
            
            if (!filtered.Any())
            {
                MessageBox.Show(
                    $"لا توجد حزم للتصدير في الفترة المحددة!\n\n" +
                    $"الفترة: من {_startDatePicker.Value:yyyy-MM-dd} إلى {_endDatePicker.Value:yyyy-MM-dd}\n" +
                    $"إجمالي الحزم في النظام: {_allPackages.Count}\n\n" +
                    $"💡 نصيحة: قم بتوسيع نطاق التاريخ",
                    "لا توجد بيانات", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            // اختيار مكان الحفظ
            using var saveDialog = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf",
                Title = "حفظ تقرير العمرة",
                FileName = $"تقرير_العمرة_{DateTime.Now:yyyy-MM-dd_HHmm}.pdf"
            };
            
            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;
            
            // استخدام FastReport لإنشاء PDF
            using var report = new FastReport.Report();
            
            // إضافة صفحة
            var page = new FastReport.ReportPage();
            page.Name = "Page1";
            report.Pages.Add(page);
            
            // إضافة DataBand
            var dataBand = new FastReport.DataBand();
            dataBand.Name = "Data1";
            dataBand.Height = FastReport.Utils.Units.Centimeters * 1;
            page.Bands.Add(dataBand);
            
            // إضافة PageHeader
            var pageHeader = new FastReport.PageHeaderBand();
            pageHeader.Name = "PageHeader1";
            pageHeader.Height = FastReport.Utils.Units.Centimeters * 3;
            page.Bands.Add(pageHeader);
            
            // عنوان التقرير
            var titleText = new FastReport.TextObject();
            titleText.Name = "Title";
            titleText.Text = "تقرير حزم العمرة";
            titleText.Bounds = new System.Drawing.RectangleF(0, 10, page.PaperWidth, 40);
            titleText.Font = new System.Drawing.Font("Arial", 18, System.Drawing.FontStyle.Bold);
            titleText.HorzAlign = FastReport.HorzAlign.Center;
            pageHeader.Objects.Add(titleText);
            
            // الفترة
            var periodText = new FastReport.TextObject();
            periodText.Name = "Period";
            periodText.Text = $"الفترة: من {_startDatePicker.Value:yyyy-MM-dd} إلى {_endDatePicker.Value:yyyy-MM-dd}";
            periodText.Bounds = new System.Drawing.RectangleF(0, 50, page.PaperWidth, 30);
            periodText.Font = new System.Drawing.Font("Arial", 12);
            periodText.HorzAlign = FastReport.HorzAlign.Center;
            pageHeader.Objects.Add(periodText);
            
            // إنشاء DataSource
            var dataSource = new System.Data.DataTable("Packages");
            dataSource.Columns.Add("رقم_الحزمة", typeof(string));
            dataSource.Columns.Add("التاريخ", typeof(string));
            dataSource.Columns.Add("اسم_الرحلة", typeof(string));
            dataSource.Columns.Add("عدد_الأفراد", typeof(int));
            dataSource.Columns.Add("الإيرادات", typeof(decimal));
            dataSource.Columns.Add("التكاليف", typeof(decimal));
            dataSource.Columns.Add("صافي_الربح", typeof(decimal));
            dataSource.Columns.Add("الحالة", typeof(string));
            
            foreach (var package in filtered)
            {
                dataSource.Rows.Add(
                    package.PackageNumber,
                    package.Date.ToString("yyyy-MM-dd"),
                    package.TripName,
                    package.NumberOfPersons,
                    package.TotalRevenue,
                    package.TotalCosts * package.NumberOfPersons,
                    package.NetProfit,
                    GetStatusArabic(package.Status)
                );
            }
            
            report.RegisterData(dataSource, "Packages");
            
            // تصدير إلى PDF
            var pdfExport = new FastReport.Export.PdfSimple.PDFSimpleExport();
            report.Prepare();
            report.Export(pdfExport, saveDialog.FileName);
            
            MessageBox.Show($"تم التصدير بنجاح!\n\nالملف: {saveDialog.FileName}\nعدد الحزم: {filtered.Count}", 
                "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            // فتح الملف
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = saveDialog.FileName,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في التصدير: {ex.Message}", "خطأ", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void PrintReport_Click(object? sender, EventArgs e)
    {
        MessageBox.Show("سيتم إضافة الطباعة قريباً", "قريباً", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    
    // ═══════════════════════════════════════════
    // Helper Methods for Arabic Display
    // ═══════════════════════════════════════════
    private string GetRoomTypeArabic(RoomType roomType)
    {
        return roomType switch
        {
            RoomType.Single => "فردي",
            RoomType.Double => "ثنائي",
            RoomType.Triple => "ثلاثي",
            RoomType.Quad => "رباعي",
            _ => roomType.ToString()
        };
    }
    
    private string GetStatusArabic(PackageStatus status)
    {
        return status switch
        {
            PackageStatus.Draft => "مسودة",
            PackageStatus.Confirmed => "مؤكد",
            PackageStatus.InProgress => "قيد التنفيذ",
            PackageStatus.Completed => "مكتمل",
            PackageStatus.Cancelled => "ملغي",
            _ => status.ToString()
        };
    }
}
