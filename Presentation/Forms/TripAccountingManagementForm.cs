using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace GraceWay.AccountingSystem.Presentation.Forms;

/// <summary>
/// نموذج إدارة حسابات الرحلات - قسم الحجوزات (الحسابات)
/// يعرض ملف الرحلة الكامل بكل حساباته ويتحكم في قفل/فتح الرحلة للتعديل
/// </summary>
public partial class TripAccountingManagementForm : Form
{
    private readonly ITripService _tripService;
    private readonly IServiceProvider _serviceProvider;
    
    // Controls
    private Label lblTitle = null!;
    private Label lblSearch = null!;
    private TextBox txtSearch = null!;
    private Label lblStatus = null!;
    private ComboBox cmbStatusFilter = null!;
    private DataGridView dgvTrips = null!;
    private Button btnViewDetails = null!;
    private Button btnLockTrip = null!;
    private Button btnUnlockTrip = null!;
    private Button btnConfirmTrip = null!;
    private Button btnCancelTrip = null!;
    private Button btnRefresh = null!;
    
    public TripAccountingManagementForm(ITripService tripService, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _tripService = tripService;
        _serviceProvider = serviceProvider;
        
        InitializeFormControls();
        LoadTripsAsync();
    }
    
    private void InitializeFormControls()
    {
        this.Text = "إدارة حسابات الرحلات - قسم الحجوزات";
        this.Size = new Size(1400, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        
        // العنوان
        lblTitle = new Label
        {
            Text = "إدارة حسابات الرحلات",
            Location = new Point(550, 20),
            Size = new Size(300, 40),
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            TextAlign = ContentAlignment.MiddleCenter
        };
        
        // البحث
        lblSearch = new Label
        {
            Text = "بحث:",
            Location = new Point(1250, 80),
            Size = new Size(50, 25),
            Font = new Font("Segoe UI", 11F),
            TextAlign = ContentAlignment.MiddleLeft
        };
        
        txtSearch = new TextBox
        {
            Location = new Point(950, 77),
            Size = new Size(290, 28),
            Font = new Font("Segoe UI", 11F)
        };
        txtSearch.TextChanged += (s, e) => LoadTripsAsync();
        
        // فلتر الحالة
        lblStatus = new Label
        {
            Text = "الحالة:",
            Location = new Point(850, 80),
            Size = new Size(60, 25),
            Font = new Font("Segoe UI", 11F),
            TextAlign = ContentAlignment.MiddleLeft
        };
        
        cmbStatusFilter = new ComboBox
        {
            Location = new Point(600, 77),
            Size = new Size(240, 28),
            Font = new Font("Segoe UI", 11F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbStatusFilter.Items.AddRange(new object[] 
        { 
            "الكل", 
            "مسودة", 
            "غير مؤكدة", 
            "مؤكدة", 
            "جاري التنفيذ", 
            "مكتملة", 
            "ملغاة" 
        });
        cmbStatusFilter.SelectedIndex = 0;
        cmbStatusFilter.SelectedIndexChanged += (s, e) => LoadTripsAsync();
        
        // DataGridView
        dgvTrips = new DataGridView
        {
            Location = new Point(20, 120),
            Size = new Size(1340, 520),
            Font = new Font("Segoe UI", 10F),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(245, 245, 245) }
        };
        dgvTrips.CellFormatting += DgvTrips_CellFormatting;
        dgvTrips.DoubleClick += BtnViewDetails_Click;
        
        // الأزرار
        btnViewDetails = CreateButton("عرض التفاصيل الكاملة", 1160, 660, 180, 45, ColorScheme.Primary);
        btnViewDetails.Click += BtnViewDetails_Click;
        
        btnLockTrip = CreateButton("🔒 قفل الرحلة", 960, 660, 180, 45, ColorScheme.Warning);
        btnLockTrip.Click += BtnLockTrip_Click;
        
        btnUnlockTrip = CreateButton("🔓 فتح الرحلة", 760, 660, 180, 45, ColorScheme.Info);
        btnUnlockTrip.Click += BtnUnlockTrip_Click;
        
        btnConfirmTrip = CreateButton("✓ تأكيد الرحلة", 560, 660, 180, 45, ColorScheme.Success);
        btnConfirmTrip.Click += BtnConfirmTrip_Click;
        
        btnCancelTrip = CreateButton("✗ إلغاء الرحلة", 360, 660, 180, 45, ColorScheme.Error);
        btnCancelTrip.Click += BtnCancelTrip_Click;
        
        btnRefresh = CreateButton("⟳ تحديث", 160, 660, 180, 45, ColorScheme.Secondary);
        btnRefresh.Click += (s, e) => LoadTripsAsync();
        
        // إضافة الأدوات للنموذج
        this.Controls.AddRange(new Control[]
        {
            lblTitle, lblSearch, txtSearch, lblStatus, cmbStatusFilter,
            dgvTrips, btnViewDetails, btnLockTrip, btnUnlockTrip, 
            btnConfirmTrip, btnCancelTrip, btnRefresh
        });
    }
    
    private async void LoadTripsAsync()
    {
        try
        {
            this.Cursor = Cursors.WaitCursor;
            DisableButtons();
            
            string? searchTerm = string.IsNullOrWhiteSpace(txtSearch.Text) ? null : txtSearch.Text;
            
            // تحويل الحالة من العربي للـ Enum
            TripStatus? statusFilter = cmbStatusFilter.SelectedIndex switch
            {
                1 => TripStatus.Draft,
                2 => TripStatus.Unconfirmed,
                3 => TripStatus.Confirmed,
                4 => TripStatus.InProgress,
                5 => TripStatus.Completed,
                6 => TripStatus.Cancelled,
                _ => null
            };
            
            var trips = await _tripService.GetAllTripsAsync();
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                trips = trips.Where(t => 
                    t.TripName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    t.TripNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    t.Destination.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
            
            if (statusFilter.HasValue)
            {
                trips = trips.Where(t => t.Status == statusFilter.Value).ToList();
            }
            
            var dataSource = trips.Select(t => new
            {
                t.TripId,
                رقم_الرحلة = t.TripNumber,
                اسم_الرحلة = t.TripName,
                الوجهة = t.Destination,
                تاريخ_البدء = t.StartDate.ToString("yyyy-MM-dd"),
                تاريخ_الانتهاء = t.EndDate.ToString("yyyy-MM-dd"),
                الأيام = t.TotalDays,
                السعة = t.TotalCapacity,
                المحجوز = t.BookedSeats,
                الإشغال = $"{t.OccupancyRate:F1}%",
                إجمالي_التكلفة = t.TotalCost.ToString("N2"),
                إجمالي_الإيراد = t.TotalRevenue.ToString("N2"),
                صافي_الربح = t.NetProfit.ToString("N2"),
                هامش_الربح = $"{t.ProfitMargin:F1}%",
                الحالة = GetStatusInArabic(t.Status),
                مقفولة = t.IsLockedForTrips ? "نعم" : "لا"
            }).ToList();
            
            dgvTrips.DataSource = dataSource;
            
            // إخفاء TripId
            if (dgvTrips.Columns["TripId"] != null)
            {
                dgvTrips.Columns["TripId"].Visible = false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل الرحلات:\n{ex.Message}", 
                "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            this.Cursor = Cursors.Default;
            EnableButtons();
        }
    }
    
    private void DgvTrips_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (dgvTrips.Columns[e.ColumnIndex].Name == "الحالة" && e.Value != null)
        {
            string status = e.Value.ToString() ?? "";
            
            e.CellStyle.BackColor = status switch
            {
                "مسودة" => Color.FromArgb(240, 240, 240),
                "غير مؤكدة" => Color.FromArgb(255, 245, 220),
                "مؤكدة" => Color.FromArgb(220, 255, 220),
                "جاري التنفيذ" => Color.FromArgb(220, 240, 255),
                "مكتملة" => Color.FromArgb(200, 230, 200),
                "ملغاة" => Color.FromArgb(255, 220, 220),
                _ => Color.White
            };
            
            e.CellStyle.ForeColor = status switch
            {
                "مسودة" => Color.Gray,
                "غير مؤكدة" => Color.DarkOrange,
                "مؤكدة" => Color.DarkGreen,
                "جاري التنفيذ" => Color.DarkBlue,
                "مكتملة" => Color.DarkGreen,
                "ملغاة" => Color.DarkRed,
                _ => Color.Black
            };
            
            e.CellStyle.Font = new Font(dgvTrips.Font, FontStyle.Bold);
        }
        
        if (dgvTrips.Columns[e.ColumnIndex].Name == "مقفولة" && e.Value != null)
        {
            string locked = e.Value.ToString() ?? "";
            e.CellStyle.BackColor = locked == "نعم" ? Color.FromArgb(255, 230, 230) : Color.FromArgb(230, 255, 230);
            e.CellStyle.ForeColor = locked == "نعم" ? Color.DarkRed : Color.DarkGreen;
            e.CellStyle.Font = new Font(dgvTrips.Font, FontStyle.Bold);
        }
    }
    
    private async void BtnViewDetails_Click(object? sender, EventArgs e)
    {
        if (dgvTrips.SelectedRows.Count == 0)
        {
            MessageBox.Show("يرجى اختيار رحلة لعرض تفاصيلها", 
                "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        try
        {
            var tripIdValue = dgvTrips.SelectedRows[0].Cells["TripId"].Value;
            if (tripIdValue == null) return;
            
            int tripId = (int)tripIdValue;
            
            // جلب تفاصيل الرحلة
            var trip = await _tripService.GetTripByIdAsync(tripId);
            if (trip == null)
            {
                MessageBox.Show("لم يتم العثور على الرحلة",
                    "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            // فتح نموذج التفاصيل المالية الكاملة
            var detailsForm = new TripFinancialDetailsForm(trip, _tripService);
            detailsForm.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في عرض التفاصيل:\n{ex.Message}",
                "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async void BtnLockTrip_Click(object? sender, EventArgs e)
    {
        if (dgvTrips.SelectedRows.Count == 0)
        {
            MessageBox.Show("يرجى اختيار رحلة لقفلها", 
                "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        var tripIdValue = dgvTrips.SelectedRows[0].Cells["TripId"].Value;
        if (tripIdValue == null) return;
        
        int tripId = (int)tripIdValue;
        string tripName = dgvTrips.SelectedRows[0].Cells["اسم_الرحلة"].Value?.ToString() ?? "";
        
        var result = MessageBox.Show(
            $"هل تريد قفل الرحلة '{tripName}' من التعديل في قسم الرحلات؟\n\n" +
            "بعد القفل، لن يتمكن قسم الرحلات من تعديل بيانات الرحلة.",
            "تأكيد القفل",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        
        if (result == DialogResult.Yes)
        {
            try
            {
                var trip = await _tripService.GetTripByIdAsync(tripId);
                if (trip != null)
                {
                    trip.LockForTrips();
                    await _tripService.UpdateTripAsync(trip);
                    
                    MessageBox.Show("تم قفل الرحلة بنجاح", 
                        "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadTripsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في قفل الرحلة:\n{ex.Message}", 
                    "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    
    private async void BtnUnlockTrip_Click(object? sender, EventArgs e)
    {
        if (dgvTrips.SelectedRows.Count == 0)
        {
            MessageBox.Show("يرجى اختيار رحلة لفتحها", 
                "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        var tripIdValue = dgvTrips.SelectedRows[0].Cells["TripId"].Value;
        if (tripIdValue == null) return;
        
        int tripId = (int)tripIdValue;
        string tripName = dgvTrips.SelectedRows[0].Cells["اسم_الرحلة"].Value?.ToString() ?? "";
        
        var result = MessageBox.Show(
            $"هل تريد فتح الرحلة '{tripName}' للتعديل في قسم الرحلات؟\n\n" +
            "بعد الفتح، سيتمكن قسم الرحلات من تعديل بيانات الرحلة.",
            "تأكيد الفتح",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        
        if (result == DialogResult.Yes)
        {
            try
            {
                var trip = await _tripService.GetTripByIdAsync(tripId);
                if (trip != null)
                {
                    trip.UnlockForTrips();
                    await _tripService.UpdateTripAsync(trip);
                    
                    MessageBox.Show("تم فتح الرحلة بنجاح", 
                        "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadTripsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح الرحلة:\n{ex.Message}", 
                    "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    
    private async void BtnConfirmTrip_Click(object? sender, EventArgs e)
    {
        if (dgvTrips.SelectedRows.Count == 0)
        {
            MessageBox.Show("يرجى اختيار رحلة لتأكيدها", 
                "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        var tripIdValue = dgvTrips.SelectedRows[0].Cells["TripId"].Value;
        if (tripIdValue == null) return;
        
        int tripId = (int)tripIdValue;
        string tripName = dgvTrips.SelectedRows[0].Cells["اسم_الرحلة"].Value?.ToString() ?? "";
        
        var result = MessageBox.Show(
            $"هل تريد تأكيد الرحلة '{tripName}'؟\n\n" +
            "بعد التأكيد:\n" +
            "• سيتم قفل الرحلة تلقائياً من التعديل في قسم الرحلات\n" +
            "• ستصبح الرحلة جاهزة للعمل عليها",
            "تأكيد الرحلة",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        
        if (result == DialogResult.Yes)
        {
            try
            {
                var trip = await _tripService.GetTripByIdAsync(tripId);
                if (trip != null)
                {
                    trip.Status = TripStatus.Confirmed;
                    trip.LockForTrips(); // قفل تلقائي عند التأكيد
                    await _tripService.UpdateTripAsync(trip);
                    
                    MessageBox.Show("تم تأكيد الرحلة بنجاح", 
                        "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadTripsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تأكيد الرحلة:\n{ex.Message}", 
                    "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    
    private async void BtnCancelTrip_Click(object? sender, EventArgs e)
    {
        if (dgvTrips.SelectedRows.Count == 0)
        {
            MessageBox.Show("يرجى اختيار رحلة لإلغائها", 
                "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        var tripIdValue = dgvTrips.SelectedRows[0].Cells["TripId"].Value;
        if (tripIdValue == null) return;
        
        int tripId = (int)tripIdValue;
        string tripName = dgvTrips.SelectedRows[0].Cells["اسم_الرحلة"].Value?.ToString() ?? "";
        
        var result = MessageBox.Show(
            $"هل تريد إلغاء الرحلة '{tripName}'؟\n\n" +
            "تحذير: الرحلة الملغاة لن تكون متاحة للعمل عليها.",
            "إلغاء الرحلة",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        
        if (result == DialogResult.Yes)
        {
            try
            {
                var trip = await _tripService.GetTripByIdAsync(tripId);
                if (trip != null)
                {
                    trip.Status = TripStatus.Cancelled;
                    await _tripService.UpdateTripAsync(trip);
                    
                    MessageBox.Show("تم إلغاء الرحلة بنجاح", 
                        "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadTripsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في إلغاء الرحلة:\n{ex.Message}", 
                    "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    
    private string GetStatusInArabic(TripStatus status)
    {
        return status switch
        {
            TripStatus.Draft => "مسودة",
            TripStatus.Unconfirmed => "غير مؤكدة",
            TripStatus.Confirmed => "مؤكدة",
            TripStatus.InProgress => "جاري التنفيذ",
            TripStatus.Completed => "مكتملة",
            TripStatus.Cancelled => "ملغاة",
            _ => status.ToString()
        };
    }
    
    private void DisableButtons()
    {
        btnViewDetails.Enabled = false;
        btnLockTrip.Enabled = false;
        btnUnlockTrip.Enabled = false;
        btnConfirmTrip.Enabled = false;
        btnCancelTrip.Enabled = false;
        btnRefresh.Enabled = false;
    }
    
    private void EnableButtons()
    {
        btnViewDetails.Enabled = true;
        btnLockTrip.Enabled = true;
        btnUnlockTrip.Enabled = true;
        btnConfirmTrip.Enabled = true;
        btnCancelTrip.Enabled = true;
        btnRefresh.Enabled = true;
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
