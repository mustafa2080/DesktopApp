using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Presentation.Forms;

/// <summary>
/// نموذج التفاصيل المالية الكاملة للرحلة
/// </summary>
public partial class TripFinancialDetailsForm : Form
{
    private readonly Trip _trip;
    private readonly ITripService _tripService;
    
    private TabControl _tabControl = null!;
    private DataGridView _revenueGrid = null!;
    private DataGridView _costGrid = null!;
    private DataGridView _bookingsGrid = null!;
    
    public TripFinancialDetailsForm(Trip trip, ITripService tripService)
    {
        _trip = trip;
        _tripService = tripService;
        
        // ✅ إعادة حساب التكلفة الإجمالية للتأكد من دقة البيانات
        _trip.CalculateTotalCost();
        
        InitializeComponent();
        SetupForm();
        InitializeCustomControls();
        LoadData();
    }
    
    private void SetupForm()
    {
        this.Text = $"التفاصيل المالية - {_trip.TripName}";
        this.Size = new Size(1400, 800);
        this.StartPosition = FormStartPosition.CenterParent;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = Color.White;
        this.Font = new Font("Cairo", 10F);
    }
    
    private void InitializeCustomControls()
    {
        // Header Panel
        Panel headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 150,
            BackColor = ColorScheme.Primary,
            Padding = new Padding(30)
        };
        
        Label titleLabel = new Label
        {
            Text = _trip.TripName,
            Font = new Font("Cairo", 18F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(30, 20)
        };
        headerPanel.Controls.Add(titleLabel);
        
        Label tripNumberLabel = new Label
        {
            Text = $"رقم الرحلة: {_trip.TripNumber}",
            Font = new Font("Cairo", 11F),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(30, 60)
        };
        headerPanel.Controls.Add(tripNumberLabel);
        
        Label datesLabel = new Label
        {
            Text = $"الفترة: {_trip.StartDate:yyyy-MM-dd} إلى {_trip.EndDate:yyyy-MM-dd} ({_trip.TotalDays} يوم)",
            Font = new Font("Cairo", 10F),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(30, 90)
        };
        headerPanel.Controls.Add(datesLabel);
        
        // Summary Cards
        CreateSummaryCard("💰 إجمالي الإيرادات", $"{_trip.ExpectedRevenue:N2} جنيه", 
            ColorScheme.Success, new Point(900, 20), headerPanel);
        CreateSummaryCard("💸 إجمالي التكاليف", $"{_trip.TotalCost:N2} جنيه", 
            ColorScheme.Error, new Point(900, 75), headerPanel);
        CreateSummaryCard("📊 صافي الربح", $"{_trip.ActualProfit:N2} جنيه", 
            _trip.ActualProfit >= 0 ? Color.Gold : Color.OrangeRed, new Point(1150, 20), headerPanel);
        CreateSummaryCard("📈 هامش الربح", $"{_trip.ProfitMargin:N2}%", 
            Color.White, new Point(1150, 75), headerPanel);
        
        // Tab Control - نضيفه قبل الـ Header عشان يظهر تحته
        _tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Padding = new Point(10, 10)
        };
        
        // Tab 1: Revenue Breakdown
        var revenueTab = new TabPage("💰 تفاصيل الإيرادات");
        _revenueGrid = CreateRevenueGrid();
        revenueTab.Controls.Add(_revenueGrid);
        _tabControl.TabPages.Add(revenueTab);
        
        // Tab 2: Cost Breakdown
        var costTab = new TabPage("💸 تفاصيل التكاليف");
        _costGrid = CreateCostGrid();
        costTab.Controls.Add(_costGrid);
        _tabControl.TabPages.Add(costTab);
        
        // Tab 3: Bookings
        var bookingsTab = new TabPage("👥 الحجوزات");
        _bookingsGrid = CreateBookingsGrid();
        bookingsTab.Controls.Add(_bookingsGrid);
        _tabControl.TabPages.Add(bookingsTab);
        
