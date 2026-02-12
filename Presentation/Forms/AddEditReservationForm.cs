using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Presentation;

namespace GraceWay.AccountingSystem.Presentation.Forms
{
    public partial class AddEditReservationForm : Form
    {
        private readonly IReservationService _reservationService;
        private readonly ICustomerService _customerService;
        private readonly ISupplierService _supplierService;
        private readonly ICashBoxService _cashBoxService;
        private readonly ITripService? _tripService;
        private Reservation? _currentReservation;
        private readonly int? _reservationId;

        // Event للإبلاغ عن تحديث الحالة
        public event EventHandler<string>? StatusChanged;

        public AddEditReservationForm(
            IReservationService reservationService,
            ICustomerService customerService,
            ISupplierService supplierService,
            ICashBoxService cashBoxService,
            int? reservationId = null,
            ITripService? tripService = null)
        {
            InitializeComponent();
            _reservationService = reservationService;
            _customerService = customerService;
            _supplierService = supplierService;
            _cashBoxService = cashBoxService;
            _tripService = tripService;
            _reservationId = reservationId;

            InitializeFormControls();
            LoadDataAsync();
        }

        private void InitializeFormControls()
        {
            // Form settings
            this.Size = new Size(850, 850);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.AutoScroll = true;

            int labelX = 680;
            int controlX = 420;
            int controlWidth = 250;
            int startY = 70;
            int rowHeight = 42;
            int currentY = startY;

            // Title
            lblTitle = CreateLabel("حجز جديد", 350, 20, new Font("Segoe UI", 16F, FontStyle.Bold), 
                ColorScheme.Primary);

            // Row 1: Reservation Number & Date
            lblReservationNumber = CreateLabel("رقم الحجز:", labelX, currentY, new Font("Segoe UI", 10F));
            txtReservationNumber = CreateTextBox(controlX, currentY - 3, controlWidth, true);

            lblReservationDate = CreateLabel("تاريخ الحجز:", 300, currentY, new Font("Segoe UI", 10F));
            dtpReservationDate = CreateDatePicker(70, currentY - 3, 220);

            // Row 2: Customer
            currentY += rowHeight;
            lblCustomer = CreateLabel("العميل: *", labelX, currentY, new Font("Segoe UI", 10F));
            cmbCustomer = CreateComboBox(controlX, currentY - 3, controlWidth);

            // Row 2.5: Trip (NEW)
            currentY += rowHeight;
            lblTrip = CreateLabel("الرحلة:", labelX, currentY, new Font("Segoe UI", 10F));
            cmbTrip = CreateComboBox(controlX, currentY - 3, controlWidth);
            cmbTrip.SelectedIndexChanged += CmbTrip_SelectedIndexChanged;
            
            btnLoadTripData = CreateButton("تحميل بيانات الرحلة", 300, currentY - 3, 100, 30, ColorScheme.Info);
            btnLoadTripData.Click += BtnLoadTripData_Click;
            btnLoadTripData.Font = new Font("Segoe UI", 8F, FontStyle.Bold);

            // Row 3: Service Type
            currentY += rowHeight;
            lblServiceType = CreateLabel("نوع الخدمة: *", labelX, currentY, new Font("Segoe UI", 10F));
            cmbServiceType = CreateComboBox(controlX, currentY - 3, controlWidth);

            // Row 4: Service Description
            currentY += rowHeight;
            lblServiceDescription = CreateLabel("وصف الخدمة:", labelX, currentY, new Font("Segoe UI", 10F));
            txtServiceDescription = CreateTextBox(70, currentY - 3, 600, false, true);
            txtServiceDescription.Height = 50;

            // Row 5: Travel & Return Dates
            currentY += 65;
            lblTravelDate = CreateLabel("تاريخ السفر:", labelX, currentY, new Font("Segoe UI", 10F));
            dtpTravelDate = CreateDatePicker(controlX, currentY - 3, 250);

            lblReturnDate = CreateLabel("تاريخ العودة:", 300, currentY, new Font("Segoe UI", 10F));
            dtpReturnDate = CreateDatePicker(70, currentY - 3, 220);

            // Row 6: Number of People
            currentY += rowHeight;
            lblNumberOfPeople = CreateLabel("عدد الأفراد:", labelX, currentY, new Font("Segoe UI", 10F));
            numNumberOfPeople = CreateNumericUpDown(controlX, currentY - 3, controlWidth, 1, 1000, 1);

            // Row 7: Selling Price
            currentY += rowHeight;
            lblSellingPrice = CreateLabel("سعر البيع: *", labelX, currentY, new Font("Segoe UI", 10F));
            numSellingPrice = CreateNumericUpDown(controlX, currentY - 3, controlWidth, 0, 999999999, 0);
            numSellingPrice.DecimalPlaces = 2;
            numSellingPrice.ValueChanged += CalculateProfit;

            // Row 8: Cost Price
            currentY += rowHeight;
            lblCostPrice = CreateLabel("سعر التكلفة: *", labelX, currentY, new Font("Segoe UI", 10F));
            numCostPrice = CreateNumericUpDown(controlX, currentY - 3, controlWidth, 0, 999999999, 0);
            numCostPrice.DecimalPlaces = 2;
            numCostPrice.ValueChanged += CalculateProfit;

            // Row 9: Profit
            currentY += rowHeight;
            lblProfit = CreateLabel("الربح:", labelX, currentY, new Font("Segoe UI", 10F, FontStyle.Bold));
            lblProfitValue = CreateLabel("0.00", controlX, currentY, 
                new Font("Segoe UI", 10F, FontStyle.Bold), ColorScheme.Success);

            // Row 10: Supplier
            currentY += rowHeight;
            lblSupplier = CreateLabel("المورد:", labelX, currentY, new Font("Segoe UI", 10F));
            cmbSupplier = CreateComboBox(controlX, currentY - 3, controlWidth);

            // Row 11: Supplier Cost
            currentY += rowHeight;
            lblSupplierCost = CreateLabel("تكلفة المورد:", labelX, currentY, new Font("Segoe UI", 10F));
            numSupplierCost = CreateNumericUpDown(controlX, currentY - 3, controlWidth, 0, 999999999, 0);
            numSupplierCost.DecimalPlaces = 2;

            // Row 12: Status
            currentY += rowHeight;
            lblStatus = CreateLabel("الحالة:", labelX, currentY, new Font("Segoe UI", 10F));
            cmbStatus = CreateComboBox(controlX, currentY - 3, controlWidth);
            cmbStatus.SelectedIndexChanged += CmbStatus_SelectedIndexChanged;

            // Row 13: CashBox
            currentY += rowHeight;
            lblCashBox = CreateLabel("الخزنة:", labelX, currentY, new Font("Segoe UI", 10F));
            cmbCashBox = CreateComboBox(controlX, currentY - 3, controlWidth);

            // Row 14: Notes
            currentY += rowHeight;
            lblNotes = CreateLabel("ملاحظات:", labelX, currentY, new Font("Segoe UI", 10F));
            txtNotes = CreateTextBox(70, currentY - 3, 600, false, true);
            txtNotes.Multiline = true;
            txtNotes.Height = 50;

            // Buttons
            currentY += 70;
            btnSave = CreateButton("حفظ", 500, currentY, 150, 40, ColorScheme.Success);
            btnSave.Click += BtnSave_Click;

            btnCancel = CreateButton("إلغاء", 330, currentY, 150, 40, ColorScheme.Secondary);
            btnCancel.Click += (s, e) => this.Close();

            // Add all controls to form
            this.Controls.AddRange(new Control[] {
                lblTitle, lblReservationNumber, txtReservationNumber, lblReservationDate, dtpReservationDate,
                lblCustomer, cmbCustomer, lblTrip, cmbTrip, btnLoadTripData,
                lblServiceType, cmbServiceType,
                lblServiceDescription, txtServiceDescription, lblTravelDate, dtpTravelDate,
                lblReturnDate, dtpReturnDate, lblNumberOfPeople, numNumberOfPeople,
                lblSellingPrice, numSellingPrice, lblCostPrice, numCostPrice,
                lblProfit, lblProfitValue, lblSupplier, cmbSupplier,
                lblSupplierCost, numSupplierCost, lblStatus, cmbStatus,
                lblCashBox, cmbCashBox, lblNotes, txtNotes, btnSave, btnCancel
            });
        }

