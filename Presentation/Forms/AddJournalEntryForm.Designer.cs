namespace GraceWay.AccountingSystem.Presentation.Forms;

partial class AddJournalEntryForm
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
        // AddJournalEntryForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1100, 700);
        this.Name = "AddJournalEntryForm";
        this.Text = "إضافة قيد يومي";
        this.ResumeLayout(false);
    }
}
