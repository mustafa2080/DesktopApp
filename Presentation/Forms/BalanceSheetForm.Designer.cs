namespace GraceWay.AccountingSystem.Presentation.Forms;

partial class BalanceSheetForm
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

    private void InitializeComponent()
    {
        this.SuspendLayout();
        // 
        // BalanceSheetForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1400, 900);
        this.Name = "BalanceSheetForm";
        this.Text = "الميزانية العمومية";
        this.ResumeLayout(false);
    }
}
