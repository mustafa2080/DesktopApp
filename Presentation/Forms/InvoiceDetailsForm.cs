using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Application.Services;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class InvoiceDetailsForm : Form
{
    private readonly IInvoiceService _invoiceService;
    private readonly int? _salesInvoiceId;
    private readonly int? _purchaseInvoiceId;
    
    private Panel _headerPanel = null!;
    private Panel _detailsPanel = null!;
    private DataGridView _itemsGrid = null!;
    private DataGridView _paymentsGrid = null!;
    private Button _closeButton = null!;
    private Button _printButton = null!;
    
    public InvoiceDetailsForm(IInvoiceService invoiceService, int? salesInvoiceId, int? purchaseInvoiceId)
    {
        _invoiceService = invoiceService;
        _salesInvoiceId = salesInvoiceId;
        _purchaseInvoiceId = purchaseInvoiceId;
        
        InitializeComponent();
        InitializeCustomControls();
        _ = LoadDataAsync();
    }
    
    private void InitializeComponent()
    {
        this.Text = "تفاصيل الفاتورة";
        this.Size = new Size(1200, 900);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
    }
    
    private void InitializeCustomControls()
    {
        // Main Container
        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(30),
            AutoScroll = true
        };
        
        // Header Panel
        _headerPanel = new Panel
        {
            Location = new Point(30, 20),
            Size = new Size(1120, 200),
            BackColor = Color.FromArgb(240, 248, 255),
            Padding = new Padding(20)
        };
        mainPanel.Controls.Add(_headerPanel);
        
        // Details Panel
        _detailsPanel = new Panel
        {
            Location = new Point(30, 240),
            Size = new Size(1120, 150),
            BackColor = Color.FromArgb(245, 245, 245),
            Padding = new Padding(20)
        };
        mainPanel.Controls.Add(_detailsPanel);
        
        // Items Section
        Label itemsLabel = new Label
        {
            Text = "📦 أصناف الفاتورة",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(30, 410)
        };
        mainPanel.Controls.Add(itemsLabel);
        
        _itemsGrid = CreateDataGrid();
        _itemsGrid.Location = new Point(30, 450);
        _itemsGrid.Size = new Size(1120, 250);
        mainPanel.Controls.Add(_itemsGrid);
        
        // Payments Section
        Label paymentsLabel = new Label
        {
            Text = "💰 الدفعات المسجلة",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = ColorScheme.Success,
            AutoSize = true,
            Location = new Point(30, 720)
        };
        mainPanel.Controls.Add(paymentsLabel);
        
        _paymentsGrid = CreateDataGrid();
        _paymentsGrid.Location = new Point(30, 760);
        _paymentsGrid.Size = new Size(1120, 200);
        mainPanel.Controls.Add(_paymentsGrid);
        
        // Action Buttons
        _printButton = CreateButton("🖨️ طباعة", ColorScheme.Primary, new Point(30, 980), PrintInvoice_Click);
        mainPanel.Controls.Add(_printButton);
        
        _closeButton = CreateButton("❌ إغلاق", ColorScheme.Error, new Point(200, 980), (s, e) => this.Close());
        mainPanel.Controls.Add(_closeButton);
        
        this.Controls.Add(mainPanel);
    }
    
    private DataGridView CreateDataGrid()
    {
        var grid = new DataGridView
        {
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
            EnableHeadersVisualStyles = false
        };
        
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleCenter,
            Padding = new Padding(8)
        };
        
        grid.ColumnHeadersHeight = 55;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.RowTemplate.Height = 40;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
        
        return grid;
    }
    
    private Button CreateButton(string text, Color bgColor, Point location, EventHandler clickHandler)
    {
        Button btn = new Button
        {
            Text = text,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            Size = new Size(160, 45),
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
            if (_salesInvoiceId.HasValue)
            {
                await LoadSalesInvoiceAsync();
            }
            else if (_purchaseInvoiceId.HasValue)
            {
                await LoadPurchaseInvoiceAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async Task LoadSalesInvoiceAsync()
    {
        var invoice = await _invoiceService.GetSalesInvoiceByIdAsync(_salesInvoiceId!.Value);
        
        if (invoice == null)
        {
            MessageBox.Show("الفاتورة غير موجودة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
            return;
        }
        
        // Header Information
        _headerPanel.Controls.Clear();
        
        Label titleLabel = new Label
        {
            Text = "📄 فاتورة مبيعات",
            Font = new Font("Cairo", 18F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(20, 15)
        };
        _headerPanel.Controls.Add(titleLabel);
        
        Label invoiceNumberLabel = new Label
        {
            Text = $"رقم الفاتورة: {invoice.InvoiceNumber}",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 60)
        };
        _headerPanel.Controls.Add(invoiceNumberLabel);
        
        Label dateLabel = new Label
        {
            Text = $"التاريخ: {invoice.InvoiceDate:yyyy-MM-dd}",
            Font = new Font("Cairo", 11F),
            AutoSize = true,
            Location = new Point(20, 90)
        };
        _headerPanel.Controls.Add(dateLabel);
        
        Label customerLabel = new Label
        {
            Text = $"العميل: {invoice.Customer?.CustomerName ?? "غير محدد"}",
            Font = new Font("Cairo", 11F),
            AutoSize = true,
            Location = new Point(20, 120)
        };
        _headerPanel.Controls.Add(customerLabel);
        
        Label statusLabel = new Label
        {
            Text = $"الحالة: {GetStatusText(invoice.Status)}",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = GetStatusColor(invoice.Status),
            AutoSize = true,
            Location = new Point(20, 150)
        };
        _headerPanel.Controls.Add(statusLabel);
        
        // Details Panel
        _detailsPanel.Controls.Clear();
        
        Label subTotalLabel = CreateDetailLabel($"الإجمالي الفرعي: {invoice.SubTotal:N2} جنيه", new Point(20, 20));
        _detailsPanel.Controls.Add(subTotalLabel);
        
        Label taxLabel = CreateDetailLabel($"الضريبة ({invoice.TaxRate}%): {invoice.TaxAmount:N2} جنيه", new Point(20, 50));
        _detailsPanel.Controls.Add(taxLabel);
        
        Label totalLabel = new Label
        {
            Text = $"الإجمالي الكلي: {invoice.TotalAmount:N2} جنيه",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(20, 85)
        };
        _detailsPanel.Controls.Add(totalLabel);
        
        Label paidLabel = CreateDetailLabel($"المدفوع: {invoice.PaidAmount:N2} جنيه", new Point(400, 20), ColorScheme.Success);
        _detailsPanel.Controls.Add(paidLabel);
        
        Label remainingLabel = CreateDetailLabel($"المتبقي: {invoice.RemainingAmount:N2} جنيه", new Point(400, 50), ColorScheme.Error);
        _detailsPanel.Controls.Add(remainingLabel);
        
        // Items Grid
        ConfigureItemsGrid();
        LoadInvoiceItems(invoice.Items);
        
        // Payments Grid
        ConfigurePaymentsGrid();
        await LoadPaymentsAsync(invoice.SalesInvoiceId, null);
    }
    
    private async Task LoadPurchaseInvoiceAsync()
    {
        var invoice = await _invoiceService.GetPurchaseInvoiceByIdAsync(_purchaseInvoiceId!.Value);
        
        if (invoice == null)
        {
            MessageBox.Show("الفاتورة غير موجودة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
            return;
        }
        
        // Header Information
        _headerPanel.Controls.Clear();
        
        Label titleLabel = new Label
        {
            Text = "📄 فاتورة مشتريات",
            Font = new Font("Cairo", 18F, FontStyle.Bold),
            ForeColor = ColorScheme.Error,
            AutoSize = true,
            Location = new Point(20, 15)
        };
        _headerPanel.Controls.Add(titleLabel);
        
        Label invoiceNumberLabel = new Label
        {
            Text = $"رقم الفاتورة: {invoice.InvoiceNumber}",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 60)
        };
        _headerPanel.Controls.Add(invoiceNumberLabel);
        
        Label dateLabel = new Label
        {
            Text = $"التاريخ: {invoice.InvoiceDate:yyyy-MM-dd}",
            Font = new Font("Cairo", 11F),
            AutoSize = true,
            Location = new Point(20, 90)
        };
        _headerPanel.Controls.Add(dateLabel);
        
        Label supplierLabel = new Label
        {
            Text = $"المورد: {invoice.Supplier?.SupplierName ?? "غير محدد"}",
            Font = new Font("Cairo", 11F),
            AutoSize = true,
            Location = new Point(20, 120)
        };
        _headerPanel.Controls.Add(supplierLabel);
        
        Label statusLabel = new Label
        {
            Text = $"الحالة: {GetStatusText(invoice.Status)}",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = GetStatusColor(invoice.Status),
            AutoSize = true,
            Location = new Point(20, 150)
        };
        _headerPanel.Controls.Add(statusLabel);
        
        // Details Panel
        _detailsPanel.Controls.Clear();
        
        Label subTotalLabel = CreateDetailLabel($"الإجمالي الفرعي: {invoice.SubTotal:N2} جنيه", new Point(20, 20));
        _detailsPanel.Controls.Add(subTotalLabel);
        
        Label taxLabel = CreateDetailLabel($"الضريبة ({invoice.TaxRate}%): {invoice.TaxAmount:N2} جنيه", new Point(20, 50));
        _detailsPanel.Controls.Add(taxLabel);
        
        Label totalLabel = new Label
        {
            Text = $"الإجمالي الكلي: {invoice.TotalAmount:N2} جنيه",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(20, 85)
        };
        _detailsPanel.Controls.Add(totalLabel);
        
        decimal paidAmount = invoice.PaidAmount;
        decimal remainingAmount = invoice.TotalAmount - paidAmount;
        
        Label paidLabel = CreateDetailLabel($"المدفوع: {paidAmount:N2} جنيه", new Point(400, 20), ColorScheme.Success);
        _detailsPanel.Controls.Add(paidLabel);
        
        Label remainingLabel = CreateDetailLabel($"المتبقي: {remainingAmount:N2} جنيه", new Point(400, 50), ColorScheme.Error);
        _detailsPanel.Controls.Add(remainingLabel);
        
        // Items Grid
        ConfigureItemsGrid();
        LoadPurchaseInvoiceItems(invoice.Items);
        
        // Payments Grid
        ConfigurePaymentsGrid();
        await LoadPaymentsAsync(null, invoice.PurchaseInvoiceId);
    }
    
    private void ConfigureItemsGrid()
    {
        _itemsGrid.Columns.Clear();
        
        _itemsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Description",
            HeaderText = "الوصف",
            Width = 400
        });
        
        _itemsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Quantity",
            HeaderText = "الكمية",
            Width = 150
        });
        
        _itemsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "UnitPrice",
            HeaderText = "سعر الوحدة",
            Width = 200
        });
        
        _itemsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Total",
            HeaderText = "الإجمالي",
            Width = 200
        });
    }
    
    private void LoadInvoiceItems(ICollection<SalesInvoiceItem> items)
    {
        _itemsGrid.Rows.Clear();
        foreach (var item in items)
        {
            _itemsGrid.Rows.Add(
                item.Description,
                item.Quantity.ToString("N2"),
                item.UnitPrice.ToString("N2"),
                item.TotalPrice.ToString("N2")
            );
        }
    }
    
    private void LoadPurchaseInvoiceItems(ICollection<PurchaseInvoiceItem> items)
    {
        _itemsGrid.Rows.Clear();
        foreach (var item in items)
        {
            _itemsGrid.Rows.Add(
                item.Description,
                item.Quantity.ToString("N2"),
                item.UnitPrice.ToString("N2"),
                item.TotalPrice.ToString("N2")
            );
        }
    }
    
    private void ConfigurePaymentsGrid()
    {
        _paymentsGrid.Columns.Clear();
        
        _paymentsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Date",
            HeaderText = "التاريخ",
            Width = 150
        });
        
        _paymentsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Amount",
            HeaderText = "المبلغ",
            Width = 150
        });
        
        _paymentsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Method",
            HeaderText = "طريقة الدفع",
            Width = 150
        });
        
        _paymentsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Reference",
            HeaderText = "رقم المرجع",
            Width = 150
        });
        
        _paymentsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "CashBox",
            HeaderText = "الخزنة",
            Width = 200
        });
        
        _paymentsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Notes",
            HeaderText = "ملاحظات",
            Width = 250
        });
    }
    
    private async Task LoadPaymentsAsync(int? salesInvoiceId, int? purchaseInvoiceId)
    {
        var payments = await _invoiceService.GetInvoicePaymentsAsync(salesInvoiceId, purchaseInvoiceId);
        
        _paymentsGrid.Rows.Clear();
        foreach (var payment in payments)
        {
            _paymentsGrid.Rows.Add(
                payment.PaymentDate.ToString("yyyy-MM-dd"),
                payment.Amount.ToString("N2"),
                GetPaymentMethodText(payment.PaymentMethod),
                payment.ReferenceNumber ?? "",
                payment.CashBox?.Name ?? "",
                payment.Notes ?? ""
            );
        }
    }
    
    private Label CreateDetailLabel(string text, Point location, Color? color = null)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Cairo", 11F),
            ForeColor = color ?? Color.Black,
            AutoSize = true,
            Location = location
        };
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
    
    private Color GetStatusColor(string status)
    {
        return status switch
        {
            "Paid" => ColorScheme.Success,
            "Partial" => ColorScheme.Warning,
            "Unpaid" => ColorScheme.Error,
            _ => Color.Black
        };
    }
    
    private string GetPaymentMethodText(string method)
    {
        return method switch
        {
            "Cash" => "نقدي",
            "Check" => "شيك",
            "Bank" => "تحويل بنكي",
            "Card" => "بطاقة",
            _ => method
        };
    }
    
    private void PrintInvoice_Click(object? sender, EventArgs e)
    {
        MessageBox.Show("سيتم إضافة وظيفة الطباعة قريباً", "قريباً",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
