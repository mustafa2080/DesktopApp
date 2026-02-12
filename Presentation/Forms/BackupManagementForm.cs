using GraceWay.AccountingSystem.Application.Services.Backup;
using GraceWay.AccountingSystem.Domain.Entities;
using System.Windows.Forms;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class BackupManagementForm : Form
{
    private readonly IBackupService _backupService;
    private DataGridView dgvBackups;
    private Button btnCreateBackup;
    private Button btnRestoreBackup;
    private Button btnDeleteBackup;
    private Button btnRefresh;
    private Label lblBackupPath;
    private TextBox txtBackupPath;
    private Button btnBrowse;
    private GroupBox grpBackupList;
    private GroupBox grpActions;

    public BackupManagementForm(IBackupService backupService)
    {
        _backupService = backupService;
        InitializeComponent();
        LoadBackupHistory();
    }

    private void InitializeComponent()
    {
        this.Text = "إدارة النسخ الاحتياطية";
        this.Size = new Size(1000, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;

        // مجموعة الإجراءات
        grpActions = new GroupBox
        {
            Text = "الإجراءات",
            Location = new Point(20, 20),
            Size = new Size(940, 120)
        };

        lblBackupPath = new Label
        {
            Text = "مسار حفظ النسخ الاحتياطية:",
            Location = new Point(720, 30),
            Size = new Size(200, 20),
            TextAlign = ContentAlignment.MiddleRight
        };

        txtBackupPath = new TextBox
        {
            Location = new Point(220, 30),
            Size = new Size(480, 25),
            ReadOnly = true,
            BackColor = Color.White
        };

        btnBrowse = new Button
        {
            Text = "تصفح...",
            Location = new Point(120, 28),
            Size = new Size(80, 30),
            BackColor = ColorScheme.Secondary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnBrowse.FlatAppearance.BorderSize = 0;
        btnBrowse.Click += BtnBrowse_Click;

        btnCreateBackup = new Button
        {
            Text = "إنشاء نسخة احتياطية",
            Location = new Point(720, 75),
            Size = new Size(200, 35),
            BackColor = ColorScheme.Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        btnCreateBackup.FlatAppearance.BorderSize = 0;
        btnCreateBackup.Click += BtnCreateBackup_Click;

        btnRestoreBackup = new Button
        {
            Text = "استعادة نسخة احتياطية",
            Location = new Point(500, 75),
            Size = new Size(200, 35),
            BackColor = ColorScheme.Warning,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        btnRestoreBackup.FlatAppearance.BorderSize = 0;
        btnRestoreBackup.Click += BtnRestoreBackup_Click;

        btnDeleteBackup = new Button
        {
            Text = "حذف النسخة المحددة",
            Location = new Point(280, 75),
            Size = new Size(200, 35),
            BackColor = ColorScheme.Danger,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        btnDeleteBackup.FlatAppearance.BorderSize = 0;
        btnDeleteBackup.Click += BtnDeleteBackup_Click;

        btnRefresh = new Button
        {
            Text = "تحديث",
            Location = new Point(120, 75),
            Size = new Size(140, 35),
            BackColor = ColorScheme.Secondary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        btnRefresh.FlatAppearance.BorderSize = 0;
        btnRefresh.Click += BtnRefresh_Click;

        grpActions.Controls.AddRange(new Control[] {
            lblBackupPath, txtBackupPath, btnBrowse,
            btnCreateBackup, btnRestoreBackup, btnDeleteBackup, btnRefresh
        });

        // مجموعة قائمة النسخ الاحتياطية
        grpBackupList = new GroupBox
        {
            Text = "النسخ الاحتياطية المتاحة",
            Location = new Point(20, 150),
            Size = new Size(940, 390)
        };

        dgvBackups = new DataGridView
        {
            Location = new Point(10, 25),
            Size = new Size(920, 350),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = ColorScheme.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = ColorScheme.TextPrimary,
                Font = new Font("Segoe UI", 9),
                SelectionBackColor = ColorScheme.Primary,
                SelectionForeColor = Color.White,
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        };

        grpBackupList.Controls.Add(dgvBackups);

        this.Controls.AddRange(new Control[] { grpActions, grpBackupList });

        // تعيين المسار الافتراضي
        txtBackupPath.Text = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "GraceWay Accounting",
            "Backups"
        );
    }

    private async void LoadBackupHistory()
    {
        try
        {
            var backups = await _backupService.GetBackupHistoryAsync();

            dgvBackups.DataSource = backups.Select(b => new
            {
                b.BackupId,
                اسم_الملف = b.BackupFileName,
                تاريخ_النسخة = b.BackupDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                حجم_الملف = b.FileSizeFormatted,
                الوصف = b.Description,
                نوع_النسخة = b.IsAutoBackup ? "تلقائية" : "يدوية",
                المسار = b.BackupFilePath,
                أنشئت_بواسطة = b.CreatedBy ?? "غير محدد"
            }).ToList();

            // إخفاء عمود BackupId
            if (dgvBackups.Columns["BackupId"] != null)
                dgvBackups.Columns["BackupId"]!.Visible = false;

            // تنسيق العرض
            if (dgvBackups.Columns["المسار"] != null)
                dgvBackups.Columns["المسار"]!.Width = 300;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"حدث خطأ أثناء تحميل قائمة النسخ الاحتياطية:\n{ex.Message}",
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void BtnBrowse_Click(object? sender, EventArgs e)
    {
        using var folderDialog = new FolderBrowserDialog
        {
            Description = "اختر مجلد حفظ النسخ الاحتياطية",
            SelectedPath = txtBackupPath.Text
        };

        if (folderDialog.ShowDialog() == DialogResult.OK)
        {
            txtBackupPath.Text = folderDialog.SelectedPath;
        }
    }

    private async void BtnCreateBackup_Click(object? sender, EventArgs e)
    {
        try
        {
            var result = MessageBox.Show(
                "هل أنت متأكد من إنشاء نسخة احتياطية؟\n\nقد تستغرق هذه العملية بضع دقائق حسب حجم قاعدة البيانات.",
                "تأكيد",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes)
                return;

            btnCreateBackup.Enabled = false;
            btnCreateBackup.Text = "جاري إنشاء النسخة...";
            this.Cursor = Cursors.WaitCursor;

            var backupResult = await _backupService.CreateBackupAsync(
                txtBackupPath.Text,
                "نسخة احتياطية يدوية"
            );

            if (backupResult.Success)
            {
                MessageBox.Show(
                    $"تم إنشاء النسخة الاحتياطية بنجاح!\n\nالمسار: {backupResult.BackupFilePath}\nالحجم: {FormatFileSize(backupResult.FileSizeBytes)}",
                    "نجح",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                LoadBackupHistory();
            }
            else
            {
                MessageBox.Show(
                    backupResult.Message,
                    "فشل",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"حدث خطأ أثناء إنشاء النسخة الاحتياطية:\n{ex.Message}",
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
        finally
        {
            btnCreateBackup.Enabled = true;
            btnCreateBackup.Text = "إنشاء نسخة احتياطية";
            this.Cursor = Cursors.Default;
        }
    }

    private async void BtnRestoreBackup_Click(object? sender, EventArgs e)
    {
        try
        {
            using var openDialog = new OpenFileDialog
            {
                Title = "اختر ملف النسخة الاحتياطية",
                Filter = "ملفات النسخ الاحتياطي (*.backup;*.dump;*.sql)|*.backup;*.dump;*.sql|جميع الملفات (*.*)|*.*",
                InitialDirectory = txtBackupPath.Text
            };

            if (openDialog.ShowDialog() != DialogResult.OK)
                return;

            // التحقق من صحة الملف
            var isValid = await _backupService.ValidateBackupFileAsync(openDialog.FileName);
            if (!isValid)
            {
                MessageBox.Show(
                    "الملف المحدد غير صالح أو تالف!",
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            var result = MessageBox.Show(
                "⚠️ تحذير: سيتم استبدال جميع البيانات الحالية بالبيانات من النسخة الاحتياطية!\n\n" +
                "هل أنت متأكد من المتابعة؟\n\n" +
                "نوصي بإنشاء نسخة احتياطية من البيانات الحالية قبل الاستعادة.",
                "تأكيد الاستعادة",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result != DialogResult.Yes)
                return;

            btnRestoreBackup.Enabled = false;
            btnRestoreBackup.Text = "جاري الاستعادة...";
            this.Cursor = Cursors.WaitCursor;

            var restoreResult = await _backupService.RestoreBackupAsync(openDialog.FileName);

            if (restoreResult.Success)
            {
                MessageBox.Show(
                    "تم استعادة قاعدة البيانات بنجاح!\n\nسيتم إعادة تشغيل التطبيق الآن.",
                    "نجح",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                // إعادة تشغيل التطبيق
                System.Diagnostics.Process.Start(System.Windows.Forms.Application.ExecutablePath);
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                MessageBox.Show(
                    restoreResult.Message,
                    "فشل",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"حدث خطأ أثناء استعادة النسخة الاحتياطية:\n{ex.Message}",
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
        finally
        {
            btnRestoreBackup.Enabled = true;
            btnRestoreBackup.Text = "استعادة نسخة احتياطية";
            this.Cursor = Cursors.Default;
        }
    }

    private async void BtnDeleteBackup_Click(object? sender, EventArgs e)
    {
        try
        {
            if (dgvBackups.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "يرجى اختيار نسخة احتياطية للحذف",
                    "تنبيه",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            var backupId = (int)dgvBackups.SelectedRows[0].Cells["BackupId"].Value!;
            var backupName = dgvBackups.SelectedRows[0].Cells["اسم_الملف"].Value?.ToString() ?? "غير معروف";

            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف النسخة الاحتياطية:\n\n{backupName}\n\nلن تتمكن من التراجع عن هذا الإجراء!",
                "تأكيد الحذف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result != DialogResult.Yes)
                return;

            var deleted = await _backupService.DeleteBackupAsync(backupId);

            if (deleted)
            {
                MessageBox.Show(
                    "تم حذف النسخة الاحتياطية بنجاح",
                    "نجح",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                LoadBackupHistory();
            }
            else
            {
                MessageBox.Show(
                    "فشل في حذف النسخة الاحتياطية",
                    "فشل",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"حدث خطأ أثناء حذف النسخة الاحتياطية:\n{ex.Message}",
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void BtnRefresh_Click(object? sender, EventArgs e)
    {
        LoadBackupHistory();
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
