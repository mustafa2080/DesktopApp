using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Infrastructure.Data;

namespace GraceWay.AccountingSystem.Presentation.Forms;

/// <summary>
/// Form for editing existing purchase invoices
/// Inherits from AddPurchaseInvoiceForm and uses its edit mode functionality
/// </summary>
public class EditPurchaseInvoiceForm : AddPurchaseInvoiceForm
{
    public EditPurchaseInvoiceForm(IInvoiceService invoiceService, ICashBoxService cashBoxService,
        AppDbContext context, int currentUserId, int invoiceId)
        : base(invoiceService, cashBoxService, context, currentUserId, invoiceId)
    {
        // Constructor automatically calls LoadInvoiceForEditAsync through parent
    }
}
