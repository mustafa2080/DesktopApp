using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Application.Services;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class EditCashBoxForm : Form
{
    private readonly int _cashBoxId;
    private readonly ICashBoxService _cashBoxService;
    private readonly int _userId;
    private CashBox? _originalCashBox;
    
    // Controls
    private Panel _mainPanel = null!;
    private Label _titleLabel = null!;
    private TextBox _nameText = null!;
    private Label _currentBalanceLabel = null!;
    private TextBox _descriptionText = null!;
    private CheckBox _isActiveCheck = null!;
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    
    public bool CashBoxUpdated { get; private set; }
    
    public EditCashBoxForm(int cashBoxId, ICashBoxService cashBoxService, int userId)
    {
        _cashBoxId = cashBoxId;
        _cashBoxService = cashBoxService;
        _userId = userId;
        
        InitializeComponent();
        InitializeCustomControls();
        _ = LoadCashBoxAsync();
    }
    
    private void InitializeComponent()
    {
        this.Text = "تعديل خزنة";
        this.Size = new Size(700, 600);
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
            Text = "✏️ تعديل خزنة",
            Font = new Font("Cairo", 18F, FontStyle.Bold),
            ForeColor = ColorScheme.Warning,
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
                TextAlign = HorizontalAlignment.Right
            };
            return _nameText;
        });
        
        // الرصيد الحالي (للعرض فقط)
        Label balanceFieldLabel = new Label
        {
            Text = "الرصيد الحالي:",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(50, 50, 50),
            Size = new Size(controlWidth, 30),
            Location = new Point(rightMargin, yPos),
            TextAlign = ContentAlignment.MiddleRight
        };
        _mainPanel.Controls.Add(balanceFieldLabel);
        yPos += 35;
        
        _currentBalanceLabel = new Label
        {
            Text = "0.00 جنيه",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            Size = new Size(controlWidth, 40),
            Location = new Point(rightMargin, yPos),
            TextAlign = ContentAlignment.MiddleRight,
            BackColor = Color.FromArgb(240, 248, 255),
            BorderStyle = BorderStyle.FixedSingle
        };
        _mainPanel.Controls.Add(_currentBalanceLabel);
        yPos += 65;
        
        // الوصف
        AddFormField("الوصف:", ref yPos, rightMargin, controlWidth, () =>
        {
            _descriptionText = new TextBox
            {
                Font = new Font("Cairo", 10F),
                Size = new Size(controlWidth, 80),
                Multiline = true,
                TextAlign = HorizontalAlignment.Right,
                ScrollBars = ScrollBars.Vertical
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
            Text = "💾 حفظ التعديلات",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            Size = new Size(280, 50),
            Location = new Point(0, 0),
            BackColor = ColorScheme.Warning,
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
    
    private async Task LoadCashBoxAsync()
    {
        try
        {
            _originalCashBox = await _cashBoxService.GetCashBoxByIdAsync(_cashBoxId);
            
            if (_originalCashBox == null)
            {
                MessageBox.Show("لم يتم العثور على الخزنة المطلوبة", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                this.Close();
                return;
            }
            
            // Populate controls
            _nameText.Text = _originalCashBox.Name;
            _currentBalanceLabel.Text = $"{_originalCashBox.CurrentBalance:N2} جنيه";
            _descriptionText.Text = _originalCashBox.Notes ?? "";
            _isActiveCheck.Checked = _originalCashBox.IsActive;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل بيانات الخزنة:\n{ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            this.Close();
        }
    }
    
    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        if (_originalCashBox == null)
        {
            MessageBox.Show("خطأ: لم يتم تحميل بيانات الخزنة الأصلية", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            return;
        }
        
        // Validate inputs
        if (string.IsNullOrWhiteSpace(_nameText.Text))
        {
            ShowError("برجاء إدخال اسم الخزنة");
            _nameText.Focus();
            return;
        }
        
        try
        {
            _saveButton.Enabled = false;
            _saveButton.Text = "⏳ جاري الحفظ...";
            
            // Update cashbox (النوع يبقى CashBox دائماً)
            _originalCashBox.Name = _nameText.Text.Trim();
            _originalCashBox.Notes = _descriptionText.Text.Trim();
            _originalCashBox.IsActive = _isActiveCheck.Checked;
            
            // Save to database
            await _cashBoxService.UpdateCashBoxAsync(_originalCashBox);
            
            CashBoxUpdated = true;
            
            MessageBox.Show(
                $"✅ تم تعديل الخزنة بنجاح\n\n" +
                $"الاسم: {_originalCashBox.Name}", 
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
            ShowError($"حدث خطأ أثناء الحفظ:\n{ex.Message}");
            _saveButton.Enabled = true;
            _saveButton.Text = "💾 حفظ التعديلات";
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
