using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using System.Drawing.Printing;

namespace GraceWay.AccountingSystem.Presentation.Forms;

/// <summary>
/// عرض تفاصيل رحلة كاملة مع جميع البيانات
/// </summary>
public partial class TripDetailsForm : Form
{
    private readonly ITripService _tripService = null!;
    private readonly int _tripId;
    private readonly int _currentUserId;
    private Trip _trip = null!;
    
    // Controls
    private Panel _headerPanel = null!;
    private Label _tripNameLabel = null!;
    private Label _tripNumberLabel = null!;
    private TabControl _tabControl = null!;
    private Button _editButton = null!;
    private Button _closeButton = null!;
    
    // Constructor with tripId
    public TripDetailsForm(ITripService tripService, int tripId, int currentUserId)
    {
        _tripService = tripService;
        _tripId = tripId;
        _currentUserId = currentUserId;
        
        InitializeComponent();
        SetupForm();
        InitializeCustomControls();
        _ = LoadDataAsync();
    }
    
    // Constructor with Trip object
    public TripDetailsForm(ITripService tripService, Trip trip, int currentUserId)
    {
        _tripService = tripService;
        _trip = trip;
        _tripId = trip.TripId;
        _currentUserId = currentUserId;
        
        InitializeComponent();
        SetupForm();
        InitializeCustomControls();
        LoadTripData();
    }
    
    private void SetupForm()
    {
        this.Text = "تفاصيل الرحلة";
        this.Size = new Size(1200, 850);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
        this.WindowState = FormWindowState.Maximized;
    }
    
    private void InitializeCustomControls()
    {
        // Header
        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 120,
            BackColor = ColorScheme.Primary,
            Padding = new Padding(20)
        };
        
