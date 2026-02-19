using System.Drawing.Drawing2D;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public class AboutForm : Form
{
    public AboutForm()
    {
        InitializeControls();
    }

    private void InitializeControls()
    {
        this.Text = "عن البرنامج";
        this.Size = new Size(520, 600);
        this.StartPosition = FormStartPosition.CenterParent;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = Color.FromArgb(15, 23, 42);
        this.Font = new Font("Cairo", 10F);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.DoubleBuffered = true;

        if (Program.AppIcon != null)
            this.Icon = Program.AppIcon;

        // ── Hero Panel (Top gradient section) ──────────────────────────
        var heroPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 210,
        };
        heroPanel.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Deep navy → rich blue gradient
            using var bg = new LinearGradientBrush(
                heroPanel.ClientRectangle,
                Color.FromArgb(15, 23, 42),
                Color.FromArgb(30, 64, 120),
                LinearGradientMode.ForwardDiagonal);
            g.FillRectangle(bg, heroPanel.ClientRectangle);

            // Decorative arc glow
            using var glowPen = new Pen(Color.FromArgb(30, 99, 190, 255), 80);
            g.DrawEllipse(glowPen, -60, -60, 280, 280);

            // Bottom divider line
            using var divider = new LinearGradientBrush(
                new Rectangle(0, heroPanel.Height - 2, heroPanel.Width, 2),
                Color.Transparent,
                Color.FromArgb(80, 99, 179, 255),
                LinearGradientMode.Horizontal);
            g.FillRectangle(divider, 0, heroPanel.Height - 2, heroPanel.Width, 2);
        };

        // Logo circle
        var logoPanel = new Panel
        {
            Size = new Size(90, 90),
            Location = new Point((520 - 90) / 2, 28),
            BackColor = Color.Transparent,
        };
        logoPanel.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Outer glow ring
            using var glowBrush = new SolidBrush(Color.FromArgb(40, 99, 179, 255));
            g.FillEllipse(glowBrush, 0, 0, 90, 90);

            // Inner circle gradient
            using var innerPath = new GraphicsPath();
            innerPath.AddEllipse(8, 8, 74, 74);
            using var innerGrad = new LinearGradientBrush(
                new Rectangle(8, 8, 74, 74),
                Color.FromArgb(41, 98, 181),
                Color.FromArgb(14, 52, 120),
                LinearGradientMode.ForwardDiagonal);
            g.FillPath(innerGrad, innerPath);

            // Border
            using var borderPen = new Pen(Color.FromArgb(99, 179, 255), 2);
            g.DrawEllipse(borderPen, 9, 9, 72, 72);

            // Logo text "GW"
            using var font = new Font("Cairo", 22F, FontStyle.Bold);
            var txt = "GW";
            var sz = g.MeasureString(txt, font);
            g.DrawString(txt, font, Brushes.White,
                (90 - sz.Width) / 2, (90 - sz.Height) / 2);
        };
        heroPanel.Controls.Add(logoPanel);

        // App title
        var lblAppName = new Label
        {
            Text = "نظام جريس واي المحاسبي",
            Font = new Font("Cairo", 17F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            Size = new Size(520, 32),
            Location = new Point(0, 128),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
        };
        heroPanel.Controls.Add(lblAppName);

        // Version badge
        var versionBadge = new Panel
        {
            Size = new Size(110, 28),
            Location = new Point((520 - 110) / 2, 166),
            BackColor = Color.Transparent,
        };
        versionBadge.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = RoundedRect(new Rectangle(0, 0, versionBadge.Width, versionBadge.Height), 14);
            using var fill = new SolidBrush(Color.FromArgb(60, 99, 179, 255));
            g.FillPath(fill, path);
            using var pen = new Pen(Color.FromArgb(99, 179, 255), 1);
            g.DrawPath(pen, path);
        };
        var lblVersion = new Label
        {
            Text = "الإصدار  1.0.0",
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(150, 210, 255),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
        };
        versionBadge.Controls.Add(lblVersion);
        heroPanel.Controls.Add(versionBadge);

        // ── Info Cards Section ─────────────────────────────────────────
        var cardsPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(15, 23, 42),
            Padding = new Padding(24, 18, 24, 0),
        };

        var rows = new (string Icon, string Label, string Value)[]
        {
            ("📅", "تاريخ الإصدار",   "2025"),
            ("🏢", "المطوّر",          "GraceWay Technology"),
            ("📧", "البريد الإلكتروني","support@graceway.com"),
            ("📞", "الهاتف",           "+20 100 000 0000"),
            ("🗄️", "قاعدة البيانات",   "PostgreSQL"),
            ("⚙️", "البيئة",           ".NET 8 · Windows Forms"),
            ("📜", "الترخيص",          "للاستخدام التجاري"),
        };

        int y = 14;
        foreach (var (icon, label, value) in rows)
        {
            var card = CreateInfoCard(icon, label, value, y);
            cardsPanel.Controls.Add(card);
            y += 46;
        }

        // ── Footer ────────────────────────────────────────────────────
        var footerPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 64,
            BackColor = Color.FromArgb(10, 16, 34),
        };
        footerPanel.Paint += (s, e) =>
        {
            using var topLine = new LinearGradientBrush(
                new Rectangle(0, 0, footerPanel.Width, 1),
                Color.Transparent,
                Color.FromArgb(60, 99, 179, 255),
                LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(topLine, 0, 0, footerPanel.Width, 1);
        };

        var lblCopyright = new Label
        {
            Text = $"© {DateTime.Now.Year} GraceWay Technology · جميع الحقوق محفوظة",
            Font = new Font("Cairo", 8.5F),
            ForeColor = Color.FromArgb(80, 120, 170),
            Dock = DockStyle.Left,
            Width = 340,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
        };
        footerPanel.Controls.Add(lblCopyright);

        var btnClose = new Button
        {
            Text = "إغلاق",
            Size = new Size(110, 38),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(41, 98, 181),
            Cursor = Cursors.Hand,
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Location = new Point(footerPanel.Width - 130, (64 - 38) / 2);
        btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnClose.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = RoundedRect(new Rectangle(0, 0, btnClose.Width, btnClose.Height), 10);
            using var brush = new SolidBrush(btnClose.BackColor);
            g.FillPath(brush, path);
            btnClose.Region = new Region(path);
            var sz = g.MeasureString(btnClose.Text, btnClose.Font);
            g.DrawString(btnClose.Text, btnClose.Font, Brushes.White,
                (btnClose.Width - sz.Width) / 2, (btnClose.Height - sz.Height) / 2);
        };
        btnClose.MouseEnter += (s, e) => { btnClose.BackColor = Color.FromArgb(60, 130, 220); btnClose.Invalidate(); };
        btnClose.MouseLeave += (s, e) => { btnClose.BackColor = Color.FromArgb(41, 98, 181); btnClose.Invalidate(); };
        btnClose.Click += (s, e) => this.Close();
        footerPanel.Controls.Add(btnClose);

        this.Controls.Add(footerPanel);
        this.Controls.Add(cardsPanel);
        this.Controls.Add(heroPanel);
    }

    private Panel CreateInfoCard(string icon, string label, string value, int yPos)
    {
        var card = new Panel
        {
            Size = new Size(470, 38),
            Location = new Point(0, yPos),
            BackColor = Color.Transparent,
        };
        card.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = RoundedRect(new Rectangle(0, 0, card.Width, card.Height), 8);
            using var fill = new SolidBrush(Color.FromArgb(22, 36, 62));
            g.FillPath(fill, path);
            using var border = new Pen(Color.FromArgb(35, 55, 90), 1);
            g.DrawPath(border, path);
        };

        // Icon
        var lblIcon = new Label
        {
            Text = icon,
            Font = new Font("Segoe UI Emoji", 13F),
            Size = new Size(36, 38),
            Location = new Point(6, 0),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
            ForeColor = Color.White,
        };
        card.Controls.Add(lblIcon);

        // Label
        var lblKey = new Label
        {
            Text = label,
            Font = new Font("Cairo", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(120, 170, 230),
            Size = new Size(140, 38),
            Location = new Point(44, 0),
            TextAlign = ContentAlignment.MiddleRight,
            BackColor = Color.Transparent,
        };
        card.Controls.Add(lblKey);

        // Separator dot
        var dot = new Label
        {
            Text = ":",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(60, 100, 160),
            Size = new Size(12, 38),
            Location = new Point(186, 0),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
        };
        card.Controls.Add(dot);

        // Value
        var lblVal = new Label
        {
            Text = value,
            Font = new Font("Cairo", 9.5F),
            ForeColor = Color.FromArgb(210, 225, 245),
            Size = new Size(270, 38),
            Location = new Point(198, 0),
            TextAlign = ContentAlignment.MiddleRight,
            BackColor = Color.Transparent,
        };
        card.Controls.Add(lblVal);

        // Hover effect
        void applyHover(bool on)
        {
            var c = on ? Color.FromArgb(30, 50, 85) : Color.FromArgb(22, 36, 62);
            foreach (Control ctrl in card.Controls)
                if (ctrl is Label lbl)
                    lbl.BackColor = Color.Transparent;
            card.Tag = on;
            card.Invalidate();
        }
        card.MouseEnter += (s, e) => applyHover(true);
        card.MouseLeave += (s, e) => applyHover(false);
        foreach (Control c in card.Controls)
        {
            c.MouseEnter += (s, e) => applyHover(true);
            c.MouseLeave += (s, e) => applyHover(false);
        }

        return card;
    }

    private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
