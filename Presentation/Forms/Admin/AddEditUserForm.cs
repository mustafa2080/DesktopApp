using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BCrypt.Net;

namespace GraceWay.AccountingSystem.Presentation.Forms.Admin;

public partial class AddEditUserForm : Form
{
    private readonly AppDbContext _context;
    private readonly int? _userId;
    private User? _user;

    private TextBox txtUsername;
    private TextBox txtPassword;
    private TextBox txtConfirmPassword;
    private TextBox txtFullName;
    private TextBox txtEmail;
    private TextBox txtPhone;
    private ComboBox cmbRole;
    private CheckBox chkIsActive;
    private Button btnSave;
    private Button btnCancel;
    private Label lblPasswordNote;

    public AddEditUserForm(AppDbContext context, int? userId = null)
    {
        _context = context;
        _userId = userId;
        InitializeComponent();
        LoadRoles();
        
        if (_userId.HasValue)
        {
            LoadUser();
        }
    }

    private void InitializeComponent()
    {
        this.Text = _userId.HasValue ? "تعديل مستخدم" : "إضافة مستخدم جديد";
        this.Size = new Size(600, 650);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;

        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(30),
            BackColor = Color.White
        };

        int yPos = 20;
        int labelWidth = 150;
        int controlWidth = 350;
        int spacing = 60;

