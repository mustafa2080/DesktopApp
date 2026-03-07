using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace GraceWay.AccountingSystem.Presentation.Controls;

public class DashboardControl : UserControl
{
    private readonly IReservationService  _reservationService;
    private readonly ICashBoxService      _cashBoxService;
    private readonly ICustomerService     _customerService;
    private readonly IInvoiceService      _invoiceService;
    private readonly ITripService         _tripService;
    private readonly IUmrahService        _umrahService;
    private readonly ISupplierService     _supplierService;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private static DateTime _lastCacheTime = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private static decimal _cachedCashBalance, _cachedTodaySales, _cachedMonthlyRevenue;
    private static int    _cachedActiveReservations, _cachedCustomersCount, _cachedTripsCount;
    private static int    _cachedUmrahCount, _cachedSuppliersCount;
    private static bool   _cacheValid = false;
    private static readonly Color BgPage      = Color.FromArgb(247, 249, 252);
    private static readonly Color BgCard      = Color.White;
    private static readonly Color BorderCard  = Color.FromArgb(226, 232, 240);
    private static readonly Color TextPrimary = Color.FromArgb(15,  23,  42);
    private static readonly Color TextMuted   = Color.FromArgb(100, 116, 139);
    private static readonly Color TextLabel   = Color.FromArgb(71,  85, 105);
    private static readonly Color AccentBlue   = Color.FromArgb(59,  130, 246);
    private static readonly Color AccentGreen  = Color.FromArgb(16,  185, 129);
    private static readonly Color AccentOrange = Color.FromArgb(251, 146,  60);
    private static readonly Color AccentPurple = Color.FromArgb(139, 92,  246);
    private static readonly Color AccentTeal   = Color.FromArgb(20,  184, 166);
    private static readonly Color AccentRed    = Color.FromArgb(239, 68,  68);
    private Panel    _mainPanel      = null!;
    private FlowLayoutPanel _kpiRow1 = null!;
    private FlowLayoutPanel _kpiRow2 = null!;
    private Panel    _chartsRow      = null!;
    private Panel    _tableRow       = null!;
    private CartesianChart? _revenueChart;
    private PieChart?       _pieChart;
    private DataGridView    _recentGrid = null!;
    private Label    _lblLastUpdate   = null!;
    private Label _kpiCash=null!, _kpiTodaySales=null!, _kpiMonthRev=null!, _kpiReservations=null!;
    private Label _kpiCustomers=null!, _kpiTrips=null!, _kpiUmrah=null!, _kpiSuppliers=null!;

    public DashboardControl(
        IReservationService  reservationService,
        ICashBoxService      cashBoxService,
        ICustomerService     customerService,
        IInvoiceService      invoiceService,
        ITripService         tripService,
        IUmrahService        umrahService,
        ISupplierService     supplierService,
        IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _reservationService = reservationService;
        _cashBoxService     = cashBoxService;
        _customerService    = customerService;
        _invoiceService     = invoiceService;
        _tripService        = tripService;
        _umrahService       = umrahService;
        _supplierService    = supplierService;
        _dbContextFactory   = dbContextFactory;
        InitializeComponent();
    }

    // ─────────────────────────────────────────────────────────────────────
    // INIT
    // ─────────────────────────────────────────────────────────────────────
    private void InitializeComponent()
    {
        SuspendLayout();
        BackColor  = BgPage;
        Dock       = DockStyle.Fill;
        AutoScroll = true;
        RightToLeft = RightToLeft.Yes;

        // Outer scrollable panel
        _mainPanel = new Panel
        {
            Dock      = DockStyle.Fill,
            AutoScroll = true,
            BackColor  = BgPage,
            Padding    = new Padding(24, 20, 24, 24),
        };
        Controls.Add(_mainPanel);

        BuildHeader();
        BuildKpiRow1();
        BuildKpiRow2();
        BuildChartsRow();
        BuildTableRow();
        ArrangeLayout();

        ResumeLayout(false);
        Load += async (_, _) => await LoadDashboardAsync(forceRefresh: false);
    }

