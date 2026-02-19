namespace GraceWay.AccountingSystem.Presentation.Helpers;

/// <summary>
/// Helper موحد للتحقق من صحة المدخلات في الفورمات
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// التحقق من أن المبلغ موجب وصالح
    /// </summary>
    public static bool ValidateAmount(TextBox amountBox, string fieldName = "المبلغ")
    {
        var text = amountBox.Text.Trim();

        if (string.IsNullOrEmpty(text))
        {
            ShowValidationError(amountBox, $"يرجى إدخال {fieldName}");
            return false;
        }

        if (!decimal.TryParse(text, out decimal amount))
        {
            ShowValidationError(amountBox, $"{fieldName} يجب أن يكون رقماً صحيحاً");
            return false;
        }

        if (amount <= 0)
        {
            ShowValidationError(amountBox, $"{fieldName} يجب أن يكون أكبر من الصفر");
            return false;
        }

        if (amount > 999_999_999)
        {
            ShowValidationError(amountBox, $"{fieldName} يتجاوز الحد الأقصى المسموح به");
            return false;
        }

        ClearValidationError(amountBox);
        return true;
    }

    /// <summary>
    /// التحقق من حقل نصي إلزامي
    /// </summary>
    public static bool ValidateRequired(TextBox textBox, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(textBox.Text))
        {
            ShowValidationError(textBox, $"يرجى إدخال {fieldName}");
            return false;
        }

        ClearValidationError(textBox);
        return true;
    }

    /// <summary>
    /// التحقق من ComboBox - يجب أن يكون هناك اختيار
    /// </summary>
    public static bool ValidateComboBox(ComboBox combo, string fieldName)
    {
        if (combo.SelectedIndex < 0 || combo.SelectedItem == null)
        {
            ShowValidationError(combo, $"يرجى اختيار {fieldName}");
            return false;
        }

        ClearValidationError(combo);
        return true;
    }

    /// <summary>
    /// التحقق من نطاق التاريخ - تاريخ النهاية لا يسبق تاريخ البداية
    /// </summary>
    public static bool ValidateDateRange(DateTimePicker startPicker, DateTimePicker endPicker,
        string startName = "تاريخ البداية", string endName = "تاريخ النهاية")
    {
        if (endPicker.Value.Date < startPicker.Value.Date)
        {
            MessageBox.Show(
                $"{endName} لا يمكن أن يكون قبل {startName}",
                "خطأ في التاريخ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            endPicker.Focus();
            return false;
        }

        return true;
    }

    /// <summary>
    /// التحقق من نطاق التاريخ عند استخدام قيم DateTime مباشرة
    /// </summary>
    public static bool ValidateDateRange(DateTime startDate, DateTime endDate,
        string startName = "تاريخ البداية", string endName = "تاريخ النهاية")
    {
        if (endDate.Date < startDate.Date)
        {
            MessageBox.Show(
                $"{endName} لا يمكن أن يكون قبل {startName}",
                "خطأ في التاريخ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        return true;
    }

    /// <summary>
    /// التحقق من حقل رقمي عام
    /// </summary>
    public static bool ValidateNumeric(TextBox textBox, string fieldName,
        decimal min = 0, decimal max = decimal.MaxValue, bool required = true)
    {
        var text = textBox.Text.Trim();

        if (string.IsNullOrEmpty(text))
        {
            if (required)
            {
                ShowValidationError(textBox, $"يرجى إدخال {fieldName}");
                return false;
            }
            return true;
        }

        if (!decimal.TryParse(text, out decimal value))
        {
            ShowValidationError(textBox, $"{fieldName} يجب أن يكون رقماً");
            return false;
        }

        if (value < min)
        {
            ShowValidationError(textBox, $"{fieldName} لا يمكن أن يكون أقل من {min}");
            return false;
        }

        if (value > max)
        {
            ShowValidationError(textBox, $"{fieldName} لا يمكن أن يتجاوز {max}");
            return false;
        }

        ClearValidationError(textBox);
        return true;
    }

    private static void ShowValidationError(Control control, string message)
    {
        MessageBox.Show(message, "بيانات غير صحيحة", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        control.Focus();
        if (control is TextBox tb)
            tb.SelectAll();
    }

    private static void ClearValidationError(Control control)
    {
        // يمكن تمديد هذا لاحقاً لإزالة Border حمراء أو ErrorProvider
    }
}
