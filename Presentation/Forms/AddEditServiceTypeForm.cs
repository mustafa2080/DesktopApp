using System;
using System.Drawing;
using System.Windows.Forms;
using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Presentation;

namespace GraceWay.AccountingSystem.Presentation.Forms
{
    public partial class AddEditServiceTypeForm : Form
    {
        private readonly IReservationService _reservationService;
        private readonly int? _serviceTypeId;
        private ServiceType? _currentServiceType;

        private Label lblTitle = null!;
        private Label lblNameAr = null!;
        private TextBox txtNameAr = null!;
        private Label lblNameEn = null!;
        private TextBox txtNameEn = null!;
        private Label lblDescription = null!;
        private TextBox txtDescription = null!;
        private CheckBox chkIsActive = null!;
        private Button btnSave = null!;
        private Button btnCancel = null!;

        public AddEditServiceTypeForm(IReservationService reservationService, int? serviceTypeId = null)
        {
            _reservationService = reservationService;
            _serviceTypeId = serviceTypeId;
            
            InitializeComponent();
            InitializeFormControls();
            
            if (_serviceTypeId.HasValue)
            {
                _ = LoadServiceTypeAsync();
            }
        }

        private void InitializeFormControls()
        {
            this.BackColor = Color.White;
            this.Size = new Size(600, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = _serviceTypeId.HasValue ? "تعديل نوع خدمة" : "إضافة نوع خدمة";
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // Title
            lblTitle = new Label
            {
                Text = _serviceTypeId.HasValue ? "تعديل نوع خدمة" : "إضافة نوع خدمة جديد",
                Location = new Point(150, 20),
                Size = new Size(300, 40),
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Arabic Name
            lblNameAr = new Label
            {
                Text = "اسم الخدمة بالعربية: *",
                Location = new Point(400, 80),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            txtNameAr = new TextBox
            {
                Location = new Point(100, 77),
                Size = new Size(290, 25),
                Font = new Font("Segoe UI", 10F),
                RightToLeft = RightToLeft.Yes
            };

            // English Name
            lblNameEn = new Label
            {
                Text = "اسم الخدمة بالإنجليزية:",
                Location = new Point(400, 120),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10F)
            };

            txtNameEn = new TextBox
            {
                Location = new Point(100, 117),
                Size = new Size(290, 25),
                Font = new Font("Segoe UI", 10F),
                RightToLeft = RightToLeft.Yes
            };

            // Description
            lblDescription = new Label
            {
                Text = "الوصف:",
                Location = new Point(400, 160),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10F)
            };

            txtDescription = new TextBox
            {
                Location = new Point(100, 157),
                Size = new Size(450, 100),
                Font = new Font("Segoe UI", 10F),
                Multiline = true,
                RightToLeft = RightToLeft.Yes,
                ScrollBars = ScrollBars.Vertical
            };

            // Is Active
            chkIsActive = new CheckBox
            {
                Text = "نشط",
                Location = new Point(100, 280),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Checked = true
            };

            // Save Button
            btnSave = new Button
            {
                Text = "حفظ",
                Location = new Point(280, 340),
                Size = new Size(120, 40),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = ColorScheme.Success,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnSave_Click;

            // Cancel Button
            btnCancel = new Button
            {
                Text = "إلغاء",
                Location = new Point(140, 340),
                Size = new Size(120, 40),
                Font = new Font("Segoe UI", 11F),
                BackColor = ColorScheme.Secondary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => this.Close();

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                lblTitle, lblNameAr, txtNameAr, lblNameEn, txtNameEn,
                lblDescription, txtDescription, chkIsActive,
                btnSave, btnCancel
            });
        }

        private async Task LoadServiceTypeAsync()
        {
            try
            {
                _currentServiceType = await _reservationService.GetServiceTypeByIdAsync(_serviceTypeId!.Value);
                
                if (_currentServiceType != null)
                {
                    txtNameAr.Text = _currentServiceType.ServiceTypeName;
                    txtNameEn.Text = _currentServiceType.ServiceTypeNameEn;
                    txtDescription.Text = _currentServiceType.Description;
                    chkIsActive.Checked = _currentServiceType.IsActive;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل بيانات نوع الخدمة: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(txtNameAr.Text))
                {
                    MessageBox.Show("يجب إدخال اسم الخدمة بالعربية", "تنبيه",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtNameAr.Focus();
                    return;
                }

                var serviceType = new ServiceType
                {
                    ServiceTypeId = _currentServiceType?.ServiceTypeId ?? 0,
                    ServiceTypeName = txtNameAr.Text.Trim(),
                    ServiceTypeNameEn = string.IsNullOrWhiteSpace(txtNameEn.Text) ? null : txtNameEn.Text.Trim(),
                    Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim(),
                    IsActive = chkIsActive.Checked
                };

                if (_currentServiceType == null)
                {
                    // Create new
                    await _reservationService.CreateServiceTypeAsync(serviceType);
                    MessageBox.Show("تم إضافة نوع الخدمة بنجاح", "نجح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Update existing
                    await _reservationService.UpdateServiceTypeAsync(serviceType);
                    MessageBox.Show("تم تحديث نوع الخدمة بنجاح", "نجح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ نوع الخدمة: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
