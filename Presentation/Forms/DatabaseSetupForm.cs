using GraceWay.AccountingSystem.Infrastructure.Configuration;
using System.Drawing.Drawing2D;

namespace GraceWay.AccountingSystem.Presentation.Forms;

/// <summary>
/// شاشة إعداد الاتصال بقاعدة البيانات - تظهر عند أول تشغيل أو عند فشل الاتصال
/// </summary>
public class DatabaseSetupForm : Form
{
    private TextBox txtHost = null!;
    private TextBox txtPort = null!;
    private TextBox txtDatabase = null!;
    private TextBox txtUsername = null!;
    private TextBox txtPassword = null!;
    private Button btnTest = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;
    private Label lblStatus = null!;
    private Panel pnlStatus = null!;
    private bool _connectionTested = false;

    public DatabaseSetupForm()
    {
        InitializeComponent();
        LoadExistingConfig();
    }

    private void InitializeComponent()
    {
        this.Text = "إعداد الاتصال بقاعدة البيانات - GraceWay";
        this.Size = new Size(520, 560);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = Color.FromArgb(248, 250, 252);
        this.Font = new Font("Cairo", 10F);

        if (Program.AppIcon != null) this.Icon = Program.AppIcon;

        BuildUI();
    }

    private void BuildUI()
    {
        // Header
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 90,
            BackColor = Color.FromArgb(37, 99, 235)
        };

