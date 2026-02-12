using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Presentation;

namespace GraceWay.AccountingSystem.Presentation.Forms
{
    public partial class ReservationReportsForm : Form
    {
        private readonly IReservationService _reservationService;

        private DateTimePicker dtpStartDate = null!;
        private DateTimePicker dtpEndDate = null!;
        private Button btnGenerate = null!;
        private Button btnExport = null!;
        private Button btnPrint = null!;
        private Panel pnlSummary = null!;
        private DataGridView dgvReport = null!;
        
        // For printing
        private int currentRow = 0;
        private bool hasMorePages = false;

        public ReservationReportsForm(IReservationService reservationService)
        {
            _reservationService = reservationService;
            
            InitializeComponent();
            InitializeFormControls();
        }

        private void InitializeFormControls()
        {
            this.BackColor = ColorScheme.Background;
            this.Size = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "تقارير الحجوزات";
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // Header Panel
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            Label lblTitle = new Label
            {
                Text = "📊 تقارير الحجوزات",
                Font = new Font("Cairo", 16F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            headerPanel.Controls.Add(lblTitle);

            // Date Range
            Label lblStartDate = new Label
            {
                Text = "من تاريخ:",
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 70)
            };
            headerPanel.Controls.Add(lblStartDate);

            dtpStartDate = new DateTimePicker
            {
                Font = new Font("Cairo", 10F),
                Size = new Size(200, 30),
                Location = new Point(120, 67),
                Format = DateTimePickerFormat.Short
            };
            dtpStartDate.Value = DateTime.Now.AddMonths(-1);
            headerPanel.Controls.Add(dtpStartDate);

            Label lblEndDate = new Label
            {
                Text = "إلى تاريخ:",
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(340, 70)
            };
            headerPanel.Controls.Add(lblEndDate);

            dtpEndDate = new DateTimePicker
            {
                Font = new Font("Cairo", 10F),
                Size = new Size(200, 30),
                Location = new Point(440, 67),
                Format = DateTimePickerFormat.Short
            };
            headerPanel.Controls.Add(dtpEndDate);

            btnGenerate = CreateButton("📈 إنشاء التقرير", ColorScheme.Success, new Point(660, 67), GenerateReport_Click);
            headerPanel.Controls.Add(btnGenerate);

            btnExport = CreateButton("📤 تصدير Excel", Color.FromArgb(0, 166, 90), new Point(840, 67), ExportReport_Click);
            headerPanel.Controls.Add(btnExport);
            
            btnPrint = CreateButton("🖨️ طباعة", ColorScheme.Warning, new Point(1020, 67), PrintReport_Click);
            headerPanel.Controls.Add(btnPrint);

            this.Controls.Add(headerPanel);

            // Main Panel
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorScheme.Background,
                Padding = new Padding(20)
            };

            // Summary Panel
            pnlSummary = new Panel
            {
                Dock = DockStyle.Top,
                Height = 200,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            mainPanel.Controls.Add(pnlSummary);

            // Report Grid
            dgvReport = CreateDataGrid();
            mainPanel.Controls.Add(dgvReport);

            this.Controls.Add(mainPanel);
        }

        private DataGridView CreateDataGrid()
        {
            var grid = new DataGridView
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
                Font = new Font("Cairo", 12F),
                RightToLeft = RightToLeft.Yes,
                EnableHeadersVisualStyles = false
            };

            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = ColorScheme.Primary,
                ForeColor = Color.White,
                Font = new Font("Cairo", 13F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(10)
            };

            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Cairo", 12F),
                Padding = new Padding(10),
                SelectionBackColor = ColorScheme.Primary,
                SelectionForeColor = Color.White,
                ForeColor = Color.FromArgb(33, 33, 33)
            };

            grid.ColumnHeadersHeight = 65;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.RowTemplate.Height = 52;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);

