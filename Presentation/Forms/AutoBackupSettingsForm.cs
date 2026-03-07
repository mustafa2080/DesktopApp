using System.Drawing.Drawing2D;
using GraceWay.AccountingSystem.Application.Services;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public class AutoBackupSettingsForm : Form
{
    private readonly AutoBackupService _autoBackup;
    private CheckBox  _chkEnabled    = null!;
    private RadioButton _rdoInterval = null!, _rdoScheduled = null!;
    private NumericUpDown _numHours  = null!;
    private DateTimePicker _dtpTime  = null!;
    private NumericUpDown _numKeep   = null!;
    private TextBox   _txtPath       = null!;
    private Label     _lblStatus     = null!;

    private static readonly Color Primary  = Color.FromArgb(59, 130, 246);
    private static readonly Color BgPage   = Color.FromArgb(247,249,252);
    private static readonly Color TxtMain  = Color.FromArgb(15, 23, 42);
    private static readonly Color TxtMuted = Color.FromArgb(100,116,139);

    public AutoBackupSettingsForm(AutoBackupService autoBackup)
    {
        _autoBackup = autoBackup;
        InitForm();
        LoadSettings();
    }

    private void InitForm()
    {
        Text = "إعدادات النسخ الاحتياطي التلقائي";
        Size = new Size(540, 500);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = BgPage; FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true; Font = new Font("Segoe UI", 9.5f);

        var card1 = MakeCard(20, 16, 490, 72, "تفعيل");
        _chkEnabled = new CheckBox { Text = "تفعيل النسخ الاحتياطي التلقائي",
            Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = TxtMain,
            AutoSize = true, Location = new Point(16, 30) };
        card1.Controls.Add(_chkEnabled);
        Controls.Add(card1);

        var card2 = MakeCard(20, 104, 490, 156, "الجدولة");
        _rdoInterval  = new RadioButton { Text = "كل:", ForeColor = TxtMain, AutoSize = true, Location = new Point(16, 34) };
        _numHours     = new NumericUpDown { Minimum=1, Maximum=168, Value=24, Location = new Point(60, 32), Size = new Size(65,26) };
        var lblH      = new Label { Text="ساعة", ForeColor=TxtMuted, AutoSize=true, Location=new Point(132,36) };
        _rdoScheduled = new RadioButton { Text="يومياً في الساعة:", ForeColor=TxtMain, AutoSize=true, Location=new Point(16,76) };
        _dtpTime      = new DateTimePicker { Format=DateTimePickerFormat.Time, ShowUpDown=true, Location=new Point(180,74), Size=new Size(100,26) };
        var lblKeep   = new Label { Text="الاحتفاظ بـ:", ForeColor=TxtMuted, AutoSize=true, Location=new Point(340,116) };
        _numKeep      = new NumericUpDown { Minimum=1,Maximum=30,Value=7, Location=new Point(262,114), Size=new Size(60,26) };
        var lblNsKh   = new Label { Text="نسخة", ForeColor=TxtMuted, AutoSize=true, Location=new Point(213,118) };
        card2.Controls.AddRange(new Control[]{ _rdoInterval,_numHours,lblH,_rdoScheduled,_dtpTime,lblKeep,_numKeep,lblNsKh });
        Controls.Add(card2);

        var card3 = MakeCard(20, 276, 490, 76, "مسار الحفظ");
        _txtPath = new TextBox { Location=new Point(76,30), Size=new Size(300,26), Font=new Font("Segoe UI",9.5f) };
        var btnBrowse = MakeBtn("📂", 16, 28, 52, Color.FromArgb(100,116,139));
        btnBrowse.Click += (_,_) => { using var d=new FolderBrowserDialog{SelectedPath=_txtPath.Text}; if(d.ShowDialog()==DialogResult.OK) _txtPath.Text=d.SelectedPath; };
        card3.Controls.AddRange(new Control[]{ _txtPath, btnBrowse });
        Controls.Add(card3);

        _lblStatus = new Label { Text="", Font=new Font("Segoe UI",9f), ForeColor=TxtMuted,
            AutoSize=false, Location=new Point(20,364), Size=new Size(490,22), TextAlign=ContentAlignment.MiddleRight };
        Controls.Add(_lblStatus);

        var btnNow   = MakeBtn("▶ نسخ الآن", 20, 394, 130, Color.FromArgb(16,185,129));
        btnNow.Click += async (_,_) => await BackupNowAsync();
        var btnSave  = MakeBtn("💾 حفظ", 168, 394, 120, Primary);
        btnSave.Click += (_,_) => SaveSettings();
        var btnClose = MakeBtn("إغلاق", 306, 394, 100, Color.FromArgb(100,116,139));
        btnClose.Click += (_,_) => Close();
        Controls.AddRange(new Control[]{ btnNow, btnSave, btnClose });
    }

    private static Panel MakeCard(int x, int y, int w, int h, string title)
    {
        var card = new Panel { Location=new Point(x,y), Size=new Size(w,h), BackColor=Color.White };
        card.Paint += (s,e) =>
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(Color.FromArgb(226,232,240),1);
            g.DrawRectangle(pen, 0,0,card.Width-1,card.Height-1);
            using var f = new Font("Segoe UI",9f,FontStyle.Bold);
            g.DrawString(title, f, new SolidBrush(Color.FromArgb(100,116,139)), card.Width-10, 6,
                new StringFormat{Alignment=StringAlignment.Far});
        };
        return card;
    }

    private static Button MakeBtn(string text, int x, int y, int w, Color bg)
    {
        var b = new Button { Text=text, Location=new Point(x,y), Size=new Size(w,34),
            BackColor=bg, ForeColor=Color.White, FlatStyle=FlatStyle.Flat, Cursor=Cursors.Hand };
        b.FlatAppearance.BorderSize=0;
        return b;
    }

    private void LoadSettings()
    {
        var s = _autoBackup.Settings;
        _chkEnabled.Checked      = s.Enabled;
        _rdoScheduled.Checked    = s.UseScheduledTime;
        _rdoInterval.Checked     = !s.UseScheduledTime;
        _numHours.Value          = Math.Max(1, Math.Min(168, s.IntervalHours));
        _dtpTime.Value           = DateTime.Today.Add(s.ScheduledTime);
        _numKeep.Value           = Math.Max(1, Math.Min(30, s.MaxBackupsToKeep));
        _txtPath.Text            = s.BackupPath;
        _lblStatus.Text          = s.Enabled
            ? $"النسخ التلقائي نشط — كل {s.IntervalHours} ساعة"
            : "النسخ التلقائي متوقف";
    }

    private void SaveSettings()
    {
        var s = new AutoBackupSettings
        {
            Enabled          = _chkEnabled.Checked,
            IntervalHours    = (int)_numHours.Value,
            UseScheduledTime = _rdoScheduled.Checked,
            ScheduledTime    = _dtpTime.Value.TimeOfDay,
            MaxBackupsToKeep = (int)_numKeep.Value,
            BackupPath       = _txtPath.Text.Trim(),
        };
        _autoBackup.ApplySettings(s);
        _lblStatus.ForeColor = Color.FromArgb(16,185,129);
        _lblStatus.Text      = "✅ تم حفظ الإعدادات وتطبيقها";
        MessageBox.Show("تم حفظ إعدادات النسخ التلقائي بنجاح", "تم", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task BackupNowAsync()
    {
        _lblStatus.ForeColor = Color.FromArgb(59,130,246);
        _lblStatus.Text      = "⏳ جاري إنشاء نسخة احتياطية...";
        Cursor               = Cursors.WaitCursor;
        try
        {
            await _autoBackup.RunBackupAsync();
            _lblStatus.ForeColor = Color.FromArgb(16,185,129);
            _lblStatus.Text      = $"✅ تمت النسخة الاحتياطية — {DateTime.Now:HH:mm}";
        }
        catch (Exception ex)
        {
            _lblStatus.ForeColor = Color.FromArgb(239,68,68);
            _lblStatus.Text      = $"❌ فشل: {ex.Message}";
        }
        finally { Cursor = Cursors.Default; }
    }
}
