using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Npgsql;

namespace GraceWay.AccountingSystem.Presentation.Forms;

/// <summary>
/// فورم "نسيت كلمة المرور" - يطلب باسورد الأدمن أولاً ثم يسمح بتغيير باسورد أي يوزر
/// </summary>
public class ForgotPasswordForm : Form
{
    private readonly AppDbContext _context;

    // ─── الخطوة الأولى: إدخال باسورد الأدمن ───
    private Panel _step1Panel = null!;
    private TextBox _txtAdminPassword = null!;
    private Button _btnVerify = null!;
    private Label _lblError = null!;

    // ─── الخطوة الثانية: اختيار اليوزر وتغيير باسورده ───
    private Panel _step2Panel = null!;
    private ComboBox _cmbUsers = null!;
    private TextBox _txtNewPassword = null!;
    private TextBox _txtConfirmPassword = null!;
    private CheckBox _chkShowPassword = null!;
    private Button _btnSave = null!;
    private Button _btnBack = null!;
    private Label _lblSelectedUser = null!;

    // باسورد الأدمن الثابت
    private const string AdminMasterPassword = "admin@2024";

    public ForgotPasswordForm(AppDbContext context)
    {
        _context = context;
        InitializeForm();
        BuildStep1();
        BuildStep2();
        ShowStep(1);
    }

    private void InitializeForm()
    {
        this.Text = "🔑 نسيت كلمة المرور";
        this.Size = new Size(520, 460);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = Color.White;
        this.Font = new Font("Cairo", 10F);
    }

    // ════════════════════════════════════════════
    // الخطوة الأولى: التحقق من باسورد الأدمن
    // ════════════════════════════════════════════
    private void BuildStep1()
    {
        _step1Panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(40)
        };

        // أيقونة + عنوان
        var lblIcon = new Label
        {
            Text = "🔒",
            Font = new Font("Segoe UI Emoji", 36F),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 80,
            BackColor = Color.Transparent
        };

        var lblTitle = new Label
        {
            Text = "التحقق من صلاحية الأدمن",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = Color.FromArgb(25, 118, 210),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 45,
            BackColor = Color.Transparent
        };

        var lblSub = new Label
        {
            Text = "أدخل كلمة مرور المسؤول للمتابعة",
            Font = new Font("Cairo", 10F),
            ForeColor = Color.Gray,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 35,
            BackColor = Color.Transparent
        };

        // حقل الباسورد
        var lblPass = new Label
        {
            Text = "كلمة مرور المسؤول:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Location = new Point(40, 200),
            Size = new Size(200, 28),
            BackColor = Color.Transparent
        };

