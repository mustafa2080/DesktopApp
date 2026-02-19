using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Application.Services;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class AddPaymentForm : Form
{
    private readonly IInvoiceService _invoiceService;
    private readonly ICashBoxService _cashBoxService;
    private readonly int _currentUserId;
    private readonly int? _salesInvoiceId;
    private readonly int? _purchaseInvoiceId;
    private readonly decimal _remainingAmount;
    private readonly string _invoiceNumber;
    
    private ComboBox _cashBoxCombo = null!;
    private TextBox _amountText = null!;
    private DateTimePicker _datePicker = null!;
    private ComboBox _methodCombo = null!;
    private TextBox _referenceText = null!;
    private TextBox _notesText = null!;
    
    private Label _invoiceInfoLabel = null!;
    private Label _remainingLabel = null!;
    
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    
    public AddPaymentForm(IInvoiceService invoiceService, ICashBoxService cashBoxService,
        int currentUserId, int? salesInvoiceId, int? purchaseInvoiceId, 
        decimal remainingAmount, string invoiceNumber)
    {
        _invoiceService = invoiceService;
        _cashBoxService = cashBoxService;
        _currentUserId = currentUserId;
        _salesInvoiceId = salesInvoiceId;
        _purchaseInvoiceId = purchaseInvoiceId;
        _remainingAmount = remainingAmount;
        _invoiceNumber = invoiceNumber;
        
        InitializeComponent();
        InitializeCustomControls();
        _ = LoadDataAsync();
    }
    
    private void InitializeComponent()
    {
        this.Text = "إضافة دفعة";
        this.Size = new Size(600, 600);
        this.StartPosition = FormStartPosition.CenterParent;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = Color.White;
        this.Font = new Font("Cairo", 10F);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
    }
    
    private void InitializeCustomControls()
    {
        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(30)
        };
        
        // Title
        Label titleLabel = new Label
        {
            Text = "💰 إضافة دفعة جديدة",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(30, 20)
        };
        mainPanel.Controls.Add(titleLabel);
        
        int yPos = 70;
        
        // Invoice Info
        Panel infoPanel = new Panel
        {
            Location = new Point(30, yPos),
            Size = new Size(520, 100),
            BackColor = Color.FromArgb(240, 248, 255),
            Padding = new Padding(15)
        };
        
        _invoiceInfoLabel = new Label
        {
            Text = $"الفاتورة: {_invoiceNumber}\nالنوع: {(_salesInvoiceId.HasValue ? "مبيعات" : "مشتريات")}",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(15, 10)
        };
        infoPanel.Controls.Add(_invoiceInfoLabel);
        
        _remainingLabel = new Label
        {
            Text = $"المبلغ المتبقي: {_remainingAmount:N2} جنيه",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = ColorScheme.Error,
            AutoSize = true,
            Location = new Point(15, 55)
        };
        infoPanel.Controls.Add(_remainingLabel);
        
        mainPanel.Controls.Add(infoPanel);
        
        yPos += 120;
        
        // CashBox
        Label cashBoxLabel = CreateLabel("الخزنة:", new Point(30, yPos));
        mainPanel.Controls.Add(cashBoxLabel);
        
        _cashBoxCombo = new ComboBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(350, 30),
            Location = new Point(180, yPos),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        mainPanel.Controls.Add(_cashBoxCombo);
        
        yPos += 50;
        
        // Amount
        Label amountLabel = CreateLabel("المبلغ:", new Point(30, yPos));
        mainPanel.Controls.Add(amountLabel);
        
        _amountText = new TextBox
        {
            Font = new Font("Cairo", 12F),
            Size = new Size(200, 30),
            Location = new Point(180, yPos),
            Text = _remainingAmount.ToString("F2")
        };
        mainPanel.Controls.Add(_amountText);
        
        Button fullAmountBtn = new Button
        {
            Text = "المبلغ الكامل",
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            Size = new Size(120, 30),
            Location = new Point(400, yPos),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        fullAmountBtn.FlatAppearance.BorderSize = 0;
        fullAmountBtn.Click += (s, e) => _amountText.Text = _remainingAmount.ToString("F2");
        mainPanel.Controls.Add(fullAmountBtn);
        
        yPos += 50;
        
        // Date
        Label dateLabel = CreateLabel("التاريخ:", new Point(30, yPos));
        mainPanel.Controls.Add(dateLabel);
        
        _datePicker = new DateTimePicker
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(200, 30),
            Location = new Point(180, yPos),
            Format = DateTimePickerFormat.Short
        };
        mainPanel.Controls.Add(_datePicker);
        
        yPos += 50;
        
        // Payment Method
        Label methodLabel = CreateLabel("طريقة الدفع:", new Point(30, yPos));
        mainPanel.Controls.Add(methodLabel);
        
        _methodCombo = new ComboBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(200, 30),
            Location = new Point(180, yPos),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _methodCombo.Items.AddRange(new object[] { "نقدي", "شيك", "تحويل بنكي", "بطاقة" });
        _methodCombo.SelectedIndex = 0;
        mainPanel.Controls.Add(_methodCombo);
        
        yPos += 50;
        
        // Reference Number
        Label refLabel = CreateLabel("رقم المرجع:", new Point(30, yPos));
        mainPanel.Controls.Add(refLabel);
        
        _referenceText = new TextBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(350, 30),
            Location = new Point(180, yPos),
            PlaceholderText = "رقم الشيك أو التحويل (اختياري)"
        };
        mainPanel.Controls.Add(_referenceText);
        
        yPos += 50;
        
        // Notes
        Label notesLabel = CreateLabel("ملاحظات:", new Point(30, yPos));
        mainPanel.Controls.Add(notesLabel);
        
        _notesText = new TextBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(350, 60),
            Location = new Point(180, yPos),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical
        };
        mainPanel.Controls.Add(_notesText);
        
        yPos += 80;
        
        // Buttons
        _saveButton = CreateButton("💾 حفظ الدفعة", ColorScheme.Success, new Point(180, yPos), SavePayment_Click);
        mainPanel.Controls.Add(_saveButton);
        
        _cancelButton = CreateButton("❌ إلغاء", ColorScheme.Error, new Point(360, yPos), (s, e) => this.Close());
        mainPanel.Controls.Add(_cancelButton);
        
        this.Controls.Add(mainPanel);
    }
    
    private Label CreateLabel(string text, Point location)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = location
        };
    }
    
    private Button CreateButton(string text, Color bgColor, Point location, EventHandler clickHandler)
    {
        Button btn = new Button
        {
            Text = text,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Size = new Size(160, 40),
            Location = location,
            BackColor = bgColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += clickHandler;
        return btn;
    }
    
    private async Task LoadDataAsync()
    {
        try
        {
            var cashBoxes = await _cashBoxService.GetActiveCashBoxesAsync();
            _cashBoxCombo.DisplayMember = "Name";
            _cashBoxCombo.ValueMember = "Id";
            _cashBoxCombo.DataSource = cashBoxes;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async void SavePayment_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_cashBoxCombo.SelectedIndex == -1)
            {
                MessageBox.Show("برجاء اختيار الخزنة", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (!decimal.TryParse(_amountText.Text.Replace(",", ""), 
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal amount) || amount <= 0)
            {
                MessageBox.Show("برجاء إدخال مبلغ صحيح", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (amount > _remainingAmount)
            {
                var result = MessageBox.Show(
                    $"المبلغ المدخل ({amount:N2}) أكبر من المبلغ المتبقي ({_remainingAmount:N2})\nهل تريد المتابعة؟",
                    "تنبيه",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                    
                if (result == DialogResult.No)
                    return;
            }
            
            string paymentMethod = _methodCombo.SelectedItem?.ToString() ?? "نقدي";
            string methodEn = paymentMethod switch
            {
                "نقدي" => "Cash",
                "شيك" => "Check",
                "تحويل بنكي" => "Bank",
                "بطاقة" => "Card",
                _ => "Cash"
            };
            
            var payment = new InvoicePayment
            {
                SalesInvoiceId = _salesInvoiceId,
                PurchaseInvoiceId = _purchaseInvoiceId,
                CashBoxId = (int)_cashBoxCombo.SelectedValue!,
                Amount = amount,
                PaymentDate = _datePicker.Value,
                PaymentMethod = methodEn,
                ReferenceNumber = _referenceText.Text,
                Notes = _notesText.Text,
                CreatedBy = _currentUserId
            };
            
            await _invoiceService.AddPaymentAsync(payment);
            
            MessageBox.Show("تم حفظ الدفعة بنجاح وتم تحديث الخزنة", "نجح", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في حفظ الدفعة: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
