using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class InvoiceSettingsForm : Form
{
    private readonly ISettingService _settingService;
    private readonly int _currentUserId;
    private InvoiceSetting? _currentSettings;
    
    // Controls
    private Panel _headerPanel = null!;
    private TabControl _tabControl = null!;
    private Panel _footerPanel = null!;
    
    // Tax Settings Controls
    private CheckBox _chkEnableTax = null!;
    private NumericUpDown _numTaxRate = null!;
    private TextBox _txtTaxLabel = null!;
    
    // Numbering Controls
    private CheckBox _chkAutoNumbering = null!;
    private TextBox _txtSalesPrefix = null!;
    private TextBox _txtPurchasePrefix = null!;
    private NumericUpDown _numNextSalesNumber = null!;
    private NumericUpDown _numNextPurchaseNumber = null!;
    private NumericUpDown _numNumberLength = null!;
    
    // Invoice Text Controls
    private TextBox _txtFooterText = null!;
    private TextBox _txtPaymentTerms = null!;
    private TextBox _txtBankDetails = null!;
    private TextBox _txtNotesTemplate = null!;
    
    // Display Settings Controls
    private CheckBox _chkShowLogo = null!;
    private CheckBox _chkShowTaxNumber = null!;
    private CheckBox _chkShowBankDetails = null!;
    private CheckBox _chkShowPaymentTerms = null!;
    
    // Print Settings Controls
    private ComboBox _comboPaperSize = null!;
    private CheckBox _chkPrintColor = null!;
    private CheckBox _chkPrintDuplicate = null!;
    
    // Action Buttons
    private Button _btnSave = null!;
    private Button _btnCancel = null!;
    private Button _btnPreview = null!;
    
    public InvoiceSettingsForm(ISettingService settingService, int currentUserId)
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
        this.Text = "إعدادات الفواتير";
        this.Size = new Size(1100, 750);
        this.StartPosition = FormStartPosition.CenterScreen;
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
            Text = "📄 إعدادات الفواتير",
            Font = new Font("Cairo", 20F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            Size = new Size(450, 80),
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
        
        // Tab Control
        _tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            RightToLeftLayout = false,
            RightToLeft = RightToLeft.Yes,
            ItemSize = new Size(200, 45),
            Padding = new Point(20, 5)
        };
        
        // Create Tabs
        CreateTaxSettingsTab();
        CreateNumberingTab();
        CreateTextsTab();
        CreateDisplayTab();
        CreatePrintTab();
        
        this.Controls.Add(_tabControl);
        
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
        
        _btnPreview = new Button
        {
            Text = "👁️ معاينة",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Size = new Size(130, 50),
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Margin = new Padding(10, 0, 0, 0)
        };
        _btnPreview.FlatAppearance.BorderSize = 0;
        _btnPreview.Click += BtnPreview_Click;
        buttonContainer.Controls.Add(_btnPreview);
        
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
    
    // ========== Tab 1: Tax Settings ==========
    private void CreateTaxSettingsTab()
    {
        TabPage tabPage = new TabPage("⚖️ الضرائب");
        Panel panel = CreateTabPanel();
        
        int y = 30;
        
        // Enable Tax
        Panel taxEnablePanel = CreateCard(20, y, 980, 70);
        _chkEnableTax = new CheckBox
        {
            Text = "✓ تفعيل الضريبة على الفواتير",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            Location = new Point(20, 20),
            Size = new Size(400, 30),
            Checked = true,
            ForeColor = ColorScheme.Primary
        };
        taxEnablePanel.Controls.Add(_chkEnableTax);
        panel.Controls.Add(taxEnablePanel);
        y += 90;
        
        // Tax Rate
        Panel taxRatePanel = CreateCard(20, y, 480, 120);
        Label lblTaxRate = new Label
        {
            Text = "نسبة الضريبة الافتراضية (%)",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.FromArgb(73, 80, 87)
        };
        taxRatePanel.Controls.Add(lblTaxRate);
        
        _numTaxRate = new NumericUpDown
        {
            Font = new Font("Cairo", 12F),
            Location = new Point(20, 50),
            Size = new Size(200, 40),
            Minimum = 0,
            Maximum = 100,
            DecimalPlaces = 2,
            Value = 14.0m,
            TextAlign = HorizontalAlignment.Center
        };
        taxRatePanel.Controls.Add(_numTaxRate);
        panel.Controls.Add(taxRatePanel);
        
        // Tax Label
        Panel taxLabelPanel = CreateCard(520, y, 480, 120);
        Label lblTaxLabel = new Label
        {
            Text = "نص الضريبة",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.FromArgb(73, 80, 87)
        };
        taxLabelPanel.Controls.Add(lblTaxLabel);
        
        _txtTaxLabel = new TextBox
        {
            Font = new Font("Cairo", 11F),
            Location = new Point(20, 50),
            Size = new Size(420, 40),
            Text = "ضريبة القيمة المضافة",
            RightToLeft = RightToLeft.Yes
        };
        taxLabelPanel.Controls.Add(_txtTaxLabel);
        panel.Controls.Add(taxLabelPanel);
        
        tabPage.Controls.Add(panel);
        _tabControl.TabPages.Add(tabPage);
    }
    
    // ========== Tab 2: Auto Numbering ==========
    private void CreateNumberingTab()
    {
        TabPage tabPage = new TabPage("🔢 الترقيم");
        Panel panel = CreateTabPanel();
        
        int y = 30;
        
        // Auto Numbering
        Panel autoNumPanel = CreateCard(20, y, 980, 70);
        _chkAutoNumbering = new CheckBox
        {
            Text = "✓ تفعيل الترقيم التلقائي للفواتير",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            Location = new Point(20, 20),
            Size = new Size(400, 30),
            Checked = true,
            ForeColor = ColorScheme.Primary
        };
        autoNumPanel.Controls.Add(_chkAutoNumbering);
        panel.Controls.Add(autoNumPanel);
        y += 90;
        
        // Sales Prefix & Next Number
        Panel salesPanel = CreateCard(20, y, 480, 220);
        AddSectionLabel(salesPanel, "📊 فواتير المبيعات", 20, 15);
        
        AddFieldLabel(salesPanel, "البادئة", 20, 60);
        _txtSalesPrefix = new TextBox
        {
            Font = new Font("Cairo", 11F),
            Location = new Point(20, 90),
            Size = new Size(420, 40),
            Text = "SI",
            RightToLeft = RightToLeft.Yes
        };
        salesPanel.Controls.Add(_txtSalesPrefix);
        
        AddFieldLabel(salesPanel, "الرقم التالي", 20, 140);
        _numNextSalesNumber = new NumericUpDown
        {
            Font = new Font("Cairo", 11F),
            Location = new Point(20, 170),
            Size = new Size(200, 40),
            Minimum = 1,
            Maximum = 999999,
            Value = 1,
            TextAlign = HorizontalAlignment.Center
        };
        salesPanel.Controls.Add(_numNextSalesNumber);
        
        panel.Controls.Add(salesPanel);
        
        // Purchase Prefix & Next Number
        Panel purchasePanel = CreateCard(520, y, 480, 220);
        AddSectionLabel(purchasePanel, "📋 فواتير المشتريات", 20, 15);
        
        AddFieldLabel(purchasePanel, "البادئة", 20, 60);
        _txtPurchasePrefix = new TextBox
        {
            Font = new Font("Cairo", 11F),
            Location = new Point(20, 90),
            Size = new Size(420, 40),
            Text = "PI",
            RightToLeft = RightToLeft.Yes
        };
        purchasePanel.Controls.Add(_txtPurchasePrefix);
        
        AddFieldLabel(purchasePanel, "الرقم التالي", 20, 140);
        _numNextPurchaseNumber = new NumericUpDown
        {
            Font = new Font("Cairo", 11F),
            Location = new Point(20, 170),
            Size = new Size(200, 40),
            Minimum = 1,
            Maximum = 999999,
            Value = 1,
            TextAlign = HorizontalAlignment.Center
        };
        purchasePanel.Controls.Add(_numNextPurchaseNumber);
        
        panel.Controls.Add(purchasePanel);
        y += 240;
        
        // Number Length
        Panel lengthPanel = CreateCard(20, y, 480, 120);
        AddFieldLabel(lengthPanel, "طول رقم الفاتورة", 20, 15);
        _numNumberLength = new NumericUpDown
        {
            Font = new Font("Cairo", 11F),
            Location = new Point(20, 50),
            Size = new Size(200, 40),
            Minimum = 1,
            Maximum = 10,
            Value = 6,
            TextAlign = HorizontalAlignment.Center
        };
        lengthPanel.Controls.Add(_numNumberLength);
        
        Label lblExample = new Label
        {
            Text = "مثال: SI-000001",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            Location = new Point(250, 55),
            AutoSize = true
        };
        lengthPanel.Controls.Add(lblExample);
        
        panel.Controls.Add(lengthPanel);
        
        tabPage.Controls.Add(panel);
        _tabControl.TabPages.Add(tabPage);
    }
    
    // ========== Tab 3: Invoice Texts ==========
    private void CreateTextsTab()
    {
        TabPage tabPage = new TabPage("📝 النصوص");
        Panel panel = CreateTabPanel();
        
        int y = 30;
        
        // Footer Text
        Panel footerPanel = CreateCard(20, y, 980, 140);
        AddFieldLabel(footerPanel, "نص تذييل الفاتورة", 20, 15);
        _txtFooterText = new TextBox
        {
            Font = new Font("Cairo", 10F),
            Location = new Point(20, 50),
            Size = new Size(920, 70),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            RightToLeft = RightToLeft.Yes
        };
        footerPanel.Controls.Add(_txtFooterText);
        panel.Controls.Add(footerPanel);
        y += 160;
        
        // Payment Terms
        Panel paymentPanel = CreateCard(20, y, 980, 140);
        AddFieldLabel(paymentPanel, "شروط الدفع", 20, 15);
        _txtPaymentTerms = new TextBox
        {
            Font = new Font("Cairo", 10F),
            Location = new Point(20, 50),
            Size = new Size(920, 70),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            RightToLeft = RightToLeft.Yes
        };
        paymentPanel.Controls.Add(_txtPaymentTerms);
        panel.Controls.Add(paymentPanel);
        y += 160;
        
        // Bank Details
        Panel bankPanel = CreateCard(20, y, 480, 140);
        AddFieldLabel(bankPanel, "بيانات البنك", 20, 15);
        _txtBankDetails = new TextBox
        {
            Font = new Font("Cairo", 10F),
            Location = new Point(20, 50),
            Size = new Size(420, 70),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            RightToLeft = RightToLeft.Yes
        };
        bankPanel.Controls.Add(_txtBankDetails);
        panel.Controls.Add(bankPanel);
        
        // Notes Template
        Panel notesPanel = CreateCard(520, y, 480, 140);
        AddFieldLabel(notesPanel, "قالب الملاحظات", 20, 15);
        _txtNotesTemplate = new TextBox
        {
            Font = new Font("Cairo", 10F),
            Location = new Point(20, 50),
            Size = new Size(420, 70),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            RightToLeft = RightToLeft.Yes
        };
        notesPanel.Controls.Add(_txtNotesTemplate);
        panel.Controls.Add(notesPanel);
        
        tabPage.Controls.Add(panel);
        _tabControl.TabPages.Add(tabPage);
    }
    
    // ========== Tab 4: Display Settings ==========
    private void CreateDisplayTab()
    {
        TabPage tabPage = new TabPage("👁️ العرض");
        Panel panel = CreateTabPanel();
        
        int y = 30;
        
        Panel displayPanel = CreateCard(20, y, 980, 250);
        AddSectionLabel(displayPanel, "عناصر الفاتورة المعروضة", 20, 15);
        
        int checkY = 60;
        _chkShowLogo = CreateStyledCheckBox("إظهار شعار الشركة", 40, checkY, true);
        displayPanel.Controls.Add(_chkShowLogo);
        checkY += 50;
        
        _chkShowTaxNumber = CreateStyledCheckBox("إظهار الرقم الضريبي", 40, checkY, true);
        displayPanel.Controls.Add(_chkShowTaxNumber);
        checkY += 50;
        
        _chkShowBankDetails = CreateStyledCheckBox("إظهار بيانات البنك", 40, checkY, true);
        displayPanel.Controls.Add(_chkShowBankDetails);
        checkY += 50;
        
        _chkShowPaymentTerms = CreateStyledCheckBox("إظهار شروط الدفع", 40, checkY, true);
        displayPanel.Controls.Add(_chkShowPaymentTerms);
        
        panel.Controls.Add(displayPanel);
        
        tabPage.Controls.Add(panel);
        _tabControl.TabPages.Add(tabPage);
    }
    
    // ========== Tab 5: Print Settings ==========
    private void CreatePrintTab()
    {
        TabPage tabPage = new TabPage("🖨️ الطباعة");
        Panel panel = CreateTabPanel();
        
        int y = 30;
        
        // Paper Size
        Panel paperPanel = CreateCard(20, y, 480, 120);
        AddFieldLabel(paperPanel, "حجم الورق", 20, 15);
        _comboPaperSize = new ComboBox
        {
            Font = new Font("Cairo", 11F),
            Location = new Point(20, 50),
            Size = new Size(200, 40),
            DropDownStyle = ComboBoxStyle.DropDownList,
            RightToLeft = RightToLeft.Yes
        };
        _comboPaperSize.Items.AddRange(new object[] { "A4", "A5", "Letter" });
        _comboPaperSize.SelectedIndex = 0;
        paperPanel.Controls.Add(_comboPaperSize);
        panel.Controls.Add(paperPanel);
        y += 140;
        
        // Print Options
        Panel optionsPanel = CreateCard(20, y, 980, 150);
        AddSectionLabel(optionsPanel, "خيارات الطباعة", 20, 15);
        
        int checkY = 60;
        _chkPrintColor = CreateStyledCheckBox("طباعة ملونة", 40, checkY, true);
        optionsPanel.Controls.Add(_chkPrintColor);
        checkY += 50;
        
        _chkPrintDuplicate = CreateStyledCheckBox("طباعة نسخة مكررة", 40, checkY, false);
        optionsPanel.Controls.Add(_chkPrintDuplicate);
        
        panel.Controls.Add(optionsPanel);
        
        tabPage.Controls.Add(panel);
        _tabControl.TabPages.Add(tabPage);
    }
    
    // ========== Helper Methods ==========
    private Panel CreateTabPanel()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(20),
            BackColor = ColorScheme.Background
        };
    }
    
    private Panel CreateCard(int x, int y, int width, int height)
    {
        return new Panel
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackColor = Color.White,
            Padding = new Padding(10)
        };
    }
    
    private void AddSectionLabel(Panel panel, string text, int x, int y)
    {
        Label label = new Label
        {
            Text = text,
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            Location = new Point(x, y),
            AutoSize = true
        };
        panel.Controls.Add(label);
    }
    
    private void AddFieldLabel(Panel panel, string text, int x, int y)
    {
        Label label = new Label
        {
            Text = text,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(73, 80, 87),
            Location = new Point(x, y),
            AutoSize = true
        };
        panel.Controls.Add(label);
    }
    
    private CheckBox CreateStyledCheckBox(string text, int x, int y, bool checked_ = false)
    {
        return new CheckBox
        {
            Text = text,
            Font = new Font("Cairo", 11F),
            Location = new Point(x, y),
            Size = new Size(400, 35),
            Checked = checked_,
            RightToLeft = RightToLeft.Yes
        };
    }
    
    // ========== Load & Save Methods ==========
    private async Task LoadSettingsAsync()
    {
        try
        {
            _currentSettings = await _settingService.GetInvoiceSettingsAsync();
            
            if (_currentSettings != null)
            {
                // Tax Settings
                _chkEnableTax.Checked = _currentSettings.EnableTax;
                _numTaxRate.Value = _currentSettings.DefaultTaxRate;
                _txtTaxLabel.Text = _currentSettings.TaxLabel ?? "ضريبة القيمة المضافة";
                
                // Numbering Settings
                _chkAutoNumbering.Checked = _currentSettings.AutoNumbering;
                _txtSalesPrefix.Text = _currentSettings.SalesInvoicePrefix ?? "SI";
                _txtPurchasePrefix.Text = _currentSettings.PurchaseInvoicePrefix ?? "PI";
                _numNextSalesNumber.Value = _currentSettings.NextSalesInvoiceNumber;
                _numNextPurchaseNumber.Value = _currentSettings.NextPurchaseInvoiceNumber;
                _numNumberLength.Value = _currentSettings.InvoiceNumberLength;
                
                // Invoice Texts
                _txtFooterText.Text = _currentSettings.InvoiceFooterText ?? "";
                _txtPaymentTerms.Text = _currentSettings.PaymentTerms ?? "";
                _txtBankDetails.Text = _currentSettings.BankDetails ?? "";
                _txtNotesTemplate.Text = _currentSettings.NotesTemplate ?? "";
                
                // Display Settings
                _chkShowLogo.Checked = _currentSettings.ShowCompanyLogo;
                _chkShowTaxNumber.Checked = _currentSettings.ShowTaxNumber;
                _chkShowBankDetails.Checked = _currentSettings.ShowBankDetails;
                _chkShowPaymentTerms.Checked = _currentSettings.ShowPaymentTerms;
                
                // Print Settings
                _comboPaperSize.SelectedItem = _currentSettings.PaperSize ?? "A4";
                _chkPrintColor.Checked = _currentSettings.PrintInColor;
                _chkPrintDuplicate.Checked = _currentSettings.PrintDuplicateCopy;
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
    
    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            var settings = _currentSettings ?? new InvoiceSetting();
            
            // Tax Settings
            settings.EnableTax = _chkEnableTax.Checked;
            settings.DefaultTaxRate = _numTaxRate.Value;
            settings.TaxLabel = _txtTaxLabel.Text.Trim();
            
            // Numbering Settings
            settings.AutoNumbering = _chkAutoNumbering.Checked;
            settings.SalesInvoicePrefix = _txtSalesPrefix.Text.Trim();
            settings.PurchaseInvoicePrefix = _txtPurchasePrefix.Text.Trim();
            settings.NextSalesInvoiceNumber = (int)_numNextSalesNumber.Value;
            settings.NextPurchaseInvoiceNumber = (int)_numNextPurchaseNumber.Value;
            settings.InvoiceNumberLength = (int)_numNumberLength.Value;
            
            // Invoice Texts
            settings.InvoiceFooterText = _txtFooterText.Text.Trim();
            settings.PaymentTerms = _txtPaymentTerms.Text.Trim();
            settings.BankDetails = _txtBankDetails.Text.Trim();
            settings.NotesTemplate = _txtNotesTemplate.Text.Trim();
            
            // Display Settings
            settings.ShowCompanyLogo = _chkShowLogo.Checked;
            settings.ShowTaxNumber = _chkShowTaxNumber.Checked;
            settings.ShowBankDetails = _chkShowBankDetails.Checked;
            settings.ShowPaymentTerms = _chkShowPaymentTerms.Checked;
            
            // Print Settings
            settings.PaperSize = _comboPaperSize.SelectedItem?.ToString() ?? "A4";
            settings.PrintInColor = _chkPrintColor.Checked;
            settings.PrintDuplicateCopy = _chkPrintDuplicate.Checked;
            
            settings.LastModifiedByUserId = _currentUserId;
            settings.LastModifiedDate = DateTime.Now;
            
            var success = await _settingService.SaveInvoiceSettingsAsync(settings);
            
            if (success)
            {
                MessageBox.Show("تم حفظ إعدادات الفواتير بنجاح ✓", "نجح",
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
    
    private void BtnPreview_Click(object? sender, EventArgs e)
    {
        MessageBox.Show("معاينة الفاتورة - قيد التطوير", "معاينة",
            MessageBoxButtons.OK, MessageBoxIcon.Information,
            MessageBoxDefaultButton.Button1,
            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
    }
}
