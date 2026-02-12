using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Presentation;

namespace GraceWay.AccountingSystem.Presentation.Forms;

/// <summary>
/// تقرير مالي شامل للرحلة
/// </summary>
public partial class TripFinancialReportForm : Form
{
    private readonly ITripService _tripService;
    private readonly int _tripId;
    private Trip _trip = null!;
    
    // Controls
    private Panel _headerPanel = null!;
    private Label _titleLabel = null!;
    private Panel _contentPanel = null!;
    private Button _printButton = null!;
    private Button _exportButton = null!;
    private Button _closeButton = null!;
    
    public TripFinancialReportForm(ITripService tripService, int tripId)
    {
        _tripService = tripService;
        _tripId = tripId;
        
        InitializeComponent();
        SetupForm();
        InitializeCustomControls();
        _ = LoadDataAsync();
    }
    
    private void SetupForm()
    {
        this.Text = "التقرير المالي";
        this.Size = new Size(900, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
    }
    
    private void InitializeCustomControls()
    {
        // Header
        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 100,
            BackColor = ColorScheme.Primary,
            Padding = new Padding(20)
        };
        
        _titleLabel = new Label
        {
            Text = "📊 التقرير المالي الشامل",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 30)
        };
        _headerPanel.Controls.Add(_titleLabel);
        
