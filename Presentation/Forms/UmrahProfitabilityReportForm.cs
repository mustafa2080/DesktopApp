using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Presentation.Forms;

/// <summary>
/// تقرير ربحية حزم العمرة
/// </summary>
public partial class UmrahProfitabilityReportForm : Form
{
    private readonly IUmrahService _umrahService;
    private readonly IExportService _exportService;
    private ComboBox cmbPackage = null!;
    private DataGridView dgvProfitability = null!;
    private Panel pnlSummary = null!;
    private Button btnGenerate = null!;
    private Button btnExportExcel = null!;
    private Button btnExportPdf = null!;
    private Button btnPrint = null!;
    
    // Summary labels
    private Label lblRevenue = null!;
    private Label lblTotalCosts = null!;
    private Label lblProfit = null!;
    private Label lblProfitMargin = null!;
    private Label lblPilgrims = null!;
    
    private UmrahProfitabilityReport? _currentReport;

    public UmrahProfitabilityReportForm(IUmrahService umrahService, IExportService exportService)
    {
        _umrahService = umrahService;
        _exportService = exportService;
        InitializeComponent();
        InitializeCustomComponents();
        LoadPackages();
    }

    private void InitializeCustomComponents()
    {
        this.Text = "🕌 تقرير ربحية العمرة";
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
            Text = "🕌 تقرير ربحية العمرة (Umrah Profitability)",
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

        Label lblPackage = new Label
        {
            Text = "اختر حزمة العمرة:",
            Font = new Font("Cairo", 11F),
            Location = new Point(1150, 20),
            AutoSize = true
        };

        cmbPackage = new ComboBox
        {
            Location = new Point(850, 17),
            Size = new Size(290, 30),
            Font = new Font("Cairo", 10F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbPackage.SelectedIndexChanged += CmbPackage_SelectedIndexChanged;

        btnGenerate = new Button
        {
            Text = "📊 إنشاء التقرير",
            Location = new Point(650, 15),
            Size = new Size(180, 35),
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnGenerate.FlatAppearance.BorderSize = 0;
        btnGenerate.Click += BtnGenerate_Click;

        btnExportExcel = new Button
        {
            Text = "📥 Excel",
            Location = new Point(510, 15),
            Size = new Size(130, 35),
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
            Location = new Point(370, 15),
            Size = new Size(130, 35),
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
            Location = new Point(230, 15),
            Size = new Size(130, 35),
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            BackColor = Color.FromArgb(96, 125, 139),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnPrint.FlatAppearance.BorderSize = 0;
        btnPrint.Click += BtnPrint_Click;

        filterPanel.Controls.AddRange(new Control[] {
            lblPackage, cmbPackage, btnGenerate, btnExportExcel, btnExportPdf, btnPrint
        });

        // Summary Panel
        pnlSummary = new Panel
        {
            Location = new Point(30, 160),
            Size = new Size(1340, 150),
            BackColor = ColorScheme.Background,
            BorderStyle = BorderStyle.FixedSingle,
            Visible = false
        };
        CreateSummaryCards();

        // DataGridView
        dgvProfitability = new DataGridView
        {
            Location = new Point(30, 320),
            Size = new Size(1340, 500),
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

        dgvProfitability.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Primary;
        dgvProfitability.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvProfitability.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        dgvProfitability.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);

        mainPanel.Controls.AddRange(new Control[] {
            lblTitle, filterPanel, pnlSummary, dgvProfitability
        });

        this.Controls.Add(mainPanel);
    }

    private void CreateSummaryCards()
    {
        int cardWidth = 260;
        int cardHeight = 130;
        int spacing = 15;
        int xPos = 20;

        CreateSummaryCard(pnlSummary, "إجمالي الإيرادات", "💵", ref lblRevenue, 
            xPos, 10, cardWidth, cardHeight, ColorScheme.Success);
        xPos += cardWidth + spacing;

        CreateSummaryCard(pnlSummary, "إجمالي التكاليف", "💸", ref lblTotalCosts, 
            xPos, 10, cardWidth, cardHeight, ColorScheme.Error);
        xPos += cardWidth + spacing;

        CreateSummaryCard(pnlSummary, "صافي الربح", "💰", ref lblProfit, 
            xPos, 10, cardWidth, cardHeight, ColorScheme.Primary);
        xPos += cardWidth + spacing;

        CreateSummaryCard(pnlSummary, "هامش الربح", "📊", ref lblProfitMargin, 
            xPos, 10, cardWidth, cardHeight, Color.FromArgb(156, 39, 176));
        xPos += cardWidth + spacing;

        CreateSummaryCard(pnlSummary, "عدد المعتمرين", "👥", ref lblPilgrims, 
            xPos, 10, cardWidth, cardHeight, Color.FromArgb(255, 152, 0));
    }

    private void CreateSummaryCard(Panel parent, string title, string icon, 
        ref Label valueLabel, int x, int y, int width, int height, Color color)
    {
        Panel card = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        Label lblIcon = new Label
        {
            Text = icon,
            Font = new Font("Segoe UI Emoji", 24F),
            ForeColor = color,
            AutoSize = false,
            Size = new Size(60, 50),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(10, 10)
        };

        Label lblTitle = new Label
        {
            Text = title,
            Font = new Font("Cairo", 10F),
            ForeColor = Color.Gray,
            AutoSize = false,
            Size = new Size(180, 25),
            TextAlign = ContentAlignment.MiddleRight,
            Location = new Point(70, 15)
        };

        valueLabel = new Label
        {
            Text = "0.00",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = color,
            AutoSize = false,
            Size = new Size(240, 40),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(10, 70)
        };

        card.Controls.AddRange(new Control[] { lblIcon, lblTitle, valueLabel });
        parent.Controls.Add(card);
    }

    private async void LoadPackages()
    {
        try
        {
            Console.WriteLine("🔄 Loading Umrah packages...");
            
            var packages = await _umrahService.GetAllPackagesAsync(activeOnly: true);
            
            Console.WriteLine($"✅ Loaded {packages.Count} packages");

            var displayPackages = packages
                .OrderByDescending(p => p.Date)
                .Select(p => new { 
                    p.UmrahPackageId, 
                    DisplayText = p.PackageNumber + " - " + p.TripName + " (" + p.Date.ToString("yyyy/MM/dd") + ")"
                })
                .ToList();

            cmbPackage.DisplayMember = "DisplayText";
            cmbPackage.ValueMember = "UmrahPackageId";
            cmbPackage.DataSource = displayPackages;

            if (displayPackages.Any())
            {
                cmbPackage.SelectedIndex = 0;
                btnGenerate.Enabled = true;
            }
            else
            {
                MessageBox.Show(
                    "لا توجد حزم عمرة في النظام!\n\nيرجى إضافة حزم عمرة أولاً.",
                    "لا توجد بيانات",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error loading packages: {ex.Message}");
            MessageBox.Show($"خطأ في تحميل الحزم: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CmbPackage_SelectedIndexChanged(object? sender, EventArgs e)
    {
        btnGenerate.Enabled = cmbPackage.SelectedValue != null;
    }

    private async Task GenerateReportForSelectedPackage()
    {
        try
        {
            if (cmbPackage.SelectedValue == null) return;

            int packageId = (int)cmbPackage.SelectedValue;
            Console.WriteLine($"📊 Generating report for package ID: {packageId}");

            var package = await _umrahService.GetPackageByIdAsync(packageId);

            if (package == null)
            {
                MessageBox.Show("لم يتم العثور على الحزمة", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // استخدام method GetProfitabilityReportAsync من الـ Service
            var reports = await _umrahService.GetProfitabilityReportAsync(package.Date, package.Date);
            _currentReport = reports.FirstOrDefault(r => r.PackageId == packageId);

            if (_currentReport == null)
            {
                MessageBox.Show("فشل في إنشاء التقرير", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DisplayReport(_currentReport);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            MessageBox.Show($"خطأ في إنشاء التقرير: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnGenerate_Click(object? sender, EventArgs e)
    {
        await GenerateReportForSelectedPackage();
    }

    private void DisplayReport(UmrahProfitabilityReport report)
    {
        lblRevenue.Text = $"{report.TotalRevenue:N0} جنيه";
        lblTotalCosts.Text = $"{report.TotalCosts:N0} جنيه";
        lblProfit.Text = report.NetProfit >= 0 ? $"{report.NetProfit:N0} جنيه" : $"({Math.Abs(report.NetProfit):N0}) جنيه خسارة";
        lblProfit.ForeColor = report.NetProfit >= 0 ? ColorScheme.Success : ColorScheme.Error;
        lblProfitMargin.Text = $"{report.ProfitMargin:N2}%";
        lblPilgrims.Text = $"{report.NumberOfPersons} معتمر";

        pnlSummary.Visible = true;

        dgvProfitability.Columns.Clear();
        dgvProfitability.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Category",
            HeaderText = "البند",
            Width = 400
        });
        dgvProfitability.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Value",
            HeaderText = "القيمة (جنيه)",
            Width = 200,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleCenter }
        });
        dgvProfitability.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "PerPerson",
            HeaderText = "للفرد",
            Width = 150,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleCenter }
        });

        dgvProfitability.Rows.Clear();

        AddHeaderRow("💰 الإيرادات");
        dgvProfitability.Rows.Add("إيرادات الحجوزات", report.TotalRevenue, report.RevenuePerPerson);
        dgvProfitability.Rows.Add($"  ✓ عدد المعتمرين: {report.NumberOfPersons}", "", "");
        AddTotalRow("إجمالي الإيرادات", report.TotalRevenue);
        dgvProfitability.Rows.Add("", "", "");

        AddHeaderRow("💸 التكاليف");
        if (report.VisaCost > 0) dgvProfitability.Rows.Add("  💳 التأشيرة", report.VisaCost, report.VisaCost / report.NumberOfPersons);
        if (report.AccommodationCost > 0) dgvProfitability.Rows.Add("  🏨 الإقامة", report.AccommodationCost, report.AccommodationCost / report.NumberOfPersons);
        if (report.BarcodeCost > 0) dgvProfitability.Rows.Add("  🎫 الباركود", report.BarcodeCost, report.BarcodeCost / report.NumberOfPersons);
        if (report.SupervisorBarcodeCost > 0)
        {
            int supBRow = dgvProfitability.Rows.Add("  🔖 باركود المشرف", report.SupervisorBarcodeCost, $"⚠️ خاص بالمشرف");
            dgvProfitability.Rows[supBRow].DefaultCellStyle.ForeColor = Color.FromArgb(183, 28, 28);
            dgvProfitability.Rows[supBRow].DefaultCellStyle.Font = new Font("Cairo", 10F, FontStyle.Bold);
        }
        if (report.FlightCost > 0) dgvProfitability.Rows.Add("  ✈️ الطيران", report.FlightCost, report.FlightCost / report.NumberOfPersons);
        if (report.FastTrainCost > 0) dgvProfitability.Rows.Add("  🚄 القطار السريع", report.FastTrainCost, report.FastTrainCost / report.NumberOfPersons);
        if (report.BusCost > 0) dgvProfitability.Rows.Add("  🚌 الباصات", report.BusCost, "-");
        if (report.GiftsCost > 0) dgvProfitability.Rows.Add("  🎁 الهدايا", report.GiftsCost, "-");
        if (report.OtherExpensesCost > 0) dgvProfitability.Rows.Add("  📦 مصروفات أخرى", report.OtherExpensesCost, report.OtherExpensesCost / report.NumberOfPersons);
        if (report.BrokerCommission > 0) dgvProfitability.Rows.Add("  👨‍💼 عمولة الوسيط", report.BrokerCommission, "-");
        if (report.SupervisorExpenses > 0) dgvProfitability.Rows.Add("  👤 مصاريف المشرف", report.SupervisorExpenses, "-");
        AddTotalRow("إجمالي التكاليف", report.TotalCosts, true);
        dgvProfitability.Rows.Add("", "", "");

        string profitTitle = report.NetProfit >= 0 ? "💎 صافي الربح" : "⚠️ صافي الخسارة";
        AddProfitRow(profitTitle, report.NetProfit, report.ProfitPerPerson);
        dgvProfitability.Rows.Add("", "", "");

        AddHeaderRow("📊 مؤشرات الأداء");
        dgvProfitability.Rows.Add("  ✦ متوسط الإيراد للمعتمر", "", report.RevenuePerPerson);
        dgvProfitability.Rows.Add("  ✦ متوسط التكلفة للمعتمر", "", report.CostPerPerson);
        dgvProfitability.Rows.Add("  ✦ متوسط الربح للمعتمر", "", report.ProfitPerPerson);
        dgvProfitability.Rows.Add("  ✦ هامش الربح %", "", report.ProfitMargin);

        btnExportExcel.Enabled = true;
        btnExportPdf.Enabled = true;
        btnPrint.Enabled = true;
    }

    private void AddHeaderRow(string title)
    {
        int idx = dgvProfitability.Rows.Add(title, "", "");
        dgvProfitability.Rows[idx].DefaultCellStyle.BackColor = ColorScheme.Primary;
        dgvProfitability.Rows[idx].DefaultCellStyle.ForeColor = Color.White;
        dgvProfitability.Rows[idx].DefaultCellStyle.Font = new Font("Cairo", 12F, FontStyle.Bold);
    }

    private void AddTotalRow(string title, decimal value, bool isExpense = false)
    {
        int idx = dgvProfitability.Rows.Add(title, value, "");
        dgvProfitability.Rows[idx].DefaultCellStyle.BackColor = isExpense ? Color.FromArgb(255, 205, 210) : Color.FromArgb(200, 230, 201);
        dgvProfitability.Rows[idx].DefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
    }

    private void AddProfitRow(string title, decimal profit, decimal profitPerPerson)
    {
        // عرض القيمة المطلقة للخسارة مع علامة سالبة
        string profitValue = profit >= 0 ? $"{profit:N0}" : $"({Math.Abs(profit):N0})";
        string profitPerPersonValue = profitPerPerson >= 0 ? $"{profitPerPerson:N0}" : $"({Math.Abs(profitPerPerson):N0})";
        
        int idx = dgvProfitability.Rows.Add(title, profitValue, profitPerPersonValue);
        dgvProfitability.Rows[idx].DefaultCellStyle.BackColor = profit >= 0 ? ColorScheme.Success : ColorScheme.Error;
        dgvProfitability.Rows[idx].DefaultCellStyle.ForeColor = Color.White;
        dgvProfitability.Rows[idx].DefaultCellStyle.Font = new Font("Cairo", 14F, FontStyle.Bold);
    }

    private async void BtnExportExcel_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_currentReport == null) return;

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "حفظ تقرير ربحية العمرة",
                FileName = $"ربحية_عمرة_{_currentReport.PackageNumber}_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                await _exportService.ExportToExcelAsync(dgvProfitability, saveDialog.FileName, "ربحية العمرة");
                MessageBox.Show("تم التصدير بنجاح!", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في التصدير: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnExportPdf_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_currentReport == null) return;

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "HTML Files|*.html",
                Title = "حفظ تقرير ربحية العمرة",
                FileName = $"ربحية_عمرة_{_currentReport.PackageNumber}_{DateTime.Now:yyyyMMdd}.html"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                string profitLabel = _currentReport.NetProfit >= 0 ? "صافي الربح" : "صافي الخسارة";
                string profitValue = _currentReport.NetProfit >= 0 ? 
                    $"{_currentReport.NetProfit:N0} جنيه" : 
                    $"({Math.Abs(_currentReport.NetProfit):N0}) جنيه";
                
                var metadata = new Dictionary<string, string>
                {
                    { "رقم الحزمة", _currentReport.PackageNumber },
                    { "اسم الرحلة", _currentReport.TripName },
                    { "التاريخ", _currentReport.Date.ToString("yyyy/MM/dd") },
                    { "عدد المعتمرين", _currentReport.NumberOfPersons.ToString() },
                    { "الإيرادات", $"{_currentReport.TotalRevenue:N0} جنيه" },
                    { "التكاليف", $"{_currentReport.TotalCosts:N0} جنيه" },
                    { profitLabel, profitValue }
                };

                await _exportService.ExportToPdfAsync(dgvProfitability, saveDialog.FileName, "🕌 ربحية العمرة", metadata);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في التصدير: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnPrint_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_currentReport == null) return;

            // إنشاء PrintDocument
            System.Drawing.Printing.PrintDocument printDoc = new System.Drawing.Printing.PrintDocument();
            printDoc.DocumentName = $"ربحية_عمرة_{_currentReport.PackageNumber}";
            
            // إضافة حدث الطباعة
            printDoc.PrintPage += (s, ev) =>
            {
                if (ev.Graphics == null) return;

                Font titleFont = new Font("Cairo", 16, FontStyle.Bold);
                Font headerFont = new Font("Cairo", 12, FontStyle.Bold);
                Font normalFont = new Font("Cairo", 10);
                
                int y = 50;
                int x = 50;
                int pageWidth = ev.PageBounds.Width - 100;

                // العنوان
                string title = $"🕌 تقرير ربحية العمرة - {_currentReport.PackageNumber}";
                ev.Graphics.DrawString(title, titleFont, Brushes.Black, x, y);
                y += 40;

                // معلومات الحزمة
                ev.Graphics.DrawString($"اسم الرحلة: {_currentReport.TripName}", normalFont, Brushes.Black, x, y);
                y += 25;
                ev.Graphics.DrawString($"التاريخ: {_currentReport.Date:yyyy/MM/dd}     عدد المعتمرين: {_currentReport.NumberOfPersons}", normalFont, Brushes.Black, x, y);
                y += 35;

                // الملخص
                ev.Graphics.DrawString("الملخص المالي:", headerFont, Brushes.DarkBlue, x, y);
                y += 30;
                ev.Graphics.DrawString($"إجمالي الإيرادات: {_currentReport.TotalRevenue:N0} جنيه", normalFont, Brushes.Green, x + 20, y);
                y += 25;
                ev.Graphics.DrawString($"إجمالي التكاليف: {_currentReport.TotalCosts:N0} جنيه", normalFont, Brushes.Red, x + 20, y);
                y += 25;
                string profitLabel = _currentReport.NetProfit >= 0 ? "صافي الربح" : "صافي الخسارة";
                string profitText = _currentReport.NetProfit >= 0 ? $"{_currentReport.NetProfit:N0}" : $"({Math.Abs(_currentReport.NetProfit):N0})";
                ev.Graphics.DrawString($"{profitLabel}: {profitText} جنيه", normalFont, _currentReport.NetProfit >= 0 ? Brushes.Green : Brushes.Red, x + 20, y);
                y += 25;
                ev.Graphics.DrawString($"هامش الربح: {_currentReport.ProfitMargin:N2}%", normalFont, Brushes.Black, x + 20, y);
                y += 40;

                // جدول التفاصيل
                ev.Graphics.DrawString("تفاصيل التكاليف:", headerFont, Brushes.DarkBlue, x, y);
                y += 30;

                foreach (DataGridViewRow row in dgvProfitability.Rows)
                {
                    if (y > ev.PageBounds.Height - 100) break; // حماية من الخروج عن الصفحة

                    string col1 = row.Cells[0].Value?.ToString() ?? "";
                    string col2 = row.Cells[1].Value?.ToString() ?? "";
                    string col3 = row.Cells[2].Value?.ToString() ?? "";

                    if (!string.IsNullOrWhiteSpace(col1))
                    {
                        Font rowFont = row.DefaultCellStyle.Font ?? normalFont;
                        Brush brush = new SolidBrush(row.DefaultCellStyle.ForeColor != Color.Empty ? row.DefaultCellStyle.ForeColor : Color.Black);
                        
                        ev.Graphics.DrawString(col1, rowFont, brush, x, y);
                        if (!string.IsNullOrWhiteSpace(col2))
                        {
                            ev.Graphics.DrawString(col2, rowFont, brush, x + 400, y);
                        }
                        if (!string.IsNullOrWhiteSpace(col3) && col3 != "-")
                        {
                            ev.Graphics.DrawString(col3, rowFont, brush, x + 600, y);
                        }
                        
                        y += 22;
                    }
                }

                // تذييل
                y = ev.PageBounds.Height - 50;
                ev.Graphics.DrawString($"طُبع في: {DateTime.Now:yyyy/MM/dd HH:mm}", new Font("Cairo", 8), Brushes.Gray, x, y);
            };

            // عرض معاينة الطباعة
            System.Windows.Forms.PrintPreviewDialog previewDialog = new System.Windows.Forms.PrintPreviewDialog
            {
                Document = printDoc,
                Width = 1000,
                Height = 700,
                StartPosition = FormStartPosition.CenterParent,
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true
            };

            previewDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في الطباعة: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
