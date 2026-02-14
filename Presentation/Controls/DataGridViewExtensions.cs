using System.Windows.Forms;

namespace GraceWay.AccountingSystem.Presentation.Controls;

/// <summary>
/// Extensions لتحسين عمل DataGridView ومنع أخطاء الترتيب
/// </summary>
public static class DataGridViewExtensions
{
    /// <summary>
    /// إضافة معالجات الأخطاء والترتيب لـ DataGridView لمنع الأخطاء عند الترتيب
    /// </summary>
    public static void AddSortErrorHandlers(this DataGridView grid)
    {
        // معالج أخطاء البيانات لمنع ظهور رسائل الخطأ
        grid.DataError += (s, e) =>
        {
            // منع ظهور رسالة الخطأ
            e.ThrowException = false;
        };
        
        // معالج الترتيب لتحويل القيم الفارغة أو غير المتوافقة
        grid.SortCompare += (s, e) =>
        {
            var val1 = e.CellValue1?.ToString() ?? "";
            var val2 = e.CellValue2?.ToString() ?? "";
            e.SortResult = string.Compare(val1, val2);
            e.Handled = true;
        };
    }
}
