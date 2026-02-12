using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class AddJournalEntryForm : Form
{
    private readonly AppDbContext _dbContext;
    private readonly int _currentUserId;
    private JournalEntry? _existingEntry;  // ✅ للتعديل
    private bool _isEditMode = false;  // ✅ وضع التعديل
    private DateTimePicker dtpEntryDate;
    private TextBox txtDescription;
    private DataGridView dgvLines;
    private Button btnAddLine;
    private Button btnRemoveLine;
    private Button btnSave;
    private Button btnCancel;
    private Label lblDebitTotal;
    private Label lblCreditTotal;
    private Label lblDifference;
    private List<JournalLineItem> _lines = new List<JournalLineItem>();

    // Constructor للإضافة
    public AddJournalEntryForm(AppDbContext dbContext, int currentUserId)
    {
        _dbContext = dbContext;
        _currentUserId = currentUserId;
        _isEditMode = false;
        InitializeComponent();
        InitializeCustomComponents();
    }

    // ✅ Constructor للتعديل
    public AddJournalEntryForm(AppDbContext dbContext, int currentUserId, JournalEntry existingEntry)
    {
        _dbContext = dbContext;
        _currentUserId = currentUserId;
        _existingEntry = existingEntry;
        _isEditMode = true;
        InitializeComponent();
        InitializeCustomComponents();
        LoadExistingEntry();
    }

    private void InitializeCustomComponents()
    {
        this.Text = _isEditMode ? "✏️ تعديل قيد يومي" : "➕ إضافة قيد يومي يدوي";
        this.Size = new Size(1100, 700);
        this.StartPosition = FormStartPosition.CenterParent;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.White;

        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(30)
        };

        // Title
        Label lblTitle = new Label
        {
            Text = _isEditMode ? "✏️ تعديل قيد يومي" : "➕ إضافة قيد يومي يدوي",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(30, 20)
        };

        // Entry Date
        Label lblDate = new Label
        {
            Text = "تاريخ القيد:",
            Font = new Font("Cairo", 11F),
            Location = new Point(930, 70),
            AutoSize = true
        };
        dtpEntryDate = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Location = new Point(750, 67),
            Size = new Size(170, 30),
            Font = new Font("Cairo", 10F),
            Value = DateTime.Today
        };

        // Description
        Label lblDesc = new Label
        {
            Text = "الوصف:",
            Font = new Font("Cairo", 11F),
            Location = new Point(660, 70),
            AutoSize = true
        };
        txtDescription = new TextBox
        {
            Location = new Point(300, 67),
            Size = new Size(350, 30),
            Font = new Font("Cairo", 10F),
            PlaceholderText = "وصف القيد..."
        };

        // Lines Section
        Label lblLines = new Label
        {
            Text = "📝 بنود القيد:",
            Font = new Font("Cairo", 13F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            Location = new Point(30, 120),
            AutoSize = true
        };

        // Add/Remove Buttons
        btnAddLine = new Button
        {
            Text = "➕ إضافة بند",
            Location = new Point(880, 115),
            Size = new Size(140, 35),
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnAddLine.FlatAppearance.BorderSize = 0;
        btnAddLine.Click += BtnAddLine_Click;

        btnRemoveLine = new Button
        {
            Text = "➖ حذف البند",
            Location = new Point(730, 115),
            Size = new Size(140, 35),
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            BackColor = ColorScheme.Error,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnRemoveLine.FlatAppearance.BorderSize = 0;
        btnRemoveLine.Click += BtnRemoveLine_Click;

        // DataGridView for Lines
        dgvLines = new DataGridView
        {
            Location = new Point(30, 160),
            Size = new Size(1020, 350),
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = new Font("Cairo", 10F),
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 40,
            RowTemplate = { Height = 35 }
        };

        dgvLines.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Primary;
        dgvLines.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvLines.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        dgvLines.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvLines.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);
        dgvLines.SelectionChanged += DgvLines_SelectionChanged;

        // Setup Columns
        var accountColumn = new DataGridViewComboBoxColumn
        {
            Name = "Account",
            HeaderText = "الحساب",
            Width = 300,
            DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
        };
        
        // Load accounts
        var accounts = _dbContext.Set<Account>()
            .Where(a => a.IsActive)
            .OrderBy(a => a.AccountCode)
            .ToList();

        foreach (var account in accounts)
        {
            accountColumn.Items.Add($"{account.AccountCode} - {account.AccountName}");
        }

        dgvLines.Columns.Add(accountColumn);
        
        dgvLines.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Description",
            HeaderText = "البيان",
            Width = 250
        });

        dgvLines.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Debit",
            HeaderText = "مدين",
            Width = 150,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleCenter }
        });

        dgvLines.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Credit",
            HeaderText = "دائن",
            Width = 150,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleCenter }
        });

        dgvLines.CellValueChanged += DgvLines_CellValueChanged;
        dgvLines.CurrentCellDirtyStateChanged += DgvLines_CurrentCellDirtyStateChanged;
        dgvLines.EditingControlShowing += DgvLines_EditingControlShowing;
        dgvLines.RowsAdded += (s, e) => CalculateTotals();
        dgvLines.RowsRemoved += (s, e) => CalculateTotals();

        // Totals Panel
        Panel totalsPanel = new Panel
        {
            Location = new Point(30, 520),
            Size = new Size(1020, 60),
            BackColor = ColorScheme.Background,
            BorderStyle = BorderStyle.FixedSingle
        };

        lblDebitTotal = new Label
        {
            Text = "إجمالي المدين: 0.00",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = ColorScheme.Success,
            Location = new Point(750, 15),
            AutoSize = true
        };

        lblCreditTotal = new Label
        {
            Text = "إجمالي الدائن: 0.00",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = ColorScheme.Error,
            Location = new Point(450, 15),
            AutoSize = true
        };

        lblDifference = new Label
        {
            Text = "الفرق: 0.00",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = ColorScheme.Warning,
            Location = new Point(200, 15),
            AutoSize = true
        };

        totalsPanel.Controls.AddRange(new Control[] { lblDebitTotal, lblCreditTotal, lblDifference });

        // Action Buttons
        btnSave = new Button
        {
            Text = "💾 حفظ القيد",
            Location = new Point(910, 600),
            Size = new Size(140, 45),
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += BtnSave_Click;

        btnCancel = new Button
        {
            Text = "❌ إلغاء",
            Location = new Point(760, 600),
            Size = new Size(140, 45),
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            BackColor = ColorScheme.Error,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderSize = 0;
        btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

        mainPanel.Controls.AddRange(new Control[] {
            lblTitle, lblDate, dtpEntryDate, lblDesc, txtDescription,
            lblLines, btnAddLine, btnRemoveLine, dgvLines, totalsPanel,
            btnSave, btnCancel
        });

        this.Controls.Add(mainPanel);
    }

    private void BtnAddLine_Click(object? sender, EventArgs e)
    {
        int rowIndex = dgvLines.Rows.Add("", "", 0, 0);
        // تفعيل التعديل على الخلية الأولى
        dgvLines.CurrentCell = dgvLines.Rows[rowIndex].Cells["Account"];
        dgvLines.BeginEdit(true);
    }

    private void BtnRemoveLine_Click(object? sender, EventArgs e)
    {
        if (dgvLines.SelectedRows.Count > 0)
        {
            dgvLines.Rows.Remove(dgvLines.SelectedRows[0]);
            CalculateTotals();
        }
    }

    private void DgvLines_SelectionChanged(object? sender, EventArgs e)
    {
        btnRemoveLine.Enabled = dgvLines.SelectedRows.Count > 0;
    }

    private void DgvLines_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0)
        {
            CalculateTotals();
        }
    }

    private void DgvLines_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
    {
        if (dgvLines.IsCurrentCellDirty)
        {
            dgvLines.CommitEdit(DataGridViewDataErrorContexts.Commit);
            // إعادة حساب المجاميع بعد التعديل مباشرة
            this.BeginInvoke((MethodInvoker)delegate
            {
                CalculateTotals();
            });
        }
    }

    private void DgvLines_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
    {
        // للتأكد من تفعيل زر الحفظ عند اختيار حساب من ComboBox
        if (dgvLines.CurrentCell?.ColumnIndex == dgvLines.Columns["Account"]?.Index)
        {
            if (e.Control is ComboBox cb)
            {
                cb.SelectedIndexChanged -= ComboBox_SelectedIndexChanged;
                cb.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            }
        }
    }

    private void ComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // إعادة حساب المجاميع عند اختيار حساب
        CalculateTotals();
    }

    private void CalculateTotals()
    {
        decimal totalDebit = 0;
        decimal totalCredit = 0;

        foreach (DataGridViewRow row in dgvLines.Rows)
        {
            if (row.Cells["Debit"].Value != null && decimal.TryParse(row.Cells["Debit"].Value?.ToString(), out decimal debit))
                totalDebit += debit;

            if (row.Cells["Credit"].Value != null && decimal.TryParse(row.Cells["Credit"].Value?.ToString(), out decimal credit))
                totalCredit += credit;
        }

        lblDebitTotal.Text = $"إجمالي المدين: {totalDebit:N2}";
        lblCreditTotal.Text = $"إجمالي الدائن: {totalCredit:N2}";

        decimal difference = Math.Abs(totalDebit - totalCredit);
        
        // ✅ عرض الفرق بشكل واضح مع رسالة تحذير إذا لم يتساوى
        if (difference > 0)
        {
            lblDifference.Text = $"⚠️ الفرق: {difference:N2} - يجب أن يتساوى المدين والدائن!";
            lblDifference.ForeColor = ColorScheme.Error;
        }
        else if (totalDebit == 0 && totalCredit == 0)
        {
            lblDifference.Text = "⚠️ الفرق: 0.00 - يجب إدخال مبالغ!";
            lblDifference.ForeColor = ColorScheme.Warning;
        }
        else
        {
            lblDifference.Text = $"✅ الفرق: {difference:N2} - القيد متوازن";
            lblDifference.ForeColor = ColorScheme.Success;
        }

        // التحقق من شروط تفعيل زر الحفظ
        bool canSave = false;
        
        // يجب أن يكون هناك بنود
        if (dgvLines.Rows.Count > 0)
        {
            // ✅ يجب أن يكون الفرق = 0 بالضبط (المدين = الدائن)
            // ✅ ويجب أن يكون المجموع أكبر من 0 (لا يمكن حفظ قيد بدون مبالغ)
            if (difference == 0 && totalDebit > 0 && totalCredit > 0)
            {
                // التحقق من أن جميع البنود لها حسابات
                bool allAccountsSelected = true;
                foreach (DataGridViewRow row in dgvLines.Rows)
                {
                    var accountValue = row.Cells["Account"].Value;
                    if (accountValue == null || string.IsNullOrWhiteSpace(accountValue.ToString()))
                    {
                        allAccountsSelected = false;
                        break;
                    }
                }
                
                canSave = allAccountsSelected;
            }
        }
        
        btnSave.Enabled = canSave;
    }

    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            // Validate
            if (dgvLines.Rows.Count == 0)
            {
                MessageBox.Show("يجب إضافة بنود للقيد!", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                return;
            }

            decimal totalDebit = 0, totalCredit = 0;
            foreach (DataGridViewRow row in dgvLines.Rows)
            {
                if (row.Cells["Account"].Value == null || string.IsNullOrEmpty(row.Cells["Account"].Value?.ToString()))
                {
                    MessageBox.Show("يجب اختيار حساب لكل بند!", "تنبيه",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                    return;
                }

                decimal.TryParse(row.Cells["Debit"].Value?.ToString() ?? "0", out decimal debit);
                decimal.TryParse(row.Cells["Credit"].Value?.ToString() ?? "0", out decimal credit);
                totalDebit += debit;
                totalCredit += credit;
            }

            // ✅ التحقق من عدم وجود مبالغ
            if (totalDebit == 0 && totalCredit == 0)
            {
                MessageBox.Show("يجب إدخال مبالغ في بنود القيد!\n\nلا يمكن حفظ قيد بدون مبالغ.", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                return;
            }

            // ✅ التحقق من تساوي المدين والدائن
            if (totalDebit != totalCredit)
            {
                decimal difference = Math.Abs(totalDebit - totalCredit);
                MessageBox.Show(
                    $"❌ إجمالي المدين يجب أن يساوي إجمالي الدائن!\n\n" +
                    $"المدين: {totalDebit:N2}\n" +
                    $"الدائن: {totalCredit:N2}\n" +
                    $"الفرق: {difference:N2}\n\n" +
                    $"يرجى تعديل المبالغ لتحقيق التوازن.",
                    "خطأ في توازن القيد",
                    MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                return;
            }

            JournalEntry entry;

            if (_isEditMode && _existingEntry != null)
            {
                // ✅ وضع التعديل
                entry = _existingEntry;
                entry.EntryDate = dtpEntryDate.Value.Date.ToUniversalTime();
                entry.Description = txtDescription.Text.Trim();
                entry.TotalDebit = totalDebit;
                entry.TotalCredit = totalCredit;

                // حذف البنود القديمة
                var oldLines = _dbContext.Set<JournalEntryLine>().Where(l => l.JournalEntryId == entry.JournalEntryId);
                _dbContext.Set<JournalEntryLine>().RemoveRange(oldLines);
            }
            else
            {
                // ✅ وضع الإضافة
                // Generate Entry Number
                var lastEntry = await _dbContext.Set<JournalEntry>()
                    .OrderByDescending(j => j.JournalEntryId)
                    .FirstOrDefaultAsync();

                int nextNumber = (lastEntry?.JournalEntryId ?? 0) + 1;
                string entryNumber = $"JE-{nextNumber:D6}";

                // Create Journal Entry
                entry = new JournalEntry
                {
                    EntryNumber = entryNumber,
                    EntryDate = dtpEntryDate.Value.Date.ToUniversalTime(),
                    EntryType = "Manual",
                    Description = txtDescription.Text.Trim(),
                    TotalDebit = totalDebit,
                    TotalCredit = totalCredit,
                    IsPosted = false,  // ✅ القيد معلق - غير مرحّل
                    PostedAt = null,   // ✅ لم يتم الترحيل بعد
                    CreatedBy = _currentUserId,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Set<JournalEntry>().Add(entry);
            }

            await _dbContext.SaveChangesAsync();

            // Add Lines
            int lineOrder = 1;
            foreach (DataGridViewRow row in dgvLines.Rows)
            {
                string accountInfo = row.Cells["Account"].Value?.ToString() ?? "";
                string accountCode = accountInfo.Split('-')[0].Trim();
                
                var account = await _dbContext.Set<Account>()
                    .FirstOrDefaultAsync(a => a.AccountCode == accountCode);

                if (account == null) continue;

                decimal.TryParse(row.Cells["Debit"].Value?.ToString() ?? "0", out decimal debit);
                decimal.TryParse(row.Cells["Credit"].Value?.ToString() ?? "0", out decimal credit);

                var line = new JournalEntryLine
                {
                    JournalEntryId = entry.JournalEntryId,
                    AccountId = account.AccountId,
                    Description = row.Cells["Description"].Value?.ToString(),
                    DebitAmount = debit,
                    CreditAmount = credit,
                    LineOrder = lineOrder++
                };

                _dbContext.Set<JournalEntryLine>().Add(line);
            }

            await _dbContext.SaveChangesAsync();

            string successMsg = _isEditMode ? "تم تعديل القيد بنجاح!" : "تم حفظ القيد بنجاح!";
            MessageBox.Show(successMsg, "نجح",
                MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في حفظ القيد: {ex.Message}\n\n{ex.InnerException?.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    // ✅ دالة تحميل القيد الموجود للتعديل
    private void LoadExistingEntry()
    {
        if (_existingEntry == null) return;

        try
        {
            // تعيين التاريخ والوصف
            dtpEntryDate.Value = _existingEntry.EntryDate.ToLocalTime();
            txtDescription.Text = _existingEntry.Description;

            // تحميل البنود
            _lines.Clear();
            dgvLines.Rows.Clear();

            foreach (var line in _existingEntry.Lines.OrderBy(l => l.LineOrder))
            {
                var lineItem = new JournalLineItem
                {
                    AccountId = line.AccountId,
                    AccountCode = line.Account?.AccountCode,
                    AccountName = line.Account?.AccountName,
                    Description = line.Description,
                    DebitAmount = line.DebitAmount,
                    CreditAmount = line.CreditAmount
                };

                _lines.Add(lineItem);

                // ✅ تنسيق الحساب بنفس طريقة ComboBox
                string accountDisplay = $"{line.Account?.AccountCode ?? ""} - {line.Account?.AccountName ?? ""}";

                dgvLines.Rows.Add(new object?[]
                {
                    accountDisplay,  // بدلاً من AccountCode فقط
                    line.Description,
                    line.DebitAmount > 0 ? line.DebitAmount.ToString("N2") : "",
                    line.CreditAmount > 0 ? line.CreditAmount.ToString("N2") : ""
                });
            }

            CalculateTotals();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل بيانات القيد:\n{ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }
}

public class JournalLineItem
{
    public int AccountId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public string? Description { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
}