        // Content Panel
        _contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(30),
            AutoScroll = true
        };
        
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
        
        _exportButton = new Button
        {
            Text = "📄 تصدير Excel",
            Size = new Size(140, 40),
            Location = new Point(600, 15),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _exportButton.FlatAppearance.BorderSize = 0;
        _exportButton.Click += ExportButton_Click;
        footerPanel.Controls.Add(_exportButton);
        
        _printButton = new Button
        {
            Text = "🖨️ طباعة",
            Size = new Size(120, 40),
            Location = new Point(750, 15),
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _printButton.FlatAppearance.BorderSize = 0;
        _printButton.Click += PrintButton_Click;
        footerPanel.Controls.Add(_printButton);
        
        this.Controls.Add(_contentPanel);
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
            
            BuildReport();
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
    
    private void BuildReport()
    {
        _contentPanel.Controls.Clear();
        int y = 20;
        
        // معلومات الرحلة
        AddReportHeader(_contentPanel, "📋 معلومات الرحلة", ref y);
        AddReportLine(_contentPanel, "اسم الرحلة:", _trip.TripName, ref y);
        AddReportLine(_contentPanel, "رقم الرحلة:", _trip.TripNumber, ref y);
        AddReportLine(_contentPanel, "النوع:", GetTripTypeText(_trip.TripType), ref y);
        AddReportLine(_contentPanel, "التاريخ:", $"{_trip.StartDate:yyyy-MM-dd} إلى {_trip.EndDate:yyyy-MM-dd}", ref y);
        AddReportLine(_contentPanel, "المدة:", $"{(_trip.EndDate - _trip.StartDate).Days + 1} يوم", ref y);
        y += 20;
        
        // الأعداد والإشغال
        AddReportHeader(_contentPanel, "👥 الأعداد والإشغال", ref y);
        AddReportLine(_contentPanel, "الطاقة الاستيعابية:", $"{_trip.TotalCapacity} فرد", ref y);
        AddReportLine(_contentPanel, "المحجوز:", $"{_trip.BookedSeats} فرد", ref y);
        AddReportLine(_contentPanel, "المتبقي:", $"{_trip.AvailableSeats} مقعد", ref y);
        AddReportLine(_contentPanel, "نسبة الإشغال:", $"{(_trip.TotalCapacity > 0 ? (_trip.BookedSeats * 100.0 / _trip.TotalCapacity) : 0):F1}%", ref y);
        y += 20;
        
        // الإيرادات
        AddReportHeader(_contentPanel, "💰 الإيرادات", ref y);
        AddReportLine(_contentPanel, "السعر للفرد:", $"{_trip.SellingPricePerPerson:N2} جنيه", ref y);
        AddReportLine(_contentPanel, "إجمالي الإيرادات المتوقعة:", $"{_trip.ExpectedRevenue:N2} جنيه", ref y, bold: true);
        
        decimal optionalToursRevenue = _trip.OptionalTours.Sum(t => t.TotalRevenue);
        if (optionalToursRevenue > 0)
        {
            AddReportLine(_contentPanel, "إيرادات الرحلات الاختيارية:", $"{optionalToursRevenue:N2} جنيه", ref y);
        }
        y += 20;
        
        // التكاليف
        AddReportHeader(_contentPanel, "💸 التكاليف", ref y);
        
        decimal transportCost = _trip.Transportation.Sum(t => t.TotalCost);
        decimal accommodationCost = _trip.Accommodations.Sum(a => a.TotalCost);
        decimal guideCost = _trip.Guides.Sum(g => g.TotalCost);
        decimal expensesCost = _trip.Expenses.Sum(e => e.Amount);
        decimal optionalToursCost = _trip.OptionalTours.Sum(t => t.TotalCost);
        
        AddReportLine(_contentPanel, "النقل:", $"{transportCost:N2} جنيه", ref y);
        AddReportLine(_contentPanel, "الإقامة:", $"{accommodationCost:N2} جنيه", ref y);
        AddReportLine(_contentPanel, "المرشد:", $"{guideCost:N2} جنيه", ref y);
        AddReportLine(_contentPanel, "مصاريف أخرى:", $"{expensesCost:N2} جنيه", ref y);
        if (optionalToursCost > 0)
        {
            AddReportLine(_contentPanel, "الرحلات الاختيارية:", $"{optionalToursCost:N2} جنيه", ref y);
        }
        
        AddHorizontalLine(_contentPanel, ref y);
        AddReportLine(_contentPanel, "إجمالي التكاليف:", $"{_trip.TotalCost:N2} جنيه", ref y, bold: true);
        y += 20;
        
        // الأرباح
        AddReportHeader(_contentPanel, "📈 الأرباح", ref y);
        
        decimal totalRevenue = _trip.ExpectedRevenue + optionalToursRevenue;
        decimal totalCost = _trip.TotalCost + optionalToursCost;
        decimal netProfit = totalRevenue - totalCost;
        decimal profitMargin = totalRevenue > 0 ? (netProfit / totalRevenue * 100) : 0;
        
        AddReportLine(_contentPanel, "إجمالي الإيرادات:", $"{totalRevenue:N2} جنيه", ref y);
        AddReportLine(_contentPanel, "إجمالي التكاليف:", $"{totalCost:N2} جنيه", ref y);
        AddHorizontalLine(_contentPanel, ref y);
        AddReportLine(_contentPanel, "صافي الربح:", $"{netProfit:N2} جنيه", ref y, bold: true,
            color: netProfit > 0 ? ColorScheme.Success : ColorScheme.Error);
        AddReportLine(_contentPanel, "هامش الربح:", $"{profitMargin:F1}%", ref y);
        AddReportLine(_contentPanel, "الربح لكل فرد:", $"{(_trip.BookedSeats > 0 ? netProfit / _trip.BookedSeats : 0):N2} جنيه", ref y);
        y += 20;
        
        // تفصيل النقل
        if (_trip.Transportation.Any())
        {
            AddReportHeader(_contentPanel, "🚗 تفصيل النقل", ref y);
            foreach (var transport in _trip.Transportation)
            {
                string detail = $"{GetTransportTypeText(transport.Type)} ({transport.NumberOfVehicles} وحدة) - {transport.TotalCost:N2} جنيه";
                AddBulletPoint(_contentPanel, detail, ref y);
            }
            y += 20;
        }
        
        // تفصيل الإقامة
        if (_trip.Accommodations.Any())
        {
            AddReportHeader(_contentPanel, "🏨 تفصيل الإقامة", ref y);
            foreach (var acc in _trip.Accommodations)
            {
                string detail = $"{acc.HotelName} - {acc.NumberOfRooms} غرفة × {acc.NumberOfNights} ليلة = {acc.TotalCost:N2} جنيه";
                AddBulletPoint(_contentPanel, detail, ref y);
            }
            y += 20;
        }
        
        // تفصيل المصاريف
        if (_trip.Expenses.Any())
        {
            AddReportHeader(_contentPanel, "💸 تفصيل المصاريف", ref y);
            foreach (var expense in _trip.Expenses.OrderByDescending(e => e.Amount))
            {
                string detail = $"{expense.ExpenseType}: {expense.Amount:N2} جنيه";
                if (!string.IsNullOrWhiteSpace(expense.Description))
                {
                    detail += $" ({expense.Description})";
                }
                AddBulletPoint(_contentPanel, detail, ref y);
            }
            y += 20;
        }
        
        // المدفوعات من الحجوزات
        if (_trip.Bookings.Any())
        {
            AddReportHeader(_contentPanel, "💳 ملخص المدفوعات", ref y);
            
            decimal totalPaid = _trip.Bookings.Sum(b => b.PaidAmount);
            decimal totalRemaining = _trip.Bookings.Sum(b => b.RemainingAmount);
            int fullyPaidCount = _trip.Bookings.Count(b => b.PaymentStatus == PaymentStatus.FullyPaid);
            int partiallyPaidCount = _trip.Bookings.Count(b => b.PaymentStatus == PaymentStatus.PartiallyPaid);
            int notPaidCount = _trip.Bookings.Count(b => b.PaymentStatus == PaymentStatus.NotPaid);
            
            AddReportLine(_contentPanel, "عدد الحجوزات:", $"{_trip.Bookings.Count}", ref y);
            AddReportLine(_contentPanel, "المدفوع:", $"{totalPaid:N2} جنيه", ref y, color: ColorScheme.Success);
            AddReportLine(_contentPanel, "المتبقي:", $"{totalRemaining:N2} جنيه", ref y, color: ColorScheme.Error);
            AddReportLine(_contentPanel, "مدفوع بالكامل:", $"{fullyPaidCount} حجز", ref y);
            AddReportLine(_contentPanel, "دفع جزئي:", $"{partiallyPaidCount} حجز", ref y);
            AddReportLine(_contentPanel, "لم يدفع:", $"{notPaidCount} حجز", ref y);
        }
    }
    
    private void PrintButton_Click(object? sender, EventArgs e)
    {
        MessageBox.Show("وظيفة الطباعة قيد التطوير", "قريباً", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    
    private void ExportButton_Click(object? sender, EventArgs e)
    {
        MessageBox.Show("وظيفة التصدير قيد التطوير", "قريباً", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    
    // Helper Methods
    private void AddReportHeader(Panel panel, string text, ref int y)
    {
        var headerPanel = new Panel
        {
            Location = new Point(0, y),
            Size = new Size(800, 40),
            BackColor = Color.FromArgb(236, 240, 241)
        };
        
        var label = new Label
        {
            Text = text,
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(15, 10)
        };
        headerPanel.Controls.Add(label);
        
        panel.Controls.Add(headerPanel);
        y += 50;
    }
    
    private void AddReportLine(Panel panel, string label, string value, ref int y, bool bold = false, Color? color = null)
    {
        var lblLabel = new Label
        {
            Text = label,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(30, y)
        };
        panel.Controls.Add(lblLabel);
        
        var lblValue = new Label
        {
            Text = value,
            Font = new Font("Cairo", 10F, bold ? FontStyle.Bold : FontStyle.Regular),
            AutoSize = true,
            Location = new Point(300, y),
            ForeColor = color ?? Color.FromArgb(52, 73, 94)
        };
        panel.Controls.Add(lblValue);
        y += 35;
    }
    
    private void AddBulletPoint(Panel panel, string text, ref int y)
    {
        var label = new Label
        {
            Text = $"• {text}",
            Font = new Font("Cairo", 10F),
            AutoSize = true,
            Location = new Point(40, y),
            MaximumSize = new Size(750, 0),
            ForeColor = Color.FromArgb(52, 73, 94)
        };
        panel.Controls.Add(label);
        y += label.Height + 5;
    }
    
    private void AddHorizontalLine(Panel panel, ref int y)
    {
        var line = new Panel
        {
            Location = new Point(30, y),
            Size = new Size(770, 2),
            BackColor = Color.FromArgb(189, 195, 199)
        };
        panel.Controls.Add(line);
        y += 15;
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
}
