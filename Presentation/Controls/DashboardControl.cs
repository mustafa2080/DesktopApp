using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace GraceWay.AccountingSystem.Presentation.Controls;

public class DashboardControl : UserControl
{
    private readonly IReservationService _reservationService;
    private readonly ICashBoxService _cashBoxService;
    private readonly ICustomerService _customerService;
    private readonly IInvoiceService _invoiceService;
    private readonly ITripService _tripService;
    private readonly IUmrahService _umrahService;
    private readonly ISupplierService _supplierService;

    private static DateTime _lastCacheTime = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private static decimal _cachedCashBalance;
    private static decimal _cachedTodaySales;
    private static decimal _cachedMonthlyRevenue;
    private static int _cachedActiveReservations;
    private static int _cachedCustomersCount;
    private static int _cachedTripsCount;
    private static int _cachedUmrahCount;
    private static int _cachedSuppliersCount;
    private static bool _cacheValid = false;

    // Main stat labels
    private Label? lblTodaySales;
    private Label? lblCashBalance;
    private Label? lblReservations;
    private Label? lblMonthlyRevenue;

    // Sub-stat labels
    private Label? lblCustomersCount;
    private Label? lblTripsCount;
    private Label? lblUmrahCount;
    private Label? lblSuppliersCount;

    // Sub-stat trend labels
    private Label? lblCustomersTrend;
    private Label? lblTripsTrend;
    private Label? lblUmrahTrend;
    private Label? lblSuppliersTrend;

    private CartesianChart? salesChart;
    private PieChart? revenueChart;

    // Quick Reports
    private Label? lblQrTodaySales, lblQrWeekSales, lblQrMonthSales, lblQrReservationsCount;
    private Label? lblQrActiveTrips, lblQrCompletedTrips, lblQrTotalBookings, lblQrOccupancyRate;
    private Label? lblQrActivePackages, lblQrPilgrimsCount, lblQrUmrahRevenue, lblQrAvgProfit;
    private Label? lblQrCurrentBalance, lblQrTodayIncome, lblQrTodayExpense, lblQrTodayNet;
    private Label? lblQrTotalCustomers, lblQrCustomersWithBalance, lblQrTotalSuppliers, lblQrSuppliersWithBalance;

    public DashboardControl(
        IReservationService reservationService,
        ICashBoxService cashBoxService,
        ICustomerService customerService,
        IInvoiceService invoiceService,
        ITripService tripService,
        IUmrahService umrahService,
        ISupplierService supplierService)
    {
        _reservationService = reservationService;
        _cashBoxService = cashBoxService;
        _customerService = customerService;
        _invoiceService = invoiceService;
        _tripService = tripService;
        _umrahService = umrahService;
        _supplierService = supplierService;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.Dock = DockStyle.Fill;
        this.BackColor = Color.FromArgb(15, 23, 42);
        this.RightToLeft = RightToLeft.Yes;
        this.AutoScroll = true;
        this.DoubleBuffered = true;
        this.Font = new Font("Cairo", 10F);
        this.Padding = new Padding(20);
        BuildUI();
        this.ResumeLayout(false);
        this.Load += async (s, e) => { await Task.Delay(10); await LoadDataAsync(); };
    }

