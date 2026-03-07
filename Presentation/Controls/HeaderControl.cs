using System.Drawing.Drawing2D;
using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Presentation;

namespace GraceWay.AccountingSystem.Presentation.Controls;

/// <summary>
/// شريط العنوان العلوي
///   يسار  : شعار + اسم النظام
///   وسط   : 3 بطاقات إحصائية (مستحق / فواتير / حجوزات)
///   يمين  : ساعة | جرس الإشعارات | اسم المستخدم | زر الخروج
/// </summary>
public class HeaderControl : Panel
{
    // ══════════════════════════════════════════════════
    // Events
    // ══════════════════════════════════════════════════
    public event EventHandler?         LogoutClicked;
    public event EventHandler<string>? NavigateRequested;

    // ══════════════════════════════════════════════════
    // Private fields
    // ══════════════════════════════════════════════════
    private readonly string _userName;

    private IReservationService?  _reservationService;
    private IInvoiceService?      _invoiceService;
    private INotificationService? _notificationService;

    private System.Windows.Forms.Timer? _clockTimer;
    private System.Windows.Forms.Timer? _notifTimer;

    // stat value labels (centre of each card)
    private Label? _valRevenue;
    private Label? _valInvoices;
    private Label? _valReservations;

    // stat card panels (needed for repositioning)
    private readonly List<Panel> _statCards = new();

    // right-side controls
    private Label?  _lblClock;
    private Label?  _lblDate;
    private Panel?  _bellPanel;
    private Label?  _bellBadge;
    private Panel?  _userChip;
    private Button? _btnLogout;

    // notification dropdown
    private Panel? _dropdown;
    private bool   _dropOpen;

    // ══════════════════════════════════════════════════
    // Layout constants
    // ══════════════════════════════════════════════════
    private const int HeaderH   = 70;
    private const int CardW     = 165;
    private const int CardH     = 48;
    private const int CardGap   = 8;
    private const int RightPad  = 14;

    // ══════════════════════════════════════════════════
    // Constructor
    // ══════════════════════════════════════════════════
    public HeaderControl(string userName)
    {
        _userName = userName;

        Dock        = DockStyle.Fill;
        Height      = HeaderH;
        BackColor   = ColorScheme.SidebarBg;
        Padding     = new Padding(0);
        RightToLeft = RightToLeft.Yes;

        Paint += DrawBottomBorder;

        BuildLeft();
        BuildCenter();
        BuildRight();
        StartClock();

        Resize += (_, _) => RepositionAll();
    }

    // ══════════════════════════════════════════════════
    // Public init
    // ══════════════════════════════════════════════════
    public void InitializeServices(IReservationService res, IInvoiceService inv)
    {
        _reservationService = res;
        _invoiceService     = inv;
        _ = LoadStatsAsync();
    }

    public void InitializeNotifications(INotificationService svc)
    {
        _notificationService = svc;
        svc.NotificationsRefreshed += (_, _) => SafeUI(RefreshBadge);

        _notifTimer = new System.Windows.Forms.Timer { Interval = 5 * 60_000 };
        _notifTimer.Tick += async (_, _) => await svc.RefreshAsync();
        _notifTimer.Start();

        var once = new System.Windows.Forms.Timer { Interval = 3000 };
        once.Tick += async (_, _) => { once.Stop(); await svc.RefreshAsync(); };
        once.Start();
    }

