using System.Drawing.Drawing2D;
using GraceWay.AccountingSystem.Application.Services;

namespace GraceWay.AccountingSystem.Presentation.Controls;

/// <summary>
/// زر الجرس + Panel الإشعارات المنسدل
/// أضفه في HeaderControl بجانب زر اللوجاوت
/// </summary>
public class NotificationBellControl : Panel
{
    public event EventHandler<string>? NavigateRequested; // يطلب الانتقال لقسم معين

    private readonly INotificationService _service;
    private Button     _bellBtn   = null!;
    private Label      _badgeLbl  = null!;
    private Panel      _dropdown  = null!;
    private bool       _dropOpen  = false;
    private System.Windows.Forms.Timer _pollTimer = null!;

    // ── ألوان ──────────────────────────────────────────────────
    private static readonly Color BgCard    = Color.White;
    private static readonly Color Border    = Color.FromArgb(226, 232, 240);
    private static readonly Color TxtMain   = Color.FromArgb(15,  23,  42);
    private static readonly Color TxtMuted  = Color.FromArgb(100,116,139);
    private static readonly Color ColWarn   = Color.FromArgb(251,146, 60);
    private static readonly Color ColDanger = Color.FromArgb(239, 68, 68);
    private static readonly Color ColInfo   = Color.FromArgb( 59,130,246);
    private static readonly Color ColOk     = Color.FromArgb( 16,185,129);

    public NotificationBellControl(INotificationService service)
    {
        _service = service;
        Size      = new Size(52, 55);
        BackColor = Color.Transparent;
        service.NotificationsRefreshed += (_, _) => SafeInvoke(RenderBadge);
        Build();
        StartPolling();
    }

