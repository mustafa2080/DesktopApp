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

    // Quick Reports Labels
    // Sales Report
    private Label? lblQrTodaySales;
    private Label? lblQrWeekSales;
    private Label? lblQrMonthSales;
    private Label? lblQrReservationsCount;
    // Trips Report
    private Label? lblQrActiveTrips;
    private Label? lblQrCompletedTrips;
    private Label? lblQrTotalBookings;
    private Label? lblQrOccupancyRate;
    // Umrah Report
    private Label? lblQrActivePackages;
    private Label? lblQrPilgrimsCount;
    private Label? lblQrUmrahRevenue;
    private Label? lblQrAvgProfit;
    // CashBox Report
    private Label? lblQrCurrentBalance;
    private Label? lblQrTodayIncome;
    private Label? lblQrTodayExpense;
    private Label? lblQrTodayNet;
    // Customers & Suppliers Report
    private Label? lblQrTotalCustomers;
    private Label? lblQrCustomersWithBalance;
    private Label? lblQrTotalSuppliers;
    private Label? lblQrSuppliersWithBalance;

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
            RowCount = 5,
            BackColor = Color.Transparent,
            Padding = new Padding(0)
        };

        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100)); // Header
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 180)); // Main Stats
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 350)); // Charts
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 220)); // Other Stats
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 400)); // Quick Reports - زيادة الارتفاع

        // Row 0: Header
        mainLayout.Controls.Add(CreateHeader(), 0, 0);

        // Row 1: Main Stats Cards
        mainLayout.Controls.Add(CreateMainStatsSection(), 0, 1);

        // Row 2: Charts Section
        mainLayout.Controls.Add(CreateChartsSection(), 0, 2);

        // Row 3: Other Stats
        mainLayout.Controls.Add(CreateOtherStatsSection(), 0, 3);

        // Row 4: Quick Reports
        mainLayout.Controls.Add(CreateQuickReportsSection(), 0, 4);

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
            // فحص الأبعاد قبل إنشاء الـ gradient
            if (panel.Width > 0 && panel.Height > 0)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), 12);
                using var brush = new LinearGradientBrush(
                    new Point(0, 0), new Point(panel.Width, 0),
                    Color.FromArgb(37, 99, 235), Color.FromArgb(59, 130, 246));
                g.FillPath(brush, path);
            }
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
            // فحص الأبعاد قبل إنشاء الـ gradient
            if (iconCircle.Width > 0 && iconCircle.Height > 0)
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
            }
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
            await LoadQuickReportsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadMainStatsAsync()
    {
        // ✅ تحميل مبيعات اليوم وإيرادات الشهر من حركات الخزينة (المصدر الوحيد الموثوق)
        decimal todaySales = 0;
        decimal monthlyRevenue = 0;
        decimal cashBalance = 0;
        int activeReservations = 0;

        // 1. تحميل رصيد الخزينة + مبيعات اليوم + إيرادات الشهر من الحركات المالية
        try
        {
            var cashBoxes = await _cashBoxService.GetAllCashBoxesAsync();
            var activeCashBoxes = cashBoxes?.Where(c => c.IsActive).ToList();
            
            if (activeCashBoxes != null && activeCashBoxes.Any())
            {
                // رصيد الخزينة
                cashBalance = activeCashBoxes.Sum(c => c.CurrentBalance);
                
                var today = DateTime.Today;
                var todayUtc = DateTime.SpecifyKind(today, DateTimeKind.Utc);
                var tomorrowUtc = DateTime.SpecifyKind(today.AddDays(1), DateTimeKind.Utc);
                var monthStart = DateTime.SpecifyKind(new DateTime(today.Year, today.Month, 1), DateTimeKind.Utc);
                
                // جلب حركات كل خزينة لهذا الشهر
                foreach (var box in activeCashBoxes)
                {
                    var monthTransactions = await _cashBoxService.GetTransactionsByCashBoxAsync(
                        box.Id, monthStart, tomorrowUtc);
                    
                    if (monthTransactions != null)
                    {
                        // إيرادات الشهر = مجموع الإيرادات (Income) هذا الشهر
                        monthlyRevenue += monthTransactions
                            .Where(t => t.Type == TransactionType.Income && !t.IsDeleted)
                            .Sum(t => t.Amount);
                        
                        // مبيعات اليوم = مجموع الإيرادات (Income) اليوم فقط
                        todaySales += monthTransactions
                            .Where(t => t.Type == TransactionType.Income && !t.IsDeleted &&
                                       t.TransactionDate.Date == today)
                            .Sum(t => t.Amount);
                    }
                }
            }
            
            SafeUpdateUI(() =>
            {
                if (lblCashBalance != null)
                    lblCashBalance.Text = FormatCurrency(cashBalance);
                if (lblTodaySales != null)
                    lblTodaySales.Text = FormatCurrency(todaySales);
                if (lblMonthlyRevenue != null)
                    lblMonthlyRevenue.Text = FormatCurrency(monthlyRevenue);
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading cash data: {ex.Message}");
            SafeUpdateUI(() =>
            {
                if (lblCashBalance != null) lblCashBalance.Text = "0 ج.م";
                if (lblTodaySales != null) lblTodaySales.Text = "0 ج.م";
                if (lblMonthlyRevenue != null) lblMonthlyRevenue.Text = "0 ج.م";
            });
        }

        // 2. تحميل عدد الحجوزات النشطة
        try
        {
            var allReservations = await _reservationService.GetAllReservationsAsync();
            if (allReservations != null && allReservations.Any())
            {
                activeReservations = allReservations.Count(r => r.Status == "Confirmed");
            }
            
            SafeUpdateUI(() =>
            {
                if (lblReservations != null)
                    lblReservations.Text = $"{activeReservations} حجز";
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading reservations: {ex.Message}");
            SafeUpdateUI(() =>
            {
                if (lblReservations != null) lblReservations.Text = "0 حجز";
            });
        }
    }

    private async Task LoadOtherStatsAsync()
    {
        // تحميل العملاء
        try
        {
            var customers = await _customerService.GetAllCustomersAsync();
            var customersCount = customers?.Count ?? 0;
            
            SafeUpdateUI(() =>
            {
                if (lblCustomersCount != null)
                {
                    if (customersCount == 0)
                        lblCustomersCount.Text = "لا يوجد عملاء";
                    else
                        lblCustomersCount.Text = $"{customersCount} عميل";
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading customers: {ex.Message}");
            SafeUpdateUI(() =>
            {
                if (lblCustomersCount != null) lblCustomersCount.Text = "0 عميل";
            });
        }
        
        // تحميل الرحلات
        try
        {
            var trips = await _tripService.GetAllTripsAsync();
            var allTripsCount = trips?.Count ?? 0;
            var activeTrips = trips?.Count(t => t.Status == TripStatus.Confirmed || t.Status == TripStatus.Unconfirmed) ?? 0;
            
            SafeUpdateUI(() =>
            {
                if (lblTripsCount != null)
                {
                    if (allTripsCount == 0)
                        lblTripsCount.Text = "لا توجد رحلات";
                    else if (activeTrips == 0)
                        lblTripsCount.Text = $"{allTripsCount} رحلة";
                    else
                        lblTripsCount.Text = $"{activeTrips} رحلة نشطة";
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading trips: {ex.Message}");
            SafeUpdateUI(() =>
            {
                if (lblTripsCount != null) lblTripsCount.Text = "0 رحلة";
            });
        }
        
        // تحميل باقات العمرة
        try
        {
            var umrahStats = await _umrahService.GetPackageStatisticsAsync();
            var umrahCount = umrahStats.ActivePackages;
            
            SafeUpdateUI(() =>
            {
                if (lblUmrahCount != null)
                {
                    if (umrahCount == 0)
                        lblUmrahCount.Text = "لا توجد باقات";
                    else
                        lblUmrahCount.Text = $"{umrahCount} باقة نشطة";
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading umrah packages: {ex.Message}");
            SafeUpdateUI(() =>
            {
                if (lblUmrahCount != null) lblUmrahCount.Text = "0 باقة";
            });
        }
        
        // تحميل الموردين
        try
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            var suppliersCount = suppliers?.Count ?? 0;
            
            SafeUpdateUI(() =>
            {
                if (lblSuppliersCount != null)
                {
                    if (suppliersCount == 0)
                        lblSuppliersCount.Text = "لا يوجد موردين";
                    else
                        lblSuppliersCount.Text = $"{suppliersCount} مورد";
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading suppliers: {ex.Message}");
            SafeUpdateUI(() =>
            {
                if (lblSuppliersCount != null) lblSuppliersCount.Text = "0 مورد";
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

    private Panel CreateQuickReportsSection()
    {
        var container = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(0)
        };

        // Enhanced section header with gradient
        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 0, 0, 10)
        };

        headerPanel.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Gradient background for header
            using var headerPath = RoundedRect(new Rectangle(0, 0, headerPanel.Width - 1, headerPanel.Height - 1), 12);
            using var headerBrush = new LinearGradientBrush(
                new Rectangle(0, 0, headerPanel.Width, headerPanel.Height),
                Color.FromArgb(249, 250, 251),
                Color.FromArgb(241, 245, 249),
                45F);
            g.FillPath(headerBrush, headerPath);
            
            // Accent left border
            using var accentBrush = new LinearGradientBrush(
                new Rectangle(0, 0, 6, headerPanel.Height),
                Color.FromArgb(59, 130, 246),
                Color.FromArgb(37, 99, 235),
                90F);
            g.FillRectangle(accentBrush, 0, 0, 6, headerPanel.Height);
        };

        var sectionIcon = new Label
        {
            Text = "📊",
            Font = new Font("Segoe UI Emoji", 22F),
            ForeColor = Color.FromArgb(59, 130, 246),
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(20, 15)
        };
        headerPanel.Controls.Add(sectionIcon);

        var sectionLabel = new Label
        {
            Text = "تقارير سريعة ومفصلة",
            Font = new Font("Cairo", 15F, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 41, 59),
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(60, 13)
        };
        headerPanel.Controls.Add(sectionLabel);

        var sectionSubtitle = new Label
        {
            Text = "نظرة شاملة على أداء جميع الأقسام",
            Font = new Font("Cairo", 9.5F, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 116, 139),
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(60, 37)
        };
        headerPanel.Controls.Add(sectionSubtitle);

        container.Controls.Add(headerPanel);

        // Cards layout
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 1,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 70, 0, 0)
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));

        // Card 1: Sales Report
        var salesCard = CreateQuickReportCard(
            "💰", "المبيعات والإيرادات", Color.FromArgb(34, 197, 94),
            new[] { "مبيعات اليوم", "مبيعات الأسبوع", "مبيعات الشهر", "عدد الحجوزات" },
            out lblQrTodaySales, out lblQrWeekSales, out lblQrMonthSales, out lblQrReservationsCount);
        layout.Controls.Add(salesCard, 0, 0);

        // Card 2: Trips Report
        var tripsCard = CreateQuickReportCard(
            "✈️", "تقرير الرحلات", Color.FromArgb(59, 130, 246),
            new[] { "رحلات نشطة", "رحلات مكتملة", "إجمالي الحجوزات", "نسبة الإشغال" },
            out lblQrActiveTrips, out lblQrCompletedTrips, out lblQrTotalBookings, out lblQrOccupancyRate);
        layout.Controls.Add(tripsCard, 1, 0);

        // Card 3: Umrah Report
        var umrahCard = CreateQuickReportCard(
            "🕌", "تقرير العمرة", Color.FromArgb(180, 83, 9),
            new[] { "باقات نشطة", "عدد المعتمرين", "إجمالي الإيرادات", "متوسط الربح" },
            out lblQrActivePackages, out lblQrPilgrimsCount, out lblQrUmrahRevenue, out lblQrAvgProfit);
        layout.Controls.Add(umrahCard, 2, 0);

        // Card 4: CashBox Report
        var cashCard = CreateQuickReportCard(
            "🏦", "تقرير الخزينة", Color.FromArgb(139, 92, 246),
            new[] { "الرصيد الحالي", "إيرادات اليوم", "مصروفات اليوم", "صافي اليوم" },
            out lblQrCurrentBalance, out lblQrTodayIncome, out lblQrTodayExpense, out lblQrTodayNet);
        layout.Controls.Add(cashCard, 3, 0);

        // Card 5: Customers & Suppliers Report
        var custSuppCard = CreateQuickReportCard(
            "👥", "العملاء والموردين", Color.FromArgb(20, 184, 166),
            new[] { "إجمالي العملاء", "عملاء لهم رصيد", "إجمالي الموردين", "موردين لهم رصيد" },
            out lblQrTotalCustomers, out lblQrCustomersWithBalance, out lblQrTotalSuppliers, out lblQrSuppliersWithBalance);
        layout.Controls.Add(custSuppCard, 4, 0);

        container.Controls.Add(layout);

        return container;
    }

    private Panel CreateQuickReportCard(string icon, string title, Color accentColor,
        string[] labels, out Label? val1, out Label? val2, out Label? val3, out Label? val4)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(8)
        };

        card.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var path = RoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 14);
            g.FillPath(Brushes.White, path);

            // Shadow effect
            using var shadowPen = new Pen(Color.FromArgb(20, 0, 0, 0), 1);
            g.DrawPath(shadowPen, path);

            // Gradient top accent bar - أعلى وأبرز
            using var accentBrush = new LinearGradientBrush(
                new Rectangle(0, 0, card.Width, 8),
                accentColor,
                Color.FromArgb(Math.Min(255, accentColor.R + 30),
                               Math.Min(255, accentColor.G + 30),
                               Math.Min(255, accentColor.B + 30)),
                45F);
            using var accentPath = RoundedRect(new Rectangle(0, 0, card.Width, 8), 14);
            g.FillPath(accentBrush, accentPath);

            // Subtle separator after header
            using var sepPen = new Pen(Color.FromArgb(226, 232, 240), 2);
            g.DrawLine(sepPen, 15, 62, card.Width - 15, 62);
        };

        // Header section with gradient background
        var headerPanel = new Panel
        {
            Location = new Point(0, 8),
            Size = new Size(card.Width, 52),
            BackColor = Color.FromArgb(248, 250, 252),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
        };
        card.Controls.Add(headerPanel);

        // Icon with circular background
        var iconCircle = new Panel
        {
            Size = new Size(38, 38),
            Location = new Point(15, 7),
            BackColor = Color.Transparent
        };
        iconCircle.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = new GraphicsPath();
            path.AddEllipse(0, 0, 38, 38);
            using var brush = new LinearGradientBrush(
                new Rectangle(0, 0, 38, 38),
                Color.FromArgb(240, accentColor.R, accentColor.G, accentColor.B),
                Color.FromArgb(210, accentColor.R, accentColor.G, accentColor.B),
                45F);
            e.Graphics.FillPath(brush, path);
        };
        headerPanel.Controls.Add(iconCircle);

        var lblIcon = new Label
        {
            Text = icon,
            Font = new Font("Segoe UI Emoji", 18F),
            ForeColor = accentColor,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(9, 8)
        };
        iconCircle.Controls.Add(lblIcon);

        var lblTitle = new Label
        {
            Text = title,
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = accentColor,
            BackColor = Color.Transparent,
            AutoSize = false,
            Size = new Size(card.Width - 70, 40),
            Location = new Point(60, 9),
            TextAlign = ContentAlignment.MiddleLeft,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
        };
        headerPanel.Controls.Add(lblTitle);

        // Create 4 data rows with enhanced styling
        int startY = 75;
        int rowHeight = 56;
        var valueLabels = new Label?[4];

        for (int i = 0; i < 4; i++)
        {
            int y = startY + (i * rowHeight);

            // Alternating row background with rounded corners
            var rowPanel = new Panel
            {
                Location = new Point(10, y),
                Size = new Size(card.Width - 20, rowHeight - 6),
                BackColor = i % 2 == 0 ? Color.FromArgb(249, 250, 251) : Color.White,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            
            if (i % 2 == 0)
            {
                rowPanel.Paint += (s, e2) =>
                {
                    e2.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using var rPath = RoundedRect(new Rectangle(0, 0, rowPanel.Width - 1, rowPanel.Height - 1), 8);
                    using var rBrush = new SolidBrush(Color.FromArgb(249, 250, 251));
                    e2.Graphics.FillPath(rBrush, rPath);
                };
            }
            card.Controls.Add(rowPanel);

            // Bullet point indicator
            var bullet = new Panel
            {
                Size = new Size(6, 6),
                Location = new Point(18, y + 22),
                BackColor = Color.Transparent
            };
            bullet.Paint += (s, e3) =>
            {
                e3.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var bPath = new GraphicsPath();
                bPath.AddEllipse(0, 0, 6, 6);
                using var bBrush = new SolidBrush(accentColor);
                e3.Graphics.FillPath(bBrush, bPath);
            };
            card.Controls.Add(bullet);

            // Label name - أكبر وأوضح
            var lblName = new Label
            {
                Text = labels[i],
                Font = new Font("Cairo", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(32, y + 14)
            };
            card.Controls.Add(lblName);

            // Value label - أكبر وأبرز مع لون تدرجي
            var lblValue = new Label
            {
                Text = "---",
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                ForeColor = accentColor,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Size = new Size(130, 30),
                Location = new Point(card.Width - 145, y + 10),
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            card.Controls.Add(lblValue);
            valueLabels[i] = lblValue;
        }

        val1 = valueLabels[0];
        val2 = valueLabels[1];
        val3 = valueLabels[2];
        val4 = valueLabels[3];

        return card;
    }

    private async Task LoadQuickReportsAsync()
    {
        // ═══════════ Sales Report ═══════════
        try
        {
            var allReservations = await _reservationService.GetAllReservationsAsync();
            if (allReservations != null && allReservations.Any())
            {
                var today = DateTime.Today;
                var weekStart = today.AddDays(-(int)today.DayOfWeek);
                var monthStart = new DateTime(today.Year, today.Month, 1);

                var confirmed = allReservations
                    .Where(r => r.Status == "Confirmed" || r.Status == "Completed" || r.Status == "Paid");

                var todaySales = confirmed.Where(r => r.ReservationDate.Date == today).Sum(r => r.SellingPrice);
                var weekSales = confirmed.Where(r => r.ReservationDate.Date >= weekStart).Sum(r => r.SellingPrice);
                var monthSales = confirmed.Where(r => r.ReservationDate >= monthStart).Sum(r => r.SellingPrice);
                var totalReservations = allReservations.Count;

                SafeUpdateUI(() =>
                {
                    if (lblQrTodaySales != null) lblQrTodaySales.Text = FormatCurrency(todaySales);
                    if (lblQrWeekSales != null) lblQrWeekSales.Text = FormatCurrency(weekSales);
                    if (lblQrMonthSales != null) lblQrMonthSales.Text = FormatCurrency(monthSales);
                    if (lblQrReservationsCount != null)
                    {
                        lblQrReservationsCount.Text = $"{totalReservations} حجز";
                        lblQrReservationsCount.ForeColor = totalReservations > 0 
                            ? Color.FromArgb(34, 197, 94) 
                            : Color.FromArgb(148, 163, 184);
                    }
                });
            }
            else
            {
                SafeUpdateUI(() =>
                {
                    if (lblQrTodaySales != null) lblQrTodaySales.Text = "0 ج.م";
                    if (lblQrWeekSales != null) lblQrWeekSales.Text = "0 ج.م";
                    if (lblQrMonthSales != null) lblQrMonthSales.Text = "0 ج.م";
                    if (lblQrReservationsCount != null)
                    {
                        lblQrReservationsCount.Text = "0 حجز";
                        lblQrReservationsCount.ForeColor = Color.FromArgb(148, 163, 184);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading sales report: {ex.Message}");
            SafeUpdateUI(() =>
            {
                if (lblQrTodaySales != null) lblQrTodaySales.Text = "0 ج.م";
                if (lblQrWeekSales != null) lblQrWeekSales.Text = "0 ج.م";
                if (lblQrMonthSales != null) lblQrMonthSales.Text = "0 ج.م";
                if (lblQrReservationsCount != null) lblQrReservationsCount.Text = "0 حجز";
            });
        }

        // ═══════════ Trips Report ═══════════
        try
        {
            var trips = await _tripService.GetAllTripsAsync();
            if (trips != null)
            {
                var activeTrips = trips.Count(t => t.Status == TripStatus.Confirmed || t.Status == TripStatus.Unconfirmed);
                var completedTrips = trips.Count(t => t.Status == TripStatus.Completed);
                var totalCapacity = trips.Where(t => t.Status == TripStatus.Confirmed).Sum(t => t.TotalCapacity);
                var totalBooked = trips.Where(t => t.Status == TripStatus.Confirmed).Sum(t => t.BookedSeats);
                var occupancy = totalCapacity > 0 ? (double)totalBooked / totalCapacity * 100 : 0;

                SafeUpdateUI(() =>
                {
                    if (lblQrActiveTrips != null)
                    {
                        lblQrActiveTrips.Text = $"{activeTrips} رحلة";
                        lblQrActiveTrips.ForeColor = activeTrips > 0 
                            ? Color.FromArgb(59, 130, 246) 
                            : Color.FromArgb(148, 163, 184);
                    }
                    if (lblQrCompletedTrips != null) lblQrCompletedTrips.Text = $"{completedTrips} رحلة";
                    if (lblQrTotalBookings != null) lblQrTotalBookings.Text = $"{totalBooked} حجز";
                    if (lblQrOccupancyRate != null)
                    {
                        lblQrOccupancyRate.Text = $"{occupancy:N0}%";
                        lblQrOccupancyRate.ForeColor = occupancy >= 70 
                            ? Color.FromArgb(34, 197, 94) 
                            : occupancy >= 40 
                                ? Color.FromArgb(234, 179, 8) 
                                : Color.FromArgb(239, 68, 68);
                    }
                });
            }
            else
            {
                SafeUpdateUI(() =>
                {
                    if (lblQrActiveTrips != null) lblQrActiveTrips.Text = "0 رحلة";
                    if (lblQrCompletedTrips != null) lblQrCompletedTrips.Text = "0 رحلة";
                    if (lblQrTotalBookings != null) lblQrTotalBookings.Text = "0 حجز";
                    if (lblQrOccupancyRate != null) lblQrOccupancyRate.Text = "0%";
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading trips report: {ex.Message}");
            SafeUpdateUI(() =>
            {
                if (lblQrActiveTrips != null) lblQrActiveTrips.Text = "0 رحلة";
                if (lblQrCompletedTrips != null) lblQrCompletedTrips.Text = "0 رحلة";
                if (lblQrTotalBookings != null) lblQrTotalBookings.Text = "0 حجز";
                if (lblQrOccupancyRate != null) lblQrOccupancyRate.Text = "0%";
            });
        }

        // ═══════════ Umrah Report ═══════════
        try
        {
            var umrahStats = await _umrahService.GetPackageStatisticsAsync();
            SafeUpdateUI(() =>
            {
                if (lblQrActivePackages != null)
                {
                    lblQrActivePackages.Text = $"{umrahStats.ActivePackages} باقة";
                    lblQrActivePackages.ForeColor = umrahStats.ActivePackages > 0 
                        ? Color.FromArgb(180, 83, 9) 
                        : Color.FromArgb(148, 163, 184);
                }
                if (lblQrPilgrimsCount != null)
                {
                    lblQrPilgrimsCount.Text = $"{umrahStats.TotalPilgrims} معتمر";
                    lblQrPilgrimsCount.ForeColor = umrahStats.TotalPilgrims > 0 
                        ? Color.FromArgb(180, 83, 9) 
                        : Color.FromArgb(148, 163, 184);
                }
                if (lblQrUmrahRevenue != null) lblQrUmrahRevenue.Text = FormatCurrency(umrahStats.TotalRevenue);
                if (lblQrAvgProfit != null)
                {
                    var avgProfit = umrahStats.TotalPackages > 0
                        ? umrahStats.TotalProfit / umrahStats.TotalPackages : 0;
                    lblQrAvgProfit.Text = FormatCurrency(avgProfit);
                    lblQrAvgProfit.ForeColor = avgProfit >= 0 
                        ? Color.FromArgb(34, 197, 94) 
                        : Color.FromArgb(239, 68, 68);
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading umrah report: {ex.Message}");
            SafeUpdateUI(() =>
            {
                if (lblQrActivePackages != null) lblQrActivePackages.Text = "0 باقة";
                if (lblQrPilgrimsCount != null) lblQrPilgrimsCount.Text = "0 معتمر";
                if (lblQrUmrahRevenue != null) lblQrUmrahRevenue.Text = "0 ج.م";
                if (lblQrAvgProfit != null) lblQrAvgProfit.Text = "0 ج.م";
            });
        }

        // ═══════════ CashBox Report ═══════════
        try
        {
            var cashBoxes = await _cashBoxService.GetAllCashBoxesAsync();
            if (cashBoxes != null && cashBoxes.Any())
            {
                var totalBalance = cashBoxes.Where(c => c.IsActive).Sum(c => c.CurrentBalance);
                decimal todayIncome = 0, todayExpense = 0;

                foreach (var cb in cashBoxes.Where(c => c.IsActive))
                {
                    try
                    {
                        var transactions = await _cashBoxService.GetTransactionsByCashBoxAsync(
                            cb.Id, DateTime.Today, DateTime.Today.AddDays(1).AddSeconds(-1));
                        if (transactions != null)
                        {
                            todayIncome += transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                            todayExpense += transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                        }
                    }
                    catch
                    {
                        // تجاهل خطأ خزنة واحدة واستكمل الباقي
                        continue;
                    }
                }

                SafeUpdateUI(() =>
                {
                    if (lblQrCurrentBalance != null) lblQrCurrentBalance.Text = FormatCurrency(totalBalance);
                    if (lblQrTodayIncome != null)
                    {
                        lblQrTodayIncome.Text = FormatCurrency(todayIncome);
                        lblQrTodayIncome.ForeColor = todayIncome > 0 
                            ? Color.FromArgb(34, 197, 94) 
                            : Color.FromArgb(148, 163, 184);
                    }
                    if (lblQrTodayExpense != null)
                    {
                        lblQrTodayExpense.Text = FormatCurrency(todayExpense);
                        lblQrTodayExpense.ForeColor = todayExpense > 0 
                            ? Color.FromArgb(239, 68, 68) 
                            : Color.FromArgb(148, 163, 184);
                    }
                    if (lblQrTodayNet != null)
                    {
                        var net = todayIncome - todayExpense;
                        lblQrTodayNet.Text = FormatCurrency(Math.Abs(net));
                        lblQrTodayNet.ForeColor = net >= 0 
                            ? Color.FromArgb(34, 197, 94) 
                            : Color.FromArgb(239, 68, 68);
                        if (net > 0)
                            lblQrTodayNet.Text = "+ " + lblQrTodayNet.Text;
                        else if (net < 0)
                            lblQrTodayNet.Text = "- " + lblQrTodayNet.Text;
                    }
                });
            }
            else
            {
                SafeUpdateUI(() =>
                {
                    if (lblQrCurrentBalance != null) lblQrCurrentBalance.Text = "0 ج.م";
                    if (lblQrTodayIncome != null) lblQrTodayIncome.Text = "0 ج.م";
                    if (lblQrTodayExpense != null) lblQrTodayExpense.Text = "0 ج.م";
                    if (lblQrTodayNet != null) lblQrTodayNet.Text = "0 ج.م";
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading cashbox report: {ex.Message}");
            SafeUpdateUI(() =>
            {
                if (lblQrCurrentBalance != null) lblQrCurrentBalance.Text = "0 ج.م";
                if (lblQrTodayIncome != null) lblQrTodayIncome.Text = "0 ج.م";
                if (lblQrTodayExpense != null) lblQrTodayExpense.Text = "0 ج.م";
                if (lblQrTodayNet != null) lblQrTodayNet.Text = "0 ج.م";
            });
        }

        // ═══════════ Customers & Suppliers Report ═══════════
        try
        {
            var customers = await _customerService.GetAllCustomersAsync();
            var customersWithBalance = await _customerService.GetCustomersWithBalanceAsync();
            
            SafeUpdateUI(() =>
            {
                if (lblQrTotalCustomers != null)
                {
                    lblQrTotalCustomers.Text = $"{customers?.Count ?? 0} عميل";
                    lblQrTotalCustomers.ForeColor = (customers?.Count ?? 0) > 0 
                        ? Color.FromArgb(20, 184, 166) 
                        : Color.FromArgb(148, 163, 184);
                }
                if (lblQrCustomersWithBalance != null)
                {
                    var balanceCount = customersWithBalance?.Count ?? 0;
                    lblQrCustomersWithBalance.Text = $"{balanceCount} عميل";
                    var totalCustomers = customers?.Count ?? 1;
                    var balanceRatio = (double)balanceCount / totalCustomers;
                    lblQrCustomersWithBalance.ForeColor = balanceRatio > 0.3 
                        ? Color.FromArgb(239, 68, 68) 
                        : balanceRatio > 0.1 
                            ? Color.FromArgb(234, 179, 8) 
                            : Color.FromArgb(34, 197, 94);
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading customers report: {ex.Message}");
            SafeUpdateUI(() =>
            {
                if (lblQrTotalCustomers != null) lblQrTotalCustomers.Text = "0 عميل";
                if (lblQrCustomersWithBalance != null) lblQrCustomersWithBalance.Text = "0 عميل";
            });
        }
        
        try
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            var suppliersWithBalance = await _supplierService.GetSuppliersWithBalanceAsync();
            
            SafeUpdateUI(() =>
            {
                if (lblQrTotalSuppliers != null)
                {
                    lblQrTotalSuppliers.Text = $"{suppliers?.Count ?? 0} مورد";
                    lblQrTotalSuppliers.ForeColor = (suppliers?.Count ?? 0) > 0 
                        ? Color.FromArgb(20, 184, 166) 
                        : Color.FromArgb(148, 163, 184);
                }
                if (lblQrSuppliersWithBalance != null)
                {
                    var balanceCount = suppliersWithBalance?.Count ?? 0;
                    lblQrSuppliersWithBalance.Text = $"{balanceCount} مورد";
                    var totalSuppliers = suppliers?.Count ?? 1;
                    var balanceRatio = (double)balanceCount / totalSuppliers;
                    lblQrSuppliersWithBalance.ForeColor = balanceRatio > 0.3 
                        ? Color.FromArgb(239, 68, 68) 
                        : balanceRatio > 0.1 
                            ? Color.FromArgb(234, 179, 8) 
                            : Color.FromArgb(34, 197, 94);
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading suppliers report: {ex.Message}");
            SafeUpdateUI(() =>
            {
                if (lblQrTotalSuppliers != null) lblQrTotalSuppliers.Text = "0 مورد";
                if (lblQrSuppliersWithBalance != null) lblQrSuppliersWithBalance.Text = "0 مورد";
            });
        }
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