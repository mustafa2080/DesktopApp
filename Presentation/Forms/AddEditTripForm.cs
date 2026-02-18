using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace GraceWay.AccountingSystem.Presentation.Forms;

/// <summary>
/// Panel مخصص يمنع التمرير التلقائي عند التركيز على عناصر التحكم
/// </summary>
public class NoScrollPanel : Panel
{
    protected override Point ScrollToControl(Control activeControl)
    {
        // نرجع الموضع الحالي بدون تحريك الـ scroll
        return this.AutoScrollPosition;
    }
}

/// <summary>
/// إضافة/تعديل رحلة - نموذج متعدد الخطوات (Wizard)
/// </summary>
public partial class AddEditTripForm : Form
{
    private readonly ITripService _tripService;
    private readonly int _currentUserId;
    private readonly int? _tripId;
    private Trip _trip = null!;
    
    // Wizard Steps
    private int _currentStep = 0;
    private const int TotalSteps = 8; // معلومات، برنامج، نقل، إقامة، مرشد، مصاريف، رحلات اختيارية، مراجعة
    
    // Controls
    private Panel _headerPanel = null!;
    private Label _titleLabel = null!;
    private Label _stepLabel = null!;
    private NoScrollPanel _contentPanel = null!;
    private Panel _footerPanel = null!;
    private Button _previousButton = null!;
    private Button _nextButton = null!;
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    
    // Step 1 Controls
    private TextBox _tripNumberBox = null!;
    private TextBox _tripNameBox = null!;
    private TextBox _destinationBox = null!;
    private ComboBox _tripTypeCombo = null!;
    private TextBox _descriptionBox = null!;
    private DateTimePicker _startDatePicker = null!;
    private DateTimePicker _endDatePicker = null!;
    private NumericUpDown _capacityNumeric = null!;
    
    // Step 2 Controls - البرنامج
    private DataGridView _programGrid = null!;
    private Button _addDayButton = null!;
    private Button _removeDayButton = null!;
    
    // Step 3 Controls - النقل
    private DataGridView _transportationGrid = null!;
    private Button _addTransportButton = null!;
    private Button _removeTransportButton = null!;
    
    // Step 4 Controls - الإقامة
    private DataGridView _accommodationGrid = null!;
    private Button _addAccommodationButton = null!;
    private Button _removeAccommodationButton = null!;
    
    // Step 5 Controls - المرشد السياحي
    private TextBox _guideNameBox = null!;
    private TextBox _guidePhoneBox = null!;
    private TextBox _guideEmailBox = null!;
    private TextBox _guideLanguagesBox = null!;
    private TextBox _guideNotesBox = null!;
    
    // Step 6 Controls - المصاريف الأخرى
    private DataGridView _expensesGrid = null!;
    private Button _addExpenseButton = null!;
    private Button _removeExpenseButton = null!;
    
    // Step 7 Controls - الرحلات الاختيارية
    private DataGridView _optionalToursGrid = null!;
    private Button _addOptionalTourButton = null!;
    private Button _removeOptionalTourButton = null!;
    
    public AddEditTripForm(ITripService tripService, int currentUserId, int? tripId = null)
    {
        _tripService = tripService;
        _currentUserId = currentUserId;
        _tripId = tripId;
        
        InitializeComponent();
        SetupForm();
        InitializeCustomControls();
        _ = LoadDataAsync();
    }
    
    private void SetupForm()
    {
        this.Text = _tripId.HasValue ? "تعديل رحلة" : "رحلة جديدة";
        this.Size = new Size(900, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
    }
    
    private void InitializeCustomControls()
    {
        // Header Panel
        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 100,
            BackColor = ColorScheme.Primary,
            Padding = new Padding(20)
        };
        
        _titleLabel = new Label
        {
            Text = _tripId.HasValue ? "تعديل رحلة" : "إضافة رحلة جديدة",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 20)
        };
        _headerPanel.Controls.Add(_titleLabel);
        
