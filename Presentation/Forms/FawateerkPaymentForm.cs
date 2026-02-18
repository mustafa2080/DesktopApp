using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GraceWay.AccountingSystem.Infrastructure.Data;
using GraceWay.AccountingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Presentation.Forms
{
    public partial class FawateerkPaymentForm : Form
    {
        private readonly AppDbContext _context;
        private readonly int _currentUserId;

        private ComboBox cmbBankAccount;
        private ComboBox cmbCustomer;
        private ComboBox cmbTrip;
        private TextBox txtAmount;
        private TextBox txtReferenceNumber;
        private DateTimePicker dtpPaymentDate;
        private TextBox txtNotes;
        private TextBox txtCustomerPhone;
        private TextBox txtCustomerEmail;
        private Button btnSave;
        private Button btnCancel;
        private Label lblBankBalance;
        private CheckBox chkCreateJournalEntry;

        public FawateerkPaymentForm(AppDbContext context, int currentUserId)
        {
            _context = context;
            _currentUserId = currentUserId;
            InitializeComponent();
            InitializeCustomComponents();
            LoadData();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "تسجيل دفعة من فواتيرك";
            this.Size = new Size(750, 850);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = Color.White;
            this.Font = new Font("Cairo", 10F);

            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                AutoScroll = true,
                BackColor = Color.White
            };

            int yPos = 10;

            // Title
            Label lblTitle = new Label
            {
                Text = "💳 تسجيل دفعة من فواتيرك",
                Location = new Point(20, yPos),
                Size = new Size(690, 40),
                Font = new Font("Cairo", 16F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblTitle);
            yPos += 50;

            // Info Panel
            Panel infoPanel = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(690, 60),
                BackColor = Color.FromArgb(230, 244, 255),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblInfo = new Label
            {
                Text = "📋 سجل هنا المدفوعات التي تم استلامها من العملاء عبر منصة فواتيرك\n" +
                       "سيتم إضافة المبلغ تلقائياً إلى رصيد البنك المحدد",
                Location = new Point(10, 10),
                Size = new Size(670, 40),
                Font = new Font("Cairo", 9F),
                ForeColor = Color.FromArgb(0, 102, 204),
                TextAlign = ContentAlignment.MiddleCenter
            };
            infoPanel.Controls.Add(lblInfo);
            mainPanel.Controls.Add(infoPanel);
            yPos += 70;

            // Bank Account Section
            Panel bankPanel = CreateSection("🏦 البنك المستلم", yPos);
            mainPanel.Controls.Add(bankPanel);
            yPos += 10;

            Label lblBank = CreateLabel("اختر الحساب البنكي:", yPos);
            mainPanel.Controls.Add(lblBank);

            cmbBankAccount = new ComboBox
            {
                Location = new Point(20, yPos + 30),
                Size = new Size(690, 30),
                Font = new Font("Cairo", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbBankAccount.SelectedIndexChanged += CmbBankAccount_SelectedIndexChanged;
            mainPanel.Controls.Add(cmbBankAccount);
            yPos += 65;

            lblBankBalance = new Label
            {
                Text = "الرصيد الحالي: 0.00 ج.م",
                Location = new Point(20, yPos),
                Size = new Size(690, 25),
                Font = new Font("Cairo", 9F, FontStyle.Bold),
                ForeColor = ColorScheme.Success
            };
            mainPanel.Controls.Add(lblBankBalance);
            yPos += 40;

            // Customer Section
            Panel customerPanel = CreateSection("👤 بيانات العميل", yPos);
            mainPanel.Controls.Add(customerPanel);
            yPos += 10;

            Label lblCustomer = CreateLabel("اختر العميل:", yPos);
            mainPanel.Controls.Add(lblCustomer);

            cmbCustomer = new ComboBox
            {
                Location = new Point(20, yPos + 30),
                Size = new Size(690, 30),
                Font = new Font("Cairo", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCustomer.SelectedIndexChanged += CmbCustomer_SelectedIndexChanged;
            mainPanel.Controls.Add(cmbCustomer);
            yPos += 65;

            Label lblPhone = CreateLabel("رقم الهاتف:", yPos);
            mainPanel.Controls.Add(lblPhone);

            txtCustomerPhone = CreateTextBox(yPos + 30);
            txtCustomerPhone.ReadOnly = true;
            txtCustomerPhone.BackColor = Color.FromArgb(245, 245, 245);
            mainPanel.Controls.Add(txtCustomerPhone);
            yPos += 65;

            Label lblEmail = CreateLabel("البريد الإلكتروني:", yPos);
            mainPanel.Controls.Add(lblEmail);

            txtCustomerEmail = CreateTextBox(yPos + 30);
            txtCustomerEmail.ReadOnly = true;
            txtCustomerEmail.BackColor = Color.FromArgb(245, 245, 245);
            mainPanel.Controls.Add(txtCustomerEmail);
            yPos += 70;

            // Trip Section
            Panel tripPanel = CreateSection("✈️ الرحلة", yPos);
            mainPanel.Controls.Add(tripPanel);
            yPos += 10;

            Label lblTrip = CreateLabel("اختر الرحلة (اختياري):", yPos);
            mainPanel.Controls.Add(lblTrip);

            cmbTrip = new ComboBox
            {
                Location = new Point(20, yPos + 30),
                Size = new Size(690, 30),
                Font = new Font("Cairo", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            mainPanel.Controls.Add(cmbTrip);
            yPos += 70;

            // Payment Details Section
            Panel paymentPanel = CreateSection("💰 تفاصيل الدفعة", yPos);
            mainPanel.Controls.Add(paymentPanel);
            yPos += 10;

            Label lblAmount = CreateLabel("المبلغ المستلم:", yPos);
            mainPanel.Controls.Add(lblAmount);

            txtAmount = CreateTextBox(yPos + 30);
            txtAmount.TextAlign = HorizontalAlignment.Right;
            mainPanel.Controls.Add(txtAmount);
            yPos += 65;

            Label lblDate = CreateLabel("تاريخ الدفعة:", yPos);
            mainPanel.Controls.Add(lblDate);

            dtpPaymentDate = new DateTimePicker
            {
                Location = new Point(20, yPos + 30),
                Size = new Size(690, 30),
                Font = new Font("Cairo", 10F),
                Format = DateTimePickerFormat.Short
            };
            mainPanel.Controls.Add(dtpPaymentDate);
            yPos += 65;

            Label lblRef = CreateLabel("رقم المرجع من فواتيرك:", yPos);
            mainPanel.Controls.Add(lblRef);

            txtReferenceNumber = CreateTextBox(yPos + 30);
            txtReferenceNumber.PlaceholderText = "مثال: FWT-2024-001234";
            mainPanel.Controls.Add(txtReferenceNumber);
            yPos += 65;

            Label lblNotes = CreateLabel("ملاحظات:", yPos);
            mainPanel.Controls.Add(lblNotes);

            txtNotes = new TextBox
            {
                Location = new Point(20, yPos + 30),
                Size = new Size(690, 80),
                Font = new Font("Cairo", 10F),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            mainPanel.Controls.Add(txtNotes);
            yPos += 120;

            // Journal Entry Option
            chkCreateJournalEntry = new CheckBox
            {
                Text = "✓ إنشاء قيد محاسبي تلقائي",
                Location = new Point(20, yPos),
                Size = new Size(690, 30),
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                Checked = true
            };
            mainPanel.Controls.Add(chkCreateJournalEntry);
            yPos += 40;

            // Buttons
            Panel buttonsPanel = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(690, 50),
                BackColor = Color.Transparent
            };

            btnSave = new Button
            {
                Text = "💾 حفظ الدفعة",
                Location = new Point(0, 0),
                Size = new Size(200, 45),
                BackColor = ColorScheme.Success,
                ForeColor = Color.White,
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            buttonsPanel.Controls.Add(btnSave);

            btnCancel = new Button
            {
                Text = "❌ إلغاء",
                Location = new Point(210, 0),
                Size = new Size(150, 45),
                BackColor = ColorScheme.Error,
                ForeColor = Color.White,
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            buttonsPanel.Controls.Add(btnCancel);

            mainPanel.Controls.Add(buttonsPanel);

            this.Controls.Add(mainPanel);
        }

        private Panel CreateSection(string title, int yPos)
        {
            Panel section = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(690, 35),
                BackColor = Color.FromArgb(240, 240, 240),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblSection = new Label
            {
                Text = title,
                Location = new Point(10, 7),
                Size = new Size(670, 20),
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary
            };
            section.Controls.Add(lblSection);

            return section;
        }

        private Label CreateLabel(string text, int yPos)
        {
            return new Label
            {
                Text = text,
                Location = new Point(20, yPos),
                Size = new Size(690, 25),
                Font = new Font("Cairo", 10F),
                ForeColor = Color.FromArgb(50, 50, 50)
            };
        }

        private TextBox CreateTextBox(int yPos)
        {
            return new TextBox
            {
                Location = new Point(20, yPos),
                Size = new Size(690, 30),
                Font = new Font("Cairo", 10F)
            };
        }

        private void LoadData()
        {
            LoadBankAccounts();
            LoadCustomers();
            LoadTrips();
        }

        private void LoadBankAccounts()
        {
            try
            {
                var banks = _context.Set<BankAccount>()
                    .Where(b => b.IsActive)
                    .OrderBy(b => b.BankName)
                    .Select(b => new
                    {
                        b.Id,
                        DisplayName = $"{b.BankName} - {b.AccountNumber} ({b.Currency})"
                    })
                    .ToList();

                cmbBankAccount.DataSource = banks;
                cmbBankAccount.DisplayMember = "DisplayName";
                cmbBankAccount.ValueMember = "Id";
                cmbBankAccount.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الحسابات البنكية: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCustomers()
        {
            try
            {
                var customers = _context.Set<Customer>()
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.CustomerName)
                    .Select(c => new
                    {
                        c.CustomerId,
                        c.CustomerName,
                        c.Phone,
                        c.Email
                    })
                    .ToList();

                var customerList = customers.Select(c => new
                {
                    Id = c.CustomerId,
                    DisplayName = $"{c.CustomerName} - {c.Phone}"
                }).ToList();

                cmbCustomer.DataSource = customerList;
                cmbCustomer.DisplayMember = "DisplayName";
                cmbCustomer.ValueMember = "Id";
                cmbCustomer.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل العملاء: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTrips()
        {
            try
            {
                var trips = _context.Set<Trip>()
                    .Where(t => t.IsActive)
                    .OrderByDescending(t => t.StartDate)
                    .Select(t => new
                    {
                        Id = t.TripId,
                        DisplayName = $"{t.TripName} - {t.StartDate:dd/MM/yyyy}"
                    })
                    .ToList();

                trips.Insert(0, new { Id = 0, DisplayName = "-- بدون رحلة --" });

                cmbTrip.DataSource = trips;
                cmbTrip.DisplayMember = "DisplayName";
                cmbTrip.ValueMember = "Id";
                cmbTrip.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الرحلات: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbBankAccount_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbBankAccount.SelectedValue != null)
            {
                try
                {
                    int bankId = Convert.ToInt32(cmbBankAccount.SelectedValue);
                    var bank = _context.Set<BankAccount>().FirstOrDefault(b => b.Id == bankId);
                    if (bank != null)
                    {
                        lblBankBalance.Text = $"الرصيد الحالي: {bank.Balance:N2} {bank.Currency}";
                        lblBankBalance.ForeColor = bank.Balance >= 0 ? ColorScheme.Success : ColorScheme.Error;
                    }
                }
                catch { }
            }
        }

        private void CmbCustomer_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbCustomer.SelectedValue != null)
            {
                try
                {
                    int customerId = Convert.ToInt32(cmbCustomer.SelectedValue);
                    var customer = _context.Set<Customer>().FirstOrDefault(c => c.CustomerId == customerId);
                    if (customer != null)
                    {
                        txtCustomerPhone.Text = customer.Phone ?? "";
                        txtCustomerEmail.Text = customer.Email ?? "";
                    }
                }
                catch { }
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            var result = MessageBox.Show("هل أنت متأكد من حفظ هذه الدفعة؟",
                "تأكيد الحفظ",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);

            if (result == DialogResult.No)
                return;

            try
            {
                decimal amount = decimal.Parse(txtAmount.Text);
                int bankId = Convert.ToInt32(cmbBankAccount.SelectedValue);
                int customerId = Convert.ToInt32(cmbCustomer.SelectedValue);

                // Use execution strategy for PostgreSQL
                var strategy = _context.Database.CreateExecutionStrategy();
                strategy.Execute(() =>
                {
                    using (var transaction = _context.Database.BeginTransaction())
                    {
                        try
                        {
                            // Update bank balance
                            var bank = _context.Set<BankAccount>().Find(bankId);
                            if (bank != null)
                            {
                                bank.Balance += amount;
                                bank.ModifiedDate = DateTime.UtcNow;
                                bank.ModifiedBy = _currentUserId;
                            }

                            // Get trip info if selected
                            string tripInfo = "";
                            int? selectedTripId = null;
                            
                            if (cmbTrip.SelectedValue != null && Convert.ToInt32(cmbTrip.SelectedValue) > 0)
                            {
                                int tripId = Convert.ToInt32(cmbTrip.SelectedValue);
                                selectedTripId = tripId;
                                
                                var trip = _context.Set<Trip>().FirstOrDefault(t => t.TripId == tripId);
                                if (trip != null)
                                {
                                    tripInfo = $"\nالرحلة: {trip.TripName} - {trip.StartDate:dd/MM/yyyy}";
                                }
                            }

                            // Create bank transfer record
                            var transfer = new BankTransfer
                            {
                                DestinationBankAccountId = bankId,
                                Amount = amount,
                                TransferType = "FawateerkPayment",
                                TransferDate = DateTime.SpecifyKind(dtpPaymentDate.Value, DateTimeKind.Utc),
                                ReferenceNumber = txtReferenceNumber.Text.Trim(),
                                Notes = $"دفعة من فواتيرك - العميل: {cmbCustomer.Text}{tripInfo}\n{txtNotes.Text.Trim()}",
                                TripId = selectedTripId,  // ✅ NEW: Save Trip ID
                                CreatedBy = _currentUserId,
                                CreatedDate = DateTime.UtcNow
                            };
                            _context.Set<BankTransfer>().Add(transfer);

                            // Create journal entry if requested
                            if (chkCreateJournalEntry.Checked)
                            {
                                CreateJournalEntry(bankId, customerId, amount, transfer.ReferenceNumber ?? "");
                            }

                            _context.SaveChanges();
                            transaction.Commit();

                            MessageBox.Show($"تم حفظ الدفعة بنجاح\nالرصيد الجديد للبنك: {bank?.Balance:N2} {bank?.Currency}",
                                "نجاح",
                                MessageBoxButtons.OK, MessageBoxIcon.Information,
                                MessageBoxDefaultButton.Button1,
                                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);

                            this.DialogResult = DialogResult.OK;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ الدفعة: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            }
        }

        private void CreateJournalEntry(int bankId, int customerId, decimal amount, string reference)
        {
            try
            {
                var bank = _context.Set<BankAccount>().Find(bankId);
                var customer = _context.Set<Customer>().Find(customerId);

                // Find or create bank account in chart of accounts
                string bankName = bank?.BankName ?? "";
                var bankAccount = _context.Set<Account>()
                    .FirstOrDefault(a => a.AccountCode.StartsWith("1020") && a.AccountName.Contains(bankName));

                if (bankAccount == null)
                {
                    // Create bank account if not exists
                    var maxCode = _context.Set<Account>()
                        .Where(a => a.AccountCode.StartsWith("1020"))
                        .OrderByDescending(a => a.AccountCode)
                        .Select(a => a.AccountCode)
                        .FirstOrDefault();

                    int nextNumber = 1;
                    if (!string.IsNullOrEmpty(maxCode))
                    {
                        if (int.TryParse(maxCode.Substring(4), out int num))
                            nextNumber = num + 1;
                    }

                    bankAccount = new Account
                    {
                        AccountCode = $"1020{nextNumber:D4}",
                        AccountName = $"بنك {bank?.BankName ?? "غير محدد"}",
                        AccountType = "Asset",
                        ParentAccountId = _context.Set<Account>().FirstOrDefault(a => a.AccountCode == "1020")?.AccountId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Set<Account>().Add(bankAccount);
                    _context.SaveChanges();
                }

                // Find customer receivables account
                var receivablesAccount = _context.Set<Account>()
                    .FirstOrDefault(a => a.AccountCode.StartsWith("1030"));

                if (receivablesAccount == null)
                {
                    throw new Exception("لم يتم العثور على حساب العملاء في دليل الحسابات");
                }

                // Create journal entry
                var journal = new JournalEntry
                {
                    EntryDate = DateTime.SpecifyKind(dtpPaymentDate.Value, DateTimeKind.Utc),
                    Description = $"دفعة من فواتيرك - {customer?.CustomerName ?? ""} - مرجع: {reference}",
                    EntryNumber = reference,
                    CreatedBy = _currentUserId,
                    CreatedAt = DateTime.UtcNow,
                    Lines = new List<JournalEntryLine>
                    {
                        new JournalEntryLine
                        {
                            AccountId = bankAccount.AccountId,
                            DebitAmount = amount,
                            CreditAmount = 0
                        },
                        new JournalEntryLine
                        {
                            AccountId = receivablesAccount.AccountId,
                            DebitAmount = 0,
                            CreditAmount = amount
                        }
                    }
                };

                _context.Set<JournalEntry>().Add(journal);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"تنبيه: تم حفظ الدفعة ولكن فشل إنشاء القيد المحاسبي: {ex.Message}",
                    "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool ValidateInputs()
        {
            if (cmbBankAccount.SelectedIndex == -1)
            {
                MessageBox.Show("يرجى اختيار الحساب البنكي", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbBankAccount.Focus();
                return false;
            }

            if (cmbCustomer.SelectedIndex == -1)
            {
                MessageBox.Show("يرجى اختيار العميل", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCustomer.Focus();
                return false;
            }

            if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("يرجى إدخال مبلغ صحيح أكبر من صفر", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAmount.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtReferenceNumber.Text))
            {
                MessageBox.Show("يرجى إدخال رقم المرجع من فواتيرك", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtReferenceNumber.Focus();
                return false;
            }

            return true;
        }
    }
}
