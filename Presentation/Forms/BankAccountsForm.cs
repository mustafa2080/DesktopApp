using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Infrastructure.Data;
using GraceWay.AccountingSystem.Presentation;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Presentation.Forms
{
    public partial class BankAccountsForm : Form
    {
        private readonly AppDbContext _context;
        private readonly int _currentUserId;
        private DataGridView dgvBanks;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnTransfer;
        private Button btnRefresh;
        private TextBox txtSearch;
        private Label lblTotalBalance;

        public BankAccountsForm(AppDbContext context, int currentUserId)
        {
            _context = context;
            _currentUserId = currentUserId;
            InitializeComponent();
            InitializeCustomComponents();
            LoadBankAccounts();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "إدارة الحسابات البنكية";
            this.Size = new Size(1400, 700);
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = ColorScheme.Background;
            this.Font = new Font("Cairo", 10F);

            // Main panel
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = ColorScheme.Background
            };

            // Header
            Label lblTitle = new Label
            {
                Text = "🏦 الحسابات البنكية",
                Font = new Font("Cairo", 18F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            mainPanel.Controls.Add(lblTitle);

            // Search panel
            Panel searchPanel = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(1140, 50),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            txtSearch = new TextBox
            {
                Location = new Point(10, 10),
                Size = new Size(300, 30),
                Font = new Font("Cairo", 10F),
                PlaceholderText = "🔍 بحث في الحسابات البنكية..."
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            searchPanel.Controls.Add(txtSearch);

            // Total balance label
            lblTotalBalance = new Label
            {
                Location = new Point(320, 15),
                Size = new Size(400, 25),
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                ForeColor = ColorScheme.Success,
                Text = "إجمالي الأرصدة: 0.00 ج.م"
            };
            searchPanel.Controls.Add(lblTotalBalance);

            mainPanel.Controls.Add(searchPanel);

            // Buttons panel
            Panel buttonsPanel = new Panel
            {
                Location = new Point(20, 130),
                Size = new Size(1340, 50),
                BackColor = Color.Transparent
            };

            btnAdd = CreateButton("➕ إضافة حساب", new Point(0, 0), ColorScheme.Success);
            btnAdd.Click += BtnAdd_Click;
            buttonsPanel.Controls.Add(btnAdd);

            btnEdit = CreateButton("✏️ تعديل", new Point(160, 0), ColorScheme.Primary);
            btnEdit.Click += BtnEdit_Click;
            buttonsPanel.Controls.Add(btnEdit);

            btnDelete = CreateButton("🗑️ حذف", new Point(320, 0), ColorScheme.Error);
            btnDelete.Click += BtnDelete_Click;
            buttonsPanel.Controls.Add(btnDelete);

            btnTransfer = CreateButton("💸 تحويل", new Point(480, 0), Color.FromArgb(156, 39, 176));
            btnTransfer.Click += BtnTransfer_Click;
            buttonsPanel.Controls.Add(btnTransfer);

            Button btnFawateerk = CreateButton("💳 دفعة فواتيرك", new Point(640, 0), Color.FromArgb(0, 150, 136));
            btnFawateerk.Click += BtnFawateerk_Click;
            buttonsPanel.Controls.Add(btnFawateerk);

            Button btnReport = CreateButton("📊 تقرير التحويلات", new Point(800, 0), Color.FromArgb(255, 152, 0));
            btnReport.Click += BtnReport_Click;
            buttonsPanel.Controls.Add(btnReport);

            Button btnFawateerkReport = CreateButton("📋 تقرير فواتيرك", new Point(960, 0), Color.FromArgb(103, 58, 183));
            btnFawateerkReport.Click += BtnFawateerkReport_Click;
            buttonsPanel.Controls.Add(btnFawateerkReport);

            btnRefresh = CreateButton("🔄 تحديث", new Point(1120, 0), ColorScheme.Primary);
            btnRefresh.Click += (s, e) => LoadBankAccounts();
            buttonsPanel.Controls.Add(btnRefresh);

            mainPanel.Controls.Add(buttonsPanel);

            // DataGridView
            dgvBanks = new DataGridView
            {
                Location = new Point(20, 190),
                Size = new Size(1140, 460),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Font = new Font("Cairo", 10F),
                ColumnHeadersHeight = 40,
                RowTemplate = { Height = 35 }
            };

            dgvBanks.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Primary;
            dgvBanks.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvBanks.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
            dgvBanks.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvBanks.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvBanks.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);

            mainPanel.Controls.Add(dgvBanks);

            this.Controls.Add(mainPanel);
        }

        private Button CreateButton(string text, Point location, Color bgColor)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                Size = new Size(150, 45),
                BackColor = bgColor,
                ForeColor = Color.White,
                Font = new Font("Cairo", 9.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            
            button.FlatAppearance.BorderSize = 0;
            
            return button;
        }

        private void LoadBankAccounts()
        {
            try
            {
                var banks = _context.Set<Domain.Entities.BankAccount>()
                    .AsNoTracking()
                    .OrderBy(b => b.BankName)
                    .Select(b => new
                    {
                        b.Id,
                        BankName = b.BankName,
                        AccountNumber = b.AccountNumber,
                        AccountType = b.AccountType,
                        Balance = b.Balance,
                        Currency = b.Currency,
                        Branch = b.Branch ?? "غير محدد",
                        Status = b.IsActive ? "نشط" : "غير نشط",
                        Notes = b.Notes ?? "",
                        CreatedDate = b.CreatedDate.ToString("dd/MM/yyyy")
                    })
                    .ToList();

                dgvBanks.DataSource = banks;

                if (dgvBanks.Columns.Count > 0)
                {
                    dgvBanks.Columns["Id"]!.Visible = false;
                    dgvBanks.Columns["BankName"]!.HeaderText = "اسم البنك";
                    dgvBanks.Columns["AccountNumber"]!.HeaderText = "رقم الحساب";
                    dgvBanks.Columns["AccountType"]!.HeaderText = "نوع الحساب";
                    dgvBanks.Columns["Balance"]!.HeaderText = "الرصيد";
                    dgvBanks.Columns["Balance"]!.DefaultCellStyle.Format = "N2";
                    dgvBanks.Columns["Currency"]!.HeaderText = "العملة";
                    dgvBanks.Columns["Branch"]!.HeaderText = "الفرع";
                    dgvBanks.Columns["Status"]!.HeaderText = "الحالة";
                    dgvBanks.Columns["Notes"]!.HeaderText = "ملاحظات";
                    dgvBanks.Columns["CreatedDate"]!.HeaderText = "تاريخ الإنشاء";
                }

                decimal totalBalance = banks.Sum(b => b.Balance);
                lblTotalBalance.Text = $"إجمالي الأرصدة: {totalBalance:N2} ج.م";

                UpdateButtonsState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الحسابات البنكية: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            }
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            try
            {
                string search = txtSearch.Text.Trim().ToLower();
                
                var banks = _context.Set<Domain.Entities.BankAccount>()
                    .AsNoTracking()
                    .Where(b => b.BankName.ToLower().Contains(search) ||
                               b.AccountNumber.ToLower().Contains(search) ||
                               (b.Branch != null && b.Branch.ToLower().Contains(search)))
                    .OrderBy(b => b.BankName)
                    .Select(b => new
                    {
                        b.Id,
                        BankName = b.BankName,
                        AccountNumber = b.AccountNumber,
                        AccountType = b.AccountType,
                        Balance = b.Balance,
                        Currency = b.Currency,
                        Branch = b.Branch ?? "غير محدد",
                        Status = b.IsActive ? "نشط" : "غير نشط",
                        Notes = b.Notes ?? "",
                        CreatedDate = b.CreatedDate.ToString("dd/MM/yyyy")
                    })
                    .ToList();

                dgvBanks.DataSource = banks;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في البحث: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var addForm = new AddEditBankAccountForm(_context, _currentUserId);
            addForm.ShowDialog();
            LoadBankAccounts();
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvBanks.SelectedRows.Count == 0)
            {
                MessageBox.Show("يرجى اختيار حساب بنكي للتعديل", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                return;
            }

            int bankId = Convert.ToInt32(dgvBanks.SelectedRows[0].Cells["Id"].Value);
            var editForm = new AddEditBankAccountForm(_context, _currentUserId, bankId);
            editForm.ShowDialog();
            LoadBankAccounts();
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvBanks.SelectedRows.Count == 0)
            {
                MessageBox.Show("يرجى اختيار حساب بنكي للحذف", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                return;
            }

            var result = MessageBox.Show("هل أنت متأكد من حذف هذا الحساب البنكي؟",
                "تأكيد الحذف",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);

            if (result == DialogResult.Yes)
            {
                try
                {
                    int bankId = Convert.ToInt32(dgvBanks.SelectedRows[0].Cells["Id"].Value);
                    var bank = _context.Set<Domain.Entities.BankAccount>().Find(bankId);
                    
                    if (bank != null)
                    {
                        _context.Set<Domain.Entities.BankAccount>().Remove(bank);
                        _context.SaveChanges();
                        
                        MessageBox.Show("تم حذف الحساب البنكي بنجاح", "نجاح",
                            MessageBoxButtons.OK, MessageBoxIcon.Information,
                            MessageBoxDefaultButton.Button1,
                            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                        
                        LoadBankAccounts();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في حذف الحساب البنكي: {ex.Message}", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                }
            }
        }

        private void BtnTransfer_Click(object? sender, EventArgs e)
        {
            var transferForm = new BankTransferForm(_context, _currentUserId);
            transferForm.ShowDialog();
            LoadBankAccounts();
        }

        private void BtnFawateerk_Click(object? sender, EventArgs e)
        {
            var fawateerkForm = new FawateerkPaymentForm(_context, _currentUserId);
            fawateerkForm.ShowDialog();
            LoadBankAccounts();
        }

        private void BtnReport_Click(object? sender, EventArgs e)
        {
            var reportForm = new BankTransfersReportForm(_context);
            reportForm.ShowDialog();
        }

        private void BtnFawateerkReport_Click(object? sender, EventArgs e)
        {
            var reportForm = new FawateerkPaymentsReportForm(_context, _currentUserId);
            reportForm.ShowDialog();
        }

        private void UpdateButtonsState()
        {
            bool hasSelection = dgvBanks.SelectedRows.Count > 0;
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
        }
    }
}
