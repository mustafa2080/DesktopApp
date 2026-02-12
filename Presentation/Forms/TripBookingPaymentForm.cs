using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Presentation.Forms;

/// <summary>
/// تسجيل دفعة على حجز موجود
/// </summary>
public partial class TripBookingPaymentForm : Form
{
    private readonly ITripBookingService _bookingService;
    private readonly ICashBoxService _cashBoxService;
    private readonly int _bookingId;
    private readonly int _currentUserId;
    private TripBooking _booking = null!;
    
    // Controls
    private Panel _headerPanel = null!;
    private Label _titleLabel = null!;
    private Label _bookingInfoLabel = null!;
    private Label _totalAmountLabel = null!;
    private Label _paidAmountLabel = null!;
    private Label _remainingAmountLabel = null!;
    private NumericUpDown _amountNumeric = null!;
    private ComboBox _cashBoxCombo = null!;
    private ComboBox _paymentMethodCombo = null!;
    private NumericUpDown _instaPayCommissionNumeric = null!;
    private TextBox _referenceNumberBox = null!;
    private TextBox _notesBox = null!;
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    
    public TripBookingPaymentForm(ITripBookingService bookingService, ICashBoxService cashBoxService, 
        int bookingId, int currentUserId)
    {
        _bookingService = bookingService;
        _cashBoxService = cashBoxService;
        _bookingId = bookingId;
        _currentUserId = currentUserId;
        
        InitializeComponent();
        SetupForm();
        InitializeCustomControls();
        _ = LoadDataAsync();
    }
    
    private void SetupForm()
    {
        this.Text = "تسجيل دفعة";
        this.Size = new Size(600, 650);
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
        // Header
        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 150,
            BackColor = ColorScheme.Primary,
            Padding = new Padding(20)
        };
        
