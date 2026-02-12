using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class TransactionDetailsForm : Form
{
    private readonly CashTransaction _transaction;
    private string _createdByUserName = "";
    
    public TransactionDetailsForm(CashTransaction transaction)
    {
        _transaction = transaction;
        InitializeComponent();
        SetupForm();
        // لا نحتاج LoadUserData لأن Transaction يحتوي على CreatedBy
        _createdByUserName = "مستخدم رقم " + _transaction.CreatedBy;
        LoadTransactionDetails();
    }
    
    private void SetupForm()
    {
        this.Text = "تفاصيل المعاملة";
        this.Size = new Size(700, 850);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.BackColor = Color.White;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.Font = new Font("Cairo", 10F);
        this.MaximizeBox = false;
        this.MinimizeBox = false;
    }
    
    private void LoadTransactionDetails()
    {
        // Header Panel بسيط
        Panel headerPanel = CreateSimpleHeaderPanel();
        this.Controls.Add(headerPanel);
        
        // Main Content Panel
        Panel contentPanel = CreateSimpleContentPanel();
        this.Controls.Add(contentPanel);
        
        // Close button بسيط
        Button btnClose = CreateSimpleCloseButton();
        this.Controls.Add(btnClose);
    }
    
    private Panel CreateSimpleHeaderPanel()
    {
        Panel headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 120,
            BackColor = _transaction.Type == TransactionType.Income 
                ? Color.FromArgb(16, 185, 129) 
                : Color.FromArgb(220, 38, 38)
        };
        
        // Type Label
        Label typeLabel = new Label
        {
            Text = _transaction.Type == TransactionType.Income ? "إيراد 💰" : "مصروف 💸",
            Font = new Font("Cairo", 20F, FontStyle.Bold),
            ForeColor = Color.White,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent
        };
        headerPanel.Controls.Add(typeLabel);
        
        return headerPanel;
    }
    
    private Panel CreateSimpleContentPanel()
    {
        Panel contentPanel = new Panel
        {
            Location = new Point(0, 120),
            Size = new Size(700, 550),
            BackColor = Color.White,
            AutoScroll = true
        };
        
        int yPos = 30;
        
        // المبلغ
        AddSimpleRow(contentPanel, "المبلغ:", $"{_transaction.Amount:N2} جنيه", yPos, true);
        yPos += 60;
        
        // رقم السند - للتأكد من ظهوره
        string voucherNum = string.IsNullOrWhiteSpace(_transaction.VoucherNumber) ? "لا يوجد" : _transaction.VoucherNumber;
        AddSimpleRow(contentPanel, "رقم السند:", voucherNum, yPos);
        yPos += 50;
        
        // التاريخ
        AddSimpleRow(contentPanel, "التاريخ:", _transaction.TransactionDate.ToString("dd/MM/yyyy - hh:mm tt"), yPos);
        yPos += 50;
        
        // التصنيف
        AddSimpleRow(contentPanel, "التصنيف:", _transaction.Category ?? "غير محدد", yPos);
        yPos += 50;
        
        // طريقة الدفع
        string paymentMethod = _transaction.PaymentMethod switch
        {
            PaymentMethod.Cash => "نقدي",
            PaymentMethod.BankTransfer => "تحويل بنكي",
            PaymentMethod.Cheque => "شيك",
            PaymentMethod.CreditCard => "بطاقة ائتمان",
            PaymentMethod.InstaPay => "إنستا باي",
            _ => "آخر"
        };
        AddSimpleRow(contentPanel, "طريقة الدفع:", paymentMethod, yPos);
        yPos += 50;
        
        // الخزنة
        AddSimpleRow(contentPanel, "الخزنة:", _transaction.CashBox?.Name ?? "غير محدد", yPos);
        yPos += 50;
        
        // الطرف
        if (!string.IsNullOrEmpty(_transaction.PartyName))
        {
            AddSimpleRow(contentPanel, "الطرف:", _transaction.PartyName, yPos);
            yPos += 50;
        }
        
        // الوصف
        if (!string.IsNullOrEmpty(_transaction.Description))
        {
            Label lblDescTitle = new Label
            {
                Text = "الوصف:",
                Font = new Font("Cairo", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Location = new Point(530, yPos),
                Size = new Size(140, 30),
                TextAlign = ContentAlignment.MiddleRight
            };
            contentPanel.Controls.Add(lblDescTitle);
            
            TextBox txtDesc = new TextBox
            {
                Text = _transaction.Description,
                Font = new Font("Cairo", 10F),
                ForeColor = Color.FromArgb(40, 40, 40),
                Location = new Point(40, yPos + 35),
                Size = new Size(620, 60),
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Right
            };
            contentPanel.Controls.Add(txtDesc);
            yPos += 110;
        }
        
        // خط فاصل
        Panel separator = new Panel
        {
            Size = new Size(620, 2),
            Location = new Point(40, yPos),
            BackColor = Color.FromArgb(220, 220, 220)
        };
        contentPanel.Controls.Add(separator);
        yPos += 20;
        
        // معلومات التدقيق
        Label lblAudit = new Label
        {
            Text = $"أنشئ بواسطة: {_createdByUserName} | {_transaction.CreatedAt.ToString("dd/MM/yyyy - hh:mm tt")}",
            Font = new Font("Cairo", 9F),
            ForeColor = Color.FromArgb(120, 120, 120),
            Location = new Point(40, yPos),
            Size = new Size(620, 25),
            TextAlign = ContentAlignment.MiddleRight
        };
        contentPanel.Controls.Add(lblAudit);
        
        return contentPanel;
    }
    
    private void AddSimpleRow(Panel parent, string label, string value, int yPos, bool isAmount = false)
    {
        // Label
        Label lbl = new Label
        {
            Text = label,
            Font = new Font("Cairo", isAmount ? 13F : 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(60, 60, 60),
            Location = new Point(530, yPos),
            Size = new Size(140, 30),
            TextAlign = ContentAlignment.MiddleRight
        };
        parent.Controls.Add(lbl);
        
        // Value
        Label val = new Label
        {
            Text = value,
            Font = new Font("Cairo", isAmount ? 22F : 11F, isAmount ? FontStyle.Bold : FontStyle.Regular),
            ForeColor = isAmount 
                ? (_transaction.Type == TransactionType.Income ? Color.FromArgb(16, 185, 129) : Color.FromArgb(220, 38, 38))
                : Color.FromArgb(40, 40, 40),
            Location = new Point(40, yPos),
            Size = new Size(480, isAmount ? 40 : 30),
            TextAlign = ContentAlignment.MiddleRight
        };
        parent.Controls.Add(val);
        
        // خط فاصل
        if (!isAmount)
        {
            Panel separator = new Panel
            {
                Size = new Size(620, 1),
                Location = new Point(40, yPos + 35),
                BackColor = Color.FromArgb(240, 240, 240)
            };
            parent.Controls.Add(separator);
        }
    }
    
    private Button CreateSimpleCloseButton()
    {
        Button btnClose = new Button
        {
            Text = "إغلاق",
            Font = new Font("Cairo", 12F, FontStyle.Bold),
            Size = new Size(150, 45),
            Location = new Point(275, 760),
            BackColor = Color.FromArgb(220, 38, 38),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Click += (s, e) => this.Close();
        
        btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.FromArgb(185, 28, 28);
        btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.FromArgb(220, 38, 38);
        
        return btnClose;
    }
}
