using GraceWay.AccountingSystem.Domain.Entities;
using ClosedXML.Excel;
using System.Drawing.Printing;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class SupplierStatementForm : Form
{
    private readonly SupplierStatement _statement;
    
    private DataGridView _transactionsGrid = null!;
    private Label _supplierNameLabel = null!;
    private Label _supplierCodeLabel = null!;
    private Label _openingBalanceLabel = null!;
    private Label _closingBalanceLabel = null!;
    private Label _totalDebitLabel = null!;
    private Label _totalCreditLabel = null!;
    private Button _printButton = null!;
    private Button _exportButton = null!;
    private Button _closeButton = null!;
    
    public SupplierStatementForm(SupplierStatement statement)
    {
        _statement = statement;
        
        InitializeComponent();
        
        this.Text = "كشف حساب المورد";
        this.Size = new Size(1400, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
        
        InitializeCustomControls();
        LoadData();
    }
    
    private void InitializeCustomControls()
    {
        // Main Panel
        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ColorScheme.Background,
            Padding = new Padding(20)
        };
        
        int yPos = 20;
        
        // ==================== HEADER ====================
        Panel headerPanel = CreateHeaderPanel();
        headerPanel.Location = new Point(20, yPos);
        mainPanel.Controls.Add(headerPanel);
        yPos += headerPanel.Height + 15;
        
        // ==================== SUPPLIER INFO ====================
        Panel infoPanel = CreateSupplierInfoPanel();
        infoPanel.Location = new Point(20, yPos);
        mainPanel.Controls.Add(infoPanel);
        yPos += infoPanel.Height + 15;
        
        // ==================== TRANSACTIONS GRID ====================
        _transactionsGrid = CreateTransactionsGrid();
        _transactionsGrid.Location = new Point(20, yPos);
        _transactionsGrid.Size = new Size(
            this.ClientSize.Width - 40,
            this.ClientSize.Height - yPos - 150
        );
        mainPanel.Controls.Add(_transactionsGrid);
        yPos += _transactionsGrid.Height + 15;
        
        // ==================== SUMMARY ====================
        Panel summaryPanel = CreateSummaryPanel();
        summaryPanel.Location = new Point(20, yPos);
        mainPanel.Controls.Add(summaryPanel);
        yPos += summaryPanel.Height + 15;
        
        // ==================== BUTTONS ====================
        Panel buttonsPanel = CreateButtonsPanel();
        buttonsPanel.Location = new Point(20, yPos);
        mainPanel.Controls.Add(buttonsPanel);
        
        this.Controls.Add(mainPanel);
    }
    
    private Panel CreateHeaderPanel()
    {
        Panel panel = new Panel
        {
            Size = new Size(this.ClientSize.Width - 40, 80),
            BackColor = Color.White
        };
        
        // Icon
        Label icon = new Label
        {
            Text = "📊",
            Font = new Font("Segoe UI Emoji", 28F),
            AutoSize = true,
            Location = new Point(panel.Width - 60, 20)
        };
        panel.Controls.Add(icon);
        
        // Title
        Label title = new Label
        {
            Text = "كشف حساب المورد",
            Font = new Font("Cairo", 18F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(panel.Width - 280, 15)
        };
        panel.Controls.Add(title);
        
        // Date
        Label dateLabel = new Label
        {
            Text = $"التاريخ: {DateTime.Now:yyyy/MM/dd}",
            Font = new Font("Cairo", 9F),
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(panel.Width - 250, 50)
        };
        panel.Controls.Add(dateLabel);
        
        return panel;
    }
    
    private Panel CreateSupplierInfoPanel()
    {
        Panel panel = new Panel
        {
            Size = new Size(this.ClientSize.Width - 40, 100),
            BackColor = Color.FromArgb(240, 248, 255),
            BorderStyle = BorderStyle.FixedSingle
        };
        
        int xPos = panel.Width - 50;
        
        // Supplier Name
        Label nameTitle = new Label
        {
            Text = "اسم المورد:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(xPos, 15)
        };
        panel.Controls.Add(nameTitle);
        xPos -= nameTitle.Width + 10;
        
        _supplierNameLabel = new Label
        {
            Text = "",
            Font = new Font("Cairo", 10F),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(xPos - 200, 15)
        };
        panel.Controls.Add(_supplierNameLabel);
        
        xPos = panel.Width - 50;
        
        // Supplier Code
        Label codeTitle = new Label
        {
            Text = "الكود:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(xPos, 45)
        };
        panel.Controls.Add(codeTitle);
        xPos -= codeTitle.Width + 10;
        
        _supplierCodeLabel = new Label
        {
            Text = "",
            Font = new Font("Cairo", 10F),
            AutoSize = true,
            Location = new Point(xPos - 100, 45)
        };
        panel.Controls.Add(_supplierCodeLabel);
        
        // Opening Balance
        Label openingTitle = new Label
        {
            Text = "الرصيد الافتتاحي:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(xPos - 300, 45)
        };
        panel.Controls.Add(openingTitle);
        
        _openingBalanceLabel = new Label
        {
            Text = "",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(xPos - 450, 45)
        };
        panel.Controls.Add(_openingBalanceLabel);
        
        return panel;
    }
    
    private DataGridView CreateTransactionsGrid()
    {
        DataGridView grid = new DataGridView
        {
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            Font = new Font("Cairo", 9.5F),
            RightToLeft = RightToLeft.Yes,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            EnableHeadersVisualStyles = false,
            RowHeadersVisible = false,
            AllowUserToResizeRows = false
        };
        
        // Configure columns
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Date",
            HeaderText = "📅 التاريخ",
            Width = 100,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Format = "yyyy/MM/dd"
            }
        });
        
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Type",
            HeaderText = "📋 النوع",
            Width = 120,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Cairo", 9.5F, FontStyle.Bold)
            }
        });
        
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ReferenceNumber",
            HeaderText = "🔢 رقم المرجع",
            Width = 120,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Arial", 9F)
            }
        });
        
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Description",
            HeaderText = "📝 الوصف",
            Width = 250,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleRight,
                Padding = new Padding(10, 0, 10, 0)
            }
        });
        
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Debit",
            HeaderText = "💵 مدين (سداد)",
            Width = 120,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "N2",
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Arial", 10F, FontStyle.Bold),
                ForeColor = Color.Red
            }
        });
        
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Credit",
            HeaderText = "💰 دائن (فاتورة)",
            Width = 120,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "N2",
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Arial", 10F, FontStyle.Bold),
                ForeColor = Color.Green
            }
        });
        
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Balance",
            HeaderText = "⚖️ الرصيد",
            Width = 130,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "N2",
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Arial", 11F, FontStyle.Bold)
            }
        });
        
        // Header styling
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            Font = new Font("Cairo", 10.5F, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleCenter,
            Padding = new Padding(8),
            SelectionBackColor = ColorScheme.Primary
        };
        
        grid.ColumnHeadersHeight = 45;
        grid.RowTemplate.Height = 40;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
        
        return grid;
    }
    
    private Panel CreateSummaryPanel()
    {
        Panel panel = new Panel
        {
            Size = new Size(this.ClientSize.Width - 40, 70),
            BackColor = Color.FromArgb(255, 255, 220),
            BorderStyle = BorderStyle.FixedSingle
        };
        
        int xPos = panel.Width - 50;
        
        // Total Debit
        Label debitTitle = new Label
        {
            Text = "إجمالي المدين (السدادات):",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(xPos, 15)
        };
        panel.Controls.Add(debitTitle);
        xPos -= debitTitle.Width + 10;
        
        _totalDebitLabel = new Label
        {
            Text = "",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = Color.Red,
            AutoSize = true,
            Location = new Point(xPos - 100, 15)
        };
        panel.Controls.Add(_totalDebitLabel);
        
        xPos -= 250;
        
        // Total Credit
        Label creditTitle = new Label
        {
            Text = "إجمالي الدائن (الفواتير):",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(xPos, 15)
        };
        panel.Controls.Add(creditTitle);
        xPos -= creditTitle.Width + 10;
        
        _totalCreditLabel = new Label
        {
            Text = "",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = Color.Green,
            AutoSize = true,
            Location = new Point(xPos - 100, 15)
        };
        panel.Controls.Add(_totalCreditLabel);
        
        xPos = panel.Width - 50;
        
        // Closing Balance
        Label closingTitle = new Label
        {
            Text = "الرصيد الختامي:",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(xPos, 45)
        };
        panel.Controls.Add(closingTitle);
        xPos -= closingTitle.Width + 10;
        
        _closingBalanceLabel = new Label
        {
            Text = "",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(xPos - 150, 42)
        };
        panel.Controls.Add(_closingBalanceLabel);
        
        return panel;
    }
    
    private Panel CreateButtonsPanel()
    {
        Panel panel = new Panel
        {
            Size = new Size(this.ClientSize.Width - 40, 60),
            BackColor = Color.Transparent
        };
        
        int buttonX = panel.Width - 180;
        
        // Close Button
        _closeButton = CreateStyledButton(
            "❌ إغلاق",
            Color.FromArgb(108, 117, 125),
            new Point(buttonX, 10),
            (s, e) => this.Close()
        );
        panel.Controls.Add(_closeButton);
        buttonX -= 180;
        
        // Print Button
        _printButton = CreateStyledButton(
            "🖨️ طباعة",
            ColorScheme.Primary,
            new Point(buttonX, 10),
            PrintStatement_Click
        );
        panel.Controls.Add(_printButton);
        buttonX -= 180;
        
        // Export Button
        _exportButton = CreateStyledButton(
            "📊 تصدير Excel",
            Color.FromArgb(40, 167, 69),
            new Point(buttonX, 10),
            ExportToExcel_Click
        );
        panel.Controls.Add(_exportButton);
        
        return panel;
    }
    
    private Button CreateStyledButton(string text, Color bgColor, Point location, EventHandler clickHandler)
    {
        Button btn = new Button
        {
            Text = text,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Size = new Size(170, 45),
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
    
    private void LoadData()
    {
        // Set supplier info
        _supplierNameLabel.Text = _statement.SupplierName;
        _supplierCodeLabel.Text = _statement.SupplierCode;
        _openingBalanceLabel.Text = $"{_statement.OpeningBalance:N2} ج.م";
        _openingBalanceLabel.ForeColor = _statement.OpeningBalance >= 0 ? Color.Green : Color.Red;
        
        // Load transactions
        _transactionsGrid.Rows.Clear();
        
        decimal totalDebit = 0;
        decimal totalCredit = 0;
        
        foreach (var line in _statement.Transactions)
        {
            int rowIndex = _transactionsGrid.Rows.Add(
                line.Date,
                line.Type,
                line.ReferenceNumber,
                line.Description,
                line.Debit,
                line.Credit,
                line.Balance
            );
            
            // Color code balance
            var balanceCell = _transactionsGrid.Rows[rowIndex].Cells["Balance"];
            balanceCell.Style.ForeColor = line.Balance >= 0 ? Color.Green : Color.Red;
            
            // Highlight opening balance row
            if (line.Type == "رصيد افتتاحي")
            {
                _transactionsGrid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 200);
                _transactionsGrid.Rows[rowIndex].DefaultCellStyle.Font = new Font("Cairo", 9.5F, FontStyle.Bold);
            }
            
            totalDebit += line.Debit;
            totalCredit += line.Credit;
        }
        
        // Set summary
        _totalDebitLabel.Text = $"{totalDebit:N2} ج.م";
        _totalCreditLabel.Text = $"{totalCredit:N2} ج.م";
        _closingBalanceLabel.Text = $"{_statement.ClosingBalance:N2} ج.م";
        _closingBalanceLabel.ForeColor = _statement.ClosingBalance >= 0 ? Color.Green : Color.Red;
    }
    
    private void PrintStatement_Click(object? sender, EventArgs e)
    {
        try
        {
            // Create print document
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += PrintDocument_PrintPage;
            
            // Show print preview dialog
            PrintPreviewDialog previewDialog = new PrintPreviewDialog
            {
                Document = printDoc,
                Width = 1024,
                Height = 768,
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = false
            };
            
            previewDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"خطأ في الطباعة:\n\n{ex.Message}",
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
    
    private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
    {
        Graphics? g = e.Graphics;
        if (g == null) return;
        
        // Fonts
        Font titleFont = new Font("Cairo", 18, FontStyle.Bold);
        Font headerFont = new Font("Cairo", 12, FontStyle.Bold);
        Font normalFont = new Font("Cairo", 10);
        Font smallFont = new Font("Cairo", 9);
        
        // Colors
        Color primaryColor = ColorScheme.Primary;
        Color headerBg = Color.FromArgb(41, 128, 185);
        
        float yPos = 50;
        float leftMargin = 50;
        float rightMargin = e.PageBounds.Width - 50;
        
        // ==================== TITLE ====================
        string title = "كشف حساب مورد";
        SizeF titleSize = g.MeasureString(title, titleFont);
        float titleX = (e.PageBounds.Width - titleSize.Width) / 2;
        g.DrawString(title, titleFont, new SolidBrush(primaryColor), titleX, yPos);
        yPos += titleSize.Height + 20;
        
        // Line separator
        g.DrawLine(new Pen(Color.Gray, 2), leftMargin, yPos, rightMargin, yPos);
        yPos += 15;
        
        // ==================== SUPPLIER INFO ====================
        g.DrawString($"اسم المورد: {_statement.SupplierName}", headerFont, Brushes.Black, rightMargin, yPos, new StringFormat { Alignment = StringAlignment.Far });
        yPos += 25;
        
        g.DrawString($"كود المورد: {_statement.SupplierCode}", normalFont, Brushes.Black, rightMargin, yPos, new StringFormat { Alignment = StringAlignment.Far });
        yPos += 25;
        
        g.DrawString($"الرصيد الافتتاحي: {_statement.OpeningBalance:N2} ج.م", normalFont, 
            new SolidBrush(_statement.OpeningBalance >= 0 ? Color.Green : Color.Red), 
            rightMargin, yPos, new StringFormat { Alignment = StringAlignment.Far });
        yPos += 25;
        
        g.DrawString($"تاريخ الطباعة: {DateTime.Now:yyyy/MM/dd HH:mm}", smallFont, Brushes.Gray, rightMargin, yPos, new StringFormat { Alignment = StringAlignment.Far });
        yPos += 35;
        
        // ==================== TABLE HEADER ====================
        float colWidth = (rightMargin - leftMargin) / 7;
        float[] colPositions = new float[7];
        for (int i = 0; i < 7; i++)
        {
            colPositions[i] = rightMargin - (colWidth * (i + 1));
        }
        
        // Header background
        g.FillRectangle(new SolidBrush(headerBg), leftMargin, yPos, rightMargin - leftMargin, 30);
        
        // Header text
        string[] headers = { "التاريخ", "النوع", "رقم المرجع", "الوصف", "مدين", "دائن", "الرصيد" };
        float headerY = yPos + 8;
        
        for (int i = 0; i < headers.Length; i++)
        {
            float x = colPositions[6 - i] + colWidth / 2;
            g.DrawString(headers[i], normalFont, Brushes.White, x, headerY, new StringFormat { Alignment = StringAlignment.Center });
        }
        
        yPos += 35;
        
        // ==================== TABLE ROWS ====================
        int rowCount = 0;
        bool alternateBg = false;
        
        foreach (var line in _statement.Transactions)
        {
            if (yPos > e.PageBounds.Height - 100)
            {
                e.HasMorePages = true;
                return;
            }
            
            // Alternate row background
            if (alternateBg)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 240)), 
                    leftMargin, yPos, rightMargin - leftMargin, 25);
            }
            
            // Highlight opening balance
            if (line.Type == "رصيد افتتاحي")
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 200)), 
                    leftMargin, yPos, rightMargin - leftMargin, 25);
            }
            
            // Row data
            string[] rowData = {
                line.Date.ToString("yyyy/MM/dd"),
                line.Type,
                line.ReferenceNumber,
                line.Description.Length > 25 ? line.Description.Substring(0, 25) + "..." : line.Description,
                line.Debit > 0 ? line.Debit.ToString("N2") : "-",
                line.Credit > 0 ? line.Credit.ToString("N2") : "-",
                line.Balance.ToString("N2")
            };
            
            for (int i = 0; i < rowData.Length; i++)
            {
                float x = colPositions[6 - i] + colWidth / 2;
                Brush brush = Brushes.Black;
                
                // Color code amounts
                if (i == 4 && line.Debit > 0) brush = Brushes.Red;
                if (i == 5 && line.Credit > 0) brush = Brushes.Green;
                if (i == 6) brush = new SolidBrush(line.Balance >= 0 ? Color.Green : Color.Red);
                
                g.DrawString(rowData[i], smallFont, brush, x, yPos + 5, new StringFormat { Alignment = StringAlignment.Center });
            }
            
            yPos += 25;
            alternateBg = !alternateBg;
            rowCount++;
        }
        
        yPos += 10;
        
        // ==================== SUMMARY ====================
        g.DrawLine(new Pen(Color.Gray, 2), leftMargin, yPos, rightMargin, yPos);
        yPos += 15;
        
        decimal totalDebit = _statement.Transactions.Sum(t => t.Debit);
        decimal totalCredit = _statement.Transactions.Sum(t => t.Credit);
        
        g.DrawString($"إجمالي المدين (السدادات): {totalDebit:N2} ج.م", headerFont, Brushes.Red, rightMargin, yPos, new StringFormat { Alignment = StringAlignment.Far });
        yPos += 25;
        
        g.DrawString($"إجمالي الدائن (الفواتير): {totalCredit:N2} ج.م", headerFont, Brushes.Green, rightMargin, yPos, new StringFormat { Alignment = StringAlignment.Far });
        yPos += 25;
        
        g.DrawString($"الرصيد الختامي: {_statement.ClosingBalance:N2} ج.م", 
            new Font("Cairo", 14, FontStyle.Bold), 
            new SolidBrush(_statement.ClosingBalance >= 0 ? Color.Green : Color.Red), 
            rightMargin, yPos, new StringFormat { Alignment = StringAlignment.Far });
        
        e.HasMorePages = false;
    }
    
    private void ExportToExcel_Click(object? sender, EventArgs e)
    {
        try
        {
            // Create SaveFileDialog
            using var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "حفظ كشف الحساب",
                FileName = $"كشف_حساب_{_statement.SupplierName}_{DateTime.Now:yyyy-MM-dd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;

            // Create Excel workbook
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("كشف الحساب");

            // Set RTL direction
            worksheet.RightToLeft = true;

            // ==================== TITLE ====================
            worksheet.Cell(1, 1).Value = "كشف حساب مورد";
            worksheet.Range(1, 1, 1, 7).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 18;
            worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.White;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(41, 128, 185);
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Row(1).Height = 35;

            // ==================== SUPPLIER INFO ====================
            int row = 3;
            
            worksheet.Cell(row, 1).Value = "اسم المورد:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = _statement.SupplierName;
            worksheet.Range(row, 2, row, 4).Merge();
            row++;
            
            worksheet.Cell(row, 1).Value = "كود المورد:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = _statement.SupplierCode;
            row++;
            
            worksheet.Cell(row, 1).Value = "الرصيد الافتتاحي:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = _statement.OpeningBalance;
            worksheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 2).Style.Font.FontColor = _statement.OpeningBalance >= 0 ? XLColor.Green : XLColor.Red;
            worksheet.Cell(row, 2).Style.Font.Bold = true;
            row++;
            
            worksheet.Cell(row, 1).Value = "تاريخ التقرير:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 2).Value = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
            row += 2;

            // ==================== TABLE HEADERS ====================
            worksheet.Cell(row, 1).Value = "التاريخ";
            worksheet.Cell(row, 2).Value = "النوع";
            worksheet.Cell(row, 3).Value = "رقم المرجع";
            worksheet.Cell(row, 4).Value = "الوصف";
            worksheet.Cell(row, 5).Value = "مدين (سداد)";
            worksheet.Cell(row, 6).Value = "دائن (فاتورة)";
            worksheet.Cell(row, 7).Value = "الرصيد";

            // Style header row
            var headerRange = worksheet.Range(row, 1, row, 7);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontSize = 12;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(41, 128, 185);
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Row(row).Height = 30;
            row++;

            // ==================== DATA ROWS ====================
            decimal totalDebit = 0;
            decimal totalCredit = 0;
            
            foreach (var line in _statement.Transactions)
            {
                worksheet.Cell(row, 1).Value = line.Date.ToString("yyyy/MM/dd");
                worksheet.Cell(row, 2).Value = line.Type;
                worksheet.Cell(row, 3).Value = line.ReferenceNumber;
                worksheet.Cell(row, 4).Value = line.Description;
                worksheet.Cell(row, 5).Value = line.Debit;
                worksheet.Cell(row, 6).Value = line.Credit;
                worksheet.Cell(row, 7).Value = line.Balance;

                // Format currency columns
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";

                // Color code amounts
                if (line.Debit > 0)
                {
                    worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.Red;
                    worksheet.Cell(row, 5).Style.Font.Bold = true;
                }
                
                if (line.Credit > 0)
                {
                    worksheet.Cell(row, 6).Style.Font.FontColor = XLColor.Green;
                    worksheet.Cell(row, 6).Style.Font.Bold = true;
                }

                // Color code balance
                worksheet.Cell(row, 7).Style.Font.FontColor = line.Balance >= 0 ? XLColor.Green : XLColor.Red;
                worksheet.Cell(row, 7).Style.Font.Bold = true;

                // Highlight opening balance row
                if (line.Type == "رصيد افتتاحي")
                {
                    worksheet.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 200);
                }

                // Alternating row colors
                if (row % 2 == 0)
                {
                    worksheet.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.FromArgb(240, 248, 255);
                }

                totalDebit += line.Debit;
                totalCredit += line.Credit;
                
                row++;
            }

            row++;

            // ==================== SUMMARY ====================
            worksheet.Cell(row, 1).Value = "الإجماليات:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 1).Style.Font.FontSize = 12;
            
            worksheet.Cell(row, 4).Value = "إجمالي المدين (السدادات):";
            worksheet.Cell(row, 4).Style.Font.Bold = true;
            worksheet.Cell(row, 5).Value = totalDebit;
            worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 5).Style.Font.Bold = true;
            worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.Red;
            worksheet.Cell(row, 5).Style.Font.FontSize = 12;
            
            row++;
            
            worksheet.Cell(row, 4).Value = "إجمالي الدائن (الفواتير):";
            worksheet.Cell(row, 4).Style.Font.Bold = true;
            worksheet.Cell(row, 6).Value = totalCredit;
            worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 6).Style.Font.Bold = true;
            worksheet.Cell(row, 6).Style.Font.FontColor = XLColor.Green;
            worksheet.Cell(row, 6).Style.Font.FontSize = 12;
            
            row++;
            
            worksheet.Cell(row, 4).Value = "الرصيد الختامي:";
            worksheet.Cell(row, 4).Style.Font.Bold = true;
            worksheet.Cell(row, 4).Style.Font.FontSize = 14;
            worksheet.Cell(row, 7).Value = _statement.ClosingBalance;
            worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 7).Style.Font.Bold = true;
            worksheet.Cell(row, 7).Style.Font.FontSize = 14;
            worksheet.Cell(row, 7).Style.Font.FontColor = _statement.ClosingBalance >= 0 ? XLColor.Green : XLColor.Red;

            // Highlight summary rows
            worksheet.Range(row - 2, 1, row, 7).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 220);

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Add borders to all data
            var dataRange = worksheet.Range(3, 1, row, 7);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Save the workbook
            workbook.SaveAs(saveDialog.FileName);

            MessageBox.Show(
                $"✅ تم تصدير كشف الحساب بنجاح!\n\nالملف: {Path.GetFileName(saveDialog.FileName)}",
                "نجاح التصدير",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            // Ask if user wants to open the file
            var openResult = MessageBox.Show(
                "هل تريد فتح الملف الآن؟",
                "فتح الملف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (openResult == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = saveDialog.FileName,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"❌ خطأ في التصدير:\n\n{ex.Message}",
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
