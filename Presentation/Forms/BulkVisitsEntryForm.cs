using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Presentation.Forms;

/// <summary>
/// نموذج إدخال المزارات لعدة أيام دفعة واحدة
/// </summary>
public partial class BulkVisitsEntryForm : Form
{
    public class VisitEntry
    {
        public DateTime DayDate { get; set; }
        public int DayNumber { get; set; }
        public string Visits { get; set; } = "";
        public decimal VisitsCost { get; set; }
        public int ParticipantsCount { get; set; }
        public decimal GuideCost { get; set; }
    }
    
    private readonly DateTime _startDate;
    private readonly int _existingDaysCount;
    private readonly string _bookingType;
    
    public List<VisitEntry> VisitEntries { get; private set; } = new();
    
    // Controls
    private NumericUpDown _numberOfDaysNumeric = null!;
    private NumericUpDown _participantsNumeric = null!;
    private NumericUpDown _guideCostNumeric = null!;
    private TextBox _visitsCostBox = null!;
    private DataGridView _daysGrid = null!;
    private Button _generateButton = null!;
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    
    public BulkVisitsEntryForm(DateTime startDate, int existingDaysCount, string bookingType, int defaultParticipants)
    {
        _startDate = startDate;
        _existingDaysCount = existingDaysCount;
        _bookingType = bookingType;
        
        InitializeComponent();
        SetupForm();
        InitializeCustomControls();
        
        // تعيين القيم الافتراضية
        _participantsNumeric.Value = defaultParticipants;
    }
    
    private void SetupForm()
    {
        this.Text = $"إضافة مزارات متعددة - {_bookingType}";
        this.Size = new Size(950, 650);
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
        // Header Panel
        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 80,
            BackColor = ColorScheme.Primary,
            Padding = new Padding(20)
        };
        
