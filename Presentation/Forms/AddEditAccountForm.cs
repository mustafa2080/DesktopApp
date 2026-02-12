using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class AddEditAccountForm : Form
{
    private readonly IAccountService _accountService;
    private readonly int? _accountId;
    private readonly int? _parentId;
    private Account? _currentAccount;
    
    private TextBox _accountCodeBox = null!;
    private TextBox _accountNameBox = null!;
    private TextBox _accountNameEnBox = null!;
    private ComboBox _accountTypeCombo = null!;
    private ComboBox _parentAccountCombo = null!;
    private TextBox _openingBalanceBox = null!;
    private CheckBox _isActiveCheck = null!;
    private TextBox _notesBox = null!;
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    
    public AddEditAccountForm(IAccountService accountService, int? accountId, int? parentId)
    {
        _accountService = accountService;
        _accountId = accountId;
        _parentId = parentId;
        
        InitializeComponent();
        InitializeCustomControls();
        _ = LoadDataAsync();
    }
    
    private void InitializeComponent()
    {
        this.Text = _accountId.HasValue ? "تعديل حساب" : "إضافة حساب";
        this.Size = new Size(800, 700);
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
        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(30),
            BackColor = Color.White
        };
        
        Label titleLabel = new Label
        {
            Text = _accountId.HasValue ? "✏️ تعديل حساب" : "➕ إضافة حساب جديد",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(30, 30)
        };
        mainPanel.Controls.Add(titleLabel);
        
        int yPos = 80;
        
        // كود الحساب
        mainPanel.Controls.Add(CreateLabel("كود الحساب:", yPos));
        _accountCodeBox = CreateTextBox(yPos, !_accountId.HasValue);
        _accountCodeBox.PlaceholderText = "سيتم إنشاؤه تلقائياً";
        mainPanel.Controls.Add(_accountCodeBox);
        yPos += 60;
        
        // اسم الحساب بالعربي
        mainPanel.Controls.Add(CreateLabel("اسم الحساب (عربي):", yPos, true));
        _accountNameBox = CreateTextBox(yPos);
        mainPanel.Controls.Add(_accountNameBox);
        yPos += 60;
        
        // اسم الحساب بالإنجليزي
        mainPanel.Controls.Add(CreateLabel("اسم الحساب (English):", yPos));
        _accountNameEnBox = CreateTextBox(yPos);
        mainPanel.Controls.Add(_accountNameEnBox);
        yPos += 60;
        
        // نوع الحساب
        mainPanel.Controls.Add(CreateLabel("نوع الحساب:", yPos, !_accountId.HasValue && !_parentId.HasValue));
        _accountTypeCombo = new ComboBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(500, 30),
            Location = new Point(200, yPos),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Enabled = !_accountId.HasValue && !_parentId.HasValue
        };
        _accountTypeCombo.Items.AddRange(new object[] { 
            "Asset - الأصول", 
            "Liability - الخصوم", 
            "Equity - حقوق الملكية", 
            "Revenue - الإيرادات", 
            "Expense - المصروفات" 
        });
        mainPanel.Controls.Add(_accountTypeCombo);
        yPos += 60;
        
        // الحساب الأب
        mainPanel.Controls.Add(CreateLabel("الحساب الأب:", yPos));
        _parentAccountCombo = new ComboBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(500, 30),
            Location = new Point(200, yPos),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Enabled = !_accountId.HasValue
        };
        mainPanel.Controls.Add(_parentAccountCombo);
        yPos += 60;
        
        // الرصيد الافتتاحي
        mainPanel.Controls.Add(CreateLabel("الرصيد الافتتاحي:", yPos));
        _openingBalanceBox = CreateTextBox(yPos, !_accountId.HasValue);
        _openingBalanceBox.Text = "0";
        mainPanel.Controls.Add(_openingBalanceBox);
        yPos += 60;
        
        // نشط
        _isActiveCheck = new CheckBox
        {
            Text = "الحساب نشط",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(200, yPos),
            Checked = true
        };
        mainPanel.Controls.Add(_isActiveCheck);
        yPos += 50;
        
        // ملاحظات
        mainPanel.Controls.Add(CreateLabel("ملاحظات:", yPos));
        _notesBox = new TextBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(500, 80),
            Location = new Point(200, yPos),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical
        };
        mainPanel.Controls.Add(_notesBox);
        yPos += 100;
        
        // Buttons
        Panel buttonsPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 80,
            BackColor = Color.FromArgb(248, 249, 250),
            Padding = new Padding(30)
        };
        
        _saveButton = new Button
        {
            Text = "💾 حفظ",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Size = new Size(150, 45),
            Location = new Point(30, 17),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _saveButton.FlatAppearance.BorderSize = 0;
        _saveButton.Click += SaveButton_Click;
        buttonsPanel.Controls.Add(_saveButton);
        
        _cancelButton = new Button
        {
            Text = "❌ إلغاء",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Size = new Size(150, 45),
            Location = new Point(200, 17),
            BackColor = Color.FromArgb(108, 117, 125),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _cancelButton.FlatAppearance.BorderSize = 0;
        _cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
        buttonsPanel.Controls.Add(_cancelButton);
        
        this.Controls.Add(mainPanel);
        this.Controls.Add(buttonsPanel);
    }
    
    private Label CreateLabel(string text, int yPos, bool required = false)
    {
        return new Label
        {
            Text = required ? text + " *" : text,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = required ? ColorScheme.Error : Color.Black,
            AutoSize = true,
            Location = new Point(30, yPos + 5)
        };
    }
    
    private TextBox CreateTextBox(int yPos, bool enabled = true)
    {
        return new TextBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(500, 30),
            Location = new Point(200, yPos),
            Enabled = enabled
        };
    }
    
    private async Task LoadDataAsync()
    {
        try
        {
            // تحميل قائمة الحسابات الأب
            var accounts = await _accountService.GetAllAccountsAsync();
            _parentAccountCombo.Items.Add("لا يوجد (حساب رئيسي)");
            foreach (var acc in accounts.Where(a => a.IsActive))
            {
                _parentAccountCombo.Items.Add(new ComboBoxItem 
                { 
                    Text = $"{acc.AccountCode} - {acc.AccountName}", 
                    Value = acc.AccountId 
                });
            }
            _parentAccountCombo.SelectedIndex = 0;
            
            // إذا كان تعديل، تحميل بيانات الحساب
            if (_accountId.HasValue)
            {
                _currentAccount = await _accountService.GetAccountByIdAsync(_accountId.Value);
                if (_currentAccount != null)
                {
                    _accountCodeBox.Text = _currentAccount.AccountCode;
                    _accountNameBox.Text = _currentAccount.AccountName;
                    _accountNameEnBox.Text = _currentAccount.AccountNameEn ?? "";
                    _accountTypeCombo.SelectedIndex = _currentAccount.AccountType switch
                    {
                        "Asset" => 0,
                        "Liability" => 1,
                        "Equity" => 2,
                        "Revenue" => 3,
                        "Expense" => 4,
                        _ => 0
                    };
                    _openingBalanceBox.Text = _currentAccount.OpeningBalance.ToString("N2");
                    _isActiveCheck.Checked = _currentAccount.IsActive;
                    _notesBox.Text = _currentAccount.Notes ?? "";
                    
                    // تحديد الحساب الأب
                    if (_currentAccount.ParentAccountId.HasValue)
                    {
                        for (int i = 1; i < _parentAccountCombo.Items.Count; i++)
                        {
                            if (_parentAccountCombo.Items[i] is ComboBoxItem item && 
                                item.Value == _currentAccount.ParentAccountId.Value)
                            {
                                _parentAccountCombo.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                }
            }
            else if (_parentId.HasValue)
            {
                // تحديد الحساب الأب تلقائياً
                for (int i = 1; i < _parentAccountCombo.Items.Count; i++)
                {
                    if (_parentAccountCombo.Items[i] is ComboBoxItem item && item.Value == _parentId.Value)
                    {
                        _parentAccountCombo.SelectedIndex = i;
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(_accountNameBox.Text))
            {
                MessageBox.Show("برجاء إدخال اسم الحساب", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _accountNameBox.Focus();
                return;
            }
            
            if (!_accountId.HasValue && !_parentId.HasValue && _accountTypeCombo.SelectedIndex == -1)
            {
                MessageBox.Show("برجاء اختيار نوع الحساب", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _accountTypeCombo.Focus();
                return;
            }
            
            if (!decimal.TryParse(_openingBalanceBox.Text, out decimal openingBalance))
            {
                openingBalance = 0;
            }
            
            if (_accountId.HasValue)
            {
                // تعديل
                if (_currentAccount != null)
                {
                    _currentAccount.AccountName = _accountNameBox.Text.Trim();
                    _currentAccount.AccountNameEn = _accountNameEnBox.Text.Trim();
                    _currentAccount.IsActive = _isActiveCheck.Checked;
                    _currentAccount.Notes = _notesBox.Text.Trim();
                    
                    await _accountService.UpdateAccountAsync(_currentAccount);
                    MessageBox.Show("تم تعديل الحساب بنجاح", "نجح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                // إضافة جديد
                var account = new Account
                {
                    AccountCode = _accountCodeBox.Text.Trim(),
                    AccountName = _accountNameBox.Text.Trim(),
                    AccountNameEn = _accountNameEnBox.Text.Trim(),
                    IsActive = _isActiveCheck.Checked,
                    OpeningBalance = openingBalance,
                    Notes = _notesBox.Text.Trim()
                };
                
                // تحديد الحساب الأب
                if (_parentAccountCombo.SelectedIndex > 0 && _parentAccountCombo.SelectedItem is ComboBoxItem item)
                {
                    account.ParentAccountId = item.Value;
                }
                
                // تحديد نوع الحساب
                if (!account.ParentAccountId.HasValue)
                {
                    account.AccountType = _accountTypeCombo.SelectedIndex switch
                    {
                        0 => "Asset",
                        1 => "Liability",
                        2 => "Equity",
                        3 => "Revenue",
                        4 => "Expense",
                        _ => "Asset"
                    };
                }
                
                await _accountService.CreateAccountAsync(account);
                MessageBox.Show("تم إضافة الحساب بنجاح", "نجح",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
            this.DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في الحفظ: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private class ComboBoxItem
    {
        public string Text { get; set; } = "";
        public int Value { get; set; }
        public override string ToString() => Text;
    }
}