        _tripNameLabel = new Label
        {
            Text = "جاري التحميل...",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 20)
        };
        _headerPanel.Controls.Add(_tripNameLabel);
        
        _tripNumberLabel = new Label
        {
            Font = new Font("Cairo", 11F),
            ForeColor = Color.FromArgb(236, 240, 241),
            AutoSize = true,
            Location = new Point(20, 60)
        };
        _headerPanel.Controls.Add(_tripNumberLabel);
        
        _editButton = new Button
        {
            Text = "✏️ تعديل",
            Size = new Size(120, 40),
            Location = new Point(840, 40),
            BackColor = ColorScheme.Warning,
            ForeColor = Color.White,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _editButton.FlatAppearance.BorderSize = 0;
        _editButton.Click += EditButton_Click;
        _headerPanel.Controls.Add(_editButton);
        
        // TabControl
        _tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Cairo", 10F)
        };
        
        // التبويبات
        _tabControl.TabPages.Add("📋 معلومات", "info");
        _tabControl.TabPages.Add("💰 مالية", "financial");
        _tabControl.TabPages.Add("📅 البرنامج", "program");
        _tabControl.TabPages.Add("🚗 النقل", "transport");
        _tabControl.TabPages.Add("🏨 الإقامة", "accommodation");
        _tabControl.TabPages.Add("👤 المرشد", "guide");
        _tabControl.TabPages.Add("🎯 رحلات اختيارية", "optional");
        _tabControl.TabPages.Add("💸 المصاريف", "expenses");
        
        // Footer
        Panel footerPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 70,
            BackColor = Color.FromArgb(245, 245, 245),
            Padding = new Padding(20)
        };
        
        _closeButton = new Button
        {
            Text = "إغلاق",
            Size = new Size(120, 40),
            Location = new Point(20, 15),
            BackColor = Color.FromArgb(149, 165, 166),
            ForeColor = Color.White,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _closeButton.FlatAppearance.BorderSize = 0;
        _closeButton.Click += (s, e) => this.Close();
        footerPanel.Controls.Add(_closeButton);
        
        this.Controls.Add(_tabControl);
        this.Controls.Add(footerPanel);
        this.Controls.Add(_headerPanel);
    }
    
    private async Task LoadDataAsync()
    {
        try
        {
            this.Cursor = Cursors.WaitCursor;
            
            _trip = await _tripService.GetTripByIdAsync(_tripId, includeDetails: true)
                ?? throw new Exception("الرحلة غير موجودة");
            
            LoadTripData();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    
    private void LoadTripData()
    {
        _tripNameLabel.Text = $"🌍 {_trip.TripName}";
        _tripNumberLabel.Text = $"رقم الرحلة: {_trip.TripNumber} | " +
            $"النوع: {GetTripTypeText(_trip.TripType)} | " +
            $"الحالة: {GetStatusText(_trip.Status)}";
        
        // ✅ تعطيل زر التعديل إذا كانت الرحلة مقفولة
        if (_trip.IsLockedForTrips)
        {
            _editButton.Enabled = false;
            _editButton.BackColor = Color.Gray;
            _editButton.Text = "🔒 مقفولة";
            _editButton.Cursor = Cursors.No;
        }
        else
        {
            _editButton.Enabled = true;
            _editButton.BackColor = ColorScheme.Warning;
            _editButton.Text = "✏️ تعديل";
            _editButton.Cursor = Cursors.Hand;
        }
        
        BuildInfoTab();
        BuildFinancialTab();
        BuildProgramTab();
        BuildTransportTab();
        BuildAccommodationTab();
        BuildGuideTab();
        BuildOptionalToursTab();
        BuildExpensesTab();
    }
    
    private void BuildInfoTab()
    {
        var tab = _tabControl.TabPages[0];
        tab.BackColor = Color.White;
        tab.Padding = new Padding(30);
        tab.AutoScroll = true;
        
        int y = 20;
        
        // ═══════════════════════════════════════════════════════════
        // 📋 المعلومات الأساسية
        // ═══════════════════════════════════════════════════════════
        AddSectionHeader(tab, "📋 المعلومات الأساسية", ref y);
        AddInfoItem(tab, "رقم الرحلة:", _trip.TripNumber, ref y);
        AddInfoItem(tab, "اسم الرحلة:", _trip.TripName, ref y, bold: true);
        AddInfoItem(tab, "الوجهة:", _trip.Destination, ref y);
        AddInfoItem(tab, "النوع:", GetTripTypeText(_trip.TripType), ref y);
        AddInfoItem(tab, "الحالة:", GetStatusText(_trip.Status), ref y);
        
        if (!string.IsNullOrWhiteSpace(_trip.Description))
        {
            y += 10;
            var descLabel = new Label
            {
                Text = "📝 الوصف:",
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(30, y)
            };
            tab.Controls.Add(descLabel);
            y += 30;
            
            var descBox = new TextBox
            {
                Text = _trip.Description,
                Location = new Point(30, y),
                Size = new Size(850, 80),
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.White,
                Font = new Font("Cairo", 10F)
            };
            tab.Controls.Add(descBox);
            y += 100;
        }
        
        y += 20;
        
        // ═══════════════════════════════════════════════════════════
        // 📅 التواريخ والمدة
        // ═══════════════════════════════════════════════════════════
        AddSectionHeader(tab, "📅 التواريخ والمدة", ref y);
        AddInfoItem(tab, "تاريخ البدء:", _trip.StartDate.ToString("yyyy-MM-dd (dddd)"), ref y);
        AddInfoItem(tab, "تاريخ الانتهاء:", _trip.EndDate.ToString("yyyy-MM-dd (dddd)"), ref y);
        AddInfoItem(tab, "إجمالي المدة:", $"{(_trip.EndDate - _trip.StartDate).Days + 1} يوم", ref y, bold: true, color: ColorScheme.Primary);
        
        y += 20;
        
        // ═══════════════════════════════════════════════════════════
        // 👥 الطاقة والحجوزات
        // ═══════════════════════════════════════════════════════════
        AddSectionHeader(tab, "👥 الطاقة والحجوزات", ref y);
        AddInfoItem(tab, "الطاقة الاستيعابية:", $"{_trip.TotalCapacity} فرد", ref y, bold: true);
        AddInfoItem(tab, "المحجوز حالياً:", $"{_trip.BookedSeats} فرد", ref y);
        AddInfoItem(tab, "المتبقي:", $"{_trip.AvailableSeats} مقعد", ref y);
        
        decimal occupancyRate = _trip.TotalCapacity > 0 ? (_trip.BookedSeats * 100.0m / _trip.TotalCapacity) : 0;
        Color occupancyColor = occupancyRate >= 80 ? Color.FromArgb(39, 174, 96) : 
                               occupancyRate >= 50 ? Color.FromArgb(243, 156, 18) : 
                               Color.FromArgb(231, 76, 60);
        AddInfoItem(tab, "نسبة الإشغال:", $"{occupancyRate:F1}%", ref y, bold: true, color: occupancyColor);
        
        y += 20;
        
        // ═══════════════════════════════════════════════════════════
        // 📊 إحصائيات المكونات
        // ═══════════════════════════════════════════════════════════
        AddSectionHeader(tab, "📊 إحصائيات مكونات الرحلة", ref y);
        AddInfoItem(tab, "أيام البرنامج:", $"{_trip.Programs.Count} يوم", ref y);
        AddInfoItem(tab, "وسائل النقل:", $"{_trip.Transportation.Count} مركبة/رحلة", ref y);
        AddInfoItem(tab, "أماكن الإقامة:", $"{_trip.Accommodations.Count} فندق/مكان", ref y);
        AddInfoItem(tab, "المرشدين:", $"{_trip.Guides.Count} مرشد", ref y);
        AddInfoItem(tab, "المصاريف الأخرى:", $"{_trip.Expenses.Count} مصروف", ref y);
        AddInfoItem(tab, "الرحلات الاختيارية:", $"{_trip.OptionalTours.Count} رحلة", ref y);
        
        y += 20;
        
        // ═══════════════════════════════════════════════════════════
        // ℹ️ معلومات النظام
        // ═══════════════════════════════════════════════════════════
        AddSectionHeader(tab, "ℹ️ معلومات النظام", ref y);
        AddInfoItem(tab, "تاريخ الإنشاء:", _trip.CreatedAt.ToString("yyyy-MM-dd HH:mm"), ref y);
        AddInfoItem(tab, "آخر تحديث:", _trip.UpdatedAt.ToString("yyyy-MM-dd HH:mm"), ref y);
        AddInfoItem(tab, "منشورة:", _trip.IsPublished ? "نعم ✅" : "لا ❌", ref y);
        AddInfoItem(tab, "نشطة:", _trip.IsActive ? "نعم ✅" : "لا ❌", ref y);
    }
    
    private void BuildFinancialTab()
    {
        var tab = _tabControl.TabPages[1];
        tab.BackColor = Color.White;
        tab.Padding = new Padding(20);
        tab.AutoScroll = true;
        tab.Controls.Clear();
        
        // Panel رئيسي للتقرير المالي
        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.White
        };
        
        int y = 10;
        
        // Header مع زر الطباعة
        var headerPanel = new Panel
        {
            Location = new Point(10, y),
            Size = new Size(1100, 60),
            BackColor = ColorScheme.Primary
        };
        
        var titleLabel = new Label
        {
            Text = "📊 التقرير المالي الشامل",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(300, 15),
            AutoSize = true
        };
        
        var printButton = new Button
        {
            Text = "🖨️ طباعة",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Size = new Size(120, 40),
            Location = new Point(20, 10),
            BackColor = Color.White,
            ForeColor = ColorScheme.Primary,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        printButton.Click += (s, e) => PrintFinancialReport();
        
        var exportButton = new Button
        {
            Text = "📊 تصدير Excel",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Size = new Size(140, 40),
            Location = new Point(150, 10),
            BackColor = Color.White,
            ForeColor = ColorScheme.Success,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        exportButton.Click += (s, e) => ExportFinancialReportToExcel();
        
        headerPanel.Controls.Add(titleLabel);
        headerPanel.Controls.Add(printButton);
        headerPanel.Controls.Add(exportButton);
        mainPanel.Controls.Add(headerPanel);
        y += 70;
        
        // ═══════════════════════════════════════════════════════════
        // 1️⃣ ملخص الإيرادات
        // ═══════════════════════════════════════════════════════════
        AddFinancialSectionHeader(mainPanel, "💵 ملخص الإيرادات", ref y);
        AddFinancialGrid(mainPanel, CreateRevenueGrid(), ref y, 180);
        
        // ═══════════════════════════════════════════════════════════
        // 2️⃣ البرنامج اليومي
        // ═══════════════════════════════════════════════════════════
        if (_trip.Programs.Any())
        {
            AddFinancialSectionHeader(mainPanel, "📅 البرنامج اليومي", ref y);
            AddFinancialGrid(mainPanel, CreateProgramGrid(), ref y, 250);
        }
        
        // ═══════════════════════════════════════════════════════════
        // 3️⃣ النقل والمواصلات
        // ═══════════════════════════════════════════════════════════
        if (_trip.Transportation.Any())
        {
            AddFinancialSectionHeader(mainPanel, "🚗 النقل والمواصلات", ref y);
            AddFinancialGrid(mainPanel, CreateTransportationGrid(), ref y, 250);
        }
        
        // ═══════════════════════════════════════════════════════════
        // 4️⃣ الإقامة والفنادق
        // ═══════════════════════════════════════════════════════════
        if (_trip.Accommodations.Any())
        {
            AddFinancialSectionHeader(mainPanel, "🏨 الإقامة والفنادق", ref y);
            AddFinancialGrid(mainPanel, CreateAccommodationGrid(), ref y, 250);
        }
        
        // ═══════════════════════════════════════════════════════════
        // 5️⃣ المرشدين
        // ═══════════════════════════════════════════════════════════
        AddFinancialSectionHeader(mainPanel, "👤 ملخص تكلفة المرشدين", ref y);
        AddFinancialGrid(mainPanel, CreateGuidesGrid(), ref y, 150);
        
        // ═══════════════════════════════════════════════════════════
        // 6️⃣ الرحلات الاختيارية
        // ═══════════════════════════════════════════════════════════
        if (_trip.OptionalTours.Any())
        {
            AddFinancialSectionHeader(mainPanel, "🎯 الرحلات الاختيارية", ref y);
            AddFinancialGrid(mainPanel, CreateOptionalToursGrid(), ref y, 250);
        }
        
        // ═══════════════════════════════════════════════════════════
        // 7️⃣ المصاريف الأخرى
        // ═══════════════════════════════════════════════════════════
        if (_trip.Expenses.Any())
        {
            AddFinancialSectionHeader(mainPanel, "💸 المصاريف الأخرى", ref y);
            AddFinancialGrid(mainPanel, CreateExpensesGrid(), ref y, 200);
        }
        
        // ═══════════════════════════════════════════════════════════
        // 8️⃣ الملخص النهائي
        // ═══════════════════════════════════════════════════════════
        AddFinancialSectionHeader(mainPanel, "📈 الملخص النهائي", ref y);
        AddFinancialGrid(mainPanel, CreateFinalSummaryGrid(), ref y, 250);
        
        tab.Controls.Add(mainPanel);
    }
    
    private void BuildProgramTab()
    {
        var tab = _tabControl.TabPages[2];
        tab.BackColor = Color.White;
        
        if (!_trip.Programs.Any())
        {
            AddEmptyMessage(tab, "📅 لا يوجد برنامج يومي مسجل");
            return;
        }
        
        int yOffset = 0;
        
        // ✅ تحذير إذا كان هناك أيام بدون سعر مرشد
        var daysWithoutGuide = _trip.Programs.Where(p => p.GuideCost == 0).ToList();
        if (daysWithoutGuide.Any())
        {
            var warningPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(255, 243, 205),  // أصفر فاتح
                BorderStyle = BorderStyle.FixedSingle
            };
            
            var warningLabel = new Label
            {
                Text = $"⚠️ تحذير: يوجد {daysWithoutGuide.Count} يوم بدون سعر مرشد مسجل! يرجى مراجعة البيانات وتحديث سعر المرشد.",
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(230, 81, 0),  // برتقالي غامق
                AutoSize = false,
                Size = new Size(1100, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 10)
            };
            warningPanel.Controls.Add(warningLabel);
            tab.Controls.Add(warningPanel);
            yOffset = 60;
        }
        
        // Panel للملخص
        var summaryPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.FromArgb(248, 249, 250),
            BorderStyle = BorderStyle.FixedSingle,
            Top = yOffset
        };
        
        var summaryLabel = new Label
        {
            Text = $"📅 إجمالي الأيام: {_trip.Programs.Count} | كبار: {_trip.Programs.Count(p => p.BookingType == "Adult")} | أطفال: {_trip.Programs.Count(p => p.BookingType == "Child")}",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = false,
            Size = new Size(1100, 40),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(10, 10)
        };
        summaryPanel.Controls.Add(summaryLabel);
        tab.Controls.Add(summaryPanel);
        
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            Font = new Font("Cairo", 9F),
            AllowUserToAddRows = false,
            ReadOnly = true,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            AllowUserToResizeRows = false
        };
        
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DayDate", HeaderText = "التاريخ", Width = 100 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DayNumber", HeaderText = "اليوم", Width = 60 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "BookingType", HeaderText = "النوع", Width = 80 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Visits", HeaderText = "المزارات", Width = 250 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "VisitsCost", HeaderText = "سعر المزارات (جنيه)", Width = 100 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ParticipantsCount", HeaderText = "عدد الأفراد", Width = 90 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GuideCost", HeaderText = "سعر المرشد (جنيه)", Width = 110 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalCost", HeaderText = "التكلفة الإجمالية (جنيه)", Width = 120 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Notes", HeaderText = "ملاحظات", Width = 150 });
        
        // Sorting: BookingType, then DayNumber
        foreach (var program in _trip.Programs.OrderBy(p => p.BookingType).ThenBy(p => p.DayNumber))
        {
            decimal totalCost = (program.VisitsCost * program.ParticipantsCount) + program.GuideCost;
            
            // تنبيه إذا كان سعر المرشد صفر
            string guideCostDisplay = $"{program.GuideCost:N2}";
            if (program.GuideCost == 0)
            {
                guideCostDisplay = "⚠️ 0.00 (غير مسجل)";
            }
            
            grid.Rows.Add(
                program.DayDate.ToString("yyyy-MM-dd"),
                program.DayNumber,
                program.BookingType,
                program.Visits,
                $"{program.VisitsCost:N2}",
                program.ParticipantsCount,
                guideCostDisplay,
                $"{totalCost:N2}",
                program.Notes ?? ""
            );
        }
        
        // Color coding by BookingType and highlight zero guide cost
        foreach (DataGridViewRow row in grid.Rows)
        {
            string bookingType = row.Cells["BookingType"].Value?.ToString() ?? "";
            string guideCostCell = row.Cells["GuideCost"].Value?.ToString() ?? "";
            
            // تلوين حسب نوع الحجز
            if (bookingType == "Adult")
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(232, 245, 233);
            }
            else if (bookingType == "Child")
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(227, 242, 253);
            }
            
            // تحذير: تلوين خلية المرشد باللون الأحمر إذا كانت صفر
            if (guideCostCell.Contains("⚠️") || guideCostCell.StartsWith("0.00"))
            {
                row.Cells["GuideCost"].Style.BackColor = Color.FromArgb(255, 205, 210);  // أحمر فاتح
                row.Cells["GuideCost"].Style.ForeColor = Color.FromArgb(183, 28, 28);    // أحمر غامق
            }
        }
        
        tab.Controls.Add(grid);
    }
    
    private void BuildTransportTab()
    {
        var tab = _tabControl.TabPages[3];
        tab.BackColor = Color.White;
        
        if (!_trip.Transportation.Any())
        {
            AddEmptyMessage(tab, "🚗 لا توجد بيانات نقل مسجلة");
            return;
        }
        
        // Panel للملخص
        var summaryPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.FromArgb(248, 249, 250),
            BorderStyle = BorderStyle.FixedSingle
        };
        
        decimal totalTransportCost = _trip.Transportation.Sum(t => 
            (t.CostPerVehicle * t.NumberOfVehicles) + t.TourLeaderTip + t.DriverTip);
        
        var summaryLabel = new Label
        {
            Text = $"🚗 عدد المركبات: {_trip.Transportation.Count} | إجمالي التكلفة: {totalTransportCost:N2} جنيه",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = false,
            Size = new Size(1100, 40),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(10, 10)
        };
        summaryPanel.Controls.Add(summaryLabel);
        tab.Controls.Add(summaryPanel);
        
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            Font = new Font("Cairo", 9F),
            AllowUserToAddRows = false,
            ReadOnly = true,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            AllowUserToResizeRows = false
        };
        
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "التاريخ", Width = 100 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "النوع", Width = 90 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Route", HeaderText = "المسار", Width = 150 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Model", HeaderText = "الموديل", Width = 100 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Seats", HeaderText = "المقاعد", Width = 70 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Participants", HeaderText = "عدد الأفراد", Width = 90 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CostPer", HeaderText = "التكلفة الأساسية (جنيه)", Width = 130 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TourLeaderTip", HeaderText = "إكرامية التور ليدر (جنيه)", Width = 140 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DriverTip", HeaderText = "إكرامية السواق (جنيه)", Width = 130 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "الإجمالي (جنيه)", Width = 120 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Supplier", HeaderText = "المورد", Width = 120 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DriverPhone", HeaderText = "هاتف السائق", Width = 110 });
        
        foreach (var transport in _trip.Transportation)
        {
            decimal total = (transport.CostPerVehicle * transport.NumberOfVehicles) + 
                           transport.TourLeaderTip + transport.DriverTip;
            
            string transportDate = "-";
            if (transport.TransportDate.HasValue)
            {
                transportDate = transport.TransportDate.Value.ToString("yyyy-MM-dd");
            }
            
            grid.Rows.Add(
                transportDate,
                GetTransportTypeText(transport.Type),
                transport.Route ?? "-",
                transport.DriverName ?? "-",
                transport.SeatsPerVehicle,
                transport.ParticipantsCount,
                $"{transport.CostPerVehicle:N2}",
                $"{transport.TourLeaderTip:N2}",
                $"{transport.DriverTip:N2}",
                $"{total:N2}",
                transport.Supplier?.SupplierName ?? "-",
                transport.DriverName ?? "-"
            );
        }
        
        tab.Controls.Add(grid);
    }
    
    private void BuildAccommodationTab()
    {
        var tab = _tabControl.TabPages[4];
        tab.BackColor = Color.White;
        tab.Controls.Clear(); // ✅ تنظيف التاب أولاً
        
        if (!_trip.Accommodations.Any())
        {
            AddEmptyMessage(tab, "🏨 لا توجد بيانات إقامة مسجلة");
            return;
        }
        
        // Panel للملخص
        var summaryPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 80,
            BackColor = Color.FromArgb(248, 249, 250),
            BorderStyle = BorderStyle.FixedSingle
        };
        
        decimal totalAccommodationCost = _trip.Accommodations.Sum(a => a.TotalCost);
        decimal totalGuideCost = _trip.Accommodations.Sum(a => a.GuideCost);
        int totalNights = _trip.Accommodations.Sum(a => a.NumberOfNights);
        int totalRooms = _trip.Accommodations.Sum(a => a.NumberOfRooms);
        
        var summaryLabel1 = new Label
        {
            Text = $"🏨 عدد الفنادق: {_trip.Accommodations.Count} | 🛏️ إجمالي الغرف: {totalRooms} | 🌙 إجمالي الليالي: {totalNights}",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = false,
            Size = new Size(1100, 30),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(10, 5)
        };
        
        var summaryLabel2 = new Label
        {
            Text = $"💰 تكلفة الإقامة: {totalAccommodationCost:N2} جنيه | 👨‍🏫 تكلفة المرشدين: {totalGuideCost:N2} جنيه",
            Font = new Font("Cairo", 10F),
            ForeColor = Color.FromArgb(80, 80, 80),
            AutoSize = false,
            Size = new Size(1100, 30),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(10, 40)
        };
        
        summaryPanel.Controls.Add(summaryLabel1);
        summaryPanel.Controls.Add(summaryLabel2);
        tab.Controls.Add(summaryPanel);
        
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            Font = new Font("Cairo", 9F),
            AllowUserToAddRows = false,
            ReadOnly = true,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            AllowUserToResizeRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            ScrollBars = ScrollBars.Both, // ✅ إضافة scroll bars
            RowTemplate = { Height = 35 }  // ✅ ارتفاع ثابت للصفوف
        };
        
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "النوع", FillWeight = 80 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "اسم الفندق", FillWeight = 150 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rating", HeaderText = "التصنيف", FillWeight = 80 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "RoomType", HeaderText = "نوع الغرفة", FillWeight = 90 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rooms", HeaderText = "الغرف", FillWeight = 60 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nights", HeaderText = "الليالي", FillWeight = 60 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CheckIn", HeaderText = "تاريخ الدخول", FillWeight = 100 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CheckOut", HeaderText = "تاريخ الخروج", FillWeight = 100 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "MealPlan", HeaderText = "الوجبات", FillWeight = 70 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CostPer", HeaderText = "السعر/ليلة", FillWeight = 90 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GuideCost", HeaderText = "تكلفة المرشد", FillWeight = 100 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "الإجمالي", FillWeight = 110 });
        
        // تنسيق الهيدر
        grid.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Primary;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 10F, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
        grid.ColumnHeadersHeight = 40;
        grid.EnableHeadersVisualStyles = false;
        grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        
        int rowIndex = 0;
        foreach (var acc in _trip.Accommodations)
        {
            // التصنيف
            string rating = "-";
            if (acc.Rating.HasValue)
            {
                rating = new string('⭐', (int)acc.Rating.Value);
            }
            else if (acc.CruiseLevel.HasValue)
            {
                rating = $"مستوى {acc.CruiseLevel.Value}";
            }
            
            grid.Rows.Add(
                GetAccommodationTypeText(acc.Type),
                acc.HotelName,
                rating,
                GetRoomTypeText(acc.RoomType),
                acc.NumberOfRooms,
                acc.NumberOfNights,
                acc.CheckInDate.ToString("dd/MM/yyyy"),
                acc.CheckOutDate.ToString("dd/MM/yyyy"),
                acc.MealPlan ?? "-",
                $"{acc.CostPerRoomPerNight:N2}",
                $"{acc.GuideCost:N2}",
                $"{acc.TotalCost:N2}"
            );
            
            // تلوين الصفوف بالتبادل
            if (rowIndex % 2 == 0)
            {
                grid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
            }
            
            // محاذاة الأرقام إلى اليمين
            for (int i = 4; i <= 11; i++)
            {
                grid.Rows[rowIndex].Cells[i].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            
            rowIndex++;
        }
        
        // إضافة صف الإجمالي
        int totalRowIndex = grid.Rows.Add(
            "الإجمالي",
            "",
            "",
            "",
            totalRooms.ToString(),
            totalNights.ToString(),
            "",
            "",
            "",
            "",
            $"{totalGuideCost:N2}",
            $"{totalAccommodationCost:N2}"
        );
        
        grid.Rows[totalRowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 205);
        grid.Rows[totalRowIndex].DefaultCellStyle.Font = new Font("Cairo", 10F, FontStyle.Bold);
        grid.Rows[totalRowIndex].DefaultCellStyle.ForeColor = ColorScheme.Primary;
        
        // محاذاة الأرقام في صف الإجمالي
        for (int i = 4; i <= 11; i++)
        {
            grid.Rows[totalRowIndex].Cells[i].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        
        // ✅ التأكد من ترتيب الإضافة الصحيح
        tab.Controls.Add(summaryPanel); // أضف الملخص أولاً
        tab.Controls.Add(grid);         // ثم أضف الجدول
    }
    
    private void BuildGuideTab()
    {
        var tab = _tabControl.TabPages[5];
        tab.BackColor = Color.White;
        tab.Controls.Clear();
        tab.AutoScroll = true;
        
        // ═══════════════════════════════════════════════════════════
        // حساب تكلفة المرشدين من مصادر مختلفة
        // ═══════════════════════════════════════════════════════════
        decimal programGuideCost = _trip.Programs.Sum(p => p.GuideCost);
        decimal accommodationGuideCost = _trip.Accommodations.Sum(a => a.GuideCost);
        decimal totalGuideCost = programGuideCost + accommodationGuideCost;
        
        int y = 20;
        
        // ═══════════════════════════════════════════════════════════
        // Header - الملخص العام
        // ═══════════════════════════════════════════════════════════
        var headerPanel = new Panel
        {
            Location = new Point(20, y),
            Size = new Size(1100, 90),
            BackColor = ColorScheme.Primary,
            BorderStyle = BorderStyle.FixedSingle
        };
        
        var titleLabel = new Label
        {
            Text = "👨‍🏫 تكاليف المرشدين السياحيين",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(20, 10),
            AutoSize = true
        };
        headerPanel.Controls.Add(titleLabel);
        
        var totalLabel = new Label
        {
            Text = $"💰 إجمالي التكلفة: {totalGuideCost:N2} جنيه",
            Font = new Font("Cairo", 13F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(20, 50),
            AutoSize = true
        };
        headerPanel.Controls.Add(totalLabel);
        
        var breakdownLabel = new Label
        {
            Text = $"📅 البرنامج اليومي: {programGuideCost:N2} ج  |  🏨 الإقامة: {accommodationGuideCost:N2} ج",
            Font = new Font("Cairo", 10F),
            ForeColor = Color.FromArgb(236, 240, 241),
            Location = new Point(700, 50),
            AutoSize = true
        };
        headerPanel.Controls.Add(breakdownLabel);
        
        tab.Controls.Add(headerPanel);
        y += 110;
        
        // ═══════════════════════════════════════════════════════════
        // جدول 1: تكاليف المرشدين من البرنامج اليومي
        // ═══════════════════════════════════════════════════════════
        if (_trip.Programs.Any(p => p.GuideCost > 0))
        {
            var sectionLabel1 = new Label
            {
                Text = "📅 تكاليف المرشد في البرنامج اليومي والمزارات",
                Font = new Font("Cairo", 12F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                Location = new Point(20, y),
                AutoSize = true
            };
            tab.Controls.Add(sectionLabel1);
            y += 40;
            
            var grid1 = CreateProgramGuideGrid();
            grid1.Location = new Point(20, y);
            grid1.Size = new Size(1100, Math.Min(300, (grid1.Rows.Count + 2) * 35 + 45));
            tab.Controls.Add(grid1);
            y += grid1.Height + 30;
        }
        
        // ═══════════════════════════════════════════════════════════
        // جدول 2: تكاليف المرشدين من الإقامة
        // ═══════════════════════════════════════════════════════════
        if (_trip.Accommodations.Any(a => a.GuideCost > 0))
        {
            var sectionLabel2 = new Label
            {
                Text = "🏨 تكاليف المرشد في الإقامة والفنادق",
                Font = new Font("Cairo", 12F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                Location = new Point(20, y),
                AutoSize = true
            };
            tab.Controls.Add(sectionLabel2);
            y += 40;
            
            var grid2 = CreateAccommodationGuideGrid();
            grid2.Location = new Point(20, y);
            grid2.Size = new Size(1100, Math.Min(300, (grid2.Rows.Count + 2) * 35 + 45));
            tab.Controls.Add(grid2);
            y += grid2.Height + 30;
        }
        
        // ═══════════════════════════════════════════════════════════
        // رسالة إذا لم يكن هناك تكاليف مرشدين
        // ═══════════════════════════════════════════════════════════
        if (totalGuideCost == 0)
        {
            var emptyLabel = new Label
            {
                Text = "👤 لا توجد تكاليف مرشدين مسجلة في هذه الرحلة",
                Font = new Font("Cairo", 12F),
                ForeColor = Color.FromArgb(149, 165, 166),
                Location = new Point(400, 200),
                AutoSize = true
            };
            tab.Controls.Add(emptyLabel);
        }
    }
    
    private DataGridView CreateProgramGuideGrid()
    {
        var grid = new DataGridView
        {
            Font = new Font("Cairo", 9F),
            AllowUserToAddRows = false,
            ReadOnly = true,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            AllowUserToResizeRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            ScrollBars = ScrollBars.Both,
            RowTemplate = { Height = 35 }
        };
        
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DayNumber", HeaderText = "اليوم", FillWeight = 60 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DayDate", HeaderText = "التاريخ", FillWeight = 100 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DayTitle", HeaderText = "عنوان اليوم", FillWeight = 150 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Visits", HeaderText = "المزارات", FillWeight = 200 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GuideCost", HeaderText = "تكلفة المرشد (جنيه)", FillWeight = 120 });
        
        // تنسيق الهيدر
        grid.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Primary;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 10F, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        grid.ColumnHeadersHeight = 40;
        grid.EnableHeadersVisualStyles = false;
        
        int rowIndex = 0;
        foreach (var program in _trip.Programs.Where(p => p.GuideCost > 0).OrderBy(p => p.DayNumber))
        {
            grid.Rows.Add(
                $"اليوم {program.DayNumber}",
                program.DayDate.ToString("dd/MM/yyyy"),
                program.DayTitle,
                string.IsNullOrWhiteSpace(program.Visits) ? "-" : program.Visits,
                $"{program.GuideCost:N2}"
            );
            
            if (rowIndex % 2 == 0)
            {
                grid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
            }
            
            grid.Rows[rowIndex].Cells[4].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            rowIndex++;
        }
        
        // صف الإجمالي
        int totalRowIndex = grid.Rows.Add(
            "",
            "",
            "",
            "الإجمالي",
            $"{_trip.Programs.Sum(p => p.GuideCost):N2}"
        );
        
        grid.Rows[totalRowIndex].DefaultCellStyle.BackColor = Color.FromArgb(227, 242, 253);
        grid.Rows[totalRowIndex].DefaultCellStyle.Font = new Font("Cairo", 10F, FontStyle.Bold);
        grid.Rows[totalRowIndex].DefaultCellStyle.ForeColor = ColorScheme.Primary;
        grid.Rows[totalRowIndex].Cells[4].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        
        return grid;
    }
    
    private DataGridView CreateAccommodationGuideGrid()
    {
        var grid = new DataGridView
        {
            Font = new Font("Cairo", 9F),
            AllowUserToAddRows = false,
            ReadOnly = true,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            AllowUserToResizeRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            ScrollBars = ScrollBars.Both,
            RowTemplate = { Height = 35 }
        };
        
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "النوع", FillWeight = 80 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "HotelName", HeaderText = "اسم الفندق/الكروز", FillWeight = 150 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Location", HeaderText = "الموقع", FillWeight = 100 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CheckIn", HeaderText = "تاريخ الدخول", FillWeight = 100 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nights", HeaderText = "الليالي", FillWeight = 60 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GuideCost", HeaderText = "تكلفة المرشد (جنيه)", FillWeight = 120 });
        
        // تنسيق الهيدر
        grid.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Primary;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 10F, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        grid.ColumnHeadersHeight = 40;
        grid.EnableHeadersVisualStyles = false;
        
        int rowIndex = 0;
        foreach (var acc in _trip.Accommodations.Where(a => a.GuideCost > 0).OrderBy(a => a.CheckInDate))
        {
            grid.Rows.Add(
                GetAccommodationTypeText(acc.Type),
                acc.HotelName,
                acc.Location ?? "-",
                acc.CheckInDate.ToString("dd/MM/yyyy"),
                acc.NumberOfNights,
                $"{acc.GuideCost:N2}"
            );
            
            if (rowIndex % 2 == 0)
            {
                grid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
            }
            
            grid.Rows[rowIndex].Cells[5].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            rowIndex++;
        }
        
        // صف الإجمالي
        int totalRowIndex = grid.Rows.Add(
            "",
            "",
            "",
            "",
            "الإجمالي",
            $"{_trip.Accommodations.Sum(a => a.GuideCost):N2}"
        );
        
        grid.Rows[totalRowIndex].DefaultCellStyle.BackColor = Color.FromArgb(227, 242, 253);
        grid.Rows[totalRowIndex].DefaultCellStyle.Font = new Font("Cairo", 10F, FontStyle.Bold);
        grid.Rows[totalRowIndex].DefaultCellStyle.ForeColor = ColorScheme.Primary;
        grid.Rows[totalRowIndex].Cells[5].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        
        return grid;
    }
    
    private void BuildOptionalToursTab()
    {
        var tab = _tabControl.TabPages[6];
        tab.BackColor = Color.White;
        
        if (!_trip.OptionalTours.Any())
        {
            AddEmptyMessage(tab, "🎯 لا توجد رحلات اختيارية مسجلة");
            return;
        }
        
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            Font = new Font("Cairo", 9F),
            AllowUserToAddRows = false,
            ReadOnly = true,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            AllowUserToResizeRows = false
        };
        
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "نوع الرحلة", Width = 200 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Participants", HeaderText = "عدد الأفراد", Width = 100 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Selling", HeaderText = "سعر البيع (جنيه)", Width = 110 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Purchase", HeaderText = "سعر الشراء (جنيه)", Width = 110 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GuideComm", HeaderText = "عمولة المرشد (جنيه)", Width = 120 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "SalesComm", HeaderText = "عمولة المندوب (جنيه)", Width = 120 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Revenue", HeaderText = "الإيراد (جنيه)", Width = 110 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cost", HeaderText = "التكلفة (جنيه)", Width = 110 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Profit", HeaderText = "الربح (جنيه)", Width = 110 });
        
        foreach (var tour in _trip.OptionalTours)
        {
            grid.Rows.Add(
                tour.TourName,
                tour.ParticipantsCount,
                $"{tour.SellingPrice:N2}",
                $"{tour.PurchasePrice:N2}",
                $"{tour.GuideCommission:N2}",
                $"{tour.SalesRepCommission:N2}",
                $"{tour.TotalRevenue:N2}",
                $"{tour.TotalCost:N2}",
                $"{tour.NetProfit:N2}"
            );
        }
        
        tab.Controls.Add(grid);
    }
    
    private void BuildExpensesTab()
    {
        var tab = _tabControl.TabPages[7];
        tab.BackColor = Color.White;
        
        if (!_trip.Expenses.Any())
        {
            AddEmptyMessage(tab, "💸 لا توجد مصاريف مسجلة");
            return;
        }
        
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            Font = new Font("Cairo", 9F),
            AllowUserToAddRows = false,
            ReadOnly = true,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            AllowUserToResizeRows = false
        };
        
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "النوع", Width = 150 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "الوصف", Width = 300 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Amount", HeaderText = "المبلغ (جنيه)", Width = 120 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "التاريخ", Width = 100 });
        
        foreach (var expense in _trip.Expenses.OrderByDescending(e => e.ExpenseDate))
        {
            grid.Rows.Add(
                expense.ExpenseType,
                expense.Description,
                $"{expense.Amount:N2}",
                expense.ExpenseDate.ToString("yyyy-MM-dd")
            );
        }
        
        tab.Controls.Add(grid);
    }
    
    private void EditButton_Click(object? sender, EventArgs e)
    {
        try
        {
            // ✅ التحقق من أن الرحلة غير مقفولة قبل فتح نموذج التعديل
            if (_trip.IsLockedForTrips)
            {
                MessageBox.Show(
                    "⚠️ لا يمكن تعديل هذه الرحلة!\n\n" +
                    "الرحلة مقفولة من قبل قسم الحجوزات.\n" +
                    "يجب فتح الرحلة من قسم الحجوزات أولاً لتتمكن من تعديلها.",
                    "رحلة مقفولة",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                return;
            }
            
            // فتح نموذج التعديل
            var editForm = new AddEditTripForm(_tripService, _currentUserId, _tripId);
            
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                // إعادة تحميل البيانات بعد التعديل
                _ = LoadDataAsync();
                
                MessageBox.Show("تم تعديل الرحلة بنجاح", "نجاح", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في فتح نموذج التعديل: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }
    
    // Helper Methods
    private void AddSectionHeader(TabPage tab, string text, ref int y)
    {
        var label = new Label
        {
            Text = text,
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(30, y)
        };
        tab.Controls.Add(label);
        y += 40;
    }
    
    private void AddInfoItem(TabPage tab, string label, string value, ref int y, bool bold = false, Color? color = null)
    {
        var lblLabel = new Label
        {
            Text = label,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(30, y)
        };
        tab.Controls.Add(lblLabel);
        
        var lblValue = new Label
        {
            Text = value,
            Font = new Font("Cairo", 10F, bold ? FontStyle.Bold : FontStyle.Regular),
            AutoSize = true,
            Location = new Point(250, y),
            ForeColor = color ?? Color.FromArgb(52, 73, 94)
        };
        tab.Controls.Add(lblValue);
        y += 35;
    }
    
    private void AddEmptyMessage(TabPage tab, string message)
    {
        var label = new Label
        {
            Text = message,
            Font = new Font("Cairo", 12F),
            ForeColor = Color.FromArgb(149, 165, 166),
            AutoSize = true,
            Location = new Point(350, 250)
        };
        tab.Controls.Add(label);
    }
    
    private string GetTripTypeText(TripType type)
    {
        return type switch
        {
            TripType.Umrah => "عمرة",
            TripType.DomesticTourism => "سياحة داخلية",
            TripType.InternationalTourism => "سياحة خارجية",
            TripType.Hajj => "حج",
            TripType.Religious => "رحلات دينية",
            TripType.Educational => "رحلات تعليمية",
            _ => "غير محدد"
        };
    }
    
    private string GetStatusText(TripStatus status)
    {
        return status switch
        {
            TripStatus.Draft => "مسودة",
            TripStatus.Unconfirmed => "مخطط",
            TripStatus.Confirmed => "نشط",
            TripStatus.InProgress => "جاري التنفيذ",
            TripStatus.Completed => "مكتمل",
            TripStatus.Cancelled => "ملغي",
            _ => "غير محدد"
        };
    }
    
    private string GetTransportTypeText(TransportationType type)
    {
        return type switch
        {
            TransportationType.Bus => "أتوبيس",
            TransportationType.MiniBus => "ميني باص",
            TransportationType.Coaster => "كوستر",
            TransportationType.HiAce => "هاي أس",
            TransportationType.Car => "ملاكي",
            TransportationType.Plane => "طائرة",
            TransportationType.Train => "قطار",
            _ => "غير محدد"
        };
    }
    
    private string GetAccommodationTypeText(AccommodationType type)
    {
        return type switch
        {
            AccommodationType.Hotel => "فندق",
            AccommodationType.NileCruise => "نايل كروز",
            AccommodationType.Resort => "منتجع",
            AccommodationType.Apartment => "شقة فندقية",
            AccommodationType.Hostel => "بيت شباب",
            _ => "غير محدد"
        };
    }
    
    
    private string GetRoomTypeText(RoomType type)
    {
        return type switch
        {
            RoomType.Single => "فردي",
            RoomType.Double => "مزدوج",
            RoomType.Triple => "ثلاثي",
            RoomType.Quad => "رباعي",
            RoomType.Suite => "جناح",
            _ => "غير محدد"
        };
    }
    
    // ═══════════════════════════════════════════════════════════
    // دوال مساعدة للتقرير المالي
    // ═══════════════════════════════════════════════════════════
    
    private void AddFinancialSectionHeader(Panel parent, string title, ref int y)
    {
        var header = new Panel
        {
            Location = new Point(10, y),
            Size = new Size(1100, 45),
            BackColor = Color.FromArgb(240, 240, 240)
        };
        
        var label = new Label
        {
            Text = title,
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            Location = new Point(15, 10),
            AutoSize = true
        };
        
        header.Controls.Add(label);
        parent.Controls.Add(header);
        y += 50;
    }
    
    private void AddFinancialGrid(Panel parent, DataGridView grid, ref int y, int height)
    {
        grid.Location = new Point(10, y);
        grid.Size = new Size(1100, height);
        parent.Controls.Add(grid);
        y += height + 15;
    }
    
    private DataGridView CreateFinancialGridBase()
    {
        return new DataGridView
        {
            Font = new Font("Cairo", 9F),
            AllowUserToAddRows = false,
            ReadOnly = true,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            AllowUserToResizeRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            ColumnHeadersHeight = 40,
            RowTemplate = { Height = 35 }
        };
    }
    
    private DataGridView CreateRevenueGrid()
    {
        var grid = CreateFinancialGridBase();
        
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Item", HeaderText = "البيان", Width = 300 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Value", HeaderText = "القيمة", Width = 200 });
        
        grid.Rows.Add("سعر الفرد", $"{_trip.SellingPricePerPerson:N2} جنيه");
        grid.Rows.Add("الطاقة الكاملة", $"{_trip.TotalCapacity} فرد");
        grid.Rows.Add("المحجوز حالياً", $"{_trip.BookedSeats} فرد");
        grid.Rows.Add("إجمالي الإيرادات المتوقعة", $"{_trip.TotalRevenue:N2} جنيه");
        grid.Rows.Add("الإيرادات الفعلية", $"{_trip.ExpectedRevenue:N2} جنيه");
        
        // تلوين الصفوف
        grid.Rows[3].DefaultCellStyle.BackColor = Color.FromArgb(232, 245, 233);
        grid.Rows[3].DefaultCellStyle.Font = new Font("Cairo", 9F, FontStyle.Bold);
        
        return grid;
    }
    
    private DataGridView CreateProgramGrid()
    {
        var grid = CreateFinancialGridBase();
        
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Day", HeaderText = "اليوم", Width = 100 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Visits", HeaderText = "المزارات", Width = 250 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "VisitsCost", HeaderText = "تكلفة المزارات", Width = 150 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Participants", HeaderText = "عدد الأفراد", Width = 120 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GuideCost", HeaderText = "تكلفة المرشد", Width = 150 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "الإجمالي", Width = 150 });
        
        decimal totalProgramCost = 0;
        foreach (var program in _trip.Programs.OrderBy(p => p.DayNumber))
        {
            decimal dayTotal = (program.VisitsCost * program.ParticipantsCount) + program.GuideCost;
            totalProgramCost += dayTotal;
            
            grid.Rows.Add(
                $"يوم {program.DayNumber}",
                program.Visits ?? "",
                $"{program.VisitsCost:N2}",
                program.ParticipantsCount,
                $"{program.GuideCost:N2}",
                $"{dayTotal:N2}"
            );
        }
        
        // صف الإجمالي
        grid.Rows.Add("", "الإجمالي", "", "", "", $"{totalProgramCost:N2} جنيه");
        grid.Rows[grid.Rows.Count - 1].DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 205);
        grid.Rows[grid.Rows.Count - 1].DefaultCellStyle.Font = new Font("Cairo", 9F, FontStyle.Bold);
        
        return grid;
    }
    
    private DataGridView CreateTransportationGrid()
    {
        var grid = CreateFinancialGridBase();
        
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "النوع", Width = 150 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Route", HeaderText = "المسار", Width = 300 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Vehicles", HeaderText = "عدد المركبات", Width = 120 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CostPerVehicle", HeaderText = "تكلفة المركبة", Width = 150 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "الإجمالي", Width = 150 });
        
        decimal totalTransportCost = 0;
        foreach (var transport in _trip.Transportation)
        {
            totalTransportCost += transport.TotalCost;
            
            grid.Rows.Add(
                GetTransportTypeText(transport.Type),
                transport.Route ?? "-",
                transport.NumberOfVehicles,
                $"{transport.CostPerVehicle:N2}",
                $"{transport.TotalCost:N2}"
            );
        }
        
        // صف الإجمالي
        grid.Rows.Add("", "الإجمالي", "", "", $"{totalTransportCost:N2} جنيه");
        grid.Rows[grid.Rows.Count - 1].DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 205);
        grid.Rows[grid.Rows.Count - 1].DefaultCellStyle.Font = new Font("Cairo", 9F, FontStyle.Bold);
        
        return grid;
    }
    
    private DataGridView CreateAccommodationGrid()
    {
        var grid = CreateFinancialGridBase();
        
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Hotel", HeaderText = "الفندق", Width = 180 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "النوع", Width = 100 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rating", HeaderText = "التصنيف", Width = 80 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "RoomType", HeaderText = "نوع الغرفة", Width = 90 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rooms", HeaderText = "عدد الغرف", Width = 80 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nights", HeaderText = "الليالي", Width = 70 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CheckIn", HeaderText = "تاريخ الدخول", Width = 100 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CheckOut", HeaderText = "تاريخ الخروج", Width = 100 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CostPerNight", HeaderText = "السعر/ليلة", Width = 100 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GuideCost", HeaderText = "تكلفة المرشد", Width = 100 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "الإجمالي", Width = 120 });
        
        decimal totalAccommodationCost = 0;
        decimal totalGuideCost = 0;
        
        foreach (var accommodation in _trip.Accommodations)
        {
            totalAccommodationCost += accommodation.TotalCost;
            totalGuideCost += accommodation.GuideCost;
            
            // التصنيف بالنجوم
            string rating = "-";
            if (accommodation.Rating.HasValue)
            {
                rating = new string('⭐', (int)accommodation.Rating.Value);
            }
            else if (accommodation.CruiseLevel.HasValue)
            {
                rating = accommodation.CruiseLevel.Value.ToString();
            }
            
            grid.Rows.Add(
                accommodation.HotelName,
                GetAccommodationTypeText(accommodation.Type),
                rating,
                GetRoomTypeText(accommodation.RoomType),
                accommodation.NumberOfRooms,
                accommodation.NumberOfNights,
                accommodation.CheckInDate.ToString("yyyy-MM-dd"),
                accommodation.CheckOutDate.ToString("yyyy-MM-dd"),
                $"{accommodation.CostPerRoomPerNight:N2}",
                $"{accommodation.GuideCost:N2}",
                $"{accommodation.TotalCost:N2}"
            );
        }
        
        // صف الإجمالي
        grid.Rows.Add("", "الإجمالي", "", "", 
            _trip.Accommodations.Sum(a => a.NumberOfRooms), 
            _trip.Accommodations.Sum(a => a.NumberOfNights),
            "", "", "",
            $"{totalGuideCost:N2}",
            $"{totalAccommodationCost:N2} جنيه");
        grid.Rows[grid.Rows.Count - 1].DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 205);
        grid.Rows[grid.Rows.Count - 1].DefaultCellStyle.Font = new Font("Cairo", 9F, FontStyle.Bold);
        
        return grid;
    }
    
    private DataGridView CreateGuidesGrid()
    {
        var grid = CreateFinancialGridBase();
        
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Source", HeaderText = "المصدر", Width = 400 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cost", HeaderText = "التكلفة", Width = 200 });
        
        decimal programGuideCost = _trip.Programs.Sum(p => p.GuideCost);
        decimal mainGuideCost = _trip.Guides.Sum(g => g.TotalCost);
        decimal totalGuideCost = programGuideCost + mainGuideCost;
        
        grid.Rows.Add("المرشدين من البرنامج اليومي", $"{programGuideCost:N2} جنيه");
        grid.Rows.Add("المرشدين الرئيسيين", $"{mainGuideCost:N2} جنيه");
        grid.Rows.Add("إجمالي تكلفة المرشدين", $"{totalGuideCost:N2} جنيه");
        
        grid.Rows[2].DefaultCellStyle.BackColor = Color.FromArgb(227, 242, 253);
        grid.Rows[2].DefaultCellStyle.Font = new Font("Cairo", 10F, FontStyle.Bold);
        
        return grid;
    }
    
    private DataGridView CreateOptionalToursGrid()
    {
        var grid = CreateFinancialGridBase();
        
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "اسم الرحلة", Width = 250 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "السعر", Width = 120 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Bookings", HeaderText = "الحجوزات", Width = 100 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Revenue", HeaderText = "الإيرادات", Width = 150 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cost", HeaderText = "التكلفة", Width = 150 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Profit", HeaderText = "الربح", Width = 150 });
        
        decimal totalRevenue = 0;
        decimal totalCost = 0;
        decimal totalProfit = 0;
        
        foreach (var tour in _trip.OptionalTours)
        {
            totalRevenue += tour.TotalRevenue;
            totalCost += tour.TotalCost;
            totalProfit += tour.NetProfit;
            
            grid.Rows.Add(
                tour.TourName,
                $"{tour.SellingPrice:N2}",
                tour.ParticipantsCount,
                $"{tour.TotalRevenue:N2}",
                $"{tour.TotalCost:N2}",
                $"{tour.NetProfit:N2}"
            );
        }
        
        // صف الإجمالي
        grid.Rows.Add("الإجمالي", "", "", $"{totalRevenue:N2}", $"{totalCost:N2}", $"{totalProfit:N2} جنيه");
        grid.Rows[grid.Rows.Count - 1].DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 205);
        grid.Rows[grid.Rows.Count - 1].DefaultCellStyle.Font = new Font("Cairo", 9F, FontStyle.Bold);
        
        return grid;
    }
    
    private DataGridView CreateExpensesGrid()
    {
        var grid = CreateFinancialGridBase();
        
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "البيان", Width = 500 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Amount", HeaderText = "المبلغ", Width = 200 });
        
        decimal totalExpenses = 0;
        foreach (var expense in _trip.Expenses)
        {
            totalExpenses += expense.Amount;
            grid.Rows.Add(expense.Description, $"{expense.Amount:N2} جنيه");
        }
        
        // صف الإجمالي
        grid.Rows.Add("الإجمالي", $"{totalExpenses:N2} جنيه");
        grid.Rows[grid.Rows.Count - 1].DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 205);
        grid.Rows[grid.Rows.Count - 1].DefaultCellStyle.Font = new Font("Cairo", 9F, FontStyle.Bold);
        
        return grid;
    }
    
    private DataGridView CreateFinalSummaryGrid()
    {
        var grid = CreateFinancialGridBase();
        
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Item", HeaderText = "البيان", Width = 500 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Value", HeaderText = "القيمة", Width = 200 });
        
        // الإيرادات
        grid.Rows.Add("💵 إجمالي الإيرادات المتوقعة", $"{_trip.TotalRevenue:N2} جنيه");
        grid.Rows[0].DefaultCellStyle.BackColor = Color.FromArgb(232, 245, 233);
        grid.Rows[0].DefaultCellStyle.Font = new Font("Cairo", 10F, FontStyle.Bold);
        
        // التكاليف
        decimal programCost = _trip.Programs.Sum(p => (p.VisitsCost * p.ParticipantsCount) + p.GuideCost);
        decimal transportCost = _trip.Transportation.Sum(t => t.TotalCost);
        decimal accommodationCost = _trip.Accommodations.Sum(a => a.TotalCost);
        decimal guideCost = _trip.Guides.Sum(g => g.TotalCost);
        decimal optionalToursCost = _trip.OptionalTours.Sum(o => o.TotalCost);
        decimal expensesCost = _trip.Expenses.Sum(e => e.Amount);
        
        grid.Rows.Add("  • البرنامج اليومي", $"{programCost:N2} جنيه");
        grid.Rows.Add("  • النقل", $"{transportCost:N2} جنيه");
        grid.Rows.Add("  • الإقامة", $"{accommodationCost:N2} جنيه");
        grid.Rows.Add("  • المرشدين الرئيسيين", $"{guideCost:N2} جنيه");
        grid.Rows.Add("  • الرحلات الاختيارية", $"{optionalToursCost:N2} جنيه");
        grid.Rows.Add("  • مصاريف أخرى", $"{expensesCost:N2} جنيه");
        grid.Rows.Add("💸 إجمالي التكاليف", $"{_trip.TotalCost:N2} جنيه");
        grid.Rows[7].DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 238);
        grid.Rows[7].DefaultCellStyle.Font = new Font("Cairo", 10F, FontStyle.Bold);
        
        // الربح
        grid.Rows.Add("📈 صافي الربح المتوقع", $"{_trip.NetProfit:N2} جنيه");
        grid.Rows[8].DefaultCellStyle.BackColor = _trip.NetProfit > 0 ? 
            Color.FromArgb(200, 230, 201) : Color.FromArgb(255, 205, 210);
        grid.Rows[8].DefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        
        grid.Rows.Add("هامش الربح", $"{_trip.ProfitMargin:F1}%");
        grid.Rows.Add("الربح لكل فرد", $"{(_trip.TotalCapacity > 0 ? _trip.NetProfit / _trip.TotalCapacity : 0):N2} جنيه");
        
        return grid;
    }
    
    private PrintDocument _printDocument = null!;
    private int _currentPrintPage = 0;
    
    private void PrintFinancialReport()
    {
        try
        {
            // إنشاء PrintDocument
            _printDocument = new PrintDocument();
            _printDocument.PrintPage += PrintDocument_PrintPage;
            _printDocument.DefaultPageSettings.Landscape = false;
            _printDocument.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(50, 50, 50, 50);
            
            // إظهار معاينة الطباعة
            PrintPreviewDialog previewDialog = new PrintPreviewDialog
            {
                Document = _printDocument,
                Width = 1000,
                Height = 700,
                StartPosition = FormStartPosition.CenterParent,
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true
            };
            
            _currentPrintPage = 0;
            
            if (previewDialog.ShowDialog() == DialogResult.OK)
            {
                // إذا وافق المستخدم، نطبع
                _printDocument.Print();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"حدث خطأ في الطباعة: {ex.Message}",
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
            );
        }
    }
    
    private void PrintDocument_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
    {
        try
        {
            Graphics g = e.Graphics!;
            Font titleFont = new Font("Cairo", 18F, FontStyle.Bold);
            Font headerFont = new Font("Cairo", 14F, FontStyle.Bold);
            Font normalFont = new Font("Cairo", 10F);
            Font boldFont = new Font("Cairo", 10F, FontStyle.Bold);
            
            Brush blackBrush = Brushes.Black;
            Brush grayBrush = Brushes.Gray;
            Pen grayPen = new Pen(Color.LightGray, 1);
            
            float y = e.MarginBounds.Top;
            float x = e.MarginBounds.Left;
            float rightX = e.MarginBounds.Right;
            
            // ═══════════════════════════════════════════════════════════
            // العنوان الرئيسي
            // ═══════════════════════════════════════════════════════════
            string title = "📊 التقرير المالي الشامل";
            SizeF titleSize = g.MeasureString(title, titleFont);
            g.DrawString(title, titleFont, blackBrush, 
                rightX - titleSize.Width, y, new StringFormat { Alignment = StringAlignment.Near });
            y += titleSize.Height + 10;
            
            // معلومات الرحلة
            g.DrawString($"الرحلة: {_trip.TripName}", headerFont, blackBrush, 
                rightX - g.MeasureString($"الرحلة: {_trip.TripName}", headerFont).Width, y);
            y += 30;
            
            g.DrawString($"رقم الرحلة: {_trip.TripNumber}", normalFont, grayBrush, 
                rightX - g.MeasureString($"رقم الرحلة: {_trip.TripNumber}", normalFont).Width, y);
            y += 25;
            
            g.DrawString($"تاريخ الطباعة: {DateTime.Now:yyyy-MM-dd HH:mm}", normalFont, grayBrush, 
                rightX - g.MeasureString($"تاريخ الطباعة: {DateTime.Now:yyyy-MM-dd HH:mm}", normalFont).Width, y);
            y += 30;
            
            // خط فاصل
            g.DrawLine(grayPen, x, y, rightX, y);
            y += 20;
            
            // ═══════════════════════════════════════════════════════════
            // 1. ملخص الإيرادات
            // ═══════════════════════════════════════════════════════════
            y = DrawSection(g, "💵 ملخص الإيرادات", y, rightX, headerFont, normalFont, boldFont, blackBrush, grayPen);
            y = DrawKeyValue(g, "سعر الفرد", $"{_trip.SellingPricePerPerson:N2} جنيه", y, rightX, normalFont, blackBrush);
            y = DrawKeyValue(g, "الطاقة الكاملة", $"{_trip.TotalCapacity} فرد", y, rightX, normalFont, blackBrush);
            y = DrawKeyValue(g, "المحجوز حالياً", $"{_trip.BookedSeats} فرد", y, rightX, normalFont, blackBrush);
            y = DrawKeyValue(g, "إجمالي الإيرادات المتوقعة", $"{_trip.TotalRevenue:N2} جنيه", y, rightX, boldFont, blackBrush);
            y += 15;
            
            // ═══════════════════════════════════════════════════════════
            // 2. التكاليف
            // ═══════════════════════════════════════════════════════════
            decimal programCost = _trip.Programs.Sum(p => (p.VisitsCost * p.ParticipantsCount) + p.GuideCost);
            decimal transportCost = _trip.Transportation.Sum(t => t.TotalCost);
            decimal accommodationCost = _trip.Accommodations.Sum(a => a.TotalCost);
            decimal guideCost = _trip.Guides.Sum(g => g.TotalCost);
            decimal optionalToursCost = _trip.OptionalTours.Sum(o => o.TotalCost);
            decimal expensesCost = _trip.Expenses.Sum(e => e.Amount);
            
            y = DrawSection(g, "💸 التكاليف التفصيلية", y, rightX, headerFont, normalFont, boldFont, blackBrush, grayPen);
            y = DrawKeyValue(g, "البرنامج اليومي", $"{programCost:N2} جنيه", y, rightX, normalFont, blackBrush);
            y = DrawKeyValue(g, "النقل والمواصلات", $"{transportCost:N2} جنيه", y, rightX, normalFont, blackBrush);
            y = DrawKeyValue(g, "الإقامة", $"{accommodationCost:N2} جنيه", y, rightX, normalFont, blackBrush);
            y = DrawKeyValue(g, "المرشدين الرئيسيين", $"{guideCost:N2} جنيه", y, rightX, normalFont, blackBrush);
            y = DrawKeyValue(g, "الرحلات الاختيارية", $"{optionalToursCost:N2} جنيه", y, rightX, normalFont, blackBrush);
            y = DrawKeyValue(g, "مصاريف أخرى", $"{expensesCost:N2} جنيه", y, rightX, normalFont, blackBrush);
            y = DrawKeyValue(g, "إجمالي التكاليف", $"{_trip.TotalCost:N2} جنيه", y, rightX, boldFont, blackBrush);
            y += 15;
            
            // ═══════════════════════════════════════════════════════════
            // 3. الملخص النهائي
            // ═══════════════════════════════════════════════════════════
            y = DrawSection(g, "📈 الملخص النهائي", y, rightX, headerFont, normalFont, boldFont, blackBrush, grayPen);
            
            // رسم مربع ملون للربح
            RectangleF profitBox = new RectangleF(rightX - 400, y, 380, 80);
            Color profitColor = _trip.NetProfit > 0 ? Color.FromArgb(200, 230, 201) : Color.FromArgb(255, 205, 210);
            g.FillRectangle(new SolidBrush(profitColor), profitBox);
            g.DrawRectangle(new Pen(profitColor, 2), Rectangle.Round(profitBox));
            
            Font largeBoldFont = new Font("Cairo", 14F, FontStyle.Bold);
            string profitText = $"صافي الربح: {_trip.NetProfit:N2} جنيه";
            SizeF profitSize = g.MeasureString(profitText, largeBoldFont);
            g.DrawString(profitText, largeBoldFont, blackBrush, 
                profitBox.Right - profitSize.Width - 10, profitBox.Y + 15);
            
            string marginText = $"هامش الربح: {_trip.ProfitMargin:F1}%";
            g.DrawString(marginText, normalFont, blackBrush, 
                profitBox.Right - g.MeasureString(marginText, normalFont).Width - 10, profitBox.Y + 50);
            
            y += profitBox.Height + 20;
            
            // معلومات إضافية
            decimal profitPerPerson = _trip.TotalCapacity > 0 ? _trip.NetProfit / _trip.TotalCapacity : 0;
            y = DrawKeyValue(g, "الربح لكل فرد", $"{profitPerPerson:N2} جنيه", y, rightX, normalFont, blackBrush);
            
            // التذييل
            y = e.MarginBounds.Bottom - 50;
            g.DrawLine(grayPen, x, y, rightX, y);
            y += 10;
            
            string footer = $"تقرير مالي - نظام إدارة الرحلات - صفحة 1";
            SizeF footerSize = g.MeasureString(footer, new Font("Cairo", 8F));
            g.DrawString(footer, new Font("Cairo", 8F), grayBrush, 
                (rightX - footerSize.Width) / 2 + x, y);
            
            // لا توجد صفحات إضافية
            e.HasMorePages = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في الطباعة: {ex.Message}", "خطأ", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            e.HasMorePages = false;
        }
    }
    
    private float DrawSection(Graphics g, string title, float y, float rightX, 
        Font headerFont, Font normalFont, Font boldFont, Brush blackBrush, Pen grayPen)
    {
        SizeF titleSize = g.MeasureString(title, headerFont);
        g.DrawString(title, headerFont, blackBrush, rightX - titleSize.Width, y);
        y += titleSize.Height + 5;
        g.DrawLine(grayPen, rightX - 400, y, rightX, y);
        y += 15;
        return y;
    }
    
    private float DrawKeyValue(Graphics g, string key, string value, float y, float rightX, 
        Font font, Brush brush)
    {
        string text = $"{key}: {value}";
        SizeF textSize = g.MeasureString(text, font);
        g.DrawString(text, font, brush, rightX - textSize.Width, y);
        y += textSize.Height + 5;
        return y;
    }
    
    private void ExportFinancialReportToExcel()
    {
        try
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"تقرير_مالي_{_trip.TripNumber}_{DateTime.Now:yyyyMMdd}.xlsx",
                DefaultExt = "xlsx",
                AddExtension = true
            };
            
            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;
            
            // إنشاء بيانات Excel
            var data = new List<List<object>>();
            
            // العنوان
            data.Add(new List<object> { "التقرير المالي الشامل" });
            data.Add(new List<object> { $"الرحلة: {_trip.TripName}" });
            data.Add(new List<object> { $"رقم الرحلة: {_trip.TripNumber}" });
            data.Add(new List<object> { $"تاريخ التصدير: {DateTime.Now:yyyy-MM-dd HH:mm}" });
            data.Add(new List<object> { "" }); // سطر فارغ
            
            // ملخص الإيرادات
            data.Add(new List<object> { "💵 ملخص الإيرادات", "" });
            data.Add(new List<object> { "سعر الفرد", _trip.SellingPricePerPerson });
            data.Add(new List<object> { "الطاقة الكاملة", _trip.TotalCapacity });
            data.Add(new List<object> { "المحجوز حالياً", _trip.BookedSeats });
            data.Add(new List<object> { "إجمالي الإيرادات المتوقعة", _trip.TotalRevenue });
            data.Add(new List<object> { "" }); // سطر فارغ
            
            // التكاليف التفصيلية
            decimal programCost = _trip.Programs.Sum(p => (p.VisitsCost * p.ParticipantsCount) + p.GuideCost);
            decimal transportCost = _trip.Transportation.Sum(t => t.TotalCost);
            decimal accommodationCost = _trip.Accommodations.Sum(a => a.TotalCost);
            decimal guideCost = _trip.Guides.Sum(g => g.TotalCost);
            decimal optionalToursCost = _trip.OptionalTours.Sum(o => o.TotalCost);
            decimal expensesCost = _trip.Expenses.Sum(e => e.Amount);
            
            data.Add(new List<object> { "💸 التكاليف التفصيلية", "" });
            data.Add(new List<object> { "البرنامج اليومي", programCost });
            data.Add(new List<object> { "النقل والمواصلات", transportCost });
            data.Add(new List<object> { "الإقامة", accommodationCost });
            data.Add(new List<object> { "المرشدين الرئيسيين", guideCost });
            data.Add(new List<object> { "الرحلات الاختيارية", optionalToursCost });
            data.Add(new List<object> { "مصاريف أخرى", expensesCost });
            data.Add(new List<object> { "إجمالي التكاليف", _trip.TotalCost });
            data.Add(new List<object> { "" }); // سطر فارغ
            
            // الملخص النهائي
            data.Add(new List<object> { "📈 الملخص النهائي", "" });
            data.Add(new List<object> { "صافي الربح المتوقع", _trip.NetProfit });
            data.Add(new List<object> { "هامش الربح %", _trip.ProfitMargin });
            decimal profitPerPerson = _trip.TotalCapacity > 0 ? _trip.NetProfit / _trip.TotalCapacity : 0;
            data.Add(new List<object> { "الربح لكل فرد", profitPerPerson });
            
            // تحويل إلى JSON للكتابة
            var jsonData = System.Text.Json.JsonSerializer.Serialize(data);
            
            // كتابة الملف
            File.WriteAllText(saveDialog.FileName.Replace(".xlsx", ".json"), jsonData);
            
            // استخدام مكتبة Excel لإنشاء الملف
            CreateExcelFile(saveDialog.FileName, data);
            
            MessageBox.Show(
                $"تم تصدير التقرير بنجاح!\n\nالملف: {Path.GetFileName(saveDialog.FileName)}",
                "نجاح",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
            );
            
            // فتح الملف
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = saveDialog.FileName,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"حدث خطأ في التصدير: {ex.Message}",
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
            );
        }
    }
    
    private void CreateExcelFile(string filePath, List<List<object>> data)
    {
        // استخدام ClosedXML أو EPPlus لإنشاء ملف Excel
        // هذا مثال بسيط باستخدام CSV كبديل مؤقت
        var csv = new System.Text.StringBuilder();
        
        foreach (var row in data)
        {
            csv.AppendLine(string.Join("\t", row));
        }
        
        File.WriteAllText(filePath.Replace(".xlsx", ".csv"), csv.ToString(), System.Text.Encoding.UTF8);
        
        // TODO: استخدام مكتبة Excel حقيقية مثل ClosedXML أو EPPlus
        // في الوقت الحالي، نحفظ كـ CSV
    }
}
