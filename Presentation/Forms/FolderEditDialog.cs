using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class FolderEditDialog : Form
{
    private TextBox _txtFolderName = null!;
    private TextBox _txtDescription = null!;
    private ComboBox _cmbIcon = null!;
    private Panel _colorPanel = null!;
    private Button _btnSave = null!;
    private Button _btnCancel = null!;
    
    private string _selectedColor = "#2196F3";
    
    public string FolderName => _txtFolderName.Text.Trim();
    public string Description => _txtDescription.Text.Trim();
    public string FolderIcon => _cmbIcon.SelectedItem?.ToString() ?? "📁";
    public string FolderColor => _selectedColor;
    public int? ParentFolderId { get; }

    public FolderEditDialog(FileFolder? existingFolder, int? parentFolderId)
    {
        ParentFolderId = parentFolderId;
        
        SetupForm();
        InitializeControls();
        
        if (existingFolder != null)
        {
            LoadExistingFolder(existingFolder);
        }
    }

    private void SetupForm()
    {
        this.Text = "إنشاء / تعديل مجلد";
        this.Size = new Size(600, 450);
        this.StartPosition = FormStartPosition.CenterParent;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 11F);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
    }

    private void InitializeControls()
    {
        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(30),
            BackColor = Color.White
        };

        int yPos = 20;

        // Folder Name
        var lblName = new Label
        {
            Text = "اسم المجلد:",
            Location = new Point(30, yPos),
            AutoSize = true,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = ColorScheme.TextPrimary
        };

        _txtFolderName = new TextBox
        {
            Location = new Point(30, yPos + 35),
            Size = new Size(510, 35),
            Font = new Font("Cairo", 11F),
            PlaceholderText = "أدخل اسم المجلد..."
        };

        yPos += 100;

        // Description
        var lblDesc = new Label
        {
            Text = "الوصف:",
            Location = new Point(30, yPos),
            AutoSize = true,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = ColorScheme.TextPrimary
        };

        _txtDescription = new TextBox
        {
            Location = new Point(30, yPos + 35),
            Size = new Size(510, 80),
            Font = new Font("Cairo", 11F),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            PlaceholderText = "وصف اختياري للمجلد..."
        };

        yPos += 150;

        // Icon Selection
        var lblIcon = new Label
        {
            Text = "الأيقونة:",
            Location = new Point(30, yPos),
            AutoSize = true,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = ColorScheme.TextPrimary
        };

        _cmbIcon = new ComboBox
        {
            Location = new Point(30, yPos + 35),
            Size = new Size(240, 35),
            Font = new Font("Segoe UI Emoji", 16F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cmbIcon.Items.AddRange(new object[] 
        { 
            "📁", "📂", "🗂️", "📋", "📊", "💼", "🏢", "💰", 
            "✈️", "🏨", "🚗", "📸", "📄", "📝", "🔒", "⭐" 
        });
        _cmbIcon.SelectedIndex = 0;

        // Color Selection
        var lblColor = new Label
        {
            Text = "اللون:",
            Location = new Point(300, yPos),
            AutoSize = true,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = ColorScheme.TextPrimary
        };

        _colorPanel = new Panel
        {
            Location = new Point(300, yPos + 35),
            Size = new Size(240, 35),
            BackColor = ColorTranslator.FromHtml(_selectedColor),
            BorderStyle = BorderStyle.FixedSingle,
            Cursor = Cursors.Hand
        };
        _colorPanel.Click += ColorPanel_Click;

        yPos += 100;

        // Buttons
        var btnPanel = new Panel
        {
            Location = new Point(30, yPos),
            Size = new Size(510, 50),
            BackColor = Color.Transparent
        };

        _btnSave = new Button
        {
            Text = "💾 حفظ",
            Location = new Point(380, 0),
            Size = new Size(130, 45),
            BackColor = ColorScheme.Success,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnSave.FlatAppearance.BorderSize = 0;
        _btnSave.Click += BtnSave_Click;

        _btnCancel = new Button
        {
            Text = "✖ إلغاء",
            Location = new Point(230, 0),
            Size = new Size(130, 45),
            BackColor = ColorScheme.Secondary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnCancel.FlatAppearance.BorderSize = 0;
        _btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

        btnPanel.Controls.AddRange(new Control[] { _btnSave, _btnCancel });

        mainPanel.Controls.AddRange(new Control[] 
        {
            lblName, _txtFolderName,
            lblDesc, _txtDescription,
            lblIcon, _cmbIcon,
            lblColor, _colorPanel,
            btnPanel
        });

        this.Controls.Add(mainPanel);
    }

    private void LoadExistingFolder(FileFolder folder)
    {
        _txtFolderName.Text = folder.FolderName;
        _txtDescription.Text = folder.Description ?? "";
        
        int iconIndex = _cmbIcon.Items.IndexOf(folder.Icon ?? "📁");
        if (iconIndex >= 0)
        {
            _cmbIcon.SelectedIndex = iconIndex;
        }
        
        if (!string.IsNullOrEmpty(folder.Color))
        {
            _selectedColor = folder.Color;
            _colorPanel.BackColor = ColorTranslator.FromHtml(_selectedColor);
        }
    }

    private void ColorPanel_Click(object? sender, EventArgs e)
    {
        using var colorDialog = new ColorDialog
        {
            AllowFullOpen = true,
            Color = _colorPanel.BackColor
        };

        if (colorDialog.ShowDialog() == DialogResult.OK)
        {
            _selectedColor = ColorTranslator.ToHtml(colorDialog.Color);
            _colorPanel.BackColor = colorDialog.Color;
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtFolderName.Text))
        {
            MessageBox.Show("يرجى إدخال اسم المجلد", "تنبيه",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtFolderName.Focus();
            return;
        }

        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}
