namespace GraceWay.AccountingSystem.Domain.Reports;

/// <summary>
/// أنواع التصدير المدعومة
/// </summary>
public enum ExportFormat
{
    PDF,
    Excel,
    CSV
}

/// <summary>
/// خيارات التصدير
/// </summary>
public class ExportOptions
{
    public ExportFormat Format { get; set; }
    public string FileName { get; set; } = string.Empty;
    public bool IncludeCharts { get; set; } = true;
    public bool IncludeSummary { get; set; } = true;
    public bool IncludeDetails { get; set; } = true;
    public string CompanyLogo { get; set; } = string.Empty;
}

/// <summary>
/// نتيجة عملية التصدير
/// </summary>
public class ExportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string FilePath { get; set; } = string.Empty;
}