        // Username
        var lblUsername = new Label
        {
            Text = "اسم المستخدم: *",
            Location = new Point(400, yPos),
            Size = new Size(labelWidth, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        txtUsername = new TextBox
        {
            Location = new Point(30, yPos),
            Size = new Size(controlWidth, 30),
            Font = new Font("Segoe UI", 10F)
        };

        yPos += spacing;

        // Password
        var lblPassword = new Label
        {
            Text = _userId.HasValue ? "كلمة المرور الجديدة:" : "كلمة المرور: *",
            Location = new Point(400, yPos),
            Size = new Size(labelWidth, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        txtPassword = new TextBox
        {
            Location = new Point(30, yPos),
            Size = new Size(controlWidth, 30),
            Font = new Font("Segoe UI", 10F),
            PasswordChar = '●',
            UseSystemPasswordChar = true
        };

        yPos += spacing;

        // Confirm Password
        var lblConfirmPassword = new Label
        {
            Text = _userId.HasValue ? "تأكيد كلمة المرور:" : "تأكيد كلمة المرور: *",
            Location = new Point(400, yPos),
            Size = new Size(labelWidth, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        txtConfirmPassword = new TextBox
        {
            Location = new Point(30, yPos),
            Size = new Size(controlWidth, 30),
            Font = new Font("Segoe UI", 10F),
            PasswordChar = '●',
            UseSystemPasswordChar = true
        };

        yPos += spacing;

        // Password Note (for edit mode)
        if (_userId.HasValue)
        {
            lblPasswordNote = new Label
            {
                Text = "ملاحظة: اترك كلمة المرور فارغة إذا كنت لا تريد تغييرها",
                Location = new Point(30, yPos),
                Size = new Size(520, 30),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = ColorScheme.Warning,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblPasswordNote);
            yPos += 40;
        }

        // Full Name
        var lblFullName = new Label
        {
            Text = "الاسم الكامل: *",
            Location = new Point(400, yPos),
            Size = new Size(labelWidth, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        txtFullName = new TextBox
        {
            Location = new Point(30, yPos),
            Size = new Size(controlWidth, 30),
            Font = new Font("Segoe UI", 10F)
        };

        yPos += spacing;

        // Email
        var lblEmail = new Label
        {
            Text = "البريد الإلكتروني:",
            Location = new Point(400, yPos),
            Size = new Size(labelWidth, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        txtEmail = new TextBox
        {
            Location = new Point(30, yPos),
            Size = new Size(controlWidth, 30),
            Font = new Font("Segoe UI", 10F)
        };

        yPos += spacing;

        // Phone
        var lblPhone = new Label
        {
            Text = "رقم الهاتف:",
            Location = new Point(400, yPos),
            Size = new Size(labelWidth, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        txtPhone = new TextBox
        {
            Location = new Point(30, yPos),
            Size = new Size(controlWidth, 30),
            Font = new Font("Segoe UI", 10F)
        };

        yPos += spacing;

        // Role
        var lblRole = new Label
        {
            Text = "الدور الوظيفي: *",
            Location = new Point(400, yPos),
            Size = new Size(labelWidth, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        cmbRole = new ComboBox
        {
            Location = new Point(30, yPos),
            Size = new Size(controlWidth, 30),
            Font = new Font("Segoe UI", 10F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        yPos += spacing;

        // Is Active
        chkIsActive = new CheckBox
        {
            Text = "المستخدم نشط",
            Location = new Point(30, yPos),
            Size = new Size(controlWidth, 30),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Checked = true
        };

        yPos += 60;

        // Buttons
        btnSave = new Button
        {
            Text = "💾 حفظ",
            Location = new Point(230, yPos),
            Size = new Size(150, 45),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += BtnSave_Click;

        btnCancel = new Button
        {
            Text = "❌ إلغاء",
            Location = new Point(30, yPos),
            Size = new Size(150, 45),
            BackColor = ColorScheme.Danger,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderSize = 0;
        btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

        // Add all controls
        mainPanel.Controls.AddRange(new Control[]
        {
            lblUsername, txtUsername,
            lblPassword, txtPassword,
            lblConfirmPassword, txtConfirmPassword,
            lblFullName, txtFullName,
            lblEmail, txtEmail,
            lblPhone, txtPhone,
            lblRole, cmbRole,
            chkIsActive,
            btnSave, btnCancel
        });

        this.Controls.Add(mainPanel);
    }

    private void LoadRoles()
    {
        var roles = _context.Roles.OrderBy(r => r.RoleName).ToList();
        cmbRole.DisplayMember = "RoleName";
        cmbRole.ValueMember = "RoleId";
        cmbRole.DataSource = roles;
    }

    private void LoadUser()
    {
        _user = _context.Users
            .Include(u => u.Role)
            .FirstOrDefault(u => u.UserId == _userId);

        if (_user != null)
        {
            txtUsername.Text = _user.Username;
            txtFullName.Text = _user.FullName;
            txtEmail.Text = _user.Email ?? "";
            txtPhone.Text = _user.Phone ?? "";
            chkIsActive.Checked = _user.IsActive;

            if (_user.RoleId.HasValue)
            {
                cmbRole.SelectedValue = _user.RoleId.Value;
            }
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(txtUsername.Text))
        {
            MessageBox.Show("الرجاء إدخال اسم المستخدم", "تنبيه", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtUsername.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(txtFullName.Text))
        {
            MessageBox.Show("الرجاء إدخال الاسم الكامل", "تنبيه", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtFullName.Focus();
            return;
        }

        if (cmbRole.SelectedValue == null)
        {
            MessageBox.Show("الرجاء اختيار الدور الوظيفي", "تنبيه", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            cmbRole.Focus();
            return;
        }

        // Password validation (for new user or if password is being changed)
        if (!_userId.HasValue || !string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("الرجاء إدخال كلمة المرور", "تنبيه", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            if (txtPassword.Text.Length < 6)
            {
                MessageBox.Show("كلمة المرور يجب أن تكون 6 أحرف على الأقل", "تنبيه", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("كلمة المرور وتأكيد كلمة المرور غير متطابقين", "تنبيه", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Focus();
                return;
            }
        }

        // Check username uniqueness
        var existingUser = _context.Users.FirstOrDefault(u => 
            u.Username.ToLower() == txtUsername.Text.Trim().ToLower() && 
            u.UserId != _userId);

        if (existingUser != null)
        {
            MessageBox.Show("اسم المستخدم موجود بالفعل، الرجاء اختيار اسم آخر", "تنبيه", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtUsername.Focus();
            return;
        }

        try
        {
            if (_userId.HasValue)
            {
                // Update existing user
                if (_user != null)
                {
                    _user.Username = txtUsername.Text.Trim();
                    _user.FullName = txtFullName.Text.Trim();
                    _user.Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
                    _user.Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim();
                    _user.RoleId = (int)cmbRole.SelectedValue;
                    _user.IsActive = chkIsActive.Checked;
                    _user.UpdatedAt = DateTime.UtcNow;

                    // Update password if provided
                    if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                    {
                        _user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(txtPassword.Text);
                    }
                }
            }
            else
            {
                // Create new user
                var newUser = new User
                {
                    Username = txtUsername.Text.Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(txtPassword.Text),
                    FullName = txtFullName.Text.Trim(),
                    Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                    Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim(),
                    RoleId = (int)cmbRole.SelectedValue,
                    IsActive = chkIsActive.Checked,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
            }

            _context.SaveChanges();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"حدث خطأ أثناء حفظ المستخدم: {ex.Message}", 
                "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
