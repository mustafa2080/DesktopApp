using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace GraceWay.AccountingSystem.Presentation.Forms.Admin;

public partial class AddEditUserForm : Form
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly int? _userId;

    // بيانات المستخدم بعد الحفظ — يستخدمها UserManagementForm للتحديث الفوري
    public int      SavedUserId   { get; private set; }
    public string   SavedUsername { get; private set; } = "";
    public string   SavedFullName { get; private set; } = "";
    public string?  SavedEmail    { get; private set; }
    public string?  SavedPhone    { get; private set; }
    public string   SavedRoleName { get; private set; } = "";
    public bool     SavedIsActive { get; private set; }

    private TextBox  txtUsername        = null!;
    private TextBox  txtPassword        = null!;
    private TextBox  txtConfirmPassword = null!;
    private TextBox  txtFullName        = null!;
    private TextBox  txtEmail           = null!;
    private TextBox  txtPhone           = null!;
    private ComboBox cmbRole            = null!;
    private CheckBox chkIsActive        = null!;
    private Button   btnSave            = null!;
    private Button   btnCancel          = null!;

    public AddEditUserForm(IDbContextFactory<AppDbContext> dbFactory, int? userId = null)
    {
        _dbFactory = dbFactory;
        _userId    = userId;
        InitializeComponent();
        LoadRoles();
        if (_userId.HasValue) LoadUser();
    }

    private void InitializeComponent()
    {
        Text              = _userId.HasValue ? "تعديل مستخدم" : "إضافة مستخدم جديد";
        Size              = new Size(580, 620);
        StartPosition     = FormStartPosition.CenterParent;
        FormBorderStyle   = FormBorderStyle.FixedDialog;
        MaximizeBox       = false;
        MinimizeBox       = false;
        RightToLeft       = RightToLeft.Yes;
        RightToLeftLayout = true;
        BackColor         = Color.White;

        var main  = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30), BackColor = Color.White };
        int y     = 20;
        int ctrlW = 340;
        int lblW  = 155;
        int step  = 58;

        void Row(string label, Control ctrl)
        {
            main.Controls.Add(new Label
            {
                Text      = label,
                Location  = new Point(ctrlW + 40, y + 4),
                Size      = new Size(lblW, 24),
                Font      = new Font("Cairo", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
            });
            ctrl.Location = new Point(30, y);
            ctrl.Size     = new Size(ctrlW, 30);
            ctrl.Font     = new Font("Cairo", 9.5F);
            main.Controls.Add(ctrl);
            y += step;
        }

        txtUsername        = new TextBox();
        Row("اسم المستخدم: *", txtUsername);

        txtPassword        = new TextBox { PasswordChar = '●', UseSystemPasswordChar = true };
        Row(_userId.HasValue ? "كلمة المرور الجديدة:" : "كلمة المرور: *", txtPassword);

        txtConfirmPassword = new TextBox { PasswordChar = '●', UseSystemPasswordChar = true };
        Row(_userId.HasValue ? "تأكيد كلمة المرور:" : "تأكيد كلمة المرور: *", txtConfirmPassword);

        if (_userId.HasValue)
        {
            main.Controls.Add(new Label
            {
                Text      = "💡 اترك كلمة المرور فارغة إذا لا تريد تغييرها",
                Location  = new Point(30, y - 14),
                Size      = new Size(480, 20),
                Font      = new Font("Cairo", 8.5F, FontStyle.Italic),
                ForeColor = ColorScheme.Warning,
            });
            y += 14;
        }

        txtFullName = new TextBox();
        Row("الاسم الكامل: *", txtFullName);

        txtEmail = new TextBox();
        Row("البريد الإلكتروني:", txtEmail);

        txtPhone = new TextBox();
        Row("رقم الهاتف:", txtPhone);

        cmbRole = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        Row("الدور الوظيفي: *", cmbRole);

        chkIsActive = new CheckBox
        {
            Text      = "المستخدم نشط",
            Location  = new Point(30, y),
            AutoSize  = true,
            Font      = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80),
            Checked   = true,
        };
        main.Controls.Add(chkIsActive);
        y += 50;

        btnSave = new Button
        {
            Text      = "💾 حفظ",
            Location  = new Point(210, y),
            Size      = new Size(160, 44),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Cairo", 11F, FontStyle.Bold),
            Cursor    = Cursors.Hand,
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += BtnSave_Click;
        main.Controls.Add(btnSave);

        btnCancel = new Button
        {
            Text      = "إلغاء",
            Location  = new Point(30, y),
            Size      = new Size(150, 44),
            BackColor = ColorScheme.Danger,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Cairo", 11F, FontStyle.Bold),
            Cursor    = Cursors.Hand,
        };
        btnCancel.FlatAppearance.BorderSize = 0;
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        main.Controls.Add(btnCancel);

        Controls.Add(main);
        Height = y + 120;
    }

    // ── تحميل الأدوار بـ fresh context ─────────────────
    private void LoadRoles()
    {
        using var db = _dbFactory.CreateDbContext();
        var roles = db.Roles
                      .OrderBy(r => r.RoleName)
                      .Select(r => new { r.RoleId, r.RoleName })
                      .ToList();

        cmbRole.DataSource    = null;
        cmbRole.DisplayMember = "RoleName";
        cmbRole.ValueMember   = "RoleId";
        cmbRole.DataSource    = roles;
    }

    // ── تحميل بيانات المستخدم ──────────────────────────
    private void LoadUser()
    {
        using var db = _dbFactory.CreateDbContext();
        var user = db.Users.AsNoTracking()
                     .FirstOrDefault(u => u.UserId == _userId!.Value);
        if (user == null) return;

        txtUsername.Text    = user.Username;
        txtFullName.Text    = user.FullName;
        txtEmail.Text       = user.Email  ?? string.Empty;
        txtPhone.Text       = user.Phone  ?? string.Empty;
        chkIsActive.Checked = user.IsActive;

        if (user.RoleId.HasValue)
            cmbRole.SelectedValue = user.RoleId.Value;
    }

    // ── حفظ ─────────────────────────────────────────────
    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtUsername.Text))
        { Warn("الرجاء إدخال اسم المستخدم", txtUsername); return; }

        if (string.IsNullOrWhiteSpace(txtFullName.Text))
        { Warn("الرجاء إدخال الاسم الكامل", txtFullName); return; }

        if (cmbRole.SelectedValue == null)
        { Warn("الرجاء اختيار الدور الوظيفي", cmbRole); return; }

        bool isNew      = !_userId.HasValue;
        bool changePass = !string.IsNullOrWhiteSpace(txtPassword.Text);

        if (isNew || changePass)
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            { Warn("الرجاء إدخال كلمة المرور", txtPassword); return; }

            if (txtPassword.Text.Length < 6)
            { Warn("كلمة المرور يجب أن تكون 6 أحرف على الأقل", txtPassword); return; }

            if (txtPassword.Text != txtConfirmPassword.Text)
            { Warn("كلمة المرور وتأكيدها غير متطابقين", txtConfirmPassword); return; }
        }

        try
        {
            using var db = _dbFactory.CreateDbContext();

            // استخراج القيمة قبل الـ query لتجنب مشكلة Nullable comparison في EF
            int currentId = _userId ?? -1;

            bool duplicate = db.Users.Any(u =>
                u.Username.ToLower() == txtUsername.Text.Trim().ToLower() &&
                u.UserId != currentId);

            if (duplicate)
            { Warn("اسم المستخدم موجود بالفعل، الرجاء اختيار اسم آخر", txtUsername); return; }

            int roleId = Convert.ToInt32(cmbRole.SelectedValue);

            if (isNew)
            {
                db.Users.Add(new User
                {
                    Username     = txtUsername.Text.Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(txtPassword.Text),
                    FullName     = txtFullName.Text.Trim(),
                    Email        = Nullify(txtEmail.Text),
                    Phone        = Nullify(txtPhone.Text),
                    RoleId       = roleId,
                    IsActive     = chkIsActive.Checked,
                    CreatedAt    = DateTime.UtcNow,
                    UpdatedAt    = DateTime.UtcNow,
                });
            }
            else
            {
                // استرجاع المستخدم بنفس الـ context لضمان الـ tracking
                var user = db.Users.Find(_userId!.Value);
                if (user == null)
                { ShowError("لم يتم العثور على المستخدم"); return; }

                user.Username  = txtUsername.Text.Trim();
                user.FullName  = txtFullName.Text.Trim();
                user.Email     = Nullify(txtEmail.Text);
                user.Phone     = Nullify(txtPhone.Text);
                user.RoleId    = roleId;
                user.IsActive  = chkIsActive.Checked;
                user.UpdatedAt = DateTime.UtcNow;

                if (changePass)
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(txtPassword.Text);
            }

            db.SaveChanges();
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            ShowError($"حدث خطأ أثناء الحفظ:\n{ex.InnerException?.Message ?? ex.Message}");
        }
    }

    private static string? Nullify(string s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private void Warn(string msg, Control? focus = null)
    {
        MessageBox.Show(msg, "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        focus?.Focus();
    }

    private void ShowError(string msg) =>
        MessageBox.Show(msg, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
