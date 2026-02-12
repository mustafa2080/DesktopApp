// نفس الكود تماماً مثل AddSalesInvoiceForm لكن مع تغييرات بسيطة:
// - تغيير Customer إلى Supplier
// - تغيير SalesInvoice إلى PurchaseInvoice
// - تغيير الألوان والعنوان

// ملاحظة: هذا الملف سيكون مشابه جداً لـ AddSalesInvoiceForm
// يمكن دمجهما في form واحد مع parameter للنوع لاحقاً

using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Application.Services;
using Microsoft.EntityFrameworkCore;
using GraceWay.AccountingSystem.Infrastructure.Data;

namespace GraceWay.AccountingSystem.Presentation.Forms;

// DTO for loading suppliers from database
public class SupplierDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
}

public partial class AddPurchaseInvoiceForm : Form
{
    private readonly IInvoiceService _invoiceService;
    private readonly ICashBoxService _cashBoxService;
    private readonly AppDbContext _context;
    private readonly int _currentUserId;
    
    // Edit Mode
    private bool _isEditMode = false;
    private int? _editInvoiceId = null;
    private PurchaseInvoice? _originalInvoice = null;
    
    private ComboBox _supplierCombo = null!;
    private ComboBox _cashBoxCombo = null!;
    private DateTimePicker _datePicker = null!;
    private TextBox _taxRateText = null!;
    private TextBox _notesText = null!;
    
    private DataGridView _itemsGrid = null!;
    private Button _addItemButton = null!;
    private Button _removeItemButton = null!;
    
    private Label _subTotalLabel = null!;
    private Label _taxLabel = null!;
    private Label _totalLabel = null!;
    
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    
    private List<PurchaseInvoiceItem> _items = new();
    
    // Constructor for adding new invoice
    public AddPurchaseInvoiceForm(IInvoiceService invoiceService, ICashBoxService cashBoxService, 
        AppDbContext context, int currentUserId)
    {
        _invoiceService = invoiceService;
        _cashBoxService = cashBoxService;
        _context = context;
        _currentUserId = currentUserId;
        _isEditMode = false;
        
        InitializeComponent();
        InitializeCustomControls();
        _ = LoadDataAsync();
    }
    
    // Constructor for editing existing invoice
    public AddPurchaseInvoiceForm(IInvoiceService invoiceService, ICashBoxService cashBoxService, 
        AppDbContext context, int currentUserId, int invoiceId)
    {
        _invoiceService = invoiceService;
        _cashBoxService = cashBoxService;
        _context = context;
        _currentUserId = currentUserId;
        _isEditMode = true;
        _editInvoiceId = invoiceId;
        
        InitializeComponent();
        InitializeCustomControls();
        _ = LoadDataAsync();
        _ = LoadInvoiceForEditAsync(invoiceId);
    }
    
    private void InitializeComponent()
    {
        this.Text = _isEditMode ? "تعديل فاتورة مشتريات" : "إضافة فاتورة مشتريات";
        this.Size = new Size(1200, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
    }
    
    private void InitializeCustomControls()
    {
        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(30)
        };
        
        Label titleLabel = new Label
        {
            Text = "📄 فاتورة مشتريات جديدة",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = ColorScheme.Error, // أحمر للمشتريات
            AutoSize = true,
            Location = new Point(30, 20)
        };
        mainPanel.Controls.Add(titleLabel);
        
        int yPos = 70;
        
        // Supplier
        Label supplierLabel = CreateLabel("المورد:", new Point(30, yPos));
        mainPanel.Controls.Add(supplierLabel);
        
        _supplierCombo = new ComboBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(300, 30),
            Location = new Point(150, yPos),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        mainPanel.Controls.Add(_supplierCombo);
        
        // Date
        Label dateLabel = CreateLabel("التاريخ:", new Point(500, yPos));
        mainPanel.Controls.Add(dateLabel);
        
        _datePicker = new DateTimePicker
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(200, 30),
            Location = new Point(620, yPos),
            Format = DateTimePickerFormat.Short
        };
        mainPanel.Controls.Add(_datePicker);
        
        yPos += 50;
        
