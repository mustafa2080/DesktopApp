using System.ComponentModel.DataAnnotations;

namespace GraceWay.AccountingSystem.Domain.Entities;

public class FileFolder
{
    [Key]
    public int FolderId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string FolderName { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string? Color { get; set; } = "#2196F3"; // Default blue color
    
    [MaxLength(50)]
    public string? Icon { get; set; } = "📁"; // Default folder icon
    
    public int? ParentFolderId { get; set; }
    
    public int DisplayOrder { get; set; }
    
    public bool IsSystem { get; set; } = false; // System folders can't be deleted
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int CreatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
    
    // Navigation Properties
    public virtual User Creator { get; set; } = null!;
    
    public virtual FileFolder? ParentFolder { get; set; }
    
    public virtual ICollection<FileFolder> SubFolders { get; set; } = new List<FileFolder>();
    
    public virtual ICollection<FileDocument> Documents { get; set; } = new List<FileDocument>();
}