    // ─────────────────────────────────────────────────────────────────────
    // BUILD: Header
    // ─────────────────────────────────────────────────────────────────────
    private void BuildHeader()
    {
        var header = new Panel
        {
            Height    = 72,
            Dock      = DockStyle.Top,
            BackColor = Color.White,
            Padding   = new Padding(0, 0, 0, 1),
            Margin    = new Padding(0, 0, 0, 20),
        };
        header.Paint += (s, e) =>
        {
            var r = new Rectangle(0, header.Height - 1, header.Width, 1);
            e.Graphics.FillRectangle(new SolidBrush(BorderCard), r);
        };

        var lblTitle = new Label
        {
            Text      = "لوحة التحكم",
            Font      = new Font("Segoe UI", 18f, FontStyle.Bold),
            ForeColor = TextPrimary,
            AutoSize  = true,
            Location  = new Point(header.Width - 180, 20),
            Anchor    = AnchorStyles.Top | AnchorStyles.Right,
        };

        _lblLastUpdate = new Label
        {
            Text      = "جاري التحميل...",
            Font      = new Font("Segoe UI", 9f),
            ForeColor = TextMuted,
            AutoSize  = true,
            Location  = new Point(16, 28),
            Anchor    = AnchorStyles.Top | AnchorStyles.Left,
        };

        var btnRefresh = CreateIconButton("⟳ تحديث", AccentBlue);
        btnRefresh.Location = new Point(120, 20);
        btnRefresh.Anchor   = AnchorStyles.Top | AnchorStyles.Left;
        btnRefresh.Click   += async (_, _) => await LoadDashboardAsync(forceRefresh: true);

        header.Controls.AddRange(new Control[] { lblTitle, _lblLastUpdate, btnRefresh });
        _mainPanel.Controls.Add(header);
    }

    // ─────────────────────────────────────────────────────────────────────
    // BUILD: KPI Row 1  (4 cards: cash, today sales, monthly revenue, reservations)
    // ─────────────────────────────────────────────────────────────────────
    private void BuildKpiRow1()
    {
        _kpiRow1 = new FlowLayoutPanel
        {
            AutoSize        = true,
            AutoSizeMode    = AutoSizeMode.GrowAndShrink,
            FlowDirection   = FlowDirection.RightToLeft,
            WrapContents    = false,
            BackColor       = Color.Transparent,
            Margin          = new Padding(0, 20, 0, 0),
        };

        _kpiRow1.Controls.Add(BuildKpiCard("رصيد الخزنة",        "0.00 ج.م", "💰", AccentGreen,  out _kpiCash));
        _kpiRow1.Controls.Add(BuildKpiCard("مبيعات اليوم",       "0.00 ج.م", "📈", AccentBlue,   out _kpiTodaySales));
        _kpiRow1.Controls.Add(BuildKpiCard("إيرادات الشهر",      "0.00 ج.م", "📊", AccentPurple, out _kpiMonthRev));
        _kpiRow1.Controls.Add(BuildKpiCard("الحجوزات النشطة",    "0",        "🗓", AccentOrange, out _kpiReservations));

        _mainPanel.Controls.Add(_kpiRow1);
    }

    // ─────────────────────────────────────────────────────────────────────
    // BUILD: KPI Row 2  (4 cards: customers, trips, umrah, suppliers)
    // ─────────────────────────────────────────────────────────────────────
    private void BuildKpiRow2()
    {
        _kpiRow2 = new FlowLayoutPanel
        {
            AutoSize        = true,
            AutoSizeMode    = AutoSizeMode.GrowAndShrink,
            FlowDirection   = FlowDirection.RightToLeft,
            WrapContents    = false,
            BackColor       = Color.Transparent,
            Margin          = new Padding(0, 12, 0, 0),
        };

        _kpiRow2.Controls.Add(BuildKpiCard("العملاء",           "0", "👥", AccentTeal,   out _kpiCustomers));
        _kpiRow2.Controls.Add(BuildKpiCard("الرحلات",           "0", "✈", AccentBlue,   out _kpiTrips));
        _kpiRow2.Controls.Add(BuildKpiCard("حزم العمرة",        "0", "🕌", AccentGreen,  out _kpiUmrah));
        _kpiRow2.Controls.Add(BuildKpiCard("الموردين",          "0", "🏢", AccentOrange, out _kpiSuppliers));

        _mainPanel.Controls.Add(_kpiRow2);
    }