    private void Build()
    {
        // ── زر الجرس ──────────────────────────────────────────
        _bellBtn = new Button
        {
            Text      = "🔔",
            Font      = new Font("Segoe UI Emoji", 17F),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(99, 102, 241),
            FlatStyle = FlatStyle.Flat,
            Size      = new Size(50, 55),
            Location  = new Point(0, 0),
            Cursor    = Cursors.Hand,
        };
        _bellBtn.FlatAppearance.BorderSize = 0;
        _bellBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(79, 70, 229);
        _bellBtn.Click += ToggleDropdown;
        Controls.Add(_bellBtn);

        // ── Badge ──────────────────────────────────────────────
        _badgeLbl = new Label
        {
            Text      = "",
            Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = ColDanger,
            Size      = new Size(16, 16),
            Location  = new Point(30, 4),
            TextAlign = ContentAlignment.MiddleCenter,
            Visible   = false,
        };
        _badgeLbl.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var b = new SolidBrush(ColDanger);
            e.Graphics.FillEllipse(b, 0, 0, 15, 15);
            e.Graphics.DrawString(_badgeLbl.Text, _badgeLbl.Font,
                Brushes.White, new RectangleF(0, 1, 15, 14));
        };
        Controls.Add(_badgeLbl);
        _badgeLbl.BringToFront();
    }

    private void ToggleDropdown(object? s, EventArgs e)
    {
        if (_dropOpen) { CloseDropdown(); return; }
        OpenDropdown();
    }

    private async void OpenDropdown()
    {
        _dropOpen = true;
        _service.MarkAllRead();
        RenderBadge();

        var notes = await _service.GetAllAsync();

        // ── بناء الـ Dropdown ──────────────────────────────────
        _dropdown?.Dispose();
        _dropdown = new Panel
        {
            Size      = new Size(360, Math.Min(420, 56 + notes.Count * 74 + 16)),
            BackColor = BgCard,
            BorderStyle = BorderStyle.FixedSingle,
        };
        _dropdown.Paint += (_, ev) =>
        {
            ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(Border, 1f);
            ev.Graphics.DrawRectangle(pen, 0, 0, _dropdown.Width - 1, _dropdown.Height - 1);
        };

        // Header
        var hdr = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.FromArgb(248,250,252) };
        hdr.Controls.Add(new Label
        {
            Text = $"الإشعارات ({notes.Count})",
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            ForeColor = TxtMain, Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0,0,12,0),
        });
        _dropdown.Controls.Add(hdr);

        // قائمة الإشعارات
        var scroll = new Panel { AutoScroll = true, Dock = DockStyle.Fill };
        int y = 4;
        if (notes.Count == 0)
        {
            scroll.Controls.Add(new Label
            {
                Text = "✅  لا توجد إشعارات جديدة",
                Font = new Font("Segoe UI", 9.5f), ForeColor = TxtMuted,
                AutoSize = false, Width = 320, Height = 60,
                TextAlign = ContentAlignment.MiddleCenter, Location = new Point(16, 20),
            });
        }
        else
        {
            foreach (var n in notes)
            {
                scroll.Controls.Add(BuildNoteRow(n, ref y));
            }
        }
        _dropdown.Controls.Add(scroll);

        // موضع الـ dropdown بالنسبة للـ form
        var form = FindForm()!;
        var ptOnForm = PointToScreen(new Point(Width - _dropdown.Width, Height));
        var ptClient = form.PointToClient(ptOnForm);
        _dropdown.Location = ptClient;

        form.Controls.Add(_dropdown);
        _dropdown.BringToFront();

        // إغلاق عند الضغط خارجه
        form.MouseClick += CloseOnOutsideClick;
    }

    private void CloseDropdown()
    {
        _dropOpen = false;
        var form = FindForm();
        if (form != null) { form.Controls.Remove(_dropdown); form.MouseClick -= CloseOnOutsideClick; }
        _dropdown?.Dispose();
    }

    private void CloseOnOutsideClick(object? s, MouseEventArgs e)
    {
        if (_dropdown == null || !_dropdown.Bounds.Contains(e.Location)) CloseDropdown();
    }

    private Panel BuildNoteRow(AppNotification n, ref int y)
    {
        Color accent = n.Type switch
        {
            NotificationType.Danger  => ColDanger,
            NotificationType.Warning => ColWarn,
            NotificationType.Success => ColOk,
            _                        => ColInfo,
        };
        string icon = n.Type switch
        {
            NotificationType.Danger  => "🔴",
            NotificationType.Warning => "🟠",
            NotificationType.Success => "🟢",
            _                        => "🔵",
        };

        var row = new Panel
        {
            Location  = new Point(8, y),
            Size      = new Size(336, 66),
            BackColor = Color.White,
            Cursor    = Cursors.Hand,
        };
        row.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            // خط جانبي ملون
            using var b = new SolidBrush(accent);
            e.Graphics.FillRectangle(b, row.Width - 4, 6, 4, row.Height - 12);
            // بوردر سفلي
            using var pen = new Pen(Color.FromArgb(240,242,246), 1);
            e.Graphics.DrawLine(pen, 8, row.Height - 1, row.Width - 8, row.Height - 1);
        };

        row.Controls.Add(new Label
        {
            Text      = icon + "  " + n.Title,
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = TxtMain,
            AutoSize  = false, Width = 310, Height = 22,
            Location  = new Point(6, 8),
            TextAlign = ContentAlignment.MiddleRight,
            BackColor = Color.Transparent,
        });
        row.Controls.Add(new Label
        {
            Text      = n.Message,
            Font      = new Font("Segoe UI", 8.5f),
            ForeColor = TxtMuted,
            AutoSize  = false, Width = 310, Height = 28,
            Location  = new Point(6, 30),
            TextAlign = ContentAlignment.TopRight,
            BackColor = Color.Transparent,
        });

        // Hover
        row.MouseEnter += (_, _) => row.BackColor = Color.FromArgb(248,250,255);
        row.MouseLeave += (_, _) => row.BackColor = Color.White;
        row.Click += (_, _) => { CloseDropdown(); NavigateRequested?.Invoke(this, n.ActionKey); };
        foreach (Control c in row.Controls)
        {
            c.MouseEnter += (_, _) => row.BackColor = Color.FromArgb(248,250,255);
            c.MouseLeave += (_, _) => row.BackColor = Color.White;
            c.Click      += (_, _) => { CloseDropdown(); NavigateRequested?.Invoke(this, n.ActionKey); };
        }

        y += 70;
        return row;
    }

    private void RenderBadge()
    {
        int cnt = _service.UnreadCount;
        _badgeLbl.Visible = cnt > 0;
        _badgeLbl.Text    = cnt > 9 ? "9+" : cnt.ToString();
        _badgeLbl.Invalidate();
    }

    private void StartPolling()
    {
        _pollTimer = new System.Windows.Forms.Timer { Interval = 5 * 60 * 1000 }; // 5 دقائق
        _pollTimer.Tick += async (_, _) => await _service.RefreshAsync();
        _pollTimer.Start();
        // تحميل أول مرة بعد 3 ثواني من فتح البرنامج
        var startup = new System.Windows.Forms.Timer { Interval = 3000 };
        startup.Tick += async (_, _) => { startup.Stop(); await _service.RefreshAsync(); };
        startup.Start();
    }

    private void SafeInvoke(Action a)
    {
        if (InvokeRequired) try { Invoke(a); } catch { } else a();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { _pollTimer?.Stop(); _pollTimer?.Dispose(); }
        base.Dispose(disposing);
    }
}