        _stepLabel = new Label
        {
            Text = "الخطوة 1 من 3: المعلومات الأساسية",
            Font = new Font("Cairo", 11F),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 60)
        };
        _headerPanel.Controls.Add(_stepLabel);
        
        // Content Panel (scrollable) - بدون auto-scroll للعناصر
        _contentPanel = new NoScrollPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(30),
            AutoScroll = true
        };
        
        // Footer Panel
        _footerPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 80,
            BackColor = Color.FromArgb(245, 245, 245),
            Padding = new Padding(20)
        };
        
        _cancelButton = new Button
        {
            Text = "إلغاء",
            Size = new Size(120, 40),
            Location = new Point(20, 20),
            BackColor = Color.FromArgb(149, 165, 166),
            ForeColor = Color.White,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _cancelButton.FlatAppearance.BorderSize = 0;
        _cancelButton.Click += (s, e) => this.Close();
        _footerPanel.Controls.Add(_cancelButton);
        
        _previousButton = new Button
        {
            Text = "⬅️ السابق",
            Size = new Size(120, 40),
            Location = new Point(620, 20),
            BackColor = Color.FromArgb(52, 152, 219),
            ForeColor = Color.White,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        _previousButton.FlatAppearance.BorderSize = 0;
        _previousButton.Click += PreviousButton_Click;
        _footerPanel.Controls.Add(_previousButton);
        
        _nextButton = new Button
        {
            Text = "التالي ➡️",
            Size = new Size(120, 40),
            Location = new Point(750, 20),
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _nextButton.FlatAppearance.BorderSize = 0;
        _nextButton.Click += NextButton_Click;
        _footerPanel.Controls.Add(_nextButton);
        
        _saveButton = new Button
        {
            Text = "💾 حفظ",
            Size = new Size(120, 40),
            Location = new Point(750, 20),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Visible = false
        };
        _saveButton.FlatAppearance.BorderSize = 0;
        _saveButton.Click += SaveButton_Click;
        _footerPanel.Controls.Add(_saveButton);
        
        this.Controls.Add(_contentPanel);
        this.Controls.Add(_footerPanel);
        this.Controls.Add(_headerPanel);
        
        BuildStep1();
    }
    
    private async Task LoadDataAsync()
    {
        if (_tripId.HasValue)
        {
            _trip = await _tripService.GetTripByIdAsync(_tripId.Value, true) ?? new Trip();
            
            // ✅ التحقق من أن الرحلة غير مقفولة قبل السماح بالتعديل
            if (_trip.IsLockedForTrips)
            {
                MessageBox.Show(
                    "⚠️ لا يمكن تعديل هذه الرحلة!\n\n" +
                    "الرحلة مقفولة من قبل قسم الحجوزات.\n" +
                    "يجب فتح الرحلة من قسم الحجوزات أولاً لتتمكن من تعديلها.",
                    "رحلة مقفولة",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                
                // إغلاق الفورم فوراً
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }
        }
        else
        {
            _trip = new Trip { CreatedBy = _currentUserId };
            // تعيين رقم الرحلة التلقائي للرحلات الجديدة
            var tripNumber = await _tripService.GenerateTripNumberAsync();
            _trip.TripNumber = tripNumber;
        }
        
        // عرض الخطوة الأولى بعد تحميل البيانات
        UpdateStep();
    }
    
    private void BuildStep1()
    {
        _contentPanel.Controls.Clear();
        int y = 20;
        
        AddLabel("رقم الرحلة:", 20, ref y);
        _tripNumberBox = AddTextBox(20, ref y, enabled: false);
        
        AddLabel("اسم الرحلة: *", 20, ref y);
        _tripNameBox = AddTextBox(20, ref y);
        
        AddLabel("نوع الرحلة: *", 20, ref y);
        _tripTypeCombo = AddComboBox(20, ref y);
        _tripTypeCombo.Items.AddRange(new object[] { "عمرة", "سياحة داخلية", "سياحة خارجية", "حج", "رحلات دينية", "رحلات تعليمية" });
        
        AddLabel("الوصف:", 20, ref y);
        _descriptionBox = AddTextBox(20, ref y, multiline: true);
        
        AddLabel("بدء الرحلة: *", 20, ref y);
        var startDestinationBox = AddTextBox(20, ref y);
        startDestinationBox.Name = "startDestinationBox";
        startDestinationBox.PlaceholderText = "مثال: القاهرة";
        
        AddLabel("تاريخ بدء الرحلة: *", 20, ref y);
        _startDatePicker = new DateTimePicker { Location = new Point(20, y), Size = new Size(400, 30), Font = new Font("Cairo", 10F) };
        _contentPanel.Controls.Add(_startDatePicker);
        y += 50;
        
        AddLabel("انتهاء الرحلة: *", 20, ref y);
        var endDestinationBox = AddTextBox(20, ref y);
        endDestinationBox.Name = "endDestinationBox";
        endDestinationBox.PlaceholderText = "مثال: الأقصر";
        
        AddLabel("تاريخ انتهاء الرحلة: *", 20, ref y);
        _endDatePicker = new DateTimePicker { Location = new Point(20, y), Size = new Size(400, 30), Font = new Font("Cairo", 10F) };
        _contentPanel.Controls.Add(_endDatePicker);
        y += 50;
        
        AddLabel("عدد البالغين (Adult): *", 20, ref y);
        var adultCountNumeric = new NumericUpDown 
        { 
            Location = new Point(20, y), 
            Size = new Size(200, 30), 
            Font = new Font("Cairo", 10F), 
            Minimum = 0, 
            Maximum = 1000,
            Name = "adultCountNumeric"
        };
        _contentPanel.Controls.Add(adultCountNumeric);
        y += 50;
        
        AddLabel("عدد الأطفال (Child): *", 20, ref y);
        var childCountNumeric = new NumericUpDown 
        { 
            Location = new Point(20, y), 
            Size = new Size(200, 30), 
            Font = new Font("Cairo", 10F), 
            Minimum = 0, 
            Maximum = 1000,
            Name = "childCountNumeric"
        };
        _contentPanel.Controls.Add(childCountNumeric);
        y += 50;
        
        // عرض المجموع
        var totalLabel = new Label
        {
            Text = "إجمالي الأفراد: 0",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, y),
            ForeColor = ColorScheme.Primary,
            Name = "totalCapacityLabel"
        };
        _contentPanel.Controls.Add(totalLabel);
        
        // تحديث المجموع تلقائياً
        adultCountNumeric.ValueChanged += (s, e) =>
        {
            var adult = adultCountNumeric.Value;
            var child = childCountNumeric.Value;
            totalLabel.Text = $"إجمالي الأفراد: {adult + child} (ADULT: {adult}, CHILD: {child})";
        };
        
        childCountNumeric.ValueChanged += (s, e) =>
        {
            var adult = adultCountNumeric.Value;
            var child = childCountNumeric.Value;
            totalLabel.Text = $"إجمالي الأفراد: {adult + child} (ADULT: {adult}, CHILD: {child})";
        };
        
        y += 50;
        
        // ✅ إضافة حقل هامش الربح (اختياري)
        AddLabel("💰 هامش الربح %: (اختياري)", 20, ref y);
        var profitMarginNumeric = new NumericUpDown 
        { 
            Location = new Point(20, y), 
            Size = new Size(200, 30), 
            Font = new Font("Cairo", 10F), 
            Minimum = 0, 
            Maximum = 100,
            DecimalPlaces = 1,
            Value = 20, // القيمة الافتراضية 20%
            Name = "profitMarginNumeric"
        };
        _contentPanel.Controls.Add(profitMarginNumeric);
        
        var profitLabel = new Label
        {
            Text = "سيتم حساب السعر تلقائياً بناءً على التكاليف + هامش الربح",
            Font = new Font("Cairo", 9F, FontStyle.Italic),
            AutoSize = true,
            Location = new Point(230, y + 5),
            ForeColor = Color.Gray
        };
        _contentPanel.Controls.Add(profitLabel);
        
        y += 50;
    }
    
    private void BuildStep2()
    {
        _contentPanel.Controls.Clear();
        int y = 20;
        
        // جدول ADULT
        AddLabel("📅 البرنامج اليومي - ADULT", 20, ref y, fontSize: 12, bold: true);
        
        var adultGrid = new DataGridView
        {
            Location = new Point(20, y),
            Size = new Size(750, 300),
            Font = new Font("Cairo", 9F),
            AllowUserToAddRows = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Name = "adultProgramGrid"
        };
        
        // الأعمدة الجديدة
        var dayDateCol = new DataGridViewTextBoxColumn { Name = "DayDate", HeaderText = "التاريخ", Width = 85 };
        adultGrid.Columns.Add(dayDateCol);
        adultGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DayNumber", HeaderText = "اليوم", Width = 50 });
        adultGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Visits", HeaderText = "المزارات", Width = 160 });
        adultGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "VisitsCost", HeaderText = "سعر المزارات", Width = 95 });
        adultGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ParticipantsCount", HeaderText = "عدد الأفراد", Width = 85 });
        adultGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GuideCost", HeaderText = "سعر المرشد", Width = 95 });
        adultGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GuideCostPerPerson", HeaderText = "المرشد/فرد", Width = 85, ReadOnly = true });
        adultGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalCostPerPerson", HeaderText = "التكلفة/فرد", Width = 95, ReadOnly = true });
        
        // ✅ إضافة معالج أخطاء للـ DataGridView لمنع أخطاء الترتيب
        adultGrid.DataError += (s, e) =>
        {
            // منع ظهور رسالة الخطأ
            e.ThrowException = false;
        };
        
        // ✅ إضافة معالج للترتيب لتحويل القيم الفارغة
        adultGrid.SortCompare += (s, e) =>
        {
            var val1 = e.CellValue1?.ToString() ?? "";
            var val2 = e.CellValue2?.ToString() ?? "";
            e.SortResult = string.Compare(val1, val2);
            e.Handled = true;
        };
        
        // حساب تلقائي عند تغيير القيم
        adultGrid.CellValueChanged += (s, e) =>
        {
            if (e.RowIndex >= 0 && (e.ColumnIndex == adultGrid.Columns["GuideCost"]!.Index || 
                                   e.ColumnIndex == adultGrid.Columns["ParticipantsCount"]!.Index ||
                                   e.ColumnIndex == adultGrid.Columns["VisitsCost"]!.Index))
            {
                var row = adultGrid.Rows[e.RowIndex];
                var guideCost = decimal.TryParse(row.Cells["GuideCost"].Value?.ToString(), out var gc) ? gc : 0;
                var visitsCost = decimal.TryParse(row.Cells["VisitsCost"].Value?.ToString(), out var vc) ? vc : 0;
                var participants = int.TryParse(row.Cells["ParticipantsCount"].Value?.ToString(), out var pc) && pc > 0 ? pc : 1;
                
                row.Cells["GuideCostPerPerson"].Value = (guideCost / participants).ToString("N2");
                row.Cells["TotalCostPerPerson"].Value = ((guideCost / participants) + visitsCost).ToString("N2");
                
                // تحديث Total
                UpdateGridTotal(adultGrid, "Adult");
            }
        };
        
        // ✅ إضافة CurrentCellDirtyStateChanged لحفظ التعديلات فوراً
        adultGrid.CurrentCellDirtyStateChanged += (s, e) =>
        {
            if (adultGrid.IsCurrentCellDirty)
            {
                adultGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };
        
        // ✅ إضافة CellEndEdit لضمان الحفظ النهائي
        adultGrid.CellEndEdit += (s, e) =>
        {
            adultGrid.RefreshEdit();
        };
        
        _contentPanel.Controls.Add(adultGrid);
        y += 310;
        
        // أزرار ADULT - تحت الجدول
        var addAdultButton = new Button
        {
            Text = "➕ إضافة يوم واحد",
            Location = new Point(20, y),
            Size = new Size(160, 35),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        addAdultButton.FlatAppearance.BorderSize = 0;
        addAdultButton.Click += (s, e) => AddDayToGrid(adultGrid, "Adult");
        _contentPanel.Controls.Add(addAdultButton);
        
        var addBulkAdultButton = new Button
        {
            Text = "📝 إضافة أيام متعددة",
            Location = new Point(190, y),
            Size = new Size(180, 35),
            BackColor = Color.FromArgb(33, 150, 243),
            ForeColor = Color.White,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        addBulkAdultButton.FlatAppearance.BorderSize = 0;
        addBulkAdultButton.Click += (s, e) => AddBulkDays(adultGrid, "Adult");
        _contentPanel.Controls.Add(addBulkAdultButton);
        
        var removeAdultButton = new Button
        {
            Text = "🗑️ حذف المحدد",
            Location = new Point(380, y),
            Size = new Size(140, 35),
            BackColor = ColorScheme.Error,
            ForeColor = Color.White,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        removeAdultButton.FlatAppearance.BorderSize = 0;
        removeAdultButton.Click += (s, e) => 
        {
            if (adultGrid.SelectedRows.Count > 0)
            {
                adultGrid.Rows.Remove(adultGrid.SelectedRows[0]);
                UpdateGridTotal(adultGrid, "Adult");
            }
        };
        _contentPanel.Controls.Add(removeAdultButton);
        
        // عرض Total للـ ADULT في سطر منفصل تحت الأزرار
        var adultTotalLabel = new Label
        {
            Text = "💰 إجمالي التكلفة (Adult): 0.00 جنيه",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(20, y + 50),
            Name = "adultTotalLabel"
        };
        _contentPanel.Controls.Add(adultTotalLabel);
        
        // ✅ إضافة سعر الفرد (Adult) - بجانب Total في نفس السطر
        var adultPricePerPersonLabel = new Label
        {
            Text = "👤 سعر الفرد (Adult): 0.00 جنيه",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(39, 174, 96),
            AutoSize = true,
            Location = new Point(400, y + 50),
            Name = "adultPricePerPersonLabel"
        };
        _contentPanel.Controls.Add(adultPricePerPersonLabel);
        
        // تحديث Total عند تغيير أي قيمة
        adultGrid.CellValueChanged += (s, e) =>
        {
            if (e.RowIndex >= 0)
            {
                // ✅ استدعاء دالة التحديث المركزية
                UpdateGridTotal(adultGrid, "Adult");
            }
        };
        
        y += 90; // ✅ مسافة مناسبة للأزرار وال Labels
        
        // جدول CHILD
        AddLabel("📅 البرنامج اليومي - CHILD", 20, ref y, fontSize: 12, bold: true);
        
        var childGrid = new DataGridView
        {
            Location = new Point(20, y),
            Size = new Size(750, 300),
            Font = new Font("Cairo", 9F),
            AllowUserToAddRows = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Name = "childProgramGrid"
        };
        
        // نفس الأعمدة للـ CHILD
        var childDayDateCol = new DataGridViewTextBoxColumn { Name = "DayDate", HeaderText = "التاريخ", Width = 85 };
        childGrid.Columns.Add(childDayDateCol);
        childGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DayNumber", HeaderText = "اليوم", Width = 50 });
        childGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Visits", HeaderText = "المزارات", Width = 160 });
        childGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "VisitsCost", HeaderText = "سعر المزارات", Width = 95 });
        childGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ParticipantsCount", HeaderText = "عدد الأفراد", Width = 85 });
        childGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GuideCost", HeaderText = "سعر المرشد", Width = 95 });
        childGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GuideCostPerPerson", HeaderText = "المرشد/فرد", Width = 85, ReadOnly = true });
        childGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalCostPerPerson", HeaderText = "التكلفة/فرد", Width = 95, ReadOnly = true });
        
        // ✅ إضافة معالج أخطاء للـ DataGridView لمنع أخطاء الترتيب
        childGrid.DataError += (s, e) =>
        {
            // منع ظهور رسالة الخطأ
            e.ThrowException = false;
        };
        
        // ✅ إضافة معالج للترتيب لتحويل القيم الفارغة
        childGrid.SortCompare += (s, e) =>
        {
            var val1 = e.CellValue1?.ToString() ?? "";
            var val2 = e.CellValue2?.ToString() ?? "";
            e.SortResult = string.Compare(val1, val2);
            e.Handled = true;
        };
        
        // حساب تلقائي عند تغيير القيم
        childGrid.CellValueChanged += (s, e) =>
        {
            if (e.RowIndex >= 0 && (e.ColumnIndex == childGrid.Columns["GuideCost"]!.Index || 
                                   e.ColumnIndex == childGrid.Columns["ParticipantsCount"]!.Index ||
                                   e.ColumnIndex == childGrid.Columns["VisitsCost"]!.Index))
            {
                var row = childGrid.Rows[e.RowIndex];
                var guideCost = decimal.TryParse(row.Cells["GuideCost"].Value?.ToString(), out var gc) ? gc : 0;
                var visitsCost = decimal.TryParse(row.Cells["VisitsCost"].Value?.ToString(), out var vc) ? vc : 0;
                var participants = int.TryParse(row.Cells["ParticipantsCount"].Value?.ToString(), out var pc) && pc > 0 ? pc : 1;
                
                row.Cells["GuideCostPerPerson"].Value = (guideCost / participants).ToString("N2");
                row.Cells["TotalCostPerPerson"].Value = ((guideCost / participants) + visitsCost).ToString("N2");
                
                // تحديث Total
                UpdateGridTotal(childGrid, "Child");
            }
        };
        
        // ✅ إضافة CurrentCellDirtyStateChanged لحفظ التعديلات فوراً
        childGrid.CurrentCellDirtyStateChanged += (s, e) =>
        {
            if (childGrid.IsCurrentCellDirty)
            {
                childGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };
        
        // ✅ إضافة CellEndEdit لضمان الحفظ النهائي
        childGrid.CellEndEdit += (s, e) =>
        {
            childGrid.RefreshEdit();
        };
        
        _contentPanel.Controls.Add(childGrid);
        y += 310;
        
        // أزرار CHILD - تحت الجدول
        var addChildButton = new Button
        {
            Text = "➕ إضافة يوم واحد",
            Location = new Point(20, y),
            Size = new Size(160, 35),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        addChildButton.FlatAppearance.BorderSize = 0;
        addChildButton.Click += (s, e) => AddDayToGrid(childGrid, "Child");
        _contentPanel.Controls.Add(addChildButton);
        
        var addBulkChildButton = new Button
        {
            Text = "📝 إضافة أيام متعددة",
            Location = new Point(190, y),
            Size = new Size(180, 35),
            BackColor = Color.FromArgb(33, 150, 243),
            ForeColor = Color.White,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        addBulkChildButton.FlatAppearance.BorderSize = 0;
        addBulkChildButton.Click += (s, e) => AddBulkDays(childGrid, "Child");
        _contentPanel.Controls.Add(addBulkChildButton);
        
        var removeChildButton = new Button
        {
            Text = "🗑️ حذف المحدد",
            Location = new Point(380, y),
            Size = new Size(140, 35),
            BackColor = ColorScheme.Error,
            ForeColor = Color.White,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        removeChildButton.FlatAppearance.BorderSize = 0;
        removeChildButton.Click += (s, e) => 
        {
            if (childGrid.SelectedRows.Count > 0)
            {
                childGrid.Rows.Remove(childGrid.SelectedRows[0]);
                UpdateGridTotal(childGrid, "Child");
            }
        };
        _contentPanel.Controls.Add(removeChildButton);
        
        // عرض Total للـ CHILD في سطر منفصل تحت الأزرار
        var childTotalLabel = new Label
        {
            Text = "💰 إجمالي التكلفة (Child): 0.00 جنيه",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(20, y + 50),
            Name = "childTotalLabel"
        };
        _contentPanel.Controls.Add(childTotalLabel);
        
        // ✅ إضافة سعر الفرد (Child) - بجانب Total في نفس السطر
        var childPricePerPersonLabel = new Label
        {
            Text = "👤 سعر الفرد (Child): 0.00 جنيه",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(39, 174, 96),
            AutoSize = true,
            Location = new Point(400, y + 50),
            Name = "childPricePerPersonLabel"
        };
        _contentPanel.Controls.Add(childPricePerPersonLabel);
        
        // تحديث Total عند تغيير أي قيمة
        childGrid.CellValueChanged += (s, e) =>
        {
            if (e.RowIndex >= 0)
            {
                // ✅ استدعاء دالة التحديث المركزية
                UpdateGridTotal(childGrid, "Child");
            }
        };
        
        y += 100; // ✅ زيادة المسافة ليتسع لـ Label سعر الفرد
        
        // ═══════════════════════════════════════════════════════════
        // 🎯 المجموع الكلي (Adult + Child)
        // ═══════════════════════════════════════════════════════════
        var grandTotalPanel = new Panel
        {
            Location = new Point(20, y),
            Size = new Size(820, 60),
            BackColor = Color.FromArgb(46, 125, 50),  // أخضر داكن
            BorderStyle = BorderStyle.FixedSingle
        };
        
        var grandTotalLabel = new Label
        {
            Text = "💎 الإجمالي الكلي (Adult + Child): 0.00 جنيه",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            Size = new Size(800, 40),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(10, 10),
            Name = "grandTotalLabel"
        };
        
        grandTotalPanel.Controls.Add(grandTotalLabel);
        _contentPanel.Controls.Add(grandTotalPanel);
        
        // تحديث Adult Total مع تحديث Grand Total
        adultGrid.CellValueChanged += (s, e) =>
        {
            if (e.RowIndex >= 0)
            {
                decimal total = 0;
                foreach (DataGridViewRow row in adultGrid.Rows)
                {
                    if (row.IsNewRow) continue;
                    decimal visitsCost = decimal.TryParse(row.Cells["VisitsCost"].Value?.ToString(), out var vc) ? vc : 0;
                    decimal guideCost = decimal.TryParse(row.Cells["GuideCost"].Value?.ToString(), out var gc) ? gc : 0;
                    int participants = int.TryParse(row.Cells["ParticipantsCount"].Value?.ToString(), out var pc) && pc > 0 ? pc : 1;
                    
                    total += (visitsCost * participants) + guideCost;
                }
                adultTotalLabel.Text = $"💰 إجمالي التكلفة (Adult): {total:N2} جنيه";
                
                // تحديث المجموع الكلي
                UpdateGrandTotalFromGrids();
            }
        };
        
        // حفظ reference للـ grids في متغير الـ program grid
        _programGrid = adultGrid; // للتوافق مع الكود القديم
    }
    
    private void AddDayToGrid(DataGridView grid, string bookingType)
    {
        int dayNumber = grid.Rows.Count + 1;
        
        // حساب التاريخ بناءً على رقم اليوم وتاريخ البداية
        DateTime dayDate = _startDatePicker.Value.AddDays(dayNumber - 1);
        
        grid.Rows.Add(
            dayDate.ToString("yyyy-MM-dd"),  // التاريخ
            dayNumber,                       // رقم اليوم
            "",                              // المزارات
            0,                               // سعر المزارات
            0,                               // عدد الأفراد
            0,                               // سعر المرشد
            "0.00",                          // المرشد/فرد (محسوب)
            "0.00"                           // التكلفة/فرد (محسوب)
        );
    }
    
    private void AddBulkDays(DataGridView grid, string bookingType)
    {
        try
        {
            // الحصول على عدد الأفراد الافتراضي من الخطوة 1
            var participantsControl = bookingType == "Adult" 
                ? _contentPanel.Controls.Find("adultCountNumeric", true).FirstOrDefault() as NumericUpDown
                : _contentPanel.Controls.Find("childCountNumeric", true).FirstOrDefault() as NumericUpDown;
            
            int defaultParticipants = (int)(participantsControl?.Value ?? 1);
            
            // فتح نموذج الإدخال المتعدد
            using var bulkForm = new BulkVisitsEntryForm(
                _startDatePicker.Value, 
                grid.Rows.Count, 
                bookingType,
                defaultParticipants
            );
            
            if (bulkForm.ShowDialog() == DialogResult.OK)
            {
                // إضافة الأيام للجدول
                foreach (var entry in bulkForm.VisitEntries)
                {
                    decimal guideCostPerPerson = entry.ParticipantsCount > 0 
                        ? entry.GuideCost / entry.ParticipantsCount 
                        : 0;
                    
                    decimal totalCostPerPerson = guideCostPerPerson + entry.VisitsCost;
                    
                    grid.Rows.Add(
                        entry.DayDate.ToString("yyyy-MM-dd"),
                        entry.DayNumber,
                        entry.Visits,
                        entry.VisitsCost,
                        entry.ParticipantsCount,
                        entry.GuideCost,
                        guideCostPerPerson.ToString("N2"),
                        totalCostPerPerson.ToString("N2")
                    );
                }
                
                // تحديث Total
                UpdateGridTotal(grid, bookingType);
                
                MessageBox.Show($"✅ تم إضافة {bulkForm.VisitEntries.Count} يوم بنجاح", 
                    "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ خطأ في إضافة الأيام: {ex.Message}", 
                "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void UpdateGridTotal(DataGridView grid, string bookingType)
    {
        decimal total = 0;
        int totalParticipants = 0;
        
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.IsNewRow) continue;
            decimal visitsCost = decimal.TryParse(row.Cells["VisitsCost"].Value?.ToString(), out var vc) ? vc : 0;
            decimal guideCost = decimal.TryParse(row.Cells["GuideCost"].Value?.ToString(), out var gc) ? gc : 0;
            int participants = int.TryParse(row.Cells["ParticipantsCount"].Value?.ToString(), out var pc) && pc > 0 ? pc : 1;
            
            total += (visitsCost * participants) + guideCost;
            
            // ✅ جمع عدد الأفراد الإجمالي
            if (totalParticipants == 0) // نأخذ القيمة من أول صف فقط لأنها ثابتة
                totalParticipants = participants;
        }
        
        // ✅ حساب سعر الفرد = التكلفة الإجمالية ÷ عدد الأفراد
        decimal pricePerPerson = totalParticipants > 0 ? total / totalParticipants : 0;
        
        // تحديث label التكلفة الإجمالية
        string labelName = bookingType == "Adult" ? "adultTotalLabel" : "childTotalLabel";
        var totalLabel = _contentPanel.Controls.Find(labelName, true).FirstOrDefault() as Label;
        if (totalLabel != null)
        {
            totalLabel.Text = $"💰 إجمالي التكلفة ({bookingType}): {total:N2} جنيه";
        }
        
        // ✅ تحديث label سعر الفرد
        string pricePerPersonLabelName = bookingType == "Adult" ? "adultPricePerPersonLabel" : "childPricePerPersonLabel";
        var pricePerPersonLabel = _contentPanel.Controls.Find(pricePerPersonLabelName, true).FirstOrDefault() as Label;
        if (pricePerPersonLabel != null)
        {
            pricePerPersonLabel.Text = $"👤 سعر الفرد ({bookingType}): {pricePerPerson:N2} جنيه";
        }
        
        // تحديث المجموع الكلي
        UpdateGrandTotalFromGrids();
    }
    
    private void UpdateGrandTotalFromGrids()
    {
        var adultGrid = _contentPanel.Controls.Find("adultProgramGrid", false).FirstOrDefault() as DataGridView;
        var childGrid = _contentPanel.Controls.Find("childProgramGrid", false).FirstOrDefault() as DataGridView;
        var grandTotalLabel = _contentPanel.Controls.Find("grandTotalLabel", true).FirstOrDefault() as Label;
        
        if (grandTotalLabel == null) return;
        
        decimal adultTotal = 0;
        decimal childTotal = 0;
        
        // حساب إجمالي Adult
        if (adultGrid != null)
        {
            foreach (DataGridViewRow row in adultGrid.Rows)
            {
                if (row.IsNewRow) continue;
                decimal visitsCost = decimal.TryParse(row.Cells["VisitsCost"].Value?.ToString(), out var vc) ? vc : 0;
                decimal guideCost = decimal.TryParse(row.Cells["GuideCost"].Value?.ToString(), out var gc) ? gc : 0;
                int participants = int.TryParse(row.Cells["ParticipantsCount"].Value?.ToString(), out var pc) && pc > 0 ? pc : 1;
                
                adultTotal += (visitsCost * participants) + guideCost;
            }
        }
        
        // حساب إجمالي Child
        if (childGrid != null)
        {
            foreach (DataGridViewRow row in childGrid.Rows)
            {
                if (row.IsNewRow) continue;
                decimal visitsCost = decimal.TryParse(row.Cells["VisitsCost"].Value?.ToString(), out var vc) ? vc : 0;
                decimal guideCost = decimal.TryParse(row.Cells["GuideCost"].Value?.ToString(), out var gc) ? gc : 0;
                int participants = int.TryParse(row.Cells["ParticipantsCount"].Value?.ToString(), out var pc) && pc > 0 ? pc : 1;
                
                childTotal += (visitsCost * participants) + guideCost;
            }
        }
        
        decimal grandTotal = adultTotal + childTotal;
        grandTotalLabel.Text = $"💎 الإجمالي الكلي (Adult + Child): {grandTotal:N2} جنيه  |  Adult: {adultTotal:N2}  |  Child: {childTotal:N2}";
    }
    
    // ✅ دالة لحساب إجمالي تكلفة النقل
    private void UpdateTransportationTotal()
    {
        if (_transportationGrid == null) return;
        
        decimal totalTransportCost = 0;
        
        foreach (DataGridViewRow row in _transportationGrid.Rows)
        {
            if (row.IsNewRow) continue;
            
            var costPerVehicle = decimal.TryParse(row.Cells["CostPerVehicle"].Value?.ToString(), out var cpv) ? cpv : 0;
            var numberOfVehicles = int.TryParse(row.Cells["NumberOfVehicles"].Value?.ToString(), out var nov) && nov > 0 ? nov : 1;
            var tourLeaderTip = decimal.TryParse(row.Cells["TourLeaderTip"].Value?.ToString(), out var tlt) ? tlt : 0;
            var driverTip = decimal.TryParse(row.Cells["DriverTip"].Value?.ToString(), out var dt) ? dt : 0;
            
            // الإجمالي = (التكلفة × عدد المركبات) + إكرامية التور ليدر + إكرامية السواق
            totalTransportCost += (costPerVehicle * numberOfVehicles) + tourLeaderTip + driverTip;
        }
        
        var transportTotalLabel = _contentPanel.Controls.Find("transportTotalLabel", true).FirstOrDefault() as Label;
        if (transportTotalLabel != null)
        {
            transportTotalLabel.Text = $"💰 إجمالي تكلفة النقل: {totalTransportCost:N2} جنيه";
        }
    }
    
    private void AddDay_Click(object? sender, EventArgs e)
    {
        // تم استبدالها بـ AddDayToGrid
        if (_programGrid != null)
            AddDayToGrid(_programGrid, "Adult");
    }
    
    private void BuildStep3()
    {
        _contentPanel.Controls.Clear();
        int y = 20;
        
        AddLabel("🚗 النقل والمواصلات (لكل مزار)", 20, ref y, fontSize: 12, bold: true);
        
        _transportationGrid = new DataGridView
        {
            Location = new Point(20, y),
            Size = new Size(820, 350),
            Font = new Font("Cairo", 9F),
            AllowUserToAddRows = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        
        // ✅ عمود اسم المزار (للقراءة فقط - مستورد من الخطوة 2)
        var visitColumn = new DataGridViewTextBoxColumn 
        { 
            Name = "VisitName", 
            HeaderText = "اسم المزار",
            Width = 150,
            ReadOnly = true,
            DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(240, 248, 255) }
        };
        _transportationGrid.Columns.Add(visitColumn);
        
        // ✅ عمود رقم اليوم (للقراءة فقط)
        var dayColumn = new DataGridViewTextBoxColumn 
        { 
            Name = "DayNumber", 
            HeaderText = "اليوم",
            Width = 50,
            ReadOnly = true,
            DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(240, 248, 255) }
        };
        _transportationGrid.Columns.Add(dayColumn);
        
        // إضافة الأعمدة الجديدة
        var typeColumn = new DataGridViewComboBoxColumn 
        { 
            Name = "Type", 
            HeaderText = "النوع",
            Width = 100,
            DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
            FlatStyle = FlatStyle.Flat
        };
        typeColumn.Items.AddRange(new object[] { "أتوبيس", "ميني باص", "كوستر", "هاي أس", "ملاكي", "طائرة", "قطار" });
        typeColumn.ValueType = typeof(string);
        _transportationGrid.Columns.Add(typeColumn);
        
        // التاريخ
        _transportationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TransportDate", HeaderText = "التاريخ", Width = 90 });
        
        // مسار النقل (اختياري - يمكن تركه فارغ)
        _transportationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Route", HeaderText = "المسار", Width = 150 });
        
        // عمود المقاعد (للقراءة فقط - يتم تحديثه تلقائياً بناءً على نوع السيارة)
        var seatsColumn = new DataGridViewTextBoxColumn 
        { 
            Name = "SeatsPerVehicle", 
            HeaderText = "المقاعد", 
            Width = 60,
            ReadOnly = true,
            DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(240, 240, 240) }
        };
        _transportationGrid.Columns.Add(seatsColumn);
        
        // ✅ إضافة عمود عدد المركبات
        _transportationGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "NumberOfVehicles", 
            HeaderText = "عدد المركبات", 
            Width = 80 
        });
        
        // عدد الأفراد
        _transportationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ParticipantsCount", HeaderText = "عدد الأفراد", Width = 80 });
        
        // التكلفة الإجمالية
        _transportationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CostPerVehicle", HeaderText = "التكلفة الإجمالية", Width = 100 });
        
        // إكرامية التور ليدر
        _transportationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TourLeaderTip", HeaderText = "إكرامية التور ليدر", Width = 110 });
        
        // إكرامية السواق
        _transportationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DriverTip", HeaderText = "إكرامية السواق", Width = 100 });
        
        // السعر/فرد (محسوب تلقائياً: التكلفة + إكرامية التور ليدر + إكرامية السواق ÷ عدد الأفراد)
        var costPerPersonCol = new DataGridViewTextBoxColumn 
        { 
            Name = "CostPerPerson", 
            HeaderText = "السعر/فرد", 
            Width = 90,
            ReadOnly = true,
            DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(255, 248, 220) }
        };
        _transportationGrid.Columns.Add(costPerPersonCol);
        
        // ✅ إضافة معالج أخطاء للـ DataGridView لمنع أخطاء الترتيب
        _transportationGrid.DataError += (s, e) =>
        {
            // منع ظهور رسالة الخطأ
            e.ThrowException = false;
        };
        
        // ✅ إضافة معالج للترتيب لتحويل القيم الفارغة
        _transportationGrid.SortCompare += (s, e) =>
        {
            var val1 = e.CellValue1?.ToString() ?? "";
            var val2 = e.CellValue2?.ToString() ?? "";
            e.SortResult = string.Compare(val1, val2);
            e.Handled = true;
        };
        
        // ✅ لا تستدعي PopulateTransportationFromVisits() هنا
        // سيتم استدعاؤها من UpdateStep() حسب الشرط
        
        // حدث تغيير نوع السيارة لتحديث عدد المقاعد تلقائياً
        _transportationGrid.CellValueChanged += TransportationGrid_CellValueChanged;
        _transportationGrid.CurrentCellDirtyStateChanged += (s, e) =>
        {
            if (_transportationGrid.IsCurrentCellDirty)
            {
                _transportationGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };
        
        // ✅ إضافة CellEndEdit لضمان الحفظ النهائي
        _transportationGrid.CellEndEdit += (s, e) =>
        {
            _transportationGrid.RefreshEdit();
        };
        
        _contentPanel.Controls.Add(_transportationGrid);
        y += 370;
        
        // ✅ زر تحديث المزارات من الخطوة 2
        var refreshButton = new Button
        {
            Text = "🔄 تحديث المزارات من البرنامج",
            Location = new Point(20, y),
            Size = new Size(200, 35),
            BackColor = ColorScheme.Info,
            ForeColor = Color.White,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        refreshButton.FlatAppearance.BorderSize = 0;
        refreshButton.Click += (s, e) => PopulateTransportationFromVisits();
        _contentPanel.Controls.Add(refreshButton);
        
        _addTransportButton = new Button
        {
            Text = "➕ إضافة نقل إضافي",
            Location = new Point(230, y),
            Size = new Size(150, 35),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _addTransportButton.FlatAppearance.BorderSize = 0;
        _addTransportButton.Click += (s, e) => _transportationGrid.Rows.Add(
            "[نقل إضافي]",                        // اسم المزار
            0,                                     // رقم اليوم
            "أتوبيس",                              // النوع
            DateTime.Now.ToString("yyyy-MM-dd"),  // التاريخ
            "",                                    // المسار
            50,                                    // المقاعد (تلقائي بناءً على النوع)
            1,                                     // ✅ عدد المركبات
            0,                                     // عدد الأفراد
            0,                                     // التكلفة الإجمالية
            0,                                     // إكرامية التور ليدر
            0,                                     // إكرامية السواق
            "0.00"                                 // السعر/فرد (محسوب)
        );
        _contentPanel.Controls.Add(_addTransportButton);
        
        _removeTransportButton = new Button
        {
            Text = "🗑️ حذف المحدد",
            Location = new Point(390, y),
            Size = new Size(150, 35),
            BackColor = ColorScheme.Error,
            ForeColor = Color.White,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _removeTransportButton.FlatAppearance.BorderSize = 0;
        _removeTransportButton.Click += (s, e) => 
        {
            if (_transportationGrid.SelectedRows.Count > 0)
            {
                _transportationGrid.Rows.Remove(_transportationGrid.SelectedRows[0]);
                // ✅ تحديث الإجمالي بعد الحذف
                UpdateTransportationTotal();
            }
        };
        _contentPanel.Controls.Add(_removeTransportButton);
        
        // ✅ إضافة label لإجمالي تكلفة النقل
        y += 50;
        var transportTotalLabel = new Label
        {
            Text = "💰 إجمالي تكلفة النقل: 0.00 جنيه",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(20, y),
            Name = "transportTotalLabel"
        };
        _contentPanel.Controls.Add(transportTotalLabel);
        
        // ✅ إضافة حدث لتحديث الإجمالي عند تغيير القيم
        _transportationGrid.CellValueChanged += (s, e) =>
        {
            if (e.RowIndex >= 0)
            {
                UpdateTransportationTotal();
            }
        };
    }
    
    private void BuildStep4()
    {
        _contentPanel.Controls.Clear();
        int y = 20;
        
        AddLabel("🏨 الإقامة والفنادق", 20, ref y, fontSize: 12, bold: true);
        
        _accommodationGrid = new DataGridView
        {
            Location = new Point(20, y),
            Size = new Size(1200, 350),
            Font = new Font("Cairo", 9F),
            AllowUserToAddRows = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ScrollBars = ScrollBars.Both
        };
        
        // عمود التاريخ
        _accommodationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "التاريخ", Width = 100 });
        
        // عمود النوع
        var typeColumn = new DataGridViewComboBoxColumn 
        { 
            Name = "Type", 
            HeaderText = "النوع",
            Width = 90,
            DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
            FlatStyle = FlatStyle.Flat
        };
        typeColumn.Items.AddRange(new object[] { "فندق", "نايل كروز", "منتجع", "شقة فندقية", "بيت شباب" });
        typeColumn.ValueType = typeof(string);
        _accommodationGrid.Columns.Add(typeColumn);
        
        // عمود الاسم
        _accommodationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "HotelName", HeaderText = "الاسم", Width = 120 });
        
        // عمود التصنيف
        var ratingColumn = new DataGridViewComboBoxColumn 
        { 
            Name = "Rating", 
            HeaderText = "التصنيف",
            Width = 70,
            DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
            FlatStyle = FlatStyle.Flat
        };
        ratingColumn.Items.AddRange(new object[] { "⭐", "⭐⭐", "⭐⭐⭐", "⭐⭐⭐⭐", "⭐⭐⭐⭐⭐" });
        ratingColumn.ValueType = typeof(string);
        _accommodationGrid.Columns.Add(ratingColumn);
        
        // عمود مستوى النايل كروز
        var cruiseLevelColumn = new DataGridViewComboBoxColumn 
        { 
            Name = "CruiseLevel", 
            HeaderText = "المستوى",
            Width = 80,
            DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
            FlatStyle = FlatStyle.Flat
        };
        cruiseLevelColumn.Items.AddRange(new object[] { "", "Standard", "Deluxe", "Luxury" });
        cruiseLevelColumn.ValueType = typeof(string);
        _accommodationGrid.Columns.Add(cruiseLevelColumn);
        
        // عمود نوع الغرفة
        var roomTypeColumn = new DataGridViewComboBoxColumn 
        { 
            Name = "RoomType", 
            HeaderText = "نوع الغرفة",
            Width = 80,
            DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
            FlatStyle = FlatStyle.Flat
        };
        roomTypeColumn.Items.AddRange(new object[] { "فردي", "مزدوج", "ثلاثي", "رباعي", "جناح" });
        roomTypeColumn.ValueType = typeof(string);
        _accommodationGrid.Columns.Add(roomTypeColumn);
        
        // عمود عدد الغرف
        _accommodationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "NumberOfRooms", HeaderText = "عدد الغرف", Width = 70 });
        
        // عمود عدد الليالي
        _accommodationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "NumberOfNights", HeaderText = "عدد الليالي", Width = 70 });
        
        // عمود عدد الأفراد
        _accommodationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ParticipantsCount", HeaderText = "عدد الأفراد", Width = 70 });
        
        // عمود العملة
        var currencyColumn = new DataGridViewComboBoxColumn 
        { 
            Name = "Currency", 
            HeaderText = "العملة",
            Width = 90,
            DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
            FlatStyle = FlatStyle.Flat
        };
        currencyColumn.Items.AddRange(new object[] { "جنيه مصري", "دولار", "جنيه استرليني" });
        currencyColumn.ValueType = typeof(string);
        _accommodationGrid.Columns.Add(currencyColumn);
        
        // عمود سعر الصرف
        _accommodationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ExchangeRate", HeaderText = "سعر الصرف", Width = 80 });
        
        // عمود سعر الليلة
        _accommodationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "PricePerNight", HeaderText = "سعر الليلة", Width = 80 });
        
        // عمود إقامة المرشد
        _accommodationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GuideCost", HeaderText = "إقامة المرشد", Width = 90 });
        
        // عمود إقامة المرشد/فرد (محسوب)
        _accommodationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GuideCostPerPerson", HeaderText = "المرشد/فرد", Width = 80, ReadOnly = true });
        
        // عمود الوجبات
        var mealPlanColumn = new DataGridViewComboBoxColumn 
        { 
            Name = "MealPlan", 
            HeaderText = "الوجبات",
            Width = 70,
            DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
            FlatStyle = FlatStyle.Flat
        };
        mealPlanColumn.Items.AddRange(new object[] { "BB", "HB", "FB" });
        mealPlanColumn.ValueType = typeof(string);
        _accommodationGrid.Columns.Add(mealPlanColumn);
        
        // عمود التكلفة الإجمالية (محسوب)
        _accommodationGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalCost", HeaderText = "الإجمالي", Width = 90, ReadOnly = true });
        
        // ✅ إضافة معالج أخطاء للـ DataGridView لمنع أخطاء الترتيب
        _accommodationGrid.DataError += (s, e) =>
        {
            // منع ظهور رسالة الخطأ
            e.ThrowException = false;
        };
        
        // ✅ إضافة معالج للترتيب لتحويل القيم الفارغة
        _accommodationGrid.SortCompare += (s, e) =>
        {
            var val1 = e.CellValue1?.ToString() ?? "";
            var val2 = e.CellValue2?.ToString() ?? "";
            e.SortResult = string.Compare(val1, val2);
            e.Handled = true;
        };
        
        // Add event handlers للحسابات التلقائية
        _accommodationGrid.CellValueChanged += AccommodationGrid_CellValueChanged;
        _accommodationGrid.CurrentCellDirtyStateChanged += (s, e) =>
        {
            if (_accommodationGrid.IsCurrentCellDirty)
            {
                _accommodationGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };
        
        // ✅ إضافة CellEndEdit لضمان الحفظ النهائي
        _accommodationGrid.CellEndEdit += (s, e) =>
        {
            _accommodationGrid.RefreshEdit();
        };
        
        _contentPanel.Controls.Add(_accommodationGrid);
        y += 370;
        
        _addAccommodationButton = new Button
        {
            Text = "➕ إضافة فندق",
            Location = new Point(20, y),
            Size = new Size(150, 35),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _addAccommodationButton.FlatAppearance.BorderSize = 0;
        _addAccommodationButton.Click += (s, e) => 
        {
            int rowIndex = _accommodationGrid.Rows.Add(
                DateTime.Now.ToString("yyyy-MM-dd"),  // التاريخ
                "فندق",                                // النوع
                "",                                    // الاسم
                "⭐⭐⭐",                                // التصنيف
                "",                                    // مستوى النايل كروز
                "مزدوج",                              // نوع الغرفة
                1,                                     // عدد الغرف
                1,                                     // عدد الليالي
                1,                                     // عدد الأفراد
                "جنيه مصري",                          // العملة
                1.0,                                   // سعر الصرف
                0,                                     // سعر الليلة
                0,                                     // إقامة المرشد
                0,                                     // المرشد/فرد (محسوب)
                "BB",                                  // الوجبات
                0                                      // الإجمالي (محسوب)
            );
        };
        _contentPanel.Controls.Add(_addAccommodationButton);
        
        _removeAccommodationButton = new Button
        {
            Text = "🗑️ حذف المحدد",
            Location = new Point(180, y),
            Size = new Size(150, 35),
            BackColor = ColorScheme.Error,
            ForeColor = Color.White,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _removeAccommodationButton.FlatAppearance.BorderSize = 0;
        _removeAccommodationButton.Click += (s, e) => 
        {
            if (_accommodationGrid.SelectedRows.Count > 0)
                _accommodationGrid.Rows.Remove(_accommodationGrid.SelectedRows[0]);
        };
        _contentPanel.Controls.Add(_removeAccommodationButton);
    }
    
    private void BuildStep5()
    {
        _contentPanel.Controls.Clear();
        int y = 20;
        
        AddLabel("👨‍✈️ بيانات المرشد السياحي", 20, ref y, fontSize: 12, bold: true);
        
        AddLabel("اسم المرشد:", 20, ref y);
        _guideNameBox = AddTextBox(20, ref y);
        
        AddLabel("رقم الهاتف:", 20, ref y);
        _guidePhoneBox = AddTextBox(20, ref y);
        
        AddLabel("البريد الإلكتروني:", 20, ref y);
        _guideEmailBox = AddTextBox(20, ref y);
        
        AddLabel("اللغات:", 20, ref y);
        _guideLanguagesBox = AddTextBox(20, ref y);
        _guideLanguagesBox.PlaceholderText = "مثال: عربي، إنجليزي، فرنسي";
        
        AddLabel("ملاحظات:", 20, ref y);
        _guideNotesBox = AddTextBox(20, ref y, multiline: true);
    }
    
    private void BuildStep6()
    {
        _contentPanel.Controls.Clear();
        int y = 20;
        
        AddLabel("💰 المصاريف الأخرى", 20, ref y, fontSize: 12, bold: true);
        
        _expensesGrid = new DataGridView
        {
            Location = new Point(20, y),
            Size = new Size(820, 350),
            Font = new Font("Cairo", 9F),
            AllowUserToAddRows = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        
        var expenseTypeColumn = new DataGridViewComboBoxColumn 
        { 
            Name = "ExpenseType", 
            HeaderText = "نوع المصروف",
            Width = 150,
            DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
            FlatStyle = FlatStyle.Flat
        };
        expenseTypeColumn.Items.AddRange(new object[] { "تأشيرات", "تأمين", "إكراميات", "مصاريف إدارية", "أخرى" });
        expenseTypeColumn.ValueType = typeof(string);
        _expensesGrid.Columns.Add(expenseTypeColumn);
        
        _expensesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "الوصف", Width = 200 });
        _expensesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Amount", HeaderText = "المبلغ", Width = 120 });
        _expensesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Notes", HeaderText = "ملاحظات", Width = 200 });
        
        // ✅ إضافة معالج أخطاء للـ DataGridView لمنع أخطاء الترتيب
        _expensesGrid.DataError += (s, e) =>
        {
            // منع ظهور رسالة الخطأ
            e.ThrowException = false;
        };
        
        // ✅ إضافة معالج للترتيب لتحويل القيم الفارغة
        _expensesGrid.SortCompare += (s, e) =>
        {
            var val1 = e.CellValue1?.ToString() ?? "";
            var val2 = e.CellValue2?.ToString() ?? "";
            e.SortResult = string.Compare(val1, val2);
            e.Handled = true;
        };
        
        // ✅ إضافة CurrentCellDirtyStateChanged لحفظ التعديلات فوراً
        _expensesGrid.CurrentCellDirtyStateChanged += (s, e) =>
        {
            if (_expensesGrid.IsCurrentCellDirty)
            {
                _expensesGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };
        
        // ✅ إضافة CellEndEdit لضمان الحفظ النهائي
        _expensesGrid.CellEndEdit += (s, e) =>
        {
            _expensesGrid.RefreshEdit();
        };
        
        _contentPanel.Controls.Add(_expensesGrid);
        y += 370;
        
        _addExpenseButton = new Button
        {
            Text = "➕ إضافة مصروف",
            Location = new Point(20, y),
            Size = new Size(150, 35),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _addExpenseButton.FlatAppearance.BorderSize = 0;
        _addExpenseButton.Click += (s, e) => _expensesGrid.Rows.Add("أخرى", "", 0, "");
        _contentPanel.Controls.Add(_addExpenseButton);
        
        _removeExpenseButton = new Button
        {
            Text = "🗑️ حذف المحدد",
            Location = new Point(180, y),
            Size = new Size(150, 35),
            BackColor = ColorScheme.Error,
            ForeColor = Color.White,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _removeExpenseButton.FlatAppearance.BorderSize = 0;
        _removeExpenseButton.Click += (s, e) => 
        {
            if (_expensesGrid.SelectedRows.Count > 0)
                _expensesGrid.Rows.Remove(_expensesGrid.SelectedRows[0]);
        };
        _contentPanel.Controls.Add(_removeExpenseButton);
    }
    
    private void BuildStep7()
    {
        _contentPanel.Controls.Clear();
        int y = 20;
        
        AddLabel("🎯 الرحلات الاختيارية", 20, ref y, fontSize: 12, bold: true);
        
        _optionalToursGrid = new DataGridView
        {
            Location = new Point(20, y),
            Size = new Size(820, 350),
            Font = new Font("Cairo", 9F),
            AllowUserToAddRows = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        
        _optionalToursGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TourName", HeaderText = "نوع الرحلة", Width = 150 });
        _optionalToursGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "SellingPrice", HeaderText = "سعر البيع", Width = 100 });
        _optionalToursGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "PurchasePrice", HeaderText = "سعر الشراء", Width = 100 });
        _optionalToursGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GuideCommission", HeaderText = "عمولة المرشد", Width = 100 });
        _optionalToursGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "SalesCommission", HeaderText = "عمولة المندوب", Width = 100 });
        _optionalToursGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ParticipantsCount", HeaderText = "عدد الأفراد", Width = 100 });
        
        // ✅ إضافة معالج أخطاء للـ DataGridView لمنع أخطاء الترتيب
        _optionalToursGrid.DataError += (s, e) =>
        {
            // منع ظهور رسالة الخطأ
            e.ThrowException = false;
        };
        
        // ✅ إضافة معالج للترتيب لتحويل القيم الفارغة
        _optionalToursGrid.SortCompare += (s, e) =>
        {
            var val1 = e.CellValue1?.ToString() ?? "";
            var val2 = e.CellValue2?.ToString() ?? "";
            e.SortResult = string.Compare(val1, val2);
            e.Handled = true;
        };
        
        // ✅ إضافة CurrentCellDirtyStateChanged لحفظ التعديلات فوراً
        _optionalToursGrid.CurrentCellDirtyStateChanged += (s, e) =>
        {
            if (_optionalToursGrid.IsCurrentCellDirty)
            {
                _optionalToursGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };
        
        // ✅ إضافة CellEndEdit لضمان الحفظ النهائي
        _optionalToursGrid.CellEndEdit += (s, e) =>
        {
            _optionalToursGrid.RefreshEdit();
        };
        
        _contentPanel.Controls.Add(_optionalToursGrid);
        y += 370;
        
        _addOptionalTourButton = new Button
        {
            Text = "➕ إضافة رحلة",
            Location = new Point(20, y),
            Size = new Size(150, 35),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _addOptionalTourButton.FlatAppearance.BorderSize = 0;
        _addOptionalTourButton.Click += (s, e) => _optionalToursGrid.Rows.Add("", 0, 0, 0, 0, 0);
        _contentPanel.Controls.Add(_addOptionalTourButton);
        
        _removeOptionalTourButton = new Button
        {
            Text = "🗑️ حذف المحدد",
            Location = new Point(180, y),
            Size = new Size(150, 35),
            BackColor = ColorScheme.Error,
            ForeColor = Color.White,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _removeOptionalTourButton.FlatAppearance.BorderSize = 0;
        _removeOptionalTourButton.Click += (s, e) => 
        {
            if (_optionalToursGrid.SelectedRows.Count > 0)
                _optionalToursGrid.Rows.Remove(_optionalToursGrid.SelectedRows[0]);
        };
        _contentPanel.Controls.Add(_removeOptionalTourButton);
    }
    
    private void BuildStep8()
    {
        _contentPanel.Controls.Clear();
        int y = 20;
        
        AddLabel("📋 ملخص الرحلة الشامل", 20, ref y, fontSize: 14, bold: true);
        y += 10;
        
        // حفظ البيانات الحالية قبل الحساب
        SaveCurrentStepData();
        
        // ملاحظة هامة عن التسجيل التلقائي (للرحلات الجديدة فقط)
        if (!_tripId.HasValue)
        {
            var notePanel = new Panel
            {
                Location = new Point(20, y),
                Size = new Size(800, 70),
                BackColor = Color.FromArgb(255, 249, 196), // لون أصفر فاتح
                BorderStyle = BorderStyle.FixedSingle
            };
            
            var noteIcon = new Label
            {
                Text = "ℹ️",
                Font = new Font("Segoe UI Emoji", 20F),
                Location = new Point(750, 10),
                Size = new Size(40, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            
            var noteText = new Label
            {
                Text = "عند حفظ الرحلة، سيتم تلقائياً:\n• تسجيل الرحلة كمحجوزة بالكامل ومنفذة\n• إضافة إيرادات الرحلة للخزنة\n• ظهور الرحلة في تقارير ربحية الرحلات",
                Font = new Font("Cairo", 9F),
                Location = new Point(10, 10),
                Size = new Size(730, 50),
                ForeColor = Color.FromArgb(102, 60, 0)
            };
            
            notePanel.Controls.AddRange(new Control[] { noteIcon, noteText });
            _contentPanel.Controls.Add(notePanel);
            y += 80;
        }
        
        // ═══════════════════════════════════════════════════════════
        // 💰 قسم التكاليف المفصلة
        // ═══════════════════════════════════════════════════════════
        var costPanel = new Panel
        {
            Location = new Point(20, y),
            Size = new Size(800, 600), // زيادة الارتفاع من 450 إلى 600
            BackColor = Color.FromArgb(248, 249, 250),
            BorderStyle = BorderStyle.FixedSingle
        };
        
        int cy = 10;
        
        AddLabelToPanel(costPanel, "💰 التكاليف التفصيلية", 10, ref cy, fontSize: 13, bold: true);
        cy += 5;
        
        // حساب تكلفة البرنامج
        decimal programCostAdult = 0;
        decimal programCostChild = 0;
        
        if (_trip?.Programs != null)
        {
            foreach (var program in _trip.Programs.Where(p => p.BookingType == "Adult"))
            {
                programCostAdult += (program.VisitsCost * program.ParticipantsCount) + program.GuideCost;
            }
            
            foreach (var program in _trip.Programs.Where(p => p.BookingType == "Child"))
            {
                programCostChild += (program.VisitsCost * program.ParticipantsCount) + program.GuideCost;
            }
        }
        
        decimal totalProgramCost = programCostAdult + programCostChild;
        
        AddCostItemToPanel(costPanel, "📅 البرنامج اليومي (Adult)", programCostAdult, ref cy);
        AddCostItemToPanel(costPanel, "📅 البرنامج اليومي (Child)", programCostChild, ref cy);
        AddCostItemToPanel(costPanel, "📅 إجمالي البرنامج", totalProgramCost, ref cy, bold: true, color: ColorScheme.Primary);
        cy += 10;
        
        // حساب تكلفة النقل
        decimal transportationCost = 0;
        
        if (_trip?.Transportation != null)
        {
            foreach (var transport in _trip.Transportation)
            {
                transportationCost += (transport.CostPerVehicle * transport.NumberOfVehicles) 
                                    + transport.TourLeaderTip 
                                    + transport.DriverTip;
            }
        }
        
        AddCostItemToPanel(costPanel, "🚗 النقل والمواصلات", transportationCost, ref cy);
        cy += 5;
        
        // حساب تكلفة الإقامة
        decimal accommodationCost = 0;
        
        if (_trip?.Accommodations != null)
        {
            foreach (var accommodation in _trip.Accommodations)
            {
                accommodationCost += (accommodation.CostPerRoomPerNight * accommodation.NumberOfRooms * accommodation.NumberOfNights);
            }
        }
        
        AddCostItemToPanel(costPanel, "🏨 الإقامة والفنادق", accommodationCost, ref cy);
        cy += 5;
        
        // حساب المصاريف الأخرى
        decimal otherExpenses = 0;
        
        if (_trip?.Expenses != null)
        {
            otherExpenses = _trip.Expenses.Sum(e => e.Amount);
        }
        
        AddCostItemToPanel(costPanel, "💼 المصاريف الأخرى", otherExpenses, ref cy);
        cy += 15;
        
        // الإجمالي الكلي للتكاليف
        decimal totalCost = totalProgramCost + transportationCost + accommodationCost + otherExpenses;
        
        var totalCostPanel = new Panel
        {
            Location = new Point(10, cy),
            Size = new Size(770, 50),
            BackColor = Color.FromArgb(231, 76, 60),
            BorderStyle = BorderStyle.FixedSingle
        };
        
        var totalCostLabel = new Label
        {
            Text = $"💎 إجمالي التكاليف: {totalCost:N2} جنيه",
            Font = new Font("Cairo", 13F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            Size = new Size(750, 40),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(10, 5)
        };
        
        totalCostPanel.Controls.Add(totalCostLabel);
        costPanel.Controls.Add(totalCostPanel);
        cy += 70; // زيادة المسافة بعد إجمالي التكاليف
        
        // ═══════════════════════════════════════════════════════════
        // حساب الإيرادات والأرباح
        // ═══════════════════════════════════════════════════════════
        
        // خط فاصل
        var separatorLine = new Panel
        {
            Location = new Point(10, cy),
            Size = new Size(770, 2),
            BackColor = Color.FromArgb(189, 189, 189)
        };
        costPanel.Controls.Add(separatorLine);
        cy += 15;
        
        // حساب السعر/فرد (متوسط)
        int totalCapacity = _trip?.TotalCapacity ?? 0;
        decimal pricePerPerson = totalCapacity > 0 ? totalCost / totalCapacity : 0;
        
        // ✅ الحصول على هامش الربح من الخطوة الأولى
        var profitMarginNumeric = _contentPanel.Controls.Find("profitMarginNumeric", false).FirstOrDefault() as NumericUpDown;
        decimal profitMarginPercent = 20m; // القيمة الافتراضية
        
        if (profitMarginNumeric != null)
        {
            profitMarginPercent = profitMarginNumeric.Value;
        }
        else
        {
            // البحث في الـ Form بأكمله
            profitMarginPercent = FindProfitMarginFromForm() ?? 20m;
        }
        
        // حساب السعر النهائي
        decimal profitAmount = pricePerPerson * (profitMarginPercent / 100m);
        decimal finalSellingPrice = pricePerPerson + profitAmount;
        
        AddLabelToPanel(costPanel, "📊 تحليل الأسعار والربحية", 10, ref cy, fontSize: 12, bold: true);
        cy += 5;
        
        AddCostItemToPanel(costPanel, $"عدد الأفراد الإجمالي: {totalCapacity}", 0, ref cy, hideValue: true, color: Color.FromArgb(52, 73, 94));
        AddCostItemToPanel(costPanel, "التكلفة/فرد (متوسط)", pricePerPerson, ref cy, color: Color.FromArgb(231, 76, 60));
        AddCostItemToPanel(costPanel, $"هامش الربح المحدد: {profitMarginPercent}%", profitAmount, ref cy, color: Color.FromArgb(142, 68, 173));
        
        // ✅ عرض السعر النهائي المحسوب
        var pricePanel = new Panel
        {
            Location = new Point(10, cy),
            Size = new Size(770, 50),
            BackColor = Color.FromArgb(39, 174, 96),
            BorderStyle = BorderStyle.FixedSingle
        };
        
        var priceLabel = new Label
        {
            Text = $"💰 السعر النهائي للفرد: {finalSellingPrice:N2} جنيه",
            Font = new Font("Cairo", 13F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            Size = new Size(750, 40),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(10, 5)
        };
        
        pricePanel.Controls.Add(priceLabel);
        costPanel.Controls.Add(pricePanel);
        cy += 70;
        
        decimal expectedRevenue = finalSellingPrice * totalCapacity;
        decimal expectedProfit = expectedRevenue - totalCost;
        decimal profitMarginActual = totalCost > 0 ? (expectedProfit / totalCost) * 100 : 0;
        
        cy += 15; // زيادة المسافة قبل الإيرادات
        AddCostItemToPanel(costPanel, "💵 الإيرادات المتوقعة (طاقة كاملة)", expectedRevenue, ref cy, bold: true, color: Color.FromArgb(41, 128, 185));
        AddCostItemToPanel(costPanel, "💎 صافي الربح المتوقع", expectedProfit, ref cy, bold: true, color: Color.FromArgb(39, 174, 96));
        AddCostItemToPanel(costPanel, $"📈 نسبة الربح الفعلية: {profitMarginActual:F1}%", 0, ref cy, hideValue: true, bold: true, color: Color.FromArgb(142, 68, 173));
        
        _contentPanel.Controls.Add(costPanel);
        y += 620; // زيادة المسافة من 460 إلى 620
        
        // ═══════════════════════════════════════════════════════════
        // معلومات أساسية
        // ═══════════════════════════════════════════════════════════
        AddLabel("📌 المعلومات الأساسية", 20, ref y, fontSize: 11, bold: true);
        AddSummaryItem("رقم الرحلة:", _trip?.TripNumber ?? "", ref y);
        AddSummaryItem("اسم الرحلة:", _trip?.TripName ?? "", ref y);
        AddSummaryItem("الوجهة:", _trip?.Destination ?? "", ref y);
        AddSummaryItem("النوع:", _trip?.TripType.ToString() ?? "", ref y);
        AddSummaryItem("تاريخ البدء:", _trip?.StartDate.ToString("yyyy-MM-dd") ?? "", ref y);
        AddSummaryItem("تاريخ الانتهاء:", _trip?.EndDate.ToString("yyyy-MM-dd") ?? "", ref y);
        AddSummaryItem("المدة:", $"{(_trip?.EndDate.Subtract(_trip?.StartDate ?? DateTime.Now).Days ?? 0) + 1} يوم", ref y);
        AddSummaryItem("الطاقة:", $"{_trip?.TotalCapacity ?? 0} فرد", ref y);
        y += 10;
        
        // إحصائيات
        AddLabel("📊 الإحصائيات", 20, ref y, fontSize: 11, bold: true);
        AddSummaryItem("أيام البرنامج:", $"{_trip?.Programs.Count ?? 0} يوم", ref y);
        AddSummaryItem("عدد المركبات:", $"{_trip?.Transportation.Count ?? 0}", ref y);
        AddSummaryItem("عدد الفنادق:", $"{_trip?.Accommodations.Count ?? 0}", ref y);
        
        // بيانات المرشد
        var guide = _trip?.Guides.FirstOrDefault();
        if (guide != null && !string.IsNullOrWhiteSpace(guide.GuideName))
        {
            y += 10;
            AddLabel("👨‍✈️ المرشد السياحي", 20, ref y, fontSize: 11, bold: true);
            AddSummaryItem("الاسم:", guide.GuideName, ref y);
            if (!string.IsNullOrWhiteSpace(guide.Phone))
                AddSummaryItem("الهاتف:", guide.Phone, ref y);
        }
        
        AddSummaryItem("عدد المصاريف الأخرى:", $"{_trip?.Expenses.Count ?? 0}", ref y);
        AddSummaryItem("عدد الرحلات الاختيارية:", $"{_trip?.OptionalTours.Count ?? 0}", ref y);
        
        // إضافة مسافة في النهاية لضمان ظهور كل المحتوى
        y += 100;
    }
    
    // دوال مساعدة لإضافة عناصر التكلفة
    private void AddLabelToPanel(Panel panel, string text, int x, ref int y, int fontSize = 10, bool bold = false)
    {
        var label = new Label
        {
            Text = text,
            Font = new Font("Cairo", fontSize, bold ? FontStyle.Bold : FontStyle.Regular),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(x, y)
        };
        panel.Controls.Add(label);
        y += 35;
    }
    
    private void AddCostItemToPanel(Panel panel, string label, decimal amount, ref int y, bool bold = false, Color? color = null, bool hideValue = false)
    {
        var labelControl = new Label
        {
            Text = label,
            Font = new Font("Cairo", 10F, bold ? FontStyle.Bold : FontStyle.Regular),
            ForeColor = color ?? Color.FromArgb(52, 73, 94),
            AutoSize = true,
            Location = new Point(15, y)
        };
        panel.Controls.Add(labelControl);
        
        if (!hideValue)
        {
            var amountControl = new Label
            {
                Text = $"{amount:N2} جنيه",
                Font = new Font("Cairo", 10F, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = color ?? Color.FromArgb(52, 73, 94),
                AutoSize = true,
                Location = new Point(550, y),
                TextAlign = ContentAlignment.MiddleRight
            };
            panel.Controls.Add(amountControl);
        }
        
        y += 30;
    }
    
    private void NextButton_Click(object? sender, EventArgs e)
    {
        Console.WriteLine($"[NextButton] 🔄 الانتقال من الخطوة {_currentStep} إلى {_currentStep + 1}");
        
        if (!ValidateCurrentStep()) 
        {
            Console.WriteLine($"[NextButton] ❌ فشل التحقق من صحة الخطوة {_currentStep}");
            return;
        }
        
        Console.WriteLine($"[NextButton] ✅ نجح التحقق، جاري حفظ بيانات الخطوة {_currentStep}");
        SaveCurrentStepData(); // حفظ البيانات قبل الانتقال
        
        _currentStep++;
        Console.WriteLine($"[NextButton] ➡️ الانتقال للخطوة {_currentStep}");
        UpdateStep();
    }
    
    private void PreviousButton_Click(object? sender, EventArgs e)
    {
        Console.WriteLine($"[PreviousButton] 🔄 الانتقال من الخطوة {_currentStep} إلى {_currentStep - 1}");
        SaveCurrentStepData(); // حفظ البيانات قبل الانتقال
        
        _currentStep--;
        Console.WriteLine($"[PreviousButton] ⬅️ الانتقال للخطوة {_currentStep}");
        UpdateStep();
    }
    
    private void SaveCurrentStepData()
    {
        // حفظ بيانات كل خطوة في متغيرات مؤقتة
        if (_trip == null) _trip = new Trip { CreatedBy = _currentUserId };
        
        switch (_currentStep)
        {
            case 0: // المعلومات الأساسية
                _trip.TripNumber = _tripNumberBox?.Text ?? "";
                _trip.TripName = _tripNameBox?.Text ?? "";
                
                // جمع بدء وانتهاء الرحلة
                var startDestBox = _contentPanel.Controls.Find("startDestinationBox", false).FirstOrDefault() as TextBox;
                var endDestBox = _contentPanel.Controls.Find("endDestinationBox", false).FirstOrDefault() as TextBox;
                _trip.Destination = $"{startDestBox?.Text ?? ""} - {endDestBox?.Text ?? ""}";
                
                if (_tripTypeCombo?.SelectedIndex >= 0)
                    _trip.TripType = (TripType)(_tripTypeCombo.SelectedIndex + 1);
                _trip.Description = _descriptionBox?.Text ?? "";
                _trip.StartDate = _startDatePicker?.Value ?? DateTime.Now;
                _trip.EndDate = _endDatePicker?.Value ?? DateTime.Now;
                
                // ✅ حساب TotalCapacity من Adult + Child وحفظهم
                var adultControl = _contentPanel.Controls.Find("adultCountNumeric", true).FirstOrDefault() as NumericUpDown;
                var childControl = _contentPanel.Controls.Find("childCountNumeric", true).FirstOrDefault() as NumericUpDown;
                int adultCount = (int)(adultControl?.Value ?? 0);
                int childCount = (int)(childControl?.Value ?? 0);
                
                _trip.AdultCount = adultCount;  // ✅ حفظ عدد Adult
                _trip.ChildCount = childCount;  // ✅ حفظ عدد Child
                _trip.TotalCapacity = adultCount + childCount;
                
                // ✅ حفظ هامش الربح
                var profitMarginControl = _contentPanel.Controls.Find("profitMarginNumeric", true).FirstOrDefault() as NumericUpDown;
                _trip.ProfitMarginPercent = profitMarginControl?.Value ?? 20m;
                break;
                
            case 1: // البرنامج اليومي
                _trip.Programs.Clear();
                
                // حفظ بيانات ADULT
                var adultGrid = _contentPanel.Controls.Find("adultProgramGrid", false).FirstOrDefault() as DataGridView;
                Console.WriteLine($"[SaveCurrentStepData - Step 1] Adult Grid: {(adultGrid != null ? "Found" : "NOT FOUND")}");
                
                if (adultGrid != null)
                {
                    Console.WriteLine($"[SaveCurrentStepData - Step 1] Adult Grid Rows: {adultGrid.Rows.Count}");
                    
                    foreach (DataGridViewRow row in adultGrid.Rows)
                    {
                        if (row.IsNewRow) continue;
                        
                        DateTime dayDate;
                        if (!DateTime.TryParse(row.Cells["DayDate"].Value?.ToString(), out dayDate))
                            dayDate = DateTime.Now;
                        
                        var program = new TripProgram
                        {
                            DayDate = dayDate,
                            DayNumber = Convert.ToInt32(row.Cells["DayNumber"].Value ?? 1),
                            Visits = row.Cells["Visits"].Value?.ToString() ?? "",
                            VisitsCost = Convert.ToDecimal(row.Cells["VisitsCost"].Value ?? 0),
                            GuideCost = Convert.ToDecimal(row.Cells["GuideCost"].Value ?? 0),
                            ParticipantsCount = Convert.ToInt32(row.Cells["ParticipantsCount"].Value ?? 0),
                            BookingType = "Adult"
                        };
                        
                        _trip.Programs.Add(program);
                        Console.WriteLine($"[SaveCurrentStepData - Step 1] Added Adult Program: Day {program.DayNumber}, Visits: {program.Visits}");
                    }
                }
                
                // حفظ بيانات CHILD
                var childGrid = _contentPanel.Controls.Find("childProgramGrid", false).FirstOrDefault() as DataGridView;
                Console.WriteLine($"[SaveCurrentStepData - Step 1] Child Grid: {(childGrid != null ? "Found" : "NOT FOUND")}");
                
                if (childGrid != null)
                {
                    Console.WriteLine($"[SaveCurrentStepData - Step 1] Child Grid Rows: {childGrid.Rows.Count}");
                    
                    foreach (DataGridViewRow row in childGrid.Rows)
                    {
                        if (row.IsNewRow) continue;
                        
                        DateTime dayDate;
                        if (!DateTime.TryParse(row.Cells["DayDate"].Value?.ToString(), out dayDate))
                            dayDate = DateTime.Now;
                        
                        var program = new TripProgram
                        {
                            DayDate = dayDate,
                            DayNumber = Convert.ToInt32(row.Cells["DayNumber"].Value ?? 1),
                            Visits = row.Cells["Visits"].Value?.ToString() ?? "",
                            VisitsCost = Convert.ToDecimal(row.Cells["VisitsCost"].Value ?? 0),
                            GuideCost = Convert.ToDecimal(row.Cells["GuideCost"].Value ?? 0),
                            ParticipantsCount = Convert.ToInt32(row.Cells["ParticipantsCount"].Value ?? 0),
                            BookingType = "Child"
                        };
                        
                        _trip.Programs.Add(program);
                        Console.WriteLine($"[SaveCurrentStepData - Step 1] Added Child Program: Day {program.DayNumber}, Visits: {program.Visits}");
                    }
                }
                
                Console.WriteLine($"[SaveCurrentStepData - Step 1] Total Programs Saved: {_trip.Programs.Count}");
                break;
                
            case 2: // النقل
                Console.WriteLine("[SaveCurrentStepData - Case 2] 🚀 بدء حفظ بيانات النقل");
                _trip.Transportation.Clear();
                Console.WriteLine("[SaveCurrentStepData - Case 2] ✅ تم مسح بيانات النقل القديمة");
                
                if (_transportationGrid != null)
                {
                    Console.WriteLine($"[SaveCurrentStepData - Case 2] 📊 عدد الصفوف في الجدول: {_transportationGrid.Rows.Count}");
                    
                    int savedCount = 0;
                    foreach (DataGridViewRow row in _transportationGrid.Rows)
                    {
                        if (row.IsNewRow) 
                        {
                            Console.WriteLine($"[SaveCurrentStepData - Case 2] ⏭️ تخطي صف جديد");
                            continue;
                        }
                        
                        var typeText = row.Cells["Type"].Value?.ToString() ?? "أتوبيس";
                        var type = typeText switch
                        {
                            "أتوبيس" => TransportationType.Bus,
                            "ميني باص" => TransportationType.MiniBus,
                            "كوستر" => TransportationType.Coaster,
                            "هاي أس" => TransportationType.HiAce,
                            "ملاكي" => TransportationType.Car,
                            "طائرة" => TransportationType.Plane,
                            "قطار" => TransportationType.Train,
                            _ => TransportationType.Bus
                        };
                        
                        DateTime? transportDate = null;
                        if (DateTime.TryParse(row.Cells["TransportDate"].Value?.ToString(), out var dt))
                            transportDate = dt;
                        
                        // ✅ حفظ اسم المزار ورقم اليوم من الأعمدة
                        var visitName = row.Cells["VisitName"].Value?.ToString() ?? "";
                        var dayNumber = 0;
                        if (int.TryParse(row.Cells["DayNumber"].Value?.ToString(), out var dn))
                            dayNumber = dn;
                        
                        var route = row.Cells["Route"].Value?.ToString();
                        
                        // إذا كان المسار فارغ، استخدم اسم المزار
                        if (string.IsNullOrWhiteSpace(route) && !string.IsNullOrWhiteSpace(visitName))
                        {
                            route = $"نقل إلى {visitName}";
                        }
                        
                        var costPerVehicle = Convert.ToDecimal(row.Cells["CostPerVehicle"].Value ?? 0);
                        var tourLeaderTip = Convert.ToDecimal(row.Cells["TourLeaderTip"].Value ?? 0);
                        var driverTip = Convert.ToDecimal(row.Cells["DriverTip"].Value ?? 0);
                        var numberOfVehicles = Convert.ToInt32(row.Cells["NumberOfVehicles"].Value ?? 1);
                        
                        Console.WriteLine($"[SaveCurrentStepData - Case 2] ➕ حفظ: {visitName} | اليوم {dayNumber} | التكلفة: {costPerVehicle} | عدد المركبات: {numberOfVehicles} | التور ليدر: {tourLeaderTip} | السواق: {driverTip}");
                        
                        _trip.Transportation.Add(new TripTransportation
                        {
                            Type = type,
                            TransportDate = transportDate,
                            Route = route,
                            NumberOfVehicles = numberOfVehicles,
                            SeatsPerVehicle = Convert.ToInt32(row.Cells["SeatsPerVehicle"].Value ?? 50),
                            ParticipantsCount = Convert.ToInt32(row.Cells["ParticipantsCount"].Value ?? 0),
                            CostPerVehicle = costPerVehicle,
                            TourLeaderTip = tourLeaderTip,
                            DriverTip = driverTip,
                            
                            // ✅ حفظ معلومات المزار والبرنامج في الحقول الجديدة
                            VisitName = visitName,
                            ProgramDayNumber = dayNumber > 0 ? dayNumber : null
                        });
                        
                        savedCount++;
                    }
                    
                    Console.WriteLine($"[SaveCurrentStepData - Case 2] ✅ تم حفظ {savedCount} سجل نقل في _trip.Transportation");
                }
                else
                {
                    Console.WriteLine("[SaveCurrentStepData - Case 2] ⚠️ جدول النقل غير موجود!");
                }
                break;
                
            case 3: // الإقامة
                _trip.Accommodations.Clear();
                if (_accommodationGrid != null)
                {
                    foreach (DataGridViewRow row in _accommodationGrid.Rows)
                    {
                        if (row.IsNewRow) continue;
                        var typeText = row.Cells["Type"].Value?.ToString() ?? "فندق";
                        var accType = typeText switch
                        {
                            "فندق" => AccommodationType.Hotel,
                            "نايل كروز" => AccommodationType.NileCruise,
                            "منتجع" => AccommodationType.Resort,
                            "شقة فندقية" => AccommodationType.Apartment,
                            "بيت شباب" => AccommodationType.Hostel,
                            _ => AccommodationType.Hotel
                        };
                        
                        var ratingText = row.Cells["Rating"].Value?.ToString() ?? "⭐⭐⭐";
                        var rating = ratingText.Length switch
                        {
                            1 => HotelRating.OneStar,
                            2 => HotelRating.TwoStars,
                            3 => HotelRating.ThreeStars,
                            4 => HotelRating.FourStars,
                            _ => HotelRating.FiveStars
                        };
                        
                        var roomTypeText = row.Cells["RoomType"].Value?.ToString() ?? "مزدوج";
                        var roomType = roomTypeText switch
                        {
                            "فردي" => RoomType.Single,
                            "مزدوج" => RoomType.Double,
                            "ثلاثي" => RoomType.Triple,
                            "رباعي" => RoomType.Quad,
                            "جناح" => RoomType.Suite,
                            _ => RoomType.Double
                        };
                        
                        // ✅ إضافة حفظ CruiseLevel
                        var cruiseLevelText = row.Cells["CruiseLevel"].Value?.ToString();
                        CruiseLevel? cruiseLevel = null;
                        if (!string.IsNullOrEmpty(cruiseLevelText))
                        {
                            cruiseLevel = cruiseLevelText switch
                            {
                                "Standard" => CruiseLevel.Standard,
                                "Deluxe" => CruiseLevel.DeluxeLuxury,
                                "Luxury" => CruiseLevel.Luxury,
                                _ => null
                            };
                        }
                        
                        // ✅ قراءة العملة وسعر الصرف
                        var currencyText = row.Cells["Currency"].Value?.ToString() ?? "جنيه مصري";
                        var exchangeRate = Convert.ToDecimal(row.Cells["ExchangeRate"].Value ?? 1);
                        var pricePerNight = Convert.ToDecimal(row.Cells["PricePerNight"].Value ?? 0);
                        
                        // ✅ تحويل السعر للجنيه المصري
                        var pricePerNightInEGP = pricePerNight * exchangeRate;
                        
                        _trip.Accommodations.Add(new TripAccommodation
                        {
                            Type = accType,
                            HotelName = row.Cells["HotelName"].Value?.ToString() ?? "",
                            Rating = rating,
                            CruiseLevel = cruiseLevel, // ✅ حفظ المستوى
                            RoomType = roomType,
                            NumberOfRooms = Convert.ToInt32(row.Cells["NumberOfRooms"].Value ?? 1),
                            NumberOfNights = Convert.ToInt32(row.Cells["NumberOfNights"].Value ?? 1),
                            ParticipantsCount = Convert.ToInt32(row.Cells["ParticipantsCount"].Value ?? 0), // ✅ حفظ عدد الأفراد
                            Currency = currencyText, // ✅ حفظ العملة
                            ExchangeRate = exchangeRate, // ✅ حفظ سعر الصرف
                            CostPerRoomPerNight = pricePerNightInEGP, // ✅ حفظ السعر بالجنيه المصري
                            GuideCost = Convert.ToDecimal(row.Cells["GuideCost"].Value ?? 0), // ✅ حفظ تكلفة إقامة المرشد
                            MealPlan = row.Cells["MealPlan"].Value?.ToString(),
                            CheckInDate = _startDatePicker?.Value ?? DateTime.Now,
                            CheckOutDate = _endDatePicker?.Value ?? DateTime.Now
                        });
                    }
                }
                break;
                
            case 4: // المرشد السياحي
                _trip.Guides.Clear();
                if (!string.IsNullOrWhiteSpace(_guideNameBox?.Text))
                {
                    _trip.Guides.Add(new TripGuide
                    {
                        TripId = _trip.TripId,
                        GuideName = _guideNameBox.Text,
                        Phone = _guidePhoneBox?.Text,
                        Email = _guideEmailBox?.Text,
                        Languages = _guideLanguagesBox?.Text,
                        BaseFee = 0,
                        CommissionPercentage = 0,
                        CommissionAmount = 0,
                        Notes = _guideNotesBox?.Text
                    });
                }
                break;
                
            case 5: // المصاريف الأخرى
                _trip.Expenses.Clear();
                if (_expensesGrid != null)
                {
                    foreach (DataGridViewRow row in _expensesGrid.Rows)
                    {
                        if (row.IsNewRow) continue;
                        var expenseType = row.Cells["ExpenseType"].Value?.ToString() ?? "أخرى";
                        
                        _trip.Expenses.Add(new TripExpense
                        {
                            TripId = _trip.TripId,
                            ExpenseType = expenseType,
                            Description = row.Cells["Description"].Value?.ToString() ?? "",
                            Amount = Convert.ToDecimal(row.Cells["Amount"].Value ?? 0),
                            Notes = row.Cells["Notes"].Value?.ToString()
                        });
                    }
                }
                break;
                
            case 6: // الرحلات الاختيارية
                _trip.OptionalTours.Clear();
                if (_optionalToursGrid != null)
                {
                    foreach (DataGridViewRow row in _optionalToursGrid.Rows)
                    {
                        if (row.IsNewRow) continue;
                        _trip.OptionalTours.Add(new TripOptionalTour
                        {
                            TourName = row.Cells["TourName"].Value?.ToString() ?? "",
                            SellingPrice = Convert.ToDecimal(row.Cells["SellingPrice"].Value ?? 0),
                            PurchasePrice = Convert.ToDecimal(row.Cells["PurchasePrice"].Value ?? 0),
                            GuideCommission = Convert.ToDecimal(row.Cells["GuideCommission"].Value ?? 0),
                            SalesRepCommission = Convert.ToDecimal(row.Cells["SalesCommission"].Value ?? 0),
                            ParticipantsCount = Convert.ToInt32(row.Cells["ParticipantsCount"].Value ?? 0)
                        });
                    }
                }
                break;
        }
    }
    
    private void UpdateStep()
    {
        _stepLabel.Text = $"الخطوة {_currentStep + 1} من {TotalSteps}: {GetStepTitle()}";
        
        switch (_currentStep)
        {
            case 0: 
                BuildStep1(); 
                RestoreStep1Data();
                break;
            case 1: 
                BuildStep2(); 
                RestoreStep2Data();
                break;
            case 2: 
                BuildStep3(); 
                // ✅ استرجاع البيانات المحفوظة ONLY إذا كانت موجودة في _trip.Transportation
                // وليس استيراد المزارات من جديد
                if (_trip?.Transportation != null && _trip.Transportation.Any())
                {
                    Console.WriteLine("[UpdateStep - Case 2] استرجاع بيانات النقل المحفوظة");
                    RestoreStep3Data();
                }
                else
                {
                    Console.WriteLine("[UpdateStep - Case 2] استيراد المزارات من البرنامج");
                    // فقط في المرة الأولى، استورد المزارات
                    PopulateTransportationFromVisits();
                }
                break;
            case 3: 
                BuildStep4(); 
                RestoreStep4Data();
                break;
            case 4:
                BuildStep5();
                RestoreStep5Data();
                break;
            case 5:
                BuildStep6();
                RestoreStep6Data();
                break;
            case 6:
                BuildStep7();
                RestoreStep7Data();
                break;
            case 7:
                BuildStep8();
                break;
        }
        
        _previousButton.Enabled = _currentStep > 0;
        _nextButton.Visible = _currentStep < TotalSteps - 1;
        _saveButton.Visible = _currentStep == TotalSteps - 1;
    }
    
    private void RestoreStep1Data()
    {
        if (_trip == null) return;
        
        if (!string.IsNullOrEmpty(_trip.TripNumber))
            _tripNumberBox.Text = _trip.TripNumber;
        if (!string.IsNullOrEmpty(_trip.TripName))
            _tripNameBox.Text = _trip.TripName;
        
        // تقسيم الوجهة إلى بداء و انتهاء
        if (!string.IsNullOrEmpty(_trip.Destination))
        {
            var destinations = _trip.Destination.Split(new[] { " - " }, StringSplitOptions.None);
            var startDestBox = _contentPanel.Controls.Find("startDestinationBox", false).FirstOrDefault() as TextBox;
            var endDestBox = _contentPanel.Controls.Find("endDestinationBox", false).FirstOrDefault() as TextBox;
            
            if (startDestBox != null && destinations.Length > 0)
                startDestBox.Text = destinations[0];
            if (endDestBox != null && destinations.Length > 1)
                endDestBox.Text = destinations[1];
        }
        
        if (_trip.TripType != 0)
            _tripTypeCombo.SelectedIndex = (int)_trip.TripType - 1;
        if (!string.IsNullOrEmpty(_trip.Description))
            _descriptionBox.Text = _trip.Description;
        _startDatePicker.Value = _trip.StartDate != default ? _trip.StartDate : DateTime.Now;
        _endDatePicker.Value = _trip.EndDate != default ? _trip.EndDate : DateTime.Now;
        
        // ✅ استرجاع عدد Adult و Child من الحقول المحفوظة
        var adultCountControl = _contentPanel.Controls.Find("adultCountNumeric", false).FirstOrDefault() as NumericUpDown;
        var childCountControl = _contentPanel.Controls.Find("childCountNumeric", false).FirstOrDefault() as NumericUpDown;
        
        if (adultCountControl != null && childCountControl != null)
        {
            // ✅ استخدام القيم المحفوظة مباشرة
            if (_trip.AdultCount > 0 || _trip.ChildCount > 0)
            {
                adultCountControl.Value = _trip.AdultCount;
                childCountControl.Value = _trip.ChildCount;
            }
            else if (_trip.TotalCapacity > 0)
            {
                // توزيع افتراضي فقط إذا لم تكن القيم محفوظة
                adultCountControl.Value = (int)(_trip.TotalCapacity * 0.7);
                childCountControl.Value = (int)(_trip.TotalCapacity * 0.3);
            }
        }
        
        // ✅ استرجاع هامش الربح المحفوظ
        var profitMarginControl = _contentPanel.Controls.Find("profitMarginNumeric", false).FirstOrDefault() as NumericUpDown;
        if (profitMarginControl != null)
        {
            profitMarginControl.Value = _trip.ProfitMarginPercent > 0 ? _trip.ProfitMarginPercent : 20m;
        }
    }
    
    private void RestoreStep2Data()
    {
        if (_trip == null) return;
        
        // استرجاع بيانات ADULT
        var adultGrid = _contentPanel.Controls.Find("adultProgramGrid", false).FirstOrDefault() as DataGridView;
        if (adultGrid != null)
        {
            adultGrid.Rows.Clear();
            foreach (var program in _trip.Programs.Where(p => p.BookingType == "Adult").OrderBy(p => p.DayNumber))
            {
                decimal guideCostPerPerson = program.ParticipantsCount > 0 ? program.GuideCost / program.ParticipantsCount : 0;
                decimal totalCostPerPerson = guideCostPerPerson + program.VisitsCost;
                
                adultGrid.Rows.Add(
                    program.DayDate.ToString("yyyy-MM-dd"),
                    program.DayNumber,
                    program.Visits,
                    program.VisitsCost,
                    program.ParticipantsCount,
                    program.GuideCost,
                    guideCostPerPerson.ToString("N2"),
                    totalCostPerPerson.ToString("N2")
                );
            }
            
            // تحديث Total
            UpdateGridTotal(adultGrid, "Adult");
        }
        
        // استرجاع بيانات CHILD
        var childGrid = _contentPanel.Controls.Find("childProgramGrid", false).FirstOrDefault() as DataGridView;
        if (childGrid != null)
        {
            childGrid.Rows.Clear();
            foreach (var program in _trip.Programs.Where(p => p.BookingType == "Child").OrderBy(p => p.DayNumber))
            {
                decimal guideCostPerPerson = program.ParticipantsCount > 0 ? program.GuideCost / program.ParticipantsCount : 0;
                decimal totalCostPerPerson = guideCostPerPerson + program.VisitsCost;
                
                childGrid.Rows.Add(
                    program.DayDate.ToString("yyyy-MM-dd"),
                    program.DayNumber,
                    program.Visits,
                    program.VisitsCost,
                    program.ParticipantsCount,
                    program.GuideCost,
                    guideCostPerPerson.ToString("N2"),
                    totalCostPerPerson.ToString("N2")
                );
            }
            
            // تحديث Total
            UpdateGridTotal(childGrid, "Child");
        }
    }
    
    private void RestoreStep3Data()
    {
        if (_trip == null || _transportationGrid == null) 
        {
            Console.WriteLine("[RestoreStep3Data] ⚠️ تخطي - الجدول أو الرحلة غير موجودة");
            return;
        }
        
        // ✅ التحقق من وجود الأعمدة المطلوبة قبل محاولة القراءة
        if (_transportationGrid.Columns.Count == 0 || _transportationGrid.Columns["Type"] == null)
        {
            Console.WriteLine("[RestoreStep3Data] ⚠️ تخطي - الأعمدة غير موجودة في الجدول بعد!");
            return;
        }
        
        Console.WriteLine($"[RestoreStep3Data] 🔄 استرجاع بيانات النقل - عدد السجلات: {_trip.Transportation.Count}");
        
        _transportationGrid.Rows.Clear();
        foreach (var transport in _trip.Transportation)
        {
            var typeText = transport.Type switch
            {
                TransportationType.Bus => "أتوبيس",
                TransportationType.MiniBus => "ميني باص",
                TransportationType.Coaster => "كوستر",
                TransportationType.HiAce => "هاي أس",
                TransportationType.Car => "ملاكي",
                TransportationType.Plane => "طائرة",
                TransportationType.Train => "قطار",
                _ => "أتوبيس"
            };
            
            // ✅ حساب السعر/فرد الصحيح: (CostPerVehicle * NumberOfVehicles + TourLeaderTip + DriverTip) / ParticipantsCount
            decimal totalCost = (transport.CostPerVehicle * transport.NumberOfVehicles) + transport.TourLeaderTip + transport.DriverTip;
            decimal costPerPerson = transport.ParticipantsCount > 0 
                ? totalCost / transport.ParticipantsCount 
                : 0;
            
            Console.WriteLine($"[RestoreStep3Data] ➕ استرجاع: {transport.VisitName} - اليوم {transport.ProgramDayNumber}");
            
            _transportationGrid.Rows.Add(
                transport.VisitName ?? "",                    // ✅ اسم المزار (من الحقل الجديد في الـ Entity)
                transport.ProgramDayNumber ?? 0,              // ✅ رقم اليوم (من الحقل الجديد في الـ Entity)
                typeText,                                      // النوع
                transport.TransportDate?.ToString("yyyy-MM-dd") ?? "", // التاريخ
                transport.Route ?? "",                         // المسار
                transport.SeatsPerVehicle,                    // المقاعد
                transport.NumberOfVehicles,                   // عدد المركبات
                transport.ParticipantsCount,                  // عدد الأفراد
                transport.CostPerVehicle,                     // التكلفة الإجمالية
                transport.TourLeaderTip,                      // إكرامية التور ليدر
                transport.DriverTip,                          // إكرامية السواق
                costPerPerson.ToString("N2")                  // السعر/فرد
            );
        }
        
        Console.WriteLine($"[RestoreStep3Data] ✅ تم استرجاع {_transportationGrid.Rows.Count} سجل");
        
        // ✅ تحديث إجمالي النقل بعد استرجاع البيانات
        UpdateTransportationTotal();
    }
    
    private void RestoreStep4Data()
    {
        if (_trip == null || _accommodationGrid == null) return;
        
        _accommodationGrid.Rows.Clear();
        foreach (var acc in _trip.Accommodations)
        {
            var typeText = acc.Type switch
            {
                AccommodationType.Hotel => "فندق",
                AccommodationType.NileCruise => "نايل كروز",
                AccommodationType.Resort => "منتجع",
                AccommodationType.Apartment => "شقة فندقية",
                AccommodationType.Hostel => "بيت شباب",
                _ => "فندق"
            };
            
            var rating = acc.Rating.HasValue ? new string('⭐', (int)acc.Rating.Value) : "⭐⭐⭐";
            
            // ✅ استرجاع CruiseLevel
            var cruiseLevelText = acc.CruiseLevel.HasValue ? acc.CruiseLevel.Value switch
            {
                CruiseLevel.Standard => "Standard",
                CruiseLevel.DeluxeLuxury => "Deluxe",
                CruiseLevel.Luxury => "Luxury",
                _ => ""
            } : "";
            
            var roomTypeText = acc.RoomType switch
            {
                RoomType.Single => "فردي",
                RoomType.Double => "مزدوج",
                RoomType.Triple => "ثلاثي",
                RoomType.Quad => "رباعي",
                RoomType.Suite => "جناح",
                _ => "مزدوج"
            };
            
            // حساب التكلفة الإجمالية (من الـ Entity بتستخدم TotalCost اللي بيحسب كل حاجة)
            decimal totalCost = acc.TotalCost;
            
            // حساب إقامة المرشد/فرد
            decimal guideCostPerPerson = acc.ParticipantsCount > 0 ? acc.GuideCost / acc.ParticipantsCount : 0;
            
            // ✅ استرجاع العملة وسعر الصرف
            var currency = acc.Currency ?? "جنيه مصري";
            var exchangeRate = acc.ExchangeRate > 0 ? acc.ExchangeRate : 1.0m;
            
            // ✅ حساب السعر الأصلي بالعملة الأجنبية (عكس التحويل)
            var priceInOriginalCurrency = acc.CostPerRoomPerNight / exchangeRate;
            
            // Grid columns order: Date, Type, HotelName, Rating, CruiseLevel, RoomType, 
            // NumberOfRooms, NumberOfNights, ParticipantsCount, Currency, ExchangeRate, PricePerNight, GuideCost,
            // GuideCostPerPerson, MealPlan, TotalCost
            _accommodationGrid.Rows.Add(new object?[]
            {
                acc.CheckInDate.ToString("yyyy-MM-dd"),  // Date (CheckInDate)
                typeText,                                 // Type
                acc.HotelName,                           // HotelName
                rating,                                   // Rating
                cruiseLevelText,                         // ✅ CruiseLevel
                roomTypeText,                            // RoomType
                acc.NumberOfRooms,                       // NumberOfRooms
                acc.NumberOfNights,                      // NumberOfNights
                acc.ParticipantsCount,                   // ✅ ParticipantsCount
                currency,                                // ✅ Currency
                exchangeRate.ToString("N2"),            // ✅ ExchangeRate
                priceInOriginalCurrency.ToString("N2"), // PricePerNight (in original currency)
                acc.GuideCost,                           // ✅ GuideCost
                guideCostPerPerson.ToString("N2"),       // GuideCostPerPerson (calculated)
                acc.MealPlan ?? "BB",                    // MealPlan
                totalCost.ToString("N2")                 // TotalCost (calculated)
            });
        }
    }
    
    private void RestoreStep5Data()
    {
        if (_trip == null) return;
        
        var guide = _trip.Guides.FirstOrDefault();
        if (guide != null)
        {
            if (_guideNameBox != null) _guideNameBox.Text = guide.GuideName ?? "";
            if (_guidePhoneBox != null) _guidePhoneBox.Text = guide.Phone ?? "";
            if (_guideEmailBox != null) _guideEmailBox.Text = guide.Email ?? "";
            if (_guideLanguagesBox != null) _guideLanguagesBox.Text = guide.Languages ?? "";
            if (_guideNotesBox != null) _guideNotesBox.Text = guide.Notes ?? "";
        }
    }
    
    private void RestoreStep6Data()
    {
        if (_trip == null || _expensesGrid == null) return;
        
        _expensesGrid.Rows.Clear();
        foreach (var expense in _trip.Expenses)
        {
            _expensesGrid.Rows.Add(new object?[]
            {
                expense.ExpenseType,
                expense.Description,
                expense.Amount,
                expense.Notes
            });
        }
    }
    
    private void RestoreStep7Data()
    {
        if (_trip == null || _optionalToursGrid == null) return;
        
        _optionalToursGrid.Rows.Clear();
        foreach (var tour in _trip.OptionalTours)
        {
            _optionalToursGrid.Rows.Add(new object?[]
            {
                tour.TourName,
                tour.SellingPrice,
                tour.PurchasePrice,
                tour.GuideCommission,
                tour.SalesRepCommission,
                tour.ParticipantsCount
            });
        }
    }
    
    private string GetStepTitle()
    {
        return _currentStep switch
        {
            0 => "المعلومات الأساسية",
            1 => "البرنامج والمزارات",
            2 => "النقل والمواصلات",
            3 => "الإقامة والفنادق",
            4 => "المرشد السياحي",
            5 => "المصاريف الأخرى",
            6 => "الرحلات الاختيارية",
            7 => "المراجعة النهائية",
            _ => ""
        };
    }
    
    private bool ValidateCurrentStep()
    {
        if (_currentStep == 0)
        {
            if (string.IsNullOrWhiteSpace(_tripNameBox.Text))
            {
                MessageBox.Show("اسم الرحلة مطلوب", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            var startDestBox = _contentPanel.Controls.Find("startDestinationBox", false).FirstOrDefault() as TextBox;
            if (startDestBox != null && string.IsNullOrWhiteSpace(startDestBox.Text))
            {
                MessageBox.Show("بدء الرحلة مطلوب", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            var endDestBox = _contentPanel.Controls.Find("endDestinationBox", false).FirstOrDefault() as TextBox;
            if (endDestBox != null && string.IsNullOrWhiteSpace(endDestBox.Text))
            {
                MessageBox.Show("انتهاء الرحلة مطلوب", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            if (_tripTypeCombo.SelectedIndex < 0)
            {
                MessageBox.Show("نوع الرحلة مطلوب", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (_startDatePicker.Value >= _endDatePicker.Value)
            {
                MessageBox.Show("تاريخ انتهاء الرحلة يجب أن يكون بعد تاريخ بدء الرحلة", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }
        return true;
    }
    
    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        try
        {
            this.Cursor = Cursors.WaitCursor;
            
            Console.WriteLine("[SaveButton] 🚀 بدء عملية الحفظ النهائي");
            
            // ✅ حفظ بيانات الخطوة الحالية قبل البدء في عملية الحفظ
            SaveCurrentStepData();
            
            Console.WriteLine($"[SaveButton] 📊 إحصائيات البيانات المحفوظة:");
            Console.WriteLine($"  - Programs: {_trip?.Programs.Count ?? 0}");
            Console.WriteLine($"  - Transportation: {_trip?.Transportation.Count ?? 0}");
            Console.WriteLine($"  - Accommodations: {_trip?.Accommodations.Count ?? 0}");
            Console.WriteLine($"  - Guides: {_trip?.Guides.Count ?? 0}");
            Console.WriteLine($"  - Expenses: {_trip?.Expenses.Count ?? 0}");
            Console.WriteLine($"  - OptionalTours: {_trip?.OptionalTours.Count ?? 0}");
            
            // ✅ طباعة تفاصيل بيانات النقل
            if (_trip?.Transportation != null && _trip.Transportation.Any())
            {
                Console.WriteLine($"[SaveButton] 🚗 تفاصيل النقل ({_trip.Transportation.Count} سجل):");
                foreach (var transport in _trip.Transportation)
                {
                    Console.WriteLine($"  - {transport.VisitName} | يوم {transport.ProgramDayNumber} | {transport.Type} | تكلفة: {transport.CostPerVehicle} | عدد: {transport.NumberOfVehicles}");
                }
            }
            else
            {
                Console.WriteLine("[SaveButton] ⚠️ لا توجد بيانات نقل محفوظة!");
            }
            
            // ✅ التحقق من أن الرحلة غير مقفولة (في حالة التعديل)
            if (_tripId.HasValue)
            {
                var existingTrip = await _tripService.GetTripByIdAsync(_tripId.Value, false);
                if (existingTrip != null && existingTrip.IsLockedForTrips)
                {
                    MessageBox.Show(
                        "⚠️ لا يمكن تعديل هذه الرحلة!\n\n" +
                        "الرحلة مقفولة من قبل قسم الحجوزات.\n" +
                        "يجب فتح الرحلة من قسم الحجوزات أولاً لتتمكن من تعديلها.",
                        "رحلة مقفولة",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    
                    this.Cursor = Cursors.Default;
                    return;
                }
            }
            
            // ✅ البيانات الأساسية محفوظة بالفعل في _trip من SaveCurrentStepData()
            // بس نتأكد من حفظ البيانات اللي ممكن تكون اتغيرت
            
            // ✅ حساب التكاليف الإجمالية من جميع المصادر
            _trip.CalculateTotalCost();
            
            Console.WriteLine($"[SaveButton] 💰 التكلفة الإجمالية بعد الحساب: {_trip.TotalCost:N2}");
            
            // ✅ حساب السعر للفرد تلقائياً بناءً على التكلفة وهامش الربح
            if (_trip.TotalCapacity > 0)
            {
                // حساب التكلفة للفرد
                decimal costPerPerson = _trip.TotalCost / _trip.TotalCapacity;
                
                // إضافة هامش الربح
                decimal profitAmount = costPerPerson * (_trip.ProfitMarginPercent / 100m);
                
                // السعر النهائي للفرد
                _trip.SellingPricePerPerson = costPerPerson + profitAmount;
                
                Console.WriteLine($"[SaveButton] 💵 السعر للفرد: {_trip.SellingPricePerPerson:N2} (تكلفة: {costPerPerson:N2} + هامش ربح {_trip.ProfitMarginPercent}%: {profitAmount:N2})");
            }
            else
            {
                _trip.SellingPricePerPerson = 0;
                Console.WriteLine("[SaveButton] ⚠️ الطاقة = 0، السعر للفرد = 0");
            }
            
            _trip.UpdatedBy = _currentUserId;
            _trip.CurrencyId = 1; // جنيه مصري (ID الصحيح من قاعدة البيانات)
            _trip.ExchangeRate = 1.0m;
            
            // حفظ في قاعدة البيانات
            if (_tripId.HasValue)
            {
                Console.WriteLine($"[SaveButton] 📝 تحديث الرحلة في قاعدة البيانات (TripId: {_tripId.Value})...");
                await _tripService.UpdateTripAsync(_trip);
                Console.WriteLine("[SaveButton] ✅ تم التحديث بنجاح");
                MessageBox.Show("✅ تم تحديث الرحلة بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                Console.WriteLine("[SaveButton] ➕ إضافة رحلة جديدة في قاعدة البيانات...");
                await _tripService.CreateTripAsync(_trip);
                Console.WriteLine("[SaveButton] ✅ تم الإضافة بنجاح");
                MessageBox.Show("✅ تم إضافة الرحلة بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SaveButton] ❌ خطأ في الحفظ: {ex.Message}");
            Console.WriteLine($"[SaveButton] Stack Trace: {ex.StackTrace}");
            
            var errorMessage = $"❌ خطأ في حفظ الرحلة:\n{ex.Message}";
            
            if (ex.InnerException != null)
            {
                errorMessage += $"\n\nتفاصيل إضافية:\n{ex.InnerException.Message}";
                Console.WriteLine($"[SaveButton] Inner Exception: {ex.InnerException.Message}");
            }
            
            MessageBox.Show(errorMessage, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    
    // CalculateProfit() - تم حذف الدالة لعدم توفر المتغيرات المطلوبة
    
    // UpdateTotalCostInEGP() - تم حذف الدالة لعدم توفر المتغيرات المطلوبة
    
    private void PopulateFields()
    {
        _tripNumberBox.Text = _trip.TripNumber;
        _tripNameBox.Text = _trip.TripName;
        
        // تقسيم الوجهة إلى بداء و انتهاء
        var destinations = _trip.Destination?.Split(new[] { " - " }, StringSplitOptions.None) ?? new string[] { "", "" };
        var startDestBox = _contentPanel.Controls.Find("startDestinationBox", false).FirstOrDefault() as TextBox;
        var endDestBox = _contentPanel.Controls.Find("endDestinationBox", false).FirstOrDefault() as TextBox;
        if (startDestBox != null) startDestBox.Text = destinations.Length > 0 ? destinations[0] : "";
        if (endDestBox != null) endDestBox.Text = destinations.Length > 1 ? destinations[1] : "";
        
        _tripTypeCombo.SelectedIndex = (int)_trip.TripType - 1;
        _descriptionBox.Text = _trip.Description;
        _startDatePicker.Value = _trip.StartDate;
        _endDatePicker.Value = _trip.EndDate;
        
        // توزيع الطاقة على ADULT (70%) و CHILD (30%)
        var adultCountControl = _contentPanel.Controls.Find("adultCountNumeric", false).FirstOrDefault() as NumericUpDown;
        var childCountControl = _contentPanel.Controls.Find("childCountNumeric", false).FirstOrDefault() as NumericUpDown;
        if (adultCountControl != null) adultCountControl.Value = (decimal)(_trip.TotalCapacity * 0.7);
        if (childCountControl != null) childCountControl.Value = (decimal)(_trip.TotalCapacity * 0.3);
        
        // تم حذف تعبئة البيانات المالية لعدم توفر المتغيرات
        
        // تعبئة البرنامج - ADULT و CHILD منفصلين
        var adultGrid = _contentPanel.Controls.Find("adultProgramGrid", false).FirstOrDefault() as DataGridView;
        var childGrid = _contentPanel.Controls.Find("childProgramGrid", false).FirstOrDefault() as DataGridView;
        
        if (_trip.Programs.Any())
        {
            // فصل البيانات حسب BookingType
            var adultPrograms = _trip.Programs.Where(p => p.BookingType == "Adult").OrderBy(p => p.DayNumber).ToList();
            var childPrograms = _trip.Programs.Where(p => p.BookingType == "Child").OrderBy(p => p.DayNumber).ToList();
            
            // تعبئة ADULT Grid
            if (adultGrid != null && adultPrograms.Any())
            {
                foreach (var program in adultPrograms)
                {
                    decimal costPerPerson = program.ParticipantsCount > 0 
                        ? (program.VisitsCost + (program.GuideCost / program.ParticipantsCount))
                        : program.VisitsCost;
                    
                    adultGrid.Rows.Add(
                        program.DayDate.ToString("yyyy-MM-dd"),
                        program.DayNumber,
                        program.Visits,
                        program.VisitsCost,
                        program.ParticipantsCount,
                        program.GuideCost,
                        (program.ParticipantsCount > 0 ? program.GuideCost / program.ParticipantsCount : 0).ToString("N2"),
                        costPerPerson.ToString("N2")
                    );
                }
            }
            
            // تعبئة CHILD Grid
            if (childGrid != null && childPrograms.Any())
            {
                foreach (var program in childPrograms)
                {
                    decimal costPerPerson = program.ParticipantsCount > 0 
                        ? (program.VisitsCost + (program.GuideCost / program.ParticipantsCount))
                        : program.VisitsCost;
                    
                    childGrid.Rows.Add(
                        program.DayDate.ToString("yyyy-MM-dd"),
                        program.DayNumber,
                        program.Visits,
                        program.VisitsCost,
                        program.ParticipantsCount,
                        program.GuideCost,
                        (program.ParticipantsCount > 0 ? program.GuideCost / program.ParticipantsCount : 0).ToString("N2"),
                        costPerPerson.ToString("N2")
                    );
                }
            }
        }
        
        // تعبئة النقل
        if (_transportationGrid != null && _trip.Transportation.Any())
        {
            foreach (var transport in _trip.Transportation)
            {
                var typeText = transport.Type switch
                {
                    TransportationType.Bus => "أتوبيس",
                    TransportationType.MiniBus => "ميني باص",
                    TransportationType.Coaster => "كوستر",
                    TransportationType.HiAce => "هاي أس",
                    TransportationType.Car => "ملاكي",
                    TransportationType.Plane => "طائرة",
                    TransportationType.Train => "قطار",
                    _ => "أتوبيس"
                };
                
                // حساب السعر/فرد مع الإكراميات
                decimal totalCost = transport.CostPerVehicle + transport.TourLeaderTip + transport.DriverTip;
                decimal costPerPerson = transport.ParticipantsCount > 0 
                    ? totalCost / transport.ParticipantsCount 
                    : 0;
                
                _transportationGrid.Rows.Add(
                    "",                                           // اسم المزار (فارغ - سيتم ملؤه من البرنامج)
                    0,                                             // رقم اليوم (فارغ - سيتم ملؤه من البرنامج)
                    typeText,                                      // النوع
                    transport.TransportDate?.ToString("yyyy-MM-dd") ?? "", // التاريخ
                    transport.Route ?? "",                         // المسار
                    transport.SeatsPerVehicle,                    // المقاعد
                    transport.NumberOfVehicles,                   // ✅ عدد المركبات
                    transport.ParticipantsCount,                  // عدد الأفراد
                    transport.CostPerVehicle,                     // التكلفة الإجمالية
                    transport.TourLeaderTip,                      // إكرامية التور ليدر
                    transport.DriverTip,                          // إكرامية السواق
                    costPerPerson.ToString("N2")                  // السعر/فرد
                );
            }
        }
        
        // تعبئة الإقامة
        if (_accommodationGrid != null && _trip.Accommodations.Any())
        {
            foreach (var acc in _trip.Accommodations)
            {
                var typeText = acc.Type switch
                {
                    AccommodationType.Hotel => "فندق",
                    AccommodationType.NileCruise => "نايل كروز",
                    AccommodationType.Resort => "منتجع",
                    AccommodationType.Apartment => "شقة فندقية",
                    AccommodationType.Hostel => "بيت شباب",
                    _ => "فندق"
                };
                
                var rating = acc.Rating.HasValue ? new string('⭐', (int)acc.Rating.Value) : "⭐⭐⭐";
                
                var roomTypeText = acc.RoomType switch
                {
                    RoomType.Single => "فردي",
                    RoomType.Double => "مزدوج",
                    RoomType.Triple => "ثلاثي",
                    RoomType.Quad => "رباعي",
                    RoomType.Suite => "جناح",
                    _ => "مزدوج"
                };
                
                _accommodationGrid.Rows.Add(new object?[]
                {
                    typeText,
                    acc.HotelName,
                    rating,
                    roomTypeText,
                    acc.NumberOfRooms,
                    acc.NumberOfNights,
                    acc.CostPerRoomPerNight,
                    acc.MealPlan
                });
            }
        }
    }
    
    // Helper Methods
    private void AddLabel(string text, int x, ref int y, int fontSize = 10, bool bold = false)
    {
        Label label = new Label
        {
            Text = text,
            Font = new Font("Cairo", fontSize, bold ? FontStyle.Bold : FontStyle.Regular),
            AutoSize = true,
            Location = new Point(x, y)
        };
        _contentPanel.Controls.Add(label);
        y += 35;
    }
    
    private TextBox AddTextBox(int x, ref int y, bool enabled = true, bool multiline = false)
    {
        TextBox textBox = new TextBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(400, multiline ? 80 : 30),
            Location = new Point(x, y),
            Enabled = enabled,
            Multiline = multiline
        };
        _contentPanel.Controls.Add(textBox);
        y += multiline ? 110 : 50;
        return textBox;
    }
    
    private ComboBox AddComboBox(int x, ref int y)
    {
        ComboBox comboBox = new ComboBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(400, 30),
            Location = new Point(x, y),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _contentPanel.Controls.Add(comboBox);
        y += 50;
        return comboBox;
    }
    
    private void AddSummaryItem(string label, string value, ref int y)
    {
        Label lblLabel = new Label
        {
            Text = label,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, y)
        };
        _contentPanel.Controls.Add(lblLabel);
        
        Label lblValue = new Label
        {
            Text = value,
            Font = new Font("Cairo", 10F),
            AutoSize = true,
            Location = new Point(200, y),
            ForeColor = Color.FromArgb(52, 73, 94)
        };
        _contentPanel.Controls.Add(lblValue);
        y += 35;
    }
    
    // ✅ دالة للبحث عن هامش الربح من النموذج
    private decimal? FindProfitMarginFromForm()
    {
        // البحث في كل الـ Controls
        foreach (Control ctrl in this.Controls)
        {
            if (ctrl is Panel panel)
            {
                var numeric = FindControlRecursive(panel, "profitMarginNumeric") as NumericUpDown;
                if (numeric != null)
                {
                    return numeric.Value;
                }
            }
        }
        return null;
    }
    
    private Control? FindControlRecursive(Control parent, string name)
    {
        if (parent.Name == name) return parent;
        
        foreach (Control child in parent.Controls)
        {
            var found = FindControlRecursive(child, name);
            if (found != null) return found;
        }
        
        return null;
    }
    
    private void AccommodationGrid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || _accommodationGrid == null) return;
        
        var row = _accommodationGrid.Rows[e.RowIndex];
        
        // إظهار/إخفاء عمود مستوى النايل كروز
        if (e.ColumnIndex == _accommodationGrid.Columns["Type"]!.Index)
        {
            var type = row.Cells["Type"].Value?.ToString();
            if (type == "نايل كروز")
            {
                // مسح قيمة مستوى النايل كروز إذا كانت غير محددة
                if (row.Cells["CruiseLevel"].Value == null || row.Cells["CruiseLevel"].Value?.ToString() == "")
                {
                    row.Cells["CruiseLevel"].Value = "Standard";
                }
            }
            else
            {
                // مسح قيمة مستوى النايل كروز للأنواع الأخرى
                row.Cells["CruiseLevel"].Value = "";
            }
        }
        
        // تحديث سعر الصرف تلقائياً عند تغيير العملة
        if (e.ColumnIndex == _accommodationGrid.Columns["Currency"]!.Index)
        {
            var currency = row.Cells["Currency"].Value?.ToString();
            decimal defaultExchangeRate = currency switch
            {
                "جنيه مصري" => 1.0m,
                "دولار" => 50.0m,      // سعر تقريبي - يمكن تحديثه
                "جنيه استرليني" => 62.0m,  // سعر تقريبي - يمكن تحديثه
                _ => 1.0m
            };
            
            row.Cells["ExchangeRate"].Value = defaultExchangeRate.ToString("N2");
        }
        
        // حساب التكاليف عند تغيير أي من القيم
        var columnsToWatch = new[] { "NumberOfRooms", "NumberOfNights", "ParticipantsCount", "PricePerNight", "GuideCost", "Currency", "ExchangeRate" };
        var columnName = _accommodationGrid.Columns[e.ColumnIndex].Name;
        
        if (columnsToWatch.Contains(columnName))
        {
            try
            {
                var numberOfRooms = decimal.TryParse(row.Cells["NumberOfRooms"].Value?.ToString(), out var nr) ? nr : 1;
                var numberOfNights = decimal.TryParse(row.Cells["NumberOfNights"].Value?.ToString(), out var nn) ? nn : 1;
                var participantsCount = decimal.TryParse(row.Cells["ParticipantsCount"].Value?.ToString(), out var pc) && pc > 0 ? pc : 1;
                var pricePerNight = decimal.TryParse(row.Cells["PricePerNight"].Value?.ToString(), out var ppn) ? ppn : 0;
                var guideCost = decimal.TryParse(row.Cells["GuideCost"].Value?.ToString(), out var gc) ? gc : 0;
                var exchangeRate = decimal.TryParse(row.Cells["ExchangeRate"].Value?.ToString(), out var er) ? er : 1;
                
                // حساب إقامة المرشد/فرد
                var guideCostPerPerson = guideCost / participantsCount;
                row.Cells["GuideCostPerPerson"].Value = guideCostPerPerson.ToString("N2");
                
                // حساب التكلفة الإجمالية بالعملة الأجنبية
                var totalCostInForeignCurrency = (numberOfRooms * numberOfNights * pricePerNight) + guideCost;
                
                // تحويل التكلفة للجنيه المصري
                var totalCostInEGP = totalCostInForeignCurrency * exchangeRate;
                
                row.Cells["TotalCost"].Value = totalCostInEGP.ToString("N2");
            }
            catch (Exception ex)
            {
                // في حالة وجود خطأ في الحسابات
                System.Diagnostics.Debug.WriteLine($"Error calculating accommodation costs: {ex.Message}");
            }
        }
    }
    
    private void TransportationGrid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || _transportationGrid == null) return;
        
        var row = _transportationGrid.Rows[e.RowIndex];
        
        // تحديث عدد المقاعد تلقائياً عند تغيير نوع السيارة
        if (e.ColumnIndex == _transportationGrid.Columns["Type"]!.Index)
        {
            var type = row.Cells["Type"].Value?.ToString();
            int defaultSeats = type switch
            {
                "أتوبيس" => 50,
                "ميني باص" => 14,
                "كوستر" => 26,
                "هاي أس" => 14,
                "ملاكي" => 4,
                "طائرة" => 180,
                "قطار" => 200,
                _ => 50
            };
            
            row.Cells["SeatsPerVehicle"].Value = defaultSeats;
        }
        
        // حساب السعر/فرد عند تغيير: التكلفة، عدد الأفراد، إكرامية التور ليدر، أو إكرامية السواق
        var columnsToWatch = new[] { "CostPerVehicle", "NumberOfVehicles", "ParticipantsCount", "TourLeaderTip", "DriverTip" };
        var columnName = _transportationGrid.Columns[e.ColumnIndex].Name;
        
        if (columnsToWatch.Contains(columnName))
        {
            try
            {
                var costPerVehicle = decimal.TryParse(row.Cells["CostPerVehicle"].Value?.ToString(), out var cpv) ? cpv : 0;
                var participants = int.TryParse(row.Cells["ParticipantsCount"].Value?.ToString(), out var pc) && pc > 0 ? pc : 1;
                var tourLeaderTip = decimal.TryParse(row.Cells["TourLeaderTip"].Value?.ToString(), out var tlt) ? tlt : 0;
                var driverTip = decimal.TryParse(row.Cells["DriverTip"].Value?.ToString(), out var dt) ? dt : 0;
                
                // السعر/فرد = (التكلفة الإجمالية + إكرامية التور ليدر + إكرامية السواق) ÷ عدد الأفراد
                var totalCost = costPerVehicle + tourLeaderTip + driverTip;
                var costPerPerson = totalCost / participants;
                
                row.Cells["CostPerPerson"].Value = costPerPerson.ToString("N2");
                
                // ✅ تحديث الإجمالي
                UpdateTransportationTotal();
            }
            catch (Exception ex)
            {
                // في حالة وجود خطأ في الحسابات
                System.Diagnostics.Debug.WriteLine($"Error calculating transportation costs: {ex.Message}");
            }
        }
    }
    
/// <summary>
    /// ✨ نسخة محسّنة: استيراد المزارات من الخطوة 2 (البرنامج اليومي) لتسعير النقل لكل مزار
    /// التحسينات:
    /// 1. حفظ بيانات الخطوة الحالية قبل الاستيراد
    /// 2. استخدام _trip.Programs كمصدر أساسي (أكثر موثوقية)
    /// 3. استخراج المزارات الفردية من كل يوم
    /// 4. تجنب التكرار باستخدام HashSet
    /// 5. عرض رسائل واضحة للمستخدم
    /// </summary>
/// <summary>
    /// ✨ نسخة محسّنة: استيراد المزارات من الخطوة 2 (البرنامج اليومي) لتسعير النقل لكل مزار
    /// التحسينات:
    /// 1. حفظ بيانات الخطوة الحالية قبل الاستيراد
    /// 2. استخدام _trip.Programs كمصدر أساسي (أكثر موثوقية)
    /// 3. استخراج المزارات الفردية من كل يوم
    /// 4. تجنب التكرار باستخدام HashSet
    /// 5. عرض رسائل واضحة للمستخدم
    /// </summary>
    private void PopulateTransportationFromVisits()
    {
        if (_transportationGrid == null) return;
        
        try
        {
            Console.WriteLine("[PopulateTransportationFromVisits] 🚀 بدء استيراد المزارات...");
            
            // ═══════════════════════════════════════════════════════════
            // 1️⃣ حفظ بيانات الخطوة الحالية أولاً لتحديث _trip.Programs
            // ═══════════════════════════════════════════════════════════
            SaveCurrentStepData();
            Console.WriteLine($"[PopulateTransportationFromVisits] ✅ عدد Programs بعد الحفظ: {_trip?.Programs.Count ?? 0}");
            
            // ═══════════════════════════════════════════════════════════
            // 2️⃣ حفظ البيانات الحالية للنقل (في حالة التحديث/التحديث)
            // ═══════════════════════════════════════════════════════════
            var existingTransportData = new Dictionary<string, DataGridViewRow>();
            foreach (DataGridViewRow row in _transportationGrid.Rows)
            {
                if (!row.IsNewRow)
                {
                    var visitName = row.Cells["VisitName"].Value?.ToString();
                    var dayNumber = row.Cells["DayNumber"].Value?.ToString();
                    if (!string.IsNullOrEmpty(visitName) && !string.IsNullOrEmpty(dayNumber))
                    {
                        var key = $"{dayNumber}:{visitName}";
                        existingTransportData[key] = row;
                        Console.WriteLine($"[PopulateTransportationFromVisits] 💾 حفظ بيانات موجودة: {key}");
                    }
                }
            }
            
            // ═══════════════════════════════════════════════════════════
            // 3️⃣ مسح الجدول للتجهيز للبيانات الجديدة
            // ═══════════════════════════════════════════════════════════
            _transportationGrid.Rows.Clear();
            
            // ═══════════════════════════════════════════════════════════
            // 4️⃣ استخدام البيانات من _trip.Programs مباشرة (المصدر الموثوق)
            // ═══════════════════════════════════════════════════════════
            if (_trip?.Programs != null && _trip.Programs.Any())
            {
                Console.WriteLine($"[PopulateTransportationFromVisits] 📊 استخدام Programs من _trip: {_trip.Programs.Count} يوم");
                
                var visitsSet = new HashSet<string>(); // لتجنب التكرار
                int totalVisitsAdded = 0;
                
                foreach (var program in _trip.Programs.OrderBy(p => p.DayNumber))
                {
                    if (string.IsNullOrWhiteSpace(program.Visits))
                    {
                        Console.WriteLine($"[PopulateTransportationFromVisits] ⚠️ اليوم {program.DayNumber} لا يحتوي على مزارات");
                        continue;
                    }
                    
                    var dayNumber = program.DayNumber.ToString();
                    var dayDate = program.DayDate;
                    
                    Console.WriteLine($"[PopulateTransportationFromVisits] 📅 معالجة اليوم {dayNumber}: {program.Visits}");
                    
                    // ✨ تقسيم المزارات المتعددة (فواصل، أسطر جديدة، فاصلة منقوطة)
                    var visitsList = program.Visits.Split(new[] { ',', '\n', '\r', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (var visit in visitsList)
                    {
                        var visitName = visit.Trim();
                        if (string.IsNullOrWhiteSpace(visitName)) continue;
                        
                        var key = $"{dayNumber}:{visitName}";
                        
                        // تجنب التكرار
                        if (visitsSet.Contains(key))
                        {
                            Console.WriteLine($"[PopulateTransportationFromVisits] ⏭️ تخطي المزار المكرر: {visitName}");
                            continue;
                        }
                        visitsSet.Add(key);
                        
                        Console.WriteLine($"[PopulateTransportationFromVisits] ✨ معالجة مزار: {visitName} - اليوم {dayNumber}");
                        
                        // التحقق من وجود بيانات نقل سابقة لهذا المزار
                        if (existingTransportData.TryGetValue(key, out var existingRow))
                        {
                            Console.WriteLine($"[PopulateTransportationFromVisits] 🔄 استعادة بيانات موجودة لـ {visitName}");
                            
                            // استخدام try-catch للتعامل مع أي مشاكل في قراءة القيم
                            try
                            {
                                var typeValue = existingRow.Cells.Cast<DataGridViewCell>().Any(c => c.OwningColumn.Name == "Type") && existingRow.Cells["Type"].Value != null
                                    ? existingRow.Cells["Type"].Value.ToString()
                                    : "أتوبيس";
                                
                                _transportationGrid.Rows.Add(
                                    visitName,                                     // اسم المزار
                                    dayNumber,                                     // رقم اليوم
                                    typeValue,                                     // النوع
                                    existingRow.Cells["TransportDate"].Value ?? dayDate.ToString("yyyy-MM-dd"),
                                    existingRow.Cells["Route"].Value ?? $"نقل إلى {visitName}",
                                    existingRow.Cells["VehicleModel"].Value,
                                    existingRow.Cells["SeatsPerVehicle"].Value,
                                    existingRow.Cells["NumberOfVehicles"].Value,
                                    existingRow.Cells["ParticipantsCount"].Value,
                                    existingRow.Cells["CostPerVehicle"].Value,
                                    existingRow.Cells["TourLeaderTip"].Value,
                                    existingRow.Cells["DriverTip"].Value,
                                    existingRow.Cells["CostPerPerson"].Value
                                );
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[PopulateTransportationFromVisits] ⚠️ خطأ في استعادة البيانات: {ex.Message}");
                                // في حالة الفشل، نضيف صف جديد بقيم افتراضية
                                _transportationGrid.Rows.Add(
                                    visitName,                              // اسم المزار
                                    dayNumber,                              // رقم اليوم
                                    "أتوبيس",                              // النوع الافتراضي
                                    dayDate.ToString("yyyy-MM-dd"),        // تاريخ اليوم
                                    $"نقل إلى {visitName}",               // المسار
                                    50,                                    // عدد المقاعد
                                    1,                                     // عدد المركبات
                                    program.ParticipantsCount,             // عدد الأفراد
                                    0,                                     // التكلفة
                                    0,                                     // إكرامية التور ليدر
                                    0,                                     // إكرامية السواق
                                    "0.00"                                 // السعر/فرد
                                );
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[PopulateTransportationFromVisits] ✅ إنشاء صف جديد لـ {visitName}");
                            
                            // إضافة صف جديد بقيم افتراضية ذكية
                            _transportationGrid.Rows.Add(
                                visitName,                              // اسم المزار
                                dayNumber,                              // رقم اليوم
                                "أتوبيس",                              // النوع الافتراضي
                                dayDate.ToString("yyyy-MM-dd"),        // تاريخ اليوم
                                $"نقل إلى {visitName}",               // المسار (افتراضي)
                                50,                                    // عدد المقاعد (افتراضي للأتوبيس)
                                1,                                     // عدد المركبات (افتراضي)
                                program.ParticipantsCount,             // ✨ عدد الأفراد من البرنامج
                                0,                                     // التكلفة الإجمالية (فارغ للتعبئة)
                                0,                                     // إكرامية التور ليدر (فارغ)
                                0,                                     // إكرامية السواق (فارغ)
                                "0.00"                                 // السعر/فرد (محسوب)
                            );
                        }
                        
                        totalVisitsAdded++;
                    }
                }
                
                // ═══════════════════════════════════════════════════════════
                // 5️⃣ عرض رسالة نجاح للمستخدم
                // ═══════════════════════════════════════════════════════════
                Console.WriteLine($"[PopulateTransportationFromVisits] ✅ تم استيراد {totalVisitsAdded} مزار للنقل");
                
                if (totalVisitsAdded > 0)
                {
                    MessageBox.Show(
                        $"✅ تم استيراد {totalVisitsAdded} مزار من البرنامج اليومي بنجاح!\n\n" +
                        "📍 يمكنك الآن:\n" +
                        "• تحديد نوع المركبة (أتوبيس، ميني باص، إلخ)\n" +
                        "• إدخال سعر المواصلة لكل مزار\n" +
                        "• إضافة إكراميات التور ليدر والسواق\n" +
                        "• تعديل عدد المركبات حسب الحاجة",
                        "✨ نجح الاستيراد",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "⚠️ لم يتم العثور على أي مزارات في البرنامج اليومي!\n\n" +
                        "الرجاء التأكد من:\n" +
                        "• إضافة المزارات في الخطوة 2 (البرنامج اليومي)\n" +
                        "• كتابة أسماء المزارات في عمود 'المزارات'",
                        "⚠️ تنبيه",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            else
            {
                // ═══════════════════════════════════════════════════════════
                // 6️⃣ في حالة عدم وجود Programs - استخدام Fallback من الجداول
                // ═══════════════════════════════════════════════════════════
                Console.WriteLine("[PopulateTransportationFromVisits] ⚠️ لا توجد Programs في _trip - محاولة Fallback من الجداول");
                
                var adultGrid = _contentPanel.Controls.Find("adultProgramGrid", false).FirstOrDefault() as DataGridView;
                var childGrid = _contentPanel.Controls.Find("childProgramGrid", false).FirstOrDefault() as DataGridView;
                
                if ((adultGrid == null || adultGrid.Rows.Count == 0) && 
                    (childGrid == null || childGrid.Rows.Count == 0))
                {
                    MessageBox.Show(
                        "⚠️ لم يتم العثور على بيانات البرنامج اليومي!\n\n" +
                        "📝 يرجى:\n" +
                        "1. العودة للخطوة 2 (البرنامج اليومي)\n" +
                        "2. إضافة أيام البرنامج والمزارات\n" +
                        "3. الضغط على 'التالي' للحفظ\n" +
                        "4. ثم العودة للخطوة 3 (النقل)",
                        "⚠️ تنبيه",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PopulateTransportationFromVisits] ❌ خطأ: {ex.Message}");
            Console.WriteLine($"[PopulateTransportationFromVisits] Stack Trace: {ex.StackTrace}");
            
            MessageBox.Show(
                $"❌ حدث خطأ في استيراد المزارات:\n\n{ex.Message}\n\n" +
                "📞 يرجى التواصل مع الدعم الفني إذا استمرت المشكلة.",
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
