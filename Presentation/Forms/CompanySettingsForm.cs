using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class CompanySettingsForm : Form
{
    private readonly ISettingService _settingService;
    private readonly int _currentUserId;
    private CompanySetting? _currentSettings;
    
    // Controls
    private Panel _headerPanel = null!;
    private Panel _contentPanel = null!;
    private Panel _footerPanel = null!;
    
    // Input Controls - Basic Info
    private TextBox _txtCompanyName = null!;
    private TextBox _txtCompanyNameEn = null!;
    private PictureBox _picLogo = null!;
    private Button _btnUploadLogo = null!;
    
    // Input Controls - Contact
    private TextBox _txtAddress = null!;
    private TextBox _txtCity = null!;
    private TextBox _txtCountry = null!;
    private TextBox _txtPhone = null!;
    private TextBox _txtMobile = null!;
    private TextBox _txtEmail = null!;
    private TextBox _txtWebsite = null!;
    
    // Input Controls - Tax Info
    private TextBox _txtTaxNumber = null!;
    private TextBox _txtCommercialNumber = null!;
    
    // Input Controls - Bank Info
    private TextBox _txtBankName = null!;
    private TextBox _txtBankAccountNumber = null!;
    private TextBox _txtBankIBAN = null!;
    
    // Action Buttons
    private Button _btnSave = null!;
    private Button _btnCancel = null!;
    
    public CompanySettingsForm(ISettingService settingService, int currentUserId)
    {
        _settingService = settingService;
        _currentUserId = currentUserId;
        
        InitializeComponent();
        SetupFormProperties();
        InitializeCustomControls();
        _ = LoadSettingsAsync();
    }
    
    private void SetupFormProperties()
    {
        this.Text = "الإعدادات";
        this.WindowState = FormWindowState.Maximized; // ملء الشاشة
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
        this.FormBorderStyle = FormBorderStyle.Sizable; // قابل لتغيير الحجم
        this.MaximizeBox = true;
        this.MinimizeBox = true;
    }
    
    private void InitializeCustomControls()
    {
        // Header Panel
        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 80,
            BackColor = ColorScheme.Primary,
            Padding = new Padding(40, 0, 40, 0)
        };
        
        // Create a container for the title to ensure proper positioning
        Panel titleContainer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent
        };
        
        Label titleLabel = new Label
        {
            Text = "⚙️ إعدادات معلومات الشركة",
            Font = new Font("Cairo", 20F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            Size = new Size(600, 80),
            TextAlign = ContentAlignment.MiddleRight,
            Location = new Point(0, 0),
            Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
            Padding = new Padding(0, 0, 20, 0)
        };
        
        // Position the label on the right side
        titleLabel.Location = new Point(titleContainer.Width - titleLabel.Width, 0);
        titleContainer.SizeChanged += (s, e) => {
            titleLabel.Location = new Point(titleContainer.Width - titleLabel.Width, 0);
        };
        
        titleContainer.Controls.Add(titleLabel);
        _headerPanel.Controls.Add(titleContainer);
        this.Controls.Add(_headerPanel);
        
        // Content Panel with Scroll
        _contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ColorScheme.Background,
            AutoScroll = false,
            Padding = new Padding(0) // بدون مسافة لأن mainContainer له padding
        };
        
        CreateFormControls();
        this.Controls.Add(_contentPanel);
        
        // Footer Panel
        _footerPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 80,
            BackColor = Color.White,
            Padding = new Padding(30, 15, 30, 15)
        };
        
        // Create button container for right alignment
        FlowLayoutPanel buttonContainer = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0)
        };
        
        _btnCancel = new Button
        {
            Text = "✖️ إلغاء",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Size = new Size(130, 50),
            BackColor = Color.FromArgb(108, 117, 125),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Margin = new Padding(10, 0, 0, 0)
        };
        _btnCancel.FlatAppearance.BorderSize = 0;
        _btnCancel.Click += (s, e) => this.Close();
        buttonContainer.Controls.Add(_btnCancel);
        
        _btnSave = new Button
        {
            Text = "💾 حفظ الإعدادات",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Size = new Size(160, 50),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Margin = new Padding(10, 0, 0, 0)
        };
        _btnSave.FlatAppearance.BorderSize = 0;
        _btnSave.Click += BtnSave_Click;
        buttonContainer.Controls.Add(_btnSave);
        
        _footerPanel.Controls.Add(buttonContainer);
        this.Controls.Add(_footerPanel);
    }
    
    private void CreateFormControls()
    {
        // Main Container - يملأ عرض الشاشة
        Panel mainContainer = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(50, 20, 50, 20),
            BackColor = ColorScheme.Background
        };
        
        // Inner Container للمحتوى
        Panel innerContainer = new Panel
        {
            Width = 1600,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Location = new Point(0, 0)
        };
        
        int yPos = 0;
        
        // ==================== القسم الأول: معلومات الشركة ====================
        Panel section1 = CreateModernSection("معلومات الشركة الأساسية", "🏢", yPos);
        
        // Grid Layout للقسم الأول
        TableLayoutPanel grid1 = new TableLayoutPanel
        {
            Location = new Point(30, 80),
            Width = section1.Width - 60,
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 2,
            Padding = new Padding(10)
        };
        
        // Setup columns: Logo (250) | Spacer (20) | Info (remaining)
        grid1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
        grid1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));
        grid1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        
        // Logo Panel - عمودي في الجانب
        Panel logoPanel = CreateLogoPanel();
        grid1.Controls.Add(logoPanel, 0, 0);
        grid1.SetRowSpan(logoPanel, 2);
        
        // Company Names في عمودين
        Panel namesPanel = new Panel { Width = grid1.Width - 280, Height = 200, AutoSize = false };
        
        CreateModernTextBox(namesPanel, "اسم الشركة بالعربية", ref _txtCompanyName, 0, 10, 
            namesPanel.Width - 20, true, "أدخل اسم شركتك بالعربية");
        
        CreateModernTextBox(namesPanel, "اسم الشركة بالإنجليزية", ref _txtCompanyNameEn, 0, 105, 
            namesPanel.Width - 20, false, "Enter your company name in English");
        _txtCompanyNameEn.RightToLeft = RightToLeft.Yes;
        _txtCompanyNameEn.Font = new Font("Segoe UI", 11F);
        
        grid1.Controls.Add(namesPanel, 2, 0);
        section1.Controls.Add(grid1);
        
        innerContainer.Controls.Add(section1);
        yPos += section1.Height + 20;
        
        // ==================== القسم الثاني: معلومات الاتصال ====================
        Panel section2 = CreateModernSection("معلومات الاتصال والتواصل", "📞", yPos);
        
        TableLayoutPanel grid2 = new TableLayoutPanel
        {
            Location = new Point(30, 80),
            Width = section2.Width - 60,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(10)
        };
        grid2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        
        int colWidth = (grid2.Width - 40) / 2;
        
        // Row 1: العنوان (Full width)
        Panel addressPanel = new Panel { Width = grid2.Width - 20, AutoSize = true };
        CreateModernTextBox(addressPanel, "العنوان الكامل", ref _txtAddress, 0, 0, 
            addressPanel.Width - 20, false, "عنوان الشركة بالتفصيل");
        grid2.Controls.Add(addressPanel, 0, 0);
        grid2.SetColumnSpan(addressPanel, 2);
        
        // Row 2: المدينة والدولة
        Panel cityPanel = new Panel { Width = colWidth - 20, AutoSize = true };
        CreateModernTextBox(cityPanel, "المدينة", ref _txtCity, 0, 0, 
            cityPanel.Width, false, "اسم المدينة");
        grid2.Controls.Add(cityPanel, 0, 1);
        
        Panel countryPanel = new Panel { Width = colWidth - 20, AutoSize = true };
        CreateModernTextBox(countryPanel, "الدولة", ref _txtCountry, 0, 0, 
            countryPanel.Width, false, "اسم الدولة");
        grid2.Controls.Add(countryPanel, 1, 1);
        
        // Row 3: الهاتف والموبايل
        Panel phonePanel = new Panel { Width = colWidth - 20, AutoSize = true };
        CreateModernTextBox(phonePanel, "الهاتف", ref _txtPhone, 0, 0, 
            phonePanel.Width, false, "رقم الهاتف الأرضي");
        grid2.Controls.Add(phonePanel, 0, 2);
        
        Panel mobilePanel = new Panel { Width = colWidth - 20, AutoSize = true };
        CreateModernTextBox(mobilePanel, "الموبايل", ref _txtMobile, 0, 0, 
            mobilePanel.Width, false, "رقم الموبايل");
        grid2.Controls.Add(mobilePanel, 1, 2);
        
        // Row 4: البريد والموقع
        Panel emailPanel = new Panel { Width = colWidth - 20, AutoSize = true };
        CreateModernTextBox(emailPanel, "البريد الإلكتروني", ref _txtEmail, 0, 0, 
            emailPanel.Width, false, "example@company.com");
        grid2.Controls.Add(emailPanel, 0, 3);
        
        Panel websitePanel = new Panel { Width = colWidth - 20, AutoSize = true };
        CreateModernTextBox(websitePanel, "الموقع الإلكتروني", ref _txtWebsite, 0, 0, 
            websitePanel.Width, false, "www.company.com");
        _txtWebsite.RightToLeft = RightToLeft.Yes;
        grid2.Controls.Add(websitePanel, 1, 3);
        
        section2.Controls.Add(grid2);
        innerContainer.Controls.Add(section2);
        yPos += section2.Height + 20;
        
        // ==================== القسم الثالث: المعلومات الضريبية ====================
        Panel section3 = CreateModernSection("المعلومات الضريبية والتجارية", "📋", yPos);
        
        TableLayoutPanel grid3 = new TableLayoutPanel
        {
            Location = new Point(30, 80),
            Width = section3.Width - 60,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(10)
        };
        grid3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        
        Panel taxPanel = new Panel { Width = colWidth - 20, AutoSize = true };
        CreateModernTextBox(taxPanel, "رقم التسجيل الضريبي", ref _txtTaxNumber, 0, 0, 
            taxPanel.Width, false, "رقم البطاقة الضريبية");
        grid3.Controls.Add(taxPanel, 0, 0);
        
        Panel commercialPanel = new Panel { Width = colWidth - 20, AutoSize = true };
        CreateModernTextBox(commercialPanel, "رقم السجل التجاري", ref _txtCommercialNumber, 0, 0, 
            commercialPanel.Width, false, "رقم السجل التجاري");
        grid3.Controls.Add(commercialPanel, 1, 0);
        
        section3.Controls.Add(grid3);
        innerContainer.Controls.Add(section3);
        yPos += section3.Height + 20;
        
        // ==================== القسم الرابع: الحسابات البنكية ====================
        Panel section4 = CreateModernSection("الحسابات البنكية", "🏦", yPos);
        
        TableLayoutPanel grid4 = new TableLayoutPanel
        {
            Location = new Point(30, 80),
            Width = section4.Width - 60,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(10)
        };
        grid4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        
        // Row 1: اسم البنك (Full width)
        Panel bankNamePanel = new Panel { Width = grid4.Width - 20, AutoSize = true };
        CreateModernTextBox(bankNamePanel, "اسم البنك", ref _txtBankName, 0, 0, 
            bankNamePanel.Width - 20, false, "اسم البنك التابع له الحساب");
        grid4.Controls.Add(bankNamePanel, 0, 0);
        grid4.SetColumnSpan(bankNamePanel, 2);
        
        // Row 2: رقم الحساب و IBAN
        Panel accountPanel = new Panel { Width = colWidth - 20, AutoSize = true };
        CreateModernTextBox(accountPanel, "رقم الحساب البنكي", ref _txtBankAccountNumber, 0, 0, 
            accountPanel.Width, false, "رقم الحساب");
        grid4.Controls.Add(accountPanel, 0, 1);
        
        Panel ibanPanel = new Panel { Width = colWidth - 20, AutoSize = true };
        CreateModernTextBox(ibanPanel, "رقم الآيبان (IBAN)", ref _txtBankIBAN, 0, 0, 
            ibanPanel.Width, false, "EG00 0000 0000 0000");
        _txtBankIBAN.RightToLeft = RightToLeft.Yes;
        grid4.Controls.Add(ibanPanel, 1, 1);
        
        section4.Controls.Add(grid4);
        innerContainer.Controls.Add(section4);
        
        mainContainer.Controls.Add(innerContainer);
        _contentPanel.Controls.Add(mainContainer);
    }
    
    private Panel CreateModernSection(string title, string icon, int yPosition)
    {
        Panel section = new Panel
        {
            Location = new Point(0, yPosition),
            Width = 1580,
            AutoSize = true,
            BackColor = Color.White,
            Padding = new Padding(0, 0, 0, 20)
        };
        
        // Modern Header with gradient effect
        Panel header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = ColorScheme.Primary,
            Padding = new Padding(20, 5, 20, 5)
        };
        
        // Icon on the right
        Label iconLabel = new Label
        {
            Text = icon,
            Font = new Font("Segoe UI Emoji", 28F),
            ForeColor = Color.White,
            AutoSize = false,
            Size = new Size(60, 60),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Right
        };
        header.Controls.Add(iconLabel);
        
        // Title next to icon
        Label titleLabel = new Label
        {
            Text = title,
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            Height = 60,
            Width = 500,
            TextAlign = ContentAlignment.MiddleRight,
            Dock = DockStyle.Right,
            Padding = new Padding(0, 0, 20, 0)
        };
        header.Controls.Add(titleLabel);
        
        section.Controls.Add(header);
        section.Height = 400; // سيتم التعديل حسب المحتوى
        
        return section;
    }
    
    private Panel CreateLogoPanel()
    {
        Panel logoPanel = new Panel
        {
            Width = 240,
            Height = 320,
            BackColor = Color.FromArgb(248, 249, 250),
            Padding = new Padding(15)
        };
        
        // Logo Title
        Label logoTitle = new Label
        {
            Text = "شعار الشركة",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = false,
            Size = new Size(210, 35),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(15, 10),
            BackColor = Color.White
        };
        logoPanel.Controls.Add(logoTitle);
        
        // Logo Border
        Panel logoBorder = new Panel
        {
            Location = new Point(20, 50),
            Size = new Size(200, 200),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        
        _picLogo = new PictureBox
        {
            Location = new Point(10, 10),
            Size = new Size(180, 180),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(250, 250, 250)
        };
        logoBorder.Controls.Add(_picLogo);
        logoPanel.Controls.Add(logoBorder);
        
        _btnUploadLogo = new Button
        {
            Text = "📁 اختيار الشعار",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Location = new Point(35, 265),
            Size = new Size(170, 45),
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnUploadLogo.FlatAppearance.BorderSize = 0;
        _btnUploadLogo.Click += BtnUploadLogo_Click;
        logoPanel.Controls.Add(_btnUploadLogo);
        
        return logoPanel;
    }
    
    private void CreateModernTextBox(Panel parent, string labelText, ref TextBox textBox, 
        int x, int y, int width, bool required = false, string placeholder = "")
    {
        // Label
        Label label = new Label
        {
            Text = labelText + (required ? " *" : ""),
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = required ? ColorScheme.Error : Color.FromArgb(52, 58, 64),
            AutoSize = false,
            Size = new Size(width, 25),
            TextAlign = ContentAlignment.MiddleRight,
            Location = new Point(x, y),
            RightToLeft = RightToLeft.Yes
        };
        parent.Controls.Add(label);
        
        // TextBox with modern styling
        textBox = new TextBox
        {
            Font = new Font("Cairo", 11F),
            Location = new Point(x, y + 30),
            Size = new Size(width, 45),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White,
            RightToLeft = RightToLeft.Yes,
            ForeColor = Color.FromArgb(33, 37, 41)
        };
        
        // Placeholder effect - store reference locally
        var txt = textBox;
        if (!string.IsNullOrEmpty(placeholder))
        {
            txt.Text = placeholder;
            txt.ForeColor = Color.Gray;
            
            txt.Enter += (s, e) =>
            {
                if (txt.Text == placeholder)
                {
                    txt.Text = "";
                    txt.ForeColor = Color.FromArgb(33, 37, 41);
                }
            };
            
            txt.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    txt.Text = placeholder;
                    txt.ForeColor = Color.Gray;
                }
            };
        }
        
        parent.Controls.Add(textBox);
    }
    
    private async Task LoadSettingsAsync()
    {
        try
        {
            _currentSettings = await _settingService.GetCompanySettingsAsync();
            
            if (_currentSettings != null)
            {
                SetTextBoxValue(_txtCompanyName, _currentSettings.CompanyName);
                SetTextBoxValue(_txtCompanyNameEn, _currentSettings.CompanyNameEnglish);
                SetTextBoxValue(_txtAddress, _currentSettings.Address);
                SetTextBoxValue(_txtCity, _currentSettings.City);
                SetTextBoxValue(_txtCountry, _currentSettings.Country ?? "مصر");
                SetTextBoxValue(_txtPhone, _currentSettings.Phone);
                SetTextBoxValue(_txtMobile, _currentSettings.Mobile);
                SetTextBoxValue(_txtEmail, _currentSettings.Email);
                SetTextBoxValue(_txtWebsite, _currentSettings.Website);
                SetTextBoxValue(_txtTaxNumber, _currentSettings.TaxRegistrationNumber);
                SetTextBoxValue(_txtCommercialNumber, _currentSettings.CommercialRegistrationNumber);
                SetTextBoxValue(_txtBankName, _currentSettings.BankName);
                SetTextBoxValue(_txtBankAccountNumber, _currentSettings.BankAccountNumber);
                SetTextBoxValue(_txtBankIBAN, _currentSettings.BankIBAN);
                
                // Load logo if exists
                if (!string.IsNullOrEmpty(_currentSettings.LogoPath))
                {
                    try
                    {
                        if (File.Exists(_currentSettings.LogoPath))
                        {
                            _picLogo.Image = Image.FromFile(_currentSettings.LogoPath);
                        }
                    }
                    catch
                    {
                        // Ignore errors loading logo
                    }
                }
            }
            else
            {
                // Set defaults for new settings
                SetTextBoxValue(_txtCountry, "مصر");
                _currentSettings = new CompanySetting
                {
                    Country = "مصر"
                };
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل الإعدادات: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }
    
    private void SetTextBoxValue(TextBox textBox, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            textBox.Text = value;
            textBox.ForeColor = Color.FromArgb(33, 37, 41);
        }
    }
    
    private void BtnUploadLogo_Click(object? sender, EventArgs e)
    {
        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            openFileDialog.Title = "اختر شعار الشركة";
            
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Load and display the image
                    _picLogo.Image = Image.FromFile(openFileDialog.FileName);
                    
                    // Copy to application directory
                    string appDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logos");
                    if (!Directory.Exists(appDir))
                    {
                        Directory.CreateDirectory(appDir);
                    }
                    
                    string fileName = $"company_logo{Path.GetExtension(openFileDialog.FileName)}";
                    string destinationPath = Path.Combine(appDir, fileName);
                    
                    File.Copy(openFileDialog.FileName, destinationPath, true);
                    
                    // Store path temporarily (will be saved when user clicks Save)
                    if (_currentSettings != null)
                    {
                        _currentSettings.LogoPath = destinationPath;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في رفع الصورة: {ex.Message}", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                }
            }
        }
    }
    
    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            // Get actual values (not placeholders)
            string? companyName = GetTextBoxValue(_txtCompanyName);
            
            // Validation
            if (string.IsNullOrWhiteSpace(companyName))
            {
                MessageBox.Show("يرجى إدخال اسم الشركة", "تحذير",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                _txtCompanyName.Focus();
                return;
            }
            
            var settings = _currentSettings ?? new CompanySetting();
            
            settings.CompanyName = companyName!;
            settings.CompanyNameEnglish = GetTextBoxValue(_txtCompanyNameEn) ?? string.Empty;
            settings.Address = GetTextBoxValue(_txtAddress);
            settings.City = GetTextBoxValue(_txtCity);
            settings.Country = GetTextBoxValue(_txtCountry) ?? "مصر";
            settings.Phone = GetTextBoxValue(_txtPhone);
            settings.Mobile = GetTextBoxValue(_txtMobile);
            settings.Email = GetTextBoxValue(_txtEmail);
            settings.Website = GetTextBoxValue(_txtWebsite);
            settings.TaxRegistrationNumber = GetTextBoxValue(_txtTaxNumber);
            settings.CommercialRegistrationNumber = GetTextBoxValue(_txtCommercialNumber);
            settings.BankName = GetTextBoxValue(_txtBankName);
            settings.BankAccountNumber = GetTextBoxValue(_txtBankAccountNumber);
            settings.BankIBAN = GetTextBoxValue(_txtBankIBAN);
            settings.LastModifiedByUserId = _currentUserId;
            settings.LastModifiedDate = DateTime.UtcNow;
            
            var success = await _settingService.SaveCompanySettingsAsync(settings);
            
            if (success)
            {
                MessageBox.Show("✓ تم حفظ إعدادات الشركة بنجاح", "نجح",
                    MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("حدث خطأ أثناء حفظ الإعدادات", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في حفظ الإعدادات: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }
    
    private string? GetTextBoxValue(TextBox textBox)
    {
        // Return null if text is placeholder (gray color)
        if (textBox.ForeColor == Color.Gray || string.IsNullOrWhiteSpace(textBox.Text))
            return null;
        
        return textBox.Text.Trim();
    }
}
