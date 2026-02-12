using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace GraceWay.AccountingSystem.Presentation.Forms;

/// <summary>
/// قائمة حجوزات الطيران - محسّنة
/// </summary>
public partial class FlightBookingsListForm : Form
{
    private readonly IFlightBookingService _flightBookingService;
    private readonly IServiceProvider _serviceProvider;
    
    // Controls
    private Panel _headerPanel = null!;
    private TextBox _searchBox = null!;
    private Button _searchButton = null!;
    private Button _addButton = null!;
    private Button _editButton = null!;
    private Button _deleteButton = null!;
    private Button _detailsButton = null!;
    private Button _statementButton = null!;
    private Button _refreshButton = null!;
    private ComboBox _statusFilterCombo = null!;
    private CheckBox _dateFilterCheck = null!;
    private DateTimePicker _fromDatePicker = null!;
    private DateTimePicker _toDatePicker = null!;
    private DataGridView _bookingsGrid = null!;
    private Label _totalBookingsLabel = null!;
    private Label _totalProfitLabel = null!;
    
    public FlightBookingsListForm(IFlightBookingService flightBookingService, IServiceProvider serviceProvider)
    {
        _flightBookingService = flightBookingService;
        _serviceProvider = serviceProvider;
        
        InitializeComponent();
        SetupForm();
        InitializeCustomControls();
        _ = LoadDataAsync();
    }
    
    private void SetupForm()
    {
        this.Text = "إدارة حجوزات الطيران";
        this.Size = new Size(1600, 950);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
        this.WindowState = FormWindowState.Maximized;
    }
    
    private void InitializeCustomControls()
    {
        // ══════════════════════════════════════
        // Header Panel
        // ══════════════════════════════════════
        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 220,
            BackColor = Color.White,
            Padding = new Padding(20)
        };
        
        Label titleLabel = new Label
        {
            Text = "✈️ إدارة حجوزات الطيران",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(20, 20)
        };
        _headerPanel.Controls.Add(titleLabel);
        
        // ══════════════════════════════════════
        // First Row: Search and Filters
        // ══════════════════════════════════════
        Label searchLabel = new Label
        {
            Text = "بحث:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 75)
        };
        _headerPanel.Controls.Add(searchLabel);
        
