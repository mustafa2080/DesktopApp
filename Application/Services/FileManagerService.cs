using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Application.Services;

public interface IFileManagerService
{
    // Folder Operations
    Task<FileFolder> CreateFolderAsync(FileFolder folder);
    Task<bool> UpdateFolderAsync(FileFolder folder);
    Task<bool> DeleteFolderAsync(int folderId);
    Task<FileFolder?> GetFolderByIdAsync(int folderId);
    Task<List<FileFolder>> GetAllFoldersAsync();
    Task<List<FileFolder>> GetRootFoldersAsync();
    Task<List<FileFolder>> GetSubFoldersAsync(int parentFolderId);
    Task<bool> MoveFolderAsync(int folderId, int? newParentFolderId);
    
    // Document Operations
    Task<FileDocument> AddDocumentAsync(FileDocument document);
    Task<bool> UpdateDocumentAsync(FileDocument document);
    Task<bool> DeleteDocumentAsync(int documentId);
    Task<FileDocument?> GetDocumentByIdAsync(int documentId);
    Task<List<FileDocument>> GetDocumentsByFolderAsync(int folderId);
    Task<List<FileDocument>> SearchDocumentsAsync(string searchTerm);
    Task<List<FileDocument>> GetFavoriteDocumentsAsync(int userId);
    Task<bool> ToggleFavoriteAsync(int documentId);
    Task<bool> MoveDocumentAsync(int documentId, int newFolderId);
    Task IncrementDownloadCountAsync(int documentId);
    
    // Statistics
    Task<int> GetFolderCountAsync();
    Task<int> GetDocumentCountAsync();
    Task<long> GetTotalStorageSizeAsync();
    Task<Dictionary<DocumentType, int>> GetDocumentTypeCountsAsync();
}

