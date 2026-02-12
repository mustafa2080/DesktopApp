using System;
using System.Drawing;
using System.Windows.Forms;
using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Presentation;

namespace GraceWay.AccountingSystem.Presentation.Forms
{
    public partial class AddEditFlightBookingForm : Form
    {
        private readonly IFlightBookingService _flightBookingService;
        private FlightBooking? _currentBooking;
        private readonly int? _bookingId;

        // Controls
        private Label lblTitle = null!;
        private TextBox txtBookingNumber = null!;
        private DateTimePicker dtpIssuanceDate = null!;
        private DateTimePicker dtpTravelDate = null!;
        private TextBox txtClientName = null!;
        private TextBox txtClientRoute = null!;
        private TextBox txtSupplier = null!;
        private ComboBox cmbSystem = null!;
        private ComboBox cmbTicketStatus = null!;
        private ComboBox cmbPaymentMethod = null!;
        private NumericUpDown numSellingPrice = null!;
        private NumericUpDown numNetPrice = null!;
        private Label lblProfit = null!;
        private NumericUpDown numTicketCount = null!;
        private TextBox txtTicketNumbers = null!;
        private TextBox txtMobileNumber = null!;
        private TextBox txtNotes = null!;
        private Button btnSave = null!;
        private Button btnCancel = null!;

        public AddEditFlightBookingForm(IFlightBookingService flightBookingService, int? bookingId = null)
        {
            InitializeComponent();
            _flightBookingService = flightBookingService;
            _bookingId = bookingId;

            InitializeFormControls();
            LoadDataAsync();
        }

