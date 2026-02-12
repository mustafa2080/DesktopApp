using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Presentation.Forms.Reports;

/// <summary>
/// 📊 تقرير حالة المدفوعات والحجوزات للعمرة
/// </summary>
public partial class UmrahPaymentStatusForm : Form
{
    private readonly IUmrahService _umrahService;
    private DataGridView _statusGrid;
    private Panel _summaryPanel = null!;
    private List<UmrahPackage> _allPackages = new();
    
    public UmrahPaymentStatusForm(IUmrahService umrahService)
    {
        _umrahService = umrahService;
        InitializeComponent();
        SetupForm();
        InitializeCustomControls();
        _ = LoadDataAsync();
    }
    
    private void SetupForm()
    {
        this.Text = "📊 حالة مدفوعات العمرة";
        this.Size = new Size(1400, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
    }
    
    private void InitializeCustomControls()
    {
        // Header
        Panel headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 100,
            BackColor = Color.White,
            Padding = new Padding(30)
        };
        
        Label titleLabel = new Label
        {
            Text = "💳 حالة المدفوعات والحجوزات",
            Font = new Font("Cairo", 16F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true
        };
        headerPanel.Controls.Add(titleLabel);
        this.Controls.Add(headerPanel);
        
        // Summary Panel
        _summaryPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 120,
            BackColor = ColorScheme.Background,
            Padding = new Padding(20)
        };
        
        FlowLayoutPanel cards = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };
        
        cards.Controls.Add(CreateCard("✅", "مدفوع بالكامل", "0", Color.FromArgb(76, 175, 80)));
        cards.Controls.Add(CreateCard("⏳", "دفعة جزئية", "0", Color.FromArgb(255, 152, 0)));
        cards.Controls.Add(CreateCard("❌", "غير مدفوع", "0", Color.FromArgb(244, 67, 54)));
        
        _summaryPanel.Controls.Add(cards);
        this.Controls.Add(_summaryPanel);
    }
    
    private Panel CreateCard(string icon, string title, string value, Color color)
    {
        Panel card = new Panel
        {
            Size = new Size(250, 80),
            BackColor = Color.White,
            Margin = new Padding(10)
        };
        
        Label iconLabel = new Label
        {
            Text = icon,
            Font = new Font("Segoe UI Emoji", 20F),
            Location = new Point(10, 15),
            ForeColor = color
        };
        card.Controls.Add(iconLabel);
        
        Label titleLabel = new Label
        {
            Text = title,
            Font = new Font("Cairo", 9F),
            Location = new Point(60, 15),
            ForeColor = Color.Gray
        };
        card.Controls.Add(titleLabel);
        
        Label valueLabel = new Label
        {
            Text = value,
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            Location = new Point(60, 40),
            ForeColor = color
        };
        card.Controls.Add(valueLabel);
        
        return card;
    }
    
    private async Task LoadDataAsync()
    {
        try
        {
            _allPackages = await _umrahService.GetAllPackagesAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