        _searchBox = new TextBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(300, 30),
            Location = new Point(80, 72),
            PlaceholderText = "رقم الحجز، اسم العميل، المسار..."
        };
        _searchBox.KeyPress += (s, e) => { if (e.KeyChar == (char)13) _ = LoadDataAsync(); };
        _headerPanel.Controls.Add(_searchBox);
        
        _searchButton = CreateButton("🔍 بحث", ColorScheme.Primary, new Point(400, 72), (s, e) => _ = LoadDataAsync());
        _headerPanel.Controls.Add(_searchButton);
        
        Label statusLabel = new Label
        {
            Text = "الحالة:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(540, 75)
        };
        _headerPanel.Controls.Add(statusLabel);
        
        _statusFilterCombo = new ComboBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(150, 30),
            Location = new Point(615, 72),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _statusFilterCombo.Items.AddRange(new object[] { "الكل", "مؤكد", "قيد الانتظار", "ملغي", "مكتمل" });
        _statusFilterCombo.SelectedIndex = 0;
        _statusFilterCombo.SelectedIndexChanged += (s, e) => _ = LoadDataAsync();
        _headerPanel.Controls.Add(_statusFilterCombo);
        
        // Date Filter
        _dateFilterCheck = new CheckBox
        {
            Text = "فلتر بالتاريخ",
            Font = new Font("Cairo", 10F),
            AutoSize = true,
            Location = new Point(800, 75)
        };
        _dateFilterCheck.CheckedChanged += (s, e) =>
        {
            _fromDatePicker.Enabled = _dateFilterCheck.Checked;
            _toDatePicker.Enabled = _dateFilterCheck.Checked;
            _ = LoadDataAsync();
        };
        _headerPanel.Controls.Add(_dateFilterCheck);
        
        Label fromLabel = new Label
        {
            Text = "من:",
            Font = new Font("Cairo", 10F),
            AutoSize = true,
            Location = new Point(960, 75)
        };
        _headerPanel.Controls.Add(fromLabel);
        
        _fromDatePicker = new DateTimePicker
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(150, 30),
            Location = new Point(1020, 72),
            Format = DateTimePickerFormat.Short,
            Enabled = false
        };
        _fromDatePicker.ValueChanged += (s, e) => { if (_dateFilterCheck.Checked) _ = LoadDataAsync(); };
        _headerPanel.Controls.Add(_fromDatePicker);
        
        Label toLabel = new Label
        {
            Text = "إلى:",
            Font = new Font("Cairo", 10F),
            AutoSize = true,
            Location = new Point(1190, 75)
        };
        _headerPanel.Controls.Add(toLabel);
        
        _toDatePicker = new DateTimePicker
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(150, 30),
            Location = new Point(1245, 72),
            Format = DateTimePickerFormat.Short,
            Enabled = false
        };
        _toDatePicker.ValueChanged += (s, e) => { if (_dateFilterCheck.Checked) _ = LoadDataAsync(); };
        _headerPanel.Controls.Add(_toDatePicker);
        
        // ══════════════════════════════════════
        // Second Row: Summary Statistics
        // ══════════════════════════════════════
        _totalBookingsLabel = new Label
        {
            Text = "إجمالي الحجوزات: 0",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(20, 120)
        };
        _headerPanel.Controls.Add(_totalBookingsLabel);
        
        _totalProfitLabel = new Label
        {
            Text = "إجمالي الربح: 0.00 جنيه",
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = ColorScheme.Success,
            AutoSize = true,
            Location = new Point(250, 120)
        };
        _headerPanel.Controls.Add(_totalProfitLabel);
        
        // ══════════════════════════════════════
        // Third Row: Action Buttons
        // ══════════════════════════════════════
        _refreshButton = CreateButton("🔄 تحديث", ColorScheme.Secondary, new Point(20, 165), (s, e) => _ = LoadDataAsync());
        _headerPanel.Controls.Add(_refreshButton);
        
        _detailsButton = CreateButton("👁️ التفاصيل", ColorScheme.Info, new Point(185, 165), BtnDetails_Click);
        _headerPanel.Controls.Add(_detailsButton);
        
        _statementButton = CreateButton("📊 كشف حساب", Color.FromArgb(142, 68, 173), new Point(350, 165), BtnStatement_Click);
        _headerPanel.Controls.Add(_statementButton);
        
        _deleteButton = CreateButton("🗑️ حذف", ColorScheme.Error, new Point(515, 165), BtnDelete_Click);
        _headerPanel.Controls.Add(_deleteButton);
        
        _editButton = CreateButton("✏️ تعديل", ColorScheme.Warning, new Point(680, 165), BtnEdit_Click);
        _headerPanel.Controls.Add(_editButton);
        
        _addButton = CreateButton("➕ حجز جديد", ColorScheme.Success, new Point(845, 165), BtnAdd_Click);
        _headerPanel.Controls.Add(_addButton);
        
        // ══════════════════════════════════════
        // Main Panel للـ Grid
        // ══════════════════════════════════════
        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(20)
        };
        
        _bookingsGrid = CreateDataGrid();
        mainPanel.Controls.Add(_bookingsGrid);
        
        this.Controls.Add(mainPanel);
        this.Controls.Add(_headerPanel);
    }
    
    private DataGridView CreateDataGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false,
            EnableHeadersVisualStyles = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            RowTemplate = { Height = 45 }
        };
        
        grid.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Primary;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8);
        grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
        grid.ColumnHeadersHeight = 50;
        
        grid.DefaultCellStyle.Font = new Font("Cairo", 10F);
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.ForeColor = Color.FromArgb(52, 73, 94);
        grid.DefaultCellStyle.SelectionBackColor = ColorScheme.Primary;
        grid.DefaultCellStyle.SelectionForeColor = Color.White;
        grid.DefaultCellStyle.Padding = new Padding(5);
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
        
        // ══════════════════════════════════════
        // تعريف الأعمدة المحسّنة
        // ══════════════════════════════════════
        
        // Hidden ID column
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "FlightBookingId",
            DataPropertyName = "FlightBookingId",
            HeaderText = "المعرف",
            Visible = false
        });
        
        // Booking Number
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "BookingNumber",
            DataPropertyName = "BookingNumber",
            HeaderText = "رقم الحجز",
            Width = 110,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Cairo", 10F, FontStyle.Bold)
            }
        });
        
        // Issuance Date
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "IssuanceDate",
            DataPropertyName = "IssuanceDate",
            HeaderText = "تاريخ الإصدار",
            Width = 110,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Format = "yyyy-MM-dd"
            }
        });
        
        // Travel Date
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TravelDate",
            DataPropertyName = "TravelDate",
            HeaderText = "تاريخ السفر",
            Width = 110,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Format = "yyyy-MM-dd"
            }
        });
        
        // Client Name
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ClientName",
            DataPropertyName = "ClientName",
            HeaderText = "اسم العميل",
            Width = 180,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleRight
            }
        });
        
        // Client Route
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ClientRoute",
            DataPropertyName = "ClientRoute",
            HeaderText = "المسار",
            Width = 150,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // Supplier
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Supplier",
            DataPropertyName = "Supplier",
            HeaderText = "المورد",
            Width = 140,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleRight
            }
        });
        
        // System
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "System",
            DataPropertyName = "System",
            HeaderText = "النظام",
            Width = 100,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // Ticket Status
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TicketStatus",
            DataPropertyName = "TicketStatus",
            HeaderText = "الحالة",
            Width = 100,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Cairo", 10F, FontStyle.Bold)
            }
        });
        
        // Ticket Count
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TicketCount",
            DataPropertyName = "TicketCount",
            HeaderText = "عدد التذاكر",
            Width = 90,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Cairo", 10F, FontStyle.Bold)
            }
        });
        
        // Selling Price
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "SellingPrice",
            DataPropertyName = "SellingPrice",
            HeaderText = "سعر البيع",
            Width = 100,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Format = "N2"
            }
        });
        
        // Net Price
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "NetPrice",
            DataPropertyName = "NetPrice",
            HeaderText = "سعر الشراء",
            Width = 100,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Format = "N2"
            }
        });
        
        // Profit
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Profit",
            DataPropertyName = "Profit",
            HeaderText = "الربح",
            Width = 100,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Format = "N2",
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                ForeColor = ColorScheme.Success
            }
        });
        
        // Payment Method
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "PaymentMethod",
            DataPropertyName = "PaymentMethod",
            HeaderText = "طريقة الدفع",
            Width = 110,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // Mobile Number
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "MobileNumber",
            DataPropertyName = "MobileNumber",
            HeaderText = "الموبايل",
            Width = 120,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // ✅ عمود اسم اليوزر
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "CreatedByUserName",
            DataPropertyName = "CreatedByUserName",
            HeaderText = "المستخدم",
            Width = 150,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                ForeColor = ColorScheme.Primary,
                Font = new Font("Cairo", 9F, FontStyle.Bold)
            }
        });
        
        grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(s, EventArgs.Empty); };
        grid.CellFormatting += Grid_CellFormatting;
        
        return grid;
    }
    
    private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (_bookingsGrid.Columns[e.ColumnIndex].Name == "TicketStatus" && e.Value != null)
        {
            string status = e.Value.ToString() ?? "";
            
            switch (status)
            {
                case "مؤكد":
                    e.CellStyle.BackColor = Color.FromArgb(212, 237, 218);
                    e.CellStyle.ForeColor = Color.FromArgb(21, 87, 36);
                    break;
                case "قيد الانتظار":
                    e.CellStyle.BackColor = Color.FromArgb(255, 243, 205);
                    e.CellStyle.ForeColor = Color.FromArgb(133, 100, 4);
                    break;
                case "ملغي":
                    e.CellStyle.BackColor = Color.FromArgb(248, 215, 218);
                    e.CellStyle.ForeColor = Color.FromArgb(114, 28, 36);
                    break;
                case "مكتمل":
                    e.CellStyle.BackColor = Color.FromArgb(209, 236, 241);
                    e.CellStyle.ForeColor = Color.FromArgb(12, 84, 96);
                    break;
            }
        }
    }
    
    private Button CreateButton(string text, Color backColor, Point location, EventHandler clickHandler)
    {
        var btn = new Button
        {
            Text = text,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Size = new Size(155, 40),
            Location = location,
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleCenter
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += clickHandler;
        return btn;
    }
    
    private async Task LoadDataAsync()
    {
        try
        {
            this.Cursor = Cursors.WaitCursor;
            
            string? searchTerm = string.IsNullOrWhiteSpace(_searchBox.Text) ? null : _searchBox.Text;
            string? status = _statusFilterCombo.SelectedIndex == 0 ? null : _statusFilterCombo.Text;
            
            DateTime? fromDate = null;
            DateTime? toDate = null;
            
            if (_dateFilterCheck.Checked)
            {
                fromDate = _fromDatePicker.Value.Date.ToUniversalTime();
                toDate = _toDatePicker.Value.Date.AddDays(1).AddSeconds(-1).ToUniversalTime();
            }

            var bookings = await _flightBookingService.GetAllFlightBookingsAsync(searchTerm, status, fromDate, toDate);

            // ✅ عمل anonymous object مع اسم اليوزر مباشرة
            var displayList = bookings.Select(b => new
            {
                b.FlightBookingId,
                b.BookingNumber,
                b.IssuanceDate,
                b.TravelDate,
                b.ClientName,
                b.ClientRoute,
                b.Supplier,
                b.System,
                b.TicketStatus,
                b.TicketCount,
                b.SellingPrice,
                b.NetPrice,
                b.Profit,
                b.PaymentMethod,
                b.MobileNumber,
                CreatedByUserName = b.CreatedByUser?.Username ?? "غير معروف" // ✅ استخدام Username بدل FullName
            }).ToList();

            _bookingsGrid.DataSource = displayList;

            // Update summary
            _totalBookingsLabel.Text = $"إجمالي الحجوزات: {bookings.Count}";
            var totalProfit = bookings.Sum(b => b.Profit);
            _totalProfitLabel.Text = $"إجمالي الربح: {totalProfit:N2} جنيه";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل حجوزات الطيران:\n{ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    
    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        var form = new AddEditFlightBookingForm(_flightBookingService);
        form.FormClosed += (s, args) => { _ = LoadDataAsync(); }; // ✅ تحديث عند الإغلاق
        form.Show(); // ✅ نافذة مستقلة
    }
    
    private void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (_bookingsGrid.SelectedRows.Count == 0)
        {
            MessageBox.Show("يرجى اختيار حجز للتعديل", "تنبيه", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int bookingId = (int)_bookingsGrid.SelectedRows[0].Cells["FlightBookingId"].Value!;
        var form = new AddEditFlightBookingForm(_flightBookingService, bookingId);
        form.FormClosed += (s, args) => { _ = LoadDataAsync(); }; // ✅ تحديث عند الإغلاق
        form.Show(); // ✅ نافذة مستقلة
    }
    
    private async void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (_bookingsGrid.SelectedRows.Count == 0)
        {
            MessageBox.Show("يرجى اختيار حجز للحذف", "تنبيه",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var result = MessageBox.Show("هل أنت متأكد من حذف حجز الطيران المحدد؟", "تأكيد الحذف",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            try
            {
                int bookingId = (int)_bookingsGrid.SelectedRows[0].Cells["FlightBookingId"].Value!;
                await _flightBookingService.DeleteFlightBookingAsync(bookingId);
                MessageBox.Show("تم حذف الحجز بنجاح", "نجح", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                _ = LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حذف الحجز: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    
    private void BtnStatement_Click(object? sender, EventArgs e)
    {
        var form = new FlightBookingStatementForm(_flightBookingService);
        form.Show(); // ✅ نافذة مستقلة
    }
    
    private async void BtnDetails_Click(object? sender, EventArgs e)
    {
        if (_bookingsGrid.SelectedRows.Count == 0)
        {
            MessageBox.Show("يرجى اختيار حجز لعرض التفاصيل", "تنبيه",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            int bookingId = (int)_bookingsGrid.SelectedRows[0].Cells["FlightBookingId"].Value!;
            var booking = await _flightBookingService.GetFlightBookingByIdAsync(bookingId);
            
            if (booking != null)
            {
                var form = new FlightBookingDetailsForm(booking);
                form.Show(); // ✅ نافذة مستقلة
            }
            else
            {
                MessageBox.Show("لم يتم العثور على الحجز", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في عرض التفاصيل: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.ClientSize = new Size(1600, 950);
        this.Name = "FlightBookingsListForm";
        this.ResumeLayout(false);
    }
}
