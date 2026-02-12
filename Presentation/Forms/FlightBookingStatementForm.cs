using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Presentation;

namespace GraceWay.AccountingSystem.Presentation.Forms
{
    public partial class FlightBookingStatementForm : Form
    {
        private readonly IFlightBookingService _flightBookingService;
        private ComboBox cmbFilterType = null!;
        private TextBox txtFilterValue = null!;
        private DateTimePicker dtpFromDate = null!;
        private DateTimePicker dtpToDate = null!;
        private DataGridView dgvStatement = null!;
        private Label lblTotalBookings = null!;
        private Label lblTotalSelling = null!;
        private Label lblTotalNet = null!;
        private Label lblTotalProfit = null!;

        public FlightBookingStatementForm(IFlightBookingService flightBookingService)
        {
            InitializeComponent();
            _flightBookingService = flightBookingService;
            InitializeFormControls();
            
            // Load data automatically when form opens
            this.Load += (s, e) => BtnSearch_Click(null, EventArgs.Empty);
        }

        private void InitializeFormControls()
        {
            this.Text = "كشف حساب حجوزات الطيران";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            var lblTitle = new Label
            {
                Text = "كشف حساب حجوزات الطيران",
                Location = new Point(450, 15),
                Size = new Size(300, 35),
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblFilterType = new Label
            {
                Text = "فلتر حسب:",
                Location = new Point(1050, 70),
                Size = new Size(80, 25),
                Font = new Font("Segoe UI", 10F)
            };

            cmbFilterType = new ComboBox
            {
                Location = new Point(900, 67),
                Size = new Size(140, 25),
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFilterType.Items.AddRange(new[] { "مورد", "عميل", "كل الحجوزات" });
            cmbFilterType.SelectedIndex = 2;
            cmbFilterType.SelectedIndexChanged += (s, e) =>
            {
                txtFilterValue.Enabled = cmbFilterType.SelectedIndex != 2;
                if (cmbFilterType.SelectedIndex == 2) txtFilterValue.Text = "";
            };

            txtFilterValue = new TextBox
            {
                Location = new Point(700, 67),
                Size = new Size(190, 25),
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "أدخل اسم المورد/العميل",
                Enabled = false
            };

            var lblFrom = new Label
            {
                Text = "من:",
                Location = new Point(650, 70),
                Size = new Size(30, 25),
                Font = new Font("Segoe UI", 9F)
            };

            dtpFromDate = new DateTimePicker
            {
                Location = new Point(480, 67),
                Size = new Size(160, 25),
                Font = new Font("Segoe UI", 9F),
                Format = DateTimePickerFormat.Short
            };
            dtpFromDate.Value = new DateTime(DateTime.Now.Year, 1, 1);

            var lblTo = new Label
            {
                Text = "إلى:",
                Location = new Point(440, 70),
                Size = new Size(30, 25),
                Font = new Font("Segoe UI", 9F)
            };

            dtpToDate = new DateTimePicker
            {
                Location = new Point(270, 67),
                Size = new Size(160, 25),
                Font = new Font("Segoe UI", 9F),
                Format = DateTimePickerFormat.Short
            };

            var btnSearch = new Button
            {
                Text = "بحث",
                Location = new Point(140, 65),
                Size = new Size(120, 30),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = ColorScheme.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSearch.Click += BtnSearch_Click;

            dgvStatement = new DataGridView
            {
                Location = new Point(20, 110),
                Size = new Size(1150, 450),
                Font = new Font("Segoe UI", 9.5F),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(230, 230, 230),
                RowTemplate = { Height = 35 },
                
                // Header Style
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = ColorScheme.Primary,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    SelectionBackColor = ColorScheme.Primary,
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Padding = new Padding(5)
                },
                ColumnHeadersHeight = 45,
                
                // Row Style
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(50, 50, 50),
                    SelectionBackColor = Color.FromArgb(200, 220, 240),
                    SelectionForeColor = Color.FromArgb(0, 0, 0),
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Padding = new Padding(3)
                },
                
                // Alternating Row Style
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(245, 250, 255),
                    ForeColor = Color.FromArgb(50, 50, 50),
                    SelectionBackColor = Color.FromArgb(200, 220, 240),
                    SelectionForeColor = Color.FromArgb(0, 0, 0)
                }
            };
            dgvStatement.CellFormatting += DgvStatement_CellFormatting;

            var panel = new Panel
            {
                Location = new Point(20, 570),
                Size = new Size(1150, 80),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(240, 248, 255)
            };

            lblTotalBookings = new Label
            {
                Text = "عدد الحجوزات: 0",
                Location = new Point(950, 10),
                Size = new Size(180, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary
            };

            lblTotalSelling = new Label
            {
                Text = "إجمالي البيع: 0.00",
                Location = new Point(650, 10),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            lblTotalNet = new Label
            {
                Text = "إجمالي الصافي: 0.00",
                Location = new Point(350, 10),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.DarkOrange
            };

            lblTotalProfit = new Label
            {
                Text = "إجمالي الربح: 0.00",
                Location = new Point(50, 10),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ColorScheme.Success
            };

            panel.Controls.AddRange(new Control[] { lblTotalBookings, lblTotalSelling, lblTotalNet, lblTotalProfit });

            this.Controls.AddRange(new Control[] {
                lblTitle, lblFilterType, cmbFilterType, txtFilterValue,
                lblFrom, dtpFromDate, lblTo, dtpToDate, btnSearch,
                dgvStatement, panel
            });
        }

        private async void BtnSearch_Click(object? sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                dgvStatement.DataSource = null;

                // Get dates and convert to UTC
                var fromDate = dtpFromDate.Value.Date.ToUniversalTime();
                var toDate = dtpToDate.Value.Date.AddDays(1).AddSeconds(-1).ToUniversalTime();

                var bookings = new System.Collections.Generic.List<Domain.Entities.FlightBooking>();

                if (cmbFilterType.SelectedIndex == 0) // مورد
                {
                    if (string.IsNullOrWhiteSpace(txtFilterValue.Text))
                    {
                        MessageBox.Show("يرجى إدخال اسم المورد", "تنبيه",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    bookings = await _flightBookingService.GetFlightBookingsBySupplierAsync(
                        txtFilterValue.Text, fromDate, toDate);
                }
                else if (cmbFilterType.SelectedIndex == 1) // عميل
                {
                    if (string.IsNullOrWhiteSpace(txtFilterValue.Text))
                    {
                        MessageBox.Show("يرجى إدخال اسم العميل", "تنبيه",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    bookings = await _flightBookingService.GetFlightBookingsByClientAsync(
                        txtFilterValue.Text, fromDate, toDate);
                }
                else // كل الحجوزات
                {
                    bookings = await _flightBookingService.GetAllFlightBookingsAsync(null, null, fromDate, toDate);
                }

                var dataSource = bookings.Select(b => new
                {
                    التاريخ = b.TravelDate.ToString("yyyy-MM-dd"),
                    رقم_الحجز = b.BookingNumber,
                    العميل = b.ClientName,
                    المسار = b.ClientRoute,
                    المورد = b.Supplier,
                    عدد_التذاكر = b.TicketCount,
                    سعر_البيع = b.SellingPrice,
                    السعر_الصافي = b.NetPrice,
                    الربح = (b.SellingPrice - b.NetPrice) * b.TicketCount
                }).ToList();

                dgvStatement.DataSource = dataSource;

                // Format columns with proper widths
                var dateCol = dgvStatement.Columns["التاريخ"];
                if (dateCol != null)
                {
                    dateCol.HeaderText = "تاريخ السفر";
                    dateCol.Width = 110;
                }
                    
                var bookingNumCol = dgvStatement.Columns["رقم_الحجز"];
                if (bookingNumCol != null)
                {
                    bookingNumCol.HeaderText = "رقم الحجز";
                    bookingNumCol.Width = 110;
                    bookingNumCol.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
                    
                var clientCol = dgvStatement.Columns["العميل"];
                if (clientCol != null)
                {
                    clientCol.HeaderText = "اسم العميل";
                    clientCol.Width = 150;
                }
                    
                var routeCol = dgvStatement.Columns["المسار"];
                if (routeCol != null)
                {
                    routeCol.HeaderText = "مسار العميل";
                    routeCol.Width = 180;
                }
                    
                var supplierCol = dgvStatement.Columns["المورد"];
                if (supplierCol != null)
                {
                    supplierCol.HeaderText = "المورد";
                    supplierCol.Width = 130;
                }
                    
                var ticketsCol = dgvStatement.Columns["عدد_التذاكر"];
                if (ticketsCol != null)
                {
                    ticketsCol.HeaderText = "عدد التأكر";
                    ticketsCol.Width = 90;
                    ticketsCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
                
                var sellingCol = dgvStatement.Columns["سعر_البيع"];
                if (sellingCol != null)
                {
                    sellingCol.HeaderText = "سعر البيع";
                    sellingCol.Width = 120;
                    sellingCol.DefaultCellStyle.Format = "N2";
                    sellingCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                
                var netCol = dgvStatement.Columns["السعر_الصافي"];
                if (netCol != null)
                {
                    netCol.HeaderText = "سعر الشراء";
                    netCol.Width = 120;
                    netCol.DefaultCellStyle.Format = "N2";
                    netCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                var profitCol = dgvStatement.Columns["الربح"];
                if (profitCol != null)
                {
                    profitCol.HeaderText = "الربح الإجمالي";
                    profitCol.Width = 140;
                    profitCol.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                    profitCol.DefaultCellStyle.Format = "N2";
                    profitCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                // Update summaries
                lblTotalBookings.Text = $"عدد الحجوزات: {bookings.Count}";
                var totalSelling = bookings.Sum(b => b.SellingPrice);
                var totalNet = bookings.Sum(b => b.NetPrice);
                var totalProfit = bookings.Sum(b => b.Profit);

                lblTotalSelling.Text = $"إجمالي البيع: {totalSelling:N2} جنيه";
                lblTotalNet.Text = $"إجمالي الصافي: {totalNet:N2} جنيه";
                lblTotalProfit.Text = $"إجمالي الربح: {totalProfit:N2} جنيه";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في عرض الكشف:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void DgvStatement_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvStatement.Columns[e.ColumnIndex].Name == "الربح" && e.Value != null)
            {
                if (decimal.TryParse(e.Value.ToString(), out decimal profit))
                {
                    if (profit > 0)
                    {
                        e.CellStyle.ForeColor = ColorScheme.Success;
                        e.CellStyle.BackColor = Color.FromArgb(240, 255, 240);
                    }
                    else if (profit < 0)
                    {
                        e.CellStyle.ForeColor = ColorScheme.Error;
                        e.CellStyle.BackColor = Color.FromArgb(255, 240, 240);
                    }
                    else
                    {
                        e.CellStyle.ForeColor = Color.Gray;
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(284, 261);
            this.Name = "FlightBookingStatementForm";
            this.ResumeLayout(false);
        }
    }
}
