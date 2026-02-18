using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Presentation;

namespace GraceWay.AccountingSystem.Presentation.Forms;

/// <summary>
/// عرض تفاصيل حزمة العمرة
/// </summary>
public partial class UmrahPackageDetailsForm : Form
{
    private readonly IUmrahService _umrahService;
    private readonly int _packageId;
    private UmrahPackage? _package;
    
    public UmrahPackageDetailsForm(IUmrahService umrahService, int packageId)
    {
        _umrahService = umrahService;
        _packageId = packageId;
        
        InitializeComponent();
        SetupForm();
        _ = LoadPackageAsync();
    }
    
    private void SetupForm()
    {
        this.Text = "تفاصيل حزمة العمرة";
        this.Size = new Size(1200, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
    }
    
    private async Task LoadPackageAsync()
    {
        try
        {
            _package = await _umrahService.GetPackageByIdAsync(_packageId);
            
            if (_package == null)
            {
                MessageBox.Show("لم يتم العثور على الحزمة!", "خطأ", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }
            
            DisplayPackageDetails();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل البيانات:\n{ex.Message}", "خطأ", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
        }
    }
    
    private void DisplayPackageDetails()
    {
        if (_package == null) return;
        
        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = ColorScheme.Background,
            Padding = new Padding(30)
        };
        
        int y = 20;
        
        // ══════════════════════════════════════
        // HEADER WITH PACKAGE STATUS
        // ══════════════════════════════════════
        Panel headerPanel = CreateHeaderPanel(_package);
        headerPanel.Location = new Point(30, y);
        mainPanel.Controls.Add(headerPanel);
        y += 140;
        
        // ══════════════════════════════════════
        // BASIC INFO SECTION
        // ══════════════════════════════════════
        Panel basicInfoPanel = CreateInfoSection(
            "📋 المعلومات الأساسية",
            new Dictionary<string, string>
            {
                { "رقم الحزمة", _package.PackageNumber },
                { "التاريخ", _package.Date.ToString("dd/MM/yyyy") },
                { "اسم الرحلة", _package.TripName },
                { "عدد الأفراد", _package.NumberOfPersons.ToString() },
                { "نوع الغرفة", _package.GetRoomTypeDisplay() },
                { "إجمالي الليالي", _package.TotalNights.ToString() }
            }
        );
        basicInfoPanel.Location = new Point(30, y);
        mainPanel.Controls.Add(basicInfoPanel);
        y += 220;
        
        // ══════════════════════════════════════
        // ACCOMMODATION SECTION
        // ══════════════════════════════════════
        Panel accommodationPanel = CreateInfoSection(
            "🏨 الإقامة",
            new Dictionary<string, string>
            {
                { "فندق مكة", _package.MakkahHotel },
                { "ليالي مكة", $"{_package.MakkahNights} ليالي" },
                { "فندق المدينة", _package.MadinahHotel },
                { "ليالي المدينة", $"{_package.MadinahNights} ليالي" },
                { "إجمالي تكلفة الإقامة", $"{_package.AccommodationTotal:N2} ج.م" }
            }
        );
        accommodationPanel.Location = new Point(30, y);
        mainPanel.Controls.Add(accommodationPanel);
        y += 200;
        
        // ══════════════════════════════════════
        // TRANSPORT SECTION
        // ══════════════════════════════════════
        Panel transportPanel = CreateInfoSection(
            "✈️ وسائل النقل",
            new Dictionary<string, string>
            {
                { "وسيلة السفر", _package.TransportMethod },
                { "سعر الطيران", $"{_package.FlightPrice:N2} ج.م" },
                { "سعر القطار السريع", $"{_package.FastTrainPriceSAR:N2} ر.س = {_package.FastTrainPriceEGP:N2} ج.م" }
            }
        );
        transportPanel.Location = new Point(30, y);
        mainPanel.Controls.Add(transportPanel);
        y += 160;
        
        // ══════════════════════════════════════
        // PRICING SECTION
        // ══════════════════════════════════════
        Panel pricingPanel = CreateInfoSection(
            "💰 التكاليف والأسعار",
            new Dictionary<string, string>
            {
                { "سعر البيع (للفرد)", $"{_package.SellingPrice:N2} ج.م" },
                { "سعر التأشيرة", $"{_package.VisaPriceSAR:N2} ر.س = {_package.VisaPriceEGP:N2} ج.م" },
                { "سعر الصرف", $"{_package.SARExchangeRate:N4}" },
                { "سعر الباركود", $"{_package.BarcodePrice:N2} ج.م" },
                { "باركود المشرف", _package.SupervisorBarcodePrice > 0 ? $"{_package.SupervisorBarcodePrice:N2} ج.م  ⚠️ خاص بالمشرف" : "0.00 ج.م" },
                { "العمولة", $"{_package.Commission:N2} ج.م" },
                { "مصاريف المشرف", $"{_package.SupervisorExpensesSAR:N2} ر.س = {_package.SupervisorExpensesEGP:N2} ج.م" }
            }
        );
        pricingPanel.Location = new Point(30, y);
        mainPanel.Controls.Add(pricingPanel);
        y += 240;
        
        // ══════════════════════════════════════
        // BROKER & SUPERVISOR
        // ══════════════════════════════════════
        if (!string.IsNullOrEmpty(_package.BrokerName) || !string.IsNullOrEmpty(_package.SupervisorName))
        {
            Panel teamPanel = CreateInfoSection(
                "👥 الوسيط والمشرف",
                new Dictionary<string, string>
                {
                    { "الوسيط", _package.BrokerName ?? "-" },
                    { "المشرف", _package.SupervisorName ?? "-" }
                }
            );
            teamPanel.Location = new Point(30, y);
            mainPanel.Controls.Add(teamPanel);
            y += 140;
        }
        
        // ══════════════════════════════════════
        // FINANCIAL SUMMARY (HIGHLIGHTED)
        // ══════════════════════════════════════
        Panel financialPanel = CreateFinancialSummary(_package);
        financialPanel.Location = new Point(30, y);
        mainPanel.Controls.Add(financialPanel);
        y += 300;
        
        // ══════════════════════════════════════
        // NOTES SECTION
        // ══════════════════════════════════════
        if (!string.IsNullOrEmpty(_package.Notes))
        {
            Panel notesPanel = CreateNotesSection(_package.Notes);
            notesPanel.Location = new Point(30, y);
            mainPanel.Controls.Add(notesPanel);
            y += 120;
        }
        
        // ══════════════════════════════════════
        // CLOSE BUTTON
        // ══════════════════════════════════════
        Button closeBtn = new Button
        {
            Text = "✖ إغلاق",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Size = new Size(150, 45),
            Location = new Point(1020, y),
            BackColor = ColorScheme.Secondary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        closeBtn.FlatAppearance.BorderSize = 0;
        closeBtn.Click += (s, e) => this.Close();
        mainPanel.Controls.Add(closeBtn);
        
        this.Controls.Add(mainPanel);
    }
    
    private Panel CreateHeaderPanel(UmrahPackage package)
    {
        Panel panel = new Panel
        {
            Size = new Size(1110, 120),
            BackColor = ColorScheme.Primary,
            Padding = new Padding(20)
        };
        
        // Icon
        Label icon = new Label
        {
            Text = "🕌",
            Font = new Font("Segoe UI Emoji", 36F),
            ForeColor = Color.White,
            Size = new Size(60, 60),
            Location = new Point(1030, 20),
            TextAlign = ContentAlignment.MiddleCenter
        };
        panel.Controls.Add(icon);
        
        // Title
        Label title = new Label
        {
            Text = $"حزمة عمرة: {package.PackageNumber}",
            Font = new Font("Cairo", 18F, FontStyle.Bold),
            ForeColor = Color.White,
            Size = new Size(900, 40),
            Location = new Point(20, 15),
            TextAlign = ContentAlignment.MiddleRight
        };
        panel.Controls.Add(title);
        
        // Pilgrim Name
        Label pilgrim = new Label
        {
            Text = $"الرحلة: {package.TripName}",
            Font = new Font("Cairo", 12F),
            ForeColor = Color.FromArgb(240, 240, 240),
            Size = new Size(900, 30),
            Location = new Point(20, 55),
            TextAlign = ContentAlignment.MiddleRight
        };
        panel.Controls.Add(pilgrim);
        
        // Status Badge
        Label status = new Label
        {
            Text = package.GetStatusDisplay(),
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Size = new Size(120, 35),
            Location = new Point(50, 20),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = GetStatusColor(package.Status),
            ForeColor = Color.White
        };
        panel.Controls.Add(status);
        
        return panel;
    }
    
    private Panel CreateInfoSection(string title, Dictionary<string, string> data)
    {
        Panel section = new Panel
        {
            Size = new Size(1110, 60 + (data.Count * 30)),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        
        // Section Title
        Label titleLabel = new Label
        {
            Text = title,
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            Size = new Size(1080, 35),
            Location = new Point(10, 10),
            TextAlign = ContentAlignment.MiddleRight,
            BackColor = Color.FromArgb(245, 248, 250),
            Padding = new Padding(0, 5, 10, 0)
        };
        section.Controls.Add(titleLabel);
        
        int y = 55;
        foreach (var item in data)
        {
            // Label
            Label lbl = new Label
            {
                Text = $"{item.Key}:",
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                ForeColor = ColorScheme.TextSecondary,
                Size = new Size(200, 25),
                Location = new Point(890, y),
                TextAlign = ContentAlignment.MiddleRight
            };
            section.Controls.Add(lbl);
            
            // Value
            Label val = new Label
            {
                Text = item.Value,
                Font = new Font("Cairo", 10F),
                ForeColor = ColorScheme.TextPrimary,
                Size = new Size(650, 25),
                Location = new Point(220, y),
                TextAlign = ContentAlignment.MiddleRight
            };
            section.Controls.Add(val);
            
            y += 30;
        }
        
        return section;
    }
    
    private Panel CreateFinancialSummary(UmrahPackage package)
    {
        Panel panel = new Panel
        {
            Size = new Size(1110, 280),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        
        // Title with gradient background
        Panel titlePanel = new Panel
        {
            Size = new Size(1110, 50),
            Location = new Point(0, 0),
            BackColor = ColorScheme.Primary
        };
        
        Label title = new Label
        {
            Text = "📊 الملخص المالي",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = Color.White,
            Size = new Size(1080, 50),
            Location = new Point(10, 0),
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 10, 20, 0)
        };
        titlePanel.Controls.Add(title);
        panel.Controls.Add(titlePanel);
        
        // Content area with grid layout
        int startY = 70;
        
        // Row 1: Total Costs (2 columns)
        CreateFinancialCard(panel, "إجمالي التكاليف", "للفرد الواحد", 
            $"{package.TotalCosts:N2}", "ج.م",
            30, startY, 530, 90, 
            Color.FromArgb(255, 243, 205), Color.FromArgb(255, 152, 0));
        
        CreateFinancialCard(panel, "إجمالي التكاليف", $"{package.NumberOfPersons} أفراد", 
            $"{package.TotalCosts * package.NumberOfPersons:N2}", "ج.م",
            580, startY, 500, 90,
            Color.FromArgb(255, 235, 238), Color.FromArgb(244, 67, 54));
        
        startY += 100;
        
        // Row 2: Total Revenue
        CreateFinancialCard(panel, "إجمالي الإيرادات", $"من {package.NumberOfPersons} أفراد", 
            $"{package.TotalRevenue:N2}", "ج.م",
            30, startY, 530, 90,
            Color.FromArgb(227, 242, 253), Color.FromArgb(33, 150, 243));
        
        // Net Profit with dynamic color
        Color profitBg = package.NetProfit >= 0 
            ? Color.FromArgb(232, 245, 233) 
            : Color.FromArgb(255, 235, 238);
        Color profitColor = package.NetProfit >= 0 
            ? Color.FromArgb(56, 142, 60) 
            : Color.FromArgb(211, 47, 47);
        
        CreateFinancialCard(panel, "صافي الربح", 
            package.NetProfit >= 0 ? "✓ ربح" : "✗ خسارة", 
            $"{package.NetProfit:N2}", "ج.م",
            580, startY, 350, 90,
            profitBg, profitColor);
        
        // Profit Margin badge
        Panel marginBadge = new Panel
        {
            Size = new Size(120, 90),
            Location = new Point(950, startY),
            BackColor = profitBg,
            BorderStyle = BorderStyle.FixedSingle
        };
        
        Label marginLabel = new Label
        {
            Text = "هامش الربح",
            Font = new Font("Cairo", 8F),
            ForeColor = Color.FromArgb(100, 100, 100),
            Size = new Size(110, 25),
            Location = new Point(5, 10),
            TextAlign = ContentAlignment.MiddleCenter
        };
        marginBadge.Controls.Add(marginLabel);
        
        Label marginValue = new Label
        {
            Text = $"{package.ProfitMarginPercent:N1}%",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = profitColor,
            Size = new Size(110, 45),
            Location = new Point(5, 35),
            TextAlign = ContentAlignment.MiddleCenter
        };
        marginBadge.Controls.Add(marginValue);
        
        panel.Controls.Add(marginBadge);
        
        return panel;
    }
    
    private void CreateFinancialCard(Panel parent, string title, string subtitle, 
        string amount, string currency, int x, int y, int width, int height,
        Color bgColor, Color accentColor)
    {
        Panel card = new Panel
        {
            Size = new Size(width, height),
            Location = new Point(x, y),
            BackColor = bgColor,
            BorderStyle = BorderStyle.FixedSingle
        };
        
        // Accent bar on the right
        Panel accentBar = new Panel
        {
            Size = new Size(5, height),
            Location = new Point(width - 5, 0),
            BackColor = accentColor
        };
        card.Controls.Add(accentBar);
        
        // Title
        Label lblTitle = new Label
        {
            Text = title,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(80, 80, 80),
            Size = new Size(width - 20, 20),
            Location = new Point(10, 8),
            TextAlign = ContentAlignment.MiddleRight
        };
        card.Controls.Add(lblTitle);
        
        // Subtitle
        Label lblSubtitle = new Label
        {
            Text = subtitle,
            Font = new Font("Cairo", 7.5F),
            ForeColor = Color.FromArgb(120, 120, 120),
            Size = new Size(width - 20, 18),
            Location = new Point(10, 26),
            TextAlign = ContentAlignment.MiddleRight
        };
        card.Controls.Add(lblSubtitle);
        
        // Amount
        Label lblAmount = new Label
        {
            Text = amount,
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = accentColor,
            Size = new Size(width - 90, 35),
            Location = new Point(10, 48),
            TextAlign = ContentAlignment.MiddleRight
        };
        card.Controls.Add(lblAmount);
        
        // Currency
        Label lblCurrency = new Label
        {
            Text = currency,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(120, 120, 120),
            Size = new Size(70, 35),
            Location = new Point(width - 85, 48),
            TextAlign = ContentAlignment.MiddleRight
        };
        card.Controls.Add(lblCurrency);
        
        parent.Controls.Add(card);
    }
    
    private Panel CreateNotesSection(string notes)
    {
        Panel panel = new Panel
        {
            Size = new Size(1110, 100),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        
        // Title
        Label title = new Label
        {
            Text = "📝 ملاحظات",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            Size = new Size(1080, 30),
            Location = new Point(10, 10),
            TextAlign = ContentAlignment.MiddleRight
        };
        panel.Controls.Add(title);
        
        // Notes Text
        TextBox notesBox = new TextBox
        {
            Text = notes,
            Font = new Font("Cairo", 10F),
            ForeColor = ColorScheme.TextPrimary,
            Size = new Size(1070, 50),
            Location = new Point(20, 45),
            Multiline = true,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = Color.FromArgb(250, 250, 250)
        };
        panel.Controls.Add(notesBox);
        
        return panel;
    }
    
    private Color GetStatusColor(PackageStatus status)
    {
        return status switch
        {
            PackageStatus.Draft => Color.FromArgb(117, 117, 117),
            PackageStatus.Confirmed => Color.FromArgb(13, 71, 161),
            PackageStatus.InProgress => Color.FromArgb(230, 81, 0),
            PackageStatus.Completed => Color.FromArgb(27, 94, 32),
            PackageStatus.Cancelled => Color.FromArgb(183, 28, 28),
            _ => ColorScheme.Secondary
        };
    }
}
