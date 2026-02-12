namespace GraceWay.AccountingSystem.Presentation.Forms
{
    partial class CashBoxExpenseReportForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 900);
            this.MinimumSize = new System.Drawing.Size(1100, 700);
            this.Name = "CashBoxExpenseReportForm";
            this.Text = "تقرير المصروفات - Expense Report";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = System.Drawing.Color.FromArgb(240, 242, 245);
        }

        #endregion
    }
}
