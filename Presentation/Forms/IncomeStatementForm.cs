using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class IncomeStatementForm : Form
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IExportService _exportService;
    private DateTimePicker dtpFrom = null!;
    private DateTimePicker dtpTo = null!;
    private DataGridView dgvIncomeStatement = null!;
    private Button btnGenerate = null!;
    private Button btnExportExcel = null!;
    private Button btnExportPdf = null!;
    private Button btnPrint = null!;
    private Panel totalsPanel = null!;

    // For printing
    private int currentRow = 0;
    private bool hasMorePages = false;

    public IncomeStatementForm(IDbContextFactory<AppDbContext> dbContextFactory, IExportService exportService)
    {
        _dbContextFactory = dbContextFactory;
        _exportService = exportService;
        InitializeComponent();
        InitializeCustomComponents();
    }

    private void InitializeCustomComponents()
    {
        this.Text = "📊 قائمة الدخل";
        this.Size = new Size(1300, 900);
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;

        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(30),
            AutoScroll = true
        };

        Label lblTitle = new Label
        {
            Text = "📊 قائمة الدخل مفصّلة بالعملات (Income Statement by Currency)",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(30, 20)
        };

        // Filter Panel
        Panel filterPanel = new Panel
        {
            Size = new Size(1240, 60),
            Location = new Point(30, 60),
            BackColor = ColorScheme.Background,
            BorderStyle = BorderStyle.FixedSingle
        };

        Label lblFrom = new Label { Text = "من:", Font = new Font("Cairo", 11F), Location = new Point(1160, 15), AutoSize = true };
        dtpFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(990, 12), Size = new Size(160, 30), Font = new Font("Cairo", 10F), Value = new DateTime(DateTime.Today.Year, 1, 1) };
        Label lblTo = new Label { Text = "إلى:", Font = new Font("Cairo", 11F), Location = new Point(950, 15), AutoSize = true };
        dtpTo = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(780, 12), Size = new Size(160, 30), Font = new Font("Cairo", 10F), Value = DateTime.Today };

        btnGenerate = CreateButton("📊 إنشاء التقرير", ColorScheme.Primary, new Point(620, 11), new Size(150, 38));
        btnGenerate.Click += BtnGenerate_Click;

        btnExportExcel = CreateButton("📥 Excel", Color.FromArgb(46, 125, 50), new Point(460, 11), new Size(150, 38));
        btnExportExcel.Click += BtnExportExcel_Click;
        btnExportExcel.Enabled = false;

        btnExportPdf = CreateButton("📄 PDF", Color.FromArgb(211, 47, 47), new Point(300, 11), new Size(150, 38));
        btnExportPdf.Click += BtnExportPdf_Click;
        btnExportPdf.Enabled = false;

        btnPrint = CreateButton("🖨️ طباعة", ColorScheme.Warning, new Point(140, 11), new Size(150, 38));
        btnPrint.Click += BtnPrint_Click;
        btnPrint.Enabled = false;

        filterPanel.Controls.AddRange(new Control[] { lblFrom, dtpFrom, lblTo, dtpTo, btnGenerate, btnExportExcel, btnExportPdf, btnPrint });

        dgvIncomeStatement = new DataGridView
        {
            Location = new Point(30, 130),
            Size = new Size(1240, 680),
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            Font = new Font("Cairo", 10F),
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 45,
            RowTemplate = { Height = 36 },
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            ScrollBars = ScrollBars.Both
        };
        dgvIncomeStatement.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Primary;
        dgvIncomeStatement.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvIncomeStatement.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        dgvIncomeStatement.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

        totalsPanel = new Panel
        {
            Location = new Point(30, 820),
            Size = new Size(1240, 50),
            BackColor = ColorScheme.Background,
            BorderStyle = BorderStyle.FixedSingle
        };

        mainPanel.Controls.AddRange(new Control[] { lblTitle, filterPanel, dgvIncomeStatement, totalsPanel });
        this.Controls.Add(mainPanel);
    }

    private Button CreateButton(string text, Color backColor, Point location, Size size)
    {
        var btn = new Button
        {
            Text = text,
            Location = location,
            Size = size,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    // ========== Data model for currency-grouped results ==========
    private class CurrencyGroup
    {
        public string CurrencyCode { get; set; } = "";
        public string CurrencyName { get; set; } = "";
        public string Symbol      { get; set; } = "";
        // Revenues
        public decimal SalesRevenue       { get; set; }
        public int     SalesCount         { get; set; }
        public decimal CashIncomeOriginal { get; set; }
        public int     CashIncomeCount    { get; set; }
        public decimal JournalRevenue     { get; set; }
        // Expenses
        public decimal PurchaseExpense    { get; set; }
        public int     PurchaseCount      { get; set; }
        public decimal CashExpenseOriginal{ get; set; }
        public int     CashExpenseCount   { get; set; }
        public decimal JournalExpense     { get; set; }
        // Totals
        public decimal TotalRevenue  => SalesRevenue + CashIncomeOriginal + JournalRevenue;
        public decimal TotalExpenses => PurchaseExpense + CashExpenseOriginal + JournalExpense;
        public decimal NetIncome     => TotalRevenue - TotalExpenses;
    }

    private async void BtnGenerate_Click(object? sender, EventArgs e)
    {
        try
        {
            btnGenerate.Enabled = false;
            btnGenerate.Text = "⏳ جاري التحميل...";

            var fromDate = DateTime.SpecifyKind(dtpFrom.Value.Date, DateTimeKind.Utc);
            var toDate   = DateTime.SpecifyKind(dtpTo.Value.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);

            using var db = _dbContextFactory.CreateDbContext();

            // ====== جلب العملات ======
            var currencies = await db.Set<Currency>().Where(c => c.IsActive).ToListAsync();
            var currencyMap = currencies.ToDictionary(c => c.CurrencyId, c => c);
            var baseCurrency = currencies.FirstOrDefault(c => c.IsBaseCurrency) ?? currencies.FirstOrDefault(c => c.Code == "EGP");

            // ====== فواتير المبيعات ======
            var salesInvoices = await db.Set<SalesInvoice>()
                .Where(s => s.InvoiceDate >= fromDate && s.InvoiceDate <= toDate)
                .ToListAsync();

            // ====== فواتير المشتريات ======
            var purchaseInvoices = await db.Set<PurchaseInvoice>()
                .Where(p => p.InvoiceDate >= fromDate && p.InvoiceDate <= toDate)
                .ToListAsync();

            // ====== حركات الخزنة ======
            var cashIncomes = await db.Set<CashTransaction>()
                .Where(t => t.TransactionDate >= fromDate && t.TransactionDate <= toDate && t.Type == TransactionType.Income && !t.IsDeleted)
                .ToListAsync();

            var cashExpenses = await db.Set<CashTransaction>()
                .Where(t => t.TransactionDate >= fromDate && t.TransactionDate <= toDate && t.Type == TransactionType.Expense && !t.IsDeleted)
                .ToListAsync();

            // ====== القيود اليومية ======
            var journalLines = await db.Set<JournalEntryLine>()
                .Include(l => l.Account)
                .Include(l => l.JournalEntry)
                .Where(l => l.JournalEntry!.EntryDate >= fromDate && l.JournalEntry.EntryDate <= toDate && l.JournalEntry.IsPosted)
                .ToListAsync();

            // ====== تجميع البيانات حسب العملة ======
            var groups = new Dictionary<string, CurrencyGroup>();

            CurrencyGroup GetOrCreate(string code, string name = "", string symbol = "")
            {
                if (!groups.ContainsKey(code))
                    groups[code] = new CurrencyGroup { CurrencyCode = code, CurrencyName = name, Symbol = symbol };
                return groups[code];
            }

            // فواتير مبيعات - التجميع حسب العملة
            foreach (var grp in salesInvoices.GroupBy(s => s.CurrencyId))
            {
                Currency? cur = grp.Key.HasValue && currencyMap.ContainsKey(grp.Key.Value) ? currencyMap[grp.Key.Value] : baseCurrency;
                string code   = cur?.Code   ?? "EGP";
                string name   = cur?.Name   ?? "جنيه مصري";
                string symbol = cur?.Symbol ?? "ج.م";
                var g = GetOrCreate(code, name, symbol);
                g.SalesRevenue += grp.Sum(s => s.TotalAmount);
                g.SalesCount   += grp.Count();
            }

            // فواتير مشتريات - التجميع حسب العملة
            foreach (var grp in purchaseInvoices.GroupBy(p => p.CurrencyId))
            {
                Currency? cur = grp.Key.HasValue && currencyMap.ContainsKey(grp.Key.Value) ? currencyMap[grp.Key.Value] : baseCurrency;
                string code   = cur?.Code   ?? "EGP";
                string name   = cur?.Name   ?? "جنيه مصري";
                string symbol = cur?.Symbol ?? "ج.م";
                var g = GetOrCreate(code, name, symbol);
                g.PurchaseExpense += grp.Sum(p => p.TotalAmount);
                g.PurchaseCount   += grp.Count();
            }

            // حركات الخزنة - الإيرادات - التجميع حسب TransactionCurrency
            foreach (var grp in cashIncomes.GroupBy(t => t.TransactionCurrency ?? "EGP"))
            {
                string code = grp.Key;
                var cur = currencies.FirstOrDefault(c => c.Code == code);
                var g = GetOrCreate(code, cur?.Name ?? code, cur?.Symbol ?? code);
                // نستخدم OriginalAmount لو موجود (بالعملة الأصلية) وإلا Amount
                g.CashIncomeOriginal += grp.Sum(t => t.OriginalAmount ?? t.Amount);
                g.CashIncomeCount    += grp.Count();
            }

            // حركات الخزنة - المصروفات - التجميع حسب TransactionCurrency
            foreach (var grp in cashExpenses.GroupBy(t => t.TransactionCurrency ?? "EGP"))
            {
                string code = grp.Key;
                var cur = currencies.FirstOrDefault(c => c.Code == code);
                var g = GetOrCreate(code, cur?.Name ?? code, cur?.Symbol ?? code);
                g.CashExpenseOriginal += grp.Sum(t => t.OriginalAmount ?? t.Amount);
                g.CashExpenseCount    += grp.Count();
            }

            // القيود اليومية - إيرادات حسابات 4xxxx  (لا عملة محددة - تُضاف للعملة الأساسية)
            var baseCode = baseCurrency?.Code ?? "EGP";
            var revenueLines = journalLines.Where(l => l.Account?.AccountCode?.StartsWith("4") == true).ToList();
            if (revenueLines.Any())
            {
                var g = GetOrCreate(baseCode, baseCurrency?.Name ?? "جنيه مصري", baseCurrency?.Symbol ?? "ج.م");
                g.JournalRevenue += revenueLines.Sum(l => l.CreditAmount) - revenueLines.Sum(l => l.DebitAmount);
            }

            // القيود اليومية - مصروفات حسابات 5xxxx
            var expenseLines = journalLines.Where(l => l.Account?.AccountCode?.StartsWith("5") == true).ToList();
            if (expenseLines.Any())
            {
                var g = GetOrCreate(baseCode, baseCurrency?.Name ?? "جنيه مصري", baseCurrency?.Symbol ?? "ج.م");
                g.JournalExpense += expenseLines.Sum(l => l.DebitAmount) - expenseLines.Sum(l => l.CreditAmount);
            }

            // ====== بناء الجدول ======
            BuildGrid(groups.Values.OrderByDescending(g => g.TotalRevenue + g.TotalExpenses).ToList(), fromDate, toDate);

            btnExportExcel.Enabled = true;
            btnExportPdf.Enabled   = true;
            btnPrint.Enabled       = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ خطأ في إنشاء قائمة الدخل:\n\n{ex.Message}",
                "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
        finally
        {
            btnGenerate.Enabled = true;
            btnGenerate.Text    = "📊 إنشاء التقرير";
        }
    }

    private void BuildGrid(List<CurrencyGroup> groups, DateTime fromDate, DateTime toDate)
    {
        dgvIncomeStatement.Columns.Clear();
        dgvIncomeStatement.Rows.Clear();

        // أعمدة: البند | المبلغ | العملة
        var colItem = new DataGridViewTextBoxColumn { Name = "Item", HeaderText = "البند / التفاصيل", Width = 500 };
        var colAmt  = new DataGridViewTextBoxColumn { Name = "Amount", HeaderText = "المبلغ", Width = 220,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleLeft, Format = "N2" } };
        var colCur  = new DataGridViewTextBoxColumn { Name = "Currency", HeaderText = "العملة", Width = 120,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } };
        dgvIncomeStatement.Columns.AddRange(colItem, colAmt, colCur);

        Color headerBg  = ColorScheme.Primary;
        Color revBg     = Color.FromArgb(200, 230, 201);
        Color expBg     = Color.FromArgb(255, 205, 210);
        Color sectionBg = Color.FromArgb(235, 245, 251);
        Color netGreenBg= Color.FromArgb(27, 94, 32);
        Color netRedBg  = Color.FromArgb(183, 28, 28);

        void AddSectionHeader(string text)
        {
            dgvIncomeStatement.Rows.Add(text, "", "");
            var r = dgvIncomeStatement.Rows[dgvIncomeStatement.Rows.Count - 1];
            r.DefaultCellStyle.BackColor = headerBg;
            r.DefaultCellStyle.ForeColor = Color.White;
            r.DefaultCellStyle.Font = new Font("Cairo", 12F, FontStyle.Bold);
        }

        void AddDetailRow(string item, decimal amount, string currency, Color? bg = null)
        {
            dgvIncomeStatement.Rows.Add(item, amount, currency);
            var r = dgvIncomeStatement.Rows[dgvIncomeStatement.Rows.Count - 1];
            if (bg.HasValue) { r.DefaultCellStyle.BackColor = bg.Value; r.DefaultCellStyle.ForeColor = Color.Black; }
        }

        void AddTotalRow(string item, decimal amount, string currency, Color bg, Color fg, float fontSize = 11F)
        {
            dgvIncomeStatement.Rows.Add(item, amount, currency);
            var r = dgvIncomeStatement.Rows[dgvIncomeStatement.Rows.Count - 1];
            r.DefaultCellStyle.BackColor = bg;
            r.DefaultCellStyle.ForeColor = fg;
            r.DefaultCellStyle.Font = new Font("Cairo", fontSize, FontStyle.Bold);
        }

        void AddSeparator()
        {
            dgvIncomeStatement.Rows.Add("", "", "");
        }

        if (!groups.Any())
        {
            AddSectionHeader("⚠️ لا توجد حركات مالية في الفترة المحددة");
            return;
        }

        // ====== لكل عملة - قسم مستقل ======
        foreach (var g in groups)
        {
            string sym = string.IsNullOrWhiteSpace(g.Symbol) ? g.CurrencyCode : $"{g.Symbol} ({g.CurrencyCode})";

            // عنوان العملة
            dgvIncomeStatement.Rows.Add($"═══  {g.CurrencyName}  ({g.CurrencyCode})  ═══", "", "");
            var headerRow = dgvIncomeStatement.Rows[dgvIncomeStatement.Rows.Count - 1];
            headerRow.DefaultCellStyle.BackColor = Color.FromArgb(13, 71, 161);
            headerRow.DefaultCellStyle.ForeColor = Color.White;
            headerRow.DefaultCellStyle.Font = new Font("Cairo", 13F, FontStyle.Bold);

            // ----- قسم الإيرادات -----
            AddSectionHeader("  💰 الإيرادات");

            if (g.SalesRevenue != 0)
                AddDetailRow($"    📑 فواتير المبيعات ({g.SalesCount} فاتورة)", g.SalesRevenue, sym, sectionBg);
            if (g.CashIncomeOriginal != 0)
                AddDetailRow($"    💵 إيرادات الخزنة ({g.CashIncomeCount} حركة)", g.CashIncomeOriginal, sym, sectionBg);
            if (g.JournalRevenue != 0)
                AddDetailRow("    📒 قيود يومية - إيرادات", g.JournalRevenue, sym, sectionBg);

            AddTotalRow("  إجمالي الإيرادات", g.TotalRevenue, sym, revBg, ColorScheme.Success);

            AddSeparator();

            // ----- قسم المصروفات -----
            AddSectionHeader("  💸 المصروفات");

            if (g.PurchaseExpense != 0)
                AddDetailRow($"    📑 فواتير المشتريات ({g.PurchaseCount} فاتورة)", g.PurchaseExpense, sym, sectionBg);
            if (g.CashExpenseOriginal != 0)
                AddDetailRow($"    💸 مصروفات الخزنة ({g.CashExpenseCount} حركة)", g.CashExpenseOriginal, sym, sectionBg);
            if (g.JournalExpense != 0)
                AddDetailRow("    📒 قيود يومية - مصروفات", g.JournalExpense, sym, sectionBg);

            AddTotalRow("  إجمالي المصروفات", g.TotalExpenses, sym, expBg, ColorScheme.Error);

            AddSeparator();

            // ----- صافي الربح/الخسارة لهذه العملة -----
            bool profit = g.NetIncome >= 0;
            string netLabel = profit ? $"  ✅ صافي الربح" : $"  ❌ صافي الخسارة";
            AddTotalRow(netLabel, Math.Abs(g.NetIncome), sym,
                profit ? netGreenBg : netRedBg, Color.White, 12F);

            AddSeparator();
            AddSeparator();
        }

        // ====== ملخص إجمالي (تحديث اللوحة السفلية) ======
        totalsPanel.Controls.Clear();

        int x = 20;
        foreach (var g in groups)
        {
            string sym = g.Symbol ?? g.CurrencyCode;
            bool profit = g.NetIncome >= 0;
            var lbl = new Label
            {
                Text = $"{g.CurrencyCode}: إيرادات {g.TotalRevenue:N2} {sym}  |  مصروفات {g.TotalExpenses:N2} {sym}  |  صافي {(profit ? "+" : "-")}{Math.Abs(g.NetIncome):N2} {sym}",
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                ForeColor = profit ? ColorScheme.Success : ColorScheme.Error,
                AutoSize = true,
                Location = new Point(x, 12)
            };
            totalsPanel.Controls.Add(lbl);
            x = 20;
            // stack vertically if multiple currencies
            totalsPanel.Height = Math.Max(50, (groups.Count * 30) + 20);
            lbl.Location = new Point(20, (groups.IndexOf(g) * 28) + 10);
        }
    }

    private async void BtnExportExcel_Click(object? sender, EventArgs e)
    {
        try
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title  = "حفظ قائمة الدخل",
                FileName = $"قائمة_الدخل_{DateTime.Now:yyyyMMdd}.xlsx"
            };
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                bool ok = await _exportService.ExportToExcelAsync(dgvIncomeStatement, saveDialog.FileName, "قائمة الدخل");
                if (ok) MessageBox.Show("تم التصدير بنجاح!", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في التصدير: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private async void BtnExportPdf_Click(object? sender, EventArgs e)
    {
        try
        {
            var saveDialog = new SaveFileDialog
            {
                Filter   = "HTML Files|*.html",
                Title    = "حفظ قائمة الدخل",
                FileName = $"قائمة_الدخل_{DateTime.Now:yyyyMMdd}.html"
            };
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                var meta = new Dictionary<string, string>
                {
                    { "الفترة", $"من {dtpFrom.Value:yyyy/MM/dd} إلى {dtpTo.Value:yyyy/MM/dd}" }
                };
                await _exportService.ExportToPdfAsync(dgvIncomeStatement, saveDialog.FileName, "📊 قائمة الدخل", meta);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في التصدير: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private void BtnPrint_Click(object? sender, EventArgs e)
    {
        try
        {
            if (dgvIncomeStatement.Rows.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات للطباعة", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                return;
            }
            var printDoc = new PrintDocument();
            printDoc.PrintPage += PrintDocument_PrintPage;
            var dlg = new PrintDialog { Document = printDoc };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                currentRow = 0;
                hasMorePages = false;
                printDoc.Print();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في الطباعة: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
    {
        var g = e.Graphics!;
        var titleFont  = new Font("Cairo", 14, FontStyle.Bold);
        var headerFont = new Font("Cairo", 11, FontStyle.Bold);
        var normalFont = new Font("Cairo", 10);
        var smallFont  = new Font("Cairo", 8);

        float y = e.MarginBounds.Top;
        float pageWidth = e.MarginBounds.Width;
        float x = e.MarginBounds.Left;

        if (currentRow == 0)
        {
            string title = $"📊 قائمة الدخل مفصّلة بالعملات | من {dtpFrom.Value:yyyy/MM/dd} إلى {dtpTo.Value:yyyy/MM/dd}";
            var sz = g.MeasureString(title, titleFont);
            g.DrawString(title, titleFont, Brushes.Black, x + (pageWidth - sz.Width) / 2, y);
            y += sz.Height + 15;
        }

        float col1W = pageWidth * 0.55f;
        float col2W = pageWidth * 0.25f;
        float col3W = pageWidth * 0.20f;
        float col1X = x + pageWidth - col1W;
        float col2X = x + col3W;
        float col3X = x;

        if (currentRow == 0)
        {
            g.FillRectangle(new SolidBrush(ColorScheme.Primary), x, y, pageWidth, 28);
            g.DrawString("البند", headerFont, Brushes.White, col1X + 5, y + 5);
            g.DrawString("المبلغ", headerFont, Brushes.White, col2X + 5, y + 5);
            g.DrawString("العملة", headerFont, Brushes.White, col3X + 5, y + 5);
            y += 32;
        }

        int rowsPerPage = (int)((e.MarginBounds.Bottom - y - 60) / 26);
        int printed = 0;

        for (int i = currentRow; i < dgvIncomeStatement.Rows.Count && printed < rowsPerPage; i++)
        {
            var row = dgvIncomeStatement.Rows[i];
            Color bg   = row.DefaultCellStyle.BackColor != Color.Empty ? row.DefaultCellStyle.BackColor : (i % 2 == 0 ? Color.White : Color.FromArgb(245, 245, 245));
            Color fg   = row.DefaultCellStyle.ForeColor != Color.Empty ? row.DefaultCellStyle.ForeColor : Color.Black;
            Font  font = row.DefaultCellStyle.Font ?? normalFont;

            g.FillRectangle(new SolidBrush(bg), x, y, pageWidth, 26);
            g.DrawString(row.Cells[0].Value?.ToString() ?? "", font, new SolidBrush(fg), col1X + 5, y + 4);
            if (row.Cells[1].Value is decimal d)
                g.DrawString(d.ToString("N2"), font, new SolidBrush(fg), col2X + 5, y + 4);
            g.DrawString(row.Cells[2].Value?.ToString() ?? "", font, new SolidBrush(fg), col3X + 5, y + 4);
            y += 26; printed++; currentRow++;
        }

        e.HasMorePages = hasMorePages = (currentRow < dgvIncomeStatement.Rows.Count);

        string pageInfo = $"تاريخ الطباعة: {DateTime.Now:yyyy/MM/dd HH:mm}";
        g.DrawString(pageInfo, smallFont, Brushes.Gray, x, e.MarginBounds.Bottom - 18);
    }
}
