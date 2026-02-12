using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Presentation;

namespace GraceWay.AccountingSystem.Presentation.Forms
{
    public partial class ServiceTypesForm : Form
    {
        private readonly IReservationService _reservationService;
        
        private DataGridView dgvServiceTypes = null!;
        private Button btnAdd = null!;
        private Button btnEdit = null!;
        private Button btnDelete = null!;
        private Button btnToggleActive = null!;
        private Button btnRefresh = null!;
        private TextBox txtSearch = null!;
        private Label lblTitle = null!;
        private Label lblSearch = null!;
        private CheckBox chkShowInactive = null!;

        public ServiceTypesForm(IReservationService reservationService)
        {
            _reservationService = reservationService;
            
            InitializeComponent();
            InitializeFormControls();
            _ = LoadServiceTypesAsync();
        }

        private void InitializeFormControls()
        {
            this.BackColor = Color.White;
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "إدارة أنواع الخدمات";
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // Title
            lblTitle = new Label
            {
                Text = "📋 إدارة أنواع الخدمات",
                Location = new Point(400, 20),
                Size = new Size(250, 40),
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Search
            lblSearch = new Label
            {
                Text = "بحث:",
                Location = new Point(850, 80),
                Size = new Size(50, 25),
                Font = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.MiddleLeft
            };

            txtSearch = new TextBox
            {
                Location = new Point(600, 77),
                Size = new Size(240, 25),
                Font = new Font("Segoe UI", 10F),
                RightToLeft = RightToLeft.Yes
            };
            txtSearch.TextChanged += (s, e) => _ = LoadServiceTypesAsync();

            // Show Inactive checkbox
            chkShowInactive = new CheckBox
            {
                Text = "عرض المعطلة",
                Location = new Point(450, 80),
                Size = new Size(120, 25),
                Font = new Font("Segoe UI", 10F),
                Checked = false
            };
            chkShowInactive.CheckedChanged += (s, e) => _ = LoadServiceTypesAsync();

            // DataGridView
            dgvServiceTypes = new DataGridView
            {
                Location = new Point(20, 120),
                Size = new Size(960, 380),
                Font = new Font("Segoe UI", 10F),
                RightToLeft = RightToLeft.Yes,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };
            dgvServiceTypes.DoubleClick += BtnEdit_Click;

            // Buttons
            btnAdd = CreateButton("إضافة نوع خدمة", 800, 520, 150, 40, ColorScheme.Success);
            btnAdd.Click += BtnAdd_Click;

            btnEdit = CreateButton("تعديل", 630, 520, 150, 40, ColorScheme.Primary);
            btnEdit.Click += BtnEdit_Click;

            btnDelete = CreateButton("حذف", 460, 520, 150, 40, ColorScheme.Error);
            btnDelete.Click += BtnDelete_Click;

            btnToggleActive = CreateButton("تفعيل/تعطيل", 290, 520, 150, 40, ColorScheme.Warning);
            btnToggleActive.Click += BtnToggleActive_Click;

            btnRefresh = CreateButton("تحديث", 120, 520, 150, 40, ColorScheme.Secondary);
            btnRefresh.Click += (s, e) => _ = LoadServiceTypesAsync();

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                lblTitle, lblSearch, txtSearch, chkShowInactive,
                dgvServiceTypes, btnAdd, btnEdit, btnDelete, btnToggleActive, btnRefresh
            });
        }

        private async Task LoadServiceTypesAsync()
        {
            try
            {
                var allServiceTypes = await _reservationService.GetAllServiceTypesAsync();
                
                var query = allServiceTypes.AsQueryable();

                // Filter by search term
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    string searchTerm = txtSearch.Text.ToLower();
                    query = query.Where(st => 
                        st.ServiceTypeName.ToLower().Contains(searchTerm) ||
                        (st.ServiceTypeNameEn != null && st.ServiceTypeNameEn.ToLower().Contains(searchTerm)) ||
                        (st.Description != null && st.Description.ToLower().Contains(searchTerm))
                    );
                }

                // Filter by active status
                if (!chkShowInactive.Checked)
                {
                    query = query.Where(st => st.IsActive);
                }

                var serviceTypes = query.OrderBy(st => st.ServiceTypeName).ToList();

                dgvServiceTypes.DataSource = serviceTypes.Select(st => new
                {
                    st.ServiceTypeId,
                    اسم_الخدمة = st.ServiceTypeName,
                    الاسم_بالإنجليزية = st.ServiceTypeNameEn,
                    الوصف = st.Description,
                    الحالة = st.IsActive ? "نشط" : "معطل",
                    عدد_الحجوزات = st.Reservations?.Count ?? 0
                }).ToList();

                if (dgvServiceTypes.Columns["ServiceTypeId"] != null)
                {
                    dgvServiceTypes.Columns["ServiceTypeId"].Visible = false;
                }

                // Color rows based on status
                foreach (DataGridViewRow row in dgvServiceTypes.Rows)
                {
                    var statusCell = row.Cells["الحالة"];
                    if (statusCell?.Value == null) continue;
                    var status = statusCell?.Value?.ToString();
                    if (status == "معطل")
                    {
                        row.DefaultCellStyle.ForeColor = Color.Gray;
                        row.DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل أنواع الخدمات: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var form = new AddEditServiceTypeForm(_reservationService);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _ = LoadServiceTypesAsync();
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvServiceTypes.SelectedRows.Count == 0)
            {
                MessageBox.Show("يرجى اختيار نوع خدمة للتعديل", "تنبيه", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int serviceTypeId = (int)dgvServiceTypes.SelectedRows[0].Cells["ServiceTypeId"].Value!;
            var form = new AddEditServiceTypeForm(_reservationService, serviceTypeId);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _ = LoadServiceTypesAsync();
            }
        }

        private async void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvServiceTypes.SelectedRows.Count == 0)
            {
                MessageBox.Show("يرجى اختيار نوع خدمة للحذف", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                "هل أنت متأكد من حذف نوع الخدمة المحدد؟\n\nملاحظة: لا يمكن حذف نوع خدمة له حجوزات.",
                "تأكيد الحذف",
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    int serviceTypeId = (int)dgvServiceTypes.SelectedRows[0].Cells["ServiceTypeId"].Value!;
                    bool deleted = await _reservationService.DeleteServiceTypeAsync(serviceTypeId);
                    
                    if (deleted)
                    {
                        MessageBox.Show("تم حذف نوع الخدمة بنجاح", "نجح",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadServiceTypesAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في حذف نوع الخدمة: {ex.Message}", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnToggleActive_Click(object? sender, EventArgs e)
        {
            if (dgvServiceTypes.SelectedRows.Count == 0)
            {
                MessageBox.Show("يرجى اختيار نوع خدمة", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int serviceTypeId = (int)dgvServiceTypes.SelectedRows[0].Cells["ServiceTypeId"].Value!;
                var serviceType = await _reservationService.GetServiceTypeByIdAsync(serviceTypeId);
                
                if (serviceType != null)
                {
                    serviceType.IsActive = !serviceType.IsActive;
                    await _reservationService.UpdateServiceTypeAsync(serviceType);
                    
                    string status = serviceType.IsActive ? "تفعيل" : "تعطيل";
                    MessageBox.Show($"تم {status} نوع الخدمة بنجاح", "نجح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadServiceTypesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تغيير حالة نوع الخدمة: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Button CreateButton(string text, int x, int y, int width, int height, Color backColor)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }
    }
}
