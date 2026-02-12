using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Presentation.Forms.Admin;

public partial class AuditLogsForm : Form
{
    private readonly IAuditService _auditService;
    private DataGridView dgvLogs = null!;
    private DateTimePicker dtpFrom = null!;
    private DateTimePicker dtpTo = null!;
    private ComboBox cmbEntityType = null!;
    private ComboBox cmbAction = null!;
    private TextBox txtSearch = null!;
    private Button btnFilter = null!;
    private Button btnClear = null!;
    private Button btnExport = null!;
    private Button btnRefresh = null!;
    private Label lblTotalRecords = null!;

    public AuditLogsForm(IAuditService auditService)
    {
        _auditService = auditService;
        InitializeComponent();
        InitializeCustomComponents();
        _ = LoadLogsAsync();
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(1400, 800);
        this.Name = "AuditLogsForm";
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.ResumeLayout(false);
    }

    private void InitializeCustomComponents()
    {
        this.Text = "سجل العمليات (Audit Trail)";
        this.Size = new Size(1400, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);

        // Main Layout
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(20)
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // Filters
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Grid
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Footer

        // === FILTER PANEL ===
        var filterPanel = CreateFilterPanel();
        mainLayout.Controls.Add(filterPanel, 0, 0);

        // === DATA GRID ===
        dgvLogs = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            Font = new Font("Cairo", 9F)
        };

        dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "Timestamp", HeaderText = "التاريخ والوقت", Width = 150 });
        dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "UserFullName", HeaderText = "المستخدم", Width = 150 });
        dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "Action", HeaderText = "الإجراء", Width = 100 });
        dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "EntityType", HeaderText = "نوع العنصر", Width = 120 });
        dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "EntityName", HeaderText = "اسم العنصر", Width = 200 });
        dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "الوصف", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

        mainLayout.Controls.Add(dgvLogs, 0, 1);

        // === FOOTER ===
        var footerPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ColorScheme.Background
        };

        lblTotalRecords = new Label
        {
            Text = "إجمالي السجلات: 0",
            Location = new Point(10, 15),
            AutoSize = true,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary
        };

        footerPanel.Controls.Add(lblTotalRecords);
        mainLayout.Controls.Add(footerPanel, 0, 2);

        this.Controls.Add(mainLayout);
    }

    private Panel CreateFilterPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(15)
        };

        // Row 1
        var lblFrom = new Label { Text = "من تاريخ:", Location = new Point(1150, 10), AutoSize = true };
        dtpFrom = new DateTimePicker
        {
            Location = new Point(950, 7),
            Size = new Size(180, 30),
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Now.AddMonths(-1)
        };

        var lblTo = new Label { Text = "إلى تاريخ:", Location = new Point(870, 10), AutoSize = true };
        dtpTo = new DateTimePicker
        {
            Location = new Point(670, 7),
            Size = new Size(180, 30),
            Format = DateTimePickerFormat.Short
        };

        var lblEntityType = new Label { Text = "نوع العنصر:", Location = new Point(580, 10), AutoSize = true };
        cmbEntityType = new ComboBox
        {
            Location = new Point(400, 7),
            Size = new Size(160, 30),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbEntityType.Items.AddRange(new object[] { "الكل", "Trip", "Umrah", "Flight", "Customer", "Supplier", "Invoice" });
        cmbEntityType.SelectedIndex = 0;

        panel.Controls.AddRange(new Control[] { lblFrom, dtpFrom, lblTo, dtpTo, lblEntityType, cmbEntityType });

        // Row 2
        var lblAction = new Label { Text = "الإجراء:", Location = new Point(1150, 50), AutoSize = true };
        cmbAction = new ComboBox
        {
            Location = new Point(950, 47),
            Size = new Size(180, 30),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbAction.Items.AddRange(new object[] { "الكل", "Create", "Update", "Delete", "Approve", "Cancel" });
        cmbAction.SelectedIndex = 0;

        var lblSearch = new Label { Text = "بحث:", Location = new Point(870, 50), AutoSize = true };
        txtSearch = new TextBox
        {
            Location = new Point(600, 47),
            Size = new Size(250, 30),
            PlaceholderText = "ابحث في الوصف أو اسم المستخدم..."
        };

        btnFilter = CreateButton("تصفية", new Point(490, 47), ColorScheme.Primary);
        btnFilter.Click += async (s, e) => await LoadLogsAsync();

        btnClear = CreateButton("مسح", new Point(390, 47), Color.Gray);
        btnClear.Click += (s, e) => ClearFilters();

        btnRefresh = CreateButton("🔄 تحديث", new Point(270, 47), ColorScheme.Success);
        btnRefresh.Click += async (s, e) => await LoadLogsAsync();

        btnExport = CreateButton("📊 تصدير", new Point(150, 47), ColorScheme.Info);
        btnExport.Click += BtnExport_Click;

        panel.Controls.AddRange(new Control[] { 
            lblAction, cmbAction, lblSearch, txtSearch, 
            btnFilter, btnClear, btnRefresh, btnExport 
        });

        return panel;
    }

    private Button CreateButton(string text, Point location, Color backColor)
    {
        return new Button
        {
            Text = text,
            Location = location,
            Size = new Size(100, 35),
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Cairo", 9F, FontStyle.Bold)
        };
    }

    private async Task LoadLogsAsync()
    {
        try
        {
            btnFilter.Enabled = false;
            btnFilter.Text = "جاري التحميل...";
            this.Cursor = Cursors.WaitCursor;

            var logs = await _auditService.GetLogsAsync(
                fromDate: dtpFrom.Value.Date,
                toDate: dtpTo.Value.Date.AddDays(1).AddSeconds(-1),
                entityType: cmbEntityType.SelectedIndex == 0 ? null : cmbEntityType.Text
            );

            // Filter by action
            if (cmbAction.SelectedIndex > 0)
            {
                var actionFilter = Enum.Parse<AuditAction>(cmbAction.Text);
                logs = logs.Where(l => l.Action == actionFilter).ToList();
            }

            // Filter by search
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var search = txtSearch.Text.ToLower();
                logs = logs.Where(l =>
                    l.Description.ToLower().Contains(search) ||
                    l.UserFullName.ToLower().Contains(search) ||
                    l.EntityName.ToLower().Contains(search)
                ).ToList();
            }

            dgvLogs.Rows.Clear();

            foreach (var log in logs)
            {
                dgvLogs.Rows.Add(
                    log.Timestamp.ToString("dd/MM/yyyy hh:mm tt"),
                    log.UserFullName,
                    GetActionText(log.Action),
                    GetEntityTypeText(log.EntityType),
                    log.EntityName,
                    log.Description
                );
            }

            lblTotalRecords.Text = $"إجمالي السجلات: {logs.Count}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل السجلات:\n{ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
        finally
        {
            btnFilter.Enabled = true;
            btnFilter.Text = "تصفية";
            this.Cursor = Cursors.Default;
        }
    }

    private void ClearFilters()
    {
        dtpFrom.Value = DateTime.Now.AddMonths(-1);
        dtpTo.Value = DateTime.Now;
        cmbEntityType.SelectedIndex = 0;
        cmbAction.SelectedIndex = 0;
        txtSearch.Clear();
        _ = LoadLogsAsync();
    }

    private void BtnExport_Click(object? sender, EventArgs e)
    {
        // TODO: Export to Excel
        MessageBox.Show("سيتم إضافة التصدير قريباً", "قريباً", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private string GetActionText(AuditAction action) => action switch
    {
        AuditAction.Create => "إضافة",
        AuditAction.Update => "تعديل",
        AuditAction.Delete => "حذف",
        AuditAction.Approve => "موافقة",
        AuditAction.Reject => "رفض",
        AuditAction.Cancel => "إلغاء",
        AuditAction.Login => "تسجيل دخول",
        AuditAction.Logout => "تسجيل خروج",
        _ => action.ToString()
    };

    private string GetEntityTypeText(string entityType) => entityType switch
    {
        "Trip" => "رحلة",
        "Umrah" => "عمرة",
        "Flight" => "طيران",
        "Customer" => "عميل",
        "Supplier" => "مورد",
        "Invoice" => "فاتورة",
        _ => entityType
    };
}
