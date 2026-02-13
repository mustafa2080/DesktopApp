using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class TrialBalanceReportForm : Form
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IExportService _exportService;
    private DateTimePicker dtpFrom = null!;
    private DateTimePicker dtpTo = null!;
    private DataGridView dgvTrialBalance = null!;
    private Button btnGenerate = null!;
    private Button btnExportExcel = null!;
    private Button btnExportPdf = null!;
    private Button btnPrint = null!;
    private Label lblTotalDebit = null!;
    private Label lblTotalCredit = null!;
    private Label lblDifference = null!;
    private Label lblAccountsCount = null!;
    
    // For printing
    private int currentRow = 0;
    private bool hasMorePages = false;

    public TrialBalanceReportForm(IDbContextFactory<AppDbContext> dbContextFactory, IExportService exportService)
    {
        _dbContextFactory = dbContextFactory;
        _exportService = exportService;
        InitializeComponent();
        InitializeCustomComponents();
    }

    private void InitializeCustomComponents()
    {
        this.Text = "⚖️ ميزان المراجعة";
        this.Size = new Size(1400, 850);
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
            Text = "⚖️ ميزان المراجعة (Trial Balance)",
            Font = new Font("Cairo", 18F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(30, 20)
        };

        // Filter Panel
        Panel filterPanel = new Panel
        {
            Size = new Size(1340, 100),
            Location = new Point(30, 70),
            BackColor = ColorScheme.Background,
            BorderStyle = BorderStyle.FixedSingle
        };

        // Date Filters Row
        Label lblFrom = new Label
        {
            Text = "من تاريخ:",
            Font = new Font("Cairo", 11F),
            Location = new Point(1230, 15),
            AutoSize = true
        };
        dtpFrom = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Location = new Point(1070, 12),
            Size = new Size(150, 30),
            Font = new Font("Cairo", 10F),
            Value = new DateTime(DateTime.Today.Year, 1, 1)
        };

        Label lblTo = new Label
        {
            Text = "إلى تاريخ:",
            Font = new Font("Cairo", 11F),
            Location = new Point(990, 15),
            AutoSize = true
        };
        dtpTo = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Location = new Point(830, 12),
            Size = new Size(150, 30),
            Font = new Font("Cairo", 10F),
            Value = DateTime.Today
        };

        // Action Buttons Row - Aligned and Equal Width
        int buttonY = 52;
        int buttonWidth = 120;
        int buttonSpacing = 15;
        int startX = 1340 - 40 - (4 * buttonWidth) - (3 * buttonSpacing);

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

        // DataGridView
        dgvTrialBalance = new DataGridView
        {
            Location = new Point(30, 180),
            Size = new Size(1340, 530),
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

        // Totals Panel - Enhanced with more info
        Panel totalsPanel = new Panel
        {
            Location = new Point(30, 720),
            Size = new Size(1340, 90),
            BackColor = ColorScheme.Background,
            BorderStyle = BorderStyle.FixedSingle
        };

        lblAccountsCount = new Label
        {
            Text = "عدد الحسابات: 0",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            Location = new Point(1100, 10),
            AutoSize = true
        };

        lblTotalDebit = new Label
        {
            Text = "إجمالي المدين: 0.00 جنيه",
            Font = new Font("Cairo", 13F, FontStyle.Bold),
            ForeColor = ColorScheme.Success,
            Location = new Point(700, 10),
            AutoSize = true
        };

        lblTotalCredit = new Label
        {
            Text = "إجمالي الدائن: 0.00 جنيه",
            Font = new Font("Cairo", 13F, FontStyle.Bold),
            ForeColor = ColorScheme.Error,
            Location = new Point(300, 10),
            AutoSize = true
        };

        lblDifference = new Label
        {
            Text = "الفرق: 0.00 جنيه",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = ColorScheme.Warning,
            Location = new Point(300, 50),
            AutoSize = true
        };

        Label lblNote = new Label
        {
            Text = "(ميزان المراجعة متوازن عندما يكون إجمالي المدين = إجمالي الدائن)",
            Font = new Font("Cairo", 9F),
            ForeColor = Color.Gray,
            Location = new Point(700, 55),
            AutoSize = true
        };

        totalsPanel.Controls.AddRange(new Control[] { 
            lblAccountsCount, lblTotalDebit, lblTotalCredit, lblDifference, lblNote 
        });

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
            var toDate = DateTime.SpecifyKind(dtpTo.Value.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);

            using var _dbContext = _dbContextFactory.CreateDbContext();

            // Get all active accounts
            var allAccounts = await _dbContext.Set<Account>()
                .Where(a => a.IsActive)
                .OrderBy(a => a.AccountCode)
                .ToListAsync();

            // Get journal entry lines within date range
            var journalLines = await _dbContext.Set<JournalEntryLine>()
                .AsNoTracking()
                .Include(l => l.Account)
                .Include(l => l.JournalEntry)
                .Where(l => l.JournalEntry!.EntryDate >= fromDate && 
                           l.JournalEntry.EntryDate <= toDate &&
                           l.JournalEntry.IsPosted)
                .ToListAsync();

            // Calculate trial balance with account type classification
            var trialBalance = allAccounts
                .Select(account =>
                {
                    var accountLines = journalLines.Where(l => l.AccountId == account.AccountId).ToList();
                    var totalDebit = accountLines.Sum(l => l.DebitAmount);
                    var totalCredit = accountLines.Sum(l => l.CreditAmount);
                    var balance = totalDebit - totalCredit;
                    
                    // Determine account type for display
                    string accountType = account.AccountCode[0] switch
                    {
                        '1' => "أصول",
                        '2' => "خصوم",
                        '3' => "حقوق ملكية",
                        '4' => "إيرادات",
                        '5' => "مصروفات",
                        _ => "أخرى"
                    };
                    
                    return new
                    {
                        AccountCode = account.AccountCode,
                        AccountName = account.AccountName,
                        AccountType = accountType,
                        OpeningDebit = totalDebit,
                        OpeningCredit = totalCredit,
                        ClosingBalance = balance,
                        DebitBalance = balance > 0 ? balance : 0,
                        CreditBalance = balance < 0 ? Math.Abs(balance) : 0,
                        MovementsCount = accountLines.Count
                    };
                })
                .Where(x => x.MovementsCount > 0) // Only accounts with movements
                .OrderBy(x => x.AccountCode)
                .ToList();

            // Setup columns
            dgvTrialBalance.Columns.Clear();

            dgvTrialBalance.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AccountCode",
                HeaderText = "رمز الحساب",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Cairo", 10F, FontStyle.Bold)
                }
            });

            dgvTrialBalance.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AccountName",
                HeaderText = "اسم الحساب",
                Width = 250,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            dgvTrialBalance.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AccountType",
                HeaderText = "نوع الحساب",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            dgvTrialBalance.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OpeningDebit",
                HeaderText = "مدين (Debit)",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    ForeColor = ColorScheme.Success
                }
            });

            dgvTrialBalance.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OpeningCredit",
                HeaderText = "دائن (Credit)",
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    ForeColor = ColorScheme.Error
                }
            });

            dgvTrialBalance.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DebitBalance",
                HeaderText = "رصيد مدين",
                Width = 130,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            dgvTrialBalance.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CreditBalance",
                HeaderText = "رصيد دائن",
                Width = 130,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            dgvTrialBalance.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MovementsCount",
                HeaderText = "عدد الحركات",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle 
                { 
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Cairo", 9F)
                }
            });

            // Add rows with grouping by account type
            decimal grandTotalDebit = 0;
            decimal grandTotalCredit = 0;
            decimal grandDebitBalance = 0;
            decimal grandCreditBalance = 0;

            var accountGroups = trialBalance.GroupBy(x => x.AccountType).OrderBy(g => g.Key);
            
            foreach (var group in accountGroups)
            {
                // Add group header
                int headerIndex = dgvTrialBalance.Rows.Add(
                    "",
                    $"📋 {group.Key}",
                    "",
                    "",
                    "",
                    "",
                    "",
                    ""
                );
                
                dgvTrialBalance.Rows[headerIndex].DefaultCellStyle.BackColor = ColorScheme.Primary;
                dgvTrialBalance.Rows[headerIndex].DefaultCellStyle.ForeColor = Color.White;
                dgvTrialBalance.Rows[headerIndex].DefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
                
                // Add accounts in this group
                foreach (var item in group)
                {
                    dgvTrialBalance.Rows.Add(
                        item.AccountCode,
                        item.AccountName,
                        item.AccountType,
                        item.OpeningDebit,
                        item.OpeningCredit,
                        item.DebitBalance,
                        item.CreditBalance,
                        item.MovementsCount
                    );

                    grandTotalDebit += item.OpeningDebit;
                    grandTotalCredit += item.OpeningCredit;
                    grandDebitBalance += item.DebitBalance;
                    grandCreditBalance += item.CreditBalance;
                }
                
                // Add group totals
                decimal groupDebit = group.Sum(x => x.OpeningDebit);
                decimal groupCredit = group.Sum(x => x.OpeningCredit);
                
                int groupTotalIndex = dgvTrialBalance.Rows.Add(
                    "",
                    $"إجمالي {group.Key}",
                    "",
                    groupDebit,
                    groupCredit,
                    group.Sum(x => x.DebitBalance),
                    group.Sum(x => x.CreditBalance),
                    group.Sum(x => x.MovementsCount)
                );
                
                dgvTrialBalance.Rows[groupTotalIndex].DefaultCellStyle.BackColor = Color.FromArgb(230, 240, 250);
                dgvTrialBalance.Rows[groupTotalIndex].DefaultCellStyle.Font = new Font("Cairo", 10F, FontStyle.Bold);
                
                // Add separator
                dgvTrialBalance.Rows.Add("", "", "", "", "", "", "", "");
            }

            // Add grand total row
            int grandTotalIndex = dgvTrialBalance.Rows.Add(
                "",
                "الإجمالي الكلي (Grand Total)",
                "",
                grandTotalDebit,
                grandTotalCredit,
                grandDebitBalance,
                grandCreditBalance,
                trialBalance.Sum(x => x.MovementsCount)
            );
            
            dgvTrialBalance.Rows[grandTotalIndex].DefaultCellStyle.BackColor = ColorScheme.Warning;
            dgvTrialBalance.Rows[grandTotalIndex].DefaultCellStyle.ForeColor = Color.White;
            dgvTrialBalance.Rows[grandTotalIndex].DefaultCellStyle.Font = new Font("Cairo", 12F, FontStyle.Bold);

            // Update summary labels
            lblAccountsCount.Text = $"عدد الحسابات النشطة: {trialBalance.Count}";
            lblTotalDebit.Text = $"إجمالي المدين: {grandTotalDebit:N2} جنيه";
            lblTotalCredit.Text = $"إجمالي الدائن: {grandTotalCredit:N2} جنيه";
            
            decimal difference = Math.Abs(grandTotalDebit - grandTotalCredit);
            lblDifference.Text = $"الفرق: {difference:N2} جنيه";
            
            if (difference < 0.01m)
            {
                lblDifference.ForeColor = ColorScheme.Success;
                lblDifference.Text += " ✅ متوازن تماماً";
            }
            else
            {
                lblDifference.ForeColor = ColorScheme.Error;
                lblDifference.Text += " ⚠️ غير متوازن";
            }

            // Enable export and print buttons
            btnExportExcel.Enabled = trialBalance.Count > 0;
            btnExportPdf.Enabled = trialBalance.Count > 0;
            btnPrint.Enabled = trialBalance.Count > 0;

            // Show message if no data
            if (trialBalance.Count == 0)
            {
                MessageBox.Show(
                    "⚠️ تنبيه: لا توجد حركات مالية في الفترة المحددة!\n\n" +
                    $"الفترة: من {fromDate:dd/MM/yyyy} إلى {toDate:dd/MM/yyyy}\n\n" +
                    "تأكد من وجود قيود يومية معتمدة في هذه الفترة.",
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
                $"❌ خطأ في إنشاء ميزان المراجعة:\n\n{ex.Message}\n\n{ex.StackTrace}",
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
                Title = "حفظ ميزان المراجعة",
                FileName = $"ميزان_المراجعة_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                bool success = await _exportService.ExportToExcelAsync(
                    dgvTrialBalance, 
                    saveDialog.FileName, 
                    "ميزان المراجعة"
                );

                if (success)
                {
                    MessageBox.Show("✅ تم تصدير التقرير بنجاح!", "نجح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ خطأ في التصدير: {ex.Message}", "خطأ",
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
                Title = "حفظ ميزان المراجعة",
                FileName = $"ميزان_المراجعة_{DateTime.Now:yyyyMMdd}.html"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                var metadata = new Dictionary<string, string>
                {
                    { "الفترة", $"من {dtpFrom.Value:yyyy/MM/dd} إلى {dtpTo.Value:yyyy/MM/dd}" },
                    { "عدد الحسابات", lblAccountsCount.Text },
                    { "إجمالي المدين", lblTotalDebit.Text },
                    { "إجمالي الدائن", lblTotalCredit.Text },
                    { "الفرق", lblDifference.Text }
                };

                bool success = await _exportService.ExportToPdfAsync(
                    dgvTrialBalance,
                    saveDialog.FileName,
                    "⚖️ ميزان المراجعة",
                    metadata
                );

                if (success)
                {
                    MessageBox.Show("✅ تم تصدير التقرير بنجاح!", "نجح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ خطأ في التصدير: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private void BtnPrint_Click(object? sender, EventArgs e)
    {
        try
        {
            if (dgvTrialBalance.Rows.Count == 0)
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
            Font headerFont = new Font("Cairo", 11, FontStyle.Bold);
            Font normalFont = new Font("Cairo", 9);
            Font smallFont = new Font("Cairo", 8);

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
            y += dateSize.Height + 5;

            // Account count
            SizeF countSize = g.MeasureString(lblAccountsCount.Text, smallFont);
            g.DrawString(lblAccountsCount.Text, smallFont, Brushes.Gray, x + (pageWidth - countSize.Width) / 2, y);
            y += countSize.Height + 15;

            // Column headers - Adjusted widths for 8 columns
            float col1Width = 70;  // Account Code
            float col2Width = 180; // Account Name
            float col3Width = 80;  // Account Type
            float col4Width = 90;  // Debit
            float col5Width = 90;  // Credit
            float col6Width = 85;  // Debit Balance
            float col7Width = 85;  // Credit Balance
            float col8Width = 60;  // Movements Count

            float totalWidth = col1Width + col2Width + col3Width + col4Width + col5Width + col6Width + col7Width + col8Width;
            float startX = x + (pageWidth - totalWidth) / 2;

            float col1X = startX;
            float col2X = col1X + col1Width;
            float col3X = col2X + col2Width;
            float col4X = col3X + col3Width;
            float col5X = col4X + col4Width;
            float col6X = col5X + col5Width;
            float col7X = col6X + col6Width;
            float col8X = col7X + col7Width;

            // Draw header background
            g.FillRectangle(new SolidBrush(ColorScheme.Primary), startX, y, totalWidth, 25);
            
            // Draw headers
            g.DrawString("رمز", headerFont, Brushes.White, col1X + 5, y + 5);
            g.DrawString("اسم الحساب", headerFont, Brushes.White, col2X + 5, y + 5);
            g.DrawString("النوع", headerFont, Brushes.White, col3X + 5, y + 5);
            g.DrawString("مدين", headerFont, Brushes.White, col4X + 5, y + 5);
            g.DrawString("دائن", headerFont, Brushes.White, col5X + 5, y + 5);
            g.DrawString("رصيد مدين", headerFont, Brushes.White, col6X + 5, y + 5);
            g.DrawString("رصيد دائن", headerFont, Brushes.White, col7X + 5, y + 5);
            g.DrawString("حركات", headerFont, Brushes.White, col8X + 5, y + 5);
            
            y += 30;

            // Draw rows
            int rowsPerPage = (int)((e.MarginBounds.Bottom - y - 120) / 22);
            int rowsPrinted = 0;

            for (int i = currentRow; i < dgvTrialBalance.Rows.Count && rowsPrinted < rowsPerPage; i++)
            {
                DataGridViewRow row = dgvTrialBalance.Rows[i];
                
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
                
                // Alternate background for regular rows
                if (i % 2 == 1 && bgColor == Color.White)
                {
                    bgColor = Color.FromArgb(240, 248, 255);
                }
                
                g.FillRectangle(new SolidBrush(bgColor), startX, y, totalWidth, 22);

                string code = row.Cells["AccountCode"].Value?.ToString() ?? "";
                string name = row.Cells["AccountName"].Value?.ToString() ?? "";
                string type = row.Cells["AccountType"].Value?.ToString() ?? "";
                string debit = row.Cells["OpeningDebit"].Value?.ToString() ?? "";
                string credit = row.Cells["OpeningCredit"].Value?.ToString() ?? "";
                string debitBal = row.Cells["DebitBalance"].Value?.ToString() ?? "";
                string creditBal = row.Cells["CreditBalance"].Value?.ToString() ?? "";
                string movements = row.Cells["MovementsCount"].Value?.ToString() ?? "";
                
                // Format numbers
                if (decimal.TryParse(debit, out decimal d1)) debit = d1 == 0 ? "-" : d1.ToString("N0");
                if (decimal.TryParse(credit, out decimal d2)) credit = d2 == 0 ? "-" : d2.ToString("N0");
                if (decimal.TryParse(debitBal, out decimal d3)) debitBal = d3 == 0 ? "-" : d3.ToString("N0");
                if (decimal.TryParse(creditBal, out decimal d4)) creditBal = d4 == 0 ? "-" : d4.ToString("N0");
                
                g.DrawString(code, rowFont, new SolidBrush(textColor), col1X + 3, y + 3);
                g.DrawString(TruncateString(name, 25), rowFont, new SolidBrush(textColor), col2X + 3, y + 3);
                g.DrawString(type, rowFont, new SolidBrush(textColor), col3X + 3, y + 3);
                g.DrawString(debit, rowFont, new SolidBrush(textColor), col4X + 3, y + 3);
                g.DrawString(credit, rowFont, new SolidBrush(textColor), col5X + 3, y + 3);
                g.DrawString(debitBal, rowFont, new SolidBrush(textColor), col6X + 3, y + 3);
                g.DrawString(creditBal, rowFont, new SolidBrush(textColor), col7X + 3, y + 3);
                g.DrawString(movements, rowFont, new SolidBrush(textColor), col8X + 3, y + 3);

                y += 22;
                rowsPrinted++;
                currentRow++;
            }

            // Draw totals if last page
            if (currentRow >= dgvTrialBalance.Rows.Count)
            {
                y += 10;
                g.DrawLine(new Pen(Color.Black, 2), startX, y, startX + totalWidth, y);
                y += 15;

                g.FillRectangle(new SolidBrush(ColorScheme.Background), startX, y, totalWidth, 22);
                g.DrawString(lblTotalDebit.Text, headerFont, new SolidBrush(ColorScheme.Success), startX + 10, y + 3);
                y += 25;
                
                g.FillRectangle(new SolidBrush(ColorScheme.Background), startX, y, totalWidth, 22);
                g.DrawString(lblTotalCredit.Text, headerFont, new SolidBrush(ColorScheme.Error), startX + 10, y + 3);
                y += 25;

                g.FillRectangle(new SolidBrush(ColorScheme.Background), startX, y, totalWidth, 22);
                g.DrawString(lblDifference.Text, headerFont, new SolidBrush(lblDifference.ForeColor), startX + 10, y + 3);

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

    private string TruncateString(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;
        return text.Substring(0, maxLength - 3) + "...";
    }
}
