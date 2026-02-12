using ClosedXML.Excel;

namespace GraceWay.AccountingSystem.Presentation.Forms;

public partial class SuppliersListForm
{
    private void ExportToExcel_Click_NEW(object? sender, EventArgs e)
    {
        try
        {
            if (_allSuppliers == null || _allSuppliers.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات للتصدير", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the currently filtered suppliers from the grid
            var suppliersToExport = new List<Domain.Entities.Supplier>();
            foreach (DataGridViewRow gridRow in _suppliersGrid.Rows)
            {
                if (gridRow.Tag is Domain.Entities.Supplier supplier)
                {
                    suppliersToExport.Add(supplier);
                }
            }

            if (suppliersToExport.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات مفلترة للتصدير", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Create SaveFileDialog
            using var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "حفظ ملف Excel",
                FileName = $"الموردين_{DateTime.Now:yyyy-MM-dd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;

            // Create Excel workbook
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("الموردين");

            // Set RTL direction
            worksheet.RightToLeft = true;

            // Add headers
            worksheet.Cell(1, 1).Value = "كود المورد";
            worksheet.Cell(1, 2).Value = "اسم المورد";
            worksheet.Cell(1, 3).Value = "الاسم بالإنجليزية";
            worksheet.Cell(1, 4).Value = "التليفون";
            worksheet.Cell(1, 5).Value = "الموبايل";
            worksheet.Cell(1, 6).Value = "البريد الإلكتروني";
            worksheet.Cell(1, 7).Value = "العنوان";
            worksheet.Cell(1, 8).Value = "المدينة";
            worksheet.Cell(1, 9).Value = "الدولة";
            worksheet.Cell(1, 10).Value = "الرقم الضريبي";
            worksheet.Cell(1, 11).Value = "حد الائتمان";
            worksheet.Cell(1, 12).Value = "مدة السداد (يوم)";
            worksheet.Cell(1, 13).Value = "الرصيد الافتتاحي";
            worksheet.Cell(1, 14).Value = "الرصيد الحالي";
            worksheet.Cell(1, 15).Value = "الحالة";
            worksheet.Cell(1, 16).Value = "ملاحظات";

            // Style header row
            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Font.FontSize = 12;
            headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(41, 128, 185);
            headerRow.Style.Font.FontColor = XLColor.White;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRow.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRow.Height = 30;

            // Add data rows
            int row = 2;
            foreach (var supplier in suppliersToExport)
            {
                worksheet.Cell(row, 1).Value = supplier.SupplierCode;
                worksheet.Cell(row, 2).Value = supplier.SupplierName;
                worksheet.Cell(row, 3).Value = supplier.SupplierNameEn;
                worksheet.Cell(row, 4).Value = supplier.Phone ?? "";
                worksheet.Cell(row, 5).Value = supplier.Mobile ?? "";
                worksheet.Cell(row, 6).Value = supplier.Email ?? "";
                worksheet.Cell(row, 7).Value = supplier.Address ?? "";
                worksheet.Cell(row, 8).Value = supplier.City ?? "";
                worksheet.Cell(row, 9).Value = supplier.Country ?? "";
                worksheet.Cell(row, 10).Value = supplier.TaxNumber ?? "";
                worksheet.Cell(row, 11).Value = supplier.CreditLimit;
                worksheet.Cell(row, 12).Value = supplier.PaymentTermDays;
                worksheet.Cell(row, 13).Value = supplier.OpeningBalance;
                worksheet.Cell(row, 14).Value = supplier.CurrentBalance;
                worksheet.Cell(row, 15).Value = supplier.IsActive ? "نشط" : "غير نشط";
                worksheet.Cell(row, 16).Value = supplier.Notes ?? "";

                // Format currency columns
                worksheet.Cell(row, 11).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(row, 13).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(row, 14).Style.NumberFormat.Format = "#,##0.00";

                // Color code the current balance
                if (supplier.CurrentBalance > 0)
                {
                    worksheet.Cell(row, 14).Style.Font.FontColor = XLColor.Green;
                    worksheet.Cell(row, 14).Style.Font.Bold = true;
                }
                else if (supplier.CurrentBalance < 0)
                {
                    worksheet.Cell(row, 14).Style.Font.FontColor = XLColor.Red;
                    worksheet.Cell(row, 14).Style.Font.Bold = true;
                }

                // Color code status
                if (supplier.IsActive)
                {
                    worksheet.Cell(row, 15).Style.Font.FontColor = XLColor.Green;
                    worksheet.Cell(row, 15).Style.Font.Bold = true;
                }
                else
                {
                    worksheet.Cell(row, 15).Style.Font.FontColor = XLColor.Red;
                }

                // Alternating row colors
                if (row % 2 == 0)
                {
                    worksheet.Range(row, 1, row, 16).Style.Fill.BackgroundColor = XLColor.FromArgb(240, 248, 255);
                }

                row++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Add borders to all cells
            var dataRange = worksheet.Range(1, 1, row - 1, 16);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Add summary row
            row++;
            worksheet.Cell(row, 1).Value = "الإجمالي:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 1).Style.Font.FontSize = 12;
            worksheet.Cell(row, 2).Value = $"{suppliersToExport.Count} مورد";
            worksheet.Cell(row, 2).Style.Font.Bold = true;

            var totalBalance = suppliersToExport.Sum(s => s.CurrentBalance);
            worksheet.Cell(row, 13).Value = "إجمالي الرصيد:";
            worksheet.Cell(row, 13).Style.Font.Bold = true;
            worksheet.Cell(row, 14).Value = totalBalance;
            worksheet.Cell(row, 14).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 14).Style.Font.Bold = true;
            worksheet.Cell(row, 14).Style.Font.FontSize = 12;

            if (totalBalance > 0)
                worksheet.Cell(row, 14).Style.Font.FontColor = XLColor.Green;
            else if (totalBalance < 0)
                worksheet.Cell(row, 14).Style.Font.FontColor = XLColor.Red;

            // Highlight summary row
            worksheet.Range(row, 1, row, 16).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 200);

            // Save the workbook
            workbook.SaveAs(saveDialog.FileName);

            MessageBox.Show(
                $"✅ تم تصدير {suppliersToExport.Count} مورد بنجاح!\n\nالملف: {Path.GetFileName(saveDialog.FileName)}",
                "نجاح التصدير",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            // Ask if user wants to open the file
            var openResult = MessageBox.Show(
                "هل تريد فتح الملف الآن؟",
                "فتح الملف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (openResult == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = saveDialog.FileName,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"❌ خطأ في التصدير:\n\n{ex.Message}",
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
