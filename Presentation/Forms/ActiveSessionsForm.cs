using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using GraceWay.AccountingSystem.Infrastructure.Data;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class ActiveSessionsForm : Form
{
    private readonly System.Windows.Forms.Timer _refreshTimer;
    private DataGridView _gridSessions = null!;
    private Label _lblTotalSessions = null!;
    private Button _btnRefresh = null!;
    private Button _btnClose = null!;
    private Button _btnCleanup = null!;
    private Button _btnForceEnd = null!;

    public ActiveSessionsForm()
    {
        InitializeComponent();
        InitializeCustomComponents();
        
        _refreshTimer = new System.Windows.Forms.Timer();
        _refreshTimer.Interval = 5000; // Refresh every 5 seconds
        _refreshTimer.Tick += RefreshTimer_Tick;
        _refreshTimer.Start();
        
        LoadSessions();
    }

    private void InitializeCustomComponents()
    {
        this.Text = "الجلسات النشطة";
        this.Size = new Size(1100, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;

        // Top panel for stats
        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = ColorScheme.Primary
        };

        _lblTotalSessions = new Label
        {
            Text = "إجمالي الجلسات النشطة: 0",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 20)
        };
        topPanel.Controls.Add(_lblTotalSessions);

        // DataGridView for sessions
        _gridSessions = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            ColumnHeadersHeight = 40,
            RowTemplate = { Height = 35 }
        };

        _gridSessions.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn
            {
                Name = "SessionId",
                HeaderText = "معرف الجلسة",
                DataPropertyName = "SessionId",
                Width = 200,
                Visible = false
            },
            new DataGridViewTextBoxColumn
            {
                Name = "Username",
                HeaderText = "اسم المستخدم",
                DataPropertyName = "Username",
                Width = 130
            },
            new DataGridViewTextBoxColumn
            {
                Name = "MachineName",
                HeaderText = "اسم الجهاز",
                DataPropertyName = "MachineName",
                Width = 130
            },
            new DataGridViewTextBoxColumn
            {
                Name = "IpAddress",
                HeaderText = "عنوان IP",
                DataPropertyName = "IpAddress",
                Width = 130
            },
            new DataGridViewTextBoxColumn
            {
                Name = "LoginTime",
                HeaderText = "وقت تسجيل الدخول",
                DataPropertyName = "LoginTime",
                Width = 150,
                DefaultCellStyle = { Format = "dd/MM/yyyy HH:mm:ss" }
            },
            new DataGridViewTextBoxColumn
            {
                Name = "LastActivityTime",
                HeaderText = "آخر نشاط",
                DataPropertyName = "LastActivityTime",
                Width = 150,
                DefaultCellStyle = { Format = "dd/MM/yyyy HH:mm:ss" }
            },
            new DataGridViewTextBoxColumn
            {
                Name = "IdleTime",
                HeaderText = "وقت الخمول",
                Width = 120
            },
            new DataGridViewTextBoxColumn
            {
                Name = "SessionDuration",
                HeaderText = "مدة الجلسة",
                Width = 120
            }
        });

        // Bottom panel for buttons
        var bottomPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            BackColor = Color.WhiteSmoke
        };

        _btnRefresh = new Button
        {
            Text = "🔄 تحديث",
            Size = new Size(120, 40),
            Location = new Point(20, 10),
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnRefresh.Click += BtnRefresh_Click;

        _btnForceEnd = new Button
        {
            Text = "⛔ إنهاء الجلسة",
            Size = new Size(160, 40),
            Location = new Point(150, 10),
            BackColor = Color.FromArgb(211, 47, 47),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnForceEnd.Click += BtnForceEnd_Click;

        _btnCleanup = new Button
        {
            Text = "🧹 تنظيف الجلسات القديمة",
            Size = new Size(200, 40),
            Location = new Point(320, 10),
            BackColor = ColorScheme.Warning,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnCleanup.Click += BtnCleanup_Click;

        _btnClose = new Button
        {
            Text = "إغلاق",
            Size = new Size(120, 40),
            Location = new Point(960, 10),
            BackColor = ColorScheme.Danger,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnClose.Click += (s, e) => this.Close();

        bottomPanel.Controls.AddRange(new Control[] { _btnRefresh, _btnForceEnd, _btnCleanup, _btnClose });

        this.Controls.Add(_gridSessions);
        this.Controls.Add(topPanel);
        this.Controls.Add(bottomPanel);
    }

    private void LoadSessions()
    {
        try
        {
            var sessions = SessionManager.Instance.GetActiveSessions();
            
            var sessionData = sessions.Select(s => new
            {
                s.SessionId,
                s.Username,
                s.MachineName,
                s.IpAddress,
                LoginTime = s.LoginTime.ToLocalTime(),
                LastActivityTime = s.LastActivityTime.ToLocalTime(),
                IdleTime = FormatTimeSpan(s.IdleTime),
                SessionDuration = FormatTimeSpan(s.SessionDuration)
            }).ToList();

            _gridSessions.DataSource = sessionData;
            _lblTotalSessions.Text = $"إجمالي الجلسات النشطة: {sessions.Count}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"خطأ في تحميل الجلسات:\n{ex.Message}",
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalDays >= 1)
            return $"{(int)timeSpan.TotalDays} يوم، {timeSpan.Hours} ساعة";
        if (timeSpan.TotalHours >= 1)
            return $"{(int)timeSpan.TotalHours} ساعة، {timeSpan.Minutes} دقيقة";
        if (timeSpan.TotalMinutes >= 1)
            return $"{(int)timeSpan.TotalMinutes} دقيقة، {timeSpan.Seconds} ثانية";
        return $"{timeSpan.Seconds} ثانية";
    }

    private void RefreshTimer_Tick(object? sender, EventArgs e)
    {
        LoadSessions();
    }

    private void BtnRefresh_Click(object? sender, EventArgs e)
    {
        LoadSessions();
    }

    private void BtnForceEnd_Click(object? sender, EventArgs e)
    {
        if (_gridSessions.SelectedRows.Count == 0)
        {
            MessageBox.Show(
                "يرجى اختيار جلسة لإنهائها",
                "تنبيه",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        var selectedRow = _gridSessions.SelectedRows[0];
        var sessionId = selectedRow.Cells["SessionId"].Value?.ToString();
        var username = selectedRow.Cells["Username"].Value?.ToString();

        if (string.IsNullOrEmpty(sessionId)) return;

        var result = MessageBox.Show(
            $"هل تريد إنهاء جلسة المستخدم '{username}'؟",
            "تأكيد إنهاء الجلسة",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            SessionManager.Instance.ForceEndSession(sessionId);
            LoadSessions();
            MessageBox.Show(
                $"تم إنهاء جلسة المستخدم '{username}' بنجاح",
                "نجح",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    private void BtnCleanup_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "هل تريد تنظيف الجلسات الخاملة (أكثر من 60 دقيقة)؟",
            "تأكيد",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            SessionManager.Instance.CleanupInactiveSessions(60);
            LoadSessions();
            MessageBox.Show(
                "تم تنظيف الجلسات القديمة بنجاح",
                "نجح",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _refreshTimer.Stop();
        _refreshTimer.Dispose();
        base.OnFormClosing(e);
    }
}