    // ══════════════════════════════════════════════════
    // Build — Left: brand
    // ══════════════════════════════════════════════════
    private void BuildLeft()
    {
        // أيقونة دائرية
        var iconPanel = new Panel
        {
            Size      = new Size(42, 42),
            BackColor = Color.Transparent,
            Location  = new Point(14, (HeaderH - 42) / 2),
        };
        iconPanel.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var b = new SolidBrush(ColorScheme.Primary);
            g.FillEllipse(b, 0, 0, 41, 41);
            using var f = new Font("Segoe UI Emoji", 17F);
            g.DrawString("💼", f, Brushes.White,
                new RectangleF(0, 0, 42, 42),
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        };
        Controls.Add(iconPanel);

        Controls.Add(new Label
        {
            Text      = "جريس واي",
            Font      = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize  = true,
            Location  = new Point(64, 12),
            BackColor = Color.Transparent,
        });
        Controls.Add(new Label
        {
            Text      = "النظام المحاسبي",
            Font      = new Font("Cairo", 8F),
            ForeColor = ColorScheme.WithOpacity(Color.White, 150),
            AutoSize  = true,
            Location  = new Point(64, 38),
            BackColor = Color.Transparent,
        });
    }

    // ══════════════════════════════════════════════════
    // Build — Center: stat cards
    // ══════════════════════════════════════════════════
    private void BuildCenter()
    {
        _valRevenue      = AddStatCard("💰", "مستحق",         "—",      ColorScheme.Success);
        _valInvoices     = AddStatCard("🧾", "فواتير معلقة",  "—",      ColorScheme.Warning);
        _valReservations = AddStatCard("📋", "حجوزات نشطة",  "—",      ColorScheme.Info);
        PositionStatCards();
    }

    private Label AddStatCard(string icon, string title, string value, Color accent)
    {
        var card = new Panel
        {
            Size      = new Size(CardW, CardH),
            BackColor = ColorScheme.WithOpacity(Color.White, 20),
        };

        // خط علوي ملون
        card.Paint += (_, e) =>
        {
            using var b = new SolidBrush(accent);
            e.Graphics.FillRectangle(b, 0, 0, card.Width, 3);
        };

        card.Controls.Add(new Label
        {
            Text      = icon + "  " + title,
            Font      = new Font("Cairo", 7.5F),
            ForeColor = ColorScheme.WithOpacity(Color.White, 160),
            AutoSize  = false,
            Size      = new Size(CardW - 6, 18),
            Location  = new Point(3, 7),
            TextAlign = ContentAlignment.MiddleRight,
            BackColor = Color.Transparent,
        });

        var valLbl = new Label
        {
            Text      = value,
            Font      = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize  = false,
            Size      = new Size(CardW - 6, 20),
            Location  = new Point(3, 25),
            TextAlign = ContentAlignment.MiddleRight,
            BackColor = Color.Transparent,
        };
        card.Controls.Add(valLbl);

        _statCards.Add(card);
        Controls.Add(card);
        return valLbl;
    }

    private void PositionStatCards()
    {
        if (_statCards.Count == 0) return;
        int totalW = _statCards.Count * CardW + (_statCards.Count - 1) * CardGap;
        int startX = (Width - totalW) / 2;
        int cardY  = (HeaderH - CardH) / 2;
        for (int i = 0; i < _statCards.Count; i++)
            _statCards[i].Location = new Point(startX + i * (CardW + CardGap), cardY);
    }

