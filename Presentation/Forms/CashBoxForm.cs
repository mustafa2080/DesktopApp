using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing.Drawing2D;
using System.IO;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class CashBoxForm : Form
{
    private readonly ICashBoxService _cashBoxService;
    private readonly IAuthService _authService;
    private readonly int _currentUserId;
    private readonly bool _isAdmin;
    private readonly IServiceProvider? _serviceProvider; // ✅ إضافة service provider
    
    private Panel _headerPanel = null!;
    private Panel _contentPanel = null!;
    private Panel _summaryPanel = null!;
    
    // Controls للـ Summary
    private Label _balanceLabel = null!;
    private Label _monthIncomeLabel = null!;
    private Label _monthExpenseLabel = null!;
    private Label _monthProfitLabel = null!;
    
    // ✅ Labels للعملات الأجنبية
    private Label _usdBalanceLabel = null!;
    private Label _eurBalanceLabel = null!;
    private Label _gbpBalanceLabel = null!;
    private Label _sarBalanceLabel = null!;
    
    // Controls للفلاتر
    private ComboBox _cashBoxCombo = null!;
    private ComboBox _monthCombo = null!;
    private ComboBox _yearCombo = null!;
    private DateTimePicker _dayFilter = null!;
    private CheckBox _filterByDayCheckbox = null!;
    private Button _filterButton = null!;
    private Button _screenshotButton = null!;
    private Button _whatsappButton = null!;
    
    // Controls للحركات
    private DataGridView _transactionsGrid = null!;
    private Button _addIncomeButton = null!;
    private Button _addExpenseButton = null!;
    private Button _viewReportButton = null!;
    
    // Controls للإدارة (Admin only)
    private Button _addCashBoxButton = null!;
    private Button _editCashBoxButton = null!;
    private Button _deleteCashBoxButton = null!;
    private Button _editTransactionButton = null!;
    private Button _deleteTransactionButton = null!;
    
    private int _selectedCashBoxId = 0;
    private int _selectedMonth;
    private int _selectedYear;
    private List<CashBox> _cashBoxes = new();
    private bool _isLoading = false;
    private DateTime _lastViewedDate; // ✅ تتبع آخر يوم تم عرضه
    
    public CashBoxForm(ICashBoxService cashBoxService, IAuthService authService, int currentUserId, IServiceProvider? serviceProvider = null)
    {
        _cashBoxService = cashBoxService;
        _authService = authService;
        _currentUserId = currentUserId;
        _serviceProvider = serviceProvider; // ✅ حفظ service provider
        
        var currentUser = _authService.CurrentUser;
        var roleName = currentUser?.Role?.RoleName?.ToLower();
        var hasPermission = _authService.HasPermission("ManageCashBox");
        
        // الاعتماد على الصلاحية فقط بدون التحقق من اسم الدور
        _isAdmin = hasPermission;
        
        // ✅ تهيئة آخر يوم معروض = اليوم الحالي
        _lastViewedDate = DateTime.Today;
        
        InitializeComponent();
        SetupForm();
        InitializeCustomControls();
        _ = LoadInitialDataAsync();
    }
    
    private void SetupForm()
    {
        this.Text = "إدارة الخزنة والبنوك";
        this.Size = new Size(1400, 900);
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = Color.FromArgb(245, 245, 245);
        this.Font = new Font("Cairo", 10F);
        
        // ✅ إعدادات مختلفة حسب الوضع (embedded في dashboard أو standalone window)
        if (_serviceProvider != null)
        {
            // Embedded mode - يعرض داخل dashboard
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
        }
        else
        {
            // Standalone mode - نافذة مستقلة
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(1200, 700);
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.ShowIcon = true;
            this.ShowInTaskbar = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // ✅ إضافة الأيقونة إذا كانت متوفرة
            try
            {
                if (Program.AppIcon != null)
                    this.Icon = Program.AppIcon;
            }
            catch
            {
                // تجاهل أخطاء تحميل الأيقونة
            }
        }
    }
    
    private void InitializeCustomControls()
    {
        // Header Panel
        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 80,
            BackColor = Color.White,
            Padding = new Padding(20)
        };
        
        Label titleLabel = new Label
        {
            Text = "💰 إدارة الخزنة والبنوك",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(41, 128, 185),
            AutoSize = true,
            Location = new Point(20, 25)
        };
        _headerPanel.Controls.Add(titleLabel);
        this.Controls.Add(_headerPanel);
        
        // Main Layout
        TableLayoutPanel mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(20)
        };
        
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 450)); // ✅ زيادة كبيرة من 380 إلى 450 بكسل
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        
        CreateSummaryPanel();
        mainLayout.Controls.Add(_summaryPanel, 0, 0);
        
        Panel filterPanel = CreateFilterPanel();
        mainLayout.Controls.Add(filterPanel, 0, 1);
        
        CreateContentPanel();
        mainLayout.Controls.Add(_contentPanel, 0, 2);
        
        this.Controls.Add(mainLayout);
    }
    
    private void CreateSummaryPanel()
    {
        _summaryPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(15),
            AutoScroll = false
        };
        
        // Main Layout بصفين
        TableLayoutPanel mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            RowStyles =
            {
                new RowStyle(SizeType.Percent, 55F), // ✅ تقليل من 62% إلى 55%
                new RowStyle(SizeType.Percent, 45F)  // ✅ زيادة من 38% إلى 45%
            }
        };
        
        // الصف الأول: بطاقات كبيرة (الرصيد، الإيرادات، المصروفات، الربح) - بالجنيه المصري فقط
        TableLayoutPanel cardsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1
        };
        
        cardsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        cardsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        cardsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        cardsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        
        Panel balanceCard = CreateSummaryCard("الرصيد الحالي (EGP)", "0.00 جنيه", Color.FromArgb(52, 152, 219), "💵");
        _balanceLabel = FindValueLabel(balanceCard);
        cardsLayout.Controls.Add(balanceCard, 0, 0);
        
        Panel incomeCard = CreateSummaryCard("إيرادات الشهر (EGP)", "0.00 جنيه", Color.FromArgb(46, 204, 113), "📈");
        _monthIncomeLabel = FindValueLabel(incomeCard);
        cardsLayout.Controls.Add(incomeCard, 1, 0);
        
        Panel expenseCard = CreateSummaryCard("مصروفات الشهر (EGP)", "0.00 جنيه", Color.FromArgb(231, 76, 60), "📉");
        _monthExpenseLabel = FindValueLabel(expenseCard);
        cardsLayout.Controls.Add(expenseCard, 2, 0);
        
        Panel profitCard = CreateSummaryCard("صافي الربح/الخسارة (EGP)", "0.00 جنيه", Color.FromArgb(230, 126, 34), "💰");
        _monthProfitLabel = FindValueLabel(profitCard);
        cardsLayout.Controls.Add(profitCard, 3, 0);
        
        mainLayout.Controls.Add(cardsLayout, 0, 0);
        
        // ✅ الصف الثاني: بطاقات صغيرة للعملات الأجنبية
        TableLayoutPanel currenciesLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Padding = new Padding(0, 35, 0, 30) // ✅ زيادة كبيرة: 35px من فوق، 30px من تحت
        };
        
        currenciesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        currenciesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        currenciesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        currenciesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        
        Panel usdCard = CreateCurrencyMiniCard("دولار أمريكي", "0.00", "$", Color.FromArgb(76, 175, 80));
        _usdBalanceLabel = FindValueLabel(usdCard);
        currenciesLayout.Controls.Add(usdCard, 0, 0);
        
        Panel eurCard = CreateCurrencyMiniCard("يورو", "0.00", "€", Color.FromArgb(33, 150, 243));
        _eurBalanceLabel = FindValueLabel(eurCard);
        currenciesLayout.Controls.Add(eurCard, 1, 0);
        
        Panel gbpCard = CreateCurrencyMiniCard("جنيه إسترليني", "0.00", "£", Color.FromArgb(156, 39, 176));
        _gbpBalanceLabel = FindValueLabel(gbpCard);
        currenciesLayout.Controls.Add(gbpCard, 2, 0);
        
        Panel sarCard = CreateCurrencyMiniCard("ريال سعودي", "0.00", "﷼", Color.FromArgb(255, 152, 0));
        _sarBalanceLabel = FindValueLabel(sarCard);
        currenciesLayout.Controls.Add(sarCard, 3, 0);
        
        mainLayout.Controls.Add(currenciesLayout, 0, 1);
        
        _summaryPanel.Controls.Add(mainLayout);
    }
    
    /// <summary>
    /// ✅ إنشاء بطاقة صغيرة للعملات الأجنبية
    /// </summary>
    private Panel CreateCurrencyMiniCard(string title, string value, string symbol, Color color)
    {
        Panel card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(18), // ✅ زيادة من 15 إلى 18 بكسل
            Margin = new Padding(12, 10, 12, 10), // ✅ زيادة: 12px أفقي، 10px عمودي
            BorderStyle = BorderStyle.FixedSingle
        };
        
        // Layout
        TableLayoutPanel layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
        
        // رمز العملة (كبير)
        Label symbolLabel = new Label
        {
            Text = symbol,
            Font = new Font("Cairo", 22F, FontStyle.Bold), // ✅ زيادة الحجم من 18 إلى 22
            ForeColor = color,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent
        };
        layout.Controls.Add(symbolLabel, 0, 0);
        layout.SetRowSpan(symbolLabel, 2);
        
        // اسم العملة
        Label titleLabel = new Label
        {
            Text = title,
            Font = new Font("Cairo", 9F, FontStyle.Bold), // ✅ زيادة الحجم من 8 إلى 9
            ForeColor = Color.FromArgb(100, 100, 100),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomRight,
            BackColor = Color.Transparent
        };
        layout.Controls.Add(titleLabel, 1, 0);
        
        // القيمة
        Label valueLabel = new Label
        {
            Text = value,
            Name = "valueLabel",
            Font = new Font("Cairo", 14F, FontStyle.Bold), // ✅ زيادة الحجم من 12 إلى 14
            ForeColor = color,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopRight,
            BackColor = Color.Transparent
        };
        layout.Controls.Add(valueLabel, 1, 1);
        
        card.Controls.Add(layout);
        return card;
    }
    
    private Label FindValueLabel(Panel card)
    {
        // البحث عن الـ TableLayoutPanel داخل الـ card
        foreach (Control control in card.Controls)
        {
            if (control is TableLayoutPanel layout)
            {
                // البحث عن الـ Label بإسم valueLabel داخل الـ TableLayoutPanel
                foreach (Control innerControl in layout.Controls)
                {
                    if (innerControl is Label label && label.Name == "valueLabel")
                    {
                        return label;
                    }
                }
            }
        }
        return null!;
    }
    
    private Panel CreateSummaryCard(string title, string value, Color color, string icon)
    {
        Panel card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = color,
            Padding = new Padding(25),
            Margin = new Padding(10)
        };
        
        // Rounded corners
        card.Paint += (s, e) =>
        {
            using (var path = new GraphicsPath())
            {
                int radius = 15;
                var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();
                card.Region = new Region(path);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            }
        };
        
        // Layout container for better organization
        TableLayoutPanel layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        
        // Title at top
        Label titleLabel = new Label
        {
            Text = title,
            Font = new Font("Cairo", 13F, FontStyle.Bold),
            ForeColor = Color.White,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent
        };
        layout.Controls.Add(titleLabel, 0, 0);
        
        // Value in the center - THIS IS THE MAIN ELEMENT
        Label valueLabel = new Label
        {
            Text = value,
            Name = "valueLabel",
            Font = new Font("Cairo", 26F, FontStyle.Bold),
            ForeColor = Color.White,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent
        };
        layout.Controls.Add(valueLabel, 0, 1);
        
        card.Controls.Add(layout);
        return card;
    }
    
    private Panel CreateFilterPanel()
    {
        Panel filterPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(15)
        };
        
        int xPos = 15;
        
        // Cashbox Selector
        Label cashBoxLabel = new Label
        {
            Text = "الخزنة/البنك:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(xPos, 15)
        };
        filterPanel.Controls.Add(cashBoxLabel);
        
        _cashBoxCombo = new ComboBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(200, 30),
            Location = new Point(xPos, 45),
            DropDownStyle = ComboBoxStyle.DropDownList,
            RightToLeft = RightToLeft.Yes
        };
        // سنضيف event handler لاحقاً في LoadInitialDataAsync
        filterPanel.Controls.Add(_cashBoxCombo);
        xPos += 220;
        
        // Month Selector
        Label monthLabel = new Label
        {
            Text = "الشهر:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(xPos, 15)
        };
        filterPanel.Controls.Add(monthLabel);
        
        _monthCombo = new ComboBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(120, 30),
            Location = new Point(xPos, 45),
            DropDownStyle = ComboBoxStyle.DropDownList,
            RightToLeft = RightToLeft.Yes
        };
        _monthCombo.Items.AddRange(new object[]
        {
            "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
            "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
        });
        _monthCombo.SelectedIndex = DateTime.Now.Month - 1;
        filterPanel.Controls.Add(_monthCombo);
        xPos += 140;
        
        // Year Selector
        Label yearLabel = new Label
        {
            Text = "السنة:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(xPos, 15)
        };
        filterPanel.Controls.Add(yearLabel);
        
        _yearCombo = new ComboBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(100, 30),
            Location = new Point(xPos, 45),
            DropDownStyle = ComboBoxStyle.DropDownList,
            RightToLeft = RightToLeft.Yes
        };
        // من سنة 2020 لحد السنة الحالية من الكمبيوتر - تزيد تلقائياً مع كل سنة جديدة
        for (int year = 2020; year <= DateTime.Now.Year; year++)
        {
            _yearCombo.Items.Add(year);
        }
        _yearCombo.SelectedItem = DateTime.Now.Year;
        filterPanel.Controls.Add(_yearCombo);
        xPos += 120;
        
        // Day Filter Checkbox
        _filterByDayCheckbox = new CheckBox
        {
            Text = "فلتر بيوم محدد",
            Font = new Font("Cairo", 9F),
            AutoSize = true,
            Location = new Point(xPos, 15),
            RightToLeft = RightToLeft.Yes
        };
        _filterByDayCheckbox.CheckedChanged += (s, e) =>
        {
            _dayFilter.Enabled = _filterByDayCheckbox.Checked;
        };
        filterPanel.Controls.Add(_filterByDayCheckbox);
        
        _dayFilter = new DateTimePicker
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(150, 30),
            Location = new Point(xPos, 45),
            Format = DateTimePickerFormat.Short,
            RightToLeft = RightToLeft.Yes,
            Enabled = false
        };
        filterPanel.Controls.Add(_dayFilter);
        xPos += 170;
        
        // Filter Button
        _filterButton = new Button
        {
            Text = "🔍 بحث",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Size = new Size(100, 55),
            Location = new Point(xPos, 15),
            BackColor = Color.FromArgb(41, 128, 185),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _filterButton.FlatAppearance.BorderSize = 0;
        _filterButton.Click += async (s, e) => await LoadDataAsync();
        filterPanel.Controls.Add(_filterButton);
        xPos += 120;
        
        // Screenshot Button
        _screenshotButton = new Button
        {
            Text = "📸 لقطة شاشة",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Size = new Size(130, 55),
            Location = new Point(xPos, 15),
            BackColor = Color.FromArgb(100, 100, 100),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _screenshotButton.FlatAppearance.BorderSize = 0;
        _screenshotButton.Click += ScreenshotButton_Click;
        filterPanel.Controls.Add(_screenshotButton);
        xPos += 150;
        
        // WhatsApp Button
        _whatsappButton = new Button
        {
            Text = "📱 مشاركة واتساب",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Size = new Size(150, 55),
            Location = new Point(xPos, 15),
            BackColor = Color.FromArgb(37, 211, 102),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _whatsappButton.FlatAppearance.BorderSize = 0;
        _whatsappButton.Click += WhatsAppButton_Click;
        _whatsappButton.MouseEnter += (s, e) => _whatsappButton.BackColor = Color.FromArgb(30, 180, 85);
        _whatsappButton.MouseLeave += (s, e) => _whatsappButton.BackColor = Color.FromArgb(37, 211, 102);
        filterPanel.Controls.Add(_whatsappButton);
        
        return filterPanel;
    }
    
    private void CreateContentPanel()
    {
        _contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(15, 15, 15, 15)
        };
        
        // Buttons Panel
        Panel buttonsPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.Transparent
        };
        
        int btnXPos = 15;
        
        _addIncomeButton = new Button
        {
            Text = "➕ إضافة إيراد",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Size = new Size(170, 45),
            Location = new Point(btnXPos, 5),
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _addIncomeButton.FlatAppearance.BorderSize = 0;
        _addIncomeButton.Click += AddIncomeButton_Click;
        buttonsPanel.Controls.Add(_addIncomeButton);
        btnXPos += 180;
        
        _addExpenseButton = new Button
        {
            Text = "➖ إضافة مصروف",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Size = new Size(170, 45),
            Location = new Point(btnXPos, 5),
            BackColor = Color.FromArgb(231, 76, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _addExpenseButton.FlatAppearance.BorderSize = 0;
        _addExpenseButton.Click += AddExpenseButton_Click;
        buttonsPanel.Controls.Add(_addExpenseButton);
        btnXPos += 180;
        
        _viewReportButton = new Button
        {
            Text = "📊 التقرير الشهري",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Size = new Size(170, 45),
            Location = new Point(btnXPos, 5),
            BackColor = Color.FromArgb(230, 126, 34),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _viewReportButton.FlatAppearance.BorderSize = 0;
        _viewReportButton.Click += ViewReportButton_Click;
        buttonsPanel.Controls.Add(_viewReportButton);
        btnXPos += 180;
        
        // زرار إضافة خزنة جديدة - متاح لجميع المستخدمين
        _addCashBoxButton = new Button
        {
            Text = "➕ خزنة جديدة",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Size = new Size(140, 45),
            Location = new Point(btnXPos, 5),
            BackColor = Color.FromArgb(155, 89, 182),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _addCashBoxButton.FlatAppearance.BorderSize = 0;
        _addCashBoxButton.Click += AddCashBoxButton_Click;
        buttonsPanel.Controls.Add(_addCashBoxButton);
        btnXPos += 150;
        
        // زرار تعديل الخزنة
        _editCashBoxButton = new Button
        {
            Text = "✏️ تعديل خزنة",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Size = new Size(140, 45),
            Location = new Point(btnXPos, 5),
            BackColor = Color.FromArgb(52, 152, 219),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _editCashBoxButton.FlatAppearance.BorderSize = 0;
        _editCashBoxButton.Click += EditCashBoxButton_Click;
        buttonsPanel.Controls.Add(_editCashBoxButton);
        btnXPos += 150;
        
        // زرار حذف الخزنة
        _deleteCashBoxButton = new Button
        {
            Text = "🗑️ حذف خزنة",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Size = new Size(140, 45),
            Location = new Point(btnXPos, 5),
            BackColor = Color.FromArgb(192, 57, 43),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _deleteCashBoxButton.FlatAppearance.BorderSize = 0;
        _deleteCashBoxButton.Click += DeleteCashBoxButton_Click;
        buttonsPanel.Controls.Add(_deleteCashBoxButton);
        btnXPos += 150;
        
        // زرار تعديل البند المحدد
        _editTransactionButton = new Button
        {
            Text = "✏️ تعديل البند",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Size = new Size(140, 45),
            Location = new Point(btnXPos, 5),
            BackColor = Color.FromArgb(230, 126, 34),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _editTransactionButton.FlatAppearance.BorderSize = 0;
        _editTransactionButton.Click += EditTransactionButton_Click;
        buttonsPanel.Controls.Add(_editTransactionButton);
        btnXPos += 150;
        
        // زرار حذف البند المحدد
        _deleteTransactionButton = new Button
        {
            Text = "🗑️ حذف البند",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Size = new Size(140, 45),
            Location = new Point(btnXPos, 5),
            BackColor = Color.FromArgb(160, 32, 32),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _deleteTransactionButton.FlatAppearance.BorderSize = 0;
        _deleteTransactionButton.Click += DeleteTransactionButton_Click;
        buttonsPanel.Controls.Add(_deleteTransactionButton);
        
        _contentPanel.Controls.Add(buttonsPanel);
        
        // Grid Container Panel with padding to show headers
        Panel gridContainer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(0, 75, 0, 0) // 75px from top to show headers clearly
        };
        
        // DataGridView
        _transactionsGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            ColumnHeadersHeight = 50,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            RowTemplate = { Height = 40 },
            Font = new Font("Cairo", 10F),
            RightToLeft = RightToLeft.Yes
        };
        
        _transactionsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
        _transactionsGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _transactionsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        _transactionsGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _transactionsGrid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        _transactionsGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
        
        // Add columns
        _transactionsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TransactionId",
            HeaderText = "م",
            Width = 50,
            Visible = false
        });
        
        _transactionsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ReceiptNumber",
            HeaderText = "رقم الإيصال",
            Width = 100
        });
        
        _transactionsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Date",
            HeaderText = "التاريخ",
            Width = 150
        });
        
        _transactionsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ExpenseName",
            HeaderText = "المصروف",
            Width = 180
        });
        
        _transactionsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ExpenseAmount",
            HeaderText = "قيمة المصروف",
            Width = 120
        });
        
        _transactionsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "IncomeName",
            HeaderText = "الإيراد",
            Width = 180
        });
        
        _transactionsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "IncomeAmount",
            HeaderText = "قيمة الإيراد",
            Width = 120
        });
        
        _transactionsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "PaymentMethod",
            HeaderText = "طريقة الدفع",
            Width = 120
        });
        
        _transactionsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Commission",
            HeaderText = "العمولة",
            Width = 100
        });
        
        // ✅ عمود جديد للعملة
        _transactionsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Currency",
            HeaderText = "العملة",
            Width = 80
        });
        
        // ✅ عمود اسم المستخدم
        _transactionsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "CreatedByUser",
            HeaderText = "المستخدم",
            Width = 120
        });
        
        // Context menu for admin
        if (_isAdmin)
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            
            ToolStripMenuItem editItem = new ToolStripMenuItem("✏️ تعديل الحركة");
            editItem.Click += EditTransactionMenuItem_Click;
            contextMenu.Items.Add(editItem);
            
            ToolStripMenuItem deleteItem = new ToolStripMenuItem("🗑️ حذف الحركة");
            deleteItem.Click += DeleteTransactionMenuItem_Click;
            contextMenu.Items.Add(deleteItem);
            
            _transactionsGrid.ContextMenuStrip = contextMenu;
        }
        
        // Add double-click event to show transaction details
        _transactionsGrid.CellDoubleClick += TransactionsGrid_CellDoubleClick;
        
        gridContainer.Controls.Add(_transactionsGrid);
        _contentPanel.Controls.Add(gridContainer);
    }
    
    private async Task LoadInitialDataAsync()
    {
        try
        {
            _cashBoxes = await _cashBoxService.GetActiveCashBoxesAsync();
            
            // إزالة الـ event handler مؤقتاً عشان SelectedIndex assignment لا يـfire LoadDataAsync
            _cashBoxCombo.SelectedIndexChanged -= CashBoxCombo_SelectedIndexChanged;
            
            _cashBoxCombo.Items.Clear();
            foreach (var cashBox in _cashBoxes)
            {
                _cashBoxCombo.Items.Add($"{cashBox.Name} ({GetCashBoxTypeArabic(cashBox.Type)})");
            }
            
            if (_cashBoxCombo.Items.Count > 0)
            {
                _cashBoxCombo.SelectedIndex = 0;
                _selectedCashBoxId = _cashBoxes[0].Id;
            }
            
            // ✅ فلتر اليوم الحالي فقط عند الفتح
            _selectedMonth = DateTime.Now.Month;
            _selectedYear = DateTime.Now.Year;
            
            // ✅ تفعيل فلتر اليوم تلقائياً
            _filterByDayCheckbox.Checked = true;
            _dayFilter.Value = DateTime.Today;
            _dayFilter.Enabled = true;
            
            // إعادة إضافة الـ event handler بعد تحديد القيمة
            _cashBoxCombo.SelectedIndexChanged += CashBoxCombo_SelectedIndexChanged;
            
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل البيانات الأولية:\n{ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }
    
    private async void CashBoxCombo_SelectedIndexChanged(object? sender, EventArgs e)
    {
        await LoadDataAsync();
    }
    
    /// <summary>
    /// ✅ التحقق من تغيير اليوم وتصفير Grid تلقائياً
    /// </summary>
    private void CheckAndResetForNewDay()
    {
        // التحقق من تغيير اليوم
        if (DateTime.Today > _lastViewedDate)
        {
            System.Diagnostics.Debug.WriteLine($"📅 يوم جديد! تصفير الـ Grid...");
            System.Diagnostics.Debug.WriteLine($"   آخر عرض: {_lastViewedDate:yyyy-MM-dd}");
            System.Diagnostics.Debug.WriteLine($"   اليوم الحالي: {DateTime.Today:yyyy-MM-dd}");
            
            // تحديث آخر يوم معروض
            _lastViewedDate = DateTime.Today;
            
            // تفعيل فلتر اليوم تلقائياً
            _filterByDayCheckbox.Checked = true;
            _dayFilter.Value = DateTime.Today;
            _dayFilter.Enabled = true;
            
            // تحديث الشهر والسنة
            _monthCombo.SelectedIndex = DateTime.Today.Month - 1;
            _yearCombo.SelectedItem = DateTime.Today.Year;
        }
    }
    
    private async Task LoadDataAsync()
    {
        System.Diagnostics.Debug.WriteLine("=== LoadDataAsync started ===");
        
        // ✅ التحقق من تغيير اليوم أولاً
        CheckAndResetForNewDay();
        
        if (_cashBoxCombo.SelectedIndex < 0 || _isLoading)
        {
            System.Diagnostics.Debug.WriteLine($"تم الإلغاء: SelectedIndex={_cashBoxCombo.SelectedIndex}, isLoading={_isLoading}");
            return;
        }
        
        _isLoading = true;
        System.Diagnostics.Debug.WriteLine("تم تعيين _isLoading = true");
        
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            
            _selectedCashBoxId = _cashBoxes[_cashBoxCombo.SelectedIndex].Id;
            _selectedMonth = _monthCombo.SelectedIndex + 1;
            _selectedYear = _yearCombo.SelectedItem != null ? (int)_yearCombo.SelectedItem : DateTime.Now.Year;
        
            System.Diagnostics.Debug.WriteLine($"CashBoxId: {_selectedCashBoxId}, Month: {_selectedMonth}, Year: {_selectedYear}");
            
            // Calculate date range first
            DateTime? startDate = null;
            DateTime? endDate = null;
            
            if (_filterByDayCheckbox.Checked)
            {
                startDate = _dayFilter.Value.Date.ToUniversalTime();
                endDate = _dayFilter.Value.Date.AddDays(1).ToUniversalTime();
            }
            else
            {
                startDate = new DateTime(_selectedYear, _selectedMonth, 1, 0, 0, 0, DateTimeKind.Utc);
                endDate = startDate.Value.AddMonths(1);
            }
            
            System.Diagnostics.Debug.WriteLine($"تحميل البيانات من {startDate} إلى {endDate}");
            
            // تحميل البيانات من قاعدة البيانات - بالترتيب الصحيح
            // 1. التقرير الشهري (يحتوي على الحركات بالفعل)
            var monthlyReport = await _cashBoxService.GetMonthlyReportAsync(_selectedCashBoxId, _selectedMonth, _selectedYear);
            System.Diagnostics.Debug.WriteLine($"عدد الحركات في التقرير: {monthlyReport?.Transactions?.Count ?? 0}");
            
            // 2. الرصيد الحالي
            var currentBalance = await _cashBoxService.GetCurrentBalanceAsync(_selectedCashBoxId);
            System.Diagnostics.Debug.WriteLine($"الرصيد الحالي: {currentBalance}");
            
            // 3. استخدام الحركات من التقرير نفسه بدلاً من query جديد
            var transactions = monthlyReport?.Transactions ?? new List<CashTransaction>();
            
            // ✅ فلترة المعاملات حسب التاريخ المحدد (إذا كان الفلتر مفعل)
            if (_filterByDayCheckbox.Checked && startDate.HasValue && endDate.HasValue)
            {
                transactions = transactions
                    .Where(t => t.TransactionDate >= startDate.Value && t.TransactionDate < endDate.Value)
                    .ToList();
                
                System.Diagnostics.Debug.WriteLine($"عدد الحركات بعد الفلترة: {transactions.Count}");
                
                // إعادة حساب الإحصائيات للمعاملات المفلترة
                var filteredIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                var filteredExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                var filteredProfit = filteredIncome - filteredExpense;
                
                // تحديث التقرير بالأرقام المفلترة
                monthlyReport = new MonthlyReport
                {
                    TotalIncome = filteredIncome,
                    TotalExpense = filteredExpense,
                    NetProfit = filteredProfit,
                    Transactions = transactions
                };
            }
            
            System.Diagnostics.Debug.WriteLine("بدء تحديث الـ UI...");
            // تحديث الـ UI
            UpdateUI((decimal)currentBalance, monthlyReport!, transactions);
            System.Diagnostics.Debug.WriteLine("✅ تم تحديث الـ UI بنجاح");
        }
        catch (OperationCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("❌ انتهت مهلة تحميل البيانات");
            ShowError("انتهت مهلة تحميل البيانات. يرجى المحاولة مرة أخرى.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ خطأ في LoadDataAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            
            var errorMsg = $"خطأ في تحميل البيانات:\n{ex.Message}";
            
            if (ex.InnerException != null)
            {
                errorMsg += $"\n\nالسبب:\n{ex.InnerException.Message}";
            }
            
            ShowError(errorMsg);
        }
        finally
        {
            _isLoading = false;
            System.Diagnostics.Debug.WriteLine("تم تعيين _isLoading = false");
            System.Diagnostics.Debug.WriteLine("=== LoadDataAsync finished ===");
        }
    }
    
    private void UpdateUI(decimal currentBalance, MonthlyReport monthlyReport, List<CashTransaction> transactions)
    {
        System.Diagnostics.Debug.WriteLine($"=== UpdateUI called with {transactions.Count} transactions ===");
        
        // ✅ حساب المصروفات الحقيقية (بعد خصم العمولة) من المعاملات بالجنيه فقط
        decimal actualExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense && t.TransactionCurrency == "EGP")
            .Sum(t => {
                decimal amount = t.Amount;
                // خصم العمولة إذا كان الدفع عن طريق InstaPay
                if (t.PaymentMethod == PaymentMethod.InstaPay && t.InstaPayCommission.HasValue)
                {
                    amount -= t.InstaPayCommission.Value;
                }
                return amount;
            });
        
        // الإيرادات بالجنيه فقط
        decimal egpIncome = transactions
            .Where(t => t.Type == TransactionType.Income && t.TransactionCurrency == "EGP")
            .Sum(t => t.Amount);
        
        decimal actualProfit = egpIncome - actualExpenses;
        
        System.Diagnostics.Debug.WriteLine($"📊 Actual Expenses (بعد خصم العمولة): {actualExpenses:N2}");
        System.Diagnostics.Debug.WriteLine($"📊 Actual Profit: {actualProfit:N2}");
        
        // Update summary with explicit text assignment - الجنيه المصري فقط
        if (_balanceLabel != null)
        {
            _balanceLabel.Text = $"{currentBalance:N2} جنيه";
            _balanceLabel.Refresh();
        }
        
        if (_monthIncomeLabel != null)
        {
            _monthIncomeLabel.Text = $"{egpIncome:N2} جنيه";
            _monthIncomeLabel.Refresh();
        }
        
        if (_monthExpenseLabel != null)
        {
            _monthExpenseLabel.Text = $"{actualExpenses:N2} جنيه";
            _monthExpenseLabel.Refresh();
        }
        
        if (_monthProfitLabel != null)
        {
            _monthProfitLabel.Text = $"{actualProfit:N2} جنيه";
            _monthProfitLabel.ForeColor = actualProfit >= 0 
                ? Color.White
                : Color.FromArgb(255, 200, 200);
            _monthProfitLabel.Refresh();
        }
        
        // ✅ حساب وعرض أرصدة العملات الأجنبية
        decimal usdBalance = transactions
            .Where(t => t.TransactionCurrency == "USD")
            .Sum(t => {
                decimal amount = t.Amount;
                if (t.Type == TransactionType.Expense)
                {
                    // خصم العمولة من المصروف
                    if (t.PaymentMethod == PaymentMethod.InstaPay && t.InstaPayCommission.HasValue)
                    {
                        amount -= t.InstaPayCommission.Value;
                    }
                    return -amount;
                }
                return amount;
            });
        
        decimal eurBalance = transactions
            .Where(t => t.TransactionCurrency == "EUR")
            .Sum(t => {
                decimal amount = t.Amount;
                if (t.Type == TransactionType.Expense)
                {
                    if (t.PaymentMethod == PaymentMethod.InstaPay && t.InstaPayCommission.HasValue)
                    {
                        amount -= t.InstaPayCommission.Value;
                    }
                    return -amount;
                }
                return amount;
            });
        
        decimal gbpBalance = transactions
            .Where(t => t.TransactionCurrency == "GBP")
            .Sum(t => {
                decimal amount = t.Amount;
                if (t.Type == TransactionType.Expense)
                {
                    if (t.PaymentMethod == PaymentMethod.InstaPay && t.InstaPayCommission.HasValue)
                    {
                        amount -= t.InstaPayCommission.Value;
                    }
                    return -amount;
                }
                return amount;
            });
        
        decimal sarBalance = transactions
            .Where(t => t.TransactionCurrency == "SAR")
            .Sum(t => {
                decimal amount = t.Amount;
                if (t.Type == TransactionType.Expense)
                {
                    if (t.PaymentMethod == PaymentMethod.InstaPay && t.InstaPayCommission.HasValue)
                    {
                        amount -= t.InstaPayCommission.Value;
                    }
                    return -amount;
                }
                return amount;
            });
        
        // تحديث البطاقات الصغيرة للعملات
        if (_usdBalanceLabel != null)
        {
            _usdBalanceLabel.Text = $"{usdBalance:N2}";
            _usdBalanceLabel.Refresh();
        }
        
        if (_eurBalanceLabel != null)
        {
            _eurBalanceLabel.Text = $"{eurBalance:N2}";
            _eurBalanceLabel.Refresh();
        }
        
        if (_gbpBalanceLabel != null)
        {
            _gbpBalanceLabel.Text = $"{gbpBalance:N2}";
            _gbpBalanceLabel.Refresh();
        }
        
        if (_sarBalanceLabel != null)
        {
            _sarBalanceLabel.Text = $"{sarBalance:N2}";
            _sarBalanceLabel.Refresh();
        }
        
        System.Diagnostics.Debug.WriteLine($"💱 USD: {usdBalance:N2}, EUR: {eurBalance:N2}, GBP: {gbpBalance:N2}, SAR: {sarBalance:N2}");
        
        // Display transactions
        System.Diagnostics.Debug.WriteLine($"تنظيف الـ Grid - عدد الصفوف قبل المسح: {_transactionsGrid.Rows.Count}");
        _transactionsGrid.Rows.Clear();
        System.Diagnostics.Debug.WriteLine($"تم تنظيف الـ Grid - عدد الصفوف الآن: {_transactionsGrid.Rows.Count}");
        
        decimal totalExpenses = 0;
        decimal totalIncome = 0;
        
        int rowIndex = 0;
        foreach (var transaction in transactions.OrderByDescending(t => t.TransactionDate))
        {
            System.Diagnostics.Debug.WriteLine($"إضافة صف {rowIndex}: ID={transaction.Id}, Amount={transaction.Amount}, Description={transaction.Description}, PaymentMethod={transaction.PaymentMethod}");
            
            var row = _transactionsGrid.Rows[_transactionsGrid.Rows.Add()];
            row.Cells["TransactionId"].Value = transaction.Id;
            row.Cells["ReceiptNumber"].Value = transaction.VoucherNumber ?? "-";
            row.Cells["Date"].Value = transaction.TransactionDate.ToString("yyyy/MM/dd HH:mm");
            
            // عرض طريقة الدفع
            string paymentMethodText = GetPaymentMethodArabic(transaction.PaymentMethod);
            row.Cells["PaymentMethod"].Value = paymentMethodText;
            row.Cells["PaymentMethod"].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            
            System.Diagnostics.Debug.WriteLine($"   → PaymentMethod عربي: {paymentMethodText}");
            
            // عرض العمولة (إن وجدت)
            if (transaction.InstaPayCommission.HasValue && transaction.InstaPayCommission.Value > 0)
            {
                row.Cells["Commission"].Value = $"{transaction.InstaPayCommission.Value:N2}";
                row.Cells["Commission"].Style.ForeColor = Color.FromArgb(200, 50, 50);
                row.Cells["Commission"].Style.Font = new Font("Cairo", 9F, FontStyle.Bold);
                row.Cells["Commission"].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            else
            {
                row.Cells["Commission"].Value = "-";
                row.Cells["Commission"].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            
            // ✅ عرض العملة مع أيقونة مميزة
            string currencyDisplay = GetCurrencyDisplay(transaction.TransactionCurrency, transaction.OriginalAmount, transaction.ExchangeRateUsed);
            row.Cells["Currency"].Value = currencyDisplay;
            row.Cells["Currency"].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            
            // تلوين العملات الأجنبية بلون مختلف
            if (transaction.TransactionCurrency != "EGP")
            {
                row.Cells["Currency"].Style.ForeColor = Color.FromArgb(0, 102, 204);
                row.Cells["Currency"].Style.Font = new Font("Cairo", 9F, FontStyle.Bold);
            }
            
            // ✅ عرض اسم المستخدم
            string creatorName = transaction.Creator?.FullName ?? "غير معروف";
            row.Cells["CreatedByUser"].Value = creatorName;
            row.Cells["CreatedByUser"].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            row.Cells["CreatedByUser"].Style.Font = new Font("Cairo", 9F, FontStyle.Bold);
            row.Cells["CreatedByUser"].Style.ForeColor = Color.FromArgb(52, 152, 219);
            
            if (transaction.Type == TransactionType.Expense)
            {
                row.Cells["ExpenseName"].Value = transaction.Description ?? transaction.Category ?? "مصروف";
                
                // ✅ عرض المبلغ بعد خصم العمولة في حالة InstaPay
                decimal displayAmount = transaction.Amount;
                if (transaction.PaymentMethod == PaymentMethod.InstaPay && transaction.InstaPayCommission.HasValue)
                {
                    displayAmount = transaction.Amount - transaction.InstaPayCommission.Value;
                }
                
                row.Cells["ExpenseAmount"].Value = $"{displayAmount:N2}";
                row.Cells["IncomeName"].Value = "";
                row.Cells["IncomeAmount"].Value = "";
                totalExpenses += displayAmount; // ✅ الإجمالي أيضاً يكون بعد الخصم
                
                row.Cells["ExpenseAmount"].Style.ForeColor = Color.FromArgb(231, 76, 60);
                row.Cells["ExpenseAmount"].Style.Font = new Font("Cairo", 10F, FontStyle.Bold);
            }
            else
            {
                row.Cells["ExpenseName"].Value = "";
                row.Cells["ExpenseAmount"].Value = "";
                row.Cells["IncomeName"].Value = transaction.Description ?? transaction.Category ?? "إيراد";
                row.Cells["IncomeAmount"].Value = $"{transaction.Amount:N2}";
                totalIncome += transaction.Amount;
                
                row.Cells["IncomeAmount"].Style.ForeColor = Color.FromArgb(46, 204, 113);
                row.Cells["IncomeAmount"].Style.Font = new Font("Cairo", 10F, FontStyle.Bold);
            }
            
            rowIndex++;
        }
        
        System.Diagnostics.Debug.WriteLine($"✅ تمت إضافة {rowIndex} صف إلى الـ Grid");
        System.Diagnostics.Debug.WriteLine($"عدد الصفوف النهائي في الـ Grid: {_transactionsGrid.Rows.Count}");
        
        // Force refresh the grid
        _transactionsGrid.Refresh();
        _transactionsGrid.Update();
        
        // Add Total Row
        if (_transactionsGrid.Rows.Count > 0)
        {
            var totalRow = _transactionsGrid.Rows[_transactionsGrid.Rows.Add()];
            totalRow.Cells["ReceiptNumber"].Value = "";
            totalRow.Cells["Date"].Value = "الإجمالي";
            totalRow.Cells["ExpenseName"].Value = "";
            totalRow.Cells["ExpenseAmount"].Value = $"{totalExpenses:N2}";
            totalRow.Cells["IncomeName"].Value = "";
            totalRow.Cells["IncomeAmount"].Value = $"{totalIncome:N2}";
            totalRow.Cells["PaymentMethod"].Value = "";
            totalRow.Cells["Commission"].Value = "";
            totalRow.Cells["Currency"].Value = ""; // ✅ عمود العملة فارغ في الإجمالي
            totalRow.Cells["CreatedByUser"].Value = ""; // ✅ عمود المستخدم فارغ في الإجمالي
            
            // Style total row
            totalRow.DefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            totalRow.DefaultCellStyle.ForeColor = Color.White;
            totalRow.DefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
            totalRow.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        
        // ✅ Force complete refresh of the grid
        _transactionsGrid.Invalidate();
        _transactionsGrid.Update();
        _transactionsGrid.Refresh();
        
        System.Diagnostics.Debug.WriteLine("✅ UpdateUI completed - Grid has been refreshed");
    }
    
    private async void AddIncomeButton_Click(object? sender, EventArgs e)
    {
        if (_selectedCashBoxId == 0)
        {
            ShowError("برجاء اختيار خزنة أولاً");
            return;
        }
        
        // ✅ فتح النافذة كـ non-modal لتمكين فتح نوافذ متعددة
        var form = new AddTransactionForm("Income", _selectedCashBoxId, _cashBoxService, _currentUserId);
        form.FormClosed += async (s, args) => await LoadDataAsync(); // ✅ تحديث البيانات عند الإغلاق
        form.Show();
    }
    
    private async void AddExpenseButton_Click(object? sender, EventArgs e)
    {
        if (_selectedCashBoxId == 0)
        {
            ShowError("برجاء اختيار خزنة أولاً");
            return;
        }
        
        // ✅ فتح النافذة كـ non-modal لتمكين فتح نوافذ متعددة
        var form = new AddTransactionForm("Expense", _selectedCashBoxId, _cashBoxService, _currentUserId);
        form.FormClosed += async (s, args) => await LoadDataAsync(); // ✅ تحديث البيانات عند الإغلاق
        form.Show();
    }
    
    private void ViewReportButton_Click(object? sender, EventArgs e)
    {
        if (_selectedCashBoxId == 0)
        {
            ShowError("برجاء اختيار خزنة أولاً");
            return;
        }
        
        // ✅ فتح النافذة كـ non-modal لتمكين فتح نوافذ متعددة
        var form = new CashBoxReportForm(_selectedCashBoxId, _cashBoxes.First(c => c.Id == _selectedCashBoxId).Name, _selectedMonth, _selectedYear, _cashBoxService);
        form.Show();
    }
    
    private async void AddCashBoxButton_Click(object? sender, EventArgs e)
    {
        // ✅ فتح النافذة كـ non-modal لتمكين فتح نوافذ متعددة
        var form = new AddCashBoxForm(_cashBoxService, _currentUserId);
        form.FormClosed += async (s, args) => await LoadInitialDataAsync(); // ✅ تحديث البيانات عند الإغلاق
        form.Show();
    }
    
    private async void EditCashBoxButton_Click(object? sender, EventArgs e)
    {
        if (_selectedCashBoxId == 0)
        {
            ShowError("برجاء اختيار خزنة أولاً");
            return;
        }
        
        // ✅ فتح النافذة كـ non-modal لتمكين فتح نوافذ متعددة
        var form = new EditCashBoxForm(_selectedCashBoxId, _cashBoxService, _currentUserId);
        form.FormClosed += async (s, args) => await LoadInitialDataAsync(); // ✅ تحديث البيانات عند الإغلاق
        form.Show();
    }
    
    private async void DeleteCashBoxButton_Click(object? sender, EventArgs e)
    {
        if (_selectedCashBoxId == 0)
        {
            ShowError("برجاء اختيار خزنة أولاً");
            return;
        }
        
        var cashBoxName = _cashBoxes.First(c => c.Id == _selectedCashBoxId).Name;
        
        var result = MessageBox.Show(
            $"هل أنت متأكد من حذف الخزنة: {cashBoxName}؟\n\n" +
            $"⚠️ تحذير: سيتم حذف جميع الحركات المرتبطة بهذه الخزنة!\n\n" +
            $"هذا الإجراء لا يمكن التراجع عنه.", 
            "تأكيد الحذف",
            MessageBoxButtons.YesNo, 
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2,
            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        
        if (result == DialogResult.Yes)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== DeleteCashBoxButton_Click ===");
                System.Diagnostics.Debug.WriteLine($"حذف الخزنة: {cashBoxName} (ID: {_selectedCashBoxId})");
                
                // حذف الخزنة
                await _cashBoxService.DeleteCashBoxAsync(_selectedCashBoxId);
                
                System.Diagnostics.Debug.WriteLine("✅ تم الحذف من قاعدة البيانات");
                
                // إعادة تعيين الـ ID
                _selectedCashBoxId = 0;
                
                // تنظيف الـ Grid فوراً
                _transactionsGrid.Rows.Clear();
                _transactionsGrid.Refresh();
                
                // تحديث Summary بأصفار
                if (_balanceLabel != null) _balanceLabel.Text = "0.00 جنيه";
                if (_monthIncomeLabel != null) _monthIncomeLabel.Text = "0.00 جنيه";
                if (_monthExpenseLabel != null) _monthExpenseLabel.Text = "0.00 جنيه";
                if (_monthProfitLabel != null) _monthProfitLabel.Text = "0.00 جنيه";
                if (_usdBalanceLabel != null) _usdBalanceLabel.Text = "0.00";
                if (_eurBalanceLabel != null) _eurBalanceLabel.Text = "0.00";
                if (_gbpBalanceLabel != null) _gbpBalanceLabel.Text = "0.00";
                if (_sarBalanceLabel != null) _sarBalanceLabel.Text = "0.00";
                
                // رسالة النجاح
                MessageBox.Show(
                    $"✅ تم حذف الخزنة \"{cashBoxName}\" بنجاح\n\n" +
                    $"تم حذف جميع الحركات المرتبطة بها", 
                    "نجح",
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                
                System.Diagnostics.Debug.WriteLine("إعادة تحميل قائمة الخزائن...");
                
                // إعادة تحميل البيانات الأولية
                await LoadInitialDataAsync();
                
                System.Diagnostics.Debug.WriteLine("✅ اكتمل التحديث بنجاح");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ خطأ في الحذف: {ex.Message}");
                ShowError($"خطأ في حذف الخزنة:\n{ex.Message}");
            }
        }
    }
    
    // زرار تعديل البند في الـ toolbar
    private async void EditTransactionButton_Click(object? sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("=== EditTransactionButton_Click ===");
        
        if (_transactionsGrid.SelectedRows.Count == 0)
        {
            ShowError("برجاء اختيار بند من القائمة أولاً");
            return;
        }

        var row = _transactionsGrid.SelectedRows[0];
        if (row.Cells["Date"].Value?.ToString() == "الإجمالي")
        {
            ShowError("لا يمكن تعديل صف الإجمالي");
            return;
        }

        if (row.Cells["TransactionId"].Value == null || row.Cells["TransactionId"].Value == DBNull.Value)
        {
            ShowError("لا يمكن تعديل هذا البند");
            return;
        }

        int transactionId = Convert.ToInt32(row.Cells["TransactionId"].Value);
        int selectedRowIndex = row.Index; // حفظ موضع الصف المحدد
        System.Diagnostics.Debug.WriteLine($"فتح EditTransactionForm للبند رقم: {transactionId}");

        // ✅ استخدام ShowDialog() لضمان الحصول على DialogResult الصحيح
        var form = new EditTransactionForm(transactionId, _cashBoxService, _currentUserId);
        var result = form.ShowDialog(this);
        
        System.Diagnostics.Debug.WriteLine($"DialogResult: {result}, TransactionUpdated: {form.TransactionUpdated}");
        
        if (result == DialogResult.OK || form.TransactionUpdated)
        {
            System.Diagnostics.Debug.WriteLine("✅ تم الحفظ بنجاح - إعادة تحميل البيانات...");
            
            // ✅ استخدام ReloadAfterEdit بدلاً من LoadDataAsync
            // لتجنب مشكلة _isLoading flag
            await ReloadAfterEdit();
            
            // ✅ إعادة تحديد الصف نفسه بعد التحديث
            if (selectedRowIndex >= 0 && selectedRowIndex < _transactionsGrid.Rows.Count)
            {
                _transactionsGrid.ClearSelection();
                _transactionsGrid.Rows[selectedRowIndex].Selected = true;
                _transactionsGrid.FirstDisplayedScrollingRowIndex = selectedRowIndex;
            }
        }
    }
    
    // ✅ Method جديدة لإعادة التحميل الإجباري بعد التعديل - بدون التحقق من _isLoading
    private async Task ReloadAfterEdit()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== ReloadAfterEdit ===");
            
            // ✅ لا نتحقق من _isLoading لأن هذا reload إجباري بعد تعديل ناجح
            
            // تعطيل الـ controls أثناء التحميل
            _transactionsGrid.Enabled = false;
            _filterButton.Enabled = false;
            
            // Clear grid بشكل كامل
            _transactionsGrid.Rows.Clear();
            _transactionsGrid.DataSource = null;
            System.Diagnostics.Debug.WriteLine("تم مسح الـ Grid بالكامل");
            
            // Get fresh data directly from database
            var monthlyReport = await _cashBoxService.GetMonthlyReportAsync(_selectedCashBoxId, _selectedMonth, _selectedYear);
            var currentBalance = await _cashBoxService.GetCurrentBalanceAsync(_selectedCashBoxId);
            var transactions = monthlyReport?.Transactions ?? new List<CashTransaction>();
            
            System.Diagnostics.Debug.WriteLine($"تم تحميل {transactions.Count} معاملة من الداتابيز");
            
            // Apply day filter if needed
            if (_filterByDayCheckbox.Checked)
            {
                var startDate = _dayFilter.Value.Date.ToUniversalTime();
                var endDate = _dayFilter.Value.Date.AddDays(1).ToUniversalTime();
                
                transactions = transactions
                    .Where(t => t.TransactionDate >= startDate && t.TransactionDate < endDate)
                    .ToList();
                    
                System.Diagnostics.Debug.WriteLine($"بعد الفلترة: {transactions.Count} معاملة");
                
                // إعادة حساب الإحصائيات
                var filteredIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                var filteredExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                var filteredProfit = filteredIncome - filteredExpense;
                
                monthlyReport = new MonthlyReport
                {
                    TotalIncome = filteredIncome,
                    TotalExpense = filteredExpense,
                    NetProfit = filteredProfit,
                    Transactions = transactions
                };
            }
            
            // Update UI
            UpdateUI((decimal)currentBalance, monthlyReport!, transactions);
            
            System.Diagnostics.Debug.WriteLine("✅ تم تحديث الـ UI");
            
            // Force complete refresh
            _transactionsGrid.Invalidate(true);
            _transactionsGrid.Update();
            _transactionsGrid.Refresh();
            this.Refresh();
            
            // إعادة تفعيل الـ controls
            _transactionsGrid.Enabled = true;
            _filterButton.Enabled = true;
            
            System.Diagnostics.Debug.WriteLine("✅ اكتمل ReloadAfterEdit بنجاح");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ خطأ في ReloadAfterEdit: {ex.Message}");
            _transactionsGrid.Enabled = true;
            _filterButton.Enabled = true;
            ShowError($"خطأ في تحديث البيانات:\n{ex.Message}");
        }
    }
    private async Task ForceReloadData()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== ForceReloadData ===");
            
            // تعطيل الـ controls أثناء التحميل
            _transactionsGrid.Enabled = false;
            _filterButton.Enabled = false;
            
            // Suspend layout
            this.SuspendLayout();
            _transactionsGrid.SuspendLayout();
            
            // Clear grid first - بشكل إجباري وواضح
            _transactionsGrid.Rows.Clear();
            _transactionsGrid.DataSource = null; // ✅ إضافة هذا السطر
            System.Diagnostics.Debug.WriteLine("تم مسح الـ Grid بالكامل");
            
            // Get fresh data directly from database
            var monthlyReport = await _cashBoxService.GetMonthlyReportAsync(_selectedCashBoxId, _selectedMonth, _selectedYear);
            var currentBalance = await _cashBoxService.GetCurrentBalanceAsync(_selectedCashBoxId);
            var transactions = monthlyReport?.Transactions ?? new List<CashTransaction>();
            
            System.Diagnostics.Debug.WriteLine($"تم تحميل {transactions.Count} معاملة من الداتابيز");
            
            // Apply day filter if needed
            if (_filterByDayCheckbox.Checked)
            {
                var startDate = _dayFilter.Value.Date.ToUniversalTime();
                var endDate = _dayFilter.Value.Date.AddDays(1).ToUniversalTime();
                
                transactions = transactions
                    .Where(t => t.TransactionDate >= startDate && t.TransactionDate < endDate)
                    .ToList();
                    
                System.Diagnostics.Debug.WriteLine($"بعد الفلترة: {transactions.Count} معاملة");
                
                // ✅ إعادة حساب الإحصائيات للمعاملات المفلترة
                var filteredIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                var filteredExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                var filteredProfit = filteredIncome - filteredExpense;
                
                // تحديث التقرير بالأرقام المفلترة
                monthlyReport = new MonthlyReport
                {
                    TotalIncome = filteredIncome,
                    TotalExpense = filteredExpense,
                    NetProfit = filteredProfit,
                    Transactions = transactions
                };
            }
            
            // Update UI
            UpdateUI((decimal)currentBalance, monthlyReport!, transactions);
            
            System.Diagnostics.Debug.WriteLine("✅ تم تحديث الـ UI");
            
            // Resume layout
            _transactionsGrid.ResumeLayout(true);
            this.ResumeLayout(true);
            
            // Force complete refresh - بترتيب معين
            _transactionsGrid.Invalidate(true);  // ✅ Invalidate أولاً
            _transactionsGrid.Update();          // ✅ Update ثانياً
            _transactionsGrid.Refresh();         // ✅ Refresh ثالثاً
            this.Refresh();
            this.Update();
            
            // إعادة تفعيل الـ controls
            _transactionsGrid.Enabled = true;
            _filterButton.Enabled = true;
            
            System.Diagnostics.Debug.WriteLine("✅ اكتمل ForceReloadData بنجاح");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ خطأ في ForceReloadData: {ex.Message}");
            _transactionsGrid.Enabled = true;
            _filterButton.Enabled = true;
            ShowError($"خطأ في تحديث البيانات:\n{ex.Message}");
        }
    }

    // زرار حذف البند في الـ toolbar
    private async void DeleteTransactionButton_Click(object? sender, EventArgs e)
    {
        if (_transactionsGrid.SelectedRows.Count == 0)
        {
            ShowError("برجاء اختيار بند من القائمة أولاً");
            return;
        }

        var row = _transactionsGrid.SelectedRows[0];
        if (row.Cells["Date"].Value?.ToString() == "الإجمالي")
        {
            ShowError("لا يمكن حذف صف الإجمالي");
            return;
        }

        int transactionId = Convert.ToInt32(row.Cells["TransactionId"].Value);

        var result = MessageBox.Show(
            "هل أنت متأكد من حذف هذا البند؟\n\nلا يمكن التراجع عن هذا القرار.",
            "تأكيد الحذف",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2,
            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);

        if (result == DialogResult.Yes)
        {
            try
            {
                await _cashBoxService.DeleteTransactionAsync(transactionId);

                MessageBox.Show("✅ تم حذف البند بنجاح", "نجح",
                    MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);

                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في حذف البند:\n{ex.Message}");
            }
        }
    }

    // context menu (right-click) - تعديل البند
    private async void EditTransactionMenuItem_Click(object? sender, EventArgs e)
    {
        if (_transactionsGrid.SelectedRows.Count == 0) return;

        var row = _transactionsGrid.SelectedRows[0];
        if (row.Cells["Date"].Value?.ToString() == "الإجمالي") return;
        
        if (row.Cells["TransactionId"].Value == null || row.Cells["TransactionId"].Value == DBNull.Value) return;

        int transactionId = Convert.ToInt32(row.Cells["TransactionId"].Value);
        int selectedRowIndex = row.Index;

        var form = new EditTransactionForm(transactionId, _cashBoxService, _currentUserId);
        var result = form.ShowDialog(this);
        
        if (result == DialogResult.OK || form.TransactionUpdated)
        {
            // ✅ استخدام ReloadAfterEdit
            await ReloadAfterEdit();
            
            // ✅ إعادة تحديد الصف نفسه بعد التحديث
            if (selectedRowIndex >= 0 && selectedRowIndex < _transactionsGrid.Rows.Count)
            {
                _transactionsGrid.ClearSelection();
                _transactionsGrid.Rows[selectedRowIndex].Selected = true;
                _transactionsGrid.FirstDisplayedScrollingRowIndex = selectedRowIndex;
            }
        }
    }

    // context menu (right-click) - حذف البند
    private async void DeleteTransactionMenuItem_Click(object? sender, EventArgs e)
    {
        if (_transactionsGrid.SelectedRows.Count == 0) return;

        int transactionId = Convert.ToInt32(_transactionsGrid.SelectedRows[0].Cells["TransactionId"].Value);

        var result = MessageBox.Show(
            "هل أنت متأكد من حذف هذا البند؟\n\nلا يمكن التراجع عن هذا القرار.",
            "تأكيد الحذف",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2,
            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);

        if (result == DialogResult.Yes)
        {
            try
            {
                await _cashBoxService.DeleteTransactionAsync(transactionId);

                MessageBox.Show("✅ تم حذف البند بنجاح", "نجح",
                    MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);

                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في حذف البند:\n{ex.Message}");
            }
        }
    }
    
    private void ScreenshotButton_Click(object? sender, EventArgs e)
    {
        try
        {
            using Bitmap bmp = new Bitmap(this.Width, this.Height);
            this.DrawToBitmap(bmp, new Rectangle(0, 0, this.Width, this.Height));
            
            string fileName = $"CashBox_Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
            
            bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            
            MessageBox.Show($"✅ تم حفظ لقطة الشاشة بنجاح\n\n{filePath}", "نجح",
                MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
        catch (Exception ex)
        {
            ShowError($"خطأ في حفظ لقطة الشاشة:\n{ex.Message}");
        }
    }
    
    private string GetCashBoxTypeArabic(string type)
    {
        return type switch
        {
            "CashBox" => "خزنة",
            "BankAccount" => "بنك",
            "EWallet" => "محفظة إلكترونية",
            _ => "خزنة"
        };
    }
    
    private string GetPaymentMethodArabic(PaymentMethod method)
    {
        return method switch
        {
            PaymentMethod.Cash => "نقدي 💵",
            PaymentMethod.Cheque => "شيك 📝",
            PaymentMethod.BankTransfer => "تحويل بنكي 🏦",
            PaymentMethod.CreditCard => "بطاقة ائتمان 💳",
            PaymentMethod.InstaPay => "إنستا باي 📲",
            PaymentMethod.Other => "آخر",
            _ => "-"
        };
    }
    
    private void ShowError(string message)
    {
        MessageBox.Show(message, "تنبيه",
            MessageBoxButtons.OK, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button1,
            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
    }
    
    private async void TransactionsGrid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        
        try
        {
            // Get the transaction ID from the selected row
            var transactionId = Convert.ToInt32(_transactionsGrid.Rows[e.RowIndex].Cells["TransactionId"].Value);
            
            // Fetch the full transaction details with all related data
            var transaction = await _cashBoxService.GetTransactionByIdAsync(transactionId);
            
            if (transaction != null)
            {
                // Show the details form - without passing DbContext
                var detailsForm = new TransactionDetailsForm(transaction);
                detailsForm.ShowDialog(this);
            }
            else
            {
                ShowError("لم يتم العثور على المعاملة");
            }
        }
        catch (Exception ex)
        {
            ShowError($"خطأ في عرض تفاصيل المعاملة:\n{ex.Message}");
        }
    }
    
    private async void WhatsAppButton_Click(object? sender, EventArgs e)
    {
        try
        {
            // التحقق من الاتصال بالإنترنت
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                ShowError("⚠️ لا يوجد اتصال بالإنترنت\n\nيرجى التحقق من اتصالك بالإنترنت ثم المحاولة مرة أخرى");
                return;
            }
            
            // التحقق من اختيار الخزنة
            if (_selectedCashBoxId == 0)
            {
                ShowError("برجاء اختيار خزنة أولاً");
                return;
            }
            
            // التحقق من وجود بيانات
            if (_transactionsGrid.Rows.Count == 0)
            {
                ShowError("لا توجد معاملات لعرضها");
                return;
            }
            
            // تحديد نطاق التاريخ بناءً على الفلتر
            string dateRangeText = "";
            
            if (_filterByDayCheckbox.Checked)
            {
                dateRangeText = _dayFilter.Value.ToString("yyyy-MM-dd");
            }
            else
            {
                dateRangeText = $"{GetMonthName(_selectedMonth)}_{_selectedYear}";
            }
            
            // عرض رسالة انتظار
            this.Cursor = Cursors.WaitCursor;
            _whatsappButton.Enabled = false;
            _whatsappButton.Text = "⏳ جاري التقاط الصورة...";
            
            // السماح للـ UI بالتحديث
            System.Windows.Forms.Application.DoEvents();
            
            // التقاط صورة كاملة للصفحة
            string imagePath = await CaptureFullPageScreenshot(dateRangeText);
            
            // استعادة حالة الزرار
            this.Cursor = Cursors.Default;
            _whatsappButton.Enabled = true;
            _whatsappButton.Text = "📱 مشاركة واتساب";
            
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
            {
                ShowError("فشل التقاط الصورة");
                return;
            }
            
            // إنشاء رسالة نصية مختصرة
            var cashBoxName = _cashBoxes.First(c => c.Id == _selectedCashBoxId).Name;
            string message = $"📊 تقرير الخزنة: {cashBoxName}\n📅 الفترة: {dateRangeText}\n\n";
            
            // حساب الإحصائيات
            decimal totalIncome = 0;
            decimal totalExpense = 0;
            
            foreach (DataGridViewRow row in _transactionsGrid.Rows)
            {
                if (row.Cells["Date"].Value?.ToString() != "الإجمالي")
                {
                    string incomeStr = row.Cells["IncomeAmount"].Value?.ToString() ?? "";
                    string expenseStr = row.Cells["ExpenseAmount"].Value?.ToString() ?? "";
                    
                    if (!string.IsNullOrEmpty(incomeStr) && decimal.TryParse(incomeStr, out decimal income))
                    {
                        totalIncome += income;
                    }
                    
                    if (!string.IsNullOrEmpty(expenseStr) && decimal.TryParse(expenseStr, out decimal expense))
                    {
                        totalExpense += expense;
                    }
                }
            }
            
            message += $"💰 إجمالي الإيرادات: {totalIncome:N2} جنيه\n";
            message += $"💸 إجمالي المصروفات: {totalExpense:N2} جنيه\n";
            message += $"📈 الفرق: {(totalIncome - totalExpense):N2} جنيه";
            
            // تشفير الرسالة
            string encodedMessage = Uri.EscapeDataString(message);
            
            // فتح واتساب ويب مع الرسالة
            string whatsappUrl = $"https://web.whatsapp.com/send?text={encodedMessage}";
            
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = whatsappUrl,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
            
            // الانتظار قليلاً ثم فتح المجلد
            await Task.Delay(1000);
            
            // فتح مستكشف الملفات مع تحديد الصورة
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{imagePath}\"");
            
            MessageBox.Show(
                $"✅ تم التقاط الصورة بنجاح!\n\n" +
                $"📁 مسار الصورة:\n{imagePath}\n\n" +
                $"📋 الخطوات:\n" +
                $"1️⃣ تم فتح واتساب ويب - اختر جهة الاتصال\n" +
                $"2️⃣ الصورة محددة في المجلد المفتوح\n" +
                $"3️⃣ اسحب الصورة وأفلتها في محادثة واتساب\n" +
                $"4️⃣ أو استخدم زر الإرفاق (📎) لإرفاق الصورة\n" +
                $"5️⃣ اضغط إرسال ✅",
                "تم بنجاح",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
        catch (Exception ex)
        {
            this.Cursor = Cursors.Default;
            _whatsappButton.Enabled = true;
            _whatsappButton.Text = "📱 مشاركة واتساب";
            ShowError($"خطأ في مشاركة التقرير:\n{ex.Message}");
        }
    }
    
    private Task<string> CaptureFullPageScreenshot(string dateRangeText)
    {
        try
        {
            // إخفاء أزرار الإدارة مؤقتاً للحصول على صورة نظيفة
            var buttonsToHide = new List<Button>();
            if (_isAdmin)
            {
                buttonsToHide.AddRange(new[] {
                    _editCashBoxButton,
                    _deleteCashBoxButton,
                    _editTransactionButton,
                    _deleteTransactionButton
                });
                buttonsToHide.ForEach(btn => btn.Visible = false);
            }
            
            // حفظ حالة الـ scroll الأصلية
            int originalFirstRow = _transactionsGrid.FirstDisplayedScrollingRowIndex >= 0 
                ? _transactionsGrid.FirstDisplayedScrollingRowIndex 
                : 0;
            
            // حساب الأبعاد
            int gridWidth = _transactionsGrid.Width;
            int headerHeight = _transactionsGrid.ColumnHeadersHeight;
            int totalRowsHeight = 0;
            
            // حساب الارتفاع الكلي للصفوف
            foreach (DataGridViewRow row in _transactionsGrid.Rows)
            {
                totalRowsHeight += row.Height;
            }
            
            // الأبعاد الكلية للصورة
            int imageWidth = this.Width - 50; // مع margins
            int summaryHeight = _summaryPanel.Height;
            int filterHeight = 120;
            int buttonsHeight = 70;
            int totalGridHeight = headerHeight + totalRowsHeight + 20;
            int totalHeight = summaryHeight + filterHeight + buttonsHeight + totalGridHeight + 100;
            
            // إنشاء الصورة الكاملة
            using Bitmap fullImage = new Bitmap(imageWidth, totalHeight);
            using Graphics g = Graphics.FromImage(fullImage);
            
            // خلفية بيضاء
            g.Clear(Color.White);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            
            int yPosition = 10;
            
            // 1. رسم الـ Summary Panel
            using (Bitmap summaryBmp = new Bitmap(_summaryPanel.Width, _summaryPanel.Height))
            {
                _summaryPanel.DrawToBitmap(summaryBmp, new Rectangle(0, 0, _summaryPanel.Width, _summaryPanel.Height));
                g.DrawImage(summaryBmp, 20, yPosition);
            }
            yPosition += summaryHeight + 20;
            
            // 2. رسم معلومات الفلتر كنص
            using (Font titleFont = new Font("Cairo", 14F, FontStyle.Bold))
            using (Font infoFont = new Font("Cairo", 11F))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(50, 50, 50)))
            {
                string cashBoxName = _cashBoxes.First(c => c.Id == _selectedCashBoxId).Name;
                g.DrawString($"📊 تقرير: {cashBoxName}", titleFont, brush, imageWidth - 20, yPosition, new StringFormat { Alignment = StringAlignment.Far });
                yPosition += 35;
                g.DrawString($"📅 الفترة: {dateRangeText}", infoFont, brush, imageWidth - 20, yPosition, new StringFormat { Alignment = StringAlignment.Far });
                yPosition += 30;
            }
            
            // خط فاصل
            using (Pen pen = new Pen(Color.FromArgb(200, 200, 200), 2))
            {
                g.DrawLine(pen, 30, yPosition, imageWidth - 30, yPosition);
            }
            yPosition += 20;
            
            // 3. رسم الـ DataGridView بالكامل
            // رسم الـ Header
            using (Font headerFont = new Font("Cairo", 11F, FontStyle.Bold))
            using (SolidBrush headerBrush = new SolidBrush(Color.White))
            using (SolidBrush headerBgBrush = new SolidBrush(Color.FromArgb(41, 128, 185)))
            {
                int xPos = 30;
                int colHeaderHeight = 45;
                
                // خلفية الـ header
                g.FillRectangle(headerBgBrush, xPos, yPosition, gridWidth - 60, colHeaderHeight);
                
                // رسم أسماء الأعمدة (نتخطى أول عمود TransactionId المخفي)
                foreach (DataGridViewColumn col in _transactionsGrid.Columns)
                {
                    if (col.Visible && col.Name != "TransactionId")
                    {
                        Rectangle headerRect = new Rectangle(xPos, yPosition, col.Width, colHeaderHeight);
                        StringFormat sf = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        g.DrawString(col.HeaderText, headerFont, headerBrush, headerRect, sf);
                        xPos += col.Width;
                    }
                }
                yPosition += colHeaderHeight;
            }
            
            // 4. رسم الصفوف
            using (Font cellFont = new Font("Cairo", 10F))
            using (SolidBrush cellBrush = new SolidBrush(Color.FromArgb(50, 50, 50)))
            using (SolidBrush incomeBrush = new SolidBrush(Color.FromArgb(46, 204, 113)))
            using (SolidBrush expenseBrush = new SolidBrush(Color.FromArgb(231, 76, 60)))
            using (SolidBrush altRowBrush = new SolidBrush(Color.FromArgb(245, 245, 245)))
            {
                int rowIndex = 0;
                foreach (DataGridViewRow row in _transactionsGrid.Rows)
                {
                    int rowHeight = row.Height;
                    int xPos = 30;
                    
                    // خلفية الصف (تبديل الألوان)
                    bool isTotal = row.Cells["Date"].Value?.ToString() == "الإجمالي";
                    if (isTotal)
                    {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(52, 73, 94)), xPos, yPosition, gridWidth - 60, rowHeight);
                    }
                    else if (rowIndex % 2 == 1)
                    {
                        g.FillRectangle(altRowBrush, xPos, yPosition, gridWidth - 60, rowHeight);
                    }
                    
                    // رسم محتوى الخلايا
                    foreach (DataGridViewColumn col in _transactionsGrid.Columns)
                    {
                        if (col.Visible && col.Name != "TransactionId")
                        {
                            var cell = row.Cells[col.Index];
                            string cellValue = cell.Value?.ToString() ?? "";
                            
                            Rectangle cellRect = new Rectangle(xPos, yPosition, col.Width, rowHeight);
                            StringFormat sf = new StringFormat
                            {
                                Alignment = StringAlignment.Center,
                                LineAlignment = StringAlignment.Center
                            };
                            
                            // تحديد اللون حسب نوع العمود
                            SolidBrush currentBrush = cellBrush;
                            Font currentFont = cellFont;
                            
                            if (isTotal)
                            {
                                currentBrush = new SolidBrush(Color.White);
                                currentFont = new Font("Cairo", 11F, FontStyle.Bold);
                            }
                            else if (col.Name == "IncomeAmount" && !string.IsNullOrEmpty(cellValue))
                            {
                                currentBrush = incomeBrush;
                                currentFont = new Font("Cairo", 10F, FontStyle.Bold);
                            }
                            else if (col.Name == "ExpenseAmount" && !string.IsNullOrEmpty(cellValue))
                            {
                                currentBrush = expenseBrush;
                                currentFont = new Font("Cairo", 10F, FontStyle.Bold);
                            }
                            
                            g.DrawString(cellValue, currentFont, currentBrush, cellRect, sf);
                            xPos += col.Width;
                        }
                    }
                    
                    // خط فاصل بين الصفوف
                    using (Pen linePen = new Pen(Color.FromArgb(220, 220, 220), 1))
                    {
                        g.DrawLine(linePen, 30, yPosition + rowHeight, gridWidth - 30, yPosition + rowHeight);
                    }
                    
                    yPosition += rowHeight;
                    rowIndex++;
                }
            }
            
            // إعادة إظهار الأزرار
            buttonsToHide.ForEach(btn => btn.Visible = true);
            
            // إعادة الـ scroll للموضع الأصلي
            if (originalFirstRow < _transactionsGrid.Rows.Count)
            {
                _transactionsGrid.FirstDisplayedScrollingRowIndex = originalFirstRow;
            }
            
            // حفظ الصورة
            string fileName = $"CashBox_{dateRangeText.Replace("/", "-")}_{DateTime.Now:HHmmss}.png";
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
            
            fullImage.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            
            return Task.FromResult(filePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في التقاط الصورة:\n{ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            return Task.FromResult(string.Empty);
        }
    }
    
    private string GetMonthName(int month)
    {
        return month switch
        {
            1 => "يناير",
            2 => "فبراير",
            3 => "مارس",
            4 => "أبريل",
            5 => "مايو",
            6 => "يونيو",
            7 => "يوليو",
            8 => "أغسطس",
            9 => "سبتمبر",
            10 => "أكتوبر",
            11 => "نوفمبر",
            12 => "ديسمبر",
            _ => ""
        };
    }
    
    /// <summary>
    /// ✅ عرض العملة بشكل احترافي مع تفاصيل التحويل
    /// </summary>
    private string GetCurrencyDisplay(string currency, decimal? originalAmount, decimal? exchangeRate)
    {
        if (currency == "EGP" || currency == null)
        {
            return "جنيه"; // عملة محلية
        }
        
        // رمز العملة
        string currencySymbol = currency switch
        {
            "USD" => "$",
            "EUR" => "€",
            "GBP" => "£",
            "SAR" => "SAR",
            _ => currency
        };
        
        // إذا كان المبلغ الأصلي موجود
        if (originalAmount.HasValue && originalAmount.Value > 0)
        {
            return $"{currencySymbol} {originalAmount.Value:N2}";
        }
        
        return currencySymbol;
    }
}
