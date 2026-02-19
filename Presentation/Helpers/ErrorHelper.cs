using GraceWay.AccountingSystem.Infrastructure.Logging;

namespace GraceWay.AccountingSystem.Presentation.Helpers;

/// <summary>
/// Helper موحد لعرض رسائل الخطأ للمستخدم بدون كشف Stack Trace
/// </summary>
public static class ErrorHelper
{
    /// <summary>
    /// عرض خطأ عملية للمستخدم - يُظهر رسالة ودية فقط
    /// </summary>
    public static void ShowError(string operation, Exception ex, string? customMessage = null)
    {
        AppLogger.Error($"Error in {operation}", ex);

        var userMessage = customMessage ?? GetFriendlyMessage(ex);

        MessageBox.Show(
            userMessage,
            "خطأ",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    /// <summary>
    /// عرض خطأ تحذيري (Warning)
    /// </summary>
    public static void ShowWarning(string message, string title = "تنبيه")
    {
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    /// <summary>
    /// تحويل الخطأ التقني إلى رسالة ودية للمستخدم
    /// </summary>
    public static string GetFriendlyMessage(Exception ex)
    {
        // رسائل الـ Business Logic تُعرض مباشرة
        if (ex is InvalidOperationException || ex is ArgumentException || ex is ArgumentNullException)
            return ex.Message;

        // أخطاء قاعدة البيانات
        if (ex.Message.Contains("connection") || ex.Message.Contains("Connection") ||
            ex.Message.Contains("Npgsql") || ex.Message.Contains("PostgreSQL"))
            return "حدث خطأ في الاتصال بقاعدة البيانات. يرجى التحقق من الاتصال والمحاولة مرة أخرى.";

        if (ex.Message.Contains("duplicate") || ex.Message.Contains("unique") || ex.Message.Contains("UniqueConstraint"))
            return "هذه البيانات موجودة مسبقاً. يرجى التحقق من المدخلات.";

        if (ex.Message.Contains("foreign key") || ex.Message.Contains("ForeignKey"))
            return "لا يمكن تنفيذ هذه العملية لوجود بيانات مرتبطة.";

        if (ex.Message.Contains("timeout") || ex.Message.Contains("Timeout"))
            return "انتهت مهلة الاتصال. يرجى المحاولة مرة أخرى.";

        // أخطاء عامة - لا نكشف التفاصيل التقنية
        return "حدث خطأ غير متوقع. يرجى التواصل مع الدعم الفني إذا تكرر الخطأ.";
    }
}
