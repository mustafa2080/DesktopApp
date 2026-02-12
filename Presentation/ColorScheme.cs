using System.Drawing;

namespace GraceWay.AccountingSystem.Presentation;

/// <summary>
/// نظام الألوان الموحد للتطبيق
/// Unified color scheme for the application
/// </summary>
public static class ColorScheme
{
    // Primary Colors - الألوان الأساسية
    public static readonly Color Primary = Color.FromArgb(41, 128, 185);        // أزرق أساسي
    public static readonly Color PrimaryDark = Color.FromArgb(31, 97, 141);     // أزرق داكن
    public static readonly Color PrimaryLight = Color.FromArgb(133, 193, 233);  // أزرق فاتح

    // Secondary Colors - الألوان الثانوية
    public static readonly Color Secondary = Color.FromArgb(52, 152, 219);      // أزرق ثانوي
    public static readonly Color Accent = Color.FromArgb(231, 76, 60);          // أحمر للتمييز

    // Status Colors - ألوان الحالات
    public static readonly Color Success = Color.FromArgb(39, 174, 96);         // أخضر للنجاح
    public static readonly Color Warning = Color.FromArgb(243, 156, 18);        // برتقالي للتحذير
    public static readonly Color Error = Color.FromArgb(231, 76, 60);           // أحمر للخطأ
    public static readonly Color Info = Color.FromArgb(52, 152, 219);           // أزرق للمعلومات
    public static readonly Color Danger = Color.FromArgb(231, 76, 60);          // أحمر للخطر (alias for Error)


    // Neutral Colors - الألوان المحايدة
    public static readonly Color Background = Color.FromArgb(236, 240, 241);    // خلفية رمادية فاتحة
    public static readonly Color Surface = Color.White;                         // سطح أبيض
    public static readonly Color Border = Color.FromArgb(189, 195, 199);        // حدود رمادية
    public static readonly Color LightGray = Color.FromArgb(240, 240, 240);     // رمادي فاتح

    // Text Colors - ألوان النصوص
    public static readonly Color TextPrimary = Color.FromArgb(44, 62, 80);      // نص أساسي داكن
    public static readonly Color TextSecondary = Color.FromArgb(127, 140, 141); // نص ثانوي رمادي
    public static readonly Color TextLight = Color.White;                       // نص فاتح

    // Sidebar Colors - ألوان الشريط الجانبي
    public static readonly Color SidebarBg = Color.FromArgb(44, 62, 80);            // خلفية داكنة
    public static readonly Color SidebarBackground = Color.FromArgb(44, 62, 80);     // خلفية داكنة (alias)
    public static readonly Color SidebarHover = Color.FromArgb(52, 73, 94);          // عند التمرير
    public static readonly Color SidebarActive = Color.FromArgb(41, 128, 185);       // العنصر النشط
    public static readonly Color SidebarText = Color.FromArgb(236, 240, 241);        // نص الشريط

    // Button Colors - ألوان الأزرار
    public static readonly Color ButtonPrimary = Color.FromArgb(41, 128, 185);
    public static readonly Color ButtonPrimaryHover = Color.FromArgb(31, 97, 141);
    public static readonly Color ButtonSecondary = Color.FromArgb(149, 165, 166);
    public static readonly Color ButtonSecondaryHover = Color.FromArgb(127, 140, 141);
    public static readonly Color ButtonSuccess = Color.FromArgb(39, 174, 96);
    public static readonly Color ButtonSuccessHover = Color.FromArgb(34, 153, 84);
    public static readonly Color ButtonDanger = Color.FromArgb(231, 76, 60);
    public static readonly Color ButtonDangerHover = Color.FromArgb(192, 57, 43);

    // Chart Colors - ألوان الرسوم البيانية
    public static readonly Color[] ChartColors = new Color[]
    {
        Color.FromArgb(41, 128, 185),   // أزرق
        Color.FromArgb(39, 174, 96),    // أخضر
        Color.FromArgb(243, 156, 18),   // برتقالي
        Color.FromArgb(231, 76, 60),    // أحمر
        Color.FromArgb(155, 89, 182),   // بنفسجي
        Color.FromArgb(52, 152, 219),   // أزرق فاتح
        Color.FromArgb(46, 204, 113),   // أخضر فاتح
        Color.FromArgb(241, 196, 15)    // أصفر
    };

    // Shadow Colors - ألوان الظلال
    public static readonly Color ShadowLight = Color.FromArgb(30, 0, 0, 0);    // ظل خفيف
    public static readonly Color ShadowMedium = Color.FromArgb(60, 0, 0, 0);   // ظل متوسط
    public static readonly Color ShadowDark = Color.FromArgb(100, 0, 0, 0);    // ظل داكن

    /// <summary>
    /// الحصول على لون أفتح من اللون المحدد
    /// Get a lighter shade of the specified color
    /// </summary>
    public static Color Lighten(Color color, float percent)
    {
        return Color.FromArgb(
            color.A,
            Math.Min(255, (int)(color.R + (255 - color.R) * percent)),
            Math.Min(255, (int)(color.G + (255 - color.G) * percent)),
            Math.Min(255, (int)(color.B + (255 - color.B) * percent))
        );
    }

    /// <summary>
    /// الحصول على لون أغمق من اللون المحدد
    /// Get a darker shade of the specified color
    /// </summary>
    public static Color Darken(Color color, float percent)
    {
        return Color.FromArgb(
            color.A,
            Math.Max(0, (int)(color.R * (1 - percent))),
            Math.Max(0, (int)(color.G * (1 - percent))),
            Math.Max(0, (int)(color.B * (1 - percent)))
        );
    }

    /// <summary>
    /// الحصول على لون بشفافية محددة
    /// Get a color with specified opacity
    /// </summary>
    public static Color WithOpacity(Color color, byte alpha)
    {
        return Color.FromArgb(alpha, color.R, color.G, color.B);
    }
}
