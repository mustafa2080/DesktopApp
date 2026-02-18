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
    private NumericUpDown _numBusesCount = null!;
    private NumericUpDown _numBusPriceSAR = null!;
    private NumericUpDown _numGiftsPrice = null!;
    private NumericUpDown _numOtherExpenses = null!;
    private TextBox _txtOtherExpensesNotes = null!;
    private NumericUpDown _numProfitMarginEGP = null!; // ✅ هامش الربح بالجنيه
    
    private NumericUpDown _numSupervisorBarcodePrice = null!;
    private Label _lblSupervisorBarcodeNote = null!;
    private TextBox _txtBrokerName = null!;
    private TextBox _txtSupervisorName = null!;
    private NumericUpDown _numCommission = null!;
    private NumericUpDown _numSupervisorExpensesSAR = null!; // ✅ تغيير لـ SAR
    
    // Calculated Fields (Read-only)
    private Label _lblVisaEGP = null!;
    private Label _lblFastTrainEGP = null!;
    private Label _lblBusEGP = null!;
    private Label _lblSupervisorEGP = null!;
    private Label _lblTotalCosts = null!;
    private Label _lblTotalRevenue = null!;
    private Label _lblNetProfit = null!;
    private Label _lblProfitMargin = null!;
    private Label _lblNetProfitPerPerson = null!;
    
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
        this.Size = new Size(1150, 850);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 11F);
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
        _txtPackageNumber = AddTextBox(mainPanel, 200, yPosition, 280);
        _txtPackageNumber.ReadOnly = true;
        _txtPackageNumber.BackColor = Color.FromArgb(245, 245, 245);
        
        AddLabel(mainPanel, "التاريخ:", 540, yPosition);
        _dtpDate = AddDatePicker(mainPanel, 650, yPosition);
        yPosition += 60;
        
        // Row 2: Trip Name
        AddLabel(mainPanel, "اسم الرحلة:", 30, yPosition);
        _txtPilgrimName = AddTextBox(mainPanel, 200, yPosition, 820);
        yPosition += 60;
        
        // Row 3: Number of Persons + Room Type
        AddLabel(mainPanel, "عدد الأفراد:", 30, yPosition);
        _numPersons = AddNumericUpDown(mainPanel, 200, yPosition, 1, 50);
        _numPersons.ValueChanged += NumPersons_ValueChanged;
        
        AddLabel(mainPanel, "نوع الغرفة:", 460, yPosition);
        _cmbRoomType = AddComboBox(mainPanel, 590, yPosition, 430);
        _cmbRoomType.Items.AddRange(new object[] { "مفردة", "ثنائي", "ثلاثي", "رباعي", "خماسي" });
        _cmbRoomType.SelectedIndex = 1; // Default to "ثنائي"
        yPosition += 60;
        
        // Row 4: Pilgrims List
        AddLabel(mainPanel, "أسماء المعتمرين:", 30, yPosition);
        yPosition += 35;
        
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
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(8),
                WrapMode = DataGridViewTriState.False
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Cairo", 10F),
                BackColor = Color.White,
                SelectionBackColor = ColorScheme.Accent,
                SelectionForeColor = Color.White,
                Padding = new Padding(5),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            },
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            ColumnHeadersHeight = 45,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            RowTemplate = new DataGridViewRow { Height = 42 },
            EnableHeadersVisualStyles = false
        };
        
        // ✅ إضافة معالج أخطاء للـ DataGridView لمنع أخطاء الترتيب
        _dgvPilgrims.DataError += (s, e) =>
        {
            // منع ظهور رسالة الخطأ
            e.ThrowException = false;
        };
        
        // ✅ إضافة معالج للترتيب لتحويل القيم الفارغة
        _dgvPilgrims.SortCompare += (s, e) =>
        {
            var val1 = e.CellValue1?.ToString() ?? "";
            var val2 = e.CellValue2?.ToString() ?? "";
            e.SortResult = string.Compare(val1, val2);
            e.Handled = true;
        };
        
        // ✅ عمود مخفي للـ PilgrimNumber (مهم للتحديث)
        _dgvPilgrims.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "PilgrimNumber",
            HeaderText = "PilgrimNumber",
            Visible = false
        });
        
        // ✅ عمود مخفي للـ UmrahPilgrimId (مهم للتحديث)
        _dgvPilgrims.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "UmrahPilgrimId",
            HeaderText = "UmrahPilgrimId",
            Visible = false
        });
        
        // Add columns with explicit widths
        _dgvPilgrims.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Number",
            HeaderText = "م",
            Width = 50,
            ReadOnly = true,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Cairo", 10F, FontStyle.Bold)
            }
        });
        
        _dgvPilgrims.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Name",
            HeaderText = "اسم المعتمر",
            Width = 470,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleRight,
                Padding = new Padding(10, 5, 10, 5)
            }
        });
        
        // إضافة عمود نوع الغرفة
        var roomTypeColumn = new DataGridViewComboBoxColumn
        {
            Name = "RoomType",
            HeaderText = "نوع الغرفة",
            Width = 150,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter
            },
            ValueType = typeof(string),
            DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing,
            DataPropertyName = "" // منع الربط التلقائي
        };
        // استخدام نفس القيم العربية الموجودة في ComboBox الرئيسي
        roomTypeColumn.Items.Add("مفردة");
        roomTypeColumn.Items.Add("ثنائي");
        roomTypeColumn.Items.Add("ثلاثي");
        roomTypeColumn.Items.Add("رباعي");
        roomTypeColumn.Items.Add("خماسي");
        
        _dgvPilgrims.Columns.Add(roomTypeColumn);
        
        // إضافة عمود رقم الغرفة المشتركة
        _dgvPilgrims.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "SharedRoomNumber",
            HeaderText = "رقم الغرفة المشتركة",
            Width = 180,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // تعطيل الترتيب في جميع الأعمدة
        foreach (DataGridViewColumn column in _dgvPilgrims.Columns)
        {
            column.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
        
        mainPanel.Controls.Add(_dgvPilgrims);
        yPosition += 220;
        
        // ══════════════════════════════════════════════════════════════
        // SECTION 2: الإقامة
        // ══════════════════════════════════════════════════════════════
        AddSectionHeader(mainPanel, "🏨 بيانات الإقامة", ref yPosition);
        
        // Row 1: Makkah Hotel + Nights
        AddLabel(mainPanel, "فندق مكة:", 30, yPosition);
        _txtMakkahHotel = AddTextBox(mainPanel, 200, yPosition, 430);
        
        AddLabel(mainPanel, "عدد الليالي:", 680, yPosition);
        _numMakkahNights = AddNumericUpDown(mainPanel, 820, yPosition, 0, 30);
        yPosition += 60;
        
        // Row 2: Madinah Hotel + Nights
        AddLabel(mainPanel, "فندق المدينة:", 30, yPosition);
        _txtMadinahHotel = AddTextBox(mainPanel, 200, yPosition, 430);
        
        AddLabel(mainPanel, "عدد الليالي:", 680, yPosition);
        _numMadinahNights = AddNumericUpDown(mainPanel, 820, yPosition, 0, 30);
        yPosition += 80;
        
        // ══════════════════════════════════════════════════════════════
        // SECTION 3: وسيلة السفر
        // ══════════════════════════════════════════════════════════════
        AddSectionHeader(mainPanel, "✈️ وسيلة السفر", ref yPosition);
        
        AddLabel(mainPanel, "وسيلة السفر:", 30, yPosition);
        _txtTransportMethod = AddTextBox(mainPanel, 200, yPosition, 820);
        _txtTransportMethod.PlaceholderText = "مثال: طيران مباشر، طيران مع ترانزيت، باص...";
        yPosition += 80;
        
        // ══════════════════════════════════════════════════════════════
        // SECTION 4: الأسعار والتكاليف
        // ══════════════════════════════════════════════════════════════
        AddSectionHeader(mainPanel, "💰 الأسعار والتكاليف", ref yPosition);
        
        // Row 1: Selling Price (Calculated - Read Only)
        AddLabel(mainPanel, "سعر البيع (للفرد):", 30, yPosition);
        _numSellingPrice = AddNumericUpDown(mainPanel, 200, yPosition, 0, 1000000, 2);
        _numSellingPrice.ReadOnly = true;
        _numSellingPrice.BackColor = ColorScheme.LightGray;
        AddCurrencyLabel(mainPanel, "ج.م", 420, yPosition);
        yPosition += 60;
        
        // Row 2: Visa Price SAR + Exchange Rate
        AddLabel(mainPanel, "سعر التأشيرة:", 30, yPosition);
        _numVisaPriceSAR = AddNumericUpDown(mainPanel, 200, yPosition, 0, 100000, 2);
        _numVisaPriceSAR.ValueChanged += (s, e) => CalculateTotals();
        AddCurrencyLabel(mainPanel, "ر.س", 420, yPosition);
        
        AddLabel(mainPanel, "سعر الصرف:", 490, yPosition);
        _numExchangeRate = AddNumericUpDown(mainPanel, 620, yPosition, 0, 100, 4);
        _numExchangeRate.Value = 13.5m;
        _numExchangeRate.ValueChanged += (s, e) => CalculateTotals();
        
        _lblVisaEGP = AddCalculatedLabel(mainPanel, "= 0 ج.م", 840, yPosition);
        yPosition += 60;
        
        // Row 3: Accommodation + Barcode
        AddLabel(mainPanel, "إجمالي الإقامة:", 30, yPosition);
        _numAccommodationTotal = AddNumericUpDown(mainPanel, 200, yPosition, 0, 1000000, 2);
        _numAccommodationTotal.ValueChanged += (s, e) => CalculateTotals();
        AddCurrencyLabel(mainPanel, "ج.م", 420, yPosition);
        
        AddLabel(mainPanel, "سعر الباركود:", 490, yPosition);
        _numBarcodePrice = AddNumericUpDown(mainPanel, 620, yPosition, 0, 100000, 2);
        _numBarcodePrice.ValueChanged += (s, e) => CalculateTotals();
        AddCurrencyLabel(mainPanel, "ج.م", 840, yPosition);
        yPosition += 60;
        
        // Row 4: Flight + Fast Train SAR
        AddLabel(mainPanel, "سعر الطيران:", 30, yPosition);
        _numFlightPrice = AddNumericUpDown(mainPanel, 200, yPosition, 0, 1000000, 2);
        _numFlightPrice.ValueChanged += (s, e) => CalculateTotals();
        AddCurrencyLabel(mainPanel, "ج.م", 420, yPosition);
        
        AddLabel(mainPanel, "القطار السريع:", 490, yPosition);
        _numFastTrainSAR = AddNumericUpDown(mainPanel, 620, yPosition, 0, 100000, 2);
        _numFastTrainSAR.ValueChanged += (s, e) => CalculateTotals();
        AddCurrencyLabel(mainPanel, "ر.س", 840, yPosition);
        _lblFastTrainEGP = AddCalculatedLabel(mainPanel, "= 0 ج.م", 950, yPosition);
        yPosition += 60;
        
        // Row 5: Buses Count + Bus Price SAR
        AddLabel(mainPanel, "عدد الباصات:", 30, yPosition);
        _numBusesCount = AddNumericUpDown(mainPanel, 200, yPosition, 0, 50);
        _numBusesCount.ValueChanged += (s, e) => CalculateTotals();

        AddLabel(mainPanel, "سعر الباص:", 490, yPosition);
        _numBusPriceSAR = AddNumericUpDown(mainPanel, 620, yPosition, 0, 100000, 2);
        _numBusPriceSAR.ValueChanged += (s, e) => CalculateTotals();
        AddCurrencyLabel(mainPanel, "ر.س", 840, yPosition);
        _lblBusEGP = AddCalculatedLabel(mainPanel, "= 0 ج.م", 950, yPosition);
        yPosition += 60;

        // Row 6: Gifts Price (Total for all persons)
        AddLabel(mainPanel, "سعر الهدايا (إجمالي):", 30, yPosition);
        _numGiftsPrice = AddNumericUpDown(mainPanel, 200, yPosition, 0, 1000000, 2);
        _numGiftsPrice.ValueChanged += (s, e) => CalculateTotals();
        AddCurrencyLabel(mainPanel, "ج.م", 420, yPosition);
        yPosition += 60;

        // Row 7: Other Expenses + Notes
        AddLabel(mainPanel, "مصروفات أخرى:", 30, yPosition);
        _numOtherExpenses = AddNumericUpDown(mainPanel, 200, yPosition, 0, 1000000, 2);
        _numOtherExpenses.ValueChanged += (s, e) => CalculateTotals();
        AddCurrencyLabel(mainPanel, "ج.م", 420, yPosition);

        AddLabel(mainPanel, "ملاحظات:", 540, yPosition);
        _txtOtherExpensesNotes = AddTextBox(mainPanel, 660, yPosition, 360);
        _txtOtherExpensesNotes.PlaceholderText = "تفاصيل المصروفات الأخرى...";
        yPosition += 60;

        // Row 8: Profit Margin in EGP (not percentage)
        AddLabel(mainPanel, "هامش الربح:", 30, yPosition);
        _numProfitMarginEGP = AddNumericUpDown(mainPanel, 200, yPosition, 0, 1000000, 2);
        _numProfitMarginEGP.ValueChanged += (s, e) => CalculateTotals();
        AddCurrencyLabel(mainPanel, "ج.م", 420, yPosition);
        yPosition += 80;
        
        // ══════════════════════════════════════════════════════════════
        // SECTION 5: الوسيط والمشرف
        // ══════════════════════════════════════════════════════════════
        AddSectionHeader(mainPanel, "👥 الوسيط والمشرف", ref yPosition);
        
        // Row 1: Broker Name + Commission
        AddLabel(mainPanel, "اسم الوسيط:", 30, yPosition);
        _txtBrokerName = AddTextBox(mainPanel, 200, yPosition, 330);
        
        AddLabel(mainPanel, "العمولة:", 580, yPosition);
        _numCommission = AddNumericUpDown(mainPanel, 680, yPosition, 0, 100000, 2);
        _numCommission.ValueChanged += (s, e) => CalculateTotals();
        AddCurrencyLabel(mainPanel, "ج.م", 900, yPosition);
        yPosition += 60;
        
        // Row 2: Supervisor Name + Expenses (SAR)
        AddLabel(mainPanel, "اسم المشرف:", 30, yPosition);
        _txtSupervisorName = AddTextBox(mainPanel, 200, yPosition, 330);
        
        AddLabel(mainPanel, "مصاريف المشرف:", 580, yPosition);
        _numSupervisorExpensesSAR = AddNumericUpDown(mainPanel, 750, yPosition, 0, 100000, 2);
        _numSupervisorExpensesSAR.ValueChanged += (s, e) => CalculateTotals();
        AddCurrencyLabel(mainPanel, "ر.س", 970, yPosition);
        _lblSupervisorEGP = AddCalculatedLabel(mainPanel, "= 0 ج.م", 1000, yPosition);
        yPosition += 60;

        // Row 3: Supervisor Barcode Price (خاص بالمشرف فقط)
        AddLabel(mainPanel, "باركود المشرف:", 30, yPosition);
        _numSupervisorBarcodePrice = AddNumericUpDown(mainPanel, 200, yPosition, 0, 100000, 2);
        _numSupervisorBarcodePrice.ValueChanged += (s, e) => CalculateTotals();
        AddCurrencyLabel(mainPanel, "ج.م", 420, yPosition);
        _lblSupervisorBarcodeNote = AddCalculatedLabel(mainPanel, "⚠️ سعر خاص بالمشرف فقط", 490, yPosition);
        _lblSupervisorBarcodeNote.ForeColor = Color.FromArgb(183, 28, 28);
        _lblSupervisorBarcodeNote.Font = new Font("Cairo", 9F, FontStyle.Bold);
        yPosition += 80;
        
        // ══════════════════════════════════════════════════════════════
        // SECTION 6: الحسابات المالية (Read-Only)
        // ══════════════════════════════════════════════════════════════
        AddSectionHeader(mainPanel, "📊 الحسابات المالية (تلقائية)", ref yPosition);
        
        Panel calculationsPanel = new Panel
        {
            Location = new Point(30, yPosition),
            Size = new Size(990, 200),
            BackColor = Color.FromArgb(245, 250, 255),
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(20)
        };
        
        int calcY = 15;
        
        // Row 1: Total Costs + Total Revenue
        AddCalculationLabel(calculationsPanel, "إجمالي التكاليف (للفرد):", 20, calcY);
        _lblTotalCosts = AddBigCalculatedValue(calculationsPanel, "0.00 ج.م", 300, calcY, ColorScheme.Primary);
        
        AddCalculationLabel(calculationsPanel, "إجمالي الإيرادات:", 520, calcY);
        _lblTotalRevenue = AddBigCalculatedValue(calculationsPanel, "0.00 ج.م", 750, calcY, ColorScheme.Primary);
        calcY += 50;
        
        // Separator Line
        Panel separatorLine1 = new Panel
        {
            Location = new Point(20, calcY),
            Size = new Size(930, 1),
            BackColor = Color.FromArgb(200, 200, 200)
        };
        calculationsPanel.Controls.Add(separatorLine1);
        calcY += 15;
        
        // Row 2: Net Profit + Profit Margin
        AddCalculationLabel(calculationsPanel, "صافي الربح الإجمالي:", 20, calcY);
        _lblNetProfit = AddBigCalculatedValue(calculationsPanel, "0.00 ج.م", 300, calcY, ColorScheme.Success);
        
        AddCalculationLabel(calculationsPanel, "هامش الربح:", 520, calcY);
        _lblProfitMargin = AddBigCalculatedValue(calculationsPanel, "0.00 %", 750, calcY, ColorScheme.Info);
        calcY += 50;
        
        // Separator Line
        Panel separatorLine2 = new Panel
        {
            Location = new Point(20, calcY),
            Size = new Size(930, 1),
            BackColor = Color.FromArgb(200, 200, 200)
        };
        calculationsPanel.Controls.Add(separatorLine2);
        calcY += 15;
        
        // Row 3: Net Profit Per Person (Centered)
        AddCalculationLabel(calculationsPanel, "صافي الربح للفرد:", 340, calcY);
        _lblNetProfitPerPerson = AddBigCalculatedValue(calculationsPanel, "0.00 ج.م", 550, calcY, Color.FromArgb(76, 175, 80));
        _lblNetProfitPerPerson.Font = new Font("Cairo", 16F, FontStyle.Bold);
        
        mainPanel.Controls.Add(calculationsPanel);
        yPosition += 220;
        
        // ══════════════════════════════════════════════════════════════
        // SECTION 7: الحالة والملاحظات
        // ══════════════════════════════════════════════════════════════
        AddSectionHeader(mainPanel, "📝 الحالة والملاحظات", ref yPosition);
        
        // Row 1: Status + Is Active
        AddLabel(mainPanel, "الحالة:", 30, yPosition);
        _cmbStatus = AddComboBox(mainPanel, 200, yPosition, 430);
        _cmbStatus.Items.AddRange(new object[] { "مسودة", "مؤكد", "قيد التنفيذ", "مكتمل", "ملغي" });
        _cmbStatus.SelectedIndex = 0;
        
        _chkIsActive = new CheckBox
        {
            Text = "حزمة نشطة",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Location = new Point(680, yPosition + 5),
            AutoSize = true,
            Checked = true,
            ForeColor = ColorScheme.Success
        };
        mainPanel.Controls.Add(_chkIsActive);
        yPosition += 60;
        
        // Row 2: Notes
        AddLabel(mainPanel, "ملاحظات:", 30, yPosition);
        _txtNotes = new TextBox
        {
            Location = new Point(200, yPosition),
            Size = new Size(820, 100),
            Font = new Font("Cairo", 11F),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            BorderStyle = BorderStyle.FixedSingle
        };
        
        bool isFirstKeyPressNotes = false;
        
        // Auto-select all text when clicking on notes field
        _txtNotes.Enter += (s, e) => 
        {
            _txtNotes.SelectAll();
            isFirstKeyPressNotes = true;
        };
        
        _txtNotes.MouseClick += (s, e) => 
        {
            if (!_txtNotes.Focused)
            {
                _txtNotes.SelectAll();
                isFirstKeyPressNotes = true;
            }
        };
        
        _txtNotes.KeyPress += (s, e) =>
        {
            if (isFirstKeyPressNotes && !char.IsControl(e.KeyChar))
            {
                _txtNotes.Clear();
                isFirstKeyPressNotes = false;
            }
        };
        
        _txtNotes.Leave += (s, e) => isFirstKeyPressNotes = false;
        
        mainPanel.Controls.Add(_txtNotes);
        yPosition += 120;
        
        // ══════════════════════════════════════════════════════════════
        // ACTION BUTTONS
        // ══════════════════════════════════════════════════════════════
        Panel buttonsPanel = new Panel
        {
            Location = new Point(30, yPosition),
            Size = new Size(1020, 60),
            BackColor = Color.Transparent
        };
        
        _btnSave = new Button
        {
            Text = "💾 حفظ الحزمة",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            Size = new Size(180, 50),
            Location = new Point(840, 5),
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
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            Size = new Size(150, 50),
            Location = new Point(670, 5),
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
            Font = new Font("Cairo", 13F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            Location = new Point(30, yPosition),
            Size = new Size(1020, 40),
            BackColor = Color.FromArgb(235, 245, 255),
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 0, 15, 0)
        };
        parent.Controls.Add(header);
        yPosition += 55;
    }
    
    private void AddLabel(Panel parent, string text, int x, int y)
    {
        Label label = new Label
        {
            Text = text,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = ColorScheme.TextPrimary,
            Location = new Point(x, y + 8),
            AutoSize = true
        };
        parent.Controls.Add(label);
    }
    
    private TextBox AddTextBox(Panel parent, int x, int y, int width)
    {
        TextBox textBox = new TextBox
        {
            Location = new Point(x, y),
            Size = new Size(width, 35),
            Font = new Font("Cairo", 11F),
            BorderStyle = BorderStyle.FixedSingle
        };
        
        bool isFirstKeyPress = false;
        
        // Auto-select all text when entering the textbox
        textBox.Enter += (s, e) => 
        {
            textBox.SelectAll();
            isFirstKeyPress = true;
        };
        
        textBox.MouseClick += (s, e) => 
        {
            if (!textBox.Focused)
            {
                textBox.SelectAll();
                isFirstKeyPress = true;
            }
        };
        
        // عند الكتابة، إذا كانت أول ضغطة، نمسح المحتوى
        textBox.KeyPress += (s, e) =>
        {
            if (isFirstKeyPress && !char.IsControl(e.KeyChar))
            {
                textBox.Clear();
                isFirstKeyPress = false;
            }
        };
        
        textBox.Leave += (s, e) => isFirstKeyPress = false;
        
        parent.Controls.Add(textBox);
        return textBox;
    }
    
    private NumericUpDown AddNumericUpDown(Panel parent, int x, int y, decimal min, decimal max, int decimalPlaces = 0)
    {
        NumericUpDown numericUpDown = new NumericUpDown
        {
            Location = new Point(x, y),
            Size = new Size(200, 35),
            Font = new Font("Cairo", 11F),
            Minimum = min,
            Maximum = max,
            DecimalPlaces = decimalPlaces,
            ThousandsSeparator = true,
            TextAlign = HorizontalAlignment.Center
        };
        
        bool shouldClearOnNextKey = false;
        
        // عند الدخول للحقل - تحديد كل النص
        numericUpDown.Enter += (s, e) => 
        {
            numericUpDown.BeginInvoke(new Action(() =>
            {
                numericUpDown.Select(0, numericUpDown.Text.Length);
                shouldClearOnNextKey = true;
            }));
        };
        
        // عند الضغط بالماوس - تحديد كل النص
        numericUpDown.MouseUp += (s, e) => 
        {
            if (numericUpDown.Focused)
            {
                numericUpDown.Select(0, numericUpDown.Text.Length);
                shouldClearOnNextKey = true;
            }
        };
        
        // معالجة الكتابة - حذف القيمة القديمة عند الكتابة أول مرة
        numericUpDown.KeyDown += (s, e) =>
        {
            if (shouldClearOnNextKey)
            {
                if (char.IsDigit((char)e.KeyCode) || 
                    (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9) ||
                    e.KeyCode == Keys.Decimal || e.KeyCode == Keys.OemPeriod)
                {
                    // حذف القيمة القديمة
                    numericUpDown.Value = 0;
                    shouldClearOnNextKey = false;
                }
                else if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
                {
                    numericUpDown.Value = 0;
                    shouldClearOnNextKey = false;
                    e.Handled = true;
                }
            }
        };
        
        // إلغاء الـ flag عند ترك الحقل
        numericUpDown.Leave += (s, e) => 
        {
            shouldClearOnNextKey = false;
        };
        
        // إلغاء الـ flag عند استخدام الأسهم
        numericUpDown.ValueChanged += (s, e) =>
        {
            if (!numericUpDown.Focused)
            {
                shouldClearOnNextKey = false;
            }
        };
        
        parent.Controls.Add(numericUpDown);
        return numericUpDown;
    }
    
    private DateTimePicker AddDatePicker(Panel parent, int x, int y)
    {
        DateTimePicker datePicker = new DateTimePicker
        {
            Location = new Point(x, y),
            Size = new Size(370, 35),
            Font = new Font("Cairo", 11F),
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
            Size = new Size(width, 35),
            Font = new Font("Cairo", 11F),
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat
        };
        parent.Controls.Add(comboBox);
        return comboBox;
    }
    
    private void AddCurrencyLabel(Panel parent, string text, int x, int y)
    {
        Label label = new Label
        {
            Text = text,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = ColorScheme.Info,
            Location = new Point(x, y + 10),
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
            Font = new Font("Cairo", 11F, FontStyle.Bold),
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
            Font = new Font("Cairo", 14F, FontStyle.Bold),
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
    
    private string GetRoomTypeDisplay(RoomType roomType)
    {
        // استخدام نفس القيم الموجودة في ComboBox تماماً
        return roomType switch
        {
            RoomType.Single => "مفردة",
            RoomType.Double => "ثنائي",
            RoomType.Triple => "ثلاثي",
            RoomType.Quad => "رباعي",
            RoomType.Quint => "خماسي",
            RoomType.Suite => "ثنائي", // تحويل Suite إلى ثنائي لأنه غير موجود في القائمة
            _ => "ثنائي"
        };
    }
    
    private RoomType GetRoomTypeFromDisplay(string? display)
    {
        if (string.IsNullOrWhiteSpace(display))
            return RoomType.Double;
            
        // التعامل مع كل الحالات الممكنة (عربي وإنجليزي)
        return display.Trim() switch
        {
            "فردي" or "مفردة" or "Single" => RoomType.Single,
            "مزدوج" or "ثنائي" or "Double" => RoomType.Double,
            "ثلاثي" or "Triple" => RoomType.Triple,
            "رباعي" or "Quad" => RoomType.Quad,
            "خماسي" or "Quint" => RoomType.Quint,
            "جناح" or "Suite" => RoomType.Double, // تحويل Suite إلى Double
            _ => RoomType.Double
        };
    }
    
    private void UpdatePilgrimsList()
    {
        int currentCount = (int)_numPersons.Value;
        
        // Save current data including IDs
        var currentData = new List<(string pilgrimNumber, int pilgrimId, string name, string roomType, string roomNumber)>();
        for (int i = 0; i < _dgvPilgrims.Rows.Count; i++)
        {
            var pilgrimNumberCell = _dgvPilgrims.Rows[i].Cells["PilgrimNumber"].Value;
            var pilgrimIdCell = _dgvPilgrims.Rows[i].Cells["UmrahPilgrimId"].Value;
            var nameCell = _dgvPilgrims.Rows[i].Cells["Name"].Value;
            var roomTypeCell = _dgvPilgrims.Rows[i].Cells["RoomType"].Value;
            var roomNumberCell = _dgvPilgrims.Rows[i].Cells["SharedRoomNumber"].Value;
            
            currentData.Add((
                pilgrimNumberCell?.ToString() ?? "",
                pilgrimIdCell != null && int.TryParse(pilgrimIdCell.ToString(), out int id) ? id : 0,
                nameCell?.ToString() ?? "",
                roomTypeCell?.ToString() ?? "",
                roomNumberCell?.ToString() ?? ""
            ));
        }
        
        // Clear and rebuild
        _dgvPilgrims.Rows.Clear();
        
        for (int i = 0; i < currentCount; i++)
        {
            string pilgrimNumber = i < currentData.Count ? currentData[i].pilgrimNumber : "";
            int pilgrimId = i < currentData.Count ? currentData[i].pilgrimId : 0;
            string name = i < currentData.Count ? currentData[i].name : "";
            string roomType = i < currentData.Count && !string.IsNullOrWhiteSpace(currentData[i].roomType) 
                ? NormalizeRoomType(currentData[i].roomType) 
                : "ثنائي"; // القيمة الافتراضية بالعربي
            string roomNumber = i < currentData.Count ? currentData[i].roomNumber : "";
            
            int rowIndex = _dgvPilgrims.Rows.Add();
            DataGridViewRow row = _dgvPilgrims.Rows[rowIndex];
            
            // ✅ الاحتفاظ بالـ IDs
            row.Cells["PilgrimNumber"].Value = pilgrimNumber;
            row.Cells["UmrahPilgrimId"].Value = pilgrimId > 0 ? pilgrimId : (object)DBNull.Value;
            
            row.Cells["Number"].Value = (i + 1).ToString();
            row.Cells["Name"].Value = name;
            row.Cells["RoomType"].Value = roomType;
            row.Cells["SharedRoomNumber"].Value = roomNumber;
        }
    }
    
    // دالة لتوحيد قيم نوع الغرفة لتطابق القائمة المتاحة
    private string NormalizeRoomType(string roomType)
    {
        if (string.IsNullOrWhiteSpace(roomType))
            return "ثنائي";
            
        // تحويل أي قيمة إلى واحدة من القيم الخمسة المتاحة
        return roomType.Trim() switch
        {
            "فردي" or "مفردة" or "Single" => "مفردة",
            "مزدوج" or "ثنائي" or "Double" => "ثنائي",
            "ثلاثي" or "Triple" => "ثلاثي",
            "رباعي" or "Quad" => "رباعي",
            "خماسي" or "Quint" => "خماسي",
            "جناح" or "Suite" => "ثنائي", // تحويل الجناح إلى ثنائي
            _ => "ثنائي" // القيمة الافتراضية
        };
    }
    
    // ══════════════════════════════════════════════════════════════════════════
    // CALCULATIONS
    // ══════════════════════════════════════════════════════════════════════════
    
    private void CalculateTotals()
    {
        try
        {
            decimal exchangeRate = _numExchangeRate.Value;
            decimal visaSAR      = _numVisaPriceSAR.Value;
            decimal fastTrainSAR = _numFastTrainSAR.Value;
            int     busesCount   = (int)_numBusesCount.Value;
            decimal busPriceSAR  = _numBusPriceSAR.Value;
            decimal supervisorExpensesSAR = _numSupervisorExpensesSAR.Value;
            int     numberOfPersons = (int)_numPersons.Value;

            // تحويل SAR → EGP
            decimal visaEGP        = visaSAR * exchangeRate;
            decimal fastTrainEGP   = fastTrainSAR * exchangeRate;           // للفرد
            decimal busEGP         = busPriceSAR * exchangeRate * busesCount; // إجمالي الباصات
            decimal supervisorEGP  = supervisorExpensesSAR * exchangeRate;  // إجمالي مصاريف المشرف

            _lblVisaEGP.Text       = $"= {visaEGP:N2} ج.م";
            _lblFastTrainEGP.Text  = $"= {fastTrainEGP:N2} ج.م";
            _lblBusEGP.Text        = $"= {busEGP:N2} ج.م";
            _lblSupervisorEGP.Text = $"= {supervisorEGP:N2} ج.م";

            // ══ تكاليف للفرد مباشرة (لا تُضرب في عدد الأفراد) ══
            decimal perPersonCosts =
                visaEGP +                         // تأشيرة للفرد
                _numAccommodationTotal.Value +    // إقامة للفرد
                _numBarcodePrice.Value +          // باركود للفرد
                _numFlightPrice.Value +           // طيران للفرد
                fastTrainEGP +                    // قطار سريع للفرد
                _numOtherExpenses.Value +         // مصروفات أخرى للفرد
                _numCommission.Value;             // عمولة للفرد

            // ══ تكاليف مشتركة تُقسَّم على عدد الأفراد ══
            if (numberOfPersons > 0)
            {
                decimal sharedCosts =
                    busEGP +                              // الباصات (إجمالي ÷ أفراد)
                    _numGiftsPrice.Value +                // هدايا (إجمالي ÷ أفراد)
                    supervisorEGP +                       // مصاريف مشرف (إجمالي ÷ أفراد)
                    _numSupervisorBarcodePrice.Value;     // باركود مشرف (إجمالي ÷ أفراد)
                perPersonCosts += sharedCosts / numberOfPersons;
            }

            _lblTotalCosts.Text = $"{perPersonCosts:N2} ج.م";

            // سعر البيع = تكاليف الفرد + هامش الربح
            decimal profitMarginEGP = _numProfitMarginEGP.Value;
            decimal sellingPrice    = perPersonCosts + profitMarginEGP;
            _numSellingPrice.Value  = sellingPrice;

            // إجمالي الإيرادات
            decimal totalRevenue = sellingPrice * numberOfPersons;
            _lblTotalRevenue.Text = $"{totalRevenue:N2} ج.م";

            // صافي الربح الكلي
            decimal totalCostsAll = perPersonCosts * numberOfPersons;
            decimal netProfit     = totalRevenue - totalCostsAll;
            _lblNetProfit.Text    = netProfit >= 0
                ? $"{netProfit:N0} ج.م"
                : $"({Math.Abs(netProfit):N0}) خسارة";
            _lblNetProfit.ForeColor = netProfit >= 0
                ? ColorScheme.Success
                : Color.FromArgb(211, 47, 47);

            // نسبة هامش الربح
            decimal profitPct = totalRevenue > 0 ? (netProfit / totalRevenue * 100) : 0;
            _lblProfitMargin.Text = $"{profitPct:N2} %";
            _lblProfitMargin.ForeColor = profitPct >= 0
                ? ColorScheme.Info
                : Color.FromArgb(211, 47, 47);

            // صافي الربح للفرد
            decimal netProfitPerPerson = numberOfPersons > 0 ? (netProfit / numberOfPersons) : 0;
            _lblNetProfitPerPerson.Text = netProfitPerPerson >= 0
                ? $"{netProfitPerPerson:N0} ج.م"
                : $"({Math.Abs(netProfitPerPerson):N0}) خسارة";
            _lblNetProfitPerPerson.ForeColor = netProfitPerPerson >= 0
                ? Color.FromArgb(76, 175, 80)
                : Color.FromArgb(211, 47, 47);
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
            _numBusesCount.Value = package.BusesCount;
            _numBusPriceSAR.Value = package.BusPriceSAR;
            _numGiftsPrice.Value = package.GiftsPrice;
            _numOtherExpenses.Value = package.OtherExpenses;
            _txtOtherExpensesNotes.Text = package.OtherExpensesNotes ?? "";
            _numProfitMarginEGP.Value = package.ProfitMarginEGP;
            
            _txtBrokerName.Text = package.BrokerName ?? "";
            _txtSupervisorName.Text = package.SupervisorName ?? "";
            _numCommission.Value = package.Commission;
            _numSupervisorExpensesSAR.Value = package.SupervisorExpensesSAR;
            _numSupervisorBarcodePrice.Value = package.SupervisorBarcodePrice;
            
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
                    // ✅ تحويل نوع الغرفة إلى عربي - يجب أن يطابق القيم في ComboBox
                    string roomTypeDisplay = "ثنائي"; // القيمة الافتراضية
                    
                    if (pilgrim.RoomType.HasValue)
                    {
                        roomTypeDisplay = pilgrim.RoomType.Value switch
                        {
                            RoomType.Single => "مفردة",
                            RoomType.Double => "ثنائي",
                            RoomType.Triple => "ثلاثي",
                            RoomType.Quad => "رباعي",
                            RoomType.Quint => "خماسي",
                            RoomType.Suite => "ثنائي", // تحويل الجناح إلى ثنائي (غير موجود في القائمة)
                            _ => "ثنائي"
                        };
                    }
                    
                    Console.WriteLine($"🔍 Loading pilgrim: {pilgrim.FullName}, RoomType Enum: {pilgrim.RoomType}, Display: {roomTypeDisplay}");
                    
                    // إنشاء الصف مع تعيين القيمة العربية مباشرة
                    int rowIndex = _dgvPilgrims.Rows.Add();
                    DataGridViewRow row = _dgvPilgrims.Rows[rowIndex];
                    
                    // ✅ حفظ الـ IDs المخفية
                    row.Cells["PilgrimNumber"].Value = pilgrim.PilgrimNumber;
                    row.Cells["UmrahPilgrimId"].Value = pilgrim.UmrahPilgrimId;
                    
                    row.Cells["Number"].Value = index.ToString();
                    row.Cells["Name"].Value = pilgrim.FullName;
                    row.Cells["RoomType"].Value = roomTypeDisplay; // ✅ القيمة العربية
                    row.Cells["SharedRoomNumber"].Value = pilgrim.SharedRoomNumber ?? "";
                    
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
            // ✅ إجبار DataGridView على commit أي تعديلات معلقة
            if (_dgvPilgrims.CurrentCell != null)
            {
                _dgvPilgrims.CurrentCell.OwningRow.Selected = false;
                _dgvPilgrims.EndEdit();
            }
            
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
                BusesCount = (int)_numBusesCount.Value,
                BusPriceSAR = _numBusPriceSAR.Value,
                GiftsPrice = _numGiftsPrice.Value,
                OtherExpenses = _numOtherExpenses.Value,
                OtherExpensesNotes = string.IsNullOrWhiteSpace(_txtOtherExpensesNotes.Text) ? null : _txtOtherExpensesNotes.Text.Trim(),
                ProfitMarginEGP = _numProfitMarginEGP.Value,
                
                BrokerName = string.IsNullOrWhiteSpace(_txtBrokerName.Text) ? null : _txtBrokerName.Text.Trim(),
                SupervisorName = string.IsNullOrWhiteSpace(_txtSupervisorName.Text) ? null : _txtSupervisorName.Text.Trim(),
                Commission = _numCommission.Value,
                SupervisorExpensesSAR = _numSupervisorExpensesSAR.Value,
                SupervisorBarcodePrice = _numSupervisorBarcodePrice.Value,
                
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
                var pilgrimNumberCell = _dgvPilgrims.Rows[i].Cells["PilgrimNumber"].Value;
                var pilgrimIdCell = _dgvPilgrims.Rows[i].Cells["UmrahPilgrimId"].Value;
                var nameCell = _dgvPilgrims.Rows[i].Cells["Name"].Value;
                var roomTypeCell = _dgvPilgrims.Rows[i].Cells["RoomType"].Value;
                var roomNumberCell = _dgvPilgrims.Rows[i].Cells["SharedRoomNumber"].Value;
                
                string pilgrimName = nameCell?.ToString()?.Trim() ?? "";
                string roomTypeDisplay = roomTypeCell?.ToString()?.Trim() ?? "ثنائي";
                string sharedRoomNumber = roomNumberCell?.ToString()?.Trim() ?? "";
                
                if (!string.IsNullOrWhiteSpace(pilgrimName))
                {
                    // ✅ استخدام الـ PilgrimNumber الموجود أو توليد واحد جديد
                    string pilgrimNumber = pilgrimNumberCell?.ToString() ?? 
                        $"UMP-{DateTime.UtcNow.Year}-{package.PackageNumber.Split('-').Last()}-{(i + 1):D2}";
                    
                    // Convert room type display to enum (handles both Arabic and English)
                    RoomType pilgrimRoomType = GetRoomTypeFromDisplay(roomTypeDisplay);
                    
                    Console.WriteLine($"🔍 Pilgrim {i+1}: Name={pilgrimName}, DisplayType={roomTypeDisplay}, EnumType={pilgrimRoomType}, PilgrimNumber={pilgrimNumber}");
                    
                    var newPilgrim = new UmrahPilgrim
                    {
                        PilgrimNumber = pilgrimNumber,
                        FullName = pilgrimName,
                        RoomType = pilgrimRoomType,
                        SharedRoomNumber = string.IsNullOrWhiteSpace(sharedRoomNumber) ? null : sharedRoomNumber,
                        TotalAmount = _numSellingPrice.Value,
                        PaidAmount = 0,
                        CreatedBy = _currentUserId,
                        RegisteredAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Status = PilgrimStatus.Registered
                    };
                    
                    // ✅ إذا كان في ID موجود، نستخدمه (للتحديث)
                    if (pilgrimIdCell != null && int.TryParse(pilgrimIdCell.ToString(), out int existingId) && existingId > 0)
                    {
                        newPilgrim.UmrahPilgrimId = existingId;
                        Console.WriteLine($"   ✅ Using existing ID: {existingId}");
                    }
                    
                    package.Pilgrims.Add(newPilgrim);
                }
            }
            
            _btnSave.Enabled = false;
            _btnSave.Text = "جاري الحفظ...";
            
            UmrahPackage savedPackage;
            
            if (_packageId.HasValue)
            {
                package.UmrahPackageId = _packageId.Value;
                
                Console.WriteLine($"🔍 [FORM] About to call UpdatePackageAsync");
                Console.WriteLine($"   - Package ID: {package.UmrahPackageId}");
                Console.WriteLine($"   - TripName: {package.TripName}");
                Console.WriteLine($"   - NumberOfPersons: {package.NumberOfPersons}");
                Console.WriteLine($"   - Pilgrims Count: {package.Pilgrims.Count}");
                
                var success = await _umrahService.UpdatePackageAsync(package);
                
                Console.WriteLine($"🔍 [FORM] UpdatePackageAsync returned: {success}");
                
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
