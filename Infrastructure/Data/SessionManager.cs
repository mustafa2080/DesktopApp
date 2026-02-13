using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Infrastructure.Data;

/// <summary>
/// Manages active user sessions using PostgreSQL database
/// All instances of the app share the same session data
/// </summary>
public class SessionManager
{
    private static readonly Lazy<SessionManager> _instance =
        new Lazy<SessionManager>(() => new SessionManager());

    public static SessionManager Instance => _instance.Value;

    private IDbContextFactory<AppDbContext>? _dbFactory;

    private SessionManager() { }

    /// <summary>
    /// Initialize with service provider for database access
    /// </summary>
    public void Initialize(IServiceProvider serviceProvider)
    {
        _dbFactory = serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        // Cleanup stale sessions from previous crashes
        CleanupStaleSessions();
    }

    private AppDbContext CreateContext()
    {
        return _dbFactory!.CreateDbContext();
    }

    /// <summary>
    /// Register a new user session in the database
    /// </summary>
    public string RegisterSession(int userId, string username, string machineName)
    {
        var sessionId = Guid.NewGuid().ToString();

        try
        {
            using var db = CreateContext();

            var ipAddress = GetLocalIpAddress();
            var now = DateTime.UtcNow;

            db.Database.ExecuteSqlInterpolated(
                $@"INSERT INTO active_sessions (session_id, user_id, username, machine_name, ip_address, login_time, last_activity_time, is_active)
                   VALUES ({sessionId}, {userId}, {username}, {machineName}, {ipAddress}, {now}, {now}, {true})");

            System.Diagnostics.Debug.WriteLine($"✅ Session registered in DB: {sessionId} for {username} from {machineName} ({ipAddress})");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error registering session: {ex.Message}");
        }

        return sessionId;
    }

    /// <summary>
    /// Update session activity time (heartbeat)
    /// </summary>
    public void UpdateActivity(string sessionId)
    {
        try
        {
            using var db = CreateContext();
            var now = DateTime.UtcNow;
            db.Database.ExecuteSqlInterpolated(
                $"UPDATE active_sessions SET last_activity_time = {now} WHERE session_id = {sessionId} AND is_active = true");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Error updating activity: {ex.Message}");
        }
    }

    /// <summary>
    /// End a user session
    /// </summary>
    public void EndSession(string sessionId)
    {
        try
        {
            using var db = CreateContext();
            var now = DateTime.UtcNow;
            db.Database.ExecuteSqlInterpolated(
                $"UPDATE active_sessions SET is_active = false, logout_time = {now} WHERE session_id = {sessionId}");
            System.Diagnostics.Debug.WriteLine($"✅ Session ended in DB: {sessionId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error ending session: {ex.Message}");
        }
    }

    /// <summary>
    /// Force end a session (admin can kick users)
    /// </summary>
    public void ForceEndSession(string sessionId)
    {
        EndSession(sessionId);
    }

    /// <summary>
    /// Get all active sessions from the database
    /// </summary>
    public List<ActiveSession> GetActiveSessions()
    {
        try
        {
            using var db = CreateContext();
            return db.ActiveSessions
                .Where(s => s.IsActive)
                .OrderBy(s => s.LoginTime)
                .ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error getting sessions: {ex.Message}");
            return new List<ActiveSession>();
        }
    }

    /// <summary>
    /// Get active sessions for a specific user
    /// </summary>
    public List<ActiveSession> GetUserSessions(int userId)
    {
        try
        {
            using var db = CreateContext();
            return db.ActiveSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderBy(s => s.LoginTime)
                .ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error getting user sessions: {ex.Message}");
            return new List<ActiveSession>();
        }
    }

    /// <summary>
    /// Check if a user has active sessions on other machines
    /// </summary>
    public bool HasOtherActiveSessions(int userId, string currentMachineName)
    {
        try
        {
            using var db = CreateContext();
            return db.ActiveSessions.Any(s =>
                s.UserId == userId &&
                s.IsActive &&
                s.MachineName != currentMachineName);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error checking sessions: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get count of active sessions
    /// </summary>
    public int GetActiveSessionCount()
    {
        try
        {
            using var db = CreateContext();
            return db.ActiveSessions.Count(s => s.IsActive);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error counting sessions: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Clean up inactive sessions (older than specified minutes)
    /// </summary>
    public void CleanupInactiveSessions(int inactiveMinutes = 60)
    {
        try
        {
            using var db = CreateContext();
            var threshold = DateTime.UtcNow.AddMinutes(-inactiveMinutes);
            var now = DateTime.UtcNow;
            
            db.Database.ExecuteSqlInterpolated(
                $"UPDATE active_sessions SET is_active = false, logout_time = {now} WHERE is_active = true AND last_activity_time < {threshold}");
            
            System.Diagnostics.Debug.WriteLine($"✅ Cleaned up stale sessions older than {inactiveMinutes} min");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error cleaning sessions: {ex.Message}");
        }
    }

    /// <summary>
    /// Cleanup sessions that were left active from crashed instances on this machine
    /// </summary>
    private void CleanupStaleSessions()
    {
        try
        {
            using var db = CreateContext();
            var machineName = Environment.MachineName;
            var now = DateTime.UtcNow;
            
            var count = db.Database.ExecuteSqlInterpolated(
                $"UPDATE active_sessions SET is_active = false, logout_time = {now} WHERE is_active = true AND machine_name = {machineName}");
            
            if (count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"✅ Cleaned up {count} stale sessions from {machineName}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Error cleaning stale sessions: {ex.Message}");
        }
    }

    /// <summary>
    /// Get local IP address
    /// </summary>
    private string GetLocalIpAddress()
    {
        try
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            var endPoint = socket.LocalEndPoint as IPEndPoint;
            return endPoint?.Address.ToString() ?? "Unknown";
        }
        catch
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                var ip = host.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
                return ip?.ToString() ?? "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }
    }
}

// Keep UserSession class for backward compatibility (if referenced elsewhere)
/// <summary>
/// Represents a user session (legacy - kept for compatibility)
/// </summary>
public class UserSession
{
    public string SessionId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
    public DateTime LastActivityTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public bool IsActive { get; set; }

    public TimeSpan SessionDuration =>
        (LogoutTime ?? DateTime.Now) - LoginTime;

    public TimeSpan IdleTime =>
        DateTime.Now - LastActivityTime;
}
