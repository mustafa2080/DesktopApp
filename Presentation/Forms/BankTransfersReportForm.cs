using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using GraceWay.AccountingSystem.Presentation;

namespace GraceWay.AccountingSystem.Presentation.Forms
{
    public partial class BankTransfersReportForm : Form
    {
        private readonly AppDbContext _context;
        private DataGridView dgvTransfers = null!;
        private ComboBox cmbTransferType = null!;
        private ComboBox cmbSourceBank = null!;
        private ComboBox cmbDestinationBank = null!;
        private DateTimePicker dtpFromDate = null!;
        private DateTimePicker dtpToDate = null!;
        private Button btnFilter = null!;
        private Button btnClear = null!;
        private Button btnExport = null!;
        private Button btnPrint = null!;
        private Label lblTotalTransfers = null!;
        private Label lblTotalAmount = null!;
        private Panel summaryPanel = null!;

        public BankTransfersReportForm(AppDbContext context)
        {
            _context = context;
            InitializeComponent();
            InitializeCustomComponents();
            LoadFilters();
            LoadTransfers();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "تقرير التحويلات البنكية";
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = Color.White;
            this.Font = new Font("Cairo", 10F);

            // Main Panel
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                BackColor = Color.White
            };

            // Title Panel
            Panel titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = ColorScheme.Primary
            };

