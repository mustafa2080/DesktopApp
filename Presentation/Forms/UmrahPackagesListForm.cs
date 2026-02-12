using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Presentation;
using Microsoft.Extensions.DependencyInjection;

namespace GraceWay.AccountingSystem.Presentation.Forms;

/// <summary>
/// قائمة حزم العمرة
/// </summary>
public partial class UmrahPackagesListForm : Form
{
    private readonly IUmrahService _umrahService;
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
    private CheckBox _activeOnlyCheck = null!;
    private DataGridView _packagesGrid = null!;
    
    public UmrahPackagesListForm(IUmrahService umrahService, int currentUserId)
    {
        _umrahService = umrahService;
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
        this.Text = "إدارة حزم العمرة";
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
            Height = 170,
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
            Text = "🕌",
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
            Text = "إدارة حزم العمرة",
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
            Text = "إدارة وتتبع حزم العمرة والمعتمرين",
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
            Size = new Size(900, 38),
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
            PlaceholderText = "ابحث باسم الرحلة أو رقم الحزمة..."
        };
        _searchBox.KeyPress += (s, e) => { if (e.KeyChar == (char)13) SearchPackages(); };
        searchContainer.Controls.Add(_searchBox);
        
        searchPanel.Controls.Add(searchContainer);
        
        // Status Filter
        Label statusLabel = new Label
        {
            Text = "الحالة:",
            Font = new Font("Cairo", 8.5F, FontStyle.Bold),
            ForeColor = ColorScheme.TextSecondary,
            AutoSize = true,
            Location = new Point(340, 10)
        };
        searchPanel.Controls.Add(statusLabel);
        
        _statusFilterCombo = new ComboBox
        {
            Font = new Font("Cairo", 8.5F),
            Size = new Size(120, 24),
            Location = new Point(390, 6),
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat
        };
        _statusFilterCombo.Items.AddRange(new object[]
        {
            "الكل",
            "مسودة",
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
            Location = new Point(520, 10),
            Checked = true
        };
        _activeOnlyCheck.CheckedChanged += (s, e) => _ = LoadDataAsync();
        searchPanel.Controls.Add(_activeOnlyCheck);
        
        // Search Button
        _searchButton = CreateModernButton("بحث", ColorScheme.Primary, new Point(630, 6), 80, (s, e) => SearchPackages());
        searchPanel.Controls.Add(_searchButton);
        
        // Refresh Button
        _refreshButton = CreateModernButton("🔄", ColorScheme.Secondary, new Point(720, 6), 50, (s, e) => _ = LoadDataAsync());
        searchPanel.Controls.Add(_refreshButton);
        
        _headerPanel.Controls.Add(searchPanel);
        
        // ══════════════════════════════════════
        // Action Buttons Section
        // ══════════════════════════════════════
        Panel actionsPanel = new Panel
        {
            Location = new Point(20, 112),
            Size = new Size(900, 38),
            BackColor = Color.Transparent
        };
        
        _addButton = CreateModernButton("➕ إضافة حزمة", ColorScheme.Success, new Point(0, 0), 130, AddPackage_Click);
        actionsPanel.Controls.Add(_addButton);
        
        _editButton = CreateModernButton("✏️ تعديل", ColorScheme.Warning, new Point(140, 0), 95, EditPackage_Click);
        _editButton.Enabled = false;
        actionsPanel.Controls.Add(_editButton);
        
        _deleteButton = CreateModernButton("🗑️ حذف", ColorScheme.Error, new Point(245, 0), 85, DeletePackage_Click);
        _deleteButton.Enabled = false;
        actionsPanel.Controls.Add(_deleteButton);
        
        _detailsButton = CreateModernButton("📋 التفاصيل", ColorScheme.Info, new Point(340, 0), 105, ShowDetails_Click);
        _detailsButton.Enabled = false;
        actionsPanel.Controls.Add(_detailsButton);
        
        _headerPanel.Controls.Add(actionsPanel);
        
        // ══════════════════════════════════════
        // Modern DataGridView with Better Styling
        // ══════════════════════════════════════
        _packagesGrid = new DataGridView
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
        _packagesGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
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
        _packagesGrid.DefaultCellStyle = new DataGridViewCellStyle
        {
            SelectionBackColor = Color.FromArgb(232, 245, 233),
            SelectionForeColor = Color.FromArgb(27, 94, 32),
            Alignment = DataGridViewContentAlignment.MiddleCenter,
            Padding = new Padding(3),
            WrapMode = DataGridViewTriState.False,
            Font = new Font("Cairo", 9.5F)
        };
        
        // Alternating Rows for Better Readability
        _packagesGrid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(248, 249, 250),
            SelectionBackColor = Color.FromArgb(232, 245, 233),
            SelectionForeColor = Color.FromArgb(27, 94, 32)
        };
        
