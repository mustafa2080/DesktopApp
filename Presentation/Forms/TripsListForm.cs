using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Presentation;
using Microsoft.Extensions.DependencyInjection;

namespace GraceWay.AccountingSystem.Presentation.Forms;

/// <summary>
/// قائمة الرحلات السياحية
/// </summary>
public partial class TripsListForm : Form
{
    private readonly ITripService _tripService;
    private readonly IAuthService _authService;
    private readonly int _currentUserId;
    
    // Controls
    private Panel _headerPanel = null!;
    private TextBox _searchBox = null!;
    private Button _searchButton = null!;
    private Button _addButton = null!;
    private Button _editButton = null!;
    private Button _deleteButton = null!;
    private Button _detailsButton = null!;
    private Button _refreshButton = null!;
    private ComboBox _statusFilterCombo = null!;
    private ComboBox _typeFilterCombo = null!;
    private CheckBox _activeOnlyCheck = null!;
    private DataGridView _tripsGrid = null!;
    
    public TripsListForm(ITripService tripService, IAuthService authService, int currentUserId)
    {
        _tripService = tripService;
        _authService = authService;
        _currentUserId = currentUserId;
        
        InitializeComponent();
        SetupForm();
        InitializeCustomControls();
        
        // تأكد من ظهور الـ Grid
        this.Load += async (s, e) =>
        {
            await LoadDataAsync();
        };
    }
    
    private void SetupForm()
    {
        this.Text = "إدارة الرحلات السياحية";
        this.Size = new Size(1600, 950);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
        this.WindowState = FormWindowState.Maximized;
    }
    
    private void InitializeCustomControls()
    {
        // ══════════════════════════════════════
        // Header Panel with Gradient Background
        // ══════════════════════════════════════
        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 160,
            BackColor = Color.White,
            Padding = new Padding(20, 10, 20, 10)
        };
        
        // Add subtle shadow effect
        _headerPanel.Paint += (s, e) =>
        {
            using (var shadowBrush = new SolidBrush(Color.FromArgb(10, 0, 0, 0)))
            {
                e.Graphics.FillRectangle(shadowBrush, 0, _headerPanel.Height - 3, _headerPanel.Width, 3);
            }
        };
        
        // ══════════════════════════════════════
        // Title Section with Icon - RTL Layout
        // ══════════════════════════════════════
        
