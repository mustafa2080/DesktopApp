using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using System.Drawing.Drawing2D;

namespace GraceWay.AccountingSystem.Presentation.Forms.Reports;

/// <summary>
/// 💰 تقرير ربحية العمرة المحسّن - Professional Edition
/// </summary>
public partial class UmrahProfitabilityReport : Form
{
    private readonly IUmrahService _umrahService;
    private readonly IExportService _exportService;
    
    // Main Controls
    private DateTimePicker _startDatePicker = null!;
    private DateTimePicker _endDatePicker = null!;
    private Button _generateButton = null!;
    private Button _exportButton = null!;
    private CheckBox _showAllCheckBox = null!;
    
    // Summary Dashboard
    private Panel _dashboardPanel = null!;
    private List<UmrahPackage> _currentPackages = new();
    
    // Data Grid
    private DataGridView _profitGrid = null!;
    
    // Detail Grid - تفاصيل الحسابات
    private DataGridView _detailGrid = null!;
    private Label _detailTitleLabel = null!;
    private Button _exportDetailsButton = null!;
    private SplitContainer _splitContainer = null!;
    private UmrahPackage? _selectedPackage = null;
    
    public UmrahProfitabilityReport(IUmrahService umrahService, IExportService exportService)
    {
        _umrahService = umrahService;
        _exportService = exportService;
        
        SetupForm();
        InitializeControls();
        
        this.Load += async (s, e) => await LoadDataAsync();
    }
    
    private void SetupForm()
    {
        this.Text = "💰 تقرير ربحية العمرة";
        this.Size = new Size(1600, 950);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = Color.FromArgb(240, 242, 245);
        this.Font = new Font("Cairo", 10F);
        this.WindowState = FormWindowState.Maximized;
    }
    
    private void InitializeControls()
    {
        // ═══════════════════════════════════════════
        // Header Panel with Gradient
        // ═══════════════════════════════════════════
        Panel headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 160,
            BackColor = Color.White
        };
        headerPanel.Paint += (s, e) =>
        {
            // فحص أن المستطيل له أبعاد صحيحة قبل إنشاء الـ gradient
            if (headerPanel.ClientRectangle.Width > 0 && headerPanel.ClientRectangle.Height > 0)
            {
                using var brush = new LinearGradientBrush(
                    headerPanel.ClientRectangle,
                    Color.FromArgb(52, 152, 219),
                    Color.FromArgb(41, 128, 185),
                    LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(brush, headerPanel.ClientRectangle);
            }
        };
        
        // Title with Icon
        Label titleLabel = new Label
        {
            Text = "🕌 تقرير ربحية حزم العمرة",
            Font = new Font("Cairo", 22F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(40, 25),
            BackColor = Color.Transparent
        };
        headerPanel.Controls.Add(titleLabel);
        
        Label subtitleLabel = new Label
        {
            Text = "تحليل شامل للإيرادات والتكاليف والأرباح",
            Font = new Font("Cairo", 11F),
            ForeColor = Color.FromArgb(230, 240, 255),
            AutoSize = true,
            Location = new Point(40, 65),
            BackColor = Color.Transparent
        };
        headerPanel.Controls.Add(subtitleLabel);
        
        // Controls Panel
        Panel controlsPanel = new Panel
        {
            Location = new Point(40, 100),
            Size = new Size(1500, 50),
            BackColor = Color.Transparent
        };
        
        // Date Range
        Label fromLabel = new Label
        {
            Text = "من:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(0, 15),
            BackColor = Color.Transparent
        };
        controlsPanel.Controls.Add(fromLabel);
        
        _startDatePicker = new DateTimePicker
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(160, 35),
            Location = new Point(50, 10),
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Now.AddMonths(-1)
        };
        controlsPanel.Controls.Add(_startDatePicker);
        
        Label toLabel = new Label
        {
            Text = "إلى:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(230, 15),
            BackColor = Color.Transparent
        };
        controlsPanel.Controls.Add(toLabel);
        
        _endDatePicker = new DateTimePicker
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(160, 35),
            Location = new Point(280, 10),
            Format = DateTimePickerFormat.Short
        };
        controlsPanel.Controls.Add(_endDatePicker);
        
        _showAllCheckBox = new CheckBox
        {
            Text = "🔓 عرض جميع الحزم",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(470, 15),
            BackColor = Color.Transparent,
            Checked = true  // افتراضياً مُفعّل
        };
        _showAllCheckBox.CheckedChanged += (s, e) =>
        {
            _startDatePicker.Enabled = !_showAllCheckBox.Checked;
            _endDatePicker.Enabled = !_showAllCheckBox.Checked;
        };
        controlsPanel.Controls.Add(_showAllCheckBox);
        
        // Buttons
        _generateButton = CreateModernButton("📊 إنشاء التقرير", Color.FromArgb(46, 204, 113), new Point(700, 10));
        _generateButton.Click += async (s, e) => await GenerateReportAsync();
        controlsPanel.Controls.Add(_generateButton);
        
        _exportButton = CreateModernButton("📥 تصدير Excel", Color.FromArgb(52, 73, 94), new Point(900, 10));
        _exportButton.Click += ExportToExcel_Click;
        controlsPanel.Controls.Add(_exportButton);
        
        Button refreshButton = CreateModernButton("🔄 تحديث", Color.FromArgb(155, 89, 182), new Point(1100, 10));
        refreshButton.Click += async (s, e) => await LoadDataAsync();
        controlsPanel.Controls.Add(refreshButton);
        
        headerPanel.Controls.Add(controlsPanel);
        