        _txtAdminPassword = new TextBox
        {
            Location = new Point(40, 230),
            Size = new Size(420, 38),
            Font = new Font("Cairo", 11F),
            UseSystemPasswordChar = true,
            BorderStyle = BorderStyle.FixedSingle
        };
        _txtAdminPassword.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) VerifyAdminPassword();
        };

        // رسالة الخطأ
        _lblError = new Label
        {
            Text = "",
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(220, 53, 69),
            Location = new Point(40, 275),
            Size = new Size(420, 25),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent
        };

        // زر التحقق
        _btnVerify = new Button
        {
            Text = "التحقق والمتابعة ←",
            Location = new Point(40, 310),
            Size = new Size(420, 48),
            BackColor = Color.FromArgb(25, 118, 210),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnVerify.FlatAppearance.BorderSize = 0;
        _btnVerify.Click += (s, e) => VerifyAdminPassword();

        // تأثير hover
        _btnVerify.MouseEnter += (s, e) => _btnVerify.BackColor = Color.FromArgb(21, 101, 192);
        _btnVerify.MouseLeave += (s, e) => _btnVerify.BackColor = Color.FromArgb(25, 118, 210);

        // زر إلغاء
        var btnCancel = new Button
        {
            Text = "إلغاء",
            Location = new Point(40, 368),
            Size = new Size(420, 38),
            BackColor = Color.FromArgb(240, 242, 245),
            ForeColor = Color.FromArgb(80, 80, 80),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Cairo", 10F),
            Cursor = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderSize = 0;
        btnCancel.Click += (s, e) => this.Close();

        _step1Panel.Controls.AddRange(new Control[]
        {
            lblIcon, lblTitle, lblSub,
            lblPass, _txtAdminPassword, _lblError,
            _btnVerify, btnCancel
        });

        this.Controls.Add(_step1Panel);
    }

    // ════════════════════════════════════════════
    // الخطوة الثانية: اختيار اليوزر وتغيير الباسورد
    // ════════════════════════════════════════════
    private void BuildStep2()
    {
        _step2Panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(40)
        };

        // عنوان
        var lblTitle = new Label
        {
            Text = "🔑 تغيير كلمة مرور مستخدم",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = Color.FromArgb(25, 118, 210),
            Location = new Point(40, 20),
            Size = new Size(420, 40),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent
        };

        // اختيار المستخدم
        var lblUser = new Label
        {
            Text = "اختر المستخدم:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Location = new Point(40, 75),
            Size = new Size(420, 28),
            BackColor = Color.Transparent
        };

        _cmbUsers = new ComboBox
        {
            Location = new Point(40, 105),
            Size = new Size(420, 38),
            Font = new Font("Cairo", 11F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cmbUsers.SelectedIndexChanged += (s, e) => UpdateSelectedUserLabel();

        _lblSelectedUser = new Label
        {
            Text = "",
            Font = new Font("Cairo", 9F, FontStyle.Italic),
            ForeColor = Color.Gray,
            Location = new Point(40, 148),
            Size = new Size(420, 22),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent
        };

        // كلمة المرور الجديدة
        var lblNewPass = new Label
        {
            Text = "كلمة المرور الجديدة:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Location = new Point(40, 180),
            Size = new Size(420, 28),
            BackColor = Color.Transparent
        };

        _txtNewPassword = new TextBox
        {
            Location = new Point(40, 210),
            Size = new Size(420, 38),
            Font = new Font("Cairo", 11F),
            UseSystemPasswordChar = true,
            BorderStyle = BorderStyle.FixedSingle
        };

        // تأكيد كلمة المرور
        var lblConfirm = new Label
        {
            Text = "تأكيد كلمة المرور:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Location = new Point(40, 258),
            Size = new Size(420, 28),
            BackColor = Color.Transparent
        };

        _txtConfirmPassword = new TextBox
        {
            Location = new Point(40, 288),
            Size = new Size(420, 38),
            Font = new Font("Cairo", 11F),
            UseSystemPasswordChar = true,
            BorderStyle = BorderStyle.FixedSingle
        };

        // إظهار/إخفاء
        _chkShowPassword = new CheckBox
        {
            Text = "إظهار كلمة المرور",
            Font = new Font("Cairo", 9F),
            Location = new Point(40, 335),
            Size = new Size(200, 25),
            BackColor = Color.Transparent
        };
        _chkShowPassword.CheckedChanged += (s, e) =>
        {
            bool show = _chkShowPassword.Checked;
            _txtNewPassword.UseSystemPasswordChar = !show;
            _txtConfirmPassword.UseSystemPasswordChar = !show;
        };

        // أزرار
        _btnSave = new Button
        {
            Text = "💾 حفظ كلمة المرور",
            Location = new Point(220, 375),
            Size = new Size(240, 48),
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnSave.FlatAppearance.BorderSize = 0;
        _btnSave.Click += SavePassword_Click;
        _btnSave.MouseEnter += (s, e) => _btnSave.BackColor = Color.FromArgb(39, 174, 96);
        _btnSave.MouseLeave += (s, e) => _btnSave.BackColor = Color.FromArgb(46, 204, 113);

        _btnBack = new Button
        {
            Text = "→ رجوع",
            Location = new Point(40, 375),
            Size = new Size(165, 48),
            BackColor = Color.FromArgb(240, 242, 245),
            ForeColor = Color.FromArgb(80, 80, 80),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnBack.FlatAppearance.BorderSize = 0;
        _btnBack.Click += (s, e) =>
        {
            _txtAdminPassword.Clear();
            _lblError.Text = "";
            ShowStep(1);
        };

        _step2Panel.Controls.AddRange(new Control[]
        {
            lblTitle, lblUser, _cmbUsers, _lblSelectedUser,
            lblNewPass, _txtNewPassword,
            lblConfirm, _txtConfirmPassword,
            _chkShowPassword, _btnSave, _btnBack
        });

        this.Controls.Add(_step2Panel);
    }

    // ════════════════════════════════════════════
    // منطق التحقق والحفظ
    // ════════════════════════════════════════════
    private void VerifyAdminPassword()
    {
        string entered = _txtAdminPassword.Text;

        if (string.IsNullOrWhiteSpace(entered))
        {
            _lblError.Text = "الرجاء إدخال كلمة مرور المسؤول";
            _txtAdminPassword.Focus();
            return;
        }

        if (entered != AdminMasterPassword)
        {
            _lblError.Text = "❌ كلمة مرور المسؤول غير صحيحة";
            _txtAdminPassword.SelectAll();
            _txtAdminPassword.Focus();
            return;
        }

        // صح - انتقل للخطوة الثانية
        LoadUsers();
        _txtNewPassword.Clear();
        _txtConfirmPassword.Clear();
        ShowStep(2);
    }

    private void LoadUsers()
    {
        try
        {
            var connStr = _context.Database.GetConnectionString()!;
            using var conn = new NpgsqlConnection(connStr);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT userid, fullname, username FROM users WHERE isactive = true ORDER BY fullname";
            using var reader = cmd.ExecuteReader();

            var users = new List<dynamic>();
            while (reader.Read())
            {
                users.Add(new
                {
                    UserId   = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Username = reader.GetString(2),
                    Display  = $"{reader.GetString(1)} ({reader.GetString(2)})"
                });
            }
            conn.Close();

            _cmbUsers.DataSource = users;
            _cmbUsers.DisplayMember = "Display";
            _cmbUsers.ValueMember = "UserId";

            if (users.Any())
                _cmbUsers.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل المستخدمين: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateSelectedUserLabel()
    {
        if (_cmbUsers.SelectedItem == null) return;
        dynamic item = _cmbUsers.SelectedItem;
        _lblSelectedUser.Text = $"سيتم تغيير كلمة مرور: {item.FullName}";
    }

    private void SavePassword_Click(object? sender, EventArgs e)
    {
        if (_cmbUsers.SelectedValue == null)
        {
            MessageBox.Show("الرجاء اختيار مستخدم", "تنبيه",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtNewPassword.Text))
        {
            MessageBox.Show("الرجاء إدخال كلمة المرور الجديدة", "تنبيه",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtNewPassword.Focus();
            return;
        }

        if (_txtNewPassword.Text.Length < 6)
        {
            MessageBox.Show("كلمة المرور يجب أن تكون 6 أحرف على الأقل", "تنبيه",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtNewPassword.Focus();
            return;
        }

        if (_txtNewPassword.Text != _txtConfirmPassword.Text)
        {
            MessageBox.Show("كلمة المرور وتأكيدها غير متطابقين", "تنبيه",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtConfirmPassword.Focus();
            return;
        }

        try
        {
            int userId = (int)_cmbUsers.SelectedValue;

            dynamic selectedItem = _cmbUsers.SelectedItem!;
            string fullName = selectedItem.FullName;

            var confirm = MessageBox.Show(
                $"هل تريد تغيير كلمة مرور المستخدم:\n\n{fullName}\n\nهذا الإجراء لا يمكن التراجع عنه.",
                "تأكيد تغيير كلمة المرور",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2,
                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading
            );

            if (confirm != DialogResult.Yes) return;

            string newHash = BCrypt.Net.BCrypt.HashPassword(_txtNewPassword.Text);

            // Npgsql مباشرة بـ parameterized query
            var connStr = _context.Database.GetConnectionString()!;
            using var conn = new NpgsqlConnection(connStr);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE users SET passwordhash = @hash, updatedat = @now WHERE userid = @id";
            cmd.Parameters.AddWithValue("hash", newHash);
            cmd.Parameters.AddWithValue("now", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("id", userId);
            int rows = cmd.ExecuteNonQuery();
            conn.Close();

            if (rows > 0)
            {
                MessageBox.Show(
                    $"✅ تم تغيير كلمة مرور المستخدم [{fullName}] بنجاح!",
                    "تم بنجاح",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading
                );
                this.Close();
            }
            else
            {
                MessageBox.Show($"لم يتم تحديث أي سجل. UserId = {userId}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ أثناء تغيير كلمة المرور:\n{ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ════════════════════════════════════════════
    // التنقل بين الخطوتين
    // ════════════════════════════════════════════
    private void ShowStep(int step)
    {
        _step1Panel.Visible = step == 1;
        _step2Panel.Visible = step == 2;

        if (step == 1)
        {
            this.Text = "🔑 نسيت كلمة المرور — التحقق";
            this.Height = 460;
            _txtAdminPassword.Focus();
        }
        else
        {
            this.Text = "🔑 نسيت كلمة المرور — تغيير كلمة المرور";
            this.Height = 490;
            if (_cmbUsers.Items.Count > 0)
                _txtNewPassword.Focus();
        }
    }
}