            Label lblTitle = new Label
            {
                Text = "📊 تقرير التحويلات البنكية",
                Dock = DockStyle.Fill,
                Font = new Font("Cairo", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            titlePanel.Controls.Add(lblTitle);
            mainPanel.Controls.Add(titlePanel);

            // Filters Panel
            Panel filtersPanel = CreateFiltersPanel();
            filtersPanel.Dock = DockStyle.Top;
            mainPanel.Controls.Add(filtersPanel);

            // Summary Panel
            summaryPanel = CreateSummaryPanel();
            summaryPanel.Dock = DockStyle.Top;
            mainPanel.Controls.Add(summaryPanel);

            // DataGridView
            dgvTransfers = CreateDataGridView();
            dgvTransfers.Dock = DockStyle.Fill;
            mainPanel.Controls.Add(dgvTransfers);

            this.Controls.Add(mainPanel);
        }

        private Panel CreateFiltersPanel()
        {
            Panel panel = new Panel
            {
                Height = 180,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(240, 240, 240)
            };

            int yPos = 10;

            // First Row - Transfer Type and Date From
            Label lblType = new Label
            {
                Text = "نوع التحويل:",
                Location = new Point(1050, yPos),
                Size = new Size(100, 25),
                Font = new Font("Cairo", 9F, FontStyle.Bold)
            };
            panel.Controls.Add(lblType);

            cmbTransferType = new ComboBox
            {
                Location = new Point(850, yPos),
                Size = new Size(190, 30),
                Font = new Font("Cairo", 9F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTransferType.Items.AddRange(new[] { 
                "الكل", 
                "من بنك إلى بنك", 
                "من بنك إلى خزنة", 
                "من خزنة إلى بنك" 
            });
            cmbTransferType.SelectedIndex = 0;
            panel.Controls.Add(cmbTransferType);

            Label lblFromDate = new Label
            {
                Text = "من تاريخ:",
                Location = new Point(740, yPos),
                Size = new Size(100, 25),
                Font = new Font("Cairo", 9F, FontStyle.Bold)
            };
            panel.Controls.Add(lblFromDate);

            dtpFromDate = new DateTimePicker
            {
                Location = new Point(540, yPos),
                Size = new Size(190, 30),
                Font = new Font("Cairo", 9F),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddMonths(-1)
            };
            panel.Controls.Add(dtpFromDate);

            Label lblToDate = new Label
            {
                Text = "إلى تاريخ:",
                Location = new Point(430, yPos),
                Size = new Size(100, 25),
                Font = new Font("Cairo", 9F, FontStyle.Bold)
            };
            panel.Controls.Add(lblToDate);

            dtpToDate = new DateTimePicker
            {
                Location = new Point(230, yPos),
                Size = new Size(190, 30),
                Font = new Font("Cairo", 9F),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };
            panel.Controls.Add(dtpToDate);

            yPos += 40;

            // Second Row - Source and Destination Banks
            Label lblSourceBank = new Label
            {
                Text = "البنك المصدر:",
                Location = new Point(1050, yPos),
                Size = new Size(100, 25),
                Font = new Font("Cairo", 9F, FontStyle.Bold)
            };
            panel.Controls.Add(lblSourceBank);

            cmbSourceBank = new ComboBox
            {
                Location = new Point(850, yPos),
                Size = new Size(190, 30),
                Font = new Font("Cairo", 9F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            panel.Controls.Add(cmbSourceBank);

            Label lblDestBank = new Label
            {
                Text = "البنك المستقبل:",
                Location = new Point(740, yPos),
                Size = new Size(100, 25),
                Font = new Font("Cairo", 9F, FontStyle.Bold)
            };
            panel.Controls.Add(lblDestBank);

            cmbDestinationBank = new ComboBox
            {
                Location = new Point(540, yPos),
                Size = new Size(190, 30),
                Font = new Font("Cairo", 9F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            panel.Controls.Add(cmbDestinationBank);

            yPos += 40;

            // Third Row - Buttons
            btnFilter = new Button
            {
                Text = "🔍 بحث",
                Location = new Point(1050, yPos),
                Size = new Size(120, 40),
                BackColor = ColorScheme.Primary,
                ForeColor = Color.White,
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnFilter.Click += BtnFilter_Click;
            panel.Controls.Add(btnFilter);

            btnClear = new Button
            {
                Text = "🔄 مسح الفلاتر",
                Location = new Point(920, yPos),
                Size = new Size(120, 40),
                BackColor = ColorScheme.Warning,
                ForeColor = Color.White,
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClear.Click += BtnClear_Click;
            panel.Controls.Add(btnClear);

            btnExport = new Button
            {
                Text = "📥 تصدير Excel",
                Location = new Point(790, yPos),
                Size = new Size(120, 40),
                BackColor = ColorScheme.Success,
                ForeColor = Color.White,
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExport.Click += BtnExport_Click;
            panel.Controls.Add(btnExport);

            btnPrint = new Button
            {
                Text = "🖨️ طباعة",
                Location = new Point(660, yPos),
                Size = new Size(120, 40),
                BackColor = ColorScheme.Secondary,
                ForeColor = Color.White,
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPrint.Click += BtnPrint_Click;
            panel.Controls.Add(btnPrint);

            return panel;
        }

        private Panel CreateSummaryPanel()
        {
            Panel panel = new Panel
            {
                Height = 80,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(245, 252, 255)
            };

            // Total Transfers Card
            Panel cardTransfers = new Panel
            {
                Location = new Point(950, 10),
                Size = new Size(300, 60),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblTransfersTitle = new Label
            {
                Text = "إجمالي عدد التحويلات",
                Location = new Point(10, 10),
                Size = new Size(280, 20),
                Font = new Font("Cairo", 9F),
                ForeColor = ColorScheme.TextSecondary,
                TextAlign = ContentAlignment.TopCenter
            };
            cardTransfers.Controls.Add(lblTransfersTitle);

            lblTotalTransfers = new Label
            {
                Text = "0",
                Location = new Point(10, 30),
                Size = new Size(280, 25),
                Font = new Font("Cairo", 14F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                TextAlign = ContentAlignment.TopCenter
            };
            cardTransfers.Controls.Add(lblTotalTransfers);

            panel.Controls.Add(cardTransfers);

            // Total Amount Card
            Panel cardAmount = new Panel
            {
                Location = new Point(640, 10),
                Size = new Size(300, 60),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblAmountTitle = new Label
            {
                Text = "إجمالي المبالغ المحولة",
                Location = new Point(10, 10),
                Size = new Size(280, 20),
                Font = new Font("Cairo", 9F),
                ForeColor = ColorScheme.TextSecondary,
                TextAlign = ContentAlignment.TopCenter
            };
            cardAmount.Controls.Add(lblAmountTitle);

            lblTotalAmount = new Label
            {
                Text = "0.00 ج.م",
                Location = new Point(10, 30),
                Size = new Size(280, 25),
                Font = new Font("Cairo", 14F, FontStyle.Bold),
                ForeColor = ColorScheme.Success,
                TextAlign = ContentAlignment.TopCenter
            };
            cardAmount.Controls.Add(lblTotalAmount);

            panel.Controls.Add(cardAmount);

            return panel;
        }

        private DataGridView CreateDataGridView()
        {
            var dgv = new DataGridView
            {
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeight = 45,
                ColumnHeadersVisible = true,
                RowTemplate = { Height = 35 },
                Font = new Font("Cairo", 9F),
                EnableHeadersVisualStyles = false,
                AllowUserToResizeColumns = true,
                RowHeadersVisible = false
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Primary;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            dgv.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

            dgv.DefaultCellStyle.SelectionBackColor = ColorScheme.Primary;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.Padding = new Padding(5);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

            return dgv;
        }

        private void LoadFilters()
        {
            try
            {
                // Load Source Banks
                var banks = _context.Set<Domain.Entities.BankAccount>()
                    .Where(b => b.IsActive)
                    .OrderBy(b => b.BankName)
                    .Select(b => new
                    {
                        b.Id,
                        Display = $"{b.BankName} - {b.AccountNumber}"
                    })
                    .ToList();

                banks.Insert(0, new { Id = 0, Display = "الكل" });

                cmbSourceBank.DataSource = banks.Select(b => new { b.Id, b.Display }).ToList();
                cmbSourceBank.DisplayMember = "Display";
                cmbSourceBank.ValueMember = "Id";

                cmbDestinationBank.DataSource = banks.Select(b => new { b.Id, b.Display }).ToList();
                cmbDestinationBank.DisplayMember = "Display";
                cmbDestinationBank.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الفلاتر: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTransfers()
        {
            try
            {
                var query = _context.Set<Domain.Entities.BankTransfer>()
                    .Include(t => t.SourceBankAccount)
                    .Include(t => t.DestinationBankAccount)
                    .Include(t => t.SourceCashBox)
                    .Include(t => t.DestinationCashBox)
                    .AsQueryable();

                // Apply filters
                var fromDate = dtpFromDate.Value.Date;
                var toDate = dtpToDate.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(t => t.TransferDate >= fromDate && t.TransferDate <= toDate);

                // Transfer Type Filter
                if (cmbTransferType.SelectedIndex > 0)
                {
                    string transferType = cmbTransferType.SelectedIndex switch
                    {
                        1 => "BankToBank",
                        2 => "BankToCash",
                        3 => "CashToBank",
                        _ => ""
                    };
                    query = query.Where(t => t.TransferType == transferType);
                }

                // Source Bank Filter
                if (cmbSourceBank.SelectedValue != null && Convert.ToInt32(cmbSourceBank.SelectedValue) > 0)
                {
                    int bankId = Convert.ToInt32(cmbSourceBank.SelectedValue);
                    query = query.Where(t => t.SourceBankAccountId == bankId);
                }

                // Destination Bank Filter
                if (cmbDestinationBank.SelectedValue != null && Convert.ToInt32(cmbDestinationBank.SelectedValue) > 0)
                {
                    int bankId = Convert.ToInt32(cmbDestinationBank.SelectedValue);
                    query = query.Where(t => t.DestinationBankAccountId == bankId);
                }

                var transfers = query
                    .OrderByDescending(t => t.TransferDate)
                    .Select(t => new
                    {
                        t.Id,
                        TransferDate = t.TransferDate,
                        TransferType = t.TransferType,
                        SourceName = t.SourceBankAccount != null 
                            ? t.SourceBankAccount.BankName + " - " + t.SourceBankAccount.AccountNumber
                            : t.SourceCashBox != null 
                                ? t.SourceCashBox.Name 
                                : "",
                        DestinationName = t.DestinationBankAccount != null 
                            ? t.DestinationBankAccount.BankName + " - " + t.DestinationBankAccount.AccountNumber
                            : t.DestinationCashBox != null 
                                ? t.DestinationCashBox.Name 
                                : "",
                        t.Amount,
                        t.ReferenceNumber,
                        t.Notes
                    })
                    .ToList();

                // Prepare data for display
                var displayData = transfers.Select(t => new
                {
                    الرقم = t.Id,
                    التاريخ = t.TransferDate.ToString("yyyy-MM-dd HH:mm"),
                    النوع = GetTransferTypeArabic(t.TransferType),
                    من = t.SourceName,
                    إلى = t.DestinationName,
                    المبلغ = t.Amount.ToString("N2"),
                    رقم_المرجع = string.IsNullOrEmpty(t.ReferenceNumber) ? "-" : t.ReferenceNumber,
                    ملاحظات = string.IsNullOrEmpty(t.Notes) ? "-" : t.Notes
                }).ToList();

                dgvTransfers.DataSource = displayData;

                // Ensure headers are visible
                dgvTransfers.ColumnHeadersVisible = true;
                dgvTransfers.Refresh();

                // Update summary
                lblTotalTransfers.Text = transfers.Count.ToString("N0");
                lblTotalAmount.Text = transfers.Sum(t => t.Amount).ToString("N2") + " ج.م";

                // Format columns
                if (dgvTransfers.Columns.Count > 0)
                {
                    try
                    {
                        if (dgvTransfers.Columns.Contains("الرقم"))
                        {
                            dgvTransfers.Columns["الرقم"]!.Width = 70;
                            dgvTransfers.Columns["الرقم"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                        }
                        
                        if (dgvTransfers.Columns.Contains("التاريخ"))
                        {
                            dgvTransfers.Columns["التاريخ"]!.Width = 120;
                            dgvTransfers.Columns["التاريخ"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                        }
                        
                        if (dgvTransfers.Columns.Contains("النوع"))
                        {
                            dgvTransfers.Columns["النوع"]!.Width = 120;
                            dgvTransfers.Columns["النوع"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                        }
                        
                        if (dgvTransfers.Columns.Contains("من"))
                        {
                            dgvTransfers.Columns["من"]!.MinimumWidth = 150;
                            dgvTransfers.Columns["من"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                            dgvTransfers.Columns["من"]!.FillWeight = 50;
                        }
                        
                        if (dgvTransfers.Columns.Contains("إلى"))
                        {
                            dgvTransfers.Columns["إلى"]!.MinimumWidth = 150;
                            dgvTransfers.Columns["إلى"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                            dgvTransfers.Columns["إلى"]!.FillWeight = 50;
                        }
                        
                        if (dgvTransfers.Columns.Contains("المبلغ"))
                        {
                            dgvTransfers.Columns["المبلغ"]!.Width = 100;
                            dgvTransfers.Columns["المبلغ"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                            dgvTransfers.Columns["المبلغ"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                            dgvTransfers.Columns["المبلغ"]!.DefaultCellStyle.ForeColor = ColorScheme.Success;
                            dgvTransfers.Columns["المبلغ"]!.DefaultCellStyle.Font = new Font("Cairo", 9F, FontStyle.Bold);
                        }
                        
                        if (dgvTransfers.Columns.Contains("رقم_المرجع"))
                        {
                            dgvTransfers.Columns["رقم_المرجع"]!.Width = 120;
                            dgvTransfers.Columns["رقم_المرجع"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                        }
                        
                        if (dgvTransfers.Columns.Contains("ملاحظات"))
                        {
                            dgvTransfers.Columns["ملاحظات"]!.Width = 150;
                            dgvTransfers.Columns["ملاحظات"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                        }
                    }
                    catch (Exception colEx)
                    {
                        // تجاهل أخطاء تنسيق الأعمدة
                        System.Diagnostics.Debug.WriteLine($"Column formatting error: {colEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل التحويلات: {ex.Message}\n{ex.StackTrace}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetTransferTypeArabic(string transferType)
        {
            return transferType switch
            {
                "BankToBank" => "بنك → بنك",
                "BankToCash" => "بنك → خزنة",
                "CashToBank" => "خزنة → بنك",
                _ => transferType
            };
        }

        private void BtnFilter_Click(object? sender, EventArgs e)
        {
            LoadTransfers();
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            cmbTransferType.SelectedIndex = 0;
            cmbSourceBank.SelectedIndex = 0;
            cmbDestinationBank.SelectedIndex = 0;
            dtpFromDate.Value = DateTime.Now.AddMonths(-1);
            dtpToDate.Value = DateTime.Now;
            LoadTransfers();
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                if (dgvTransfers.Rows.Count == 0)
                {
                    MessageBox.Show("لا توجد بيانات للتصدير", "تنبيه",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    FileName = $"BankTransfersReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                    Title = "حفظ تقرير التحويلات"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    // Simple CSV export (can be upgraded to proper Excel later)
                    using (var writer = new System.IO.StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                    {
                        // Write headers
                        var headers = new List<string>();
                        foreach (DataGridViewColumn column in dgvTransfers.Columns)
                        {
                            headers.Add(column.HeaderText);
                        }
                        writer.WriteLine(string.Join(",", headers));

                        // Write rows
                        foreach (DataGridViewRow row in dgvTransfers.Rows)
                        {
                            var values = new List<string>();
                            foreach (DataGridViewCell cell in row.Cells)
                            {
                                values.Add($"\"{cell.Value?.ToString() ?? ""}\"");
                            }
                            writer.WriteLine(string.Join(",", values));
                        }
                    }

                    MessageBox.Show("تم تصدير التقرير بنجاح", "نجاح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تصدير التقرير: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPrint_Click(object? sender, EventArgs e)
        {
            try
            {
                if (dgvTransfers.Rows.Count == 0)
                {
                    MessageBox.Show("لا توجد بيانات للطباعة", "تنبيه",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create print dialog
                PrintDialog printDialog = new PrintDialog();
                System.Drawing.Printing.PrintDocument printDocument = new System.Drawing.Printing.PrintDocument();
                
                printDialog.Document = printDocument;
                
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDocument.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في الطباعة: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