    // ─────────────────────────────────────────────────────────────────────
    // BUILD: KPI Card Factory
    // ─────────────────────────────────────────────────────────────────────
    private Panel BuildKpiCard(string title, string value, string icon, Color accent, out Label valueLabel)
    {
        var card = new Panel
        {
            Width     = 220,
            Height    = 110,
            BackColor = BgCard,
            Margin    = new Padding(0, 0, 12, 0),
            Cursor    = Cursors.Hand,
        };
        card.Paint += (s, e) => PaintCard(e.Graphics, card, accent);

        // Accent stripe top
        var stripe = new Panel { Height = 4, Dock = DockStyle.Top, BackColor = accent };
        card.Controls.Add(stripe);

        // Icon
        var lblIcon = new Label
        {
            Text      = icon,
            Font      = new Font("Segoe UI Emoji", 22f),
            ForeColor = accent,
            AutoSize  = false,
            Width     = 52,
            Height    = 52,
            TextAlign = ContentAlignment.MiddleCenter,
            Location  = new Point(12, 28),
        };
        card.Controls.Add(lblIcon);

        // Value
        valueLabel = new Label
        {
            Text      = value,
            Font      = new Font("Segoe UI", 17f, FontStyle.Bold),
            ForeColor = TextPrimary,
            AutoSize  = false,
            Width     = 145,
            Height    = 32,
            TextAlign = ContentAlignment.MiddleRight,
            Location  = new Point(60, 26),
        };
        card.Controls.Add(valueLabel);

        // Title
        var lblTitle = new Label
        {
            Text      = title,
            Font      = new Font("Segoe UI", 9f),
            ForeColor = TextMuted,
            AutoSize  = false,
            Width     = 145,
            Height    = 20,
            TextAlign = ContentAlignment.MiddleRight,
            Location  = new Point(60, 60),
        };
        card.Controls.Add(lblTitle);

        // Hover effect
        card.MouseEnter += (_, _) => { card.BackColor = Color.FromArgb(250, 252, 255); card.Invalidate(); };
        card.MouseLeave += (_, _) => { card.BackColor = BgCard;                         card.Invalidate(); };
        foreach (Control c in card.Controls)
        {
            c.MouseEnter += (_, _) => { card.BackColor = Color.FromArgb(250, 252, 255); card.Invalidate(); };
            c.MouseLeave += (_, _) => { card.BackColor = BgCard;                         card.Invalidate(); };
        }

        return card;
    }