        // الأيقونة على اليمين
        Label iconLabel = new Label
        {
            Text = "✈️",
            Font = new Font("Segoe UI Emoji", 32F),
            Size = new Size(50, 50),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(_headerPanel.Width - 70, 10),
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _headerPanel.Controls.Add(iconLabel);
        
        // العنوان بجانب الأيقونة
        Label titleLabel = new Label
        {
            Text = "إدارة الرحلات السياحية",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            Size = new Size(280, 32),
            TextAlign = ContentAlignment.MiddleRight,
            Location = new Point(_headerPanel.Width - 360, 12),
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _headerPanel.Controls.Add(titleLabel);
        
        // الوصف تحت العنوان
        Label subtitleLabel = new Label
        {
            Text = "إدارة وتتبع الرحلات السياحية والبرامج",
            Font = new Font("Cairo", 9F),
            ForeColor = ColorScheme.TextSecondary,
            Size = new Size(320, 22),
            TextAlign = ContentAlignment.MiddleRight,
            Location = new Point(_headerPanel.Width - 360, 42),
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _headerPanel.Controls.Add(subtitleLabel);
        
        // ══════════════════════════════════════
        // Search and Filter Section
        // ══════════════════════════════════════
        Panel searchPanel = new Panel
        {
            Location = new Point(20, 68),
            Size = new Size(950, 38),
            BackColor = ColorScheme.LightGray,
            Padding = new Padding(8)
        };
        
        // Search Box with Icon
        Panel searchContainer = new Panel
        {
            Location = new Point(8, 6),
            Size = new Size(320, 26),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        
        Label searchIcon = new Label
        {
            Text = "🔍",
            Font = new Font("Segoe UI Emoji", 10F),
            AutoSize = true,
            Location = new Point(6, 3)
        };
        searchContainer.Controls.Add(searchIcon);
        
        _searchBox = new TextBox
        {
            Font = new Font("Cairo", 9F),
            Size = new Size(280, 24),
            Location = new Point(30, 1),
            BorderStyle = BorderStyle.None,
            PlaceholderText = "ابحث باسم الرحلة أو رقم الرحلة..."
        };
        _searchBox.KeyPress += (s, e) => { if (e.KeyChar == (char)13) SearchTrips(); };
        searchContainer.Controls.Add(_searchBox);
        
        searchPanel.Controls.Add(searchContainer);
        
        // Type Filter
        Label typeLabel = new Label
        {
            Text = "النوع:",
            Font = new Font("Cairo", 8.5F, FontStyle.Bold),
            ForeColor = ColorScheme.TextSecondary,
            AutoSize = true,
            Location = new Point(340, 10)
        };
        searchPanel.Controls.Add(typeLabel);
        
        _typeFilterCombo = new ComboBox
        {
            Font = new Font("Cairo", 8.5F),
            Size = new Size(110, 24),
            Location = new Point(390, 6),
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat
        };
        _typeFilterCombo.Items.AddRange(new object[]
        {
            "الكل",
            "عمرة",
            "سياحة",
            "حج",
            "مؤتمرات"
        });
        _typeFilterCombo.SelectedIndex = 0;
        _typeFilterCombo.SelectedIndexChanged += (s, e) => _ = LoadDataAsync();
        searchPanel.Controls.Add(_typeFilterCombo);
        
        // Status Filter
        Label statusLabel = new Label
        {
            Text = "الحالة:",
            Font = new Font("Cairo", 8.5F, FontStyle.Bold),
            ForeColor = ColorScheme.TextSecondary,
            AutoSize = true,
            Location = new Point(510, 10)
        };
        searchPanel.Controls.Add(statusLabel);
        
        _statusFilterCombo = new ComboBox
        {
            Font = new Font("Cairo", 8.5F),
            Size = new Size(120, 24),
            Location = new Point(560, 6),
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat
        };
        _statusFilterCombo.Items.AddRange(new object[]
        {
            "الكل",
            "مسودة",
            "غير مؤكد",
            "مؤكد",
            "قيد التنفيذ",
            "مكتمل",
            "ملغي"
        });
        _statusFilterCombo.SelectedIndex = 0;
        _statusFilterCombo.SelectedIndexChanged += (s, e) => _ = LoadDataAsync();
        searchPanel.Controls.Add(_statusFilterCombo);
        
        // Active Only Checkbox
        _activeOnlyCheck = new CheckBox
        {
            Text = "النشط فقط ✓",
            Font = new Font("Cairo", 8.5F),
            ForeColor = ColorScheme.TextSecondary,
            AutoSize = true,
            Location = new Point(690, 10),
            Checked = true
        };
        _activeOnlyCheck.CheckedChanged += (s, e) => _ = LoadDataAsync();
        searchPanel.Controls.Add(_activeOnlyCheck);
        
        // Search Button
        _searchButton = CreateModernButton("بحث", ColorScheme.Primary, new Point(790, 6), 80, (s, e) => SearchTrips());
        searchPanel.Controls.Add(_searchButton);
        
        // Refresh Button
        _refreshButton = CreateModernButton("🔄", ColorScheme.Secondary, new Point(880, 6), 50, (s, e) => _ = LoadDataAsync());
        searchPanel.Controls.Add(_refreshButton);
        
        _headerPanel.Controls.Add(searchPanel);
        
        // ══════════════════════════════════════
        // Action Buttons Section
        // ══════════════════════════════════════
        Panel actionsPanel = new Panel
        {
            Location = new Point(20, 112),
            Size = new Size(950, 38),
            BackColor = Color.Transparent
        };
        
        _addButton = CreateModernButton("➕ إضافة رحلة", ColorScheme.Success, new Point(0, 0), 130, AddTrip_Click);
        actionsPanel.Controls.Add(_addButton);
        
        _editButton = CreateModernButton("✏️ تعديل", ColorScheme.Warning, new Point(140, 0), 95, EditTrip_Click);
        _editButton.Enabled = false;
        actionsPanel.Controls.Add(_editButton);
        
        _deleteButton = CreateModernButton("🗑️ حذف", ColorScheme.Error, new Point(245, 0), 85, DeleteTrip_Click);
        _deleteButton.Enabled = false;
        actionsPanel.Controls.Add(_deleteButton);
        
        _detailsButton = CreateModernButton("📋 التفاصيل", ColorScheme.Info, new Point(340, 0), 105, ShowDetails_Click);
        _detailsButton.Enabled = false;
        actionsPanel.Controls.Add(_detailsButton);
        
        // أزرار تغيير حالة الرحلة
        Button confirmTripButton = CreateModernButton("✅ تأكيد الرحلة", Color.FromArgb(39, 174, 96), new Point(455, 0), 135, ConfirmTrip_Click);
        confirmTripButton.Enabled = false;
        confirmTripButton.Name = "confirmButton";
        actionsPanel.Controls.Add(confirmTripButton);
        
        Button unconfirmTripButton = CreateModernButton("⏸️ إلغاء التأكيد", Color.FromArgb(241, 196, 15), new Point(600, 0), 140, UnconfirmTrip_Click);
        unconfirmTripButton.Enabled = false;
        unconfirmTripButton.Name = "unconfirmButton";
        actionsPanel.Controls.Add(unconfirmTripButton);
        
        Button cancelTripButton = CreateModernButton("❌ إلغاء الرحلة", Color.FromArgb(231, 76, 60), new Point(750, 0), 125, CancelTrip_Click);
        cancelTripButton.Enabled = false;
        cancelTripButton.Name = "cancelButton";
        actionsPanel.Controls.Add(cancelTripButton);
        
        _headerPanel.Controls.Add(actionsPanel);
        
        // ══════════════════════════════════════
        // Modern DataGridView with Better Styling
        // ══════════════════════════════════════
        _tripsGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            RowHeadersVisible = false,
            Font = new Font("Cairo", 9.5F),
            ColumnHeadersHeight = 50,
            RowTemplate = { Height = 50 },
            EnableHeadersVisualStyles = false,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = Color.FromArgb(224, 224, 224),
            AllowUserToResizeColumns = true,
            AllowUserToResizeRows = false,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
            ScrollBars = ScrollBars.Both
        };
        
        // Modern Header Style - More prominent
        _tripsGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleCenter,
            Padding = new Padding(5),
            SelectionBackColor = ColorScheme.Primary,
            SelectionForeColor = Color.White,
            WrapMode = DataGridViewTriState.True
        };
        
        // Modern Cell Style
        _tripsGrid.DefaultCellStyle = new DataGridViewCellStyle
        {
            SelectionBackColor = Color.FromArgb(232, 245, 233),
            SelectionForeColor = Color.FromArgb(27, 94, 32),
            Alignment = DataGridViewContentAlignment.MiddleCenter,
            Padding = new Padding(3),
            WrapMode = DataGridViewTriState.False,
            Font = new Font("Cairo", 9.5F)
        };
        
        // Alternating Rows for Better Readability
        _tripsGrid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(248, 249, 250),
            SelectionBackColor = Color.FromArgb(232, 245, 233),
            SelectionForeColor = Color.FromArgb(27, 94, 32)
        };
        
        InitializeGridColumns();
        _tripsGrid.SelectionChanged += TripsGrid_SelectionChanged;
        _tripsGrid.CellDoubleClick += (s, e) =>
        {
            if (e.RowIndex >= 0) ShowDetails_Click(s, e);
        };
        
        // Add hover effect
        _tripsGrid.CellMouseEnter += (s, e) =>
        {
            if (e.RowIndex >= 0)
            {
                _tripsGrid.Cursor = Cursors.Hand;
            }
        };
        
        _tripsGrid.CellMouseLeave += (s, e) =>
        {
            _tripsGrid.Cursor = Cursors.Default;
        };
        
        // إضافة الـ Controls بالترتيب الصحيح
        this.SuspendLayout();
        
        // أولاً: إضافة الـ Grid (لأنه Dock.Fill سيملأ كل المساحة)
        this.Controls.Add(_tripsGrid);
        
        // ثانياً: إضافة الـ Header (لأنه Dock.Top سيظهر في الأعلى)
        this.Controls.Add(_headerPanel);
        
        this.ResumeLayout(false);
        this.PerformLayout();
    }
    
