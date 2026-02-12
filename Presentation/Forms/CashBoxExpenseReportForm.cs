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
    public partial class CashBoxExpenseReportForm : Form
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly int _currentUserId;

        // Controls
        private ComboBox cmbCashBox = null!;
        private DateTimePicker dtpStartDate = null!;
        private DateTimePicker dtpEndDate = null!;
        private Button btnRefresh = null!;
        private Button btnExport = null!;
        private Button btnPrint = null!;
        private ComboBox cmbCurrencyFilter = null!;
        private TextBox txtSearch = null!;

        // Main Panels
        private Panel pnlHeader = null!;
        private Panel pnlMainContent = null!;
        private Panel pnlSummary = null!;

        // Data Grid
        private DataGridView dgvExpenses = null!;

        // Data
        private CashBox? _selectedCashBox;
        private DateTime _startDate;
        private DateTime _endDate;
        private List<CashTransaction> _expenseTransactions = new();
        private List<CashTransaction> _filteredTransactions = new();
        private bool _isInitializing = true;

        public CashBoxExpenseReportForm(IDbContextFactory<AppDbContext> contextFactory, int currentUserId)
        {
            _contextFactory = contextFactory;
            _currentUserId = currentUserId;
            _startDate = DateTime.UtcNow.Date.AddDays(-30); // آخر 30 يوم
            _endDate = DateTime.UtcNow.Date;

            InitializeComponent();
            BuildUI();
            _isInitializing = false;
            _ = LoadDataAsync();
        }

        private void BuildUI()
        {
            this.SuspendLayout();

            // Main wrapper
            var wrapper = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 15, 20, 15), // زيادة المساحة من الأعلى والأسفل
                BackColor = Color.FromArgb(245, 247, 250)
            };
            this.Controls.Add(wrapper);

            // Header Section
            pnlHeader = BuildHeaderSection();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 200; // Increased from 140 to accommodate filter row
            wrapper.Controls.Add(pnlHeader);

            // Main Content Area
            pnlMainContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 15, 0, 0) // زيادة المساحة من الأعلى
            };
            wrapper.Controls.Add(pnlMainContent);

            // Data Grid Section - يتضاف أولاً عشان يكون في الخلفية
            var gridSection = BuildGridSection();
            gridSection.Dock = DockStyle.Fill;
            gridSection.Margin = new Padding(0, 0, 0, 0); // إزالة المساحة العلوية
            pnlMainContent.Controls.Add(gridSection);

            // Summary Panel - يتضاف ثانياً عشان يكون فوق
            pnlSummary = BuildSummaryPanel();
            pnlSummary.Dock = DockStyle.Top;
            pnlSummary.Height = 150;
            pnlSummary.Margin = new Padding(0, 0, 0, 25); // مساحة سفلية فقط
            pnlMainContent.Controls.Add(pnlSummary);

            this.ResumeLayout(true);
        }

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
                Text = "💸 تقرير المصروفات - Expense Report",
                Font = new Font("Cairo", 18F, FontStyle.Bold),
                ForeColor = ColorScheme.Error,
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
                Width = 220,
                Height = 36,
                Font = new Font("Cairo", 10.5F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 0, 20, 0),
                FlatStyle = FlatStyle.Flat
            };
            cmbCashBox.SelectedIndexChanged += async (s, e) =>
            {
                if (!_isInitializing)
                    await LoadDataAsync();
            };

            // من تاريخ
            var lblStartDate = new Label
            {
                Text = "من:",
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                ForeColor = ColorScheme.TextPrimary,
                Width = 40,
                Height = 36,
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(0, 0, 10, 0)
            };

            dtpStartDate = new DateTimePicker
            {
                Width = 160,
                Height = 36,
                Font = new Font("Cairo", 10.5F),
                Format = DateTimePickerFormat.Short,
                Margin = new Padding(0, 0, 15, 0),
                Value = _startDate
            };
            dtpStartDate.ValueChanged += async (s, e) =>
            {
                if (_isInitializing) return;
                _startDate = DateTime.SpecifyKind(dtpStartDate.Value.Date, DateTimeKind.Utc);
                await LoadDataAsync();
            };

            // إلى تاريخ
            var lblEndDate = new Label
            {
                Text = "إلى:",
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                ForeColor = ColorScheme.TextPrimary,
                Width = 40,
                Height = 36,
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(0, 0, 10, 0)
            };

            dtpEndDate = new DateTimePicker
            {
                Width = 160,
                Height = 36,
                Font = new Font("Cairo", 10.5F),
                Format = DateTimePickerFormat.Short,
                Margin = new Padding(0, 0, 30, 0),
                Value = _endDate
            };
            dtpEndDate.ValueChanged += async (s, e) =>
            {
                if (_isInitializing) return;
                _endDate = DateTime.SpecifyKind(dtpEndDate.Value.Date, DateTimeKind.Utc);
                await LoadDataAsync();
            };

            // Buttons
            btnRefresh = CreateActionButton("🔄 تحديث", ColorScheme.Error);
            btnRefresh.Click += async (s, e) => await LoadDataAsync();

            btnExport = CreateActionButton("📤 Excel", ColorScheme.Primary);
            btnExport.Click += BtnExport_Click;

            btnPrint = CreateActionButton("🖨️ طباعة", ColorScheme.Info);
            btnPrint.Click += BtnPrint_Click;

            flowPanel.Controls.AddRange(new Control[]
            {
                lblCashBox, cmbCashBox,
                lblStartDate, dtpStartDate,
                lblEndDate, dtpEndDate,
                btnRefresh, btnExport, btnPrint
            });

            // Filter Row (Second row in header)
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.Transparent
            };
            header.Controls.Add(filterPanel);

            var filterFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoSize = false,
                Padding = new Padding(0, 8, 0, 0)
            };
            filterPanel.Controls.Add(filterFlow);

            // Currency Filter
            var lblCurrency = new Label
            {
                Text = "العملة:",
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                ForeColor = ColorScheme.TextPrimary,
                Width = 70,
                Height = 36,
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(0, 0, 10, 0)
            };

            cmbCurrencyFilter = new ComboBox
            {
                Width = 150,
                Height = 36,
                Font = new Font("Cairo", 10.5F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 0, 30, 0),
                FlatStyle = FlatStyle.Flat
            };
            cmbCurrencyFilter.Items.AddRange(new object[]
            {
                "الكل",
                "EGP - جنيه",
                "USD - دولار",
                "EUR - يورو",
                "GBP - إسترليني",
                "SAR - ريال"
            });
            cmbCurrencyFilter.SelectedIndex = 0;
            cmbCurrencyFilter.SelectedIndexChanged += (s, e) => ApplyFilters();

            // Search
            var lblSearch = new Label
            {
                Text = "🔍 بحث:",
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                ForeColor = ColorScheme.TextPrimary,
                Width = 70,
                Height = 36,
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(0, 0, 10, 0)
            };

            txtSearch = new TextBox
            {
                Width = 300,
                Height = 36,
                Font = new Font("Cairo", 10.5F),
                Margin = new Padding(0, 0, 20, 0),
                PlaceholderText = "ابحث في البيان، التصنيف، أو الطرف..."
            };
            txtSearch.TextChanged += (s, e) => ApplyFilters();

            var btnClearFilters = CreateActionButton("🗑️ مسح الفلتر", ColorScheme.ButtonSecondary);
            btnClearFilters.Click += (s, e) =>
            {
                cmbCurrencyFilter.SelectedIndex = 0;
                txtSearch.Clear();
            };

            filterFlow.Controls.AddRange(new Control[]
            {
                lblCurrency, cmbCurrencyFilter,
                lblSearch, txtSearch,
                btnClearFilters
            });

            return header;
        }

        private Panel BuildSummaryPanel()
        {
            var panel = new Panel
            {
                BackColor = Color.White,
                Padding = new Padding(25)
            };
            panel.Paint += PaintCard;
            panel.Paint += (s, e) =>
            {
                using var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, panel.Width, 6),
                    ColorScheme.Error,
                    Color.FromArgb(180, 0, 0),
                    LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(brush, 0, 0, panel.Width, 6);
            };

            // العنوان
            var lblTitle = new Label
            {
                Text = "📊 ملخص المصروفات",
                Font = new Font("Cairo", 14F, FontStyle.Bold),
                ForeColor = ColorScheme.Error,
                Dock = DockStyle.Top,
                Height = 35,
                TextAlign = ContentAlignment.MiddleRight
            };
            panel.Controls.Add(lblTitle);

            // Statistics Container
            var statsContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            statsContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            statsContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            statsContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            statsContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            // إجمالي المصروفات
            statsContainer.Controls.Add(CreateStatBox("💵 إجمالي المصروفات", "lblTotalExpense", "0.00"), 0, 0);
            
            // عدد الحركات
            statsContainer.Controls.Add(CreateStatBox("📝 عدد الحركات", "lblTransactionCount", "0"), 1, 0);
            
            // متوسط المصروف
            statsContainer.Controls.Add(CreateStatBox("📈 متوسط المصروف", "lblAverageExpense", "0.00"), 2, 0);
            
            // أعلى مصروف
            statsContainer.Controls.Add(CreateStatBox("⭐ أعلى مصروف", "lblMaxExpense", "0.00"), 3, 0);

            panel.Controls.Add(statsContainer);

            return panel;
        }

        private Panel CreateStatBox(string title, string labelName, string defaultValue)
        {
            var box = new Panel
            {
                Padding = new Padding(15),
                Margin = new Padding(5)
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Cairo", 9.5F),
                ForeColor = ColorScheme.TextSecondary,
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.TopRight
            };

            var lblValue = new Label
            {
                Name = labelName,
                Text = defaultValue,
                Font = new Font("Cairo", 16F, FontStyle.Bold),
                ForeColor = ColorScheme.Error,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleRight
            };

            box.Controls.Add(lblTitle);
            box.Controls.Add(lblValue);

            return box;
        }

        private Panel BuildGridSection()
        {
            var section = new Panel
            {
                BackColor = Color.White,
                Padding = new Padding(25, 180, 25, 15), // زيادة Padding العلوي من 30 إلى 180 (عشان يبدأ بعد الـ Summary)
                Margin = new Padding(0, 0, 0, 0)
            };
            section.Paint += PaintCard;

            var lblTitle = new Label
            {
                Text = "📋 تفاصيل المصروفات",
                Font = new Font("Cairo", 13F, FontStyle.Bold),
                ForeColor = ColorScheme.TextPrimary,
                Dock = DockStyle.Top,
                Height = 55, // زيادة الارتفاع من 45 إلى 55
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 10, 0, 20) // زيادة المساحة السفلية من 10 إلى 20
            };
            section.Controls.Add(lblTitle);

            dgvExpenses = CreateStyledDataGrid();
            dgvExpenses.Dock = DockStyle.Fill;
            dgvExpenses.Margin = new Padding(0, 10, 0, 0); // إضافة مساحة علوية للجدول
            section.Controls.Add(dgvExpenses);

            return section;
        }

        private DataGridView CreateStyledDataGrid()
        {
            var grid = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 50, // زيادة من 45 إلى 50
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(235, 237, 240),
                RowTemplate = { Height = 45 }, // زيادة من 40 إلى 45
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Cairo", 10F),
                RightToLeft = RightToLeft.Yes,
                Margin = new Padding(0, 25, 0, 25), // زيادة المساحة من الأعلى والأسفل
                ScrollBars = ScrollBars.Both, // إضافة scroll bars
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None // منع auto resize للسطور
            };

            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = ColorScheme.Error,
                ForeColor = Color.White,
                Font = new Font("Cairo", 10.5F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(8) // زيادة من 5 إلى 8
            };

            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = ColorScheme.TextPrimary,
                SelectionBackColor = Color.FromArgb(255, 220, 220),
                SelectionForeColor = ColorScheme.Error,
                Padding = new Padding(10), // زيادة من 8 إلى 10
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(252, 248, 248)
            };

            // Add double-click event
            grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < _filteredTransactions.Count)
                {
                    var transaction = _filteredTransactions[e.RowIndex];
                    var detailsForm = new TransactionDetailsReportForm(transaction);
                    detailsForm.ShowDialog();
                }
            };

            return grid;
        }

        private void PaintCard(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel p) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var shadowBrush = new SolidBrush(Color.FromArgb(15, 0, 0, 0));
            e.Graphics.FillRectangle(shadowBrush, 2, 3, p.Width - 2, p.Height);

            var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
            using var path = GetRoundedPath(rect, 10);
            using var brush = new SolidBrush(p.BackColor);
            e.Graphics.FillPath(brush, path);

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

        private async Task LoadDataAsync()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                using var ctx1 = _contextFactory.CreateDbContext();
                var cashBoxes = await ctx1.CashBoxes
                    .Where(c => !c.IsDeleted && c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                if (cmbCashBox.Items.Count == 0)
                {
                    cmbCashBox.DisplayMember = "Name";
                    cmbCashBox.ValueMember = "Id";
                    
                    foreach (var cb in cashBoxes)
                        cmbCashBox.Items.Add(cb);

                    if (cmbCashBox.Items.Count > 0)
                        cmbCashBox.SelectedIndex = 0;
                }

                var selectedCashBoxId = (_selectedCashBox as CashBox)?.Id ?? 
                                       (cmbCashBox.SelectedItem as CashBox)?.Id;
                
                if (!selectedCashBoxId.HasValue) return;

                using var ctx2 = _contextFactory.CreateDbContext();
                _selectedCashBox = await ctx2.CashBoxes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == selectedCashBoxId.Value && !c.IsDeleted);

                if (_selectedCashBox == null) return;

                using var ctx3 = _contextFactory.CreateDbContext();
                _expenseTransactions = await ctx3.CashTransactions
                    .AsNoTracking()
                    .Where(t => t.CashBoxId == _selectedCashBox.Id && 
                                !t.IsDeleted &&
                                t.Type == TransactionType.Expense &&
                                t.TransactionDate.Date >= _startDate.Date &&
                                t.TransactionDate.Date <= _endDate.Date)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();

                // تشخيص: عرض عدد المعاملات لكل عملة
                var currencyCounts = _expenseTransactions
                    .GroupBy(t => t.TransactionCurrency ?? "NULL")
                    .Select(g => $"{g.Key}: {g.Count()}")
                    .ToList();
                
                if (currencyCounts.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"المصروفات حسب العملة: {string.Join(", ", currencyCounts)}");
                }

                UpdateSummary();
                UpdateGrid();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل البيانات:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void UpdateSummary()
        {
            // Use filtered transactions for summary
            var transactionsToSummarize = _filteredTransactions.Count > 0 ? _filteredTransactions : _expenseTransactions;
            
            // حساب المبالغ الفعلية (بعد إضافة عمولة إنستا باي)
            var transactionsWithActualAmounts = transactionsToSummarize.Select(t => new
            {
                Transaction = t,
                ActualAmount = t.PaymentMethod == PaymentMethod.InstaPay && t.InstaPayCommission.HasValue 
                    ? t.Amount + t.InstaPayCommission.Value 
                    : t.Amount
            }).ToList();

            // Group by currency
            var groupedByCurrency = transactionsWithActualAmounts
                .GroupBy(t => t.Transaction.TransactionCurrency)
                .Select(g => new
                {
                    Currency = g.Key,
                    Total = g.Sum(t => t.ActualAmount),
                    Count = g.Count(),
                    Average = g.Average(t => t.ActualAmount),
                    Max = g.Max(t => t.ActualAmount)
                })
                .ToList();

            // Display summary
            var totalLabel = FindLabel(pnlSummary, "lblTotalExpense");
            var countLabel = FindLabel(pnlSummary, "lblTransactionCount");
            var avgLabel = FindLabel(pnlSummary, "lblAverageExpense");
            var maxLabel = FindLabel(pnlSummary, "lblMaxExpense");

            if (groupedByCurrency.Count == 1)
            {
                var stats = groupedByCurrency.First();
                totalLabel!.Text = $"{stats.Total:N2} {GetCurrencyName(stats.Currency)}";
                countLabel!.Text = $"{stats.Count} حركة";
                avgLabel!.Text = $"{stats.Average:N2} {GetCurrencyName(stats.Currency)}";
                maxLabel!.Text = $"{stats.Max:N2} {GetCurrencyName(stats.Currency)}";
            }
            else if (groupedByCurrency.Count > 1)
            {
                // Multiple currencies
                totalLabel!.Text = string.Join(" + ", groupedByCurrency.Select(g => 
                    $"{g.Total:N2} {GetCurrencyName(g.Currency)}"));
                countLabel!.Text = $"{transactionsToSummarize.Count} حركة";
                avgLabel!.Text = "متعدد العملات";
                maxLabel!.Text = string.Join(" / ", groupedByCurrency.Select(g => 
                    $"{g.Max:N2} {GetCurrencyName(g.Currency)}"));
            }
            else
            {
                totalLabel!.Text = "0.00";
                countLabel!.Text = "0 حركة";
                avgLabel!.Text = "0.00";
                maxLabel!.Text = "0.00";
            }
        }

        private void UpdateGrid()
        {
            _filteredTransactions = _expenseTransactions.ToList();
            UpdateSummary();
            RefreshGrid();
        }

        private void ApplyFilters()
        {
            if (_isInitializing || _expenseTransactions == null || _expenseTransactions.Count == 0)
                return;

            _filteredTransactions = _expenseTransactions.ToList();

            // Currency filter
            if (cmbCurrencyFilter.SelectedIndex > 0)
            {
                var selectedItem = cmbCurrencyFilter.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selectedItem))
                {
                    var selectedCurrency = selectedItem.Split('-')[0].Trim();
                    _filteredTransactions = _filteredTransactions
                        .Where(t => t.TransactionCurrency?.ToUpper() == selectedCurrency.ToUpper())
                        .ToList();
                }
            }

            // Search filter
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var searchTerm = txtSearch.Text.Trim().ToLower();
                _filteredTransactions = _filteredTransactions
                    .Where(t =>
                        (t.Description?.ToLower().Contains(searchTerm) ?? false) ||
                        (t.Category?.ToLower().Contains(searchTerm) ?? false) ||
                        (t.PartyName?.ToLower().Contains(searchTerm) ?? false) ||
                        (t.VoucherNumber?.ToLower().Contains(searchTerm) ?? false))
                    .ToList();
            }

            UpdateSummary();
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            dgvExpenses.Columns.Clear();
            
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "VoucherNumber", 
                HeaderText = "رقم السند", 
                MinimumWidth = 100,
                FillWeight = 70
            });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "DateTime", 
                HeaderText = "التاريخ والوقت", 
                MinimumWidth = 140,
                FillWeight = 110
            });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Category", 
                HeaderText = "التصنيف", 
                MinimumWidth = 100,
                FillWeight = 80
            });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "PartyName", 
                HeaderText = "إلى", 
                MinimumWidth = 120,
                FillWeight = 100
            });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Description", 
                HeaderText = "البيان", 
                MinimumWidth = 200,
                FillWeight = 180
            });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "PaymentMethod", 
                HeaderText = "طريقة الدفع", 
                MinimumWidth = 100,
                FillWeight = 80
            });
            dgvExpenses.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Amount", 
                HeaderText = "المبلغ", 
                MinimumWidth = 120,
                FillWeight = 90
            });

            dgvExpenses.Rows.Clear();

            // متغيرات لحساب الإجماليات حسب العملة
            var totalsByCurrency = new Dictionary<string, decimal>();

            foreach (var t in _filteredTransactions)
            {
                string currencyName = GetCurrencyName(t.TransactionCurrency);
                string paymentMethod = GetPaymentMethodName(t.PaymentMethod);

                // حساب المبلغ الفعلي (مع إضافة عمولة إنستا باي إن وجدت)
                decimal actualAmount = t.Amount;
                string amountDisplay;
                
                if (t.PaymentMethod == PaymentMethod.InstaPay && t.InstaPayCommission.HasValue && t.InstaPayCommission.Value > 0)
                {
                    actualAmount = t.Amount + t.InstaPayCommission.Value;
                    amountDisplay = $"{actualAmount:N2} {currencyName} (عمولة: {t.InstaPayCommission.Value:N2})";
                }
                else
                {
                    amountDisplay = $"{actualAmount:N2} {currencyName}";
                }

                // تجميع الإجماليات حسب العملة
                if (!totalsByCurrency.ContainsKey(t.TransactionCurrency))
                    totalsByCurrency[t.TransactionCurrency] = 0;
                totalsByCurrency[t.TransactionCurrency] += actualAmount;

                int idx = dgvExpenses.Rows.Add();
                var row = dgvExpenses.Rows[idx];

                row.Cells["VoucherNumber"].Value = t.VoucherNumber ?? "---";
                row.Cells["DateTime"].Value = t.TransactionDate.ToString("dd/MM/yyyy - hh:mm tt");
                row.Cells["Category"].Value = t.Category ?? "غير محدد";
                row.Cells["PartyName"].Value = t.PartyName ?? "---";
                row.Cells["Description"].Value = t.Description ?? "---";
                row.Cells["PaymentMethod"].Value = paymentMethod;
                row.Cells["Amount"].Value = amountDisplay;
                row.Cells["Amount"].Style = new DataGridViewCellStyle 
                { 
                    ForeColor = ColorScheme.Error, 
                    Font = new Font("Cairo", 11F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                };
            }

            // إضافة صفوف الإجمالي - صف لكل عملة
            if (_filteredTransactions.Count > 0 && totalsByCurrency.Count > 0)
            {
                // إضافة سطر فارغ قبل الإجماليات
                int separatorIdx = dgvExpenses.Rows.Add();
                var separatorRow = dgvExpenses.Rows[separatorIdx];
                separatorRow.DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(240, 240, 240)
                };

                // إضافة صف إجمالي لكل عملة
                foreach (var currencyTotal in totalsByCurrency.OrderBy(x => x.Key))
                {
                    int totalIdx = dgvExpenses.Rows.Add();
                    var totalRow = dgvExpenses.Rows[totalIdx];

                    totalRow.Cells["VoucherNumber"].Value = "";
                    totalRow.Cells["DateTime"].Value = "";
                    totalRow.Cells["Category"].Value = "";
                    totalRow.Cells["PartyName"].Value = "";
                    totalRow.Cells["Description"].Value = $"إجمالي {GetCurrencyName(currencyTotal.Key)}";
                    totalRow.Cells["PaymentMethod"].Value = "";
                    totalRow.Cells["Amount"].Value = $"{currencyTotal.Value:N2} {GetCurrencyName(currencyTotal.Key)}";

                    // تنسيق صف الإجمالي
                    totalRow.DefaultCellStyle = new DataGridViewCellStyle
                    {
                        BackColor = Color.FromArgb(255, 240, 240),
                        Font = new Font("Cairo", 12F, FontStyle.Bold),
                        ForeColor = ColorScheme.Error,
                        Alignment = DataGridViewContentAlignment.MiddleCenter
                    };
                }

                // إذا كان هناك أكثر من عملة، أضف صف الإجمالي العام
                if (totalsByCurrency.Count > 1)
                {
                    int grandTotalIdx = dgvExpenses.Rows.Add();
                    var grandTotalRow = dgvExpenses.Rows[grandTotalIdx];

                    grandTotalRow.Cells["VoucherNumber"].Value = "";
                    grandTotalRow.Cells["DateTime"].Value = "";
                    grandTotalRow.Cells["Category"].Value = "";
                    grandTotalRow.Cells["PartyName"].Value = "";
                    grandTotalRow.Cells["Description"].Value = "📊 الإجمالي الكلي";
                    grandTotalRow.Cells["PaymentMethod"].Value = "";
                    
                    string allTotals = string.Join(" + ", 
                        totalsByCurrency.OrderBy(x => x.Key).Select(kvp => 
                            $"{kvp.Value:N2} {GetCurrencyName(kvp.Key)}"));
                    grandTotalRow.Cells["Amount"].Value = allTotals;

                    grandTotalRow.DefaultCellStyle = new DataGridViewCellStyle
                    {
                        BackColor = Color.FromArgb(220, 53, 69),
                        Font = new Font("Cairo", 13F, FontStyle.Bold),
                        ForeColor = Color.White,
                        Alignment = DataGridViewContentAlignment.MiddleCenter
                    };
                }
            }
        }

        private string GetCurrencyName(string currencyCode)
        {
            return currencyCode?.ToUpper() switch
            {
                "EGP" => "جنيه",
                "USD" => "دولار",
                "EUR" => "يورو",
                "GBP" => "إسترليني",
                "SAR" => "ريال",
                _ => "جنيه"
            };
        }

        private string GetPaymentMethodName(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Cash => "نقدي",
                PaymentMethod.Cheque => "شيك",
                PaymentMethod.BankTransfer => "تحويل بنكي",
                PaymentMethod.CreditCard or PaymentMethod.Card or PaymentMethod.Visa => "بطاقة",
                PaymentMethod.InstaPay => "إنستا باي",
                _ => "آخر"
            };
        }

        private static Label? FindLabel(Panel parent, string name) =>
            parent.Controls.Find(name, true).FirstOrDefault() as Label;

        private Button CreateActionButton(string text, Color color)
        {
            var btn = new Button
            {
                Text = text,
                Width = 120,
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

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                using var sfd = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    FileName = $"تقرير_المصروفات_{_selectedCashBox?.Name}_{_startDate:yyyy-MM-dd}_to_{_endDate:yyyy-MM-dd}.xlsx"
                };

                if (sfd.ShowDialog() != DialogResult.OK) return;

                using var workbook = new XLWorkbook();
                var sheet = workbook.Worksheets.Add("المصروفات");

                // Headers
                for (int col = 0; col < dgvExpenses.Columns.Count; col++)
                {
                    var cell = sheet.Cell(1, col + 1);
                    cell.Value = dgvExpenses.Columns[col].HeaderText;
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#dc3545");
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // Data
                for (int row = 0; row < dgvExpenses.Rows.Count; row++)
                {
                    for (int col = 0; col < dgvExpenses.Columns.Count; col++)
                    {
                        sheet.Cell(row + 2, col + 1).Value = dgvExpenses.Rows[row].Cells[col].Value?.ToString() ?? "";
                    }
                }

                sheet.Columns().AdjustToContents();
                sheet.RightToLeft = true;
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

        private void BtnPrint_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_filteredTransactions == null || _filteredTransactions.Count == 0)
                {
                    MessageBox.Show("لا توجد بيانات للطباعة", "تنبيه",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // إنشاء PrintDocument
                var printDoc = new System.Drawing.Printing.PrintDocument();
                printDoc.DocumentName = $"تقرير_المصروفات_{_selectedCashBox?.Name}_{DateTime.Now:yyyy-MM-dd}";
                
                // متغيرات للطباعة
                int currentRow = 0;
                int rowsPerPage = 30; // عدد السطور في الصفحة
                Font titleFont = new Font("Cairo", 16F, FontStyle.Bold);
                Font headerFont = new Font("Cairo", 11F, FontStyle.Bold);
                Font dataFont = new Font("Cairo", 10F);
                
                printDoc.PrintPage += (s, ev) =>
                {
                    if (ev.Graphics == null) return;
                    
                    float yPos = ev.MarginBounds.Top;
                    float leftMargin = ev.MarginBounds.Left;
                    float rightMargin = ev.MarginBounds.Right;
                    
                    // رسم العنوان
                    string title = $"تقرير المصروفات - {_selectedCashBox?.Name}";
                    var titleSize = ev.Graphics.MeasureString(title, titleFont);
                    ev.Graphics.DrawString(title, titleFont, Brushes.Black, 
                        rightMargin - titleSize.Width, yPos);
                    yPos += titleSize.Height + 10;
                    
                    // رسم التاريخ
                    string dateRange = $"من {_startDate:dd/MM/yyyy} إلى {_endDate:dd/MM/yyyy}";
                    var dateSize = ev.Graphics.MeasureString(dateRange, dataFont);
                    ev.Graphics.DrawString(dateRange, dataFont, Brushes.Gray,
                        rightMargin - dateSize.Width, yPos);
                    yPos += dateSize.Height + 20;
                    
                    // رسم خط فاصل
                    ev.Graphics.DrawLine(Pens.Black, leftMargin, yPos, rightMargin, yPos);
                    yPos += 10;
                    
                    // رسم عناوين الأعمدة
                    string[] headers = { "رقم السند", "التاريخ", "التصنيف", "إلى", "البيان", "طريقة الدفع", "المبلغ" };
                    float[] colWidths = { 80, 100, 80, 100, 150, 90, 100 };
                    float xPos = rightMargin;
                    
                    for (int i = 0; i < headers.Length; i++)
                    {
                        xPos -= colWidths[i];
                        ev.Graphics.DrawString(headers[i], headerFont, Brushes.White,
                            new RectangleF(xPos, yPos, colWidths[i], 25),
                            new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                        ev.Graphics.FillRectangle(new SolidBrush(ColorScheme.Error),
                            xPos, yPos, colWidths[i], 25);
                        ev.Graphics.DrawString(headers[i], headerFont, Brushes.White,
                            new RectangleF(xPos, yPos, colWidths[i], 25),
                            new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                    }
                    yPos += 30;
                    
                    // رسم البيانات
                    int rowsOnPage = 0;
                    while (currentRow < _filteredTransactions.Count && rowsOnPage < rowsPerPage)
                    {
                        var t = _filteredTransactions[currentRow];
                        xPos = rightMargin;
                        
                        string[] rowData = {
                            t.VoucherNumber ?? "---",
                            t.TransactionDate.ToString("dd/MM/yyyy"),
                            t.Category ?? "غير محدد",
                            t.PartyName ?? "---",
                            t.Description ?? "---",
                            GetPaymentMethodName(t.PaymentMethod),
                            $"{t.Amount:N2} {GetCurrencyName(t.TransactionCurrency)}"
                        };
                        
                        for (int i = 0; i < rowData.Length; i++)
                        {
                            xPos -= colWidths[i];
                            ev.Graphics.DrawString(rowData[i], dataFont, Brushes.Black,
                                new RectangleF(xPos, yPos, colWidths[i], 20),
                                new StringFormat { Alignment = StringAlignment.Center });
                        }
                        
                        yPos += 22;
                        currentRow++;
                        rowsOnPage++;
                        
                        // رسم خط تحت كل صف
                        ev.Graphics.DrawLine(Pens.LightGray, leftMargin, yPos, rightMargin, yPos);
                        yPos += 3;
                    }
                    
                    // التحقق من وجود صفحات إضافية
                    ev.HasMorePages = currentRow < _filteredTransactions.Count;
                    
                    // رسم رقم الصفحة
                    if (!ev.HasMorePages)
                    {
                        yPos = ev.MarginBounds.Bottom + 20;
                        string pageInfo = $"تاريخ الطباعة: {DateTime.Now:dd/MM/yyyy hh:mm tt}";
                        ev.Graphics.DrawString(pageInfo, dataFont, Brushes.Gray,
                            leftMargin, yPos);
                    }
                };
                
                // عرض معاينة الطباعة
                var previewDialog = new PrintPreviewDialog
                {
                    Document = printDoc,
                    Width = 1000,
                    Height = 700,
                    StartPosition = FormStartPosition.CenterParent,
                    RightToLeft = RightToLeft.Yes,
                    RightToLeftLayout = false,
                    Text = "معاينة الطباعة - Print Preview"
                };
                
                previewDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في الطباعة:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