        // CashBox
        Label cashBoxLabel = CreateLabel("الخزنة:", new Point(30, yPos));
        mainPanel.Controls.Add(cashBoxLabel);
        
        _cashBoxCombo = new ComboBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(300, 30),
            Location = new Point(150, yPos),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        mainPanel.Controls.Add(_cashBoxCombo);
        
        // Tax Rate
        Label taxLabel = CreateLabel("نسبة الضريبة %:", new Point(500, yPos));
        mainPanel.Controls.Add(taxLabel);
        
        _taxRateText = new TextBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(100, 30),
            Location = new Point(670, yPos),
            Text = "0"
        };
        _taxRateText.TextChanged += CalculateTotals;
        mainPanel.Controls.Add(_taxRateText);
        
        yPos += 60;
        
        // Items Section
        Label itemsLabel = new Label
        {
            Text = "📦 الأصناف",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            ForeColor = ColorScheme.Error,
            AutoSize = true,
            Location = new Point(30, yPos)
        };
        mainPanel.Controls.Add(itemsLabel);
        
        yPos += 40;
        
        // Items Grid
        _itemsGrid = new DataGridView
        {
            Location = new Point(30, yPos),
            Size = new Size(1100, 250),
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            Font = new Font("Cairo", 10F),
            RightToLeft = RightToLeft.Yes,
            EnableHeadersVisualStyles = false
        };
        
        ConfigureItemsGrid();
        mainPanel.Controls.Add(_itemsGrid);
        
        yPos += 260;
        
        // Item Buttons
        _addItemButton = CreateButton("➕ إضافة صنف", ColorScheme.Success, new Point(30, yPos), AddItem_Click);
        mainPanel.Controls.Add(_addItemButton);
        
        _removeItemButton = CreateButton("➖ حذف صنف", ColorScheme.Error, new Point(200, yPos), RemoveItem_Click);
        mainPanel.Controls.Add(_removeItemButton);
        
        yPos += 60;
        
        // Totals Panel
        Panel totalsPanel = new Panel
        {
            Location = new Point(750, yPos),
            Size = new Size(380, 150),
            BackColor = Color.FromArgb(245, 245, 245),
            Padding = new Padding(20)
        };
        
        _subTotalLabel = CreateTotalLabel("الإجمالي الفرعي:", "0.00 جنيه", new Point(20, 20));
        totalsPanel.Controls.Add(_subTotalLabel);
        
        _taxLabel = CreateTotalLabel("الضريبة:", "0.00 جنيه", new Point(20, 60));
        totalsPanel.Controls.Add(_taxLabel);
        
        Panel separatorLine = new Panel
        {
            Location = new Point(20, 95),
            Size = new Size(340, 2),
            BackColor = ColorScheme.Error
        };
        totalsPanel.Controls.Add(separatorLine);
        
        _totalLabel = new Label
        {
            Text = "الإجمالي الكلي: 0.00 جنيه",
            Font = new Font("Cairo", 14F, FontStyle.Bold),
            ForeColor = ColorScheme.Error,
            AutoSize = true,
            Location = new Point(20, 105)
        };
        totalsPanel.Controls.Add(_totalLabel);
        
        mainPanel.Controls.Add(totalsPanel);
        
        // Notes
        Label notesLabel = CreateLabel("ملاحظات:", new Point(30, yPos));
        mainPanel.Controls.Add(notesLabel);
        
        _notesText = new TextBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(600, 100),
            Location = new Point(30, yPos + 30),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical
        };
        mainPanel.Controls.Add(_notesText);
        
        yPos += 160;
        
        // Action Buttons
        _saveButton = CreateButton("💾 حفظ الفاتورة", ColorScheme.Success, new Point(30, yPos), SaveInvoice_Click);
        mainPanel.Controls.Add(_saveButton);
        
        _cancelButton = CreateButton("❌ إلغاء", ColorScheme.Error, new Point(200, yPos), (s, e) => this.Close());
        mainPanel.Controls.Add(_cancelButton);
        
        this.Controls.Add(mainPanel);
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
        
        _itemsGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ColorScheme.Error,
            ForeColor = Color.White,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleCenter,
            Padding = new Padding(8)
        };
        
        _itemsGrid.ColumnHeadersHeight = 55;
        _itemsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _itemsGrid.EnableHeadersVisualStyles = false;
        _itemsGrid.RowTemplate.Height = 40;
    }
    
    private Label CreateLabel(string text, Point location)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = location
        };
    }
    
    private Label CreateTotalLabel(string label, string value, Point location)
    {
        return new Label
        {
            Text = $"{label} {value}",
            Font = new Font("Cairo", 11F),
            AutoSize = true,
            Location = location
        };
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
            // Load Suppliers using Raw SQL to avoid EF Core query translation issues
            var supplierQuery = @"
                SELECT supplierid as SupplierId, suppliername as SupplierName 
                FROM suppliers 
                WHERE isactive = true 
                ORDER BY suppliername";
            
            var suppliers = await _context.Database
                .SqlQueryRaw<SupplierDto>(supplierQuery)
                .ToListAsync();
            
            _supplierCombo.DisplayMember = "SupplierName";
            _supplierCombo.ValueMember = "SupplierId";
            _supplierCombo.DataSource = suppliers;
            
            // Load CashBoxes
            var cashBoxes = await _cashBoxService.GetActiveCashBoxesAsync();
            _cashBoxCombo.DisplayMember = "Name";
            _cashBoxCombo.ValueMember = "Id";
            _cashBoxCombo.DataSource = cashBoxes;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void AddItem_Click(object? sender, EventArgs e)
    {
        using (var dialog = new AddInvoiceItemDialog())
        {
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var item = new PurchaseInvoiceItem
                {
                    Description = dialog.ItemDescription,
                    Quantity = dialog.ItemQuantity,
                    UnitPrice = dialog.ItemUnitPrice
                };
                
                _items.Add(item);
                RefreshItemsGrid();
                CalculateTotals(null, EventArgs.Empty);
            }
        }
    }
    
    private void RemoveItem_Click(object? sender, EventArgs e)
    {
        if (_itemsGrid.SelectedRows.Count > 0)
        {
            int index = _itemsGrid.SelectedRows[0].Index;
            _items.RemoveAt(index);
            RefreshItemsGrid();
            CalculateTotals(null, EventArgs.Empty);
        }
    }
    
    private void RefreshItemsGrid()
    {
        _itemsGrid.Rows.Clear();
        foreach (var item in _items)
        {
            _itemsGrid.Rows.Add(
                item.Description,
                item.Quantity.ToString("N2"),
                item.UnitPrice.ToString("N2"),
                item.TotalPrice.ToString("N2")
            );
        }
    }
    
    private void CalculateTotals(object? sender, EventArgs e)
    {
        decimal subTotal = _items.Sum(i => i.TotalPrice);
        decimal taxRate = 0;
        
        if (decimal.TryParse(_taxRateText.Text, out decimal rate))
        {
            taxRate = rate;
        }
        
        decimal taxAmount = subTotal * (taxRate / 100);
        decimal total = subTotal + taxAmount;
        
        _subTotalLabel.Text = $"الإجمالي الفرعي: {subTotal:N2} جنيه";
        _taxLabel.Text = $"الضريبة ({taxRate}%): {taxAmount:N2} جنيه";
        _totalLabel.Text = $"الإجمالي الكلي: {total:N2} جنيه";
    }
    
    // Load invoice data for editing
    public async Task LoadInvoiceForEditAsync(int invoiceId)
    {
        try
        {
            _originalInvoice = await _context.PurchaseInvoices
                .Include(i => i.Items)
                .Include(i => i.Supplier)
                .Include(i => i.CashBox)
                .FirstOrDefaultAsync(i => i.PurchaseInvoiceId == invoiceId);
            
            if (_originalInvoice == null)
            {
                MessageBox.Show("لم يتم العثور على الفاتورة", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }
            
            if (_originalInvoice.PaidAmount > 0)
            {
                var result = MessageBox.Show(
                    $"تحذير: هذه الفاتورة تم دفع {_originalInvoice.PaidAmount:N2} جنيه منها.\n" +
                    "التعديل قد يؤثر على السجلات المالية.\n\n" +
                    "هل تريد المتابعة؟",
                    "تحذير",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                
                if (result != DialogResult.Yes)
                {
                    this.Close();
                    return;
                }
            }
            
            _supplierCombo.SelectedValue = _originalInvoice.SupplierId;
            _datePicker.Value = _originalInvoice.InvoiceDate;
            if (_originalInvoice.CashBoxId.HasValue)
                _cashBoxCombo.SelectedValue = _originalInvoice.CashBoxId.Value;
            _taxRateText.Text = _originalInvoice.TaxRate.ToString("0.##");
            _notesText.Text = _originalInvoice.Notes ?? "";
            
            _items.Clear();
            _itemsGrid.Rows.Clear();
            
            foreach (var item in _originalInvoice.Items)
            {
                var invoiceItem = new PurchaseInvoiceItem
                {
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };
                
                _items.Add(invoiceItem);
                _itemsGrid.Rows.Add(
                    item.Description,
                    item.Quantity,
                    item.UnitPrice.ToString("N2"),
                    item.TotalPrice.ToString("N2")
                );
            }
            
            CalculateTotals(null, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل بيانات الفاتورة: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
        }
    }
    
    private async void SaveInvoice_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_supplierCombo.SelectedIndex == -1)
            {
                MessageBox.Show("برجاء اختيار المورد", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (_items.Count == 0)
            {
                MessageBox.Show("برجاء إضافة صنف واحد على الأقل", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            decimal subTotal = _items.Sum(i => i.TotalPrice);
            decimal taxRate = decimal.Parse(_taxRateText.Text);
            decimal taxAmount = subTotal * (taxRate / 100);
            decimal total = subTotal + taxAmount;
            
            if (_isEditMode && _editInvoiceId.HasValue && _originalInvoice != null)
            {
                // Edit Mode
                if (_originalInvoice.PaidAmount > 0 && total != _originalInvoice.TotalAmount)
                {
                    var result = MessageBox.Show(
                        $"تحذير: تغيير الإجمالي من {_originalInvoice.TotalAmount:N2} إلى {total:N2} جنيه\n" +
                        $"مع وجود مدفوعات بقيمة {_originalInvoice.PaidAmount:N2} جنيه.\n\n" +
                        "هل تريد المتابعة؟",
                        "تأكيد التعديل",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                    
                    if (result != DialogResult.Yes)
                        return;
                }
                
                _originalInvoice.SupplierId = (int)_supplierCombo.SelectedValue!;
                _originalInvoice.InvoiceDate = _datePicker.Value;
                _originalInvoice.CashBoxId = _cashBoxCombo.SelectedValue != null ? (int)_cashBoxCombo.SelectedValue : null;
                _originalInvoice.SubTotal = subTotal;
                _originalInvoice.TaxRate = taxRate;
                _originalInvoice.TaxAmount = taxAmount;
                _originalInvoice.TotalAmount = total;
                _originalInvoice.Notes = _notesText.Text;
                
                if (_originalInvoice.PaidAmount >= total)
                    _originalInvoice.Status = "Paid";
                else if (_originalInvoice.PaidAmount > 0)
                    _originalInvoice.Status = "PartiallyPaid";
                else
                    _originalInvoice.Status = "Unpaid";
                
                _context.PurchaseInvoiceItems.RemoveRange(_originalInvoice.Items);
                _originalInvoice.Items = _items;
                
                await _context.SaveChangesAsync();
                
                MessageBox.Show("تم تحديث الفاتورة بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Add Mode
                var invoice = new PurchaseInvoice
                {
                    SupplierId = (int)_supplierCombo.SelectedValue!,
                    InvoiceDate = _datePicker.Value,
                    CashBoxId = _cashBoxCombo.SelectedValue != null ? (int)_cashBoxCombo.SelectedValue : null,
                    SubTotal = subTotal,
                    TaxRate = taxRate,
                    TaxAmount = taxAmount,
                    TotalAmount = total,
                    PaidAmount = 0,
                    Status = "Unpaid",
                    Notes = _notesText.Text,
                    CreatedBy = _currentUserId,
                    Items = _items
                };
                
                await _invoiceService.CreatePurchaseInvoiceAsync(invoice);
                
                MessageBox.Show("تم حفظ الفاتورة بنجاح", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في حفظ الفاتورة: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