        var lblTitle = new Label
        {
            Text = "⚙️ إعداد قاعدة البيانات",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 18),
            BackColor = Color.Transparent
        };
        var lblSubtitle = new Label
        {
            Text = "أدخل بيانات الاتصال بـ PostgreSQL",
            Font = new Font("Cairo", 10F),
            ForeColor = Color.FromArgb(200, 225, 255),
            AutoSize = true,
            Location = new Point(20, 55),
            BackColor = Color.Transparent
        };
        header.Controls.AddRange(new Control[] { lblTitle, lblSubtitle });
        this.Controls.Add(header);

        // Form body
        var body = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(30, 20, 30, 20),
            BackColor = Color.FromArgb(248, 250, 252)
        };

        int y = 10;

        // Host
        body.Controls.Add(MakeLabel("🖥️  عنوان السيرفر (Host)", y));
        txtHost = MakeTextBox(y + 28); txtHost.Text = "localhost";
        body.Controls.Add(txtHost);
        y += 75;

        // Port
        body.Controls.Add(MakeLabel("🔌  رقم المنفذ (Port)", y));
        txtPort = MakeTextBox(y + 28); txtPort.Text = "5432"; txtPort.Width = 120;
        body.Controls.Add(txtPort);
        y += 75;

        // Database
        body.Controls.Add(MakeLabel("🗄️  اسم قاعدة البيانات", y));
        txtDatabase = MakeTextBox(y + 28); txtDatabase.Text = "graceway_accounting";
        body.Controls.Add(txtDatabase);
        y += 75;

        // Username
        body.Controls.Add(MakeLabel("👤  اسم المستخدم", y));
        txtUsername = MakeTextBox(y + 28); txtUsername.Text = "postgres";
        body.Controls.Add(txtUsername);
        y += 75;

        // Password
        body.Controls.Add(MakeLabel("🔑  كلمة المرور", y));
        txtPassword = MakeTextBox(y + 28);
        txtPassword.UseSystemPasswordChar = true;
        body.Controls.Add(txtPassword);
        y += 75;

        // Status Panel
        pnlStatus = new Panel
        {
            Location = new Point(0, y),
            Size = new Size(460, 40),
            Visible = false,
            BorderStyle = BorderStyle.None
        };
        lblStatus = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = false
        };
        pnlStatus.Controls.Add(lblStatus);
        body.Controls.Add(pnlStatus);
        y += 50;

        // Buttons
        btnTest = new Button
        {
            Text = "🔗  اختبار الاتصال",
            Location = new Point(0, y),
            Size = new Size(220, 42),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnTest.FlatAppearance.BorderSize = 0;
        btnTest.Click += BtnTest_Click;

        btnSave = new Button
        {
            Text = "💾  حفظ والمتابعة",
            Location = new Point(240, y),
            Size = new Size(220, 42),
            BackColor = Color.FromArgb(34, 197, 94),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += BtnSave_Click;

        body.Controls.AddRange(new Control[] { btnTest, btnSave });

        // Cancel link
        btnCancel = new Button
        {
            Text = "إلغاء وخروج",
            Location = new Point(160, y + 55),
            Size = new Size(140, 30),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.FromArgb(100, 116, 139),
            BackColor = Color.Transparent,
            Font = new Font("Cairo", 9F),
            Cursor = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderSize = 0;
        btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
        body.Controls.Add(btnCancel);

        this.Controls.Add(body);
    }

    private Label MakeLabel(string text, int y) => new Label
    {
        Text = text,
        Location = new Point(0, y),
        AutoSize = true,
        ForeColor = Color.FromArgb(51, 65, 85),
        Font = new Font("Cairo", 10F, FontStyle.Bold)
    };

    private TextBox MakeTextBox(int y) => new TextBox
    {
        Location = new Point(0, y),
        Size = new Size(460, 32),
        Font = new Font("Cairo", 11F),
        BorderStyle = BorderStyle.FixedSingle,
        BackColor = Color.White
    };

    private void LoadExistingConfig()
    {
        var config = SecureConfigManager.LoadConfig();
        if (config != null)
        {
            txtHost.Text = config.Host;
            txtPort.Text = config.Port.ToString();
            txtDatabase.Text = config.DatabaseName;
            txtUsername.Text = config.Username;
            txtPassword.Text = config.Password;
        }
    }

    private async void BtnTest_Click(object? sender, EventArgs e)
    {
        btnTest.Enabled = false;
        btnTest.Text = "⏳  جاري الاختبار...";
        ShowStatus("جاري الاتصال بقاعدة البيانات...", Color.FromArgb(59, 130, 246), Color.White);

        try
        {
            var config = GetFormConfig();
            var connStr = SecureConfigManager.BuildConnectionString(config);

            await Task.Run(() =>
            {
                using var conn = new Npgsql.NpgsqlConnection(connStr);
                conn.Open();
                conn.Close();
            });

            ShowStatus("✅  الاتصال ناجح! يمكنك الآن الحفظ.", Color.FromArgb(34, 197, 94), Color.White);
            btnSave.Enabled = true;
            _connectionTested = true;
        }
        catch (Exception ex)
        {
            var msg = ex.Message.Length > 80 ? ex.Message.Substring(0, 80) + "..." : ex.Message;
            ShowStatus($"❌  فشل الاتصال: {msg}", Color.FromArgb(239, 68, 68), Color.White);
            btnSave.Enabled = false;
        }
        finally
        {
            btnTest.Text = "🔗  اختبار الاتصال";
            btnTest.Enabled = true;
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (!_connectionTested)
        {
            ShowStatus("⚠️  اختبر الاتصال أولاً قبل الحفظ", Color.FromArgb(245, 158, 11), Color.White);
            return;
        }

        try
        {
            var config = GetFormConfig();
            SecureConfigManager.SaveConfig(config);
            AppConfiguration.Instance.RefreshConnectionString();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            ShowStatus($"❌  خطأ في الحفظ: {ex.Message}", Color.FromArgb(239, 68, 68), Color.White);
        }
    }

    private DatabaseConfig GetFormConfig()
    {
        if (!int.TryParse(txtPort.Text.Trim(), out int port))
            port = 5432;

        return new DatabaseConfig
        {
            Host = txtHost.Text.Trim(),
            Port = port,
            DatabaseName = txtDatabase.Text.Trim(),
            Username = txtUsername.Text.Trim(),
            Password = txtPassword.Text
        };
    }

    private void ShowStatus(string message, Color backColor, Color foreColor)
    {
        if (InvokeRequired)
        {
            Invoke(() => ShowStatus(message, backColor, foreColor));
            return;
        }
        lblStatus.Text = message;
        pnlStatus.BackColor = backColor;
        lblStatus.ForeColor = foreColor;
        pnlStatus.Visible = true;
    }
}
