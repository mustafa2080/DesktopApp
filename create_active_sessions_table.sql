-- Create active_sessions table for tracking user sessions across all app instances
CREATE TABLE IF NOT EXISTS active_sessions (
    id SERIAL PRIMARY KEY,
    session_id VARCHAR(100) NOT NULL UNIQUE,
    user_id INTEGER NOT NULL,
    username VARCHAR(100) NOT NULL,
    machine_name VARCHAR(200) NOT NULL DEFAULT '',
    ip_address VARCHAR(50) NOT NULL DEFAULT '',
    login_time TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    last_activity_time TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    logout_time TIMESTAMP WITH TIME ZONE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

-- Index for fast lookup of active sessions
CREATE INDEX IF NOT EXISTS idx_active_sessions_is_active ON active_sessions (is_active) WHERE is_active = TRUE;
CREATE INDEX IF NOT EXISTS idx_active_sessions_user_id ON active_sessions (user_id);
CREATE INDEX IF NOT EXISTS idx_active_sessions_session_id ON active_sessions (session_id);

-- Cleanup any old stale sessions (optional - run on first setup)
-- UPDATE active_sessions SET is_active = false, logout_time = NOW() WHERE is_active = true;
