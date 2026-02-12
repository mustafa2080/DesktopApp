using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Application.Services;
using System.Drawing.Drawing2D;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class CashBoxReportForm : Form
{
    private readonly int _cashBoxId;
    private readonly int _month;
    private readonly int _year;
    private readonly string _cashBoxName;
    private readonly ICashBoxService _cashBoxService;
    
    // Controls
    private Panel _headerPanel = null!;
    private Panel _summaryPanel = null!;
    private Panel _categoryPanel = null!;
    private DataGridView _detailsGrid = null!;
    
    private MonthlyReport? _report;
    
    public CashBoxReportForm(int cashBoxId, string cashBoxName, int month, int year, ICashBoxService cashBoxService)
    {
        _cashBoxId = cashBoxId;
        _cashBoxName = cashBoxName;
        _month = month;
        _year = year;
        _cashBoxService = cashBoxService;
        
        InitializeComponent();
        InitializeCustomControls();
        _ = LoadReportDataAsync();
    }
    
    private void InitializeComponent()
    {
        this.Text = "التقرير التفصيلي";
        this.Size = new Size(1400, 950);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
        this.MinimumSize = new Size(1200, 800);
    }
    
    private void InitializeCustomControls()
    {
        // Header Panel
        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 100,
            BackColor = Color.White,
            Padding = new Padding(30)
        };
        
        Label titleLabel = new Label
        {
            Text = $"📊 التقرير التفصيلي - {_cashBoxName}",
            Font = new Font("Cairo", 18F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(30, 20)
        };
        _headerPanel.Controls.Add(titleLabel);
        
        string[] monthNames = { "يناير", "فبراير", "مارس", "إبريل", "مايو", "يونيو",
                               "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" };
        
        Label periodLabel = new Label
        {
            Text = $"الفترة: {monthNames[_month - 1]} {_year}",
            Font = new Font("Cairo", 12F),
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(30, 60)
        };
        _headerPanel.Controls.Add(periodLabel);
        
        this.Controls.Add(_headerPanel);
        
        // Main Container
        TableLayoutPanel mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(20, 10, 20, 20)
        };
        
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 350)); // مساحة كاملة للبطاقات (الكبيرة + العملات)
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 180)); // قسم التصنيفات
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // الجدول - باقي المساحة
        
        // Summary Panel
        CreateSummaryPanel();
        mainLayout.Controls.Add(_summaryPanel, 0, 0);
        
        // Category Panel
        CreateCategoryPanel();
        mainLayout.Controls.Add(_categoryPanel, 0, 1);
        
        // Details Grid
        CreateDetailsGrid();
        Panel gridContainer = new Panel 
        { 
            Dock = DockStyle.Fill, 
            BackColor = Color.White, 
            Padding = new Padding(15)
        };
        
        // عنوان الجدول
        Panel titlePanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.White
        };
        
        Label gridTitle = new Label
        {
            Text = "📋 تفاصيل الحركات",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(10, 0, 10, 0)
        };
        titlePanel.Controls.Add(gridTitle);
        gridContainer.Controls.Add(titlePanel);
        
        // الجدول
        _detailsGrid.Dock = DockStyle.Fill;
        gridContainer.Controls.Add(_detailsGrid);
        
        mainLayout.Controls.Add(gridContainer, 0, 2);
        
        this.Controls.Add(mainLayout);
    }
    
    private void CreateSummaryPanel()
    {
        _summaryPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(0),
            AutoScroll = false
        };
        
        // Main Layout بصفين
        TableLayoutPanel mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 180F)); // البطاقات الكبيرة
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F)); // العملات
        
        // Create TableLayoutPanel for cards
        TableLayoutPanel cardsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            AutoSize = false
        };
        
        cardsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        cardsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        cardsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        cardsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        
        // Card 1: الإيرادات - أخضر
        Panel incomeCard = CreateSummaryCard(
            "إجمالي الإيرادات (EGP)",
            "0.00 جنيه",
            Color.FromArgb(46, 204, 113) // أخضر فاتح واضح
        );
        cardsLayout.Controls.Add(incomeCard, 0, 0);
        
        // Card 2: المصروفات - أحمر
        Panel expenseCard = CreateSummaryCard(
            "إجمالي المصروفات (EGP)",
            "0.00 جنيه",
            Color.FromArgb(231, 76, 60) // أحمر واضح
        );
        cardsLayout.Controls.Add(expenseCard, 1, 0);
        
        // Card 3: الربح/الخسارة - برتقالي
        Panel profitCard = CreateSummaryCard(
            "صافي الربح/الخسارة (EGP)",
            "0.00 جنيه",
            Color.FromArgb(230, 126, 34) // برتقالي أغمق واضح
        );
        cardsLayout.Controls.Add(profitCard, 2, 0);
        
        mainLayout.Controls.Add(cardsLayout, 0, 0);
        
        // ✅ الصف الثاني: بطاقات العملات الأجنبية بتصميم محسّن
        TableLayoutPanel currenciesLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Padding = new Padding(0, 5, 0, 5),
            BackColor = Color.Transparent
        };
        
        currenciesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        currenciesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        currenciesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        currenciesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        
        // بطاقة الدولار
        Panel usdCard = CreateCurrencyMiniCard("دولار", "0.00", "$", Color.FromArgb(76, 175, 80));
        usdCard.Name = "usdCard";
        currenciesLayout.Controls.Add(usdCard, 0, 0);
        
        // بطاقة اليورو
        Panel eurCard = CreateCurrencyMiniCard("يورو", "0.00", "€", Color.FromArgb(33, 150, 243));
        eurCard.Name = "eurCard";
        currenciesLayout.Controls.Add(eurCard, 1, 0);
        
        // بطاقة الجنيه الإسترليني
        Panel gbpCard = CreateCurrencyMiniCard("جنيه £", "0.00", "£", Color.FromArgb(156, 39, 176));
        gbpCard.Name = "gbpCard";
        currenciesLayout.Controls.Add(gbpCard, 2, 0);
        
        // بطاقة الريال
        Panel sarCard = CreateCurrencyMiniCard("ريال", "0.00", "﷼", Color.FromArgb(255, 152, 0));
        sarCard.Name = "sarCard";
        currenciesLayout.Controls.Add(sarCard, 3, 0);
        
        mainLayout.Controls.Add(currenciesLayout, 0, 1);
        
        _summaryPanel.Controls.Add(mainLayout);
    }
    
    /// <summary>
    /// ✅ إنشاء بطاقة احترافية للعملات الأجنبية مع تصميم محسّن
    /// </summary>
    private Panel CreateCurrencyMiniCard(string title, string value, string symbol, Color color)
    {
        Panel card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(10),
            Margin = new Padding(5, 3, 5, 3),
            BorderStyle = BorderStyle.None
        };
        
        // إضافة حدود ملونة احترافية
        card.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (Pen pen = new Pen(color, 2))
            {
                e.Graphics.DrawRectangle(pen, 1, 1, card.Width - 3, card.Height - 3);
            }
        };
        
        // Layout رأسي محسّن
        TableLayoutPanel layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); // الرمز
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F)); // العنوان
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // القيمة
        
        // رمز العملة في الأعلى
        Label symbolLabel = new Label
        {
            Text = symbol,
            Font = new Font("Cairo", 26F, FontStyle.Bold),
            ForeColor = color,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false
        };
        layout.Controls.Add(symbolLabel, 0, 0);
        
        // العنوان في المنتصف
        Label titleLabel = new Label
        {
            Text = title,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 100, 100),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopCenter,
            AutoSize = false
        };
        layout.Controls.Add(titleLabel, 0, 1);
        
        // القيمة في الأسفل
        Label valueLabel = new Label
        {
            Text = value,
            Name = "valueLabel",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = color,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopCenter,
            AutoSize = false
        };
        layout.Controls.Add(valueLabel, 0, 2);
        
        card.Controls.Add(layout);
        
        return card;
    }
    
    private Panel CreateSummaryCard(string title, string value, Color color)
    {
        Panel card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = color,
            Padding = new Padding(15),
            Margin = new Padding(8, 5, 8, 5)
        };
        
        // إضافة حدود مستديرة وظل خفيف
        card.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var path = new GraphicsPath())
            {
                int radius = 12;
                var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();
                
                card.Region = new Region(path);
            }
        };
        
        // Layout داخلي
        TableLayoutPanel innerLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        
        innerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // العنوان
        innerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // القيمة
        
        Label titleLabel = new Label
        {
            Text = title,
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        innerLayout.Controls.Add(titleLabel, 0, 0);
        
        Label valueLabel = new Label
        {
            Name = "valueLabel",
            Text = value,
            Font = new Font("Cairo", 24F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        innerLayout.Controls.Add(valueLabel, 0, 1);
        
        card.Controls.Add(innerLayout);
        
        return card;
    }
    
    private void CreateCategoryPanel()
    {
        _categoryPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(20),
            AutoScroll = true
        };
        
        Label titleLabel = new Label
        {
            Text = "📊 التوزيع حسب التصنيف",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(20, 10)
        };
        _categoryPanel.Controls.Add(titleLabel);
        
        // Will be populated with data
    }
    
    private void CreateDetailsGrid()
    {
        _detailsGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            Font = new Font("Cairo", 11F),
            RightToLeft = RightToLeft.Yes,
            EnableHeadersVisualStyles = false,
            MultiSelect = false,
            AllowUserToResizeRows = false
        };
        
        // Configure columns
        _detailsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Date",
            HeaderText = "التاريخ",
            Width = 120,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
        });
        
        _detailsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "VoucherNumber",
            HeaderText = "رقم السند",
            Width = 120,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
        });
        
        _detailsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Type",
            HeaderText = "النوع",
            Width = 90,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Alignment = DataGridViewContentAlignment.MiddleCenter, 
                Font = new Font("Cairo", 11F, FontStyle.Bold) 
            }
        });
        
        _detailsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Category",
            HeaderText = "التصنيف",
            Width = 140,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
        });
        
        _detailsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Description",
            HeaderText = "الوصف",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
        });
        
        _detailsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "PaymentMethod",
            HeaderText = "طريقة الدفع",
            Width = 130,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
        });
        
        _detailsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Commission",
            HeaderText = "العمولة",
            Width = 100,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Format = "N2",
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(200, 50, 50),
                Font = new Font("Cairo", 10F, FontStyle.Bold)
            }
        });
        
        // ✅ عمود العملة
        _detailsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Currency",
            HeaderText = "العملة",
            Width = 90,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Cairo", 10F, FontStyle.Bold)
            }
        });
        
        _detailsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Amount",
            HeaderText = "المبلغ",
            Width = 140,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Format = "N2",
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Cairo", 11F, FontStyle.Bold)
            }
        });
        
        _detailsGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleCenter,
            Padding = new Padding(8)
        };
        
        _detailsGrid.DefaultCellStyle = new DataGridViewCellStyle
        {
            Font = new Font("Cairo", 11F),
            Padding = new Padding(8, 5, 8, 5),
            SelectionBackColor = ColorScheme.Primary,
            SelectionForeColor = Color.White,
            ForeColor = Color.FromArgb(33, 33, 33),
            WrapMode = DataGridViewTriState.False
        };
        
        _detailsGrid.ColumnHeadersHeight = 50;
        _detailsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _detailsGrid.EnableHeadersVisualStyles = false;
        _detailsGrid.RowTemplate.Height = 45;
        _detailsGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        
        // إضافة خط فاصل بين الصفوف
        _detailsGrid.CellPainting += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && e.Graphics != null)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                
                using (Pen pen = new Pen(Color.FromArgb(230, 230, 230), 1))
                {
                    e.Graphics.DrawLine(pen, 
                        e.CellBounds.Left, 
                        e.CellBounds.Bottom - 1,
                        e.CellBounds.Right, 
                        e.CellBounds.Bottom - 1);
                }
                
                e.Handled = true;
            }
        };
    }
    
    private async Task LoadReportDataAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            
            System.Diagnostics.Debug.WriteLine($"=== LoadReportDataAsync ===");
            System.Diagnostics.Debug.WriteLine($"CashBoxId: {_cashBoxId}, Month: {_month}, Year: {_year}");
            
            // Load report and transactions in one go to avoid multiple queries
            _report = await _cashBoxService.GetMonthlyReportAsync(_cashBoxId, _month, _year);
            
            if (_report == null)
            {
                System.Diagnostics.Debug.WriteLine("❌ Report is NULL!");
                MessageBox.Show("لا توجد بيانات متاحة للشهر المحدد", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                return;
            }
            
            System.Diagnostics.Debug.WriteLine($"✅ Report loaded:");
            System.Diagnostics.Debug.WriteLine($"   TotalIncome: {_report.TotalIncome:N2}");
            System.Diagnostics.Debug.WriteLine($"   TotalExpense: {_report.TotalExpense:N2}");
            System.Diagnostics.Debug.WriteLine($"   NetProfit: {_report.NetProfit:N2}");
            System.Diagnostics.Debug.WriteLine($"   Transactions count: {_report.Transactions?.Count ?? 0}");
            
            // Update summary cards
            UpdateSummaryCards();
            
            // Load category breakdown
            LoadCategoryBreakdown();
            
            // Load transactions - use separate query with AsNoTracking
            await LoadTransactionsAsync();
        }
        catch (OperationCanceledException)
        {
            MessageBox.Show("انتهت مهلة تحميل التقرير. يرجى المحاولة مرة أخرى.", "انتهاء المهلة",
                MessageBoxButtons.OK, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
        catch (Exception ex)
        {
            var errorMessage = $"خطأ في تحميل التقرير:\n{ex.Message}";
            
            if (ex.InnerException != null)
            {
                errorMessage += $"\n\nالسبب:\n{ex.InnerException.Message}";
            }
            
            MessageBox.Show(errorMessage, "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                
            // Log للتشخيص
            System.Diagnostics.Debug.WriteLine($"تفاصيل الخطأ: {ex}");
        }
    }
    
    private void UpdateSummaryCards()
    {
        if (_report == null) return;
        
        // Get the main TableLayoutPanel
        var mainLayout = _summaryPanel.Controls[0] as TableLayoutPanel;
        if (mainLayout == null) return;
        
        // Get the cards layout (first row)
        var cardsLayout = mainLayout.GetControlFromPosition(0, 0) as TableLayoutPanel;
        if (cardsLayout == null) return;
        
        // ✅ استخدام القيم الجاهزة من التقرير بدل إعادة الحساب
        decimal egpIncome = _report.TotalIncome;
        decimal egpExpense = _report.TotalExpense;
        decimal egpProfit = _report.NetProfit;
        
        System.Diagnostics.Debug.WriteLine($"=== UpdateSummaryCards ===");
        System.Diagnostics.Debug.WriteLine($"Income: {egpIncome:N2}");
        System.Diagnostics.Debug.WriteLine($"Expense: {egpExpense:N2}");
        System.Diagnostics.Debug.WriteLine($"Profit: {egpProfit:N2}");
        
        // Update income card (column 0)
        var incomeCard = cardsLayout.GetControlFromPosition(0, 0) as Panel;
        if (incomeCard != null)
        {
            var valueLabel = incomeCard.Controls.Find("valueLabel", true).FirstOrDefault() as Label;
            if (valueLabel != null)
            {
                valueLabel.Text = $"{egpIncome:N2} جنيه";
            }
        }
        
        // Update expense card (column 1)
        var expenseCard = cardsLayout.GetControlFromPosition(1, 0) as Panel;
        if (expenseCard != null)
        {
            var valueLabel = expenseCard.Controls.Find("valueLabel", true).FirstOrDefault() as Label;
            if (valueLabel != null)
            {
                valueLabel.Text = $"{egpExpense:N2} جنيه";
            }
        }
        
        // Update profit card (column 2)
        var profitCard = cardsLayout.GetControlFromPosition(2, 0) as Panel;
        if (profitCard != null)
        {
            var valueLabel = profitCard.Controls.Find("valueLabel", true).FirstOrDefault() as Label;
            if (valueLabel != null)
            {
                valueLabel.Text = $"{(egpProfit >= 0 ? "+" : "")}{egpProfit:N2} جنيه";
                
                // Change card color based on profit/loss
                if (egpProfit > 0)
                    profitCard.BackColor = Color.FromArgb(46, 204, 113); // أخضر للربح
                else if (egpProfit < 0)
                    profitCard.BackColor = Color.FromArgb(231, 76, 60); // أحمر للخسارة
                else
                    profitCard.BackColor = Color.FromArgb(230, 126, 34); // برتقالي للتعادل
            }
        }
        
        // ✅ حساب أرصدة العملات الأجنبية
        UpdateCurrencyCards(mainLayout);
    }
    
    /// <summary>
    /// ✅ تحديث بطاقات العملات الأجنبية
    /// </summary>
    private void UpdateCurrencyCards(TableLayoutPanel mainLayout)
    {
        if (_report == null || _report.Transactions == null) return;
        
        // Get currencies layout (second row)
        var currenciesLayout = mainLayout.GetControlFromPosition(0, 1) as TableLayoutPanel;
        if (currenciesLayout == null) return;
        
        // حساب الرصيد لكل عملة
        var usdBalance = _report.Transactions
            .Where(t => t.TransactionCurrency == "USD")
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
        
        var eurBalance = _report.Transactions
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
        
        var gbpBalance = _report.Transactions
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
        
        var sarBalance = _report.Transactions
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
        
        // تحديث البطاقات
        UpdateCurrencyCard(currenciesLayout.Controls.Find("usdCard", false).FirstOrDefault() as Panel, usdBalance);
        UpdateCurrencyCard(currenciesLayout.Controls.Find("eurCard", false).FirstOrDefault() as Panel, eurBalance);
        UpdateCurrencyCard(currenciesLayout.Controls.Find("gbpCard", false).FirstOrDefault() as Panel, gbpBalance);
        UpdateCurrencyCard(currenciesLayout.Controls.Find("sarCard", false).FirstOrDefault() as Panel, sarBalance);
    }
    
    /// <summary>
    /// ✅ تحديث قيمة بطاقة عملة واحدة
    /// </summary>
    private void UpdateCurrencyCard(Panel? card, decimal balance)
    {
        if (card == null) return;
        
        var valueLabel = card.Controls.Find("valueLabel", true).FirstOrDefault() as Label;
        if (valueLabel != null)
        {
            valueLabel.Text = $"{balance:N2}";
        }
    }
    
    private void LoadCategoryBreakdown()
    {
        if (_report == null) return;
        
        int yPos = 50;
        int leftX = 20;
        int rightX = 680;
        
        // Income categories - Left side
        Label incomeTitle = new Label
        {
            Text = "✅ تصنيفات الإيرادات",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = ColorScheme.Success,
            AutoSize = true,
            Location = new Point(leftX, yPos)
        };
        _categoryPanel.Controls.Add(incomeTitle);
        yPos += 35;
        
        foreach (var category in _report.IncomeByCategory)
        {
            AddCategoryBar(category, ColorScheme.Success, leftX, yPos, 600);
            yPos += 70; // زيادة المسافة من 40 إلى 70
        }
        
        // Expense categories - Right side
        yPos = 50;
        Label expenseTitle = new Label
        {
            Text = "❌ تصنيفات المصروفات",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = ColorScheme.Error,
            AutoSize = true,
            Location = new Point(rightX, yPos)
        };
        _categoryPanel.Controls.Add(expenseTitle);
        yPos += 35;
        
        foreach (var category in _report.ExpenseByCategory)
        {
            AddCategoryBar(category, ColorScheme.Error, rightX, yPos, 600);
            yPos += 70; // زيادة المسافة من 40 إلى 70
        }
    }
    
    private void AddCategoryBar(CategorySummary category, Color color, int x, int y, int maxWidth)
    {
        Panel container = new Panel
        {
            Size = new Size(maxWidth, 60), // زيادة الارتفاع من 35 إلى 60
            Location = new Point(x, y),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        
        // Progress bar background
        Panel progressBg = new Panel
        {
            Location = new Point(5, 35),
            Size = new Size(maxWidth - 10, 20),
            BackColor = Color.FromArgb(240, 240, 240)
        };
        container.Controls.Add(progressBg);
        
        // Progress bar fill
        int fillWidth = (int)((maxWidth - 10) * (category.Percentage / 100m));
        Panel progressFill = new Panel
        {
            Location = new Point(5, 35),
            Size = new Size(fillWidth, 20),
            BackColor = color
        };
        container.Controls.Add(progressFill);
        
        Label nameLabel = new Label
        {
            Text = category.Category,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            AutoSize = false,
            Size = new Size(maxWidth - 200, 25),
            Location = new Point(5, 5),
            ForeColor = Color.FromArgb(33, 33, 33),
            TextAlign = ContentAlignment.MiddleRight
        };
        container.Controls.Add(nameLabel);
        
        Label amountLabel = new Label
        {
            Text = category.Amount > 0 ? $"{category.Amount:N2} جنيه" : "-",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = color,
            AutoSize = false,
            Size = new Size(180, 25),
            Location = new Point(maxWidth - 190, 5),
            TextAlign = ContentAlignment.MiddleLeft
        };
        container.Controls.Add(amountLabel);
        
        Label percentLabel = new Label
        {
            Text = $"{category.Percentage:F1}%",
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(10, 38),
            BackColor = Color.Transparent
        };
        progressFill.Controls.Add(percentLabel);
        
        _categoryPanel.Controls.Add(container);
    }
    
    private Task LoadTransactionsAsync()
    {
        try
        {
            _detailsGrid.Rows.Clear();
            
            // استخدام الحركات من التقرير بدلاً من عمل query جديد
            if (_report == null || _report.Transactions == null)
                return Task.CompletedTask;
            
            var transactions = _report.Transactions;
            
            foreach (var trans in transactions.OrderBy(t => t.TransactionDate))
            {
                string type = trans.Type == TransactionType.Income ? "إيراد" : "مصروف";
                
                // تحويل طريقة الدفع للعربي
                string paymentMethod = GetPaymentMethodArabic(trans.PaymentMethod);
                
                // عرض العمولة (إن وجدت)
                string commission = trans.InstaPayCommission.HasValue && trans.InstaPayCommission.Value > 0
                    ? trans.InstaPayCommission.Value.ToString("N2")
                    : "-";
                
                // ✅ حساب المبلغ المعروض (بعد خصم العمولة للمصروفات)
                decimal displayAmount = trans.Amount;
                if (trans.Type == TransactionType.Expense && 
                    trans.PaymentMethod == PaymentMethod.InstaPay && 
                    trans.InstaPayCommission.HasValue)
                {
                    displayAmount = trans.Amount - trans.InstaPayCommission.Value;
                }
                
                // ✅ عرض العملة
                string currencyDisplay = GetCurrencyDisplay(trans.TransactionCurrency);
                
                int rowIndex = _detailsGrid.Rows.Add(
                    trans.TransactionDate.ToString("yyyy-MM-dd"),
                    trans.VoucherNumber,
                    type,
                    trans.Category,
                    trans.Description,
                    paymentMethod,
                    commission,
                    currencyDisplay, // ✅ العملة
                    displayAmount
                );
                
                var row = _detailsGrid.Rows[rowIndex];
                
                // Color code the Type column
                if (trans.Type == TransactionType.Income)
                {
                    row.Cells["Type"].Style.BackColor = Color.FromArgb(220, 255, 220);
                    row.Cells["Type"].Style.ForeColor = Color.FromArgb(0, 128, 0);
                }
                else
                {
                    row.Cells["Type"].Style.BackColor = Color.FromArgb(255, 220, 220);
                    row.Cells["Type"].Style.ForeColor = Color.FromArgb(200, 0, 0);
                }
                
                // ✅ تلوين خلية العملة حسب نوع العملة
                if (!string.IsNullOrEmpty(trans.TransactionCurrency) && trans.TransactionCurrency != "EGP")
                {
                    row.Cells["Currency"].Style.ForeColor = trans.TransactionCurrency switch
                    {
                        "USD" => Color.FromArgb(76, 175, 80),      // أخضر
                        "EUR" => Color.FromArgb(33, 150, 243),     // أزرق
                        "GBP" => Color.FromArgb(156, 39, 176),     // بنفسجي
                        "SAR" => Color.FromArgb(255, 152, 0),      // برتقالي
                        _ => Color.FromArgb(0, 102, 204)
                    };
                    row.Cells["Currency"].Style.Font = new Font("Cairo", 10F, FontStyle.Bold);
                }
            }
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل الحركات:\n{ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            return Task.CompletedTask;
        }
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
    
    /// <summary>
    /// ✅ عرض العملة بشكل واضح
    /// </summary>
    private string GetCurrencyDisplay(string? currency)
    {
        return currency switch
        {
            "EGP" => "جنيه 💵",
            "USD" => "دولار $",
            "EUR" => "يورو €",
            "GBP" => "جنيه £",
            "SAR" => "ريال ﷼",
            null => "جنيه 💵",
            _ => currency
        };
    }
}
