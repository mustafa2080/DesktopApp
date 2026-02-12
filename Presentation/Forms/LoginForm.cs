using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Infrastructure.Data;
using GraceWay.AccountingSystem.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class LoginForm : Form
{
    private readonly IAuthService _authService;
    private int _loginAttempts = 0;
    private const int MaxLoginAttempts = 3;

    public LoginForm(IAuthService authService)
    {
        _authService = authService;
        InitializeComponent();
        
        // Set form properties
        this.KeyPreview = true;
        this.KeyDown += LoginForm_KeyDown;
        
        // Add placeholder text effect
        SetupPlaceholders();
        
        // Add hover effects to button
        SetupButtonEffects();
        
        // Focus on username field after form loads
        this.Load += (s, e) => txtUsername.Focus();
    }

    private void SetupPlaceholders()
    {
        // Username placeholder
        txtUsername.GotFocus += (s, e) =>
        {
            if (txtUsername.ForeColor == System.Drawing.Color.Gray)
            {
                txtUsername.Text = "";
                txtUsername.ForeColor = System.Drawing.Color.Black;
            }
        };

        txtUsername.LostFocus += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                txtUsername.Text = "أدخل اسم المستخدم";
                txtUsername.ForeColor = System.Drawing.Color.Gray;
            }
        };

        // Password placeholder
        txtPassword.GotFocus += (s, e) =>
        {
            if (txtPassword.ForeColor == System.Drawing.Color.Gray)
            {
                txtPassword.Text = "";
                txtPassword.ForeColor = System.Drawing.Color.Black;
                txtPassword.UseSystemPasswordChar = true;
            }
        };

        txtPassword.LostFocus += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                txtPassword.Text = "أدخل كلمة المرور";
                txtPassword.ForeColor = System.Drawing.Color.Gray;
                txtPassword.UseSystemPasswordChar = false;
            }
        };
    }

    private void SetupButtonEffects()
    {
        var originalColor = btnLogin.BackColor;
        var hoverColor = System.Drawing.Color.FromArgb(21, 101, 192);

        btnLogin.MouseEnter += (s, e) =>
        {
            btnLogin.BackColor = hoverColor;
            btnLogin.Cursor = Cursors.Hand;
        };

        btnLogin.MouseLeave += (s, e) =>
        {
            btnLogin.BackColor = originalColor;
        };
    }

    private void LoginForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            btnLogin.PerformClick();
        }
    }

    private async void btnLogin_Click(object sender, EventArgs e)
    {
        // Validate inputs - check actual text content, not just color
        var username = txtUsername.Text?.Trim() ?? string.Empty;
        var password = txtPassword.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username) || username == "أدخل اسم المستخدم")
        {
            ShowError("يرجى إدخال اسم المستخدم");
            txtUsername.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(password) || password == "أدخل كلمة المرور")
        {
            ShowError("يرجى إدخال كلمة المرور");
            txtPassword.Focus();
            return;
        }

        // Disable button and show loading state
        btnLogin.Enabled = false;
        var originalText = btnLogin.Text;
        btnLogin.Text = "جاري التحقق...";
        this.Cursor = Cursors.WaitCursor;

        try
        {
            // Attempt login
            var result = await _authService.LoginAsync(username, password);

            if (result.Success)
            {
                // Successful login
                //ShowSuccess("تم تسجيل الدخول بنجاح");
               // await Task.Delay(500); // Brief delay to show success message
                
                // Open main form
                this.Hide();
                var mainForm = new MainForm(username, result.User!.UserId, Program.ServiceProvider);
                mainForm.FormClosed += (s, args) => this.Close();
                mainForm.Show();
            }
            else
            {
                // Failed login
                _loginAttempts++;
                
                if (_loginAttempts >= MaxLoginAttempts)
                {
                    ShowError($"تم تجاوز الحد الأقصى لمحاولات تسجيل الدخول ({MaxLoginAttempts})\nسيتم إغلاق البرنامج");
                    await Task.Delay(2000);
                    System.Windows.Forms.Application.Exit();
                    return;
                }

                ShowError($"{result.Message}\nالمحاولة {_loginAttempts} من {MaxLoginAttempts}");
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }
        catch (Exception ex)
        {
            ShowError($"حدث خطأ أثناء الاتصال بقاعدة البيانات:\n{ex.Message}");
        }
        finally
        {
            // Restore button state
            btnLogin.Enabled = true;
            btnLogin.Text = originalText;
            this.Cursor = Cursors.Default;
        }
    }

    private void ShowError(string message)
    {
        MessageBox.Show(
            message,
            "خطأ",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button1,
            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading
        );
    }

    private void ShowSuccess(string message)
    {
        MessageBox.Show(
            message,
            "نجاح",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information,
            MessageBoxDefaultButton.Button1,
            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading
        );
    }
}
