using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class InvoiceFilterForm : Form
{
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public string? Status { get; private set; }
    public decimal? MinAmount { get; private set; }
    public decimal? MaxAmount { get; private set; }
    public string? SearchText { get; private set; }
    
    private DateTimePicker _startDatePicker = null!;
    private DateTimePicker _endDatePicker = null!;
    private CheckBox _useDateRangeCheck = null!;
    private ComboBox _statusCombo = null!;
    private TextBox _minAmountText = null!;
    private TextBox _maxAmountText = null!;
    private TextBox _searchText = null!;
    private Button _applyButton = null!;
    private Button _resetButton = null!;
    private Button _cancelButton = null!;
    
    public InvoiceFilterForm()
    {
        InitializeComponent();
        InitializeCustomControls();
    }
    
    private void InitializeComponent()
    {
        this.Text = "فلتر الفواتير";
        this.Size = new Size(600, 550);
        this.StartPosition = FormStartPosition.CenterParent;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = Color.White;
        this.Font = new Font("Cairo", 10F);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
    }
    
    private void InitializeCustomControls()
    {
        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(30)
        };
        
        // Title
        Label titleLabel = new Label
        {
            Text = "🔍 فلتر وبحث الفواتير",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(30, 20)
        };
        mainPanel.Controls.Add(titleLabel);
        
        int yPos = 80;
        
        // Search Text
        Label searchLabel = CreateLabel("بحث (رقم الفاتورة أو العميل/المورد):", new Point(30, yPos));
        mainPanel.Controls.Add(searchLabel);
        
        _searchText = new TextBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(500, 30),
            Location = new Point(30, yPos + 30),
            PlaceholderText = "ابحث برقم الفاتورة أو اسم العميل/المورد"
        };
        mainPanel.Controls.Add(_searchText);
        
        yPos += 80;
        
        // Date Range
        _useDateRangeCheck = new CheckBox
        {
            Text = "تحديد نطاق التاريخ",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(30, yPos)
        };
        _useDateRangeCheck.CheckedChanged += UseDateRangeCheck_CheckedChanged;
        mainPanel.Controls.Add(_useDateRangeCheck);
        
        yPos += 40;
        
        Label startDateLabel = CreateLabel("من تاريخ:", new Point(50, yPos));
        mainPanel.Controls.Add(startDateLabel);
        
        _startDatePicker = new DateTimePicker
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(200, 30),
            Location = new Point(160, yPos),
            Format = DateTimePickerFormat.Short,
            Enabled = false
        };
        mainPanel.Controls.Add(_startDatePicker);
        
        yPos += 40;
        
        Label endDateLabel = CreateLabel("إلى تاريخ:", new Point(50, yPos));
        mainPanel.Controls.Add(endDateLabel);
        
        _endDatePicker = new DateTimePicker
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(200, 30),
            Location = new Point(160, yPos),
            Format = DateTimePickerFormat.Short,
            Enabled = false
        };
        mainPanel.Controls.Add(_endDatePicker);
        
        yPos += 60;
        
        // Status
        Label statusLabel = CreateLabel("الحالة:", new Point(30, yPos));
        mainPanel.Controls.Add(statusLabel);
        
        _statusCombo = new ComboBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(200, 30),
            Location = new Point(160, yPos),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _statusCombo.Items.AddRange(new object[] { "الكل", "مدفوعة", "مدفوعة جزئياً", "غير مدفوعة" });
        _statusCombo.SelectedIndex = 0;
        mainPanel.Controls.Add(_statusCombo);
        
        yPos += 50;
        
        // Amount Range
        Label amountLabel = CreateLabel("نطاق المبلغ:", new Point(30, yPos));
        mainPanel.Controls.Add(amountLabel);
        
        Label minLabel = CreateLabel("من:", new Point(160, yPos));
        mainPanel.Controls.Add(minLabel);
        
        _minAmountText = new TextBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(120, 30),
            Location = new Point(200, yPos),
            PlaceholderText = "0.00"
        };
        mainPanel.Controls.Add(_minAmountText);
        
        Label maxLabel = CreateLabel("إلى:", new Point(340, yPos));
        mainPanel.Controls.Add(maxLabel);
        
        _maxAmountText = new TextBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(120, 30),
            Location = new Point(380, yPos),
            PlaceholderText = "0.00"
        };
        mainPanel.Controls.Add(_maxAmountText);
        
        yPos += 70;
        
        // Buttons
        _applyButton = CreateButton("✔️ تطبيق الفلتر", ColorScheme.Success, new Point(30, yPos), ApplyFilter_Click);
        mainPanel.Controls.Add(_applyButton);
        
        _resetButton = CreateButton("🔄 إعادة تعيين", ColorScheme.Warning, new Point(200, yPos), ResetFilter_Click);
        mainPanel.Controls.Add(_resetButton);
        
        _cancelButton = CreateButton("❌ إلغاء", ColorScheme.Error, new Point(370, yPos), (s, e) => this.Close());
        mainPanel.Controls.Add(_cancelButton);
        
        this.Controls.Add(mainPanel);
    }
    
    private Label CreateLabel(string text, Point location)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = location
        };
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
    
    private void UseDateRangeCheck_CheckedChanged(object? sender, EventArgs e)
    {
        bool enabled = _useDateRangeCheck.Checked;
        _startDatePicker.Enabled = enabled;
        _endDatePicker.Enabled = enabled;
    }
    
    private void ApplyFilter_Click(object? sender, EventArgs e)
    {
        try
        {
            // Date Range
            if (_useDateRangeCheck.Checked)
            {
                StartDate = _startDatePicker.Value.Date;
                EndDate = _endDatePicker.Value.Date;
                
                if (StartDate > EndDate)
                {
                    MessageBox.Show("تاريخ البداية يجب أن يكون قبل تاريخ النهاية", "تنبيه",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                StartDate = null;
                EndDate = null;
            }
            
            // Status
            Status = _statusCombo.SelectedIndex switch
            {
                1 => "Paid",
                2 => "Partial",
                3 => "Unpaid",
                _ => null
            };
            
            // Amount Range
            if (!string.IsNullOrWhiteSpace(_minAmountText.Text))
            {
                if (decimal.TryParse(_minAmountText.Text, out decimal min))
                {
                    MinAmount = min;
                }
            }
            
            if (!string.IsNullOrWhiteSpace(_maxAmountText.Text))
            {
                if (decimal.TryParse(_maxAmountText.Text, out decimal max))
                {
                    MaxAmount = max;
                }
            }
            
            if (MinAmount.HasValue && MaxAmount.HasValue && MinAmount > MaxAmount)
            {
                MessageBox.Show("الحد الأدنى للمبلغ يجب أن يكون أقل من الحد الأقصى", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Search Text
            SearchText = string.IsNullOrWhiteSpace(_searchText.Text) ? null : _searchText.Text.Trim();
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تطبيق الفلتر: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void ResetFilter_Click(object? sender, EventArgs e)
    {
        _searchText.Text = string.Empty;
        _useDateRangeCheck.Checked = false;
        _statusCombo.SelectedIndex = 0;
        _minAmountText.Text = string.Empty;
        _maxAmountText.Text = string.Empty;
        
        StartDate = null;
        EndDate = null;
        Status = null;
        MinAmount = null;
        MaxAmount = null;
        SearchText = null;
    }
}
