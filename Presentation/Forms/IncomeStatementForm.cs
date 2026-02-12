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
    private Label lblTotalRevenue = null!;
    private Label lblTotalExpenses = null!;
    private Label lblNetIncome = null!;
    
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
        this.Size = new Size(1200, 800);
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;

        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(30)
        };

        Label lblTitle = new Label
        {
            Text = "📊 قائمة الدخل (Income Statement)",
            Font = new Font("Cairo", 18F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(30, 20)
        };

        // Filter Panel
        Panel filterPanel = new Panel
        {
            Size = new Size(1140, 100),
            Location = new Point(30, 70),
            BackColor = ColorScheme.Background,
            BorderStyle = BorderStyle.FixedSingle
        };

        // Date Filters Row
        Label lblFrom = new Label
        {
            Text = "من تاريخ:",
            Font = new Font("Cairo", 11F),
            Location = new Point(1030, 15),
            AutoSize = true
        };
        dtpFrom = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Location = new Point(870, 12),
            Size = new Size(150, 30),
            Font = new Font("Cairo", 10F),
            Value = new DateTime(DateTime.Today.Year, 1, 1)
        };

        Label lblTo = new Label
        {
            Text = "إلى تاريخ:",
            Font = new Font("Cairo", 11F),
            Location = new Point(790, 15),
            AutoSize = true
        };
        dtpTo = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Location = new Point(630, 12),
            Size = new Size(150, 30),
            Font = new Font("Cairo", 10F),
            Value = DateTime.Today
        };

        // Action Buttons Row - Aligned and Equal Width
        int buttonY = 52;
        int buttonWidth = 120;
        int buttonSpacing = 15;
        int startX = 1140 - 40 - (4 * buttonWidth) - (3 * buttonSpacing);

        btnGenerate = new Button
        {
            Text = "📊 إنشاء التقرير",
            Location = new Point(startX + (3 * (buttonWidth + buttonSpacing)), buttonY),
            Size = new Size(buttonWidth, 38),
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnGenerate.FlatAppearance.BorderSize = 0;
        btnGenerate.Click += BtnGenerate_Click;

        btnExportExcel = new Button
        {
            Text = "📥 Excel",
            Location = new Point(startX + (2 * (buttonWidth + buttonSpacing)), buttonY),
            Size = new Size(buttonWidth, 38),
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            BackColor = Color.FromArgb(46, 125, 50),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnExportExcel.FlatAppearance.BorderSize = 0;
        btnExportExcel.Click += BtnExportExcel_Click;

        btnExportPdf = new Button
        {
            Text = "📄 PDF",
            Location = new Point(startX + (buttonWidth + buttonSpacing), buttonY),
            Size = new Size(buttonWidth, 38),
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            BackColor = Color.FromArgb(211, 47, 47),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnExportPdf.FlatAppearance.BorderSize = 0;
        btnExportPdf.Click += BtnExportPdf_Click;

        btnPrint = new Button
        {
            Text = "🖨️ طباعة",
            Location = new Point(startX, buttonY),
            Size = new Size(buttonWidth, 38),
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            BackColor = ColorScheme.Warning,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnPrint.FlatAppearance.BorderSize = 0;
        btnPrint.Click += BtnPrint_Click;

        filterPanel.Controls.AddRange(new Control[] {
            lblFrom, dtpFrom, lblTo, dtpTo, btnGenerate, btnExportExcel, btnExportPdf, btnPrint
        });

        dgvIncomeStatement = new DataGridView
        {
            Location = new Point(30, 180),
            Size = new Size(1140, 490),
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = new Font("Cairo", 10F),
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 40,
            RowTemplate = { Height = 35 }
        };

        dgvIncomeStatement.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Primary;
        dgvIncomeStatement.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvIncomeStatement.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        dgvIncomeStatement.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);

        Panel totalsPanel = new Panel
        {
            Location = new Point(30, 680),
            Size = new Size(1140, 80),
            BackColor = ColorScheme.Background,
            BorderStyle = BorderStyle.FixedSingle
        };

        lblTotalRevenue = new Label
        {
            Text = "إجمالي الإيرادات: 0.00",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = ColorScheme.Success,
            Location = new Point(750, 10),
            AutoSize = true
        };

        lblTotalExpenses = new Label
        {
            Text = "إجمالي المصروفات: 0.00",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = ColorScheme.Error,
            Location = new Point(400, 10),
            AutoSize = true
        };

        lblNetIncome = new Label
        {
            Text = "صافي الربح: 0.00",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            Location = new Point(100, 10),
            AutoSize = true
        };

        Label lblNetLabel = new Label
        {
            Text = "(الإيرادات - المصروفات)",
            Font = new Font("Cairo", 10F),
            ForeColor = Color.Gray,
            Location = new Point(100, 45),
            AutoSize = true
        };

        totalsPanel.Controls.AddRange(new Control[] { 
            lblTotalRevenue, lblTotalExpenses, lblNetIncome, lblNetLabel 
        });

        mainPanel.Controls.AddRange(new Control[] {
            lblTitle, filterPanel, dgvIncomeStatement, totalsPanel
        });

        this.Controls.Add(mainPanel);
    }

    private async void BtnGenerate_Click(object? sender, EventArgs e)
    {
        try
        {
            var fromDate = DateTime.SpecifyKind(dtpFrom.Value.Date, DateTimeKind.Utc);
            var toDate = DateTime.SpecifyKind(dtpTo.Value.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);

            using var _dbContext = _dbContextFactory.CreateDbContext();

            // ========== جمع البيانات من كل المصادر ==========
            
            // 1. فواتير المبيعات (الإيرادات)
            var salesInvoices = await _dbContext.Set<SalesInvoice>()
                .Where(s => s.InvoiceDate >= fromDate && s.InvoiceDate <= toDate)
                .ToListAsync();
            
            decimal salesRevenue = salesInvoices.Sum(s => s.TotalAmount);

            // 2. حركات الخزنة - الإيرادات
            var cashIncomes = await _dbContext.Set<CashTransaction>()
                .Where(t => t.TransactionDate >= fromDate && 
                           t.TransactionDate <= toDate &&
                           t.Type == TransactionType.Income &&
                           !t.IsDeleted)
                .ToListAsync();
            
            decimal cashIncomeTotal = cashIncomes.Sum(t => t.Amount);

            // 3. فواتير المشتريات (المصروفات)
            var purchaseInvoices = await _dbContext.Set<PurchaseInvoice>()
                .Where(p => p.InvoiceDate >= fromDate && p.InvoiceDate <= toDate)
                .ToListAsync();
            
            decimal purchaseExpense = purchaseInvoices.Sum(p => p.TotalAmount);

            // 4. حركات الخزنة - المصروفات
            var cashExpenses = await _dbContext.Set<CashTransaction>()
                .Where(t => t.TransactionDate >= fromDate && 
                           t.TransactionDate <= toDate &&
                           t.Type == TransactionType.Expense &&
                           !t.IsDeleted)
                .ToListAsync();
            
            decimal cashExpenseTotal = cashExpenses.Sum(t => t.Amount);

            // 5. حركات القيود اليومية
            var journalLines = await _dbContext.Set<JournalEntryLine>()
                .Include(l => l.Account)
                .Include(l => l.JournalEntry)
                .Where(l => l.JournalEntry!.EntryDate >= fromDate && 
                           l.JournalEntry.EntryDate <= toDate &&
                           l.JournalEntry.IsPosted)
                .ToListAsync();

            // حسابات الإيرادات (4xxxx)
            var revenueAccountLines = journalLines
                .Where(l => l.Account != null && l.Account.AccountCode.StartsWith("4"))
                .ToList();
            decimal journalRevenue = revenueAccountLines.Sum(l => l.CreditAmount) - 
                                   revenueAccountLines.Sum(l => l.DebitAmount);

            // حسابات المصروفات (5xxxx)
            var expenseAccountLines = journalLines
                .Where(l => l.Account != null && l.Account.AccountCode.StartsWith("5"))
                .ToList();
            decimal journalExpense = expenseAccountLines.Sum(l => l.DebitAmount) - 
                                    expenseAccountLines.Sum(l => l.CreditAmount);

            // ========== حساب الإجماليات ==========
            decimal totalRevenue = salesRevenue + cashIncomeTotal + journalRevenue;
            decimal totalExpenses = purchaseExpense + cashExpenseTotal + journalExpense;
            decimal netIncome = totalRevenue - totalExpenses;

            // ========== عرض النتائج ==========
            dgvIncomeStatement.Columns.Clear();
            dgvIncomeStatement.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Category",
                HeaderText = "البند",
                Width = 400
            });
            dgvIncomeStatement.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Amount",
                HeaderText = "المبلغ",
                Width = 200,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            // ========== قسم الإيرادات ==========
            dgvIncomeStatement.Rows.Add("💰 الإيرادات (Revenues)", "");
            dgvIncomeStatement.Rows[dgvIncomeStatement.Rows.Count - 1].DefaultCellStyle.BackColor = ColorScheme.Primary;
            dgvIncomeStatement.Rows[dgvIncomeStatement.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.White;
            dgvIncomeStatement.Rows[dgvIncomeStatement.Rows.Count - 1].DefaultCellStyle.Font = new Font("Cairo", 12F, FontStyle.Bold);

            if (salesRevenue > 0)
                dgvIncomeStatement.Rows.Add($"  📑 فواتير المبيعات ({salesInvoices.Count} فاتورة)", salesRevenue);
            
            if (cashIncomeTotal > 0)
                dgvIncomeStatement.Rows.Add($"  💵 إيرادات الخزنة ({cashIncomes.Count} حركة)", cashIncomeTotal);
            
            if (journalRevenue > 0)
                dgvIncomeStatement.Rows.Add($"  📒 قيود يومية - إيرادات", journalRevenue);

            // إجمالي الإيرادات
            dgvIncomeStatement.Rows.Add("إجمالي الإيرادات", totalRevenue);
            dgvIncomeStatement.Rows[dgvIncomeStatement.Rows.Count - 1].DefaultCellStyle.BackColor = Color.FromArgb(200, 230, 201);
            dgvIncomeStatement.Rows[dgvIncomeStatement.Rows.Count - 1].DefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
            dgvIncomeStatement.Rows[dgvIncomeStatement.Rows.Count - 1].DefaultCellStyle.ForeColor = ColorScheme.Success;

            // فاصل
            dgvIncomeStatement.Rows.Add("", "");

            // ========== قسم المصروفات ==========
            dgvIncomeStatement.Rows.Add("💸 المصروفات (Expenses)", "");
            dgvIncomeStatement.Rows[dgvIncomeStatement.Rows.Count - 1].DefaultCellStyle.BackColor = ColorScheme.Primary;
            dgvIncomeStatement.Rows[dgvIncomeStatement.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.White;
            dgvIncomeStatement.Rows[dgvIncomeStatement.Rows.Count - 1].DefaultCellStyle.Font = new Font("Cairo", 12F, FontStyle.Bold);

            if (purchaseExpense > 0)
                dgvIncomeStatement.Rows.Add($"  📑 فواتير المشتريات ({purchaseInvoices.Count} فاتورة)", purchaseExpense);
            
            if (cashExpenseTotal > 0)
                dgvIncomeStatement.Rows.Add($"  💸 مصروفات الخزنة ({cashExpenses.Count} حركة)", cashExpenseTotal);
            
            if (journalExpense > 0)
                dgvIncomeStatement.Rows.Add($"  📒 قيود يومية - مصروفات", journalExpense);

            // إجمالي المصروفات
            dgvIncomeStatement.Rows.Add("إجمالي المصروفات", totalExpenses);
            dgvIncomeStatement.Rows[dgvIncomeStatement.Rows.Count - 1].DefaultCellStyle.BackColor = Color.FromArgb(255, 205, 210);
            dgvIncomeStatement.Rows[dgvIncomeStatement.Rows.Count - 1].DefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
            dgvIncomeStatement.Rows[dgvIncomeStatement.Rows.Count - 1].DefaultCellStyle.ForeColor = ColorScheme.Error;

            // فاصل
            dgvIncomeStatement.Rows.Add("", "");
            dgvIncomeStatement.Rows.Add("", "");

            // ========== صافي الربح/الخسارة ==========
            string profitLabel = netIncome >= 0 ? "✅ صافي الربح (Net Profit)" : "❌ صافي الخسارة (Net Loss)";
            dgvIncomeStatement.Rows.Add(profitLabel, Math.Abs(netIncome));
            dgvIncomeStatement.Rows[dgvIncomeStatement.Rows.Count - 1].DefaultCellStyle.BackColor = netIncome >= 0 ? ColorScheme.Success : ColorScheme.Error;
            dgvIncomeStatement.Rows[dgvIncomeStatement.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.White;
            dgvIncomeStatement.Rows[dgvIncomeStatement.Rows.Count - 1].DefaultCellStyle.Font = new Font("Cairo", 14F, FontStyle.Bold);

            // ========== تحديث التسميات ==========
            lblTotalRevenue.Text = $"إجمالي الإيرادات: {totalRevenue:N2} جنيه";
            lblTotalExpenses.Text = $"إجمالي المصروفات: {totalExpenses:N2} جنيه";
            
            if (netIncome >= 0)
            {
                lblNetIncome.Text = $"صافي الربح: {netIncome:N2} جنيه";
                lblNetIncome.ForeColor = ColorScheme.Success;
            }
            else
            {
                lblNetIncome.Text = $"صافي الخسارة: {Math.Abs(netIncome):N2} جنيه";
                lblNetIncome.ForeColor = ColorScheme.Error;
            }

            // تمكين الأزرار
            btnExportExcel.Enabled = true;
            btnExportPdf.Enabled = true;
            btnPrint.Enabled = true;

            // رسالة نجاح
            if (totalRevenue == 0 && totalExpenses == 0)
            {
                MessageBox.Show(
                    "⚠️ تنبيه: لا توجد حركات مالية في الفترة المحددة!\n\n" +
                    $"الفترة: من {fromDate:dd/MM/yyyy} إلى {toDate:dd/MM/yyyy}\n\n" +
                    "تأكد من وجود:\n" +
                    "• فواتير مبيعات\n" +
                    "• فواتير مشتريات\n" +
                    "• حركات خزنة (إيرادات/مصروفات)\n" +
                    "• قيود يومية معتمدة",
                    "لا توجد بيانات",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                );
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"❌ خطأ في إنشاء قائمة الدخل:\n\n{ex.Message}\n\n{ex.StackTrace}",
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
            );
        }
    }

    private async void BtnExportExcel_Click(object? sender, EventArgs e)
    {
        try
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "حفظ قائمة الدخل",
                FileName = $"قائمة_الدخل_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                bool success = await _exportService.ExportToExcelAsync(
                    dgvIncomeStatement, 
                    saveDialog.FileName, 
                    "قائمة الدخل"
                );

                if (success)
                {
                    MessageBox.Show("تم تصدير التقرير بنجاح!", "نجح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في التصدير: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private async void BtnExportPdf_Click(object? sender, EventArgs e)
    {
        try
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "HTML Files|*.html",
                Title = "حفظ قائمة الدخل",
                FileName = $"قائمة_الدخل_{DateTime.Now:yyyyMMdd}.html"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                var metadata = new Dictionary<string, string>
                {
                    { "الفترة", $"من {dtpFrom.Value:yyyy/MM/dd} إلى {dtpTo.Value:yyyy/MM/dd}" },
                    { "إجمالي الإيرادات", lblTotalRevenue.Text },
                    { "إجمالي المصروفات", lblTotalExpenses.Text },
                    { "صافي الربح", lblNetIncome.Text }
                };

                bool success = await _exportService.ExportToPdfAsync(
                    dgvIncomeStatement,
                    saveDialog.FileName,
                    "📊 قائمة الدخل",
                    metadata
                );
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في التصدير: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private void BtnPrint_Click(object? sender, EventArgs e)
    {
        try
        {
            if (dgvIncomeStatement.Rows.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات للطباعة", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                return;
            }
            
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += PrintDocument_PrintPage;
            
            PrintDialog printDialog = new PrintDialog { Document = printDoc };

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                currentRow = 0;
                hasMorePages = false;
                printDoc.Print();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في الطباعة: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
    {
        try
        {
            Graphics g = e.Graphics!;
            Font titleFont = new Font("Cairo", 16, FontStyle.Bold);
            Font headerFont = new Font("Cairo", 12, FontStyle.Bold);
            Font normalFont = new Font("Cairo", 10);
            Font smallFont = new Font("Cairo", 9);

            float y = e.MarginBounds.Top;
            float x = e.MarginBounds.Left;
            float pageWidth = e.MarginBounds.Width;

            // Title
            string title = "📊 قائمة الدخل";
            SizeF titleSize = g.MeasureString(title, titleFont);
            g.DrawString(title, titleFont, Brushes.Black, x + (pageWidth - titleSize.Width) / 2, y);
            y += titleSize.Height + 10;

            // Date range
            string dateRange = $"من {dtpFrom.Value:yyyy/MM/dd} إلى {dtpTo.Value:yyyy/MM/dd}";
            SizeF dateSize = g.MeasureString(dateRange, normalFont);
            g.DrawString(dateRange, normalFont, Brushes.Black, x + (pageWidth - dateSize.Width) / 2, y);
            y += dateSize.Height + 20;

            // Column headers
            float col1Width = pageWidth * 0.65f; // Account
            float col2Width = pageWidth * 0.35f; // Amount

            float col1X = x + pageWidth - col1Width;
            float col2X = x;

            // Draw header background
            g.FillRectangle(new SolidBrush(ColorScheme.Primary), x, y, pageWidth, 30);
            
            // Draw headers
            g.DrawString("البند", headerFont, Brushes.White, col1X + 10, y + 5);
            g.DrawString("المبلغ", headerFont, Brushes.White, col2X + 10, y + 5);
            
            y += 35;

            // Draw rows
            int rowsPerPage = (int)((e.MarginBounds.Bottom - y - 120) / 25);
            int rowsPrinted = 0;

            for (int i = currentRow; i < dgvIncomeStatement.Rows.Count && rowsPrinted < rowsPerPage; i++)
            {
                DataGridViewRow row = dgvIncomeStatement.Rows[i];
                
                // Apply row styling
                Color bgColor = Color.White;
                Font rowFont = normalFont;
                Color textColor = Color.Black;
                
                if (row.DefaultCellStyle.BackColor != Color.Empty && 
                    row.DefaultCellStyle.BackColor != Color.White)
                {
                    bgColor = row.DefaultCellStyle.BackColor;
                }
                if (row.DefaultCellStyle.Font != null)
                {
                    rowFont = row.DefaultCellStyle.Font;
                }
                if (row.DefaultCellStyle.ForeColor != Color.Empty)
                {
                    textColor = row.DefaultCellStyle.ForeColor;
                }
                
                g.FillRectangle(new SolidBrush(bgColor), x, y, pageWidth, 25);

                string item = row.Cells[0].Value?.ToString() ?? "";
                string amount = row.Cells[1].Value?.ToString() ?? "";
                
                g.DrawString(item, rowFont, new SolidBrush(textColor), col1X + 10, y + 3);
                g.DrawString(amount, rowFont, new SolidBrush(textColor), col2X + 10, y + 3);

                y += 25;
                rowsPrinted++;
                currentRow++;
            }

            // Draw totals if last page
            if (currentRow >= dgvIncomeStatement.Rows.Count)
            {
                y += 10;
                g.DrawLine(new Pen(Color.Black, 2), x, y, x + pageWidth, y);
                y += 15;

                g.FillRectangle(new SolidBrush(ColorScheme.Background), x, y, pageWidth, 25);
                g.DrawString(lblTotalRevenue.Text, headerFont, new SolidBrush(ColorScheme.Success), x + 10, y + 3);
                y += 30;
                
                g.FillRectangle(new SolidBrush(ColorScheme.Background), x, y, pageWidth, 25);
                g.DrawString(lblTotalExpenses.Text, headerFont, new SolidBrush(ColorScheme.Error), x + 10, y + 3);
                y += 30;

                g.FillRectangle(new SolidBrush(ColorScheme.Background), x, y, pageWidth, 25);
                g.DrawString(lblNetIncome.Text, headerFont, new SolidBrush(lblNetIncome.ForeColor), x + 10, y + 3);

                hasMorePages = false;
            }
            else
            {
                hasMorePages = true;
            }

            // Page number
            string pageNum = $"صفحة {(currentRow / rowsPerPage) + 1}";
            SizeF pageNumSize = g.MeasureString(pageNum, smallFont);
            g.DrawString(pageNum, smallFont, Brushes.Black, 
                x + (pageWidth - pageNumSize.Width) / 2, e.MarginBounds.Bottom - 20);

            // Print date
            string printDate = $"تاريخ الطباعة: {DateTime.Now:yyyy/MM/dd HH:mm}";
            g.DrawString(printDate, smallFont, Brushes.Black, x, e.MarginBounds.Bottom - 20);

            e.HasMorePages = hasMorePages;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تنسيق الطباعة: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }
}
