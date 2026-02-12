using System.ComponentModel.DataAnnotations;

namespace GraceWay.AccountingSystem.Domain.Entities;

public class RolePermission
{
    [Key]
    public int RolePermissionId { get; set; }
    public int RoleId { get; set; }
    public int PermissionId { get; set; }

    // Navigation
    public Role? Role { get; set; }
    public Permission? Permission { get; set; }
}
