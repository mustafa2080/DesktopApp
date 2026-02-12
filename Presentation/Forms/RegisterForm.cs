using System;
using System.Linq;
using System.Windows.Forms;
using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using GraceWay.AccountingSystem.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace GraceWay.AccountingSystem.Presentation.Forms
{
    public partial class RegisterForm : Form
    {
        private AppDbContext? _context;

        public RegisterForm()
        {
            InitializeComponent();
            this.Load += RegisterForm_Load;
        }

        private async void RegisterForm_Load(object? sender, EventArgs e)
        {
            try
            {
                // Initialize database context
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                var connectionString = AppConfiguration.Instance.GetConnectionString();
                optionsBuilder.UseNpgsql(connectionString);
                _context = new AppDbContext(optionsBuilder.Options);

                // Load roles into combobox
                await LoadRolesAsync();
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تهيئة النموذج:\n{ex.Message}");
                this.Close();
            }
        }

        private async Task LoadRolesAsync()
        {
            if (_context == null) return;

            try
            {
                var roles = await _context.Roles
                    .Select(r => new { r.RoleId, r.RoleName })
                    .ToListAsync();

                cmbRole.DisplayMember = "RoleName";
                cmbRole.ValueMember = "RoleId";
                cmbRole.DataSource = roles;

                if (roles.Any())
                {
                    cmbRole.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تحميل الصلاحيات:\n{ex.Message}");
            }
        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {
            if (_context == null)
            {
                ShowError("خطأ: الاتصال بقاعدة البيانات غير متوفر");
                return;
            }

            // Validate inputs
            if (!ValidateInputs())
            {
                return;
            }

            // Disable controls
            SetControlsEnabled(false);
            btnRegister.Text = "جاري التسجيل...";
            this.Cursor = Cursors.WaitCursor;

            try
            {
                // Check if username already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == txtUsername.Text.Trim());

                if (existingUser != null)
                {
                    ShowError("اسم المستخدم موجود بالفعل. الرجاء اختيار اسم آخر");
                    txtUsername.Focus();
                    return;
                }

                // Create new user
                var newUser = new User
                {
                    Username = txtUsername.Text.Trim(),
                    FullName = txtFullName.Text.Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(txtPassword.Text),
                    RoleId = (int)cmbRole.SelectedValue!,
                    IsActive = chkIsActive.Checked,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                ShowSuccess($"تم تسجيل المستخدم '{newUser.FullName}' بنجاح!");
                
                // Clear form
                ClearForm();
                
                // Ask if user wants to add another
                var result = MessageBox.Show(
                    "هل تريد إضافة مستخدم آخر؟",
                    "نجاح",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading
                );

                if (result == DialogResult.No)
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تسجيل المستخدم:\n{ex.Message}\n\nتفاصيل:\n{ex.InnerException?.Message}");
            }
            finally
            {
                SetControlsEnabled(true);
                btnRegister.Text = "تسجيل المستخدم";
                this.Cursor = Cursors.Default;
            }
        }

        private bool ValidateInputs()
        {
            // Username validation
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                ShowError("يرجى إدخال اسم المستخدم");
                txtUsername.Focus();
                return false;
            }

            if (txtUsername.Text.Length < 3)
            {
                ShowError("اسم المستخدم يجب أن يكون 3 أحرف على الأقل");
                txtUsername.Focus();
                return false;
            }

            // Full name validation
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                ShowError("يرجى إدخال الاسم الكامل");
                txtFullName.Focus();
                return false;
            }

            // Password validation
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                ShowError("يرجى إدخال كلمة المرور");
                txtPassword.Focus();
                return false;
            }

            if (txtPassword.Text.Length < 6)
            {
                ShowError("كلمة المرور يجب أن تكون 6 أحرف على الأقل");
                txtPassword.Focus();
                return false;
            }

            // Confirm password validation
            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                ShowError("كلمة المرور وتأكيد كلمة المرور غير متطابقتين");
                txtConfirmPassword.Focus();
                return false;
            }

            // Role validation
            if (cmbRole.SelectedValue == null)
            {
                ShowError("يرجى اختيار الصلاحية");
                cmbRole.Focus();
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            txtUsername.Clear();
            txtFullName.Clear();
            txtPassword.Clear();
            txtConfirmPassword.Clear();
            chkIsActive.Checked = true;
            if (cmbRole.Items.Count > 0)
            {
                cmbRole.SelectedIndex = 0;
            }
            txtUsername.Focus();
        }

        private void SetControlsEnabled(bool enabled)
        {
            txtUsername.Enabled = enabled;
            txtFullName.Enabled = enabled;
            txtPassword.Enabled = enabled;
            txtConfirmPassword.Enabled = enabled;
            cmbRole.Enabled = enabled;
            chkIsActive.Enabled = enabled;
            btnRegister.Enabled = enabled;
            btnCancel.Enabled = enabled;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ShowError(string message)
        {
            MessageBox.Show(
                message,
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading
            );
        }

        private void ShowSuccess(string message)
        {
            MessageBox.Show(
                message,
                "نجاح",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading
            );
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                _context?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
