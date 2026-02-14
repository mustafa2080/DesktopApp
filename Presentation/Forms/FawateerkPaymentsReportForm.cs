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
                var query = _context.Set<BankTransfer>()
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
                foreach (var entry in _context.ChangeTracker.Entries().ToList())
                {
                    entry.State = EntityState.Detached;
                }

                // Load payment fresh from database
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

                // Store original values for later
                decimal originalAmount = payment.Amount;
                int? originalBankId = payment.DestinationBankAccountId;
                decimal originalBankBalance = payment.DestinationBankAccount?.Balance ?? 0;

                using (var editForm = new Form())
                {
                    editForm.Text = "تعديل دفعة فواتيرك";
                    editForm.Size = new Size(600, 620);
                    editForm.RightToLeft = RightToLeft.Yes;
                    editForm.RightToLeftLayout = false;
                    editForm.StartPosition = FormStartPosition.CenterParent;
                    editForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    editForm.MaximizeBox = false;
                    editForm.MinimizeBox = false;
                    editForm.Font = new Font("Cairo", 10F);

                    // Extract current trip name from notes
                    string currentTripName = "";
                    if (!string.IsNullOrEmpty(payment.Notes))
                    {
                        var lines = payment.Notes.Split('\n');
                        foreach (var line in lines)
                        {
                            if (line.Contains("الرحلة:"))
                            {
                                currentTripName = line.Replace("الرحلة:", "").Trim();
                                break;
                            }
                        }
                    }

                    // Amount
                    Label lblAmount = new Label
                    {
                        Text = "المبلغ:",
                        Location = new Point(450, 30),
                        Size = new Size(100, 25)
                    };
                    editForm.Controls.Add(lblAmount);

                    NumericUpDown numAmount = new NumericUpDown
                    {
                        Location = new Point(150, 30),
                        Size = new Size(280, 30),
                        Maximum = 1000000000m,
                        DecimalPlaces = 2,
                        Font = new Font("Cairo", 10F)
                    };
                    // Set Value BEFORE Minimum to avoid ArgumentOutOfRangeException
                    numAmount.Value = payment.Amount <= 0 ? 100m : payment.Amount;
                    numAmount.Minimum = 0.01m;
                    editForm.Controls.Add(numAmount);

                    // Reference Number
                    Label lblRef = new Label
                    {
                        Text = "رقم المرجع:",
                        Location = new Point(450, 80),
                        Size = new Size(100, 25)
                    };
                    editForm.Controls.Add(lblRef);

                    TextBox txtRef = new TextBox
                    {
                        Location = new Point(150, 80),
                        Size = new Size(280, 30),
                        Text = payment.ReferenceNumber ?? "",
                        Font = new Font("Cairo", 10F)
                    };
                    editForm.Controls.Add(txtRef);

                    // Transfer Date
                    Label lblDate = new Label
                    {
                        Text = "التاريخ:",
                        Location = new Point(450, 130),
                        Size = new Size(100, 25)
                    };
                    editForm.Controls.Add(lblDate);

                    DateTimePicker dtpDate = new DateTimePicker
                    {
                        Location = new Point(150, 130),
                        Size = new Size(280, 30),
                        Value = payment.TransferDate.ToLocalTime(),
                        Format = DateTimePickerFormat.Custom,
                        CustomFormat = "dd/MM/yyyy hh:mm tt",
                        ShowUpDown = false,
                        Font = new Font("Cairo", 10F)
                    };
                    editForm.Controls.Add(dtpDate);

                    // Trip ComboBox
                    Label lblTrip = new Label
                    {
                        Text = "الرحلة:",
                        Location = new Point(450, 180),
                        Size = new Size(100, 25)
                    };
                    editForm.Controls.Add(lblTrip);

                    ComboBox cmbTrip = new ComboBox
                    {
                        Location = new Point(150, 180),
                        Size = new Size(280, 30),
                        Font = new Font("Cairo", 10F),
                        DropDownStyle = ComboBoxStyle.DropDownList
                    };

                    // Load trips
                    var trips = _context.Set<Trip>()
                        .AsNoTracking()
                        .Where(t => t.IsActive)
                        .OrderByDescending(t => t.StartDate)
                        .Select(t => new
                        {
                            Id = t.TripId,
                            Name = t.TripName,
                            DisplayName = $"{t.TripName} - {t.StartDate:dd/MM/yyyy}"
                        })
                        .ToList();

                    trips.Insert(0, new { Id = 0, Name = "", DisplayName = "-- بدون رحلة --" });
                    cmbTrip.DataSource = trips;
                    cmbTrip.DisplayMember = "DisplayName";
                    cmbTrip.ValueMember = "Id";

                    // Try to select current trip - match by DisplayName or Name
                    if (!string.IsNullOrEmpty(currentTripName))
                    {
                        // First try exact match on DisplayName (notes store: "TripName - dd/MM/yyyy")
                        var selectedTrip = trips.FirstOrDefault(t =>
                            !string.IsNullOrEmpty(t.DisplayName) &&
                            t.DisplayName.Trim().Equals(currentTripName.Trim(), StringComparison.OrdinalIgnoreCase));

                        // If no exact match, try matching just the trip name part
                        if (selectedTrip == null)
                        {
                            string tripNameOnly = currentTripName;
                            if (currentTripName.Contains(" - "))
                            {
                                tripNameOnly = currentTripName.Split(new[] { " - " }, StringSplitOptions.None)[0].Trim();
                            }
                            selectedTrip = trips.FirstOrDefault(t =>
                                !string.IsNullOrEmpty(t.Name) &&
                                t.Name.Trim().Equals(tripNameOnly, StringComparison.OrdinalIgnoreCase));
                        }

                        // If still no match, try contains
                        if (selectedTrip == null)
                        {
                            string tripNameOnly = currentTripName.Contains(" - ")
                                ? currentTripName.Split(new[] { " - " }, StringSplitOptions.None)[0].Trim()
                                : currentTripName;
                            selectedTrip = trips.FirstOrDefault(t =>
                                !string.IsNullOrEmpty(t.Name) &&
                                t.Name.Contains(tripNameOnly));
                        }

                        if (selectedTrip != null)
                        {
                            cmbTrip.SelectedValue = selectedTrip.Id;
                        }
                        else
                        {
                            cmbTrip.SelectedIndex = 0;
                        }
                    }
                    else
                    {
                        cmbTrip.SelectedIndex = 0;
                    }

                    editForm.Controls.Add(cmbTrip);

                    // Notes
                    Label lblNotes = new Label
                    {
                        Text = "الملاحظات:",
                        Location = new Point(450, 230),
                        Size = new Size(100, 25)
                    };
                    editForm.Controls.Add(lblNotes);

                    // Extract clean notes (without customer and trip info)
                    string cleanNotes = "";
                    if (!string.IsNullOrEmpty(payment.Notes))
                    {
                        var lines = payment.Notes.Split('\n');
                        var noteLines = new List<string>();
                        foreach (var line in lines)
                        {
                            if (!line.Contains("فواتيرك") && !line.Contains("العميل:") && !line.Contains("الرحلة:") && !string.IsNullOrWhiteSpace(line))
                            {
                                noteLines.Add(line.Trim());
                            }
                        }
                        cleanNotes = string.Join("\n", noteLines);
                    }

                    TextBox txtNotes = new TextBox
                    {
                        Location = new Point(150, 230),
                        Size = new Size(280, 120),
                        Multiline = true,
                        ScrollBars = ScrollBars.Vertical,
                        Text = cleanNotes,
                        Font = new Font("Cairo", 10F)
                    };
                    editForm.Controls.Add(txtNotes);

                    // Save Button
                    Button btnSave = new Button
                    {
                        Text = "💾 حفظ",
                        Location = new Point(300, 370),
                        Size = new Size(130, 40),
                        BackColor = ColorScheme.Success,
                        ForeColor = Color.White,
                        Font = new Font("Cairo", 10F, FontStyle.Bold),
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Hand
                    };
                    btnSave.FlatAppearance.BorderSize = 0;
                    btnSave.Click += (s, e) =>
                    {
                        try
                        {
                            // Validate amount
                            if (numAmount.Value <= 0)
                            {
                                MessageBox.Show("المبلغ يجب أن يكون أكبر من صفر", "تنبيه",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            // Extract customer info from original notes
                            string customerInfo = "";
                            string existingNotes = "";
                            if (!string.IsNullOrEmpty(payment.Notes))
                            {
                                var lines = payment.Notes.Split('\n');
                                foreach (var line in lines)
                                {
                                    if (line.Contains("دفعة من فواتيرك") || line.Contains("العميل:"))
                                    {
                                        if (line.Contains("العميل:"))
                                        {
                                            customerInfo = line;
                                        }
                                    }
                                    else if (!line.Contains("الرحلة:") && !string.IsNullOrWhiteSpace(line))
                                    {
                                        existingNotes += (string.IsNullOrEmpty(existingNotes) ? "" : "\n") + line;
                                    }
                                }
                            }
                            
                            // If no customer info found, this is NOT a Fawateerk payment - abort!
                            if (string.IsNullOrEmpty(customerInfo))
                            {
                                MessageBox.Show("خطأ: هذه الدفعة ليست دفعة فواتيرك!", "خطأ",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            // Build trip info
                            string tripInfo = "";
                            Console.WriteLine($"DEBUG SAVE: cmbTrip.SelectedValue = {cmbTrip.SelectedValue}");
                            Console.WriteLine($"DEBUG SAVE: cmbTrip.SelectedIndex = {cmbTrip.SelectedIndex}");
                            Console.WriteLine($"DEBUG SAVE: cmbTrip.Text = {cmbTrip.Text}");
                            
                            if (cmbTrip.SelectedValue != null && Convert.ToInt32(cmbTrip.SelectedValue) > 0)
                            {
                                int tripId = Convert.ToInt32(cmbTrip.SelectedValue);
                                Console.WriteLine($"DEBUG SAVE: Selected trip ID: {tripId}");
                                
                                var trip = _context.Set<Trip>().AsNoTracking().FirstOrDefault(t => t.TripId == tripId);
                                if (trip != null)
                                {
                                    tripInfo = $"\nالرحلة: {trip.TripName} - {trip.StartDate:dd/MM/yyyy}";
                                    Console.WriteLine($"DEBUG SAVE: Trip info built: {tripInfo}");
                                }
                                else
                                {
                                    Console.WriteLine($"DEBUG SAVE: Trip not found in database with ID: {tripId}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"DEBUG SAVE: No trip selected or trip ID is 0");
                            }

                            decimal newAmount = numAmount.Value;

                            // Rebuild notes with customer, trip, and user notes
                            string newNotes = "دفعة من فواتيرك - " + customerInfo;
                            if (!string.IsNullOrEmpty(tripInfo))
                            {
                                newNotes += tripInfo;
                            }
                            if (!string.IsNullOrWhiteSpace(txtNotes.Text))
                            {
                                newNotes += "\n" + txtNotes.Text.Trim();
                            }
                            else if (!string.IsNullOrEmpty(existingNotes))
                            {
                                // Preserve existing notes if user didn't edit them
                                newNotes += "\n" + existingNotes;
                            }

                            var newDate = DateTime.SpecifyKind(dtpDate.Value, DateTimeKind.Utc);
                            var newRef = txtRef.Text.Trim();

                            // Diagnostic logging
                            Console.WriteLine($"=== FAWATEERK SAVE DEBUG ===");
                            Console.WriteLine($"Payment ID: {paymentId}");
                            Console.WriteLine($"New Amount: {newAmount}");
                            Console.WriteLine($"New Ref: {newRef}");
                            Console.WriteLine($"New Date: {newDate}");
                            Console.WriteLine($"New Notes: {newNotes}");
                            Console.WriteLine($"Original Amount: {originalAmount}");
                            Console.WriteLine($"Trip Info: {tripInfo}");
                            Console.WriteLine($"=== END DEBUG ===");

                            // Use execution strategy with proper transaction handling
                            var strategy = _context.Database.CreateExecutionStrategy();
                            strategy.Execute(() =>
                            {
                                using (var transaction = _context.Database.BeginTransaction())
                                {
                                    try
                                    {
                                        // Detach any existing tracked entities
                                        _context.ChangeTracker.Clear();

                                        // Fetch and attach the entity
                                        var paymentToUpdate = _context.Set<BankTransfer>().Find(paymentId);
                                        if (paymentToUpdate == null)
                                        {
                                            throw new Exception("لم يتم العثور على الدفعة");
                                        }

                                        // Update payment fields
                                        paymentToUpdate.Amount = newAmount;
                                        paymentToUpdate.ReferenceNumber = newRef;
                                        paymentToUpdate.TransferDate = newDate;
                                        paymentToUpdate.Notes = newNotes;

                                        // If amount changed, update bank balance
                                        if (originalAmount != newAmount && originalBankId.HasValue)
                                        {
                                            var bankAccount = _context.Set<BankAccount>().Find(originalBankId.Value);
                                            if (bankAccount != null)
                                            {
                                                decimal balanceDiff = newAmount - originalAmount;
                                                bankAccount.Balance += balanceDiff;
                                                Console.WriteLine($"Bank balance updated: {originalBankBalance} + {balanceDiff} = {bankAccount.Balance}");
                                            }
                                        }

                                        // Save changes
                                        int changesSaved = _context.SaveChanges();
                                        Console.WriteLine($"Changes saved: {changesSaved} entities");

                                        transaction.Commit();
                                        Console.WriteLine("Transaction committed successfully");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"ERROR in transaction: {ex.Message}");
                                        transaction.Rollback();
                                        throw;
                                    }
                                }
                            });

                            MessageBox.Show("تم تحديث الدفعة بنجاح", "نجح",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            editForm.DialogResult = DialogResult.OK;
                            editForm.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"خطأ في حفظ التعديلات: {ex.Message}\n\nالتفاصيل: {ex.InnerException?.Message}", "خطأ",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };
                    editForm.Controls.Add(btnSave);

                    // Cancel Button
                    Button btnCancel = new Button
                    {
                        Text = "❌ إلغاء",
                        Location = new Point(150, 370),
                        Size = new Size(130, 40),
                        BackColor = Color.FromArgb(108, 117, 125),
                        ForeColor = Color.White,
                        Font = new Font("Cairo", 10F, FontStyle.Bold),
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Hand
                    };
                    btnCancel.FlatAppearance.BorderSize = 0;
                    btnCancel.Click += (s, e) => editForm.Close();
                    editForm.Controls.Add(btnCancel);

                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadPayments();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تعديل الدفعة: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
