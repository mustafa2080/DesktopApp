using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Presentation.Forms
{
    public partial class BankTransferForm : Form
    {
        private readonly AppDbContext _context;
        private readonly int _currentUserId;

        private ComboBox cmbTransferType;
        private ComboBox cmbSourceBank;
        private ComboBox cmbSourceCashBox;
        private ComboBox cmbDestinationBank;
        private ComboBox cmbDestinationCashBox;
        private TextBox txtAmount;
        private TextBox txtReferenceNumber;
        private DateTimePicker dtpTransferDate;
        private TextBox txtNotes;
        private Button btnTransfer;
        private Button btnCancel;
        private Label lblSourceBalance;
        private Label lblDestinationBalance;
        private Panel sourcePanel;
        private Panel destinationPanel;

        public BankTransferForm(AppDbContext context, int currentUserId)
        {
            _context = context;
            _currentUserId = currentUserId;
            InitializeComponent();
            InitializeCustomComponents();
            LoadBanks();
            LoadCashBoxes();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "تحويل بين البنوك والخزن";
            this.Size = new Size(700, 750);
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
                AutoScroll = true
            };

            int yPos = 10;

            // Title
            Label lblTitle = new Label
            {
                Text = "💸 تحويل أموال",
                Location = new Point(20, yPos),
                Size = new Size(640, 35),
                Font = new Font("Cairo", 16F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblTitle);
            yPos += 45;

            // Transfer Type
            Label lblTransferType = CreateLabel("نوع التحويل:", yPos);
            mainPanel.Controls.Add(lblTransferType);

            cmbTransferType = new ComboBox
            {
                Location = new Point(20, yPos + 30),
                Size = new Size(640, 30),
                Font = new Font("Cairo", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTransferType.Items.AddRange(new[] { 
                "من بنك إلى بنك", 
                "من بنك إلى خزنة", 
                "من خزنة إلى بنك" 
            });
            cmbTransferType.SelectedIndex = 0;
            cmbTransferType.SelectedIndexChanged += CmbTransferType_SelectedIndexChanged;
            mainPanel.Controls.Add(cmbTransferType);
            yPos += 70;

            // Source Panel
            sourcePanel = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(640, 130),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            Label lblSource = new Label
            {
                Text = "📤 من:",
                Location = new Point(10, 10),
                Size = new Size(620, 25),
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                ForeColor = ColorScheme.Error
            };
            sourcePanel.Controls.Add(lblSource);

            Label lblSourceBank = new Label
            {
                Text = "البنك:",
                Location = new Point(10, 45),
                Size = new Size(620, 20),
                Font = new Font("Cairo", 9F)
            };
            sourcePanel.Controls.Add(lblSourceBank);

            cmbSourceBank = new ComboBox
            {
                Location = new Point(10, 70),
                Size = new Size(620, 30),
                Font = new Font("Cairo", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSourceBank.SelectedIndexChanged += CmbSourceBank_SelectedIndexChanged;
            sourcePanel.Controls.Add(cmbSourceBank);

            cmbSourceCashBox = new ComboBox
            {
                Location = new Point(10, 70),
                Size = new Size(620, 30),
                Font = new Font("Cairo", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Visible = false
            };
            cmbSourceCashBox.SelectedIndexChanged += CmbSourceCashBox_SelectedIndexChanged;
            sourcePanel.Controls.Add(cmbSourceCashBox);

            lblSourceBalance = new Label
            {
                Text = "الرصيد الحالي: 0.00 ج.م",
                Location = new Point(10, 105),
                Size = new Size(620, 20),
                Font = new Font("Cairo", 9F, FontStyle.Bold),
                ForeColor = ColorScheme.Success
            };
            sourcePanel.Controls.Add(lblSourceBalance);

            mainPanel.Controls.Add(sourcePanel);
            yPos += 140;

            // Destination Panel
            destinationPanel = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(640, 130),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            Label lblDestination = new Label
            {
                Text = "📥 إلى:",
                Location = new Point(10, 10),
                Size = new Size(620, 25),
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                ForeColor = ColorScheme.Success
            };
            destinationPanel.Controls.Add(lblDestination);

            Label lblDestBank = new Label
            {
                Text = "البنك:",
                Location = new Point(10, 45),
                Size = new Size(620, 20),
                Font = new Font("Cairo", 9F)
            };
            destinationPanel.Controls.Add(lblDestBank);

            cmbDestinationBank = new ComboBox
            {
                Location = new Point(10, 70),
                Size = new Size(620, 30),
                Font = new Font("Cairo", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbDestinationBank.SelectedIndexChanged += CmbDestinationBank_SelectedIndexChanged;
            destinationPanel.Controls.Add(cmbDestinationBank);

            cmbDestinationCashBox = new ComboBox
            {
                Location = new Point(10, 70),
                Size = new Size(620, 30),
                Font = new Font("Cairo", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Visible = false
            };
            cmbDestinationCashBox.SelectedIndexChanged += CmbDestinationCashBox_SelectedIndexChanged;
            destinationPanel.Controls.Add(cmbDestinationCashBox);

            lblDestinationBalance = new Label
            {
                Text = "الرصيد الحالي: 0.00 ج.م",
                Location = new Point(10, 105),
                Size = new Size(620, 20),
                Font = new Font("Cairo", 9F, FontStyle.Bold),
                ForeColor = ColorScheme.Success
            };
            destinationPanel.Controls.Add(lblDestinationBalance);

            mainPanel.Controls.Add(destinationPanel);
            yPos += 140;

            // Amount
            Label lblAmount = CreateLabel("المبلغ:", yPos);
            mainPanel.Controls.Add(lblAmount);

            txtAmount = CreateTextBox(yPos + 30);
            txtAmount.TextChanged += TxtAmount_TextChanged;
            mainPanel.Controls.Add(txtAmount);
            yPos += 70;

            // Transfer Date
            Label lblDate = CreateLabel("تاريخ التحويل:", yPos);
            mainPanel.Controls.Add(lblDate);

            dtpTransferDate = new DateTimePicker
            {
                Location = new Point(20, yPos + 30),
                Size = new Size(640, 30),
                Font = new Font("Cairo", 10F),
                Format = DateTimePickerFormat.Short
            };
            mainPanel.Controls.Add(dtpTransferDate);
            yPos += 70;

            // Reference Number
            Label lblRef = CreateLabel("رقم المرجع (اختياري):", yPos);
            mainPanel.Controls.Add(lblRef);

            txtReferenceNumber = CreateTextBox(yPos + 30);
            mainPanel.Controls.Add(txtReferenceNumber);
            yPos += 70;

            // Notes
            Label lblNotes = CreateLabel("ملاحظات:", yPos);
            mainPanel.Controls.Add(lblNotes);

            txtNotes = new TextBox
            {
                Location = new Point(20, yPos + 30),
                Size = new Size(640, 60),
                Font = new Font("Cairo", 10F),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            mainPanel.Controls.Add(txtNotes);
            yPos += 100;

            // Buttons
            Panel buttonsPanel = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(640, 50),
                BackColor = Color.Transparent
            };

            btnTransfer = new Button
            {
                Text = "💸 تنفيذ التحويل",
                Location = new Point(0, 0),
                Size = new Size(200, 45),
                BackColor = ColorScheme.Success,
                ForeColor = Color.White,
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnTransfer.Click += BtnTransfer_Click;
            buttonsPanel.Controls.Add(btnTransfer);

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
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            buttonsPanel.Controls.Add(btnCancel);

            mainPanel.Controls.Add(buttonsPanel);

            this.Controls.Add(mainPanel);
        }

        private Label CreateLabel(string text, int yPos)
        {
            return new Label
            {
                Text = text,
                Location = new Point(20, yPos),
                Size = new Size(640, 25),
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                ForeColor = ColorScheme.TextPrimary
            };
        }

        private TextBox CreateTextBox(int yPos)
        {
            return new TextBox
            {
                Location = new Point(20, yPos),
                Size = new Size(640, 30),
                Font = new Font("Cairo", 10F)
            };
        }

        private void LoadBanks()
        {
            try
            {
                var banks = _context.Set<Domain.Entities.BankAccount>()
                    .Where(b => b.IsActive)
                    .OrderBy(b => b.BankName)
                    .Select(b => new
                    {
                        b.Id,
                        Display = $"{b.BankName} - {b.AccountNumber} ({b.Currency})"
                    })
                    .ToList();

                cmbSourceBank.DataSource = banks.Select(b => new { b.Id, b.Display }).ToList();
                cmbSourceBank.DisplayMember = "Display";
                cmbSourceBank.ValueMember = "Id";

                cmbDestinationBank.DataSource = banks.Select(b => new { b.Id, b.Display }).ToList();
                cmbDestinationBank.DisplayMember = "Display";
                cmbDestinationBank.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل البنوك: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCashBoxes()
        {
            try
            {
                var cashBoxes = _context.Set<Domain.Entities.CashBox>()
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .Select(c => new
                    {
                        c.Id,
                        Display = $"{c.Name} ({c.Currency})"
                    })
                    .ToList();

                cmbSourceCashBox.DataSource = cashBoxes.Select(c => new { c.Id, c.Display }).ToList();
                cmbSourceCashBox.DisplayMember = "Display";
                cmbSourceCashBox.ValueMember = "Id";

                cmbDestinationCashBox.DataSource = cashBoxes.Select(c => new { c.Id, c.Display }).ToList();
                cmbDestinationCashBox.DisplayMember = "Display";
                cmbDestinationCashBox.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الخزن: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbTransferType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var transferType = cmbTransferType.SelectedIndex;

            // Reset visibility
            cmbSourceBank.Visible = false;
            cmbSourceCashBox.Visible = false;
            cmbDestinationBank.Visible = false;
            cmbDestinationCashBox.Visible = false;

            switch (transferType)
            {
                case 0: // من بنك إلى بنك
                    cmbSourceBank.Visible = true;
                    cmbDestinationBank.Visible = true;
                    sourcePanel.Controls[1].Text = "البنك المصدر:";
                    destinationPanel.Controls[1].Text = "البنك المستقبل:";
                    break;
                case 1: // من بنك إلى خزنة
                    cmbSourceBank.Visible = true;
                    cmbDestinationCashBox.Visible = true;
                    sourcePanel.Controls[1].Text = "البنك:";
                    destinationPanel.Controls[1].Text = "الخزنة:";
                    break;
                case 2: // من خزنة إلى بنك
                    cmbSourceCashBox.Visible = true;
                    cmbDestinationBank.Visible = true;
                    sourcePanel.Controls[1].Text = "الخزنة:";
                    destinationPanel.Controls[1].Text = "البنك:";
                    break;
            }

            UpdateBalances();
        }

        private void CmbSourceBank_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateBalances();
        }

        private void CmbSourceCashBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateBalances();
        }

        private void CmbDestinationBank_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateBalances();
        }

        private void CmbDestinationCashBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateBalances();
        }

        private void TxtAmount_TextChanged(object? sender, EventArgs e)
        {
            // Validate amount is numeric
            if (!string.IsNullOrWhiteSpace(txtAmount.Text))
            {
                if (!decimal.TryParse(txtAmount.Text, out _))
                {
                    txtAmount.BackColor = Color.FromArgb(255, 200, 200);
                }
                else
                {
                    txtAmount.BackColor = Color.White;
                }
            }
        }

        private void UpdateBalances()
        {
            try
            {
                var transferType = cmbTransferType.SelectedIndex;

                // Update source balance
                if (transferType == 0 || transferType == 1) // Bank as source
                {
                    if (cmbSourceBank.SelectedValue != null)
                    {
                        int bankId = Convert.ToInt32(cmbSourceBank.SelectedValue);
                        var bank = _context.Set<Domain.Entities.BankAccount>()
                            .FirstOrDefault(b => b.Id == bankId);
                        if (bank != null)
                        {
                            lblSourceBalance.Text = $"الرصيد الحالي: {bank.Balance:N2} {bank.Currency}";
                        }
                    }
                }
                else if (transferType == 2) // CashBox as source
                {
                    if (cmbSourceCashBox.SelectedValue != null)
                    {
                        int cashBoxId = Convert.ToInt32(cmbSourceCashBox.SelectedValue);
                        var cashBox = _context.Set<Domain.Entities.CashBox>()
                            .FirstOrDefault(c => c.Id == cashBoxId);
                        if (cashBox != null)
                        {
                            lblSourceBalance.Text = $"الرصيد الحالي: {cashBox.CurrentBalance:N2} {cashBox.Currency}";
                        }
                    }
                }

                // Update destination balance
                if (transferType == 0 || transferType == 2) // Bank as destination
                {
                    if (cmbDestinationBank.SelectedValue != null)
                    {
                        int bankId = Convert.ToInt32(cmbDestinationBank.SelectedValue);
                        var bank = _context.Set<Domain.Entities.BankAccount>()
                            .FirstOrDefault(b => b.Id == bankId);
                        if (bank != null)
                        {
                            lblDestinationBalance.Text = $"الرصيد الحالي: {bank.Balance:N2} {bank.Currency}";
                        }
                    }
                }
                else if (transferType == 1) // CashBox as destination
                {
                    if (cmbDestinationCashBox.SelectedValue != null)
                    {
                        int cashBoxId = Convert.ToInt32(cmbDestinationCashBox.SelectedValue);
                        var cashBox = _context.Set<Domain.Entities.CashBox>()
                            .FirstOrDefault(c => c.Id == cashBoxId);
                        if (cashBox != null)
                        {
                            lblDestinationBalance.Text = $"الرصيد الحالي: {cashBox.CurrentBalance:N2} {cashBox.Currency}";
                        }
                    }
                }
            }
            catch { }
        }

        private void BtnTransfer_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            var result = MessageBox.Show("هل أنت متأكد من تنفيذ عملية التحويل؟",
                "تأكيد التحويل",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);

            if (result == DialogResult.No)
                return;

            try
            {
                decimal amount = decimal.Parse(txtAmount.Text);
                var transferType = cmbTransferType.SelectedIndex;

                var transfer = new Domain.Entities.BankTransfer
                {
                    Amount = amount,
                    TransferDate = dtpTransferDate.Value,
                    ReferenceNumber = txtReferenceNumber.Text.Trim(),
                    Notes = txtNotes.Text.Trim(),
                    CreatedBy = _currentUserId,
                    CreatedDate = DateTime.Now
                };

                switch (transferType)
                {
                    case 0: // من بنك إلى بنك
                        transfer.TransferType = "BankToBank";
                        transfer.SourceBankAccountId = Convert.ToInt32(cmbSourceBank.SelectedValue);
                        transfer.DestinationBankAccountId = Convert.ToInt32(cmbDestinationBank.SelectedValue);
                        
                        var sourceBank = _context.Set<Domain.Entities.BankAccount>().Find(transfer.SourceBankAccountId);
                        var destBank = _context.Set<Domain.Entities.BankAccount>().Find(transfer.DestinationBankAccountId);
                        
                        if (sourceBank != null && sourceBank.Balance < amount)
                        {
                            MessageBox.Show("الرصيد غير كافي في البنك المصدر", "خطأ",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        
                        if (sourceBank != null)
                            sourceBank.Balance -= amount;
                        if (destBank != null)
                            destBank.Balance += amount;
                        break;

                    case 1: // من بنك إلى خزنة
                        transfer.TransferType = "BankToCash";
                        transfer.SourceBankAccountId = Convert.ToInt32(cmbSourceBank.SelectedValue);
                        transfer.DestinationCashBoxId = Convert.ToInt32(cmbDestinationCashBox.SelectedValue);
                        
                        var srcBank = _context.Set<Domain.Entities.BankAccount>().Find(transfer.SourceBankAccountId);
                        var destCash = _context.Set<Domain.Entities.CashBox>().Find(transfer.DestinationCashBoxId);
                        
                        if (srcBank != null && srcBank.Balance < amount)
                        {
                            MessageBox.Show("الرصيد غير كافي في البنك", "خطأ",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        
                        if (srcBank != null)
                            srcBank.Balance -= amount;
                        if (destCash != null)
                            destCash.CurrentBalance += amount;
                        break;

                    case 2: // من خزنة إلى بنك
                        transfer.TransferType = "CashToBank";
                        transfer.SourceCashBoxId = Convert.ToInt32(cmbSourceCashBox.SelectedValue);
                        transfer.DestinationBankAccountId = Convert.ToInt32(cmbDestinationBank.SelectedValue);
                        
                        var srcCash = _context.Set<Domain.Entities.CashBox>().Find(transfer.SourceCashBoxId);
                        var dstBank = _context.Set<Domain.Entities.BankAccount>().Find(transfer.DestinationBankAccountId);
                        
                        if (srcCash != null && srcCash.CurrentBalance < amount)
                        {
                            MessageBox.Show("الرصيد غير كافي في الخزنة", "خطأ",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        
                        if (srcCash != null)
                            srcCash.CurrentBalance -= amount;
                        if (dstBank != null)
                            dstBank.Balance += amount;
                        break;
                }

                _context.Set<Domain.Entities.BankTransfer>().Add(transfer);
                _context.SaveChanges();

                MessageBox.Show("تم تنفيذ التحويل بنجاح", "نجاح",
                    MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تنفيذ التحويل: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            }
        }

        private bool ValidateInputs()
        {
            if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("يرجى إدخال مبلغ صحيح", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                txtAmount.Focus();
                return false;
            }

            var transferType = cmbTransferType.SelectedIndex;

            if ((transferType == 0 || transferType == 1) && cmbSourceBank.SelectedValue == null)
            {
                MessageBox.Show("يرجى اختيار البنك المصدر", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (transferType == 2 && cmbSourceCashBox.SelectedValue == null)
            {
                MessageBox.Show("يرجى اختيار الخزنة المصدر", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if ((transferType == 0 || transferType == 2) && cmbDestinationBank.SelectedValue == null)
            {
                MessageBox.Show("يرجى اختيار البنك المستقبل", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (transferType == 1 && cmbDestinationCashBox.SelectedValue == null)
            {
                MessageBox.Show("يرجى اختيار الخزنة المستقبلة", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Check if source and destination are the same
            if (transferType == 0)
            {
                if (Convert.ToInt32(cmbSourceBank.SelectedValue) == Convert.ToInt32(cmbDestinationBank.SelectedValue))
                {
                    MessageBox.Show("لا يمكن التحويل من نفس البنك إلى نفسه", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            return true;
        }
    }
}
