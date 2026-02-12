using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Presentation.Forms;

/// <summary>
/// إضافة حجز جديد لرحلة
/// </summary>
public partial class AddTripBookingForm : Form
{
    private readonly ITripBookingService _bookingService;
    private readonly ITripService _tripService;
    private readonly ICustomerService _customerService;
    private readonly ICashBoxService _cashBoxService;
    private readonly int _tripId;
    private readonly int _currentUserId;
    private Trip _trip = null!;
    
    // Controls
    private Panel _headerPanel = null!;
    private Label _titleLabel = null!;
    private Label _tripInfoLabel = null!;
    private ComboBox _customerCombo = null!;
    private Button _newCustomerButton = null!;
    private NumericUpDown _numberOfPersonsNumeric = null!;
    private NumericUpDown _pricePerPersonNumeric = null!;
    private Label _totalAmountLabel = null!;
    private NumericUpDown _paidAmountNumeric = null!;
    private Label _remainingAmountLabel = null!;
    private ComboBox _cashBoxCombo = null!;
    private ComboBox _paymentMethodCombo = null!;
    private TextBox _specialRequestsBox = null!;
    private TextBox _notesBox = null!;
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    
    public AddTripBookingForm(ITripBookingService bookingService, ITripService tripService,
        ICustomerService customerService, ICashBoxService cashBoxService, 
        int tripId, int currentUserId)
    {
        _bookingService = bookingService;
        _tripService = tripService;
        _customerService = customerService;
        _cashBoxService = cashBoxService;
        _tripId = tripId;
        _currentUserId = currentUserId;
        
        InitializeComponent();
        SetupForm();
        InitializeCustomControls();
        _ = LoadDataAsync();
    }
    
    private void SetupForm()
    {
        this.Text = "حجز جديد";
        this.Size = new Size(650, 750);
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
            Height = 100,
            BackColor = ColorScheme.Primary,
            Padding = new Padding(20)
        };
        