            return grid;
        }

        private Button CreateButton(string text, Color bgColor, Point location, EventHandler clickHandler)
        {
            Button btn = new Button
            {
                Text = text,
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                Size = new Size(160, 40),
                Location = location,
                BackColor = bgColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += clickHandler;
            return btn;
        }

        private async void GenerateReport_Click(object? sender, EventArgs e)
        {
            try
            {
                DateTime startDate = dtpStartDate.Value.Date;
                DateTime endDate = dtpEndDate.Value.Date;

                if (startDate > endDate)
                {
                    MessageBox.Show("تاريخ البداية يجب أن يكون قبل تاريخ النهاية", "تنبيه",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get statistics
                var stats = await _reservationService.GetReservationStatisticsAsync(startDate, endDate);
                DisplaySummary(stats);

                // Get reservations
                var reservations = await _reservationService.GetReservationsByDateRangeAsync(startDate, endDate);
                DisplayReservations(reservations);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في إنشاء التقرير: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplaySummary(Dictionary<string, decimal> stats)
        {
            pnlSummary.Controls.Clear();

            Label titleLabel = new Label
            {
                Text = "ملخص الإحصائيات",
                Font = new Font("Cairo", 14F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            pnlSummary.Controls.Add(titleLabel);

            int y = 60;
            int x1 = 20, x2 = 300, x3 = 580, x4 = 860;

            AddSummaryCard("إجمالي الحجوزات", stats["TotalReservations"].ToString("N0"), x1, y, ColorScheme.Primary);
            AddSummaryCard("إجمالي المبيعات", stats["TotalSales"].ToString("N2") + " جنيه", x2, y, ColorScheme.Secondary);
            AddSummaryCard("إجمالي التكاليف", stats["TotalCosts"].ToString("N2") + " جنيه", x3, y, ColorScheme.Warning);
            AddSummaryCard("صافي الربح", stats["TotalProfit"].ToString("N2") + " جنيه", x4, y, ColorScheme.Success);

            y = 130;
            AddSummaryLabel($"مؤكدة: {stats["ConfirmedCount"]:N0}", x1, y);
            AddSummaryLabel($"مكتملة: {stats["CompletedCount"]:N0}", x2, y);
            AddSummaryLabel($"ملغاة: {stats["CancelledCount"]:N0}", x3, y);
            AddSummaryLabel($"مسودات: {stats["DraftCount"]:N0}", x4, y);
        }

        private void AddSummaryCard(string title, string value, int x, int y, Color color)
        {
            Panel card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(260, 60),
                BackColor = Color.FromArgb(240, 248, 255),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Cairo", 9F),
                ForeColor = Color.Gray,
                Location = new Point(10, 10),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            Label lblValue = new Label
            {
                Text = value,
                Font = new Font("Cairo", 12F, FontStyle.Bold),
                ForeColor = color,
                Location = new Point(10, 30),
                AutoSize = true
            };
            card.Controls.Add(lblValue);

            pnlSummary.Controls.Add(card);
        }

        private void AddSummaryLabel(string text, int x, int y)
        {
            Label lbl = new Label
            {
                Text = text,
                Font = new Font("Cairo", 10F),
                AutoSize = true,
                Location = new Point(x, y)
            };
            pnlSummary.Controls.Add(lbl);
        }

        private void DisplayReservations(System.Collections.Generic.List<GraceWay.AccountingSystem.Domain.Entities.Reservation> reservations)
        {
            dgvReport.Columns.Clear();

            dgvReport.Columns.Add(new DataGridViewTextBoxColumn { Name = "ReservationNumber", HeaderText = "رقم الحجز", Width = 120 });
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "التاريخ", Width = 120 });
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn { Name = "Customer", HeaderText = "العميل", Width = 200 });
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn { Name = "ServiceType", HeaderText = "نوع الخدمة", Width = 150 });
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn { Name = "People", HeaderText = "عدد الأفراد", Width = 100 });
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn { Name = "SellingPrice", HeaderText = "سعر البيع", Width = 120 });
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn { Name = "CostPrice", HeaderText = "التكلفة", Width = 120 });
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn { Name = "Profit", HeaderText = "الربح", Width = 120 });
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn { Name = "CashBox", HeaderText = "الخزنة", Width = 120 });
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "الحالة", Width = 100 });

            dgvReport.Rows.Clear();

            foreach (var reservation in reservations)
            {
                dgvReport.Rows.Add(
                    reservation.ReservationNumber,
                    reservation.ReservationDate.ToString("yyyy-MM-dd"),
                    reservation.Customer?.CustomerName ?? "",
                    reservation.ServiceType?.ServiceTypeName ?? "",
                    reservation.NumberOfPeople,
                    reservation.SellingPrice.ToString("N2"),
                    reservation.CostPrice.ToString("N2"),
                    reservation.ProfitAmount.ToString("N2"),
                    reservation.CashBox?.Name ?? "-",
                    GetStatusInArabic(reservation.Status)
                );
            }
        }

        private string GetStatusInArabic(string status)
        {
            return status switch
            {
                "Draft" => "مسودة",
                "Confirmed" => "مؤكد",
                "Completed" => "مكتمل",
                "Cancelled" => "ملغي",
                _ => status
            };
        }

        private void ExportReport_Click(object? sender, EventArgs e)
        {
            try
            {
                if (dgvReport.Rows.Count == 0)
                {
                    MessageBox.Show("لا توجد بيانات للتصدير", "تنبيه",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    DefaultExt = "csv",
                    FileName = $"تقرير_الحجوزات_{DateTime.Now:yyyy-MM-dd}.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportToCSV(dgvReport, saveDialog.FileName);
                    MessageBox.Show("تم تصدير التقرير بنجاح", "نجح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في التصدير: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCSV(DataGridView grid, string fileName)
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(fileName, false, System.Text.Encoding.UTF8))
            {
                // Headers
                var headers = new System.Collections.Generic.List<string>();
                foreach (DataGridViewColumn column in grid.Columns)
                {
                    if (column.Visible)
                        headers.Add(column.HeaderText);
                }
                writer.WriteLine(string.Join(",", headers));

                // Rows
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row.IsNewRow) continue;

                    var values = new System.Collections.Generic.List<string>();
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (cell.OwningColumn?.Visible == true)
                        {
                            string value = cell.Value?.ToString() ?? "";
                            if (value.Contains(",") || value.Contains("\""))
                            {
                                value = $"\"{value.Replace("\"", "\"\"")}\"";
                            }
                            values.Add(value);
                        }
                    }
                    writer.WriteLine(string.Join(",", values));
                }
            }
        }
        
        private void PrintReport_Click(object? sender, EventArgs e)
        {
            try
            {
                if (dgvReport.Rows.Count == 0)
                {
                    MessageBox.Show("لا توجد بيانات للطباعة", "تنبيه",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics!;
                Font titleFont = new Font("Cairo", 16, FontStyle.Bold);
                Font headerFont = new Font("Cairo", 10, FontStyle.Bold);
                Font normalFont = new Font("Cairo", 8);
                Font smallFont = new Font("Cairo", 7);

                float y = e.MarginBounds.Top;
                float x = e.MarginBounds.Left;
                float pageWidth = e.MarginBounds.Width;

                // Title
                string title = "📊 تقرير الحجوزات";
                SizeF titleSize = g.MeasureString(title, titleFont);
                g.DrawString(title, titleFont, Brushes.Black, x + (pageWidth - titleSize.Width) / 2, y);
                y += titleSize.Height + 10;

                // Date range
                string dateRange = $"من {dtpStartDate.Value:yyyy/MM/dd} إلى {dtpEndDate.Value:yyyy/MM/dd}";
                SizeF dateSize = g.MeasureString(dateRange, normalFont);
                g.DrawString(dateRange, normalFont, Brushes.Black, x + (pageWidth - dateSize.Width) / 2, y);
                y += dateSize.Height + 20;

                // Calculate column widths
                int visibleColumns = dgvReport.Columns.Cast<DataGridViewColumn>().Count(c => c.Visible);
                float columnWidth = pageWidth / visibleColumns;

                // Draw headers
                float currentX = x + pageWidth;
                g.FillRectangle(new SolidBrush(ColorScheme.Primary), x, y, pageWidth, 25);
                
                for (int i = dgvReport.Columns.Count - 1; i >= 0; i--)
                {
                    if (!dgvReport.Columns[i].Visible) continue;
                    currentX -= columnWidth;
                    g.DrawString(dgvReport.Columns[i].HeaderText, headerFont, Brushes.White, currentX + 3, y + 5);
                }
                
                y += 28;

                // Draw rows
                int rowsPerPage = (int)((e.MarginBounds.Bottom - y - 50) / 20);
                int rowsPrinted = 0;

                for (int i = currentRow; i < dgvReport.Rows.Count && rowsPrinted < rowsPerPage; i++)
                {
                    DataGridViewRow row = dgvReport.Rows[i];
                    
                    if (i % 2 == 1)
                    {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(240, 248, 255)), x, y, pageWidth, 20);
                    }

                    currentX = x + pageWidth;
                    for (int j = dgvReport.Columns.Count - 1; j >= 0; j--)
                    {
                        if (!dgvReport.Columns[j].Visible) continue;
                        currentX -= columnWidth;
                        
                        string cellValue = row.Cells[j].Value?.ToString() ?? "";
                        g.DrawString(cellValue, normalFont, Brushes.Black, currentX + 3, y + 2);
                    }

                    y += 20;
                    rowsPrinted++;
                    currentRow++;
                }

                hasMorePages = currentRow < dgvReport.Rows.Count;

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
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
