using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using GraceWay.AccountingSystem.Infrastructure.Data;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class JournalEntriesListForm : Form
{
    private readonly AppDbContext _dbContext;
    private readonly int _currentUserId;
    private DataGridView dgvJournalEntries = null!;
    private DateTimePicker dtpFrom = null!;
    private DateTimePicker dtpTo = null!;
    private ComboBox cmbEntryType = null!;
    private TextBox txtSearch = null!;
    private Button btnSearch = null!;
    private Button btnAddManualEntry = null!;
    private Button btnRefresh = null!;
    private Button btnViewDetails = null!;
    private Button btnEdit = null!;  // ✅ زر التعديل
    private Button btnDelete = null!;  // ✅ زر الحذف
    private Button btnPost = null!;  // ✅ زر الترحيل
    private Label lblTotal = null!;

    public JournalEntriesListForm(AppDbContext dbContext, int currentUserId)
    {
        _dbContext = dbContext;
        _currentUserId = currentUserId;
        InitializeComponent();
        InitializeCustomComponents();
        LoadJournalEntries();
    }

    private void InitializeCustomComponents()
    {
        this.Text = "📋 القيود اليومية";
        this.Size = new Size(1400, 800);
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;

        // Main Panel
        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(30)
        };

        // Title
        Label lblTitle = new Label
        {
            Text = "📋 القيود اليومية",
            Font = new Font("Cairo", 18F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(30, 20)
        };

        // Filter Panel
        Panel filterPanel = new Panel
        {
            Size = new Size(1340, 120),
            Location = new Point(30, 70),
            BackColor = ColorScheme.Background,
            BorderStyle = BorderStyle.FixedSingle
        };

        // Date From
        Label lblFrom = new Label
        {
            Text = "من تاريخ:",
            Font = new Font("Cairo", 11F),
            Location = new Point(1180, 15),
            AutoSize = true
        };
        dtpFrom = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Location = new Point(1020, 12),
            Size = new Size(150, 30),
            Font = new Font("Cairo", 10F),
            Value = DateTime.Today.AddMonths(-1)
        };

        // Date To
        Label lblTo = new Label
        {
            Text = "إلى تاريخ:",
            Font = new Font("Cairo", 11F),
            Location = new Point(940, 15),
            AutoSize = true
        };
        dtpTo = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Location = new Point(780, 12),
            Size = new Size(150, 30),
            Font = new Font("Cairo", 10F),
            Value = DateTime.Today
        };

        // Entry Type Filter
        Label lblType = new Label
        {
            Text = "نوع القيد:",
            Font = new Font("Cairo", 11F),
            Location = new Point(690, 15),
            AutoSize = true
        };
        cmbEntryType = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(480, 12),
            Size = new Size(200, 30),
            Font = new Font("Cairo", 10F)
        };
        cmbEntryType.Items.AddRange(new object[] { "الكل", "تلقائي", "يدوي" });
        cmbEntryType.SelectedIndex = 0;

        // Search Box
        Label lblSearch = new Label
        {
            Text = "بحث:",
            Font = new Font("Cairo", 11F),
            Location = new Point(410, 15),
            AutoSize = true
        };
        txtSearch = new TextBox
        {
            Location = new Point(200, 12),
            Size = new Size(200, 30),
            Font = new Font("Cairo", 10F),
            PlaceholderText = "رقم القيد أو الوصف..."
        };

        // Buttons Row 1
        btnSearch = new Button
        {
            Text = "🔍 بحث",
            Location = new Point(100, 10),
            Size = new Size(90, 35),
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnSearch.FlatAppearance.BorderSize = 0;
        btnSearch.Click += BtnSearch_Click;

        btnRefresh = new Button
        {
            Text = "🔄 تحديث",
            Location = new Point(10, 10),
            Size = new Size(85, 35),
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnRefresh.FlatAppearance.BorderSize = 0;
        btnRefresh.Click += (s, e) => LoadJournalEntries();

        // Buttons Row 2
        btnAddManualEntry = new Button
        {
            Text = "➕ إضافة قيد يدوي",
            Location = new Point(1150, 55),
            Size = new Size(160, 40),
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnAddManualEntry.FlatAppearance.BorderSize = 0;
        btnAddManualEntry.Click += BtnAddManualEntry_Click;

        btnViewDetails = new Button
        {
            Text = "👁 عرض التفاصيل",
            Location = new Point(980, 55),
            Size = new Size(160, 40),
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnViewDetails.FlatAppearance.BorderSize = 0;
        btnViewDetails.Click += BtnViewDetails_Click;

        // ✅ زر التعديل
        btnEdit = new Button
        {
            Text = "✏️ تعديل القيد",
            Location = new Point(810, 55),
            Size = new Size(160, 40),
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            BackColor = ColorScheme.Warning,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnEdit.FlatAppearance.BorderSize = 0;
        btnEdit.Click += BtnEdit_Click;

        // ✅ زر الحذف
        btnDelete = new Button
        {
            Text = "🗑️ حذف القيد",
            Location = new Point(640, 55),
            Size = new Size(160, 40),
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            BackColor = ColorScheme.Error,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnDelete.FlatAppearance.BorderSize = 0;
        btnDelete.Click += BtnDelete_Click;

        // ✅ زر الترحيل/إلغاء الترحيل
        btnPost = new Button
        {
            Text = "📌 ترحيل القيد",
            Location = new Point(470, 55),
            Size = new Size(160, 40),
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            BackColor = Color.FromArgb(52, 152, 219), // أزرق
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnPost.FlatAppearance.BorderSize = 0;
        btnPost.Click += BtnPost_Click;

        filterPanel.Controls.AddRange(new Control[] {
            lblFrom, dtpFrom, lblTo, dtpTo,
            lblType, cmbEntryType, lblSearch, txtSearch,
            btnSearch, btnRefresh, btnAddManualEntry, btnViewDetails, btnEdit, btnDelete, btnPost
        });

        // DataGridView
        dgvJournalEntries = new DataGridView
        {
            Location = new Point(30, 200),
            Size = new Size(1340, 480),
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ColumnHeadersHeight = 40,
            RowTemplate = { Height = 35 },
            Font = new Font("Cairo", 10F),
            EnableHeadersVisualStyles = false
        };

        dgvJournalEntries.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Primary;
        dgvJournalEntries.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvJournalEntries.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        dgvJournalEntries.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvJournalEntries.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvJournalEntries.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 255);
        dgvJournalEntries.SelectionChanged += DgvJournalEntries_SelectionChanged;
        dgvJournalEntries.CellDoubleClick += DgvJournalEntries_CellDoubleClick;

        // Total Label
        lblTotal = new Label
        {
            Text = "إجمالي القيود: 0",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(30, 690)
        };

        mainPanel.Controls.AddRange(new Control[] {
            lblTitle, filterPanel, dgvJournalEntries, lblTotal
        });

        this.Controls.Add(mainPanel);
    }

    private void LoadJournalEntries()
    {
        try
        {
            var query = _dbContext.Set<JournalEntry>()
                .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
                .AsQueryable();

            // Apply filters
            var fromDateUtc = dtpFrom.Value.Date.ToUniversalTime();
            var toDateUtc = dtpTo.Value.Date.AddDays(1).ToUniversalTime(); // Add 1 day to include the entire end date
            
            query = query.Where(j => j.EntryDate >= fromDateUtc && j.EntryDate < toDateUtc);

            if (cmbEntryType.SelectedIndex == 1) // تلقائي
                query = query.Where(j => j.EntryType == "Auto");
            else if (cmbEntryType.SelectedIndex == 2) // يدوي
                query = query.Where(j => j.EntryType == "Manual");

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var searchTerm = txtSearch.Text.Trim();
                query = query.Where(j => j.EntryNumber.Contains(searchTerm) || 
                                       (j.Description != null && j.Description.Contains(searchTerm)));
            }

            var entries = query.OrderByDescending(j => j.EntryDate)
                              .ThenByDescending(j => j.JournalEntryId)
                              .ToList();

            dgvJournalEntries.Columns.Clear();
            dgvJournalEntries.DataSource = null;

            dgvJournalEntries.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "JournalEntryId",
                HeaderText = "المعرف",
                DataPropertyName = "JournalEntryId",
                Visible = false
            });

            dgvJournalEntries.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "EntryNumber",
                HeaderText = "رقم القيد",
                Width = 120
            });

            dgvJournalEntries.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "EntryDate",
                HeaderText = "التاريخ",
                Width = 120
            });

            dgvJournalEntries.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "EntryType",
                HeaderText = "النوع",
                Width = 100
            });

            dgvJournalEntries.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ReferenceType",
                HeaderText = "المرجع",
                Width = 120
            });

            dgvJournalEntries.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Description",
                HeaderText = "الوصف",
                Width = 280
            });

            dgvJournalEntries.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalDebit",
                HeaderText = "إجمالي المدين",
                Width = 130,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
            });

            dgvJournalEntries.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalCredit",
                HeaderText = "إجمالي الدائن",
                Width = 130,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
            });

            dgvJournalEntries.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "IsPosted",
                HeaderText = "الحالة",
                Width = 100
            });

            // ✅ عمود مخفي لتخزين القيمة الحقيقية لـ IsPosted
            dgvJournalEntries.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "IsPostedValue",
                HeaderText = "IsPostedValue",
                Visible = false
            });

            dgvJournalEntries.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CreatedAt",
                HeaderText = "تاريخ الإنشاء",
                Width = 150
            });

            foreach (var entry in entries)
            {
                dgvJournalEntries.Rows.Add(
                    entry.JournalEntryId,
                    entry.EntryNumber,
                    entry.EntryDate.ToString("dd/MM/yyyy"),
                    entry.EntryType == "Auto" ? "تلقائي" : "يدوي",
                    entry.ReferenceType ?? "-",
                    entry.Description ?? "-",
                    entry.TotalDebit,
                    entry.TotalCredit,
                    entry.IsPosted ? "✅ مرحل" : "⏳ معلق",
                    entry.IsPosted,  // ✅ القيمة الحقيقية في العمود المخفي
                    entry.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                );
            }

            lblTotal.Text = $"إجمالي القيود: {entries.Count}";
            btnViewDetails.Enabled = dgvJournalEntries.Rows.Count > 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل القيود: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private void BtnSearch_Click(object? sender, EventArgs e)
    {
        LoadJournalEntries();
    }

    private void BtnAddManualEntry_Click(object? sender, EventArgs e)
    {
        var addForm = new AddJournalEntryForm(_dbContext, _currentUserId);
        if (addForm.ShowDialog() == DialogResult.OK)
        {
            LoadJournalEntries();
        }
    }

    private void BtnViewDetails_Click(object? sender, EventArgs e)
    {
        if (dgvJournalEntries.SelectedRows.Count == 0) return;

        int entryId = Convert.ToInt32(dgvJournalEntries.SelectedRows[0].Cells["JournalEntryId"].Value);
        ShowEntryDetails(entryId);
    }

    private void DgvJournalEntries_SelectionChanged(object? sender, EventArgs e)
    {
        bool hasSelection = dgvJournalEntries.SelectedRows.Count > 0;
        btnViewDetails.Enabled = hasSelection;
        
        if (hasSelection)
        {
            var selectedRow = dgvJournalEntries.SelectedRows[0];
            string entryType = selectedRow.Cells["EntryType"].Value?.ToString() ?? "";
            
            // ✅ قراءة القيمة الحقيقية من العمود المخفي
            bool isPosted = Convert.ToBoolean(selectedRow.Cells["IsPostedValue"].Value ?? false);
            
            bool isManualEntry = entryType == "يدوي";
            
            // ✅ التعديل والحذف: فقط للقيود اليدوية غير المرحّلة
            btnEdit.Enabled = isManualEntry && !isPosted;
            btnDelete.Enabled = isManualEntry && !isPosted;
            
            // ✅ الترحيل: فقط للقيود اليدوية
            btnPost.Enabled = isManualEntry;
            
            // ✅ تغيير نص زر الترحيل حسب الحالة
            if (isPosted)
            {
                btnPost.Text = "↩️ إلغاء الترحيل";
                btnPost.BackColor = Color.FromArgb(231, 76, 60); // أحمر
            }
            else
            {
                btnPost.Text = "📌 ترحيل القيد";
                btnPost.BackColor = Color.FromArgb(52, 152, 219); // أزرق
            }
        }
        else
        {
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            btnPost.Enabled = false;
        }
    }

    private void DgvJournalEntries_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0)
        {
            int entryId = Convert.ToInt32(dgvJournalEntries.Rows[e.RowIndex].Cells["JournalEntryId"].Value);
            ShowEntryDetails(entryId);
        }
    }

    private void ShowEntryDetails(int entryId)
    {
        var entry = _dbContext.Set<JournalEntry>()
            .Include(j => j.Lines)
            .ThenInclude(l => l.Account)
            .FirstOrDefault(j => j.JournalEntryId == entryId);

        if (entry == null)
        {
            MessageBox.Show("لم يتم العثور على القيد!", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            return;
        }

        // Create details form
        Form detailsForm = new Form
        {
            Text = $"تفاصيل القيد - {entry.EntryNumber}",
            Size = new Size(900, 600),
            StartPosition = FormStartPosition.CenterParent,
            RightToLeft = RightToLeft.Yes,
            RightToLeftLayout = false,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(20)
        };

        // Header Info
        Label lblHeader = new Label
        {
            Text = $"القيد رقم: {entry.EntryNumber}",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            Location = new Point(20, 20),
            AutoSize = true
        };

        Label lblDate = new Label
        {
            Text = $"التاريخ: {entry.EntryDate:dd/MM/yyyy}",
            Font = new Font("Cairo", 11F),
            Location = new Point(20, 60),
            AutoSize = true
        };

        Label lblType = new Label
        {
            Text = $"النوع: {(entry.EntryType == "Auto" ? "تلقائي" : "يدوي")}",
            Font = new Font("Cairo", 11F),
            Location = new Point(20, 90),
            AutoSize = true
        };

        Label lblDesc = new Label
        {
            Text = $"الوصف: {entry.Description ?? "-"}",
            Font = new Font("Cairo", 11F),
            Location = new Point(20, 120),
            Size = new Size(840, 30)
        };

        // Lines DataGridView
        DataGridView dgvLines = new DataGridView
        {
            Location = new Point(20, 160),
            Size = new Size(840, 300),
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = new Font("Cairo", 10F),
            EnableHeadersVisualStyles = false
        };

        dgvLines.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Primary;
        dgvLines.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvLines.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);

        dgvLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "AccountCode", HeaderText = "رمز الحساب", Width = 100 });
        dgvLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "AccountName", HeaderText = "اسم الحساب", Width = 250 });
        dgvLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "البيان", Width = 200 });
        dgvLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Debit", HeaderText = "مدين", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
        dgvLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Credit", HeaderText = "دائن", Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });

        foreach (var line in entry.Lines.OrderBy(l => l.LineOrder))
        {
            dgvLines.Rows.Add(
                line.Account?.AccountCode ?? "-",
                line.Account?.AccountName ?? "-",
                line.Description ?? "-",
                line.DebitAmount,
                line.CreditAmount
            );
        }

        // Totals
        Label lblTotals = new Label
        {
            Text = $"إجمالي المدين: {entry.TotalDebit:N2} | إجمالي الدائن: {entry.TotalCredit:N2}",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            Location = new Point(20, 470),
            AutoSize = true
        };

        // Close Button
        Button btnClose = new Button
        {
            Text = "إغلاق",
            Size = new Size(120, 40),
            Location = new Point(740, 510),
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Click += (s, e) => detailsForm.Close();

        mainPanel.Controls.AddRange(new Control[] {
            lblHeader, lblDate, lblType, lblDesc, dgvLines, lblTotals, btnClose
        });

        detailsForm.Controls.Add(mainPanel);
        detailsForm.ShowDialog();
    }

    // ✅ دالة التعديل
    private void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dgvJournalEntries.SelectedRows.Count == 0) return;

        int entryId = Convert.ToInt32(dgvJournalEntries.SelectedRows[0].Cells["JournalEntryId"].Value);
        string entryType = dgvJournalEntries.SelectedRows[0].Cells["EntryType"].Value?.ToString() ?? "";
        
        // ✅ قراءة حالة الترحيل من العمود المخفي
        bool isPosted = Convert.ToBoolean(dgvJournalEntries.SelectedRows[0].Cells["IsPostedValue"].Value ?? false);

        // التأكد من أن القيد يدوي
        if (entryType != "يدوي")
        {
            MessageBox.Show("لا يمكن تعديل القيود التلقائية. يمكن تعديل القيود اليدوية فقط.", "تنبيه",
                MessageBoxButtons.OK, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            return;
        }

        // ✅ التحقق من حالة الترحيل من DataGridView
        if (isPosted)
        {
            MessageBox.Show("لا يمكن تعديل قيد تم ترحيله بالفعل!\n\nقم بإلغاء الترحيل أولاً ثم عدّل القيد.", "تنبيه",
                MessageBoxButtons.OK, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            return;
        }

        var entry = _dbContext.Set<JournalEntry>()
            .Include(j => j.Lines)
            .ThenInclude(l => l.Account)
            .FirstOrDefault(j => j.JournalEntryId == entryId);

        if (entry == null)
        {
            MessageBox.Show("لم يتم العثور على القيد!", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            return;
        }

        // فتح نموذج التعديل
        var editForm = new AddJournalEntryForm(_dbContext, _currentUserId, entry);
        if (editForm.ShowDialog() == DialogResult.OK)
        {
            LoadJournalEntries();
            MessageBox.Show("✅ تم تعديل القيد بنجاح", "نجح",
                MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    // ✅ دالة الحذف
    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dgvJournalEntries.SelectedRows.Count == 0) return;

        int entryId = Convert.ToInt32(dgvJournalEntries.SelectedRows[0].Cells["JournalEntryId"].Value);
        string entryType = dgvJournalEntries.SelectedRows[0].Cells["EntryType"].Value?.ToString() ?? "";
        string entryNumber = dgvJournalEntries.SelectedRows[0].Cells["EntryNumber"].Value?.ToString() ?? "";
        
        // ✅ قراءة حالة الترحيل من العمود المخفي بدلاً من قاعدة البيانات
        bool isPosted = Convert.ToBoolean(dgvJournalEntries.SelectedRows[0].Cells["IsPostedValue"].Value ?? false);

        // التأكد من أن القيد يدوي
        if (entryType != "يدوي")
        {
            MessageBox.Show("لا يمكن حذف القيود التلقائية. يمكن حذف القيود اليدوية فقط.", "تنبيه",
                MessageBoxButtons.OK, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            return;
        }

        // ✅ التحقق من حالة الترحيل من DataGridView (أحدث من قاعدة البيانات)
        if (isPosted)
        {
            MessageBox.Show("لا يمكن حذف قيد تم ترحيله بالفعل!\n\nقم بإلغاء الترحيل أولاً ثم احذف القيد.", "تنبيه",
                MessageBoxButtons.OK, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            return;
        }

        var entry = _dbContext.Set<JournalEntry>()
            .Include(j => j.Lines)
            .FirstOrDefault(j => j.JournalEntryId == entryId);

        if (entry == null)
        {
            MessageBox.Show("لم يتم العثور على القيد!", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            return;
        }

        // تأكيد الحذف
        var result = MessageBox.Show(
            $"هل أنت متأكد من حذف القيد رقم {entryNumber}؟\n\n" +
            $"سيتم حذف القيد وجميع بنوده بشكل نهائي.",
            "تأكيد الحذف",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2,
            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);

        if (result != DialogResult.Yes) return;

        try
        {
            // حذف البنود أولاً
            _dbContext.Set<JournalEntryLine>().RemoveRange(entry.Lines);
            
            // ثم حذف القيد نفسه
            _dbContext.Set<JournalEntry>().Remove(entry);
            
            _dbContext.SaveChanges();

            MessageBox.Show("✅ تم حذف القيد بنجاح", "نجح",
                MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);

            LoadJournalEntries();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في حذف القيد:\n{ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    // ✅ دالة الترحيل/إلغاء الترحيل
    private void BtnPost_Click(object? sender, EventArgs e)
    {
        if (dgvJournalEntries.SelectedRows.Count == 0) return;

        int entryId = Convert.ToInt32(dgvJournalEntries.SelectedRows[0].Cells["JournalEntryId"].Value);
        string entryType = dgvJournalEntries.SelectedRows[0].Cells["EntryType"].Value?.ToString() ?? "";
        string entryNumber = dgvJournalEntries.SelectedRows[0].Cells["EntryNumber"].Value?.ToString() ?? "";
        
        // ✅ قراءة القيمة الحقيقية من العمود المخفي
        bool isPosted = Convert.ToBoolean(dgvJournalEntries.SelectedRows[0].Cells["IsPostedValue"].Value ?? false);

        // التأكد من أن القيد يدوي
        if (entryType != "يدوي")
        {
            MessageBox.Show("لا يمكن تغيير حالة القيود التلقائية.", "تنبيه",
                MessageBoxButtons.OK, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            return;
        }

        var entry = _dbContext.Set<JournalEntry>()
            .FirstOrDefault(j => j.JournalEntryId == entryId);

        if (entry == null)
        {
            MessageBox.Show("لم يتم العثور على القيد!", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            return;
        }

        // رسالة التأكيد
        string confirmMessage;
        string confirmTitle;
        
        if (isPosted)
        {
            confirmMessage = $"هل تريد إلغاء ترحيل القيد رقم {entryNumber}؟\n\n" +
                           $"سيتم تحويل القيد من 'مرحّل' إلى 'معلق'.\n" +
                           $"سيمكنك تعديله أو حذفه بعد ذلك.";
            confirmTitle = "تأكيد إلغاء الترحيل";
        }
        else
        {
            confirmMessage = $"هل تريد ترحيل القيد رقم {entryNumber}؟\n\n" +
                           $"بعد الترحيل لن يمكن تعديل أو حذف القيد إلا بعد إلغاء الترحيل.";
            confirmTitle = "تأكيد الترحيل";
        }

        var result = MessageBox.Show(
            confirmMessage,
            confirmTitle,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2,
            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);

        if (result != DialogResult.Yes) return;

        try
        {
            // حفظ الحالة الجديدة
            bool newPostedStatus;
            
            if (isPosted)
            {
                // إلغاء الترحيل
                entry.IsPosted = false;
                entry.PostedAt = null;
                newPostedStatus = false;
            }
            else
            {
                // الترحيل
                entry.IsPosted = true;
                entry.PostedAt = DateTime.UtcNow;
                newPostedStatus = true;
            }

            _dbContext.SaveChanges();

            // ✅ تحديث الصف في DataGridView مباشرة
            var selectedRow = dgvJournalEntries.SelectedRows[0];
            selectedRow.Cells["IsPosted"].Value = newPostedStatus ? "✅ مرحل" : "⏳ معلق";
            selectedRow.Cells["IsPostedValue"].Value = newPostedStatus;

            // ✅ تحديث حالة الأزرار فوراً
            btnEdit.Enabled = !newPostedStatus;
            btnDelete.Enabled = !newPostedStatus;
            
            if (newPostedStatus)
            {
                btnPost.Text = "↩️ إلغاء الترحيل";
                btnPost.BackColor = Color.FromArgb(231, 76, 60);
            }
            else
            {
                btnPost.Text = "📌 ترحيل القيد";
                btnPost.BackColor = Color.FromArgb(52, 152, 219);
            }

            string successMsg = isPosted 
                ? "✅ تم إلغاء ترحيل القيد بنجاح! يمكنك الآن تعديله أو حذفه." 
                : "✅ تم ترحيل القيد بنجاح!";

            MessageBox.Show(successMsg, "نجح",
                MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تغيير حالة القيد:\n{ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }
}