        _titleLabel = new Label
        {
            Text = "➕ حجز جديد",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 15)
        };
        _headerPanel.Controls.Add(_titleLabel);
        
        _tripInfoLabel = new Label
        {
            Text = "جاري التحميل...",
            Font = new Font("Cairo", 10F),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 55)
        };
        _headerPanel.Controls.Add(_tripInfoLabel);
        
        // Content Panel
        Panel contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(30),
            AutoScroll = true
        };
        
        int y = 20;
        
        // العميل
        AddLabel(contentPanel, "👤 العميل: *", ref y);
        
        Panel customerPanel = new Panel
        {
            Location = new Point(30, y),
            Size = new Size(560, 35)
        };
        
        _customerCombo = new ComboBox
        {
            Location = new Point(0, 0),
            Size = new Size(450, 30),
            Font = new Font("Cairo", 10F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        customerPanel.Controls.Add(_customerCombo);
        
        _newCustomerButton = new Button
        {
            Text = "+ جديد",
            Location = new Point(460, 0),
            Size = new Size(100, 30),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _newCustomerButton.FlatAppearance.BorderSize = 0;
        _newCustomerButton.Click += NewCustomerButton_Click;
        customerPanel.Controls.Add(_newCustomerButton);
        
        contentPanel.Controls.Add(customerPanel);
        y += 55;
        
        // عدد الأفراد
        AddLabel(contentPanel, "👥 عدد الأفراد: *", ref y);
        _numberOfPersonsNumeric = new NumericUpDown
        {
            Location = new Point(30, y),
            Size = new Size(200, 30),
            Font = new Font("Cairo", 10F),
            Minimum = 1,
            Maximum = 100,
            Value = 1
        };
        _numberOfPersonsNumeric.ValueChanged += CalculateTotalAmount;
        contentPanel.Controls.Add(_numberOfPersonsNumeric);
        y += 50;
        
        // السعر للفرد
        AddLabel(contentPanel, "💵 السعر للفرد: *", ref y);
        _pricePerPersonNumeric = new NumericUpDown
        {
            Location = new Point(30, y),
            Size = new Size(200, 30),
            Font = new Font("Cairo", 10F),
            Minimum = 0,
            Maximum = 1000000,
            DecimalPlaces = 2
        };
        _pricePerPersonNumeric.ValueChanged += CalculateTotalAmount;
        contentPanel.Controls.Add(_pricePerPersonNumeric);
        y += 50;
        
        // الإجمالي
        _totalAmountLabel = new Label
        {
            Text = "💰 الإجمالي: 0.00 جنيه",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(30, y)
        };
        contentPanel.Controls.Add(_totalAmountLabel);
        y += 50;
        
        // المبلغ المدفوع
        AddLabel(contentPanel, "💳 المبلغ المدفوع الآن: *", ref y);
        _paidAmountNumeric = new NumericUpDown
        {
            Location = new Point(30, y),
            Size = new Size(200, 30),
            Font = new Font("Cairo", 10F),
            Minimum = 0,
            Maximum = 1000000,
            DecimalPlaces = 2
        };
        _paidAmountNumeric.ValueChanged += CalculateRemainingAmount;
        contentPanel.Controls.Add(_paidAmountNumeric);
        y += 50;
        
        // المتبقي
        _remainingAmountLabel = new Label
        {
            Text = "⏳ المتبقي: 0.00 جنيه",
            Font = new Font("Cairo", 10F),
            ForeColor = ColorScheme.Error,
            AutoSize = true,
            Location = new Point(30, y)
        };
        contentPanel.Controls.Add(_remainingAmountLabel);
        y += 50;
        
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
        contentPanel.Controls.Add(_paymentMethodCombo);
        y += 50;
        
        // طلبات خاصة
        AddLabel(contentPanel, "📝 طلبات خاصة:", ref y);
        _specialRequestsBox = new TextBox
        {
            Location = new Point(30, y),
            Size = new Size(540, 60),
            Font = new Font("Cairo", 10F),
            Multiline = true
        };
        contentPanel.Controls.Add(_specialRequestsBox);
        y += 80;
        
        // ملاحظات
        AddLabel(contentPanel, "📄 ملاحظات:", ref y);
        _notesBox = new TextBox
        {
            Location = new Point(30, y),
            Size = new Size(540, 60),
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
            Text = "💾 حفظ الحجز",
            Size = new Size(140, 40),
            Location = new Point(480, 15),
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
            // تحميل بيانات الرحلة
            _trip = await _tripService.GetTripByIdAsync(_tripId, includeDetails: false) 
                ?? throw new Exception("الرحلة غير موجودة");
            
            _tripInfoLabel.Text = $"🌍 {_trip.TripName} | 📅 {_trip.StartDate:yyyy-MM-dd} | 👥 متبقي: {_trip.AvailableSeats} مقعد";
            _pricePerPersonNumeric.Value = _trip.SellingPricePerPerson;
            
            // تحميل العملاء
            var customers = await _customerService.GetAllCustomersAsync();
            _customerCombo.DataSource = customers;
            _customerCombo.DisplayMember = "Name";
            _customerCombo.ValueMember = "CustomerId";
            
            // تحميل الخزن
            var cashBoxes = await _cashBoxService.GetAllCashBoxesAsync();
            _cashBoxCombo.DataSource = cashBoxes;
            _cashBoxCombo.DisplayMember = "Name";
            _cashBoxCombo.ValueMember = "CashBoxId";
            
            CalculateTotalAmount(null, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
        }
    }
    
    private void CalculateTotalAmount(object? sender, EventArgs e)
    {
        decimal total = _numberOfPersonsNumeric.Value * _pricePerPersonNumeric.Value;
        _totalAmountLabel.Text = $"💰 الإجمالي: {total:N2} جنيه";
        CalculateRemainingAmount(sender, e);
    }
    
    private void CalculateRemainingAmount(object? sender, EventArgs e)
    {
        decimal total = _numberOfPersonsNumeric.Value * _pricePerPersonNumeric.Value;
        decimal remaining = total - _paidAmountNumeric.Value;
        _remainingAmountLabel.Text = $"⏳ المتبقي: {remaining:N2} جنيه";
        _remainingAmountLabel.ForeColor = remaining > 0 ? ColorScheme.Error : ColorScheme.Success;
    }
    
    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        try
        {
            // التحقق
            if (_customerCombo.SelectedIndex < 0)
            {
                MessageBox.Show("اختر العميل", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            int numberOfPersons = (int)_numberOfPersonsNumeric.Value;
            if (numberOfPersons > _trip.AvailableSeats)
            {
                MessageBox.Show($"عدد الأفراد ({numberOfPersons}) أكبر من الأماكن المتاحة ({_trip.AvailableSeats})", 
                    "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (_paidAmountNumeric.Value == 0)
            {
                var result = MessageBox.Show("لم يتم دفع أي مبلغ. هل تريد المتابعة؟", "تأكيد", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes) return;
            }
            
            this.Cursor = Cursors.WaitCursor;
            
            // إنشاء الحجز
            var booking = new TripBooking
            {
                TripId = _tripId,
                CustomerId = _customerCombo.SelectedValue != null ? (int)_customerCombo.SelectedValue : 0,
                NumberOfPersons = numberOfPersons,
                PricePerPerson = _pricePerPersonNumeric.Value,
                PaidAmount = _paidAmountNumeric.Value,
                Status = BookingStatus.Pending,
                PaymentStatus = _paidAmountNumeric.Value >= (_numberOfPersonsNumeric.Value * _pricePerPersonNumeric.Value) 
                    ? PaymentStatus.FullyPaid 
                    : _paidAmountNumeric.Value > 0 ? PaymentStatus.PartiallyPaid : PaymentStatus.NotPaid,
                SpecialRequests = _specialRequestsBox.Text,
                Notes = _notesBox.Text,
                CreatedBy = _currentUserId
            };
            
            await _bookingService.CreateBookingAsync(booking);
            
            // تسجيل الدفعة إذا كانت أكبر من صفر
            if (_paidAmountNumeric.Value > 0)
            {
                var payment = new TripBookingPayment
                {
                    TripBookingId = booking.TripBookingId,
                    Amount = _paidAmountNumeric.Value,
                    PaymentMethod = GetPaymentMethod(_paymentMethodCombo.SelectedIndex),
                    CashBoxId = _cashBoxCombo.SelectedValue != null ? (int)_cashBoxCombo.SelectedValue : 0,
                    CreatedBy = _currentUserId,
                    Notes = "دفعة أولى عند الحجز"
                };
                
                await _bookingService.RecordPaymentAsync(payment);
            }
            
            MessageBox.Show("✅ تم إضافة الحجز بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    
    private void NewCustomerButton_Click(object? sender, EventArgs e)
    {
        // فتح نموذج إضافة عميل جديد
        MessageBox.Show("سيتم فتح نموذج إضافة عميل", "قريباً", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    
    private PaymentMethod GetPaymentMethod(int index)
    {
        return index switch
        {
            0 => PaymentMethod.Cash,
            1 => PaymentMethod.Visa,
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
