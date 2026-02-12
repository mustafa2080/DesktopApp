using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class AddEditCustomerForm : Form
{
    private readonly ICustomerService _customerService;
    private readonly int? _customerId;
    private Customer? _existingCustomer;
    
    private TextBox _codeText = null!;
    private TextBox _nameText = null!;
    private TextBox _nameEnText = null!;
    private TextBox _phoneText = null!;
    private TextBox _mobileText = null!;
    private TextBox _emailText = null!;
    private TextBox _addressText = null!;
    private TextBox _cityText = null!;
    private ComboBox _countryCombo = null!;
    private TextBox _taxNumberText = null!;
    private TextBox _creditLimitText = null!;
    private TextBox _paymentTermsText = null!;
    private TextBox _openingBalanceText = null!;
    private CheckBox _isActiveCheck = null!;
    private TextBox _notesText = null!;
    
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    
    public AddEditCustomerForm(ICustomerService customerService, int? customerId)
    {
        _customerService = customerService;
        _customerId = customerId;
        
        InitializeComponent();
        InitializeCustomControls();
        
        if (_customerId.HasValue)
        {
            _ = LoadCustomerDataAsync();
        }
    }
    
    private void InitializeComponent()
    {
        this.Text = _customerId.HasValue ? "تعديل عميل" : "إضافة عميل جديد";
        this.Size = new Size(900, 750);
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
            Padding = new Padding(30),
            AutoScroll = true
        };
        
        // Title
        Label titleLabel = new Label
        {
            Text = _customerId.HasValue ? "✏️ تعديل بيانات العميل" : "➕ إضافة عميل جديد",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(30, 20)
        };
        mainPanel.Controls.Add(titleLabel);
        
        int yPos = 80;
        int labelWidth = 150;
        int fieldX = labelWidth + 40;
        int fieldWidth = 600;
        
        // Customer Code
        AddLabel(mainPanel, "كود العميل:", new Point(30, yPos));
        _codeText = AddTextBox(mainPanel, new Point(fieldX, yPos), 250);
        _codeText.ReadOnly = _customerId.HasValue;
        _codeText.BackColor = _customerId.HasValue ? Color.FromArgb(240, 240, 240) : Color.White;
        yPos += 50;
        
        // Customer Name (Arabic)
        AddLabel(mainPanel, "* اسم العميل (عربي):", new Point(30, yPos));
        _nameText = AddTextBox(mainPanel, new Point(fieldX, yPos), fieldWidth);
        yPos += 50;
        
        // Customer Name (English)
        AddLabel(mainPanel, "اسم العميل (إنجليزي):", new Point(30, yPos));
        _nameEnText = AddTextBox(mainPanel, new Point(fieldX, yPos), fieldWidth);
        yPos += 50;
        
        // Phone & Mobile in same row
        AddLabel(mainPanel, "الهاتف:", new Point(30, yPos));
        _phoneText = AddTextBox(mainPanel, new Point(fieldX, yPos), 250);
        
        AddLabel(mainPanel, "الموبايل:", new Point(480, yPos));
        _mobileText = AddTextBox(mainPanel, new Point(570, yPos), 250);
        yPos += 50;
        
        // Email
        AddLabel(mainPanel, "البريد الإلكتروني:", new Point(30, yPos));
        _emailText = AddTextBox(mainPanel, new Point(fieldX, yPos), fieldWidth);
        yPos += 50;
        
        // Address
        AddLabel(mainPanel, "العنوان:", new Point(30, yPos));
        _addressText = AddTextBox(mainPanel, new Point(fieldX, yPos), fieldWidth, 60, true);
        yPos += 80;
        
        // City & Country in same row
        AddLabel(mainPanel, "المدينة:", new Point(30, yPos));
        _cityText = AddTextBox(mainPanel, new Point(fieldX, yPos), 250);
        
        AddLabel(mainPanel, "الدولة:", new Point(480, yPos));
        _countryCombo = AddComboBox(mainPanel, new Point(570, yPos), 250);
        _countryCombo.Items.AddRange(new object[] { "مصر", "السعودية", "الإمارات", "الكويت", "قطر", "البحرين", "عمان", "الأردن", "لبنان" });
        yPos += 50;
        
        // Tax Number
        AddLabel(mainPanel, "الرقم الضريبي:", new Point(30, yPos));
        _taxNumberText = AddTextBox(mainPanel, new Point(fieldX, yPos), 250);
        yPos += 60;
        
        // Section: Financial Settings
        Label financialLabel = new Label
        {
            Text = "💰 الإعدادات المالية",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = ColorScheme.Success,
            AutoSize = true,
            Location = new Point(30, yPos)
        };
        mainPanel.Controls.Add(financialLabel);
        yPos += 50;
        
        // Credit Limit & Payment Terms
        AddLabel(mainPanel, "الحد الائتماني:", new Point(30, yPos));
        _creditLimitText = AddTextBox(mainPanel, new Point(fieldX, yPos), 250);
        _creditLimitText.Text = "0";
        
        AddLabel(mainPanel, "مدة السداد (أيام):", new Point(480, yPos));
        _paymentTermsText = AddTextBox(mainPanel, new Point(620, yPos), 200);
        _paymentTermsText.Text = "30";
        yPos += 50;
        
        // Opening Balance
        AddLabel(mainPanel, "الرصيد الافتتاحي:", new Point(30, yPos));
        _openingBalanceText = AddTextBox(mainPanel, new Point(fieldX, yPos), 250);
        _openingBalanceText.Text = "0";
        _openingBalanceText.ReadOnly = _customerId.HasValue;
        _openingBalanceText.BackColor = _customerId.HasValue ? Color.FromArgb(240, 240, 240) : Color.White;
        yPos += 60;
        
        // Active Status
        _isActiveCheck = new CheckBox
        {
            Text = "عميل نشط",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(fieldX, yPos),
            Checked = true
        };
        mainPanel.Controls.Add(_isActiveCheck);
        yPos += 50;
        
        // Notes
        AddLabel(mainPanel, "ملاحظات:", new Point(30, yPos));
        _notesText = AddTextBox(mainPanel, new Point(fieldX, yPos), fieldWidth, 80, true);
        yPos += 100;
        
        // Buttons
        _saveButton = CreateButton("💾 حفظ", ColorScheme.Success, new Point(fieldX, yPos), SaveCustomer_Click);
        mainPanel.Controls.Add(_saveButton);
        
        _cancelButton = CreateButton("❌ إلغاء", ColorScheme.Error, new Point(fieldX + 180, yPos), (s, e) => this.Close());
        mainPanel.Controls.Add(_cancelButton);
        
        this.Controls.Add(mainPanel);
    }
    
    private void AddLabel(Panel panel, string text, Point location)
    {
        Label label = new Label
        {
            Text = text,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = location
        };
        panel.Controls.Add(label);
    }
    
    private TextBox AddTextBox(Panel panel, Point location, int width, int height = 30, bool multiline = false)
    {
        TextBox textBox = new TextBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(width, height),
            Location = location,
            Multiline = multiline
        };
        
        if (multiline)
        {
            textBox.ScrollBars = ScrollBars.Vertical;
        }
        
        panel.Controls.Add(textBox);
        return textBox;
    }
    
    private ComboBox AddComboBox(Panel panel, Point location, int width)
    {
        ComboBox comboBox = new ComboBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(width, 30),
            Location = location,
            DropDownStyle = ComboBoxStyle.DropDown
        };
        panel.Controls.Add(comboBox);
        return comboBox;
    }
    
    private Button CreateButton(string text, Color bgColor, Point location, EventHandler clickHandler)
    {
        Button btn = new Button
        {
            Text = text,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Size = new Size(160, 45),
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
    
    private async Task LoadCustomerDataAsync()
    {
        try
        {
            _existingCustomer = await _customerService.GetCustomerByIdAsync(_customerId!.Value);
            
            if (_existingCustomer == null)
            {
                MessageBox.Show("العميل غير موجود", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }
            
            _codeText.Text = _existingCustomer.CustomerCode;
            _nameText.Text = _existingCustomer.CustomerName;
            _nameEnText.Text = _existingCustomer.CustomerNameEn ?? "";
            _phoneText.Text = _existingCustomer.Phone ?? "";
            _mobileText.Text = _existingCustomer.Mobile ?? "";
            _emailText.Text = _existingCustomer.Email ?? "";
            _addressText.Text = _existingCustomer.Address ?? "";
            _cityText.Text = _existingCustomer.City ?? "";
            _countryCombo.Text = _existingCustomer.Country ?? "";
            _taxNumberText.Text = _existingCustomer.TaxNumber ?? "";
            _creditLimitText.Text = _existingCustomer.CreditLimit.ToString();
            _paymentTermsText.Text = _existingCustomer.PaymentTermDays.ToString();
            _openingBalanceText.Text = _existingCustomer.OpeningBalance.ToString();
            _isActiveCheck.Checked = _existingCustomer.IsActive;
            _notesText.Text = _existingCustomer.Notes ?? "";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async void SaveCustomer_Click(object? sender, EventArgs e)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(_nameText.Text))
            {
                MessageBox.Show("برجاء إدخال اسم العميل", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _nameText.Focus();
                return;
            }
            
            if (!decimal.TryParse(_creditLimitText.Text, out decimal creditLimit))
            {
                MessageBox.Show("برجاء إدخال حد ائتماني صحيح", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _creditLimitText.Focus();
                return;
            }
            
            if (!int.TryParse(_paymentTermsText.Text, out int paymentTerms))
            {
                MessageBox.Show("برجاء إدخال مدة سداد صحيحة", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _paymentTermsText.Focus();
                return;
            }
            
            if (!decimal.TryParse(_openingBalanceText.Text, out decimal openingBalance))
            {
                MessageBox.Show("برجاء إدخال رصيد افتتاحي صحيح", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _openingBalanceText.Focus();
                return;
            }
            
            if (_customerId.HasValue)
            {
                // Update existing customer
                _existingCustomer!.CustomerName = _nameText.Text.Trim();
                _existingCustomer.CustomerNameEn = _nameEnText.Text.Trim();
                _existingCustomer.Phone = _phoneText.Text.Trim();
                _existingCustomer.Mobile = _mobileText.Text.Trim();
                _existingCustomer.Email = _emailText.Text.Trim();
                _existingCustomer.Address = _addressText.Text.Trim();
                _existingCustomer.City = _cityText.Text.Trim();
                _existingCustomer.Country = _countryCombo.Text.Trim();
                _existingCustomer.TaxNumber = _taxNumberText.Text.Trim();
                _existingCustomer.CreditLimit = creditLimit;
                _existingCustomer.PaymentTermDays = paymentTerms;
                _existingCustomer.IsActive = _isActiveCheck.Checked;
                _existingCustomer.Notes = _notesText.Text.Trim();
                
                await _customerService.UpdateCustomerAsync(_existingCustomer);
                MessageBox.Show("تم تحديث بيانات العميل بنجاح", "نجح",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Create new customer
                var customer = new Customer
                {
                    CustomerCode = string.IsNullOrWhiteSpace(_codeText.Text) ? "" : _codeText.Text.Trim(),
                    CustomerName = _nameText.Text.Trim(),
                    CustomerNameEn = _nameEnText.Text.Trim(),
                    Phone = _phoneText.Text.Trim(),
                    Mobile = _mobileText.Text.Trim(),
                    Email = _emailText.Text.Trim(),
                    Address = _addressText.Text.Trim(),
                    City = _cityText.Text.Trim(),
                    Country = _countryCombo.Text.Trim(),
                    TaxNumber = _taxNumberText.Text.Trim(),
                    CreditLimit = creditLimit,
                    PaymentTermDays = paymentTerms,
                    OpeningBalance = openingBalance,
                    IsActive = _isActiveCheck.Checked,
                    Notes = _notesText.Text.Trim()
                };
                
                await _customerService.CreateCustomerAsync(customer);
                MessageBox.Show("تم إضافة العميل بنجاح", "نجح",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في حفظ البيانات: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
