using ClosedXML.Excel;
using System.Data;
using System.Text;

namespace GraceWay.AccountingSystem.Application.Services;

public class ExportService : IExportService
{
    public async Task<bool> ExportToExcelAsync(DataGridView dataGridView, string fileName, string sheetName = "Sheet1")
    {
        try
        {
            // Convert DataGridView to DataTable
            DataTable dataTable = new DataTable();
            
            // Add columns
            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                dataTable.Columns.Add(column.HeaderText);
            }
            
            // Add rows
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (!row.IsNewRow)
                {
                    DataRow dataRow = dataTable.NewRow();
                    for (int i = 0; i < dataGridView.Columns.Count; i++)
                    {
                        dataRow[i] = row.Cells[i].Value ?? string.Empty;
                    }
                    dataTable.Rows.Add(dataRow);
                }
            }
            
            return await ExportToExcelAsync(dataTable, fileName, sheetName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في التصدير: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            return false;
        }
    }

    public async Task<bool> ExportToExcelAsync(DataTable dataTable, string fileName, string sheetName = "Sheet1")
    {
        return await Task.Run(() =>
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add(sheetName);
                    
                    // Add header row
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = dataTable.Columns[i].ColumnName;
                        worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(25, 118, 210);
                        worksheet.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(1, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    
                    // Add data rows
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        for (int j = 0; j < dataTable.Columns.Count; j++)
                        {
                            var cellValue = dataTable.Rows[i][j];
                            var cell = worksheet.Cell(i + 2, j + 1);
                            
                            // Handle different data types
                            if (cellValue is decimal || cellValue is double || cellValue is float)
                            {
                                cell.Value = Convert.ToDouble(cellValue);
                                cell.Style.NumberFormat.Format = "#,##0.00";
                            }
                            else if (cellValue is int || cellValue is long)
                            {
                                cell.Value = Convert.ToInt64(cellValue);
                            }
                            else if (cellValue is DateTime)
                            {
                                cell.Value = (DateTime)cellValue;
                                cell.Style.DateFormat.Format = "yyyy/MM/dd";
                            }
                            else
                            {
                                cell.Value = cellValue?.ToString() ?? string.Empty;
                            }
                        }
                    }
                    
                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();
                    
                    // Ensure file has .xlsx extension
                    if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    {
                        fileName += ".xlsx";
                    }
                    
                    workbook.SaveAs(fileName);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ ملف Excel: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                return false;
            }
        });
    }

    public async Task<bool> ExportToPdfAsync(DataGridView dataGridView, string fileName, string title,
        Dictionary<string, string>? metadata = null)
    {
        try
        {
            string html = CreateHtmlFromDataGridView(dataGridView, title, metadata);
            return await ExportHtmlToPdfAsync(html, fileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطأ في التصدير إلى PDF: {ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            return false;
        }
    }

    public async Task<bool> ExportHtmlToPdfAsync(string html, string fileName)
    {
        return await Task.Run(() =>
        {
            try
            {
                // For now, save as HTML
                // In future: use iTextSharp or similar library for proper PDF generation
                string htmlFileName = fileName.Replace(".pdf", ".html");
                File.WriteAllText(htmlFileName, html, Encoding.UTF8);
                
                MessageBox.Show($"تم حفظ الملف بصيغة HTML:{Environment.NewLine}{htmlFileName}{Environment.NewLine}{Environment.NewLine}يمكنك طباعته كـ PDF من المتصفح", 
                    "نجح الحفظ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                
                // Open in browser
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = htmlFileName,
                    UseShellExecute = true
                });
                
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ PDF: {ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                return false;
            }
        });
    }

    public string CreateHtmlFromDataGridView(DataGridView dataGridView, string title,
        Dictionary<string, string>? metadata = null)
    {
        StringBuilder html = new StringBuilder();
        
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html dir='rtl' lang='ar'>");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset='UTF-8'>");
        html.AppendLine($"    <title>{title}</title>");
        html.AppendLine("    <style>");
        html.AppendLine("        @import url('https://fonts.googleapis.com/css2?family=Cairo:wght@400;700&display=swap');");
        html.AppendLine("        body { font-family: 'Cairo', Arial, sans-serif; margin: 20px; direction: rtl; }");
        html.AppendLine("        h1 { color: #1976D2; text-align: center; margin-bottom: 10px; }");
        html.AppendLine("        .metadata { text-align: center; color: #666; margin-bottom: 20px; font-size: 14px; }");
        html.AppendLine("        table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
        html.AppendLine("        th { background-color: #1976D2; color: white; padding: 12px; text-align: center; font-weight: bold; }");
        html.AppendLine("        td { padding: 10px; text-align: center; border: 1px solid #ddd; }");
        html.AppendLine("        tr:nth-child(even) { background-color: #f8f8f8; }");
        html.AppendLine("        tr:hover { background-color: #e3f2fd; }");
        html.AppendLine("        .total-row { background-color: #c8e6c9 !important; font-weight: bold; }");
        html.AppendLine("        .header-row { background-color: #1976D2 !important; color: white !important; font-weight: bold; }");
        html.AppendLine("        .footer { text-align: center; margin-top: 30px; color: #999; font-size: 12px; }");
        html.AppendLine("        @media print { body { margin: 0; } }");
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        
        // Title
        html.AppendLine($"    <h1>{title}</h1>");
        
        // Metadata
        if (metadata != null && metadata.Count > 0)
        {
            html.AppendLine("    <div class='metadata'>");
            foreach (var item in metadata)
            {
                html.AppendLine($"        <p>{item.Key}: {item.Value}</p>");
            }
            html.AppendLine("    </div>");
        }
        
        // Table
        html.AppendLine("    <table>");
        html.AppendLine("        <thead>");
        html.AppendLine("            <tr>");
        foreach (DataGridViewColumn column in dataGridView.Columns)
        {
            html.AppendLine($"                <th>{column.HeaderText}</th>");
        }
        html.AppendLine("            </tr>");
        html.AppendLine("        </thead>");
        html.AppendLine("        <tbody>");
        
        foreach (DataGridViewRow row in dataGridView.Rows)
        {
            if (!row.IsNewRow)
            {
                // Check if this is a special row (header, total, etc.)
                string rowClass = "";
                if (row.DefaultCellStyle.BackColor == Color.FromArgb(200, 230, 201) ||
                    row.DefaultCellStyle.BackColor == Color.FromArgb(255, 205, 210))
                {
                    rowClass = " class='total-row'";
                }
                else if (row.DefaultCellStyle.BackColor == Color.FromArgb(25, 118, 210))
                {
                    rowClass = " class='header-row'";
                }
                
                html.AppendLine($"            <tr{rowClass}>");
                foreach (DataGridViewCell cell in row.Cells)
                {
                    string cellValue = cell.Value?.ToString() ?? string.Empty;
                    html.AppendLine($"                <td>{cellValue}</td>");
                }
                html.AppendLine("            </tr>");
            }
        }
        
        html.AppendLine("        </tbody>");
        html.AppendLine("    </table>");
        
        // Footer
        html.AppendLine("    <div class='footer'>");
        html.AppendLine($"        <p>تم الإنشاء في: {DateTime.Now:yyyy/MM/dd HH:mm}</p>");
        html.AppendLine("        <p>نظام جراس واي المحاسبي</p>");
        html.AppendLine("    </div>");
        
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        
        return html.ToString();
    }
}
