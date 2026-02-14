using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;
using System.Diagnostics;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class FileManagerForm : Form
{
    private readonly IFileManagerService _fileService;
    private readonly int _currentUserId;

    // Layout - TableLayoutPanel بدل SplitContainer لتجنب مشكلة SplitterDistance
    private TableLayoutPanel _mainLayout = null!;

    // Left Panel
    private Panel _leftPanel = null!;
    private Panel _leftHeaderPanel = null!;
    private Panel _leftBtnPanel = null!;
    private Panel _statsPanel = null!;
    private TreeView _tvFolders = null!;
    private Button _btnNewFolder = null!;
    private Button _btnEditFolder = null!;
    private Button _btnDeleteFolder = null!;
    private Button _btnRefreshFolders = null!;
    private TextBox _txtFolderSearch = null!;
    private Label _lblFolderCount = null!;
    private Label _lblDocumentCount = null!;
    private Label _lblStorageSize = null!;

    // Right Panel
    private Panel _rightPanel = null!;
    private Panel _rightHeaderPanel = null!;
    private Panel _toolbar1Panel = null!;
    private Panel _toolbar2Panel = null!;
    private ListView _lvDocuments = null!;
    private TableLayoutPanel _rightContentLayout = null!;
    private Panel _previewPanel = null!;
    private PictureBox _previewImage = null!;
    private Label _previewFileName = null!;
    private Label _previewFileInfo = null!;
    private Panel _previewPlaceholder = null!;
    private ComboBox _cmbViewMode = null!;
    private ComboBox _cmbFilterType = null!;
    private TextBox _txtDocumentSearch = null!;
    private Button _btnUploadFile = null!;
    private Button _btnDownload = null!;
    private Button _btnDelete = null!;
    private Button _btnFavorite = null!;
    private Button _btnRefresh = null!;

    // Context Menus
    private ContextMenuStrip _folderContextMenu = null!;
    private ContextMenuStrip _documentContextMenu = null!;

    // State
    private int? _currentFolderId = null;
    private List<FileFolder> _allFolders = new();
    private ImageList _documentImageList = null!;

    public FileManagerForm(IFileManagerService fileService, int currentUserId)
    {
        _fileService = fileService;
        _currentUserId = currentUserId;
        SetupForm();
        InitializeControls();
        _ = LoadDataAsync();
    }

    private void SetupForm()
    {
        this.Text = "📁 إدارة الملفات والمستندات";
        this.Size = new Size(1400, 800);
        this.MinimumSize = new Size(1000, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = Color.FromArgb(245, 247, 250);
        this.Font = new Font("Cairo", 10F);
        this.WindowState = FormWindowState.Maximized;
    }

    private void InitializeControls()
    {
        _documentImageList = new ImageList
        {
            ImageSize = new Size(48, 48),
            ColorDepth = ColorDepth.Depth32Bit
        };

        // TableLayoutPanel - لا يحتاج SplitterDistance أبداً
        _mainLayout = new TableLayoutPanel
        {
            Dock            = DockStyle.Fill,
            RowCount        = 1,
            ColumnCount     = 2,
            BackColor       = Color.FromArgb(200, 210, 230),
            Padding         = new Padding(0),
            Margin          = new Padding(0),
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
        };
        _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));
        _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        BuildLeftPanel();
        BuildRightPanel();
        BuildContextMenus();

        this.Controls.Add(_mainLayout);
    }

    private void BuildLeftPanel()
    {
        _leftPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

        _leftHeaderPanel = new Panel
        {
            Dock = DockStyle.Top, Height = 50,
            BackColor = ColorScheme.Primary, Padding = new Padding(12, 0, 12, 0)
        };
        _leftHeaderPanel.Controls.Add(new Label
        {
            Text = "📂  المجلدات", Font = new Font("Cairo", 13F, FontStyle.Bold),
            ForeColor = Color.White, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight
        });

        var searchPanel = new Panel
        {
            Dock = DockStyle.Top, Height = 50,
            BackColor = Color.White, Padding = new Padding(10, 8, 10, 8)
        };
        _txtFolderSearch = new TextBox
        {
            Dock = DockStyle.Fill, Font = new Font("Cairo", 10F),
            PlaceholderText = "🔍 بحث في المجلدات...", BorderStyle = BorderStyle.FixedSingle
        };
        _txtFolderSearch.TextChanged += TxtFolderSearch_TextChanged;
        searchPanel.Controls.Add(_txtFolderSearch);

        _leftBtnPanel = new Panel
        {
            Dock = DockStyle.Top, Height = 48,
            BackColor = Color.FromArgb(248, 249, 252), Padding = new Padding(8, 6, 8, 6)
        };
        var btnFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false, BackColor = Color.Transparent
        };
        _btnNewFolder     = MakeIconBtn("➕", ColorScheme.Success, "مجلد جديد");
        _btnEditFolder    = MakeIconBtn("✏️", ColorScheme.Info, "تعديل");
        _btnDeleteFolder  = MakeIconBtn("🗑️", Color.FromArgb(211, 47, 47), "حذف");
        _btnRefreshFolders = MakeIconBtn("🔄", ColorScheme.Secondary, "تحديث");
        _btnNewFolder.Click     += async (s, e) => await CreateNewFolderAsync();
        _btnEditFolder.Click    += async (s, e) => await EditSelectedFolderAsync();
        _btnDeleteFolder.Click  += async (s, e) => await DeleteSelectedFolderAsync();
        _btnRefreshFolders.Click += async (s, e) => await LoadFoldersAsync();
        btnFlow.Controls.AddRange(new Control[] { _btnNewFolder, _btnEditFolder, _btnDeleteFolder, _btnRefreshFolders });
        _leftBtnPanel.Controls.Add(btnFlow);

        _tvFolders = new TreeView
        {
            Dock = DockStyle.Fill, Font = new Font("Cairo", 11F),
            BorderStyle = BorderStyle.None, ShowLines = true, ShowPlusMinus = true,
            ShowRootLines = true, FullRowSelect = true, HideSelection = false,
            BackColor = Color.White, Indent = 20, ItemHeight = 32
        };
        _tvFolders.AfterSelect          += TvFolders_AfterSelect;
        _tvFolders.MouseUp              += TvFolders_MouseUp;
        _tvFolders.NodeMouseDoubleClick += TvFolders_NodeMouseDoubleClick;

        _statsPanel = new Panel
        {
            Dock = DockStyle.Bottom, Height = 95,
            BackColor = Color.FromArgb(240, 244, 255), Padding = new Padding(12, 8, 12, 8)
        };
        _lblFolderCount   = MakeStatLabel("📁  المجلدات: 0",  Color.FromArgb(63, 81, 181));
        _lblDocumentCount = MakeStatLabel("📄  الملفات: 0",   Color.FromArgb(2, 136, 209));
        _lblStorageSize   = MakeStatLabel("💾  المساحة: 0 KB", Color.FromArgb(56, 142, 60));
        _lblFolderCount.Dock = _lblDocumentCount.Dock = _lblStorageSize.Dock = DockStyle.Top;
        _statsPanel.Controls.Add(_lblStorageSize);
        _statsPanel.Controls.Add(_lblDocumentCount);
        _statsPanel.Controls.Add(_lblFolderCount);

        _leftPanel.Controls.Add(_tvFolders);
        _leftPanel.Controls.Add(_leftBtnPanel);
        _leftPanel.Controls.Add(searchPanel);
        _leftPanel.Controls.Add(_leftHeaderPanel);
        _leftPanel.Controls.Add(_statsPanel);

        _mainLayout.Controls.Add(_leftPanel, 0, 0);
    }

    private void BuildRightPanel()
    {
        _rightPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

        _rightHeaderPanel = new Panel
        {
            Dock = DockStyle.Top, Height = 50,
            BackColor = ColorScheme.Primary, Padding = new Padding(12, 0, 12, 0)
        };
        _rightHeaderPanel.Controls.Add(new Label
        {
            Text = "📄  المستندات والملفات", Font = new Font("Cairo", 13F, FontStyle.Bold),
            ForeColor = Color.White, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight
        });

        _toolbar1Panel = new Panel
        {
            Dock = DockStyle.Top, Height = 50,
            BackColor = Color.FromArgb(248, 249, 252), Padding = new Padding(10, 8, 10, 8)
        };
        var t1Flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false, BackColor = Color.Transparent
        };
        _txtDocumentSearch = new TextBox
        {
            Width = 280, Height = 34, Font = new Font("Cairo", 10F),
            PlaceholderText = "🔍 بحث في الملفات...", BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(6, 0, 0, 0)
        };
        _txtDocumentSearch.TextChanged += TxtDocumentSearch_TextChanged;
        _cmbViewMode = new ComboBox
        {
            Width = 150, DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Cairo", 10F), Margin = new Padding(6, 0, 0, 0)
        };
        _cmbViewMode.Items.AddRange(new object[] { "عرض كبير", "عرض صغير", "قائمة تفصيلية" });
        _cmbViewMode.SelectedIndex = 0;
        _cmbViewMode.SelectedIndexChanged += CmbViewMode_SelectedIndexChanged;
        _cmbFilterType = new ComboBox
        {
            Width = 150, DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Cairo", 10F), Margin = new Padding(6, 0, 0, 0)
        };
        _cmbFilterType.Items.AddRange(new object[] { "الكل", "صور", "مستندات", "PDF", "Excel" });
        _cmbFilterType.SelectedIndex = 0;
        _cmbFilterType.SelectedIndexChanged += CmbFilterType_SelectedIndexChanged;
        t1Flow.Controls.AddRange(new Control[] { _txtDocumentSearch, _cmbViewMode, _cmbFilterType });
        _toolbar1Panel.Controls.Add(t1Flow);

        _toolbar2Panel = new Panel
        {
            Dock = DockStyle.Top, Height = 52,
            BackColor = Color.White, Padding = new Padding(10, 6, 10, 6)
        };
        var t2Flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false, BackColor = Color.Transparent
        };
        _btnUploadFile = MakeActionBtn("📤  رفع ملف", ColorScheme.Primary);
        _btnDownload   = MakeActionBtn("📥  تحميل",   ColorScheme.Success);
        _btnDelete     = MakeActionBtn("🗑️  حذف",     Color.FromArgb(211, 47, 47));
        _btnFavorite   = MakeActionBtn("⭐  مفضلة",   Color.FromArgb(255, 152, 0));
        _btnRefresh    = MakeActionBtn("🔄  تحديث",   ColorScheme.Info);
        _btnUploadFile.Click += async (s, e) => await UploadFileAsync();
        _btnDownload.Click   += async (s, e) => await DownloadFileAsync();
        _btnDelete.Click     += async (s, e) => await DeleteFileAsync();
        _btnFavorite.Click   += async (s, e) => await ToggleFavoriteAsync();
        _btnRefresh.Click    += async (s, e) => await LoadDocumentsAsync();
        t2Flow.Controls.AddRange(new Control[] { _btnUploadFile, _btnDownload, _btnDelete, _btnFavorite, _btnRefresh });
        _toolbar2Panel.Controls.Add(t2Flow);

        _lvDocuments = new ListView
        {
            Dock = DockStyle.Fill, View = View.LargeIcon,
            LargeImageList = _documentImageList, SmallImageList = _documentImageList,
            Font = new Font("Cairo", 10F), BackColor = Color.White,
            BorderStyle = BorderStyle.None, FullRowSelect = true,
            MultiSelect = false, Activation = ItemActivation.TwoClick
        };
        _lvDocuments.Columns.Add("اسم الملف", 350);
        _lvDocuments.Columns.Add("النوع", 100);
        _lvDocuments.Columns.Add("الحجم", 100);
        _lvDocuments.Columns.Add("تاريخ الرفع", 150);
        _lvDocuments.Columns.Add("المستخدم", 150);
        _lvDocuments.ItemActivate         += LvDocuments_ItemActivate;
        _lvDocuments.MouseUp              += LvDocuments_MouseUp;
        _lvDocuments.SelectedIndexChanged += LvDocuments_SelectedIndexChanged;

        // ─── Preview Panel ───
        _previewPanel = new Panel
        {
            Dock = DockStyle.Fill, BackColor = Color.FromArgb(28, 32, 48),
            Padding = new Padding(0), Visible = false
        };

        // Header
        var previewHeader = new Panel
        {
            Dock = DockStyle.Top, Height = 46,
            BackColor = Color.FromArgb(20, 24, 40)
        };
        var lblPreviewTitle = new Label
        {
            Text = "👁  معاينة الصورة", Font = new Font("Cairo", 11F, FontStyle.Bold),
            ForeColor = Color.White, Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 14, 0)
        };
        var btnClosePreview = new Button
        {
            Text = "✕", Size = new Size(40, 40), Dock = DockStyle.Left,
            BackColor = Color.Transparent, ForeColor = Color.FromArgb(180, 180, 180),
            FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 13F),
            Cursor = Cursors.Hand
        };
        btnClosePreview.FlatAppearance.BorderSize = 0;
        btnClosePreview.Click += (s, e) => ClosePreview();
        previewHeader.Controls.Add(lblPreviewTitle);
        previewHeader.Controls.Add(btnClosePreview);

        // Image Box
        _previewImage = new PictureBox
        {
            Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(18, 20, 32)
        };

        // Footer info
        var previewFooter = new Panel
        {
            Dock = DockStyle.Bottom, Height = 64,
            BackColor = Color.FromArgb(20, 24, 40), Padding = new Padding(14, 6, 14, 6)
        };
        _previewFileName = new Label
        {
            Dock = DockStyle.Top, Height = 30, AutoSize = false,
            Font = new Font("Cairo", 11F, FontStyle.Bold), ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleRight, Text = ""
        };
        _previewFileInfo = new Label
        {
            Dock = DockStyle.Fill, AutoSize = false,
            Font = new Font("Cairo", 9.5F), ForeColor = Color.FromArgb(160, 170, 200),
            TextAlign = ContentAlignment.MiddleRight, Text = ""
        };
        previewFooter.Controls.Add(_previewFileInfo);
        previewFooter.Controls.Add(_previewFileName);

        _previewPanel.Controls.Add(_previewImage);
        _previewPanel.Controls.Add(previewFooter);
        _previewPanel.Controls.Add(previewHeader);

        // ─── Content Layout: ListView + Preview جنب بعض ───
        _rightContentLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, RowCount = 1, ColumnCount = 2,
            BackColor = Color.White, Padding = new Padding(0), Margin = new Padding(0)
        };
        _rightContentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _rightContentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0F)); // مخفي في البداية
        _rightContentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _rightContentLayout.Controls.Add(_lvDocuments, 0, 0);
        _rightContentLayout.Controls.Add(_previewPanel, 1, 0);

        _rightPanel.Controls.Add(_rightContentLayout);
        _rightPanel.Controls.Add(_toolbar2Panel);
        _rightPanel.Controls.Add(_toolbar1Panel);
        _rightPanel.Controls.Add(_rightHeaderPanel);

        _mainLayout.Controls.Add(_rightPanel, 1, 0);
    }

    private void BuildContextMenus()
    {
        _folderContextMenu = new ContextMenuStrip { Font = new Font("Cairo", 10F), RightToLeft = RightToLeft.Yes };
        var mNF = new ToolStripMenuItem("➕  مجلد جديد");
        var mEF = new ToolStripMenuItem("✏️  تعديل");
        var mDF = new ToolStripMenuItem("🗑️  حذف") { ForeColor = Color.FromArgb(211, 47, 47) };
        var mRF = new ToolStripMenuItem("🔄  تحديث");
        mNF.Click += async (s, e) => await CreateNewFolderAsync();
        mEF.Click += async (s, e) => await EditSelectedFolderAsync();
        mDF.Click += async (s, e) => await DeleteSelectedFolderAsync();
        mRF.Click += async (s, e) => await LoadFoldersAsync();
        _folderContextMenu.Items.AddRange(new ToolStripItem[] { mNF, new ToolStripSeparator(), mEF, new ToolStripSeparator(), mDF, new ToolStripSeparator(), mRF });

        _documentContextMenu = new ContextMenuStrip { Font = new Font("Cairo", 10F), RightToLeft = RightToLeft.Yes };
        var mOpen  = new ToolStripMenuItem("📂  فتح الملف");
        var mDl    = new ToolStripMenuItem("📥  تحميل");
        var mFav   = new ToolStripMenuItem("⭐  إضافة للمفضلة");
        var mRen   = new ToolStripMenuItem("✏️  إعادة تسمية");
        var mMov   = new ToolStripMenuItem("📦  نقل إلى مجلد آخر");
        var mProp  = new ToolStripMenuItem("ℹ️  الخصائص");
        var mDel   = new ToolStripMenuItem("🗑️  حذف") { ForeColor = Color.FromArgb(211, 47, 47) };
        mOpen.Click  += async (s, e) => await OpenFileAsync();
        mDl.Click    += async (s, e) => await DownloadFileAsync();
        mFav.Click   += async (s, e) => await ToggleFavoriteAsync();
        mRen.Click   += async (s, e) => await RenameFileAsync();
        mMov.Click   += async (s, e) => await MoveFileAsync();
        mProp.Click  += ShowFileProperties;
        mDel.Click   += async (s, e) => await DeleteFileAsync();
        _documentContextMenu.Items.AddRange(new ToolStripItem[] { mOpen, mDl, new ToolStripSeparator(), mFav, mRen, mMov, new ToolStripSeparator(), mProp, new ToolStripSeparator(), mDel });

        _tvFolders.ContextMenuStrip   = _folderContextMenu;
        _lvDocuments.ContextMenuStrip = _documentContextMenu;
    }

    private Button MakeIconBtn(string text, Color color, string tip)
    {
        var b = new Button
        {
            Text = text, Size = new Size(44, 34), BackColor = color,
            ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand, Font = new Font("Segoe UI Emoji", 11F),
            Margin = new Padding(4, 0, 0, 0)
        };
        b.FlatAppearance.BorderSize = 0;
        new ToolTip().SetToolTip(b, tip);
        return b;
    }

    private Button MakeActionBtn(string text, Color color)
    {
        var b = new Button
        {
            Text = text, Size = new Size(130, 38), BackColor = color,
            ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand, Font = new Font("Cairo", 10F, FontStyle.Bold),
            Margin = new Padding(6, 0, 0, 0)
        };
        b.FlatAppearance.BorderSize = 0;
        return b;
    }

    private Label MakeStatLabel(string text, Color color) => new Label
    {
        Text = text, AutoSize = false, Height = 26,
        Font = new Font("Cairo", 10F, FontStyle.Bold),
        ForeColor = color, TextAlign = ContentAlignment.MiddleRight
    };

    private async Task LoadDataAsync() { await LoadFoldersAsync(); await LoadStatisticsAsync(); }

    private async Task LoadFoldersAsync()
    {
        try
        {
            this.Cursor = Cursors.WaitCursor;
            _allFolders = await _fileService.GetAllFoldersAsync();
            _tvFolders.Nodes.Clear();
            foreach (var f in _allFolders.Where(x => x.ParentFolderId == null).OrderBy(x => x.DisplayOrder))
            {
                var node = MakeFolderNode(f);
                _tvFolders.Nodes.Add(node);
                AddSubFolders(node, f.FolderId);
            }
            if (_tvFolders.Nodes.Count > 0) _tvFolders.SelectedNode = _tvFolders.Nodes[0];
        }
        catch (Exception ex) { MessageBox.Show($"خطأ في تحميل المجلدات:\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { this.Cursor = Cursors.Default; }
    }

    private TreeNode MakeFolderNode(FileFolder f) => new TreeNode
    {
        Text = $"  {f.Icon}  {f.FolderName}  ({f.Documents.Count})",
        Tag = f, ForeColor = f.IsSystem ? ColorScheme.Secondary : ColorScheme.TextPrimary
    };

    private void AddSubFolders(TreeNode parent, int parentId)
    {
        foreach (var f in _allFolders.Where(x => x.ParentFolderId == parentId).OrderBy(x => x.DisplayOrder))
        { var n = MakeFolderNode(f); parent.Nodes.Add(n); AddSubFolders(n, f.FolderId); }
    }

    private async Task LoadDocumentsAsync()
    {
        if (_currentFolderId == null) return;
        try
        {
            this.Cursor = Cursors.WaitCursor;
            _lvDocuments.Items.Clear();
            var docs = await _fileService.GetDocumentsByFolderAsync(_currentFolderId.Value);
            if (_cmbFilterType.SelectedIndex > 0)
                docs = docs.Where(d => d.DocumentType == (DocumentType)_cmbFilterType.SelectedIndex).ToList();
            foreach (var doc in docs)
            {
                var item = new ListViewItem(doc.OriginalFileName) { Tag = doc, ImageIndex = (int)doc.DocumentType };
                item.SubItems.Add(doc.DocumentType.ToString());
                item.SubItems.Add(FormatFileSize(doc.FileSize));
                item.SubItems.Add(doc.UploadedAt.ToString("dd/MM/yyyy HH:mm"));
                item.SubItems.Add(doc.Uploader?.FullName ?? "غير معروف");
                if (doc.IsFavorite) item.BackColor = Color.FromArgb(255, 248, 225);
                _lvDocuments.Items.Add(item);
            }
        }
        catch (Exception ex) { MessageBox.Show($"خطأ في تحميل الملفات:\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { this.Cursor = Cursors.Default; }
    }

    private async Task LoadStatisticsAsync()
    {
        try
        {
            _lblFolderCount.Text   = $"📁  المجلدات: {await _fileService.GetFolderCountAsync()}";
            _lblDocumentCount.Text = $"📄  الملفات: {await _fileService.GetDocumentCountAsync()}";
            _lblStorageSize.Text   = $"💾  المساحة: {FormatFileSize(await _fileService.GetTotalStorageSizeAsync())}";
        }
        catch { }
    }

    private async void TvFolders_AfterSelect(object? sender, TreeViewEventArgs e)
    { if (e.Node?.Tag is FileFolder f) { _currentFolderId = f.FolderId; await LoadDocumentsAsync(); } }

    private void TvFolders_MouseUp(object? sender, MouseEventArgs e)
    { if (e.Button == MouseButtons.Right) { var n = _tvFolders.GetNodeAt(e.X, e.Y); if (n != null) _tvFolders.SelectedNode = n; } }

    private void TvFolders_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
    { if (e.Node?.Tag is FileFolder) _ = EditSelectedFolderAsync(); }

    private void LvDocuments_MouseUp(object? sender, MouseEventArgs e)
    { if (e.Button == MouseButtons.Right) { var it = _lvDocuments.GetItemAt(e.X, e.Y); if (it != null) it.Selected = true; } }

    private async void LvDocuments_ItemActivate(object? sender, EventArgs e) => await OpenFileAsync();

    private void LvDocuments_SelectedIndexChanged(object? sender, EventArgs e)
    {
        bool has = _lvDocuments.SelectedItems.Count > 0;
        _btnDownload.Enabled = _btnDelete.Enabled = _btnFavorite.Enabled = has;

        if (has && _lvDocuments.SelectedItems[0].Tag is FileDocument doc)
            ShowPreviewIfImage(doc);
        else
            ClosePreview();
    }

    private static readonly HashSet<string> _imageExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff" };

    private void ShowPreviewIfImage(FileDocument doc)
    {
        if (!_imageExtensions.Contains(doc.FileExtension))
        {
            ClosePreview();
            return;
        }

        try
        {
            if (!File.Exists(doc.FilePath)) { ClosePreview(); return; }

            // تحميل الصورة
            using var stream = new FileStream(doc.FilePath, FileMode.Open, FileAccess.Read);
            var img = Image.FromStream(stream, false, false);
            _previewImage.Image?.Dispose();
            _previewImage.Image = img;

            _previewFileName.Text = doc.OriginalFileName;
            _previewFileInfo.Text = $"📐 {img.Width} × {img.Height} px     💾 {FormatFileSize(doc.FileSize)}     📅 {doc.UploadedAt:dd/MM/yyyy}";

            // افتح الـ preview column
            _rightContentLayout.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, 320F);
            _previewPanel.Visible = true;
        }
        catch
        {
            ClosePreview();
        }
    }

    private void ClosePreview()
    {
        _previewPanel.Visible = false;
        _previewImage.Image?.Dispose();
        _previewImage.Image = null;
        _rightContentLayout.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, 0F);
    }

    private async void TxtFolderSearch_TextChanged(object? sender, EventArgs e) => await FilterFoldersAsync();

    private async void TxtDocumentSearch_TextChanged(object? sender, EventArgs e)
    { if (string.IsNullOrWhiteSpace(_txtDocumentSearch.Text)) await LoadDocumentsAsync(); else await SearchDocumentsAsync(); }

    private void CmbViewMode_SelectedIndexChanged(object? sender, EventArgs e)
    { _lvDocuments.View = _cmbViewMode.SelectedIndex switch { 0 => View.LargeIcon, 1 => View.SmallIcon, _ => View.Details }; }

    private async void CmbFilterType_SelectedIndexChanged(object? sender, EventArgs e) => await LoadDocumentsAsync();

    private async Task FilterFoldersAsync()
    {
        if (string.IsNullOrWhiteSpace(_txtFolderSearch.Text)) { await LoadFoldersAsync(); return; }
        var q = _txtFolderSearch.Text.ToLower();
        _tvFolders.Nodes.Clear();
        foreach (var f in _allFolders.Where(x => x.FolderName.ToLower().Contains(q) || (x.Description?.ToLower().Contains(q) ?? false)))
            _tvFolders.Nodes.Add(MakeFolderNode(f));
    }

    private async Task CreateNewFolderAsync()
    {
        var dlg = new FolderEditDialog(null, _currentFolderId);
        if (dlg.ShowDialog() != DialogResult.OK) return;
        try
        {
            await _fileService.CreateFolderAsync(new FileFolder
            {
                FolderName = dlg.FolderName, Description = dlg.Description,
                Color = dlg.FolderColor, Icon = dlg.FolderIcon,
                ParentFolderId = dlg.ParentFolderId, DisplayOrder = 0, CreatedBy = _currentUserId
            });
            MessageBox.Show("تم إنشاء المجلد بنجاح!", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadFoldersAsync(); await LoadStatisticsAsync();
        }
        catch (Exception ex) { MessageBox.Show($"خطأ:\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task EditSelectedFolderAsync()
    {
        if (_tvFolders.SelectedNode?.Tag is not FileFolder folder)
        { MessageBox.Show("يرجى اختيار مجلد", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        var dlg = new FolderEditDialog(folder, null);
        if (dlg.ShowDialog() != DialogResult.OK) return;
        try
        {
            folder.FolderName = dlg.FolderName; folder.Description = dlg.Description;
            folder.Color = dlg.FolderColor; folder.Icon = dlg.FolderIcon;
            await _fileService.UpdateFolderAsync(folder);
            MessageBox.Show("تم التحديث!", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadFoldersAsync();
        }
        catch (Exception ex) { MessageBox.Show($"خطأ:\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task DeleteSelectedFolderAsync()
    {
        if (_tvFolders.SelectedNode?.Tag is not FileFolder folder)
        { MessageBox.Show("يرجى اختيار مجلد", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (folder.IsSystem)
        { MessageBox.Show("لا يمكن حذف المجلدات النظامية", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (MessageBox.Show($"حذف '{folder.FolderName}'؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        try
        {
            await _fileService.DeleteFolderAsync(folder.FolderId);
            MessageBox.Show("تم الحذف!", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadFoldersAsync(); await LoadStatisticsAsync();
        }
        catch (Exception ex) { MessageBox.Show($"خطأ:\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task UploadFileAsync()
    {
        if (_currentFolderId == null) { MessageBox.Show("يرجى اختيار مجلد أولاً", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        using var dlg = new OpenFileDialog { Title = "اختر ملف", Filter = "جميع الملفات (*.*)|*.*", Multiselect = true };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        try
        {
            this.Cursor = Cursors.WaitCursor;
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads");
            Directory.CreateDirectory(dir);
            foreach (var path in dlg.FileNames)
            {
                var fi = new FileInfo(path);
                var name = $"{Guid.NewGuid()}{fi.Extension}";
                var dest = Path.Combine(dir, name);
                File.Copy(path, dest, true);
                await _fileService.AddDocumentAsync(new FileDocument
                {
                    FolderId = _currentFolderId.Value, FileName = name,
                    OriginalFileName = fi.Name, FilePath = dest,
                    FileExtension = fi.Extension, FileSize = fi.Length,
                    DocumentType = GetDocumentType(fi.Extension),
                    MimeType = GetMimeType(fi.Extension), UploadedBy = _currentUserId
                });
            }
            MessageBox.Show($"تم رفع {dlg.FileNames.Length} ملف!", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadDocumentsAsync(); await LoadStatisticsAsync();
        }
        catch (Exception ex) { MessageBox.Show($"خطأ:\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { this.Cursor = Cursors.Default; }
    }

    private async Task DownloadFileAsync()
    {
        if (_lvDocuments.SelectedItems.Count == 0 || _lvDocuments.SelectedItems[0].Tag is not FileDocument doc) return;
        using var dlg = new SaveFileDialog { FileName = doc.OriginalFileName, Filter = "جميع الملفات (*.*)|*.*" };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        try { File.Copy(doc.FilePath, dlg.FileName, true); await _fileService.IncrementDownloadCountAsync(doc.DocumentId); MessageBox.Show("تم التحميل!", "نجح", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        catch (Exception ex) { MessageBox.Show($"خطأ:\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task OpenFileAsync()
    {
        if (_lvDocuments.SelectedItems.Count == 0 || _lvDocuments.SelectedItems[0].Tag is not FileDocument doc) return;
        try
        {
            if (File.Exists(doc.FilePath)) { Process.Start(new ProcessStartInfo { FileName = doc.FilePath, UseShellExecute = true }); await _fileService.IncrementDownloadCountAsync(doc.DocumentId); }
            else MessageBox.Show("الملف غير موجود!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex) { MessageBox.Show($"خطأ:\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task DeleteFileAsync()
    {
        if (_lvDocuments.SelectedItems.Count == 0 || _lvDocuments.SelectedItems[0].Tag is not FileDocument doc) return;
        if (MessageBox.Show($"حذف '{doc.OriginalFileName}'؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        try { await _fileService.DeleteDocumentAsync(doc.DocumentId); await LoadDocumentsAsync(); await LoadStatisticsAsync(); }
        catch (Exception ex) { MessageBox.Show($"خطأ:\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task ToggleFavoriteAsync()
    {
        if (_lvDocuments.SelectedItems.Count == 0 || _lvDocuments.SelectedItems[0].Tag is not FileDocument doc) return;
        try { await _fileService.ToggleFavoriteAsync(doc.DocumentId); await LoadDocumentsAsync(); }
        catch (Exception ex) { MessageBox.Show($"خطأ:\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task RenameFileAsync()
    {
        if (_lvDocuments.SelectedItems.Count == 0 || _lvDocuments.SelectedItems[0].Tag is not FileDocument doc) return;
        var name = Microsoft.VisualBasic.Interaction.InputBox("أدخل الاسم الجديد:", "إعادة تسمية", doc.OriginalFileName);
        if (string.IsNullOrWhiteSpace(name)) return;
        try { doc.OriginalFileName = name; await _fileService.UpdateDocumentAsync(doc); await LoadDocumentsAsync(); }
        catch (Exception ex) { MessageBox.Show($"خطأ:\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private Task MoveFileAsync()
    { MessageBox.Show("سيتم إضافة هذه الميزة قريباً", "قريباً", MessageBoxButtons.OK, MessageBoxIcon.Information); return Task.CompletedTask; }

    private void ShowFileProperties(object? sender, EventArgs e)
    {
        if (_lvDocuments.SelectedItems.Count == 0 || _lvDocuments.SelectedItems[0].Tag is not FileDocument doc) return;
        MessageBox.Show(
            $"📄  {doc.OriginalFileName}\n📊  {doc.DocumentType}\n💾  {FormatFileSize(doc.FileSize)}\n" +
            $"📅  {doc.UploadedAt:dd/MM/yyyy HH:mm}\n👤  {doc.Uploader?.FullName ?? "غير معروف"}\n" +
            $"📥  مرات التحميل: {doc.DownloadCount}\n⭐  مفضل: {(doc.IsFavorite ? "نعم" : "لا")}",
            "خصائص الملف", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task SearchDocumentsAsync()
    {
        if (string.IsNullOrWhiteSpace(_txtDocumentSearch.Text)) return;
        try
        {
            this.Cursor = Cursors.WaitCursor;
            _lvDocuments.Items.Clear();
            foreach (var doc in await _fileService.SearchDocumentsAsync(_txtDocumentSearch.Text))
            {
                var item = new ListViewItem(doc.OriginalFileName) { Tag = doc, ImageIndex = (int)doc.DocumentType };
                item.SubItems.Add(doc.DocumentType.ToString()); item.SubItems.Add(FormatFileSize(doc.FileSize));
                item.SubItems.Add(doc.UploadedAt.ToString("dd/MM/yyyy HH:mm")); item.SubItems.Add(doc.Uploader?.FullName ?? "غير معروف");
                if (doc.IsFavorite) item.BackColor = Color.FromArgb(255, 248, 225);
                _lvDocuments.Items.Add(item);
            }
        }
        catch (Exception ex) { MessageBox.Show($"خطأ:\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { this.Cursor = Cursors.Default; }
    }

    private DocumentType GetDocumentType(string ext) => ext.ToLower() switch
    {
        ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => DocumentType.Image,
        ".pdf" => DocumentType.PDF,
        ".xlsx" or ".xls" => DocumentType.Excel,
        ".doc" or ".docx" => DocumentType.Document,
        _ => DocumentType.Other
    };

    private string GetMimeType(string ext) => ext.ToLower() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg", ".png" => "image/png", ".gif" => "image/gif",
        ".pdf" => "application/pdf",
        ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        ".xls" => "application/vnd.ms-excel",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".doc" => "application/msword",
        _ => "application/octet-stream"
    };

    private string FormatFileSize(long bytes)
    {
        string[] s = { "B", "KB", "MB", "GB" };
        double v = bytes; int i = 0;
        while (v >= 1024 && i < s.Length - 1) { v /= 1024; i++; }
        return $"{v:0.##} {s[i]}";
    }
}
