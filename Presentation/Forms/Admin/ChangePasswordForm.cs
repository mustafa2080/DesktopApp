using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Drawing;
using System.Windows.Forms;
using BCrypt.Net;

namespace GraceWay.AccountingSystem.Presentation.Forms.Admin;

public partial class ChangePasswordForm : Form
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly int _userId;
    private readonly string _username;

    private TextBox txtNewPassword = null!;
    private TextBox txtConfirmPassword = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;
    private CheckBox chkShowPassword = null!;

    public ChangePasswordForm(IDbContextFactory<AppDbContext> dbFactory, int userId, string username)
    {
        _dbFactory = dbFactory;
        _userId    = userId;
        _username  = username;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "تغيير كلمة المرور";
        this.Size = new Size(500, 350);
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

        // Username Label
        var lblUser = new Label
        {
            Text = $"المستخدم: {_username}",
            Location = new Point(30, 20),
            Size = new Size(400, 30),
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            TextAlign = ContentAlignment.MiddleCenter
        };

        // New Password
        var lblNewPassword = new Label
        {
            Text = "كلمة المرور الجديدة: *",
            Location = new Point(280, 80),
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        txtNewPassword = new TextBox
        {
            Location = new Point(30, 80),
            Size = new Size(230, 30),
            Font = new Font("Segoe UI", 10F),
            PasswordChar = '●',
            UseSystemPasswordChar = true
        };

        // Confirm Password
        var lblConfirmPassword = new Label
        {
            Text = "تأكيد كلمة المرور: *",
            Location = new Point(280, 130),
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        txtConfirmPassword = new TextBox
        {
            Location = new Point(30, 130),
            Size = new Size(230, 30),
            Font = new Font("Segoe UI", 10F),
            PasswordChar = '●',
            UseSystemPasswordChar = true
        };

        // Show Password Checkbox
        chkShowPassword = new CheckBox
        {
            Text = "إظهار كلمة المرور",
            Location = new Point(30, 175),
            Size = new Size(200, 25),
            Font = new Font("Segoe UI", 9F)
        };
        chkShowPassword.CheckedChanged += (s, e) =>
        {
            if (chkShowPassword.Checked)
            {
                txtNewPassword.UseSystemPasswordChar = false;
                txtNewPassword.PasswordChar = '\0';
                txtConfirmPassword.UseSystemPasswordChar = false;
                txtConfirmPassword.PasswordChar = '\0';
            }
            else
            {
                txtNewPassword.UseSystemPasswordChar = true;
                txtNewPassword.PasswordChar = '●';
                txtConfirmPassword.UseSystemPasswordChar = true;
                txtConfirmPassword.PasswordChar = '●';
            }
        };

        // Buttons
        btnSave = new Button
        {
            Text = "💾 حفظ",
            Location = new Point(250, 230),
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
            Location = new Point(70, 230),
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
            lblUser,
            lblNewPassword, txtNewPassword,
            lblConfirmPassword, txtConfirmPassword,
            chkShowPassword,
            btnSave, btnCancel
        });

        this.Controls.Add(mainPanel);
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(txtNewPassword.Text))
        {
            MessageBox.Show("الرجاء إدخال كلمة المرور الجديدة", "تنبيه",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtNewPassword.Focus();
            return;
        }

        if (txtNewPassword.Text.Length < 6)
        {
            MessageBox.Show("كلمة المرور يجب أن تكون 6 أحرف على الأقل", "تنبيه",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtNewPassword.Focus();
            return;
        }

        if (txtNewPassword.Text != txtConfirmPassword.Text)
        {
            MessageBox.Show("كلمة المرور وتأكيد كلمة المرور غير متطابقين", "تنبيه",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtConfirmPassword.Focus();
            return;
        }

        try
        {
            using var db = _dbFactory.CreateDbContext();
            var user = db.Users.Find(_userId);
            if (user != null)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(txtNewPassword.Text);
                user.UpdatedAt    = DateTime.UtcNow;
                db.SaveChanges();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("لم يتم العثور على المستخدم", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"حدث خطأ أثناء تغيير كلمة المرور: {ex.Message}",
                "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