public class FileManagerService : IFileManagerService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IAuthService _authService;
    private readonly IAuditService _auditService;

    public FileManagerService(
        IDbContextFactory<AppDbContext> contextFactory,
        IAuthService authService,
        IAuditService auditService)
    {
        _contextFactory = contextFactory;
        _authService = authService;
        _auditService = auditService;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // FOLDER OPERATIONS
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<FileFolder> CreateFolderAsync(FileFolder folder)
    {
        using var context = _contextFactory.CreateDbContext();
        
        folder.CreatedAt = DateTime.UtcNow;
        folder.CreatedBy = _authService.CurrentUser?.UserId ?? 1;
        
        context.FileFolders.Add(folder);
        await context.SaveChangesAsync();
        
        await _auditService.LogAsync(
            AuditAction.Create,
            "FileFolder",
            folder.FolderId,
            folder.FolderName,
            $"تم إنشاء مجلد جديد: {folder.FolderName}"
        );
        
        return folder;
    }

    public async Task<bool> UpdateFolderAsync(FileFolder folder)
    {
        using var context = _contextFactory.CreateDbContext();
        
        var existing = await context.FileFolders.FindAsync(folder.FolderId);
        if (existing == null) return false;
        
        existing.FolderName = folder.FolderName;
        existing.Description = folder.Description;
        existing.Color = folder.Color;
        existing.Icon = folder.Icon;
        existing.DisplayOrder = folder.DisplayOrder;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = _authService.CurrentUser?.UserId ?? 1;
        
        await context.SaveChangesAsync();
        
        await _auditService.LogAsync(
            AuditAction.Update,
            "FileFolder",
            folder.FolderId,
            folder.FolderName,
            $"تم تحديث المجلد: {folder.FolderName}"
        );
        
        return true;
    }

    public async Task<bool> DeleteFolderAsync(int folderId)
    {
        using var context = _contextFactory.CreateDbContext();
        
        var folder = await context.FileFolders
            .Include(f => f.Documents)
            .Include(f => f.SubFolders)
            .FirstOrDefaultAsync(f => f.FolderId == folderId);
        
        if (folder == null || folder.IsSystem) return false;
        
        // Check if folder has documents or subfolders
        if (folder.Documents.Any() || folder.SubFolders.Any())
        {
            throw new InvalidOperationException("لا يمكن حذف مجلد يحتوي على ملفات أو مجلدات فرعية");
        }
        
        context.FileFolders.Remove(folder);
        await context.SaveChangesAsync();
        
        await _auditService.LogAsync(
            AuditAction.Delete,
            "FileFolder",
            folderId,
            folder.FolderName,
            $"تم حذف المجلد: {folder.FolderName}"
        );
        
        return true;
    }

    public async Task<FileFolder?> GetFolderByIdAsync(int folderId)
    {
        using var context = _contextFactory.CreateDbContext();
        
        return await context.FileFolders
            .Include(f => f.ParentFolder)
            .Include(f => f.Creator)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FolderId == folderId);
    }

    public async Task<List<FileFolder>> GetAllFoldersAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        
        return await context.FileFolders
            .Include(f => f.ParentFolder)
            .Include(f => f.SubFolders)
            .Include(f => f.Documents)
            .AsNoTracking()
            .OrderBy(f => f.DisplayOrder)
            .ThenBy(f => f.FolderName)
            .ToListAsync();
    }

    public async Task<List<FileFolder>> GetRootFoldersAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        
        return await context.FileFolders
            .Include(f => f.SubFolders)
            .Include(f => f.Documents)
            .AsNoTracking()
            .Where(f => f.ParentFolderId == null)
            .OrderBy(f => f.DisplayOrder)
            .ThenBy(f => f.FolderName)
            .ToListAsync();
    }

    public async Task<List<FileFolder>> GetSubFoldersAsync(int parentFolderId)
    {
        using var context = _contextFactory.CreateDbContext();
        
        return await context.FileFolders
            .Include(f => f.SubFolders)
            .Include(f => f.Documents)
            .AsNoTracking()
            .Where(f => f.ParentFolderId == parentFolderId)
            .OrderBy(f => f.DisplayOrder)
            .ThenBy(f => f.FolderName)
            .ToListAsync();
    }

    public async Task<bool> MoveFolderAsync(int folderId, int? newParentFolderId)
    {
        using var context = _contextFactory.CreateDbContext();
        
        var folder = await context.FileFolders.FindAsync(folderId);
        if (folder == null) return false;
        
        folder.ParentFolderId = newParentFolderId;
        folder.UpdatedAt = DateTime.UtcNow;
        folder.UpdatedBy = _authService.CurrentUser?.UserId ?? 1;
        
        await context.SaveChangesAsync();
        
        return true;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // DOCUMENT OPERATIONS
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<FileDocument> AddDocumentAsync(FileDocument document)
    {
        using var context = _contextFactory.CreateDbContext();
        
        document.UploadedAt = DateTime.UtcNow;
        document.UploadedBy = _authService.CurrentUser?.UserId ?? 1;
        
        context.FileDocuments.Add(document);
        await context.SaveChangesAsync();
        
        await _auditService.LogAsync(
            AuditAction.Create,
            "FileDocument",
            document.DocumentId,
            document.OriginalFileName,
            $"تم رفع ملف جديد: {document.OriginalFileName}"
        );
        
        return document;
    }

    public async Task<bool> UpdateDocumentAsync(FileDocument document)
    {
        using var context = _contextFactory.CreateDbContext();
        
        var existing = await context.FileDocuments.FindAsync(document.DocumentId);
        if (existing == null) return false;
        
        existing.FileName = document.FileName;
        existing.Description = document.Description;
        existing.Tags = document.Tags;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = _authService.CurrentUser?.UserId ?? 1;
        
        await context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> DeleteDocumentAsync(int documentId)
    {
        using var context = _contextFactory.CreateDbContext();
        
        var document = await context.FileDocuments.FindAsync(documentId);
        if (document == null) return false;
        
        // Delete physical file
        if (File.Exists(document.FilePath))
        {
            try
            {
                File.Delete(document.FilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Could not delete physical file: {ex.Message}");
            }
        }
        
        context.FileDocuments.Remove(document);
        await context.SaveChangesAsync();
        
        await _auditService.LogAsync(
            AuditAction.Delete,
            "FileDocument",
            documentId,
            document.OriginalFileName,
            $"تم حذف الملف: {document.OriginalFileName}"
        );
        
        return true;
    }

    public async Task<FileDocument?> GetDocumentByIdAsync(int documentId)
    {
        using var context = _contextFactory.CreateDbContext();
        
        return await context.FileDocuments
            .Include(d => d.Folder)
            .Include(d => d.Uploader)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);
    }

    public async Task<List<FileDocument>> GetDocumentsByFolderAsync(int folderId)
    {
        using var context = _contextFactory.CreateDbContext();
        
        return await context.FileDocuments
            .Include(d => d.Uploader)
            .AsNoTracking()
            .Where(d => d.FolderId == folderId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    public async Task<List<FileDocument>> SearchDocumentsAsync(string searchTerm)
    {
        using var context = _contextFactory.CreateDbContext();
        
        var search = searchTerm.ToLower();
        
        return await context.FileDocuments
            .Include(d => d.Folder)
            .Include(d => d.Uploader)
            .AsNoTracking()
            .Where(d => 
                d.OriginalFileName.ToLower().Contains(search) ||
                d.Description!.ToLower().Contains(search) ||
                d.Tags!.ToLower().Contains(search))
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    public async Task<List<FileDocument>> GetFavoriteDocumentsAsync(int userId)
    {
        using var context = _contextFactory.CreateDbContext();
        
        return await context.FileDocuments
            .Include(d => d.Folder)
            .AsNoTracking()
            .Where(d => d.IsFavorite && d.UploadedBy == userId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    public async Task<bool> ToggleFavoriteAsync(int documentId)
    {
        using var context = _contextFactory.CreateDbContext();
        
        var document = await context.FileDocuments.FindAsync(documentId);
        if (document == null) return false;
        
        document.IsFavorite = !document.IsFavorite;
        await context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> MoveDocumentAsync(int documentId, int newFolderId)
    {
        using var context = _contextFactory.CreateDbContext();
        
        var document = await context.FileDocuments.FindAsync(documentId);
        if (document == null) return false;
        
        document.FolderId = newFolderId;
        document.UpdatedAt = DateTime.UtcNow;
        document.UpdatedBy = _authService.CurrentUser?.UserId ?? 1;
        
        await context.SaveChangesAsync();
        
        return true;
    }

    public async Task IncrementDownloadCountAsync(int documentId)
    {
        using var context = _contextFactory.CreateDbContext();
        
        var document = await context.FileDocuments.FindAsync(documentId);
        if (document != null)
        {
            document.DownloadCount++;
            await context.SaveChangesAsync();
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // STATISTICS
    // ══════════════════════════════════════════════════════════════════════════

    public async Task<int> GetFolderCountAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.FileFolders.CountAsync();
    }

    public async Task<int> GetDocumentCountAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.FileDocuments.CountAsync();
    }

    public async Task<long> GetTotalStorageSizeAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.FileDocuments.SumAsync(d => d.FileSize);
    }

    public async Task<Dictionary<DocumentType, int>> GetDocumentTypeCountsAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        
        return await context.FileDocuments
            .GroupBy(d => d.DocumentType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);
    }
}