    // ══════════════════════════════════════════════════════════
    //  BUILD UI
    // ══════════════════════════════════════════════════════════
    private void BuildUI()
    {
        var main = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            BackColor = Color.Transparent,
        };
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));   // header
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));  // main stats
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 320));  // charts
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));  // sub stats
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 420));  // quick reports

        main.Controls.Add(BuildHeader(),         0, 0);
        main.Controls.Add(BuildMainStats(),      0, 1);
        main.Controls.Add(BuildChartsSection(),  0, 2);
        main.Controls.Add(BuildSubStats(),       0, 3);
        main.Controls.Add(BuildQuickReports(),   0, 4);
        this.Controls.Add(main);
    }

    // ── Header ────────────────────────────────────────────────
    private Panel BuildHeader()
    {
        var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Margin = new Padding(0,0,0,14) };
        p.Paint += (s, e) =>
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            if (p.Width < 1 || p.Height < 1) return;
            using var path = Pill(new Rectangle(0,0,p.Width-1,p.Height-1), 14);
            using var br = new LinearGradientBrush(p.ClientRectangle,
                Color.FromArgb(30,58,138), Color.FromArgb(37,99,235), LinearGradientMode.Horizontal);
            g.FillPath(br, path);
            // sheen
            using var sh = new SolidBrush(Color.FromArgb(18,255,255,255));
            g.FillRectangle(sh, new Rectangle(0,0,p.Width,p.Height/2));
        };

        var title = Lbl("🎯  لوحة التحكم", "Cairo", 15, FontStyle.Bold, Color.White);
        title.Location = new Point(20, 16);
        var date  = Lbl($"📅  {DateTime.Now:dddd، dd MMMM yyyy}", "Cairo", 9.5f, FontStyle.Regular,
                        Color.FromArgb(190,220,255));
        date.Location = new Point(20, 46);

        var btn = new Button
        {
            Text = "🔄  تحديث", Font = new Font("Cairo",9.5f,FontStyle.Bold),
            ForeColor = Color.White, BackColor = Color.FromArgb(16,185,129),
            FlatStyle = FlatStyle.Flat, Size = new Size(110,36), Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.CornerRadius(10);
        p.Resize += (_,__) => btn.Location = new Point(p.Width - 130, (p.Height-36)/2);
        btn.Click += async (_,__) =>
        {
            btn.Enabled = false; btn.Text = "⏳ جاري...";
            try   { _cacheValid=false; await LoadDataAsync(); btn.Text="✅ تم"; await Task.Delay(800); }
            catch { btn.Text="❌ خطأ"; await Task.Delay(800); }
            finally { btn.Text="🔄  تحديث"; btn.Enabled=true; }
        };
        p.Controls.AddRange(new Control[]{ title, date, btn });
        return p;
    }

    // ── Main 4 stat cards ─────────────────────────────────────
    private TableLayoutPanel BuildMainStats()
    {
        var tl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1,
            BackColor = Color.Transparent, Margin = new Padding(0,0,0,14)
        };
        for (int i = 0; i < 4; i++)
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        tl.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        tl.Controls.Add(MainCard("💰","مبيعات اليوم",   "---", Color.FromArgb(16,185,129), Color.FromArgb(5,150,105),  out lblTodaySales),    0,0);
        tl.Controls.Add(MainCard("💵","رصيد الخزينة",   "---", Color.FromArgb(96,165,250), Color.FromArgb(37,99,235),  out lblCashBalance),   1,0);
        tl.Controls.Add(MainCard("✈️","الحجوزات النشطة","---", Color.FromArgb(251,146,60), Color.FromArgb(234,88,12),  out lblReservations),  2,0);
        tl.Controls.Add(MainCard("📈","إيرادات الشهر",  "---", Color.FromArgb(192,132,252),Color.FromArgb(147,51,234), out lblMonthlyRevenue),3,0);
        return tl;
    }

    private Panel MainCard(string icon, string title, string val,
                           Color c1, Color c2, out Label valLbl)
    {
        var card = new Panel { Dock=DockStyle.Fill, BackColor=Color.Transparent, Margin=new Padding(6) };
        bool hov = false;

        card.Paint += (s,e) =>
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            if (card.Width<1||card.Height<1) return;
            var r = new Rectangle(0,0,card.Width-1,card.Height-1);
            using var path = Pill(r,14);
            var cc1 = hov ? Lighten(c1,15) : c1;
            var cc2 = hov ? Lighten(c2,10) : c2;
            using var br = new LinearGradientBrush(r, cc1, cc2, LinearGradientMode.ForwardDiagonal);
            g.FillPath(br, path);
            using var sh = new SolidBrush(Color.FromArgb(22,255,255,255));
            using var sp = Pill(new Rectangle(1,1,card.Width-3,card.Height/2),13);
            g.FillPath(sh, sp);
            using var pen = new Pen(Color.FromArgb(40,255,255,255),1.5f);
            g.DrawPath(pen, path);
        };

        var ib = new Panel { Size=new Size(44,44), BackColor=Color.Transparent };
        ib.Paint += (_,e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var p = Pill(new Rectangle(0,0,44,44),10);
            using var b = new SolidBrush(Color.FromArgb(55,255,255,255));
            e.Graphics.FillPath(b,p);
        };
        var ico = Lbl(icon,"Segoe UI Emoji",18,FontStyle.Regular,Color.White);
        ico.Size=new Size(44,44); ico.TextAlign=ContentAlignment.MiddleCenter;
        ib.Controls.Add(ico);

        var lTitle = Lbl(title,"Cairo",9f,FontStyle.Regular,Color.FromArgb(220,255,255,255));
        lTitle.AutoSize=false; lTitle.Size=new Size(10,18); lTitle.TextAlign=ContentAlignment.MiddleRight;

        var vLbl = Lbl(val,"Cairo",20f,FontStyle.Bold,Color.White);
        vLbl.AutoSize=false; vLbl.Size=new Size(10,34); vLbl.TextAlign=ContentAlignment.MiddleRight;
        valLbl = vLbl; // assign out param immediately (no lambda reference)

        void LayoutMainCard()
        {
            if (card.Width < 10 || card.Height < 10) return;
            ib.Location     = new Point(card.Width - 52, (card.Height - 44) / 2);
            lTitle.Location = new Point(10, 18);
            lTitle.Size     = new Size(card.Width - 62, 20);
            vLbl.Location   = new Point(10, 42);
            vLbl.Size       = new Size(card.Width - 62, 50);
        }
        card.Resize += (_,__) => LayoutMainCard();
        card.HandleCreated += (_,__) => LayoutMainCard();

        void hover(bool on) { hov=on; card.Invalidate(); }
        card.MouseEnter+=(_,__)=>hover(true);  card.MouseLeave+=(_,__)=>hover(false);
        foreach(Control c in ib.Controls) { c.MouseEnter+=(_,__)=>hover(true); c.MouseLeave+=(_,__)=>hover(false); }
        ib.MouseEnter+=(_,__)=>hover(true);    ib.MouseLeave+=(_,__)=>hover(false);

        card.Controls.AddRange(new Control[]{ ib, lTitle, vLbl });
        return card;
    }

    // ── Charts Section ────────────────────────────────────────
    private TableLayoutPanel BuildChartsSection()
    {
        var tl = new TableLayoutPanel
        {
            Dock=DockStyle.Fill, ColumnCount=2, RowCount=1,
            BackColor=Color.Transparent, Margin=new Padding(0,0,0,14)
        };
        tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,62f));
        tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,38f));
        tl.RowStyles.Add(new RowStyle(SizeType.Percent,100f));

        // Sales chart panel
        var sp = ChartPanel("📊  اتجاه المبيعات (آخر 6 أشهر)", Color.FromArgb(96,165,250));
        salesChart = new CartesianChart { Dock=DockStyle.Fill, BackColor=Color.FromArgb(22,36,62) };
        sp.Controls.Add(salesChart);
        tl.Controls.Add(sp, 0, 0);

        // Revenue pie panel
        var rp = ChartPanel("🎯  توزيع الإيرادات", Color.FromArgb(192,132,252));
        revenueChart = new PieChart
        {
            Dock=DockStyle.Fill, BackColor=Color.FromArgb(22,36,62),
            LegendPosition=LiveChartsCore.Measure.LegendPosition.Bottom
        };
        rp.Controls.Add(revenueChart);
        tl.Controls.Add(rp, 1, 0);

        return tl;
    }

    private Panel ChartPanel(string title, Color accent)
    {
        var p = new Panel { Dock=DockStyle.Fill, BackColor=Color.FromArgb(22,36,62),
                            Margin=new Padding(6), Padding=new Padding(1) };
        p.Paint += (s,e) =>
        {
            var g=e.Graphics; g.SmoothingMode=SmoothingMode.AntiAlias;
            if(p.Width<1||p.Height<1) return;
            using var path=Pill(new Rectangle(0,0,p.Width-1,p.Height-1),14);
            using var br=new SolidBrush(Color.FromArgb(22,36,62));
            g.FillPath(br,path);
            using var pen=new Pen(Color.FromArgb(40,99,150,220),1);
            g.DrawPath(pen,path);
        };

        var hdr = new Panel { Dock=DockStyle.Top, Height=46, BackColor=Color.Transparent };
        hdr.Paint += (s,e) =>
        {
            using var br = new SolidBrush(accent);
            e.Graphics.FillRectangle(br, 0, 0, hdr.Width, 4);
            using var sep = new SolidBrush(Color.FromArgb(30,99,150,220));
            e.Graphics.FillRectangle(sep, 0, 44, hdr.Width, 2);
        };
        var lbl = Lbl(title,"Cairo",11.5f,FontStyle.Bold,accent);
        lbl.Location=new Point(14,10); lbl.BackColor=Color.Transparent;
        hdr.Controls.Add(lbl);
        p.Controls.Add(hdr);
        return p;
    }

    // ── Sub Stats (4 info cards) ──────────────────────────────
    private TableLayoutPanel BuildSubStats()
    {
        var tl = new TableLayoutPanel
        {
            Dock=DockStyle.Fill, ColumnCount=4, RowCount=1,
            BackColor=Color.Transparent, Margin=new Padding(0,0,0,14)
        };
        for(int i=0;i<4;i++) tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25f));
        tl.RowStyles.Add(new RowStyle(SizeType.Percent,100f));

        tl.Controls.Add(SubCard("👥","العملاء",      "---","",Color.FromArgb(20,184,166), Color.FromArgb(13,148,136), out lblCustomersCount, out lblCustomersTrend),0,0);
        tl.Controls.Add(SubCard("🌍","الرحلات",      "---","",Color.FromArgb(96,165,250), Color.FromArgb(37,99,235),  out lblTripsCount,     out lblTripsTrend),     1,0);
        tl.Controls.Add(SubCard("🕌","باقات العمرة", "---","",Color.FromArgb(251,146,60), Color.FromArgb(194,65,12),  out lblUmrahCount,     out lblUmrahTrend),     2,0);
        tl.Controls.Add(SubCard("📦","الموردين",     "---","",Color.FromArgb(167,139,250),Color.FromArgb(124,58,237), out lblSuppliersCount, out lblSuppliersTrend), 3,0);
        return tl;
    }

    private Panel SubCard(string icon, string title, string val, string trend,
                          Color c1, Color c2, out Label valLbl, out Label trendLbl)
    {
        var card = new Panel { Dock=DockStyle.Fill, BackColor=Color.Transparent, Margin=new Padding(6) };
        card.Paint += (s,e) =>
        {
            var g=e.Graphics; g.SmoothingMode=SmoothingMode.AntiAlias;
            if(card.Width<1||card.Height<1) return;
            var r=new Rectangle(0,0,card.Width-1,card.Height-1);
            using var path=Pill(r,14);
            using var br=new SolidBrush(Color.FromArgb(22,36,62));
            g.FillPath(br,path);
            using var pen=new Pen(Color.FromArgb(40,99,150,220),1);
            g.DrawPath(pen,path);
            using var ab=new LinearGradientBrush(new Rectangle(0,20,5,card.Height-40),c1,c2,90f);
            g.FillRectangle(ab, new Rectangle(0,20,5,card.Height-40));
        };

        var circle = new Panel { Size=new Size(52,52), BackColor=Color.Transparent };
        circle.Paint += (_,e) =>
        {
            e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
            using var path=new GraphicsPath(); path.AddEllipse(0,0,52,52);
            using var br=new LinearGradientBrush(new Rectangle(0,0,52,52),
                Color.FromArgb(80,c1.R,c1.G,c1.B), Color.FromArgb(50,c2.R,c2.G,c2.B),45f);
            e.Graphics.FillPath(br,path);
            using var pen=new Pen(Color.FromArgb(60,c1.R,c1.G,c1.B),1.5f);
            e.Graphics.DrawPath(pen,path);
        };
        var ico = Lbl(icon,"Segoe UI Emoji",20,FontStyle.Regular,c1);
        ico.Size=new Size(52,52); ico.TextAlign=ContentAlignment.MiddleCenter; ico.BackColor=Color.Transparent;
        circle.Controls.Add(ico);

        var lTitle = Lbl(title,"Cairo",9.5f,FontStyle.Regular,Color.FromArgb(148,180,210));
        lTitle.AutoSize=false;

        var vLbl = Lbl(val, "Cairo",18f,FontStyle.Bold,Color.White);
        vLbl.AutoSize=false;
        valLbl = vLbl; // assign out param before any lambda

        var tLbl = Lbl(trend,"Cairo",8.5f,FontStyle.Regular,Color.FromArgb(100,160,200));
        tLbl.AutoSize=false;
        trendLbl = tLbl; // assign out param before any lambda

        void LayoutSubCard()
        {
            if (card.Width < 10 || card.Height < 10) return;
            circle.Location = new Point(14, (card.Height - 52) / 2);
            int tx = 76;
            lTitle.Location = new Point(tx, 18);          lTitle.Size = new Size(card.Width - tx - 10, 20);
            vLbl.Location   = new Point(tx, 42);           vLbl.Size   = new Size(card.Width - tx - 10, 34);
            tLbl.Location   = new Point(tx, 80);           tLbl.Size   = new Size(card.Width - tx - 10, 20);
        }
        card.Resize      += (_,__) => LayoutSubCard();
        card.HandleCreated += (_,__) => LayoutSubCard();

        card.Controls.AddRange(new Control[]{ circle, lTitle, vLbl, tLbl });
        return card;
    }

    // ── Quick Reports (5 cards) ───────────────────────────────
    private Panel BuildQuickReports()
    {
        var container = new Panel { Dock=DockStyle.Fill, BackColor=Color.Transparent };

        // section header
        var hdr = new Panel { Dock=DockStyle.Top, Height=50, BackColor=Color.Transparent };
        hdr.Paint += (s,e) =>
        {
            var g=e.Graphics; g.SmoothingMode=SmoothingMode.AntiAlias;
            if(hdr.Width<1||hdr.Height<1) return;
            using var path=Pill(new Rectangle(0,0,hdr.Width-1,hdr.Height-1),12);
            using var br=new SolidBrush(Color.FromArgb(22,36,62));
            g.FillPath(br,path);
            using var ab=new LinearGradientBrush(new Rectangle(0,0,6,hdr.Height),
                Color.FromArgb(59,130,246),Color.FromArgb(37,99,235),90f);
            g.FillRectangle(ab,0,0,6,hdr.Height);
        };
        var hLbl1=Lbl("📊","Segoe UI Emoji",18,FontStyle.Regular,Color.FromArgb(96,165,250));
        hLbl1.Location=new Point(18,12);
        var hLbl2=Lbl("تقارير سريعة","Cairo",13f,FontStyle.Bold,Color.White);
        hLbl2.Location=new Point(52,10);
        var hLbl3=Lbl("نظرة شاملة على أداء جميع الأقسام","Cairo",9f,FontStyle.Regular,Color.FromArgb(120,170,210));
        hLbl3.Location=new Point(52,32);
        hdr.Controls.AddRange(new Control[]{hLbl1,hLbl2,hLbl3});
        container.Controls.Add(hdr);

        var tl = new TableLayoutPanel
        {
            Dock=DockStyle.Fill, ColumnCount=5, RowCount=1,
            BackColor=Color.Transparent, Padding=new Padding(0,54,0,0)
        };
        for(int i=0;i<5;i++) tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,20f));
        tl.RowStyles.Add(new RowStyle(SizeType.Percent,100f));

        tl.Controls.Add(QCard("💰","المبيعات والإيرادات", Color.FromArgb(16,185,129),
            new[]{"مبيعات اليوم","مبيعات الأسبوع","مبيعات الشهر","عدد الحجوزات"},
            out lblQrTodaySales, out lblQrWeekSales, out lblQrMonthSales, out lblQrReservationsCount), 0,0);

        tl.Controls.Add(QCard("✈️","تقرير الرحلات", Color.FromArgb(96,165,250),
            new[]{"رحلات نشطة","رحلات مكتملة","إجمالي الحجوزات","نسبة الإشغال"},
            out lblQrActiveTrips, out lblQrCompletedTrips, out lblQrTotalBookings, out lblQrOccupancyRate), 1,0);

        tl.Controls.Add(QCard("🕌","تقرير العمرة", Color.FromArgb(251,146,60),
            new[]{"باقات نشطة","عدد المعتمرين","إجمالي الإيرادات","متوسط الربح"},
            out lblQrActivePackages, out lblQrPilgrimsCount, out lblQrUmrahRevenue, out lblQrAvgProfit), 2,0);

        tl.Controls.Add(QCard("🏦","تقرير الخزينة", Color.FromArgb(167,139,250),
            new[]{"الرصيد الحالي","إيرادات اليوم","مصروفات اليوم","صافي اليوم"},
            out lblQrCurrentBalance, out lblQrTodayIncome, out lblQrTodayExpense, out lblQrTodayNet), 3,0);

        tl.Controls.Add(QCard("👥","العملاء والموردين", Color.FromArgb(20,184,166),
            new[]{"إجمالي العملاء","عملاء لهم رصيد","إجمالي الموردين","موردين لهم رصيد"},
            out lblQrTotalCustomers, out lblQrCustomersWithBalance, out lblQrTotalSuppliers, out lblQrSuppliersWithBalance), 4,0);

        container.Controls.Add(tl);
        return container;
    }

    private Panel QCard(string icon, string title, Color accent, string[] labels,
        out Label? v1, out Label? v2, out Label? v3, out Label? v4)
    {
        var card = new Panel { Dock=DockStyle.Fill, BackColor=Color.Transparent, Margin=new Padding(6) };
        card.Paint += (s,e) =>
        {
            var g=e.Graphics; g.SmoothingMode=SmoothingMode.AntiAlias;
            if(card.Width<1||card.Height<1) return;
            var r=new Rectangle(0,0,card.Width-1,card.Height-1);
            using var path=Pill(r,14);
            using var br=new SolidBrush(Color.FromArgb(22,36,62));
            g.FillPath(br,path);
            using var pen=new Pen(Color.FromArgb(35,55,90),1);
            g.DrawPath(pen,path);
            // top accent
            using var ab=new LinearGradientBrush(new Rectangle(0,0,card.Width,5),
                accent, Lighten(accent,20), LinearGradientMode.Horizontal);
            using var ap=Pill(new Rectangle(0,0,card.Width,5),14);
            g.FillPath(ab,ap);
            // header bg
            using var hbr=new SolidBrush(Color.FromArgb(15,255,255,255));
            g.FillRectangle(hbr, new Rectangle(0,5,card.Width,50));
            // separator
            using var sep=new Pen(Color.FromArgb(30,99,150,220),1);
            g.DrawLine(sep, 12, 55, card.Width-12, 55);
        };

        // icon+title header — fixed initial sizes
        var ico=Lbl(icon,"Segoe UI Emoji",14,FontStyle.Regular,accent);
        ico.Location=new Point(12,12); ico.BackColor=Color.Transparent; ico.Size=new Size(28,28);
        var ttl=Lbl(title,"Cairo",10f,FontStyle.Bold,accent);
        ttl.Location=new Point(44,10); ttl.BackColor=Color.Transparent; ttl.AutoSize=false;
        ttl.Size=new Size(200,32);

        void LayoutQCard() { if(card.Width>10) ttl.Size=new Size(Math.Max(20,card.Width-56),32); }
        card.Resize      += (_,__) => LayoutQCard();
        card.HandleCreated += (_,__) => LayoutQCard();
        card.Controls.AddRange(new Control[]{ico, ttl});

        // 4 data rows — fixed Y positions, no card.Height dependency
        var vals = new Label?[4];
        int[] rowYs = { 60, 132, 204, 276 };   // fixed positions

        for(int i=0;i<4;i++)
        {
            int rowY = rowYs[i];

            var dot = new Panel { Size=new Size(8,8), Location=new Point(10,rowY+8), BackColor=Color.Transparent };
            dot.Paint += (_,e) =>
            {
                e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
                using var br=new SolidBrush(accent);
                e.Graphics.FillEllipse(br,0,0,8,8);
            };

            var lName = Lbl(labels[i],"Cairo",8.5f,FontStyle.Regular,Color.FromArgb(140,180,215));
            lName.AutoSize=false; lName.Location=new Point(22,rowY+4); lName.Size=new Size(200,18);

            var lVal = Lbl("---","Cairo",13f,FontStyle.Bold,Color.White);
            lVal.AutoSize=false; lVal.Location=new Point(10,rowY+24);
            lVal.Size=new Size(200,26); lVal.TextAlign=ContentAlignment.MiddleRight;

            int ci = i; // capture
            void LayoutRow()
            {
                if(card.Width<10) return;
                lName.Size = new Size(card.Width-28, 18);
                lVal.Size  = new Size(card.Width-20, 26);
            }
            card.Resize      += (_,__) => LayoutRow();
            card.HandleCreated += (_,__) => LayoutRow();

            card.Controls.AddRange(new Control[]{dot, lName, lVal});
            vals[i]=lVal;
        }
        v1=vals[0]; v2=vals[1]; v3=vals[2]; v4=vals[3];
        return card;
    }

    // ══════════════════════════════════════════════════════════
    //  DATA LOADING
    // ══════════════════════════════════════════════════════════
    private async Task LoadDataAsync()
    {
        if (_cacheValid && (DateTime.Now-_lastCacheTime)<CacheDuration)
        {
            SafeUI(() =>
            {
                if(lblCashBalance    !=null) lblCashBalance.Text    = FormatCurrency(_cachedCashBalance);
                if(lblTodaySales     !=null) lblTodaySales.Text     = FormatCurrency(_cachedTodaySales);
                if(lblMonthlyRevenue !=null) lblMonthlyRevenue.Text = FormatCurrency(_cachedMonthlyRevenue);
                if(lblReservations   !=null) lblReservations.Text   = $"{_cachedActiveReservations} حجز";
                if(lblCustomersCount !=null) lblCustomersCount.Text = $"{_cachedCustomersCount} عميل";
                if(lblTripsCount     !=null) lblTripsCount.Text     = $"{_cachedTripsCount} رحلة";
                if(lblUmrahCount     !=null) lblUmrahCount.Text     = $"{_cachedUmrahCount} باقة";
                if(lblSuppliersCount !=null) lblSuppliersCount.Text = $"{_cachedSuppliersCount} مورد";
            });
            return;
        }
        // ⚠️ EF Core DbContext غير thread-safe - نستخدم sequential
        // لكن نعرض البيانات تدريجياً فور توفرها
        await LoadMainStatsAsync();
        await LoadOtherStatsAsync();
        await LoadChartsAsync();
        await LoadQuickReportsAsync();
        _lastCacheTime=DateTime.Now; _cacheValid=true;
    }

    public static void InvalidateCache() => _cacheValid = false;

    private async Task LoadMainStatsAsync()
    {
        // ⚠️ DbContext غير thread-safe - sequential queries
        try
        {
            var boxes = await _cashBoxService.GetAllCashBoxesAsync();
            var active = boxes?.Where(c=>c.IsActive).ToList();
            decimal cashBalance=0, todaySales=0, monthlyRevenue=0;
            if(active!=null && active.Any())
            {
                cashBalance = active.Sum(c=>c.CurrentBalance);
                var today=DateTime.Today;
                var tomorrowUtc= DateTime.SpecifyKind(today.AddDays(1),DateTimeKind.Utc);
                var monthStart = DateTime.SpecifyKind(new DateTime(today.Year,today.Month,1),DateTimeKind.Utc);
                foreach(var box in active)
                {
                    var txs = await _cashBoxService.GetTransactionsByCashBoxAsync(box.Id, monthStart, tomorrowUtc);
                    if(txs!=null)
                    {
                        monthlyRevenue += txs.Where(t=>t.Type==TransactionType.Income&&!t.IsDeleted).Sum(t=>t.Amount);
                        todaySales     += txs.Where(t=>t.Type==TransactionType.Income&&!t.IsDeleted&&t.TransactionDate.Date==today).Sum(t=>t.Amount);
                    }
                }
            }
            SafeUI(()=>
            {
                if(lblCashBalance    !=null) lblCashBalance.Text    = FormatCurrency(cashBalance);
                if(lblTodaySales     !=null) lblTodaySales.Text     = FormatCurrency(todaySales);
                if(lblMonthlyRevenue !=null) lblMonthlyRevenue.Text = FormatCurrency(monthlyRevenue);
            });
            _cachedCashBalance=cashBalance; _cachedTodaySales=todaySales; _cachedMonthlyRevenue=monthlyRevenue;
        }
        catch(Exception ex){ Debug(ex,"cash"); SafeUI(()=>{ if(lblCashBalance!=null)lblCashBalance.Text="0"; if(lblTodaySales!=null)lblTodaySales.Text="0"; if(lblMonthlyRevenue!=null)lblMonthlyRevenue.Text="0"; }); }

        try
        {
            var res = await _reservationService.GetAllReservationsAsync();
            int activeReservations = res?.Count(r=>r.Status=="Confirmed") ?? 0;
            SafeUI(()=>{ if(lblReservations!=null) lblReservations.Text=$"{activeReservations} حجز"; });
            _cachedActiveReservations=activeReservations;
        }
        catch(Exception ex){ Debug(ex,"reservations"); SafeUI(()=>{ if(lblReservations!=null)lblReservations.Text="0 حجز"; }); }
    }

    private async Task LoadOtherStatsAsync()
    {
        // ⚠️ DbContext غير thread-safe - sequential queries
        try
        {
            var c = await _customerService.GetAllCustomersAsync();
            int cnt = c?.Count ?? 0; _cachedCustomersCount=cnt;
            SafeUI(()=>{ if(lblCustomersCount!=null) lblCustomersCount.Text=cnt==0?"لا يوجد":$"{cnt} عميل"; if(lblCustomersTrend!=null) lblCustomersTrend.Text=cnt>0?"عملاء نشطون":""; });
        } catch(Exception ex){ Debug(ex,"customers"); }

        try
        {
            var t = await _tripService.GetAllTripsAsync();
            int act=t?.Count(x=>x.Status==TripStatus.Confirmed||x.Status==TripStatus.Unconfirmed)??0;
            int all=t?.Count??0; int cnt=act>0?act:all; _cachedTripsCount=cnt;
            SafeUI(()=>{ if(lblTripsCount!=null) lblTripsCount.Text=all==0?"لا توجد":act>0?$"{act} نشطة":$"{all} رحلة"; if(lblTripsTrend!=null) lblTripsTrend.Text=all>0?$"الإجمالي: {all} رحلة":""; });
        } catch(Exception ex){ Debug(ex,"trips"); }

        try
        {
            var s = await _umrahService.GetPackageStatisticsAsync();
            _cachedUmrahCount=s.ActivePackages;
            SafeUI(()=>{ if(lblUmrahCount!=null) lblUmrahCount.Text=s.ActivePackages==0?"لا توجد":$"{s.ActivePackages} باقة"; if(lblUmrahTrend!=null) lblUmrahTrend.Text=s.TotalPilgrims>0?$"{s.TotalPilgrims} معتمر":""; });
        } catch(Exception ex){ Debug(ex,"umrah"); }

        try
        {
            var sp = await _supplierService.GetAllSuppliersAsync();
            int cnt=sp?.Count??0; _cachedSuppliersCount=cnt;
            SafeUI(()=>{ if(lblSuppliersCount!=null) lblSuppliersCount.Text=cnt==0?"لا يوجد":$"{cnt} مورد"; if(lblSuppliersTrend!=null) lblSuppliersTrend.Text=cnt>0?"مورد نشط":""; });
        } catch(Exception ex){ Debug(ex,"suppliers"); }
    }

    private async Task LoadChartsAsync()
    {
        try
        {
            var res = await _reservationService.GetAllReservationsAsync();
            if(res!=null) { LoadSalesChart(res); LoadRevenueChart(res); }
        }
        catch(Exception ex){ Debug(ex,"charts"); }
    }

    private void LoadSalesChart(IEnumerable<Reservation> reservations)
    {
        var ago = DateTime.Today.AddMonths(-6);
        var months = new[]{"يناير","فبراير","مارس","إبريل","مايو","يونيو","يوليو","أغسطس","سبتمبر","أكتوبر","نوفمبر","ديسمبر"};
        var data = reservations
            .Where(r=>r.CreatedAt>=ago&&(r.Status=="Confirmed"||r.Status=="Completed"||r.Status=="Paid"))
            .GroupBy(r=>new{r.CreatedAt.Year,r.CreatedAt.Month})
            .Select(g=>new{g.Key.Year,g.Key.Month,Total=(double)g.Sum(r=>r.SellingPrice)})
            .OrderBy(x=>x.Year).ThenBy(x=>x.Month).ToList();

        SafeUI(()=>
        {
            if(salesChart==null) return;
            salesChart.Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = data.Any()?data.Select(d=>d.Total).ToArray():new[]{0.0},
                    Fill   = new SolidColorPaint(new SKColor(96,165,250)),
                    MaxBarWidth = 40, Name = "المبيعات",
                    Rx = 4, Ry = 4
                }
            };
            salesChart.XAxes = new[]{ new Axis
            {
                Labels = data.Any()?data.Select(d=>months[d.Month-1]).ToArray():new[]{"لا توجد بيانات"},
                TextSize = 11,
                LabelsPaint = new SolidColorPaint(new SKColor(148,180,215)),
                SeparatorsPaint = new SolidColorPaint(new SKColor(40,60,100))
            }};
            salesChart.YAxes = new[]{ new Axis
            {
                Labeler = v=>$"{v:N0}",
                TextSize = 10, MinLimit = 0,
                LabelsPaint = new SolidColorPaint(new SKColor(148,180,215)),
                SeparatorsPaint = new SolidColorPaint(new SKColor(40,60,100))
            }};
            salesChart.LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden;
        });
    }

    private void LoadRevenueChart(IEnumerable<Reservation> reservations)
    {
        var data = reservations
            .Where(r=>r.CreatedAt>=DateTime.Today.AddMonths(-1)&&(r.Status=="Confirmed"||r.Status=="Completed"||r.Status=="Paid"))
            .GroupBy(r=>r.ServiceType?.ServiceTypeName??"غير محدد")
            .Select(g=>new{Name=g.Key,Total=(double)g.Sum(r=>r.SellingPrice)})
            .OrderByDescending(x=>x.Total).Take(5).ToList();

        var colors = new SKColor[]{new(96,165,250),new(34,197,94),new(251,146,60),new(239,68,68),new(192,132,252)};
        SafeUI(()=>
        {
            if(revenueChart==null) return;
            revenueChart.Series = data.Count==0
                ? new ISeries[]{ new PieSeries<double>{ Values=new[]{1.0}, Name="لا توجد بيانات", Fill=new SolidColorPaint(new SKColor(40,55,80)) }}
                : data.Select((d,i)=>new PieSeries<double>
                  {
                      Values=new[]{d.Total}, Name=d.Name,
                      Fill=new SolidColorPaint(colors[i%colors.Length])
                  }).ToArray();
        });
    }

    private async Task LoadQuickReportsAsync()
    {
        // ⚠️ DbContext غير thread-safe - sequential queries
        // Sales report
        try
        {
            var res = await _reservationService.GetAllReservationsAsync();
            if(res!=null)
            {
                var today=DateTime.Today; var wk=today.AddDays(-(int)today.DayOfWeek); var mn=new DateTime(today.Year,today.Month,1);
                var ok=res.Where(r=>r.Status=="Confirmed"||r.Status=="Completed"||r.Status=="Paid");
                SafeUI(()=>
                {
                    if(lblQrTodaySales       !=null) lblQrTodaySales.Text       = FormatCurrency(ok.Where(r=>r.ReservationDate.Date==today).Sum(r=>r.SellingPrice));
                    if(lblQrWeekSales        !=null) lblQrWeekSales.Text        = FormatCurrency(ok.Where(r=>r.ReservationDate.Date>=wk).Sum(r=>r.SellingPrice));
                    if(lblQrMonthSales       !=null) lblQrMonthSales.Text       = FormatCurrency(ok.Where(r=>r.ReservationDate>=mn).Sum(r=>r.SellingPrice));
                    if(lblQrReservationsCount!=null) lblQrReservationsCount.Text= $"{res.Count} حجز";
                });
            }
        } catch(Exception ex){ Debug(ex,"qr-sales"); }

        // Trips report
        try
        {
            var trips = await _tripService.GetAllTripsAsync();
            if(trips!=null)
            {
                var act=trips.Count(t=>t.Status==TripStatus.Confirmed||t.Status==TripStatus.Unconfirmed);
                var cmp=trips.Count(t=>t.Status==TripStatus.Completed);
                var cap=trips.Where(t=>t.Status==TripStatus.Confirmed).Sum(t=>t.TotalCapacity);
                var bkd=trips.Where(t=>t.Status==TripStatus.Confirmed).Sum(t=>t.BookedSeats);
                var occ=cap>0?(double)bkd/cap*100:0;
                SafeUI(()=>
                {
                    if(lblQrActiveTrips   !=null) lblQrActiveTrips.Text   = $"{act} رحلة";
                    if(lblQrCompletedTrips!=null) lblQrCompletedTrips.Text= $"{cmp} رحلة";
                    if(lblQrTotalBookings !=null) lblQrTotalBookings.Text  = $"{bkd} حجز";
                    if(lblQrOccupancyRate !=null){ lblQrOccupancyRate.Text=$"{occ:N0}%"; lblQrOccupancyRate.ForeColor=occ>=70?Color.FromArgb(34,197,94):occ>=40?Color.FromArgb(234,179,8):Color.FromArgb(239,68,68); }
                });
            }
        } catch(Exception ex){ Debug(ex,"qr-trips"); }

        // Umrah report
        try
        {
            var u = await _umrahService.GetPackageStatisticsAsync();
            SafeUI(()=>
            {
                if(lblQrActivePackages!=null) lblQrActivePackages.Text= $"{u.ActivePackages} باقة";
                if(lblQrPilgrimsCount !=null) lblQrPilgrimsCount.Text = $"{u.TotalPilgrims} معتمر";
                if(lblQrUmrahRevenue  !=null) lblQrUmrahRevenue.Text  = FormatCurrency(u.TotalRevenue);
                if(lblQrAvgProfit     !=null){
                    var avg=u.TotalPackages>0?u.TotalProfit/u.TotalPackages:0;
                    lblQrAvgProfit.Text=FormatCurrency(avg);
                    lblQrAvgProfit.ForeColor=avg>=0?Color.FromArgb(34,197,94):Color.FromArgb(239,68,68);
                }
            });
        } catch(Exception ex){ Debug(ex,"qr-umrah"); }

        // CashBox report - sequential per box
        try
        {
            var boxes = await _cashBoxService.GetAllCashBoxesAsync();
            if(boxes!=null&&boxes.Any())
            {
                var activeBoxes = boxes.Where(c=>c.IsActive).ToList();
                var bal=activeBoxes.Sum(c=>c.CurrentBalance);
                decimal inc=0, exp=0;
                foreach(var cb in activeBoxes)
                {
                    try
                    {
                        var txs = await _cashBoxService.GetTransactionsByCashBoxAsync(cb.Id,DateTime.Today,DateTime.Today.AddDays(1).AddSeconds(-1));
                        if(txs!=null){ inc+=txs.Where(t=>t.Type==TransactionType.Income).Sum(t=>t.Amount); exp+=txs.Where(t=>t.Type==TransactionType.Expense).Sum(t=>t.Amount); }
                    } catch{ }
                }
                var net=inc-exp;
                SafeUI(()=>
                {
                    if(lblQrCurrentBalance!=null) lblQrCurrentBalance.Text= FormatCurrency(bal);
                    if(lblQrTodayIncome   !=null){ lblQrTodayIncome.Text=FormatCurrency(inc);   lblQrTodayIncome.ForeColor=inc>0?Color.FromArgb(34,197,94):Color.White; }
                    if(lblQrTodayExpense  !=null){ lblQrTodayExpense.Text=FormatCurrency(exp);  lblQrTodayExpense.ForeColor=exp>0?Color.FromArgb(239,68,68):Color.White; }
                    if(lblQrTodayNet      !=null){ lblQrTodayNet.Text=(net>=0?"+":"-")+" "+FormatCurrency(Math.Abs(net)); lblQrTodayNet.ForeColor=net>=0?Color.FromArgb(34,197,94):Color.FromArgb(239,68,68); }
                });
            }
        } catch(Exception ex){ Debug(ex,"qr-cash"); }

        // Customers
        try
        {
            var c=await _customerService.GetAllCustomersAsync();
            var cwb=await _customerService.GetCustomersWithBalanceAsync();
            SafeUI(()=>{ if(lblQrTotalCustomers!=null) lblQrTotalCustomers.Text=$"{c?.Count??0} عميل"; if(lblQrCustomersWithBalance!=null) lblQrCustomersWithBalance.Text=$"{cwb?.Count??0} عميل"; });
        } catch(Exception ex){ Debug(ex,"qr-cust"); }

        // Suppliers
        try
        {
            var s=await _supplierService.GetAllSuppliersAsync();
            var swb=await _supplierService.GetSuppliersWithBalanceAsync();
            SafeUI(()=>{ if(lblQrTotalSuppliers!=null) lblQrTotalSuppliers.Text=$"{s?.Count??0} مورد"; if(lblQrSuppliersWithBalance!=null) lblQrSuppliersWithBalance.Text=$"{swb?.Count??0} مورد"; });
        } catch(Exception ex){ Debug(ex,"qr-supp"); }
    }

    // ══════════════════════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════════════════════
    private void SafeUI(Action a)
    {
        if(IsDisposed||Disposing) return;
        try { if(InvokeRequired) Invoke(new Action(()=>{ if(!IsDisposed&&!Disposing) a(); })); else { if(!IsDisposed&&!Disposing) a(); } }
        catch(ObjectDisposedException){ }
        catch(Exception ex){ Console.WriteLine($"SafeUI error: {ex.Message}"); }
    }

    private static void Debug(Exception ex, string tag) =>
        System.Diagnostics.Debug.WriteLine($"[Dashboard/{tag}] {ex.Message}");

    private static string FormatCurrency(decimal v)
    {
        if(v>=1_000_000) return $"{v/1_000_000:N1} م ج.م";
        if(v>=1_000)     return $"{v/1_000:N0} ألف ج.م";
        return $"{v:N0} ج.م";
    }

    private static GraphicsPath Pill(Rectangle r, int radius)
    {
        var p=new GraphicsPath(); int d=radius*2;
        p.AddArc(r.X,       r.Y,        d,d,180,90);
        p.AddArc(r.Right-d, r.Y,        d,d,270,90);
        p.AddArc(r.Right-d, r.Bottom-d, d,d,  0,90);
        p.AddArc(r.X,       r.Bottom-d, d,d, 90,90);
        p.CloseFigure(); return p;
    }

    private static Color Lighten(Color c,int a) =>
        Color.FromArgb(Math.Min(255,c.R+a), Math.Min(255,c.G+a), Math.Min(255,c.B+a));

    private static Label Lbl(string text, string font, float size, FontStyle style, Color fore)
    {
        var l=new Label
        {
            Text=text, AutoSize=true, BackColor=Color.Transparent, ForeColor=fore,
            Font=new Font(font,size,style)
        };
        return l;
    }

    protected override void Dispose(bool disposing)
    {
        if(disposing){ salesChart?.Dispose(); revenueChart?.Dispose(); }
        base.Dispose(disposing);
    }
}

// Extension to avoid crash on FlatAppearance.CornerRadius which doesn't exist natively
internal static class ButtonExt
{
    internal static void CornerRadius(this System.Windows.Forms.FlatButtonAppearance fa, int r) { }
}