        private void InitializeFormControls()
        {
            this.Size = new Size(900, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            int labelX = 750;
            int controlX = 450;
            int controlWidth = 280;
            int startY = 70;
            int rowHeight = 45;
            int currentY = startY;

            // Title
            lblTitle = new Label
            {
                Text = "حجز طيران جديد",
                Location = new Point(350, 20),
                Size = new Size(200, 35),
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Row 1: رقم الحجز + تاريخ الإصدار
            var lblBookingNumber = CreateLabel("رقم الحجز:", labelX, currentY);
            txtBookingNumber = CreateTextBox(controlX, currentY - 3, controlWidth, true);

            var lblIssuanceDate = CreateLabel("تاريخ الإصدار:", 350, currentY);
            dtpIssuanceDate = CreateDatePicker(120, currentY - 3, 220);

            // Row 2: تاريخ السفر + عدد التذاكر
            currentY += rowHeight;
            var lblTravelDate = CreateLabel("تاريخ السفر: *", labelX, currentY);
            dtpTravelDate = CreateDatePicker(controlX, currentY - 3, controlWidth);

            var lblTicketCount = CreateLabel("عدد التذاكر: *", 350, currentY);
            numTicketCount = CreateNumericUpDown(120, currentY - 3, 220, 1, 100, 1);
            numTicketCount.ValueChanged += CalculateProfit;

            // Row 3: اسم العميل
            currentY += rowHeight;
            var lblClientName = CreateLabel("اسم العميل: *", labelX, currentY);
            txtClientName = CreateTextBox(120, currentY - 3, 610);

            // Row 4: مسار العميل
            currentY += rowHeight;
            var lblClientRoute = CreateLabel("مسار العميل:", labelX, currentY);
            txtClientRoute = CreateTextBox(120, currentY - 3, 610);

            // Row 5: المورد + النظام
            currentY += rowHeight;
            var lblSupplier = CreateLabel("المورد: *", labelX, currentY);
            txtSupplier = CreateTextBox(controlX, currentY - 3, controlWidth);

            var lblSystem = CreateLabel("النظام: *", 350, currentY);
            cmbSystem = CreateComboBox(120, currentY - 3, 220);
            cmbSystem.Items.AddRange(new[] { "Sabre", "Amadeus", "Galileo", "Worldspan", "آخر" });

            // Row 6: حالة التذكرة + طريقة الدفع
            currentY += rowHeight;
            var lblTicketStatus = CreateLabel("حالة التذكرة: *", labelX, currentY);
            cmbTicketStatus = CreateComboBox(controlX, currentY - 3, controlWidth);
            cmbTicketStatus.Items.AddRange(new[] { "مؤكد", "قيد الانتظار", "ملغي", "مكتمل" });

            var lblPaymentMethod = CreateLabel("طريقة الدفع: *", 350, currentY);
            cmbPaymentMethod = CreateComboBox(120, currentY - 3, 220);
            cmbPaymentMethod.Items.AddRange(new[] { "نقدي", "بطاقة ائتمان", "تحويل بنكي", "شيك", "آجل" });

            // Row 7: سعر البيع + السعر الصافي
            currentY += rowHeight;
            var lblSellingPrice = CreateLabel("سعر بيع التذكرة: *", labelX, currentY);
            numSellingPrice = CreateNumericUpDown(controlX, currentY - 3, controlWidth, 0, 999999999, 0);
            numSellingPrice.DecimalPlaces = 2;
            numSellingPrice.ValueChanged += CalculateProfit;

            var lblNetPrice = CreateLabel("سعر شراء التذكرة: *", 350, currentY);
            numNetPrice = CreateNumericUpDown(120, currentY - 3, 220, 0, 999999999, 0);
            numNetPrice.DecimalPlaces = 2;
            numNetPrice.ValueChanged += CalculateProfit;

            // Row 8: الربح الإجمالي
            currentY += rowHeight;
            var lblProfitTitle = CreateLabel("الربح الإجمالي:", labelX, currentY);
            lblProfitTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            
            lblProfit = CreateLabel("0.00 جنيه", controlX, currentY);
            lblProfit.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblProfit.ForeColor = ColorScheme.Success;

            // Row 9: رقم الموبايل
            currentY += rowHeight;
            var lblMobileNumber = CreateLabel("رقم الموبايل: *", labelX, currentY);
            txtMobileNumber = CreateTextBox(controlX, currentY - 3, controlWidth);

            // Row 10: أرقام التذاكر
            currentY += rowHeight;
            var lblTicketNumbers = CreateLabel("أرقام التذاكر:", labelX, currentY);
            txtTicketNumbers = CreateTextBox(120, currentY - 3, 610);
            txtTicketNumbers.PlaceholderText = "أدخل أرقام التذاكر مفصولة بفاصلة";

            // Row 11: ملاحظات
            currentY += rowHeight;
            var lblNotes = CreateLabel("ملاحظات:", labelX, currentY);
            txtNotes = new TextBox
            {
                Location = new Point(120, currentY - 3),
                Size = new Size(610, 60),
                Font = new Font("Segoe UI", 10F),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            // Buttons
            currentY += 80;
            btnSave = CreateButton("حفظ", 500, currentY, 150, 40, ColorScheme.Success);
            btnSave.Click += BtnSave_Click;

            btnCancel = CreateButton("إلغاء", 330, currentY, 150, 40, ColorScheme.Secondary);
            btnCancel.Click += (s, e) => this.Close();

            // Add controls
            this.Controls.AddRange(new Control[] {
                lblTitle, lblBookingNumber, txtBookingNumber, lblIssuanceDate, dtpIssuanceDate,
                lblTravelDate, dtpTravelDate, lblTicketCount, numTicketCount,
                lblClientName, txtClientName, lblClientRoute, txtClientRoute,
                lblSupplier, txtSupplier, lblSystem, cmbSystem,
                lblTicketStatus, cmbTicketStatus, lblPaymentMethod, cmbPaymentMethod,
                lblSellingPrice, numSellingPrice, lblNetPrice, numNetPrice,
                lblProfitTitle, lblProfit, lblMobileNumber, txtMobileNumber,
                lblTicketNumbers, txtTicketNumbers, lblNotes, txtNotes,
                btnSave, btnCancel
            });
        }

        private async void LoadDataAsync()
        {
            try
            {
                if (_bookingId.HasValue)
                {
                    _currentBooking = await _flightBookingService.GetFlightBookingByIdAsync(_bookingId.Value);
                    if (_currentBooking != null)
                    {
                        LoadBookingData();
                        lblTitle.Text = "تعديل حجز طيران";
                    }
                }
                else
                {
                    txtBookingNumber.Text = await _flightBookingService.GenerateBookingNumberAsync();
                    cmbSystem.SelectedIndex = 0;
                    cmbTicketStatus.SelectedIndex = 0;
                    cmbPaymentMethod.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBookingData()
        {
            if (_currentBooking == null) return;

            txtBookingNumber.Text = _currentBooking.BookingNumber;
            dtpIssuanceDate.Value = _currentBooking.IssuanceDate.ToLocalTime();
            dtpTravelDate.Value = _currentBooking.TravelDate.ToLocalTime();
            txtClientName.Text = _currentBooking.ClientName;
            txtClientRoute.Text = _currentBooking.ClientRoute;
            txtSupplier.Text = _currentBooking.Supplier;
            cmbSystem.Text = _currentBooking.System;
            cmbTicketStatus.Text = _currentBooking.TicketStatus;
            cmbPaymentMethod.Text = _currentBooking.PaymentMethod;
            numSellingPrice.Value = _currentBooking.SellingPrice;
            numNetPrice.Value = _currentBooking.NetPrice;
            numTicketCount.Value = _currentBooking.TicketCount;
            txtTicketNumbers.Text = _currentBooking.TicketNumbers;
            txtMobileNumber.Text = _currentBooking.MobileNumber;
            txtNotes.Text = _currentBooking.Notes;

            CalculateProfit(null, null);
        }

        private void CalculateProfit(object? sender, EventArgs? e)
        {
            decimal profitPerTicket = numSellingPrice.Value - numNetPrice.Value;
            decimal totalProfit = profitPerTicket * numTicketCount.Value;
            lblProfit.Text = $"{totalProfit:N2} جنيه";
            lblProfit.ForeColor = totalProfit >= 0 ? ColorScheme.Success : ColorScheme.Error;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(txtClientName.Text))
                {
                    MessageBox.Show("يجب إدخال اسم العميل", "تنبيه",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtClientName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtSupplier.Text))
                {
                    MessageBox.Show("يجب إدخال المورد", "تنبيه",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtSupplier.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtMobileNumber.Text))
                {
                    MessageBox.Show("يجب إدخال رقم الموبايل", "تنبيه",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtMobileNumber.Focus();
                    return;
                }

                if (cmbSystem.SelectedIndex < 0 || cmbTicketStatus.SelectedIndex < 0 || cmbPaymentMethod.SelectedIndex < 0)
                {
                    MessageBox.Show("يجب اختيار جميع القوائم المطلوبة", "تنبيه",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var booking = new FlightBooking
                {
                    FlightBookingId = _currentBooking?.FlightBookingId ?? 0,
                    BookingNumber = txtBookingNumber.Text,
                    IssuanceDate = dtpIssuanceDate.Value.Date.ToUniversalTime(),
                    TravelDate = dtpTravelDate.Value.Date.ToUniversalTime(),
                    ClientName = txtClientName.Text.Trim(),
                    ClientRoute = txtClientRoute.Text.Trim(),
                    Supplier = txtSupplier.Text.Trim(),
                    System = cmbSystem.Text,
                    TicketStatus = cmbTicketStatus.Text,
                    PaymentMethod = cmbPaymentMethod.Text,
                    SellingPrice = numSellingPrice.Value,
                    NetPrice = numNetPrice.Value,
                    TicketCount = (int)numTicketCount.Value,
                    TicketNumbers = txtTicketNumbers.Text.Trim(),
                    MobileNumber = txtMobileNumber.Text.Trim(),
                    Notes = txtNotes.Text.Trim()
                };

                if (_currentBooking == null)
                {
                    await _flightBookingService.CreateFlightBookingAsync(booking);
                    MessageBox.Show("تم إضافة حجز الطيران بنجاح", "نجح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    await _flightBookingService.UpdateFlightBookingAsync(booking);
                    MessageBox.Show("تم تحديث حجز الطيران بنجاح", "نجح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ حجز الطيران:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper methods
        private Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Font = new Font("Segoe UI", 10F),
                AutoSize = true,
                RightToLeft = RightToLeft.Yes
            };
        }

        private TextBox CreateTextBox(int x, int y, int width, bool readOnly = false)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 25),
                Font = new Font("Segoe UI", 10F),
                ReadOnly = readOnly
            };
        }

        private ComboBox CreateComboBox(int x, int y, int width)
        {
            return new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 25),
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
        }

        private DateTimePicker CreateDatePicker(int x, int y, int width)
        {
            return new DateTimePicker
            {
                Location = new Point(x, y),
                Size = new Size(width, 25),
                Font = new Font("Segoe UI", 10F),
                Format = DateTimePickerFormat.Short
            };
        }

        private NumericUpDown CreateNumericUpDown(int x, int y, int width, decimal min, decimal max, decimal value)
        {
            return new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(width, 25),
                Font = new Font("Segoe UI", 10F),
                Minimum = min,
                Maximum = max,
                Value = value
            };
        }

        private Button CreateButton(string text, int x, int y, int width, int height, Color backColor)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(284, 261);
            this.Name = "AddEditFlightBookingForm";
            this.ResumeLayout(false);
        }
    }
}
