using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GraceWay.AccountingSystem.Infrastructure.Data;

/// <summary>
/// Manages active user sessions and database connections
/// </summary>
public class SessionManager
{
    private static readonly Lazy<SessionManager> _instance = 
        new Lazy<SessionManager>(() => new SessionManager());
    
    public static SessionManager Instance => _instance.Value;

    private readonly ConcurrentDictionary<string, UserSession> _activeSessions;
    private readonly object _lockObject = new object();

    private SessionManager()
    {
        _activeSessions = new ConcurrentDictionary<string, UserSession>();
    }

    /// <summary>
    /// Register a new user session
    /// </summary>
    public string RegisterSession(int userId, string username, string machineName)
    {
        var sessionId = Guid.NewGuid().ToString();
        
        var session = new UserSession
        {
            SessionId = sessionId,
            UserId = userId,
            Username = username,
            MachineName = machineName,
            LoginTime = DateTime.Now,
            LastActivityTime = DateTime.Now,
            IsActive = true
        };

        _activeSessions.TryAdd(sessionId, session);
        
        return sessionId;
    }

    /// <summary>
    /// Update session activity time
    /// </summary>
    public void UpdateActivity(string sessionId)
    {
        if (_activeSessions.TryGetValue(sessionId, out var session))
        {
            session.LastActivityTime = DateTime.Now;
        }
    }

    /// <summary>
    /// End a user session
    /// </summary>
    public void EndSession(string sessionId)
    {
        if (_activeSessions.TryRemove(sessionId, out var session))
        {
            session.IsActive = false;
            session.LogoutTime = DateTime.Now;
        }
    }

    /// <summary>
    /// Get all active sessions
    /// </summary>
    public List<UserSession> GetActiveSessions()
    {
        return _activeSessions.Values
            .Where(s => s.IsActive)
            .OrderBy(s => s.LoginTime)
            .ToList();
    }

    /// <summary>
    /// Get active sessions for a specific user
    /// </summary>
    public List<UserSession> GetUserSessions(int userId)
    {
        return _activeSessions.Values
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderBy(s => s.LoginTime)
            .ToList();
    }

    /// <summary>
    /// Check if a user has active sessions on other machines
    /// </summary>
    public bool HasOtherActiveSessions(int userId, string currentMachineName)
    {
        return _activeSessions.Values.Any(s => 
            s.UserId == userId && 
            s.IsActive && 
            s.MachineName != currentMachineName);
    }

    /// <summary>
    /// Get count of active sessions
    /// </summary>
    public int GetActiveSessionCount()
    {
        return _activeSessions.Count(s => s.Value.IsActive);
    }

    /// <summary>
    /// Clean up inactive sessions (older than specified minutes)
    /// </summary>
    public void CleanupInactiveSessions(int inactiveMinutes = 60)
    {
        var threshold = DateTime.Now.AddMinutes(-inactiveMinutes);
        
        var inactiveSessions = _activeSessions
            .Where(s => s.Value.IsActive && s.Value.LastActivityTime < threshold)
            .Select(s => s.Key)
            .ToList();

        foreach (var sessionId in inactiveSessions)
        {
            EndSession(sessionId);
        }
    }
}

/// <summary>
/// Represents a user session
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
