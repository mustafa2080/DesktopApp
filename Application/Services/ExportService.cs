using ClosedXML.Excel;
using FastReport;
using FastReport.Export.PdfSimple;
using System.Data;
using System.Text;

namespace GraceWay.AccountingSystem.Application.Services;

/// <summary>
/// خدمة التصدير — Excel عبر ClosedXML، PDF حقيقي عبر FastReport
/// </summary>
public class ExportService : IExportService
{
    // ══════════════════════════════════════════════════════════
    // EXCEL
    // ══════════════════════════════════════════════════════════

    public async Task<bool> ExportToExcelAsync(DataGridView dgv, string fileName, string sheetName = "Sheet1")
        => await ExportToExcelAsync(GridToDataTable(dgv), fileName, sheetName);

    public async Task<bool> ExportToExcelAsync(DataTable dataTable, string fileName, string sheetName = "Sheet1")
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    fileName += ".xlsx";

                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add(sheetName);

                int cols = dataTable.Columns.Count;
                int rows = dataTable.Rows.Count;

                // Header
                for (int c = 0; c < cols; c++)
                {
                    var cell = ws.Cell(1, c + 1);
                    cell.Value = dataTable.Columns[c].ColumnName;
                    cell.Style.Font.Bold             = true;
                    cell.Style.Font.FontColor        = XLColor.White;
                    cell.Style.Fill.BackgroundColor  = XLColor.FromArgb(41, 128, 185);
                    cell.Style.Alignment.Horizontal  = XLAlignmentHorizontalValues.Center;
                    cell.Style.Border.BottomBorder   = XLBorderStyleValues.Thin;
                    cell.Style.Border.BottomBorderColor = XLColor.FromArgb(25, 100, 150);
                }

                // Data
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        var val  = dataTable.Rows[r][c];
                        var cell = ws.Cell(r + 2, c + 1);

                        if (val is decimal or double or float)
                        {
                            cell.Value = Convert.ToDouble(val);
                            cell.Style.NumberFormat.Format = "#,##0.00";
                        }
                        else if (val is int or long)
                            cell.Value = Convert.ToInt64(val);
                        else if (val is DateTime dt2)
                        {
                            cell.Value = dt2;
                            cell.Style.DateFormat.Format = "yyyy/MM/dd";
                        }
                        else
                            cell.Value = val?.ToString() ?? string.Empty;

