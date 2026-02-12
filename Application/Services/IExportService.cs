using System.Data;

namespace GraceWay.AccountingSystem.Application.Services;

/// <summary>
/// خدمة التصدير للتقارير
/// </summary>
public interface IExportService
{
    /// <summary>
    /// تصدير DataGridView إلى Excel
    /// </summary>
    Task<bool> ExportToExcelAsync(DataGridView dataGridView, string fileName, string sheetName = "Sheet1");
    
    /// <summary>
    /// تصدير DataTable إلى Excel
    /// </summary>
    Task<bool> ExportToExcelAsync(DataTable dataTable, string fileName, string sheetName = "Sheet1");
    
    /// <summary>
    /// تصدير DataGridView إلى PDF
    /// </summary>
    Task<bool> ExportToPdfAsync(DataGridView dataGridView, string fileName, string title, 
        Dictionary<string, string>? metadata = null);
    
    /// <summary>
    /// تصدير نص HTML إلى PDF
    /// </summary>
    Task<bool> ExportHtmlToPdfAsync(string html, string fileName);
    
    /// <summary>
    /// إنشاء HTML من DataGridView
    /// </summary>
    string CreateHtmlFromDataGridView(DataGridView dataGridView, string title, 
        Dictionary<string, string>? metadata = null);
}