        // ✅ الترتيب مهم: TabControl الأول (Fill)، بعدين Header (Top)
        this.Controls.Add(_tabControl);
        this.Controls.Add(headerPanel);
    }
    
    private void CreateSummaryCard(string title, string value, Color color, Point location, Panel parent)
    {
        Label lblTitle = new Label
        {
            Text = title,
            Font = new Font("Cairo", 9F),
            ForeColor = Color.FromArgb(230, 230, 230),
            AutoSize = true,
            Location = location
        };
        parent.Controls.Add(lblTitle);
        
        Label lblValue = new Label
        {
            Text = value,
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = color,
            AutoSize = true,
            Location = new Point(location.X, location.Y + 25)
        };
        parent.Controls.Add(lblValue);
    }
    
    private DataGridView CreateRevenueGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowTemplate = { Height = 40 },
            EnableHeadersVisualStyles = false,
            AllowUserToResizeRows = false,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        };
        
        grid.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Success;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        grid.ColumnHeadersHeight = 45;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
        
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "البند",
            Name = "Item",
            Width = 200
        });
        
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "الوصف",
            Name = "Description",
            Width = 600
        });
        
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "المبلغ (جنيه)",
            Name = "Amount",
            Width = 200,
            DefaultCellStyle = { 
                Format = "N2",
                Alignment = DataGridViewContentAlignment.MiddleRight,
                Font = new Font("Cairo", 10F, FontStyle.Bold)
            }
        });
        
        return grid;
    }
    
    private DataGridView CreateCostGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowTemplate = { Height = 40 },
            EnableHeadersVisualStyles = false,
            AllowUserToResizeRows = false,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        };
        
        grid.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Error;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        grid.ColumnHeadersHeight = 45;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
        
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "البند",
            Name = "Item",
            Width = 150
        });
        
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "الوصف",
            Name = "Description",
            Width = 500
        });
        
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "الكمية/العدد",
            Name = "Quantity",
            Width = 150,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
        });
        
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "المبلغ (جنيه)",
            Name = "Amount",
            Width = 200,
            DefaultCellStyle = { 
                Format = "N2",
                Alignment = DataGridViewContentAlignment.MiddleRight,
                Font = new Font("Cairo", 10F, FontStyle.Bold)
            }
        });
        
        return grid;
    }
    
    private DataGridView CreateBookingsGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowTemplate = { Height = 40 },
            EnableHeadersVisualStyles = false,
            AllowUserToResizeRows = false,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        };
        
        grid.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Primary;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        grid.ColumnHeadersHeight = 45;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
        
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "رقم الحجز",
            Name = "BookingNumber",
            Width = 120
        });
        
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "العميل",
            Name = "CustomerName",
            Width = 250
        });
        
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "عدد الأفراد",
            Name = "NumberOfPersons",
            Width = 100,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
        });
        
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "الإجمالي",
            Name = "TotalAmount",
            Width = 150,
            DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
        });
        
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "المدفوع",
            Name = "PaidAmount",
            Width = 150,
            DefaultCellStyle = { 
                Format = "N2", 
                Alignment = DataGridViewContentAlignment.MiddleRight,
                ForeColor = ColorScheme.Success
            }
        });
        
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "المتبقي",
            Name = "RemainingAmount",
            Width = 150,
            DefaultCellStyle = { 
                Format = "N2", 
                Alignment = DataGridViewContentAlignment.MiddleRight,
                ForeColor = ColorScheme.Error
            }
        });
        
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "حالة الدفع",
            Name = "PaymentStatus",
            Width = 130,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
        });
        
        return grid;
    }
    
    private void LoadData()
    {
        LoadRevenueData();
        LoadCostData();
        LoadBookingsData();
    }
    
    private void LoadRevenueData()
    {
        _revenueGrid.Rows.Clear();
        
        // Revenue from bookings
        _revenueGrid.Rows.Add(
            "إيرادات الحجوزات",
            $"{_trip.BookedSeats} حجز × {_trip.SellingPricePerPersonInEGP:N2} جنيه",
            _trip.ExpectedRevenue
        );
        
        // Optional tours revenue
        var optionalToursRevenue = _trip.Bookings
            .SelectMany(b => b.OptionalTourBookings)
            .Sum(ot => ot.PricePerPerson * ot.NumberOfParticipants);
        
        if (optionalToursRevenue > 0)
        {
            _revenueGrid.Rows.Add(
                "إيرادات الرحلات الاختيارية",
                "إجمالي مبيعات الرحلات الاختيارية",
                optionalToursRevenue
            );
        }
        
        // Total row
        var totalRow = _revenueGrid.Rows.Add(
            "الإجمالي",
            "",
            _trip.ExpectedRevenue + optionalToursRevenue
        );
        _revenueGrid.Rows[totalRow].DefaultCellStyle.BackColor = Color.FromArgb(230, 247, 255);
        _revenueGrid.Rows[totalRow].DefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
    }
    
    private void LoadCostData()
    {
        _costGrid.Rows.Clear();
        
        // ✅ Daily Program costs (المزارات + المرشد + إكرامية السواق)
        if (_trip.Programs.Any())
        {
            foreach (var program in _trip.Programs)
            {
                // حساب التكلفة الإجمالية للبرنامج اليومي
                decimal programTotalCost = (program.VisitsCost * program.ParticipantsCount) + program.GuideCost + program.DriverTip;
                
                string description = $"اليوم {program.DayNumber}: {program.DayTitle}";
                if (program.VisitsCost > 0)
                    description += $" - مزارات: {program.VisitsCost:N2} × {program.ParticipantsCount} فرد";
                if (program.GuideCost > 0)
                    description += $" + مرشد: {program.GuideCost:N2}";
                if (program.DriverTip > 0)
                    description += $" + إكرامية: {program.DriverTip:N2}";
                
                _costGrid.Rows.Add(
                    "📅 برنامج يومي",
                    description,
                    program.ParticipantsCount.ToString(),
                    programTotalCost
                );
            }
        }
        
        // Transportation costs
        if (_trip.Transportation.Any())
        {
            foreach (var transport in _trip.Transportation)
            {
                string transportType = transport.Type switch
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
                
                // ✅ عرض اسم المزار ورقم اليوم في الوصف
                string description = "";
                
                if (!string.IsNullOrEmpty(transport.VisitName))
                    description = $"{transport.VisitName}";
                
                if (transport.ProgramDayNumber.HasValue)
                {
                    if (!string.IsNullOrEmpty(description))
                        description += $" (اليوم {transport.ProgramDayNumber})";
                    else
                        description = $"اليوم {transport.ProgramDayNumber}";
                }
                
                description += $" - {transportType}";
                
                if (!string.IsNullOrEmpty(transport.VehicleModel))
                    description += $" ({transport.VehicleModel})";
                
                // إضافة تفاصيل الإكراميات
                if (transport.TourLeaderTip > 0 || transport.DriverTip > 0)
                {
                    description += " [";
                    if (transport.TourLeaderTip > 0)
                        description += $"تور ليدر: {transport.TourLeaderTip:N2}";
                    if (transport.TourLeaderTip > 0 && transport.DriverTip > 0)
                        description += " + ";
                    if (transport.DriverTip > 0)
                        description += $"سواق: {transport.DriverTip:N2}";
                    description += "]";
                }
                
                _costGrid.Rows.Add(
                    "🚗 نقل",
                    description,
                    $"{transport.NumberOfVehicles} مركبة",
                    transport.TotalCost
                );
            }
        }
        
        // Accommodation costs
        if (_trip.Accommodations.Any())
        {
            foreach (var accommodation in _trip.Accommodations)
            {
                string description = $"{accommodation.HotelName} - {accommodation.RoomType}";
                
                // إضافة تفاصيل الإكراميات
                if (accommodation.GuideCost > 0 || accommodation.DriverTip > 0)
                {
                    description += " (";
                    if (accommodation.GuideCost > 0)
                        description += $"مرشد: {accommodation.GuideCost:N2}";
                    if (accommodation.GuideCost > 0 && accommodation.DriverTip > 0)
                        description += " + ";
                    if (accommodation.DriverTip > 0)
                        description += $"سواق: {accommodation.DriverTip:N2}";
                    description += ")";
                }
                
                _costGrid.Rows.Add(
                    "🏨 إقامة",
                    description,
                    $"{accommodation.NumberOfNights} ليالي",
                    accommodation.TotalCost
                );
            }
        }
        
        // Guide costs
        if (_trip.Guides.Any())
        {
            foreach (var guide in _trip.Guides)
            {
                string description = guide.GuideName;
                
                // إضافة تفاصيل الإكرامية
                if (guide.DriverTip > 0)
                    description += $" (إكرامية سواق: {guide.DriverTip:N2})";
                
                _costGrid.Rows.Add(
                    "👨‍🏫 مرشد",
                    description,
                    "1",
                    guide.TotalCost
                );
            }
        }
        
        // Optional tours costs
        if (_trip.OptionalTours.Any())
        {
            foreach (var tour in _trip.OptionalTours)
            {
                _costGrid.Rows.Add(
                    "🎯 رحلة اختيارية",
                    tour.TourName,
                    "1",
                    tour.TotalCost
                );
            }
        }
        
        // Other expenses
        if (_trip.Expenses.Any())
        {
            foreach (var expense in _trip.Expenses)
            {
                _costGrid.Rows.Add(
                    "📝 مصروف",
                    $"{expense.ExpenseType} - {expense.Description}",
                    "1",
                    expense.Amount
                );
            }
        }
        
        // Total row
        var totalRow = _costGrid.Rows.Add(
            "الإجمالي",
            "",
            "",
            _trip.TotalCost
        );
        _costGrid.Rows[totalRow].DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 235);
        _costGrid.Rows[totalRow].DefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
    }
    
    private void LoadBookingsData()
    {
        _bookingsGrid.Rows.Clear();
        
        foreach (var booking in _trip.Bookings.OrderBy(b => b.BookingDate))
        {
            _bookingsGrid.Rows.Add(
                booking.BookingNumber,
                booking.Customer?.CustomerName ?? "غير محدد",
                booking.NumberOfPersons,
                booking.TotalAmount,
                booking.PaidAmount,
                booking.RemainingAmount,
                GetPaymentStatusText(booking.PaymentStatus)
            );
        }
        
        // Summary row
        if (_trip.Bookings.Any())
        {
            var summaryRow = _bookingsGrid.Rows.Add(
                "الإجمالي",
                $"{_trip.Bookings.Count} حجز",
                _trip.Bookings.Sum(b => b.NumberOfPersons),
                _trip.Bookings.Sum(b => b.TotalAmount),
                _trip.Bookings.Sum(b => b.PaidAmount),
                _trip.Bookings.Sum(b => b.RemainingAmount),
                ""
            );
            _bookingsGrid.Rows[summaryRow].DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            _bookingsGrid.Rows[summaryRow].DefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        }
    }
    
    private string GetPaymentStatusText(PaymentStatus status)
    {
        return status switch
        {
            PaymentStatus.NotPaid => "⏳ لم يدفع",
            PaymentStatus.PartiallyPaid => "⚠️ جزئي",
            PaymentStatus.FullyPaid => "✅ مدفوع",
            PaymentStatus.Refunded => "🔄 مسترد",
            _ => "غير محدد"
        };
    }
}
