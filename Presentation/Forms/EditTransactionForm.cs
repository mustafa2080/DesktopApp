using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Application.Services;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class EditTransactionForm : Form
{
    private readonly int _transactionId;
    private readonly ICashBoxService _cashBoxService;
    private readonly int _userId;
    private CashTransaction? _originalTransaction;
    
    // Controls
    private Panel _mainPanel = null!;
    private Label _titleLabel = null!;
    private DateTimePicker _datePicker = null!;
    private TextBox _voucherNumberText = null!;
    private ComboBox _categoryCombo = null!;
    private TextBox _amountText = null!;
    private TextBox _descriptionText = null!;
    private ComboBox _paymentMethodCombo = null!;
    private Label _commissionLabel = null!;
    private TextBox _commissionText = null!;
    private Label _netAmountLabel = null!;
    private TextBox _netAmountText = null!;
    private TextBox _notesText = null!;
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    
    public bool TransactionUpdated { get; private set; }
    private bool _isDataLoaded = false;
    
    public EditTransactionForm(int transactionId, ICashBoxService cashBoxService, int userId)
    {
        _transactionId = transactionId;
        _cashBoxService = cashBoxService;
        _userId = userId;
        
        InitializeComponent();
        InitializeCustomControls();
        this.Load += async (s, e) => await LoadTransactionAsync();
    }
    
    private void InitializeComponent()
    {
        this.Text = "تعديل حركة مالية";
        this.Size = new Size(700, 750);
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
        _mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(40, 30, 40, 30),
            AutoScroll = true
        };
        
        int yPos = 20;
        int controlWidth = 600;
        int rightMargin = 20;
        
        // Title with icon
        _titleLabel = new Label
        {
            Text = "✏️ تعديل حركة مالية",
            Font = new Font("Cairo", 18F, FontStyle.Bold),
            ForeColor = ColorScheme.Warning,
            Size = new Size(controlWidth, 50),
            Location = new Point(rightMargin, yPos),
            TextAlign = ContentAlignment.MiddleRight
        };
        _mainPanel.Controls.Add(_titleLabel);
        yPos += 70;
        
        // Separator line
        Panel separator = new Panel
        {
            Size = new Size(controlWidth, 2),
            Location = new Point(rightMargin, yPos),
            BackColor = Color.FromArgb(230, 230, 230)
        };
        _mainPanel.Controls.Add(separator);
        yPos += 20;
        
        // التاريخ
        AddFormField("التاريخ:", ref yPos, rightMargin, controlWidth, () =>
        {
            _datePicker = new DateTimePicker
            {
                Font = new Font("Cairo", 11F),
                Size = new Size(controlWidth, 35),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now,
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = false
            };
            return _datePicker;
        });
        
        // رقم السند
        AddFormField("رقم السند:", ref yPos, rightMargin, controlWidth, () =>
        {
            _voucherNumberText = new TextBox
            {
                Font = new Font("Cairo", 11F),
                Size = new Size(controlWidth, 35),
                TextAlign = HorizontalAlignment.Right,
                ReadOnly = true,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            return _voucherNumberText;
        });
        
        // التصنيف
        AddFormField("التصنيف:", ref yPos, rightMargin, controlWidth, () =>
        {
            _categoryCombo = new ComboBox
            {
                Font = new Font("Cairo", 11F),
                Size = new Size(controlWidth, 35),
                DropDownStyle = ComboBoxStyle.DropDownList,
                RightToLeft = RightToLeft.Yes
            };
            return _categoryCombo;
        });
        
        // المبلغ
        AddFormField("المبلغ (جنيه):", ref yPos, rightMargin, controlWidth, () =>
        {
            _amountText = new TextBox
            {
                Font = new Font("Cairo", 12F, FontStyle.Bold),
                Size = new Size(controlWidth, 35),
                TextAlign = HorizontalAlignment.Right,
                PlaceholderText = "0.00"
            };
            _amountText.KeyPress += AmountText_KeyPress;
            _amountText.TextChanged += AmountText_TextChanged;
            return _amountText;
        });
        
        // طريقة الدفع
        AddFormField("طريقة الدفع:", ref yPos, rightMargin, controlWidth, () =>
        {
            _paymentMethodCombo = new ComboBox
            {
                Font = new Font("Cairo", 11F),
                Size = new Size(controlWidth, 35),
                DropDownStyle = ComboBoxStyle.DropDownList,
                RightToLeft = RightToLeft.Yes
            };
            _paymentMethodCombo.Items.AddRange(new object[]
            {
                "نقدي",
                "شيك",
                "تحويل بنكي",
                "بطاقة ائتمان",
                "إنستا باي",
                "آخر"
            });
            _paymentMethodCombo.SelectedIndexChanged += PaymentMethodCombo_SelectedIndexChanged;
            return _paymentMethodCombo;
        });
        
        // عمولة إنستا باي (تظهر فقط عند اختيار إنستا باي)
        _commissionLabel = new Label
        {
            Text = "عمولة إنستا باي:",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(50, 50, 50),
            Size = new Size(controlWidth, 30),
            Location = new Point(rightMargin, yPos),
            TextAlign = ContentAlignment.MiddleRight,
            Visible = false
        };
        _mainPanel.Controls.Add(_commissionLabel);
        yPos += 35;
        
        _commissionText = new TextBox
        {
            Font = new Font("Cairo", 11F),
            Size = new Size(controlWidth, 35),
            TextAlign = HorizontalAlignment.Right,
            ReadOnly = true,
            BackColor = Color.FromArgb(245, 245, 245),
            Location = new Point(rightMargin, yPos),
            Visible = false,
            PlaceholderText = "يتم حسابها تلقائياً"
        };
        _mainPanel.Controls.Add(_commissionText);
        yPos += 60;
        
        // صافي المبلغ (المبلغ بعد خصم العمولة)
        _netAmountLabel = new Label
        {
            Text = "صافي المبلغ (بعد العمولة):",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = ColorScheme.Success,
            Size = new Size(controlWidth, 30),
            Location = new Point(rightMargin, yPos),
            TextAlign = ContentAlignment.MiddleRight,
            Visible = false
        };
        _mainPanel.Controls.Add(_netAmountLabel);
        yPos += 35;
        
        _netAmountText = new TextBox
        {
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            Size = new Size(controlWidth, 35),
            TextAlign = HorizontalAlignment.Right,
            ReadOnly = true,
            BackColor = Color.FromArgb(240, 255, 240),
            ForeColor = ColorScheme.Success,
            Location = new Point(rightMargin, yPos),
            Visible = false
        };
        _mainPanel.Controls.Add(_netAmountText);
        yPos += 60;
        
        // الوصف
        AddFormField("الوصف:", ref yPos, rightMargin, controlWidth, () =>
        {
            _descriptionText = new TextBox
            {
                Font = new Font("Cairo", 11F),
                Size = new Size(controlWidth, 35),
                TextAlign = HorizontalAlignment.Right
            };
            return _descriptionText;
        });
        
        // ملاحظات
        AddFormField("ملاحظات (اختياري):", ref yPos, rightMargin, controlWidth, () =>
        {
            _notesText = new TextBox
            {
                Font = new Font("Cairo", 10F),
                Size = new Size(controlWidth, 80),
                Multiline = true,
                TextAlign = HorizontalAlignment.Right,
                ScrollBars = ScrollBars.Vertical
            };
            return _notesText;
        }, 100);
        
        yPos += 30;
        
        // Buttons Panel
        Panel buttonPanel = new Panel
        {
            Size = new Size(controlWidth, 55),
            Location = new Point(rightMargin, yPos),
            BackColor = Color.Transparent
        };
        
        _saveButton = new Button
        {
            Text = "⏳ جاري التحميل...",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            Size = new Size(280, 50),
            Location = new Point(0, 0),
            BackColor = ColorScheme.Warning,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Right,
            Enabled = false
        };
        _saveButton.FlatAppearance.BorderSize = 0;
        _saveButton.Click += SaveButton_Click;
        buttonPanel.Controls.Add(_saveButton);
        
        _cancelButton = new Button
        {
            Text = "✖ إلغاء",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            Size = new Size(280, 50),
            Location = new Point(310, 0),
            BackColor = Color.FromArgb(120, 120, 120),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Left
        };
        _cancelButton.FlatAppearance.BorderSize = 0;
        _cancelButton.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
        buttonPanel.Controls.Add(_cancelButton);
        
        _mainPanel.Controls.Add(buttonPanel);
        this.Controls.Add(_mainPanel);
    }
    
    private void AddFormField(string labelText, ref int yPos, int rightMargin, 
        int controlWidth, Func<Control> createControl, int additionalSpacing = 0)
    {
        // Label
        Label label = new Label
        {
            Text = labelText,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(50, 50, 50),
            Size = new Size(controlWidth, 30),
            Location = new Point(rightMargin, yPos),
            TextAlign = ContentAlignment.MiddleRight
        };
        _mainPanel.Controls.Add(label);
        yPos += 35;
        
        // Control
        var control = createControl();
        control.Location = new Point(rightMargin, yPos);
        _mainPanel.Controls.Add(control);
        yPos += control.Height + 25 + additionalSpacing;
    }
    
    private async Task LoadTransactionAsync()
    {
        try
        {
            _originalTransaction = await _cashBoxService.GetTransactionByIdAsync(_transactionId);
            
            if (_originalTransaction == null)
            {
                MessageBox.Show("لم يتم العثور على الحركة المطلوبة", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                this.Close();
                return;
            }
            
            // Populate controls with existing data
            _datePicker.Value = _originalTransaction.TransactionDate;
            _voucherNumberText.Text = _originalTransaction.VoucherNumber;
            _amountText.Text = _originalTransaction.Amount.ToString("F2");
            _descriptionText.Text = _originalTransaction.Description;
            _notesText.Text = _originalTransaction.Notes ?? "";
            
            // Load categories based on transaction type
            LoadCategories(_originalTransaction.Type);
            
            // Set category if it exists in the list
            if (!string.IsNullOrEmpty(_originalTransaction.Category))
            {
                int index = _categoryCombo.Items.IndexOf(_originalTransaction.Category);
                if (index >= 0)
                {
                    _categoryCombo.SelectedIndex = index;
                }
                else
                {
                    // Add custom category if not in list
                    _categoryCombo.Items.Add(_originalTransaction.Category);
                    _categoryCombo.SelectedItem = _originalTransaction.Category;
                }
            }
            
            // Set payment method
            _paymentMethodCombo.SelectedIndex = _originalTransaction.PaymentMethod switch
            {
                PaymentMethod.Cash => 0,
                PaymentMethod.Cheque => 1,
                PaymentMethod.BankTransfer => 2,
                PaymentMethod.CreditCard => 3,
                PaymentMethod.InstaPay => 4,
                PaymentMethod.Other => 5,
                _ => 0
            };
            
            // إذا كانت طريقة الدفع إنستا باي، اعرض العمولة
            if (_originalTransaction.PaymentMethod == PaymentMethod.InstaPay)
            {
                _commissionLabel.Visible = true;
                _commissionText.Visible = true;
                _netAmountLabel.Visible = true;
                _netAmountText.Visible = true;
                
                if (_originalTransaction.InstaPayCommission.HasValue)
                {
                    _commissionText.Text = _originalTransaction.InstaPayCommission.Value.ToString("N2");
                    decimal netAmount = _originalTransaction.Amount - _originalTransaction.InstaPayCommission.Value;
                    _netAmountText.Text = netAmount.ToString("N2");
                }
                else
                {
                    // حساب العمولة إذا لم تكن محفوظة
                    CalculateInstaPayCommission(_originalTransaction.Amount);
                }
            }
            
            // Update title color based on type
            if (_originalTransaction.Type == TransactionType.Income)
            {
                _titleLabel.ForeColor = ColorScheme.Success;
                _titleLabel.Text = "✏️ تعديل حركة إيراد";
            }
            else
            {
                _titleLabel.ForeColor = ColorScheme.Error;
                _titleLabel.Text = "✏️ تعديل حركة مصروف";
            }
            
            _isDataLoaded = true;
            _saveButton.Enabled = true;
            _saveButton.Text = "💾 حفظ التعديلات";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل بيانات الحركة:\n{ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            this.Close();
        }
    }
    
    private void LoadCategories(TransactionType transactionType)
    {
        if (transactionType == TransactionType.Income)
        {
            _categoryCombo.Items.AddRange(new object[]
            {
                "مبيعات",
                "حجوزات رحلات",
                "خدمات سياحية",
                "عمولات",
                "أخرى"
            });
        }
        else
        {
            _categoryCombo.Items.AddRange(new object[]
            {
                "رواتب وأجور",
                "إيجار",
                "فواتير (كهرباء، ماء، إنترنت)",
                "صيانة وإصلاحات",
                "مشتريات مكتبية",
                "مصاريف تسويق وإعلان",
                "مصاريف إدارية",
                "مصاريف تشغيلية",
                "أخرى"
            });
        }
    }
    
    private void AmountText_KeyPress(object? sender, KeyPressEventArgs e)
    {
        // Allow only numbers, decimal point, and control keys
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
        {
            e.Handled = true;
        }
        
        // Allow only one decimal point
        if (e.KeyChar == '.' && ((TextBox)sender!).Text.Contains('.'))
        {
            e.Handled = true;
        }
    }
    
    private void AmountText_TextChanged(object? sender, EventArgs e)
    {
        // حساب العمولة تلقائياً عند تغيير المبلغ
        if (_paymentMethodCombo.SelectedIndex == 4 && // إنستا باي
            decimal.TryParse(_amountText.Text, out decimal amount))
        {
            CalculateInstaPayCommission(amount);
        }
    }
    
    private void PaymentMethodCombo_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // إظهار/إخفاء حقول العمولة عند اختيار إنستا باي
        bool isInstaPay = _paymentMethodCombo.SelectedIndex == 4; // إنستا باي
        
        _commissionLabel.Visible = isInstaPay;
        _commissionText.Visible = isInstaPay;
        _netAmountLabel.Visible = isInstaPay;
        _netAmountText.Visible = isInstaPay;
        
        // حساب العمولة إذا كان هناك مبلغ
        if (isInstaPay && decimal.TryParse(_amountText.Text, out decimal amount))
        {
            CalculateInstaPayCommission(amount);
        }
        else
        {
            _commissionText.Text = "";
            _netAmountText.Text = "";
        }
    }
    
    private void CalculateInstaPayCommission(decimal amount)
    {
        decimal commission = 0;
        
        if (amount <= 0)
        {
            _commissionText.Text = "0.00";
            _netAmountText.Text = amount.ToString("N2");
            return;
        }
        
        // ✅ حساب العمولة للمصروفات حسب قواعد InstaPay الجديدة:
        // - من 100 إلى 500 جنيه → 0.50 جنيه (50 قرش)
        // - من 500.01 إلى 1000 جنيه → 0.50 + (0.10 لكل زيادة)
        //   مثال: 600 جنيه = 0.50 + 0.10 = 0.60 جنيه
        //   مثال: 700 جنيه = 0.50 + 0.20 = 0.70 جنيه
        //   مثال: 1000 جنيه = 0.50 + 0.50 = 1.00 جنيه
        // - فوق 1000 جنيه → نفس المعادلة ولكن بحد أقصى 20 جنيه
        
        if (amount >= 100m && amount <= 500m)
        {
            // من 100 إلى 500 جنيه: عمولة ثابتة 50 قرش
            commission = 0.50m;
        }
        else if (amount > 500m && amount <= 1000m)
        {
            // من 500.01 إلى 1000 جنيه:
            // العمولة الأساسية 0.50 + (0.10 لكل 100 جنيه زيادة عن 500)
            decimal extraAmount = amount - 500m;
            decimal extraCommission = Math.Ceiling(extraAmount / 100m) * 0.10m;
            commission = 0.50m + extraCommission;
        }
        else if (amount > 1000m)
        {
            // فوق 1000 جنيه: نفس المعادلة
            decimal extraAmount = amount - 500m;
            decimal extraCommission = Math.Ceiling(extraAmount / 100m) * 0.10m;
            commission = 0.50m + extraCommission;
        }
        else
        {
            // أقل من 100 جنيه: لا توجد عمولة
            commission = 0.00m;
        }
        
        // تطبيق الحد الأقصى: 20 جنيه فقط
        if (commission > 20m)
        {
            commission = 20m;
        }
        
        // حساب صافي المبلغ (بعد خصم العمولة للمصروفات)
        decimal netAmount = amount - commission;
        
        _commissionText.Text = commission.ToString("N2");
        _netAmountText.Text = netAmount.ToString("N2");
    }
    
    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        if (!_isDataLoaded || _originalTransaction == null)
        {
            MessageBox.Show("يرجى الانتظار حتى تكتمل عملية تحميل البيانات", "تنبيه",
                MessageBoxButtons.OK, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            return;
        }
        
        // Validate inputs
        if (string.IsNullOrWhiteSpace(_amountText.Text) || 
            !decimal.TryParse(_amountText.Text, out decimal amount) || amount <= 0)
        {
            ShowError("برجاء إدخال مبلغ صحيح أكبر من صفر");
            _amountText.Focus();
            return;
        }
        
        if (string.IsNullOrWhiteSpace(_descriptionText.Text))
        {
            ShowError("برجاء إدخال وصف العملية");
            _descriptionText.Focus();
            return;
        }
        
        try
        {
            _saveButton.Enabled = false;
            _saveButton.Text = "⏳ جاري الحفظ...";
            
            System.Diagnostics.Debug.WriteLine($"=== بدء حفظ التعديلات ===");
            System.Diagnostics.Debug.WriteLine($"Transaction ID: {_originalTransaction.Id}");
            System.Diagnostics.Debug.WriteLine($"المبلغ القديم: {_originalTransaction.Amount}");
            System.Diagnostics.Debug.WriteLine($"المبلغ الجديد: {amount}");
            
            // Update transaction
            _originalTransaction.TransactionDate = DateTime.SpecifyKind(_datePicker.Value, DateTimeKind.Utc);
            _originalTransaction.Category = _categoryCombo.SelectedItem?.ToString() ?? "";
            _originalTransaction.Description = _descriptionText.Text;
            _originalTransaction.Amount = amount;
            _originalTransaction.Notes = _notesText.Text;
            _originalTransaction.PaymentMethod = GetPaymentMethod();
            
            System.Diagnostics.Debug.WriteLine($"الوصف الجديد: {_originalTransaction.Description}");
            System.Diagnostics.Debug.WriteLine($"طريقة الدفع: {_originalTransaction.PaymentMethod}");
            
            // ✅ تحديث العمولة فقط إذا كانت طريقة الدفع InstaPay
            if (GetPaymentMethod() == PaymentMethod.InstaPay && 
                decimal.TryParse(_commissionText.Text, out decimal commission))
            {
                _originalTransaction.InstaPayCommission = commission;
                System.Diagnostics.Debug.WriteLine($"عمولة InstaPay: {commission}");
            }
            else
            {
                _originalTransaction.InstaPayCommission = null;
            }
            
            // Save to database
            System.Diagnostics.Debug.WriteLine($"استدعاء UpdateTransactionAsync...");
            await _cashBoxService.UpdateTransactionAsync(_originalTransaction);
            System.Diagnostics.Debug.WriteLine($"✅ تم الحفظ بنجاح في قاعدة البيانات");
            
            TransactionUpdated = true;
            
            string transactionTypeAr = _originalTransaction.Type == TransactionType.Income ? "الإيراد" : "المصروف";
            
            string successMessage = $"✅ تم تعديل {transactionTypeAr} بنجاح\n\n" +
                $"رقم السند: {_originalTransaction.VoucherNumber}\n" +
                $"المبلغ الجديد: {amount:N2} جنيه";
            
            // ملاحظة: إذا كانت طريقة الدفع InstaPay، يجب إضافة العمولة يدوياً كمصروف منفصل
            if (_originalTransaction.Type == TransactionType.Income && 
                GetPaymentMethod() == PaymentMethod.InstaPay && 
                _originalTransaction.InstaPayCommission.HasValue && 
                _originalTransaction.InstaPayCommission.Value > 0)
            {
                successMessage += $"\n\n⚠️ تنبيه: يرجى إضافة عمولة InstaPay ({_originalTransaction.InstaPayCommission.Value:N2} جنيه) كمصروف منفصل.";
            }
            
            MessageBox.Show(
                successMessage, 
                "تم الحفظ بنجاح",
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            
            System.Diagnostics.Debug.WriteLine($"تعيين DialogResult = OK وإغلاق النافذة");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (InvalidOperationException ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ InvalidOperationException: {ex.Message}");
            ShowError(ex.Message);
            _saveButton.Enabled = true;
            _saveButton.Text = "💾 حفظ التعديلات";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Exception: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            ShowError($"حدث خطأ أثناء الحفظ:\n{ex.Message}");
            _saveButton.Enabled = true;
            _saveButton.Text = "💾 حفظ التعديلات";
        }
    }
    
    private PaymentMethod GetPaymentMethod()
    {
        return _paymentMethodCombo.SelectedIndex switch
        {
            0 => PaymentMethod.Cash,
            1 => PaymentMethod.Cheque,
            2 => PaymentMethod.BankTransfer,
            3 => PaymentMethod.CreditCard,
            4 => PaymentMethod.InstaPay,
            5 => PaymentMethod.Other,
            _ => PaymentMethod.Cash
        };
    }
    
    private void ShowError(string message)
    {
        MessageBox.Show(message, "تنبيه",
            MessageBoxButtons.OK, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button1,
            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
    }
}
