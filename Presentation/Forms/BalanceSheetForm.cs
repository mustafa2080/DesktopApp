using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class BalanceSheetForm : Form
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private DateTimePicker dtpAsOf;
    private DataGridView dgvBalanceSheet;
    private Button btnGenerate;
    private Button btnExport;
    private Button btnPrint;
    private Label lblTotalAssets;
    private Label lblTotalLiabilities;
    private Label lblTotalEquity;
    private Label lblBalance;
    
    // For printing
    private int currentRow = 0;
    private bool hasMorePages = false;

    public BalanceSheetForm(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        InitializeComponent();
        InitializeCustomComponents();
    }

    private void InitializeCustomComponents()
    {
        this.Text = "💼 الميزانية العمومية (المركز المالي)";
        this.Size = new Size(1400, 900);
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
            Text = "💼 الميزانية العمومية (Balance Sheet)",
            Font = new Font("Cairo", 18F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(30, 20)
        };

        Panel filterPanel = new Panel
        {
            Size = new Size(1340, 80),
            Location = new Point(30, 70),
            BackColor = ColorScheme.Background,
            BorderStyle = BorderStyle.FixedSingle
        };

        Label lblAsOf = new Label
        {
            Text = "كما في تاريخ:",
            Font = new Font("Cairo", 11F),
            Location = new Point(1180, 20),
            AutoSize = true
        };
        dtpAsOf = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Location = new Point(1020, 17),
            Size = new Size(150, 30),
            Font = new Font("Cairo", 10F),
            Value = DateTime.Today
        };

        btnGenerate = new Button
        {
            Text = "📊 إنشاء التقرير",
            Location = new Point(820, 15),
            Size = new Size(150, 35),
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnGenerate.FlatAppearance.BorderSize = 0;
        btnGenerate.Click += BtnGenerate_Click;

        btnExport = new Button
        {
            Text = "📥 تصدير",
            Location = new Point(660, 15),
            Size = new Size(150, 35),
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnExport.FlatAppearance.BorderSize = 0;

        btnPrint = new Button
        {
            Text = "🖨️ طباعة",
            Location = new Point(500, 15),
            Size = new Size(150, 35),
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            BackColor = ColorScheme.Warning,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnPrint.FlatAppearance.BorderSize = 0;
        btnPrint.Click += BtnPrint_Click;

        filterPanel.Controls.AddRange(new Control[] {
            lblAsOf, dtpAsOf, btnGenerate, btnExport, btnPrint
        });

        dgvBalanceSheet = new DataGridView
        {
            Location = new Point(30, 160),
            Size = new Size(1340, 580),
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

        dgvBalanceSheet.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Primary;
        dgvBalanceSheet.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvBalanceSheet.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        dgvBalanceSheet.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);

        Panel totalsPanel = new Panel
        {
            Location = new Point(30, 750),
            Size = new Size(1340, 100),
            BackColor = ColorScheme.Background,
            BorderStyle = BorderStyle.FixedSingle
        };

        lblTotalAssets = new Label
        {
            Text = "إجمالي الأصول: 0.00",
            Font = new Font("Cairo", 13F, FontStyle.Bold),
            ForeColor = ColorScheme.Success,
            Location = new Point(950, 15),
            AutoSize = true
        };

        lblTotalLiabilities = new Label
        {
            Text = "إجمالي الخصوم: 0.00",
            Font = new Font("Cairo", 13F, FontStyle.Bold),
            ForeColor = ColorScheme.Error,
            Location = new Point(550, 15),
            AutoSize = true
        };

        lblTotalEquity = new Label
        {
            Text = "إجمالي حقوق الملكية: 0.00",
            Font = new Font("Cairo", 13F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            Location = new Point(100, 15),
            AutoSize = true
        };

        lblBalance = new Label
        {
            Text = "التوازن: 0.00",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = ColorScheme.Warning,
            Location = new Point(100, 55),
            AutoSize = true
        };

        Label lblFormula = new Label
        {
            Text = "(الأصول = الخصوم + حقوق الملكية)",
            Font = new Font("Cairo", 10F),
            ForeColor = Color.Gray,
            Location = new Point(550, 60),
            AutoSize = true
        };

        totalsPanel.Controls.AddRange(new Control[] { 
            lblTotalAssets, lblTotalLiabilities, lblTotalEquity, lblBalance, lblFormula
        });

        mainPanel.Controls.AddRange(new Control[] {
            lblTitle, filterPanel, dgvBalanceSheet, totalsPanel
        });

        this.Controls.Add(mainPanel);
    }

    private async void BtnGenerate_Click(object? sender, EventArgs e)
    {
        try
        {
            var asOfDate = DateTime.SpecifyKind(dtpAsOf.Value.Date, DateTimeKind.Utc);

            using var _dbContext = _dbContextFactory.CreateDbContext();

            // Get all accounts
            var allAccounts = await _dbContext.Set<Account>()
                .Where(a => a.IsActive)
                .OrderBy(a => a.AccountCode)
                .ToListAsync();

            // Get journal lines up to the date
            var journalLines = await _dbContext.Set<JournalEntryLine>()
                .Include(l => l.Account)
                .Include(l => l.JournalEntry)
                .Where(l => l.JournalEntry!.EntryDate <= asOfDate && l.JournalEntry.IsPosted)
                .ToListAsync();

            // Separate accounts by type
            var assetAccounts = allAccounts.Where(a => a.AccountCode.StartsWith("1")).ToList();
            var liabilityAccounts = allAccounts.Where(a => a.AccountCode.StartsWith("2")).ToList();
            var equityAccounts = allAccounts.Where(a => a.AccountCode.StartsWith("3")).ToList();

            dgvBalanceSheet.Columns.Clear();
            dgvBalanceSheet.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Category",
                HeaderText = "البند",
                Width = 400
            });
            dgvBalanceSheet.Columns.Add(new DataGridViewTextBoxColumn
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

            decimal totalAssets = 0;
            decimal totalLiabilities = 0;
            decimal totalEquity = 0;

            // ASSETS SECTION
            dgvBalanceSheet.Rows.Add("الأصول (Assets)", "");
            dgvBalanceSheet.Rows[dgvBalanceSheet.Rows.Count - 1].DefaultCellStyle.BackColor = ColorScheme.Primary;
            dgvBalanceSheet.Rows[dgvBalanceSheet.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.White;
            dgvBalanceSheet.Rows[dgvBalanceSheet.Rows.Count - 1].DefaultCellStyle.Font = new Font("Cairo", 13F, FontStyle.Bold);

            foreach (var account in assetAccounts)
            {
                var accountLines = journalLines.Where(l => l.AccountId == account.AccountId).ToList();
                decimal balance = accountLines.Sum(l => l.DebitAmount) - accountLines.Sum(l => l.CreditAmount);
                
                if (balance != 0)
                {
                    dgvBalanceSheet.Rows.Add($"  {account.AccountCode} - {account.AccountName}", balance);
                    totalAssets += balance;
                }
            }

            dgvBalanceSheet.Rows.Add("إجمالي الأصول", totalAssets);
            dgvBalanceSheet.Rows[dgvBalanceSheet.Rows.Count - 1].DefaultCellStyle.BackColor = Color.FromArgb(200, 230, 201);
            dgvBalanceSheet.Rows[dgvBalanceSheet.Rows.Count - 1].DefaultCellStyle.Font = new Font("Cairo", 12F, FontStyle.Bold);

            // Empty row
            dgvBalanceSheet.Rows.Add("", "");

            // LIABILITIES SECTION
            dgvBalanceSheet.Rows.Add("الخصوم (Liabilities)", "");
            dgvBalanceSheet.Rows[dgvBalanceSheet.Rows.Count - 1].DefaultCellStyle.BackColor = ColorScheme.Primary;
            dgvBalanceSheet.Rows[dgvBalanceSheet.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.White;
            dgvBalanceSheet.Rows[dgvBalanceSheet.Rows.Count - 1].DefaultCellStyle.Font = new Font("Cairo", 13F, FontStyle.Bold);

            foreach (var account in liabilityAccounts)
            {
                var accountLines = journalLines.Where(l => l.AccountId == account.AccountId).ToList();
                decimal balance = accountLines.Sum(l => l.CreditAmount) - accountLines.Sum(l => l.DebitAmount);
                
                if (balance != 0)
                {
                    dgvBalanceSheet.Rows.Add($"  {account.AccountCode} - {account.AccountName}", balance);
                    totalLiabilities += balance;
                }
            }

            dgvBalanceSheet.Rows.Add("إجمالي الخصوم", totalLiabilities);
            dgvBalanceSheet.Rows[dgvBalanceSheet.Rows.Count - 1].DefaultCellStyle.BackColor = Color.FromArgb(255, 205, 210);
            dgvBalanceSheet.Rows[dgvBalanceSheet.Rows.Count - 1].DefaultCellStyle.Font = new Font("Cairo", 12F, FontStyle.Bold);

            // Empty row
            dgvBalanceSheet.Rows.Add("", "");

            // EQUITY SECTION
            dgvBalanceSheet.Rows.Add("حقوق الملكية (Equity)", "");
            dgvBalanceSheet.Rows[dgvBalanceSheet.Rows.Count - 1].DefaultCellStyle.BackColor = ColorScheme.Primary;
            dgvBalanceSheet.Rows[dgvBalanceSheet.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.White;
            dgvBalanceSheet.Rows[dgvBalanceSheet.Rows.Count - 1].DefaultCellStyle.Font = new Font("Cairo", 13F, FontStyle.Bold);

            foreach (var account in equityAccounts)
            {
                var accountLines = journalLines.Where(l => l.AccountId == account.AccountId).ToList();
                decimal balance = accountLines.Sum(l => l.CreditAmount) - accountLines.Sum(l => l.DebitAmount);
                
                if (balance != 0)
                {
                    dgvBalanceSheet.Rows.Add($"  {account.AccountCode} - {account.AccountName}", balance);
                    totalEquity += balance;
                }
            }

            dgvBalanceSheet.Rows.Add("إجمالي حقوق الملكية", totalEquity);
            dgvBalanceSheet.Rows[dgvBalanceSheet.Rows.Count - 1].DefaultCellStyle.BackColor = Color.FromArgb(187, 222, 251);
            dgvBalanceSheet.Rows[dgvBalanceSheet.Rows.Count - 1].DefaultCellStyle.Font = new Font("Cairo", 12F, FontStyle.Bold);

            // Empty row
            dgvBalanceSheet.Rows.Add("", "");

            // TOTALS
            decimal liabilitiesAndEquity = totalLiabilities + totalEquity;
            dgvBalanceSheet.Rows.Add("إجمالي الخصوم + حقوق الملكية", liabilitiesAndEquity);
            dgvBalanceSheet.Rows[dgvBalanceSheet.Rows.Count - 1].DefaultCellStyle.BackColor = ColorScheme.Warning;
            dgvBalanceSheet.Rows[dgvBalanceSheet.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.White;
            dgvBalanceSheet.Rows[dgvBalanceSheet.Rows.Count - 1].DefaultCellStyle.Font = new Font("Cairo", 13F, FontStyle.Bold);

            // Update labels
            lblTotalAssets.Text = $"إجمالي الأصول: {totalAssets:N2}";
            lblTotalLiabilities.Text = $"إجمالي الخصوم: {totalLiabilities:N2}";
            lblTotalEquity.Text = $"إجمالي حقوق الملكية: {totalEquity:N2}";

            decimal difference = totalAssets - liabilitiesAndEquity;
            lblBalance.Text = $"التوازن: {Math.Abs(difference):N2}";

            if (Math.Abs(difference) < 0.01m)
            {
                lblBalance.ForeColor = ColorScheme.Success;
                lblBalance.Text += " ✅ متوازنة";
            }
            else
            {
                lblBalance.ForeColor = ColorScheme.Error;
                lblBalance.Text += " ⚠️ غير متوازنة";
            }

            btnExport.Enabled = true;
            btnPrint.Enabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في إنشاء التقرير: {ex.Message}\n\n{ex.StackTrace}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }
    
    private void BtnPrint_Click(object? sender, EventArgs e)
    {
        try
        {
            if (dgvBalanceSheet.Rows.Count == 0)
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
            string title = "💼 الميزانية العمومية";
            SizeF titleSize = g.MeasureString(title, titleFont);
            g.DrawString(title, titleFont, Brushes.Black, x + (pageWidth - titleSize.Width) / 2, y);
            y += titleSize.Height + 10;

            // Date
            string date = $"كما في تاريخ {dtpAsOf.Value:yyyy/MM/dd}";
            SizeF dateSize = g.MeasureString(date, normalFont);
            g.DrawString(date, normalFont, Brushes.Black, x + (pageWidth - dateSize.Width) / 2, y);
            y += dateSize.Height + 20;

            // Column headers
            float col1Width = pageWidth * 0.65f; // Account
            float col2Width = pageWidth * 0.35f; // Amount

            float col1X = x + pageWidth - col1Width;
            float col2X = x;

            // Draw header background
            g.FillRectangle(new SolidBrush(ColorScheme.Primary), x, y, pageWidth, 30);
            
            // Draw headers
            g.DrawString("الحساب", headerFont, Brushes.White, col1X + 10, y + 5);
            g.DrawString("المبلغ", headerFont, Brushes.White, col2X + 10, y + 5);
            
            y += 35;

            // Draw rows
            int rowsPerPage = (int)((e.MarginBounds.Bottom - y - 120) / 25);
            int rowsPrinted = 0;

            for (int i = currentRow; i < dgvBalanceSheet.Rows.Count && rowsPrinted < rowsPerPage; i++)
            {
                DataGridViewRow row = dgvBalanceSheet.Rows[i];
                
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

                string account = row.Cells[0].Value?.ToString() ?? "";
                string amount = row.Cells[1].Value?.ToString() ?? "";
                
                g.DrawString(account, rowFont, new SolidBrush(textColor), col1X + 10, y + 3);
                g.DrawString(amount, rowFont, new SolidBrush(textColor), col2X + 10, y + 3);

                y += 25;
                rowsPrinted++;
                currentRow++;
            }

            // Draw totals if last page
            if (currentRow >= dgvBalanceSheet.Rows.Count)
            {
                y += 10;
                g.DrawLine(new Pen(Color.Black, 2), x, y, x + pageWidth, y);
                y += 15;

                g.FillRectangle(new SolidBrush(ColorScheme.Background), x, y, pageWidth, 25);
                g.DrawString(lblTotalAssets.Text, headerFont, new SolidBrush(ColorScheme.Primary), x + 10, y + 3);
                y += 30;
                
                g.FillRectangle(new SolidBrush(ColorScheme.Background), x, y, pageWidth, 25);
                g.DrawString(lblTotalLiabilities.Text, headerFont, new SolidBrush(ColorScheme.Error), x + 10, y + 3);
                y += 30;

                g.FillRectangle(new SolidBrush(ColorScheme.Background), x, y, pageWidth, 25);
                g.DrawString(lblTotalEquity.Text, headerFont, new SolidBrush(ColorScheme.Success), x + 10, y + 3);
                y += 30;

                g.FillRectangle(new SolidBrush(ColorScheme.Background), x, y, pageWidth, 25);
                g.DrawString(lblBalance.Text, headerFont, new SolidBrush(lblBalance.ForeColor), x + 10, y + 3);

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
