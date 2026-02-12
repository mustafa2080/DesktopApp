using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class TrialBalanceReportForm : Form
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private DateTimePicker dtpFrom = null!;
    private DateTimePicker dtpTo = null!;
    private DataGridView dgvTrialBalance = null!;
    private Button btnGenerate = null!;
    private Button btnExport = null!;
    private Button btnPrint = null!;
    private Label lblTotalDebit = null!;
    private Label lblTotalCredit = null!;
    private Label lblDifference = null!;
    
    // For printing
    private int currentRow = 0;
    private bool hasMorePages = false;

    public TrialBalanceReportForm(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        InitializeComponent();
        InitializeCustomComponents();
    }

    private void InitializeCustomComponents()
    {
        this.Text = "⚖️ ميزان المراجعة";
        this.Size = new Size(1400, 800);
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;

        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(30)
        };

        // Title
        Label lblTitle = new Label
        {
            Text = "⚖️ ميزان المراجعة",
            Font = new Font("Cairo", 18F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(30, 20)
        };

        // Filter Panel
        Panel filterPanel = new Panel
        {
            Size = new Size(1340, 80),
            Location = new Point(30, 70),
            BackColor = ColorScheme.Background,
            BorderStyle = BorderStyle.FixedSingle
        };

        Label lblFrom = new Label
        {
            Text = "من تاريخ:",
            Font = new Font("Cairo", 11F),
            Location = new Point(1180, 20),
            AutoSize = true
        };
        dtpFrom = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Location = new Point(1020, 17),
            Size = new Size(150, 30),
            Font = new Font("Cairo", 10F),
            Value = new DateTime(DateTime.Today.Year, 1, 1)
        };

        Label lblTo = new Label
        {
            Text = "إلى تاريخ:",
            Font = new Font("Cairo", 11F),
            Location = new Point(940, 20),
            AutoSize = true
        };
        dtpTo = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Location = new Point(780, 17),
            Size = new Size(150, 30),
            Font = new Font("Cairo", 10F),
            Value = DateTime.Today
        };

        btnGenerate = new Button
        {
            Text = "📊 إنشاء التقرير",
            Location = new Point(600, 15),
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
            Text = "📥 تصدير Excel",
            Location = new Point(440, 15),
            Size = new Size(150, 35),
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnExport.FlatAppearance.BorderSize = 0;
        btnExport.Click += BtnExport_Click;

        btnPrint = new Button
        {
            Text = "🖨️ طباعة",
            Location = new Point(280, 15),
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
            lblFrom, dtpFrom, lblTo, dtpTo, btnGenerate, btnExport, btnPrint
        });

        // DataGridView
        dgvTrialBalance = new DataGridView
        {
            Location = new Point(30, 160),
            Size = new Size(1340, 510),
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

        dgvTrialBalance.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Primary;
        dgvTrialBalance.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvTrialBalance.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        dgvTrialBalance.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvTrialBalance.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvTrialBalance.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);

        // Totals Panel
        Panel totalsPanel = new Panel
        {
            Location = new Point(30, 680),
            Size = new Size(1340, 60),
            BackColor = ColorScheme.Background,
            BorderStyle = BorderStyle.FixedSingle
        };

        lblTotalDebit = new Label
        {
            Text = "إجمالي المدين: 0.00",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = ColorScheme.Success,
            Location = new Point(900, 15),
            AutoSize = true
        };

        lblTotalCredit = new Label
        {
            Text = "إجمالي الدائن: 0.00",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = ColorScheme.Error,
            Location = new Point(500, 15),
            AutoSize = true
        };

        lblDifference = new Label
        {
            Text = "الفرق: 0.00",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = ColorScheme.Warning,
            Location = new Point(200, 15),
            AutoSize = true
        };

        totalsPanel.Controls.AddRange(new Control[] { lblTotalDebit, lblTotalCredit, lblDifference });

        mainPanel.Controls.AddRange(new Control[] {
            lblTitle, filterPanel, dgvTrialBalance, totalsPanel
        });

        this.Controls.Add(mainPanel);
    }

    private async void BtnGenerate_Click(object? sender, EventArgs e)
    {
        try
        {
            var fromDate = DateTime.SpecifyKind(dtpFrom.Value.Date, DateTimeKind.Utc);
            var toDate = DateTime.SpecifyKind(dtpTo.Value.Date, DateTimeKind.Utc);

            // Get all journal entry lines within date range
            using var _dbContext = _dbContextFactory.CreateDbContext();
            var journalLines = await _dbContext.Set<JournalEntryLine>()
                .AsNoTracking()  // إضافة AsNoTracking لتحسين الأداء
                .Include(l => l.Account)
                .Include(l => l.JournalEntry)
                .Where(l => l.JournalEntry!.EntryDate >= fromDate && 
                           l.JournalEntry.EntryDate <= toDate &&
                           l.JournalEntry.IsPosted)
                .ToListAsync();  // إنهاء الاستعلام بالكامل

            // Group by account and calculate totals - في الذاكرة
            var trialBalance = journalLines
                .GroupBy(l => new { l.AccountId, l.Account!.AccountCode, l.Account.AccountName })
                .Select(g => new
                {
                    AccountCode = g.Key.AccountCode,
                    AccountName = g.Key.AccountName,
                    TotalDebit = g.Sum(l => l.DebitAmount),
                    TotalCredit = g.Sum(l => l.CreditAmount),
                    Balance = g.Sum(l => l.DebitAmount) - g.Sum(l => l.CreditAmount)
                })
                .OrderBy(x => x.AccountCode)
                .ToList();

            // Setup columns
            dgvTrialBalance.Columns.Clear();

            dgvTrialBalance.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AccountCode",
                HeaderText = "رمز الحساب",
                Width = 120
            });

            dgvTrialBalance.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AccountName",
                HeaderText = "اسم الحساب",
                Width = 300
            });

            dgvTrialBalance.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalDebit",
                HeaderText = "إجمالي المدين",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            dgvTrialBalance.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalCredit",
                HeaderText = "إجمالي الدائن",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            dgvTrialBalance.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Balance",
                HeaderText = "الرصيد",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            // Add rows
            decimal grandTotalDebit = 0;
            decimal grandTotalCredit = 0;

            foreach (var item in trialBalance)
            {
                dgvTrialBalance.Rows.Add(
                    item.AccountCode,
                    item.AccountName,
                    item.TotalDebit,
                    item.TotalCredit,
                    item.Balance
                );

                grandTotalDebit += item.TotalDebit;
                grandTotalCredit += item.TotalCredit;
            }

            // Update totals
            lblTotalDebit.Text = $"إجمالي المدين: {grandTotalDebit:N2}";
            lblTotalCredit.Text = $"إجمالي الدائن: {grandTotalCredit:N2}";
            
            decimal difference = Math.Abs(grandTotalDebit - grandTotalCredit);
            lblDifference.Text = $"الفرق: {difference:N2}";
            
            if (difference == 0)
            {
                lblDifference.ForeColor = ColorScheme.Success;
                lblDifference.Text += " ✅ متوازن";
            }
            else
            {
                lblDifference.ForeColor = ColorScheme.Error;
                lblDifference.Text += " ⚠️ غير متوازن";
            }

            btnExport.Enabled = dgvTrialBalance.Rows.Count > 0;
            btnPrint.Enabled = dgvTrialBalance.Rows.Count > 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في إنشاء التقرير: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private void BtnExport_Click(object? sender, EventArgs e)
    {
        try
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"ميزان_المراجعة_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                // TODO: Implement Excel export using EPPlus or similar
                MessageBox.Show("سيتم تنفيذ التصدير إلى Excel قريباً", "قيد التطوير",
                    MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
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
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += PrintDocument_PrintPage;
            
            PrintDialog printDialog = new PrintDialog
            {
                Document = printDoc
            };

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
            string title = "⚖️ ميزان المراجعة";
            SizeF titleSize = g.MeasureString(title, titleFont);
            g.DrawString(title, titleFont, Brushes.Black, x + (pageWidth - titleSize.Width) / 2, y);
            y += titleSize.Height + 10;

            // Date range
            string dateRange = $"من {dtpFrom.Value:yyyy/MM/dd} إلى {dtpTo.Value:yyyy/MM/dd}";
            SizeF dateSize = g.MeasureString(dateRange, normalFont);
            g.DrawString(dateRange, normalFont, Brushes.Black, x + (pageWidth - dateSize.Width) / 2, y);
            y += dateSize.Height + 20;

            // Column headers
            float col1Width = 100; // Account Code
            float col2Width = 250; // Account Name
            float col3Width = 120; // Debit
            float col4Width = 120; // Credit
            float col5Width = 120; // Balance

            float col1X = x + pageWidth - col1Width;
            float col2X = col1X - col2Width;
            float col3X = col2X - col3Width;
            float col4X = col3X - col4Width;
            float col5X = col4X - col5Width;

            // Draw header background
            g.FillRectangle(new SolidBrush(ColorScheme.Primary), col5X, y, pageWidth, 30);
            
            // Draw headers
            g.DrawString("رمز الحساب", headerFont, Brushes.White, col1X, y + 5);
            g.DrawString("اسم الحساب", headerFont, Brushes.White, col2X, y + 5);
            g.DrawString("إجمالي المدين", headerFont, Brushes.White, col3X, y + 5);
            g.DrawString("إجمالي الدائن", headerFont, Brushes.White, col4X, y + 5);
            g.DrawString("الرصيد", headerFont, Brushes.White, col5X, y + 5);
            
            y += 35;

            // Draw rows
            int rowsPerPage = (int)((e.MarginBounds.Bottom - y - 100) / 25);
            int rowsPrinted = 0;

            for (int i = currentRow; i < dgvTrialBalance.Rows.Count && rowsPrinted < rowsPerPage; i++)
            {
                DataGridViewRow row = dgvTrialBalance.Rows[i];
                
                // Alternate row background
                if (i % 2 == 1)
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(240, 248, 255)), 
                        col5X, y, pageWidth, 25);
                }

                g.DrawString(row.Cells["AccountCode"].Value?.ToString() ?? "", normalFont, Brushes.Black, col1X, y);
                g.DrawString(row.Cells["AccountName"].Value?.ToString() ?? "", normalFont, Brushes.Black, col2X, y);
                g.DrawString(Convert.ToDecimal(row.Cells["TotalDebit"].Value).ToString("N2"), normalFont, Brushes.Black, col3X, y);
                g.DrawString(Convert.ToDecimal(row.Cells["TotalCredit"].Value).ToString("N2"), normalFont, Brushes.Black, col4X, y);
                g.DrawString(Convert.ToDecimal(row.Cells["Balance"].Value).ToString("N2"), normalFont, Brushes.Black, col5X, y);

                y += 25;
                rowsPrinted++;
                currentRow++;
            }

            // Draw totals if last page
            if (currentRow >= dgvTrialBalance.Rows.Count)
            {
                y += 10;
                g.DrawLine(new Pen(Color.Black, 2), col5X, y, x + pageWidth, y);
                y += 15;

                g.FillRectangle(new SolidBrush(ColorScheme.Background), col5X, y, pageWidth, 30);
                g.DrawString(lblTotalDebit.Text, headerFont, new SolidBrush(ColorScheme.Success), col3X, y + 5);
                y += 35;
                
                g.FillRectangle(new SolidBrush(ColorScheme.Background), col5X, y, pageWidth, 30);
                g.DrawString(lblTotalCredit.Text, headerFont, new SolidBrush(ColorScheme.Error), col3X, y + 5);
                y += 35;

                g.FillRectangle(new SolidBrush(ColorScheme.Background), col5X, y, pageWidth, 30);
                g.DrawString(lblDifference.Text, headerFont, new SolidBrush(ColorScheme.Warning), col3X, y + 5);

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
