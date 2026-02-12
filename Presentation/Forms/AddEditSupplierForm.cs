using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class AddEditSupplierForm : Form
{
    private readonly ISupplierService _supplierService;
    private readonly Supplier? _existingSupplier;
    private readonly bool _isEditMode;
    
    // Controls
    private TextBox _codeText = null!;
    private TextBox _nameText = null!;
    private TextBox _nameEnText = null!;
    private TextBox _phoneText = null!;
    private TextBox _mobileText = null!;
    private TextBox _emailText = null!;
    private TextBox _addressText = null!;
    private TextBox _cityText = null!;
    private TextBox _countryText = null!;
    private TextBox _taxNumberText = null!;
    private TextBox _creditLimitText = null!;
    private TextBox _paymentTermsText = null!;
    private TextBox _openingBalanceText = null!;
    private CheckBox _isActiveCheck = null!;
    private TextBox _notesText = null!;
    
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    
    public AddEditSupplierForm(ISupplierService supplierService, Supplier? existingSupplier = null)
    {
        _supplierService = supplierService;
        _existingSupplier = existingSupplier;
        _isEditMode = existingSupplier != null;
        
        InitializeComponent();
        
        this.Text = _isEditMode ? "تعديل مورد" : "إضافة مورد جديد";
        this.Size = new Size(1100, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 9.5F);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        
        InitializeCustomControls();
        
        if (_isEditMode)
        {
            LoadSupplierData();
        }
        else
        {
            _ = LoadDefaultDataAsync();
        }
    }
    
    private void InitializeCustomControls()
    {
        // Main Container with scroll
        Panel scrollPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = ColorScheme.Background,
            Padding = new Padding(0)
        };
        
        // Inner Container
        Panel mainContainer = new Panel
        {
            Location = new Point(20, 0),
            Size = new Size(1040, 2000),
            BackColor = ColorScheme.Background
        };
        
        int yPos = 20;
        
        // ==================== HEADER SECTION ====================
        Panel headerPanel = CreateHeaderPanel(_isEditMode);
        headerPanel.Location = new Point(20, yPos);
        mainContainer.Controls.Add(headerPanel);
        yPos += headerPanel.Height + 20;
        
        // ==================== BASIC INFO SECTION ====================
        GroupBox basicInfoPanel = CreateBasicInfoSection();
        basicInfoPanel.Location = new Point(20, yPos);
        mainContainer.Controls.Add(basicInfoPanel);
        yPos += basicInfoPanel.Height + 20;
        
        // ==================== CONTACT INFO SECTION ====================
        GroupBox contactPanel = CreateContactInfoSection();
        contactPanel.Location = new Point(20, yPos);
        mainContainer.Controls.Add(contactPanel);
        yPos += contactPanel.Height + 20;
        
        // ==================== LOCATION INFO SECTION ====================
        GroupBox locationPanel = CreateLocationSection();
        locationPanel.Location = new Point(20, yPos);
        mainContainer.Controls.Add(locationPanel);
        yPos += locationPanel.Height + 20;
        
        // ==================== FINANCIAL INFO SECTION ====================
        GroupBox financialPanel = CreateFinancialSection();
        financialPanel.Location = new Point(20, yPos);
        mainContainer.Controls.Add(financialPanel);
        yPos += financialPanel.Height + 20;
        
        // ==================== ADDITIONAL INFO SECTION ====================
        GroupBox additionalPanel = CreateAdditionalInfoSection();
        additionalPanel.Location = new Point(20, yPos);
        mainContainer.Controls.Add(additionalPanel);
        yPos += additionalPanel.Height + 30;
        
        // ==================== ACTION BUTTONS ====================
        Panel buttonPanel = CreateButtonsPanel();
        buttonPanel.Location = new Point(20, yPos);
        mainContainer.Controls.Add(buttonPanel);
        yPos += buttonPanel.Height + 30;
        
        // Set container height
        mainContainer.Height = yPos;
        
        scrollPanel.Controls.Add(mainContainer);
        this.Controls.Add(scrollPanel);
    }
    
    // ==================== SECTION CREATORS ====================
    
    private Panel CreateHeaderPanel(bool isEdit)
    {
        Panel header = new Panel
        {
            Size = new Size(1000, 80),
            BackColor = Color.White,
            Padding = new Padding(20)
        };
        
        // Add subtle shadow effect
        header.Paint += (s, e) =>
        {
            using (var shadow = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Rectangle(0, header.Height - 3, header.Width, 3),
                Color.FromArgb(30, 0, 0, 0),
                Color.Transparent,
                90f))
            {
                e.Graphics.FillRectangle(shadow, 0, header.Height - 3, header.Width, 3);
            }
        };
        
        // Icon and Title
        Label icon = new Label
        {
            Text = isEdit ? "✏️" : "➕",
            Font = new Font("Segoe UI Emoji", 24F),
            AutoSize = true,
            Location = new Point(940, 15)
        };
        header.Controls.Add(icon);
        
        Label title = new Label
        {
            Text = isEdit ? "تعديل بيانات المورد" : "إضافة مورد جديد",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(750, 20)
        };
        header.Controls.Add(title);
        
        Label subtitle = new Label
        {
            Text = isEdit ? "قم بتعديل البيانات المطلوبة وحفظها" : "أدخل بيانات المورد الجديد بشكل كامل",
            Font = new Font("Cairo", 9F),
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(640, 50)
        };
        header.Controls.Add(subtitle);
        
        return header;
    }
    
    private GroupBox CreateBasicInfoSection()
    {
        GroupBox group = CreateSectionGroup("📋 المعلومات الأساسية", 1000, 200);
        
        int y = 40;
        
        // Supplier Code (Full Width)
        CreateFieldLabel("كود المورد", 30, y, group);
        _codeText = CreateStyledTextBox(250, y, 700, group);
        _codeText.ReadOnly = _isEditMode;
        _codeText.BackColor = _isEditMode ? Color.FromArgb(245, 245, 245) : Color.White;
        if (_isEditMode)
        {
            _codeText.Font = new Font("Cairo", 9.5F, FontStyle.Bold);
            _codeText.ForeColor = Color.FromArgb(100, 100, 100);
        }
        y += 50;
        
        // Supplier Name Arabic (Required Field)
        CreateFieldLabel("اسم المورد (عربي)", 30, y, group, true);
        _nameText = CreateStyledTextBox(250, y, 700, group);
        _nameText.Font = new Font("Cairo", 10F, FontStyle.Bold);
        y += 50;
        
        // Supplier Name English
        CreateFieldLabel("اسم المورد (إنجليزي)", 30, y, group);
        _nameEnText = CreateStyledTextBox(250, y, 700, group);
        _nameEnText.Font = new Font("Arial", 10F);
        
        return group;
    }
    
    private GroupBox CreateContactInfoSection()
    {
        GroupBox group = CreateSectionGroup("📞 بيانات الاتصال", 1000, 200);
        
        int y = 40;
        
        // Phone and Mobile (Two Columns)
        CreateFieldLabel("📱 الهاتف", 30, y, group);
        _phoneText = CreateStyledTextBox(250, y, 280, group);
        _phoneText.PlaceholderText = "مثال: 0123456789";
        
        CreateFieldLabel("📱 الموبايل", 560, y, group);
        _mobileText = CreateStyledTextBox(720, y, 230, group);
        _mobileText.PlaceholderText = "مثال: 01012345678";
        y += 50;
        
        // Email (Full Width)
        CreateFieldLabel("✉️ البريد الإلكتروني", 30, y, group);
        _emailText = CreateStyledTextBox(250, y, 700, group);
        _emailText.Font = new Font("Arial", 10F);
        _emailText.PlaceholderText = "example@company.com";
        y += 50;
        
        // Address (Multiline Full Width)
        CreateFieldLabel("🏢 العنوان التفصيلي", 30, y, group);
        _addressText = CreateStyledTextBox(250, y, 700, group);
        _addressText.Multiline = true;
        _addressText.Height = 60;
        _addressText.ScrollBars = ScrollBars.Vertical;
        
        return group;
    }
    
    private GroupBox CreateLocationSection()
    {
        GroupBox group = CreateSectionGroup("🌍 معلومات الموقع", 1000, 110);
        
        int y = 40;
        
        // City and Country (Two Columns)
        CreateFieldLabel("🏙️ المدينة", 30, y, group);
        _cityText = CreateStyledTextBox(250, y, 280, group);
        _cityText.PlaceholderText = "مثال: القاهرة";
        
        CreateFieldLabel("🌐 الدولة", 560, y, group);
        _countryText = CreateStyledTextBox(720, y, 230, group);
        _countryText.PlaceholderText = "مثال: مصر";
        
        return group;
    }
    
    private GroupBox CreateFinancialSection()
    {
        GroupBox group = CreateSectionGroup("💰 المعلومات المالية والضريبية", 1000, _isEditMode ? 160 : 210);
        
        int y = 40;
        
        // Tax Number (Full Width)
        CreateFieldLabel("🏛️ الرقم الضريبي", 30, y, group);
        _taxNumberText = CreateStyledTextBox(250, y, 700, group);
        _taxNumberText.Font = new Font("Arial", 10F);
        _taxNumberText.PlaceholderText = "مثال: 123-456-789";
        y += 50;
        
        // Credit Limit and Payment Terms (Two Columns)
        CreateFieldLabel("💳 حد الائتمان (جنيه)", 30, y, group);
        _creditLimitText = CreateStyledTextBox(250, y, 280, group);
        _creditLimitText.Text = "0";
        _creditLimitText.Font = new Font("Arial", 10F, FontStyle.Bold);
        _creditLimitText.ForeColor = ColorScheme.Primary;
        _creditLimitText.TextAlign = HorizontalAlignment.Right;
        
        CreateFieldLabel("⏱️ مدة السداد (أيام)", 560, y, group);
        _paymentTermsText = CreateStyledTextBox(720, y, 230, group);
        _paymentTermsText.Text = "30";
        _paymentTermsText.Font = new Font("Arial", 10F);
        _paymentTermsText.TextAlign = HorizontalAlignment.Center;
        y += 50;
        
        // Opening Balance (Only for new suppliers)
        if (!_isEditMode)
        {
            CreateFieldLabel("💵 الرصيد الافتتاحي (جنيه)", 30, y, group);
            _openingBalanceText = CreateStyledTextBox(250, y, 280, group);
            _openingBalanceText.Text = "0";
            _openingBalanceText.Font = new Font("Arial", 10F, FontStyle.Bold);
            _openingBalanceText.ForeColor = ColorScheme.Success;
            _openingBalanceText.TextAlign = HorizontalAlignment.Right;
            
            // Info Label
            Label infoLabel = new Label
            {
                Text = "ℹ️ الرصيد الافتتاحي يستخدم فقط عند إنشاء المورد",
                Font = new Font("Cairo", 8F),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(560, y + 10)
            };
            group.Controls.Add(infoLabel);
        }
        
        return group;
    }
    
    private GroupBox CreateAdditionalInfoSection()
    {
        GroupBox group = CreateSectionGroup("📝 معلومات إضافية", 1000, 200);
        
        int y = 40;
        
        // Active Status Checkbox with enhanced styling
        Panel statusPanel = new Panel
        {
            Size = new Size(200, 35),
            Location = new Point(750, y),
            BackColor = Color.FromArgb(240, 248, 255),
            BorderStyle = BorderStyle.FixedSingle
        };
        
        _isActiveCheck = new CheckBox
        {
            Text = "✓ مورد نشط",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = ColorScheme.Success,
            Checked = true,
            AutoSize = true,
            Location = new Point(90, 7)
        };
        statusPanel.Controls.Add(_isActiveCheck);
        group.Controls.Add(statusPanel);
        y += 50;
        
        // Notes (Multiline Full Width)
        CreateFieldLabel("📌 ملاحظات", 30, y, group);
        _notesText = CreateStyledTextBox(250, y, 700, group);
        _notesText.Multiline = true;
        _notesText.Height = 80;
        _notesText.ScrollBars = ScrollBars.Vertical;
        _notesText.PlaceholderText = "أضف أي ملاحظات أو تفاصيل إضافية هنا...";
        
        return group;
    }
    
    private Panel CreateButtonsPanel()
    {
        Panel buttonPanel = new Panel
        {
            Size = new Size(1000, 60),
            BackColor = Color.Transparent
        };
        
        // Save Button
        _saveButton = new Button
        {
            Text = "💾 حفظ البيانات",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Size = new Size(180, 50),
            Location = new Point(800, 5),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            TabIndex = 0
        };
        _saveButton.FlatAppearance.BorderSize = 0;
        _saveButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 167, 69);
        _saveButton.Click += SaveSupplier_Click;
        buttonPanel.Controls.Add(_saveButton);
        
        // Cancel Button
        _cancelButton = new Button
        {
            Text = "✖️ إلغاء",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Size = new Size(150, 50),
            Location = new Point(630, 5),
            BackColor = Color.FromArgb(108, 117, 125),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            TabIndex = 1
        };
        _cancelButton.FlatAppearance.BorderSize = 0;
        _cancelButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(90, 98, 104);
        _cancelButton.Click += (s, e) => this.Close();
        buttonPanel.Controls.Add(_cancelButton);
        
        return buttonPanel;
    }
    
    // ==================== HELPER METHODS ====================
    
    private GroupBox CreateSectionGroup(string title, int width, int height)
    {
        GroupBox group = new GroupBox
        {
            Text = "  " + title + "  ",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            Size = new Size(width, height),
            BackColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        
        // Add shadow border
        group.Paint += (s, e) =>
        {
            using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, group.Width - 1, group.Height - 1);
            }
        };
        
        return group;
    }
    
    private void CreateFieldLabel(string text, int x, int y, Control parent, bool required = false)
    {
        Label label = new Label
        {
            Text = text + (required ? " *" : ""),
            Font = new Font("Cairo", 9.5F, FontStyle.Bold),
            ForeColor = required ? ColorScheme.Error : Color.FromArgb(70, 70, 70),
            AutoSize = true,
            Location = new Point(x, y + 5)
        };
        parent.Controls.Add(label);
    }
    
    private TextBox CreateStyledTextBox(int x, int y, int width, Control parent)
    {
        TextBox txt = new TextBox
        {
            Font = new Font("Cairo", 9.5F),
            Size = new Size(width, 32),
            Location = new Point(x, y),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White
        };
        
        // Add focus effect
        txt.Enter += (s, e) =>
        {
            txt.BackColor = Color.FromArgb(255, 252, 240);
        };
        
        txt.Leave += (s, e) =>
        {
            if (!txt.ReadOnly)
                txt.BackColor = Color.White;
        };
        
        parent.Controls.Add(txt);
        return txt;
    }
    
    // ==================== DATA LOADING ====================
    
    private async Task LoadDefaultDataAsync()
    {
        try
        {
            string code = await _supplierService.GenerateSupplierCodeAsync();
            _codeText.Text = code;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void LoadSupplierData()
    {
        if (_existingSupplier == null) return;
        
        _codeText.Text = _existingSupplier.SupplierCode;
        _nameText.Text = _existingSupplier.SupplierName;
        _nameEnText.Text = _existingSupplier.SupplierNameEn;
        _phoneText.Text = _existingSupplier.Phone;
        _mobileText.Text = _existingSupplier.Mobile;
        _emailText.Text = _existingSupplier.Email;
        _addressText.Text = _existingSupplier.Address;
        _cityText.Text = _existingSupplier.City;
        _countryText.Text = _existingSupplier.Country;
        _taxNumberText.Text = _existingSupplier.TaxNumber;
        _creditLimitText.Text = _existingSupplier.CreditLimit.ToString("N2");
        _paymentTermsText.Text = _existingSupplier.PaymentTermDays.ToString();
        _isActiveCheck.Checked = _existingSupplier.IsActive;
        _notesText.Text = _existingSupplier.Notes;
    }
    
    // ==================== SAVE LOGIC ====================
    
    private async void SaveSupplier_Click(object? sender, EventArgs e)
    {
        try
        {
            // Disable save button to prevent double-click
            _saveButton.Enabled = false;
            _saveButton.Text = "⏳ جاري الحفظ...";
            
            // ========== VALIDATION ==========
            
            // Supplier Name (Required)
            if (string.IsNullOrWhiteSpace(_nameText.Text))
            {
                ShowValidationError("برجاء إدخال اسم المورد", _nameText);
                return;
            }
            
            // Credit Limit
            if (!decimal.TryParse(_creditLimitText.Text, out decimal creditLimit))
            {
                ShowValidationError("برجاء إدخال حد ائتمان صحيح (رقم فقط)", _creditLimitText);
                return;
            }
            
            if (creditLimit < 0)
            {
                ShowValidationError("حد الائتمان لا يمكن أن يكون سالب", _creditLimitText);
                return;
            }
            
            // Payment Terms
            if (!int.TryParse(_paymentTermsText.Text, out int paymentTerms))
            {
                ShowValidationError("برجاء إدخال مدة سداد صحيحة (رقم صحيح فقط)", _paymentTermsText);
                return;
            }
            
            if (paymentTerms < 0)
            {
                ShowValidationError("مدة السداد لا يمكن أن تكون سالبة", _paymentTermsText);
                return;
            }
            
            // Email Validation (if provided)
            if (!string.IsNullOrWhiteSpace(_emailText.Text) && !IsValidEmail(_emailText.Text))
            {
                ShowValidationError("برجاء إدخال بريد إلكتروني صحيح", _emailText);
                return;
            }
            
            // Opening Balance (for new suppliers only)
            decimal openingBalance = 0;
            if (!_isEditMode && _openingBalanceText != null)
            {
                if (!decimal.TryParse(_openingBalanceText.Text, out openingBalance))
                {
                    ShowValidationError("برجاء إدخال رصيد افتتاحي صحيح (رقم فقط)", _openingBalanceText);
                    return;
                }
            }
            
            // ========== SAVE/UPDATE ==========
            
            if (_isEditMode && _existingSupplier != null)
            {
                // Update existing supplier
                _existingSupplier.SupplierName = _nameText.Text.Trim();
                _existingSupplier.SupplierNameEn = _nameEnText.Text.Trim();
                _existingSupplier.Phone = _phoneText.Text.Trim();
                _existingSupplier.Mobile = _mobileText.Text.Trim();
                _existingSupplier.Email = _emailText.Text.Trim();
                _existingSupplier.Address = _addressText.Text.Trim();
                _existingSupplier.City = _cityText.Text.Trim();
                _existingSupplier.Country = _countryText.Text.Trim();
                _existingSupplier.TaxNumber = _taxNumberText.Text.Trim();
                _existingSupplier.CreditLimit = creditLimit;
                _existingSupplier.PaymentTermDays = paymentTerms;
                _existingSupplier.IsActive = _isActiveCheck.Checked;
                _existingSupplier.Notes = _notesText.Text.Trim();
                
                await _supplierService.UpdateSupplierAsync(_existingSupplier);
                
                MessageBox.Show(
                    "✅ تم تحديث بيانات المورد بنجاح",
                    "عملية ناجحة",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                // Create new supplier
                var supplier = new Supplier
                {
                    SupplierCode = _codeText.Text.Trim(),
                    SupplierName = _nameText.Text.Trim(),
                    SupplierNameEn = _nameEnText.Text.Trim(),
                    Phone = _phoneText.Text.Trim(),
                    Mobile = _mobileText.Text.Trim(),
                    Email = _emailText.Text.Trim(),
                    Address = _addressText.Text.Trim(),
                    City = _cityText.Text.Trim(),
                    Country = _countryText.Text.Trim(),
                    TaxNumber = _taxNumberText.Text.Trim(),
                    CreditLimit = creditLimit,
                    PaymentTermDays = paymentTerms,
                    OpeningBalance = openingBalance,
                    IsActive = _isActiveCheck.Checked,
                    Notes = _notesText.Text.Trim()
                };
                
                await _supplierService.CreateSupplierAsync(supplier);
                
                MessageBox.Show(
                    "✅ تم إضافة المورد بنجاح\n\nيمكنك الآن استخدامه في العمليات المختلفة",
                    "عملية ناجحة",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"❌ خطأ في حفظ المورد:\n\n{ex.Message}\n\nبرجاء المحاولة مرة أخرى أو الاتصال بالدعم الفني",
                "خطأ في النظام",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            // Re-enable save button
            _saveButton.Enabled = true;
            _saveButton.Text = "💾 حفظ البيانات";
        }
    }
    
    // ==================== VALIDATION HELPERS ====================
    
    private void ShowValidationError(string message, Control? control = null)
    {
        MessageBox.Show(
            message,
            "⚠️ بيانات غير صحيحة",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
        
        if (control != null)
        {
            control.BackColor = Color.FromArgb(255, 240, 240);
            control.Focus();
            
            // Reset color after 2 seconds
            var timer = new System.Windows.Forms.Timer { Interval = 2000 };
            timer.Tick += (s, e) =>
            {
                if (control is TextBox textBox && !textBox.ReadOnly)
                    control.BackColor = Color.White;
                else if (control is not TextBox)
                    control.BackColor = Color.White;
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }
        
        // Re-enable save button
        _saveButton.Enabled = true;
        _saveButton.Text = "💾 حفظ البيانات";
    }
    
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