        _titleLabel = new Label
        {
            Text = "💳 تسجيل دفعة جديدة",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 15)
        };
        _headerPanel.Controls.Add(_titleLabel);
        
        _bookingInfoLabel = new Label
        {
            Text = "جاري التحميل...",
            Font = new Font("Cairo", 10F),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 55)
        };
        _headerPanel.Controls.Add(_bookingInfoLabel);
        
        _totalAmountLabel = new Label
        {
            Font = new Font("Cairo", 10F),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 80)
        };
        _headerPanel.Controls.Add(_totalAmountLabel);
        
        _paidAmountLabel = new Label
        {
            Font = new Font("Cairo", 10F),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 105)
        };
        _headerPanel.Controls.Add(_paidAmountLabel);
        
        _remainingAmountLabel = new Label
        {
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(255, 243, 156),
            AutoSize = true,
            Location = new Point(20, 125)
        };
        _headerPanel.Controls.Add(_remainingAmountLabel);
        
        // Content Panel
        Panel contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(30),
            AutoScroll = true
        };
        
        int y = 20;
        
        // المبلغ
        AddLabel(contentPanel, "💵 المبلغ: *", ref y);
        _amountNumeric = new NumericUpDown
        {
            Location = new Point(30, y),
            Size = new Size(250, 35),
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            Minimum = 0,
            Maximum = 1000000,
            DecimalPlaces = 2
        };
        contentPanel.Controls.Add(_amountNumeric);
        
        Button setRemainingButton = new Button
        {
            Text = "المتبقي كاملاً",
            Location = new Point(290, y),
            Size = new Size(120, 35),
            BackColor = Color.FromArgb(52, 152, 219),
            ForeColor = Color.White,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        setRemainingButton.FlatAppearance.BorderSize = 0;
        setRemainingButton.Click += (s, e) => _amountNumeric.Value = _booking.RemainingAmount;
        contentPanel.Controls.Add(setRemainingButton);
        y += 60;
        
        // الخزنة
        AddLabel(contentPanel, "🏦 الخزنة: *", ref y);
        _cashBoxCombo = new ComboBox
        {
            Location = new Point(30, y),
            Size = new Size(400, 30),
            Font = new Font("Cairo", 10F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        contentPanel.Controls.Add(_cashBoxCombo);
        y += 50;
        
        // طريقة الدفع
        AddLabel(contentPanel, "💳 طريقة الدفع: *", ref y);
        _paymentMethodCombo = new ComboBox
        {
            Location = new Point(30, y),
            Size = new Size(400, 30),
            Font = new Font("Cairo", 10F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _paymentMethodCombo.Items.AddRange(new[] { "نقدي", "فيزا", "إنستا باي", "تحويل بنكي" });
        _paymentMethodCombo.SelectedIndex = 0;
        _paymentMethodCombo.SelectedIndexChanged += PaymentMethodChanged;
        contentPanel.Controls.Add(_paymentMethodCombo);
        y += 50;
        
        // عمولة إنستا باي (مخفي افتراضياً)
        Label instaPayLabel = new Label
        {
            Text = "💸 عمولة إنستا باي:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(30, y),
            Visible = false
        };
        instaPayLabel.Name = "instaPayLabel";
        contentPanel.Controls.Add(instaPayLabel);
        y += 35;
        
        _instaPayCommissionNumeric = new NumericUpDown
        {
            Location = new Point(30, y),
            Size = new Size(200, 30),
            Font = new Font("Cairo", 10F),
            Minimum = 0,
            Maximum = 10000,
            DecimalPlaces = 2,
            Visible = false
        };
        _instaPayCommissionNumeric.Name = "instaPayCommissionNumeric";
        contentPanel.Controls.Add(_instaPayCommissionNumeric);
        y += 50;
        
        // رقم المرجع
        AddLabel(contentPanel, "🔢 رقم المرجع:", ref y);
        _referenceNumberBox = new TextBox
        {
            Location = new Point(30, y),
            Size = new Size(400, 30),
            Font = new Font("Cairo", 10F)
        };
        contentPanel.Controls.Add(_referenceNumberBox);
        y += 50;
        
        // ملاحظات
        AddLabel(contentPanel, "📄 ملاحظات:", ref y);
        _notesBox = new TextBox
        {
            Location = new Point(30, y),
            Size = new Size(510, 80),
            Font = new Font("Cairo", 10F),
            Multiline = true
        };
        contentPanel.Controls.Add(_notesBox);
        
        // Footer
        Panel footerPanel = new Panel
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
        _cancelButton.Click += (s, e) => this.Close();
        footerPanel.Controls.Add(_cancelButton);
        
        _saveButton = new Button
        {
            Text = "💾 تسجيل الدفعة",
            Size = new Size(140, 40),
            Location = new Point(430, 15),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _saveButton.FlatAppearance.BorderSize = 0;
        _saveButton.Click += SaveButton_Click;
        footerPanel.Controls.Add(_saveButton);
        
        this.Controls.Add(contentPanel);
        this.Controls.Add(footerPanel);
        this.Controls.Add(_headerPanel);
    }
    
    private async Task LoadDataAsync()
    {
        try
        {
            // تحميل بيانات الحجز
            _booking = await _bookingService.GetBookingByIdAsync(_bookingId) 
                ?? throw new Exception("الحجز غير موجود");
            
            _bookingInfoLabel.Text = $"📋 حجز رقم: {_booking.BookingNumber} | 👤 {_booking.Customer?.CustomerName ?? "غير معروف"}";
            _totalAmountLabel.Text = $"💰 الإجمالي: {_booking.TotalAmount:N2} جنيه";
            _paidAmountLabel.Text = $"✅ المدفوع: {_booking.PaidAmount:N2} جنيه";
            _remainingAmountLabel.Text = $"⏳ المتبقي: {_booking.RemainingAmount:N2} جنيه";
            
            _amountNumeric.Maximum = _booking.RemainingAmount;
            _amountNumeric.Value = _booking.RemainingAmount;
            
            // تحميل الخزن
            var cashBoxes = await _cashBoxService.GetAllCashBoxesAsync();
            _cashBoxCombo.DataSource = cashBoxes;
            _cashBoxCombo.DisplayMember = "Name";
            _cashBoxCombo.ValueMember = "CashBoxId";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
        }
    }
    
    private void PaymentMethodChanged(object? sender, EventArgs e)
    {
        bool isInstaPay = _paymentMethodCombo.SelectedIndex == 2;
        
        // إظهار/إخفاء حقل عمولة إنستا باي
        foreach (Control control in this.Controls)
        {
            if (control is Panel panel && panel.Dock == DockStyle.Fill)
            {
                foreach (Control c in panel.Controls)
                {
                    if (c.Name == "instaPayLabel" || c.Name == "instaPayCommissionNumeric")
                    {
                        c.Visible = isInstaPay;
                    }
                }
            }
        }
        
        // حساب عمولة إنستا باي تلقائياً
        // النسبة: 0.1% من المبلغ
        // الحد الأدنى: 0.50 جنيه
        // الحد الأقصى: 20 جنيه
        if (isInstaPay)
        {
            decimal commission = _amountNumeric.Value * 0.001m; // 0.1%
            
            if (commission < 0.50m)
                commission = 0.50m;
            
            if (commission > 20m)
                commission = 20m;
            
            _instaPayCommissionNumeric.Value = commission;
        }
    }
    
    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        try
        {
            // التحقق
            if (_amountNumeric.Value <= 0)
            {
                MessageBox.Show("المبلغ يجب أن يكون أكبر من صفر", "تنبيه", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (_amountNumeric.Value > _booking.RemainingAmount)
            {
                MessageBox.Show($"المبلغ ({_amountNumeric.Value:N2}) أكبر من المتبقي ({_booking.RemainingAmount:N2})", 
                    "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (_cashBoxCombo.SelectedIndex < 0)
            {
                MessageBox.Show("اختر الخزنة", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            this.Cursor = Cursors.WaitCursor;
            
            // إنشاء الدفعة
            var payment = new TripBookingPayment
            {
                TripBookingId = _bookingId,
                Amount = _amountNumeric.Value,
                PaymentMethod = GetPaymentMethod(_paymentMethodCombo.SelectedIndex),
                CashBoxId = (int)_cashBoxCombo.SelectedValue!,
                ReferenceNumber = _referenceNumberBox.Text,
                InstaPayCommission = _paymentMethodCombo.SelectedIndex == 2 ? _instaPayCommissionNumeric.Value : null,
                Notes = _notesBox.Text,
                CreatedBy = _currentUserId
            };
            
            await _bookingService.RecordPaymentAsync(payment);
            
            MessageBox.Show("✅ تم تسجيل الدفعة بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ خطأ: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    
    private PaymentMethod GetPaymentMethod(int index)
    {
        return index switch
        {
            0 => PaymentMethod.Cash,
            1 => PaymentMethod.Card,
            2 => PaymentMethod.InstaPay,
            3 => PaymentMethod.BankTransfer,
            _ => PaymentMethod.Cash
        };
    }
    
    private void AddLabel(Panel panel, string text, ref int y)
    {
        var label = new Label
        {
            Text = text,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(30, y)
        };
        panel.Controls.Add(label);
        y += 35;
    }
}