        // ═══════════════════════════════════════════
        // Dashboard Panel - Summary Cards
        // ═══════════════════════════════════════════
        _dashboardPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 200,
            BackColor = Color.Transparent,
            Padding = new Padding(20, 15, 20, 15)
        };
        
        FlowLayoutPanel cardsFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = true,
            Padding = new Padding(10)
        };
        
        // Card 1: Total Packages
        cardsFlow.Controls.Add(CreateDashboardCard(
            "📦", "إجمالي الحزم", "0", "حزمة عمرة",
            Color.FromArgb(52, 152, 219), "totalPackages"));
        
        // Card 2: Total Pilgrims
        cardsFlow.Controls.Add(CreateDashboardCard(
            "👥", "إجمالي المعتمرين", "0", "معتمر",
            Color.FromArgb(155, 89, 182), "totalPilgrims"));
        
        // Card 3: Total Revenue
        cardsFlow.Controls.Add(CreateDashboardCard(
            "💵", "إجمالي الإيرادات", "0.00", "جنيه مصري",
            Color.FromArgb(46, 204, 113), "totalRevenue"));
        
        // Card 4: Total Costs
        cardsFlow.Controls.Add(CreateDashboardCard(
            "💸", "إجمالي التكاليف", "0.00", "جنيه مصري",
            Color.FromArgb(231, 76, 60), "totalCosts"));
        
        // Card 5: Net Profit
        cardsFlow.Controls.Add(CreateDashboardCard(
            "💰", "صافي الربح", "0.00", "جنيه مصري",
            Color.FromArgb(26, 188, 156), "netProfit"));
        
        // Card 6: Avg Profit Margin
        cardsFlow.Controls.Add(CreateDashboardCard(
            "📈", "متوسط هامش الربح", "0.00", "%",
            Color.FromArgb(241, 196, 15), "avgMargin"));
        
        _dashboardPanel.Controls.Add(cardsFlow);
        
        // ═══════════════════════════════════════════
        // Data Grid Panel with Split Container
        // ═══════════════════════════════════════════
        Panel gridPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 10, 20, 20),
            BackColor = Color.Transparent
        };
        
        _splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 350,
            SplitterWidth = 6,
            BackColor = Color.FromArgb(220, 225, 230)
        };
        
        // ═══════════════════════════════════════════
        // Top Panel: Summary Grid
        // ═══════════════════════════════════════════
        _profitGrid = new DataGridView
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
            ColumnHeadersHeight = 45,
            RowTemplate = { Height = 45 },
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(44, 62, 80),
                ForeColor = Color.White,
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(5)
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                SelectionBackColor = Color.FromArgb(52, 152, 219),
                SelectionForeColor = Color.White,
                Padding = new Padding(8)
            },
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(250, 251, 252)
            }
        };
        
        // Add Columns
        _profitGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "PackageNumber", 
            HeaderText = "رقم الحزمة",
            MinimumWidth = 130
        });
        
        _profitGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "Date", 
            HeaderText = "التاريخ",
            MinimumWidth = 110
        });
        
        _profitGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "TripName", 
            HeaderText = "اسم الرحلة",
            MinimumWidth = 200
        });
        
        _profitGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "Persons", 
            HeaderText = "عدد الأفراد",
            MinimumWidth = 100,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        _profitGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "Revenue", 
            HeaderText = "الإيرادات",
            MinimumWidth = 140,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Format = "N2",
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(46, 204, 113),
                Font = new Font("Cairo", 10F, FontStyle.Bold)
            }
        });
        
        _profitGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "Costs", 
            HeaderText = "التكاليف",
            MinimumWidth = 140,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Format = "N2",
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(231, 76, 60),
                Font = new Font("Cairo", 10F, FontStyle.Bold)
            }
        });
        
        _profitGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "Profit", 
            HeaderText = "صافي الربح",
            MinimumWidth = 140,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Format = "N2",
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Font = new Font("Cairo", 11F, FontStyle.Bold)
            }
        });
        
        _profitGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "Margin", 
            HeaderText = "هامش الربح %",
            MinimumWidth = 120,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Format = "N2",
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Cairo", 10F, FontStyle.Bold)
            }
        });
        
        _profitGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "Status", 
            HeaderText = "الحالة",
            MinimumWidth = 110,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // Color-code profit cells
        _profitGrid.CellFormatting += ProfitGrid_CellFormatting;
        
        // Row selection event → show details
        _profitGrid.SelectionChanged += ProfitGrid_SelectionChanged;
        
        _splitContainer.Panel1.Controls.Add(_profitGrid);
        
        // ═══════════════════════════════════════════
        // Bottom Panel: Detail Cost Breakdown
        // ═══════════════════════════════════════════
        Panel detailPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(0)
        };
        
        // Detail Header Panel - يحتوي على العنوان والزر
        Panel detailHeaderPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = Color.FromArgb(41, 128, 185)
        };
        
        // Detail title
        _detailTitleLabel = new Label
        {
            Text = "📋 تفاصيل حسابات الرحلة — اضغط على حزمة لعرض التفاصيل",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            AutoSize = false,
            Height = 40,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(15, 0, 180, 0),
            Dock = DockStyle.Fill
        };
        detailHeaderPanel.Controls.Add(_detailTitleLabel);
        
        // Export Details Button
        _exportDetailsButton = new Button
        {
            Text = "📥 تصدير التفاصيل",
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            Size = new Size(160, 30),
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false  // معطل في البداية
        };
        _exportDetailsButton.FlatAppearance.BorderSize = 0;
        _exportDetailsButton.Click += ExportDetailsToExcel_Click;
        
        // وضع الزر على اليسار
        detailHeaderPanel.Resize += (s, e) => 
        {
            _exportDetailsButton.Location = new Point(10, 5);
        };
        _exportDetailsButton.Location = new Point(10, 5);
        
        detailHeaderPanel.Controls.Add(_exportDetailsButton);
        
        _detailGrid = new DataGridView
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
            ColumnHeadersHeight = 40,
            RowTemplate = { Height = 38 },
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(5)
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                SelectionBackColor = Color.FromArgb(174, 214, 241),
                SelectionForeColor = Color.Black,
                Padding = new Padding(5)
            },
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(245, 248, 250)
            }
        };
        
        // Detail columns
        _detailGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "DetailItem",
            HeaderText = "البند",
            MinimumWidth = 250,
            FillWeight = 35,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleRight
            }
        });
        
        _detailGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "DetailValuePerPerson",
            HeaderText = "تكلفة الفرد (ج.م)",
            MinimumWidth = 160,
            FillWeight = 20,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "N2",
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Cairo", 10F)
            }
        });
        
        _detailGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "DetailTotal",
            HeaderText = "الإجمالي (ج.م)",
            MinimumWidth = 160,
            FillWeight = 20,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "N2",
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Cairo", 10F, FontStyle.Bold)
            }
        });
        
        _detailGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "DetailPercentage",
            HeaderText = "النسبة %",
            MinimumWidth = 120,
            FillWeight = 15,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "N2",
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        _detailGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "DetailNotes",
            HeaderText = "ملاحظات",
            MinimumWidth = 200,
            FillWeight = 10,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Cairo", 9F),
                ForeColor = Color.Gray,
                Alignment = DataGridViewContentAlignment.MiddleRight
            }
        });
        
        // Color-code detail rows
        _detailGrid.CellFormatting += DetailGrid_CellFormatting;
        
        detailPanel.Controls.Add(_detailGrid);
        detailPanel.Controls.Add(detailHeaderPanel);
        
        _splitContainer.Panel2.Controls.Add(detailPanel);
        
        gridPanel.Controls.Add(_splitContainer);
        
        // IMPORTANT: Add controls in reverse order for proper docking
        // (Fill controls first, then Top controls)
        this.Controls.Add(gridPanel);      // Dock.Fill - add first
        this.Controls.Add(_dashboardPanel); // Dock.Top - add second
        this.Controls.Add(headerPanel);     // Dock.Top - add last (appears at top)
    }
    
    // ═══════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════
    private Button CreateModernButton(string text, Color color, Point location)
    {
        var btn = new Button
        {
            Text = text,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Size = new Size(180, 40),
            Location = location,
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        
        btn.FlatAppearance.BorderSize = 0;
        btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(color, 0.2f);
        btn.MouseLeave += (s, e) => btn.BackColor = color;
        
        return btn;
    }
    
    private Panel CreateDashboardCard(string icon, string title, string value, string unit, Color accentColor, string cardId)
    {
        Panel card = new Panel
        {
            Size = new Size(250, 160),
            BackColor = Color.White,
            Margin = new Padding(8),
            Tag = cardId
        };
        
        // Add subtle shadow effect
        card.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(Color.FromArgb(230, 232, 235), 2);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };
        
        // Top colored bar
        Panel colorBar = new Panel
        {
            Height = 6,
            Dock = DockStyle.Top,
            BackColor = accentColor
        };
        card.Controls.Add(colorBar);
        
        // Icon
        Label iconLabel = new Label
        {
            Text = icon,
            Font = new Font("Segoe UI Emoji", 32F),
            Location = new Point(20, 25),
            Size = new Size(60, 60),
            ForeColor = accentColor,
            BackColor = Color.Transparent
        };
        card.Controls.Add(iconLabel);
        
        // Title
        Label titleLabel = new Label
        {
            Text = title,
            Font = new Font("Cairo", 9F),
            Location = new Point(20, 95),
            Size = new Size(210, 25),
            ForeColor = Color.FromArgb(108, 117, 125),
            BackColor = Color.Transparent
        };
        card.Controls.Add(titleLabel);
        
        // Value
        Label valueLabel = new Label
        {
            Text = value,
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            Location = new Point(95, 30),
            Size = new Size(140, 40),
            ForeColor = accentColor,
            BackColor = Color.Transparent,
            Name = $"value_{cardId}"
        };
        card.Controls.Add(valueLabel);
        
        // Unit
        Label unitLabel = new Label
        {
            Text = unit,
            Font = new Font("Cairo", 8F),
            Location = new Point(95, 70),
            Size = new Size(140, 20),
            ForeColor = Color.Gray,
            BackColor = Color.Transparent
        };
        card.Controls.Add(unitLabel);
        
        return card;
    }
    
    private void ProfitGrid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (_profitGrid.Columns[e.ColumnIndex].Name == "Profit" && e.Value != null)
        {
            if (decimal.TryParse(e.Value.ToString(), out decimal profit))
            {
                if (profit > 0)
                {
                    e.CellStyle.ForeColor = Color.FromArgb(46, 204, 113);
                }
                else if (profit < 0)
                {
                    e.CellStyle.ForeColor = Color.FromArgb(231, 76, 60);
                }
                else
                {
                    e.CellStyle.ForeColor = Color.Gray;
                }
            }
        }
        
        if (_profitGrid.Columns[e.ColumnIndex].Name == "Margin" && e.Value != null)
        {
            if (decimal.TryParse(e.Value.ToString(), out decimal margin))
            {
                if (margin >= 30)
                {
                    e.CellStyle.BackColor = Color.FromArgb(212, 237, 218);
                    e.CellStyle.ForeColor = Color.FromArgb(21, 87, 36);
                }
                else if (margin >= 15)
                {
                    e.CellStyle.BackColor = Color.FromArgb(255, 243, 205);
                    e.CellStyle.ForeColor = Color.FromArgb(133, 100, 4);
                }
                else if (margin > 0)
                {
                    e.CellStyle.BackColor = Color.FromArgb(248, 215, 218);
                    e.CellStyle.ForeColor = Color.FromArgb(114, 28, 36);
                }
                else
                {
                    e.CellStyle.BackColor = Color.FromArgb(220, 53, 69);
                    e.CellStyle.ForeColor = Color.White;
                }
            }
        }
        
        if (_profitGrid.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
        {
            string status = e.Value.ToString() ?? "";
            
            switch (status)
            {
                case "مكتمل":
                    e.CellStyle.BackColor = Color.FromArgb(46, 204, 113);
                    e.CellStyle.ForeColor = Color.White;
                    break;
                case "قيد التنفيذ":
                    e.CellStyle.BackColor = Color.FromArgb(52, 152, 219);
                    e.CellStyle.ForeColor = Color.White;
                    break;
                case "مؤكد":
                    e.CellStyle.BackColor = Color.FromArgb(241, 196, 15);
                    e.CellStyle.ForeColor = Color.White;
                    break;
                case "ملغي":
                    e.CellStyle.BackColor = Color.FromArgb(231, 76, 60);
                    e.CellStyle.ForeColor = Color.White;
                    break;
                case "مسودة":
                    e.CellStyle.BackColor = Color.FromArgb(149, 165, 166);
                    e.CellStyle.ForeColor = Color.White;
                    break;
            }
        }
    }
    
    // ═══════════════════════════════════════════
    // Data Loading & Processing
    // ═══════════════════════════════════════════
    private async Task LoadDataAsync()
    {
        try
        {
            // Load all packages with detailed logging
            _currentPackages = await _umrahService.GetAllPackagesAsync();
            
            Console.WriteLine($"📦 Total packages loaded: {_currentPackages.Count}");
            
            if (_currentPackages.Any())
            {
                Console.WriteLine($"📊 Date range of packages:");
                var minDate = _currentPackages.Min(p => p.Date);
                var maxDate = _currentPackages.Max(p => p.Date);
                Console.WriteLine($"   From: {minDate:yyyy-MM-dd}");
                Console.WriteLine($"   To: {maxDate:yyyy-MM-dd}");
                
                // Set date pickers to cover all data
                _startDatePicker.Value = minDate.Date;
                _endDatePicker.Value = maxDate.Date;
                
                // Generate report immediately after loading data
                await GenerateReportAsync();
            }
            else
            {
                Console.WriteLine("⚠️ No packages found in database");
                MessageBox.Show("لا توجد حزم عمرة في قاعدة البيانات", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error loading data: {ex.Message}");
            MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private Task GenerateReportAsync()
    {
        try
        {
            Console.WriteLine($"🔄 GenerateReportAsync started...");
            Console.WriteLine($"   _currentPackages count: {_currentPackages?.Count ?? 0}");
            
            if (_currentPackages == null || !_currentPackages.Any())
            {
                Console.WriteLine("❌ No packages available - clearing grid");
                _profitGrid.Rows.Clear();
                UpdateDashboard(new List<UmrahPackage>());
                return Task.CompletedTask;
            }
            
            List<UmrahPackage> filtered;
            
            if (_showAllCheckBox.Checked)
            {
                filtered = _currentPackages;
                Console.WriteLine($"🔓 Showing ALL packages: {filtered.Count}");
            }
            else
            {
                var startDate = _startDatePicker.Value.Date;
                var endDate = _endDatePicker.Value.Date.AddDays(1).AddSeconds(-1); // Include full end day
                
                Console.WriteLine($"📅 Filtering by date range:");
                Console.WriteLine($"   Start: {startDate:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"   End: {endDate:yyyy-MM-dd HH:mm:ss}");
                
                filtered = _currentPackages
                    .Where(p => {
                        var packageDate = p.Date.Date;
                        var inRange = packageDate >= startDate && packageDate <= endDate;
                        Console.WriteLine($"   Package {p.PackageNumber}: {packageDate:yyyy-MM-dd} -> {(inRange ? "✅" : "❌")}");
                        return inRange;
                    })
                    .ToList();
                
                Console.WriteLine($"📊 Filtered result: {filtered.Count} packages");
            }
            
            if (!filtered.Any())
            {
                Console.WriteLine("⚠️ No packages match filter criteria - clearing grid");
                _profitGrid.Rows.Clear();
                UpdateDashboard(new List<UmrahPackage>());
                return Task.CompletedTask;
            }
            
            Console.WriteLine($"✅ Updating dashboard with {filtered.Count} packages...");
            UpdateDashboard(filtered);
            
            Console.WriteLine($"✅ Updating grid with {filtered.Count} packages...");
            UpdateGrid(filtered);
            
            Console.WriteLine($"✅ Report generation completed successfully!");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error generating report: {ex.Message}");
            Console.WriteLine($"   StackTrace: {ex.StackTrace}");
            MessageBox.Show($"خطأ في إنشاء التقرير: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return Task.CompletedTask;
        }
    }
    
    private void UpdateDashboard(List<UmrahPackage> packages)
    {
        Console.WriteLine($"📊 UpdateDashboard called with {packages.Count} packages");
        
        // Calculate statistics
        int totalPackages = packages.Count;
        int totalPilgrims = packages.Sum(p => p.NumberOfPersons);
        decimal totalRevenue = packages.Sum(p => p.TotalRevenue);
        decimal totalCosts = packages.Sum(p => p.TotalCosts * p.NumberOfPersons);
        decimal netProfit = totalRevenue - totalCosts;
        decimal avgMargin = totalPackages > 0 ? packages.Average(p => p.ProfitMargin) : 0;
        
        Console.WriteLine($"   Total packages: {totalPackages}");
        Console.WriteLine($"   Total pilgrims: {totalPilgrims}");
        Console.WriteLine($"   Total revenue: {totalRevenue:N2}");
        Console.WriteLine($"   Total costs: {totalCosts:N2}");
        Console.WriteLine($"   Net profit: {netProfit:N2}");
        Console.WriteLine($"   Avg margin: {avgMargin:N2}%");
        
        // Update cards
        UpdateCardValue("totalPackages", totalPackages.ToString("N0"));
        UpdateCardValue("totalPilgrims", totalPilgrims.ToString("N0"));
        UpdateCardValue("totalRevenue", totalRevenue.ToString("N2"));
        UpdateCardValue("totalCosts", totalCosts.ToString("N2"));
        UpdateCardValue("netProfit", netProfit.ToString("N2"));
        UpdateCardValue("avgMargin", avgMargin.ToString("N2"));
        
        Console.WriteLine("✅ Dashboard updated successfully");
    }
    
    private void UpdateCardValue(string cardId, string value)
    {
        Console.WriteLine($"   🔄 Updating card '{cardId}' with value: {value}");
        
        foreach (Control card in _dashboardPanel.Controls[0].Controls)
        {
            if (card is Panel panel && panel.Tag?.ToString() == cardId)
            {
                var valueLabel = panel.Controls.Find($"value_{cardId}", true).FirstOrDefault();
                if (valueLabel is Label lbl)
                {
                    lbl.Text = value;
                    Console.WriteLine($"      ✅ Card '{cardId}' updated successfully");
                }
                else
                {
                    Console.WriteLine($"      ❌ Label not found for card '{cardId}'");
                }
                break;
            }
        }
    }
    
    private void UpdateGrid(List<UmrahPackage> packages)
    {
        Console.WriteLine($"📋 UpdateGrid called with {packages.Count} packages");
        _profitGrid.Rows.Clear();
        
        // Sort by profit (highest first)
        var sorted = packages.OrderByDescending(p => p.NetProfit).ToList();
        Console.WriteLine($"   Sorted {sorted.Count} packages by profit");
        
        int rowIndex = 0;
        foreach (var package in sorted)
        {
            try
            {
                Console.WriteLine($"   Adding row {rowIndex}: {package.PackageNumber}");
                _profitGrid.Rows.Add(
                    package.PackageNumber,
                    package.Date.ToString("yyyy-MM-dd"),
                    package.TripName,
                    package.NumberOfPersons,
                    package.TotalRevenue,
                    package.TotalCosts * package.NumberOfPersons,
                    package.NetProfit,
                    package.ProfitMargin,
                    GetStatusArabic(package.Status)
                );
                rowIndex++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error adding row {rowIndex}: {ex.Message}");
            }
        }
        
        Console.WriteLine($"✅ Grid updated with {_profitGrid.Rows.Count} rows");
    }
    
    // ═══════════════════════════════════════════
    // Detail Panel - تفاصيل حسابات الحزمة
    // ═══════════════════════════════════════════
    private void ProfitGrid_SelectionChanged(object? sender, EventArgs e)
    {
        if (_profitGrid.SelectedRows.Count == 0 || _currentPackages == null) return;
        
        var selectedRow = _profitGrid.SelectedRows[0];
        var packageNumber = selectedRow.Cells["PackageNumber"].Value?.ToString();
        
        if (string.IsNullOrEmpty(packageNumber)) return;
        
        var package = _currentPackages.FirstOrDefault(p => p.PackageNumber == packageNumber);
        if (package != null)
        {
            ShowPackageDetails(package);
        }
    }
    
    private void ShowPackageDetails(UmrahPackage package)
    {
        // حفظ الباكج المختارة
        _selectedPackage = package;
        
        // تفعيل زر التصدير
        _exportDetailsButton.Enabled = true;
        
        _detailGrid.Rows.Clear();
        
        // Update title
        _detailTitleLabel.Text = $"📋 تفاصيل حسابات: {package.PackageNumber} — {package.TripName} ({package.NumberOfPersons} أفراد)";
        
        int persons = package.NumberOfPersons;
        decimal totalCostPerPerson = package.TotalCosts;
        decimal totalCostAll = totalCostPerPerson * persons;
        
        // ═══════════════════════════════════════════
        // قسم الإيرادات
        // ═══════════════════════════════════════════
        int hdrRow = _detailGrid.Rows.Add("💰 الإيرادات", "", "", "", "");
        _detailGrid.Rows[hdrRow].DefaultCellStyle.BackColor = Color.FromArgb(46, 204, 113);
        _detailGrid.Rows[hdrRow].DefaultCellStyle.ForeColor = Color.White;
        _detailGrid.Rows[hdrRow].DefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        
        _detailGrid.Rows.Add(
            "  💵 سعر البيع للفرد",
            package.SellingPrice,
            package.SellingPrice * persons,
            100m,
            $"سعر البيع × {persons} فرد"
        );
        
        // ═══════════════════════════════════════════
        // قسم التكاليف
        // ═══════════════════════════════════════════
        hdrRow = _detailGrid.Rows.Add("💸 التكاليف", "", "", "", "");
        _detailGrid.Rows[hdrRow].DefaultCellStyle.BackColor = Color.FromArgb(231, 76, 60);
        _detailGrid.Rows[hdrRow].DefaultCellStyle.ForeColor = Color.White;
        _detailGrid.Rows[hdrRow].DefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        
        // Helper function to avoid division by zero
        decimal CostPct(decimal val) => totalCostPerPerson > 0 ? (val / totalCostPerPerson * 100) : 0;
        
        // 1. تكلفة التأشيرة
        _detailGrid.Rows.Add(
            "  🪪 تكلفة التأشيرة",
            package.VisaPriceEGP,
            package.VisaPriceEGP * persons,
            CostPct(package.VisaPriceEGP),
            $"{package.VisaPriceSAR:N2} ريال × {package.SARExchangeRate:N2}"
        );
        
        // 2. تكلفة الإقامة
        _detailGrid.Rows.Add(
            "  🏨 تكلفة الإقامة",
            package.AccommodationTotal,
            package.AccommodationTotal * persons,
            CostPct(package.AccommodationTotal),
            $"مكة: {package.MakkahHotel} ({package.MakkahNights} ليالي) — المدينة: {package.MadinahHotel} ({package.MadinahNights} ليالي)"
        );
        
        // 3. تكلفة الباركود
        if (package.BarcodePrice > 0)
        {
            _detailGrid.Rows.Add(
                "  📱 تكلفة الباركود",
                package.BarcodePrice,
                package.BarcodePrice * persons,
                CostPct(package.BarcodePrice),
                ""
            );
        }
        
        // 4. تكلفة الطيران
        if (package.FlightPrice > 0)
        {
            _detailGrid.Rows.Add(
                "  ✈️ تكلفة الطيران",
                package.FlightPrice,
                package.FlightPrice * persons,
                CostPct(package.FlightPrice),
                $"وسيلة السفر: {package.TransportMethod}"
            );
        }
        
        // 5. تكلفة القطار السريع
        if (package.FastTrainPriceSAR > 0)
        {
            _detailGrid.Rows.Add(
                "  🚄 القطار السريع",
                package.FastTrainPriceEGP,
                package.FastTrainPriceEGP * persons,
                CostPct(package.FastTrainPriceEGP),
                $"{package.FastTrainPriceSAR:N2} ريال × {package.SARExchangeRate:N2}"
            );
        }
        
        // 6. العمولة
        if (package.Commission > 0)
        {
            _detailGrid.Rows.Add(
                "  🤝 العمولة",
                package.Commission,
                package.Commission * persons,
                CostPct(package.Commission),
                !string.IsNullOrEmpty(package.BrokerName) ? $"الوسيط: {package.BrokerName}" : ""
            );
        }
        
        // 7. مصاريف المشرف
        if (package.SupervisorExpenses > 0)
        {
            _detailGrid.Rows.Add(
                "  👤 مصاريف المشرف",
                package.SupervisorExpenses,
                package.SupervisorExpenses * persons,
                CostPct(package.SupervisorExpenses),
                !string.IsNullOrEmpty(package.SupervisorName) ? $"المشرف: {package.SupervisorName}" : ""
            );
        }
        
        // ═══════════════════════════════════════════
        // إجمالي التكاليف
        // ═══════════════════════════════════════════
        int totalRow = _detailGrid.Rows.Add(
            "📊 إجمالي التكاليف للفرد",
            totalCostPerPerson,
            totalCostAll,
            100m,
            $"{persons} فرد × {totalCostPerPerson:N2} = {totalCostAll:N2}"
        );
        _detailGrid.Rows[totalRow].DefaultCellStyle.BackColor = Color.FromArgb(255, 205, 210);
        _detailGrid.Rows[totalRow].DefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        
        // ═══════════════════════════════════════════
        // سطر فارغ
        // ═══════════════════════════════════════════
        _detailGrid.Rows.Add("", "", "", "", "");
        
        // ═══════════════════════════════════════════
        // إجمالي الإيرادات
        // ═══════════════════════════════════════════
        int revRow = _detailGrid.Rows.Add(
            "💵 إجمالي الإيرادات",
            package.SellingPrice,
            package.TotalRevenue,
            "",
            $"{persons} فرد × {package.SellingPrice:N2}"
        );
        _detailGrid.Rows[revRow].DefaultCellStyle.BackColor = Color.FromArgb(200, 230, 201);
        _detailGrid.Rows[revRow].DefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        
        // ═══════════════════════════════════════════
        // صافي الربح
        // ═══════════════════════════════════════════
        int profitRow = _detailGrid.Rows.Add(
            "💎 صافي الربح",
            package.NetProfitPerPerson,
            package.NetProfit,
            package.ProfitMargin,
            package.NetProfit >= 0 ? "✅ ربح" : "❌ خسارة"
        );
        Color profitColor = package.NetProfit >= 0 ? Color.FromArgb(46, 204, 113) : Color.FromArgb(231, 76, 60);
        _detailGrid.Rows[profitRow].DefaultCellStyle.BackColor = profitColor;
        _detailGrid.Rows[profitRow].DefaultCellStyle.ForeColor = Color.White;
        _detailGrid.Rows[profitRow].DefaultCellStyle.Font = new Font("Cairo", 13F, FontStyle.Bold);
        
        // ═══════════════════════════════════════════
        // معلومات إضافية
        // ═══════════════════════════════════════════
        hdrRow = _detailGrid.Rows.Add("📝 معلومات إضافية", "", "", "", "");
        _detailGrid.Rows[hdrRow].DefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
        _detailGrid.Rows[hdrRow].DefaultCellStyle.ForeColor = Color.White;
        _detailGrid.Rows[hdrRow].DefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        
        _detailGrid.Rows.Add("  🏷️ نوع الغرفة", "", "", "", package.GetRoomTypeDisplay());
        _detailGrid.Rows.Add("  🏨 إجمالي الليالي", "", "", "", $"{package.TotalNights} ليلة (مكة: {package.MakkahNights} + المدينة: {package.MadinahNights})");
        _detailGrid.Rows.Add("  🚌 وسيلة السفر", "", "", "", package.TransportMethod);
        _detailGrid.Rows.Add("  💱 سعر صرف الريال", "", "", "", $"{package.SARExchangeRate:N2} ج.م");
        _detailGrid.Rows.Add("  📋 الحالة", "", "", "", package.GetStatusDisplay());
        
        if (!string.IsNullOrEmpty(package.Notes))
        {
            _detailGrid.Rows.Add("  📌 ملاحظات", "", "", "", package.Notes);
        }
    }
    
    private void DetailGrid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        // No additional formatting needed — already handled inline
    }
    
    private void ExportToExcel_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_currentPackages == null || !_currentPackages.Any())
            {
                MessageBox.Show("لا توجد بيانات للتصدير!", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // تحديد الحزم المعروضة حالياً
            List<UmrahPackage> packagesToExport;
            if (_showAllCheckBox.Checked)
            {
                packagesToExport = _currentPackages.OrderByDescending(p => p.NetProfit).ToList();
            }
            else
            {
                var startDate = _startDatePicker.Value.Date;
                var endDate = _endDatePicker.Value.Date.AddDays(1).AddSeconds(-1);
                packagesToExport = _currentPackages
                    .Where(p => p.Date.Date >= startDate && p.Date.Date <= endDate)
                    .OrderByDescending(p => p.NetProfit)
                    .ToList();
            }

            if (!packagesToExport.Any())
            {
                MessageBox.Show("لا توجد حزم في النطاق الزمني المحدد!", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "حفظ تقرير ربحية العمرة - تفاصيل الحسابات",
                FileName = $"تفاصيل_حسابات_العمرة_{DateTime.Now:yyyy-MM-dd_HHmm}.xlsx"
            };

            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;

            using var workbook = new ClosedXML.Excel.XLWorkbook();

            // ═══════════════════════════════════════════
            // ورقة واحدة لكل حزمة (تفاصيل حسابات)
            // ═══════════════════════════════════════════
            foreach (var package in packagesToExport)
            {
                // اسم الورقة: رقم الحزمة (مع تنظيف الرموز غير المسموح بها)
                string sheetName = package.PackageNumber
                    .Replace("/", "-").Replace("\\", "-").Replace("*", "")
                    .Replace("[", "").Replace("]", "").Replace(":", "-")
                    .Replace("?", "");
                if (sheetName.Length > 31) sheetName = sheetName.Substring(0, 31);

                var ws = workbook.Worksheets.Add(sheetName);

                int persons = package.NumberOfPersons;
                decimal totalCostPerPerson = package.TotalCosts;

                // ─── العنوان الرئيسي ───
                ws.Cell(1, 1).Value = "تفاصيل حسابات حزمة العمرة";
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 18;
                ws.Cell(1, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(41, 128, 185);
                ws.Cell(1, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                ws.Range(1, 1, 1, 5).Merge();
                ws.Range(1, 1, 1, 5).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                // ─── معلومات الحزمة ───
                ws.Cell(2, 1).Value = $"رقم الحزمة: {package.PackageNumber}";
                ws.Range(2, 1, 2, 2).Merge();
                ws.Cell(2, 3).Value = $"اسم الرحلة: {package.TripName}";
                ws.Range(2, 3, 2, 5).Merge();

                ws.Cell(3, 1).Value = $"عدد الأفراد: {persons}";
                ws.Range(3, 1, 3, 2).Merge();
                ws.Cell(3, 3).Value = $"التاريخ: {package.Date:yyyy-MM-dd}";
                ws.Range(3, 3, 3, 5).Merge();

                ws.Cell(4, 1).Value = $"الحالة: {GetStatusArabic(package.Status)}";
                ws.Range(4, 1, 4, 2).Merge();
                ws.Cell(4, 3).Value = $"سعر صرف الريال: {package.SARExchangeRate:N2} ج.م";
                ws.Range(4, 3, 4, 5).Merge();

                // ─── رؤوس الأعمدة ───
                int headerRow = 6;
                string[] headers = { "البند", "تكلفة الفرد (ج.م)", "الإجمالي (ج.م)", "النسبة %", "ملاحظات" };
                for (int i = 0; i < headers.Length; i++)
                    ws.Cell(headerRow, i + 1).Value = headers[i];

                var headerRange = ws.Range(headerRow, 1, headerRow, 5);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(52, 73, 94);
                headerRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                int row = headerRow + 1;

                // ─── قسم الإيرادات ───
                ws.Cell(row, 1).Value = "💰 الإيرادات";
                var revHdrRange = ws.Range(row, 1, row, 5);
                revHdrRange.Style.Font.Bold = true;
                revHdrRange.Style.Font.FontSize = 12;
                revHdrRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(46, 204, 113);
                revHdrRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                row++;

                ws.Cell(row, 1).Value = "  💵 سعر البيع للفرد";
                ws.Cell(row, 2).Value = package.SellingPrice;
                ws.Cell(row, 3).Value = package.SellingPrice * persons;
                ws.Cell(row, 4).Value = 100m;
                ws.Cell(row, 5).Value = $"سعر البيع × {persons} فرد";
                row++;

                // ─── قسم التكاليف ───
                ws.Cell(row, 1).Value = "💸 التكاليف";
                var costHdrRange = ws.Range(row, 1, row, 5);
                costHdrRange.Style.Font.Bold = true;
                costHdrRange.Style.Font.FontSize = 12;
                costHdrRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(231, 76, 60);
                costHdrRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                row++;

                decimal CostPct(decimal val) => totalCostPerPerson > 0 ? (val / totalCostPerPerson * 100) : 0;

                // تكلفة التأشيرة
                ws.Cell(row, 1).Value = "  🪪 تكلفة التأشيرة";
                ws.Cell(row, 2).Value = package.VisaPriceEGP;
                ws.Cell(row, 3).Value = package.VisaPriceEGP * persons;
                ws.Cell(row, 4).Value = CostPct(package.VisaPriceEGP);
                ws.Cell(row, 5).Value = $"{package.VisaPriceSAR:N2} ريال × {package.SARExchangeRate:N2}";
                row++;

                // تكلفة الإقامة
                ws.Cell(row, 1).Value = "  🏨 تكلفة الإقامة";
                ws.Cell(row, 2).Value = package.AccommodationTotal;
                ws.Cell(row, 3).Value = package.AccommodationTotal * persons;
                ws.Cell(row, 4).Value = CostPct(package.AccommodationTotal);
                ws.Cell(row, 5).Value = $"مكة: {package.MakkahHotel} ({package.MakkahNights} ليالي) — المدينة: {package.MadinahHotel} ({package.MadinahNights} ليالي)";
                row++;

                // تكلفة الباركود
                if (package.BarcodePrice > 0)
                {
                    ws.Cell(row, 1).Value = "  📱 تكلفة الباركود";
                    ws.Cell(row, 2).Value = package.BarcodePrice;
                    ws.Cell(row, 3).Value = package.BarcodePrice * persons;
                    ws.Cell(row, 4).Value = CostPct(package.BarcodePrice);
                    ws.Cell(row, 5).Value = "";
                    row++;
                }

                // تكلفة الطيران
                if (package.FlightPrice > 0)
                {
                    ws.Cell(row, 1).Value = "  ✈️ تكلفة الطيران";
                    ws.Cell(row, 2).Value = package.FlightPrice;
                    ws.Cell(row, 3).Value = package.FlightPrice * persons;
                    ws.Cell(row, 4).Value = CostPct(package.FlightPrice);
                    ws.Cell(row, 5).Value = $"وسيلة السفر: {package.TransportMethod}";
                    row++;
                }

                // القطار السريع
                if (package.FastTrainPriceSAR > 0)
                {
                    ws.Cell(row, 1).Value = "  🚄 القطار السريع";
                    ws.Cell(row, 2).Value = package.FastTrainPriceEGP;
                    ws.Cell(row, 3).Value = package.FastTrainPriceEGP * persons;
                    ws.Cell(row, 4).Value = CostPct(package.FastTrainPriceEGP);
                    ws.Cell(row, 5).Value = $"{package.FastTrainPriceSAR:N2} ريال × {package.SARExchangeRate:N2}";
                    row++;
                }

                // العمولة
                if (package.Commission > 0)
                {
                    ws.Cell(row, 1).Value = "  🤝 العمولة";
                    ws.Cell(row, 2).Value = package.Commission;
                    ws.Cell(row, 3).Value = package.Commission * persons;
                    ws.Cell(row, 4).Value = CostPct(package.Commission);
                    ws.Cell(row, 5).Value = !string.IsNullOrEmpty(package.BrokerName) ? $"الوسيط: {package.BrokerName}" : "";
                    row++;
                }

                // مصاريف المشرف
                if (package.SupervisorExpenses > 0)
                {
                    ws.Cell(row, 1).Value = "  👤 مصاريف المشرف";
                    ws.Cell(row, 2).Value = package.SupervisorExpenses;
                    ws.Cell(row, 3).Value = package.SupervisorExpenses * persons;
                    ws.Cell(row, 4).Value = CostPct(package.SupervisorExpenses);
                    ws.Cell(row, 5).Value = !string.IsNullOrEmpty(package.SupervisorName) ? $"المشرف: {package.SupervisorName}" : "";
                    row++;
                }

                // ─── إجمالي التكاليف ───
                ws.Cell(row, 1).Value = "📊 إجمالي التكاليف للفرد";
                ws.Cell(row, 2).Value = totalCostPerPerson;
                ws.Cell(row, 3).Value = totalCostPerPerson * persons;
                ws.Cell(row, 4).Value = 100m;
                ws.Cell(row, 5).Value = $"{persons} فرد × {totalCostPerPerson:N2} = {totalCostPerPerson * persons:N2}";
                var totalCostRange = ws.Range(row, 1, row, 5);
                totalCostRange.Style.Font.Bold = true;
                totalCostRange.Style.Font.FontSize = 11;
                totalCostRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(255, 205, 210);
                row++;

                // ─── سطر فارغ ───
                row++;

                // ─── إجمالي الإيرادات ───
                ws.Cell(row, 1).Value = "💵 إجمالي الإيرادات";
                ws.Cell(row, 2).Value = package.SellingPrice;
                ws.Cell(row, 3).Value = package.TotalRevenue;
                ws.Cell(row, 4).Value = "";
                ws.Cell(row, 5).Value = $"{persons} فرد × {package.SellingPrice:N2}";
                var totalRevRange = ws.Range(row, 1, row, 5);
                totalRevRange.Style.Font.Bold = true;
                totalRevRange.Style.Font.FontSize = 11;
                totalRevRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(200, 230, 201);
                row++;

                // ─── صافي الربح ───
                ws.Cell(row, 1).Value = "💎 صافي الربح";
                ws.Cell(row, 2).Value = package.NetProfitPerPerson;
                ws.Cell(row, 3).Value = package.NetProfit;
                ws.Cell(row, 4).Value = package.ProfitMargin;
                ws.Cell(row, 5).Value = package.NetProfit >= 0 ? "✅ ربح" : "❌ خسارة";
                var profitRange = ws.Range(row, 1, row, 5);
                profitRange.Style.Font.Bold = true;
                profitRange.Style.Font.FontSize = 13;
                profitRange.Style.Fill.BackgroundColor = package.NetProfit >= 0
                    ? ClosedXML.Excel.XLColor.FromArgb(46, 204, 113)
                    : ClosedXML.Excel.XLColor.FromArgb(231, 76, 60);
                profitRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                row++;

                // ─── سطر فارغ ───
                row++;

                // ─── معلومات إضافية ───
                ws.Cell(row, 1).Value = "📝 معلومات إضافية";
                var infoHdrRange = ws.Range(row, 1, row, 5);
                infoHdrRange.Style.Font.Bold = true;
                infoHdrRange.Style.Font.FontSize = 12;
                infoHdrRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(52, 73, 94);
                infoHdrRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                row++;

                ws.Cell(row, 1).Value = "  🏷️ نوع الغرفة";
                ws.Cell(row, 5).Value = package.GetRoomTypeDisplay();
                row++;

                ws.Cell(row, 1).Value = "  🏨 إجمالي الليالي";
                ws.Cell(row, 5).Value = $"{package.TotalNights} ليلة (مكة: {package.MakkahNights} + المدينة: {package.MadinahNights})";
                row++;

                ws.Cell(row, 1).Value = "  🚌 وسيلة السفر";
                ws.Cell(row, 5).Value = package.TransportMethod;
                row++;

                ws.Cell(row, 1).Value = "  💱 سعر صرف الريال";
                ws.Cell(row, 5).Value = $"{package.SARExchangeRate:N2} ج.م";
                row++;

                ws.Cell(row, 1).Value = "  📋 الحالة";
                ws.Cell(row, 5).Value = package.GetStatusDisplay();
                row++;

                if (!string.IsNullOrEmpty(package.Notes))
                {
                    ws.Cell(row, 1).Value = "  📌 ملاحظات";
                    ws.Cell(row, 5).Value = package.Notes;
                    row++;
                }

                // ─── تنسيق الأعمدة ───
                ws.Column(1).Width = 35;
                ws.Column(2).Width = 22;
                ws.Column(3).Width = 22;
                ws.Column(4).Width = 15;
                ws.Column(5).Width = 45;

                ws.Column(2).Style.NumberFormat.Format = "#,##0.00";
                ws.Column(3).Style.NumberFormat.Format = "#,##0.00";
                ws.Column(4).Style.NumberFormat.Format = "0.00";

                ws.Column(1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
                ws.Column(2).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                ws.Column(3).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                ws.Column(4).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                ws.Column(5).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;

                // تنسيق معلومات الحزمة في الأعلى
                ws.Range(2, 1, 4, 5).Style.Font.Bold = true;
                ws.Range(2, 1, 4, 5).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(235, 245, 255);
            }

            workbook.SaveAs(saveDialog.FileName);

            MessageBox.Show($"✅ تم التصدير بنجاح!\n\nعدد الحزم: {packagesToExport.Count}\nالملف: {saveDialog.FileName}",
                "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
    
    private void ExportDetailsToExcel_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_selectedPackage == null)
            {
                MessageBox.Show("لا توجد حزمة محددة!", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (_detailGrid.Rows.Count == 0)
            {
                MessageBox.Show("لا توجد تفاصيل للتصدير!", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            using var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "حفظ تفاصيل حسابات الحزمة",
                FileName = $"تفاصيل_{_selectedPackage.PackageNumber}_{DateTime.Now:yyyy-MM-dd_HHmm}.xlsx"
            };
            
            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;
            
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("تفاصيل الحسابات");
            
            // Title
            worksheet.Cell(1, 1).Value = "تفاصيل حسابات حزمة العمرة";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 18;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(41, 128, 185);
            worksheet.Cell(1, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            worksheet.Range(1, 1, 1, 5).Merge();
            worksheet.Range(1, 1, 1, 5).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            
            // Package Info
            worksheet.Cell(2, 1).Value = $"رقم الحزمة: {_selectedPackage.PackageNumber}";
            worksheet.Range(2, 1, 2, 2).Merge();
            worksheet.Cell(2, 3).Value = $"اسم الرحلة: {_selectedPackage.TripName}";
            worksheet.Range(2, 3, 2, 5).Merge();
            
            worksheet.Cell(3, 1).Value = $"عدد الأفراد: {_selectedPackage.NumberOfPersons}";
            worksheet.Range(3, 1, 3, 2).Merge();
            worksheet.Cell(3, 3).Value = $"التاريخ: {_selectedPackage.Date:yyyy-MM-dd}";
            worksheet.Range(3, 3, 3, 5).Merge();
            
            // Headers
            int headerRow = 5;
            string[] headers = { "البند", "تكلفة الفرد (ج.م)", "الإجمالي (ج.م)", "النسبة %", "ملاحظات" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(headerRow, i + 1).Value = headers[i];
            }
            
            var headerRange = worksheet.Range(headerRow, 1, headerRow, 5);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(52, 73, 94);
            headerRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            
            // Data
            int currentRow = headerRow + 1;
            
            foreach (DataGridViewRow row in _detailGrid.Rows)
            {
                // تخطي الصفوف الفارغة
                if (row.Cells["DetailItem"].Value == null || 
                    string.IsNullOrWhiteSpace(row.Cells["DetailItem"].Value.ToString()))
                    continue;
                
                string itemName = row.Cells["DetailItem"].Value?.ToString() ?? "";
                
                // كتابة البيانات
                worksheet.Cell(currentRow, 1).Value = itemName;
                
                // القيمة لكل فرد
                var perPersonValue = row.Cells["DetailValuePerPerson"].Value;
                if (perPersonValue != null && !string.IsNullOrEmpty(perPersonValue.ToString()))
                {
                    if (decimal.TryParse(perPersonValue.ToString(), out decimal perPerson))
                        worksheet.Cell(currentRow, 2).Value = perPerson;
                    else
                        worksheet.Cell(currentRow, 2).Value = perPersonValue.ToString();
                }
                
                // الإجمالي
                var totalValue = row.Cells["DetailTotal"].Value;
                if (totalValue != null && !string.IsNullOrEmpty(totalValue.ToString()))
                {
                    if (decimal.TryParse(totalValue.ToString(), out decimal total))
                        worksheet.Cell(currentRow, 3).Value = total;
                    else
                        worksheet.Cell(currentRow, 3).Value = totalValue.ToString();
                }
                
                // النسبة
                var percentValue = row.Cells["DetailPercentage"].Value;
                if (percentValue != null && !string.IsNullOrEmpty(percentValue.ToString()))
                {
                    if (decimal.TryParse(percentValue.ToString(), out decimal percent))
                        worksheet.Cell(currentRow, 4).Value = percent;
                    else
                        worksheet.Cell(currentRow, 4).Value = percentValue.ToString();
                }
                
                // الملاحظات
                worksheet.Cell(currentRow, 5).Value = row.Cells["DetailNotes"].Value?.ToString() ?? "";
                
                // تنسيق الصفوف الخاصة (العناوين، الإجماليات)
                if (itemName.Contains("💰 الإيرادات") || itemName.Contains("💸 التكاليف") || 
                    itemName.Contains("📝 معلومات إضافية"))
                {
                    var headerRowRange = worksheet.Range(currentRow, 1, currentRow, 5);
                    headerRowRange.Style.Font.Bold = true;
                    headerRowRange.Style.Font.FontSize = 12;
                    
                    if (itemName.Contains("💰 الإيرادات"))
                        headerRowRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(46, 204, 113);
                    else if (itemName.Contains("💸 التكاليف"))
                        headerRowRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(231, 76, 60);
                    else
                        headerRowRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(52, 73, 94);
                    
                    headerRowRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                }
                else if (itemName.Contains("📊 إجمالي") || itemName.Contains("💵 إجمالي") || 
                         itemName.Contains("💎 صافي"))
                {
                    var totalRowRange = worksheet.Range(currentRow, 1, currentRow, 5);
                    totalRowRange.Style.Font.Bold = true;
                    totalRowRange.Style.Font.FontSize = 11;
                    
                    if (itemName.Contains("💎 صافي"))
                    {
                        var profit = _selectedPackage.NetProfit;
                        totalRowRange.Style.Fill.BackgroundColor = profit >= 0 ? 
                            ClosedXML.Excel.XLColor.FromArgb(46, 204, 113) : 
                            ClosedXML.Excel.XLColor.FromArgb(231, 76, 60);
                        totalRowRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                    }
                    else
                    {
                        totalRowRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
                    }
                }
                
                currentRow++;
            }
            
            // Formatting
            worksheet.Column(1).Width = 35;
            worksheet.Column(2).Width = 20;
            worksheet.Column(3).Width = 20;
            worksheet.Column(4).Width = 15;
            worksheet.Column(5).Width = 40;
            
            worksheet.Column(2).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Column(3).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Column(4).Style.NumberFormat.Format = "0.00";
            
            // تنسيق محاذاة
            worksheet.Column(1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
            worksheet.Column(2).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            worksheet.Column(3).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            worksheet.Column(4).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            worksheet.Column(5).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
            
            workbook.SaveAs(saveDialog.FileName);
            
            MessageBox.Show($"✅ تم تصدير التفاصيل بنجاح!\n\nالملف: {saveDialog.FileName}", 
                "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = saveDialog.FileName,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تصدير التفاصيل: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
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