        InitializeGridColumns();
        _packagesGrid.SelectionChanged += PackagesGrid_SelectionChanged;
        _packagesGrid.CellDoubleClick += (s, e) =>
        {
            if (e.RowIndex >= 0) ShowDetails_Click(s, e);
        };
        
        // Add hover effect
        _packagesGrid.CellMouseEnter += (s, e) =>
        {
            if (e.RowIndex >= 0)
            {
                _packagesGrid.Cursor = Cursors.Hand;
            }
        };
        
        _packagesGrid.CellMouseLeave += (s, e) =>
        {
            _packagesGrid.Cursor = Cursors.Default;
        };
        
        // إضافة الـ Controls بالترتيب الصحيح
        this.SuspendLayout();
        
        // أولاً: إضافة الـ Grid (لأنه Dock.Fill سيملأ كل المساحة)
        this.Controls.Add(_packagesGrid);
        
        // ثانياً: إضافة الـ Header (لأنه Dock.Top سيظهر في الأعلى)
        this.Controls.Add(_headerPanel);
        
        this.ResumeLayout(false);
        this.PerformLayout();
    }
    
    private void InitializeGridColumns()
    {
        // Clear any existing columns
        _packagesGrid.Columns.Clear();
        
        // Hidden ID column
        _packagesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "UmrahPackageId",
            HeaderText = "ID",
            DataPropertyName = "UmrahPackageId",
            Visible = false
        });
        
        // Package Number - Compact but readable
        _packagesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "PackageNumber",
            HeaderText = "رقم الحزمة",
            DataPropertyName = "PackageNumber",
            Width = 115,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Font = new Font("Cairo", 9.5F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(250, 250, 250)
            }
        });
        
        // Date - Compact
        _packagesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Date",
            HeaderText = "التاريخ",
            DataPropertyName = "Date",
            Width = 95,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Format = "dd/MM/yyyy",
                Font = new Font("Cairo", 9F),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // Pilgrim Name - Fill remaining space
        _packagesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TripName",
            HeaderText = "اسم الرحلة",
            DataPropertyName = "TripName",
            MinimumWidth = 150,
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
        
        // Number of Persons - Compact
        _packagesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "NumberOfPersons",
            HeaderText = "الأفراد",
            DataPropertyName = "NumberOfPersons",
            Width = 70,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 87, 34),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(255, 243, 224)
            }
        });
        
        // Total Nights - Compact
        _packagesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TotalNights",
            HeaderText = "الليالي",
            DataPropertyName = "TotalNights",
            Width = 70,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Cairo", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(123, 31, 162),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // Selling Price per person
        _packagesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "SellingPrice",
            HeaderText = "السعر/فرد",
            DataPropertyName = "SellingPrice",
            Width = 110,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Format = "#,##0 ج",
                Font = new Font("Cairo", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 121, 107),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // Total Revenue
        _packagesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TotalRevenue",
            HeaderText = "الإيرادات",
            DataPropertyName = "TotalRevenue",
            Width = 110,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Format = "#,##0 ج",
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(21, 101, 192),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(227, 242, 253)
            }
        });
        
        // Net Profit
        _packagesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "NetProfit",
            HeaderText = "صافي الربح",
            DataPropertyName = "NetProfit",
            Width = 110,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle 
            { 
                Format = "#,##0 ج",
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // Status
        _packagesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Status",
            HeaderText = "الحالة",
            Width = 95,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Cairo", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(5, 3, 5, 3)
            }
        });
        
        // ✅ عمود اسم اليوزر
        _packagesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "CreatedByUserName",
            HeaderText = "المستخدم",
            Width = 140,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Cairo", 9F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // Set grid properties for optimal display
        _packagesGrid.RowTemplate.Height = 50;
        _packagesGrid.ColumnHeadersHeight = 50;
        _packagesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _packagesGrid.ScrollBars = ScrollBars.Both;
        _packagesGrid.AllowUserToResizeColumns = true;
        _packagesGrid.AllowUserToResizeRows = false;
    }
    
    private async Task LoadDataAsync()
    {
        try
        {
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("🚀 UmrahPackagesListForm.LoadDataAsync - START");
            Console.WriteLine($"👤 Current UserId: {_currentUserId}");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            
            _packagesGrid.DataSource = null;
            
            // Get filter parameters
            var statusFilter = _statusFilterCombo.SelectedIndex > 0
                ? (PackageStatus)_statusFilterCombo.SelectedIndex
                : (PackageStatus?)null;
            
            Console.WriteLine($"📊 Filters: activeOnly={_activeOnlyCheck.Checked}, status={statusFilter}");
            
            var packages = await _umrahService.GetAllPackagesAsync(
                activeOnly: _activeOnlyCheck.Checked,
                status: statusFilter
            );
            
            Console.WriteLine($"✅ Service returned {packages.Count} packages");
            
            // Debug: Print all packages
            if (packages.Any())
            {
                Console.WriteLine("\n📦 Packages received from service:");
                foreach (var p in packages)
                {
                    Console.WriteLine($"  - ID={p.UmrahPackageId}, Number={p.PackageNumber}, CreatedBy={p.CreatedBy}, Creator={p.Creator?.FullName ?? "NULL"}");
                }
            }
            else
            {
                Console.WriteLine("⚠️ No packages received from service!");
            }
            
            // Create display list
            var displayList = packages.Select(p => new
            {
                p.UmrahPackageId,
                p.PackageNumber,
                p.Date,
                TripName = p.TripName,
                p.NumberOfPersons,
                p.TotalNights,
                p.SellingPrice,
                p.TotalRevenue,
                p.NetProfit,
                Status = p.GetStatusDisplay(),
                p.IsActive,
                CreatedByUserName = p.Creator?.FullName ?? "غير معروف" // ✅ اسم المستخدم مباشرة
            }).ToList();
            
            Console.WriteLine($"📋 Display list created with {displayList.Count} items");
            
            _packagesGrid.DataSource = displayList;
            
            Console.WriteLine($"✅ Grid DataSource set. Row count: {_packagesGrid.Rows.Count}");
            
            // Color-code status and profit with modern badges
            foreach (DataGridViewRow row in _packagesGrid.Rows)
            {
                var status = row.Cells["Status"].Value?.ToString();
                var profitValue = row.Cells["NetProfit"].Value;
                
                if (profitValue == null || profitValue == DBNull.Value)
                    continue;
                
                decimal profit = Convert.ToDecimal(profitValue);
                
                // Status badge colors with better contrast
                var statusCell = row.Cells["Status"];
                statusCell.Style.Font = new Font("Cairo", 9F, FontStyle.Bold);
                statusCell.Style.Padding = new Padding(8, 5, 8, 5);
                
                switch (status)
                {
                    case "مكتمل":
                        statusCell.Style.BackColor = Color.FromArgb(232, 245, 233); // Light green
                        statusCell.Style.ForeColor = Color.FromArgb(27, 94, 32);    // Dark green
                        break;
                    case "ملغي":
                        statusCell.Style.BackColor = Color.FromArgb(255, 235, 238); // Light red
                        statusCell.Style.ForeColor = Color.FromArgb(183, 28, 28);   // Dark red
                        break;
                    case "قيد التنفيذ":
                        statusCell.Style.BackColor = Color.FromArgb(255, 243, 224); // Light orange
                        statusCell.Style.ForeColor = Color.FromArgb(230, 81, 0);    // Dark orange
                        break;
                    case "مؤكد":
                        statusCell.Style.BackColor = Color.FromArgb(227, 242, 253); // Light blue
                        statusCell.Style.ForeColor = Color.FromArgb(13, 71, 161);   // Dark blue
                        break;
                    case "مسودة":
                        statusCell.Style.BackColor = Color.FromArgb(245, 245, 245); // Light gray
                        statusCell.Style.ForeColor = Color.FromArgb(97, 97, 97);    // Dark gray
                        break;
                }
                
                // Profit colors - Just change color, don't modify value
                var profitCell = row.Cells["NetProfit"];
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
    
    private async void SearchPackages()
    {
        try
        {
            var searchTerm = _searchBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                await LoadDataAsync();
                return;
            }
            
            var packages = await _umrahService.SearchPackagesAsync(searchTerm);
            
            var displayList = packages.Select(p => new
            {
                p.UmrahPackageId,
                p.PackageNumber,
                p.Date,
                TripName = p.TripName,
                p.NumberOfPersons,
                p.TotalNights,
                p.SellingPrice,
                p.TotalRevenue,
                p.NetProfit,
                Status = p.GetStatusDisplay(),
                p.IsActive,
                CreatedByUserName = p.Creator?.FullName ?? "غير معروف" // ✅ إضافة عمود المستخدم
            }).ToList();
            
            _packagesGrid.DataSource = displayList;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"حدث خطأ أثناء البحث:\n{ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void AddPackage_Click(object? sender, EventArgs e)
    {
        var form = new AddEditUmrahPackageForm(_umrahService, _currentUserId);
        form.FormClosed += (s, args) => _ = LoadDataAsync();; // ✅ تحديث عند الإغلاق
        form.Show(); // ✅ فتح كنافذة منفصلة
    }
    
    private void EditPackage_Click(object? sender, EventArgs e)
    {
        if (_packagesGrid.SelectedRows.Count == 0) return;
        
        int packageId = Convert.ToInt32(_packagesGrid.SelectedRows[0].Cells["UmrahPackageId"].Value);
        var form = new AddEditUmrahPackageForm(_umrahService, _currentUserId, packageId);
        form.FormClosed += (s, args) => _ = LoadDataAsync();; // ✅ تحديث عند الإغلاق
        form.Show(); // ✅ فتح كنافذة منفصلة
    }
    
    private async void DeletePackage_Click(object? sender, EventArgs e)
    {
        if (_packagesGrid.SelectedRows.Count == 0) return;
        
        var packageId = Convert.ToInt32(_packagesGrid.SelectedRows[0].Cells["UmrahPackageId"].Value);
        var packageNumber = _packagesGrid.SelectedRows[0].Cells["PackageNumber"].Value?.ToString() ?? "غير معروف";
        
        var result = MessageBox.Show(
            $"هل أنت متأكد من حذف الحزمة {packageNumber}؟",
            "تأكيد الحذف",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );
        
        if (result == DialogResult.Yes)
        {
            try
            {
                await _umrahService.DeletePackageAsync(packageId);
                MessageBox.Show("تم حذف الحزمة بنجاح", "نجاح",
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
        if (_packagesGrid.SelectedRows.Count == 0) return;
        
        int packageId = Convert.ToInt32(_packagesGrid.SelectedRows[0].Cells["UmrahPackageId"].Value!);
        var form = new UmrahPackageDetailsForm(_umrahService, packageId);
        form.Show(); // ✅ فتح كنافذة منفصلة
    }
    
    private void PackagesGrid_SelectionChanged(object? sender, EventArgs e)
    {
        bool hasSelection = _packagesGrid.SelectedRows.Count > 0;
        _editButton.Enabled = hasSelection;
        _deleteButton.Enabled = hasSelection;
        _detailsButton.Enabled = hasSelection;
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
