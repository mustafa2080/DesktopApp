using System.ComponentModel.DataAnnotations;

namespace GraceWay.AccountingSystem.Domain.Entities;

public enum DocumentType
{
    Document = 1,
    Image = 2,
    PDF = 3,
    Excel = 4,
    Other = 5
}

public class FileDocument
{
    [Key]
    public int DocumentId { get; set; }
    
    [Required]
    public int FolderId { get; set; }
    
    [Required]
    [MaxLength(300)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(300)]
    public string OriginalFileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? FileExtension { get; set; }
    
    public long FileSize { get; set; } // in bytes
    
    public DocumentType DocumentType { get; set; }
    
    [MaxLength(100)]
    public string? MimeType { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [MaxLength(500)]
    public string? Tags { get; set; } // Comma-separated tags
    
    public bool IsFavorite { get; set; } = false;
    
    public int DownloadCount { get; set; } = 0;
    
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    public int UploadedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
    
    // Navigation Properties
    public virtual FileFolder Folder { get; set; } = null!;
    
    public virtual User Uploader { get; set; } = null!;
}
