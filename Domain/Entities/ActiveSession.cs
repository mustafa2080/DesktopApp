using System;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// Represents an active user session stored in the database
/// </summary>
public class ActiveSession
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
    public DateTime LastActivityTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public bool IsActive { get; set; }

    public TimeSpan SessionDuration =>
        (LogoutTime ?? DateTime.Now) - LoginTime;

    public TimeSpan IdleTime =>
        DateTime.Now - LastActivityTime;
}
