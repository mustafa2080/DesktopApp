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

    private Label? lblTodaySales;
    private Label? lblCashBalance;
    private Label? lblReservations;
    private Label? lblMonthlyRevenue;
    
    private Label? lblCustomersCount;
    private Label? lblTripsCount;
    private Label? lblUmrahCount;
    private Label? lblSuppliersCount;
    
    private CartesianChart? salesChart;
    private PieChart? revenueChart;

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
        this.BackColor = Color.FromArgb(248, 250, 252);
        this.RightToLeft = RightToLeft.Yes;
        this.AutoScroll = true;
        this.DoubleBuffered = true;
        this.Font = new Font("Cairo", 10F, FontStyle.Regular);
        this.Padding = new Padding(20);
        
        BuildUI();
        
        this.ResumeLayout(false);
        
        // Load data when control is loaded
        this.Load += OnControlLoad;
    }

    private async void OnControlLoad(object? sender, EventArgs e)
    {
        try
        {
            await Task.Delay(50); // Small delay to ensure UI is ready
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnControlLoad: {ex.Message}");
        }
    }


    private void BuildUI()
    {
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Color.Transparent,
            Padding = new Padding(0)
        };

        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100)); // Header
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 180)); // Main Stats
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 350)); // Charts
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 220)); // Other Stats

        // Row 0: Header
        mainLayout.Controls.Add(CreateHeader(), 0, 0);

        // Row 1: Main Stats Cards
        mainLayout.Controls.Add(CreateMainStatsSection(), 0, 1);

        // Row 2: Charts Section
        mainLayout.Controls.Add(CreateChartsSection(), 0, 2);

        // Row 3: Other Stats
        mainLayout.Controls.Add(CreateOtherStatsSection(), 0, 3);

        this.Controls.Add(mainLayout);
    }

    private Panel CreateHeader()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 0, 20)
        };

        panel.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = RoundedRect(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), 12);
            using var brush = new LinearGradientBrush(
                new Point(0, 0), new Point(panel.Width, 0),
                Color.FromArgb(37, 99, 235), Color.FromArgb(59, 130, 246));
            g.FillPath(brush, path);
        };

        var lblTitle = new Label
        {
            Text = "🎯 لوحة التحكم",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(20, 15)
        };

        var lblDate = new Label
        {
            Text = $"📅 {DateTime.Now:dddd، dd MMMM yyyy}",
            Font = new Font("Cairo", 10F, FontStyle.Regular),
            ForeColor = Color.FromArgb(220, 235, 255),
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(20, 45)
        };

        // زر التحديث
        var btnRefresh = new Button
        {
            Text = "🔄 تحديث",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(16, 185, 129),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(120, 40),
            Cursor = Cursors.Hand
        };
        btnRefresh.FlatAppearance.BorderSize = 0;
        btnRefresh.Click += async (s, e) => 
        {
            btnRefresh.Enabled = false;
            btnRefresh.Text = "⏳ جاري التحديث...";
            try
            {
                await LoadDataAsync();
                btnRefresh.Text = "✅ تم التحديث";
                await Task.Delay(1000);
            }
            catch
            {
                btnRefresh.Text = "❌ خطأ";
                await Task.Delay(1000);
            }
            finally
            {
                btnRefresh.Text = "🔄 تحديث";
                btnRefresh.Enabled = true;
            }
        };
        
        // حساب موضع الزر (على اليسار)
        panel.Resize += (s, e) => 
        {
            btnRefresh.Location = new Point(panel.Width - btnRefresh.Width - 20, 30);
        };
        btnRefresh.Location = new Point(panel.Width - btnRefresh.Width - 20, 30);

        panel.Controls.Add(lblTitle);
        panel.Controls.Add(lblDate);
        panel.Controls.Add(btnRefresh);

        return panel;
    }

    private TableLayoutPanel CreateMainStatsSection()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = Color.Transparent,
            Padding = new Padding(0),
            Margin = new Padding(0, 0, 0, 20)
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        // Card 1: Today Sales
        var card1 = CreateStatCard("💰", "مبيعات اليوم", "---", Color.FromArgb(34, 197, 94), out lblTodaySales);
        layout.Controls.Add(card1, 0, 0);

        // Card 2: Cash Balance
        var card2 = CreateStatCard("💵", "رصيد الخزينة", "---", Color.FromArgb(59, 130, 246), out lblCashBalance);
        layout.Controls.Add(card2, 1, 0);

        // Card 3: Reservations
        var card3 = CreateStatCard("✈️", "الحجوزات النشطة", "---", Color.FromArgb(249, 115, 22), out lblReservations);
        layout.Controls.Add(card3, 2, 0);

        // Card 4: Monthly Revenue
        var card4 = CreateStatCard("📈", "إيرادات الشهر", "---", Color.FromArgb(168, 85, 247), out lblMonthlyRevenue);
        layout.Controls.Add(card4, 3, 0);

        return layout;
    }

    private TableLayoutPanel CreateChartsSection()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Padding = new Padding(0),
            Margin = new Padding(0, 0, 0, 20)
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        // Sales Chart
        var salesPanel = CreateChartPanel("📊 اتجاه المبيعات (آخر 6 أشهر)", Color.FromArgb(59, 130, 246));
        salesChart = new CartesianChart
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(15, 60, 15, 15),
            BackColor = Color.Transparent
        };
        salesPanel.Controls.Add(salesChart);
        layout.Controls.Add(salesPanel, 0, 0);

        // Revenue Distribution Chart
        var revenuePanel = CreateChartPanel("🎯 توزيع الإيرادات", Color.FromArgb(168, 85, 247));
        revenueChart = new PieChart
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(15, 60, 15, 15),
            BackColor = Color.Transparent,
            LegendPosition = LiveChartsCore.Measure.LegendPosition.Bottom
        };
        revenuePanel.Controls.Add(revenueChart);
        layout.Controls.Add(revenuePanel, 1, 0);

        return layout;
    }

    private TableLayoutPanel CreateOtherStatsSection()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = Color.Transparent,
            Padding = new Padding(0)
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        // Card 1: Customers
        var card1 = CreateInfoCard("👥", "العملاء", "---", Color.FromArgb(20, 184, 166), out lblCustomersCount);
        layout.Controls.Add(card1, 0, 0);

        // Card 2: Trips
        var card2 = CreateInfoCard("🌍", "الرحلات", "---", Color.FromArgb(59, 130, 246), out lblTripsCount);
        layout.Controls.Add(card2, 1, 0);

        // Card 3: Umrah
        var card3 = CreateInfoCard("🕌", "باقات العمرة", "---", Color.FromArgb(180, 83, 9), out lblUmrahCount);
        layout.Controls.Add(card3, 2, 0);

        // Card 4: Suppliers
        var card4 = CreateInfoCard("📦", "الموردين", "---", Color.FromArgb(99, 102, 241), out lblSuppliersCount);
        layout.Controls.Add(card4, 3, 0);

        return layout;
    }

    private Panel CreateChartPanel(string title, Color accentColor)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(10)
        };

        panel.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            using var path = RoundedRect(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), 12);
            g.FillPath(Brushes.White, path);
            
            using var pen = new Pen(Color.FromArgb(226, 232, 240), 1);
            g.DrawPath(pen, path);
            
            using var accentBrush = new SolidBrush(accentColor);
            g.FillRectangle(accentBrush, 0, 0, panel.Width, 5);
        };

        var lblTitle = new Label
        {
            Text = title,
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = accentColor,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(20, 15)
        };
        panel.Controls.Add(lblTitle);

        return panel;
    }

    private Panel CreateInfoCard(string icon, string title, string value, Color color, out Label valueLabel)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(10)
        };

        card.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            using var path = RoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 12);
            g.FillPath(Brushes.White, path);
            
            using var pen = new Pen(Color.FromArgb(226, 232, 240), 1);
            g.DrawPath(pen, path);
            
            // Top accent bar (أعرض وأوضح)
            using var accentBrush = new SolidBrush(color);
            using var accentPath = RoundedRect(new Rectangle(0, 0, card.Width, 6), 12);
            g.FillPath(accentBrush, accentPath);
        };

        // Icon with background circle
        Panel iconCircle = new Panel
        {
            Size = new Size(70, 70),
            Location = new Point((card.Width - 70) / 2, 25),
            BackColor = Color.Transparent
        };
        
        iconCircle.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = new GraphicsPath();
            path.AddEllipse(0, 0, 70, 70);
            
            // Gradient background
            using var brush = new LinearGradientBrush(
                new Rectangle(0, 0, 70, 70),
                Color.FromArgb(240, color.R, color.G, color.B),
                Color.FromArgb(220, color.R, color.G, color.B),
                45F);
            e.Graphics.FillPath(brush, path);
        };
        
        var lblIcon = new Label
        {
            Text = icon,
            Font = new Font("Segoe UI Emoji", 32F),
            ForeColor = color,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(19, 19)
        };
        iconCircle.Controls.Add(lblIcon);
        card.Controls.Add(iconCircle);

        // Title
        var lblTitle = new Label
        {
            Text = title,
            Font = new Font("Cairo", 13F, FontStyle.Bold),
            ForeColor = Color.FromArgb(71, 85, 105),
            BackColor = Color.Transparent,
            AutoSize = false,
            Size = new Size(card.Width - 40, 30),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(20, 105)
        };
        card.Controls.Add(lblTitle);

        // Value (أكبر وأوضح)
        valueLabel = new Label
        {
            Text = value,
            Font = new Font("Cairo", 18F, FontStyle.Bold),
            ForeColor = color,
            BackColor = Color.Transparent,
            AutoSize = false,
            Size = new Size(card.Width - 40, 40),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(20, 140)
        };
        card.Controls.Add(valueLabel);

        // Center controls on resize
        var valLbl = valueLabel; // نسخة محلية عشان تستخدمها جوا اللامبداء
        card.Resize += (s, e) =>
        {
            iconCircle.Location = new Point((card.Width - 70) / 2, 25);
            lblTitle.Location = new Point(20, 105);
            lblTitle.Size = new Size(card.Width - 40, 30);
            valLbl.Location = new Point(20, 140);
            valLbl.Size = new Size(card.Width - 40, 40);
        };

        return card;
    }

    private Panel CreateStatCard(string icon, string title, string value, Color color, out Label valueLabel)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(10)
        };

        card.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            using var path = RoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 12);
            g.FillPath(Brushes.White, path);
            
            using var pen = new Pen(Color.FromArgb(226, 232, 240), 1);
            g.DrawPath(pen, path);
            
            using var accentBrush = new SolidBrush(color);
            using var accentPath = RoundedRect(new Rectangle(card.Width - 8, 20, 6, card.Height - 40), 3);
            g.FillPath(accentBrush, accentPath);
        };

        var lblIcon = new Label
        {
            Text = icon,
            Font = new Font("Segoe UI Emoji", 32F),
            ForeColor = color,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(20, 25)
        };

        var lblTitle = new Label
        {
            Text = title,
            Font = new Font("Cairo", 10F, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 116, 139),
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(20, 75)
        };

        valueLabel = new Label
        {
            Text = value,
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = color,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(20, 95)
        };

        card.Controls.Add(lblIcon);
        card.Controls.Add(lblTitle);
        card.Controls.Add(valueLabel);

        return card;
    }

    private GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        
        return path;
    }

    private async Task LoadDataAsync()
    {
        try
        {
            // تنفيذ متسلسل لتجنب خطأ "A command is already in progress" في PostgreSQL
            // لأن DbContext لا يدعم الاستعلامات المتوازية على نفس الاتصال
            await LoadMainStatsAsync();
            await LoadOtherStatsAsync();
            await LoadChartsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadMainStatsAsync()
    {
        // Load Reservations Stats
        try
        {
            // Get all reservations first
            var allReservations = await _reservationService.GetAllReservationsAsync();
            
            if (allReservations != null && allReservations.Any())
            {
                var today = DateTime.Today;
                var monthStart = new DateTime(today.Year, today.Month, 1);
                
                // Today's sales - reservations created today
                var todaySales = allReservations
                    .Where(r => r.ReservationDate.Date == today && 
                               (r.Status == "Confirmed" || r.Status == "Completed"))
                    .Sum(r => r.SellingPrice);
                
                // Active reservations (Confirmed status - not yet completed)
                var activeCount = allReservations
                    .Count(r => r.Status == "Confirmed");
                
                // Monthly revenue - this month's confirmed/completed reservations
                var monthlyRevenue = allReservations
                    .Where(r => r.ReservationDate >= monthStart && 
                               (r.Status == "Confirmed" || r.Status == "Completed"))
                    .Sum(r => r.SellingPrice);

                SafeUpdateUI(() =>
                {
                    if (lblTodaySales != null)
                        lblTodaySales.Text = FormatCurrency(todaySales);
                    if (lblReservations != null)
                        lblReservations.Text = $"{activeCount} حجز";
                    if (lblMonthlyRevenue != null)
                        lblMonthlyRevenue.Text = FormatCurrency(monthlyRevenue);
                });
            }
            else
            {
                SafeUpdateUI(() =>
                {
                    if (lblTodaySales != null) lblTodaySales.Text = "0 ج.م";
                    if (lblReservations != null) lblReservations.Text = "0 حجز";
                    if (lblMonthlyRevenue != null) lblMonthlyRevenue.Text = "0 ج.م";
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading reservations: {ex.Message}");
            SafeUpdateUI(() =>
            {
                if (lblTodaySales != null) lblTodaySales.Text = "0 ج.م";
                if (lblReservations != null) lblReservations.Text = "0 حجز";
                if (lblMonthlyRevenue != null) lblMonthlyRevenue.Text = "0 ج.م";
            });
        }

        // Load Cash Balance
        try
        {
            var cashBoxes = await _cashBoxService.GetAllCashBoxesAsync();
            var balance = cashBoxes?.Where(c => c.IsActive).Sum(c => c.CurrentBalance) ?? 0;
            
            SafeUpdateUI(() =>
            {
                if (lblCashBalance != null)
                    lblCashBalance.Text = FormatCurrency(balance);
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading cash balance: {ex.Message}");
            SafeUpdateUI(() =>
            {
                if (lblCashBalance != null) lblCashBalance.Text = "0 ج.م";
            });
        }
    }

    private async Task LoadOtherStatsAsync()
    {
        try
        {
            Console.WriteLine("Starting LoadOtherStatsAsync...");
            
            // تحميل العملاء
            var customers = await _customerService.GetAllCustomersAsync();
            var customersCount = customers?.Count ?? 0;
            Console.WriteLine($"Customers count: {customersCount}");
            
            // تحميل الرحلات
            var trips = await _tripService.GetAllTripsAsync();
            var allTripsCount = trips?.Count ?? 0;
            var activeTrips = trips?.Count(t => t.Status == TripStatus.Confirmed || t.Status == TripStatus.Unconfirmed) ?? 0;
            Console.WriteLine($"Total trips: {allTripsCount}, Active trips: {activeTrips}");
            
            // تحميل باقات العمرة
            var umrahStats = await _umrahService.GetPackageStatisticsAsync();
            var umrahCount = umrahStats.ActivePackages;
            Console.WriteLine($"Umrah packages: {umrahCount}");
            
            // تحميل الموردين
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            var suppliersCount = suppliers?.Count ?? 0;
            Console.WriteLine($"Suppliers count: {suppliersCount}");

            // تحديث الواجهة
            SafeUpdateUI(() =>
            {
                if (lblCustomersCount != null)
                {
                    if (customersCount == 0)
                        lblCustomersCount.Text = "لا يوجد عملاء";
                    else
                        lblCustomersCount.Text = $"{customersCount} عميل";
                    Console.WriteLine($"Updated lblCustomersCount: {lblCustomersCount.Text}");
                }
                
                if (lblTripsCount != null)
                {
                    if (allTripsCount == 0)
                        lblTripsCount.Text = "لا توجد رحلات";
                    else if (activeTrips == 0)
                        lblTripsCount.Text = $"{allTripsCount} رحلة (لا نشطة)";
                    else
                        lblTripsCount.Text = $"{activeTrips} رحلة نشطة";
                    Console.WriteLine($"Updated lblTripsCount: {lblTripsCount.Text}");
                }
                
                if (lblUmrahCount != null)
                {
                    if (umrahCount == 0)
                        lblUmrahCount.Text = "لا توجد باقات";
                    else
                        lblUmrahCount.Text = $"{umrahCount} باقة نشطة";
                    Console.WriteLine($"Updated lblUmrahCount: {lblUmrahCount.Text}");
                }
                
                if (lblSuppliersCount != null)
                {
                    if (suppliersCount == 0)
                        lblSuppliersCount.Text = "لا يوجد موردين";
                    else
                        lblSuppliersCount.Text = $"{suppliersCount} مورد";
                    Console.WriteLine($"Updated lblSuppliersCount: {lblSuppliersCount.Text}");
                }
            });
            
            Console.WriteLine("LoadOtherStatsAsync completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading other stats: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // في حالة حدوث خطأ، عرض رسالة مناسبة
            SafeUpdateUI(() =>
            {
                if (lblCustomersCount != null) lblCustomersCount.Text = "خطأ في التحميل";
                if (lblTripsCount != null) lblTripsCount.Text = "خطأ في التحميل";
                if (lblUmrahCount != null) lblUmrahCount.Text = "خطأ في التحميل";
                if (lblSuppliersCount != null) lblSuppliersCount.Text = "خطأ في التحميل";
            });
        }
    }

    private async Task LoadChartsAsync()
    {
        try
        {
            var reservations = await _reservationService.GetAllReservationsAsync();
            if (reservations != null)
            {
                LoadSalesChart(reservations);
                LoadRevenueChart(reservations);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading charts: {ex.Message}");
        }
    }

    private void LoadSalesChart(IEnumerable<Reservation> reservations)
    {
        try
        {
            var sixMonthsAgo = DateTime.Today.AddMonths(-6);
            var data = reservations
                .Where(r => r.CreatedAt >= sixMonthsAgo && 
                          (r.Status == "Confirmed" || r.Status == "Completed" || r.Status == "Paid"))
                .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
                .Select(g => new 
                { 
                    Year = g.Key.Year,
                    Month = g.Key.Month, 
                    Total = (double)g.Sum(r => r.SellingPrice) 
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            var months = new[] 
            { 
                "يناير", "فبراير", "مارس", "إبريل", "مايو", "يونيو", 
                "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" 
            };

            SafeUpdateUI(() =>
            {
                if (salesChart == null) return;

                salesChart.Series = new ISeries[]
                {
                    new ColumnSeries<double>
                    {
                        Values = data.Any() ? data.Select(d => d.Total).ToArray() : new[] { 0.0 },
                        Fill = new SolidColorPaint(new SKColor(59, 130, 246)),
                        MaxBarWidth = 50,
                        Name = "المبيعات"
                    }
                };
                
                salesChart.XAxes = new[] 
                { 
                    new Axis 
                    { 
                        Labels = data.Any() ? data.Select(d => months[d.Month - 1]).ToArray() : new[] { "لا توجد بيانات" }, 
                        TextSize = 11
                    } 
                };
                
                salesChart.YAxes = new[] 
                { 
                    new Axis 
                    { 
                        Labeler = v => $"{v:N0}", 
                        TextSize = 11,
                        MinLimit = 0
                    } 
                };
                
                salesChart.LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden;
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in LoadSalesChart: {ex.Message}");
        }
    }

    private void LoadRevenueChart(IEnumerable<Reservation> reservations)
    {
        try
        {
            var data = reservations
                .Where(r => r.CreatedAt >= DateTime.Today.AddMonths(-1) && 
                          (r.Status == "Confirmed" || r.Status == "Completed" || r.Status == "Paid"))
                .GroupBy(r => r.ServiceType?.ServiceTypeName ?? "غير محدد")
                .Select(g => new { Name = g.Key, Total = (double)g.Sum(r => r.SellingPrice) })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToList();

            var colors = new SKColor[] 
            { 
                new(59, 130, 246),   // Blue
                new(34, 197, 94),    // Green
                new(249, 115, 22),   // Orange
                new(239, 68, 68),    // Red
                new(168, 85, 247)    // Purple
            };

            SafeUpdateUI(() =>
            {
                if (revenueChart == null) return;

                if (data.Count == 0)
                {
                    revenueChart.Series = new ISeries[]
                    { 
                        new PieSeries<double>
                        {
                            Values = new[] { 1.0 },
                            Name = "لا توجد بيانات",
                            Fill = new SolidColorPaint(new SKColor(200, 200, 200))
                        }
                    };
                }
                else
                {
                    revenueChart.Series = data.Select((d, i) => new PieSeries<double>
                    {
                        Values = new[] { d.Total },
                        Name = d.Name,
                        Fill = new SolidColorPaint(colors[i % colors.Length])
                    }).ToArray();
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in LoadRevenueChart: {ex.Message}");
        }
    }

    private void SafeUpdateUI(Action action)
    {
        if (IsDisposed || this.Disposing) return;
        
        try
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => 
                {
                    if (!IsDisposed && !Disposing)
                        action();
                }));
            }
            else
            {
                if (!IsDisposed && !Disposing)
                    action();
            }
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SafeUpdateUI: {ex.Message}");
        }
    }

    private string FormatCurrency(decimal value)
    {
        if (value >= 1000000)
            return $"{(value / 1000000):N1} م ج.م";
        if (value >= 1000)
            return $"{(value / 1000):N0} ألف ج.م";
        
        return $"{value:N0} ج.م";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            salesChart?.Dispose();
            revenueChart?.Dispose();
        }
        
        base.Dispose(disposing);
    }
}