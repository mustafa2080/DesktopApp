using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class CustomersListForm : Form
{
    private readonly ICustomerService _customerService;
    private readonly int _currentUserId;
    
    private Panel _headerPanel = null!;
    private TextBox _searchBox = null!;
    private Button _searchButton = null!;
    private Button _addButton = null!;
    private Button _editButton = null!;
    private Button _deleteButton = null!;
    private Button _statementButton = null!;
    private Button _refreshButton = null!;
    private CheckBox _activeOnlyCheck = null!;
    private DataGridView _customersGrid = null!;
    
    public CustomersListForm(ICustomerService customerService, int currentUserId)
    {
        _customerService = customerService;
        _currentUserId = currentUserId;
        
        InitializeComponent();
        InitializeCustomControls();
        _ = LoadDataAsync();
    }
    
    private void InitializeComponent()
    {
        this.Text = "إدارة العملاء";
        this.Size = new Size(1500, 900);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
        this.WindowState = FormWindowState.Maximized; // فتح في وضع ملء الشاشة
    }
    
    private void InitializeCustomControls()
    {
        // Header Panel
        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 140,
            BackColor = Color.White,
            Padding = new Padding(20)
        };
        
        Label titleLabel = new Label
        {
            Text = "👥 إدارة العملاء",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(20, 20)
        };
        _headerPanel.Controls.Add(titleLabel);
        
        // Search Box
        Label searchLabel = new Label
        {
            Text = "بحث:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 75)
        };
        _headerPanel.Controls.Add(searchLabel);
        
        _searchBox = new TextBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(300, 30),
            Location = new Point(80, 72),
            PlaceholderText = "اسم العميل، الكود، الهاتف..."
        };
        _searchBox.KeyPress += (s, e) => { if (e.KeyChar == (char)13) SearchCustomers(); };
        _headerPanel.Controls.Add(_searchBox);
        
        _searchButton = CreateButton("🔍 بحث", ColorScheme.Primary, new Point(400, 72), (s, e) => SearchCustomers());
        _headerPanel.Controls.Add(_searchButton);
        
        _activeOnlyCheck = new CheckBox
        {
            Text = "العملاء النشطين فقط",
            Font = new Font("Cairo", 10F),
            AutoSize = true,
            Location = new Point(580, 75),
            Checked = false
        };
        _activeOnlyCheck.CheckedChanged += (s, e) => _ = LoadDataAsync();
        _headerPanel.Controls.Add(_activeOnlyCheck);
        
        // Action Buttons
        _addButton = CreateButton("➕ إضافة عميل", ColorScheme.Success, new Point(800, 72), AddCustomer_Click);
        _headerPanel.Controls.Add(_addButton);
        
        _editButton = CreateButton("✏️ تعديل", ColorScheme.Warning, new Point(980, 72), EditCustomer_Click);
        _headerPanel.Controls.Add(_editButton);
        
        _deleteButton = CreateButton("🗑️ حذف", ColorScheme.Error, new Point(1120, 72), DeleteCustomer_Click);
        _headerPanel.Controls.Add(_deleteButton);
        
        // Main Panel للـ Grid (يملأ المساحة المتبقية) - يضاف أولاً
        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(20)
        };
        
        // DataGridView
        _customersGrid = CreateDataGrid();
        mainPanel.Controls.Add(_customersGrid);
        
        this.Controls.Add(mainPanel);
        
        // Additional Buttons Panel (بين الهيدر والجدول) - يضاف ثانياً
        Panel buttonsPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.White,
            Padding = new Padding(20, 10, 20, 10)
        };
        
        _statementButton = CreateButton("📋 كشف حساب", Color.FromArgb(52, 152, 219), new Point(20, 10), ViewStatement_Click);
        buttonsPanel.Controls.Add(_statementButton);
        
        _refreshButton = CreateButton("🔄 تحديث", Color.FromArgb(52, 73, 94), new Point(200, 10), (s, e) => _ = LoadDataAsync());
        buttonsPanel.Controls.Add(_refreshButton);
        
        this.Controls.Add(buttonsPanel);
        
        // Header Panel - يضاف أخيراً (عشان يظهر فوق)
        this.Controls.Add(_headerPanel);
    }
    
    private DataGridView CreateDataGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            RowHeadersVisible = false,
            Font = new Font("Cairo", 9.5F),
            RightToLeft = RightToLeft.Yes,
            EnableHeadersVisualStyles = false,
            ScrollBars = ScrollBars.Both,
            AllowUserToResizeColumns = true
        };
        
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleCenter,
            Padding = new Padding(5),
            WrapMode = DataGridViewTriState.False
        };
        
        grid.DefaultCellStyle.Padding = new Padding(5);
        grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        grid.ColumnHeadersHeight = 55;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.RowTemplate.Height = 40;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
        grid.DefaultCellStyle.SelectionBackColor = ColorScheme.Primary;
        grid.DefaultCellStyle.SelectionForeColor = Color.White;
        
        return grid;
    }
    
    private Button CreateButton(string text, Color bgColor, Point location, EventHandler clickHandler)
    {
        Button btn = new Button
        {
            Text = text,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Size = new Size(160, 40),
            Location = location,
            BackColor = bgColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += clickHandler;
        return btn;
    }
    
    private async Task LoadDataAsync()
    {
        try
        {
            List<Customer> customers;
            
            if (_activeOnlyCheck.Checked)
            {
                customers = await _customerService.GetActiveCustomersAsync();
            }
            else
            {
                customers = await _customerService.GetAllCustomersAsync();
            }
            
            DisplayCustomers(customers);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void DisplayCustomers(List<Customer> customers)
    {
        _customersGrid.Columns.Clear();
        _customersGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        
        _customersGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Visible = false });
        _customersGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Code", HeaderText = "الكود", Width = 90 });
        _customersGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "اسم العميل", Width = 180, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 100 });
        _customersGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Phone", HeaderText = "الهاتف", Width = 110 });
        _customersGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Mobile", HeaderText = "الموبايل", Width = 110 });
        _customersGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "البريد الإلكتروني", Width = 160, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 80 });
        _customersGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Balance", HeaderText = "الرصيد", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Cairo", 9.5F, FontStyle.Bold) } });
        _customersGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CreditLimit", HeaderText = "الحد الائتماني", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
        _customersGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "الحالة", Width = 70, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } });
        
        _customersGrid.Rows.Clear();
        
        foreach (var customer in customers)
        {
            int rowIndex = _customersGrid.Rows.Add(
                customer.CustomerId,
                customer.CustomerCode,
                customer.CustomerName,
                customer.Phone ?? "",
                customer.Mobile ?? "",
                customer.Email ?? "",
                customer.CurrentBalance.ToString("N2"),
                customer.CreditLimit.ToString("N2"),
                customer.IsActive ? "نشط" : "موقف"
            );
            
            // Color code balance
            if (customer.CurrentBalance > 0)
            {
                _customersGrid.Rows[rowIndex].Cells["Balance"].Style.ForeColor = ColorScheme.Error; // له عندنا
            }
            else if (customer.CurrentBalance < 0)
            {
                _customersGrid.Rows[rowIndex].Cells["Balance"].Style.ForeColor = ColorScheme.Success; // لنا عنده
            }
            
            // Highlight if exceeding credit limit
            if (customer.CreditLimit > 0 && customer.CurrentBalance > customer.CreditLimit)
            {
                _customersGrid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 240, 240);
            }
        }
    }
    
    private async void SearchCustomers()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_searchBox.Text))
            {
                await LoadDataAsync();
                return;
            }
            
            var customers = await _customerService.SearchCustomersAsync(_searchBox.Text);
            
            if (_activeOnlyCheck.Checked)
            {
                customers = customers.Where(c => c.IsActive).ToList();
            }
            
            DisplayCustomers(customers);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في البحث: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void AddCustomer_Click(object? sender, EventArgs e)
    {
        try
        {
            using var form = new AddEditCustomerForm(_customerService, null);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _ = LoadDataAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void EditCustomer_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_customersGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("برجاء اختيار عميل للتعديل", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            int customerId = Convert.ToInt32(_customersGrid.SelectedRows[0].Cells["Id"].Value);
            
            using var form = new AddEditCustomerForm(_customerService, customerId);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _ = LoadDataAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async void DeleteCustomer_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_customersGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("برجاء اختيار عميل للحذف", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            int customerId = Convert.ToInt32(_customersGrid.SelectedRows[0].Cells["Id"].Value);
            string customerName = _customersGrid.SelectedRows[0].Cells["Name"].Value?.ToString() ?? "";
            
            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف العميل: {customerName}؟",
                "تأكيد الحذف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            
            if (result == DialogResult.Yes)
            {
                await _customerService.DeleteCustomerAsync(customerId);
                MessageBox.Show("تم حذف العميل بنجاح", "نجح",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadDataAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في الحذف: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void ViewStatement_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_customersGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("برجاء اختيار عميل لعرض كشف الحساب", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            int customerId = Convert.ToInt32(_customersGrid.SelectedRows[0].Cells["Id"].Value);
            
            using var form = new CustomerStatementForm(_customerService, customerId);
            form.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
