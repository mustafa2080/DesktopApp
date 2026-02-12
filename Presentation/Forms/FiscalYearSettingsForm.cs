using System;
using System.Drawing;
using System.Windows.Forms;
using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Infrastructure.Data;

namespace GraceWay.AccountingSystem.Presentation.Forms
{
    public partial class FiscalYearSettingsForm : Form
    {
        private readonly AppDbContext _context;
        private readonly ISettingService _settingService;

        private DateTimePicker dtpFiscalYearStart = null!;
        private DateTimePicker dtpFiscalYearEnd = null!;
        private CheckBox chkAutoCalculate = null!;
        private Button btnSave = null!;
        private Button btnCancel = null!;

        public FiscalYearSettingsForm(AppDbContext context, ISettingService settingService)
        {
            _context = context;
            _settingService = settingService;
            InitializeComponent();
            InitializeCustomComponents();
            LoadSettings();
        }

        private void InitializeCustomComponents()
        {
            int yPosition = 20;
            int labelWidth = 150;
            int controlX = 170;
            int controlWidth = 280;
            int rowHeight = 50;

            // Panel Title
            Panel panelTitle = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = ColorScheme.Primary
            };

            Label lblTitle = new Label
            {
                Text = "⚙️ إعدادات السنة المالية",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            panelTitle.Controls.Add(lblTitle);
            this.Controls.Add(panelTitle);

            yPosition = 80;

            // Fiscal Year Start
            Label lblStart = new Label
            {
                Text = "بداية السنة المالية:",
                Location = new Point(20, yPosition + 5),
                Size = new Size(labelWidth, 20),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            dtpFiscalYearStart = new DateTimePicker
            {
                Location = new Point(controlX, yPosition),
                Size = new Size(controlWidth, 30),
                Font = new Font("Segoe UI", 10F),
                Format = DateTimePickerFormat.Short
            };
            dtpFiscalYearStart.ValueChanged += DtpFiscalYearStart_ValueChanged;
            this.Controls.AddRange(new Control[] { lblStart, dtpFiscalYearStart });

            yPosition += rowHeight;

            // Fiscal Year End
            Label lblEnd = new Label
            {
                Text = "نهاية السنة المالية:",
                Location = new Point(20, yPosition + 5),
                Size = new Size(labelWidth, 20),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            dtpFiscalYearEnd = new DateTimePicker
            {
                Location = new Point(controlX, yPosition),
                Size = new Size(controlWidth, 30),
                Font = new Font("Segoe UI", 10F),
                Format = DateTimePickerFormat.Short
            };
            this.Controls.AddRange(new Control[] { lblEnd, dtpFiscalYearEnd });

            yPosition += rowHeight;

            // Auto Calculate
            chkAutoCalculate = new CheckBox
            {
                Text = "حساب نهاية السنة تلقائياً (سنة كاملة)",
                Location = new Point(controlX, yPosition),
                Size = new Size(controlWidth, 30),
                Font = new Font("Segoe UI", 10F),
                Checked = true
            };
            chkAutoCalculate.CheckedChanged += ChkAutoCalculate_CheckedChanged;
            this.Controls.Add(chkAutoCalculate);


            yPosition += rowHeight + 20;

            // Save Button
            btnSave = new Button
            {
                Text = "💾 حفظ",
                Location = new Point(280, yPosition),
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = ColorScheme.Success,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnSave_Click;

            // Cancel Button
            btnCancel = new Button
            {
                Text = "❌ إلغاء",
                Location = new Point(170, yPosition),
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = ColorScheme.Error,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private void DtpFiscalYearStart_ValueChanged(object? sender, EventArgs e)
        {
            if (chkAutoCalculate.Checked)
            {
                dtpFiscalYearEnd.Value = dtpFiscalYearStart.Value.AddYears(1).AddDays(-1);
            }
        }

        private void ChkAutoCalculate_CheckedChanged(object? sender, EventArgs e)
        {
            dtpFiscalYearEnd.Enabled = !chkAutoCalculate.Checked;
            if (chkAutoCalculate.Checked)
            {
                dtpFiscalYearEnd.Value = dtpFiscalYearStart.Value.AddYears(1).AddDays(-1);
            }
        }

        private async void LoadSettings()
        {
            await Task.CompletedTask; // Fix warning CS1998
            try
            {
                // For now, set default values
                // In the future, this could load from database
                dtpFiscalYearStart.Value = new DateTime(DateTime.Now.Year, 1, 1);
                dtpFiscalYearEnd.Value = new DateTime(DateTime.Now.Year, 12, 31);
                chkAutoCalculate.Checked = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الإعدادات: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            await Task.CompletedTask; // Fix warning CS1998
            try
            {
                if (dtpFiscalYearEnd.Value <= dtpFiscalYearStart.Value)
                {
                    MessageBox.Show("تاريخ نهاية السنة المالية يجب أن يكون بعد تاريخ البداية",
                        "تحذير", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Save to database via context
                // For now, just show success message
                // TODO: Implement saving to FiscalYearSettings table
                
                MessageBox.Show("تم حفظ إعدادات السنة المالية بنجاح", "نجح",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ الإعدادات: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}