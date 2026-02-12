using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Presentation;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class AddEditUmrahPackageForm : Form
{
    private readonly IUmrahService _umrahService;
    private readonly int _currentUserId;
    private readonly int? _packageId;
    
    // Basic Info Controls
    private TextBox _txtPackageNumber = null!;
    private DateTimePicker _dtpDate = null!;
    private TextBox _txtPilgrimName = null!;
    private NumericUpDown _numPersons = null!;
    private ComboBox _cmbRoomType = null!;
    
    // Pilgrims List
    private DataGridView _dgvPilgrims = null!;
    private List<string> _pilgrimNames = new List<string>();
    
    // Hotels
    private TextBox _txtMakkahHotel = null!;
    private NumericUpDown _numMakkahNights = null!;
    private TextBox _txtMadinahHotel = null!;
    private NumericUpDown _numMadinahNights = null!;
    
    // Transport
    private TextBox _txtTransportMethod = null!;
    
    // Pricing
    private NumericUpDown _numSellingPrice = null!;
    private NumericUpDown _numVisaPriceSAR = null!;
    private NumericUpDown _numExchangeRate = null!;
    private NumericUpDown _numAccommodationTotal = null!;
    private NumericUpDown _numBarcodePrice = null!;
    private NumericUpDown _numFlightPrice = null!;
    private NumericUpDown _numFastTrainSAR = null!;
    
    // Broker & Supervisor
    private TextBox _txtBrokerName = null!;
    private TextBox _txtSupervisorName = null!;
    private NumericUpDown _numCommission = null!;
    private NumericUpDown _numSupervisorExpenses = null!;
    
    // Calculated Fields (Read-only)
    private Label _lblVisaEGP = null!;
    private Label _lblFastTrainEGP = null!;
    private Label _lblTotalCosts = null!;
    private Label _lblTotalRevenue = null!;
    private Label _lblNetProfit = null!;
    private Label _lblProfitMargin = null!;
    
    // Status & Notes
    private ComboBox _cmbStatus = null!;
    private CheckBox _chkIsActive = null!;
    private TextBox _txtNotes = null!;
    
    // Buttons
    private Button _btnSave = null!;
    private Button _btnCancel = null!;
    
    public AddEditUmrahPackageForm(IUmrahService umrahService, int currentUserId, int? packageId = null)
    {
        _umrahService = umrahService;
        _currentUserId = currentUserId;
        _packageId = packageId;
        
        InitializeComponent();
        SetupForm();
        InitializeControls();
        
        if (_packageId.HasValue)
        {
            _ = LoadPackageDataAsync();
        }
        else
        {
            _ = GeneratePackageNumberAsync();
        }
    }
    
    private void SetupForm()
    {
        this.Text = _packageId.HasValue ? "تعديل حزمة العمرة" : "إضافة حزمة عمرة جديدة";
        this.Size = new Size(1100, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
    }
    
    private void InitializeControls()
    {
        // Main Container with Scroll
        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = ColorScheme.Background,
            Padding = new Padding(20)
        };
        
        int yPosition = 20;
        
        // ══════════════════════════════════════════════════════════════
        // SECTION 1: معلومات أساسية
        // ══════════════════════════════════════════════════════════════
        AddSectionHeader(mainPanel, "📋 المعلومات الأساسية", ref yPosition);
        
        // Row 1: Package Number + Date
        AddLabel(mainPanel, "رقم الحزمة:", 30, yPosition);
        _txtPackageNumber = AddTextBox(mainPanel, 200, yPosition, 250);
        _txtPackageNumber.ReadOnly = true;
        _txtPackageNumber.BackColor = Color.WhiteSmoke;
        
        AddLabel(mainPanel, "التاريخ:", 530, yPosition);
        _dtpDate = AddDatePicker(mainPanel, 650, yPosition);
        yPosition += 50;
        
        // Row 2: Trip Name
        AddLabel(mainPanel, "اسم الرحلة:", 30, yPosition);
        _txtPilgrimName = AddTextBox(mainPanel, 200, yPosition, 820);
        yPosition += 50;
        
        // Row 3: Number of Persons + Room Type
        AddLabel(mainPanel, "عدد الأفراد:", 30, yPosition);
        _numPersons = AddNumericUpDown(mainPanel, 200, yPosition, 1, 50);
        _numPersons.ValueChanged += NumPersons_ValueChanged;
        
        AddLabel(mainPanel, "نوع الغرفة:", 530, yPosition);
        _cmbRoomType = AddComboBox(mainPanel, 650, yPosition, 370);
        _cmbRoomType.Items.AddRange(new object[] { "مفردة", "ثنائي", "ثلاثي", "رباعي", "خماسي" });
        _cmbRoomType.SelectedIndex = 1;
        yPosition += 50;
        
        // Row 4: Pilgrims List
        AddLabel(mainPanel, "أسماء المعتمرين:", 30, yPosition);
        yPosition += 30;
        
        _dgvPilgrims = new DataGridView
        {
            Location = new Point(30, yPosition),
            Size = new Size(990, 200),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = ColorScheme.Primary,
                ForeColor = Color.White,
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Cairo", 9.5F),
                BackColor = Color.White,
                SelectionBackColor = ColorScheme.Accent,
                SelectionForeColor = Color.White
            },
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        
        // Add columns
        _dgvPilgrims.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Number",
            HeaderText = "رقم",
            Width = 60,
            ReadOnly = true
        });
        
        _dgvPilgrims.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Name",
            HeaderText = "اسم المعتمر",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        
        mainPanel.Controls.Add(_dgvPilgrims);
        yPosition += 220;
        
        // ══════════════════════════════════════════════════════════════
        // SECTION 2: الإقامة
        // ══════════════════════════════════════════════════════════════
        AddSectionHeader(mainPanel, "🏨 بيانات الإقامة", ref yPosition);
        
        // Row 1: Makkah Hotel + Nights
        AddLabel(mainPanel, "فندق مكة:", 30, yPosition);
        _txtMakkahHotel = AddTextBox(mainPanel, 200, yPosition, 370);
        
        AddLabel(mainPanel, "عدد الليالي:", 600, yPosition);
        _numMakkahNights = AddNumericUpDown(mainPanel, 720, yPosition, 0, 30);
        yPosition += 50;
        
        // Row 2: Madinah Hotel + Nights
        AddLabel(mainPanel, "فندق المدينة:", 30, yPosition);
        _txtMadinahHotel = AddTextBox(mainPanel, 200, yPosition, 370);
        
        AddLabel(mainPanel, "عدد الليالي:", 600, yPosition);
        _numMadinahNights = AddNumericUpDown(mainPanel, 720, yPosition, 0, 30);
        yPosition += 70;
        
        // ══════════════════════════════════════════════════════════════
        // SECTION 3: وسيلة السفر
        // ══════════════════════════════════════════════════════════════
        AddSectionHeader(mainPanel, "✈️ وسيلة السفر", ref yPosition);
        
        AddLabel(mainPanel, "وسيلة السفر:", 30, yPosition);
        _txtTransportMethod = AddTextBox(mainPanel, 200, yPosition, 820);
        _txtTransportMethod.PlaceholderText = "مثال: طيران مباشر، طيران مع ترانزيت، باص...";
        yPosition += 70;
        
        // ══════════════════════════════════════════════════════════════
        // SECTION 4: الأسعار والتكاليف
        // ══════════════════════════════════════════════════════════════
        AddSectionHeader(mainPanel, "💰 الأسعار والتكاليف", ref yPosition);
        
        // Row 1: Selling Price
        AddLabel(mainPanel, "سعر البيع (للفرد):", 30, yPosition);
        _numSellingPrice = AddNumericUpDown(mainPanel, 200, yPosition, 0, 1000000, 2);
        _numSellingPrice.ValueChanged += (s, e) => CalculateTotals();
        AddCurrencyLabel(mainPanel, "ج.م", 480, yPosition);
        yPosition += 50;
        
        // Row 2: Visa Price SAR + Exchange Rate
        AddLabel(mainPanel, "سعر التأشيرة:", 30, yPosition);
        _numVisaPriceSAR = AddNumericUpDown(mainPanel, 200, yPosition, 0, 100000, 2);
        _numVisaPriceSAR.ValueChanged += (s, e) => CalculateTotals();
        AddCurrencyLabel(mainPanel, "ر.س", 480, yPosition);
        
        AddLabel(mainPanel, "سعر الصرف:", 530, yPosition);
        _numExchangeRate = AddNumericUpDown(mainPanel, 650, yPosition, 0, 100, 4);
        _numExchangeRate.Value = 13.5m;
        _numExchangeRate.ValueChanged += (s, e) => CalculateTotals();
        
        _lblVisaEGP = AddCalculatedLabel(mainPanel, "= 0 ج.م", 930, yPosition);
        yPosition += 50;
        
        // Row 3: Accommodation + Barcode
        AddLabel(mainPanel, "إجمالي الإقامة:", 30, yPosition);
        _numAccommodationTotal = AddNumericUpDown(mainPanel, 200, yPosition, 0, 1000000, 2);
        _numAccommodationTotal.ValueChanged += (s, e) => CalculateTotals();
        AddCurrencyLabel(mainPanel, "ج.م", 480, yPosition);
        
        AddLabel(mainPanel, "سعر الباركود:", 530, yPosition);
        _numBarcodePrice = AddNumericUpDown(mainPanel, 650, yPosition, 0, 100000, 2);
        _numBarcodePrice.ValueChanged += (s, e) => CalculateTotals();
        AddCurrencyLabel(mainPanel, "ج.م", 930, yPosition);
        yPosition += 50;
        
        // Row 4: Flight + Fast Train SAR
        AddLabel(mainPanel, "سعر الطيران:", 30, yPosition);
        _numFlightPrice = AddNumericUpDown(mainPanel, 200, yPosition, 0, 1000000, 2);
        _numFlightPrice.ValueChanged += (s, e) => CalculateTotals();
        AddCurrencyLabel(mainPanel, "ج.م", 480, yPosition);
        
        AddLabel(mainPanel, "القطار السريع:", 530, yPosition);
        _numFastTrainSAR = AddNumericUpDown(mainPanel, 650, yPosition, 0, 100000, 2);
        _numFastTrainSAR.ValueChanged += (s, e) => CalculateTotals();
        AddCurrencyLabel(mainPanel, "ر.س", 930, yPosition);
        _lblFastTrainEGP = AddCalculatedLabel(mainPanel, "= 0 ج.م", 1000, yPosition);
        yPosition += 70;
        
        // ══════════════════════════════════════════════════════════════
        // SECTION 5: الوسيط والمشرف
        // ══════════════════════════════════════════════════════════════
        AddSectionHeader(mainPanel, "👥 الوسيط والمشرف", ref yPosition);
        
        // Row 1: Broker Name + Commission
        AddLabel(mainPanel, "اسم الوسيط:", 30, yPosition);
        _txtBrokerName = AddTextBox(mainPanel, 200, yPosition, 280);
        
        AddLabel(mainPanel, "العمولة:", 510, yPosition);
        _numCommission = AddNumericUpDown(mainPanel, 600, yPosition, 0, 100000, 2);
        _numCommission.ValueChanged += (s, e) => CalculateTotals();
        AddCurrencyLabel(mainPanel, "ج.م", 880, yPosition);
        yPosition += 50;
        
        // Row 2: Supervisor Name + Expenses
        AddLabel(mainPanel, "اسم المشرف:", 30, yPosition);
        _txtSupervisorName = AddTextBox(mainPanel, 200, yPosition, 280);
        
        AddLabel(mainPanel, "مصاريف المشرف:", 510, yPosition);
        _numSupervisorExpenses = AddNumericUpDown(mainPanel, 650, yPosition, 0, 100000, 2);
        _numSupervisorExpenses.ValueChanged += (s, e) => CalculateTotals();
        AddCurrencyLabel(mainPanel, "ج.م", 930, yPosition);
        yPosition += 70;
        
        // ══════════════════════════════════════════════════════════════
        // SECTION 6: الحسابات المالية (Read-Only)
        // ══════════════════════════════════════════════════════════════
        AddSectionHeader(mainPanel, "📊 الحسابات المالية (تلقائية)", ref yPosition);
        
        Panel calculationsPanel = new Panel
        {
            Location = new Point(30, yPosition),
            Size = new Size(1020, 140),
            BackColor = Color.FromArgb(245, 248, 250),
            BorderStyle = BorderStyle.FixedSingle
        };
        
        int calcY = 15;
        
        // Row 1: Total Costs + Total Revenue
        AddCalculationLabel(calculationsPanel, "إجمالي التكاليف (للفرد):", 20, calcY);
        _lblTotalCosts = AddBigCalculatedValue(calculationsPanel, "0.00 ج.م", 400, calcY);
        
        AddCalculationLabel(calculationsPanel, "إجمالي الإيرادات:", 520, calcY);
        _lblTotalRevenue = AddBigCalculatedValue(calculationsPanel, "0.00 ج.م", 850, calcY);
        calcY += 45;
        
        // Row 2: Net Profit + Profit Margin
        AddCalculationLabel(calculationsPanel, "صافي الربح:", 20, calcY);
        _lblNetProfit = AddBigCalculatedValue(calculationsPanel, "0.00 ج.م", 400, calcY, ColorScheme.Success);
        
        AddCalculationLabel(calculationsPanel, "هامش الربح:", 520, calcY);
        _lblProfitMargin = AddBigCalculatedValue(calculationsPanel, "0.00 %", 850, calcY, ColorScheme.Info);
        
        mainPanel.Controls.Add(calculationsPanel);
        yPosition += 160;
        
        // ══════════════════════════════════════════════════════════════
        // SECTION 7: الحالة والملاحظات
        // ══════════════════════════════════════════════════════════════
        AddSectionHeader(mainPanel, "📝 الحالة والملاحظات", ref yPosition);
        
        // Row 1: Status + Is Active
        AddLabel(mainPanel, "الحالة:", 30, yPosition);
        _cmbStatus = AddComboBox(mainPanel, 200, yPosition, 370);
        _cmbStatus.Items.AddRange(new object[] { "مسودة", "مؤكد", "قيد التنفيذ", "مكتمل", "ملغي" });
        _cmbStatus.SelectedIndex = 0;
        
        _chkIsActive = new CheckBox
        {
            Text = "حزمة نشطة",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Location = new Point(650, yPosition),
            AutoSize = true,
            Checked = true
        };
        mainPanel.Controls.Add(_chkIsActive);
        yPosition += 50;
        
        // Row 2: Notes
        AddLabel(mainPanel, "ملاحظات:", 30, yPosition);
        _txtNotes = new TextBox
        {
            Location = new Point(200, yPosition),
            Size = new Size(820, 80),
            Font = new Font("Cairo", 10F),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical
        };
        mainPanel.Controls.Add(_txtNotes);
        yPosition += 100;
        
        // ══════════════════════════════════════════════════════════════
        // ACTION BUTTONS
        // ══════════════════════════════════════════════════════════════
        Panel buttonsPanel = new Panel
        {
            Location = new Point(30, yPosition),
            Size = new Size(1020, 50),
            BackColor = Color.Transparent
        };
        
        _btnSave = new Button
        {
            Text = "💾 حفظ",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Size = new Size(150, 45),
            Location = new Point(870, 0),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnSave.FlatAppearance.BorderSize = 0;
        _btnSave.Click += BtnSave_Click;
        buttonsPanel.Controls.Add(_btnSave);
        
        _btnCancel = new Button
        {
            Text = "✖ إلغاء",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Size = new Size(150, 45),
            Location = new Point(700, 0),
            BackColor = ColorScheme.Secondary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnCancel.FlatAppearance.BorderSize = 0;
        _btnCancel.Click += (s, e) => this.Close();
        buttonsPanel.Controls.Add(_btnCancel);
        
        mainPanel.Controls.Add(buttonsPanel);
        
        this.Controls.Add(mainPanel);
        
        // Initial calculation
        CalculateTotals();
    }
    
    // ══════════════════════════════════════════════════════════════════════════
    // HELPER METHODS FOR CREATING CONTROLS
    // ══════════════════════════════════════════════════════════════════════════
    
    private void AddSectionHeader(Panel parent, string text, ref int yPosition)
    {
        Label header = new Label
        {
            Text = text,
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            Location = new Point(30, yPosition),
            Size = new Size(1020, 30),
            BackColor = Color.FromArgb(240, 245, 250),
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 0, 10, 0)
        };
        parent.Controls.Add(header);
        yPosition += 45;
    }
    
    private void AddLabel(Panel parent, string text, int x, int y)
    {
        Label label = new Label
        {
            Text = text,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Location = new Point(x, y + 5),
            AutoSize = true
        };
        parent.Controls.Add(label);
    }
    
    private TextBox AddTextBox(Panel parent, int x, int y, int width)
    {
        TextBox textBox = new TextBox
        {
            Location = new Point(x, y),
            Size = new Size(width, 30),
            Font = new Font("Cairo", 10F)
        };
        parent.Controls.Add(textBox);
        return textBox;
    }
    
    private NumericUpDown AddNumericUpDown(Panel parent, int x, int y, decimal min, decimal max, int decimalPlaces = 0)
    {
        NumericUpDown numericUpDown = new NumericUpDown
        {
            Location = new Point(x, y),
            Size = new Size(250, 30),
            Font = new Font("Cairo", 10F),
            Minimum = min,
            Maximum = max,
            DecimalPlaces = decimalPlaces,
            ThousandsSeparator = true
        };
        parent.Controls.Add(numericUpDown);
        return numericUpDown;
    }
    
    private DateTimePicker AddDatePicker(Panel parent, int x, int y)
    {
        DateTimePicker datePicker = new DateTimePicker
        {
            Location = new Point(x, y),
            Size = new Size(370, 30),
            Font = new Font("Cairo", 10F),
            Format = DateTimePickerFormat.Short
        };
        parent.Controls.Add(datePicker);
        return datePicker;
    }
    
    private ComboBox AddComboBox(Panel parent, int x, int y, int width)
    {
        ComboBox comboBox = new ComboBox
        {
            Location = new Point(x, y),
            Size = new Size(width, 30),
            Font = new Font("Cairo", 10F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        parent.Controls.Add(comboBox);
        return comboBox;
    }
    
    private void AddCurrencyLabel(Panel parent, string text, int x, int y)
    {
        Label label = new Label
        {
            Text = text,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            ForeColor = ColorScheme.TextSecondary,
            Location = new Point(x, y + 5),
            AutoSize = true
        };
        parent.Controls.Add(label);
    }
    
    private Label AddCalculatedLabel(Panel parent, string text, int x, int y)
    {
        Label label = new Label
        {
            Text = text,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            ForeColor = ColorScheme.Info,
            Location = new Point(x, y + 5),
            AutoSize = true
        };
        parent.Controls.Add(label);
        return label;
    }
    
    private void AddCalculationLabel(Panel parent, string text, int x, int y)
    {
        Label label = new Label
        {
            Text = text,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = ColorScheme.TextPrimary,
            Location = new Point(x, y),
            AutoSize = true
        };
        parent.Controls.Add(label);
    }
    
    private Label AddBigCalculatedValue(Panel parent, string text, int x, int y, Color? color = null)
    {
        Label label = new Label
        {
            Text = text,
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = color ?? ColorScheme.Primary,
            Location = new Point(x, y),
            AutoSize = true
        };
        parent.Controls.Add(label);
        return label;
    }
    
    // ══════════════════════════════════════════════════════════════════════════
    // EVENT HANDLERS
    // ══════════════════════════════════════════════════════════════════════════
    
    private void NumPersons_ValueChanged(object? sender, EventArgs e)
    {
        UpdatePilgrimsList();
        CalculateTotals();
    }
    
    private void UpdatePilgrimsList()
    {
        int currentCount = (int)_numPersons.Value;
        
        // Save current names
        var currentNames = new List<string>();
        for (int i = 0; i < _dgvPilgrims.Rows.Count; i++)
        {
            var nameCell = _dgvPilgrims.Rows[i].Cells["Name"].Value;
            currentNames.Add(nameCell?.ToString() ?? "");
        }
        
        // Clear and rebuild
        _dgvPilgrims.Rows.Clear();
        
        for (int i = 0; i < currentCount; i++)
        {
            string name = i < currentNames.Count ? currentNames[i] : "";
            _dgvPilgrims.Rows.Add((i + 1).ToString(), name);
        }
    }
    
    // ══════════════════════════════════════════════════════════════════════════
    // CALCULATIONS
    // ══════════════════════════════════════════════════════════════════════════
    
    private void CalculateTotals()
    {
        try
        {
            decimal exchangeRate = _numExchangeRate.Value;
            decimal visaSAR = _numVisaPriceSAR.Value;
            decimal fastTrainSAR = _numFastTrainSAR.Value;
            
            // Convert SAR to EGP
            decimal visaEGP = visaSAR * exchangeRate;
            decimal fastTrainEGP = fastTrainSAR * exchangeRate;
            
            _lblVisaEGP.Text = $"= {visaEGP:N2} ج.م";
            _lblFastTrainEGP.Text = $"= {fastTrainEGP:N2} ج.م";
            
            // Total Costs per person
            decimal totalCosts = visaEGP +
                                _numAccommodationTotal.Value +
                                _numBarcodePrice.Value +
                                _numFlightPrice.Value +
                                fastTrainEGP +
                                _numCommission.Value +
                                _numSupervisorExpenses.Value;
            
            _lblTotalCosts.Text = $"{totalCosts:N2} ج.م";
            
            // Total Revenue
            decimal totalRevenue = _numSellingPrice.Value * _numPersons.Value;
            _lblTotalRevenue.Text = $"{totalRevenue:N2} ج.م";
            
            // Net Profit
            decimal netProfit = totalRevenue - (totalCosts * _numPersons.Value);
            _lblNetProfit.Text = $"{netProfit:N2} ج.م";
            _lblNetProfit.ForeColor = netProfit >= 0 ? ColorScheme.Success : Color.FromArgb(211, 47, 47);
            
            // Profit Margin
            decimal profitMargin = totalRevenue > 0 ? (netProfit / totalRevenue * 100) : 0;
            _lblProfitMargin.Text = $"{profitMargin:N2} %";
            _lblProfitMargin.ForeColor = profitMargin >= 0 ? ColorScheme.Info : Color.FromArgb(211, 47, 47);
        }
        catch
        {
            // Silent fail
        }
    }
    
    // ══════════════════════════════════════════════════════════════════════════
    // GENERATE PACKAGE NUMBER
    // ══════════════════════════════════════════════════════════════════════════
    
    private async Task GeneratePackageNumberAsync()
    {
        try
        {
            var allPackages = await _umrahService.GetAllPackagesAsync();
            int nextNumber = allPackages.Count + 1;
            string year = DateTime.Now.Year.ToString();
            _txtPackageNumber.Text = $"UMR-{year}-{nextNumber:D3}";
        }
        catch
        {
            _txtPackageNumber.Text = $"UMR-{DateTime.Now.Year}-001";
        }
    }
    
    // ══════════════════════════════════════════════════════════════════════════
    // LOAD EXISTING PACKAGE DATA
    // ══════════════════════════════════════════════════════════════════════════
    
    private async Task LoadPackageDataAsync()
    {
        try
        {
            var packages = await _umrahService.GetAllPackagesAsync();
            var package = packages.FirstOrDefault(p => p.UmrahPackageId == _packageId);
            
            if (package == null)
            {
                MessageBox.Show("لم يتم العثور على الحزمة!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }
            
            // Fill form with package data
            _txtPackageNumber.Text = package.PackageNumber;
            _dtpDate.Value = package.Date;
            _txtPilgrimName.Text = package.TripName;
            _numPersons.Value = package.NumberOfPersons;
            _cmbRoomType.SelectedIndex = (int)package.RoomType;
            
            _txtMakkahHotel.Text = package.MakkahHotel;
            _numMakkahNights.Value = package.MakkahNights;
            _txtMadinahHotel.Text = package.MadinahHotel;
            _numMadinahNights.Value = package.MadinahNights;
            
            _txtTransportMethod.Text = package.TransportMethod;
            
            _numSellingPrice.Value = package.SellingPrice;
            _numVisaPriceSAR.Value = package.VisaPriceSAR;
            _numExchangeRate.Value = package.SARExchangeRate;
            _numAccommodationTotal.Value = package.AccommodationTotal;
            _numBarcodePrice.Value = package.BarcodePrice;
            _numFlightPrice.Value = package.FlightPrice;
            _numFastTrainSAR.Value = package.FastTrainPriceSAR;
            
            _txtBrokerName.Text = package.BrokerName ?? "";
            _txtSupervisorName.Text = package.SupervisorName ?? "";
            _numCommission.Value = package.Commission;
            _numSupervisorExpenses.Value = package.SupervisorExpenses;
            
            _cmbStatus.SelectedIndex = (int)package.Status - 1;
            _chkIsActive.Checked = package.IsActive;
            _txtNotes.Text = package.Notes ?? "";
            
            // Load pilgrims if available
            if (package.Pilgrims != null && package.Pilgrims.Any())
            {
                _dgvPilgrims.Rows.Clear();
                int index = 1;
                foreach (var pilgrim in package.Pilgrims.OrderBy(p => p.UmrahPilgrimId))
                {
                    _dgvPilgrims.Rows.Add(index.ToString(), pilgrim.FullName);
                    index++;
                }
            }
            else
            {
                // Initialize empty list based on NumberOfPersons
                UpdatePilgrimsList();
            }
            
            CalculateTotals();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
        }
    }
    
    // ══════════════════════════════════════════════════════════════════════════
    // SAVE PACKAGE
    // ══════════════════════════════════════════════════════════════════════════
    
    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(_txtPilgrimName.Text))
            {
                MessageBox.Show("يرجى إدخال اسم الرحلة", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtPilgrimName.Focus();
                return;
            }
            
            if (_numPersons.Value == 0)
            {
                MessageBox.Show("يرجى إدخال عدد الأفراد", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _numPersons.Focus();
                return;
            }
            
            if (string.IsNullOrWhiteSpace(_txtMakkahHotel.Text))
            {
                MessageBox.Show("يرجى إدخال فندق مكة", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtMakkahHotel.Focus();
                return;
            }
            
            if (string.IsNullOrWhiteSpace(_txtMadinahHotel.Text))
            {
                MessageBox.Show("يرجى إدخال فندق المدينة", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtMadinahHotel.Focus();
                return;
            }
            
            if (string.IsNullOrWhiteSpace(_txtTransportMethod.Text))
            {
                MessageBox.Show("يرجى إدخال وسيلة السفر", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtTransportMethod.Focus();
                return;
            }
            
            // Create or update package
            var package = new UmrahPackage
            {
                PackageNumber = _txtPackageNumber.Text,
                Date = _dtpDate.Value,
                TripName = _txtPilgrimName.Text.Trim(),
                NumberOfPersons = (int)_numPersons.Value,
                RoomType = (RoomType)_cmbRoomType.SelectedIndex,
                
                MakkahHotel = _txtMakkahHotel.Text.Trim(),
                MakkahNights = (int)_numMakkahNights.Value,
                MadinahHotel = _txtMadinahHotel.Text.Trim(),
                MadinahNights = (int)_numMadinahNights.Value,
                
                TransportMethod = _txtTransportMethod.Text.Trim(),
                
                SellingPrice = _numSellingPrice.Value,
                VisaPriceSAR = _numVisaPriceSAR.Value,
                SARExchangeRate = _numExchangeRate.Value,
                AccommodationTotal = _numAccommodationTotal.Value,
                BarcodePrice = _numBarcodePrice.Value,
                FlightPrice = _numFlightPrice.Value,
                FastTrainPriceSAR = _numFastTrainSAR.Value,
                
                BrokerName = string.IsNullOrWhiteSpace(_txtBrokerName.Text) ? null : _txtBrokerName.Text.Trim(),
                SupervisorName = string.IsNullOrWhiteSpace(_txtSupervisorName.Text) ? null : _txtSupervisorName.Text.Trim(),
                Commission = _numCommission.Value,
                SupervisorExpenses = _numSupervisorExpenses.Value,
                
                Status = (PackageStatus)(_cmbStatus.SelectedIndex + 1),
                IsActive = _chkIsActive.Checked,
                Notes = string.IsNullOrWhiteSpace(_txtNotes.Text) ? null : _txtNotes.Text.Trim(),
                
                CreatedBy = _currentUserId,
                UpdatedBy = _currentUserId,
                
                // Initialize Pilgrims collection
                Pilgrims = new List<UmrahPilgrim>()
            };
            
            // Add pilgrims from grid
            for (int i = 0; i < _dgvPilgrims.Rows.Count; i++)
            {
                var nameCell = _dgvPilgrims.Rows[i].Cells["Name"].Value;
                string pilgrimName = nameCell?.ToString()?.Trim() ?? "";
                
                if (!string.IsNullOrWhiteSpace(pilgrimName))
                {
                    // Generate pilgrim number
                    string pilgrimNumber = $"UMP-{DateTime.UtcNow.Year}-{package.PackageNumber.Split('-').Last()}-{(i + 1):D2}";
                    
                    // For updates: use existing packageId, for new: will be set by EF after save
                    int packageIdForPilgrim = _packageId ?? 0;
                    
                    package.Pilgrims.Add(new UmrahPilgrim
                    {
                        PilgrimNumber = pilgrimNumber,
                        FullName = pilgrimName,
                        // Don't set UmrahPackageId here - let EF handle it via navigation property
                        TotalAmount = _numSellingPrice.Value, // Set to selling price
                        PaidAmount = 0, // Initially unpaid
                        CreatedBy = _currentUserId,
                        RegisteredAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Status = PilgrimStatus.Registered
                    });
                }
            }
            
            _btnSave.Enabled = false;
            _btnSave.Text = "جاري الحفظ...";
            
            UmrahPackage savedPackage;
            
            if (_packageId.HasValue)
            {
                package.UmrahPackageId = _packageId.Value;
                var success = await _umrahService.UpdatePackageAsync(package);
                if (success)
                {
                    MessageBox.Show("تم تحديث الحزمة بنجاح!", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("فشل تحديث الحزمة!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _btnSave.Enabled = true;
                    _btnSave.Text = "💾 حفظ";
                }
            }
            else
            {
                savedPackage = await _umrahService.CreatePackageAsync(package);
                
                if (savedPackage != null && savedPackage.UmrahPackageId > 0)
                {
                    MessageBox.Show($"تم إضافة الحزمة بنجاح!\n\nرقم الحزمة: {savedPackage.PackageNumber}\nالمعرف: {savedPackage.UmrahPackageId}", 
                        "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("فشل حفظ الحزمة! لم يتم إنشاء المعرف.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _btnSave.Enabled = true;
                    _btnSave.Text = "💾 حفظ";
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في الحفظ: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _btnSave.Enabled = true;
            _btnSave.Text = "💾 حفظ";
        }
    }
}
