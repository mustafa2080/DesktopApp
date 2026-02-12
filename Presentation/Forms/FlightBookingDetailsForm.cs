using System;
using System.Drawing;
using System.Windows.Forms;
using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Presentation;

namespace GraceWay.AccountingSystem.Presentation.Forms
{
    public partial class FlightBookingDetailsForm : Form
    {
        private readonly FlightBooking _booking;
        private Panel mainPanel = null!;

        public FlightBookingDetailsForm(FlightBooking booking)
        {
            _booking = booking;
            InitializeComponent();
            InitializeFormControls();
            LoadBookingDetails();
        }

        private void InitializeFormControls()
        {
            this.Text = "تفاصيل حجز الطيران";
            this.Size = new Size(900, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // Title Panel
            var titlePanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(900, 70),
                BackColor = ColorScheme.Primary,
                Dock = DockStyle.Top
            };

            var lblTitle = new Label
            {
                Text = $"تفاصيل حجز الطيران - {_booking.BookingNumber}",
                Location = new Point(250, 15),
                Size = new Size(400, 40),
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            titlePanel.Controls.Add(lblTitle);

            // Main Scrollable Panel
            mainPanel = new Panel
            {
                Location = new Point(20, 90),
                Size = new Size(850, 560),
                AutoScroll = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Close Button
            var btnClose = new Button
            {
                Text = "إغلاق",
                Location = new Point(370, 665),
                Size = new Size(160, 45),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BackColor = ColorScheme.Secondary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { titlePanel, mainPanel, btnClose });
        }

        private void LoadBookingDetails()
        {
            int currentY = 20;
            int sectionSpacing = 25;

            // معلومات الحجز الأساسية
            currentY = AddSectionTitle("معلومات الحجز", currentY);
            currentY = AddDetailRow("رقم الحجز:", _booking.BookingNumber, currentY, true);
            currentY = AddDetailRow("تاريخ الإصدار:", _booking.IssuanceDate.ToString("dd/MM/yyyy"), currentY);
            currentY = AddDetailRow("تاريخ السفر:", _booking.TravelDate.ToString("dd/MM/yyyy"), currentY);
            currentY = AddDetailRow("حالة التذكرة:", _booking.TicketStatus, currentY, false, GetStatusColor(_booking.TicketStatus));
            currentY += sectionSpacing;

            // معلومات العميل
            currentY = AddSectionTitle("معلومات العميل", currentY);
            currentY = AddDetailRow("اسم العميل:", _booking.ClientName, currentY);
            currentY = AddDetailRow("رقم الموبايل:", _booking.MobileNumber, currentY);
            currentY = AddDetailRow("مسار العميل:", _booking.ClientRoute, currentY);
            currentY += sectionSpacing;

            // معلومات المورد والنظام
            currentY = AddSectionTitle("معلومات المورد", currentY);
            currentY = AddDetailRow("المورد:", _booking.Supplier, currentY);
            currentY = AddDetailRow("النظام:", _booking.System, currentY);
            currentY = AddDetailRow("طريقة الدفع:", _booking.PaymentMethod, currentY);
            currentY += sectionSpacing;

            // معلومات التذاكر
            currentY = AddSectionTitle("معلومات التذاكر", currentY);
            currentY = AddDetailRow("عدد التذاكر:", _booking.TicketCount.ToString(), currentY);
            currentY = AddDetailRow("أرقام التذاكر:", _booking.TicketNumbers, currentY);
            currentY += sectionSpacing;

            // المعلومات المالية
            currentY = AddSectionTitle("المعلومات المالية", currentY);
            currentY = AddDetailRow("سعر البيع:", $"{_booking.SellingPrice:N2} جنيه", currentY, false, Color.DarkBlue);
            currentY = AddDetailRow("السعر الصافي:", $"{_booking.NetPrice:N2} جنيه", currentY, false, Color.DarkOrange);
            
            // Profit with special styling
            var profitColor = _booking.Profit >= 0 ? ColorScheme.Success : ColorScheme.Error;
            var profitBg = _booking.Profit >= 0 ? Color.FromArgb(240, 255, 240) : Color.FromArgb(255, 240, 240);
            currentY = AddProfitRow("الربح:", $"{_booking.Profit:N2} جنيه", currentY, profitColor, profitBg);
            currentY += sectionSpacing;

            // الملاحظات
            if (!string.IsNullOrWhiteSpace(_booking.Notes))
            {
                currentY = AddSectionTitle("ملاحظات", currentY);
                currentY = AddNotesBox(_booking.Notes, currentY);
            }
        }

        private int AddSectionTitle(string title, int y)
        {
            var panel = new Panel
            {
                Location = new Point(20, y),
                Size = new Size(790, 40),
                BackColor = Color.FromArgb(240, 248, 255)
            };

            var label = new Label
            {
                Text = title,
                Location = new Point(10, 8),
                Size = new Size(770, 24),
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary
            };

            panel.Controls.Add(label);
            mainPanel.Controls.Add(panel);

            return y + 50;
        }

        private int AddDetailRow(string label, string value, int y, bool bold = false, Color? valueColor = null)
        {
            var lblLabel = new Label
            {
                Text = label,
                Location = new Point(650, y),
                Size = new Size(140, 30),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 70, 70),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblValue = new Label
            {
                Text = value,
                Location = new Point(50, y),
                Size = new Size(580, 30),
                Font = new Font("Segoe UI", 11F, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = valueColor ?? Color.FromArgb(50, 50, 50),
                TextAlign = ContentAlignment.MiddleRight
            };

            mainPanel.Controls.AddRange(new Control[] { lblLabel, lblValue });

            return y + 35;
        }

        private int AddProfitRow(string label, string value, int y, Color textColor, Color bgColor)
        {
            var lblLabel = new Label
            {
                Text = label,
                Location = new Point(650, y),
                Size = new Size(140, 40),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 70, 70),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var profitPanel = new Panel
            {
                Location = new Point(200, y),
                Size = new Size(430, 40),
                BackColor = bgColor,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblValue = new Label
            {
                Text = value,
                Location = new Point(10, 5),
                Size = new Size(410, 30),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleCenter
            };

            profitPanel.Controls.Add(lblValue);
            mainPanel.Controls.AddRange(new Control[] { lblLabel, profitPanel });

            return y + 50;
        }

        private int AddNotesBox(string notes, int y)
        {
            var txtNotes = new TextBox
            {
                Text = notes,
                Location = new Point(50, y),
                Size = new Size(740, 80),
                Font = new Font("Segoe UI", 10F),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.FixedSingle
            };

            mainPanel.Controls.Add(txtNotes);

            return y + 90;
        }

        private Color GetStatusColor(string status)
        {
            return status switch
            {
                "مؤكد" => ColorScheme.Success,
                "قيد الانتظار" => Color.Orange,
                "ملغي" => ColorScheme.Error,
                "مكتمل" => Color.Blue,
                _ => Color.Gray
            };
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(284, 261);
            this.Name = "FlightBookingDetailsForm";
            this.ResumeLayout(false);
        }
    }
}
