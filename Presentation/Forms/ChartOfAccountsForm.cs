using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class ChartOfAccountsForm : Form
{
    private readonly IAccountService _accountService;
    private readonly int _currentUserId;
    
    private Panel _headerPanel = null!;
    private TreeView _accountsTree = null!;
    private Button _addButton = null!;
    private Button _editButton = null!;
    private Button _deleteButton = null!;
    private Button _refreshButton = null!;
    private ComboBox _accountTypeFilter = null!;
    private CheckBox _showInactiveCheck = null!;
    
    public ChartOfAccountsForm(IAccountService accountService, int currentUserId)
    {
        _accountService = accountService;
        _currentUserId = currentUserId;
        
        InitializeComponent();
        InitializeCustomControls();
        _ = LoadDataAsync();
    }
    
    private void InitializeComponent()
    {
        this.Text = "شجرة الحسابات";
        this.Size = new Size(1400, 900);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
        this.WindowState = FormWindowState.Maximized;
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
            Text = "🌳 شجرة الحسابات",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(20, 20)
        };
        _headerPanel.Controls.Add(titleLabel);
        
        // Filter Controls
        Label filterLabel = new Label
        {
            Text = "نوع الحساب:",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 75)
        };
        _headerPanel.Controls.Add(filterLabel);
        
        _accountTypeFilter = new ComboBox
        {
            Font = new Font("Cairo", 10F),
            Size = new Size(200, 30),
            Location = new Point(130, 72),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _accountTypeFilter.Items.AddRange(new object[] { "الكل", "الأصول", "الخصوم", "حقوق الملكية", "الإيرادات", "المصروفات" });
        _accountTypeFilter.SelectedIndex = 0;
        _accountTypeFilter.SelectedIndexChanged += (s, e) => _ = LoadDataAsync();
        _headerPanel.Controls.Add(_accountTypeFilter);
        
        _showInactiveCheck = new CheckBox
        {
            Text = "إظهار الموقوفة",
            Font = new Font("Cairo", 10F),
            AutoSize = true,
            Location = new Point(350, 75),
            Checked = false
        };
        _showInactiveCheck.CheckedChanged += (s, e) => _ = LoadDataAsync();
        _headerPanel.Controls.Add(_showInactiveCheck);
        
        // Action Buttons
        _addButton = CreateButton("➕ إضافة حساب", ColorScheme.Success, new Point(550, 72), AddAccount_Click);
        _headerPanel.Controls.Add(_addButton);
        
        _editButton = CreateButton("✏️ تعديل", ColorScheme.Warning, new Point(730, 72), EditAccount_Click);
        _headerPanel.Controls.Add(_editButton);
        
        _deleteButton = CreateButton("🗑️ حذف", ColorScheme.Error, new Point(870, 72), DeleteAccount_Click);
        _headerPanel.Controls.Add(_deleteButton);
        
        _refreshButton = CreateButton("🔄 تحديث", Color.FromArgb(52, 73, 94), new Point(1010, 72), (s, e) => _ = LoadDataAsync());
        _headerPanel.Controls.Add(_refreshButton);
        
        // Main Panel
        Panel mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(20)
        };
        
        // TreeView
        _accountsTree = new TreeView
        {
            Dock = DockStyle.Fill,
            Font = new Font("Cairo", 11F),
            BackColor = Color.White,
            BorderStyle = BorderStyle.None,
            RightToLeft = RightToLeft.Yes,
            RightToLeftLayout = false,
            ShowLines = true,
            ShowPlusMinus = true,
            ShowRootLines = true,
            FullRowSelect = true,
            HideSelection = false,
            ItemHeight = 40,
            Indent = 30
        };
        _accountsTree.NodeMouseDoubleClick += (s, e) => EditAccount_Click(s, e);
        _accountsTree.DrawMode = TreeViewDrawMode.OwnerDrawText;
        _accountsTree.DrawNode += AccountsTree_DrawNode;
        mainPanel.Controls.Add(_accountsTree);
        
        // Add controls to form
        this.Controls.Add(mainPanel);
        this.Controls.Add(_headerPanel);
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
            _accountsTree.Nodes.Clear();
            
            // تحديد النوع المطلوب
            string? accountType = _accountTypeFilter.SelectedIndex switch
            {
                1 => "Asset",
                2 => "Liability",
                3 => "Equity",
                4 => "Revenue",
                5 => "Expense",
                _ => null
            };
            
            List<Account> rootAccounts;
            if (accountType != null)
            {
                rootAccounts = (await _accountService.GetAccountsByTypeAsync(accountType))
                    .Where(a => a.ParentAccountId == null).ToList();
            }
            else
            {
                rootAccounts = await _accountService.GetRootAccountsAsync();
            }
            
            // Filter inactive if needed
            if (!_showInactiveCheck.Checked)
            {
                rootAccounts = rootAccounts.Where(a => a.IsActive).ToList();
            }
            
            foreach (var account in rootAccounts)
            {
                var node = CreateTreeNode(account);
                _accountsTree.Nodes.Add(node);
                await LoadChildNodesAsync(node, account.AccountId);
            }
            
            _accountsTree.ExpandAll();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async Task LoadChildNodesAsync(TreeNode parentNode, int parentAccountId)
    {
        var childAccounts = await _accountService.GetChildAccountsAsync(parentAccountId);
        
        // Filter inactive if needed
        if (!_showInactiveCheck.Checked)
        {
            childAccounts = childAccounts.Where(a => a.IsActive).ToList();
        }
        
        foreach (var child in childAccounts)
        {
            var childNode = CreateTreeNode(child);
            parentNode.Nodes.Add(childNode);
            await LoadChildNodesAsync(childNode, child.AccountId);
        }
    }
    
    private TreeNode CreateTreeNode(Account account)
    {
        var displayText = $"{account.AccountCode} - {account.AccountName}";
        if (account.CurrentBalance != 0)
        {
            displayText += $" ({account.CurrentBalance:N2})";
        }
        if (!account.IsActive)
        {
            displayText += " [موقوف]";
        }
        
        var node = new TreeNode(displayText)
        {
            Tag = account,
            ForeColor = account.IsActive ? Color.Black : Color.Gray
        };
        
        // تلوين حسب النوع
        if (account.Level == 1)
        {
            node.NodeFont = new Font("Cairo", 11F, FontStyle.Bold);
            node.ForeColor = account.AccountType switch
            {
                "Asset" => Color.FromArgb(0, 123, 255),
                "Liability" => Color.FromArgb(220, 53, 69),
                "Equity" => Color.FromArgb(108, 117, 125),
                "Revenue" => Color.FromArgb(40, 167, 69),
                "Expense" => Color.FromArgb(255, 193, 7),
                _ => Color.Black
            };
        }
        
        return node;
    }
    
    private void AddAccount_Click(object? sender, EventArgs e)
    {
        try
        {
            int? parentId = null;
            if (_accountsTree.SelectedNode != null && _accountsTree.SelectedNode.Tag is Account selectedAccount)
            {
                parentId = selectedAccount.AccountId;
            }
            
            using var form = new AddEditAccountForm(_accountService, null, parentId);
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
    
    private void EditAccount_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_accountsTree.SelectedNode == null || _accountsTree.SelectedNode.Tag is not Account account)
            {
                MessageBox.Show("برجاء اختيار حساب للتعديل", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            using var form = new AddEditAccountForm(_accountService, account.AccountId, null);
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
    
    private async void DeleteAccount_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_accountsTree.SelectedNode == null || _accountsTree.SelectedNode.Tag is not Account account)
            {
                MessageBox.Show("برجاء اختيار حساب للحذف", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف الحساب: {account.AccountName}؟",
                "تأكيد الحذف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            
            if (result == DialogResult.Yes)
            {
                await _accountService.DeleteAccountAsync(account.AccountId);
                MessageBox.Show("تم حذف الحساب بنجاح", "نجح",
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
    
    private void AccountsTree_DrawNode(object? sender, DrawTreeNodeEventArgs e)
    {
        if (e.Node == null) return;
        
        // رسم الخلفية
        e.Graphics.FillRectangle(
            e.Node.IsSelected ? new SolidBrush(Color.FromArgb(230, 240, 255)) : Brushes.White,
            e.Bounds);
        
        // رسم النص بدون قطع
        var textFont = e.Node.NodeFont ?? _accountsTree.Font;
        var textBrush = new SolidBrush(e.Node.ForeColor);
        
        // حساب الموضع المناسب للنص - محاذاة لليمين
        var textBounds = new Rectangle(
            e.Bounds.X,
            e.Bounds.Y + 2,
            _accountsTree.Width - e.Bounds.X - 20,
            e.Bounds.Height - 4);
        
        var format = new StringFormat
        {
            Alignment = StringAlignment.Far,  // محاذاة لليمين
            LineAlignment = StringAlignment.Center,
            FormatFlags = StringFormatFlags.DirectionRightToLeft
        };
        
        e.Graphics.DrawString(e.Node.Text, textFont, textBrush, textBounds, format);
    }
}