        private async void LoadDataAsync()
        {
            try
            {
                // Load customers
                var customers = await _customerService.GetAllCustomersAsync();
                cmbCustomer.DataSource = customers;
                cmbCustomer.DisplayMember = "CustomerName";
                cmbCustomer.ValueMember = "CustomerId";

                // Load trips (if service available)
                if (_tripService != null)
                {
                    try
                    {
                        var trips = await _tripService.GetActiveTripsAsync();
                        
                        // Create a list with a placeholder item
                        var tripDisplayList = new List<object>();
                        tripDisplayList.Add(new { TripId = 0, TripName = "-- بدون رحلة --" });
                        
                        foreach (var trip in trips)
                        {
                            tripDisplayList.Add(new { TripId = trip.TripId, TripName = trip.TripName });
                        }
                        
                        cmbTrip.DataSource = tripDisplayList;
                        cmbTrip.DisplayMember = "TripName";
                        cmbTrip.ValueMember = "TripId";
                        cmbTrip.SelectedIndex = 0;
                    }
                    catch
                    {
                        // If trips fail to load, hide the controls
                        lblTrip.Visible = false;
                        cmbTrip.Visible = false;
                        btnLoadTripData.Visible = false;
                    }
                }
                else
                {
                    // Hide trip controls if service not available
                    lblTrip.Visible = false;
                    cmbTrip.Visible = false;
                    btnLoadTripData.Visible = false;
                }

                // Load service types
                var serviceTypes = await _reservationService.GetAllServiceTypesAsync();
                if (serviceTypes == null || serviceTypes.Count == 0)
                {
                    MessageBox.Show("تحذير: لا توجد أنواع خدمات في النظام", "تنبيه", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                cmbServiceType.DataSource = serviceTypes;
                cmbServiceType.DisplayMember = "ServiceTypeName";
                cmbServiceType.ValueMember = "ServiceTypeId";

                // Load suppliers
                var suppliers = await _supplierService.GetAllSuppliersAsync();
                cmbSupplier.DataSource = suppliers;
                cmbSupplier.DisplayMember = "SupplierName";
                cmbSupplier.ValueMember = "SupplierId";

                // Load statuses
                var statusList = new[]
                {
                    new { Value = "Draft", Display = "مسودة" },
                    new { Value = "Confirmed", Display = "مؤكد" },
                    new { Value = "Completed", Display = "مكتمل" },
                    new { Value = "Cancelled", Display = "ملغي" }
                };
                cmbStatus.DataSource = statusList;
                cmbStatus.DisplayMember = "Display";
                cmbStatus.ValueMember = "Value";
                cmbStatus.SelectedIndex = 0;

                // Load cashboxes
                var cashBoxes = await _cashBoxService.GetActiveCashBoxesAsync();
                var cashBoxList = cashBoxes.ToList();
                cashBoxList.Insert(0, new CashBox { Id = 0, Name = "-- اختر الخزنة --" });
                cmbCashBox.DataSource = cashBoxList;
                cmbCashBox.DisplayMember = "Name";
                cmbCashBox.ValueMember = "Id";
                cmbCashBox.SelectedIndex = 0;

                if (_reservationId.HasValue)
                {
                    // Edit mode
                    _currentReservation = await _reservationService.GetReservationByIdAsync(_reservationId.Value);
                    if (_currentReservation != null)
                    {
                        LoadReservationData();
                        lblTitle.Text = "تعديل حجز";
                    }
                }
                else
                {
                    // New reservation
                    txtReservationNumber.Text = await _reservationService.GenerateReservationNumberAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadReservationData()
        {
            if (_currentReservation == null) return;

            txtReservationNumber.Text = _currentReservation.ReservationNumber;
            dtpReservationDate.Value = _currentReservation.ReservationDate;
            cmbCustomer.SelectedValue = _currentReservation.CustomerId;
            
            // تحميل الرحلة
            if (_currentReservation.TripId.HasValue && cmbTrip.Visible)
            {
                cmbTrip.SelectedValue = _currentReservation.TripId.Value;
            }
            
            cmbServiceType.SelectedValue = _currentReservation.ServiceTypeId;
            txtServiceDescription.Text = _currentReservation.ServiceDescription;
            dtpTravelDate.Value = _currentReservation.TravelDate ?? DateTime.Now;
            dtpReturnDate.Value = _currentReservation.ReturnDate ?? DateTime.Now;
            numNumberOfPeople.Value = _currentReservation.NumberOfPeople;
            numSellingPrice.Value = _currentReservation.SellingPrice;
            numCostPrice.Value = _currentReservation.CostPrice;
            
            if (_currentReservation.SupplierId.HasValue)
                cmbSupplier.SelectedValue = _currentReservation.SupplierId.Value;
            
            numSupplierCost.Value = _currentReservation.SupplierCost;
            
            // تحميل الحالة بشكل صحيح
            if (!string.IsNullOrEmpty(_currentReservation.Status))
            {
                cmbStatus.SelectedValue = _currentReservation.Status;
            }
            
            if (_currentReservation.CashBoxId.HasValue)
                cmbCashBox.SelectedValue = _currentReservation.CashBoxId.Value;
            else
                cmbCashBox.SelectedIndex = 0;
            
            txtNotes.Text = _currentReservation.Notes;

            CalculateProfit(null, null);
        }
        
        private void CmbTrip_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // تفعيل/تعطيل زر تحميل البيانات
            if (cmbTrip.SelectedValue != null && cmbTrip.SelectedValue is int tripId && tripId > 0)
            {
                btnLoadTripData.Enabled = true;
            }
            else
            {
                btnLoadTripData.Enabled = false;
            }
        }
        
        private async void CmbStatus_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // تحديث الحالة في قاعدة البيانات مباشرة إذا كنا في وضع التعديل
            if (_currentReservation != null && cmbStatus.SelectedValue != null)
            {
                try
                {
                    string newStatus = cmbStatus.SelectedValue.ToString() ?? "Draft";
                    
                    // تحديث الحالة في الكائن الحالي
                    _currentReservation.Status = newStatus;
                    
                    // حفظ التغيير في قاعدة البيانات
                    await _reservationService.UpdateReservationAsync(_currentReservation);
                    
                    // إطلاق الـ event لإبلاغ الفورم الرئيسي
                    StatusChanged?.Invoke(this, newStatus);
                    
                    // تغيير لون العنوان حسب الحالة للتأكيد البصري
                    lblTitle.ForeColor = newStatus switch
                    {
                        "Confirmed" => ColorScheme.Success,
                        "Completed" => ColorScheme.Primary,
                        "Cancelled" => ColorScheme.Error,
                        _ => ColorScheme.Warning
                    };
                    
                    // عرض رسالة تأكيد صغيرة
                    lblTitle.Text = $"تم تحديث الحالة ✓ - {GetStatusDisplayText(newStatus)}";
                    
                    // إعادة النص الأصلي بعد ثانية
                    await Task.Delay(1500);
                    lblTitle.Text = "تعديل حجز";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في تحديث الحالة: {ex.Message}", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                }
            }
        }
        
        private string GetStatusDisplayText(string status)
        {
            return status switch
            {
                "Draft" => "مسودة",
                "Confirmed" => "مؤكد",
                "Completed" => "مكتمل",
                "Cancelled" => "ملغي",
                _ => status
            };
        }
        
        private async void BtnLoadTripData_Click(object? sender, EventArgs e)
        {
            try
            {
                if (cmbTrip.SelectedValue == null)
                {
                    MessageBox.Show("يرجى اختيار رحلة أولاً", "تنبيه", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Get TripId safely
                int tripId;
                if (cmbTrip.SelectedValue is int)
                {
                    tripId = (int)cmbTrip.SelectedValue;
                }
                else
                {
                    MessageBox.Show("خطأ في قراءة معرف الرحلة", "خطأ", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                if (tripId == 0)
                {
                    MessageBox.Show("يرجى اختيار رحلة صحيحة", "تنبيه", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                if (_tripService == null)
                {
                    MessageBox.Show("خدمة الرحلات غير متوفرة", "خطأ", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                var trip = await _tripService.GetTripByIdAsync(tripId, true);
                
                if (trip == null)
                {
                    MessageBox.Show("لم يتم العثور على الرحلة", "خطأ", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // التحقق من توفر أماكن
                int numberOfPeople = (int)numNumberOfPeople.Value;
                bool hasAvailability = await _tripService.CheckAvailabilityAsync(tripId, numberOfPeople);
                
                if (!hasAvailability)
                {
                    var result = MessageBox.Show(
                        $"عدد الأماكن المتاحة: {trip.AvailableSeats}\n" +
                        $"هل تريد المتابعة؟",
                        "تحذير - لا توجد أماكن كافية",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                    
                    if (result == DialogResult.No)
                        return;
                }
                
                // ملء البيانات تلقائياً
                txtServiceDescription.Text = $"{trip.TripName} - {trip.Destination} ({trip.StartDate:yyyy-MM-dd} إلى {trip.EndDate:yyyy-MM-dd})";
                dtpTravelDate.Value = trip.StartDate;
                dtpReturnDate.Value = trip.EndDate;
                
                // حساب السعر والتكلفة
                decimal pricePerPerson = trip.SellingPricePerPersonInEGP;
                
                // حساب التكلفة الإجمالية شاملة تكلفة المرشد وإكرامية السواق
                decimal totalCostIncludingGuide = trip.TotalCost + trip.GuideCost + trip.DriverTip;
                decimal costPerPerson = totalCostIncludingGuide / Math.Max(trip.TotalCapacity, 1);
                
                numSellingPrice.Value = pricePerPerson * numberOfPeople;
                numCostPrice.Value = costPerPerson * numberOfPeople;
                
                MessageBox.Show(
                    $"تم تحميل بيانات الرحلة بنجاح!\n\n" +
                    $"الرحلة: {trip.TripName}\n" +
                    $"الوجهة: {trip.Destination}\n" +
                    $"المدة: {trip.TotalDays} يوم\n" +
                    $"السعر للفرد: {pricePerPerson:N2} جنيه\n" +
                    $"التكلفة للفرد: {costPerPerson:N2} جنيه\n" +
                    $"(شاملة تكلفة المرشد: {trip.GuideCost:N2} + إكرامية السواق: {trip.DriverTip:N2})\n" +
                    $"الأماكن المتاحة: {trip.AvailableSeats}",
                    "نجح",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل بيانات الرحلة:\n{ex.Message}", "خطأ", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalculateProfit(object? sender, EventArgs? e)
        {
            decimal sellingPrice = numSellingPrice.Value;
            decimal costPrice = numCostPrice.Value;
            decimal profit = sellingPrice - costPrice;
            decimal profitPercentage = costPrice > 0 ? (profit / costPrice * 100) : 0;

            lblProfitValue.Text = $"{profit:N2} ({profitPercentage:N2}%)";
            lblProfitValue.ForeColor = profit >= 0 ? ColorScheme.Success : ColorScheme.Error;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                // Validation
                if (cmbCustomer.SelectedValue == null)
                {
                    MessageBox.Show("يجب اختيار العميل", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cmbServiceType.SelectedValue == null)
                {
                    MessageBox.Show("يجب اختيار نوع الخدمة", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (numSellingPrice.Value <= 0)
                {
                    MessageBox.Show("يجب إدخال سعر البيع", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get TripId safely
                int? tripId = null;
                if (cmbTrip?.SelectedValue != null && cmbTrip.SelectedValue is int selectedTripId && selectedTripId > 0)
                {
                    tripId = selectedTripId;
                }

                var reservation = new Reservation
                {
                    ReservationId = _currentReservation?.ReservationId ?? 0,
                    ReservationNumber = txtReservationNumber.Text,
                    ReservationDate = dtpReservationDate.Value.ToUniversalTime(),
                    CustomerId = (int)cmbCustomer.SelectedValue,
                    ServiceTypeId = (int)cmbServiceType.SelectedValue,
                    ServiceDescription = txtServiceDescription.Text,
                    TravelDate = dtpTravelDate.Value.ToUniversalTime(),
                    ReturnDate = dtpReturnDate.Value.ToUniversalTime(),
                    NumberOfPeople = (int)numNumberOfPeople.Value,
                    SellingPrice = numSellingPrice.Value,
                    CostPrice = numCostPrice.Value,
                    SupplierId = cmbSupplier.SelectedValue as int?,
                    SupplierCost = numSupplierCost.Value,
                    Status = cmbStatus.SelectedValue?.ToString() ?? "Draft",
                    CashBoxId = cmbCashBox.SelectedValue != null && (int)cmbCashBox.SelectedValue > 0 ? (int?)cmbCashBox.SelectedValue : null,
                    TripId = tripId,
                    Notes = txtNotes.Text,
                    ExchangeRate = 1.000000m
                };

                if (_currentReservation == null)
                {
                    await _reservationService.CreateReservationAsync(reservation);
                    MessageBox.Show("تم إضافة الحجز بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    await _reservationService.UpdateReservationAsync(reservation);
                    MessageBox.Show("تم تحديث الحجز بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                string errorDetails = ex.InnerException != null 
                    ? $"{ex.Message}\n\nتفاصيل إضافية:\n{ex.InnerException.Message}" 
                    : ex.Message;
                    
                MessageBox.Show($"خطأ في حفظ الحجز:\n{errorDetails}", "خطأ", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper methods for creating controls
        private Label CreateLabel(string text, int x, int y, Font font, Color? color = null)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Font = font,
                AutoSize = true,
                ForeColor = color ?? Color.Black,
                RightToLeft = RightToLeft.Yes
            };
        }

        private TextBox CreateTextBox(int x, int y, int width, bool readOnly = false, bool multiline = false)
        {
            var textBox = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 25),
                Font = new Font("Segoe UI", 10F),
                RightToLeft = RightToLeft.Yes,
                ReadOnly = readOnly,
                Multiline = multiline
            };

            if (multiline)
            {
                textBox.ScrollBars = ScrollBars.Vertical;
            }

            return textBox;
        }

        private ComboBox CreateComboBox(int x, int y, int width)
        {
            return new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 25),
                Font = new Font("Segoe UI", 10F),
                RightToLeft = RightToLeft.Yes,
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
                Format = DateTimePickerFormat.Short,
                RightToLeft = RightToLeft.Yes
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
                Value = value,
                RightToLeft = RightToLeft.Yes
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
                Cursor = Cursors.Hand,
                RightToLeft = RightToLeft.Yes
            };
        }
    }
}
