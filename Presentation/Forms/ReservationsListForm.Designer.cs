namespace GraceWay.AccountingSystem.Presentation.Forms
{
    partial class ReservationsListForm
    {
        private System.ComponentModel.IContainer components = null;
        
        // Controls
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ComboBox cmbStatusFilter;
        private System.Windows.Forms.DataGridView dgvTrips;
        private System.Windows.Forms.Button btnViewDetails;
        private System.Windows.Forms.Button btnLockTrip;
        private System.Windows.Forms.Button btnUnlockTrip;
        private System.Windows.Forms.Button btnConfirmTrip;
        private System.Windows.Forms.Button btnCancelTrip;
        private System.Windows.Forms.Button btnRefresh;

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
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1500, 850);
            this.Text = "قسم الحجوزات - إدارة حسابات الرحلات";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        }
    }
}
