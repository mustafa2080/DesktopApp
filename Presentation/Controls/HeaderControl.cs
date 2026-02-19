using System.Drawing.Drawing2D;
using GraceWay.AccountingSystem.Application.Services;

namespace GraceWay.AccountingSystem.Presentation.Controls;

public class HeaderControl : Panel
{
    public event EventHandler? LogoutClicked;

    private readonly string _userName;
    private System.Windows.Forms.Timer? _timeTimer;
    
    private Label? _lblTodayRevenue;
    private Label? _lblPendingInvoices;
    private Label? _lblActiveReservations;
    
    private IReservationService? _reservationService;
    private IInvoiceService? _invoiceService;

    public HeaderControl(string userName)
    {
        _userName = userName;
        this.Dock = DockStyle.Fill;
        this.BackColor = Color.White;
        this.Padding = new Padding(0);
        this.RightToLeft = RightToLeft.Yes;

        InitializeHeader();
        StartClock();
    }

    public void InitializeServices(IReservationService reservationService, IInvoiceService invoiceService)
    {
        _reservationService = reservationService;
        _invoiceService = invoiceService;
        _ = LoadHeaderStatsAsync();
    }

    private void InitializeHeader()
    {
        // Main Container with gradient background
        Panel mainContainer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(25, 10, 25, 10)
        };
        mainContainer.Paint += (s, e) =>
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(
                mainContainer.ClientRectangle,
                Color.FromArgb(245, 247, 250),
                Color.White,
                90F))
            {
                e.Graphics.FillRectangle(brush, mainContainer.ClientRectangle);
            }
        };

        // Right Section - Company Info
        Panel companySection = CreateCompanySection();
        companySection.Location = new Point(25, 12);
        mainContainer.Controls.Add(companySection);

        // Center Section - Quick Stats
        Panel statsSection = CreateQuickStatsSection();
        mainContainer.Controls.Add(statsSection);

        // Left Section - User Info & Logout
        Panel userSection = CreateUserSection();
        userSection.Location = new Point(mainContainer.Width - userSection.Width - 25, 8);
        userSection.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        mainContainer.Controls.Add(userSection);

        // Handle resize to center stats
        mainContainer.Resize += (s, e) =>
        {
            statsSection.Location = new Point(
                (mainContainer.Width - statsSection.Width) / 2,
                8
            );
            userSection.Location = new Point(mainContainer.Width - userSection.Width - 25, 8);
        };

        this.Controls.Add(mainContainer);

        // Bottom shadow line
        Panel shadowLine = new Panel
        {
            Height = 2,
            Dock = DockStyle.Bottom,
            BackColor = Color.FromArgb(220, 220, 225)
        };
        this.Controls.Add(shadowLine);
    }

    private Panel CreateCompanySection()
    {
        Panel panel = new Panel
        {
            AutoSize = true,
            BackColor = Color.Transparent
        };

        // Company icon/logo circle
        Panel logoCircle = new Panel
        {
            Size = new Size(50, 50),
            Location = new Point(0, 0),
            BackColor = ColorScheme.Primary
        };
        logoCircle.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(0, 0, 50, 50);
                e.Graphics.FillPath(new SolidBrush(ColorScheme.Primary), path);
                
                // Draw icon text
                string iconText = "GW";
                using (Font font = new Font("Cairo", 12F, FontStyle.Bold))
                {
                    SizeF textSize = e.Graphics.MeasureString(iconText, font);
                    e.Graphics.DrawString(iconText, font, Brushes.White, 
                        (50 - textSize.Width) / 2, (50 - textSize.Height) / 2);
                }
            }
        };
        panel.Controls.Add(logoCircle);

        // Company name
        Label companyName = new Label
        {
            Text = "شركة جراس واي",
            Font = new Font("Cairo", 13F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(60, 5),
            BackColor = Color.Transparent
        };
        panel.Controls.Add(companyName);

        // Company subtitle
        Label companySubtitle = new Label
        {
            Text = "للسياحة والسفر",
            Font = new Font("Cairo", 9F),
            ForeColor = ColorScheme.TextSecondary,
            AutoSize = true,
            Location = new Point(60, 28),
            BackColor = Color.Transparent
        };
        panel.Controls.Add(companySubtitle);

        panel.Width = 220;
        panel.Height = 55;

        return panel;
    }

    private Panel CreateQuickStatsSection()
    {
        // 3 cards × 195px + 2 gaps × 10px = 605px
        Panel panel = new Panel
        {
            Size = new Size(615, 64),
            BackColor = Color.Transparent
        };

        var stat1 = CreateQuickStat("💰", "---", "إيرادات اليوم",
            0,   Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105),   out _lblTodayRevenue);
        var stat2 = CreateQuickStat("📄", "---", "فواتير معلقة",
            205, Color.FromArgb(245, 101, 101), Color.FromArgb(220, 38, 38),  out _lblPendingInvoices);
        var stat3 = CreateQuickStat("✈️", "---", "حجوزات نشطة",
            410, Color.FromArgb(96, 165, 250), Color.FromArgb(37, 99, 235),   out _lblActiveReservations);

        panel.Controls.Add(stat1);
        panel.Controls.Add(stat2);
        panel.Controls.Add(stat3);
        return panel;
    }

    private Panel CreateQuickStat(string icon, string value, string label,
        int xPos, Color colorTop, Color colorBottom, out Label valueLabel)
    {
        var card = new Panel
        {
            Size     = new Size(195, 64),
            Location = new Point(xPos, 0),
            BackColor = Color.Transparent,
            Cursor    = Cursors.Hand,
        };

        bool hovered = false;

        card.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
            using var path = RoundedRect(rect, 12);

            // Gradient fill
            var c1 = hovered ? Lighten(colorTop, 18)    : colorTop;
            var c2 = hovered ? Lighten(colorBottom, 12) : colorBottom;
            using var grad = new LinearGradientBrush(rect, c1, c2,
                LinearGradientMode.ForwardDiagonal);
            g.FillPath(grad, path);

            // Subtle inner-top white sheen
            using var sheenPath = RoundedRect(new Rectangle(1, 1, card.Width - 3, 28), 11);
            using var sheenBrush = new SolidBrush(Color.FromArgb(28, 255, 255, 255));
            g.FillPath(sheenBrush, sheenPath);

            // Thin white border
            using var pen = new Pen(Color.FromArgb(55, 255, 255, 255), 1.2f);
            g.DrawPath(pen, path);

            // Bottom glow strip
            using var stripBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0));
            g.FillRectangle(stripBrush, 0, card.Height - 10, card.Width, 10);
        };

        // Icon box — small rounded square
        var iconBox = new Panel
        {
            Size      = new Size(40, 40),
            Location  = new Point(card.Width - 50, 12),
            BackColor = Color.Transparent,
        };
        iconBox.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var p = RoundedRect(new Rectangle(0, 0, 40, 40), 8);
            using var b = new SolidBrush(Color.FromArgb(55, 255, 255, 255));
            g.FillPath(b, p);
        };

        var lblIcon = new Label
        {
            Text      = icon,
            Font      = new Font("Segoe UI Emoji", 17F),
            ForeColor = Color.White,
            Size      = new Size(40, 40),
            Location  = new Point(0, 0),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
        };
        iconBox.Controls.Add(lblIcon);
        card.Controls.Add(iconBox);

        // Value label
        valueLabel = new Label
        {
            Text      = value,
            Font      = new Font("Cairo", 17F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize  = false,
            Size      = new Size(130, 32),
            Location  = new Point(6, 6),
            TextAlign = ContentAlignment.MiddleRight,
            BackColor = Color.Transparent,
        };
        card.Controls.Add(valueLabel);

        // Label text
        var lblText = new Label
        {
            Text      = label,
            Font      = new Font("Cairo", 8.5F, FontStyle.Regular),
            ForeColor = Color.FromArgb(220, 255, 255, 255),
            AutoSize  = false,
            Size      = new Size(130, 20),
            Location  = new Point(6, 40),
            TextAlign = ContentAlignment.MiddleRight,
            BackColor = Color.Transparent,
        };
        card.Controls.Add(lblText);

        // Hover
        void SetHover(bool on)
        {
            hovered = on;
            card.Invalidate();
        }
        card.MouseEnter += (s, e) => SetHover(true);
        card.MouseLeave += (s, e) => SetHover(false);
        foreach (Control c in card.Controls)
        {
            c.MouseEnter += (s, e) => SetHover(true);
            c.MouseLeave += (s, e) => SetHover(false);
        }
        foreach (Control c in iconBox.Controls)
        {
            c.MouseEnter += (s, e) => SetHover(true);
            c.MouseLeave += (s, e) => SetHover(false);
        }

        return card;
    }

    private static Color Lighten(Color c, int amount) =>
        Color.FromArgb(
            Math.Min(255, c.R + amount),
            Math.Min(255, c.G + amount),
            Math.Min(255, c.B + amount));

    private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        path.AddArc(bounds.X,              bounds.Y,               d, d, 180, 90);
        path.AddArc(bounds.Right - d,      bounds.Y,               d, d, 270, 90);
        path.AddArc(bounds.Right - d,      bounds.Bottom - d,      d, d,   0, 90);
        path.AddArc(bounds.X,              bounds.Bottom - d,      d, d,  90, 90);
        path.CloseFigure();
        return path;
    }

    private async Task LoadHeaderStatsAsync()
    {
        try
        {
            // ⚠️ DbContext غير thread-safe - sequential queries
            var today = DateTime.Today;

            if (_reservationService != null)
            {
                var stats = await _reservationService.GetReservationStatisticsAsync(today, today.AddDays(1));
                var todayRevenue = stats.GetValueOrDefault("TotalSales", 0m);
                var allReservations = await _reservationService.GetAllReservationsAsync();
                var activeCount = allReservations?.Count(r => r.Status == "Confirmed" || r.Status == "Pending") ?? 0;
                SafeUpdateUI(() =>
                {
                    if (_lblTodayRevenue != null) _lblTodayRevenue.Text = FormatCurrency(todayRevenue);
                    if (_lblActiveReservations != null) _lblActiveReservations.Text = activeCount.ToString();
                });
            }

            if (_invoiceService != null)
            {
                var salesInvoices    = await _invoiceService.GetUnpaidSalesInvoicesAsync();
                var purchaseInvoices = await _invoiceService.GetUnpaidPurchaseInvoicesAsync();
                var pendingCount = (salesInvoices?.Count ?? 0) + (purchaseInvoices?.Count ?? 0);
                SafeUpdateUI(() => { if (_lblPendingInvoices != null) _lblPendingInvoices.Text = pendingCount.ToString(); });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error loading header stats: {ex.Message}");
            GraceWay.AccountingSystem.Infrastructure.Logging.AppLogger.Error("Error loading header stats", ex);
        }
    }

    private void SafeUpdateUI(Action action)
    {
        if (InvokeRequired)
        {
            try
            {
                Invoke(action);
            }
            catch { }
        }
        else
        {
            action();
        }
    }

    private string FormatCurrency(decimal value)
    {
        if (value >= 1000000)
            return $"{(value / 1000000):N1}م";
        if (value >= 1000)
            return $"{(value / 1000):N0}ك";
        
        return $"{value:N0}";
    }

    private Panel CreateUserSection()
    {
        Panel panel = new Panel
        {
            Size = new Size(320, 60),
            BackColor = Color.Transparent
        };

        // User info card
        Panel userCard = new Panel
        {
            Size = new Size(180, 55),
            Location = new Point(0, 0),
            BackColor = Color.FromArgb(240, 245, 255)
        };
        userCard.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = new GraphicsPath())
            {
                int radius = 12;
                Rectangle rect = new Rectangle(0, 0, userCard.Width - 1, userCard.Height - 1);
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();

                using (SolidBrush brush = new SolidBrush(Color.FromArgb(240, 245, 255)))
                {
                    e.Graphics.FillPath(brush, path);
                }
                using (Pen pen = new Pen(Color.FromArgb(200, 210, 230), 1))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        };

        // User avatar circle
        Panel avatarCircle = new Panel
        {
            Size = new Size(38, 38),
            Location = new Point(10, 8),
            BackColor = ColorScheme.Primary
        };
        avatarCircle.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(0, 0, 38, 38);
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    avatarCircle.ClientRectangle,
                    ColorScheme.Primary,
                    Color.FromArgb(ColorScheme.Primary.R + 30, ColorScheme.Primary.G + 30, ColorScheme.Primary.B + 30),
                    45F))
                {
                    e.Graphics.FillPath(brush, path);
                }
                
                // Draw user icon
                string iconText = _userName.Length > 0 ? _userName.Substring(0, 1).ToUpper() : "U";
                using (Font font = new Font("Cairo", 14F, FontStyle.Bold))
                {
                    SizeF textSize = e.Graphics.MeasureString(iconText, font);
                    e.Graphics.DrawString(iconText, font, Brushes.White, 
                        (38 - textSize.Width) / 2, (38 - textSize.Height) / 2 - 1);
                }
            }
        };
        userCard.Controls.Add(avatarCircle);

        // User name
        Label userName = new Label
        {
            Text = _userName,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = ColorScheme.TextPrimary,
            AutoSize = true,
            Location = new Point(55, 12),
            BackColor = Color.Transparent
        };
        userCard.Controls.Add(userName);

        // User role
        Label userRole = new Label
        {
            Text = "👑 مدير النظام",
            Font = new Font("Cairo", 8F),
            ForeColor = ColorScheme.TextSecondary,
            AutoSize = true,
            Location = new Point(55, 30),
            BackColor = Color.Transparent
        };
        userCard.Controls.Add(userRole);

        panel.Controls.Add(userCard);

        // Logout button
        Button logoutButton = new Button
        {
            Text = "🚪",
            Font = new Font("Segoe UI Emoji", 16F),
            ForeColor = Color.White,
            BackColor = ColorScheme.Error,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(50, 55),
            Location = new Point(190, 0),
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleCenter
        };
        logoutButton.FlatAppearance.BorderSize = 0;
        logoutButton.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        
        // Rounded corners for logout button
        logoutButton.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = new GraphicsPath())
            {
                int radius = 12;
                Rectangle rect = new Rectangle(0, 0, logoutButton.Width - 1, logoutButton.Height - 1);
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();

                logoutButton.Region = new Region(path);
            }
        };

        // Tooltip
        ToolTip tooltip = new ToolTip();
        tooltip.SetToolTip(logoutButton, "تسجيل الخروج");

        // Hover effects
        logoutButton.MouseEnter += (s, e) =>
        {
            logoutButton.BackColor = Color.FromArgb(200, 50, 45);
            logoutButton.Font = new Font("Segoe UI Emoji", 18F);
        };
        logoutButton.MouseLeave += (s, e) =>
        {
            logoutButton.BackColor = ColorScheme.Error;
            logoutButton.Font = new Font("Segoe UI Emoji", 16F);
        };

        logoutButton.Click += (s, e) => LogoutClicked?.Invoke(this, EventArgs.Empty);

        panel.Controls.Add(logoutButton);

        // About button
        Button aboutButton = new Button
        {
            Text = "ℹ️",
            Font = new Font("Segoe UI Emoji", 14F),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(52, 152, 219),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(50, 55),
            Location = new Point(244, 0),
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleCenter
        };
        aboutButton.FlatAppearance.BorderSize = 0;
        ToolTip aboutTooltip = new ToolTip();
        aboutTooltip.SetToolTip(aboutButton, "عن البرنامج");
        aboutButton.Click += (s, e) =>
        {
            using var aboutForm = new GraceWay.AccountingSystem.Presentation.Forms.AboutForm();
            aboutForm.ShowDialog();
        };
        panel.Controls.Add(aboutButton);
        panel.Width = 305;

        // Separator line
        Panel separator = new Panel
        {
            Size = new Size(1, 45),
            Location = new Point(185, 5),
            BackColor = Color.FromArgb(220, 220, 225)
        };
        panel.Controls.Add(separator);

        return panel;
    }

    private void StartClock()
    {
        _timeTimer = new System.Windows.Forms.Timer();
        _timeTimer.Interval = 1000;
        _timeTimer.Tick += (s, e) => { /* Timer removed from header */ };
        _timeTimer.Start();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timeTimer?.Stop();
            _timeTimer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