    private void InitializeGridColumns()
    {
        // Clear any existing columns
        _tripsGrid.Columns.Clear();
        
        // إعداد الـ Grid بدون AutoGenerateColumns
        _tripsGrid.AutoGenerateColumns = false;
        
        // Hidden ID column
        _tripsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TripId",
            HeaderText = "ID",
            DataPropertyName = "TripId",
            Visible = false
        });
        
        // Trip Number - Compact but readable
        _tripsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TripNumber",
            HeaderText = "رقم الرحلة",
            DataPropertyName = "TripNumber",
            Width = 110,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Font = new Font("Cairo", 9.5F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(250, 250, 250)
            }
        });
        
        // Trip Name - Fill remaining space
        _tripsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TripName",
            HeaderText = "اسم الرحلة",
            DataPropertyName = "TripName",
            MinimumWidth = 180,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 100,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 33, 33),
                Alignment = DataGridViewContentAlignment.MiddleRight,
                Padding = new Padding(5, 5, 10, 5)
            }
        });
        
        // ✅ Created By User - اسم اليوزر اللي عمل الرحلة
        _tripsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "CreatedByUser",
            HeaderText = "أنشأها",
            DataPropertyName = "CreatedByUser",
            Width = 120,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Cairo", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(13, 71, 161),
                BackColor = Color.FromArgb(227, 242, 253),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(5, 3, 5, 3)
            }
        });
        
        // Destination
        _tripsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Destination",
            HeaderText = "الوجهة",
            DataPropertyName = "Destination",
            Width = 120,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Cairo", 9.5F),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // Trip Type
        _tripsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TripTypeDisplay",
            DataPropertyName = "TripTypeDisplay",
            HeaderText = "النوع",
            Width = 120,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Cairo", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // Start Date
        _tripsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "StartDate",
            HeaderText = "تاريخ البدء",
            DataPropertyName = "StartDate",
            Width = 100,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Format = "dd/MM/yyyy",
                Font = new Font("Cairo", 9F),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // End Date
        _tripsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "EndDate",
            HeaderText = "تاريخ الانتهاء",
            DataPropertyName = "EndDate",
            Width = 100,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Format = "dd/MM/yyyy",
                Font = new Font("Cairo", 9F),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // Total Days
        _tripsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TotalDays",
            DataPropertyName = "TotalDays",
            HeaderText = "المدة",
            Width = 85,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Cairo", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(123, 31, 162),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // Price per person (in EGP)
        _tripsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "PricePerPersonDisplay",
            DataPropertyName = "PricePerPersonDisplay",
            HeaderText = "السعر/فرد",
            Width = 110,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Format = "#,##0 ج.م",
                Font = new Font("Cairo", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 121, 107),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // Total Cost
        _tripsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TotalCostDisplay",
            DataPropertyName = "TotalCostDisplay",
            HeaderText = "التكلفة الإجمالية",
            Width = 130,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Format = "#,##0 ج.م",
                Font = new Font("Cairo", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(211, 47, 47),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // Net Profit
        _tripsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "NetProfitDisplay",
            DataPropertyName = "NetProfitDisplay",
            HeaderText = "صافي الربح",
            Width = 120,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Format = "#,##0 ج.م",
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // Status
        _tripsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "StatusDisplay",
            DataPropertyName = "StatusDisplay",
            HeaderText = "الحالة",
            Width = 100,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Cairo", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(5, 3, 5, 3)
            }
        });
        
        // IsLocked (مقفولة من قسم الحجوزات؟)
        _tripsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "IsLockedDisplay",
            DataPropertyName = "IsLockedDisplay",
            HeaderText = "مقفولة؟",
            Width = 90,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Cairo", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(5, 3, 5, 3)
            }
        });
        
        // Set grid properties for optimal display
        _tripsGrid.RowTemplate.Height = 50;
        _tripsGrid.ColumnHeadersHeight = 50;
        _tripsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _tripsGrid.ScrollBars = ScrollBars.Both;
        _tripsGrid.AllowUserToResizeColumns = true;
        _tripsGrid.AllowUserToResizeRows = false;
    }
    
    private async Task LoadDataAsync()
    {
        try
        {
            Console.WriteLine("🔍 LoadDataAsync started...");
            _tripsGrid.DataSource = null;
            
            // Get filter parameters
            TripType? typeFilter = _typeFilterCombo.SelectedIndex switch
            {
                1 => TripType.Umrah,
                2 => TripType.DomesticTourism,
                3 => TripType.Hajj,
                4 => TripType.Religious,
                _ => null
            };
            
            TripStatus? statusFilter = _statusFilterCombo.SelectedIndex switch
            {
                1 => TripStatus.Draft,
                2 => TripStatus.Unconfirmed,
                3 => TripStatus.Confirmed,
                4 => TripStatus.InProgress,
                5 => TripStatus.Completed,
                6 => TripStatus.Cancelled,
                _ => null
            };
            
            Console.WriteLine($"📊 Filters: activeOnly={_activeOnlyCheck.Checked}, type={typeFilter}, status={statusFilter}");
            
            var trips = await _tripService.GetAllTripsAsync(includeDetails: false);
            
            // ✅ فلترة الرحلات حسب الصلاحيات:
            // - الأدمن يشوف كل الرحلات من كل اليوزرات
            // - باقي اليوزرات يشوفوا رحلاتهم فقط
            var currentUser = _authService.CurrentUser;
            bool isAdmin = currentUser?.Role?.RoleName == "Administrator" || currentUser?.Role?.RoleName == "مدير النظام";
            
            if (!isAdmin)
            {
                // اليوزرات العاديين يشوفوا رحلاتهم فقط
                trips = trips.Where(t => t.CreatedBy == _currentUserId).ToList();
            }
            // الأدمن يشوف كل الرحلات - مش محتاج فلترة
            
            // Apply filters
            if (_activeOnlyCheck.Checked)
            {
                trips = trips.Where(t => t.IsActive).ToList();
            }
            
            if (typeFilter.HasValue)
            {
                trips = trips.Where(t => t.TripType == typeFilter.Value).ToList();
            }
            
            if (statusFilter.HasValue)
            {
                trips = trips.Where(t => t.Status == statusFilter.Value).ToList();
            }
            
            Console.WriteLine($"✅ Loaded {trips.Count} trips for user {_currentUserId}");
            
            // Debug: Print first trip if exists
            if (trips.Any())
            {
                var first = trips.First();
                Console.WriteLine($"📦 First trip: ID={first.TripId}, Number={first.TripNumber}, Name={first.TripName}, Creator={first.Creator?.FullName}");
            }
            else
            {
                Console.WriteLine("⚠️ No trips found for this user!");
            }
            
            // Create display list
            var displayList = trips.Select(t => new
            {
                t.TripId,
                t.TripNumber,
                t.TripName,
                t.Destination,
                TripTypeDisplay = GetTripTypeDisplay(t.TripType),
                t.StartDate,
                t.EndDate,
                TotalDays = t.TotalDays + " يوم",
                PricePerPersonDisplay = t.SellingPricePerPerson * (t.ExchangeRate == 0 ? 1 : t.ExchangeRate),
                TotalCostDisplay = t.TotalCost,
                NetProfitDisplay = CalculateNetProfit(t),
                StatusDisplay = GetStatusDisplay(t.Status),
                IsLockedDisplay = t.IsLockedForTrips ? "🔒 نعم" : "🔓 لا",
                CreatedByUser = t.Creator?.FullName ?? "غير محدد", // ✅ اسم اليوزر اللي عمل الرحلة
                t.IsLockedForTrips,
                t.IsActive
            }).ToList();
            
            Console.WriteLine($"📋 Display list created with {displayList.Count} items");
            
            _tripsGrid.DataSource = displayList;
            
            Console.WriteLine($"✅ Grid DataSource set. Row count: {_tripsGrid.Rows.Count}");
            
            // Apply cell formatting
            ApplyCellFormatting();
            
            Console.WriteLine("✅ LoadDataAsync completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in LoadDataAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            MessageBox.Show($"حدث خطأ أثناء تحميل البيانات:\n{ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void ApplyCellFormatting()
    {
        foreach (DataGridViewRow row in _tripsGrid.Rows)
        {
            var status = row.Cells["StatusDisplay"].Value?.ToString();
            var profitValue = row.Cells["NetProfitDisplay"].Value;
            var typeValue = row.Cells["TripTypeDisplay"].Value?.ToString();
            
            // لو الرحلة ملغية، نلون الصف كله باللون الأحمر الفاتح
            if (status == "ملغي")
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Style.BackColor = Color.FromArgb(255, 235, 238);
                    cell.Style.ForeColor = Color.FromArgb(183, 28, 28);
                }
            }
            
            // Type badge colors
            if (!string.IsNullOrEmpty(typeValue))
            {
                var typeCell = row.Cells["TripTypeDisplay"];
                typeCell.Style.Font = new Font("Cairo", 9F, FontStyle.Bold);
                typeCell.Style.Padding = new Padding(5, 3, 5, 3);
                
                switch (typeValue)
                {
                    case "عمرة":
                        typeCell.Style.BackColor = Color.FromArgb(232, 245, 233);
                        typeCell.Style.ForeColor = Color.FromArgb(27, 94, 32);
                        break;
                    case "سياحة":
                        typeCell.Style.BackColor = Color.FromArgb(227, 242, 253);
                        typeCell.Style.ForeColor = Color.FromArgb(13, 71, 161);
                        break;
                    case "حج":
                        typeCell.Style.BackColor = Color.FromArgb(255, 243, 224);
                        typeCell.Style.ForeColor = Color.FromArgb(230, 81, 0);
                        break;
                    case "مؤتمرات":
                        typeCell.Style.BackColor = Color.FromArgb(243, 229, 245);
                        typeCell.Style.ForeColor = Color.FromArgb(123, 31, 162);
                        break;
                }
            }
            
            // Status badge colors with better contrast
            if (!string.IsNullOrEmpty(status))
            {
                var statusCell = row.Cells["StatusDisplay"];
                statusCell.Style.Font = new Font("Cairo", 9F, FontStyle.Bold);
                statusCell.Style.Padding = new Padding(8, 5, 8, 5);
                
                switch (status)
                {
                    case "مكتمل":
                        statusCell.Style.BackColor = Color.FromArgb(232, 245, 233);
                        statusCell.Style.ForeColor = Color.FromArgb(27, 94, 32);
                        break;
                    case "ملغي":
                        statusCell.Style.BackColor = Color.FromArgb(255, 235, 238);
                        statusCell.Style.ForeColor = Color.FromArgb(183, 28, 28);
                        break;
                    case "قيد التنفيذ":
                        statusCell.Style.BackColor = Color.FromArgb(255, 243, 224);
                        statusCell.Style.ForeColor = Color.FromArgb(230, 81, 0);
                        break;
                    case "مؤكد":
                        statusCell.Style.BackColor = Color.FromArgb(227, 242, 253);
                        statusCell.Style.ForeColor = Color.FromArgb(13, 71, 161);
                        break;
                    case "مسودة":
                    case "غير مؤكد":
                        statusCell.Style.BackColor = Color.FromArgb(245, 245, 245);
                        statusCell.Style.ForeColor = Color.FromArgb(97, 97, 97);
                        break;
                }
            }
            
            // Profit colors
            if (profitValue != null && profitValue != DBNull.Value)
            {
                decimal profit = Convert.ToDecimal(profitValue);
                var profitCell = row.Cells["NetProfitDisplay"];
                
                if (profit < 0)
                {
                    profitCell.Style.ForeColor = Color.FromArgb(211, 47, 47);  // Red for loss
                    profitCell.Style.Font = new Font("Cairo", 10F, FontStyle.Bold);
                }
                else if (profit > 0)
                {
                    profitCell.Style.ForeColor = Color.FromArgb(56, 142, 60);  // Green for profit
                    profitCell.Style.Font = new Font("Cairo", 10F, FontStyle.Bold);
                }
                else
                {
                    profitCell.Style.ForeColor = Color.FromArgb(117, 117, 117); // Gray for break-even
                }
            }
            
            // IsLocked colors
            var isLockedValue = row.Cells["IsLockedDisplay"].Value?.ToString();
            if (!string.IsNullOrEmpty(isLockedValue))
            {
                var lockedCell = row.Cells["IsLockedDisplay"];
                lockedCell.Style.Font = new Font("Cairo", 9F, FontStyle.Bold);
                lockedCell.Style.Padding = new Padding(5, 3, 5, 3);
                
                if (isLockedValue.Contains("نعم"))
                {
                    lockedCell.Style.BackColor = Color.FromArgb(255, 235, 238);
                    lockedCell.Style.ForeColor = Color.FromArgb(183, 28, 28);
                }
                else
                {
                    lockedCell.Style.BackColor = Color.FromArgb(232, 245, 233);
                    lockedCell.Style.ForeColor = Color.FromArgb(27, 94, 32);
                }
            }
        }
    }
    
    private decimal CalculateNetProfit(Trip trip)
    {
        // Net Profit = Expected Revenue - Total Cost
        decimal expectedRevenue = trip.BookedSeats * trip.SellingPricePerPerson * (trip.ExchangeRate == 0 ? 1 : trip.ExchangeRate);
        decimal netProfit = expectedRevenue - trip.TotalCost;
        return netProfit;
    }
    
    private string GetTripTypeDisplay(TripType type)
    {
        return type switch
        {
            TripType.Umrah => "عمرة",
            TripType.DomesticTourism => "سياحة داخلية",
            TripType.InternationalTourism => "سياحة خارجية",
            TripType.Hajj => "حج",
            TripType.Religious => "رحلات دينية",
            TripType.Educational => "رحلات تعليمية",
            _ => "غير محدد"
        };
    }
    
    private string GetStatusDisplay(TripStatus status)
    {
        return status switch
        {
            TripStatus.Draft => "مسودة",
            TripStatus.Unconfirmed => "غير مؤكد",
            TripStatus.Confirmed => "مؤكد",
            TripStatus.InProgress => "قيد التنفيذ",
            TripStatus.Completed => "مكتمل",
            TripStatus.Cancelled => "ملغي",
            _ => "غير محدد"
        };
    }
    
    private async void SearchTrips()
    {
        try
        {
            var searchTerm = _searchBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                await LoadDataAsync();
                return;
            }
            
            var trips = await _tripService.SearchTripsAsync(searchTerm);
            
            var displayList = trips.Select(t => new
            {
                t.TripId,
                t.TripNumber,
                t.TripName,
                t.Destination,
                TripTypeDisplay = GetTripTypeDisplay(t.TripType),
                t.StartDate,
                t.EndDate,
                TotalDays = t.TotalDays + " يوم",
                PricePerPersonDisplay = t.SellingPricePerPerson * (t.ExchangeRate == 0 ? 1 : t.ExchangeRate),
                TotalCostDisplay = t.TotalCost,
                NetProfitDisplay = CalculateNetProfit(t),
                StatusDisplay = GetStatusDisplay(t.Status),
                t.IsActive
            }).ToList();
            
            _tripsGrid.DataSource = displayList;
            ApplyCellFormatting();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"حدث خطأ أثناء البحث:\n{ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void AddTrip_Click(object? sender, EventArgs e)
    {
        var form = new AddEditTripForm(_tripService, _currentUserId);
        form.FormClosed += (s, args) => _ = LoadDataAsync(); // ✅ تحديث البيانات عند إغلاق النموذج
        form.Show(); // ✅ فتح كنافذة منفصلة
    }
    
    private async void EditTrip_Click(object? sender, EventArgs e)
    {
        if (_tripsGrid.SelectedRows.Count == 0) return;
        
        int tripId = Convert.ToInt32(_tripsGrid.SelectedRows[0].Cells["TripId"].Value);
        
        // التحقق من حالة القفل
        var trip = await _tripService.GetTripByIdAsync(tripId, false);
        if (trip?.IsLockedForTrips == true)
        {
            MessageBox.Show(
                "⚠️ هذه الرحلة مقفولة من قسم الحجوزات\n\n" +
                "لا يمكن تعديلها من قسم الرحلات.\n" +
                "يرجى التواصل مع قسم الحجوزات لفتح الرحلة أولاً.",
                "رحلة مقفولة",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }
        
        var form = new AddEditTripForm(_tripService, _currentUserId, tripId);
        form.FormClosed += (s, args) => _ = LoadDataAsync(); // ✅ تحديث البيانات عند إغلاق النموذج
        form.Show(); // ✅ فتح كنافذة منفصلة
    }
    
    private async void DeleteTrip_Click(object? sender, EventArgs e)
    {
        if (_tripsGrid.SelectedRows.Count == 0) return;
        
        var tripId = Convert.ToInt32(_tripsGrid.SelectedRows[0].Cells["TripId"].Value);
        var tripNumber = _tripsGrid.SelectedRows[0].Cells["TripNumber"].Value?.ToString() ?? "غير معروف";
        
        // التحقق من حالة القفل
        var trip = await _tripService.GetTripByIdAsync(tripId, false);
        if (trip?.IsLockedForTrips == true)
        {
            MessageBox.Show(
                "⚠️ هذه الرحلة مقفولة من قسم الحجوزات\n\n" +
                "لا يمكن حذفها من قسم الرحلات.\n" +
                "يرجى التواصل مع قسم الحجوزات لفتح الرحلة أولاً.",
                "رحلة مقفولة",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }
        
        var result = MessageBox.Show(
            $"هل أنت متأكد من حذف الرحلة {tripNumber}؟",
            "تأكيد الحذف",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );
        
        if (result == DialogResult.Yes)
        {
            try
            {
                await _tripService.DeleteTripAsync(tripId);
                MessageBox.Show("تم حذف الرحلة بنجاح", "نجاح",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الحذف:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    
    private void ShowDetails_Click(object? sender, EventArgs e)
    {
        if (_tripsGrid.SelectedRows.Count == 0) return;
        
        int tripId = Convert.ToInt32(_tripsGrid.SelectedRows[0].Cells["TripId"].Value!);
        var form = new TripDetailsForm(_tripService, tripId, _currentUserId);
        form.Show(); // ✅ تغيير من ShowDialog إلى Show للسماح بفتح عدة نوافذ
    }
    
    private void TripsGrid_SelectionChanged(object? sender, EventArgs e)
    {
        bool hasSelection = _tripsGrid.SelectedRows.Count > 0;
        bool isLocked = false;
        
        // التحقق من حالة القفل من البيانات المعروضة
        if (hasSelection && _tripsGrid.SelectedRows[0].DataBoundItem != null)
        {
            // الحصول على الكائن المجهول الذي تم إنشاؤه في displayList
            var item = _tripsGrid.SelectedRows[0].DataBoundItem;
            if (item != null)
            {
                var type = item.GetType();
                var property = type.GetProperty("IsLockedForTrips");
                if (property != null)
                {
                    isLocked = (bool)(property.GetValue(item) ?? false);
                }
            }
        }
        
        // زر التفاصيل دائماً متاح
        _detailsButton.Enabled = hasSelection;
        
        // أزرار التعديل والحذف وتغيير الحالة متاحة فقط إذا كانت الرحلة غير مقفولة
        _editButton.Enabled = hasSelection && !isLocked;
        _deleteButton.Enabled = hasSelection && !isLocked;
        
        // تفعيل/تعطيل أزرار تغيير الحالة - استخدام ControlCollection للبحث بـ Name
        foreach (Control ctrl in _headerPanel.Controls)
        {
            if (ctrl is Panel actionsPanel)
            {
                foreach (Control btn in actionsPanel.Controls)
                {
                    if (btn is Button button)
                    {
                        if (button.Name == "confirmButton" || 
                            button.Name == "unconfirmButton" || 
                            button.Name == "cancelButton")
                        {
                            button.Enabled = hasSelection && !isLocked;
                        }
                    }
                }
            }
        }
    }
    
    private async void ConfirmTrip_Click(object? sender, EventArgs e)
    {
        if (_tripsGrid.SelectedRows.Count == 0) return;
        
        int tripId = Convert.ToInt32(_tripsGrid.SelectedRows[0].Cells["TripId"].Value);
        var tripNumber = _tripsGrid.SelectedRows[0].Cells["TripNumber"].Value?.ToString();
        
        // التحقق من حالة القفل
        var trip = await _tripService.GetTripByIdAsync(tripId, false);
        if (trip?.IsLockedForTrips == true)
        {
            MessageBox.Show(
                "⚠️ هذه الرحلة مقفولة من قسم الحجوزات\n\n" +
                "لا يمكن تغيير حالتها من قسم الرحلات.\n" +
                "يرجى التواصل مع قسم الحجوزات لفتح الرحلة أولاً.",
                "رحلة مقفولة",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }
        
        var result = MessageBox.Show(
            $"هل تريد تأكيد الرحلة {tripNumber}؟\nسيتم السماح بظهورها في قسم الحجوزات.",
            "تأكيد الرحلة",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );
        
        if (result == DialogResult.Yes)
        {
            try
            {
                await _tripService.UpdateTripStatusAsync(tripId, TripStatus.Confirmed, _currentUserId);
                MessageBox.Show("تم تأكيد الرحلة بنجاح", "نجاح",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تأكيد الرحلة:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    
    private async void UnconfirmTrip_Click(object? sender, EventArgs e)
    {
        if (_tripsGrid.SelectedRows.Count == 0) return;
        
        int tripId = Convert.ToInt32(_tripsGrid.SelectedRows[0].Cells["TripId"].Value);
        var tripNumber = _tripsGrid.SelectedRows[0].Cells["TripNumber"].Value?.ToString();
        
        // التحقق من حالة القفل
        var trip = await _tripService.GetTripByIdAsync(tripId, false);
        if (trip?.IsLockedForTrips == true)
        {
            MessageBox.Show(
                "⚠️ هذه الرحلة مقفولة من قسم الحجوزات\n\n" +
                "لا يمكن تغيير حالتها من قسم الرحلات.\n" +
                "يرجى التواصل مع قسم الحجوزات لفتح الرحلة أولاً.",
                "رحلة مقفولة",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }
        
        var result = MessageBox.Show(
            $"هل تريد إلغاء تأكيد الرحلة {tripNumber}؟\nلن تظهر في قسم الحجوزات.",
            "إلغاء التأكيد",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );
        
        if (result == DialogResult.Yes)
        {
            try
            {
                await _tripService.UpdateTripStatusAsync(tripId, TripStatus.Unconfirmed, _currentUserId);
                MessageBox.Show("تم إلغاء تأكيد الرحلة بنجاح", "نجاح",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء إلغاء التأكيد:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    
    private async void CancelTrip_Click(object? sender, EventArgs e)
    {
        if (_tripsGrid.SelectedRows.Count == 0) return;
        
        int tripId = Convert.ToInt32(_tripsGrid.SelectedRows[0].Cells["TripId"].Value);
        var tripNumber = _tripsGrid.SelectedRows[0].Cells["TripNumber"].Value?.ToString();
        
        // التحقق من حالة القفل
        var trip = await _tripService.GetTripByIdAsync(tripId, false);
        if (trip?.IsLockedForTrips == true)
        {
            MessageBox.Show(
                "⚠️ هذه الرحلة مقفولة من قسم الحجوزات\n\n" +
                "لا يمكن تغيير حالتها من قسم الرحلات.\n" +
                "يرجى التواصل مع قسم الحجوزات لفتح الرحلة أولاً.",
                "رحلة مقفولة",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }
        
        var result = MessageBox.Show(
            $"هل تريد إلغاء الرحلة {tripNumber}؟\nستظهر باللون الأحمر في جميع الأقسام.",
            "إلغاء الرحلة",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );
        
        if (result == DialogResult.Yes)
        {
            try
            {
                await _tripService.UpdateTripStatusAsync(tripId, TripStatus.Cancelled, _currentUserId);
                MessageBox.Show("تم إلغاء الرحلة بنجاح", "نجاح",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء إلغاء الرحلة:\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    
    private Button CreateModernButton(string text, Color backColor, Point location, int width, EventHandler clickHandler)
    {
        var button = new Button
        {
            Text = text,
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            Size = new Size(width, 32),
            Location = location,
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleCenter
        };
        
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = ColorScheme.Darken(backColor, 0.1f);
        button.FlatAppearance.MouseDownBackColor = ColorScheme.Darken(backColor, 0.2f);
        
        button.Click += clickHandler;
        
        // Add hover animation
        button.MouseEnter += (s, e) =>
        {
            button.BackColor = ColorScheme.Darken(backColor, 0.1f);
        };
        
        button.MouseLeave += (s, e) =>
        {
            button.BackColor = backColor;
        };
        
        return button;
    }
}
