using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Presentation.Forms
{
    public partial class TransactionDetailsReportForm : Form
    {
        private readonly CashTransaction _transaction;

        public TransactionDetailsReportForm(CashTransaction transaction)
        {
            _transaction = transaction;
            InitializeComponent();
            BuildUI();
            LoadTransactionDetails();
        }

        private void BuildUI()
        {
            this.Text = "تفاصيل البند";
            this.Size = new Size(650, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Cairo", 10F);
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            var wrapper = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.Transparent
            };
            this.Controls.Add(wrapper);

            // Header
            var headerPanel = CreateHeaderPanel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 80;
            wrapper.Controls.Add(headerPanel);

            // Content
            var contentPanel = CreateContentPanel();
            contentPanel.Dock = DockStyle.Fill;
            wrapper.Controls.Add(contentPanel);

            // Footer
            var footerPanel = CreateFooterPanel();
            footerPanel.Dock = DockStyle.Bottom;
            footerPanel.Height = 70;
            wrapper.Controls.Add(footerPanel);
        }

        private Panel CreateHeaderPanel()
        {
            var panel = new Panel
            {
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            panel.Paint += PaintCard;

            var isExpense = _transaction.Type == TransactionType.Expense;
            var icon = isExpense ? "💸" : "💰";
            var title = isExpense ? "تفاصيل مصروف" : "تفاصيل إيراد";
            var color = isExpense ? ColorScheme.Error : ColorScheme.Success;

            var lblTitle = new Label
            {
                Text = $"{icon} {title}",
                Font = new Font("Cairo", 16F, FontStyle.Bold),
                ForeColor = color,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            };
            panel.Controls.Add(lblTitle);

            return panel;
        }

        private Panel CreateContentPanel()
        {
            var panel = new Panel
            {
                BackColor = Color.White,
                Padding = new Padding(25),
                Margin = new Padding(0, 10, 0, 10)
            };
            panel.Paint += PaintCard;

            var tablePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 12,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                AutoScroll = true
            };

            tablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            tablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));

            for (int i = 0; i < 12; i++)
                tablePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

            // Add fields
            AddDetailRow(tablePanel, 0, "📋 رقم السند:", "lblVoucherNumber");
            AddDetailRow(tablePanel, 1, "📅 التاريخ:", "lblDate");
            AddDetailRow(tablePanel, 2, "⏰ الوقت:", "lblTime");
            AddDetailRow(tablePanel, 3, "🏷️ التصنيف:", "lblCategory");
            AddDetailRow(tablePanel, 4, "👤 الطرف:", "lblPartyName");
            AddDetailRow(tablePanel, 5, "📝 البيان:", "lblDescription");
            AddDetailRow(tablePanel, 6, "💳 طريقة الدفع:", "lblPaymentMethod");
            AddDetailRow(tablePanel, 7, "💵 المبلغ:", "lblAmount");
            AddDetailRow(tablePanel, 8, "💱 العملة:", "lblCurrency");
            AddDetailRow(tablePanel, 9, "📊 عمولة إنستا باي:", "lblCommission");
            AddDetailRow(tablePanel, 10, "💰 المبلغ الفعلي:", "lblActualAmount");
            AddDetailRow(tablePanel, 11, "👨‍💼 المستخدم:", "lblUser");

            panel.Controls.Add(tablePanel);
            return panel;
        }

        private void AddDetailRow(TableLayoutPanel table, int row, string labelText, string valueLabelName)
        {
            var lblField = new Label
            {
                Text = labelText,
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                ForeColor = ColorScheme.TextSecondary,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 0, 10, 0)
            };

            var lblValue = new Label
            {
                Name = valueLabelName,
                Text = "---",
                Font = new Font("Cairo", 11F),
                ForeColor = ColorScheme.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 0, 10, 0),
                AutoEllipsis = true
            };

            table.Controls.Add(lblField, 0, row);
            table.Controls.Add(lblValue, 1, row);
        }

        private Panel CreateFooterPanel()
        {
            var panel = new Panel
            {
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 0, 0)
            };

            var btnClose = new Button
            {
                Text = "إغلاق",
                Width = 150,
                Height = 45,
                BackColor = ColorScheme.Primary,
                ForeColor = Color.White,
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            btnClose.MouseEnter += (s, e) => btnClose.BackColor = ColorScheme.Darken(ColorScheme.Primary, 0.15f);
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = ColorScheme.Primary;

            panel.Controls.Add(btnClose);
            return panel;
        }

        private void LoadTransactionDetails()
        {
            FindLabel("lblVoucherNumber")!.Text = _transaction.VoucherNumber ?? "غير محدد";
            FindLabel("lblDate")!.Text = _transaction.TransactionDate.ToString("dd/MM/yyyy");
            FindLabel("lblTime")!.Text = _transaction.TransactionDate.ToString("hh:mm tt");
            FindLabel("lblCategory")!.Text = _transaction.Category ?? "غير محدد";
            FindLabel("lblPartyName")!.Text = _transaction.PartyName ?? "---";
            
            var lblDescription = FindLabel("lblDescription")!;
            lblDescription.Text = _transaction.Description ?? "---";
            lblDescription.AutoSize = false;
            lblDescription.Height = 80;
            
            FindLabel("lblPaymentMethod")!.Text = GetPaymentMethodName(_transaction.PaymentMethod);
            
            var isExpense = _transaction.Type == TransactionType.Expense;
            var amountColor = isExpense ? ColorScheme.Error : ColorScheme.Success;
            
            var lblAmount = FindLabel("lblAmount")!;
            lblAmount.Text = $"{_transaction.Amount:N2}";
            lblAmount.Font = new Font("Cairo", 13F, FontStyle.Bold);
            lblAmount.ForeColor = amountColor;
            
            FindLabel("lblCurrency")!.Text = GetCurrencyName(_transaction.TransactionCurrency);
            
            var lblCommission = FindLabel("lblCommission")!;
            if (_transaction.PaymentMethod == PaymentMethod.InstaPay && _transaction.InstaPayCommission.HasValue)
            {
                lblCommission.Text = $"{_transaction.InstaPayCommission.Value:N2} {GetCurrencyName(_transaction.TransactionCurrency)}";
                lblCommission.ForeColor = ColorScheme.Warning;
            }
            else
            {
                lblCommission.Text = "لا يوجد";
            }
            
            var lblActualAmount = FindLabel("lblActualAmount")!;
            decimal actualAmount = _transaction.Amount;
            if (_transaction.PaymentMethod == PaymentMethod.InstaPay && _transaction.InstaPayCommission.HasValue)
            {
                actualAmount = _transaction.Amount - _transaction.InstaPayCommission.Value;
            }
            lblActualAmount.Text = $"{actualAmount:N2} {GetCurrencyName(_transaction.TransactionCurrency)}";
            lblActualAmount.Font = new Font("Cairo", 13F, FontStyle.Bold);
            lblActualAmount.ForeColor = amountColor;
            
            FindLabel("lblUser")!.Text = _transaction.CreatedBy.ToString();
        }

        private string GetCurrencyName(string currencyCode)
        {
            return currencyCode?.ToUpper() switch
            {
                "EGP" => "جنيه مصري",
                "USD" => "دولار أمريكي",
                "EUR" => "يورو",
                "GBP" => "جنيه إسترليني",
                "SAR" => "ريال سعودي",
                _ => "جنيه مصري"
            };
        }

        private string GetPaymentMethodName(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Cash => "نقدي",
                PaymentMethod.Cheque => "شيك",
                PaymentMethod.BankTransfer => "تحويل بنكي",
                PaymentMethod.CreditCard or PaymentMethod.Card or PaymentMethod.Visa => "بطاقة",
                PaymentMethod.InstaPay => "إنستا باي",
                _ => "آخر"
            };
        }

        private Label? FindLabel(string name) =>
            this.Controls.Find(name, true).FirstOrDefault() as Label;

        private void PaintCard(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel p) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var shadowBrush = new SolidBrush(Color.FromArgb(15, 0, 0, 0));
            e.Graphics.FillRectangle(shadowBrush, 2, 3, p.Width - 2, p.Height);

            var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
            using var path = GetRoundedPath(rect, 10);
            using var brush = new SolidBrush(p.BackColor);
            e.Graphics.FillPath(brush, path);

            using var pen = new Pen(Color.FromArgb(220, 225, 230), 1);
            e.Graphics.DrawPath(pen, path);
        }

        private static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            var arc = new Rectangle(rect.X, rect.Y, radius * 2, radius * 2);

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - radius * 2;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - radius * 2;
            path.AddArc(arc, 0, 90);
            arc.X = rect.X;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