    private static void PaintCard(Graphics g, Panel card, Color accent)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
        using var borderPen = new Pen(BorderCard, 1f);
        DrawRoundedRect(g, borderPen, rect, 10);
        // soft shadow simulation at bottom
        using var shadowBrush = new SolidBrush(Color.FromArgb(8, 0, 0, 0));
        g.FillRectangle(shadowBrush, new Rectangle(2, card.Height - 3, card.Width - 4, 3));
    }

    private static void DrawRoundedRect(Graphics g, Pen pen, Rectangle r, int radius)
    {
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);
        path.AddArc(r.Right - radius * 2, r.Y, radius * 2, radius * 2, 270, 90);
        path.AddArc(r.Right - radius * 2, r.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
        path.AddArc(r.X, r.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
        path.CloseFigure();
        g.DrawPath(pen, path);
    }

    // ─────────────────────────────────────────────────────────────────────
    // BUILD: Charts Row
    // ─────────────────────────────────────────────────────────────────────
    private void BuildChartsRow()
    {
        _chartsRow = new Panel
        {
            Height    = 280,
            BackColor = Color.Transparent,
            Margin    = new Padding(0, 16, 0, 0),
            Dock      = DockStyle.None,
        };

        // Revenue bar chart (left ~65%)
        var revenueCard = new Panel
        {
            BackColor = BgCard,
            Left      = 0,
            Top       = 0,
            Height    = 280,
        };
        revenueCard.Paint += (s, e) => PaintCard(e.Graphics, revenueCard, AccentBlue);
        revenueCard.Controls.Add(BuildSectionLabel("الإيرادات - آخر 6 أشهر", AccentBlue));

        _revenueChart = new CartesianChart
        {
            Left      = 0,
            Top       = 36,
            BackColor = Color.Transparent,
        };
        _revenueChart.XAxes = new Axis[]
        {
            new Axis { Labels = new[] { "شهر1","شهر2","شهر3","شهر4","شهر5","شهر6" },
                       LabelsPaint = new SolidColorPaint(new SKColor(100, 116, 139)) }
        };
        _revenueChart.YAxes = new Axis[]
        {
            new Axis { LabelsPaint = new SolidColorPaint(new SKColor(100, 116, 139)),
                       Labeler = v => $"{v:N0}" }
        };
        revenueCard.Controls.Add(_revenueChart);

        // Pie chart (right ~33%)
        var pieCard = new Panel
        {
            BackColor = BgCard,
            Top       = 0,
            Height    = 280,
        };
        pieCard.Paint += (s, e) => PaintCard(e.Graphics, pieCard, AccentPurple);
        pieCard.Controls.Add(BuildSectionLabel("توزيع الإيرادات", AccentPurple));

        _pieChart = new PieChart
        {
            Left      = 0,
            Top       = 36,
            BackColor = Color.Transparent,
        };
        pieCard.Controls.Add(_pieChart);

        _chartsRow.Controls.AddRange(new Control[] { revenueCard, pieCard });
        _mainPanel.Controls.Add(_chartsRow);

        // Resize handler to keep proportions
        _chartsRow.SizeChanged += (_, _) => SizeChartsRow(revenueCard, pieCard);
        SizeChanged            += (_, _) => SizeChartsRow(revenueCard, pieCard);
    }

    private void SizeChartsRow(Panel rev, Panel pie)
    {
        int totalW = _chartsRow.Width;
        if (totalW < 100) return;
        int gap    = 12;
        int revW   = (int)(totalW * 0.64) - gap;
        int pieW   = totalW - revW - gap;
        rev.Width  = revW;
        rev.Left   = totalW - revW;
        pie.Width  = pieW;
        pie.Left   = 0;
        foreach (Control c in rev.Controls)
            if (c is CartesianChart ch) { ch.Width = revW - 16; ch.Height = 236; }
        foreach (Control c in pie.Controls)
            if (c is PieChart pc) { pc.Width = pieW - 16; pc.Height = 236; pc.Left = 8; }
    }

    // ─────────────────────────────────────────────────────────────────────
    // BUILD: Recent Transactions Table Row
    // ─────────────────────────────────────────────────────────────────────
    private void BuildTableRow()
    {
        _tableRow = new Panel
        {
            Height    = 320,
            BackColor = BgCard,
            Margin    = new Padding(0, 16, 0, 0),
            Dock      = DockStyle.None,
        };
        _tableRow.Paint += (s, e) => PaintCard(e.Graphics, _tableRow, AccentGreen);

        _tableRow.Controls.Add(BuildSectionLabel("آخر الحركات المالية", AccentGreen));

        _recentGrid = new DataGridView
        {
            Left                     = 0,
            Top                      = 40,
            BackgroundColor          = BgCard,
            BorderStyle              = BorderStyle.None,
            CellBorderStyle          = DataGridViewCellBorderStyle.SingleHorizontal,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
            GridColor                = BorderCard,
            RowHeadersVisible        = false,
            AllowUserToAddRows       = false,
            AllowUserToDeleteRows    = false,
            ReadOnly                 = true,
            SelectionMode            = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode      = DataGridViewAutoSizeColumnsMode.Fill,
            RightToLeft              = RightToLeft.Yes,
            Font                     = new Font("Segoe UI", 9.5f),
        };
        StyleGrid(_recentGrid);

        _recentGrid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "التاريخ",       Name = "colDate",   FillWeight = 15 },
            new DataGridViewTextBoxColumn { HeaderText = "البيان",         Name = "colDesc",   FillWeight = 40 },
            new DataGridViewTextBoxColumn { HeaderText = "النوع",          Name = "colType",   FillWeight = 15 },
            new DataGridViewTextBoxColumn { HeaderText = "المبلغ",         Name = "colAmount", FillWeight = 20,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } },
            new DataGridViewTextBoxColumn { HeaderText = "الرصيد",         Name = "colBal",    FillWeight = 10,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } }
        );

        _recentGrid.CellFormatting += (s, e) =>
        {
            if (_recentGrid.Columns[e.ColumnIndex].Name == "colType" && e.Value != null)
            {
                e.CellStyle.ForeColor = e.Value.ToString() == "وارد" ? AccentGreen : AccentRed;
                e.CellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
            }
        };

        _tableRow.Controls.Add(_recentGrid);
        _mainPanel.Controls.Add(_tableRow);
    }

    private static void StyleGrid(DataGridView grid)
    {
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor  = Color.FromArgb(248, 250, 252),
            ForeColor  = Color.FromArgb(71, 85, 105),
            Font       = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Alignment  = DataGridViewContentAlignment.MiddleRight,
            Padding    = new Padding(8, 0, 8, 0),
        };
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor    = BgCard,
            ForeColor    = TextPrimary,
            SelectionBackColor = Color.FromArgb(239, 246, 255),
            SelectionForeColor = TextPrimary,
            Padding      = new Padding(6, 0, 6, 0),
        };
        grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(249, 251, 253),
            ForeColor = TextPrimary,
            SelectionBackColor = Color.FromArgb(239, 246, 255),
            SelectionForeColor = TextPrimary,
        };
        grid.ColumnHeadersHeight = 40;
        grid.RowTemplate.Height  = 36;
        grid.EnableHeadersVisualStyles = false;
    }

    // ─────────────────────────────────────────────────────────────────────
    // LAYOUT: Arrange all rows when panel resizes
    // ─────────────────────────────────────────────────────────────────────
    private void ArrangeLayout()
    {
        _mainPanel.Resize += (_, _) => DoLayout();
        Resize            += (_, _) => DoLayout();
    }

    private void DoLayout()
    {
        int padH  = _mainPanel.Padding.Left + _mainPanel.Padding.Right;
        int w     = Math.Max(_mainPanel.ClientSize.Width - padH, 400);
        int pad   = _mainPanel.Padding.Top;
        int y     = pad;

        // Header is DockStyle.Top so skip explicit placement for it
        // KPI rows
        _kpiRow1.Width = w;
        _kpiRow1.Top   = y + 80;
        _kpiRow1.Left  = _mainPanel.Padding.Left;
        SizeKpiCards(_kpiRow1, w, 4);
        y = _kpiRow1.Bottom;

        _kpiRow2.Width = w;
        _kpiRow2.Top   = y + 12;
        _kpiRow2.Left  = _mainPanel.Padding.Left;
        SizeKpiCards(_kpiRow2, w, 4);
        y = _kpiRow2.Bottom;

        // Charts row
        _chartsRow.Width  = w;
        _chartsRow.Top    = y + 16;
        _chartsRow.Left   = _mainPanel.Padding.Left;
        SizeChartsRow(
            _chartsRow.Controls.Count > 0 ? (Panel)_chartsRow.Controls[1] : new Panel(),
            _chartsRow.Controls.Count > 1 ? (Panel)_chartsRow.Controls[0] : new Panel()
        );
        y = _chartsRow.Bottom;

        // Table row
        _tableRow.Width  = w;
        _tableRow.Top    = y + 16;
        _tableRow.Left   = _mainPanel.Padding.Left;
        _recentGrid.Width  = w - 16;
        _recentGrid.Height = _tableRow.Height - 48;
        _recentGrid.Left   = 8;
    }

    private static void SizeKpiCards(FlowLayoutPanel row, int totalW, int count)
    {
        int margin = 12;
        int cardW  = (totalW - margin * (count - 1)) / count;
        foreach (Panel card in row.Controls.OfType<Panel>())
        {
            card.Width  = cardW;
            card.Margin = new Padding(0, 0, margin, 0);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // DATA LOADING
    // ─────────────────────────────────────────────────────────────────────
    public async Task LoadDashboardAsync(bool forceRefresh = false)
    {
        bool needRefresh = forceRefresh || !_cacheValid ||
                           (DateTime.Now - _lastCacheTime) > CacheDuration;

        if (needRefresh)
        {
            SetLoadingState(true);
            try { await FetchAllDataAsync(); }
            catch (Exception ex) { ShowError(ex.Message); return; }
            finally { SetLoadingState(false); }
        }
        UpdateKpiLabels();
        await LoadChartsAsync();
        await LoadRecentTransactionsAsync();
        _lblLastUpdate.Text = $"آخر تحديث: {DateTime.Now:hh:mm tt}";
    }

    // كل query في context مستقل لتجنب "A command is already in progress"
    private async Task FetchAllDataAsync()
    {
        // TransactionDate مخزن بـ UTC في قاعدة البيانات
        var now    = DateTime.UtcNow;
        var today  = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var month1 = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // رصيد الخزنة - يستثني المحذوفين
        using (var db = _dbContextFactory.CreateDbContext())
            _cachedCashBalance = await db.CashTransactions.AsNoTracking()
                .Where(t => !t.IsDeleted)
                .GroupBy(_ => 1)
                .Select(g => g.Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount))
                .FirstOrDefaultAsync();

        // مبيعات اليوم - يستثني المحذوفين - UTC range
        using (var db = _dbContextFactory.CreateDbContext())
            _cachedTodaySales = await db.CashTransactions.AsNoTracking()
                .Where(t => !t.IsDeleted
                         && t.Type == TransactionType.Income
                         && t.TransactionDate >= today
                         && t.TransactionDate < today.AddDays(1))
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

        // إيرادات الشهر - يستثني المحذوفين - UTC range كامل للشهر
        using (var db = _dbContextFactory.CreateDbContext())
            _cachedMonthlyRevenue = await db.CashTransactions.AsNoTracking()
                .Where(t => !t.IsDeleted
                         && t.Type == TransactionType.Income
                         && t.TransactionDate >= month1
                         && t.TransactionDate < month1.AddMonths(1))
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

        // الحجوزات النشطة - القيم الحقيقية: "Confirmed" و "Draft"
        using (var db = _dbContextFactory.CreateDbContext())
            _cachedActiveReservations = await db.Reservations.AsNoTracking()
                .CountAsync(r => r.Status == "Confirmed" || r.Status == "Draft");

        using (var db = _dbContextFactory.CreateDbContext())
            _cachedCustomersCount = await db.Customers.AsNoTracking().CountAsync();

        using (var db = _dbContextFactory.CreateDbContext())
            _cachedTripsCount = await db.Trips.AsNoTracking().CountAsync();

        using (var db = _dbContextFactory.CreateDbContext())
            _cachedUmrahCount = await db.UmrahPackages.AsNoTracking().CountAsync();

        // الموردين النشطين فقط
        using (var db = _dbContextFactory.CreateDbContext())
            _cachedSuppliersCount = await db.Suppliers.AsNoTracking()
                .CountAsync(s => s.IsActive);

        _cacheValid    = true;
        _lastCacheTime = DateTime.Now;
    }

    private void UpdateKpiLabels()
    {
        SafeInvoke(() =>
        {
            _kpiCash.Text         = $"{_cachedCashBalance:N2} ج.م";
            _kpiTodaySales.Text   = $"{_cachedTodaySales:N2} ج.م";
            _kpiMonthRev.Text     = $"{_cachedMonthlyRevenue:N2} ج.م";
            _kpiReservations.Text = _cachedActiveReservations.ToString();
            _kpiCustomers.Text    = _cachedCustomersCount.ToString();
            _kpiTrips.Text        = _cachedTripsCount.ToString();
            _kpiUmrah.Text        = _cachedUmrahCount.ToString();
            _kpiSuppliers.Text    = _cachedSuppliersCount.ToString();
        });
    }

    private async Task LoadChartsAsync()
    {
        try
        {
            var now     = DateTime.Now;
            var months  = new string[6];
            var income  = new double[6];
            var expense = new double[6];

            // كل شهر في context منفصل
            for (int i = 5; i >= 0; i--)
            {
                var d = now.AddMonths(-i);
                months[5 - i] = d.ToString("MMM");
                var m1 = new DateTime(d.Year, d.Month, 1);
                var m2 = m1.AddMonths(1);
                using var db = _dbContextFactory.CreateDbContext();
                income[5 - i]  = (double)(await db.CashTransactions.AsNoTracking()
                    .Where(t => t.Type == TransactionType.Income && t.TransactionDate >= m1 && t.TransactionDate < m2)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0);
                expense[5 - i] = (double)(await db.CashTransactions.AsNoTracking()
                    .Where(t => t.Type == TransactionType.Expense && t.TransactionDate >= m1 && t.TransactionDate < m2)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0);
            }

            SafeInvoke(() =>
            {
                if (_revenueChart == null) return;
                _revenueChart.Series = new ISeries[]
                {
                    new ColumnSeries<double>
                    {
                        Name   = "إيرادات",
                        Values = income,
                        Fill   = new SolidColorPaint(new SKColor(59, 130, 246, 200)),
                        Stroke = new SolidColorPaint(new SKColor(37, 99, 235)) { StrokeThickness = 1.5f },
                        MaxBarWidth = 28,
                    },
                    new ColumnSeries<double>
                    {
                        Name   = "مصروفات",
                        Values = expense,
                        Fill   = new SolidColorPaint(new SKColor(239, 68, 68, 180)),
                        Stroke = new SolidColorPaint(new SKColor(220, 38, 38)) { StrokeThickness = 1.5f },
                        MaxBarWidth = 28,
                    },
                };
                _revenueChart.XAxes = new Axis[]
                {
                    new Axis { Labels = months,
                               LabelsPaint = new SolidColorPaint(new SKColor(100, 116, 139)) }
                };
            });

            // Pie: reservation status distribution - context منفصل
            List<(string Status, int Count)> groups;
            using (var db = _dbContextFactory.CreateDbContext())
            {
                var raw = await db.Reservations.AsNoTracking()
                    .GroupBy(r => r.Status ?? "غير محدد")
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();
                groups = raw.Select(x => (x.Status, x.Count)).ToList();
            }

            var pieColors = new[] {
                new SKColor(59, 130, 246), new SKColor(16, 185, 129),
                new SKColor(251, 146, 60), new SKColor(139, 92, 246),
                new SKColor(239, 68, 68)
            };
            int ci = 0;
            SafeInvoke(() =>
            {
                if (_pieChart == null) return;
                _pieChart.Series = groups.Select(g => (ISeries)new PieSeries<double>
                {
                    Name   = g.Item1,
                    Values = new[] { (double)g.Item2 },
                    Fill   = new SolidColorPaint(pieColors[ci++ % pieColors.Length]),
                }).ToArray();
            });
        }
        catch { /* charts are optional - swallow */ }
    }

    private async Task LoadRecentTransactionsAsync()
    {
        try
        {
            var from = DateTime.Now.AddDays(-30);
            List<CashTransaction> recent;
            using (var db = _dbContextFactory.CreateDbContext())
            {
                recent = await db.CashTransactions.AsNoTracking()
                    .Where(t => t.TransactionDate >= from)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(20)
                    .ToListAsync();
            }

            SafeInvoke(() =>
            {
                _recentGrid.Rows.Clear();
                foreach (var tx in recent)
                {
                    string typeAr = tx.Type == TransactionType.Income ? "وارد" : "صادر";
                    string amount = tx.Type == TransactionType.Income
                        ? $"+{tx.Amount:N2}"
                        : $"-{tx.Amount:N2}";
                    _recentGrid.Rows.Add(
                        tx.TransactionDate.ToString("dd/MM/yyyy"),
                        tx.Description ?? "-",
                        typeAr,
                        amount,
                        $"{tx.Amount:N0}"
                    );
                }
            });
        }
        catch { /* optional */ }
    }

    // ─────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────
    private static Label BuildSectionLabel(string text, Color accent)
    {
        return new Label
        {
            Text      = text,
            Font      = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            ForeColor = TextPrimary,
            AutoSize  = false,
            Height    = 36,
            Dock      = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleRight,
            Padding   = new Padding(0, 0, 12, 0),
        };
    }

    private static Button CreateIconButton(string text, Color bg)
    {
        var btn = new Button
        {
            Text      = text,
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = bg,
            FlatStyle = FlatStyle.Flat,
            Height    = 32,
            Width     = 100,
            Cursor    = Cursors.Hand,
            UseVisualStyleBackColor = false,
        };
        btn.FlatAppearance.BorderSize  = 0;
        btn.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(bg, 0.08f);
        return btn;
    }

    private void SetLoadingState(bool loading)
    {
        SafeInvoke(() =>
        {
            _lblLastUpdate.Text = loading ? "جاري التحميل..." : _lblLastUpdate.Text;
            Cursor = loading ? Cursors.WaitCursor : Cursors.Default;
        });
    }

    private void ShowError(string msg)
    {
        SafeInvoke(() =>
            MessageBox.Show($"خطأ في تحميل البيانات:\n{msg}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Warning));
    }

    private void SafeInvoke(Action action)
    {
        if (InvokeRequired) Invoke(action);
        else action();
    }

    public static void InvalidateCache() => _cacheValid = false;
}