        var titleLabel = new Label
        {
            Text = $"📝 إضافة مزارات متعددة دفعة واحدة - {_bookingType}",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 20)
        };
        headerPanel.Controls.Add(titleLabel);
        
        // Content Panel
        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(20),
            AutoScroll = true
        };
        
        int y = 20;
        
        // ملاحظة إرشادية
        var notePanel = new Panel
        {
            Location = new Point(20, y),
            Size = new Size(880, 60),
            BackColor = Color.FromArgb(232, 245, 233),
            BorderStyle = BorderStyle.FixedSingle
        };
        
        var noteLabel = new Label
        {
            Text = "💡 هذا النموذج يسمح لك بإضافة عدة أيام دفعة واحدة بنفس البيانات (عدد الأفراد وسعر المرشد)\nثم تعديل المزارات وسعرها لكل يوم على حدة في الجدول",
            Font = new Font("Cairo", 9F),
            Location = new Point(10, 10),
            Size = new Size(860, 40),
            ForeColor = Color.FromArgb(46, 125, 50)
        };
        notePanel.Controls.Add(noteLabel);
        contentPanel.Controls.Add(notePanel);
        y += 70;
        
        // البيانات الأساسية
        var settingsPanel = new Panel
        {
            Location = new Point(20, y),
            Size = new Size(880, 120),
            BackColor = Color.FromArgb(245, 245, 245),
            BorderStyle = BorderStyle.FixedSingle
        };
        
        // عدد الأيام
        var daysLabel = new Label
        {
            Text = "عدد الأيام المراد إضافتها:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Location = new Point(680, 15),
            Size = new Size(180, 25),
            TextAlign = ContentAlignment.MiddleRight
        };
        settingsPanel.Controls.Add(daysLabel);
        
        _numberOfDaysNumeric = new NumericUpDown
        {
            Location = new Point(520, 15),
            Size = new Size(150, 30),
            Font = new Font("Cairo", 10F),
            Minimum = 1,
            Maximum = 30,
            Value = 1
        };
        settingsPanel.Controls.Add(_numberOfDaysNumeric);
        
        // عدد الأفراد (مشترك)
        var participantsLabel = new Label
        {
            Text = "عدد الأفراد (مشترك):",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Location = new Point(330, 15),
            Size = new Size(180, 25),
            TextAlign = ContentAlignment.MiddleRight
        };
        settingsPanel.Controls.Add(participantsLabel);
        
        _participantsNumeric = new NumericUpDown
        {
            Location = new Point(170, 15),
            Size = new Size(150, 30),
            Font = new Font("Cairo", 10F),
            Minimum = 1,
            Maximum = 1000,
            Value = 1
        };
        settingsPanel.Controls.Add(_participantsNumeric);
        
        // سعر المرشد (مشترك)
        var guideCostLabel = new Label
        {
            Text = "سعر المرشد (مشترك):",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Location = new Point(680, 60),
            Size = new Size(180, 25),
            TextAlign = ContentAlignment.MiddleRight
        };
        settingsPanel.Controls.Add(guideCostLabel);
        
        _guideCostNumeric = new NumericUpDown
        {
            Location = new Point(520, 60),
            Size = new Size(150, 30),
            Font = new Font("Cairo", 10F),
            Minimum = 0,
            Maximum = 100000,
            DecimalPlaces = 2,
            Value = 0
        };
        settingsPanel.Controls.Add(_guideCostNumeric);
        
        // زر التوليد
        _generateButton = new Button
        {
            Text = "🔄 توليد الأيام",
            Location = new Point(20, 60),
            Size = new Size(280, 40),
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _generateButton.FlatAppearance.BorderSize = 0;
        _generateButton.Click += GenerateButton_Click;
        settingsPanel.Controls.Add(_generateButton);
        
        contentPanel.Controls.Add(settingsPanel);
        y += 130;
        
        // الجدول
        var gridLabel = new Label
        {
            Text = "📅 قم بكتابة المزارات وسعرها لكل يوم:",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Location = new Point(20, y),
            AutoSize = true
        };
        contentPanel.Controls.Add(gridLabel);
        y += 35;
        
        _daysGrid = new DataGridView
        {
            Location = new Point(20, y),
            Size = new Size(880, 270),
            Font = new Font("Cairo", 9F),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };
        
        _daysGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "DayDate", 
            HeaderText = "التاريخ", 
            Width = 100,
            ReadOnly = true,
            DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(240, 240, 240) }
        });
        
        _daysGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "DayNumber", 
            HeaderText = "اليوم", 
            Width = 60,
            ReadOnly = true,
            DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(240, 240, 240) }
        });
        
        _daysGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "Visits", 
            HeaderText = "المزارات (اكتب هنا)", 
            Width = 250
        });
        
        _daysGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "VisitsCost", 
            HeaderText = "سعر المزارات", 
            Width = 120
        });
        
        _daysGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "ParticipantsCount", 
            HeaderText = "عدد الأفراد", 
            Width = 100,
            ReadOnly = true,
            DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(240, 240, 240) }
        });
        
        _daysGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "GuideCost", 
            HeaderText = "سعر المرشد", 
            Width = 100,
            ReadOnly = true,
            DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(240, 240, 240) }
        });
        
        _daysGrid.Columns.Add(new DataGridViewTextBoxColumn 
        { 
            Name = "TotalPerPerson", 
            HeaderText = "الإجمالي/فرد", 
            Width = 110,
            ReadOnly = true,
            DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(232, 245, 233), ForeColor = Color.FromArgb(46, 125, 50), Font = new Font("Cairo", 9F, FontStyle.Bold) }
        });
        
        // حساب تلقائي عند تغيير سعر المزارات
        _daysGrid.CellValueChanged += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == _daysGrid.Columns["VisitsCost"]!.Index)
            {
                UpdateRowTotal(e.RowIndex);
            }
        };
        
        contentPanel.Controls.Add(_daysGrid);
        
        // Footer Panel
        var footerPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 70,
            BackColor = Color.FromArgb(245, 245, 245),
            Padding = new Padding(20)
        };
        
        _cancelButton = new Button
        {
            Text = "إلغاء",
            Size = new Size(120, 40),
            Location = new Point(20, 15),
            BackColor = Color.FromArgb(149, 165, 166),
            ForeColor = Color.White,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _cancelButton.FlatAppearance.BorderSize = 0;
        _cancelButton.Click += (s, e) => 
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        };
        footerPanel.Controls.Add(_cancelButton);
        
        _saveButton = new Button
        {
            Text = "✅ حفظ وإضافة",
            Size = new Size(150, 40),
            Location = new Point(760, 15),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        _saveButton.FlatAppearance.BorderSize = 0;
        _saveButton.Click += SaveButton_Click;
        footerPanel.Controls.Add(_saveButton);
        
        this.Controls.Add(contentPanel);
        this.Controls.Add(footerPanel);
        this.Controls.Add(headerPanel);
    }
    
    private void GenerateButton_Click(object? sender, EventArgs e)
    {
        _daysGrid.Rows.Clear();
        
        int numberOfDays = (int)_numberOfDaysNumeric.Value;
        int participants = (int)_participantsNumeric.Value;
        decimal guideCost = _guideCostNumeric.Value;
        
        for (int i = 0; i < numberOfDays; i++)
        {
            int dayNumber = _existingDaysCount + i + 1;
            DateTime dayDate = _startDate.AddDays(dayNumber - 1);
            
            int rowIndex = _daysGrid.Rows.Add(
                dayDate.ToString("yyyy-MM-dd"),
                dayNumber,
                "",  // المزارات (فارغ للتعبئة)
                0,   // سعر المزارات (افتراضي صفر)
                participants,
                guideCost.ToString("N2"),
                CalculateTotal(0, guideCost, participants)
            );
        }
        
        _saveButton.Enabled = true;
        
        MessageBox.Show($"✅ تم توليد {numberOfDays} يوم\nقم الآن بكتابة المزارات وسعرها لكل يوم", 
            "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    
    private void UpdateRowTotal(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= _daysGrid.Rows.Count) return;
        
        var row = _daysGrid.Rows[rowIndex];
        
        decimal visitsCost = decimal.TryParse(row.Cells["VisitsCost"].Value?.ToString(), out var vc) ? vc : 0;
        decimal guideCost = decimal.TryParse(row.Cells["GuideCost"].Value?.ToString(), out var gc) ? gc : 0;
        int participants = int.TryParse(row.Cells["ParticipantsCount"].Value?.ToString(), out var pc) && pc > 0 ? pc : 1;
        
        row.Cells["TotalPerPerson"].Value = CalculateTotal(visitsCost, guideCost, participants);
    }
    
    private string CalculateTotal(decimal visitsCost, decimal guideCost, int participants)
    {
        decimal guideCostPerPerson = participants > 0 ? guideCost / participants : 0;
        decimal totalPerPerson = visitsCost + guideCostPerPerson;
        return totalPerPerson.ToString("N2");
    }
    
    private void SaveButton_Click(object? sender, EventArgs e)
    {
        VisitEntries.Clear();
        
        foreach (DataGridViewRow row in _daysGrid.Rows)
        {
            if (row.IsNewRow) continue;
            
            DateTime dayDate = DateTime.Parse(row.Cells["DayDate"].Value?.ToString() ?? DateTime.Now.ToString());
            int dayNumber = int.Parse(row.Cells["DayNumber"].Value?.ToString() ?? "1");
            string visits = row.Cells["Visits"].Value?.ToString() ?? "";
            decimal visitsCost = decimal.TryParse(row.Cells["VisitsCost"].Value?.ToString(), out var vc) ? vc : 0;
            int participants = int.TryParse(row.Cells["ParticipantsCount"].Value?.ToString(), out var pc) ? pc : 1;
            decimal guideCost = decimal.TryParse(row.Cells["GuideCost"].Value?.ToString(), out var gc) ? gc : 0;
            
            VisitEntries.Add(new VisitEntry
            {
                DayDate = dayDate,
                DayNumber = dayNumber,
                Visits = visits,
                VisitsCost = visitsCost,
                ParticipantsCount = participants,
                GuideCost = guideCost
            });
        }
        
        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}
