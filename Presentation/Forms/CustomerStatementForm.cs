using GraceWay.AccountingSystem.Application.Services;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class CustomerStatementForm : Form
{
    private readonly ICustomerService _customerService;
    private readonly int _customerId;
    
    private DateTimePicker _startDatePicker = null!;
    private DateTimePicker _endDatePicker = null!;
    private CheckBox _allPeriodCheck = null!;
    private Button _generateButton = null!;
    private Button _exportButton = null!;
    private Button _printButton = null!;
    
    private Panel _summaryPanel = null!;
    private DataGridView _transactionsGrid = null!;
    
    public CustomerStatementForm(ICustomerService customerService, int customerId)
    {
        _customerService = customerService;
        _customerId = customerId;
        
        InitializeComponent();
        InitializeCustomControls();
        _ = LoadStatementAsync();
    }
    
    private void InitializeComponent()
    {
        this.Text = "كشف حساب عميل";
        this.Size = new Size(1400, 900);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
    }
    
    private void InitializeCustomControls()
    {
        // Header Panel
        Panel headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 140,
            BackColor = Color.White,
            Padding = new Padding(20)
        };
        
        Label titleLabel = new Label
        {
            Text = "📋 كشف حساب عميل",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(20, 20)
        };
        headerPanel.Controls.Add(titleLabel);
        
        // Date Range Controls
        _allPeriodCheck = new CheckBox
        {
            Text = "كل الفترة",
            Font = new Font("Cairo", 10F),
            AutoSize = true,
            Location = new Point(20, 75),
            Checked = true
        };
        _allPeriodCheck.CheckedChanged += AllPeriodCheck_CheckedChanged;
        headerPanel.Controls.Add(_allPeriodCheck);
        
        Label startLabel = new Label
        {
            Text = "من تاريخ:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(150, 75),
            Enabled = false
        };
        headerPanel.Controls.Add(startLabel);
        
        _startDatePicker = new DateTimePicker
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(200, 30),
            Location = new Point(250, 72),
            Format = DateTimePickerFormat.Short,
            Enabled = false
        };
        _startDatePicker.Value = DateTime.Now.AddMonths(-1);
        headerPanel.Controls.Add(_startDatePicker);
        
        Label endLabel = new Label
        {
            Text = "إلى تاريخ:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(470, 75),
            Enabled = false
        };
        headerPanel.Controls.Add(endLabel);
        
        _endDatePicker = new DateTimePicker
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(200, 30),
            Location = new Point(570, 72),
            Format = DateTimePickerFormat.Short,
            Enabled = false
        };
        headerPanel.Controls.Add(_endDatePicker);
        
        _generateButton = CreateButton("📊 إنشاء الكشف", ColorScheme.Success, new Point(790, 72), (s, e) => _ = LoadStatementAsync());
        headerPanel.Controls.Add(_generateButton);
        
        _exportButton = CreateButton("📤 تصدير", Color.FromArgb(0, 166, 90), new Point(970, 72), ExportStatement_Click);
        headerPanel.Controls.Add(_exportButton);
        
        _printButton = CreateButton("🖨️ طباعة", Color.FromArgb(52, 152, 219), new Point(1150, 72), PrintStatement_Click);
        headerPanel.Controls.Add(_printButton);
        
        this.Controls.Add(headerPanel);
        
        // Main Container
        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ColorScheme.Background,
            Padding = new Padding(20)
        };
        
        // Summary Panel
        _summaryPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 200,
            BackColor = Color.White,
            Padding = new Padding(20)
        };
        mainPanel.Controls.Add(_summaryPanel);
        
        // Transactions Grid
        _transactionsGrid = CreateDataGrid();
        mainPanel.Controls.Add(_transactionsGrid);
        
        this.Controls.Add(mainPanel);
    }
    
    private void AllPeriodCheck_CheckedChanged(object? sender, EventArgs e)
    {
        bool enabled = !_allPeriodCheck.Checked;
        _startDatePicker.Enabled = enabled;
        _endDatePicker.Enabled = enabled;
        
        if (_startDatePicker.Parent != null)
        {
            foreach (Control control in _startDatePicker.Parent.Controls)
            {
                if (control is Label label && (label.Text.Contains("من تاريخ") || label.Text.Contains("إلى تاريخ")))
                {
                    label.Enabled = enabled;
                }
            }
        }
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
    
    private async Task LoadStatementAsync()
    {
        try
        {
            DateTime? startDate = _allPeriodCheck.Checked ? null : _startDatePicker.Value.Date;
            DateTime? endDate = _allPeriodCheck.Checked ? null : _endDatePicker.Value.Date;
            
            var statement = await _customerService.GetCustomerStatementAsync(_customerId, startDate, endDate);
            
            // Display Summary
            DisplaySummary(statement);
            
            // Display Transactions
            DisplayTransactions(statement);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل الكشف: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void DisplaySummary(CustomerStatementDto statement)
    {
        _summaryPanel.Controls.Clear();
        
        // Customer Info
        Label customerLabel = new Label
        {
            Text = $"العميل: {statement.Customer.CustomerName}",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(20, 20)
        };
        _summaryPanel.Controls.Add(customerLabel);
        
        Label codeLabel = new Label
        {
            Text = $"الكود: {statement.Customer.CustomerCode}",
            Font = new Font("Cairo", 11F),
            AutoSize = true,
            Location = new Point(20, 55)
        };
        _summaryPanel.Controls.Add(codeLabel);
        
        // Financial Summary
        int xPos = 400;
        AddSummaryLabel($"الرصيد الافتتاحي:", statement.OpeningBalance, new Point(xPos, 20));
        AddSummaryLabel($"إجمالي المديونية:", statement.TotalDebit, new Point(xPos, 60), ColorScheme.Error);
        AddSummaryLabel($"إجمالي المدفوعات:", statement.TotalCredit, new Point(xPos, 100), ColorScheme.Success);
        
        xPos = 850;
        Color balanceColor = statement.ClosingBalance > 0 ? ColorScheme.Error : ColorScheme.Success;
        string balanceText = statement.ClosingBalance > 0 ? "له عندنا" : "لنا عنده";
        
        Label closingBalanceLabel = new Label
        {
            Text = $"الرصيد الختامي: {Math.Abs(statement.ClosingBalance):N2} جنيه",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = balanceColor,
            AutoSize = true,
            Location = new Point(xPos, 20)
        };
        _summaryPanel.Controls.Add(closingBalanceLabel);
        
        Label balanceTypeLabel = new Label
        {
            Text = balanceText,
            Font = new Font("Cairo", 12F),
            ForeColor = balanceColor,
            AutoSize = true,
            Location = new Point(xPos, 60)
        };
        _summaryPanel.Controls.Add(balanceTypeLabel);
        
        // Period
        if (!_allPeriodCheck.Checked)
        {
            Label periodLabel = new Label
            {
                Text = $"الفترة: من {_startDatePicker.Value:yyyy-MM-dd} إلى {_endDatePicker.Value:yyyy-MM-dd}",
                Font = new Font("Cairo", 10F),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(20, 90)
            };
            _summaryPanel.Controls.Add(periodLabel);
        }
    }
    
    private void AddSummaryLabel(string label, decimal amount, Point location, Color? color = null)
    {
        Label lbl = new Label
        {
            Text = $"{label} {amount:N2} جنيه",
            Font = new Font("Cairo", 11F),
            ForeColor = color ?? Color.Black,
            AutoSize = true,
            Location = location
        };
        _summaryPanel.Controls.Add(lbl);
    }
    
    private void DisplayTransactions(CustomerStatementDto statement)
    {
        _transactionsGrid.Columns.Clear();
        
        _transactionsGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "التاريخ", Width = 120 });
        _transactionsGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "البيان", Width = 250 });
        _transactionsGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Reference", HeaderText = "المرجع", Width = 150 });
        _transactionsGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Debit", HeaderText = "مدين", Width = 150 });
        _transactionsGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Credit", HeaderText = "دائن", Width = 150 });
        _transactionsGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Balance", HeaderText = "الرصيد", Width = 150 });
        
        _transactionsGrid.Rows.Clear();
        
        // Add opening balance row
        int rowIndex = _transactionsGrid.Rows.Add(
            "",
            "الرصيد الافتتاحي",
            "",
            "",
            "",
            statement.OpeningBalance.ToString("N2")
        );
        _transactionsGrid.Rows[rowIndex].DefaultCellStyle.Font = new Font("Cairo", 10F, FontStyle.Bold);
        _transactionsGrid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);
        
        // Add transactions
        foreach (var transaction in statement.Transactions)
        {
            rowIndex = _transactionsGrid.Rows.Add(
                transaction.Date.ToString("yyyy-MM-dd"),
                transaction.Description,
                transaction.ReferenceNumber,
                transaction.Debit > 0 ? transaction.Debit.ToString("N2") : "",
                transaction.Credit > 0 ? transaction.Credit.ToString("N2") : "",
                transaction.Balance.ToString("N2")
            );
            
            // Color code balance
            if (transaction.Balance > 0)
            {
                _transactionsGrid.Rows[rowIndex].Cells["Balance"].Style.ForeColor = ColorScheme.Error;
            }
            else if (transaction.Balance < 0)
            {
                _transactionsGrid.Rows[rowIndex].Cells["Balance"].Style.ForeColor = ColorScheme.Success;
            }
        }
        
        // Add closing balance row
        rowIndex = _transactionsGrid.Rows.Add(
            "",
            "الرصيد الختامي",
            "",
            statement.TotalDebit.ToString("N2"),
            statement.TotalCredit.ToString("N2"),
            statement.ClosingBalance.ToString("N2")
        );
        _transactionsGrid.Rows[rowIndex].DefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        _transactionsGrid.Rows[rowIndex].DefaultCellStyle.BackColor = ColorScheme.Primary;
        _transactionsGrid.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.White;
    }
    
    private void ExportStatement_Click(object? sender, EventArgs e)
    {
        try
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                DefaultExt = "csv",
                FileName = $"كشف_حساب_عميل_{DateTime.Now:yyyy-MM-dd}.csv"
            };
            
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                ExportToCSV(_transactionsGrid, saveDialog.FileName);
                MessageBox.Show("تم تصدير الكشف بنجاح", "نجح",
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
        using (StreamWriter writer = new StreamWriter(fileName, false, System.Text.Encoding.UTF8))
        {
            // Headers
            List<string> headers = new List<string>();
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
                
                List<string> values = new List<string>();
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
    
    private void PrintStatement_Click(object? sender, EventArgs e)
    {
        MessageBox.Show("سيتم إضافة وظيفة الطباعة قريباً", "قريباً",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
