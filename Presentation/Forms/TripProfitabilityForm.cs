using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Domain.Reports;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class TripProfitabilityForm : Form
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IExportService _exportService;
    private ComboBox cmbTrip = null!;
    private DataGridView dgvProfitability = null!;
    private Panel pnlSummary = null!;
    private Button btnGenerate = null!;
    private Button btnExportExcel = null!;
    private Button btnExportPdf = null!;
    
    // Summary labels
    private Label lblRevenue = null!;
    private Label lblTotalCosts = null!;
    private Label lblProfit = null!;
    private Label lblProfitMargin = null!;
    private Label lblOccupancyRate = null!;
    
    private TripProfitabilityReport? _currentReport;

    public TripProfitabilityForm(IDbContextFactory<AppDbContext> dbContextFactory, IExportService exportService)
    {
        _dbContextFactory = dbContextFactory;
        _exportService = exportService;
        InitializeComponent();
        InitializeCustomComponents();
        LoadTrips();
    }

    private void InitializeCustomComponents()
    {
        this.Text = "💰 تقرير ربحية الرحلات";
        this.Size = new Size(1400, 900);
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;

        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(30)
        };

        Label lblTitle = new Label
        {
            Text = "💰 تقرير ربحية الرحلات (Trip Profitability)",
            Font = new Font("Cairo", 18F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(30, 20)
        };

        // Filter Panel
        Panel filterPanel = new Panel
        {
            Size = new Size(1340, 80),
            Location = new Point(30, 70),
            BackColor = ColorScheme.Background,
            BorderStyle = BorderStyle.FixedSingle
        };

        Label lblTrip = new Label
        {
            Text = "اختر الرحلة:",
            Font = new Font("Cairo", 11F),
            Location = new Point(1200, 20),
            AutoSize = true
        };

        cmbTrip = new ComboBox
        {
            Location = new Point(900, 17),
            Size = new Size(290, 30),
            Font = new Font("Cairo", 10F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbTrip.SelectedIndexChanged += CmbTrip_SelectedIndexChanged;

        btnGenerate = new Button
        {
            Text = "📊 إنشاء التقرير",
            Location = new Point(700, 15),
            Size = new Size(180, 35),
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnGenerate.FlatAppearance.BorderSize = 0;
        btnGenerate.Click += BtnGenerate_Click;

        btnExportExcel = new Button
        {
            Text = "📥 Excel",
            Location = new Point(560, 15),
            Size = new Size(130, 35),
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            BackColor = Color.FromArgb(46, 125, 50),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnExportExcel.FlatAppearance.BorderSize = 0;
        btnExportExcel.Click += BtnExportExcel_Click;

        btnExportPdf = new Button
        {
            Text = "📄 PDF",
            Location = new Point(420, 15),
            Size = new Size(130, 35),
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            BackColor = Color.FromArgb(211, 47, 47),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnExportPdf.FlatAppearance.BorderSize = 0;
        btnExportPdf.Click += BtnExportPdf_Click;

        filterPanel.Controls.AddRange(new Control[] {
            lblTrip, cmbTrip, btnGenerate, btnExportExcel, btnExportPdf
        });

        // Summary Panel
        pnlSummary = new Panel
        {
            Location = new Point(30, 160),
            Size = new Size(1340, 150),
            BackColor = ColorScheme.Background,
            BorderStyle = BorderStyle.FixedSingle,
            Visible = false
        };
        CreateSummaryCards();

        // DataGridView
        dgvProfitability = new DataGridView
        {
            Location = new Point(30, 320),
            Size = new Size(1340, 500),
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = new Font("Cairo", 10F),
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 40,
            RowTemplate = { Height = 35 }
        };

        dgvProfitability.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Primary;
        dgvProfitability.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvProfitability.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        dgvProfitability.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);

        mainPanel.Controls.AddRange(new Control[] {
            lblTitle, filterPanel, pnlSummary, dgvProfitability
        });

        this.Controls.Add(mainPanel);
    }

    private void CreateSummaryCards()
    {
        int cardWidth = 260;
        int cardHeight = 130;
        int spacing = 15;
        int xPos = 20;

        // Card 1: Revenue
        CreateSummaryCard(pnlSummary, "إجمالي الإيرادات", "💵", ref lblRevenue, 
            xPos, 10, cardWidth, cardHeight, ColorScheme.Success);
        xPos += cardWidth + spacing;

        // Card 2: Total Costs
        CreateSummaryCard(pnlSummary, "إجمالي التكاليف", "💸", ref lblTotalCosts, 
            xPos, 10, cardWidth, cardHeight, ColorScheme.Error);
        xPos += cardWidth + spacing;

        // Card 3: Profit
        CreateSummaryCard(pnlSummary, "صافي الربح", "💰", ref lblProfit, 
            xPos, 10, cardWidth, cardHeight, ColorScheme.Primary);
        xPos += cardWidth + spacing;

        // Card 4: Profit Margin
        CreateSummaryCard(pnlSummary, "هامش الربح", "📊", ref lblProfitMargin, 
            xPos, 10, cardWidth, cardHeight, Color.FromArgb(156, 39, 176));
        xPos += cardWidth + spacing;

        // Card 5: Occupancy Rate
        CreateSummaryCard(pnlSummary, "نسبة الإشغال", "👥", ref lblOccupancyRate, 
            xPos, 10, cardWidth, cardHeight, Color.FromArgb(255, 152, 0));
    }

    private void CreateSummaryCard(Panel parent, string title, string icon, 
        ref Label valueLabel, int x, int y, int width, int height, Color color)
    {
        Panel card = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        Label lblIcon = new Label
        {
            Text = icon,
            Font = new Font("Segoe UI Emoji", 24F),
            ForeColor = color,
            AutoSize = false,
            Size = new Size(60, 50),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(10, 10)
        };

        Label lblTitle = new Label
        {
            Text = title,
            Font = new Font("Cairo", 10F),
            ForeColor = Color.Gray,
            AutoSize = false,
            Size = new Size(180, 25),
            TextAlign = ContentAlignment.MiddleRight,
            Location = new Point(70, 15)
        };

        valueLabel = new Label
        {
            Text = "0.00",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = color,
            AutoSize = false,
            Size = new Size(240, 40),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(10, 70)
        };

        card.Controls.AddRange(new Control[] { lblIcon, lblTitle, valueLabel });
        parent.Controls.Add(card);
    }

    private async void LoadTrips()
    {
        try
        {
            Console.WriteLine("🔄 Loading trips...");
            
            using var _dbContext = _dbContextFactory.CreateDbContext();
            var trips = await _dbContext.Set<Trip>()
                .AsNoTracking()
                .OrderByDescending(t => t.StartDate)
                .Select(t => new { 
                    t.TripId, 
                    DisplayText = (t.TripCode ?? "N/A") + " - " + t.TripName
                })
                .ToListAsync();

            Console.WriteLine($"✅ Loaded {trips.Count} trips");

            cmbTrip.DisplayMember = "DisplayText";
            cmbTrip.ValueMember = "TripId";
            cmbTrip.DataSource = trips;

            if (trips.Any())
            {
                cmbTrip.SelectedIndex = 0;
                btnGenerate.Enabled = true;
                Console.WriteLine($"✅ Trips loaded. User can click Generate Report.");
            }
            else
            {
                Console.WriteLine("⚠️ No trips found in database");
                MessageBox.Show(
                    "لا توجد رحلات في النظام!\n\n" +
                    "يرجى إضافة رحلات أولاً من قسم \"الرحلات\" في القائمة الرئيسية.\n\n" +
                    "📍 القائمة الرئيسية ← الرحلات ← إضافة رحلة جديدة",
                    "لا توجد بيانات",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error loading trips: {ex.Message}");
            MessageBox.Show($"خطأ في تحميل الرحلات: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private void CmbTrip_SelectedIndexChanged(object? sender, EventArgs e)
    {
        btnGenerate.Enabled = cmbTrip.SelectedValue != null;
        // لا نولد التقرير تلقائياً - نترك المستخدم يضغط زر "إنشاء التقرير"
    }

    private async Task GenerateReportForSelectedTrip()
    {
        try
        {
            if (cmbTrip.SelectedValue == null) return;

            int tripId = (int)cmbTrip.SelectedValue;
            Console.WriteLine($"📊 Generating report for trip ID: {tripId}");

            using var _dbContext = _dbContextFactory.CreateDbContext();

            // Get trip data - load only database columns, not computed properties
            var trip = await _dbContext.Set<Trip>()
                .AsNoTracking()
                .Where(t => t.TripId == tripId)
                .FirstOrDefaultAsync();

            if (trip == null)
            {
                Console.WriteLine($"❌ Trip not found: {tripId}");
                MessageBox.Show("لم يتم العثور على الرحلة", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Console.WriteLine($"✅ Trip found: {trip.TripName}");
            Console.WriteLine($"   BookedSeats: {trip.BookedSeats}/{trip.TotalCapacity}");
            Console.WriteLine($"   ExpectedRevenue: {trip.ExpectedRevenue:N2}");

            // Calculate report
            _currentReport = await CalculateTripProfitability(trip);

            Console.WriteLine($"✅ Report calculated:");
            Console.WriteLine($"   Revenue: {_currentReport.Revenue:N2}");
            Console.WriteLine($"   Total Costs: {_currentReport.Costs.Total:N2}");
            Console.WriteLine($"   Profit: {_currentReport.Profit:N2}");

            // Update UI
            DisplayReport(_currentReport);
            
            Console.WriteLine($"✅ Report displayed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error generating report: {ex.Message}");
            Console.WriteLine($"   StackTrace: {ex.StackTrace}");
            MessageBox.Show($"خطأ في إنشاء التقرير: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private async void BtnGenerate_Click(object? sender, EventArgs e)
    {
        await GenerateReportForSelectedTrip();
    }

    private async Task<TripProfitabilityReport> CalculateTripProfitability(Trip trip)
    {
        using var _dbContext = _dbContextFactory.CreateDbContext();
        Console.WriteLine($"💰 Calculating profitability for trip: {trip.TripName}");
        
        var report = new TripProfitabilityReport
        {
            TripId = trip.TripId,
            TripName = trip.TripName,
            TripCode = trip.TripCode ?? string.Empty,
            StartDate = trip.StartDate,
            EndDate = trip.EndDate,
            AvailableSeats = trip.TotalCapacity
        };

        // ============================================
        // 1️⃣ حساب الإيرادات (Revenue Calculation)
        // ============================================
        Console.WriteLine("📊 Calculating revenue...");
        
        // طريقة 1: من حجوزات الرحلات (TripBookings)
        var bookings = await _dbContext.Set<TripBooking>()
            .Where(b => b.TripId == trip.TripId && b.Status != BookingStatus.Cancelled)
            .ToListAsync();

        decimal bookingsRevenue = 0;
        int totalBookedPersons = 0;
        
        if (bookings.Any())
        {
            totalBookedPersons = bookings.Sum(b => b.NumberOfPersons);
            bookingsRevenue = bookings.Sum(b => b.NumberOfPersons * b.PricePerPerson);
            Console.WriteLine($"   ✓ Bookings: {bookings.Count} حجز، {totalBookedPersons} شخص، {bookingsRevenue:N2} جنيه");
        }
        else
        {
            Console.WriteLine($"   ⚠️ No bookings found in TripBookings table");
        }

        // طريقة 2: من فواتير المبيعات المرتبطة بحجوزات الرحلة
        var salesInvoices = await _dbContext.Set<SalesInvoice>()
            .Where(s => bookings.Select(b => b.SalesInvoiceId).Contains(s.SalesInvoiceId))
            .ToListAsync();
        
        decimal salesRevenue = salesInvoices.Sum(s => s.TotalAmount);
        if (salesRevenue > 0)
        {
            Console.WriteLine($"   ✓ Sales Invoices: {salesInvoices.Count} فاتورة، {salesRevenue:N2} جنيه");
        }

        // طريقة 3: من بيانات الرحلة نفسها (السعر المحدد في الرحلة × المقاعد المحجوزة)
        decimal tripDirectRevenue = trip.BookedSeats * trip.SellingPricePerPerson * trip.ExchangeRate;
        Console.WriteLine($"   ✓ Trip Direct Revenue: {trip.BookedSeats} مقعد × {trip.SellingPricePerPerson:N2} × {trip.ExchangeRate} = {tripDirectRevenue:N2} جنيه");

        // طريقة 4: من ExpectedRevenue المحسوب في الـ entity
        decimal tripExpectedRevenue = trip.BookedSeats * trip.SellingPricePerPerson * trip.ExchangeRate;
        Console.WriteLine($"   ✓ Trip Expected Revenue: {tripExpectedRevenue:N2} جنيه");

        // نختار أعلى قيمة من المصادر المتعددة
        report.Revenue = Math.Max(bookingsRevenue, Math.Max(salesRevenue, Math.Max(tripDirectRevenue, tripExpectedRevenue)));
        report.BookingsCount = bookings.Any() ? bookings.Count : (trip.BookedSeats > 0 ? 1 : 0);
        report.TotalParticipants = totalBookedPersons > 0 ? totalBookedPersons : trip.BookedSeats;
        
        Console.WriteLine($"   💰 Final Revenue: {report.Revenue:N2} جنيه");

        // إضافة إيرادات الرحلات الاختيارية
        var optionalToursRevenue = await _dbContext.Set<TripOptionalTour>()
            .Where(o => o.TripId == trip.TripId)
            .SumAsync(o => o.ParticipantsCount * o.SellingPrice);
        
        if (optionalToursRevenue > 0)
        {
            report.Revenue += optionalToursRevenue;
            Console.WriteLine($"   ✓ Optional Tours: {optionalToursRevenue:N2} جنيه");
        }

        Console.WriteLine($"   📊 Total Revenue (with optional tours): {report.Revenue:N2} جنيه");

        // ============================================
        // 2️⃣ حساب التكاليف (Costs Calculation)
        // ============================================
        Console.WriteLine("💸 Calculating costs...");
        var costs = new TripCosts();

        // 💰 تكاليف الإقامة
        var accommodations = await _dbContext.Set<TripAccommodation>()
            .Where(a => a.TripId == trip.TripId)
            .ToListAsync();
        
        costs.Accommodation = accommodations.Sum(a => a.NumberOfRooms * a.NumberOfNights * a.CostPerRoomPerNight);
        if (costs.Accommodation > 0)
            Console.WriteLine($"   ✓ Accommodation: {accommodations.Count} إقامة، {costs.Accommodation:N2} جنيه");

        // 🚌 تكاليف النقل
        var transportations = await _dbContext.Set<TripTransportation>()
            .Where(t => t.TripId == trip.TripId)
            .ToListAsync();
        
        costs.Transportation = transportations.Sum(t => t.NumberOfVehicles * t.CostPerVehicle);
        if (costs.Transportation > 0)
            Console.WriteLine($"   ✓ Transportation: {transportations.Count} نقل، {costs.Transportation:N2} جنيه");

        // 👤 تكاليف المرشدين
        var guides = await _dbContext.Set<TripGuide>()
            .Where(g => g.TripId == trip.TripId)
            .ToListAsync();
        
        costs.Guides = guides.Sum(g => g.BaseFee + g.CommissionAmount);
        if (costs.Guides > 0)
            Console.WriteLine($"   ✓ Guides: {guides.Count} مرشد، {costs.Guides:N2} جنيه");

        // 🎫 تكاليف الرحلات الاختيارية
        var optionalTours = await _dbContext.Set<TripOptionalTour>()
            .Where(o => o.TripId == trip.TripId)
            .ToListAsync();
        
        costs.OptionalTours = optionalTours.Sum(o => 
            (o.ParticipantsCount * o.PurchasePrice) + o.GuideCommission + o.SalesRepCommission);
        if (costs.OptionalTours > 0)
            Console.WriteLine($"   ✓ Optional Tours: {optionalTours.Count} رحلة، {costs.OptionalTours:N2} جنيه");

        // 💸 تكاليف من فواتير المشتريات
        var purchaseInvoices = await _dbContext.Set<PurchaseInvoice>()
            .Where(p => p.Notes != null && 
                       (p.Notes.Contains($"Trip-{trip.TripId}") || 
                        p.Notes.Contains(trip.TripCode ?? "")))
            .ToListAsync();
        
        costs.Other = purchaseInvoices.Sum(p => p.TotalAmount);
        if (costs.Other > 0)
            Console.WriteLine($"   ✓ Purchase Invoices: {purchaseInvoices.Count} فاتورة، {costs.Other:N2} جنيه");

        // حساب من جدول TripExpenses إذا كان موجود
        try
        {
            var tripExpenses = await _dbContext.Set<TripExpense>()
                .Where(e => e.TripId == trip.TripId)
                .ToListAsync();
            
            decimal expensesTotal = tripExpenses.Sum(e => e.Amount);
            if (expensesTotal > 0)
            {
                costs.Other += expensesTotal;
                Console.WriteLine($"   ✓ Trip Expenses: {tripExpenses.Count} مصروف، {expensesTotal:N2} جنيه");
            }
        }
        catch
        {
            // TripExpense table might not exist
        }

        report.Costs = costs;
        
        // إذا لم توجد تكاليف في الجداول التفصيلية، نستخدم TotalCost من بيانات الرحلة
        if (costs.Total == 0 && trip.TotalCost > 0)
        {
            costs.Other = trip.TotalCost;
            Console.WriteLine($"   ⚠️ Using Trip.TotalCost as fallback: {trip.TotalCost:N2} جنيه");
        }
        
        Console.WriteLine($"   💸 Total Costs: {costs.Total:N2} جنيه");
        Console.WriteLine($"   💎 Net Profit: {report.Profit:N2} جنيه ({report.ProfitMargin:N2}%)");

        return report;
    }

    private void DisplayReport(TripProfitabilityReport report)
    {
        Console.WriteLine($"🎨 Displaying report...");
        
        // ============================================
        // 📊 تحديث بطاقات الملخص (Update Summary Cards)
        // ============================================
        lblRevenue.Text = $"{report.Revenue:N2} جنيه";
        lblTotalCosts.Text = $"{report.Costs.Total:N2} جنيه";
        lblProfit.Text = $"{report.Profit:N2} جنيه";
        lblProfit.ForeColor = report.Profit >= 0 ? ColorScheme.Success : ColorScheme.Error;
        lblProfitMargin.Text = $"{report.ProfitMargin:N2}%";
        lblOccupancyRate.Text = $"{report.OccupancyRate:N2}%";

        Console.WriteLine($"   Summary cards updated:");
        Console.WriteLine($"     Revenue: {report.Revenue:N2}");
        Console.WriteLine($"     Costs: {report.Costs.Total:N2}");
        Console.WriteLine($"     Profit: {report.Profit:N2}");
        Console.WriteLine($"     Margin: {report.ProfitMargin:N2}%");
        Console.WriteLine($"     Occupancy: {report.OccupancyRate:N2}%");

        pnlSummary.Visible = true;

        // ============================================
        // 📋 إعداد جدول التقرير (Setup Report Grid)
        // ============================================
        dgvProfitability.Columns.Clear();
        dgvProfitability.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Category",
            HeaderText = "البند",
            Width = 350
        });
        dgvProfitability.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Value",
            HeaderText = "القيمة (جنيه)",
            Width = 200,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Format = "N2",
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        dgvProfitability.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Percentage",
            HeaderText = "النسبة %",
            Width = 150,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Format = "N2",
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });

        dgvProfitability.Rows.Clear();

        // ============================================
        // 💰 قسم الإيرادات (Revenue Section)
        // ============================================
        AddHeaderRow("💰 الإيرادات (Revenue)");
        dgvProfitability.Rows.Add("إيرادات الحجوزات", report.Revenue, 100m);
        dgvProfitability.Rows.Add($"  ✓ عدد الحجوزات: {report.BookingsCount}", "", "");
        dgvProfitability.Rows.Add($"  ✓ عدد المشاركين: {report.TotalParticipants} من {report.AvailableSeats}", "", "");
        AddTotalRow("إجمالي الإيرادات", report.Revenue);

        // Empty row for spacing
        dgvProfitability.Rows.Add("", "", "");

        // ============================================
        // 💸 قسم التكاليف (Costs Section)
        // ============================================
        AddHeaderRow("💸 التكاليف (Costs)");
        decimal totalCosts = report.Costs.Total;
        
        if (totalCosts == 0)
        {
            dgvProfitability.Rows.Add("⚠️ لا توجد تكاليف مسجلة", "", "");
        }
        else
        {
            if (report.Costs.Accommodation > 0)
            {
                decimal pct = totalCosts > 0 ? (report.Costs.Accommodation / totalCosts) * 100 : 0;
                dgvProfitability.Rows.Add("  🏨 تكاليف الإقامة", report.Costs.Accommodation, pct);
            }
            if (report.Costs.Transportation > 0)
            {
                decimal pct = totalCosts > 0 ? (report.Costs.Transportation / totalCosts) * 100 : 0;
                dgvProfitability.Rows.Add("  🚌 تكاليف النقل", report.Costs.Transportation, pct);
            }
            if (report.Costs.Guides > 0)
            {
                decimal pct = totalCosts > 0 ? (report.Costs.Guides / totalCosts) * 100 : 0;
                dgvProfitability.Rows.Add("  👤 تكاليف المرشدين", report.Costs.Guides, pct);
            }
            if (report.Costs.OptionalTours > 0)
            {
                decimal pct = totalCosts > 0 ? (report.Costs.OptionalTours / totalCosts) * 100 : 0;
                dgvProfitability.Rows.Add("  🎫 تكاليف الرحلات الاختيارية", report.Costs.OptionalTours, pct);
            }
            if (report.Costs.Other > 0)
            {
                decimal pct = totalCosts > 0 ? (report.Costs.Other / totalCosts) * 100 : 0;
                dgvProfitability.Rows.Add("  📦 تكاليف أخرى", report.Costs.Other, pct);
            }
        }
        
        AddTotalRow("إجمالي التكاليف", totalCosts, true);

        // Empty row for spacing
        dgvProfitability.Rows.Add("", "", "");

        // ============================================
        // 📈 قسم الربح (Profit Section)
        // ============================================
        AddProfitRow("💎 صافي الربح (Net Profit)", report.Profit, report.ProfitMargin);

        // Empty row for spacing
        dgvProfitability.Rows.Add("", "", "");

        // ============================================
        // 📊 مؤشرات الأداء الرئيسية (Key Performance Indicators)
        // ============================================
        AddHeaderRow("📊 مؤشرات الأداء (KPIs)");
        
        if (report.TotalParticipants > 0)
        {
            dgvProfitability.Rows.Add("  ✦ متوسط الإيراد للمشارك", report.RevenuePerParticipant, "");
            dgvProfitability.Rows.Add("  ✦ متوسط التكلفة للمشارك", report.CostPerParticipant, "");
            dgvProfitability.Rows.Add("  ✦ متوسط الربح للمشارك", report.ProfitPerParticipant, "");
        }
        else
        {
            dgvProfitability.Rows.Add("  ⚠️ لا يوجد مشاركين لحساب المتوسطات", "", "");
        }
        
        // إضافة نسبة الإشغال
        int rowIndex = dgvProfitability.Rows.Add($"  ✦ نسبة الإشغال", 
            $"{report.TotalParticipants} / {report.AvailableSeats}", 
            $"{report.OccupancyRate:N2}");
        
        // تلوين نسبة الإشغال حسب القيمة
        Color occupancyColor = report.OccupancyRate >= 80 ? ColorScheme.Success :
                              report.OccupancyRate >= 50 ? Color.FromArgb(255, 152, 0) :
                              ColorScheme.Error;
        dgvProfitability.Rows[rowIndex].DefaultCellStyle.ForeColor = occupancyColor;
        dgvProfitability.Rows[rowIndex].DefaultCellStyle.Font = new Font("Cairo", 10F, FontStyle.Bold);

        // Empty row for spacing
        dgvProfitability.Rows.Add("", "", "");

        // ============================================
        // ℹ️ ملاحظات إضافية (Additional Notes)
        // ============================================
        if (report.Revenue == 0)
        {
            AddWarningRow("⚠️ لا توجد حجوزات مسجلة لهذه الرحلة");
            AddInfoRow("💡 قم بإضافة حجوزات للرحلة من قسم 'حجوزات الرحلات'");
        }
        
        if (report.Costs.Total == 0)
        {
            AddWarningRow("⚠️ لا توجد تكاليف مسجلة لهذه الرحلة");
            AddInfoRow("💡 قم بإضافة تكاليف الرحلة (إقامة، نقل، مرشدين) للحصول على تقرير دقيق");
        }

        // تفعيل أزرار التصدير
        btnExportExcel.Enabled = true;
        btnExportPdf.Enabled = true;
    }

    private void AddWarningRow(string message)
    {
        int rowIndex = dgvProfitability.Rows.Add(message, "", "");
        dgvProfitability.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 243, 224);
        dgvProfitability.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(230, 81, 0);
        dgvProfitability.Rows[rowIndex].DefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
    }

    private void AddInfoRow(string message)
    {
        int rowIndex = dgvProfitability.Rows.Add(message, "", "");
        dgvProfitability.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(227, 242, 253);
        dgvProfitability.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(13, 71, 161);
        dgvProfitability.Rows[rowIndex].DefaultCellStyle.Font = new Font("Cairo", 10F, FontStyle.Italic);
    }

    private void AddHeaderRow(string title)
    {
        int rowIndex = dgvProfitability.Rows.Add(title, "", "");
        dgvProfitability.Rows[rowIndex].DefaultCellStyle.BackColor = ColorScheme.Primary;
        dgvProfitability.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.White;
        dgvProfitability.Rows[rowIndex].DefaultCellStyle.Font = new Font("Cairo", 12F, FontStyle.Bold);
    }

    private void AddTotalRow(string title, decimal value, bool isExpense = false)
    {
        int rowIndex = dgvProfitability.Rows.Add(title, value, "");
        Color bgColor = isExpense ? Color.FromArgb(255, 205, 210) : Color.FromArgb(200, 230, 201);
        dgvProfitability.Rows[rowIndex].DefaultCellStyle.BackColor = bgColor;
        dgvProfitability.Rows[rowIndex].DefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
    }

    private void AddProfitRow(string title, decimal profit, decimal margin)
    {
        int rowIndex = dgvProfitability.Rows.Add(title, profit, margin);
        Color bgColor = profit >= 0 ? ColorScheme.Success : ColorScheme.Error;
        dgvProfitability.Rows[rowIndex].DefaultCellStyle.BackColor = bgColor;
        dgvProfitability.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.White;
        dgvProfitability.Rows[rowIndex].DefaultCellStyle.Font = new Font("Cairo", 14F, FontStyle.Bold);
    }

    private async void BtnExportExcel_Click(object? sender, EventArgs e)
    {
        try
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "حفظ تقرير ربحية الرحلة",
                FileName = $"ربحية_رحلة_{_currentReport?.TripCode}_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                bool success = await _exportService.ExportToExcelAsync(
                    dgvProfitability, 
                    saveDialog.FileName, 
                    "ربحية الرحلة"
                );

                if (success)
                {
                    MessageBox.Show("تم تصدير التقرير بنجاح!", "نجح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في التصدير: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private async void BtnExportPdf_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_currentReport == null) return;

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "HTML Files|*.html",
                Title = "حفظ تقرير ربحية الرحلة",
                FileName = $"ربحية_رحلة_{_currentReport.TripCode}_{DateTime.Now:yyyyMMdd}.html"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                var metadata = new Dictionary<string, string>
                {
                    { "الرحلة", $"{_currentReport.TripCode} - {_currentReport.TripName}" },
                    { "الفترة", $"من {_currentReport.StartDate:yyyy/MM/dd} إلى {_currentReport.EndDate:yyyy/MM/dd}" },
                    { "الإيرادات", $"{_currentReport.Revenue:N2} جنيه" },
                    { "التكاليف", $"{_currentReport.Costs.Total:N2} جنيه" },
                    { "صافي الربح", $"{_currentReport.Profit:N2} جنيه" },
                    { "هامش الربح", $"{_currentReport.ProfitMargin:N2}%" },
                    { "نسبة الإشغال", $"{_currentReport.OccupancyRate:N2}%" }
                };

                bool success = await _exportService.ExportToPdfAsync(
                    dgvProfitability,
                    saveDialog.FileName,
                    "💰 تقرير ربحية الرحلة",
                    metadata
                );
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في التصدير: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }
}
