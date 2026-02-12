using System;
using System.Drawing;
using System.Windows.Forms;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Presentation.Forms
{
    public partial class AddEditBankAccountForm : Form
    {
        private readonly AppDbContext _context;
        private readonly int _currentUserId;
        private readonly int? _bankId;
        
        private TextBox txtBankName;
        private TextBox txtAccountNumber;
        private ComboBox cmbAccountType;
        private TextBox txtBalance;
        private ComboBox cmbCurrency;
        private TextBox txtBranch;
        private CheckBox chkIsActive;
        private TextBox txtNotes;
        private Button btnSave;
        private Button btnCancel;

        public AddEditBankAccountForm(AppDbContext context, int currentUserId, int? bankId = null)
        {
            _context = context;
            _currentUserId = currentUserId;
            _bankId = bankId;
            InitializeComponent();
            InitializeCustomComponents();
            
            if (_bankId.HasValue)
                LoadBankData();
        }

        private void InitializeCustomComponents()
        {
            this.Text = _bankId.HasValue ? "تعديل حساب بنكي" : "إضافة حساب بنكي جديد";
            this.Size = new Size(600, 650);
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

            // Bank Name
            Label lblBankName = CreateLabel("اسم البنك:", yPos);
            mainPanel.Controls.Add(lblBankName);
            
            txtBankName = CreateTextBox(yPos + 30);
            mainPanel.Controls.Add(txtBankName);
            yPos += 70;

            // Account Number
            Label lblAccountNumber = CreateLabel("رقم الحساب:", yPos);
            mainPanel.Controls.Add(lblAccountNumber);
            
            txtAccountNumber = CreateTextBox(yPos + 30);
            mainPanel.Controls.Add(txtAccountNumber);
            yPos += 70;

            // Account Type
            Label lblAccountType = CreateLabel("نوع الحساب:", yPos);
            mainPanel.Controls.Add(lblAccountType);
            
            cmbAccountType = new ComboBox
            {
                Location = new Point(20, yPos + 30),
                Size = new Size(540, 30),
                Font = new Font("Cairo", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbAccountType.Items.AddRange(new[] { "حساب جاري", "حساب توفير", "حساب وديعة", "أخرى" });
            cmbAccountType.SelectedIndex = 0;
            mainPanel.Controls.Add(cmbAccountType);
            yPos += 70;

            // Balance
            Label lblBalance = CreateLabel("الرصيد الافتتاحي:", yPos);
            mainPanel.Controls.Add(lblBalance);
            
            txtBalance = CreateTextBox(yPos + 30);
            txtBalance.Text = "0";
            mainPanel.Controls.Add(txtBalance);
            yPos += 70;

            // Currency
            Label lblCurrency = CreateLabel("العملة:", yPos);
            mainPanel.Controls.Add(lblCurrency);
            
            cmbCurrency = new ComboBox
            {
                Location = new Point(20, yPos + 30),
                Size = new Size(540, 30),
                Font = new Font("Cairo", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCurrency.Items.AddRange(new[] { "EGP", "USD", "EUR", "SAR", "AED" });
            cmbCurrency.SelectedIndex = 0;
            mainPanel.Controls.Add(cmbCurrency);
            yPos += 70;

            // Branch
            Label lblBranch = CreateLabel("الفرع:", yPos);
            mainPanel.Controls.Add(lblBranch);
            
            txtBranch = CreateTextBox(yPos + 30);
            mainPanel.Controls.Add(txtBranch);
            yPos += 70;

            // IsActive
            chkIsActive = new CheckBox
            {
                Text = "الحساب نشط",
                Location = new Point(20, yPos),
                Size = new Size(200, 30),
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                Checked = true
            };
            mainPanel.Controls.Add(chkIsActive);
            yPos += 40;

            // Notes
            Label lblNotes = CreateLabel("ملاحظات:", yPos);
            mainPanel.Controls.Add(lblNotes);
            
            txtNotes = new TextBox
            {
                Location = new Point(20, yPos + 30),
                Size = new Size(540, 80),
                Font = new Font("Cairo", 10F),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            mainPanel.Controls.Add(txtNotes);
            yPos += 120;

            // Buttons
            Panel buttonsPanel = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(540, 50),
                BackColor = Color.Transparent
            };

            btnSave = new Button
            {
                Text = "💾 حفظ",
                Location = new Point(0, 0),
                Size = new Size(150, 45),
                BackColor = ColorScheme.Success,
                ForeColor = Color.White,
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnSave_Click;
            buttonsPanel.Controls.Add(btnSave);

            btnCancel = new Button
            {
                Text = "❌ إلغاء",
                Location = new Point(160, 0),
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
                Size = new Size(540, 25),
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                ForeColor = ColorScheme.TextPrimary
            };
        }

        private TextBox CreateTextBox(int yPos)
        {
            return new TextBox
            {
                Location = new Point(20, yPos),
                Size = new Size(540, 30),
                Font = new Font("Cairo", 10F)
            };
        }

        private void LoadBankData()
        {
            try
            {
                var bank = _context.Set<Domain.Entities.BankAccount>()
                    .AsNoTracking()
                    .FirstOrDefault(b => b.Id == _bankId);

                if (bank != null)
                {
                    txtBankName.Text = bank.BankName;
                    txtAccountNumber.Text = bank.AccountNumber;
                    cmbAccountType.Text = bank.AccountType;
                    txtBalance.Text = bank.Balance.ToString();
                    cmbCurrency.Text = bank.Currency;
                    txtBranch.Text = bank.Branch ?? "";
                    chkIsActive.Checked = bank.IsActive;
                    txtNotes.Text = bank.Notes ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل بيانات الحساب البنكي: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            try
            {
                Domain.Entities.BankAccount bank;

                if (_bankId.HasValue)
                {
                    // استخدام FirstOrDefault مع Tracking للتعديل
                    bank = _context.Set<Domain.Entities.BankAccount>()
                        .FirstOrDefault(b => b.Id == _bankId.Value);
                    
                    if (bank == null)
                    {
                        MessageBox.Show("لم يتم العثور على الحساب البنكي", "خطأ",
                            MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1,
                            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                        return;
                    }
                    
                    // تحديث البيانات
                    bank.BankName = txtBankName.Text.Trim();
                    bank.AccountNumber = txtAccountNumber.Text.Trim();
                    bank.AccountType = cmbAccountType.Text;
                    bank.Balance = decimal.Parse(txtBalance.Text);
                    bank.Currency = cmbCurrency.Text;
                    bank.Branch = string.IsNullOrWhiteSpace(txtBranch.Text) ? null : txtBranch.Text.Trim();
                    bank.IsActive = chkIsActive.Checked;
                    bank.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();
                    bank.ModifiedDate = DateTime.UtcNow;
                    bank.ModifiedBy = _currentUserId;
                }
                else
                {
                    // إنشاء حساب جديد
                    bank = new Domain.Entities.BankAccount
                    {
                        BankName = txtBankName.Text.Trim(),
                        AccountNumber = txtAccountNumber.Text.Trim(),
                        AccountType = cmbAccountType.Text,
                        Balance = decimal.Parse(txtBalance.Text),
                        Currency = cmbCurrency.Text,
                        Branch = string.IsNullOrWhiteSpace(txtBranch.Text) ? null : txtBranch.Text.Trim(),
                        IsActive = chkIsActive.Checked,
                        Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim(),
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = _currentUserId
                    };
                    _context.Set<Domain.Entities.BankAccount>().Add(bank);
                }

                _context.SaveChanges();

                MessageBox.Show("تم حفظ الحساب البنكي بنجاح", "نجاح",
                    MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);

                this.DialogResult = DialogResult.OK;
            }
            catch (DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                MessageBox.Show($"خطأ في قاعدة البيانات:\n{innerMessage}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"خطأ في حفظ الحساب البنكي:\n{innerMessage}\n\nالتفاصيل: {ex.StackTrace}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtBankName.Text))
            {
                MessageBox.Show("يرجى إدخال اسم البنك", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                txtBankName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtAccountNumber.Text))
            {
                MessageBox.Show("يرجى إدخال رقم الحساب", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                txtAccountNumber.Focus();
                return false;
            }

            if (!decimal.TryParse(txtBalance.Text, out decimal balance) || balance < 0)
            {
                MessageBox.Show("يرجى إدخال رصيد افتتاحي صحيح", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                txtBalance.Focus();
                return false;
            }

            return true;
        }
    }
}
