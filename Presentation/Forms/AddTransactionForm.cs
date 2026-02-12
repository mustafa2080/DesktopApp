using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Application.Services;
using System.Linq;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class AddTransactionForm : Form
{
    private readonly string _transactionType; // "Income" or "Expense"
    private readonly int _cashBoxId;
    private readonly ICashBoxService _cashBoxService;
    private readonly int _userId;
    
    // Controls
    private Panel _mainPanel = null!;
    private Label _titleLabel = null!;
    private DateTimePicker _datePicker = null!;
    private TextBox _voucherNumberText = null!;
    private ComboBox _categoryCombo = null!;
    private ComboBox _currencyCombo = null!;
    private TextBox _exchangeRateText = null!;
    private Label _exchangeRateLabel = null!;
    private Label _amountLabel = null!;  // ✅ إضافة Label منفصل للمبلغ لسهولة التحديث
    private TextBox _amountText = null!;
    private Label _equivalentAmountLabel = null!;  // ✅ Label لعرض المبلغ المعادل بالجنيه
    private TextBox _descriptionText = null!;
    private ComboBox _paymentMethodCombo = null!;
    private Label _commissionLabel = null!;
    private TextBox _commissionText = null!;
    private Label _netAmountLabel = null!;
    private TextBox _netAmountText = null!;
    private TextBox _notesText = null!;
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    
    public bool TransactionSaved { get; private set; }
    
    public AddTransactionForm(string transactionType, int cashBoxId, 
        ICashBoxService cashBoxService, int userId)
    {
        _transactionType = transactionType;
        _cashBoxId = cashBoxId;
        _cashBoxService = cashBoxService;
        _userId = userId;
        
        InitializeComponent();
        InitializeCustomControls();
        LoadCategories();
        _ = LoadNextReceiptNumberAsync();
    }
    
    private void InitializeComponent()
    {
        this.Text = _transactionType == "Income" ? "إضافة إيراد" : "إضافة مصروف";
        this.Size = new Size(700, 800);
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
            Padding = new Padding(30, 20, 30, 20),
            AutoScroll = true
        };
        
        int yPos = 15;
        int controlWidth = 620;
        int rightMargin = 15;
        
        // Title with icon and background panel
        Panel titlePanel = new Panel
        {
            Size = new Size(controlWidth, 60),
            Location = new Point(rightMargin, yPos),
            BackColor = _transactionType == "Income" ? 
                Color.FromArgb(240, 255, 240) : Color.FromArgb(255, 245, 245)
        };
        
        _titleLabel = new Label
        {
            Text = _transactionType == "Income" ? "➕ إضافة إيراد جديد" : "➖ إضافة مصروف جديد",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = _transactionType == "Income" ? ColorScheme.Success : ColorScheme.Error,
            Size = new Size(controlWidth - 20, 60),
            Location = new Point(10, 0),
            TextAlign = ContentAlignment.MiddleRight
        };
        titlePanel.Controls.Add(_titleLabel);
        _mainPanel.Controls.Add(titlePanel);
        yPos += 70;
        
        // Separator line
        Panel separator = new Panel
        {
            Size = new Size(controlWidth, 1),
            Location = new Point(rightMargin, yPos),
            BackColor = Color.FromArgb(220, 220, 220)
        };
        _mainPanel.Controls.Add(separator);
        yPos += 15;
        
        // التاريخ
        yPos = AddFormField("📅 التاريخ:", yPos, rightMargin, controlWidth, () =>
        {
            _datePicker = new DateTimePicker
            {
                Font = new Font("Cairo", 10.5F),
                Size = new Size(controlWidth, 32),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now,
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = false
            };
            return _datePicker;
        });
        
        // رقم الإيصال
        yPos = AddFormField("🔢 رقم الإيصال:", yPos, rightMargin, controlWidth, () =>
        {
            _voucherNumberText = new TextBox
            {
                Font = new Font("Cairo", 10.5F),
                Size = new Size(controlWidth, 32),
                TextAlign = HorizontalAlignment.Right,
                ReadOnly = false,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            return _voucherNumberText;
        });
        
        // التصنيف
        yPos = AddFormField("📋 التصنيف:", yPos, rightMargin, controlWidth, () =>
        {
            _categoryCombo = new ComboBox
            {
                Font = new Font("Cairo", 10.5F),
                Size = new Size(controlWidth, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                RightToLeft = RightToLeft.Yes,
                FlatStyle = FlatStyle.Flat
            };
            return _categoryCombo;
        });
        
        // العملة
        yPos = AddFormField("💱 العملة:", yPos, rightMargin, controlWidth, () =>
        {
            _currencyCombo = new ComboBox
            {
                Font = new Font("Cairo", 10.5F, FontStyle.Bold),
                Size = new Size(controlWidth, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                RightToLeft = RightToLeft.Yes,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(250, 250, 255)
            };
            _currencyCombo.Items.AddRange(new object[]
            {
                "جنيه مصري (EGP)",
                "دولار أمريكي (USD)",
                "يورو (EUR)",
                "جنيه إسترليني (GBP)",
                "ريال سعودي (SAR)"
            });
            _currencyCombo.SelectedIndex = 0;
            _currencyCombo.SelectedIndexChanged += CurrencyCombo_SelectedIndexChanged;
            return _currencyCombo;
        });
        
        // سعر الصرف (مخفي افتراضياً) - للمعلومات فقط
        _exchangeRateLabel = new Label
        {
            Text = "💲 سعر الصرف (للمعلومات فقط):",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(50, 50, 50),
            Size = new Size(controlWidth, 30),
            Location = new Point(rightMargin, yPos),
            TextAlign = ContentAlignment.MiddleRight,
            Visible = false
        };
        _mainPanel.Controls.Add(_exchangeRateLabel);
        
        _exchangeRateText = new TextBox
        {
            Font = new Font("Cairo", 10.5F, FontStyle.Bold),
            Size = new Size(controlWidth, 32),
            Location = new Point(rightMargin, yPos + 35),
            TextAlign = HorizontalAlignment.Right,
            PlaceholderText = "مثال: 50 (اختياري)",
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(255, 250, 240),
            Visible = false
        };
        _exchangeRateText.KeyPress += AmountText_KeyPress;
        _exchangeRateText.TextChanged += ExchangeRateText_TextChanged;
        _mainPanel.Controls.Add(_exchangeRateText);
        
        // ✅ المبلغ مع Label منفصل قابل للتحديث
        _amountLabel = new Label
        {
            Text = "💰 المبلغ (بالجنيه المصري):",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(60, 60, 60),
            Size = new Size(controlWidth, 24),
            Location = new Point(rightMargin, yPos),
            TextAlign = ContentAlignment.MiddleRight
        };
        _mainPanel.Controls.Add(_amountLabel);
        yPos += 28;
        
        _amountText = new TextBox
        {
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Size = new Size(controlWidth, 35),
            Location = new Point(rightMargin, yPos),
            TextAlign = HorizontalAlignment.Right,
            PlaceholderText = "أدخل المبلغ",
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(252, 252, 252)
        };
        _amountText.KeyPress += AmountText_KeyPress;
        _amountText.TextChanged += AmountText_TextChanged;
        _mainPanel.Controls.Add(_amountText);
        yPos += _amountText.Height + 8;
        
        // ✅ Label لعرض المبلغ المعادل (مخفي افتراضياً)
        _equivalentAmountLabel = new Label
        {
            Text = "",
            Font = new Font("Cairo", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 130, 180),
            Size = new Size(controlWidth, 24),
            Location = new Point(rightMargin, yPos),
            TextAlign = ContentAlignment.MiddleRight,
            Visible = false
        };
        _mainPanel.Controls.Add(_equivalentAmountLabel);
        yPos += 18;
        
        // طريقة الدفع
        yPos = AddFormField("💳 طريقة الدفع:", yPos, rightMargin, controlWidth, () =>
        {
            _paymentMethodCombo = new ComboBox
            {
                Font = new Font("Cairo", 10.5F),
                Size = new Size(controlWidth, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                RightToLeft = RightToLeft.Yes,
                FlatStyle = FlatStyle.Flat
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
            _paymentMethodCombo.SelectedIndex = 0;
            _paymentMethodCombo.SelectedIndexChanged += PaymentMethodCombo_SelectedIndexChanged;
            return _paymentMethodCombo;
        });
        
        // عمولة إنستا باي (مخفية افتراضياً)
        Panel instaPayPanel = new Panel
        {
            Size = new Size(controlWidth, 160),
            Location = new Point(rightMargin, yPos),
            BackColor = Color.FromArgb(250, 250, 255),
            BorderStyle = BorderStyle.FixedSingle,
            Visible = false,
            Tag = "InstaPayPanel"
        };
        
        int instaY = 10;
        
        Label instaInfoLabel = new Label
        {
            Text = _transactionType == "Expense" 
                ? "ℹ️ عمولة إنستا باي (سيتم خصمها من المبلغ):" 
                : "ℹ️ عند اختيار إنستا باي للإيرادات، لا تُحسب عمولة:",
            Font = new Font("Cairo", 9.5F, FontStyle.Bold),
            ForeColor = _transactionType == "Expense" 
                ? Color.FromArgb(70, 70, 150) 
                : Color.FromArgb(50, 150, 50),
            Size = new Size(controlWidth - 20, 25),
            Location = new Point(10, instaY),
            TextAlign = ContentAlignment.MiddleRight
        };
        instaPayPanel.Controls.Add(instaInfoLabel);
        instaY += 30;
        
        _commissionLabel = new Label
        {
            Text = "قيمة العمولة:",
            Font = new Font("Cairo", 9.5F),
            ForeColor = Color.FromArgb(80, 80, 80),
            Size = new Size(150, 25),
            Location = new Point(controlWidth - 160, instaY),
            TextAlign = ContentAlignment.MiddleRight
        };
        instaPayPanel.Controls.Add(_commissionLabel);
        
        _commissionText = new TextBox
        {
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Size = new Size(controlWidth - 170, 28),
            Location = new Point(10, instaY),
            TextAlign = HorizontalAlignment.Right,
            ReadOnly = true,
            BackColor = Color.FromArgb(255, 250, 250),
            ForeColor = Color.FromArgb(200, 50, 50),
            PlaceholderText = "0.00",
            BorderStyle = BorderStyle.FixedSingle
        };
        instaPayPanel.Controls.Add(_commissionText);
        instaY += 35;
        
        Panel separatorLine = new Panel
        {
            Size = new Size(controlWidth - 20, 1),
            Location = new Point(10, instaY),
            BackColor = Color.FromArgb(200, 200, 220)
        };
        instaPayPanel.Controls.Add(separatorLine);
        instaY += 8;
        
        _netAmountLabel = new Label
        {
            Text = _transactionType == "Expense" 
                ? "💵 المبلغ بعد إضافة العمولة:" 
                : "💵 المبلغ الإجمالي:",
            Font = new Font("Cairo", 9.5F, FontStyle.Bold),
            ForeColor = _transactionType == "Expense" ? Color.FromArgb(220, 53, 69) : ColorScheme.Success,
            Size = new Size(190, 25),
            Location = new Point(controlWidth - 200, instaY),
            TextAlign = ContentAlignment.MiddleRight
        };
        instaPayPanel.Controls.Add(_netAmountLabel);
        
        _netAmountText = new TextBox
        {
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Size = new Size(controlWidth - 210, 30),
            Location = new Point(10, instaY),
            TextAlign = HorizontalAlignment.Right,
            ReadOnly = true,
            BackColor = _transactionType == "Expense" 
                ? Color.FromArgb(255, 245, 245) 
                : Color.FromArgb(240, 255, 240),
            ForeColor = _transactionType == "Expense" ? Color.FromArgb(220, 53, 69) : ColorScheme.Success,
            BorderStyle = BorderStyle.FixedSingle
        };
        instaPayPanel.Controls.Add(_netAmountText);
        
        _mainPanel.Controls.Add(instaPayPanel);
        yPos += 0; // لا نضيف مسافة عندما يكون مخفياً
        
        // الوصف
        yPos = AddFormField("📝 الوصف:", yPos, rightMargin, controlWidth, () =>
        {
            _descriptionText = new TextBox
            {
                Font = new Font("Cairo", 10.5F),
                Size = new Size(controlWidth, 32),
                TextAlign = HorizontalAlignment.Right,
                BorderStyle = BorderStyle.FixedSingle
            };
            return _descriptionText;
        });
        
        // ملاحظات
        yPos = AddFormField("🗒️ ملاحظات (اختياري):", yPos, rightMargin, controlWidth, () =>
        {
            _notesText = new TextBox
            {
                Font = new Font("Cairo", 10F),
                Size = new Size(controlWidth, 70),
                Multiline = true,
                TextAlign = HorizontalAlignment.Right,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle
            };
            return _notesText;
        }, 0);
        
        yPos += 15;
        
        // Buttons Panel
        Panel buttonPanel = new Panel
        {
            Size = new Size(controlWidth, 50),
            Location = new Point(rightMargin, yPos),
            BackColor = Color.Transparent
        };
        
        _saveButton = new Button
        {
            Text = "💾 حفظ",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Size = new Size(295, 48),
            Location = new Point(0, 0),
            BackColor = _transactionType == "Income" ? ColorScheme.Success : ColorScheme.Error,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Right
        };
        _saveButton.FlatAppearance.BorderSize = 0;
        _saveButton.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        _saveButton.Click += SaveButton_Click;
        
        // Hover effect
        _saveButton.MouseEnter += (s, e) => 
        {
            _saveButton.BackColor = _transactionType == "Income" ? 
                Color.FromArgb(40, 167, 69) : Color.FromArgb(220, 53, 69);
        };
        _saveButton.MouseLeave += (s, e) => 
        {
            _saveButton.BackColor = _transactionType == "Income" ? 
                ColorScheme.Success : ColorScheme.Error;
        };
        buttonPanel.Controls.Add(_saveButton);
        
        _cancelButton = new Button
        {
            Text = "✖ إلغاء",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Size = new Size(295, 48),
            Location = new Point(315, 0),
            BackColor = Color.FromArgb(108, 117, 125),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Left
        };
        _cancelButton.FlatAppearance.BorderSize = 0;
        _cancelButton.Click += (s, e) => this.Close();
        
        // Hover effect
        _cancelButton.MouseEnter += (s, e) => _cancelButton.BackColor = Color.FromArgb(90, 98, 104);
        _cancelButton.MouseLeave += (s, e) => _cancelButton.BackColor = Color.FromArgb(108, 117, 125);
        buttonPanel.Controls.Add(_cancelButton);
        
        _mainPanel.Controls.Add(buttonPanel);
        this.Controls.Add(_mainPanel);
    }
    
    private int AddFormField(string labelText, int currentY, int rightMargin, 
        int controlWidth, Func<Control> createControl, int extraSpacing = 0)
    {
        // Label
        Label label = new Label
        {
            Text = labelText,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(60, 60, 60),
            Size = new Size(controlWidth, 24),
            Location = new Point(rightMargin, currentY),
            TextAlign = ContentAlignment.MiddleRight
        };
        _mainPanel.Controls.Add(label);
        currentY += 28;
        
        // Control
        var control = createControl();
        control.Location = new Point(rightMargin, currentY);
        _mainPanel.Controls.Add(control);
        currentY += control.Height + 18 + extraSpacing;
        
        return currentY;
    }
    
    private void LoadCategories()
    {
        if (_transactionType == "Income")
        {
            _categoryCombo.Items.AddRange(new object[]
            {
                "عمرة وحج",
                "سياحة داخلية",
                "سياحة خارجية",
                "عمولات",
                "طيران",
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
        
        if (_categoryCombo.Items.Count > 0)
            _categoryCombo.SelectedIndex = 0;
    }
    
    private async Task LoadNextReceiptNumberAsync()
    {
        try
        {
            // Get last voucher number for this cashbox
            var lastNumber = await _cashBoxService.GetLastVoucherNumberAsync(_cashBoxId);
            
            if (string.IsNullOrEmpty(lastNumber))
            {
                // First transaction - suggest starting number
                _voucherNumberText.Text = "1";
            }
            else
            {
                // Try to extract number and increment
                if (int.TryParse(lastNumber, out int number))
                {
                    _voucherNumberText.Text = (number + 1).ToString();
                }
                else
                {
                    // If not a pure number, suggest user to enter manually
                    _voucherNumberText.Text = "";
                    _voucherNumberText.PlaceholderText = "أدخل رقم الإيصال";
                }
            }
        }
        catch
        {
            // If error, let user enter manually
            _voucherNumberText.Text = "";
            _voucherNumberText.PlaceholderText = "أدخل رقم الإيصال";
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
        
        // تحديث المبلغ المعادل عند التغيير
        RecalculateAmount();
    }
    
    private void PaymentMethodCombo_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // البحث عن البانل الخاص بإنستا باي
        Panel? instaPayPanel = _mainPanel.Controls.OfType<Panel>()
            .FirstOrDefault(p => p.Tag?.ToString() == "InstaPayPanel");
        
        if (instaPayPanel == null) return;
        
        bool isInstaPay = _paymentMethodCombo.SelectedIndex == 4; // إنستا باي
        
        if (isInstaPay)
        {
            // ❌ إخفاء البانل للإيرادات - لا توجد عمولة
            if (_transactionType == "Income")
            {
                instaPayPanel.Visible = false;
                _commissionText.Text = "";
                _netAmountText.Text = "";
                return;
            }
            
            // ✅ إظهار البانل للمصروفات فقط وضبط موضعه بعد طريقة الدفع مباشرة
            instaPayPanel.Visible = true;
            instaPayPanel.Location = new Point(
                _paymentMethodCombo.Location.X, 
                _paymentMethodCombo.Location.Y + _paymentMethodCombo.Height + 10
            );
            
            // تحديث موضع الحقول التي بعد البانل
            int newY = instaPayPanel.Location.Y + instaPayPanel.Height + 18;
            UpdateControlsPositionAfter(instaPayPanel, newY);
            
            // حساب العمولة إذا كان هناك مبلغ
            if (decimal.TryParse(_amountText.Text, out decimal amount))
            {
                CalculateInstaPayCommission(amount);
            }
        }
        else
        {
            // إخفاء البانل
            int oldBottom = instaPayPanel.Location.Y + instaPayPanel.Height;
            instaPayPanel.Visible = false;
            
            // تحديث موضع الحقول التي بعد البانل لإزالة الفراغ
            int newY = _paymentMethodCombo.Location.Y + _paymentMethodCombo.Height + 18;
            UpdateControlsPositionAfter(instaPayPanel, newY);
            
            _commissionText.Text = "";
            _netAmountText.Text = "";
        }
    }
    
    private void UpdateControlsPositionAfter(Control referenceControl, int newStartY)
    {
        // الحصول على جميع الكونترولز مرتبة حسب Y
        var controlsToMove = _mainPanel.Controls.OfType<Control>()
            .Where(c => c != referenceControl && 
                       c.Location.Y > _paymentMethodCombo.Location.Y + _paymentMethodCombo.Height)
            .OrderBy(c => c.Location.Y)
            .ToList();
        
        int currentY = newStartY;
        
        foreach (var control in controlsToMove)
        {
            control.Location = new Point(control.Location.X, currentY);
            
            // إذا كان Label، نضيف مسافة أصغر قبل الكونترول التالي
            if (control is Label)
            {
                currentY += control.Height + 4;
            }
            else if (control is Panel && control.Controls.Count > 0) // Button Panel
            {
                currentY += control.Height + 20;
            }
            else
            {
                currentY += control.Height + 18;
            }
        }
    }
    
    private void CalculateInstaPayCommission(decimal amount)
    {
        // ❌ إلغاء حساب العمولة للإيرادات - العمولة تطبق على المصروفات فقط
        if (_transactionType == "Income")
        {
            // في الإيرادات: لا يوجد عمولة
            _commissionText.Text = "0.00";
            _netAmountText.Text = amount.ToString("N2");
            return;
        }
        
        decimal commission = 0;
        
        if (amount <= 0)
        {
            _commissionText.Text = "0.00";
            _netAmountText.Text = amount.ToString("N2");
            return;
        }
        
        // ✅ حساب العمولة للمصروفات حسب قواعد InstaPay الجديدة:
        if (amount >= 100m && amount <= 500m)
        {
            commission = 0.50m;
        }
        else if (amount > 500m && amount <= 1000m)
        {
            decimal extraAmount = amount - 500m;
            decimal extraCommission = Math.Ceiling(extraAmount / 100m) * 0.10m;
            commission = 0.50m + extraCommission;
        }
        else if (amount > 1000m)
        {
            decimal extraAmount = amount - 500m;
            decimal extraCommission = Math.Ceiling(extraAmount / 100m) * 0.10m;
            commission = 0.50m + extraCommission;
        }
        else
        {
            commission = 0.00m;
        }
        
        // تطبيق الحد الأقصى: 20 جنيه فقط
        if (commission > 20m)
        {
            commission = 20m;
        }
        
        // في المصروفات: نضيف العمولة على المبلغ الأساسي
        decimal netAmount = amount + commission;
        
        _commissionText.Text = commission.ToString("N2");
        _netAmountText.Text = netAmount.ToString("N2");
    }
    
    // ✅ دالة محدثة لتوضيح العملة المختارة
    private void CurrencyCombo_SelectedIndexChanged(object? sender, EventArgs e)
    {
        bool isForeignCurrency = _currencyCombo.SelectedIndex > 0; // 0 = EGP
        
        _exchangeRateLabel.Visible = isForeignCurrency;
        _exchangeRateText.Visible = isForeignCurrency;
        
        if (isForeignCurrency)
        {
            // تحديث label المبلغ ليوضح العملة المختارة بوضوح
            string currencyName = _currencyCombo.SelectedIndex switch
            {
                1 => "بالدولار الأمريكي (USD)",
                2 => "باليورو (EUR)",
                3 => "بالجنيه الإسترليني (GBP)",
                4 => "بالريال السعودي (SAR)",
                _ => "بالجنيه المصري"
            };
            
            _amountLabel.Text = $"💰 المبلغ {currencyName}:";
            _amountText.PlaceholderText = "أدخل المبلغ بالعملة المختارة";
            _amountText.BackColor = Color.FromArgb(255, 250, 240); // لون مميز للعملة الأجنبية
            
            // إضافة مسافة لحقل سعر الصرف
            int newY = _currencyCombo.Location.Y + _currencyCombo.Height + 18;
            _exchangeRateLabel.Location = new Point(_exchangeRateLabel.Location.X, newY);
            _exchangeRateText.Location = new Point(_exchangeRateText.Location.X, newY + 35);
            
            // تحديث موضع المبلغ (بدون Label المعادل)
            int amountY = newY + 35 + _exchangeRateText.Height + 18;
            _amountLabel.Location = new Point(_amountLabel.Location.X, amountY);
            _amountText.Location = new Point(_amountText.Location.X, amountY + 28);
            
            UpdateControlsPositionAfter(_amountText, amountY + 28 + _amountText.Height + 18);
            
            // تعيين سعر صرف افتراضي (للمعلومات فقط)
            SetDefaultExchangeRate();
        }
        else
        {
            _amountLabel.Text = "💰 المبلغ (بالجنيه المصري):";
            _amountText.PlaceholderText = "أدخل المبلغ";
            _amountText.BackColor = Color.FromArgb(252, 252, 252);
            _exchangeRateText.Text = "";
            
            // إعادة ترتيب الحقول لإزالة المسافة الإضافية
            int amountY = _currencyCombo.Location.Y + _currencyCombo.Height + 18;
            _amountLabel.Location = new Point(_amountLabel.Location.X, amountY);
            _amountText.Location = new Point(_amountText.Location.X, amountY + 28);
            
            UpdateControlsPositionAfter(_amountText, amountY + 28 + _amountText.Height + 18);
        }
        
        // إخفاء المعادل (لأننا لا نحتاجه)
        _equivalentAmountLabel.Visible = false;
    }
    
    private void ExchangeRateText_TextChanged(object? sender, EventArgs e)
    {
        RecalculateAmount();
    }
    
    private void SetDefaultExchangeRate()
    {
        // أسعار صرف افتراضية تقريبية
        _exchangeRateText.Text = _currencyCombo.SelectedIndex switch
        {
            1 => "50.00",  // USD
            2 => "55.00",  // EUR
            3 => "63.00",  // GBP
            4 => "13.33",  // SAR
            _ => "1.00"
        };
    }
    
    // ✅ دالة محدثة - إزالة المعادل لأن كل عملة تتسجل بنفسها
    private void RecalculateAmount()
    {
        // إخفاء Label المعادل لأننا لا نحتاجه
        _equivalentAmountLabel.Visible = false;
    }
    
    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(_voucherNumberText.Text))
        {
            ShowError("برجاء إدخال رقم السند");
            _voucherNumberText.Focus();
            return;
        }
        
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
            
            // ✅ التسجيل بنفس العملة المختارة (بدون تحويل)
            string currency = _currencyCombo.SelectedIndex switch
            {
                0 => "EGP",
                1 => "USD",
                2 => "EUR",
                3 => "GBP",
                4 => "SAR",
                _ => "EGP"
            };
            
            decimal? exchangeRate = null;
            
            // حفظ سعر الصرف للمعلومات فقط (مش للتحويل)
            if (_currencyCombo.SelectedIndex > 0 && decimal.TryParse(_exchangeRateText.Text, out decimal rate) && rate > 0)
            {
                exchangeRate = rate;
            }
            
            // Create transaction
            var transaction = new CashTransaction
            {
                CashBoxId = _cashBoxId,
                VoucherNumber = _voucherNumberText.Text,
                TransactionDate = DateTime.SpecifyKind(_datePicker.Value, DateTimeKind.Utc),
                Category = _categoryCombo.SelectedItem?.ToString() ?? "",
                Description = _descriptionText.Text,
                Amount = amount, // ✅ المبلغ بنفس العملة المختارة
                TransactionCurrency = currency,
                ExchangeRateUsed = exchangeRate, // للمعلومات فقط
                OriginalAmount = null, // مش محتاجين originalAmount لأننا مش بنحول
                Notes = _notesText.Text,
                PaymentMethod = GetPaymentMethod(),
                InstaPayCommission = null,
                CreatedBy = _userId
            };
            
            // ✅ في حالة المصروف مع InstaPay: تسجيل العمولة في نفس السند
            if (_transactionType == "Expense" && GetPaymentMethod() == PaymentMethod.InstaPay && 
                decimal.TryParse(_commissionText.Text, out decimal expenseCommission) && expenseCommission > 0)
            {
                transaction.InstaPayCommission = expenseCommission;
            }
            
            // Save to database
            if (_transactionType == "Income")
            {
                await _cashBoxService.AddIncomeAsync(transaction);
            }
            else
            {
                await _cashBoxService.AddExpenseAsync(transaction);
            }
            
            TransactionSaved = true;
            
            // ✅ رسالة النجاح بنفس العملة
            string currencySymbol = currency switch
            {
                "USD" => "$",
                "EUR" => "€",
                "GBP" => "£",
                "SAR" => "ريال",
                "EGP" => "جنيه",
                _ => ""
            };
            
            string currencyName = currency switch
            {
                "USD" => "دولار أمريكي",
                "EUR" => "يورو",
                "GBP" => "جنيه إسترليني",
                "SAR" => "ريال سعودي",
                "EGP" => "جنيه مصري",
                _ => ""
            };
            
            string successMessage = $"✅ تم حفظ {(_transactionType == "Income" ? "الإيراد" : "المصروف")} بنجاح\n\n" +
                $"رقم السند: {transaction.VoucherNumber}\n" +
                $"المبلغ: {amount:N2} {currencyName}\n";
            
            // إضافة سعر الصرف إذا كانت عملة أجنبية
            if (currency != "EGP" && exchangeRate.HasValue)
            {
                successMessage += $"سعر الصرف: {exchangeRate.Value:N2} جنيه/{currencyName}\n";
            }
            
            // إضافة رسالة عن العمولة في المصروف (بنفس العملة)
            if (_transactionType == "Expense" && GetPaymentMethod() == PaymentMethod.InstaPay && 
                decimal.TryParse(_commissionText.Text, out decimal comm) && comm > 0)
            {
                decimal netAmount = amount + comm;
                successMessage += $"عمولة إنستا باي: {comm:N2} {currencyName}\n";
                successMessage += $"المبلغ بعد إضافة العمولة: {netAmount:N2} {currencyName}\n";
            }
            
            successMessage += $"الرصيد الجديد: {transaction.BalanceAfter:N2} {currencyName}";
            
            MessageBox.Show(
                successMessage, 
                "تم الحفظ بنجاح",
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (InvalidOperationException ex)
        {
            ShowError(ex.Message);
            _saveButton.Enabled = true;
            _saveButton.Text = "💾 حفظ";
        }
        catch (Exception ex)
        {
            ShowError($"حدث خطأ أثناء الحفظ:\n{ex.Message}");
            _saveButton.Enabled = true;
            _saveButton.Text = "💾 حفظ";
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
