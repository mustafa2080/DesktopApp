using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing.Printing;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class InvoicesListForm : Form
{
    private readonly IInvoiceService _invoiceService;
    private readonly ICashBoxService _cashBoxService;
    private readonly int _currentUserId;
    
    private Panel _headerPanel = null!;
    private TabControl _tabControl = null!;
    private DataGridView _salesGrid = null!;
    private DataGridView _purchaseGrid = null!;
    
    private Button _addSalesButton = null!;
    private Button _addPurchaseButton = null!;
    private Button _viewButton = null!;
    private Button _addPaymentButton = null!;
    private Button _printButton = null!;
    private Button _filterButton = null!;
    private Button _refreshButton = null!;
    private Button _editButton = null!;
    private Button _deleteButton = null!;
    
    public InvoicesListForm(IInvoiceService invoiceService, ICashBoxService cashBoxService, int currentUserId)
    {
        _invoiceService = invoiceService;
        _cashBoxService = cashBoxService;
        _currentUserId = currentUserId;
        
        InitializeComponent();
        InitializeCustomControls();
        _ = LoadDataAsync();
    }
    
    private void InitializeComponent()
    {
        this.Text = "إدارة الفواتير";
        this.Size = new Size(1400, 900);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
    }
    
    private void InitializeCustomControls()
    {
        // Header Panel
        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 80,
            BackColor = Color.White,
            Padding = new Padding(20)
        };
        
        Label titleLabel = new Label
        {
            Text = "📋 إدارة الفواتير",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(20, 25)
        };
        _headerPanel.Controls.Add(titleLabel);
        
        // Main Container
        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            BackColor = ColorScheme.Background
        };
        
        // Tab Control
        _tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Padding = new Point(10, 10)
        };
        
        // Sales Tab
        TabPage salesTab = new TabPage("فواتير المبيعات");
        CreateSalesTab(salesTab);
        _tabControl.TabPages.Add(salesTab);
        
        // Purchase Tab
        TabPage purchaseTab = new TabPage("فواتير المشتريات");
        CreatePurchaseTab(purchaseTab);
        _tabControl.TabPages.Add(purchaseTab);
        
        mainPanel.Controls.Add(_tabControl);
        
        // Add controls in correct order
        this.Controls.Add(mainPanel);
        this.Controls.Add(_headerPanel);
    }
    
    private void CreateSalesTab(TabPage tab)
    {
        Panel container = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ColorScheme.Background,
            Padding = new Padding(15)
        };
        
        // Buttons Panel
        Panel buttonsPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 120,
            BackColor = Color.White,
            Padding = new Padding(10)
        };
        
        _addSalesButton = CreateButton("➕ فاتورة مبيعات جديدة", ColorScheme.Success, new Point(10, 10), AddSalesInvoice_Click);
        buttonsPanel.Controls.Add(_addSalesButton);
        
        _viewButton = CreateButton("👁️ عرض التفاصيل", ColorScheme.Primary, new Point(210, 10), ViewInvoice_Click);
        buttonsPanel.Controls.Add(_viewButton);
        
        _editButton = CreateButton("✏️ تعديل", Color.FromArgb(52, 152, 219), new Point(410, 10), EditInvoice_Click);
        buttonsPanel.Controls.Add(_editButton);
        
        _deleteButton = CreateButton("🗑️ حذف", ColorScheme.Error, new Point(610, 10), DeleteInvoice_Click);
        buttonsPanel.Controls.Add(_deleteButton);
        
        _addPaymentButton = CreateButton("💰 إضافة دفعة", ColorScheme.Warning, new Point(810, 10), AddPayment_Click);
        buttonsPanel.Controls.Add(_addPaymentButton);
        
        _printButton = CreateButton("🖨️ طباعة", Color.FromArgb(41, 128, 185), new Point(1010, 10), PrintInvoice_Click);
        buttonsPanel.Controls.Add(_printButton);
        
        _filterButton = CreateButton("🔍 فلتر", Color.FromArgb(155, 89, 182), new Point(10, 60), FilterInvoices_Click);
        buttonsPanel.Controls.Add(_filterButton);
        
        _refreshButton = CreateButton("🔄 تحديث", Color.FromArgb(52, 73, 94), new Point(210, 60), (s, e) => _ = LoadDataAsync());
        buttonsPanel.Controls.Add(_refreshButton);
        
        // Grid Container
        Panel gridContainer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ColorScheme.Background,
            Padding = new Padding(0, 10, 0, 0)
        };
        
        // DataGridView
        _salesGrid = CreateDataGrid();
        gridContainer.Controls.Add(_salesGrid);
        
        // Add controls in correct order (bottom to top for Dock.Fill to work)
        container.Controls.Add(gridContainer);
        container.Controls.Add(buttonsPanel);
        
        tab.Controls.Add(container);
    }
    
    private void CreatePurchaseTab(TabPage tab)
    {
        Panel container = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ColorScheme.Background,
            Padding = new Padding(15)
        };
        
        // Buttons Panel
        Panel buttonsPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 120,
            BackColor = Color.White,
            Padding = new Padding(10)
        };
        
        _addPurchaseButton = CreateButton("➕ فاتورة مشتريات جديدة", ColorScheme.Error, new Point(10, 10), AddPurchaseInvoice_Click);
        buttonsPanel.Controls.Add(_addPurchaseButton);
        
        Button viewPurchaseButton = CreateButton("👁️ عرض التفاصيل", ColorScheme.Primary, new Point(210, 10), ViewInvoice_Click);
        buttonsPanel.Controls.Add(viewPurchaseButton);
        
        Button editPurchaseButton = CreateButton("✏️ تعديل", Color.FromArgb(52, 152, 219), new Point(410, 10), EditInvoice_Click);
        buttonsPanel.Controls.Add(editPurchaseButton);
        
        Button deletePurchaseButton = CreateButton("🗑️ حذف", ColorScheme.Error, new Point(610, 10), DeleteInvoice_Click);
        buttonsPanel.Controls.Add(deletePurchaseButton);
        
        Button addPurchasePaymentButton = CreateButton("💰 إضافة دفعة", ColorScheme.Warning, new Point(810, 10), AddPayment_Click);
        buttonsPanel.Controls.Add(addPurchasePaymentButton);
        
        Button printPurchaseButton = CreateButton("🖨️ طباعة", Color.FromArgb(41, 128, 185), new Point(1010, 10), PrintInvoice_Click);
        buttonsPanel.Controls.Add(printPurchaseButton);
        
        Button filterPurchaseButton = CreateButton("🔍 فلتر", Color.FromArgb(155, 89, 182), new Point(10, 60), FilterInvoices_Click);
        buttonsPanel.Controls.Add(filterPurchaseButton);
        
        Button refreshPurchaseButton = CreateButton("🔄 تحديث", Color.FromArgb(52, 73, 94), new Point(210, 60), (s, e) => _ = LoadDataAsync());
        buttonsPanel.Controls.Add(refreshPurchaseButton);
        
        // Grid Container
        Panel gridContainer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ColorScheme.Background,
            Padding = new Padding(0, 10, 0, 0)
        };
        
        // DataGridView
        _purchaseGrid = CreateDataGrid();
        gridContainer.Controls.Add(_purchaseGrid);
        
        // Add controls in correct order (bottom to top for Dock.Fill to work)
        container.Controls.Add(gridContainer);
        container.Controls.Add(buttonsPanel);
        
        tab.Controls.Add(container);
    }
    
    private DataGridView CreateDataGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            Font = new Font("Cairo", 10F),
            RightToLeft = RightToLeft.Yes,
            EnableHeadersVisualStyles = false,
            Margin = new Padding(0)
        };
        
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleCenter,
            Padding = new Padding(5),
            WrapMode = DataGridViewTriState.True
        };
        
        grid.ColumnHeadersHeight = 45;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.RowTemplate.Height = 45;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
        grid.DefaultCellStyle.SelectionBackColor = ColorScheme.Primary;
        grid.DefaultCellStyle.SelectionForeColor = Color.White;
        grid.DefaultCellStyle.Padding = new Padding(5);
        grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        
        // Add border around grid
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = Color.FromArgb(220, 220, 220);
        
        return grid;
    }
    
    private Button CreateButton(string text, Color bgColor, Point location, EventHandler clickHandler)
    {
        Button btn = new Button
        {
            Text = text,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Size = new Size(180, 40),
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
            await LoadSalesInvoicesAsync();
            await LoadPurchaseInvoicesAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async Task LoadSalesInvoicesAsync()
    {
        try
        {
            var invoices = await _invoiceService.GetAllSalesInvoicesAsync();
            
            // تهيئة الأعمدة فقط إذا لم تكن موجودة
            if (_salesGrid.Columns.Count == 0)
            {
                _salesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Visible = false });
                _salesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "InvoiceNumber", HeaderText = "رقم الفاتورة", Width = 130 });
                _salesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "التاريخ", Width = 130 });
                _salesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Customer", HeaderText = "العميل", Width = 220 });
                _salesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "الإجمالي", Width = 130 });
                _salesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Paid", HeaderText = "المدفوع", Width = 130 });
                _salesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Remaining", HeaderText = "المتبقي", Width = 130 });
                _salesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "الحالة", Width = 120 });
            }
            
            _salesGrid.Rows.Clear();
            
            Console.WriteLine($"📊 عدد فواتير المبيعات المسترجعة: {invoices?.Count ?? 0}");
            
            if (invoices == null || invoices.Count == 0)
            {
                Console.WriteLine("⚠️ لا توجد فواتير مبيعات");
                // لا نعمل return هنا - نترك الـ Grid فاضي فقط
            }
            else
            {
                foreach (var invoice in invoices)
                {
                    _salesGrid.Rows.Add(
                        invoice.SalesInvoiceId,
                        invoice.InvoiceNumber,
                        invoice.InvoiceDate.ToString("yyyy-MM-dd"),
                        invoice.Customer?.CustomerName ?? "",
                        invoice.TotalAmount.ToString("N2"),
                        invoice.PaidAmount.ToString("N2"),
                        invoice.RemainingAmount.ToString("N2"),
                        GetStatusText(invoice.Status)
                    );
                }
                
                Console.WriteLine($"✅ تم تحميل {invoices.Count} فاتورة مبيعات");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل فواتير المبيعات: {ex.Message}\n\nStack: {ex.StackTrace}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async Task LoadPurchaseInvoicesAsync()
    {
        try
        {
            var invoices = await _invoiceService.GetAllPurchaseInvoicesAsync();
            
            // تهيئة الأعمدة فقط إذا لم تكن موجودة
            if (_purchaseGrid.Columns.Count == 0)
            {
                _purchaseGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Visible = false });
                _purchaseGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "InvoiceNumber", HeaderText = "رقم الفاتورة", Width = 130 });
                _purchaseGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "التاريخ", Width = 130 });
                _purchaseGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Supplier", HeaderText = "المورد", Width = 220 });
                _purchaseGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "الإجمالي", Width = 130 });
                _purchaseGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Paid", HeaderText = "المدفوع", Width = 130 });
                _purchaseGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Remaining", HeaderText = "المتبقي", Width = 130 });
                _purchaseGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "الحالة", Width = 120 });
            }
            
            _purchaseGrid.Rows.Clear();
            
            Console.WriteLine($"📊 عدد فواتير المشتريات المسترجعة: {invoices?.Count ?? 0}");
            
            if (invoices == null || invoices.Count == 0)
            {
                Console.WriteLine("⚠️ لا توجد فواتير مشتريات");
                // لا نعمل return هنا - نترك الـ Grid فاضي فقط
            }
            else
            {
                foreach (var invoice in invoices)
                {
                    _purchaseGrid.Rows.Add(
                        invoice.PurchaseInvoiceId,
                        invoice.InvoiceNumber,
                        invoice.InvoiceDate.ToString("yyyy-MM-dd"),
                        invoice.Supplier?.SupplierName ?? "",
                        invoice.TotalAmount.ToString("N2"),
                        invoice.PaidAmount.ToString("N2"),
                        (invoice.TotalAmount - invoice.PaidAmount).ToString("N2"),
                        GetStatusText(invoice.Status)
                    );
                }
                
                Console.WriteLine($"✅ تم تحميل {invoices.Count} فاتورة مشتريات");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل فواتير المشتريات: {ex.Message}\n\nStack: {ex.StackTrace}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private string GetStatusText(string status)
    {
        return status switch
        {
            "Paid" => "مدفوعة",
            "Partial" => "مدفوعة جزئياً",
            "Unpaid" => "غير مدفوعة",
            _ => status
        };
    }
    
    private void AddSalesInvoice_Click(object? sender, EventArgs e)
    {
        try
        {
            var serviceProvider = Program.ServiceProvider;
            var context = serviceProvider.GetRequiredService<GraceWay.AccountingSystem.Infrastructure.Data.AppDbContext>();
            
            AddSalesInvoiceForm form = new AddSalesInvoiceForm(_invoiceService, _cashBoxService, context, _currentUserId);
            form.FormClosed += (s, args) => _ = LoadDataAsync();
            form.Show(); // ✅ تغيير من ShowDialog إلى Show لفتح نوافذ متعددة
        }
        catch (Exception ex)
        {
            MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void AddPurchaseInvoice_Click(object? sender, EventArgs e)
    {
        try
        {
            var serviceProvider = Program.ServiceProvider;
            var context = serviceProvider.GetRequiredService<GraceWay.AccountingSystem.Infrastructure.Data.AppDbContext>();
            
            AddPurchaseInvoiceForm form = new AddPurchaseInvoiceForm(_invoiceService, _cashBoxService, context, _currentUserId);
            form.FormClosed += (s, args) => _ = LoadDataAsync();
            form.Show(); // ✅ تغيير من ShowDialog إلى Show لفتح نوافذ متعددة
        }
        catch (Exception ex)
        {
            MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    // وظيفة التعديل الجديدة
    private void EditInvoice_Click(object? sender, EventArgs e)
    {
        try
        {
            DataGridView currentGrid = _tabControl.SelectedIndex == 0 ? _salesGrid : _purchaseGrid;
            
            if (currentGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("برجاء اختيار فاتورة أولاً", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var selectedRow = currentGrid.SelectedRows[0];
            int invoiceId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
            string status = selectedRow.Cells["Status"].Value?.ToString() ?? "";
            
            // منع التعديل على الفواتير المدفوعة بالكامل
            if (status == "مدفوعة")
            {
                var result = MessageBox.Show(
                    "هذه الفاتورة مدفوعة بالكامل. هل تريد تعديلها؟\nقد يؤثر ذلك على حسابات النظام.",
                    "تحذير",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                    
                if (result != DialogResult.Yes)
                    return;
            }
            
            var serviceProvider = Program.ServiceProvider;
            var context = serviceProvider.GetRequiredService<GraceWay.AccountingSystem.Infrastructure.Data.AppDbContext>();
            
            if (_tabControl.SelectedIndex == 0) // فاتورة مبيعات
            {
                EditSalesInvoiceForm form = new EditSalesInvoiceForm(_invoiceService, _cashBoxService, context, _currentUserId, invoiceId);
                form.FormClosed += (s, args) => _ = LoadDataAsync();
                form.Show(); // ✅ تغيير من ShowDialog إلى Show لفتح نوافذ متعددة
            }
            else // فاتورة مشتريات
            {
                EditPurchaseInvoiceForm form = new EditPurchaseInvoiceForm(_invoiceService, _cashBoxService, context, _currentUserId, invoiceId);
                form.FormClosed += (s, args) => _ = LoadDataAsync();
                form.Show(); // ✅ تغيير من ShowDialog إلى Show لفتح نوافذ متعددة
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في التعديل: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    // وظيفة الحذف الجديدة
    private async void DeleteInvoice_Click(object? sender, EventArgs e)
    {
        try
        {
            DataGridView currentGrid = _tabControl.SelectedIndex == 0 ? _salesGrid : _purchaseGrid;
            
            if (currentGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("برجاء اختيار فاتورة أولاً", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var selectedRow = currentGrid.SelectedRows[0];
            int invoiceId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
            string invoiceNumber = selectedRow.Cells["InvoiceNumber"].Value?.ToString() ?? "";
            string status = selectedRow.Cells["Status"].Value?.ToString() ?? "";
            decimal paidAmount = Convert.ToDecimal(selectedRow.Cells["Paid"].Value);
            
            // تحذير خاص للفواتير المدفوعة جزئياً أو بالكامل
            string warningMessage = "هل أنت متأكد من حذف هذه الفاتورة؟";
            
            if (status == "مدفوعة" || status == "مدفوعة جزئياً")
            {
                warningMessage = $"تحذير: هذه الفاتورة {status} بمبلغ {paidAmount:N2} جنيه.\n" +
                               $"حذفها سيؤثر على حسابات النظام والخزينة.\n\n" +
                               $"هل أنت متأكد من حذف الفاتورة رقم {invoiceNumber}؟";
            }
            else
            {
                warningMessage = $"هل أنت متأكد من حذف الفاتورة رقم {invoiceNumber}؟";
            }
            
            var result = MessageBox.Show(
                warningMessage,
                "تأكيد الحذف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            
            if (result != DialogResult.Yes)
                return;
            
            // حذف الفاتورة
            bool success = false;
            if (_tabControl.SelectedIndex == 0) // فاتورة مبيعات
            {
                success = await _invoiceService.DeleteSalesInvoiceAsync(invoiceId);
            }
            else // فاتورة مشتريات
            {
                success = await _invoiceService.DeletePurchaseInvoiceAsync(invoiceId);
            }
            
            if (success)
            {
                MessageBox.Show("تم حذف الفاتورة بنجاح", "نجاح",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                _salesGrid.Columns.Clear();
                _purchaseGrid.Columns.Clear();
                await LoadDataAsync();
            }
            else
            {
                MessageBox.Show("فشل حذف الفاتورة. قد تكون مرتبطة ببيانات أخرى في النظام.", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في الحذف:\n{ex.Message}\n\n{ex.InnerException?.Message}", "خطأ - تفاصيل",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void ViewInvoice_Click(object? sender, EventArgs e)
    {
        try
        {
            DataGridView currentGrid = _tabControl.SelectedIndex == 0 ? _salesGrid : _purchaseGrid;
            
            if (currentGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("برجاء اختيار فاتورة أولاً", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var selectedRow = currentGrid.SelectedRows[0];
            int invoiceId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
            
            int? salesInvoiceId = null;
            int? purchaseInvoiceId = null;
            
            if (_tabControl.SelectedIndex == 0) // Sales
            {
                salesInvoiceId = invoiceId;
            }
            else // Purchase
            {
                purchaseInvoiceId = invoiceId;
            }
            
            var detailsForm = new InvoiceDetailsForm(_invoiceService, salesInvoiceId, purchaseInvoiceId);
            detailsForm.FormClosed += (s, args) => detailsForm.Dispose(); // ✅ تنظيف الذاكرة عند الإغلاق
            detailsForm.Show(); // ✅ تغيير من ShowDialog إلى Show لفتح نوافذ متعددة
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async void AddPayment_Click(object? sender, EventArgs e)
    {
        try
        {
            DataGridView currentGrid = _tabControl.SelectedIndex == 0 ? _salesGrid : _purchaseGrid;
            
            if (currentGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("برجاء اختيار فاتورة أولاً", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var selectedRow = currentGrid.SelectedRows[0];
            int invoiceId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
            string invoiceNumber = selectedRow.Cells["InvoiceNumber"].Value?.ToString() ?? "";
            decimal remainingAmount = Convert.ToDecimal(selectedRow.Cells["Remaining"].Value);
            
            if (remainingAmount <= 0)
            {
                MessageBox.Show("هذه الفاتورة مدفوعة بالكامل", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            int? salesInvoiceId = null;
            int? purchaseInvoiceId = null;
            
            if (_tabControl.SelectedIndex == 0) // Sales
            {
                salesInvoiceId = invoiceId;
            }
            else // Purchase
            {
                purchaseInvoiceId = invoiceId;
            }
            
            var paymentForm = new AddPaymentForm(
                _invoiceService,
                _cashBoxService,
                _currentUserId,
                salesInvoiceId,
                purchaseInvoiceId,
                remainingAmount,
                invoiceNumber
            );
            
            paymentForm.FormClosed += async (s, args) =>
            {
                paymentForm.Dispose();
                // إعادة تهيئة الأعمدة لضمان تحديث البيانات من الـ DB
                _salesGrid.Columns.Clear();
                _purchaseGrid.Columns.Clear();
                await LoadDataAsync();
            };
            
            paymentForm.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async void PrintInvoice_Click(object? sender, EventArgs e)
    {
        try
        {
            DataGridView currentGrid = _tabControl.SelectedIndex == 0 ? _salesGrid : _purchaseGrid;
            
            if (currentGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("برجاء اختيار فاتورة أولاً", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var selectedRow = currentGrid.SelectedRows[0];
            int invoiceId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
            
            // تجهيز بيانات الطباعة
            string content = await PrepareInvoicePrintContentAsync(invoiceId, _tabControl.SelectedIndex == 0);
            
            if (string.IsNullOrEmpty(content))
            {
                MessageBox.Show("لا توجد بيانات كافية للطباعة", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // طباعة الفاتورة
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += (s, ev) => PrintInvoicePage(ev, content);
            
            PrintPreviewDialog previewDialog = new PrintPreviewDialog
            {
                Document = printDoc,
                Width = 800,
                Height = 600,
                StartPosition = FormStartPosition.CenterParent
            };
            
            previewDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في الطباعة: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async Task<string> PrepareInvoicePrintContentAsync(int invoiceId, bool isSales)
    {
        try
        {
            if (isSales)
            {
                var invoice = await _invoiceService.GetSalesInvoiceByIdAsync(invoiceId);
                if (invoice == null) return string.Empty;
                
                var payments = await _invoiceService.GetInvoicePaymentsAsync(invoiceId, null);
                
                var content = new System.Text.StringBuilder();
                content.AppendLine("========================================");
                content.AppendLine("            فاتورة مبيعات");
                content.AppendLine("========================================");
                content.AppendLine($"رقم الفاتورة: {invoice.InvoiceNumber}");
                content.AppendLine($"التاريخ: {invoice.InvoiceDate:yyyy-MM-dd}");
                content.AppendLine($"العميل: {invoice.Customer?.CustomerName ?? "غير محدد"}");
                content.AppendLine("----------------------------------------");
                content.AppendLine("الأصناف:");
                content.AppendLine("----------------------------------------");
                
                decimal itemsTotal = 0;
                foreach (var item in invoice.Items)
                {
                    decimal lineTotal = item.Quantity * item.UnitPrice;
                    itemsTotal += lineTotal;
                    content.AppendLine($"{item.Description}");
                    content.AppendLine($"  الكمية: {item.Quantity} × {item.UnitPrice:N2} = {lineTotal:N2}");
                }
                
                content.AppendLine("----------------------------------------");
                content.AppendLine($"الإجمالي الفرعي: {invoice.SubTotal:N2} جنيه");
                content.AppendLine($"الضريبة: {invoice.TaxAmount:N2} جنيه");
                content.AppendLine($"الإجمالي النهائي: {invoice.TotalAmount:N2} جنيه");
                content.AppendLine("========================================");
                content.AppendLine($"المدفوع: {invoice.PaidAmount:N2} جنيه");
                content.AppendLine($"المتبقي: {invoice.RemainingAmount:N2} جنيه");
                content.AppendLine($"الحالة: {GetStatusText(invoice.Status)}");
                
                if (payments.Any())
                {
                    content.AppendLine("----------------------------------------");
                    content.AppendLine("سجل الدفعات:");
                    content.AppendLine("----------------------------------------");
                    foreach (var payment in payments)
                    {
                        content.AppendLine($"{payment.PaymentDate:yyyy-MM-dd} - {payment.Amount:N2} جنيه - {payment.PaymentMethod}");
                    }
                }
                
                content.AppendLine("========================================");
                
                if (!string.IsNullOrEmpty(invoice.Notes))
                {
                    content.AppendLine($"ملاحظات: {invoice.Notes}");
                }
                
                return content.ToString();
            }
            else
            {
                var invoice = await _invoiceService.GetPurchaseInvoiceByIdAsync(invoiceId);
                if (invoice == null) return string.Empty;
                
                var payments = await _invoiceService.GetInvoicePaymentsAsync(null, invoiceId);
                
                var content = new System.Text.StringBuilder();
                content.AppendLine("========================================");
                content.AppendLine("           فاتورة مشتريات");
                content.AppendLine("========================================");
                content.AppendLine($"رقم الفاتورة: {invoice.InvoiceNumber}");
                content.AppendLine($"التاريخ: {invoice.InvoiceDate:yyyy-MM-dd}");
                content.AppendLine($"المورد: {invoice.Supplier?.SupplierName ?? "غير محدد"}");
                content.AppendLine("----------------------------------------");
                content.AppendLine("الأصناف:");
                content.AppendLine("----------------------------------------");
                
                decimal itemsTotal = 0;
                foreach (var item in invoice.Items)
                {
                    decimal lineTotal = item.Quantity * item.UnitPrice;
                    itemsTotal += lineTotal;
                    content.AppendLine($"{item.Description}");
                    content.AppendLine($"  الكمية: {item.Quantity} × {item.UnitPrice:N2} = {lineTotal:N2}");
                }
                
                content.AppendLine("----------------------------------------");
                content.AppendLine($"الإجمالي الفرعي: {invoice.SubTotal:N2} جنيه");
                content.AppendLine($"الضريبة: {invoice.TaxAmount:N2} جنيه");
                content.AppendLine($"الإجمالي النهائي: {invoice.TotalAmount:N2} جنيه");
                content.AppendLine("========================================");
                content.AppendLine($"المدفوع: {invoice.PaidAmount:N2} جنيه");
                content.AppendLine($"المتبقي: {(invoice.TotalAmount - invoice.PaidAmount):N2} جنيه");
                content.AppendLine($"الحالة: {GetStatusText(invoice.Status)}");
                
                if (payments.Any())
                {
                    content.AppendLine("----------------------------------------");
                    content.AppendLine("سجل الدفعات:");
                    content.AppendLine("----------------------------------------");
                    foreach (var payment in payments)
                    {
                        content.AppendLine($"{payment.PaymentDate:yyyy-MM-dd} - {payment.Amount:N2} جنيه - {payment.PaymentMethod}");
                    }
                }
                
                content.AppendLine("========================================");
                
                if (!string.IsNullOrEmpty(invoice.Notes))
                {
                    content.AppendLine($"ملاحظات: {invoice.Notes}");
                }
                
                return content.ToString();
            }
        }
        catch
        {
            return string.Empty;
        }
    }
    
    private void PrintInvoicePage(PrintPageEventArgs e, string content)
    {
        Font printFont = new Font("Cairo", 10);
        Font titleFont = new Font("Cairo", 14, FontStyle.Bold);
        
        float yPos = 50;
        
        StringFormat rightAlign = new StringFormat
        {
            Alignment = StringAlignment.Far,
            LineAlignment = StringAlignment.Near
        };
        
        var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        
        foreach (string line in lines)
        {
            if (yPos + printFont.GetHeight(e.Graphics!) > e.MarginBounds.Bottom)
            {
                e.HasMorePages = true;
                return;
            }
            
            Font currentFont = line.Contains("====") || line.Contains("فاتورة") ? titleFont : printFont;
            
            if (e.Graphics != null)
            {
                e.Graphics.DrawString(line, currentFont, Brushes.Black,
                    e.MarginBounds.Right, yPos, rightAlign);
                
                yPos += currentFont.GetHeight(e.Graphics);
            }
        }
        
        e.HasMorePages = false;
    }
    
    private void FilterInvoices_Click(object? sender, EventArgs e)
    {
        try
        {
            var filterForm = new InvoiceFilterForm();
            filterForm.FormClosed += (s, args) => filterForm.Dispose(); // ✅ تنظيف الذاكرة عند الإغلاق
            filterForm.Show(); // ✅ تغيير من ShowDialog إلى Show لفتح نوافذ متعددة
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async Task ApplyFilterAsync(DateTime? startDate, DateTime? endDate, string? status,
        decimal? minAmount, decimal? maxAmount, string? searchText)
    {
        try
        {
            if (_tabControl.SelectedIndex == 0) // Sales
            {
                var invoices = await _invoiceService.GetAllSalesInvoicesAsync();
                
                // Apply filters
                if (startDate.HasValue)
                    invoices = invoices.Where(i => i.InvoiceDate >= startDate.Value).ToList();
                
                if (endDate.HasValue)
                    invoices = invoices.Where(i => i.InvoiceDate <= endDate.Value).ToList();
                
                if (!string.IsNullOrEmpty(status))
                    invoices = invoices.Where(i => i.Status == status).ToList();
                
                if (minAmount.HasValue)
                    invoices = invoices.Where(i => i.TotalAmount >= minAmount.Value).ToList();
                
                if (maxAmount.HasValue)
                    invoices = invoices.Where(i => i.TotalAmount <= maxAmount.Value).ToList();
                
                if (!string.IsNullOrEmpty(searchText))
                {
                    searchText = searchText.ToLower();
                    invoices = invoices.Where(i =>
                        i.InvoiceNumber.ToLower().Contains(searchText) ||
                        (i.Customer?.CustomerName?.ToLower().Contains(searchText) ?? false)
                    ).ToList();
                }
                
                // Display filtered results
                _salesGrid.Rows.Clear();
                foreach (var invoice in invoices)
                {
                    _salesGrid.Rows.Add(
                        invoice.SalesInvoiceId,
                        invoice.InvoiceNumber,
                        invoice.InvoiceDate.ToString("yyyy-MM-dd"),
                        invoice.Customer?.CustomerName ?? "",
                        invoice.TotalAmount.ToString("N2"),
                        invoice.PaidAmount.ToString("N2"),
                        invoice.RemainingAmount.ToString("N2"),
                        GetStatusText(invoice.Status)
                    );
                }
            }
            else // Purchase
            {
                var invoices = await _invoiceService.GetAllPurchaseInvoicesAsync();
                
                // Apply filters
                if (startDate.HasValue)
                    invoices = invoices.Where(i => i.InvoiceDate >= startDate.Value).ToList();
                
                if (endDate.HasValue)
                    invoices = invoices.Where(i => i.InvoiceDate <= endDate.Value).ToList();
                
                if (!string.IsNullOrEmpty(status))
                    invoices = invoices.Where(i => i.Status == status).ToList();
                
                if (minAmount.HasValue)
                    invoices = invoices.Where(i => i.TotalAmount >= minAmount.Value).ToList();
                
                if (maxAmount.HasValue)
                    invoices = invoices.Where(i => i.TotalAmount <= maxAmount.Value).ToList();
                
                if (!string.IsNullOrEmpty(searchText))
                {
                    searchText = searchText.ToLower();
                    invoices = invoices.Where(i =>
                        i.InvoiceNumber.ToLower().Contains(searchText) ||
                        (i.Supplier?.SupplierName?.ToLower().Contains(searchText) ?? false)
                    ).ToList();
                }
                
                // Display filtered results
                _purchaseGrid.Rows.Clear();
                foreach (var invoice in invoices)
                {
                    _purchaseGrid.Rows.Add(
                        invoice.PurchaseInvoiceId,
                        invoice.InvoiceNumber,
                        invoice.InvoiceDate.ToString("yyyy-MM-dd"),
                        invoice.Supplier?.SupplierName ?? "",
                        invoice.TotalAmount.ToString("N2"),
                        invoice.PaidAmount.ToString("N2"),
                        (invoice.TotalAmount - invoice.PaidAmount).ToString("N2"),
                        GetStatusText(invoice.Status)
                    );
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تطبيق الفلتر: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