    // ══════════════════════════════════════════════════
    // Build — Right: clock + bell + user + logout
    // ══════════════════════════════════════════════════
    private void BuildRight()
    {
        // ── ساعة ──────────────────────────────────────
        _lblClock = new Label
        {
            Text      = DateTime.Now.ToString("HH:mm:ss"),
            Font      = new Font("Consolas", 13F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize  = true,
            BackColor = Color.Transparent,
        };
        Controls.Add(_lblClock);

        _lblDate = new Label
        {
            Text      = DateTime.Now.ToString("ddd dd/MM/yyyy"),
            Font      = new Font("Cairo", 7.5F),
            ForeColor = ColorScheme.WithOpacity(Color.White, 150),
            AutoSize  = true,
            BackColor = Color.Transparent,
        };
        Controls.Add(_lblDate);

        // ── جرس ───────────────────────────────────────
        _bellPanel = new Panel
        {
            Size      = new Size(38, 38),
            BackColor = ColorScheme.WithOpacity(Color.White, 22),
            Cursor    = Cursors.Hand,
        };
        _bellPanel.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var b = new SolidBrush(_bellPanel.BackColor);
            e.Graphics.FillEllipse(b, 0, 0, 37, 37);
            using var f = new Font("Segoe UI Emoji", 15F);
            e.Graphics.DrawString("🔔", f, Brushes.White,
                new RectangleF(0, 0, 38, 38),
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        };
        _bellPanel.Click      += BellClick;
        _bellPanel.MouseEnter += (_, _) => { _bellPanel.BackColor = ColorScheme.WithOpacity(Color.White, 45); _bellPanel.Invalidate(); };
        _bellPanel.MouseLeave += (_, _) => { _bellPanel.BackColor = ColorScheme.WithOpacity(Color.White, 22); _bellPanel.Invalidate(); };
        Controls.Add(_bellPanel);

        // badge
        _bellBadge = new Label
        {
            Size      = new Size(18, 18),
            BackColor = ColorScheme.Danger,
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 7F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Visible   = false,
        };
        _bellBadge.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var b = new SolidBrush(ColorScheme.Danger);
            e.Graphics.FillEllipse(b, 0, 0, 17, 17);
            e.Graphics.DrawString(_bellBadge!.Text, new Font("Segoe UI", 7F, FontStyle.Bold),
                Brushes.White, new RectangleF(0, 0, 17, 17),
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        };
        Controls.Add(_bellBadge);
        _bellBadge.BringToFront();

        // ── chip المستخدم ──────────────────────────────
        _userChip = new Panel
        {
            Size      = new Size(110, 36),
            BackColor = ColorScheme.WithOpacity(Color.White, 18),
        };
        _userChip.Controls.Add(new Label
        {
            Text      = "👤  " + TruncateName(_userName, 10),
            Font      = new Font("Cairo", 8.5F, FontStyle.Bold),
            ForeColor = Color.White,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
        });
        Controls.Add(_userChip);

        // ── زر الخروج ──────────────────────────────────
        _btnLogout = new Button
        {
            Text      = "خروج  ⏏",
            Font      = new Font("Cairo", 9F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = ColorScheme.Danger,
            FlatStyle = FlatStyle.Flat,
            Size      = new Size(88, 36),
            Cursor    = Cursors.Hand,
        };
        _btnLogout.FlatAppearance.BorderSize             = 0;
        _btnLogout.FlatAppearance.MouseOverBackColor     = ColorScheme.Darken(ColorScheme.Danger, 0.15f);
        _btnLogout.Click += (_, _) => LogoutClicked?.Invoke(this, EventArgs.Empty);
        Controls.Add(_btnLogout);

        RepositionAll();
    }

    // ══════════════════════════════════════════════════
    // Reposition — runs on Resize
    // ══════════════════════════════════════════════════
    private void RepositionAll()
    {
        if (Width < 10) return;

        PositionStatCards();

        int x = Width - RightPad;

        // logout
        if (_btnLogout != null)
        {
            x -= _btnLogout.Width;
            _btnLogout.Location = new Point(x, (HeaderH - _btnLogout.Height) / 2);
            x -= 10;
        }

        // user chip
        if (_userChip != null)
        {
            x -= _userChip.Width;
            _userChip.Location = new Point(x, (HeaderH - _userChip.Height) / 2);
            x -= 10;
        }

        // bell
        if (_bellPanel != null)
        {
            x -= _bellPanel.Width;
            _bellPanel.Location = new Point(x, (HeaderH - _bellPanel.Height) / 2);

            if (_bellBadge != null)
                _bellBadge.Location = new Point(_bellPanel.Right - 10, _bellPanel.Top - 4);

            x -= 14;
        }

        // separator space
        x -= 10;

        // clock & date (right-aligned to current x)
        if (_lblClock != null)
        {
            _lblClock.Location = new Point(x - _lblClock.PreferredWidth, 10);
        }
        if (_lblDate != null)
        {
            _lblDate.Location = new Point(x - _lblDate.PreferredWidth, 40);
        }
    }

    // ══════════════════════════════════════════════════
    // Clock
    // ══════════════════════════════════════════════════
    private void StartClock()
    {
        _clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _clockTimer.Tick += (_, _) => SafeUI(() =>
        {
            if (_lblClock != null) _lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
            if (_lblDate  != null) _lblDate.Text  = DateTime.Now.ToString("ddd dd/MM/yyyy");
        });
        _clockTimer.Start();
    }

    // ══════════════════════════════════════════════════
    // Stats
    // ══════════════════════════════════════════════════
    private async Task LoadStatsAsync()
    {
        try
        {
            if (_invoiceService != null)
            {
                var unpaid = await _invoiceService.GetUnpaidSalesInvoicesAsync();
                SafeUI(() =>
                {
                    if (_valRevenue  != null) _valRevenue.Text  = $"{unpaid.Sum(i => i.RemainingAmount):N0} ج.م";
                    if (_valInvoices != null) _valInvoices.Text = $"{unpaid.Count}";
                });
            }
            if (_reservationService != null)
            {
                var all    = await _reservationService.GetAllReservationsAsync();
                int active = all.Count(r => r.Status != "Cancelled" && r.Status != "Completed");
                SafeUI(() =>
                {
                    if (_valReservations != null) _valReservations.Text = $"{active}";
                });
            }
        }
        catch { /* تجاهل */ }
    }

    // ══════════════════════════════════════════════════
    // Notification badge
    // ══════════════════════════════════════════════════
    private void RefreshBadge()
    {
        if (_notificationService == null || _bellPanel == null || _bellBadge == null) return;
        int cnt = _notificationService.UnreadCount;
        _bellBadge.Visible  = cnt > 0;
        _bellBadge.Text     = cnt > 9 ? "9+" : cnt.ToString();
        _bellBadge.Location = new Point(_bellPanel.Right - 10, _bellPanel.Top - 4);
        _bellBadge.Invalidate();
    }

    // ══════════════════════════════════════════════════
    // Notification dropdown
    // ══════════════════════════════════════════════════
    private async void BellClick(object? sender, EventArgs e)
    {
        if (_dropOpen) { CloseDropdown(); return; }
        if (_notificationService == null) return;

        _dropOpen = true;
        _notificationService.MarkAllRead();
        RefreshBadge();

        var notes = await _notificationService.GetAllAsync();
        OpenDropdown(notes);
    }

    private void OpenDropdown(List<AppNotification> notes)
    {
        _dropdown?.Dispose();

        int dropH = Math.Min(500, 52 + Math.Max(1, notes.Count) * 70 + 10);
        _dropdown = new Panel
        {
            Size      = new Size(390, dropH),
            BackColor = Color.FromArgb(30, 41, 59),
        };

        // header
        var hdr = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.FromArgb(15, 23, 42) };
        hdr.Controls.Add(new Label
        {
            Text      = $"🔔   الإشعارات  ({notes.Count})",
            Font      = new Font("Cairo", 10.5F, FontStyle.Bold),
            ForeColor = Color.White,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Padding   = new Padding(0, 0, 14, 0),
            BackColor = Color.Transparent,
        });
        hdr.Paint += (_, e) =>
        {
            using var b = new SolidBrush(ColorScheme.Primary);
            e.Graphics.FillRectangle(b, 0, hdr.Height - 3, hdr.Width, 3);
        };
        _dropdown.Controls.Add(hdr);

        // scroll list
        var scroll = new Panel { AutoScroll = true, Dock = DockStyle.Fill, Padding = new Padding(6, 4, 6, 4) };

        if (notes.Count == 0)
        {
            scroll.Controls.Add(new Label
            {
                Text      = "✅  لا توجد إشعارات جديدة",
                Font      = new Font("Cairo", 10F),
                ForeColor = ColorScheme.WithOpacity(Color.White, 150),
                AutoSize  = false, Size = new Size(360, 80),
                Location  = new Point(6, 16),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
            });
        }
        else
        {
            int y = 6;
            foreach (var n in notes)
                scroll.Controls.Add(BuildNoteRow(n, ref y));
        }

        _dropdown.Controls.Add(scroll);

        // position
        var form = FindForm()!;
        var pt   = _bellPanel!.PointToScreen(new Point(_bellPanel.Width - _dropdown.Width, _bellPanel.Height + 4));
        _dropdown.Location = form.PointToClient(pt);

        form.Controls.Add(_dropdown);
        _dropdown.BringToFront();
        form.MouseClick += CloseOnClickOutside;
    }

    private Panel BuildNoteRow(AppNotification n, ref int y)
    {
        (Color accent, string icon) = n.Type switch
        {
            NotificationType.Danger  => (ColorScheme.Danger,  "🔴"),
            NotificationType.Warning => (ColorScheme.Warning, "🟠"),
            NotificationType.Success => (ColorScheme.Success, "🟢"),
            _                        => (ColorScheme.Info,    "🔵"),
        };

        var row = new Panel
        {
            Location  = new Point(0, y),
            Size      = new Size(376, 62),
            BackColor = ColorScheme.WithOpacity(Color.White, 14),
            Cursor    = Cursors.Hand,
        };

        Color ac = accent;
        row.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var b   = new SolidBrush(ac);
            e.Graphics.FillRectangle(b, row.Width - 4, 8, 4, row.Height - 16);
            using var pen = new Pen(ColorScheme.WithOpacity(Color.White, 18), 1);
            e.Graphics.DrawLine(pen, 8, row.Height - 1, row.Width - 8, row.Height - 1);
        };

        var lblTitle = new Label
        {
            Text      = icon + "  " + n.Title,
            Font      = new Font("Cairo", 9F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize  = false, Size = new Size(368, 22),
            Location  = new Point(4, 8),
            TextAlign = ContentAlignment.MiddleRight,
            BackColor = Color.Transparent,
        };
        var lblMsg = new Label
        {
            Text      = n.Message,
            Font      = new Font("Cairo", 8F),
            ForeColor = ColorScheme.WithOpacity(Color.White, 155),
            AutoSize  = false, Size = new Size(368, 24),
            Location  = new Point(4, 30),
            TextAlign = ContentAlignment.MiddleRight,
            BackColor = Color.Transparent,
        };

        string key = n.ActionKey;
        EventHandler act = (_, _) => { CloseDropdown(); NavigateRequested?.Invoke(this, key); };
        row.Click      += act;
        lblTitle.Click += act;
        lblMsg.Click   += act;
        row.MouseEnter += (_, _) => { row.BackColor = ColorScheme.WithOpacity(Color.White, 28); };
        row.MouseLeave += (_, _) => { row.BackColor = ColorScheme.WithOpacity(Color.White, 14); };

        row.Controls.Add(lblTitle);
        row.Controls.Add(lblMsg);
        y += 66;
        return row;
    }

    private void CloseDropdown()
    {
        _dropOpen = false;
        var form  = FindForm();
        if (form != null)
        {
            form.Controls.Remove(_dropdown);
            form.MouseClick -= CloseOnClickOutside;
        }
        _dropdown?.Dispose();
        _dropdown = null;
    }

    private void CloseOnClickOutside(object? s, MouseEventArgs e)
    {
        if (_dropdown == null || !_dropdown.Bounds.Contains(e.Location))
            CloseDropdown();
    }

    // ══════════════════════════════════════════════════
    // Painting helpers
    // ══════════════════════════════════════════════════
    private void DrawBottomBorder(object? s, PaintEventArgs e)
    {
        using var pen = new Pen(ColorScheme.WithOpacity(Color.White, 25), 1);
        e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1);
    }

    // ══════════════════════════════════════════════════
    // Utilities
    // ══════════════════════════════════════════════════
    private static string TruncateName(string name, int max) =>
        name.Length <= max ? name : name[..max] + "…";

    private void SafeUI(Action a)
    {
        if (InvokeRequired) try { Invoke(a); } catch { } else a();
    }
}
