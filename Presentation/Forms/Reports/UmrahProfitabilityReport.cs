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
            using var brush = new LinearGradientBrush(
                headerPanel.ClientRectangle,
                Color.FromArgb(52, 152, 219),
                Color.FromArgb(41, 128, 185),
                LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(brush, headerPanel.ClientRectangle);
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
        // Data Grid Panel
        // ═══════════════════════════════════════════
        Panel gridPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 10, 20, 20),
            BackColor = Color.Transparent
        };
        
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
        
        gridPanel.Controls.Add(_profitGrid);
        
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
    
    private void ExportToExcel_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_profitGrid.Rows.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات للتصدير!", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            using var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "حفظ تقرير ربحية العمرة",
                FileName = $"ربحية_العمرة_{DateTime.Now:yyyy-MM-dd_HHmm}.xlsx"
            };
            
            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;
            
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("ربحية العمرة");
            
            // Title
            worksheet.Cell(1, 1).Value = "تقرير ربحية حزم العمرة";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 18;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(52, 152, 219);
            worksheet.Cell(1, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            worksheet.Range(1, 1, 1, 9).Merge();
            worksheet.Range(1, 1, 1, 9).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            
            // Period
            string period = _showAllCheckBox.Checked ? 
                "جميع الفترات" : 
                $"من {_startDatePicker.Value:yyyy-MM-dd} إلى {_endDatePicker.Value:yyyy-MM-dd}";
            worksheet.Cell(2, 1).Value = $"الفترة: {period}";
            worksheet.Range(2, 1, 2, 9).Merge();
            worksheet.Range(2, 1, 2, 9).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            
            // Headers
            int headerRow = 4;
            string[] headers = { "رقم الحزمة", "التاريخ", "اسم الرحلة", "عدد الأفراد", 
                               "الإيرادات", "التكاليف", "صافي الربح", "هامش الربح %", "الحالة" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(headerRow, i + 1).Value = headers[i];
            }
            
            var headerRange = worksheet.Range(headerRow, 1, headerRow, 9);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(44, 62, 80);
            headerRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            
            // Data
            int currentRow = headerRow + 1;
            decimal totalRevenue = 0, totalCosts = 0, totalProfit = 0;
            
            foreach (DataGridViewRow row in _profitGrid.Rows)
            {
                worksheet.Cell(currentRow, 1).Value = row.Cells["PackageNumber"].Value?.ToString() ?? "";
                worksheet.Cell(currentRow, 2).Value = row.Cells["Date"].Value?.ToString() ?? "";
                worksheet.Cell(currentRow, 3).Value = row.Cells["TripName"].Value?.ToString() ?? "";
                worksheet.Cell(currentRow, 4).Value = Convert.ToInt32(row.Cells["Persons"].Value ?? 0);
                worksheet.Cell(currentRow, 5).Value = Convert.ToDecimal(row.Cells["Revenue"].Value ?? 0);
                worksheet.Cell(currentRow, 6).Value = Convert.ToDecimal(row.Cells["Costs"].Value ?? 0);
                worksheet.Cell(currentRow, 7).Value = Convert.ToDecimal(row.Cells["Profit"].Value ?? 0);
                worksheet.Cell(currentRow, 8).Value = Convert.ToDecimal(row.Cells["Margin"].Value ?? 0);
                worksheet.Cell(currentRow, 9).Value = row.Cells["Status"].Value?.ToString() ?? "";
                
                if (decimal.TryParse(row.Cells["Revenue"].Value?.ToString(), out decimal rev))
                    totalRevenue += rev;
                if (decimal.TryParse(row.Cells["Costs"].Value?.ToString(), out decimal cost))
                    totalCosts += cost;
                if (decimal.TryParse(row.Cells["Profit"].Value?.ToString(), out decimal prof))
                    totalProfit += prof;
                
                currentRow++;
            }
            
            // Totals
            int totalRow = currentRow + 1;
            worksheet.Cell(totalRow, 1).Value = "الإجماليات";
            worksheet.Cell(totalRow, 1).Style.Font.Bold = true;
            worksheet.Cell(totalRow, 5).Value = totalRevenue;
            worksheet.Cell(totalRow, 6).Value = totalCosts;
            worksheet.Cell(totalRow, 7).Value = totalProfit;
            
            var totalRange = worksheet.Range(totalRow, 1, totalRow, 9);
            totalRange.Style.Font.Bold = true;
            totalRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
            
            // Formatting
            worksheet.Column(5).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Column(6).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Column(7).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Column(8).Style.NumberFormat.Format = "0.00";
            
            worksheet.Columns().AdjustToContents();
            
            workbook.SaveAs(saveDialog.FileName);
            
            MessageBox.Show($"✅ تم التصدير بنجاح!\n\nالملف: {saveDialog.FileName}", 
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
