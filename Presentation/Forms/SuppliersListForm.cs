using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using ClosedXML.Excel;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class SuppliersListForm : Form
{
    private readonly ISupplierService _supplierService;
    private readonly int _currentUserId;
    
    private DataGridView _suppliersGrid = null!;
    private TextBox _searchText = null!;
    private ComboBox _filterCombo = null!;
    private Button _addButton = null!;
    private Button _editButton = null!;
    private Button _deleteButton = null!;
    private Button _viewStatementButton = null!;
    private Button _refreshButton = null!;
    private Button _exportButton = null!;
    private CheckBox _showInactiveCheck = null!;
    private Label _statsLabel = null!;
    
    private List<Supplier> _allSuppliers = new();
    private bool _isInitialized = false;
    
    public SuppliersListForm(ISupplierService supplierService, int currentUserId)
    {
        _supplierService = supplierService;
        _currentUserId = currentUserId;
        
        InitializeComponent();
        
        this.Text = "إدارة الموردين";
        this.Size = new Size(1500, 850);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
        this.WindowState = FormWindowState.Maximized;
        
        InitializeCustomControls();
        _ = LoadSuppliersAsync();
    }
    
    private void InitializeCustomControls()
    {
        // Main Panel with gradient background
        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ColorScheme.Background,
            Padding = new Padding(25)
        };
        
        int yPos = 20;
        
        // ==================== HEADER SECTION ====================
        Panel headerPanel = CreateHeaderPanel();
        headerPanel.Location = new Point(25, yPos);
        mainPanel.Controls.Add(headerPanel);
        yPos += headerPanel.Height + 20;
        
        // ==================== SEARCH AND FILTER SECTION ====================
        Panel searchPanel = CreateSearchFilterPanel();
        searchPanel.Location = new Point(25, yPos);
        mainPanel.Controls.Add(searchPanel);
        yPos += searchPanel.Height + 15;
        
        // ==================== ACTION BUTTONS SECTION ====================
        Panel buttonsPanel = CreateActionButtonsPanel();
        buttonsPanel.Location = new Point(25, yPos);
        mainPanel.Controls.Add(buttonsPanel);
        yPos += buttonsPanel.Height + 15;
        
        // ==================== STATISTICS BAR ====================
        Panel statsPanel = CreateStatsPanel();
        statsPanel.Location = new Point(25, yPos);
        mainPanel.Controls.Add(statsPanel);
        yPos += statsPanel.Height + 15;
        
        // ==================== DATA GRID ====================
        _suppliersGrid = CreateSuppliersDataGrid();
        _suppliersGrid.Location = new Point(25, yPos);
        _suppliersGrid.Size = new Size(
            this.ClientSize.Width - 50,
            this.ClientSize.Height - yPos - 40
        );
        mainPanel.Controls.Add(_suppliersGrid);
        
        this.Controls.Add(mainPanel);
        
        // Mark initialization as complete
        _isInitialized = true;
        
        // Handle form resize
        this.Resize += (s, e) =>
        {
            if (_suppliersGrid != null && this.WindowState != FormWindowState.Minimized)
            {
                _suppliersGrid.Size = new Size(
                    this.ClientSize.Width - 50,
                    this.ClientSize.Height - _suppliersGrid.Top - 40
                );
            }
        };
    }
    
    // ==================== UI COMPONENT CREATORS ====================
    
    private Panel CreateHeaderPanel()
    {
        Panel header = new Panel
        {
            Size = new Size(this.ClientSize.Width - 50, 100),
            BackColor = Color.White
        };
        
        // Add shadow effect
        header.Paint += (s, e) =>
        {
            using (var shadow = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Rectangle(0, header.Height - 4, header.Width, 4),
                Color.FromArgb(40, 0, 0, 0),
                Color.Transparent,
                90f))
            {
                e.Graphics.FillRectangle(shadow, 0, header.Height - 4, header.Width, 4);
            }
        };
        
        // Icon
        Label icon = new Label
        {
            Text = "🏢",
            Font = new Font("Segoe UI Emoji", 32F),
            AutoSize = true,
            Location = new Point(header.Width - 70, 25)
        };
        header.Controls.Add(icon);
        
        // Title
        Label title = new Label
        {
            Text = "إدارة الموردين",
            Font = new Font("Cairo", 20F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(header.Width - 280, 20)
        };
        header.Controls.Add(title);
        
        // Subtitle
        Label subtitle = new Label
        {
            Text = "إدارة شاملة لجميع الموردين ومتابعة حساباتهم",
            Font = new Font("Cairo", 10F),
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(header.Width - 350, 60)
        };
        header.Controls.Add(subtitle);
        
        return header;
    }
    
    private Panel CreateSearchFilterPanel()
    {
        Panel panel = new Panel
        {
            Size = new Size(this.ClientSize.Width - 50, 70),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        
        // Search Icon and Label
        Label searchLabel = new Label
        {
            Text = "🔍",
            Font = new Font("Segoe UI Emoji", 14F),
            AutoSize = true,
            Location = new Point(panel.Width - 45, 22)
        };
        panel.Controls.Add(searchLabel);
        
        // Search TextBox
        _searchText = new TextBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(350, 35),
            Location = new Point(panel.Width - 430, 18),
            BorderStyle = BorderStyle.FixedSingle
        };
        _searchText.TextChanged += SearchText_Changed;
        panel.Controls.Add(_searchText);
        
        // Placeholder for search
        _searchText.Enter += (s, e) =>
        {
            if (_searchText.Text == "ابحث بالكود، الاسم، الهاتف، أو البريد...")
            {
                _searchText.Text = "";
                _searchText.ForeColor = Color.Black;
            }
        };
        
        _searchText.Leave += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(_searchText.Text))
            {
                _searchText.Text = "ابحث بالكود، الاسم، الهاتف، أو البريد...";
                _searchText.ForeColor = Color.Gray;
            }
        };
        
        _searchText.Text = "ابحث بالكود، الاسم، الهاتف، أو البريد...";
        _searchText.ForeColor = Color.Gray;
        
        // Filter Label
        Label filterLabel = new Label
        {
            Text = "تصفية:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(panel.Width - 520, 25)
        };
        panel.Controls.Add(filterLabel);
        
        // Filter ComboBox
        _filterCombo = new ComboBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(180, 35),
            Location = new Point(panel.Width - 720, 20),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _filterCombo.Items.AddRange(new object[] { 
            "الكل", 
            "النشطين فقط", 
            "غير النشطين", 
            "لهم رصيد دائن",
            "عليهم رصيد مدين" 
        });
        _filterCombo.SelectedIndex = 1; // Default to "النشطين فقط"
        _filterCombo.SelectedIndexChanged += (s, e) => FilterSuppliers();
        panel.Controls.Add(_filterCombo);
        
        // Show Inactive CheckBox
        _showInactiveCheck = new CheckBox
        {
            Text = "📋 عرض غير النشطين",
            Font = new Font("Cairo", 9.5F),
            AutoSize = true,
            Location = new Point(20, 25),
            Visible = false // Will be controlled by filter combo
        };
        _showInactiveCheck.CheckedChanged += (s, e) => FilterSuppliers();
        panel.Controls.Add(_showInactiveCheck);
        
        return panel;
    }
    
    private Panel CreateActionButtonsPanel()
    {
        Panel panel = new Panel
        {
            Size = new Size(this.ClientSize.Width - 50, 60),
            BackColor = Color.Transparent
        };
        
        int buttonX = panel.Width - 200;
        
        // Add Supplier Button
        _addButton = CreateStyledButton(
            "➕ مورد جديد",
            ColorScheme.Success,
            new Point(buttonX, 10),
            AddSupplier_Click
        );
        panel.Controls.Add(_addButton);
        buttonX -= 180;
        
        // Edit Button
        _editButton = CreateStyledButton(
            "✏️ تعديل",
            ColorScheme.Warning,
            new Point(buttonX, 10),
            EditSupplier_Click
        );
        panel.Controls.Add(_editButton);
        buttonX -= 160;
        
        // Delete Button
        _deleteButton = CreateStyledButton(
            "🗑️ حذف",
            ColorScheme.Error,
            new Point(buttonX, 10),
            DeleteSupplier_Click
        );
        panel.Controls.Add(_deleteButton);
        buttonX -= 160;
        
        // View Statement Button
        _viewStatementButton = CreateStyledButton(
            "📊 كشف حساب",
            Color.FromArgb(13, 110, 253), // Bootstrap blue
            new Point(buttonX, 10),
            ViewStatement_Click
        );
        panel.Controls.Add(_viewStatementButton);
        buttonX -= 180;
        
        // Refresh Button
        _refreshButton = CreateStyledButton(
            "🔄 تحديث",
            ColorScheme.Primary,
            new Point(buttonX, 10),
            (s, e) => _ = LoadSuppliersAsync()
        );
        panel.Controls.Add(_refreshButton);
        buttonX -= 180;
        
        // Export Button
        _exportButton = CreateStyledButton(
            "📊 تصدير Excel",
            Color.FromArgb(40, 167, 69),
            new Point(buttonX, 10),
            ExportToExcel_Click
        );
        panel.Controls.Add(_exportButton);
        
        return panel;
    }
    
    private Panel CreateStatsPanel()
    {
        Panel panel = new Panel
        {
            Size = new Size(this.ClientSize.Width - 50, 50),
            BackColor = Color.FromArgb(240, 248, 255),
            BorderStyle = BorderStyle.FixedSingle
        };
        
        _statsLabel = new Label
        {
            Text = "📊 جاري التحميل...",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = false,
            Size = new Size(panel.Width - 40, 40),
            Location = new Point(20, 10),
            TextAlign = ContentAlignment.MiddleRight
        };
        panel.Controls.Add(_statsLabel);
        
        return panel;
    }
    
    private DataGridView CreateSuppliersDataGrid()
    {
        DataGridView grid = new DataGridView
        {
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            Font = new Font("Cairo", 10F),
            RightToLeft = RightToLeft.Yes,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            EnableHeadersVisualStyles = false,
            RowHeadersVisible = false,
            AllowUserToResizeRows = false
        };
        
        ConfigureSuppliersGrid(grid);
        
        // Double click to edit
        grid.CellDoubleClick += (s, e) =>
        {
            if (e.RowIndex >= 0)
                EditSupplier_Click(s, e);
        };
        
        // Alternating row colors
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
        
        return grid;
    }
    
    private void ConfigureSuppliersGrid(DataGridView grid)
    {
        grid.Columns.Clear();
        
        // Hidden ID Column
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "SupplierId",
            HeaderText = "الرقم",
            Visible = false
        });
        
        // Supplier Code
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "SupplierCode",
            HeaderText = "📋 كود",
            Width = 90,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Cairo", 9.5F, FontStyle.Bold),
                ForeColor = ColorScheme.Primary,
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // Supplier Name
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "SupplierName",
            HeaderText = "🏢 اسم المورد",
            Width = 250,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Cairo", 10F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleRight,
                Padding = new Padding(10, 0, 10, 0)
            }
        });
        
        // Phone
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Phone",
            HeaderText = "📞 تليفون",
            Width = 110,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Arial", 9.5F)
            }
        });
        
        // Mobile
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Mobile",
            HeaderText = "📱 موبايل",
            Width = 110,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Arial", 9.5F)
            }
        });
        
        // Email
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Email",
            HeaderText = "✉️ البريد",
            Width = 200,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Font = new Font("Arial", 9F),
                Padding = new Padding(10, 0, 10, 0)
            }
        });
        
        // City
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "City",
            HeaderText = "🏙️ المدينة",
            Width = 110,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        });
        
        // Current Balance
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "CurrentBalance",
            HeaderText = "💰 الرصيد",
            Width = 120,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "N2",
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Arial", 10F, FontStyle.Bold)
            }
        });
        
        // Payment Terms
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "PaymentTerms",
            HeaderText = "⏱️ السداد",
            Width = 90,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Cairo", 9F)
            }
        });
        
        // Status
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "IsActive",
            HeaderText = "✓ الحالة",
            Width = 90,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Cairo", 9.5F, FontStyle.Bold)
            }
        });
        
        // Header Styling
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            Font = new Font("Cairo", 10.5F, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleCenter,
            Padding = new Padding(10),
            SelectionBackColor = ColorScheme.Primary
        };
        
        grid.ColumnHeadersHeight = 50;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.RowTemplate.Height = 45;
        
        // Selection colors
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 240, 255);
        grid.DefaultCellStyle.SelectionForeColor = Color.Black;
        
        // Allow column resizing
        grid.AllowUserToResizeColumns = true;
    }
    
    private Button CreateStyledButton(string text, Color bgColor, Point location, EventHandler clickHandler)
    {
        Button btn = new Button
        {
            Text = text,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Size = new Size(170, 45),
            Location = location,
            BackColor = bgColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            TabStop = false
        };
        
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(bgColor, 0.1f);
        btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(bgColor, 0.1f);
        
        btn.Click += clickHandler;
        return btn;
    }
    
    // ==================== DATA LOADING AND FILTERING ====================
    
    private async Task LoadSuppliersAsync()
    {
        try
        {
            _statsLabel.Text = "⏳ جاري تحميل البيانات...";
            _statsLabel.ForeColor = Color.Gray;
            
            _allSuppliers = await _supplierService.GetAllSuppliersAsync();
            FilterSuppliers();
            
            UpdateStatistics();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"❌ خطأ في تحميل الموردين:\n\n{ex.Message}",
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            
            _statsLabel.Text = "❌ فشل التحميل";
            _statsLabel.ForeColor = ColorScheme.Error;
        }
    }
    
    // ==================== EVENT HANDLERS ====================
    
    private void SearchText_Changed(object? sender, EventArgs e)
    {
        FilterSuppliers();
    }
    
    private void FilterSuppliers()
    {
        // Don't filter until initialization is complete
        if (!_isInitialized) return;
        
        var filtered = _allSuppliers.AsEnumerable();
        
        // Apply search filter
        if (!string.IsNullOrWhiteSpace(_searchText.Text) && 
            _searchText.Text != "ابحث بالكود، الاسم، الهاتف، أو البريد...")
        {
            var searchTerm = _searchText.Text.ToLower().Trim();
            filtered = filtered.Where(s => 
                (s.SupplierCode ?? "").ToLower().Contains(searchTerm) ||
                (s.SupplierName ?? "").ToLower().Contains(searchTerm) ||
                (s.Phone ?? "").Contains(searchTerm) ||
                (s.Mobile ?? "").Contains(searchTerm) ||
                (s.Email ?? "").ToLower().Contains(searchTerm));
        }
        
        // Apply combo filter
        switch (_filterCombo.SelectedIndex)
        {
            case 0: // الكل
                break;
            case 1: // النشطين فقط
                filtered = filtered.Where(s => s.IsActive);
                break;
            case 2: // غير النشطين
                filtered = filtered.Where(s => !s.IsActive);
                break;
            case 3: // لهم رصيد دائن (Creditors - we owe them)
                filtered = filtered.Where(s => s.CurrentBalance > 0);
                break;
            case 4: // عليهم رصيد مدين (Debtors - they owe us)
                filtered = filtered.Where(s => s.CurrentBalance < 0);
                break;
        }
        
        RefreshGrid(filtered.ToList());
    }
    
    private void RefreshGrid(List<Supplier> suppliers)
    {
        if (!_isInitialized || _suppliersGrid == null) return;
        
        _suppliersGrid.Rows.Clear();
        
        foreach (var supplier in suppliers)
        {
            var balanceColor = supplier.CurrentBalance > 0 ? Color.Green : 
                              supplier.CurrentBalance < 0 ? Color.Red : Color.Gray;
            
            var statusText = supplier.IsActive ? "✅ نشط" : "⛔ غير نشط";
            var statusColor = supplier.IsActive ? Color.Green : Color.Red;
            
            var paymentTerms = supplier.PaymentTermDays > 0 
                ? $"{supplier.PaymentTermDays} يوم" 
                : "فوري";
            
            int rowIndex = _suppliersGrid.Rows.Add(
                supplier.SupplierId,
                supplier.SupplierCode,
                supplier.SupplierName,
                supplier.Phone ?? "-",
                supplier.Mobile ?? "-",
                supplier.Email ?? "-",
                supplier.City ?? "-",
                supplier.CurrentBalance,
                paymentTerms,
                statusText
            );
            
            // تلوين الرصيد
            _suppliersGrid.Rows[rowIndex].Cells["CurrentBalance"].Style.ForeColor = balanceColor;
            
            // تلوين الحالة
            _suppliersGrid.Rows[rowIndex].Cells["IsActive"].Style.ForeColor = statusColor;
            
            // Store the supplier object in Tag
            _suppliersGrid.Rows[rowIndex].Tag = supplier;
        }
        
        UpdateStatistics(suppliers);
    }
    
    private void UpdateStatistics(List<Supplier>? suppliers = null)
    {
        if (!_isInitialized || _statsLabel == null) return;
        
        suppliers ??= _allSuppliers;
        
        if (suppliers == null || suppliers.Count == 0)
        {
            _statsLabel.Text = "📊 لا توجد نتائج";
            _statsLabel.ForeColor = Color.Gray;
            return;
        }
        
        var total = suppliers.Count;
        var active = suppliers.Count(s => s.IsActive);
        var inactive = total - active;
        var totalBalance = suppliers.Sum(s => s.CurrentBalance);
        var creditors = suppliers.Count(s => s.CurrentBalance > 0); // لنا عليهم
        var debtors = suppliers.Count(s => s.CurrentBalance < 0); // لهم علينا
        
        var balanceColor = totalBalance > 0 ? "🟢" : totalBalance < 0 ? "🔴" : "⚪";
        
        _statsLabel.Text = $"📊 إجمالي: {total} | ✅ نشط: {active} | ⛔ غير نشط: {inactive} | " +
                          $"{balanceColor} الرصيد: {totalBalance:N2} ج.م | " +
                          $"📈 دائن: {creditors} | 📉 مدين: {debtors}";
        
        _statsLabel.ForeColor = ColorScheme.Primary;
    }
    
    private async void AddSupplier_Click(object? sender, EventArgs e)
    {
        var addForm = Program.ServiceProvider!.GetRequiredService<AddEditSupplierForm>();
        if (addForm.ShowDialog() == DialogResult.OK)
        {
            await LoadSuppliersAsync();
        }
    }
    
    private async void EditSupplier_Click(object? sender, EventArgs e)
    {
        try
        {
            // Check if any row is selected
            if (_suppliersGrid.SelectedRows.Count == 0 && _suppliersGrid.CurrentRow == null)
            {
                MessageBox.Show("الرجاء اختيار مورد للتعديل", "تنبيه", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Get the selected row
            DataGridViewRow selectedRow = _suppliersGrid.SelectedRows.Count > 0 
                ? _suppliersGrid.SelectedRows[0] 
                : _suppliersGrid.CurrentRow!;
            
            // Try to get supplier from Tag
            var supplier = selectedRow.Tag as Supplier;
            
            // If Tag is null, try to get supplier by ID from first cell
            if (supplier == null)
            {
                var supplierIdCell = selectedRow.Cells["SupplierId"].Value;
                if (supplierIdCell != null && int.TryParse(supplierIdCell.ToString(), out int supplierId))
                {
                    supplier = _allSuppliers.FirstOrDefault(s => s.SupplierId == supplierId);
                }
            }
            
            if (supplier == null)
            {
                MessageBox.Show(
                    "خطأ في قراءة بيانات المورد. يرجى تحديث القائمة والمحاولة مرة أخرى.",
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            
            // Open edit form
            var editForm = new AddEditSupplierForm(_supplierService, supplier);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                await LoadSuppliersAsync();
                MessageBox.Show("تم التحديث بنجاح ✅", "نجاح", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"خطأ في فتح نموذج التعديل:\n\n{ex.Message}",
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
    
    private async void DeleteSupplier_Click(object? sender, EventArgs e)
    {
        try
        {
            // Check if any row is selected
            if (_suppliersGrid.SelectedRows.Count == 0 && _suppliersGrid.CurrentRow == null)
            {
                MessageBox.Show("الرجاء اختيار مورد للحذف", "تنبيه", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Get the selected row
            DataGridViewRow selectedRow = _suppliersGrid.SelectedRows.Count > 0 
                ? _suppliersGrid.SelectedRows[0] 
                : _suppliersGrid.CurrentRow!;
            
            // Try to get supplier from Tag
            var supplier = selectedRow.Tag as Supplier;
            
            // If Tag is null, try to get supplier by ID from first cell
            if (supplier == null)
            {
                var supplierIdCell = selectedRow.Cells["SupplierId"].Value;
                if (supplierIdCell != null && int.TryParse(supplierIdCell.ToString(), out int supplierId))
                {
                    supplier = _allSuppliers.FirstOrDefault(s => s.SupplierId == supplierId);
                }
            }
            
            if (supplier == null)
            {
                MessageBox.Show(
                    "خطأ في قراءة بيانات المورد. يرجى تحديث القائمة والمحاولة مرة أخرى.",
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            
            // Confirm deletion
            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف المورد:\n\n" +
                $"الاسم: {supplier.SupplierName}\n" +
                $"الكود: {supplier.SupplierCode}\n\n" +
                $"⚠️ ملحوظة: الحذف سيكون soft delete (تعطيل المورد فقط)",
                "تأكيد الحذف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            
            if (result == DialogResult.Yes)
            {
                await _supplierService.DeleteSupplierAsync(supplier.SupplierId);
                MessageBox.Show(
                    "✅ تم حذف المورد بنجاح\n\nالمورد الآن غير نشط ولن يظهر في القوائم",
                    "نجاح", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);
                await LoadSuppliersAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"❌ خطأ في حذف المورد:\n\n{ex.Message}",
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
    
    private void ExportToExcel_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_allSuppliers == null || _allSuppliers.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات للتصدير", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the currently filtered suppliers from the grid
            var suppliersToExport = new List<Supplier>();
            foreach (DataGridViewRow gridRow in _suppliersGrid.Rows)
            {
                if (gridRow.Tag is Supplier supplier)
                {
                    suppliersToExport.Add(supplier);
                }
            }

            if (suppliersToExport.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات مفلترة للتصدير", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Create SaveFileDialog
            using var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "حفظ ملف Excel",
                FileName = $"الموردين_{DateTime.Now:yyyy-MM-dd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;

            // Create Excel workbook
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("الموردين");

            // Set RTL direction
            worksheet.RightToLeft = true;

            // Add headers
            worksheet.Cell(1, 1).Value = "كود المورد";
            worksheet.Cell(1, 2).Value = "اسم المورد";
            worksheet.Cell(1, 3).Value = "الاسم بالإنجليزية";
            worksheet.Cell(1, 4).Value = "التليفون";
            worksheet.Cell(1, 5).Value = "الموبايل";
            worksheet.Cell(1, 6).Value = "البريد الإلكتروني";
            worksheet.Cell(1, 7).Value = "العنوان";
            worksheet.Cell(1, 8).Value = "المدينة";
            worksheet.Cell(1, 9).Value = "الدولة";
            worksheet.Cell(1, 10).Value = "الرقم الضريبي";
            worksheet.Cell(1, 11).Value = "حد الائتمان";
            worksheet.Cell(1, 12).Value = "مدة السداد (يوم)";
            worksheet.Cell(1, 13).Value = "الرصيد الافتتاحي";
            worksheet.Cell(1, 14).Value = "الرصيد الحالي";
            worksheet.Cell(1, 15).Value = "الحالة";
            worksheet.Cell(1, 16).Value = "ملاحظات";

            // Style header row
            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Font.FontSize = 12;
            headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(41, 128, 185);
            headerRow.Style.Font.FontColor = XLColor.White;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRow.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRow.Height = 30;

            // Add data rows
            int row = 2;
            foreach (var supplier in suppliersToExport)
            {
                worksheet.Cell(row, 1).Value = supplier.SupplierCode;
                worksheet.Cell(row, 2).Value = supplier.SupplierName;
                worksheet.Cell(row, 3).Value = supplier.SupplierNameEn;
                worksheet.Cell(row, 4).Value = supplier.Phone ?? "";
                worksheet.Cell(row, 5).Value = supplier.Mobile ?? "";
                worksheet.Cell(row, 6).Value = supplier.Email ?? "";
                worksheet.Cell(row, 7).Value = supplier.Address ?? "";
                worksheet.Cell(row, 8).Value = supplier.City ?? "";
                worksheet.Cell(row, 9).Value = supplier.Country ?? "";
                worksheet.Cell(row, 10).Value = supplier.TaxNumber ?? "";
                worksheet.Cell(row, 11).Value = supplier.CreditLimit;
                worksheet.Cell(row, 12).Value = supplier.PaymentTermDays;
                worksheet.Cell(row, 13).Value = supplier.OpeningBalance;
                worksheet.Cell(row, 14).Value = supplier.CurrentBalance;
                worksheet.Cell(row, 15).Value = supplier.IsActive ? "نشط" : "غير نشط";
                worksheet.Cell(row, 16).Value = supplier.Notes ?? "";

                // Format currency columns
                worksheet.Cell(row, 11).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(row, 13).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(row, 14).Style.NumberFormat.Format = "#,##0.00";

                // Color code the current balance
                if (supplier.CurrentBalance > 0)
                {
                    worksheet.Cell(row, 14).Style.Font.FontColor = XLColor.Green;
                    worksheet.Cell(row, 14).Style.Font.Bold = true;
                }
                else if (supplier.CurrentBalance < 0)
                {
                    worksheet.Cell(row, 14).Style.Font.FontColor = XLColor.Red;
                    worksheet.Cell(row, 14).Style.Font.Bold = true;
                }

                // Color code status
                if (supplier.IsActive)
                {
                    worksheet.Cell(row, 15).Style.Font.FontColor = XLColor.Green;
                    worksheet.Cell(row, 15).Style.Font.Bold = true;
                }
                else
                {
                    worksheet.Cell(row, 15).Style.Font.FontColor = XLColor.Red;
                }

                // Alternating row colors
                if (row % 2 == 0)
                {
                    worksheet.Range(row, 1, row, 16).Style.Fill.BackgroundColor = XLColor.FromArgb(240, 248, 255);
                }

                row++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Add borders to all cells
            var dataRange = worksheet.Range(1, 1, row - 1, 16);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Add summary row
            row++;
            worksheet.Cell(row, 1).Value = "الإجمالي:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 1).Style.Font.FontSize = 12;
            worksheet.Cell(row, 2).Value = $"{suppliersToExport.Count} مورد";
            worksheet.Cell(row, 2).Style.Font.Bold = true;

            var totalBalance = suppliersToExport.Sum(s => s.CurrentBalance);
            worksheet.Cell(row, 13).Value = "إجمالي الرصيد:";
            worksheet.Cell(row, 13).Style.Font.Bold = true;
            worksheet.Cell(row, 14).Value = totalBalance;
            worksheet.Cell(row, 14).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 14).Style.Font.Bold = true;
            worksheet.Cell(row, 14).Style.Font.FontSize = 12;

            if (totalBalance > 0)
                worksheet.Cell(row, 14).Style.Font.FontColor = XLColor.Green;
            else if (totalBalance < 0)
                worksheet.Cell(row, 14).Style.Font.FontColor = XLColor.Red;

            // Highlight summary row
            worksheet.Range(row, 1, row, 16).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 200);

            // Save the workbook
            workbook.SaveAs(saveDialog.FileName);

            MessageBox.Show(
                $"✅ تم تصدير {suppliersToExport.Count} مورد بنجاح!\n\nالملف: {Path.GetFileName(saveDialog.FileName)}",
                "نجاح التصدير",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            // Ask if user wants to open the file
            var openResult = MessageBox.Show(
                "هل تريد فتح الملف الآن؟",
                "فتح الملف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (openResult == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = saveDialog.FileName,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"❌ خطأ في التصدير:\n\n{ex.Message}",
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
    
    private async void ViewStatement_Click(object? sender, EventArgs e)
    {
        if (_suppliersGrid.CurrentRow == null)
        {
            MessageBox.Show("الرجاء اختيار مورد لعرض كشف حسابه", "تنبيه", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        var supplier = _suppliersGrid.CurrentRow.Tag as Supplier;
        if (supplier == null)
        {
            MessageBox.Show("خطأ في قراءة بيانات المورد", "خطأ", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        
        try
        {
            // Get date range (default: current year)
            var startDate = new DateTime(DateTime.Now.Year, 1, 1);
            var endDate = DateTime.Now;
            
            // You can add a date picker dialog here if needed
            var result = MessageBox.Show(
                $"عرض كشف حساب للمورد:\n{supplier.SupplierName}\n\n" +
                $"الفترة: من {startDate:yyyy/MM/dd} إلى {endDate:yyyy/MM/dd}\n\n" +
                "هل تريد المتابعة؟",
                "تأكيد",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            
            if (result != DialogResult.Yes)
                return;
            
            // Get supplier statement
            var statement = await _supplierService.GetSupplierStatementAsync(
                supplier.SupplierId,
                startDate,
                endDate
            );
            
            // Create and show statement form
            var statementForm = new SupplierStatementForm(statement);
            statementForm.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"خطأ في عرض كشف الحساب:\n\n{ex.Message}",
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
