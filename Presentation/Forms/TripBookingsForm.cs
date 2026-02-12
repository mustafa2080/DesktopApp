using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace GraceWay.AccountingSystem.Presentation.Forms;

/// <summary>
/// قائمة حجوزات رحلة معينة
/// </summary>
public partial class TripBookingsForm : Form
{
    private readonly ITripBookingService _bookingService;
    private readonly ICustomerService _customerService;
    private readonly int _currentUserId;
    private readonly int _tripId;
#pragma warning disable CS0414
    private Trip _trip = null!;
#pragma warning restore CS0414
    
    // Controls
    private Panel _headerPanel = null!;
    private Label _tripInfoLabel = null!;
    private Button _addBookingButton = null!;
    private Button _recordPaymentButton = null!;
    private Button _cancelBookingButton = null!;
    private Button _confirmBookingButton = null!;
    private Button _refreshButton = null!;
    private Button _viewTripDetailsButton = null!;
    private TabControl _mainTabControl = null!;
    private DataGridView _bookingsGrid = null!;
    private DataGridView _accountingGrid = null!;
    private Panel _statsPanel = null!;
    private Panel _accountingSummaryPanel = null!;
    
    public TripBookingsForm(ITripBookingService bookingService, ICustomerService customerService, 
        int currentUserId, int tripId)
    {
        _bookingService = bookingService;
        _customerService = customerService;
        _currentUserId = currentUserId;
        _tripId = tripId;
        
        InitializeComponent();
        SetupForm();
        InitializeCustomControls();
        _ = LoadDataAsync();
    }
    
    private void SetupForm()
    {
        this.Text = "إدارة حجوزات الرحلة";
        this.Size = new Size(1500, 900);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
        this.WindowState = FormWindowState.Maximized;
    }
    
    private void InitializeCustomControls()
    {
        // Header Panel
        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 160,
            BackColor = Color.White,
            Padding = new Padding(20)
        };
        
        Label titleLabel = new Label
        {
            Text = "💼 إدارة حجوزات الرحلة",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(20, 20)
        };
        _headerPanel.Controls.Add(titleLabel);
        
        _tripInfoLabel = new Label
        {
            Text = "جاري التحميل...",
            Font = new Font("Cairo", 11F),
            ForeColor = Color.FromArgb(52, 73, 94),
            AutoSize = true,
            Location = new Point(20, 60)
        };
        _headerPanel.Controls.Add(_tripInfoLabel);
        
        // Action Buttons
        _addBookingButton = CreateButton("➕ حجز جديد", ColorScheme.Success, new Point(20, 105), AddBooking_Click);
        _headerPanel.Controls.Add(_addBookingButton);
        
        _recordPaymentButton = CreateButton("💰 تسجيل دفعة", Color.FromArgb(41, 128, 185), new Point(180, 105), RecordPayment_Click);
        _headerPanel.Controls.Add(_recordPaymentButton);
        
        _confirmBookingButton = CreateButton("✅ تأكيد الحجز", ColorScheme.Success, new Point(340, 105), ConfirmBooking_Click);
        _headerPanel.Controls.Add(_confirmBookingButton);
        
        _cancelBookingButton = CreateButton("❌ إلغاء الحجز", ColorScheme.Error, new Point(500, 105), CancelBooking_Click);
        _headerPanel.Controls.Add(_cancelBookingButton);
        
        _refreshButton = CreateButton("🔄 تحديث", Color.FromArgb(52, 73, 94), new Point(660, 105), (s, e) => _ = LoadDataAsync());
        _headerPanel.Controls.Add(_refreshButton);
        
        _viewTripDetailsButton = CreateButton("📋 تفاصيل الرحلة", Color.FromArgb(142, 68, 173), new Point(820, 105), ViewTripDetails_Click);
        _headerPanel.Controls.Add(_viewTripDetailsButton);
        
