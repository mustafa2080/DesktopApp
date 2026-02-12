using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using GraceWay.AccountingSystem.Infrastructure.Data;
using GraceWay.AccountingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

namespace GraceWay.AccountingSystem.Presentation.Forms
{
    public partial class CashBoxInventoryReportForm : Form
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly int _currentUserId;

        // Controls
        private ComboBox cmbCashBox = null!;
        private DateTimePicker dtpDate = null!;
        private Button btnRefresh = null!;
        private Button btnExport = null!;
        private Button btnPrint = null!;

        // Main Panels
        private Panel pnlHeader = null!;
        private Panel pnlMainContent = null!;

        // Summary Section
        private Panel pnlCashBoxInfo = null!;
        private Panel pnlQuickStats = null!;
        
        // Data Grids
        private DataGridView dgvTransactions = null!;
        private DataGridView dgvDailySummary = null!;
        private DataGridView dgvMonthlySummary = null!;

        // Data
        private CashBox? _selectedCashBox;
        private DateTime _selectedDate;
        private List<CashTransaction> _allTransactions = new();
        private bool _isInitializing = true;  // ✅ flag لمنع التحديث أثناء البناء

        public CashBoxInventoryReportForm(IDbContextFactory<AppDbContext> contextFactory, int currentUserId)
        {
            _contextFactory = contextFactory;
            _currentUserId = currentUserId;
            _selectedDate = DateTime.UtcNow.Date;

            InitializeComponent();
            BuildUI();
            _isInitializing = false;  // ✅ انتهى البناء
            _ = LoadDataAsync();
        }

        private void BuildUI()
        {
            this.SuspendLayout();

            // Main wrapper
            var wrapper = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(245, 247, 250)
            };
            this.Controls.Add(wrapper);

            // Header Section
            pnlHeader = BuildHeaderSection();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 140;
            wrapper.Controls.Add(pnlHeader);

