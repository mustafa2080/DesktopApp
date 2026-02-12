using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Application.Services;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class AddCashBoxForm : Form
{
    private readonly ICashBoxService _cashBoxService;
    private readonly int _userId;
    
    // Controls
    private Panel _mainPanel = null!;
    private Label _titleLabel = null!;
    private TextBox _nameText = null!;
    private TextBox _initialBalanceText = null!;
    private TextBox _descriptionText = null!;
    private CheckBox _isActiveCheck = null!;
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    
    public bool CashBoxSaved { get; private set; }
    
    public AddCashBoxForm(ICashBoxService cashBoxService, int userId)
    {
        _cashBoxService = cashBoxService;
        _userId = userId;
        
        InitializeComponent();
        InitializeCustomControls();
    }
    
    private void InitializeComponent()
    {
        this.Text = "إضافة خزنة جديدة";
        this.Size = new Size(700, 650);
        this.StartPosition = FormStartPosition.CenterParent;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
    }
    
    private void InitializeCustomControls()
    {
        _mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(40, 30, 40, 30),
            AutoScroll = true
        };
        
        int yPos = 20;
        int controlWidth = 600;
        int rightMargin = 20;
        
        // Title with icon
        _titleLabel = new Label
        {
            Text = "➕ إضافة خزنة جديدة",
            Font = new Font("Cairo", 18F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            Size = new Size(controlWidth, 50),
            Location = new Point(rightMargin, yPos),
            TextAlign = ContentAlignment.MiddleRight
        };
        _mainPanel.Controls.Add(_titleLabel);
        yPos += 70;
        
        // Separator line
        Panel separator = new Panel
        {
            Size = new Size(controlWidth, 2),
            Location = new Point(rightMargin, yPos),
            BackColor = Color.FromArgb(230, 230, 230)
        };
        _mainPanel.Controls.Add(separator);
        yPos += 20;
        
        // اسم الخزنة
        AddFormField("اسم الخزنة: *", ref yPos, rightMargin, controlWidth, () =>
        {
            _nameText = new TextBox
            {
                Font = new Font("Cairo", 11F),
                Size = new Size(controlWidth, 35),
                TextAlign = HorizontalAlignment.Right,
                PlaceholderText = "مثال: الخزنة الرئيسية، خزنة الفرع"
            };
            return _nameText;
        });
        
        // الرصيد الافتتاحي
        AddFormField("الرصيد الافتتاحي (جنيه): *", ref yPos, rightMargin, controlWidth, () =>
        {
            _initialBalanceText = new TextBox
            {
                Font = new Font("Cairo", 12F, FontStyle.Bold),
                Size = new Size(controlWidth, 35),
                TextAlign = HorizontalAlignment.Right,
                PlaceholderText = "0.00",
                Text = "0.00"
            };
            _initialBalanceText.KeyPress += AmountText_KeyPress;
            return _initialBalanceText;
        });
        
        // الوصف
        AddFormField("الوصف:", ref yPos, rightMargin, controlWidth, () =>
        {
            _descriptionText = new TextBox
            {
                Font = new Font("Cairo", 10F),
                Size = new Size(controlWidth, 80),
                Multiline = true,
                TextAlign = HorizontalAlignment.Right,
                ScrollBars = ScrollBars.Vertical,
                PlaceholderText = "اختياري - وصف مختصر للخزنة"
            };
            return _descriptionText;
        }, 100);
        
        // نشط؟
        _isActiveCheck = new CheckBox
        {
            Text = "✓ الخزنة نشطة (يمكن استخدامها)",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Size = new Size(controlWidth, 40),
            Location = new Point(rightMargin, yPos),
            Checked = true,
            RightToLeft = RightToLeft.Yes,
            CheckAlign = ContentAlignment.MiddleRight,
            TextAlign = ContentAlignment.MiddleRight
        };
        _mainPanel.Controls.Add(_isActiveCheck);
        yPos += 60;
        
        // Buttons Panel
        Panel buttonPanel = new Panel
        {
            Size = new Size(controlWidth, 55),
            Location = new Point(rightMargin, yPos),
            BackColor = Color.Transparent
        };
        
        _saveButton = new Button
        {
            Text = "💾 حفظ",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            Size = new Size(280, 50),
            Location = new Point(0, 0),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Right
        };
        _saveButton.FlatAppearance.BorderSize = 0;
        _saveButton.Click += SaveButton_Click;
        buttonPanel.Controls.Add(_saveButton);
        
        _cancelButton = new Button
        {
            Text = "✖ إلغاء",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            Size = new Size(280, 50),
            Location = new Point(310, 0),
            BackColor = Color.FromArgb(120, 120, 120),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Left
        };
        _cancelButton.FlatAppearance.BorderSize = 0;
        _cancelButton.Click += (s, e) => this.Close();
        buttonPanel.Controls.Add(_cancelButton);
        
        _mainPanel.Controls.Add(buttonPanel);
        this.Controls.Add(_mainPanel);
    }
    
    private void AddFormField(string labelText, ref int yPos, int rightMargin, 
        int controlWidth, Func<Control> createControl, int additionalSpacing = 0)
    {
        // Label
        Label label = new Label
        {
            Text = labelText,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(50, 50, 50),
            Size = new Size(controlWidth, 30),
            Location = new Point(rightMargin, yPos),
            TextAlign = ContentAlignment.MiddleRight
        };
        _mainPanel.Controls.Add(label);
        yPos += 35;
        
        // Control
        var control = createControl();
        control.Location = new Point(rightMargin, yPos);
        _mainPanel.Controls.Add(control);
        yPos += control.Height + 25 + additionalSpacing;
    }
    
    private void AmountText_KeyPress(object? sender, KeyPressEventArgs e)
    {
        // Allow only numbers, decimal point, and control keys
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
        {
            e.Handled = true;
        }
        
        // Allow only one decimal point
        if (e.KeyChar == '.' && ((TextBox)sender!).Text.Contains('.'))
        {
            e.Handled = true;
        }
    }
    
    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(_nameText.Text))
        {
            ShowError("برجاء إدخال اسم الخزنة");
            _nameText.Focus();
            return;
        }
        
        if (!decimal.TryParse(_initialBalanceText.Text, out decimal initialBalance))
        {
            ShowError("برجاء إدخال رصيد افتتاحي صحيح");
            _initialBalanceText.Focus();
            return;
        }
        
        try
        {
            _saveButton.Enabled = false;
            _saveButton.Text = "⏳ جاري الحفظ...";
            
            // Create cashbox
            var cashBox = new CashBox
            {
                CashBoxCode = await GenerateCashBoxCodeAsync(),
                Name = _nameText.Text.Trim(),
                Type = "CashBox", // ✅ دائماً خزنة
                AccountNumber = null,
                BankName = null,
                OpeningBalance = initialBalance,
                CurrentBalance = initialBalance,
                Notes = _descriptionText.Text.Trim(),
                IsActive = _isActiveCheck.Checked,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _userId
            };
            
            // Save to database
            await _cashBoxService.CreateCashBoxAsync(cashBox);
            
            CashBoxSaved = true;
            
            MessageBox.Show(
                $"✅ تم حفظ الخزنة بنجاح\n\n" +
                $"الاسم: {cashBox.Name}\n" +
                $"الرصيد الافتتاحي: {initialBalance:N2} جنيه", 
                "تم الحفظ بنجاح",
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            // Show detailed error including inner exceptions
            var errorMessage = $"حدث خطأ أثناء الحفظ:\n{ex.Message}";
            
            if (ex.InnerException != null)
            {
                errorMessage += $"\n\nتفاصيل إضافية:\n{ex.InnerException.Message}";
                
                if (ex.InnerException.InnerException != null)
                {
                    errorMessage += $"\n\n{ex.InnerException.InnerException.Message}";
                }
            }
            
            ShowError(errorMessage);
            _saveButton.Enabled = true;
            _saveButton.Text = "💾 حفظ";
        }
    }
    
    private async Task<string> GenerateCashBoxCodeAsync()
    {
        try
        {
            // Get all cashboxes to generate next code
            var allCashBoxes = await _cashBoxService.GetAllCashBoxesAsync();
            var nextNumber = allCashBoxes.Count + 1;
            
            // Generate code for CashBox only
            return $"CB{nextNumber:D3}";
        }
        catch
        {
            // Fallback to simple code
            return $"CB{DateTime.Now:yyyyMMddHHmmss}";
        }
    }
    
    private void ShowError(string message)
    {
        MessageBox.Show(message, "تنبيه",
            MessageBoxButtons.OK, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button1,
            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
    }
}