        // Stats Panel
        _statsPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 100,
            BackColor = Color.White,
            Padding = new Padding(20)
        };
        
        // Tab Control for main content
        _mainTabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            RightToLeftLayout = true
        };
        
        // Tab 1: Bookings List
        TabPage bookingsTab = new TabPage("📋 قائمة الحجوزات");
        Panel bookingsPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(20)
        };
        _bookingsGrid = CreateDataGrid();
        bookingsPanel.Controls.Add(_bookingsGrid);
        bookingsTab.Controls.Add(bookingsPanel);
        _mainTabControl.TabPages.Add(bookingsTab);
        
        // Tab 2: Accounting Details
        TabPage accountingTab = new TabPage("💰 تفاصيل الحسابات");
        Panel accountingMainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(20)
        };
        
        _accountingSummaryPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 200,
            BackColor = Color.White,
            Padding = new Padding(10)
        };
        accountingMainPanel.Controls.Add(_accountingSummaryPanel);
        
        Panel accountingGridPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(0, 210, 0, 0)
        };
        _accountingGrid = CreateAccountingGrid();
        accountingGridPanel.Controls.Add(_accountingGrid);
        accountingMainPanel.Controls.Add(accountingGridPanel);
        
        accountingTab.Controls.Add(accountingMainPanel);
        _mainTabControl.TabPages.Add(accountingTab);
        
        // Tab 3: التقرير المفصل للرحلة
        TabPage tripReportTab = new TabPage("📊 تقرير الرحلة المفصل");
        Panel tripReportPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(20),
            AutoScroll = true
        };
        tripReportTab.Controls.Add(tripReportPanel);
        _mainTabControl.TabPages.Add(tripReportTab);
        
        this.Controls.Add(_mainTabControl);
        this.Controls.Add(_statsPanel);
        this.Controls.Add(_headerPanel);
    }
    
    private DataGridView CreateDataGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false,
            EnableHeadersVisualStyles = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowTemplate = { Height = 40 }
        };
        
        grid.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Primary;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
        grid.ColumnHeadersHeight = 45;
        
        grid.DefaultCellStyle.Font = new Font("Cairo", 10F);
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.ForeColor = Color.FromArgb(52, 73, 94);
        grid.DefaultCellStyle.SelectionBackColor = ColorScheme.Primary;
        grid.DefaultCellStyle.SelectionForeColor = Color.White;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
        
        // Columns مع DataPropertyName للربط مع البيانات
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "TripBookingId", 
            DataPropertyName = "TripBookingId",
            HeaderText = "المعرف", 
            Visible = false 
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "BookingNumber", 
            DataPropertyName = "BookingNumber",
            HeaderText = "رقم الحجز", 
            Width = 120 
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "CustomerName", 
            DataPropertyName = "CustomerName",
            HeaderText = "اسم العميل", 
            FillWeight = 20 
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "CreatorName", 
            DataPropertyName = "CreatorName",
            HeaderText = "المستخدم", 
            Width = 120 
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "NumberOfPersons", 
            DataPropertyName = "NumberOfPersons",
            HeaderText = "الأفراد", 
            Width = 80, 
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } 
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "TotalAmount", 
            DataPropertyName = "TotalAmount",
            HeaderText = "المبلغ الكلي", 
            Width = 110, 
            DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } 
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "PaidAmount", 
            DataPropertyName = "PaidAmount",
            HeaderText = "المدفوع", 
            Width = 110, 
            DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } 
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "RemainingAmount", 
            DataPropertyName = "RemainingAmount",
            HeaderText = "المتبقي", 
            Width = 110, 
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } 
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "PaymentStatusDisplay", 
            DataPropertyName = "PaymentStatusDisplay",
            HeaderText = "حالة الدفع", 
            Width = 110 
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "StatusDisplay", 
            DataPropertyName = "StatusDisplay",
            HeaderText = "حالة الحجز", 
            Width = 110 
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "BookingDate", 
            DataPropertyName = "BookingDate",
            HeaderText = "تاريخ الحجز", 
            Width = 110, 
            DefaultCellStyle = { Format = "yyyy-MM-dd" } 
        });
        
        grid.CellFormatting += Grid_CellFormatting;
        return grid;
    }
    
    private DataGridView CreateAccountingGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false,
            EnableHeadersVisualStyles = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowTemplate = { Height = 45 }
        };
        
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
        grid.ColumnHeadersHeight = 45;
        
        grid.DefaultCellStyle.Font = new Font("Cairo", 10F);
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.ForeColor = Color.FromArgb(52, 73, 94);
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(41, 128, 185);
        grid.DefaultCellStyle.SelectionForeColor = Color.White;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
        
        // Columns مع DataPropertyName للربط مع البيانات
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "BookingNumber", 
            DataPropertyName = "BookingNumber",
            HeaderText = "رقم الحجز", 
            Width = 120 
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "CustomerName", 
            DataPropertyName = "CustomerName",
            HeaderText = "اسم العميل", 
            FillWeight = 20 
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "BookingDate", 
            DataPropertyName = "BookingDate",
            HeaderText = "تاريخ الحجز", 
            Width = 110, 
            DefaultCellStyle = { Format = "yyyy-MM-dd" } 
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "NumberOfPersons", 
            DataPropertyName = "NumberOfPersons",
            HeaderText = "عدد الأفراد", 
            Width = 90, 
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } 
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "PricePerPerson", 
            DataPropertyName = "PricePerPerson",
            HeaderText = "السعر/فرد", 
            Width = 100, 
            DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } 
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "TotalAmount", 
            DataPropertyName = "TotalAmount",
            HeaderText = "الإجمالي", 
            Width = 110, 
            DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Cairo", 10F, FontStyle.Bold) } 
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "PaidAmount", 
            DataPropertyName = "PaidAmount",
            HeaderText = "المدفوع", 
            Width = 110, 
            DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } 
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "RemainingAmount", 
            DataPropertyName = "RemainingAmount",
            HeaderText = "المتبقي", 
            Width = 110, 
            DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } 
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "PaymentsCount", 
            DataPropertyName = "PaymentsCount",
            HeaderText = "عدد الدفعات", 
            Width = 90, 
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } 
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "LastPaymentDate", 
            DataPropertyName = "LastPaymentDate",
            HeaderText = "آخر دفعة", 
            Width = 110, 
            DefaultCellStyle = { Format = "yyyy-MM-dd" } 
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "PaymentStatusDisplay", 
            DataPropertyName = "PaymentStatusDisplay",
            HeaderText = "حالة الدفع", 
            Width = 110 
        });
        
        grid.CellFormatting += AccountingGrid_CellFormatting;
        return grid;
    }
    
    private async Task LoadDataAsync()
    {
        try
        {
            this.Cursor = Cursors.WaitCursor;
            
            var bookings = await _bookingService.GetBookingsByTripAsync(_tripId);
            var bookingDisplayList = bookings.Select(b => new
            {
                b.TripBookingId,
                b.BookingNumber,
                CustomerName = b.Customer?.CustomerName ?? "غير محدد",
                CreatorName = b.Creator?.Username ?? "غير محدد",
                b.NumberOfPersons,
                b.TotalAmount,
                b.PaidAmount,
                b.RemainingAmount,
                PaymentStatusDisplay = GetPaymentStatusDisplay(b.PaymentStatus),
                StatusDisplay = GetBookingStatusDisplay(b.Status),
                b.BookingDate,
                SourceObject = b
            }).ToList();
            
            _bookingsGrid.DataSource = bookingDisplayList;
            UpdateStats(bookings);
            UpdateAccountingData(bookings);
            await UpdateTripReportAsync(); // ✅ تحديث التقرير المفصل
            UpdateButtonStates();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    
    private void UpdateStats(List<TripBooking> bookings)
    {
        int totalBookings = bookings.Count;
        int totalPersons = bookings.Sum(b => b.NumberOfPersons);
        decimal totalRevenue = bookings.Sum(b => b.TotalAmount);
        decimal totalPaid = bookings.Sum(b => b.PaidAmount);
        decimal totalRemaining = bookings.Sum(b => b.RemainingAmount);
        
        _statsPanel.Controls.Clear();
        int x = 20;
        
        AddStatBox("📊 إجمالي الحجوزات", totalBookings.ToString(), Color.FromArgb(52, 152, 219), x); x += 200;
        AddStatBox("👥 إجمالي الأفراد", totalPersons.ToString(), Color.FromArgb(155, 89, 182), x); x += 200;
        AddStatBox("💰 الإيرادات", $"{totalRevenue:N2}", Color.FromArgb(39, 174, 96), x); x += 200;
        AddStatBox("✅ المدفوع", $"{totalPaid:N2}", Color.FromArgb(26, 188, 156), x); x += 200;
        AddStatBox("⏳ المتبقي", $"{totalRemaining:N2}", Color.FromArgb(230, 126, 34), x);
    }
    
    private void UpdateAccountingData(List<TripBooking> bookings)
    {
        // Update accounting summary panel
        _accountingSummaryPanel.Controls.Clear();
        
        decimal totalRevenue = bookings.Sum(b => b.TotalAmount);
        decimal totalPaid = bookings.Sum(b => b.PaidAmount);
        decimal totalRemaining = bookings.Sum(b => b.RemainingAmount);
        decimal collectionRate = totalRevenue > 0 ? (totalPaid / totalRevenue) * 100 : 0;
        
        int fullyPaidCount = bookings.Count(b => b.PaymentStatus == PaymentStatus.FullyPaid);
        int partiallyPaidCount = bookings.Count(b => b.PaymentStatus == PaymentStatus.PartiallyPaid);
        int notPaidCount = bookings.Count(b => b.PaymentStatus == PaymentStatus.NotPaid);
        
        // Title
        Label titleLabel = new Label
        {
            Text = "📊 ملخص الحسابات المالية",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = Color.FromArgb(41, 128, 185),
            AutoSize = true,
            Location = new Point(10, 10)
        };
        _accountingSummaryPanel.Controls.Add(titleLabel);
        
        // Row 1: Financial Summary
        int y = 50;
        CreateAccountingSummaryCard("💰 إجمالي الإيرادات", $"{totalRevenue:N2} جنيه", Color.FromArgb(52, 152, 219), new Point(10, y));
        CreateAccountingSummaryCard("✅ إجمالي المحصل", $"{totalPaid:N2} جنيه", Color.FromArgb(39, 174, 96), new Point(280, y));
        CreateAccountingSummaryCard("⏳ إجمالي المتبقي", $"{totalRemaining:N2} جنيه", Color.FromArgb(230, 126, 34), new Point(550, y));
        CreateAccountingSummaryCard("📈 نسبة التحصيل", $"{collectionRate:N1}%", Color.FromArgb(155, 89, 182), new Point(820, y));
        
        // Row 2: Payment Status Breakdown
        y = 120;
        CreateAccountingSummaryCard("✓ مدفوع بالكامل", $"{fullyPaidCount} حجز", Color.FromArgb(26, 188, 156), new Point(10, y));
        CreateAccountingSummaryCard("⚠ دفع جزئي", $"{partiallyPaidCount} حجز", Color.FromArgb(241, 196, 15), new Point(280, y));
        CreateAccountingSummaryCard("✗ غير مدفوع", $"{notPaidCount} حجز", Color.FromArgb(231, 76, 60), new Point(550, y));
        CreateAccountingSummaryCard("📋 إجمالي الحجوزات", $"{bookings.Count} حجز", Color.FromArgb(52, 73, 94), new Point(820, y));
        
        // Update accounting grid
        var accountingData = bookings.Select(b => new
        {
            b.BookingNumber,
            CustomerName = b.Customer?.CustomerName ?? "غير محدد",
            b.BookingDate,
            b.NumberOfPersons,
            b.PricePerPerson,
            b.TotalAmount,
            b.PaidAmount,
            b.RemainingAmount,
            PaymentsCount = b.Payments?.Count ?? 0,
            LastPaymentDate = b.Payments?.OrderByDescending(p => p.PaymentDate).FirstOrDefault()?.PaymentDate,
            PaymentStatusDisplay = GetPaymentStatusDisplay(b.PaymentStatus),
            SourceObject = b
        }).OrderByDescending(b => b.BookingDate).ToList();
        
        _accountingGrid.DataSource = accountingData;
        
        // Add summary row
        if (bookings.Any())
        {
            _accountingGrid.Rows.Add(
                "الإجمالي",
                $"{bookings.Count} حجز",
                "",
                bookings.Sum(b => b.NumberOfPersons),
                "",
                totalRevenue,
                totalPaid,
                totalRemaining,
                bookings.Sum(b => b.Payments?.Count ?? 0),
                "",
                ""
            );
            
            var lastRow = _accountingGrid.Rows[_accountingGrid.Rows.Count - 1];
            lastRow.DefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);
            lastRow.DefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
            lastRow.DefaultCellStyle.ForeColor = Color.FromArgb(41, 128, 185);
        }
    }
    
    private void CreateAccountingSummaryCard(string title, string value, Color color, Point location)
    {
        Panel card = new Panel
        {
            Location = location,
            Size = new Size(260, 55),
            BackColor = color,
            Padding = new Padding(10)
        };
        
        Label lblTitle = new Label
        {
            Text = title,
            ForeColor = Color.White,
            Font = new Font("Cairo", 9F),
            AutoSize = true,
            Location = new Point(10, 8)
        };
        card.Controls.Add(lblTitle);
        
        Label lblValue = new Label
        {
            Text = value,
            ForeColor = Color.White,
            Font = new Font("Cairo", 13F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(10, 28)
        };
        card.Controls.Add(lblValue);
        
        _accountingSummaryPanel.Controls.Add(card);
    }
    
    private void AddStatBox(string label, string value, Color color, int x)
    {
        Panel box = new Panel { Location = new Point(x, 20), Size = new Size(180, 60), BackColor = color };
        Label lblTitle = new Label { Text = label, ForeColor = Color.White, Font = new Font("Cairo", 9F), AutoSize = true, Location = new Point(10, 8) };
        Label lblValue = new Label { Text = value, ForeColor = Color.White, Font = new Font("Cairo", 14F, FontStyle.Bold), AutoSize = true, Location = new Point(10, 30) };
        box.Controls.Add(lblTitle);
        box.Controls.Add(lblValue);
        _statsPanel.Controls.Add(box);
    }
    
    private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (_bookingsGrid.Rows.Count == 0 || e.RowIndex < 0) return;
        
        var row = _bookingsGrid.Rows[e.RowIndex];
        var item = row.DataBoundItem;
        if (item == null) return;
        
        var sourceObject = item.GetType().GetProperty("SourceObject")?.GetValue(item) as TripBooking;
        if (sourceObject == null) return;
        
        if (_bookingsGrid.Columns[e.ColumnIndex].Name == "RemainingAmount")
        {
            decimal remaining = sourceObject.RemainingAmount;
            e.Value = remaining.ToString("N2");
            e.CellStyle.ForeColor = remaining > 0 ? Color.FromArgb(230, 126, 34) : Color.FromArgb(39, 174, 96);
            e.FormattingApplied = true;
        }
    }
    
    private void AccountingGrid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (_accountingGrid.Rows.Count == 0 || e.RowIndex < 0) return;
        
        var row = _accountingGrid.Rows[e.RowIndex];
        var item = row.DataBoundItem;
        if (item == null) return;
        
        var sourceObject = item.GetType().GetProperty("SourceObject")?.GetValue(item) as TripBooking;
        if (sourceObject == null) return;
        
        // Color code for remaining amount
        if (_accountingGrid.Columns[e.ColumnIndex].Name == "RemainingAmount")
        {
            decimal remaining = sourceObject.RemainingAmount;
            if (remaining > 0)
            {
                e.CellStyle.ForeColor = Color.FromArgb(230, 126, 34);
                e.CellStyle.Font = new Font("Cairo", 10F, FontStyle.Bold);
            }
            else
            {
                e.CellStyle.ForeColor = Color.FromArgb(39, 174, 96);
            }
        }
        
        // Color code for paid amount
        if (_accountingGrid.Columns[e.ColumnIndex].Name == "PaidAmount")
        {
            decimal paid = sourceObject.PaidAmount;
            if (paid > 0)
            {
                e.CellStyle.ForeColor = Color.FromArgb(39, 174, 96);
            }
        }
        
        // Color code for payment status
        if (_accountingGrid.Columns[e.ColumnIndex].Name == "PaymentStatusDisplay")
        {
            switch (sourceObject.PaymentStatus)
            {
                case PaymentStatus.FullyPaid:
                    e.CellStyle.BackColor = Color.FromArgb(212, 237, 218);
                    e.CellStyle.ForeColor = Color.FromArgb(27, 94, 32);
                    break;
                case PaymentStatus.PartiallyPaid:
                    e.CellStyle.BackColor = Color.FromArgb(255, 243, 205);
                    e.CellStyle.ForeColor = Color.FromArgb(130, 88, 0);
                    break;
                case PaymentStatus.NotPaid:
                    e.CellStyle.BackColor = Color.FromArgb(248, 215, 218);
                    e.CellStyle.ForeColor = Color.FromArgb(114, 28, 36);
                    break;
            }
        }
    }
    
    private void UpdateButtonStates()
    {
        bool hasSelection = _bookingsGrid.SelectedRows.Count > 0;
        _recordPaymentButton.Enabled = hasSelection;
        _confirmBookingButton.Enabled = hasSelection;
        _cancelBookingButton.Enabled = hasSelection;
    }
    
    private void AddBooking_Click(object? sender, EventArgs e)
    {
        MessageBox.Show("سيتم إنشاء نموذج الحجز قريباً", "قريباً", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    
    private void RecordPayment_Click(object? sender, EventArgs e)
    {
        if (_bookingsGrid.SelectedRows.Count == 0) return;
        MessageBox.Show("سيتم إنشاء نموذج الدفع قريباً", "قريباً", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    
    private async void ConfirmBooking_Click(object? sender, EventArgs e)
    {
        if (_bookingsGrid.SelectedRows.Count == 0) return;
        
        var item = _bookingsGrid.SelectedRows[0].DataBoundItem;
        var booking = item?.GetType().GetProperty("SourceObject")?.GetValue(item) as TripBooking;
        if (booking == null) return;
        
        try
        {
            await _bookingService.ConfirmBookingAsync(booking.TripBookingId, _currentUserId);
            MessageBox.Show("تم تأكيد الحجز بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async void CancelBooking_Click(object? sender, EventArgs e)
    {
        if (_bookingsGrid.SelectedRows.Count == 0) return;
        
        var item = _bookingsGrid.SelectedRows[0].DataBoundItem;
        var booking = item?.GetType().GetProperty("SourceObject")?.GetValue(item) as TripBooking;
        if (booking == null) return;
        
        var reason = Microsoft.VisualBasic.Interaction.InputBox("سبب الإلغاء:", "إلغاء الحجز", "");
        if (string.IsNullOrEmpty(reason)) return;
        
        try
        {
            await _bookingService.CancelBookingAsync(booking.TripBookingId, reason, _currentUserId);
            MessageBox.Show("تم إلغاء الحجز بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void ViewTripDetails_Click(object? sender, EventArgs e)
    {
        try
        {
            var tripService = Program.ServiceProvider?.GetService<ITripService>();
            if (tripService == null)
            {
                MessageBox.Show("خطأ في تحميل خدمة الرحلات", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            var form = new TripDetailsForm(tripService, _tripId, _currentUserId);
            form.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في فتح تفاصيل الرحلة: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private Button CreateButton(string text, Color backColor, Point location, EventHandler onClick)
    {
        var button = new Button
        {
            Text = text, Font = new Font("Cairo", 10F, FontStyle.Bold), Size = new Size(150, 40),
            Location = location, BackColor = backColor, ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.Click += onClick;
        return button;
    }
    
    private string GetPaymentStatusDisplay(PaymentStatus status)
    {
        return status switch
        {
            PaymentStatus.NotPaid => "لم يدفع",
            PaymentStatus.PartiallyPaid => "دفع جزئي",
            PaymentStatus.FullyPaid => "مدفوع كامل",
            PaymentStatus.Refunded => "مسترد",
            _ => "غير محدد"
        };
    }
    
    private string GetBookingStatusDisplay(BookingStatus status)
    {
        return status switch
        {
            BookingStatus.Pending => "معلق",
            BookingStatus.Confirmed => "مؤكد",
            BookingStatus.Cancelled => "ملغي",
            BookingStatus.Completed => "مكتمل",
            _ => "غير محدد"
        };
    }
    
    /// <summary>
    /// تحديث التقرير المفصل للرحلة
    /// </summary>
    private async Task UpdateTripReportAsync()
    {
        try
        {
            // الحصول على بيانات الرحلة الكاملة
            var tripService = Program.ServiceProvider?.GetService<ITripService>();
            if (tripService == null) return;
            
            var trip = await tripService.GetTripByIdAsync(_tripId, includeDetails: true);
            if (trip == null) return;
            
            _trip = trip;
            
            // الحصول على الحجوزات
            var bookings = await _bookingService.GetBookingsByTripAsync(_tripId);
            
            // تحديث معلومات الرحلة في الـ Header
            _tripInfoLabel.Text = $"📍 {trip.TripName} | رقم الرحلة: {trip.TripNumber} | " +
                                 $"من {trip.StartDate:dd/MM/yyyy} إلى {trip.EndDate:dd/MM/yyyy} | " +
                                 $"الطاقة: {trip.TotalCapacity} | المحجوز: {trip.BookedSeats}";
            
            // الحصول على التبويب الثالث
            var reportTab = _mainTabControl.TabPages[2];
            var reportPanel = reportTab.Controls[0] as Panel;
            if (reportPanel == null) return;
            
            reportPanel.Controls.Clear();
            int y = 20;
            
            // ═══════════════════════════════════════════════════════════
            // القسم 1: معلومات الرحلة الأساسية
            // ═══════════════════════════════════════════════════════════
            AddReportSection(reportPanel, "📋 معلومات الرحلة الأساسية", ref y);
            AddReportItem(reportPanel, "رقم الرحلة:", trip.TripNumber, ref y);
            AddReportItem(reportPanel, "اسم الرحلة:", trip.TripName, ref y, bold: true);
            AddReportItem(reportPanel, "الوجهة:", trip.Destination, ref y);
            AddReportItem(reportPanel, "النوع:", GetTripTypeDisplay(trip.TripType), ref y);
            AddReportItem(reportPanel, "التواريخ:", $"من {trip.StartDate:dd/MM/yyyy} إلى {trip.EndDate:dd/MM/yyyy} ({trip.TotalDays} يوم)", ref y);
            AddReportItem(reportPanel, "الحالة:", GetTripStatusDisplay(trip.Status), ref y, 
                color: trip.Status == TripStatus.Completed ? ColorScheme.Success : ColorScheme.Warning);
            y += 20;
            
            // ═══════════════════════════════════════════════════════════
            // القسم 2: الطاقة والحجوزات
            // ═══════════════════════════════════════════════════════════
            AddReportSection(reportPanel, "👥 الطاقة والحجوزات", ref y);
            AddReportItem(reportPanel, "الطاقة الاستيعابية:", $"{trip.TotalCapacity} فرد", ref y, bold: true);
            AddReportItem(reportPanel, "المحجوز حالياً:", $"{trip.BookedSeats} فرد ({bookings.Count} حجز)", ref y);
            AddReportItem(reportPanel, "المتبقي:", $"{trip.AvailableSeats} مقعد", ref y);
            decimal occupancyRate = trip.TotalCapacity > 0 ? (trip.BookedSeats * 100.0m / trip.TotalCapacity) : 0;
            AddReportItem(reportPanel, "نسبة الإشغال:", $"{occupancyRate:F1}%", ref y, bold: true,
                color: occupancyRate >= 80 ? ColorScheme.Success : occupancyRate >= 50 ? ColorScheme.Warning : ColorScheme.Error);
            y += 20;
            
            // ═══════════════════════════════════════════════════════════
            // القسم 3: الإيرادات (من الحجوزات الفعلية)
            // ═══════════════════════════════════════════════════════════
            AddReportSection(reportPanel, "💰 الإيرادات من الحجوزات", ref y);
            decimal totalBookingRevenue = bookings.Sum(b => b.TotalAmount);
            decimal totalPaid = bookings.Sum(b => b.PaidAmount);
            decimal totalRemaining = bookings.Sum(b => b.RemainingAmount);
            decimal collectionRate = totalBookingRevenue > 0 ? (totalPaid / totalBookingRevenue) * 100 : 0;
            
            AddReportItem(reportPanel, "إجمالي إيرادات الحجوزات:", $"{totalBookingRevenue:N2} جنيه", ref y, bold: true, color: ColorScheme.Success);
            AddReportItem(reportPanel, "المحصّل:", $"{totalPaid:N2} جنيه ({collectionRate:F1}%)", ref y, color: Color.FromArgb(26, 188, 156));
            AddReportItem(reportPanel, "المتبقي:", $"{totalRemaining:N2} جنيه", ref y, color: ColorScheme.Warning);
            y += 20;
            
            // ═══════════════════════════════════════════════════════════
            // القسم 4: التكاليف المتوقعة
            // ═══════════════════════════════════════════════════════════
            AddReportSection(reportPanel, "💸 التكاليف المتوقعة", ref y);
            decimal transportCost = trip.Transportation.Sum(t => t.TotalCost);
            decimal accommodationCost = trip.Accommodations.Sum(a => a.TotalCost);
            decimal guideCost = trip.Guides.Sum(g => g.TotalCost);
            decimal programCost = trip.Programs.Sum(p => (p.VisitsCost * p.ParticipantsCount) + p.GuideCost);
            decimal optionalToursCost = trip.OptionalTours.Sum(o => o.TotalCost);
            decimal expensesCost = trip.Expenses.Sum(e => e.Amount);
            
            AddReportItem(reportPanel, "تكلفة البرنامج اليومي:", $"{programCost:N2} جنيه", ref y);
            AddReportItem(reportPanel, "تكلفة النقل:", $"{transportCost:N2} جنيه", ref y);
            AddReportItem(reportPanel, "تكلفة الإقامة:", $"{accommodationCost:N2} جنيه", ref y);
            AddReportItem(reportPanel, "تكلفة المرشد:", $"{guideCost:N2} جنيه", ref y);
            AddReportItem(reportPanel, "تكلفة الرحلات الاختيارية:", $"{optionalToursCost:N2} جنيه", ref y);
            AddReportItem(reportPanel, "مصاريف أخرى:", $"{expensesCost:N2} جنيه", ref y);
            AddReportItem(reportPanel, "إجمالي التكاليف:", $"{trip.TotalCost:N2} جنيه", ref y, bold: true, color: ColorScheme.Error);
            y += 20;
            
            // ═══════════════════════════════════════════════════════════
            // القسم 5: الأرباح والخسائر
            // ═══════════════════════════════════════════════════════════
            AddReportSection(reportPanel, "📈 الأرباح والخسائر", ref y);
            
            // الربح الفعلي = إيرادات الحجوزات - التكاليف
            decimal actualProfit = totalBookingRevenue - trip.TotalCost;
            decimal profitMargin = totalBookingRevenue > 0 ? (actualProfit / totalBookingRevenue) * 100 : 0;
            
            // الربح المتوقع لو اكتملت الطاقة
            decimal expectedRevenue = trip.TotalCapacity * trip.SellingPricePerPersonInEGP;
            decimal expectedProfit = expectedRevenue - trip.TotalCost;
            
            AddReportItem(reportPanel, "الإيرادات المتوقعة (طاقة كاملة):", $"{expectedRevenue:N2} جنيه", ref y);
            AddReportItem(reportPanel, "الربح المتوقع (طاقة كاملة):", $"{expectedProfit:N2} جنيه", ref y,
                color: expectedProfit > 0 ? ColorScheme.Success : ColorScheme.Error);
            
            y += 10;
            AddReportItem(reportPanel, "الإيرادات الفعلية (من الحجوزات):", $"{totalBookingRevenue:N2} جنيه", ref y, bold: true);
            AddReportItem(reportPanel, "الربح الفعلي (من الحجوزات):", $"{actualProfit:N2} جنيه", ref y, bold: true,
                color: actualProfit > 0 ? ColorScheme.Success : ColorScheme.Error);
            AddReportItem(reportPanel, "هامش الربح:", $"{profitMargin:F1}%", ref y);
            
            y += 10;
            // حساب الفرق
            decimal profitDifference = actualProfit - expectedProfit;
            string differenceText = profitDifference >= 0 ? 
                $"+{profitDifference:N2} جنيه (زيادة)" : 
                $"{profitDifference:N2} جنيه (نقص)";
            AddReportItem(reportPanel, "الفرق:", differenceText, ref y,
                color: profitDifference >= 0 ? ColorScheme.Success : ColorScheme.Warning);
            
            y += 20;
            
            // ═══════════════════════════════════════════════════════════
            // القسم 6: تفاصيل إضافية
            // ═══════════════════════════════════════════════════════════
            AddReportSection(reportPanel, "ℹ️ تفاصيل إضافية", ref y);
            AddReportItem(reportPanel, "عدد أيام البرنامج:", $"{trip.Programs.Count} يوم", ref y);
            AddReportItem(reportPanel, "وسائل النقل:", $"{trip.Transportation.Count} مركبة", ref y);
            AddReportItem(reportPanel, "أماكن الإقامة:", $"{trip.Accommodations.Count} فندق", ref y);
            AddReportItem(reportPanel, "المرشدين:", $"{trip.Guides.Count} مرشد", ref y);
            AddReportItem(reportPanel, "الرحلات الاختيارية:", $"{trip.OptionalTours.Count} رحلة", ref y);
            AddReportItem(reportPanel, "مقفولة من الحجوزات:", trip.IsLockedForTrips ? "🔒 نعم" : "🔓 لا", ref y,
                color: trip.IsLockedForTrips ? ColorScheme.Error : ColorScheme.Success);
            
            y += 30;
            
            // ═══════════════════════════════════════════════════════════
            // القسم 7: البرنامج اليومي
            // ═══════════════════════════════════════════════════════════
            if (trip.Programs.Any())
            {
                AddReportSection(reportPanel, "📅 البرنامج اليومي للرحلة", ref y);
                foreach (var program in trip.Programs.OrderBy(p => p.DayNumber))
                {
                    string dayTitle = $"اليوم {program.DayNumber}";
                    if (!string.IsNullOrEmpty(program.DayTitle))
                        dayTitle += $" - {program.DayTitle}";
                    
                    AddReportItem(reportPanel, dayTitle + ":", "", ref y, bold: true, color: ColorScheme.Primary);
                    
                    if (!string.IsNullOrEmpty(program.Activities))
                        AddReportItem(reportPanel, "  الأنشطة:", program.Activities, ref y);
                    
                    AddReportItem(reportPanel, "  عدد المشاركين:", $"{program.ParticipantsCount} فرد", ref y);
                    AddReportItem(reportPanel, "  تكلفة الزيارات:", $"{program.VisitsCost:N2} جنيه/فرد", ref y);
                    AddReportItem(reportPanel, "  تكلفة المرشد:", $"{program.GuideCost:N2} جنيه", ref y);
                    
                    decimal totalDayCost = (program.VisitsCost * program.ParticipantsCount) + program.GuideCost;
                    AddReportItem(reportPanel, "  الإجمالي:", $"{totalDayCost:N2} جنيه", ref y, bold: true, color: ColorScheme.Warning);
                    
                    y += 10;
                }
                y += 20;
            }
            
            // ═══════════════════════════════════════════════════════════
            // القسم 8: وسائل النقل
            // ═══════════════════════════════════════════════════════════
            if (trip.Transportation.Any())
            {
                AddReportSection(reportPanel, "🚌 وسائل النقل", ref y);
                int transportIndex = 1;
                foreach (var transport in trip.Transportation)
                {
                    AddReportItem(reportPanel, $"النقل {transportIndex}:", transport.Type.ToString(), ref y, bold: true);
                    
                    if (!string.IsNullOrEmpty(transport.VehicleModel))
                        AddReportItem(reportPanel, "  الموديل:", transport.VehicleModel, ref y);
                    
                    AddReportItem(reportPanel, "  السعة:", $"{transport.TotalSeats} فرد", ref y);
                    AddReportItem(reportPanel, "  التكلفة الإجمالية:", $"{transport.TotalCost:N2} جنيه", ref y, bold: true, color: ColorScheme.Warning);
                    
                    if (!string.IsNullOrEmpty(transport.Notes))
                        AddReportItem(reportPanel, "  ملاحظات:", transport.Notes, ref y);
                    
                    y += 10;
                    transportIndex++;
                }
                y += 20;
            }
            
            // ═══════════════════════════════════════════════════════════
            // القسم 9: أماكن الإقامة
            // ═══════════════════════════════════════════════════════════
            if (trip.Accommodations.Any())
            {
                AddReportSection(reportPanel, "🏨 أماكن الإقامة", ref y);
                int hotelIndex = 1;
                foreach (var accommodation in trip.Accommodations)
                {
                    AddReportItem(reportPanel, $"الفندق {hotelIndex}:", accommodation.HotelName, ref y, bold: true);
                    AddReportItem(reportPanel, "  الموقع:", accommodation.Location ?? "غير محدد", ref y);
                    AddReportItem(reportPanel, "  نوع الغرفة:", accommodation.RoomType.ToString(), ref y);
                    AddReportItem(reportPanel, "  عدد الليالي:", $"{accommodation.NumberOfNights} ليلة", ref y);
                    AddReportItem(reportPanel, "  عدد الغرف:", $"{accommodation.NumberOfRooms} غرفة", ref y);
                    AddReportItem(reportPanel, "  التكلفة الإجمالية:", $"{accommodation.TotalCost:N2} جنيه", ref y, bold: true, color: ColorScheme.Warning);
                    
                    if (!string.IsNullOrEmpty(accommodation.Notes))
                        AddReportItem(reportPanel, "  ملاحظات:", accommodation.Notes, ref y);
                    
                    y += 10;
                    hotelIndex++;
                }
                y += 20;
            }
            
            // ═══════════════════════════════════════════════════════════
            // القسم 10: المرشدين
            // ═══════════════════════════════════════════════════════════
            if (trip.Guides.Any())
            {
                AddReportSection(reportPanel, "👨‍🏫 المرشدين", ref y);
                int guideIndex = 1;
                foreach (var guide in trip.Guides)
                {
                    AddReportItem(reportPanel, $"المرشد {guideIndex}:", guide.GuideName, ref y, bold: true);
                    AddReportItem(reportPanel, "  رقم الهاتف:", guide.Phone ?? "غير محدد", ref y);
                    AddReportItem(reportPanel, "  الأساسي:", $"{guide.BaseFee:N2} جنيه", ref y);
                    AddReportItem(reportPanel, "  العمولة:", $"{guide.CommissionAmount:N2} جنيه", ref y);
                    AddReportItem(reportPanel, "  التكلفة الإجمالية:", $"{guide.TotalCost:N2} جنيه", ref y, bold: true, color: ColorScheme.Warning);
                    
                    if (!string.IsNullOrEmpty(guide.Notes))
                        AddReportItem(reportPanel, "  ملاحظات:", guide.Notes, ref y);
                    
                    y += 10;
                    guideIndex++;
                }
                y += 20;
            }
            
            // ═══════════════════════════════════════════════════════════
            // القسم 11: الرحلات الاختيارية
            // ═══════════════════════════════════════════════════════════
            if (trip.OptionalTours.Any())
            {
                AddReportSection(reportPanel, "🎯 الرحلات الاختيارية", ref y);
                int tourIndex = 1;
                foreach (var tour in trip.OptionalTours)
                {
                    AddReportItem(reportPanel, $"الرحلة {tourIndex}:", tour.TourName, ref y, bold: true);
                    
                    if (!string.IsNullOrEmpty(tour.Description))
                        AddReportItem(reportPanel, "  الوصف:", tour.Description, ref y);
                    
                    AddReportItem(reportPanel, "  التكلفة للفرد:", $"{tour.SellingPrice:N2} جنيه/فرد", ref y);
                    AddReportItem(reportPanel, "  المشتركين:", $"{tour.ParticipantsCount} فرد", ref y);
                    AddReportItem(reportPanel, "  التكلفة الإجمالية:", $"{tour.TotalCost:N2} جنيه", ref y, bold: true, color: ColorScheme.Warning);
                    
                    y += 10;
                    tourIndex++;
                }
                y += 20;
            }
            
            // ═══════════════════════════════════════════════════════════
            // القسم 12: المصاريف الأخرى
            // ═══════════════════════════════════════════════════════════
            if (trip.Expenses.Any())
            {
                AddReportSection(reportPanel, "💵 المصاريف الأخرى", ref y);
                int expenseIndex = 1;
                foreach (var expense in trip.Expenses)
                {
                    string expenseTitle = !string.IsNullOrEmpty(expense.Description) ? expense.Description : $"مصروف {expenseIndex}";
                    AddReportItem(reportPanel, expenseTitle + ":", "", ref y, bold: true);
                    AddReportItem(reportPanel, "  النوع:", expense.ExpenseType, ref y);
                    AddReportItem(reportPanel, "  المبلغ:", $"{expense.Amount:N2} جنيه", ref y, bold: true, color: ColorScheme.Warning);
                    
                    if (!string.IsNullOrEmpty(expense.Notes))
                        AddReportItem(reportPanel, "  ملاحظات:", expense.Notes, ref y);
                    
                    y += 10;
                    expenseIndex++;
                }
                y += 20;
            }
            
            // ═══════════════════════════════════════════════════════════
            // القسم 13: قائمة الحجوزات
            // ═══════════════════════════════════════════════════════════
            if (bookings.Any())
            {
                AddReportSection(reportPanel, "📋 قائمة الحجوزات", ref y);
                int bookingIndex = 1;
                foreach (var booking in bookings.OrderByDescending(b => b.BookingDate))
                {
                    string customerName = booking.Customer?.CustomerName ?? "غير محدد";
                    AddReportItem(reportPanel, $"الحجز {bookingIndex} - {customerName}:", "", ref y, bold: true, color: ColorScheme.Primary);
                    AddReportItem(reportPanel, "  رقم الحجز:", booking.BookingNumber, ref y);
                    AddReportItem(reportPanel, "  تاريخ الحجز:", booking.BookingDate.ToString("dd/MM/yyyy"), ref y);
                    AddReportItem(reportPanel, "  عدد الأفراد:", $"{booking.NumberOfPersons} فرد", ref y);
                    AddReportItem(reportPanel, "  السعر للفرد:", $"{booking.PricePerPerson:N2} جنيه", ref y);
                    AddReportItem(reportPanel, "  المبلغ الإجمالي:", $"{booking.TotalAmount:N2} جنيه", ref y, bold: true);
                    AddReportItem(reportPanel, "  المدفوع:", $"{booking.PaidAmount:N2} جنيه", ref y, color: ColorScheme.Success);
                    AddReportItem(reportPanel, "  المتبقي:", $"{booking.RemainingAmount:N2} جنيه", ref y, 
                        color: booking.RemainingAmount > 0 ? ColorScheme.Warning : ColorScheme.Success);
                    AddReportItem(reportPanel, "  حالة الدفع:", GetPaymentStatusDisplay(booking.PaymentStatus), ref y);
                    AddReportItem(reportPanel, "  حالة الحجز:", GetBookingStatusDisplay(booking.Status), ref y);
                    
                    // عرض الدفعات إذا وُجدت
                    if (booking.Payments != null && booking.Payments.Any())
                    {
                        AddReportItem(reportPanel, "  الدفعات:", $"{booking.Payments.Count} دفعة", ref y, color: ColorScheme.Info);
                        int paymentNum = 1;
                        foreach (var payment in booking.Payments.OrderBy(p => p.PaymentDate))
                        {
                            string paymentInfo = $"    دفعة {paymentNum}: {payment.Amount:N2} جنيه - {payment.PaymentDate:dd/MM/yyyy} - {payment.PaymentMethod}";
                            AddReportItem(reportPanel, "", paymentInfo, ref y);
                            paymentNum++;
                        }
                    }
                    
                    y += 15;
                    bookingIndex++;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل التقرير: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void AddReportSection(Panel panel, string title, ref int y)
    {
        var label = new Label
        {
            Text = title,
            Font = new Font("Cairo", 13F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(20, y),
            BackColor = Color.FromArgb(240, 248, 255),
            Padding = new Padding(10, 5, 10, 5)
        };
        panel.Controls.Add(label);
        y += 45;
    }
    
    private void AddReportItem(Panel panel, string label, string value, ref int y, bool bold = false, Color? color = null)
    {
        var lblLabel = new Label
        {
            Text = label,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(40, y),
            ForeColor = Color.FromArgb(52, 73, 94)
        };
        panel.Controls.Add(lblLabel);
        
        var lblValue = new Label
        {
            Text = value,
            Font = new Font("Cairo", 10F, bold ? FontStyle.Bold : FontStyle.Regular),
            AutoSize = true,
            Location = new Point(350, y),
            ForeColor = color ?? Color.FromArgb(44, 62, 80)
        };
        panel.Controls.Add(lblValue);
        y += 35;
    }
    
    private string GetTripTypeDisplay(TripType type)
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
    
    private string GetTripStatusDisplay(TripStatus status)
    {
        return status switch
        {
            TripStatus.Draft => "مسودة",
            TripStatus.Unconfirmed => "غير مؤكد",
            TripStatus.Confirmed => "مؤكد",
            TripStatus.InProgress => "قيد التنفيذ",
            TripStatus.Completed => "مكتمل",
            TripStatus.Cancelled => "ملغي",
            _ => "غير محدد"
        };
    }
}
