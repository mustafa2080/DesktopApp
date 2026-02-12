using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

[Table("database_backups")]
public class DatabaseBackup
{
    [Key]
    [Column("backup_id")]
    public int BackupId { get; set; }

    [Required]
    [Column("backup_file_name")]
    [MaxLength(255)]
    public string BackupFileName { get; set; } = string.Empty;

    [Required]
    [Column("backup_file_path")]
    [MaxLength(500)]
    public string BackupFilePath { get; set; } = string.Empty;

    [Column("backup_date")]
    public DateTime BackupDate { get; set; } = DateTime.UtcNow;

    [Column("file_size_bytes")]
    public long FileSizeBytes { get; set; }

    [Column("description")]
    [MaxLength(500)]
    public string? Description { get; set; }

    [Column("is_auto_backup")]
    public bool IsAutoBackup { get; set; } = false;

    [Column("created_by")]
    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    [Column("restore_date")]
    public DateTime? RestoreDate { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    // Helper property for file size display
    [NotMapped]
    public string FileSizeFormatted
    {
        get
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = FileSizeBytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
