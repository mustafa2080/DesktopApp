using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GraceWay.AccountingSystem.Presentation.Forms.Admin;

public partial class UserManagementForm : Form
{
    private readonly AppDbContext _context;
    private DataGridView dgvUsers;
    private Button btnAdd;
    private Button btnEdit;
    private Button btnDelete;
    private Button btnRefresh;
    private Button btnChangePassword;
    private TextBox txtSearch;
    private ComboBox cmbRoleFilter;
    private CheckBox chkShowInactive;

    public UserManagementForm(AppDbContext context)
    {
        _context = context;
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        this.Text = "إدارة المستخدمين";
        this.Size = new Size(1200, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = Color.FromArgb(245, 247, 250);

        // Top Panel - Search and Filters
        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 100,
            Padding = new Padding(20),
            BackColor = Color.White
        };

        // Add shadow effect
        topPanel.Paint += (s, e) =>
        {
            e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 220, 220), 1),
                0, topPanel.Height - 1, topPanel.Width, topPanel.Height - 1);
        };

        var searchLabel = new Label
        {
            Text = "بحث:",
            Location = new Point(1050, 25),
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(60, 60, 60)
        };

        txtSearch = new TextBox
        {
            Location = new Point(850, 22),
            Width = 180,
            Height = 35,
            Font = new Font("Segoe UI", 10F),
            BorderStyle = BorderStyle.FixedSingle
        };
        txtSearch.TextChanged += (s, e) => LoadData();

        var roleLabel = new Label
        {
            Text = "الدور:",
            Location = new Point(770, 25),
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(60, 60, 60)
        };

        cmbRoleFilter = new ComboBox
        {
            Location = new Point(600, 22),
            Width = 150,
            Height = 35,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10F),
            FlatStyle = FlatStyle.Flat
        };
        cmbRoleFilter.SelectedIndexChanged += (s, e) => LoadData();

        chkShowInactive = new CheckBox
        {
            Text = "عرض الغير نشطين",
            Location = new Point(400, 25),
            AutoSize = true,
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.FromArgb(60, 60, 60),
            Checked = false
        };
        chkShowInactive.CheckedChanged += (s, e) => LoadData();

        topPanel.Controls.AddRange(new Control[] { 
            searchLabel, txtSearch, 
            roleLabel, cmbRoleFilter,
            chkShowInactive
        });

        // DataGridView Container
        var gridPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 10, 20, 10),
            BackColor = Color.FromArgb(245, 247, 250)
        };

        dgvUsers = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            ColumnHeadersHeight = 45,
            RowHeadersVisible = false,
            RowTemplate = { Height = 40 },
            Font = new Font("Segoe UI", 10F),
            GridColor = Color.FromArgb(230, 230, 230),
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
        };

        dgvUsers.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn { Name = "UserId", HeaderText = "ID", DataPropertyName = "UserId", Width = 60 },
            new DataGridViewTextBoxColumn { Name = "Username", HeaderText = "اسم المستخدم", DataPropertyName = "Username", Width = 150 },
            new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "الاسم الكامل", DataPropertyName = "FullName", Width = 200 },
            new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "البريد الإلكتروني", DataPropertyName = "Email", Width = 200 },
            new DataGridViewTextBoxColumn { Name = "Phone", HeaderText = "الهاتف", DataPropertyName = "Phone", Width = 120 },
            new DataGridViewTextBoxColumn { Name = "RoleName", HeaderText = "الدور", DataPropertyName = "RoleName", Width = 150 },
            new DataGridViewCheckBoxColumn { Name = "IsActive", HeaderText = "نشط", DataPropertyName = "IsActive", Width = 70 },
            new DataGridViewTextBoxColumn { Name = "CreatedAt", HeaderText = "تاريخ الإنشاء", DataPropertyName = "CreatedAt", Width = 150 }
        });

        // Style DataGridView
        dgvUsers.EnableHeadersVisualStyles = false;
        dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Primary;
        dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        dgvUsers.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvUsers.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
        
        dgvUsers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
        dgvUsers.DefaultCellStyle.BackColor = Color.White;
        dgvUsers.DefaultCellStyle.ForeColor = Color.FromArgb(60, 60, 60);
        dgvUsers.DefaultCellStyle.SelectionBackColor = ColorScheme.Primary;
        dgvUsers.DefaultCellStyle.SelectionForeColor = Color.White;
        dgvUsers.DefaultCellStyle.Padding = new Padding(5);

        gridPanel.Controls.Add(dgvUsers);

        // Bottom Panel - Buttons
        var bottomPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 80,
            BackColor = Color.White,
            Padding = new Padding(20, 15, 20, 15)
        };

        // Add top border
        bottomPanel.Paint += (s, e) =>
        {
            e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 220, 220), 1),
                0, 0, bottomPanel.Width, 0);
        };

        int buttonWidth = 160;
        int buttonHeight = 45;
        int spacing = 15;
        int startX = 20;

        btnAdd = CreateStyledButton("➕ إضافة مستخدم", startX, 15, buttonWidth, buttonHeight, ColorScheme.Success);
        btnAdd.Click += BtnAdd_Click;

        btnEdit = CreateStyledButton("✏️ تعديل", startX + (buttonWidth + spacing), 15, buttonWidth, buttonHeight, ColorScheme.Warning);
        btnEdit.Click += BtnEdit_Click;

        btnChangePassword = CreateStyledButton("🔑 تغيير كلمة المرور", startX + (buttonWidth + spacing) * 2, 15, buttonWidth + 30, buttonHeight, ColorScheme.Info);
        btnChangePassword.Click += BtnChangePassword_Click;

        btnDelete = CreateStyledButton("🗑️ حذف", startX + (buttonWidth + spacing) * 3 + 30, 15, buttonWidth, buttonHeight, ColorScheme.Danger);
        btnDelete.Click += BtnDelete_Click;

        btnRefresh = CreateStyledButton("🔄 تحديث", startX + (buttonWidth + spacing) * 4 + 30, 15, buttonWidth, buttonHeight, ColorScheme.Primary);
        btnRefresh.Click += (s, e) => LoadData();

        bottomPanel.Controls.AddRange(new Control[] { 
            btnAdd, btnEdit, btnChangePassword, btnDelete, btnRefresh 
        });

        // Add controls to form
        this.Controls.Add(gridPanel);
        this.Controls.Add(topPanel);
        this.Controls.Add(bottomPanel);

        LoadRoles();
    }

    private Button CreateStyledButton(string text, int x, int y, int width, int height, Color backColor)
    {
        var button = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleCenter
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.BorderColor = backColor;
        
        // Hover effect
        button.MouseEnter += (s, e) =>
        {
            button.BackColor = ControlPaint.Light(backColor, 0.1f);
        };
        button.MouseLeave += (s, e) =>
        {
            button.BackColor = backColor;
        };

        return button;
    }

    private void LoadRoles()
    {
        var roles = _context.Roles.OrderBy(r => r.RoleName).ToList();
        cmbRoleFilter.Items.Clear();
        cmbRoleFilter.Items.Add("الكل");
        foreach (var role in roles)
        {
            cmbRoleFilter.Items.Add(role.RoleName);
        }
        cmbRoleFilter.SelectedIndex = 0;
    }

    private void LoadData()
    {
        var query = _context.Users
            .Include(u => u.Role)
            .AsQueryable();

        // Filter by search
        if (!string.IsNullOrWhiteSpace(txtSearch.Text))
        {
            var searchTerm = txtSearch.Text.Trim().ToLower();
            query = query.Where(u => 
                u.Username.ToLower().Contains(searchTerm) ||
                u.FullName.ToLower().Contains(searchTerm) ||
                (u.Email != null && u.Email.ToLower().Contains(searchTerm)) ||
                (u.Phone != null && u.Phone.Contains(searchTerm))
            );
        }

        // Filter by role
        if (cmbRoleFilter.SelectedIndex > 0 && cmbRoleFilter.SelectedItem != null)
        {
            var roleName = cmbRoleFilter.SelectedItem.ToString();
            query = query.Where(u => u.Role != null && u.Role.RoleName == roleName);
        }

        // Filter by active status
        if (!chkShowInactive.Checked)
        {
            query = query.Where(u => u.IsActive);
        }

        var users = query
            .OrderBy(u => u.Username)
            .Select(u => new
            {
                u.UserId,
                u.Username,
                u.FullName,
                u.Email,
                u.Phone,
                RoleName = u.Role != null ? u.Role.RoleName : "بدون دور",
                u.IsActive,
                u.CreatedAt
            })
            .ToList();

        dgvUsers.DataSource = users;
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        var form = new AddEditUserForm(_context);
        if (form.ShowDialog() == DialogResult.OK)
        {
            LoadData();
            MessageBox.Show("تم إضافة المستخدم بنجاح", "نجاح", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dgvUsers.SelectedRows.Count == 0)
        {
            MessageBox.Show("الرجاء اختيار مستخدم للتعديل", "تنبيه", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var userId = (int)dgvUsers.SelectedRows[0].Cells["UserId"].Value;
        var form = new AddEditUserForm(_context, userId);
        if (form.ShowDialog() == DialogResult.OK)
        {
            LoadData();
            MessageBox.Show("تم تعديل المستخدم بنجاح", "نجاح", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void BtnChangePassword_Click(object? sender, EventArgs e)
    {
        if (dgvUsers.SelectedRows.Count == 0)
        {
            MessageBox.Show("الرجاء اختيار مستخدم لتغيير كلمة المرور", "تنبيه", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var userId = (int)dgvUsers.SelectedRows[0].Cells["UserId"].Value;
        var username = dgvUsers.SelectedRows[0].Cells["Username"].Value.ToString();
        
        var form = new ChangePasswordForm(_context, userId, username!);
        if (form.ShowDialog() == DialogResult.OK)
        {
            MessageBox.Show("تم تغيير كلمة المرور بنجاح", "نجاح", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dgvUsers.SelectedRows.Count == 0)
        {
            MessageBox.Show("الرجاء اختيار مستخدم للحذف", "تنبيه", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var userId = (int)dgvUsers.SelectedRows[0].Cells["UserId"].Value;
        var username = dgvUsers.SelectedRows[0].Cells["Username"].Value.ToString();

        var result = MessageBox.Show(
            $"هل أنت متأكد من حذف المستخدم '{username}'؟\n\nملاحظة: سيتم تعطيل المستخدم وليس حذفه نهائياً", 
            "تأكيد الحذف",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );

        if (result == DialogResult.Yes)
        {
            try
            {
                var user = _context.Users.Find(userId);
                if (user != null)
                {
                    user.IsActive = false;
                    user.UpdatedAt = DateTime.UtcNow;
                    _context.SaveChanges();
                    LoadData();
                    MessageBox.Show("تم تعطيل المستخدم بنجاح", "نجاح", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء حذف المستخدم: {ex.Message}", 
                    "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
