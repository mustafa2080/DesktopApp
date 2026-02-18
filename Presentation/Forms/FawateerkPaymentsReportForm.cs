using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GraceWay.AccountingSystem.Infrastructure.Data;
using GraceWay.AccountingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Presentation.Forms
{
    public partial class FawateerkPaymentsReportForm : Form
    {
        private readonly AppDbContext _context;
        private readonly int _currentUserId;
        
        private DataGridView dgvPayments;
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private ComboBox cmbBank;
        private ComboBox cmbCustomer;
        private Button btnFilter;
        private Button btnClear;
        private Button btnExport;
        private Button btnRefresh;
        private Label lblTotalAmount;
        private Label lblCount;
        private TextBox txtSearch;

        public FawateerkPaymentsReportForm(AppDbContext context, int currentUserId)
        {
            _context = context;
            _currentUserId = currentUserId;
            InitializeComponent();
            InitializeCustomComponents();
            LoadFilters();
            LoadPayments();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "تقرير دفعات فواتيرك";
            this.Size = new Size(1400, 800);
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = ColorScheme.Background;
            this.Font = new Font("Cairo", 10F);
            this.StartPosition = FormStartPosition.CenterParent;

            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = ColorScheme.Background,
                AutoScroll = true
            };

            // Header
            Label lblTitle = new Label
            {
                Text = "💳 تقرير دفعات فواتيرك",
                Font = new Font("Cairo", 18F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            mainPanel.Controls.Add(lblTitle);

            // Filter Panel
            Panel filterPanel = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(1340, 120),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Row 1: Date Range
            Label lblFrom = new Label
            {
                Text = "من تاريخ:",
                Location = new Point(1100, 15),
                Size = new Size(80, 25),
                Font = new Font("Cairo", 9F)
            };
            filterPanel.Controls.Add(lblFrom);

            dtpFrom = new DateTimePicker
            {
                Location = new Point(900, 15),
                Size = new Size(190, 30),
                Font = new Font("Cairo", 9F),
                Format = DateTimePickerFormat.Short
            };
            dtpFrom.Value = DateTime.Now.AddMonths(-1);
            filterPanel.Controls.Add(dtpFrom);

            Label lblTo = new Label
            {
                Text = "إلى تاريخ:",
                Location = new Point(790, 15),
                Size = new Size(80, 25),
                Font = new Font("Cairo", 9F)
            };
            filterPanel.Controls.Add(lblTo);

            dtpTo = new DateTimePicker
            {
                Location = new Point(590, 15),
                Size = new Size(190, 30),
                Font = new Font("Cairo", 9F),
                Format = DateTimePickerFormat.Short
            };
            filterPanel.Controls.Add(dtpTo);

            // Row 2: Bank and Customer
            Label lblBank = new Label
            {
                Text = "البنك:",
                Location = new Point(1100, 55),
                Size = new Size(80, 25),
                Font = new Font("Cairo", 9F)
            };
            filterPanel.Controls.Add(lblBank);

            cmbBank = new ComboBox
            {
                Location = new Point(900, 55),
                Size = new Size(190, 30),
                Font = new Font("Cairo", 9F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            filterPanel.Controls.Add(cmbBank);

            Label lblCustomer = new Label
            {
                Text = "العميل:",
                Location = new Point(790, 55),
                Size = new Size(80, 25),
                Font = new Font("Cairo", 9F)
            };
            filterPanel.Controls.Add(lblCustomer);

            cmbCustomer = new ComboBox
            {
                Location = new Point(590, 55),
                Size = new Size(190, 30),
                Font = new Font("Cairo", 9F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            filterPanel.Controls.Add(cmbCustomer);

            // Search
            txtSearch = new TextBox
            {
                Location = new Point(10, 15),
                Size = new Size(300, 30),
                Font = new Font("Cairo", 9F),
                PlaceholderText = "🔍 بحث برقم المرجع..."
            };
            txtSearch.TextChanged += (s, e) => LoadPayments();
            filterPanel.Controls.Add(txtSearch);

            // Buttons
            btnFilter = new Button
            {
                Text = "🔍 بحث",
                Location = new Point(320, 15),
                Size = new Size(100, 35),
                BackColor = ColorScheme.Primary,
                ForeColor = Color.White,
                Font = new Font("Cairo", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnFilter.FlatAppearance.BorderSize = 0;
            btnFilter.Click += (s, e) => LoadPayments();
            filterPanel.Controls.Add(btnFilter);

            btnClear = new Button
            {
                Text = "❌ مسح",
                Location = new Point(430, 15),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Cairo", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += BtnClear_Click;
            filterPanel.Controls.Add(btnClear);

            btnRefresh = new Button
            {
                Text = "🔄 تحديث",
                Location = new Point(10, 55),
                Size = new Size(100, 35),
                BackColor = ColorScheme.Success,
                ForeColor = Color.White,
                Font = new Font("Cairo", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadPayments();
            filterPanel.Controls.Add(btnRefresh);

            btnExport = new Button
            {
                Text = "📊 تصدير Excel",
                Location = new Point(120, 55),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(0, 123, 85),
                ForeColor = Color.White,
                Font = new Font("Cairo", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExport_Click;
            filterPanel.Controls.Add(btnExport);

            mainPanel.Controls.Add(filterPanel);

            // Summary Panel
            Panel summaryPanel = new Panel
            {
                Location = new Point(20, 200),
                Size = new Size(1340, 50),
                BackColor = Color.FromArgb(230, 244, 255),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblCount = new Label
            {
                Text = "عدد الدفعات: 0",
                Location = new Point(1000, 12),
                Size = new Size(320, 25),
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                TextAlign = ContentAlignment.MiddleRight
            };
            summaryPanel.Controls.Add(lblCount);

            lblTotalAmount = new Label
            {
                Text = "إجمالي المبالغ: 0.00 ج.م",
                Location = new Point(20, 12),
                Size = new Size(400, 25),
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                ForeColor = ColorScheme.Success,
                TextAlign = ContentAlignment.MiddleLeft
            };
            summaryPanel.Controls.Add(lblTotalAmount);

            mainPanel.Controls.Add(summaryPanel);

            // DataGridView
            dgvPayments = new DataGridView
            {
                Location = new Point(20, 260),
                Size = new Size(1340, 450),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                RowHeadersVisible = false,
                Font = new Font("Cairo", 9F),
                ColumnHeadersHeight = 40,
                RowTemplate = { Height = 35 },
                ScrollBars = ScrollBars.Both
            };

            dgvPayments.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Primary;
            dgvPayments.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPayments.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 10F, FontStyle.Bold);
            dgvPayments.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvPayments.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvPayments.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvPayments.EnableHeadersVisualStyles = false;

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "المعرف",
                Width = 60,
                Visible = false
            });

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TransferDate",
                HeaderText = "التاريخ",
                Width = 100
            });

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BankName",
                HeaderText = "البنك",
                Width = 150
            });

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CustomerName",
                HeaderText = "العميل",
                Width = 150
            });

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CustomerPhone",
                HeaderText = "الهاتف",
                Width = 110
            });

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Amount",
                HeaderText = "المبلغ",
                Width = 100
            });

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ReferenceNumber",
                HeaderText = "رقم المرجع",
                Width = 130
            });

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TripName",
                HeaderText = "الرحلة",
                Width = 150
            });

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Notes",
                HeaderText = "ملاحظات",
                Width = 200
            });

            // Action Buttons Column
            DataGridViewButtonColumn btnDetails = new DataGridViewButtonColumn
            {
                Name = "Details",
                HeaderText = "تفاصيل",
                Text = "📋 تفاصيل",
                UseColumnTextForButtonValue = true,
                Width = 90,
                FlatStyle = FlatStyle.Flat
            };
            dgvPayments.Columns.Add(btnDetails);

            DataGridViewButtonColumn btnEdit = new DataGridViewButtonColumn
            {
                Name = "Edit",
                HeaderText = "تعديل",
                Text = "✏️ تعديل",
                UseColumnTextForButtonValue = true,
                Width = 90,
                FlatStyle = FlatStyle.Flat
            };
            dgvPayments.Columns.Add(btnEdit);

            DataGridViewButtonColumn btnDelete = new DataGridViewButtonColumn
            {
                Name = "Delete",
                HeaderText = "حذف",
                Text = "🗑️ حذف",
                UseColumnTextForButtonValue = true,
                Width = 80,
                FlatStyle = FlatStyle.Flat
            };
            dgvPayments.Columns.Add(btnDelete);

            // Style the action buttons
            dgvPayments.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex >= 0 && dgvPayments.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
                {
                    var columnName = dgvPayments.Columns[e.ColumnIndex].Name;
                    if (columnName == "Details")
                    {
                        e.CellStyle.BackColor = ColorScheme.Primary;
                        e.CellStyle.ForeColor = Color.White;
                    }
                    else if (columnName == "Edit")
                    {
                        e.CellStyle.BackColor = Color.FromArgb(255, 193, 7);
                        e.CellStyle.ForeColor = Color.Black;
                    }
                    else if (columnName == "Delete")
                    {
                        e.CellStyle.BackColor = Color.FromArgb(220, 53, 69);
                        e.CellStyle.ForeColor = Color.White;
                    }
                    e.CellStyle.Font = new Font("Cairo", 9F, FontStyle.Bold);
                }
            };

            // Handle button clicks
            dgvPayments.CellClick += DgvPayments_CellClick;

            mainPanel.Controls.Add(dgvPayments);

            this.Controls.Add(mainPanel);
        }

        private void LoadFilters()
        {
            // Load Banks
            var banks = _context.Set<BankAccount>()
                .Where(b => b.IsActive)
                .OrderBy(b => b.BankName)
                .Select(b => new { b.Id, b.BankName })
                .ToList();
            banks.Insert(0, new { Id = 0, BankName = "-- كل البنوك --" });
            cmbBank.DataSource = banks;
            cmbBank.DisplayMember = "BankName";
            cmbBank.ValueMember = "Id";

            // Load Customers
            var customers = _context.Set<Customer>()
                .Where(c => c.IsActive)
                .OrderBy(c => c.CustomerName)
                .Select(c => new { Id = c.CustomerId, Name = c.CustomerName })
                .ToList();
            customers.Insert(0, new { Id = 0, Name = "-- كل العملاء --" });
            cmbCustomer.DataSource = customers;
            cmbCustomer.DisplayMember = "Name";
            cmbCustomer.ValueMember = "Id";
        }

        private void LoadPayments()
        {
            try
            {
                // Always clear tracker first to force fresh data from database
                _context.ChangeTracker.Clear();

                var query = _context.Set<BankTransfer>()
                    .AsNoTracking()
                    .Include(t => t.DestinationBankAccount)
                    .Where(t => t.TransferType == "FawateerkPayment");

                // Apply filters
                if (dtpFrom.Value.Date <= dtpTo.Value.Date)
                {
                    DateTime fromDate = DateTime.SpecifyKind(dtpFrom.Value.Date, DateTimeKind.Utc);
                    DateTime toDate = DateTime.SpecifyKind(dtpTo.Value.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);
                    query = query.Where(t => t.TransferDate >= fromDate && t.TransferDate <= toDate);
                }

                if (cmbBank.SelectedValue != null && Convert.ToInt32(cmbBank.SelectedValue) > 0)
                {
                    int bankId = Convert.ToInt32(cmbBank.SelectedValue);
                    query = query.Where(t => t.DestinationBankAccountId == bankId);
                }

                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    string search = txtSearch.Text.Trim().ToLower();
                    query = query.Where(t => t.ReferenceNumber != null && t.ReferenceNumber.ToLower().Contains(search));
                }

                var payments = query
                    .OrderByDescending(t => t.TransferDate)
                    .Select(t => new
                    {
                        t.Id,
                        t.TransferDate,
                        BankName = t.DestinationBankAccount!.BankName,
                        t.Amount,
                        t.ReferenceNumber,
                        t.Notes
                    })
                    .ToList();

                dgvPayments.Rows.Clear();
                decimal totalAmount = 0;

                foreach (var payment in payments)
                {
                    // Extract customer info and trip from notes
                    string customerName = "";
                    string customerPhone = "";
                    string tripName = "";
                    string notes = payment.Notes ?? "";

                    if (notes.Contains("العميل:"))
                    {
                        var lines = notes.Split('\n');
                        foreach (var line in lines)
                        {
                            if (line.Contains("العميل:"))
                            {
                                var customerPart = line.Replace("دفعة من فواتيرك - العميل:", "").Trim();
                                var customerInfo = customerPart.Split('-');
                                customerName = customerInfo[0].Trim();
                                if (customerInfo.Length > 1)
                                {
                                    customerPhone = customerInfo[1].Trim();
                                }
                            }
                            else if (line.Contains("الرحلة:"))
                            {
                                tripName = line.Replace("الرحلة:", "").Trim();
                            }
                            else if (!string.IsNullOrWhiteSpace(line) && !line.Contains("فواتيرك"))
                            {
                                if (string.IsNullOrEmpty(notes) || notes == payment.Notes)
                                {
                                    notes = line.Trim();
                                }
                            }
                        }
                    }

                    dgvPayments.Rows.Add(
                        payment.Id,
                        payment.TransferDate.ToString("dd/MM/yyyy"),
                        payment.BankName,
                        customerName,
                        customerPhone,
                        payment.Amount.ToString("N2"),
                        payment.ReferenceNumber ?? "",
                        tripName,
                        notes
                    );

                    totalAmount += payment.Amount;
                }

                // Apply customer filter if selected
                if (cmbCustomer.SelectedValue != null && Convert.ToInt32(cmbCustomer.SelectedValue) > 0)
                {
                    string selectedCustomer = cmbCustomer.Text;
                    foreach (DataGridViewRow row in dgvPayments.Rows)
                    {
                        string customerName = row.Cells["CustomerName"].Value?.ToString() ?? "";
                        row.Visible = customerName.Contains(selectedCustomer);
                    }
                }

                // Update summary
                int visibleCount = dgvPayments.Rows.Cast<DataGridViewRow>().Count(r => r.Visible);
                decimal visibleTotal = dgvPayments.Rows.Cast<DataGridViewRow>()
                    .Where(r => r.Visible)
                    .Sum(r => decimal.Parse(r.Cells["Amount"].Value?.ToString() ?? "0"));

                lblCount.Text = $"عدد الدفعات: {visibleCount}";
                lblTotalAmount.Text = $"إجمالي المبالغ: {visibleTotal:N2} ج.م";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            dtpFrom.Value = DateTime.Now.AddMonths(-1);
            dtpTo.Value = DateTime.Now;
            cmbBank.SelectedIndex = 0;
            cmbCustomer.SelectedIndex = 0;
            txtSearch.Clear();
            LoadPayments();
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Excel Files|*.xlsx";
                    sfd.FileName = $"فواتيرك_{DateTime.Now:yyyyMMdd}.xlsx";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        // Export logic here - you can implement using EPPlus or similar library
                        MessageBox.Show("سيتم إضافة وظيفة التصدير قريباً", "تنبيه",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في التصدير: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvPayments_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var columnName = dgvPayments.Columns[e.ColumnIndex].Name;
            var paymentId = Convert.ToInt32(dgvPayments.Rows[e.RowIndex].Cells["Id"].Value);

            switch (columnName)
            {
                case "Details":
                    ShowPaymentDetails(paymentId);
                    break;
                case "Edit":
                    EditPayment(paymentId);
                    break;
                case "Delete":
                    DeletePayment(paymentId);
                    break;
            }
        }

        private void ShowPaymentDetails(int paymentId)
        {
            try
            {
                var payment = _context.Set<BankTransfer>()
                    .Include(t => t.DestinationBankAccount)
                    .FirstOrDefault(t => t.Id == paymentId);

                if (payment == null)
                {
                    MessageBox.Show("لم يتم العثور على الدفعة", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Create a custom details form
                using (var detailsForm = new Form())
                {
                    detailsForm.Text = "تفاصيل دفعة فواتيرك";
                    detailsForm.Size = new Size(700, 550);
                    detailsForm.RightToLeft = RightToLeft.Yes;
                    detailsForm.RightToLeftLayout = false;
                    detailsForm.StartPosition = FormStartPosition.CenterParent;
                    detailsForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    detailsForm.MaximizeBox = false;
                    detailsForm.MinimizeBox = false;
                    detailsForm.BackColor = ColorScheme.Background;
                    detailsForm.Font = new Font("Cairo", 10F);

                    Panel mainPanel = new Panel
                    {
                        Dock = DockStyle.Fill,
                        BackColor = ColorScheme.Background,
                        Padding = new Padding(20),
                        AutoScroll = true
                    };

                    // Title
                    Label lblTitle = new Label
                    {
                        Text = "💳 تفاصيل الدفعة",
                        Font = new Font("Cairo", 14F, FontStyle.Bold),
                        ForeColor = ColorScheme.Primary,
                        AutoSize = true,
                        Location = new Point(20, 20)
                    };
                    mainPanel.Controls.Add(lblTitle);

                    // Payment Info Panel
                    Panel infoPanel = new Panel
                    {
                        Location = new Point(20, 60),
                        Size = new Size(620, 350),
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle
                    };

                    int yPos = 20;

                    // Date
                    AddDetailRow(infoPanel, "📅 التاريخ:", payment.TransferDate.ToString("dd/MM/yyyy hh:mm tt"), ref yPos);
                    
                    // Bank
                    AddDetailRow(infoPanel, "🏦 البنك:", payment.DestinationBankAccount?.BankName ?? "غير محدد", ref yPos);
                    
                    // Amount
                    AddDetailRow(infoPanel, "💰 المبلغ:", $"{payment.Amount:N2} ج.م", ref yPos);
                    
                    // Reference Number
                    AddDetailRow(infoPanel, "🔢 رقم المرجع:", payment.ReferenceNumber ?? "غير محدد", ref yPos);

                    // Extract customer info and trip from notes
                    string customerInfo = "غير محدد";
                    string tripInfo = "غير محدد";
                    string cleanNotes = payment.Notes ?? "";

                    if (!string.IsNullOrEmpty(payment.Notes))
                    {
                        var lines = payment.Notes.Split('\n');
                        foreach (var line in lines)
                        {
                            if (line.Contains("العميل:"))
                            {
                                customerInfo = line.Replace("دفعة من فواتيرك - العميل:", "").Trim();
                            }
                            else if (line.Contains("الرحلة:"))
                            {
                                tripInfo = line.Replace("الرحلة:", "").Trim();
                            }
                            else if (!string.IsNullOrWhiteSpace(line) && !line.Contains("فواتيرك"))
                            {
                                cleanNotes = line.Trim();
                            }
                        }
                    }

                    // Customer
                    AddDetailRow(infoPanel, "👤 العميل:", customerInfo, ref yPos);
                    
                    // Trip
                    AddDetailRow(infoPanel, "✈️ الرحلة:", tripInfo, ref yPos);

                    // Notes
                    Label lblNotesTitle = new Label
                    {
                        Text = "📝 الملاحظات:",
                        Location = new Point(500, yPos),
                        Size = new Size(100, 25),
                        Font = new Font("Cairo", 10F, FontStyle.Bold)
                    };
                    infoPanel.Controls.Add(lblNotesTitle);

                    TextBox txtNotes = new TextBox
                    {
                        Location = new Point(20, yPos),
                        Size = new Size(470, 80),
                        Text = cleanNotes,
                        Multiline = true,
                        ScrollBars = ScrollBars.Vertical,
                        ReadOnly = true,
                        BackColor = Color.FromArgb(245, 245, 245),
                        Font = new Font("Cairo", 9F)
                    };
                    infoPanel.Controls.Add(txtNotes);
                    yPos += 90;

                    // System Info
                    Label lblSystemInfo = new Label
                    {
                        Text = $"🆔 معرف الدفعة: {payment.Id}  |  👤 المستخدم: {payment.CreatedBy}  |  ⏰ تاريخ الإنشاء: {payment.CreatedDate:dd/MM/yyyy HH:mm}",
                        Location = new Point(20, yPos),
                        Size = new Size(580, 30),
                        Font = new Font("Cairo", 8F),
                        ForeColor = Color.Gray,
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    infoPanel.Controls.Add(lblSystemInfo);

                    mainPanel.Controls.Add(infoPanel);

                    // Close Button
                    Button btnClose = new Button
                    {
                        Text = "✅ إغلاق",
                        Location = new Point(270, 425),
                        Size = new Size(150, 40),
                        BackColor = ColorScheme.Primary,
                        ForeColor = Color.White,
                        Font = new Font("Cairo", 10F, FontStyle.Bold),
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Hand
                    };
                    btnClose.FlatAppearance.BorderSize = 0;
                    btnClose.Click += (s, e) => detailsForm.Close();
                    mainPanel.Controls.Add(btnClose);

                    detailsForm.Controls.Add(mainPanel);
                    detailsForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في عرض التفاصيل: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddDetailRow(Panel panel, string label, string value, ref int yPos)
        {
            Label lblLabel = new Label
            {
                Text = label,
                Location = new Point(500, yPos),
                Size = new Size(100, 25),
                Font = new Font("Cairo", 10F, FontStyle.Bold)
            };
            panel.Controls.Add(lblLabel);

            Label lblValue = new Label
            {
                Text = value,
                Location = new Point(20, yPos),
                Size = new Size(470, 25),
                Font = new Font("Cairo", 10F),
                TextAlign = ContentAlignment.MiddleLeft
            };
            panel.Controls.Add(lblValue);

            yPos += 35;
        }

        private void EditPayment(int paymentId)
        {
            try
            {
                // Detach ALL tracked entities to avoid any conflicts
                _context.ChangeTracker.Clear();

                // Load payment fresh from database
                var payment = _context.Set<BankTransfer>()
                    .AsNoTracking()
                    .Include(t => t.DestinationBankAccount)
                    .Include(t => t.Trip)
                    .FirstOrDefault(t => t.Id == paymentId);

                if (payment == null)
                {
                    MessageBox.Show("لم يتم العثور على الدفعة", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                decimal originalAmount = payment.Amount;
                int? originalBankId = payment.DestinationBankAccountId;
                int? currentTripId = payment.TripId;

                // Safe date - ensure it's within DateTimePicker range (1753-9998)
                DateTime safeDate;
                try
                {
                    // Handle UTC stored dates carefully to avoid overflow
                    if (payment.TransferDate.Kind == DateTimeKind.Utc)
                        safeDate = payment.TransferDate.ToLocalTime();
                    else if (payment.TransferDate.Kind == DateTimeKind.Unspecified)
                        safeDate = DateTime.SpecifyKind(payment.TransferDate, DateTimeKind.Local);
                    else
                        safeDate = payment.TransferDate;
                    
                    // Hard clamp to safe DateTimePicker range
                    var dtpMin = new DateTime(1753, 1, 1);
                    var dtpMax = new DateTime(9998, 12, 31);
                    if (safeDate < dtpMin || safeDate > dtpMax)
                        safeDate = DateTime.Now;
                }
                catch
                {
                    safeDate = DateTime.Now;
                }

                // Safe amount - clamp within NumericUpDown range
                decimal safeAmount = payment.Amount;
                if (safeAmount < 0m) safeAmount = 0m;
                if (safeAmount > 999999999m) safeAmount = 999999999m;

                using (var editForm = new Form())
                {
                    editForm.Text = "تعديل دفعة فواتيرك";
                    editForm.Size = new Size(600, 480);
                    editForm.RightToLeft = RightToLeft.Yes;
                    editForm.RightToLeftLayout = false;
                    editForm.StartPosition = FormStartPosition.CenterParent;
                    editForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    editForm.MaximizeBox = false;
                    editForm.MinimizeBox = false;
                    editForm.Font = new Font("Cairo", 10F);

                    // Amount
                    editForm.Controls.Add(new Label { Text = "المبلغ:", Location = new Point(450, 30), Size = new Size(100, 25) });
                    // CRITICAL ORDER: Must set Minimum, Maximum, DecimalPlaces BEFORE Value
                    // Also: default Maximum is 100, so if Amount > 100 and we set Value first it throws
                    var numAmount = new NumericUpDown();
                    numAmount.Location = new Point(150, 30);
                    numAmount.Size = new Size(280, 30);
                    numAmount.Font = new Font("Cairo", 10F);
                    numAmount.Minimum = 0m;
                    numAmount.Maximum = 999999999m;
                    numAmount.DecimalPlaces = 2;
                    // Clamp value to valid range before assigning
                    if (safeAmount < 0m) safeAmount = 0m;
                    if (safeAmount > 999999999m) safeAmount = 999999999m;
                    numAmount.Value = safeAmount;
                    editForm.Controls.Add(numAmount);

                    // Reference Number
                    editForm.Controls.Add(new Label { Text = "رقم المرجع:", Location = new Point(450, 80), Size = new Size(100, 25) });
                    var txtRef = new TextBox { Location = new Point(150, 80), Size = new Size(280, 30), Text = payment.ReferenceNumber ?? "", Font = new Font("Cairo", 10F) };
                    editForm.Controls.Add(txtRef);

                    // Transfer Date
                    editForm.Controls.Add(new Label { Text = "التاريخ:", Location = new Point(450, 130), Size = new Size(100, 25) });
                    var dtpDate = new DateTimePicker();
                    dtpDate.Location = new Point(150, 130);
                    dtpDate.Size = new Size(280, 30);
                    dtpDate.Format = DateTimePickerFormat.Short;
                    dtpDate.Font = new Font("Cairo", 10F);
                    // Set Value after all other properties to avoid range issues
                    try
                    {
                        var dtpMin = new DateTime(1753, 1, 2);
                        var dtpMax = new DateTime(9998, 12, 30);
                        dtpDate.Value = (safeDate < dtpMin || safeDate > dtpMax) ? DateTime.Now : safeDate;
                    }
                    catch { dtpDate.Value = DateTime.Now; }
                    editForm.Controls.Add(dtpDate);

                    // Trip ComboBox
                    editForm.Controls.Add(new Label { Text = "الرحلة:", Location = new Point(450, 180), Size = new Size(100, 25) });
                    var cmbTrip = new ComboBox { Location = new Point(150, 180), Size = new Size(280, 30), Font = new Font("Cairo", 10F), DropDownStyle = ComboBoxStyle.DropDownList };

                    var tripItems = new List<TripComboItem>();
                    tripItems.Add(new TripComboItem { Id = -1, DisplayName = "-- بدون رحلة --" });
                    foreach (var t in _context.Set<Trip>().AsNoTracking().Where(t => t.IsActive).OrderByDescending(t => t.StartDate).ToList())
                        tripItems.Add(new TripComboItem { Id = t.TripId, DisplayName = $"{t.TripName} - {t.StartDate:dd/MM/yyyy}" });

                    // IMPORTANT: Set DataSource BEFORE SelectedIndex to avoid ComboBox exception
                    cmbTrip.DataSource = null;
                    cmbTrip.Items.Clear();
                    cmbTrip.DisplayMember = "DisplayName";
                    cmbTrip.ValueMember = "Id";
                    cmbTrip.DataSource = tripItems;

                    // Only set SelectedIndex if there are items
                    if (cmbTrip.Items.Count > 0)
                        cmbTrip.SelectedIndex = 0;

                    // Try to select the current trip
                    if (currentTripId.HasValue && currentTripId.Value > 0)
                    {
                        var found = tripItems.FirstOrDefault(t => t.Id == currentTripId.Value);
                        if (found != null) cmbTrip.SelectedItem = found;
                    }
                    editForm.Controls.Add(cmbTrip);

                    // Notes
                    editForm.Controls.Add(new Label { Text = "الملاحظات:", Location = new Point(450, 230), Size = new Size(100, 25) });
                    string cleanNotes = "";
                    if (!string.IsNullOrEmpty(payment.Notes))
                    {
                        cleanNotes = string.Join("\n", payment.Notes.Split('\n')
                            .Where(l => !l.Contains("فواتيرك") && !l.Contains("العميل:") && !l.Contains("الرحلة:") && !string.IsNullOrWhiteSpace(l))
                            .Select(l => l.Trim()));
                    }
                    var txtNotes = new TextBox { Location = new Point(150, 230), Size = new Size(280, 80), Multiline = true, ScrollBars = ScrollBars.Vertical, Text = cleanNotes, Font = new Font("Cairo", 10F) };
                    editForm.Controls.Add(txtNotes);

                    // Save Button
                    var btnSave = new Button { Text = "💾 حفظ", Location = new Point(300, 330), Size = new Size(130, 40), BackColor = ColorScheme.Success, ForeColor = Color.White, Font = new Font("Cairo", 10F, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
                    btnSave.FlatAppearance.BorderSize = 0;
                    btnSave.Click += (s, e) =>
                    {
                        try
                        {
                            // Extract customer info from original notes
                            string customerInfo = "";
                            string existingNotes = "";
                            if (!string.IsNullOrEmpty(payment.Notes))
                            {
                                foreach (var line in payment.Notes.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()))
                                {
                                    if (line.Contains("العميل:"))
                                        customerInfo = line.Contains("دفعة من فواتيرك - العميل:")
                                            ? "العميل:" + line.Split(new[] { "العميل:" }, StringSplitOptions.None)[1].Trim()
                                            : line;
                                    else if (!line.Contains("الرحلة:") && !line.Contains("دفعة من فواتيرك") && !string.IsNullOrWhiteSpace(line))
                                        existingNotes += (string.IsNullOrEmpty(existingNotes) ? "" : "\n") + line;
                                }
                            }

                            if (string.IsNullOrEmpty(customerInfo))
                            {
                                MessageBox.Show("خطأ: هذه الدفعة ليست دفعة فواتيرك!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            int? selectedTripId = null;
                            string tripInfo = "";
                            if (cmbTrip.SelectedItem is TripComboItem sel && sel.Id > 0)
                            {
                                selectedTripId = sel.Id;
                                var trip = _context.Set<Trip>().AsNoTracking().FirstOrDefault(t => t.TripId == sel.Id);
                                if (trip != null) tripInfo = $"\nالرحلة: {trip.TripName} - {trip.StartDate:dd/MM/yyyy}";
                            }

                            decimal newAmount = numAmount.Value;
                            string newNotes = "دفعة من فواتيرك - " + customerInfo + tripInfo;
                            string userNotes = string.IsNullOrWhiteSpace(txtNotes.Text) ? existingNotes : txtNotes.Text.Trim();
                            if (!string.IsNullOrEmpty(userNotes)) newNotes += "\n" + userNotes;

                            var strategy = _context.Database.CreateExecutionStrategy();
                            strategy.Execute(() =>
                            {
                                using var transaction = _context.Database.BeginTransaction();
                                _context.ChangeTracker.Clear();

                                // IMPORTANT: Must use AsTracking() because context default is NoTracking
                                // Without tracking, SaveChanges won't detect changes
                                var toUpdate = _context.Set<BankTransfer>()
                                    .AsTracking()
                                    .FirstOrDefault(t => t.Id == paymentId)
                                    ?? throw new Exception("لم يتم العثور على الدفعة");

                                toUpdate.Amount = newAmount;
                                toUpdate.ReferenceNumber = txtRef.Text.Trim();
                                toUpdate.TransferDate = DateTime.SpecifyKind(dtpDate.Value, DateTimeKind.Utc);
                                toUpdate.Notes = newNotes;
                                toUpdate.TripId = selectedTripId;

                                if (originalAmount != newAmount && originalBankId.HasValue)
                                {
                                    var bank = _context.Set<BankAccount>()
                                        .AsTracking()
                                        .FirstOrDefault(b => b.Id == originalBankId.Value);
                                    if (bank != null) bank.Balance += (newAmount - originalAmount);
                                }

                                int saved = _context.SaveChanges();
                                if (saved == 0) throw new Exception("لم يتم حفظ أي تغييرات - تأكد من تعديل البيانات");
                                transaction.Commit();
                            });

                            MessageBox.Show("تم تحديث الدفعة بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            editForm.DialogResult = DialogResult.OK;
                            editForm.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"خطأ في حفظ التعديلات: {ex.Message}\n{ex.InnerException?.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };
                    editForm.Controls.Add(btnSave);

                    var btnCancel = new Button { Text = "❌ إلغاء", Location = new Point(150, 330), Size = new Size(130, 40), BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, Font = new Font("Cairo", 10F, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
                    btnCancel.FlatAppearance.BorderSize = 0;
                    btnCancel.Click += (s, e) => editForm.Close();
                    editForm.Controls.Add(btnCancel);

                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        // Force clear ALL cached entities before reload so we get fresh data from DB
                        _context.ChangeTracker.Clear();
                        LoadPayments();
                    }
                }
            }
            catch (Exception ex)
            {
                // Show full stack trace to identify exact line causing the error
                string fullTrace = ex.StackTrace ?? "";
                string[] traceLines = fullTrace.Split('\n');
                string relevantLines = string.Join("\n", traceLines.Take(5));
                MessageBox.Show(
                    $"خطأ في تعديل الدفعة:\n{ex.Message}\n\nInner: {ex.InnerException?.Message}\n\nStack:\n{relevantLines}",
                    "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper class to avoid anonymous type DataBinding issues with ComboBox
        private class TripComboItem
        {
            public int Id { get; set; }
            public string DisplayName { get; set; } = "";
        }

        private void DeletePayment(int paymentId)
        {
            try
            {
                // Detach ALL tracked entities to avoid conflicts
                foreach (var entry in _context.ChangeTracker.Entries().ToList())
                {
                    entry.State = EntityState.Detached;
                }

                // Load payment info (no tracking needed, we'll use raw SQL)
                var payment = _context.Set<BankTransfer>()
                    .AsNoTracking()
                    .Include(t => t.DestinationBankAccount)
                    .FirstOrDefault(t => t.Id == paymentId);

                if (payment == null)
                {
                    MessageBox.Show("لم يتم العثور على الدفعة", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var result = MessageBox.Show(
                    $"هل أنت متأكد من حذف هذه الدفعة؟\n\nالمبلغ: {payment.Amount:N2} ج.م\nرقم المرجع: {payment.ReferenceNumber ?? "غير محدد"}",
                    "تأكيد الحذف",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    var strategy = _context.Database.CreateExecutionStrategy();
                    strategy.Execute(() =>
                    {
                        using (var transaction = _context.Database.BeginTransaction())
                        {
                            try
                            {
                                // Clear tracking to avoid conflicts
                                _context.ChangeTracker.Clear();

                                // Get and attach the payment entity
                                var paymentToDelete = _context.Set<BankTransfer>().Find(paymentId);
                                if (paymentToDelete == null)
                                {
                                    throw new Exception("لم يتم العثور على الدفعة");
                                }

                                // Restore bank balance before deleting
                                if (paymentToDelete.DestinationBankAccountId.HasValue && paymentToDelete.Amount > 0)
                                {
                                    var bankAccount = _context.Set<BankAccount>().Find(paymentToDelete.DestinationBankAccountId.Value);
                                    if (bankAccount != null)
                                    {
                                        bankAccount.Balance -= paymentToDelete.Amount;
                                        Console.WriteLine($"Bank balance updated: {bankAccount.Balance + paymentToDelete.Amount} - {paymentToDelete.Amount} = {bankAccount.Balance}");
                                    }
                                }

                                // Delete the payment
                                _context.Set<BankTransfer>().Remove(paymentToDelete);
                                
                                // Save changes
                                int changes = _context.SaveChanges();
                                Console.WriteLine($"Delete changes saved: {changes} entities");

                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"ERROR in delete transaction: {ex.Message}");
                                transaction.Rollback();
                                throw;
                            }
                        }
                    });

                    MessageBox.Show("تم حذف الدفعة بنجاح", "نجح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadPayments();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حذف الدفعة: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