                        // alternating rows
                        if (r % 2 == 1)
                            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(235, 245, 251);
                    }
                }

                // borders + auto-fit
                if (rows > 0)
                {
                    var range = ws.Range(1, 1, rows + 1, cols);
                    range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.InsideBorder  = XLBorderStyleValues.Hair;
                }
                ws.Columns().AdjustToContents(8, 60);

                wb.SaveAs(fileName);
                return true;
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في حفظ ملف Excel:\n{ex.Message}");
                return false;
            }
        });
    }

    // ══════════════════════════════════════════════════════════
    // PDF — FastReport PdfSimple
    // ══════════════════════════════════════════════════════════

    public async Task<bool> ExportToPdfAsync(DataGridView dgv, string fileName, string title,
        Dictionary<string, string>? metadata = null)
        => await ExportDataTableToPdfAsync(GridToDataTable(dgv), fileName, title, metadata);

    public async Task<bool> ExportHtmlToPdfAsync(string html, string fileName)
    {
        // fallback: save HTML and open in browser
        return await Task.Run(() =>
        {
            try
            {
                string htmlFile = Path.ChangeExtension(fileName, ".html");
                File.WriteAllText(htmlFile, html, Encoding.UTF8);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = htmlFile, UseShellExecute = true,
                });
                return true;
            }
            catch (Exception ex) { ShowError(ex.Message); return false; }
        });
    }

    /// <summary>
    /// بيبني تقرير FastReport من DataTable ويصدّره PDF حقيقي
    /// </summary>
    public async Task<bool> ExportDataTableToPdfAsync(DataTable dataTable, string fileName, string title,
        Dictionary<string, string>? metadata = null)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    fileName += ".pdf";

                int cols = dataTable.Columns.Count;
                if (cols == 0) return false;

                // A4 landscape in mm
                float pageW_mm  = 297f;
                float pageH_mm  = 210f;
                float margin_mm = 10f;
                float usableW   = (pageW_mm - margin_mm * 2) * 2.835f;  // → points

                float colW    = usableW / cols;
                float rowH    = 18f;
                float hdrH    = 24f;
                float titleH  = 32f;
                float metaH   = 16f;
                float footerH = 18f;

                using var report = new Report();

                // ── Page setup ───────────────────────────────
                var page = new ReportPage();
                report.Pages.Add(page);
                page.PaperWidth   = pageW_mm;
                page.PaperHeight  = pageH_mm;
                page.Landscape    = true;
                page.LeftMargin   = margin_mm;
                page.RightMargin  = margin_mm;
                page.TopMargin    = margin_mm;
                page.BottomMargin = margin_mm;

                // (no DataSource needed — rows rendered manually below)

                // ── ReportTitle (title + metadata) ───────────
                int metaCount = metadata?.Count ?? 0;
                float rptHdrH = titleH + metaCount * metaH + 8f;
                var rptTitle  = new ReportTitleBand { Height = rptHdrH };
                page.ReportTitle = rptTitle;

                rptTitle.Objects.Add(new TextObject
                {
                    Bounds      = new System.Drawing.RectangleF(0, 4, usableW, titleH),
                    Text        = title,
                    Font        = new System.Drawing.Font("Arial", 15, System.Drawing.FontStyle.Bold),
                    HorzAlign   = HorzAlign.Center,
                    TextColor   = System.Drawing.Color.FromArgb(41, 128, 185),
                    RightToLeft = true,
                });

                if (metadata != null)
                {
                    float mY = titleH + 4f;
                    foreach (var kv in metadata)
                    {
                        rptTitle.Objects.Add(new TextObject
                        {
                            Bounds      = new System.Drawing.RectangleF(0, mY, usableW, metaH),
                            Text        = $"{kv.Key}: {kv.Value}",
                            Font        = new System.Drawing.Font("Arial", 8.5f),
                            HorzAlign   = HorzAlign.Center,
                            TextColor   = System.Drawing.Color.FromArgb(100, 116, 139),
                            RightToLeft = true,
                        });
                        mY += metaH;
                    }
                }

                // ── PageHeader (column headers) ───────────────
                var pgHdr = new PageHeaderBand { Height = hdrH };
                page.PageHeader = pgHdr;

                for (int c = 0; c < cols; c++)
                {
                    pgHdr.Objects.Add(new TextObject
                    {
                        Bounds      = new System.Drawing.RectangleF(c * colW, 0, colW, hdrH),
                        Text        = dataTable.Columns[c].ColumnName,
                        Font        = new System.Drawing.Font("Arial", 8.5f, System.Drawing.FontStyle.Bold),
                        HorzAlign   = HorzAlign.Center,
                        VertAlign   = VertAlign.Center,
                        FillColor   = System.Drawing.Color.FromArgb(41, 128, 185),
                        TextColor   = System.Drawing.Color.White,
                        RightToLeft = true,
                        Border      = new Border
                        {
                            Lines = BorderLines.All,
                            Color = System.Drawing.Color.FromArgb(25, 100, 155),
                            Width = 0.5f,
                        },
                    });
                }

                // ── Data rows — rendered manually in ReportTitle ──
                var dataSection = new ReportSummaryBand
                {
                    Height = dataTable.Rows.Count * rowH,
                };
                page.ReportSummary = dataSection;

                for (int r = 0; r < dataTable.Rows.Count; r++)
                {
                    var rowColor = r % 2 == 0
                        ? System.Drawing.Color.White
                        : System.Drawing.Color.FromArgb(235, 245, 251);

                    for (int c = 0; c < cols; c++)
                    {
                        dataSection.Objects.Add(new TextObject
                        {
                            Bounds      = new System.Drawing.RectangleF(c * colW, r * rowH, colW, rowH),
                            Text        = dataTable.Rows[r][c]?.ToString() ?? string.Empty,
                            Font        = new System.Drawing.Font("Arial", 8f),
                            HorzAlign   = HorzAlign.Center,
                            VertAlign   = VertAlign.Center,
                            FillColor   = rowColor,
                            RightToLeft = true,
                            Border      = new Border
                            {
                                Lines = BorderLines.All,
                                Color = System.Drawing.Color.FromArgb(210, 218, 226),
                                Width = 0.3f,
                            },
                        });
                    }
                }

                // ── PageFooter ────────────────────────────────
                var pgFtr = new PageFooterBand { Height = footerH };
                page.PageFooter = pgFtr;

                pgFtr.Objects.Add(new TextObject
                {
                    Bounds      = new System.Drawing.RectangleF(0, 2, usableW * 0.7f, footerH - 4),
                    Text        = $"تم الإنشاء: {DateTime.Now:yyyy/MM/dd HH:mm}  —  نظام جريس واي المحاسبي",
                    Font        = new System.Drawing.Font("Arial", 7f),
                    TextColor   = System.Drawing.Color.FromArgb(140, 150, 160),
                    HorzAlign   = HorzAlign.Right,
                    RightToLeft = true,
                });
                pgFtr.Objects.Add(new TextObject
                {
                    Bounds    = new System.Drawing.RectangleF(usableW * 0.7f, 2, usableW * 0.3f, footerH - 4),
                    Text      = "صفحة [Page#] من [TotalPages#]",
                    Font      = new System.Drawing.Font("Arial", 7f),
                    TextColor = System.Drawing.Color.FromArgb(140, 150, 160),
                    HorzAlign = HorzAlign.Left,
                });

                // ── Prepare & export ──────────────────────────
                report.Prepare();

                using var pdf = new PDFSimpleExport();
                pdf.Export(report, fileName);

                return true;
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تصدير PDF:\n{ex.Message}");
                return false;
            }
        });
    }

    // ══════════════════════════════════════════════════════════
    // HTML helper (backward compat)
    // ══════════════════════════════════════════════════════════

    public string CreateHtmlFromDataGridView(DataGridView dgv, string title,
        Dictionary<string, string>? metadata = null)
    {
        var dt = GridToDataTable(dgv);
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html dir='rtl' lang='ar'><head><meta charset='UTF-8'>");
        sb.AppendLine($"<title>{title}</title><style>");
        sb.AppendLine("body{{font-family:Cairo,Arial;margin:20px;direction:rtl}}");
        sb.AppendLine("h1{{color:#2980B9;text-align:center}}");
        sb.AppendLine("table{{width:100%;border-collapse:collapse}}");
        sb.AppendLine("th{{background:#2980B9;color:#fff;padding:10px;text-align:center}}");
        sb.AppendLine("td{{padding:8px;text-align:center;border:1px solid #ddd}}");
        sb.AppendLine("tr:nth-child(even){{background:#EBF5FB}}");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h1>{title}</h1>");
        if (metadata != null)
        {
            sb.AppendLine("<p style='text-align:center;color:#666'>");
            foreach (var kv in metadata) sb.Append($" {kv.Key}: <b>{kv.Value}</b> &nbsp;|&nbsp;");
            sb.AppendLine("</p>");
        }
        sb.AppendLine("<table><thead><tr>");
        foreach (DataColumn col in dt.Columns) sb.AppendLine($"<th>{col.ColumnName}</th>");
        sb.AppendLine("</tr></thead><tbody>");
        foreach (DataRow row in dt.Rows)
        {
            sb.AppendLine("<tr>");
            foreach (var v in row.ItemArray) sb.AppendLine($"<td>{v}</td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</tbody></table>");
        sb.AppendLine($"<p style='text-align:center;color:#aaa;font-size:11px'>تم الإنشاء: {DateTime.Now:yyyy/MM/dd HH:mm} — نظام جريس واي</p>");
        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    // ══════════════════════════════════════════════════════════
    // Private helpers
    // ══════════════════════════════════════════════════════════

    private static DataTable GridToDataTable(DataGridView dgv)
    {
        var dt = new DataTable();
        foreach (DataGridViewColumn col in dgv.Columns)
            if (col.Visible) dt.Columns.Add(col.HeaderText);

        foreach (DataGridViewRow row in dgv.Rows)
        {
            if (row.IsNewRow) continue;
            var dr  = dt.NewRow();
            int idx = 0;
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (!col.Visible) continue;
                dr[idx++] = row.Cells[col.Index].Value ?? string.Empty;
            }
            dt.Rows.Add(dr);
        }
        return dt;
    }

    private static void ShowError(string msg)
    {
        var form = System.Windows.Forms.Application.OpenForms.Count > 0
            ? System.Windows.Forms.Application.OpenForms[0] : null;
        if (form == null) return;

        void Show() => MessageBox.Show(msg, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button1,
            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);

        if (form.InvokeRequired) form.Invoke(Show); else Show();
    }
}