            // Main Content Area
            pnlMainContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 0, 0)
            };
            wrapper.Controls.Add(pnlMainContent);

            // Summary Cards Row
            var summaryRow = BuildSummaryCards();
            summaryRow.Dock = DockStyle.Top;
            summaryRow.Height = 200;
            pnlMainContent.Controls.Add(summaryRow);

            // Tabs Section
            var tabSection = BuildTabsSection();
            tabSection.Dock = DockStyle.Fill;
            pnlMainContent.Controls.Add(tabSection);

            this.ResumeLayout(true);
        }

        // ═══════════════════════════════════════════════════════════════
        // HEADER SECTION
        // ═══════════════════════════════════════════════════════════════
        private Panel BuildHeaderSection()
        {
            var header = new Panel
            {
                BackColor = Color.White,
                Padding = new Padding(25, 20, 25, 20)
            };
            header.Paint += PaintCard;

            // Title
            var lblTitle = new Label
            {
                Text = "📊 جرد الخزنة - تقرير شامل",
                Font = new Font("Cairo", 18F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                Dock = DockStyle.Top,
                Height = 45,
                TextAlign = ContentAlignment.MiddleRight
            };
            header.Controls.Add(lblTitle);

            // Controls Panel
            var controlsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.Transparent
            };
            header.Controls.Add(controlsPanel);

            var flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoSize = false,
                Padding = new Padding(0, 8, 0, 0)
            };
            controlsPanel.Controls.Add(flowPanel);

            // الخزنة
            var lblCashBox = new Label
            {
                Text = "الخزنة:",
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                ForeColor = ColorScheme.TextPrimary,
                Width = 70,
                Height = 36,
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(0, 0, 10, 0)
            };

            cmbCashBox = new ComboBox
            {
                Width = 250,
                Height = 36,
                Font = new Font("Cairo", 10.5F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 0, 20, 0),
                FlatStyle = FlatStyle.Flat
            };
            cmbCashBox.SelectedIndexChanged += async (s, e) =>
            {
                if (!_isInitializing)  // ✅ تجاهل أثناء البناء
                    await LoadDataAsync();
            };

            // التاريخ
            var lblDate = new Label
            {
                Text = "التاريخ:",
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                ForeColor = ColorScheme.TextPrimary,
                Width = 70,
                Height = 36,
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(0, 0, 10, 0)
            };

            dtpDate = new DateTimePicker
            {
                Width = 200,
                Height = 36,
                Font = new Font("Cairo", 10.5F),
                Format = DateTimePickerFormat.Short,
                Margin = new Padding(0, 0, 30, 0),
                Value = _selectedDate  // ✅ تعيين التاريخ الافتراضي
            };
            dtpDate.ValueChanged += async (s, e) =>
            {
                if (_isInitializing) return;  // ✅ تجاهل أثناء البناء
                
                _selectedDate = DateTime.SpecifyKind(dtpDate.Value.Date, DateTimeKind.Utc);
                System.Diagnostics.Debug.WriteLine($"🗓️ Date changed to: {_selectedDate:dd/MM/yyyy}");
                await LoadDataAsync();
            };

            // Buttons
            btnRefresh = CreateActionButton("🔄 تحديث", ColorScheme.Primary);
            btnRefresh.Click += async (s, e) => await LoadDataAsync();

            btnExport = CreateActionButton("📤 تصدير Excel", ColorScheme.Success);
            btnExport.Click += BtnExport_Click;

            btnPrint = CreateActionButton("🖨️ طباعة", ColorScheme.Info);
            btnPrint.Click += BtnPrint_Click;

            flowPanel.Controls.AddRange(new Control[]
            {
                lblCashBox, cmbCashBox,
                lblDate, dtpDate,
                btnRefresh, btnExport, btnPrint
            });

            return header;
        }

        // ═══════════════════════════════════════════════════════════════
        // SUMMARY CARDS
        // ═══════════════════════════════════════════════════════════════
        private Panel BuildSummaryCards()
        {
            var container = new Panel
            {
                BackColor = Color.Transparent,
                Padding = new Padding(0, 0, 0, 15)
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));

            // الجزء الأيمن - بطاقة معلومات الخزنة
            pnlCashBoxInfo = BuildCashBoxInfoCard();
            pnlCashBoxInfo.Margin = new Padding(0, 0, 10, 0);
            table.Controls.Add(pnlCashBoxInfo, 0, 0);

            // الجزء الأيسر - الإحصائيات السريعة
            pnlQuickStats = BuildQuickStatsPanel();
            pnlQuickStats.Margin = new Padding(10, 0, 0, 0);
            table.Controls.Add(pnlQuickStats, 1, 0);

            container.Controls.Add(table);
            return container;
        }

        private Panel BuildCashBoxInfoCard()
        {
            var card = new Panel
            {
                BackColor = Color.White,
                Padding = new Padding(25)
            };
            card.Paint += PaintCard;
            card.Paint += (s, e) =>
            {
                // شريط علوي بلون مميز
                using var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, card.Width, 6),
                    ColorScheme.Primary,
                    ColorScheme.PrimaryDark,
                    LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(brush, 0, 0, card.Width, 6);
            };

            // العنوان مع أيقونة
            var lblTitle = new Label
            {
                Text = "💰 رصيد الخزنة الحالي",
                Font = new Font("Cairo", 12F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                Dock = DockStyle.Top,
                Height = 35,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 6, 0, 0)
            };
            card.Controls.Add(lblTitle);

            // قيمة الرصيد - كبيرة ومميزة
            var lblBalance = new Label
            {
                Name = "lblBalance",
                Text = "0.00 جنيه",
                Font = new Font("Cairo", 28F, FontStyle.Bold),
                ForeColor = ColorScheme.Success,
                Dock = DockStyle.Top,
                Height = 70,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 10, 0, 0)
            };
            card.Controls.Add(lblBalance);

            // خط فاصل
            var divider = new Panel
            {
                Dock = DockStyle.Top,
                Height = 2,
                BackColor = Color.FromArgb(230, 235, 240),
                Margin = new Padding(0, 10, 0, 10)
            };
            card.Controls.Add(divider);

            // معلومات إضافية
            var infoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.Transparent
            };
            card.Controls.Add(infoPanel);

            var lblTransactions = new Label
            {
                Name = "lblTransactions",
                Text = "📝 عدد الحركات: 0 حركة",
                Font = new Font("Cairo", 10.5F),
                ForeColor = ColorScheme.TextSecondary,
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign = ContentAlignment.TopRight
            };
            infoPanel.Controls.Add(lblTransactions);

            var lblLastTransaction = new Label
            {
                Name = "lblLastTransaction",
                Text = "🕒 آخر حركة: --",
                Font = new Font("Cairo", 10.5F),
                ForeColor = ColorScheme.TextSecondary,
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign = ContentAlignment.TopRight
            };
            infoPanel.Controls.Add(lblLastTransaction);

            return card;
        }

        private Panel BuildQuickStatsPanel()
        {
            var panel = new Panel
            {
                BackColor = Color.Transparent
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));

            // بطاقة اليومي
            var dailyCard = BuildStatCard("اليوم", "lblDailyIncome", "lblDailyExpense", "lblDailyNet", ColorScheme.Primary);
            dailyCard.Margin = new Padding(0, 0, 8, 0);
            table.Controls.Add(dailyCard, 0, 0);

            // بطاقة الشهري
            var monthlyCard = BuildStatCard("هذا الشهر", "lblMonthlyIncome", "lblMonthlyExpense", "lblMonthlyNet", ColorScheme.Info);
            monthlyCard.Margin = new Padding(4, 0, 4, 0);
            table.Controls.Add(monthlyCard, 1, 0);

            // بطاقة السنوي
            var yearlyCard = BuildStatCard("هذا العام", "lblYearlyIncome", "lblYearlyExpense", "lblYearlyNet", ColorScheme.Warning);
            yearlyCard.Margin = new Padding(8, 0, 0, 0);
            table.Controls.Add(yearlyCard, 2, 0);

            panel.Controls.Add(table);
            return panel;
        }

        private Panel BuildStatCard(string title, string incomeLabel, string expenseLabel, string netLabel, Color accentColor)
        {
            var card = new Panel
            {
                BackColor = Color.White
            };
            card.Paint += PaintCard;
            card.Paint += (s, e) =>
            {
                using var brush = new SolidBrush(accentColor);
                e.Graphics.FillRectangle(brush, 0, 0, card.Width, 4);
            };

            card.Padding = new Padding(15, 12, 15, 10);

            // العنوان
            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                ForeColor = accentColor,
                Dock = DockStyle.Top,
                Height = 26,
                TextAlign = ContentAlignment.TopRight
            };
            card.Controls.Add(lblTitle);

            // الإيرادات
            var pnlIncome = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.Transparent };
            var lblIncTitle = new Label
            {
                Text = "↗️ الإيرادات",
                Font = new Font("Cairo", 8.5F),
                ForeColor = ColorScheme.TextSecondary,
                Dock = DockStyle.Top,
                Height = 18,
                TextAlign = ContentAlignment.TopRight
            };
            var lblIncome = new Label
            {
                Name = incomeLabel,
                Text = "0.00",
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                ForeColor = ColorScheme.Success,
                Dock = DockStyle.Top,
                Height = 24,
                TextAlign = ContentAlignment.TopRight
            };
            pnlIncome.Controls.Add(lblIncTitle);
            pnlIncome.Controls.Add(lblIncome);
            card.Controls.Add(pnlIncome);

            // المصروفات
            var pnlExpense = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.Transparent };
            var lblExpTitle = new Label
            {
                Text = "↘️ المصروفات",
                Font = new Font("Cairo", 8.5F),
                ForeColor = ColorScheme.TextSecondary,
                Dock = DockStyle.Top,
                Height = 18,
                TextAlign = ContentAlignment.TopRight
            };
            var lblExpense = new Label
            {
                Name = expenseLabel,
                Text = "0.00",
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                ForeColor = ColorScheme.Error,
                Dock = DockStyle.Top,
                Height = 24,
                TextAlign = ContentAlignment.TopRight
            };
            pnlExpense.Controls.Add(lblExpTitle);
            pnlExpense.Controls.Add(lblExpense);
            card.Controls.Add(pnlExpense);

            // خط فاصل
            var divider = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = Color.FromArgb(230, 235, 240),
                Margin = new Padding(0, 6, 0, 6)
            };
            card.Controls.Add(divider);

            // الصافي
            var pnlNet = new Panel { Dock = DockStyle.Top, Height = 38, BackColor = Color.Transparent };
            var lblNetTitle = new Label
            {
                Text = "💵 الصافي",
                Font = new Font("Cairo", 8.5F),
                ForeColor = ColorScheme.TextSecondary,
                Dock = DockStyle.Top,
                Height = 18,
                TextAlign = ContentAlignment.TopRight
            };
            var lblNet = new Label
            {
                Name = netLabel,
                Text = "0.00",
                Font = new Font("Cairo", 12.5F, FontStyle.Bold),
                ForeColor = accentColor,
                Dock = DockStyle.Top,
                Height = 26,
                TextAlign = ContentAlignment.TopRight
            };
            pnlNet.Controls.Add(lblNetTitle);
            pnlNet.Controls.Add(lblNet);
            card.Controls.Add(pnlNet);

            return card;
        }

        // ═══════════════════════════════════════════════════════════════
        // TABS SECTION
        // ═══════════════════════════════════════════════════════════════
        private Panel BuildTabsSection()
        {
            var section = new Panel
            {
                BackColor = Color.White,
                Padding = new Padding(20, 25, 20, 25)  // ✅ زيادة المسافة من الأعلى والأسفل
            };
            section.Paint += PaintCard;

            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Cairo", 10.5F, FontStyle.Bold),
                Padding = new Point(12, 8),  // ✅ زيادة المسافة داخل التاب
                ItemSize = new Size(180, 40)
            };

            // تاب الحركات التفصيلية
            dgvTransactions = CreateStyledDataGrid();
            var tabTransactions = new TabPage("📋 الحركات التفصيلية");
            tabTransactions.Padding = new Padding(10, 15, 10, 15);  // ✅ مسافة داخل التاب
            tabTransactions.Controls.Add(dgvTransactions);
            tabControl.TabPages.Add(tabTransactions);

            // تاب الملخص اليومي
            dgvDailySummary = CreateStyledDataGrid();
            var tabDaily = new TabPage("📊 الملخص اليومي");
            tabDaily.Padding = new Padding(10, 15, 10, 15);  // ✅ مسافة داخل التاب
            tabDaily.Controls.Add(dgvDailySummary);
            tabControl.TabPages.Add(tabDaily);

            // تاب الملخص الشهري
            dgvMonthlySummary = CreateStyledDataGrid();
            var tabMonthly = new TabPage("📈 الملخص الشهري");
            tabMonthly.Padding = new Padding(10, 15, 10, 15);  // ✅ مسافة داخل التاب
            tabMonthly.Controls.Add(dgvMonthlySummary);
            tabControl.TabPages.Add(tabMonthly);

            section.Controls.Add(tabControl);
            return section;
        }

        private DataGridView CreateStyledDataGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 45,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(235, 237, 240),
                RowTemplate = { Height = 38 },
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Cairo", 10F),
                RightToLeft = RightToLeft.Yes,
                Margin = new Padding(0, 15, 0, 15)  // ✅ إضافة مسافة من الأعلى والأسفل
            };

            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = ColorScheme.Primary,
                ForeColor = Color.White,
                Font = new Font("Cairo", 10.5F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(5)
            };

            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = ColorScheme.TextPrimary,
                SelectionBackColor = Color.FromArgb(230, 240, 255),
                SelectionForeColor = ColorScheme.PrimaryDark,
                Padding = new Padding(8),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(248, 250, 252)
            };

            return grid;
        }

        // ═══════════════════════════════════════════════════════════════
        // PAINT HELPERS
        // ═══════════════════════════════════════════════════════════════
        private void PaintCard(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel p) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Shadow
            using var shadowBrush = new SolidBrush(Color.FromArgb(15, 0, 0, 0));
            e.Graphics.FillRectangle(shadowBrush, 2, 3, p.Width - 2, p.Height);

            // Background
            var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
            using var path = GetRoundedPath(rect, 10);
            using var brush = new SolidBrush(p.BackColor);
            e.Graphics.FillPath(brush, path);

            // Border
            using var pen = new Pen(Color.FromArgb(220, 225, 230), 1);
            e.Graphics.DrawPath(pen, path);
        }

        private static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            var arc = new Rectangle(rect.X, rect.Y, radius * 2, radius * 2);

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - radius * 2;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - radius * 2;
            path.AddArc(arc, 0, 90);
            arc.X = rect.X;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();

            return path;
        }

        // ═══════════════════════════════════════════════════════════════
        // DATA LOADING
        // ═══════════════════════════════════════════════════════════════
        private async Task LoadDataAsync()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                // Load CashBoxes - ✅ تحديث: إعادة تحميل الخزن في كل مرة
                using var ctx1 = _contextFactory.CreateDbContext();
                var cashBoxes = await ctx1.CashBoxes
                    .Where(c => !c.IsDeleted && c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                // ✅ إذا كانت القائمة فارغة، املأها لأول مرة
                if (cmbCashBox.Items.Count == 0)
                {
                    cmbCashBox.DisplayMember = "Name";
                    cmbCashBox.ValueMember = "Id";
                    
                    foreach (var cb in cashBoxes)
                        cmbCashBox.Items.Add(cb);

                    if (cmbCashBox.Items.Count > 0)
                        cmbCashBox.SelectedIndex = 0;
                }

                // ✅ الحصول على الخزنة المحددة وإعادة تحميلها من قاعدة البيانات
                var selectedCashBoxId = (_selectedCashBox as CashBox)?.Id ?? 
                                       (cmbCashBox.SelectedItem as CashBox)?.Id;
                
                if (!selectedCashBoxId.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ No CashBox selected!");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"📦 Loading data for CashBox ID: {selectedCashBoxId.Value}");

                // ✅ إعادة تحميل الخزنة من قاعدة البيانات للحصول على الرصيد المحدث
                using var ctx2 = _contextFactory.CreateDbContext();
                _selectedCashBox = await ctx2.CashBoxes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == selectedCashBoxId.Value && !c.IsDeleted);

                if (_selectedCashBox == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ _selectedCashBox not found in database!");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"✅ CashBox loaded: {_selectedCashBox.Name}, Balance: {_selectedCashBox.CurrentBalance:N2}");

                // ✅ تحميل جميع الحركات الخاصة بهذه الخزنة
                using var ctx3 = _contextFactory.CreateDbContext();
                _allTransactions = await ctx3.CashTransactions
                    .AsNoTracking()
                    .Where(t => t.CashBoxId == _selectedCashBox.Id && !t.IsDeleted)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"✅ Loaded {_allTransactions.Count} transactions");
                System.Diagnostics.Debug.WriteLine($"🗓️ Selected Date: {_selectedDate:dd/MM/yyyy}");

                // عرض نطاق التواريخ المتاحة
                if (_allTransactions.Count > 0)
                {
                    var minDate = _allTransactions.Min(t => t.TransactionDate);
                    var maxDate = _allTransactions.Max(t => t.TransactionDate);
                    System.Diagnostics.Debug.WriteLine($"📅 Date Range: {minDate:dd/MM/yyyy} to {maxDate:dd/MM/yyyy}");
                    
                    // عدد الحركات للتاريخ المحدد
                    var selectedDateCount = _allTransactions.Count(t => t.TransactionDate.Date == _selectedDate.Date);
                    System.Diagnostics.Debug.WriteLine($"📊 Transactions for selected date: {selectedDateCount}");
                }

                // ✅ تحديث الواجهة
                UpdateSummarySection();
                UpdateAllGrids();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error in LoadDataAsync: {ex.Message}");
                MessageBox.Show($"خطأ في تحميل البيانات:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // UPDATE UI
        // ═══════════════════════════════════════════════════════════════
        private void UpdateSummarySection()
        {
            if (_selectedCashBox == null) return;

            var today = _selectedDate.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var yearStart = new DateTime(today.Year, 1, 1);

            // Update Cash Box Info Card
            FindLabel(pnlCashBoxInfo, "lblBalance")!.Text = $"{_selectedCashBox.CurrentBalance:N2} جنيه";
            FindLabel(pnlCashBoxInfo, "lblTransactions")!.Text = $"📝 عدد الحركات: {_allTransactions.Count} حركة";
            
            var lastTrx = _allTransactions.FirstOrDefault();
            FindLabel(pnlCashBoxInfo, "lblLastTransaction")!.Text = 
                lastTrx != null ? $"🕒 آخر حركة: {lastTrx.TransactionDate:dd/MM/yyyy - hh:mm tt}" : "🕒 آخر حركة: --";

            // Daily Stats
            var dailyIncome = _allTransactions
                .Where(t => t.TransactionDate.Date == today && t.Type == TransactionType.Income)
                .Sum(t => t.Amount);
            var dailyExpense = _allTransactions
                .Where(t => t.TransactionDate.Date == today && t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);
            UpdateStatCard(pnlQuickStats, "lblDailyIncome", "lblDailyExpense", "lblDailyNet", dailyIncome, dailyExpense);

            // Monthly Stats
            var monthlyIncome = _allTransactions
                .Where(t => t.TransactionDate >= monthStart && t.TransactionDate <= today && t.Type == TransactionType.Income)
                .Sum(t => t.Amount);
            var monthlyExpense = _allTransactions
                .Where(t => t.TransactionDate >= monthStart && t.TransactionDate <= today && t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);
            UpdateStatCard(pnlQuickStats, "lblMonthlyIncome", "lblMonthlyExpense", "lblMonthlyNet", monthlyIncome, monthlyExpense);

            // Yearly Stats
            var yearlyIncome = _allTransactions
                .Where(t => t.TransactionDate >= yearStart && t.TransactionDate <= today && t.Type == TransactionType.Income)
                .Sum(t => t.Amount);
            var yearlyExpense = _allTransactions
                .Where(t => t.TransactionDate >= yearStart && t.TransactionDate <= today && t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);
            UpdateStatCard(pnlQuickStats, "lblYearlyIncome", "lblYearlyExpense", "lblYearlyNet", yearlyIncome, yearlyExpense);
        }

        private void UpdateStatCard(Panel parent, string incLabel, string expLabel, string netLabel, decimal income, decimal expense)
        {
            var net = income - expense;
            
            FindLabel(parent, incLabel)!.Text = $"{income:N2}";
            FindLabel(parent, expLabel)!.Text = $"{expense:N2}";
            
            var lblNet = FindLabel(parent, netLabel)!;
            lblNet.Text = $"{net:N2}";
            lblNet.ForeColor = net >= 0 ? ColorScheme.Success : ColorScheme.Error;
        }

        private static Label? FindLabel(Panel parent, string name) =>
            parent.Controls.Find(name, true).FirstOrDefault() as Label;

        // ═══════════════════════════════════════════════════════════════
        // GRIDS UPDATE
        // ═══════════════════════════════════════════════════════════════
        private void UpdateAllGrids()
        {
            UpdateTransactionsGrid();
            UpdateDailySummaryGrid();
            UpdateMonthlySummaryGrid();
        }

        private void UpdateTransactionsGrid()
        {
            dgvTransactions.Columns.Clear();
            
            dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "VoucherNumber", 
                HeaderText = "رقم السند", 
                MinimumWidth = 100,
                FillWeight = 80
            });
            dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "DateTime", 
                HeaderText = "التاريخ والوقت", 
                MinimumWidth = 140,
                FillWeight = 120
            });
            dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Type", 
                HeaderText = "النوع", 
                MinimumWidth = 80,
                FillWeight = 60
            });
            dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Category", 
                HeaderText = "الفئة", 
                MinimumWidth = 100,
                FillWeight = 80
            });
            dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "PartyName", 
                HeaderText = "الطرف", 
                MinimumWidth = 120,
                FillWeight = 100
            });
            dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Description", 
                HeaderText = "البيان", 
                MinimumWidth = 150,
                FillWeight = 150
            });
            dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Amount", 
                HeaderText = "المبلغ", 
                MinimumWidth = 110,
                FillWeight = 90
            });
            dgvTransactions.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Balance", 
                HeaderText = "الرصيد بعد الحركة", 
                MinimumWidth = 120,
                FillWeight = 100
            });

            dgvTransactions.Rows.Clear();

            // ✅ إظهار حركات التاريخ المحدد فقط
            var transactions = _allTransactions
                .Where(t => t.TransactionDate.Date == _selectedDate.Date)
                .OrderByDescending(t => t.TransactionDate)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"📊 Displaying {transactions.Count} transactions for date {_selectedDate:dd/MM/yyyy}");

            if (transactions.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ No transactions found for {_selectedDate:dd/MM/yyyy}");
                System.Diagnostics.Debug.WriteLine($"Total transactions in memory: {_allTransactions.Count}");
                if (_allTransactions.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"First transaction date: {_allTransactions.First().TransactionDate:dd/MM/yyyy}");
                    System.Diagnostics.Debug.WriteLine($"Last transaction date: {_allTransactions.Last().TransactionDate:dd/MM/yyyy}");
                }
            }

            foreach (var t in transactions)
            {
                bool isIncome = t.Type == TransactionType.Income;
                var typeColor = isIncome ? ColorScheme.Success : ColorScheme.Error;
                var typeText = isIncome ? "إيراد ↗️" : "مصروف ↘️";

                // ✅ الحصول على اسم العملة الصحيح
                string currencyName = GetCurrencyName(t.TransactionCurrency);

                int idx = dgvTransactions.Rows.Add();
                var row = dgvTransactions.Rows[idx];

                row.Cells["VoucherNumber"].Value = t.VoucherNumber ?? "---";
                row.Cells["DateTime"].Value = t.TransactionDate.ToString("dd/MM/yyyy - hh:mm tt");
                row.Cells["Type"].Value = typeText;
                row.Cells["Type"].Style = new DataGridViewCellStyle 
                { 
                    ForeColor = typeColor, 
                    Font = new Font("Cairo", 10F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                };
                row.Cells["Category"].Value = t.Category ?? "غير محدد";
                row.Cells["PartyName"].Value = t.PartyName ?? "---";
                row.Cells["Description"].Value = t.Description ?? "---";
                
                // ✅ عرض المبلغ بالعملة الصحيحة
                row.Cells["Amount"].Value = $"{t.Amount:N2} {currencyName}";
                row.Cells["Amount"].Style = new DataGridViewCellStyle 
                { 
                    ForeColor = typeColor, 
                    Font = new Font("Cairo", 10.5F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                };
                
                // ✅ عرض الرصيد بالعملة الصحيحة
                row.Cells["Balance"].Value = $"{t.BalanceAfter:N2} {currencyName}";
                row.Cells["Balance"].Style = new DataGridViewCellStyle
                {
                    Font = new Font("Cairo", 10F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                };
            }
        }

        // ✅ دالة مساعدة لتحويل رمز العملة إلى اسمها بالعربي
        private string GetCurrencyName(string currencyCode)
        {
            return currencyCode?.ToUpper() switch
            {
                "EGP" => "جنيه",
                "USD" => "دولار",
                "EUR" => "يورو",
                "GBP" => "إسترليني",
                "SAR" => "ريال",
                _ => "جنيه" // افتراضي
            };
        }

        private void UpdateDailySummaryGrid()
        {
            dgvDailySummary.Columns.Clear();
            
            dgvDailySummary.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "التاريخ", MinimumWidth = 110 });
            dgvDailySummary.Columns.Add(new DataGridViewTextBoxColumn { Name = "Currency", HeaderText = "العملة", MinimumWidth = 80 });
            dgvDailySummary.Columns.Add(new DataGridViewTextBoxColumn { Name = "Count", HeaderText = "عدد الحركات", MinimumWidth = 90 });
            dgvDailySummary.Columns.Add(new DataGridViewTextBoxColumn { Name = "Income", HeaderText = "الإيرادات", MinimumWidth = 110 });
            dgvDailySummary.Columns.Add(new DataGridViewTextBoxColumn { Name = "Expense", HeaderText = "المصروفات", MinimumWidth = 110 });
            dgvDailySummary.Columns.Add(new DataGridViewTextBoxColumn { Name = "Net", HeaderText = "الصافي", MinimumWidth = 110 });

            dgvDailySummary.Rows.Clear();

            var monthStart = new DateTime(_selectedDate.Year, _selectedDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // ✅ التجميع حسب التاريخ والعملة
            var dailyData = _allTransactions
                .Where(t => t.TransactionDate >= monthStart && t.TransactionDate <= monthEnd)
                .GroupBy(t => new { Date = t.TransactionDate.Date, Currency = t.TransactionCurrency })
                .Select(g => new
                {
                    g.Key.Date,
                    g.Key.Currency,
                    Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    Expense = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(d => d.Date)
                .ThenBy(d => d.Currency);

            foreach (var d in dailyData)
            {
                var net = d.Income - d.Expense;
                string currencyName = GetCurrencyName(d.Currency);
                
                int idx = dgvDailySummary.Rows.Add();
                var row = dgvDailySummary.Rows[idx];

                row.Cells["Date"].Value = d.Date.ToString("dd/MM/yyyy");
                row.Cells["Currency"].Value = currencyName;
                row.Cells["Currency"].Style = new DataGridViewCellStyle 
                { 
                    Font = new Font("Cairo", 10F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                };
                row.Cells["Count"].Value = $"{d.Count} حركة";
                row.Cells["Income"].Value = $"{d.Income:N2}";
                row.Cells["Income"].Style = new DataGridViewCellStyle 
                { 
                    ForeColor = ColorScheme.Success,
                    Font = new Font("Cairo", 10F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                };
                row.Cells["Expense"].Value = $"{d.Expense:N2}";
                row.Cells["Expense"].Style = new DataGridViewCellStyle 
                { 
                    ForeColor = ColorScheme.Error,
                    Font = new Font("Cairo", 10F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                };
                row.Cells["Net"].Value = $"{net:N2}";
                row.Cells["Net"].Style = new DataGridViewCellStyle 
                { 
                    ForeColor = net >= 0 ? ColorScheme.Success : ColorScheme.Error,
                    Font = new Font("Cairo", 11F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                };
            }
        }

        private void UpdateMonthlySummaryGrid()
        {
            dgvMonthlySummary.Columns.Clear();
            
            dgvMonthlySummary.Columns.Add(new DataGridViewTextBoxColumn { Name = "Month", HeaderText = "الشهر", MinimumWidth = 110 });
            dgvMonthlySummary.Columns.Add(new DataGridViewTextBoxColumn { Name = "Currency", HeaderText = "العملة", MinimumWidth = 80 });
            dgvMonthlySummary.Columns.Add(new DataGridViewTextBoxColumn { Name = "Count", HeaderText = "عدد الحركات", MinimumWidth = 90 });
            dgvMonthlySummary.Columns.Add(new DataGridViewTextBoxColumn { Name = "Income", HeaderText = "الإيرادات", MinimumWidth = 110 });
            dgvMonthlySummary.Columns.Add(new DataGridViewTextBoxColumn { Name = "Expense", HeaderText = "المصروفات", MinimumWidth = 110 });
            dgvMonthlySummary.Columns.Add(new DataGridViewTextBoxColumn { Name = "Net", HeaderText = "الصافي", MinimumWidth = 110 });

            dgvMonthlySummary.Rows.Clear();

            string[] monthNames = { "يناير", "فبراير", "مارس", "إبريل", "مايو", "يونيو",
                                    "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" };

            // ✅ التجميع حسب الشهر والعملة
            var monthlyData = _allTransactions
                .Where(t => t.TransactionDate.Year == _selectedDate.Year)
                .GroupBy(t => new { Month = t.TransactionDate.Month, Currency = t.TransactionCurrency })
                .Select(g => new
                {
                    g.Key.Month,
                    g.Key.Currency,
                    Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    Expense = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(m => m.Month)
                .ThenBy(m => m.Currency);

            foreach (var m in monthlyData)
            {
                var net = m.Income - m.Expense;
                string currencyName = GetCurrencyName(m.Currency);
                
                int idx = dgvMonthlySummary.Rows.Add();
                var row = dgvMonthlySummary.Rows[idx];

                row.Cells["Month"].Value = $"{monthNames[m.Month - 1]} {_selectedDate.Year}";
                row.Cells["Currency"].Value = currencyName;
                row.Cells["Currency"].Style = new DataGridViewCellStyle 
                { 
                    Font = new Font("Cairo", 10F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                };
                row.Cells["Count"].Value = $"{m.Count} حركة";
                row.Cells["Income"].Value = $"{m.Income:N2}";
                row.Cells["Income"].Style = new DataGridViewCellStyle 
                { 
                    ForeColor = ColorScheme.Success,
                    Font = new Font("Cairo", 10F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                };
                row.Cells["Expense"].Value = $"{m.Expense:N2}";
                row.Cells["Expense"].Style = new DataGridViewCellStyle 
                { 
                    ForeColor = ColorScheme.Error,
                    Font = new Font("Cairo", 10F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                };
                row.Cells["Net"].Value = $"{net:N2}";
                row.Cells["Net"].Style = new DataGridViewCellStyle 
                { 
                    ForeColor = net >= 0 ? ColorScheme.Success : ColorScheme.Error,
                    Font = new Font("Cairo", 11F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // EXPORT & PRINT
        // ═══════════════════════════════════════════════════════════════
        private void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                using var sfd = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    FileName = $"جرد_الخزنة_{_selectedCashBox?.Name}_{_selectedDate:yyyy-MM-dd}.xlsx"
                };

                if (sfd.ShowDialog() != DialogResult.OK) return;

                using var workbook = new XLWorkbook();

                // Export all grids
                ExportGridToSheet(dgvTransactions, workbook.Worksheets.Add("الحركات التفصيلية"));
                ExportGridToSheet(dgvDailySummary, workbook.Worksheets.Add("الملخص اليومي"));
                ExportGridToSheet(dgvMonthlySummary, workbook.Worksheets.Add("الملخص الشهري"));

                workbook.SaveAs(sfd.FileName);

                MessageBox.Show("✅ تم تصدير التقرير بنجاح!", "نجاح", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في التصدير:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void ExportGridToSheet(DataGridView grid, IXLWorksheet sheet)
        {
            // Headers
            for (int col = 0; col < grid.Columns.Count; col++)
            {
                var cell = sheet.Cell(1, col + 1);
                cell.Value = grid.Columns[col].HeaderText;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2980B9");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Data
            for (int row = 0; row < grid.Rows.Count; row++)
            {
                for (int col = 0; col < grid.Columns.Count; col++)
                {
                    var cell = sheet.Cell(row + 2, col + 1);
                    cell.Value = grid.Rows[row].Cells[col].Value?.ToString() ?? "";
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
            }

            sheet.Columns().AdjustToContents();
            sheet.RightToLeft = true;
        }

        private void BtnPrint_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_selectedCashBox == null)
                {
                    MessageBox.Show("الرجاء اختيار خزنة أولاً", "تنبيه",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var printDoc = new System.Drawing.Printing.PrintDocument();
                var printDialog = new PrintDialog
                {
                    Document = printDoc,
                    UseEXDialog = true
                };

                // إعدادات الصفحة
                printDoc.DefaultPageSettings.Landscape = true;
                printDoc.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(50, 50, 50, 50);

                int currentPage = 0;
                int totalPages = 1;

                printDoc.PrintPage += (s, ev) =>
                {
                    try
                    {
                        var g = ev.Graphics;
                        if (g == null) return;

                        var pageWidth = ev.MarginBounds.Width;
                        var pageHeight = ev.MarginBounds.Height;
                        var yPos = ev.MarginBounds.Top;

                        // خطوط مخصصة
                        var titleFont = new Font("Cairo", 18, FontStyle.Bold);
                        var headerFont = new Font("Cairo", 14, FontStyle.Bold);
                        var normalFont = new Font("Cairo", 10);
                        var smallFont = new Font("Cairo", 9);

                        // ═══════════════════════════════════════
                        // رأس التقرير
                        // ═══════════════════════════════════════
                        var title = "تقرير جرد الخزنة";
                        var titleSize = g.MeasureString(title, titleFont);
                        g.DrawString(title, titleFont, Brushes.Black,
                            ev.MarginBounds.Left + (pageWidth - titleSize.Width) / 2, yPos);
                        yPos += (int)titleSize.Height + 10;

                        // خط فاصل
                        g.DrawLine(new Pen(Color.Gray, 2), ev.MarginBounds.Left, yPos,
                            ev.MarginBounds.Right, yPos);
                        yPos += 15;

                        // معلومات الخزنة
                        var cashBoxInfo = $"الخزنة: {_selectedCashBox.Name}     الرصيد الحالي: {_selectedCashBox.CurrentBalance:N2} جنيه     التاريخ: {DateTime.Now:dd/MM/yyyy}";
                        g.DrawString(cashBoxInfo, normalFont, Brushes.Black, ev.MarginBounds.Left, yPos);
                        yPos += 30;

                        g.DrawLine(Pens.LightGray, ev.MarginBounds.Left, yPos, ev.MarginBounds.Right, yPos);
                        yPos += 15;

                        // ═══════════════════════════════════════
                        // الملخص المالي
                        // ═══════════════════════════════════════
                        g.DrawString("الملخص المالي", headerFont, Brushes.DarkBlue, ev.MarginBounds.Left, yPos);
                        yPos += 30;

                        var today = DateTime.UtcNow.Date;
                        var monthStart = DateTime.SpecifyKind(new DateTime(today.Year, today.Month, 1), DateTimeKind.Utc);
                        var yearStart = DateTime.SpecifyKind(new DateTime(today.Year, 1, 1), DateTimeKind.Utc);

                        // اليوم
                        var dailyIncome = _allTransactions.Where(t => t.TransactionDate.Date == today && t.Type == TransactionType.Income).Sum(t => t.Amount);
                        var dailyExpense = _allTransactions.Where(t => t.TransactionDate.Date == today && t.Type == TransactionType.Expense).Sum(t => t.Amount);
                        var dailyNet = dailyIncome - dailyExpense;

                        // الشهر
                        var monthlyIncome = _allTransactions.Where(t => t.TransactionDate >= monthStart && t.TransactionDate <= today && t.Type == TransactionType.Income).Sum(t => t.Amount);
                        var monthlyExpense = _allTransactions.Where(t => t.TransactionDate >= monthStart && t.TransactionDate <= today && t.Type == TransactionType.Expense).Sum(t => t.Amount);
                        var monthlyNet = monthlyIncome - monthlyExpense;

                        // السنة
                        var yearlyIncome = _allTransactions.Where(t => t.TransactionDate >= yearStart && t.TransactionDate <= today && t.Type == TransactionType.Income).Sum(t => t.Amount);
                        var yearlyExpense = _allTransactions.Where(t => t.TransactionDate >= yearStart && t.TransactionDate <= today && t.Type == TransactionType.Expense).Sum(t => t.Amount);
                        var yearlyNet = yearlyIncome - yearlyExpense;

                        int colWidth = pageWidth / 3;
                        int xPos = ev.MarginBounds.Left;

                        // اليوم
                        g.DrawString("اليوم", normalFont, Brushes.Black, xPos, yPos);
                        g.DrawString($"الإيرادات: {dailyIncome:N2}", smallFont, Brushes.DarkGreen, xPos, yPos + 20);
                        g.DrawString($"المصروفات: {dailyExpense:N2}", smallFont, Brushes.DarkRed, xPos, yPos + 35);
                        g.DrawString($"الصافي: {dailyNet:N2}", smallFont, dailyNet >= 0 ? Brushes.DarkGreen : Brushes.DarkRed, xPos, yPos + 50);

                        xPos += colWidth;

                        // الشهر
                        g.DrawString("هذا الشهر", normalFont, Brushes.Black, xPos, yPos);
                        g.DrawString($"الإيرادات: {monthlyIncome:N2}", smallFont, Brushes.DarkGreen, xPos, yPos + 20);
                        g.DrawString($"المصروفات: {monthlyExpense:N2}", smallFont, Brushes.DarkRed, xPos, yPos + 35);
                        g.DrawString($"الصافي: {monthlyNet:N2}", smallFont, monthlyNet >= 0 ? Brushes.DarkGreen : Brushes.DarkRed, xPos, yPos + 50);

                        xPos += colWidth;

                        // السنة
                        g.DrawString("هذا العام", normalFont, Brushes.Black, xPos, yPos);
                        g.DrawString($"الإيرادات: {yearlyIncome:N2}", smallFont, Brushes.DarkGreen, xPos, yPos + 20);
                        g.DrawString($"المصروفات: {yearlyExpense:N2}", smallFont, Brushes.DarkRed, xPos, yPos + 35);
                        g.DrawString($"الصافي: {yearlyNet:N2}", smallFont, yearlyNet >= 0 ? Brushes.DarkGreen : Brushes.DarkRed, xPos, yPos + 50);

                        yPos += 80;

                        g.DrawLine(Pens.LightGray, ev.MarginBounds.Left, yPos, ev.MarginBounds.Right, yPos);
                        yPos += 15;

                        // ═══════════════════════════════════════
                        // جدول الحركات
                        // ═══════════════════════════════════════
                        g.DrawString("الحركات الحديثة (آخر 20 حركة)", headerFont, Brushes.DarkBlue, ev.MarginBounds.Left, yPos);
                        yPos += 30;

                        // رأس الجدول
                        var headerBrush = new SolidBrush(Color.FromArgb(41, 128, 185));
                        var headerRect = new Rectangle(ev.MarginBounds.Left, yPos, pageWidth, 25);
                        g.FillRectangle(headerBrush, headerRect);

                        var colWidths = new[] { 80, 120, 60, 80, 250, 100, 100 };
                        xPos = ev.MarginBounds.Left + 5;

                        g.DrawString("رقم السند", smallFont, Brushes.White, xPos, yPos + 5);
                        xPos += colWidths[0];
                        g.DrawString("التاريخ", smallFont, Brushes.White, xPos, yPos + 5);
                        xPos += colWidths[1];
                        g.DrawString("النوع", smallFont, Brushes.White, xPos, yPos + 5);
                        xPos += colWidths[2];
                        g.DrawString("الفئة", smallFont, Brushes.White, xPos, yPos + 5);
                        xPos += colWidths[3];
                        g.DrawString("البيان", smallFont, Brushes.White, xPos, yPos + 5);
                        xPos += colWidths[4];
                        g.DrawString("المبلغ", smallFont, Brushes.White, xPos, yPos + 5);
                        xPos += colWidths[5];
                        g.DrawString("الرصيد", smallFont, Brushes.White, xPos, yPos + 5);

                        yPos += 30;

                        // صفوف الجدول
                        var transactions = _allTransactions.OrderByDescending(t => t.TransactionDate).Take(20).ToList();
                        int rowCount = 0;

                        foreach (var t in transactions)
                        {
                            if (yPos > ev.MarginBounds.Bottom - 50)
                            {
                                ev.HasMorePages = true;
                                return;
                            }

                            // خلفية السطر
                            var rowBrush = rowCount % 2 == 0 ? Brushes.White : new SolidBrush(Color.FromArgb(248, 250, 252));
                            g.FillRectangle(rowBrush, ev.MarginBounds.Left, yPos, pageWidth, 20);

                            xPos = ev.MarginBounds.Left + 5;
                            var textBrush = Brushes.Black;

                            g.DrawString(t.VoucherNumber ?? "---", smallFont, textBrush, xPos, yPos + 2);
                            xPos += colWidths[0];

                            g.DrawString(t.TransactionDate.ToString("dd/MM/yyyy"), smallFont, textBrush, xPos, yPos + 2);
                            xPos += colWidths[1];

                            var typeText = t.Type == TransactionType.Income ? "إيراد" : "مصروف";
                            var typeColor = t.Type == TransactionType.Income ? Brushes.DarkGreen : Brushes.DarkRed;
                            g.DrawString(typeText, smallFont, typeColor, xPos, yPos + 2);
                            xPos += colWidths[2];

                            g.DrawString(t.Category ?? "غير محدد", smallFont, textBrush, xPos, yPos + 2);
                            xPos += colWidths[3];

                            var desc = t.Description ?? "---";
                            if (desc.Length > 30) desc = desc.Substring(0, 27) + "...";
                            g.DrawString(desc, smallFont, textBrush, xPos, yPos + 2);
                            xPos += colWidths[4];

                            g.DrawString($"{t.Amount:N2}", smallFont, typeColor, xPos, yPos + 2);
                            xPos += colWidths[5];

                            g.DrawString($"{t.BalanceAfter:N2}", smallFont, textBrush, xPos, yPos + 2);

                            yPos += 22;
                            rowCount++;
                        }

                        // ذيل الصفحة
                        var footerY = ev.MarginBounds.Bottom + 10;
                        var footerText = $"صفحة {currentPage + 1} من {totalPages}     طُبع في: {DateTime.Now:dd/MM/yyyy hh:mm tt}";
                        var footerSize = g.MeasureString(footerText, smallFont);
                        g.DrawString(footerText, smallFont, Brushes.Gray,
                            ev.MarginBounds.Left + (pageWidth - footerSize.Width) / 2, footerY);

                        currentPage++;
                        ev.HasMorePages = false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"خطأ أثناء الطباعة:\n{ex.Message}", "خطأ",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ev.HasMorePages = false;
                    }
                };

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDoc.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في الطباعة:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════════════════════════
        private Button CreateActionButton(string text, Color color)
        {
            var btn = new Button
            {
                Text = text,
                Width = 140,
                Height = 36,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(5, 0, 0, 0)
            };

            btn.FlatAppearance.BorderSize = 0;
            
            btn.MouseEnter += (s, e) => btn.BackColor = ColorScheme.Darken(color, 0.15f);
            btn.MouseLeave += (s, e) => btn.BackColor = color;

            return btn;
        }
    }
}
