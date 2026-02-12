using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace GraceWay.AccountingSystem.Presentation.Forms;

/// <summary>
/// قسم الحجوزات (الحسابات) - إدارة حسابات الرحلات
/// يعرض ملف الرحلة الكامل بكل حساباته ويتحكم في قفل/فتح الرحلة للتعديل
/// </summary>
public partial class ReservationsListForm : Form
{
    private readonly ITripService _tripService;
    private readonly IServiceProvider _serviceProvider;
    private readonly int _currentUserId;
    
    // Controls are declared in Designer.cs
    
    public ReservationsListForm(ITripService tripService, IServiceProvider serviceProvider, int currentUserId)
    {
        InitializeComponent();
        _tripService = tripService;
        _serviceProvider = serviceProvider;
        _currentUserId = currentUserId;
        
        InitializeFormControls();
        this.Load += async (s, e) => await LoadTripsAsync();
    }
    
    private void InitializeFormControls()
    {
        this.Text = "قسم الحجوزات - إدارة حسابات الرحلات";
        this.Size = new Size(1500, 850);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.Font = new Font("Cairo", 10F);
        
        // العنوان
        lblTitle = new Label
        {
            Text = "💼 قسم الحجوزات - إدارة حسابات الرحلات",
            Location = new Point(450, 20),
            Size = new Size(600, 45),
            Font = new Font("Cairo", 20F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            TextAlign = ContentAlignment.MiddleCenter
        };
        
        // البحث
        lblSearch = new Label
        {
            Text = "بحث:",
            Location = new Point(1350, 85),
            Size = new Size(60, 30),
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        
        txtSearch = new TextBox
        {
            Location = new Point(1050, 82),
            Size = new Size(290, 30),
            Font = new Font("Cairo", 11F),
            PlaceholderText = "رقم الرحلة، الاسم..."
        };
        txtSearch.TextChanged += (s, e) => _ = LoadTripsAsync();
        
        // فلتر الحالة
        lblStatus = new Label
        {
            Text = "الحالة:",
            Location = new Point(950, 85),
            Size = new Size(70, 30),
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        
        cmbStatusFilter = new ComboBox
        {
            Location = new Point(700, 82),
            Size = new Size(240, 30),
            Font = new Font("Cairo", 11F),
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
        cmbStatusFilter.SelectedIndexChanged += (s, e) => _ = LoadTripsAsync();
        
        // DataGridView
        dgvTrips = new DataGridView
        {
            Location = new Point(20, 130),
            Size = new Size(1440, 560),
            Font = new Font("Cairo", 10F),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            EnableHeadersVisualStyles = false,
            RowTemplate = { Height = 45 }
        };
        
        dgvTrips.ColumnHeadersDefaultCellStyle.BackColor = ColorScheme.Primary;
        dgvTrips.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvTrips.ColumnHeadersDefaultCellStyle.Font = new Font("Cairo", 11F, FontStyle.Bold);
        dgvTrips.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvTrips.ColumnHeadersHeight = 50;
        dgvTrips.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
        
        dgvTrips.CellFormatting += DgvTrips_CellFormatting;
        dgvTrips.DoubleClick += BtnViewDetails_Click;
        dgvTrips.SelectionChanged += (s, e) => UpdateButtonStates();
        
        SetupGridColumns();
        
        // الأزرار
        btnViewDetails = CreateButton("📋 عرض التفاصيل الكاملة", 1260, 710, 200, 50, ColorScheme.Primary);
        btnViewDetails.Click += BtnViewDetails_Click;
        
        btnLockTrip = CreateButton("🔒 قفل الرحلة", 1040, 710, 200, 50, ColorScheme.Warning);
        btnLockTrip.Click += BtnLockTrip_Click;
        
        btnUnlockTrip = CreateButton("🔓 فتح الرحلة", 820, 710, 200, 50, Color.FromArgb(52, 152, 219));
        btnUnlockTrip.Click += BtnUnlockTrip_Click;
        
        btnConfirmTrip = CreateButton("✅ تأكيد الرحلة", 600, 710, 200, 50, ColorScheme.Success);
        btnConfirmTrip.Click += BtnConfirmTrip_Click;
        
        btnCancelTrip = CreateButton("❌ إلغاء الرحلة", 380, 710, 200, 50, ColorScheme.Error);
        btnCancelTrip.Click += BtnCancelTrip_Click;
        
        btnRefresh = CreateButton("🔄 تحديث", 160, 710, 200, 50, ColorScheme.Secondary);
        btnRefresh.Click += (s, e) => _ = LoadTripsAsync();
        
        // إضافة الأدوات للنموذج
        this.Controls.AddRange(new Control[]
        {
            lblTitle, lblSearch, txtSearch, lblStatus, cmbStatusFilter,
            dgvTrips, btnViewDetails, btnLockTrip, btnUnlockTrip, 
            btnConfirmTrip, btnCancelTrip, btnRefresh
        });
    }
    
    private void SetupGridColumns()
    {
        dgvTrips.Columns.Clear();
        
        // Hidden ID
        dgvTrips.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TripId",
            DataPropertyName = "TripId",
            Visible = false
        });
        
        // رقم الرحلة
        dgvTrips.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TripNumber",
            DataPropertyName = "TripNumber",
            HeaderText = "رقم الرحلة",
            Width = 110
        });
        
        // اسم الرحلة
        dgvTrips.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TripName",
            DataPropertyName = "TripName",
            HeaderText = "اسم الرحلة",
            Width = 200
        });
        
        // الوجهة
        dgvTrips.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Destination",
            DataPropertyName = "Destination",
            HeaderText = "الوجهة",
            Width = 130
        });
        
        // التاريخ
        dgvTrips.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "StartDate",
            HeaderText = "التاريخ",
            Width = 180
        });
        
        // الأيام
        dgvTrips.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TotalDays",
            HeaderText = "الأيام",
            Width = 70
        });
        
        // السعة / المحجوز
        dgvTrips.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Capacity",
            HeaderText = "السعة/المحجوز",
            Width = 110
        });
        
        // التكلفة الإجمالية
        dgvTrips.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TotalCost",
            HeaderText = "التكلفة",
            Width = 120
        });
        
        // الإيراد المتوقع
        dgvTrips.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TotalRevenue",
            HeaderText = "الإيراد",
            Width = 120
        });
        
        // صافي الربح
        dgvTrips.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "NetProfit",
            HeaderText = "صافي الربح",
            Width = 130
        });
        
        // الحالة
        dgvTrips.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Status",
            HeaderText = "الحالة",
            Width = 120
        });
        
        // مقفولة؟
        dgvTrips.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "IsLocked",
            HeaderText = "مقفولة؟",
            Width = 90
        });
    }
    
    private async Task LoadTripsAsync()
    {
        try
        {
            this.Cursor = Cursors.WaitCursor;
            DisableButtons();
            
            string? searchTerm = string.IsNullOrWhiteSpace(txtSearch.Text) ? null : txtSearch.Text;
            
            // تحويل الحالة
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
            
            var trips = await _tripService.GetAllTripsAsync(includeDetails: true);
            
            // ✅ إخفاء الرحلات الغير مؤكدة من قسم الحجوزات
            // إلا إذا المستخدم اختار "غير مؤكدة" من الفلتر
            if (!statusFilter.HasValue || statusFilter.Value != TripStatus.Unconfirmed)
            {
                trips = trips.Where(t => t.Status != TripStatus.Unconfirmed).ToList();
            }
            
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
            
            dgvTrips.DataSource = trips;
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
        if (dgvTrips.Rows.Count == 0 || e.RowIndex < 0) return;
        
        try
        {
            var row = dgvTrips.Rows[e.RowIndex];
            if (row.DataBoundItem is not Trip trip) return;
            
            // ✅ لو الرحلة ملغية، نلون الصف كله باللون الأحمر الفاتح
            if (trip.Status == TripStatus.Cancelled)
            {
                e.CellStyle.BackColor = Color.FromArgb(255, 235, 238);
                e.CellStyle.ForeColor = Color.FromArgb(183, 28, 28);
            }
            
            var columnName = dgvTrips.Columns[e.ColumnIndex].Name;
            
            // التاريخ
            if (columnName == "StartDate")
            {
                e.Value = $"{trip.StartDate:yyyy-MM-dd} → {trip.EndDate:yyyy-MM-dd}";
                e.FormattingApplied = true;
            }
            
            // الأيام
            else if (columnName == "TotalDays")
            {
                e.Value = $"{trip.TotalDays} يوم";
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                e.FormattingApplied = true;
            }
            // السعة/المحجوز
            else if (columnName == "Capacity")
            {
                e.Value = $"{trip.BookedSeats}/{trip.TotalCapacity}";
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                
                if (trip.BookedSeats >= trip.TotalCapacity)
                    e.CellStyle.ForeColor = ColorScheme.Success;
                else if (trip.BookedSeats > trip.TotalCapacity / 2)
                    e.CellStyle.ForeColor = ColorScheme.Warning;
                    
                e.FormattingApplied = true;
            }
            // التكلفة
            else if (columnName == "TotalCost")
            {
                e.Value = $"{trip.TotalCost:N2} ج.م";
                e.CellStyle.ForeColor = Color.FromArgb(231, 76, 60);
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                e.FormattingApplied = true;
            }
            // الإيراد
            else if (columnName == "TotalRevenue")
            {
                e.Value = $"{trip.TotalRevenue:N2} ج.م";
                e.CellStyle.ForeColor = ColorScheme.Primary;
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                e.FormattingApplied = true;
            }
            
            // صافي الربح
            else if (columnName == "NetProfit")
            {
                if (trip.NetProfit > 0)
                {
                    e.Value = $"➕ {trip.NetProfit:N2} ج.م";
                    e.CellStyle.ForeColor = ColorScheme.Success;
                }
                else if (trip.NetProfit < 0)
                {
                    e.Value = $"⚠️ خسارة: {Math.Abs(trip.NetProfit):N2} ج.م";
                    e.CellStyle.ForeColor = ColorScheme.Error;
                    e.CellStyle.Font = new Font(dgvTrips.Font, FontStyle.Bold);
                }
                else
                {
                    e.Value = "0.00 ج.م";
                    e.CellStyle.ForeColor = Color.Gray;
                }
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                e.FormattingApplied = true;
            }
            // الحالة
            else if (columnName == "Status")
            {
                string statusText = GetStatusInArabic(trip.Status);
                e.Value = $"{GetStatusEmoji(trip.Status)} {statusText}";
                e.CellStyle.ForeColor = GetStatusColor(trip.Status);
                e.CellStyle.Font = new Font(dgvTrips.Font, FontStyle.Bold);
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                e.FormattingApplied = true;
            }
            // مقفولة؟
            else if (columnName == "IsLocked")
            {
                e.Value = trip.IsLockedForTrips ? "🔒 نعم" : "🔓 لا";
                e.CellStyle.ForeColor = trip.IsLockedForTrips ? ColorScheme.Error : ColorScheme.Success;
                e.CellStyle.Font = new Font(dgvTrips.Font, FontStyle.Bold);
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                e.FormattingApplied = true;
            }
        }
        catch { /* Ignore formatting errors */ }
    }
    
    private void BtnViewDetails_Click(object? sender, EventArgs e)
    {
        if (dgvTrips.SelectedRows.Count == 0)
        {
            MessageBox.Show("يرجى اختيار رحلة لعرض تفاصيلها", 
                "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        var trip = dgvTrips.SelectedRows[0].DataBoundItem as Trip;
        if (trip == null) return;
        
        try
        {
            // فتح نموذج التفاصيل المالية الكاملة
            var detailsForm = new TripFinancialDetailsForm(trip, _tripService);
            detailsForm.Show(); // ✅ نافذة مستقلة
            
            // تحديث بعد الإغلاق
            _ = LoadTripsAsync();
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
        
        var trip = dgvTrips.SelectedRows[0].DataBoundItem as Trip;
        if (trip == null) return;
        
        if (trip.IsLockedForTrips)
        {
            MessageBox.Show("الرحلة مقفولة بالفعل", 
                "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        
        var result = MessageBox.Show(
            $"هل تريد قفل الرحلة '{trip.TripName}' من التعديل في قسم الرحلات؟\n\n" +
            "بعد القفل، لن يتمكن قسم الرحلات من تعديل بيانات الرحلة.",
            "تأكيد القفل",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        
        if (result == DialogResult.Yes)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                
                // ✅ استخدام الـ service method مباشرة بدلاً من تعديل الكائن
                bool success = await _tripService.LockTripForTripsAsync(trip.TripId);
                
                if (success)
                {
                    MessageBox.Show("تم قفل الرحلة بنجاح", 
                        "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadTripsAsync();
                }
                else
                {
                    MessageBox.Show("فشل قفل الرحلة", 
                        "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في قفل الرحلة:\n{ex.Message}", 
                    "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
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
        
        var trip = dgvTrips.SelectedRows[0].DataBoundItem as Trip;
        if (trip == null) return;
        
        if (!trip.IsLockedForTrips)
        {
            MessageBox.Show("الرحلة مفتوحة بالفعل", 
                "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        
        var result = MessageBox.Show(
            $"هل تريد فتح الرحلة '{trip.TripName}' للتعديل في قسم الرحلات؟\n\n" +
            "بعد الفتح، سيتمكن قسم الرحلات من تعديل بيانات الرحلة.",
            "تأكيد الفتح",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        
        if (result == DialogResult.Yes)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                
                // ✅ استخدام الـ service method مباشرة بدلاً من تعديل الكائن
                bool success = await _tripService.UnlockTripForTripsAsync(trip.TripId);
                
                if (success)
                {
                    MessageBox.Show("تم فتح الرحلة بنجاح", 
                        "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadTripsAsync();
                }
                else
                {
                    MessageBox.Show("فشل فتح الرحلة", 
                        "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح الرحلة:\n{ex.Message}", 
                    "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
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
        
        var trip = dgvTrips.SelectedRows[0].DataBoundItem as Trip;
        if (trip == null) return;
        
        if (trip.Status == TripStatus.Confirmed)
        {
            MessageBox.Show("الرحلة مؤكدة بالفعل", 
                "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        
        var result = MessageBox.Show(
            $"هل تريد تأكيد الرحلة '{trip.TripName}'؟\n\n" +
            "بعد التأكيد:\n" +
            "• سيتم قفل الرحلة تلقائياً من التعديل في قسم الرحلات\n" +
            "• ستصبح الرحلة جاهزة للعمل عليها\n" +
            "• يمكنك فتحها لاحقاً إذا احتجت لتعديلها",
            "تأكيد الرحلة",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        
        if (result == DialogResult.Yes)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                
                // ✅ استخدام الـ service method لتحديث الحالة والقفل
                await _tripService.UpdateTripStatusAsync(trip.TripId, TripStatus.Confirmed, _currentUserId);
                await _tripService.LockTripForTripsAsync(trip.TripId); // قفل تلقائي عند التأكيد
                
                MessageBox.Show("✅ تم تأكيد الرحلة وقفلها بنجاح", 
                    "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadTripsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تأكيد الرحلة:\n{ex.Message}", 
                    "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
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
        
        var trip = dgvTrips.SelectedRows[0].DataBoundItem as Trip;
        if (trip == null) return;
        
        if (trip.Status == TripStatus.Cancelled)
        {
            MessageBox.Show("الرحلة ملغاة بالفعل", 
                "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        
        var result = MessageBox.Show(
            $"⚠️ هل تريد إلغاء الرحلة '{trip.TripName}'؟\n\n" +
            "تحذير: الرحلة الملغاة لن تكون متاحة للعمل عليها.",
            "إلغاء الرحلة",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        
        if (result == DialogResult.Yes)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                
                // ✅ استخدام الـ service method لتحديث الحالة
                await _tripService.UpdateTripStatusAsync(trip.TripId, TripStatus.Cancelled, _currentUserId);
                
                MessageBox.Show("تم إلغاء الرحلة بنجاح", 
                    "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadTripsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في إلغاء الرحلة:\n{ex.Message}", 
                    "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
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
    
    private string GetStatusEmoji(TripStatus status)
    {
        return status switch
        {
            TripStatus.Draft => "📝",
            TripStatus.Unconfirmed => "❓",
            TripStatus.Confirmed => "✅",
            TripStatus.InProgress => "🚀",
            TripStatus.Completed => "✔️",
            TripStatus.Cancelled => "❌",
            _ => "❓"
        };
    }
    
    private Color GetStatusColor(TripStatus status)
    {
        return status switch
        {
            TripStatus.Draft => Color.Gray,
            TripStatus.Unconfirmed => Color.FromArgb(243, 156, 18),
            TripStatus.Confirmed => Color.FromArgb(39, 174, 96),
            TripStatus.InProgress => Color.FromArgb(52, 152, 219),
            TripStatus.Completed => Color.FromArgb(46, 204, 113),
            TripStatus.Cancelled => Color.FromArgb(231, 76, 60),
            _ => Color.Black
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
    
    private void UpdateButtonStates()
    {
        bool hasSelection = dgvTrips.SelectedRows.Count > 0;
        btnViewDetails.Enabled = hasSelection;
        btnLockTrip.Enabled = hasSelection;
        btnUnlockTrip.Enabled = hasSelection;
        btnConfirmTrip.Enabled = hasSelection;
        btnCancelTrip.Enabled = hasSelection;
    }
    
    private Button CreateButton(string text, int x, int y, int width, int height, Color backColor)
    {
        var btn = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }
}
